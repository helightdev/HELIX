using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Mixins.Compiler.Steps;

public sealed class PreludeHoistingStep : MixinExpressionCompilerStep {
  private static readonly HashSet<MixinExpressionRoot> _roslynRoots = [
    MixinExpressionRoot.Target, MixinExpressionRoot.This, MixinExpressionRoot.Attribute, MixinExpressionRoot.Argument
  ];

  public override bool TryTransform(
    MixinCompilerSyntax input, MixinExpressionPreparedState preparedState,
    out MixinCompilerSyntax output, out string error, out int errorLine
  ) {
    if (!TryHoistRoslynReferences(
      input.Prelude, input.Expression,
      out var prelude, out var late, out error, out errorLine
    )) {
      output = input;
      return false;
    }
    output = new MixinCompilerSyntax(prelude, late);
    return true;
  }

  private static bool TryHoistRoslynReferences(
    ProgramAst explicitPrelude,
    ProgramAst expression, out ProgramAst prelude, out ProgramAst lateExpression,
    out string error, out int errorLine
  ) {
    var generated = new List<InstructionAst>();
    var labels = new Dictionary<string, string>(StringComparer.Ordinal);
    var structuralLocals = new HashSet<string>(StringComparer.Ordinal);
    var late = new List<InstructionAst>();
    error = null;
    errorLine = 0;
    var localFunctions = new HashSet<string>(
      expression.AvailableInstructions().OfType<FunctionAst>().Select(item => item.Name), StringComparer.Ordinal
    );
    foreach (var parsed in expression.AvailableInstructions()) {
      switch (parsed) {
        case CallAst call when !localFunctions.Contains(call.Function ?? ""):
          prelude = explicitPrelude;
          lateExpression = expression;
          error =
            $"Prelude-model expressions may only call functions declared in the same expression; " +
            $"imported call '{call.Function ?? ""}' is not supported";
          errorLine = parsed.Line;
          return false;
        case DirectiveInvocationAst { Definition.HoistedLocalArgumentIndex: >= 0 }
          when !TryHoistStructuralDirective(parsed, generated, labels, structuralLocals, out error):
          prelude = explicitPrelude;
          lateExpression = expression;
          errorLine = parsed.Line;
          return false;
        case DirectiveInvocationAst { Definition.HoistedLocalArgumentIndex: >= 0 }:
          late.Add(new EmptyDirectiveAst().InheritFrom(parsed));
          continue;
        default: late.Add(RewriteRoslynReferences(parsed, generated, labels, structuralLocals)); break;
      }
    }
    prelude = new ProgramAst(explicitPrelude.AvailableInstructions().Concat(generated));
    lateExpression = new ProgramAst(late);
    return true;
  }

  private static bool TryHoistStructuralDirective(
    InstructionAst ast,
    ICollection<InstructionAst> generated, IDictionary<string, string> labels,
    ISet<string> structuralLocals, out string error
  ) {
    var invocation = (DirectiveInvocationAst)ast;
    var localIndex = invocation.Definition.HoistedLocalArgumentIndex;
    var localArgument = localIndex >= 0 && invocation.ParsedArguments.Count > localIndex
      ? invocation.ParsedArguments[localIndex]
      : null;
    var local = localArgument?.Literal;
    if (string.IsNullOrEmpty(local) || localArgument.IsDynamic) {
      error = $"@{invocation.Definition.Name} cannot be hoisted because its result local is dynamic";
      return false;
    }
    generated.Add(ast);
    var localReference = new MixinExpressionReference(MixinExpressionRoot.Local, local, []);
    var label = $"__{labels.Count.ToString(CultureInfo.InvariantCulture)}";
    labels[MixinSyntaxRenderer.RenderReference(localReference)] = label;
    generated.Add(
      new CarryAst(label, [new ValueAst(null, localReference)]).InheritFrom(ast).WithDefinition("CARRY", 1)
    );
    structuralLocals.Add(local);
    error = null;
    return true;
  }

  private static InstructionAst RewriteRoslynReferences(
    InstructionAst ast,
    ICollection<InstructionAst> generated, IDictionary<string, string> labels,
    ISet<string> structuralLocals
  ) {
    MixinExpressionReference Rewrite(MixinExpressionReference reference) {
      var roslyn = _roslynRoots.Contains(reference.Root) || (reference.Root == MixinExpressionRoot.Local &&
        structuralLocals?.Contains(reference.Member ?? "") == true);
      if (!roslyn) return RewriteNested(reference);
      var key = MixinSyntaxRenderer.RenderReference(reference);
      if (labels.TryGetValue(key, out var label)) {
        return new MixinExpressionReference(
          MixinExpressionRoot.Carry, label, [], reference.Parenthesized, reference.SourceRange
        );
      }
      label = $"__{labels.Count.ToString(CultureInfo.InvariantCulture)}";
      labels.Add(key, label);
      generated.Add(
        new CarryAst(label, [new ValueAst(null, reference)]).InheritFrom(ast).WithDefinition("CARRY", 1)
      );
      return new MixinExpressionReference(
        MixinExpressionRoot.Carry, label, [], reference.Parenthesized, reference.SourceRange
      );
    }

    MixinExpressionReference RewriteNested(MixinExpressionReference reference) {
      return new MixinExpressionReference(
        reference.Root,
        reference.Member,
        [.. reference.Properties.Select(property => FunctionBindingStep.RewriteProperty(property, Rewrite))],
        reference.Parenthesized, reference.SourceRange
      );
    }

    return FunctionBindingStep.RewriteReferences(ast, Rewrite);
  }

  internal static void HoistLateCarries(
    ProgramAst expression, ProgramAst lateExpression,
    out ProgramAst primary, out ProgramAst late
  ) {
    primary = expression;
    late = lateExpression;
    if (lateExpression is null || lateExpression.Count == 0) return;
    var carries = lateExpression.AvailableInstructions().OfType<CarryAst>().Cast<InstructionAst>().ToArray();
    if (carries.Length == 0) return;
    primary = new ProgramAst(carries.Concat(expression.AvailableInstructions()));
    late = new ProgramAst(lateExpression.AvailableInstructions().Where(item => item is not CarryAst));
  }
}