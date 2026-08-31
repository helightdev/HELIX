using System;
using System.Collections.Generic;
using System.Linq;

namespace MixinLanguage;

public enum DirectiveOperandKind { None, Value, Boolean }

/// <summary>
/// Coarse language types used by editors and other analysis clients. They are intentionally
/// broader than runtime <see cref="IMixinValue"/> implementations: dynamic expressions remain
/// valid while statically meaningful call sites can still offer completion and diagnostics.
/// </summary>
public enum MixinLanguageValueKind {
  None, Any, Text, Table, Symbol, Type, Boolean, Function, CSharpType, Identifier, Label,
  OutputTarget, Expression
}

public class DirectiveDefinition(string name, DirectiveOperandKind operandKind, int maximumArguments = 1,
  int minimumArguments = 0) {
  public string Name { get; } = name;
  public DirectiveOperandKind OperandKind { get; } = operandKind;
  public int MaximumArguments { get; } = maximumArguments;
  public int MinimumArguments { get; } = minimumArguments;
  public MixinLanguageValueKind OperandType { get; private set; } = operandKind switch {
    DirectiveOperandKind.Boolean => MixinLanguageValueKind.Boolean,
    DirectiveOperandKind.Value => MixinLanguageValueKind.Expression,
    _ => MixinLanguageValueKind.None
  };
  public IReadOnlyList<MixinLanguageValueKind> ArgumentTypes { get; private set; } = [];
  public string Documentation { get; private set; } = "Runtime directive.";

  internal DirectiveDefinition WithLanguageSignature(
    IReadOnlyList<MixinLanguageValueKind> argumentTypes,
    MixinLanguageValueKind? operandType = null,
    string documentation = null
  ) {
    ArgumentTypes = argumentTypes ?? [];
    if (operandType is not null) OperandType = operandType.Value;
    if (!string.IsNullOrWhiteSpace(documentation)) Documentation = documentation;
    return this;
  }

  internal virtual bool Validate(IReadOnlyList<string> arguments, string operand, out string error) {
    if (arguments.Count >= MinimumArguments && arguments.Count <= MaximumArguments) {
      error = null;
      return true;
    }
    error = arguments.Count < MinimumArguments
      ? Name + " requires at least " + MinimumArguments + " arguments"
      : Name + " accepts at most " + MaximumArguments + " arguments";
    return false;
  }
}

public abstract class DirectiveFunctionDefinition(string name, DirectiveOperandKind operandKind,
  int maximumArguments = 1, int hoistedLocalArgumentIndex = -1, int minimumArguments = 0
) : DirectiveDefinition(name, operandKind, maximumArguments, minimumArguments) {
  public int HoistedLocalArgumentIndex { get; } = hoistedLocalArgumentIndex;

  public abstract IMixinValue Invoke(
    ExecutionContext context, IReadOnlyList<IMixinValue> arguments,
    IMixinValue operand
  );
}

public static class DirectiveLibrary {
  private static readonly IReadOnlyDictionary<string, DirectiveDefinition> Definitions =
    CreateDefinitions();

  private static IReadOnlyDictionary<string, DirectiveDefinition> CreateDefinitions() {
    using var profile = MixinProfiler.Measure("static.directive_library");
    var definitions = new Dictionary<string, DirectiveDefinition>(StringComparer.Ordinal) {
      ["RESOLVE_MIXIN"] = new ResolveMixinDirective(), ["PUSH"] = new PushDirective(), ["PUT"] = new PutDirective()
    };
    definitions["RESOLVE_MIXIN"].WithLanguageSignature([MixinLanguageValueKind.Text]);
    definitions["PUSH"].WithLanguageSignature([MixinLanguageValueKind.Identifier]);
    definitions["PUT"].WithLanguageSignature([
      MixinLanguageValueKind.Identifier, MixinLanguageValueKind.Any
    ]);
    return definitions;
  }

  public static bool TryGet(string name, out DirectiveDefinition definition) {
    return Definitions.TryGetValue(name ?? "", out definition);
  }

  public static IEnumerable<DirectiveDefinition> Enumerate() => Definitions.Values;

  /// <summary>All built-in and runtime directives with editor-facing language signatures.</summary>
  public static IEnumerable<DirectiveDefinition> EnumerateLanguageDefinitions() =>
    BuiltinDefinitions.Concat(Definitions.Values);

  private static readonly IReadOnlyList<DirectiveDefinition> BuiltinDefinitions = [
    Builtin("SCOPE", 0, 1, DirectiveOperandKind.None, [MixinLanguageValueKind.Label]),
    Builtin("LABEL", 1, 1, DirectiveOperandKind.None, [MixinLanguageValueKind.Label]),
    Builtin("FUNC", 1, 1, DirectiveOperandKind.None, [MixinLanguageValueKind.Function]),
    Builtin("CALL", 1, 2, DirectiveOperandKind.Value,
      [MixinLanguageValueKind.Identifier, MixinLanguageValueKind.Function]),
    Builtin("INLINE", 1, 1, DirectiveOperandKind.None, [MixinLanguageValueKind.Function]),
    Builtin("END", 0, 0, DirectiveOperandKind.None, []),
    Builtin("MATCH", 0, 1, DirectiveOperandKind.Boolean, [MixinLanguageValueKind.Label]),
    Builtin("ASSERT", 0, 0, DirectiveOperandKind.Boolean, []),
    Builtin("CODE", 0, 1, DirectiveOperandKind.Value, [MixinLanguageValueKind.OutputTarget]),
    Builtin("MIXIN", 1, 2, DirectiveOperandKind.Value,
      [MixinLanguageValueKind.Any, MixinLanguageValueKind.Any]),
    Builtin("USING", 0, 0, DirectiveOperandKind.Value, []),
    Builtin("LOG", 0, 0, DirectiveOperandKind.Value, []),
    Builtin("LOCAL", 1, 1, DirectiveOperandKind.Value, [MixinLanguageValueKind.Identifier]),
    Builtin("VAR", 1, 1, DirectiveOperandKind.Value, [MixinLanguageValueKind.Identifier]),
    Builtin("TAR", 1, 1, DirectiveOperandKind.Value, [MixinLanguageValueKind.Identifier]),
    Builtin("CARRY", 1, 1, DirectiveOperandKind.Value, [MixinLanguageValueKind.Identifier]),
    Builtin("RETURN", 0, 0, DirectiveOperandKind.Value, []),
    Builtin("GOTO", 1, 1, DirectiveOperandKind.None, [MixinLanguageValueKind.Label]),
    Builtin("SKIP", 0, 0, DirectiveOperandKind.None, []),
    Builtin("FAIL", 0, 0, DirectiveOperandKind.Value, []),
    Builtin("ANNOTATION", 1, 1, DirectiveOperandKind.None, [MixinLanguageValueKind.CSharpType]),
    Builtin("DERIVATION", 1, 1, DirectiveOperandKind.None, [MixinLanguageValueKind.CSharpType]),
    Builtin("PRELUDE", 0, 0, DirectiveOperandKind.None, []),
    Builtin("DEFINE_TARGET", 2, 2, DirectiveOperandKind.None,
      [MixinLanguageValueKind.Identifier, MixinLanguageValueKind.Any]),
    Builtin("CONFIG", 1, 1, DirectiveOperandKind.Value, [MixinLanguageValueKind.Identifier])
  ];

  private static DirectiveDefinition Builtin(string name, int minimum, int maximum,
    DirectiveOperandKind operand, IReadOnlyList<MixinLanguageValueKind> arguments) =>
    new DirectiveDefinition(name, operand, maximum, minimum).WithLanguageSignature(
      arguments, documentation: "Built-in @" + name + " directive.");
}

internal abstract class EvaluatedDirective(string name, int arguments, int hoistedLocal)
  : DirectiveFunctionDefinition(name, DirectiveOperandKind.Value, arguments, hoistedLocal, arguments) {
  protected virtual bool EvaluateOperand => true;

  public sealed override IMixinValue Invoke(
    ExecutionContext context, IReadOnlyList<IMixinValue> arguments,
    IMixinValue operand
  ) {
    var values = arguments.Select(context.Evaluate).ToArray();
    var error = values.OfType<ErrorMixinValue>().FirstOrDefault();
    if (error is not null) return error;
    if (EvaluateOperand) operand = context.Evaluate(operand);
    return operand is ErrorMixinValue ? operand : Apply(context, values, operand);
  }

  protected abstract IMixinValue Apply(
    ExecutionContext context, IReadOnlyList<IMixinValue> arguments,
    IMixinValue operand
  );
}

internal sealed class ResolveMixinDirective() : EvaluatedDirective("RESOLVE_MIXIN", 1, 0) {
  protected override IMixinValue Apply(
    ExecutionContext context, IReadOnlyList<IMixinValue> arguments,
    IMixinValue operand
  ) {
    return context.ResolveMixin(arguments[0].Render(context), operand);
  }
}

internal abstract class TableDirective(string name, int arguments)
  : DirectiveFunctionDefinition(name, DirectiveOperandKind.Value, arguments, minimumArguments: arguments) {
  public override IMixinValue Invoke(
    ExecutionContext context, IReadOnlyList<IMixinValue> arguments,
    IMixinValue operand
  ) {
    var values = arguments.Select(context.Evaluate).ToArray();
    var error = values.OfType<ErrorMixinValue>().FirstOrDefault();
    if (error is not null) return error;
    operand = context.Evaluate(operand);
    if (operand is ErrorMixinValue) return operand;
    var local = values[0].Render(context);
    var table = context.Locals.TryGetValue(local, out var existing) && existing is MixinTableValue typed
      ? typed
      : MixinTableValue.Empty;
    table = Mutate(context, table, values, operand);
    context.Locals.StoreIsolated(local, table);
    return table;
  }

  protected abstract MixinTableValue Mutate(
    ExecutionContext context, MixinTableValue table,
    IReadOnlyList<IMixinValue> arguments, IMixinValue operand
  );
}

internal sealed class PushDirective() : TableDirective("PUSH", 1) {
  protected override MixinTableValue Mutate(
    ExecutionContext context, MixinTableValue table,
    IReadOnlyList<IMixinValue> arguments, IMixinValue operand
  ) {
    return table.Push(context, operand);
  }
}

internal sealed class PutDirective() : TableDirective("PUT", 2) {
  protected override MixinTableValue Mutate(
    ExecutionContext context, MixinTableValue table,
    IReadOnlyList<IMixinValue> arguments, IMixinValue operand
  ) {
    return table.Put(context, arguments[1].Render(context), operand);
  }
}
