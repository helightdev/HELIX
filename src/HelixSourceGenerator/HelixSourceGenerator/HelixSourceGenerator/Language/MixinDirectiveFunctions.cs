using System;
using System.Collections.Generic;
using HelixSourceGenerator.Language.DirectiveFunctions;

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

  internal bool Evaluate(out object value, out string error) => MixinExpressionEvaluator.TryEvaluateExpression(
    Instruction.ValueExpression, Context, Locals, Variables, out value, out error
  );

  internal bool Interpolate(out string value, out string error) => MixinExpressionEvaluator.TryInterpolate(
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
      ["RESOLVE_MIXIN"] = new ResolveMixinDirectiveFunction(), ["PROP_STRUCT"] = new PropStructDirectiveFunction(),
      ["AUGMENT_STRUCT"] = new AugmentStructDirectiveFunction(), ["PUSH"] = new PushDirectiveFunction(),
      ["PUT"] = new PutDirectiveFunction()
    };

  internal static bool TryGet(string name, out DirectiveFunctionDefinition definition) =>
    Definitions.TryGetValue(name ?? "", out definition);
}