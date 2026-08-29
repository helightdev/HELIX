using System;
using System.Collections.Generic;
using HelixSourceGenerator.Language.Compiler;

namespace HelixSourceGenerator.Language.DirectiveFunctions;

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
    if (invocation.Instruction.Expression is not { Count: 1 } expression ||
      expression[0].Reference is null) {
      error = "PROP_STRUCT syntax target must be a single reference";
      return false;
    }
    var datatype = false;
    var declaration = true;
    for (var index = 2; index < invocation.Instruction.ParsedArguments.Count; index++) {
      if (!invocation.ResolveArgument(index, out var flag, out error)) return false;
      if (string.Equals(flag, "datatype", StringComparison.OrdinalIgnoreCase)) {
        if (datatype) {
          error = "PROP_STRUCT flag 'datatype' was specified more than once";
          return false;
        }
        datatype = true;
      } else if (string.Equals(flag, "noGenerate", StringComparison.OrdinalIgnoreCase)) {
        if (!declaration) {
          error = "PROP_STRUCT flag 'noGenerate' was specified more than once";
          return false;
        }
        declaration = false;
      } else {
        error = "unknown PROP_STRUCT flag '" + flag + "'";
        return false;
      }
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
    if (arguments.Count == 1 && !string.IsNullOrEmpty(arguments[0])) {
      error = null;
      return true;
    }
    error = "AUGMENT_STRUCT requires a local name";
    return false;
  }

  internal override bool Invoke(DirectiveFunctionInvocation invocation, out string error) {
    if (invocation.Context is not IMixinExpressionStructAugmentationContext context) {
      error = "the expression context does not support struct augmentation";
      return false;
    }
    if (!invocation.ResolveArgument(0, out var local, out error)) return false;
    if (string.IsNullOrEmpty(local)) {
      error = "AUGMENT_STRUCT local name is empty";
      return false;
    }
    if (invocation.Instruction.Expression is not { Count: 1 } expression ||
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