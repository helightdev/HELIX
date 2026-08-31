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

    internal static ProgramAst Bind(ProgramAst program) {
      return new ProgramAst(
        program.AvailableInstructions().Select(instruction => RewriteReferences(instruction, reference => reference))
      );
    }

    internal static DirectiveAst RewriteReferences(
      DirectiveAst ast,
      Func<MixinExpressionReference, MixinExpressionReference> rewrite
    ) {
      IReadOnlyList<ValueAst> Value(IReadOnlyList<ValueAst> value) {
        return [
          .. (value ?? []).Select(part => part.Reference is null
            ? part
            : new ValueAst(null, RewriteComplete(part.Reference))
          )
        ];
      }

      IReadOnlyList<MixinExpressionReference> Boolean(IReadOnlyList<MixinExpressionReference> value) {
        return [.. (value ?? []).Select(item => item is null ? null : RewriteBoolean(item))];
      }

      DirectiveArgumentAst Argument(DirectiveArgumentAst value) {
        return value is null
          ? null
          : value.IsDynamic
            ? new DirectiveArgumentAst(null, Value(value.Expression))
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

      return ast switch {
        DirectiveInvocationAst item => new DirectiveInvocationAst(
          item.SourceRange, item.Definition,
          [.. item.ParsedArguments.Select(Argument)], Value(item.Expression)
        ),
        CallDirectiveAst item => new CallDirectiveAst(
          item.SourceRange, item.Function, item.ReturnLocal, Value(item.Expression)
        ),
        MatchDirectiveAst item => new MatchDirectiveAst(item.SourceRange, item.FailureLabel, Boolean(item.Expression)),
        AssertDirectiveAst item => new AssertDirectiveAst(item.SourceRange, Boolean(item.Expression)),
        CodeDirectiveAst item => new CodeDirectiveAst(
          item.SourceRange, item.Target, item.InjectionTarget, Value(item.Expression)
        ),
        TargetedCodeDirectiveAst item => new TargetedCodeDirectiveAst(
          item.SourceRange, Argument(item.Target), Argument(item.Priority), Value(item.Expression)
        ),
        UsingDirectiveSyntax item => new UsingDirectiveSyntax(item.SourceRange, Value(item.Expression)),
        LogDirectiveSyntax item => new LogDirectiveSyntax(item.SourceRange, Value(item.Expression)),
        LocalDirectiveSyntax item => new LocalDirectiveSyntax(item.SourceRange, item.Name, Value(item.Expression)),
        VariableDirectiveAst item => new VariableDirectiveAst(item.SourceRange, item.Name, Value(item.Expression)),
        TargetVariableDirectiveAst item => new TargetVariableDirectiveAst(
          item.SourceRange, item.Name, Value(item.Expression)
        ),
        CarryDirectiveAst item => new CarryDirectiveAst(item.SourceRange, item.Label, Value(item.Expression)),
        ReturnDirectiveAst item => new ReturnDirectiveAst(item.SourceRange, Value(item.Expression)),
        FailDirectiveAst item => new FailDirectiveAst(item.SourceRange, Value(item.Expression)), _ => ast
      };
    }

    internal static MixinExpressionProperty RewriteProperty(
      MixinExpressionProperty property,
      Func<MixinExpressionReference, MixinExpressionReference> rewrite
    ) {
      if (property.ParsedArguments.Count == 0) {
        return new MixinExpressionProperty(
          property.Name, property.ParsedArguments, property.Negated,
          BoundFunction(property), property.SourceRange
        );
      }
      var parsed = property.ParsedArguments.Select(argument => {
          var value = argument.ValueExpression is null
            ? null
            : argument.ValueExpression.Select(part =>
              part.Reference is null ? part : new ValueAst(null, rewrite(part.Reference))
            ).ToArray();
          var boolean = argument.BooleanExpression is null
            ? null
            : argument.BooleanExpression.Select(rewrite).ToArray();
          return new MixinPropertyArgumentAst(argument.Literal, value, boolean, argument.SourceRange);
        }
      ).ToArray();
      return new MixinExpressionProperty(
        property.Name, parsed, property.Negated, BoundFunction(property), property.SourceRange
      );
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
