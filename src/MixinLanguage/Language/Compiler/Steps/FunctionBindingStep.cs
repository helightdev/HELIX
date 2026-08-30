using System;
using System.Collections.Generic;
using System.Linq;

namespace MixinLanguage.Compiler;

public static partial class MixinExpressionCompiler {
  private sealed class FunctionBindingStep : MixinExpressionCompilerStep {
    public override bool TryTransform(
      MixinCompilerSyntax input, MixinExpressionPreparedState preparedState,
      out MixinCompilerSyntax output, out string error, out int errorLine
    ) {
      output = new MixinCompilerSyntax(Bind(input.Prelude), Bind(input.Expression));
      error = null;
      errorLine = 0;
      return true;
    }

    internal static MixinProgramSyntax Bind(MixinProgramSyntax program) {
      return new MixinProgramSyntax(
        program.AvailableInstructions().Select(instruction => RewriteReferences(instruction, reference => reference))
      );
    }

    internal static DirectiveInstruction RewriteReferences(
      DirectiveInstruction instruction,
      Func<MixinExpressionReference, MixinExpressionReference> rewrite
    ) {
      IReadOnlyList<IMixinValue> Value(IReadOnlyList<IMixinValue> value) {
        return [
          .. (value ?? []).Select(part => part.Reference is null
            ? part
            : new IMixinValue(null, RewriteComplete(part.Reference))
          )
        ];
      }

      IReadOnlyList<MixinExpressionReference> Boolean(IReadOnlyList<MixinExpressionReference> value) {
        return [.. (value ?? []).Select(item => item is null ? null : RewriteBoolean(item))];
      }

      DirectiveArgumentSyntax Argument(DirectiveArgumentSyntax value) {
        return value is null
          ? null
          : value.IsDynamic
            ? new DirectiveArgumentSyntax(null, Value(value.Expression))
            : value;
      }

      MixinExpressionReference RewriteComplete(MixinExpressionReference reference) {
        return rewrite(
          new MixinExpressionReference(
            reference.Root, reference.Member, [
              .. reference.Properties.Select(property =>
                RewriteProperty(property, RewriteComplete)
              )
            ], reference.Parenthesized
          )
        );
      }

      MixinExpressionReference RewriteBoolean(MixinExpressionReference reference) {
        return rewrite(
          new MixinExpressionReference(
            reference.Root, reference.Member, [
              .. reference.Properties.Select(property =>
                RewriteProperty(property, RewriteComplete)
              )
            ], reference.Parenthesized
          )
        );
      }

      return instruction switch {
        DirectiveInvocationSyntax item => new DirectiveInvocationSyntax(
          item.Line, item.Definition,
          [.. item.ParsedArguments.Select(Argument)], Value(item.Expression)
        ),
        CallDirectiveSyntax item => new CallDirectiveSyntax(
          item.Line, item.Function, item.ReturnLocal, Value(item.Expression)
        ),
        MatchDirectiveSyntax item => new MatchDirectiveSyntax(item.Line, item.FailureLabel, Boolean(item.Expression)),
        AssertDirectiveSyntax item => new AssertDirectiveSyntax(item.Line, Boolean(item.Expression)),
        CodeDirectiveSyntax item => new CodeDirectiveSyntax(
          item.Line, item.Target, item.InjectionTarget, Value(item.Expression)
        ),
        MixinDirectiveSyntax item => new MixinDirectiveSyntax(
          item.Line, Argument(item.Target), Argument(item.Priority), Value(item.Expression)
        ),
        UsingDirectiveSyntax item => new UsingDirectiveSyntax(item.Line, Value(item.Expression)),
        LogDirectiveSyntax item => new LogDirectiveSyntax(item.Line, Value(item.Expression)),
        LocalDirectiveSyntax item => new LocalDirectiveSyntax(item.Line, item.Name, Value(item.Expression)),
        VariableDirectiveSyntax item => new VariableDirectiveSyntax(item.Line, item.Name, Value(item.Expression)),
        TargetVariableDirectiveSyntax item => new TargetVariableDirectiveSyntax(
          item.Line, item.Name, Value(item.Expression)
        ),
        CarryDirectiveSyntax item => new CarryDirectiveSyntax(item.Line, item.Label, Value(item.Expression)),
        ReturnDirectiveSyntax item => new ReturnDirectiveSyntax(item.Line, Value(item.Expression)),
        FailDirectiveSyntax item => new FailDirectiveSyntax(item.Line, Value(item.Expression)), _ => instruction
      };
    }

    internal static MixinExpressionProperty RewriteProperty(
      MixinExpressionProperty property,
      Func<MixinExpressionReference, MixinExpressionReference> rewrite
    ) {
      if (property.ParsedArguments.Count == 0) {
        return new MixinExpressionProperty(
          property.Name, property.ParsedArguments, property.Negated,
          BoundFunction(property)
        );
      }
      var parsed = property.ParsedArguments.Select(argument => {
          var value = argument.ValueExpression is null
            ? null
            : argument.ValueExpression.Select(part =>
              part.Reference is null ? part : new IMixinValue(null, rewrite(part.Reference))
            ).ToArray();
          var boolean = argument.BooleanExpression is null
            ? null
            : argument.BooleanExpression.Select(rewrite).ToArray();
          return new MixinPropertyArgumentSyntax(argument.Literal, value, boolean);
        }
      ).ToArray();
      return new MixinExpressionProperty(property.Name, parsed, property.Negated, BoundFunction(property));
    }

    private static MixinLanguage.FunctionDefinition BoundFunction(
      MixinExpressionProperty property
    ) {
      if (property.Definition is not null) return property.Definition;
      FunctionLibrary.TryGet(property.Name, out var definition);
      return definition;
    }
  }
}