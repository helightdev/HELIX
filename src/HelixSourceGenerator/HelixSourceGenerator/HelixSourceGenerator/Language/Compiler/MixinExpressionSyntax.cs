using System.Collections.Generic;
using System.Linq;

namespace HelixSourceGenerator.Language.Compiler;

/// <summary>Parser-only reference syntax. This type never crosses the lowering boundary.</summary>
public sealed class MixinExpressionReference(
  MixinExpressionRoot root, string member, IReadOnlyList<MixinExpressionProperty> properties,
  bool parenthesized = false
) {
  public MixinExpressionRoot Root { get; } = root;
  public string Member { get; } = member;
  public IReadOnlyList<MixinExpressionProperty> Properties { get; } = properties ?? [];
  internal bool Parenthesized { get; } = parenthesized;
  internal void CollectConstants(MixinStringPoolBuilder pool) {
    pool.Intern(Member);
    foreach (var property in Properties) property.CollectConstants(pool);
  }
}

public sealed class MixinExpressionProperty(
  string name, IReadOnlyList<MixinPropertyArgumentSyntax> arguments, bool negated = false,
  FunctionDefinition definition = null
) {
  internal MixinExpressionProperty(string name, string argument) : this(
    name, argument is null ? [] : [new MixinPropertyArgumentSyntax(argument, null, null)]
  ) { }
  public string Name { get; } = name;
  public IReadOnlyList<MixinPropertyArgumentSyntax> ParsedArguments { get; } = arguments ?? [];
  public IReadOnlyList<string> Arguments { get; } = [.. (arguments ?? []).Select(item => item.Literal)];
  public string Argument => Arguments.Count == 0 ? null : Arguments[0];
  public bool Negated { get; } = negated;
  internal FunctionDefinition Definition { get; } = definition;
  internal void CollectConstants(MixinStringPoolBuilder pool) {
    pool.Intern(Name);
    foreach (var argument in ParsedArguments) {
      pool.Intern(argument.Literal);
      foreach (var part in argument.ValueExpression ?? []) {
        if (part.Reference is null) pool.Intern(part.Literal); else part.Reference.CollectConstants(pool);
      }
      foreach (var reference in argument.BooleanExpression ?? []) reference.CollectConstants(pool);
    }
  }
}
