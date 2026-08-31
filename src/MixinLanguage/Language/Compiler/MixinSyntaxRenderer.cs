using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MixinLanguage.Compiler;

/// <summary>Produces a canonical textual representation from semantic syntax nodes.</summary>
public static class MixinSyntaxRenderer {
  internal static string Keyword(this MixinExpressionRoot root) {
    return MixinRootLibrary.TryGet(root, out var definition) ? definition.Name : "null";
  }

  internal static string DescribeFailedCondition(MixinExpressionReference reference) {
    var subject = reference.Root switch {
      MixinExpressionRoot.Variable => "Variable " + (reference.Member ?? "<unnamed>"),
      MixinExpressionRoot.TargetVariable => "Target variable " + (reference.Member ?? "<unnamed>"),
      MixinExpressionRoot.Local => "Local variable " + (reference.Member ?? "<unnamed>"),
      MixinExpressionRoot.Argument => "Argument " + (reference.Member ?? "<unspecified>"),
      MixinExpressionRoot.This => "Current type" + MemberSuffix(reference.Member),
      MixinExpressionRoot.Target => "Target" + MemberSuffix(reference.Member),
      MixinExpressionRoot.Attribute => "Attribute" + MemberSuffix(reference.Member),
      _ => "Value @" + reference.Root.Keyword() + MemberSuffix(reference.Member)
    };
    var predicate = reference.Properties.FirstOrDefault(item => FunctionLibrary.IsPredicate(item.Name));
    if (predicate is null) return subject + " is null, false or invalid";
    var expected = predicate.Argument ?? "";
    return predicate.Name switch {
      "eq" => subject + (predicate.Negated ? " is " : " is not ") +
        (expected.Length == 0 ? "the expected value" : expected),
      "exists" => subject + (predicate.Negated ? " exists" : " does not exist"),
      "is" => subject + (predicate.Negated ? " is of type " : " is not of type ") + expected,
      "has" => subject + (predicate.Negated ? " has member " : " does not have member ") + expected,
      "isSelf" => subject + (predicate.Negated ? " is the current type" : " is not the current type"),
      _ => subject + (predicate.Negated ? " is " : " is not ") + PredicateDescription(predicate.Name)
    };
  }

  private static string MemberSuffix(string member) {
    return string.IsNullOrEmpty(member) ? "" : " member " + member;
  }

  private static string PredicateDescription(string name) {
    return name switch {
      "ref" => "a ref parameter", "in" => "an in parameter", "out" => "an out parameter",
      "inout" => "an in or out parameter", "argument" => "a normal argument", "static" => "static",
      "public" => "public", "exposed" => "public or internal", "top" => "a top-level type",
      "concrete" => "concrete", "partial" => "partial", "generic" => "generic",
      "struct" => "a struct", "class" => "a class", _ => name
    };
  }

  public static string RenderProgram(ProgramAst program) {
    return RenderInstructions(program.AvailableInstructions());
  }

  internal static string RenderInstructions(IEnumerable<DirectiveAst> instructions) {
    return string.Join("\n", instructions.Select(item => RenderInstruction(item)));
  }

  internal static string RenderInstruction(DirectiveAst ast, string firstArgument = null) {
    var command = MixinSyntaxFacts.Command(ast);
    var arguments = MixinSyntaxFacts.Arguments(ast);
    var operand = MixinSyntaxFacts.Operand(ast);
    if (string.IsNullOrEmpty(command)) return operand ?? "";
    var builder = new StringBuilder("@").Append(command);
    for (var index = 0; index < arguments.Count; index++) {
      builder.Append('<').Append(index == 0 && firstArgument is not null ? firstArgument : arguments[index])
        .Append('>');
    }
    if (!string.IsNullOrEmpty(operand)) builder.Append(' ').Append(operand);
    return RenderLogicalLine(builder.ToString());
  }

  internal static string RenderLogicalLine(string line) {
    return (line ?? "").Replace("\n", "\n@\\");
  }

  internal static string RenderArgument(DirectiveArgumentAst argument) {
    return argument is null
      ? null
      : argument.IsDynamic
        ? "(" + RenderValue(argument.Expression) + ")"
        : argument.Literal;
  }

  internal static string RenderValue(IReadOnlyList<ValueAst> expression) {
    var builder = new StringBuilder();
    var parts = expression ?? [];
    for (var index = 0; index < parts.Count; index++) {
      var part = parts[index];
      if (part.Reference is null)
        builder.Append(part.Verbatim ? part.Literal ?? "" : (part.Literal ?? "").Replace("@", "@@"));
      else {
        var next = index + 1 < parts.Count ? parts[index + 1].Literal : null;
        builder.Append(
          RenderReference(
            part.Reference, part.Reference.Parenthesized || (next is { Length: > 0 } &&
              (char.IsLetterOrDigit(next[0]) || next[0] is '_' or '#' or ':'))
          )
        );
      }
    }
    return builder.ToString();
  }

  internal static string RenderBoolean(IReadOnlyList<MixinExpressionReference> expression) {
    return string.Join(" ", (expression ?? []).Where(item => item is not null).Select(RenderReference));
  }

  internal static string RenderReference(MixinExpressionReference reference) {
    return RenderReference(reference, reference.Parenthesized);
  }

  private static string RenderReference(MixinExpressionReference reference, bool parenthesized) {
    var builder = new StringBuilder(parenthesized ? "@(" : "@").Append(reference.Root.Keyword());
    if (reference.Member is not null) builder.Append('#').Append(reference.Member);
    foreach (var property in reference.Properties) {
      builder.Append(':');
      if (property.Negated) builder.Append('!');
      builder.Append(property.Name);
      foreach (var argument in property.ParsedArguments) {
        builder.Append('<');
        if (argument.Literal is not null) builder.Append(argument.Literal);
        else if (argument.BooleanExpression is not null)
          builder.Append('(').Append(RenderBoolean(argument.BooleanExpression)).Append(')');
        else builder.Append('(').Append(RenderValue(argument.ValueExpression)).Append(')');
        builder.Append('>');
      }
    }
    if (parenthesized) builder.Append(')');
    return builder.ToString();
  }

}
