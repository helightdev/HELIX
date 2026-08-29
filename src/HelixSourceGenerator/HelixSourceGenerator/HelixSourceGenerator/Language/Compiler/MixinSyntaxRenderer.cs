using System.Collections.Generic;
using System.Linq;
using System.Text;
using HelixSourceGenerator.Language.Functions;

namespace HelixSourceGenerator.Language.Compiler;

/// <summary>Produces a canonical textual representation from semantic syntax nodes.</summary>
internal static class MixinSyntaxRenderer {
  internal static string Keyword(this MixinExpressionRoot root) => Root(root);

  internal static string DescribeFailedCondition(MixinExpressionReference reference) {
    var subject = reference.Root switch {
      MixinExpressionRoot.Variable => "Variable " + (reference.Member ?? "<unnamed>"),
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
      "eq" => subject + (predicate.Negated ? " is " : " is not ") + (expected.Length == 0 ? "the expected value" : expected),
      "exists" => subject + (predicate.Negated ? " exists" : " does not exist"),
      "is" => subject + (predicate.Negated ? " is of type " : " is not of type ") + expected,
      "has" => subject + (predicate.Negated ? " has member " : " does not have member ") + expected,
      "isSelf" => subject + (predicate.Negated ? " is the current type" : " is not the current type"),
      _ => subject + (predicate.Negated ? " is " : " is not ") + PredicateDescription(predicate.Name)
    };
  }

  private static string MemberSuffix(string member) => string.IsNullOrEmpty(member) ? "" : " member " + member;
  private static string PredicateDescription(string name) => name switch {
    "ref" => "a ref parameter", "in" => "an in parameter", "out" => "an out parameter",
    "inout" => "an in or out parameter", "argument" => "a normal argument", "static" => "static",
    "public" => "public", "exposed" => "public or internal", "top" => "a top-level type",
    "concrete" => "concrete", "partial" => "partial", "generic" => "generic",
    "struct" => "a struct", "class" => "a class", _ => name
  };
  internal static string RenderProgram(MixinProgramSyntax program) =>
    string.Join("\n", program.AvailableInstructions().Select(item => RenderInstruction(item)));

  internal static string RenderInstruction(DirectiveInstruction instruction, string firstArgument = null) {
    if (string.IsNullOrEmpty(instruction.Command)) return instruction.Operand ?? "";
    var builder = new StringBuilder("@").Append(instruction.Command);
    for (var index = 0; index < instruction.Arguments.Count; index++)
      builder.Append('<').Append(index == 0 && firstArgument is not null ? firstArgument : instruction.Arguments[index]).Append('>');
    if (!string.IsNullOrEmpty(instruction.Operand)) builder.Append(' ').Append(instruction.Operand);
    return RenderLogicalLine(builder.ToString());
  }

  internal static string RenderLogicalLine(string line) => (line ?? "").Replace("\n", "\n@\\");

  internal static string RenderArgument(DirectiveArgumentSyntax argument) => argument is null
    ? null
    : argument.IsDynamic ? "(" + RenderValue(argument.Expression) + ")" : argument.Literal;

  internal static string RenderValue(IReadOnlyList<ValueExpressionPart> expression) {
    var builder = new StringBuilder();
    var parts = expression ?? [];
    for (var index = 0; index < parts.Count; index++) {
      var part = parts[index];
      if (part.Reference is null) builder.Append((part.Literal ?? "").Replace("@", "@@"));
      else {
        var next = index + 1 < parts.Count ? parts[index + 1].Literal : null;
        builder.Append(RenderReference(part.Reference, next is { Length: > 0 } &&
          (char.IsLetterOrDigit(next[0]) || next[0] is '_' or '#' or ':')));
      }
    }
    return builder.ToString();
  }

  internal static string RenderBoolean(IReadOnlyList<MixinExpressionReference> expression) =>
    string.Join(" ", (expression ?? []).Where(item => item is not null).Select(RenderReference));

  internal static string RenderReference(MixinExpressionReference reference) => RenderReference(reference, false);

  private static string RenderReference(MixinExpressionReference reference, bool parenthesized) {
    var builder = new StringBuilder(parenthesized ? "@(" : "@").Append(Root(reference.Root));
    if (reference.Member is not null) builder.Append('#').Append(reference.Member);
    foreach (var property in reference.Properties) {
      builder.Append(':');
      if (property.Negated) builder.Append('!');
      builder.Append(property.Name);
      foreach (var argument in property.Arguments) builder.Append('<').Append(argument).Append('>');
    }
    if (parenthesized) builder.Append(')');
    return builder.ToString();
  }

  private static string Root(MixinExpressionRoot root) => root switch {
    MixinExpressionRoot.Target => "target", MixinExpressionRoot.This => "this",
    MixinExpressionRoot.Attribute => "attr", MixinExpressionRoot.Argument => "arg",
    MixinExpressionRoot.Variable => "var", MixinExpressionRoot.Local => "local",
    MixinExpressionRoot.True => "true", MixinExpressionRoot.False => "false",
    MixinExpressionRoot.Null => "null", MixinExpressionRoot.Table => "table",
    MixinExpressionRoot.Parameter => "param", MixinExpressionRoot.Carry => "carry", _ => "null"
  };
}
