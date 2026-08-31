using System;
using System.Collections.Generic;
using System.Linq;

namespace Mixins.Compiler.Steps;

public sealed class FunctionBindingStep : MixinExpressionCompilerStep {
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

  internal static InstructionAst RewriteReferences(
    InstructionAst ast,
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
        item.Definition,
        [.. item.ParsedArguments.Select(Argument)], Value(item.Expression)
      ).InheritFrom(item),
      CallAst item => new CallAst(
        item.Function, item.ReturnLocal, Value(item.Expression)
      ).InheritFrom(item),
      MatchAst item => new MatchAst(item.FailureLabel, Boolean(item.Expression)).InheritFrom(item),
      AssertAst item => new AssertAst(Boolean(item.Expression)).InheritFrom(item),
      CodeAst item => new CodeAst(
        item.Target, item.InjectionTarget, Value(item.Expression)
      ).InheritFrom(item),
      TargetedCodeAst item => new TargetedCodeAst(
        Argument(item.Target), Argument(item.Priority), Value(item.Expression)
      ).InheritFrom(item),
      UsingAst item => new UsingAst(Value(item.Expression)).InheritFrom(item),
      LogAst item => new LogAst(Value(item.Expression)).InheritFrom(item),
      LocalAst item => new LocalAst(item.Name, Value(item.Expression)).InheritFrom(item),
      VariableAst item => new VariableAst(item.Name, Value(item.Expression)).InheritFrom(item),
      TargetVariableAst item => new TargetVariableAst(
        item.Name, Value(item.Expression)
      ).InheritFrom(item),
      CarryAst item => new CarryAst(item.Label, Value(item.Expression)).InheritFrom(item),
      ReturnAst item => new ReturnAst(Value(item.Expression)).InheritFrom(item),
      FailAst item => new FailAst(Value(item.Expression)).InheritFrom(item), _ => ast
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

  private static FunctionDefinition BoundFunction(
    MixinExpressionProperty property
  ) {
    if (property.Definition is not null) return property.Definition;
    FunctionLibrary.TryGet(property.Name, property.ParsedArguments.Count, out var definition);
    return definition;
  }
}
