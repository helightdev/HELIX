using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using HelixSourceGenerator.Language;
using HelixSourceGenerator.Language.Compiler;

namespace HelixSourceGenerator.Generators;

public sealed partial class MixinGenerator {
  private static string DebugStateKey(DebugExpressionWork work) {
    return string.Join("\u001f", work.Provider, work.SourceType, work.SourceMember, work.LateProgram);
  }

  private static string DebugStateKey(LateExpressionWork work) {
    return string.Join(
      "\u001f", work.Provider, work.SourceType, work.SourceMember, MixinSyntaxRenderer.RenderProgram(work.Expression)
    );
  }

  private static string BuildDebugTrace(MixinRenderModel render) {
    var builder = new StringBuilder();
    builder.AppendLine("// ============================================================================");
    builder.AppendLine("// HELIX MIXIN PROGRAM DUMP");
    if (render.DebugStringPool) AppendDebugStringPool(builder, render.StringPool);
    builder.Append("// generationVersion = ")
      .AppendLine(render.GenerationVersion.ToString(CultureInfo.InvariantCulture));
    AppendDebugFingerprint(builder, "outputs", render.Fingerprint.Outputs);
    AppendDebugFingerprint(builder, "variables", render.Fingerprint.Variables);
    AppendDebugFingerprint(builder, "signatures", render.Fingerprint.Signatures);
    for (var index = 0; index < render.DebugExpressions.Length; index++) {
      var work = render.DebugExpressions[index];
      builder.AppendLine("// ----------------------------------------------------------------------------");
      builder.Append("// EXPRESSION ").Append(index + 1).Append(": ").Append(work.Provider);
      if (!string.IsNullOrEmpty(work.SourceType)) builder.Append(" on ").Append(work.SourceType);
      if (!string.IsNullOrEmpty(work.SourceMember)) builder.Append('.').Append(work.SourceMember);
      builder.AppendLine();
      AppendDebugProgram(builder, "PRELUDE PROGRAM (PREPARED)", work.PreludeProgram, render);
      AppendDebugProgram(builder, "LATE PROGRAM (PREPARED)", work.LateProgram, render);
      builder.AppendLine("// CARRIED VALUES");
      var carries = work.Variables.Where(item => item.Key.StartsWith(
          MixinExpressionVirtualMachine.CarryLocalPrefix, StringComparison.Ordinal
        ) && IsCarryReferenced(
          work.LateProgram, item.Key.Substring(MixinExpressionVirtualMachine.CarryLocalPrefix.Length)
        )
      ).OrderBy(item => item.Key, StringComparer.Ordinal).ToArray();
      if (carries.Length == 0) builder.AppendLine("//   <none>");
      foreach (var carry in carries) {
        var label = carry.Key.Substring(MixinExpressionVirtualMachine.CarryLocalPrefix.Length);
        builder.Append("//   @carry#").Append(label).Append(" = ")
          .AppendLine(MixinValue.From(carry.Value).Render().Replace("\r", "\\r").Replace("\n", "\\n"));
      }
    }
    builder.AppendLine("// ============================================================================");
    return builder.ToString();
  }

  private static string BuildFinalDebugState(
    IEnumerable<FinalDebugState> states, IReadOnlyDictionary<string, object> sharedVariables
  ) {
    var builder = new StringBuilder();
    builder.AppendLine().AppendLine("// ============================================================================");
    builder.AppendLine("// HELIX MIXIN FINAL STATE");
    var finalStates = states as FinalDebugState[] ?? [.. states];
    foreach (var state in finalStates) {
      builder.Append("// ").Append(state.Work.Provider);
      if (!string.IsNullOrEmpty(state.Work.SourceType)) builder.Append(" on ").Append(state.Work.SourceType);
      if (!string.IsNullOrEmpty(state.Work.SourceMember)) builder.Append('.').Append(state.Work.SourceMember);
      builder.AppendLine();
      builder.Append("//   preludeOperations = ")
        .AppendLine(state.Work.PreludeOperations.ToString(CultureInfo.InvariantCulture));
      builder.Append("//   preludeDurationMs = ").AppendLine(FormatDebugMilliseconds(state.Work.PreludeMilliseconds));
      builder.Append("//   lateOperations = ").AppendLine(state.LateOperations.ToString(CultureInfo.InvariantCulture));
      builder.Append("//   lateDurationMs = ").AppendLine(FormatDebugMilliseconds(state.LateMilliseconds));
    }
    builder.AppendLine("// SHARED VARIABLES");
    var persistent = sharedVariables.Where(item => !item.Key.StartsWith(
        MixinExpressionVirtualMachine.CarryLocalPrefix, StringComparison.Ordinal
      )
    ).OrderBy(item => item.Key, StringComparer.Ordinal).ToArray();
    if (persistent.Length == 0) builder.AppendLine("//   <empty>");
    foreach (var variable in persistent) {
      builder.Append("//   @var#").Append(variable.Key).Append(" = ")
        .AppendLine(MixinValue.From(variable.Value).Render().Replace("\r", "\\r").Replace("\n", "\\n"));
    }
    var total = finalStates.Sum(state => state.Work.PreludeMilliseconds + state.LateMilliseconds);
    builder.Append("// TOTAL durationMs = ").AppendLine(FormatDebugMilliseconds(total));
    builder.AppendLine("// ============================================================================");
    return builder.ToString();
  }

  private static void AppendDebugFingerprint(
    StringBuilder builder, string name, MixinRenderFingerprint.FingerprintPart fingerprint
  ) {
    builder.Append("// ").Append(name).Append("Fingerprint = 0x")
      .Append(fingerprint.Hash.ToString("X16", CultureInfo.InvariantCulture)).Append("; length = ")
      .AppendLine(fingerprint.Length.ToString(CultureInfo.InvariantCulture));
  }

  private static void AppendDebugStringPool(StringBuilder builder, MixinStringPool pool) {
    builder.AppendLine("// INTERNED STRING POOL");
    for (var id = 0; id < pool.Count; id++)
      builder.Append("//   §").Append(id).Append(" = ").AppendLine(EscapeDebugString(pool[id]));
  }

  private static void AppendDebugProgram(StringBuilder builder, string title, string program, MixinRenderModel render) {
    builder.Append("// ").AppendLine(title);
    var lines = (program ?? "").Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
    if (lines.Length == 1 && lines[0].Length == 0) {
      builder.AppendLine("//   <empty>");
      return;
    }
    for (var index = 0; index < lines.Length; index++) {
      if (index != lines.Length - 1 || lines[index].Length != 0)
        builder.Append("//   ").AppendLine(
          render.DebugStringPool ? InternDebugLine(lines[index], render.StringPool) : lines[index]
        );
    }
  }

  private static string InternDebugLine(string line, MixinStringPool pool) {
    var candidates = Enumerable.Range(0, pool.Count).Select(id => new { Id = id, Value = pool[id] })
      .Where(item => !string.IsNullOrEmpty(item.Value) && item.Value.IndexOfAny(['\r', '\n']) < 0)
      .OrderByDescending(item => item.Value.Length).ThenBy(item => item.Id).ToArray();
    var builder = new StringBuilder(line.Length);
    for (var offset = 0; offset < line.Length;) {
      var match = candidates.FirstOrDefault(item => offset + item.Value.Length <= line.Length &&
        string.CompareOrdinal(line, offset, item.Value, 0, item.Value.Length) == 0
      );
      if (match is null) builder.Append(line[offset++]);
      else {
        builder.Append('§').Append(match.Id.ToString(CultureInfo.InvariantCulture));
        offset += match.Value.Length;
      }
    }
    return builder.ToString();
  }

  private static string EscapeDebugString(string value) {
    return (value ?? "").Replace("\\", @"\\").Replace("\r", "\\r").Replace("\n", "\\n");
  }

  private static string FormatDebugMilliseconds(double milliseconds) {
    return milliseconds.ToString("F3", CultureInfo.InvariantCulture);
  }

  private static bool IsReferenceNameCharacter(char character) {
    return char.IsLetterOrDigit(character) || character is '_' or '$';
  }

  private static bool IsCarryReferenced(string program, string label) {
    var reference = "@carry#" + label;
    var offset = 0;
    while (offset < (program?.Length ?? 0)) {
      var index = program.IndexOf(reference, offset, StringComparison.Ordinal);
      if (index < 0) return false;
      var end = index + reference.Length;
      if (end == program.Length || !IsReferenceNameCharacter(program[end])) return true;
      offset = end;
    }
    return false;
  }
}