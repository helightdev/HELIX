using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;
using Mixins.Runtime;

namespace Mixins.Diagnostics;

public sealed record MixinDebugExpression(
  MixinExpressionExecutionProgram PreludeProgram,
  MixinExpressionExecutionProgram LateProgram,
  ImmutableDictionary<string, object> Variables,
  ImmutableDictionary<string, object> Carries,
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
    return string.Join("\u001f", work.Provider, work.SourceType, work.SourceMember, work.LateProgram.Identity);
  }

  public static string BuildTrace(MixinDebugRenderData render) {
    var builder = new StringBuilder();
    builder.AppendLine("// ============================================================================");
    builder.AppendLine("// HELIX MIXIN PROGRAM DUMP");
    var machine = new MixinVirtualMachine(render.Expressions.SelectMany(work =>
      new[] {work.PreludeProgram, work.LateProgram}), render.StringPool);
    AppendProgram(builder, "GLOBAL POOLS", machine.DisassemblePools());
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
      AppendProgram(builder, "PRELUDE BYTECODE", machine.Disassemble(work.PreludeProgram));
      AppendProgram(builder, "LATE BYTECODE", machine.Disassemble(work.LateProgram));
      builder.AppendLine("// CARRIED VALUES");
      var carries = work.Carries
        .OrderBy(item => item.Key, StringComparer.Ordinal).ToArray();
      if (carries.Length == 0) builder.AppendLine("//   <none>");
      foreach (var carry in carries) {
        builder.Append("//   carry local ").Append(carry.Key).Append(" = ")
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
    var persistent = sharedVariables.OrderBy(item => item.Key, StringComparer.Ordinal).ToArray();
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

  private static void AppendProgram(
    StringBuilder builder, string title, string program
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
          lines[index]
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

  private static string FormatMilliseconds(double milliseconds) {
    return milliseconds.ToString("F3", CultureInfo.InvariantCulture);
  }

}
