using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;
using Mixins.Runtime;

namespace Mixins.Diagnostics;

public sealed record MixinDebugExpression(
  string PreludeProgram,
  string LateProgram,
  ImmutableDictionary<string, object> Variables,
  string Provider,
  string SourceType,
  string SourceMember,
  int PreludeOperations,
  double PreludeMilliseconds
);

public sealed record MixinDebugFinalState(
  MixinDebugExpression Work,
  int LateOperations,
  double LateMilliseconds
);

public readonly record struct MixinDebugFingerprintPart(ulong Hash, long Length);

public sealed record MixinDebugRenderData(
  MixinStringPool StringPool,
  ImmutableArray<MixinDebugExpression> Expressions,
  bool InternStringPool,
  long GenerationVersion,
  MixinDebugFingerprintPart Outputs,
  MixinDebugFingerprintPart Variables,
  MixinDebugFingerprintPart Signatures
);

/// <summary>Produces deterministic, host-independent diagnostics for compiled mixin programs.</summary>
public static class MixinDebugRenderer {
  public static string StateKey(MixinDebugExpression work) {
    return string.Join("\u001f", work.Provider, work.SourceType, work.SourceMember, work.LateProgram);
  }

  public static string BuildTrace(MixinDebugRenderData render) {
    var builder = new StringBuilder();
    var debugPoolBuilder = new MixinStringPoolBuilder(render.StringPool);
    foreach (var work in render.Expressions)
    foreach (var token in (work.PreludeProgram + "\n" + work.LateProgram).Split(
      ['@', '<', '>', '#', ':', '(', ')', ' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries
    ))
      debugPoolBuilder.Intern(token);
    var debugPool = debugPoolBuilder.Freeze();
    builder.AppendLine("// ============================================================================");
    builder.AppendLine("// HELIX MIXIN PROGRAM DUMP");
    if (render.InternStringPool) AppendStringPool(builder, debugPool);
    builder.Append("// generationVersion = ")
      .AppendLine(render.GenerationVersion.ToString(CultureInfo.InvariantCulture));
    AppendFingerprint(builder, "outputs", render.Outputs);
    AppendFingerprint(builder, "variables", render.Variables);
    AppendFingerprint(builder, "signatures", render.Signatures);
    for (var index = 0; index < render.Expressions.Length; index++) {
      var work = render.Expressions[index];
      builder.AppendLine("// ----------------------------------------------------------------------------");
      builder.Append("// EXPRESSION ").Append(index + 1).Append(": ").Append(work.Provider);
      if (!string.IsNullOrEmpty(work.SourceType)) builder.Append(" on ").Append(work.SourceType);
      if (!string.IsNullOrEmpty(work.SourceMember)) builder.Append('.').Append(work.SourceMember);
      builder.AppendLine();
      AppendProgram(builder, "PRELUDE PROGRAM (PREPARED)", work.PreludeProgram, render.InternStringPool, debugPool);
      AppendProgram(builder, "LATE PROGRAM (PREPARED)", work.LateProgram, render.InternStringPool, debugPool);
      builder.AppendLine("// CARRIED VALUES");
      var carries = work.Variables.Where(item => item.Key.StartsWith(
          MixinVirtualMachine.CarryLocalPrefix, StringComparison.Ordinal
        ) && IsCarryReferenced(
          work.LateProgram, item.Key.Substring(MixinVirtualMachine.CarryLocalPrefix.Length)
        )
      ).OrderBy(item => item.Key, StringComparer.Ordinal).ToArray();
      if (carries.Length == 0) builder.AppendLine("//   <none>");
      foreach (var carry in carries) {
        var label = carry.Key.Substring(MixinVirtualMachine.CarryLocalPrefix.Length);
        builder.Append("//   @carry#").Append(label).Append(" = ")
          .AppendLine(FormatValue(carry.Value));
      }
    }
    builder.AppendLine("// ============================================================================");
    return builder.ToString();
  }

  public static string BuildFinalState(
    IEnumerable<MixinDebugFinalState> states,
    IReadOnlyDictionary<string, object> sharedVariables
  ) {
    var builder = new StringBuilder();
    builder.AppendLine().AppendLine("// ============================================================================");
    builder.AppendLine("// HELIX MIXIN FINAL STATE");
    var finalStates = states as MixinDebugFinalState[] ?? [.. states];
    foreach (var state in finalStates) {
      builder.Append("// ").Append(state.Work.Provider);
      if (!string.IsNullOrEmpty(state.Work.SourceType)) builder.Append(" on ").Append(state.Work.SourceType);
      if (!string.IsNullOrEmpty(state.Work.SourceMember)) builder.Append('.').Append(state.Work.SourceMember);
      builder.AppendLine();
      builder.Append("//   preludeOperations = ")
        .AppendLine(state.Work.PreludeOperations.ToString(CultureInfo.InvariantCulture));
      builder.Append("//   preludeDurationMs = ").AppendLine(FormatMilliseconds(state.Work.PreludeMilliseconds));
      builder.Append("//   lateOperations = ").AppendLine(state.LateOperations.ToString(CultureInfo.InvariantCulture));
      builder.Append("//   lateDurationMs = ").AppendLine(FormatMilliseconds(state.LateMilliseconds));
    }
    builder.AppendLine("// SHARED VARIABLES");
    var persistent = sharedVariables.Where(item => !item.Key.StartsWith(
        MixinVirtualMachine.CarryLocalPrefix, StringComparison.Ordinal
      )
    ).OrderBy(item => item.Key, StringComparer.Ordinal).ToArray();
    if (persistent.Length == 0) builder.AppendLine("//   <empty>");
    foreach (var variable in persistent)
      builder.Append("//   @var#").Append(variable.Key).Append(" = ").AppendLine(FormatValue(variable.Value));
    var total = finalStates.Sum(state => state.Work.PreludeMilliseconds + state.LateMilliseconds);
    builder.Append("// TOTAL durationMs = ").AppendLine(FormatMilliseconds(total));
    builder.AppendLine("// ============================================================================");
    return builder.ToString();
  }

  private static void AppendFingerprint(
    StringBuilder builder, string name, MixinDebugFingerprintPart fingerprint
  ) {
    builder.Append("// ").Append(name).Append("Fingerprint = 0x")
      .Append(fingerprint.Hash.ToString("X16", CultureInfo.InvariantCulture)).Append("; length = ")
      .AppendLine(fingerprint.Length.ToString(CultureInfo.InvariantCulture));
  }

  private static void AppendStringPool(StringBuilder builder, MixinStringPool pool) {
    builder.AppendLine("// INTERNED STRING POOL");
    for (var id = 0; id < pool.Count; id++)
      builder.Append("//   §").Append(id).Append(" = ").AppendLine(EscapeString(pool[id]));
  }

  private static void AppendProgram(
    StringBuilder builder, string title, string program, bool internStringPool, MixinStringPool debugPool
  ) {
    builder.Append("// ").AppendLine(title);
    var lines = (program ?? "").Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
    if (lines.Length == 1 && lines[0].Length == 0) {
      builder.AppendLine("//   <empty>");
      return;
    }
    for (var index = 0; index < lines.Length; index++) {
      if (index != lines.Length - 1 || lines[index].Length != 0) {
        builder.Append("//   ").AppendLine(
          internStringPool ? InternLine(lines[index], debugPool) : lines[index].Replace("@INLINE<", "@CALL<")
        );
      }
    }
  }

  private static string FormatValue(object value) {
    return value switch {
      bool boolean => boolean ? "true" : "false",
      _ => Convert.ToString(value, CultureInfo.InvariantCulture).Replace("\r", "\\r").Replace("\n", "\\n")
    };
  }

  private static string InternLine(string line, MixinStringPool pool) {
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

  private static string EscapeString(string value) {
    return (value ?? "").Replace("\\", @"\\").Replace("\r", "\\r").Replace("\n", "\\n");
  }

  private static string FormatMilliseconds(double milliseconds) {
    return milliseconds.ToString("F3", CultureInfo.InvariantCulture);
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

  private static bool IsReferenceNameCharacter(char character) {
    return char.IsLetterOrDigit(character) || character is '_' or '$';
  }
}