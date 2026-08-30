using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace HelixSourceGenerator.Language.Compiler;

public static partial class MixinExpressionCompiler {
  private sealed class PreludeHoistingStep : MixinExpressionCompilerStep {
    private static readonly HashSet<MixinExpressionRoot> RoslynRoots = [
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
      MixinProgramSyntax explicitPrelude,
      MixinProgramSyntax expression, out MixinProgramSyntax prelude, out MixinProgramSyntax lateExpression,
      out string error, out int errorLine
    ) {
      var generated = new List<DirectiveInstruction>();
      var labels = new Dictionary<string, string>(StringComparer.Ordinal);
      var structuralLocals = new HashSet<string>(StringComparer.Ordinal);
      var late = new List<DirectiveInstruction>();
      error = null;
      errorLine = 0;
      var localFunctions = new HashSet<string>(
        expression.AvailableInstructions()
          .OfType<FunctionDirectiveSyntax>().Select(item => item.Name), StringComparer.Ordinal
      );
      foreach (var parsed in expression.AvailableInstructions()) {
        if (parsed is CallDirectiveSyntax call && !localFunctions.Contains(call.Function ?? "")) {
          prelude = explicitPrelude;
          lateExpression = expression;
          error = "Prelude-model expressions may only call functions declared in the same expression; imported call '" +
            (call.Function ?? "") + "' is not supported";
          errorLine = parsed.Line;
          return false;
        }
        if (parsed is DirectiveInvocationSyntax {
          Definition: DirectiveFunctionDefinition { HoistedLocalArgumentIndex: >= 0 }
        }) {
          if (!TryHoistStructuralDirective(parsed, generated, labels, structuralLocals, out error)) {
            prelude = explicitPrelude;
            lateExpression = expression;
            errorLine = parsed.Line;
            return false;
          }
          late.Add(new EmptyDirectiveSyntax(parsed.Line));
          continue;
        }
        late.Add(RewriteRoslynReferences(parsed, generated, labels, structuralLocals));
      }
      prelude = new MixinProgramSyntax(explicitPrelude.AvailableInstructions().Concat(generated));
      lateExpression = new MixinProgramSyntax(late);
      return true;
    }

    private static bool TryHoistStructuralDirective(
      DirectiveInstruction instruction,
      ICollection<DirectiveInstruction> generated, IDictionary<string, string> labels,
      ISet<string> structuralLocals, out string error
    ) {
      var invocation = (DirectiveInvocationSyntax)instruction;
      var localIndex = invocation.Definition is DirectiveFunctionDefinition function
        ? function.HoistedLocalArgumentIndex
        : -1;
      var localArgument = localIndex >= 0 && invocation.ParsedArguments.Count > localIndex
        ? invocation.ParsedArguments[localIndex]
        : null;
      var local = localArgument?.Literal;
      if (string.IsNullOrEmpty(local) || localArgument.IsDynamic) {
        error = "@" + invocation.Definition.Name + " cannot be hoisted because its result local is dynamic";
        return false;
      }
      generated.Add(instruction);
      var localReference = new MixinExpressionReference(MixinExpressionRoot.Local, local, []);
      var label = "__" + labels.Count.ToString(CultureInfo.InvariantCulture);
      labels[MixinSyntaxRenderer.RenderReference(localReference)] = label;
      generated.Add(new CarryDirectiveSyntax(instruction.Line, label, [new IMixinValue(null, localReference)]));
      structuralLocals.Add(local);
      error = null;
      return true;
    }

    private static DirectiveInstruction RewriteRoslynReferences(
      DirectiveInstruction instruction,
      ICollection<DirectiveInstruction> generated, IDictionary<string, string> labels,
      ISet<string> structuralLocals
    ) {
      MixinExpressionReference Rewrite(MixinExpressionReference reference) {
        var roslyn = RoslynRoots.Contains(reference.Root) || (reference.Root == MixinExpressionRoot.Local &&
          structuralLocals?.Contains(reference.Member ?? "") == true);
        if (!roslyn) return RewriteNested(reference);
        var key = MixinSyntaxRenderer.RenderReference(reference);
        if (!labels.TryGetValue(key, out var label)) {
          label = "__" + labels.Count.ToString(CultureInfo.InvariantCulture);
          labels.Add(key, label);
          generated.Add(new CarryDirectiveSyntax(instruction.Line, label, [new IMixinValue(null, reference)]));
        }
        return new MixinExpressionReference(MixinExpressionRoot.Carry, label, [], reference.Parenthesized);
      }

      MixinExpressionReference RewriteNested(MixinExpressionReference reference) {
        return new MixinExpressionReference(
          reference.Root,
          reference.Member, reference.Properties.Select(property =>
            FunctionBindingStep.RewriteProperty(property, Rewrite)
          ).ToArray(), reference.Parenthesized
        );
      }

      return FunctionBindingStep.RewriteReferences(instruction, Rewrite);
    }

    internal static void HoistLateCarries(
      MixinProgramSyntax expression, MixinProgramSyntax lateExpression,
      out MixinProgramSyntax primary, out MixinProgramSyntax late
    ) {
      primary = expression;
      late = lateExpression;
      if (lateExpression is null || lateExpression.Count == 0) return;
      var carries = lateExpression.AvailableInstructions().OfType<CarryDirectiveSyntax>()
        .Cast<DirectiveInstruction>().ToArray();
      if (carries.Length == 0) return;
      primary = new MixinProgramSyntax(carries.Concat(expression.AvailableInstructions()));
      late = new MixinProgramSyntax(
        lateExpression.AvailableInstructions()
          .Where(item => item is not CarryDirectiveSyntax)
      );
    }
  }
}