using System;
using System.Collections.Generic;
using System.Globalization;

namespace HelixSourceGenerator.Language;

internal sealed class DirectiveFunctionInvocation(
  DirectiveInstruction instruction,
  IMixinExpressionContext context,
  MixinValueDictionary locals,
  MixinValueDictionary variables,
  ICollection<MixinExpressionOutput> outputs
) {
  internal DirectiveInstruction Instruction { get; } = instruction;
  internal IMixinExpressionContext Context { get; } = context;
  internal MixinValueDictionary Locals { get; } = locals;
  internal MixinValueDictionary Variables { get; } = variables;

  internal bool ResolveArgument(int index, out string value, out string error) =>
    MixinExpressionEvaluator.TryResolveDirectiveArgument(
      Instruction.Arguments[index], Context, Locals, Variables, out value, out error
    );

  internal bool Evaluate(out object value, out string error) =>
    MixinExpressionEvaluator.TryEvaluateExpression(
      Instruction.ValueExpression, Context, Locals, Variables, out value, out error
    );

  internal bool Interpolate(out string value, out string error) =>
    MixinExpressionEvaluator.TryInterpolate(
      Instruction.ValueExpression, Context, Locals, Variables, out value, out error
    );

  internal void Store(string local, object value) => Locals[local] = value;
  internal bool TryGetLocal(string local, out object value) => Locals.TryGetValue(local, out value);
  internal void EmitClass(string code) =>
    outputs.Add(new MixinExpressionOutput(MixinExpressionOutputTarget.Class, code));
}

internal abstract class DirectiveFunctionDefinition(
  string name,
  DirectiveOperandKind operandKind,
  int hoistedLocalArgumentIndex = -1
) : DirectiveDefinition(name, DirectiveOpcode.None, operandKind) {
  internal int HoistedLocalArgumentIndex { get; } = hoistedLocalArgumentIndex;
  internal abstract bool Invoke(DirectiveFunctionInvocation invocation, out string error);
}

internal static class DirectiveFunctionLibrary {
  private static readonly IReadOnlyDictionary<string, DirectiveFunctionDefinition> Definitions =
    new Dictionary<string, DirectiveFunctionDefinition>(StringComparer.Ordinal) {
      ["RESOLVE_MIXIN"] = new ResolveMixinDirectiveFunction(),
      ["PROP_STRUCT"] = new PropStructDirectiveFunction(),
      ["AUGMENT_STRUCT"] = new AugmentStructDirectiveFunction(),
      ["PUSH"] = new TableMutationDirectiveFunction("PUSH", true),
      ["PUT"] = new TableMutationDirectiveFunction("PUT", false)
    };

  internal static bool TryGet(string name, out DirectiveFunctionDefinition definition) =>
    Definitions.TryGetValue(name ?? "", out definition);
}

internal sealed class ResolveMixinDirectiveFunction()
  : DirectiveFunctionDefinition("RESOLVE_MIXIN", DirectiveOperandKind.Value, 0) {
  internal override bool Validate(IReadOnlyList<string> arguments, string operand, out string error) {
    if (arguments.Count == 1 && !string.IsNullOrEmpty(arguments[0])) { error = null; return true; }
    error = "RESOLVE_MIXIN requires a local name";
    return false;
  }

  internal override bool Invoke(DirectiveFunctionInvocation invocation, out string error) {
    if (invocation.Context is not IMixinExpressionSignatureContext context) {
      error = "the expression context does not support mixin resolution";
      return false;
    }
    if (!invocation.ResolveArgument(0, out var local, out error)) return false;
    if (string.IsNullOrEmpty(local)) { error = "RESOLVE_MIXIN local name is empty"; return false; }
    if (!invocation.Interpolate(out var target, out error)) return false;
    if (string.IsNullOrWhiteSpace(target)) { error = "RESOLVE_MIXIN target is empty"; return false; }
    if (!context.TryResolveMixin(target, out var callable, out error)) return false;
    invocation.Store(local, callable);
    return true;
  }
}

internal sealed class PropStructDirectiveFunction()
  : DirectiveFunctionDefinition("PROP_STRUCT", DirectiveOperandKind.Value, 1) {
  protected override int MaximumArguments => 4;

  internal override bool Validate(IReadOnlyList<string> arguments, string operand, out string error) {
    if (!base.Validate(arguments, operand, out error)) return false;
    if (arguments.Count < 2 ||
      string.IsNullOrEmpty(arguments[0]) || string.IsNullOrEmpty(arguments[1])) {
      error = "PROP_STRUCT requires a struct name and local name";
      return false;
    }
    var flags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    for (var index = 2; index < arguments.Count; index++) {
      var flag = arguments[index];
      if (MixinExpressionParser.IsDynamicArgument(flag)) continue;
      if (!string.Equals(flag, "datatype", StringComparison.OrdinalIgnoreCase) &&
        !string.Equals(flag, "noGenerate", StringComparison.OrdinalIgnoreCase)) {
        error = "unknown PROP_STRUCT flag '" + flag + "'";
        return false;
      }
      if (!flags.Add(flag)) {
        error = "PROP_STRUCT flag '" + flag + "' was specified more than once";
        return false;
      }
    }
    error = null;
    return true;
  }

  internal override bool Invoke(DirectiveFunctionInvocation invocation, out string error) {
    if (invocation.Context is not IMixinExpressionPropStructContext context) {
      error = "the expression context does not support prop structs";
      return false;
    }
    if (!invocation.ResolveArgument(0, out var name, out error) ||
      !invocation.ResolveArgument(1, out var local, out error)) return false;
    if (invocation.Instruction.ValueExpression is not { Count: 1 } expression ||
      expression[0].Reference is null) {
      error = "PROP_STRUCT syntax target must be a single reference";
      return false;
    }
    var datatype = false;
    var declaration = true;
    for (var index = 2; index < invocation.Instruction.Arguments.Count; index++) {
      if (!invocation.ResolveArgument(index, out var flag, out error)) return false;
      if (string.Equals(flag, "datatype", StringComparison.OrdinalIgnoreCase)) {
        if (datatype) { error = "PROP_STRUCT flag 'datatype' was specified more than once"; return false; }
        datatype = true;
      } else if (string.Equals(flag, "noGenerate", StringComparison.OrdinalIgnoreCase)) {
        if (!declaration) { error = "PROP_STRUCT flag 'noGenerate' was specified more than once"; return false; }
        declaration = false;
      } else { error = "unknown PROP_STRUCT flag '" + flag + "'"; return false; }
    }
    object handle;
    string code;
    if (context is IMixinExpressionConfigurablePropStructContext configurable) {
      if (!configurable.TryCreatePropStruct(
        name, expression[0].Reference, datatype && declaration, declaration,
        out handle, out code, out error
      )) return false;
    } else {
      if (datatype || !declaration) {
        error = "the expression context does not support configurable prop structs";
        return false;
      }
      if (!context.TryCreatePropStruct(name, expression[0].Reference, out handle, out code, out error))
        return false;
    }
    invocation.Store(local, handle);
    if (declaration) invocation.EmitClass(code);
    return true;
  }
}

internal sealed class AugmentStructDirectiveFunction()
  : DirectiveFunctionDefinition("AUGMENT_STRUCT", DirectiveOperandKind.Value, 0) {
  internal override bool Validate(IReadOnlyList<string> arguments, string operand, out string error) {
    if (arguments.Count == 1 && !string.IsNullOrEmpty(arguments[0])) { error = null; return true; }
    error = "AUGMENT_STRUCT requires a local name";
    return false;
  }

  internal override bool Invoke(DirectiveFunctionInvocation invocation, out string error) {
    if (invocation.Context is not IMixinExpressionStructAugmentationContext context) {
      error = "the expression context does not support struct augmentation";
      return false;
    }
    if (!invocation.ResolveArgument(0, out var local, out error)) return false;
    if (string.IsNullOrEmpty(local)) { error = "AUGMENT_STRUCT local name is empty"; return false; }
    if (invocation.Instruction.ValueExpression is not { Count: 1 } expression ||
      expression[0].Reference is null) {
      error = "AUGMENT_STRUCT syntax target must be a single reference";
      return false;
    }
    if (!context.TryAugmentPropStruct(
      expression[0].Reference, out var handle, out var code, out error
    )) return false;
    invocation.Store(local, handle);
    invocation.EmitClass(code);
    return true;
  }
}

internal sealed class TableMutationDirectiveFunction(string name, bool push)
  : DirectiveFunctionDefinition(name, DirectiveOperandKind.Value) {
  protected override int MaximumArguments => push ? 1 : 2;

  internal override bool Validate(IReadOnlyList<string> arguments, string operand, out string error) {
    var expected = push ? 1 : 2;
    if (arguments.Count == expected && !string.IsNullOrEmpty(arguments[0])) { error = null; return true; }
    error = Name + " requires " + (push ? "a local name" : "a local name and key");
    return false;
  }

  internal override bool Invoke(DirectiveFunctionInvocation invocation, out string error) {
    if (!invocation.ResolveArgument(0, out var local, out error)) return false;
    if (string.IsNullOrEmpty(local)) { error = Name + " local name is empty"; return false; }
    if (!invocation.Evaluate(out var item, out error)) return false;
    var table = invocation.TryGetLocal(local, out var current) && current is MixinExpressionTable existing
      ? existing
      : new MixinExpressionTable();
    string key;
    if (push) key = table.Count.ToString(CultureInfo.InvariantCulture);
    else if (!invocation.ResolveArgument(1, out key, out error)) return false;
    invocation.Store(local, table.Put(key, item));
    return true;
  }
}
