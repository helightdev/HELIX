using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Helix.Weave.Compare;

/// <summary>
/// Compares two generated files by what they mean rather than how they are laid out: the set of
/// base types, the set of members, and — order-sensitively, because priority is the whole point —
/// the statement list of each generated method.
/// </summary>
public static class Semantic {
  public static IReadOnlyList<string> Compare(string left, string right) {
    var differences = new List<string>();
    var leftTypes = TypesOf(left);
    var rightTypes = TypesOf(right);

    foreach (var name in leftTypes.Keys.Union(rightTypes.Keys).OrderBy(key => key, StringComparer.Ordinal)) {
      var inLeft = leftTypes.TryGetValue(name, out var leftType);
      var inRight = rightTypes.TryGetValue(name, out var rightType);
      if (!inLeft) { differences.Add($"type only in weave: {name}"); continue; }
      if (!inRight) { differences.Add($"type only in hix:   {name}"); continue; }

      foreach (var missing in leftType.Bases.Except(rightType.Bases))
        differences.Add($"{name}: base only in hix:   {missing}");
      foreach (var extra in rightType.Bases.Except(leftType.Bases))
        differences.Add($"{name}: base only in weave: {extra}");

      foreach (var key in leftType.Members.Keys.Union(rightType.Members.Keys)
                 .OrderBy(key => key, StringComparer.Ordinal)) {
        var hasLeft = leftType.Members.TryGetValue(key, out var leftBody);
        var hasRight = rightType.Members.TryGetValue(key, out var rightBody);
        if (!hasLeft) { differences.Add($"{name}: member only in weave: {key}"); continue; }
        if (!hasRight) { differences.Add($"{name}: member only in hix:   {key}"); continue; }
        if (leftBody != rightBody) differences.Add($"{name}: body differs: {key}");
      }
    }
    return differences;
  }

  private sealed record TypeShape(HashSet<string> Bases, Dictionary<string, string> Members);

  private static Dictionary<string, TypeShape> TypesOf(string source) {
    var root = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview)).GetRoot();
    var result = new Dictionary<string, TypeShape>(StringComparer.Ordinal);

    foreach (var declaration in root.DescendantNodes().OfType<TypeDeclarationSyntax>()) {
      var name = QualifiedName(declaration);
      var bases = new HashSet<string>(
        declaration.BaseList?.Types.Select(type => Canonical(type.ToString())) ?? Enumerable.Empty<string>(),
        StringComparer.Ordinal);

      var members = new Dictionary<string, string>(StringComparer.Ordinal);
      foreach (var member in declaration.Members) {
        if (member is TypeDeclarationSyntax) continue; // handled on its own pass
        var key = Canonical(KeyOf(member));
        var body = Canonical(member.NormalizeWhitespace().ToFullString());
        members[key] = members.TryGetValue(key, out var existing) ? existing + "\n" + body : body;
      }
      result[name] = new TypeShape(bases, members);
    }
    return result;
  }

  /// <summary>A member's identity, so the same member can be found on both sides.</summary>
  private static string KeyOf(MemberDeclarationSyntax member) => member switch {
    MethodDeclarationSyntax method =>
      "method " + method.Identifier.Text + "(" +
      string.Join(",", method.ParameterList.Parameters.Select(p => p.Type?.ToString())) + ")",
    ConstructorDeclarationSyntax constructor =>
      "ctor(" + string.Join(",", constructor.ParameterList.Parameters.Select(p => p.Type?.ToString())) + ")",
    PropertyDeclarationSyntax property => "property " + property.Identifier.Text,
    FieldDeclarationSyntax field =>
      "field " + string.Join(",", field.Declaration.Variables.Select(v => v.Identifier.Text)),
    OperatorDeclarationSyntax op => "operator " + op.OperatorToken.Text,
    _ => member.Kind() + " " + member.ToString().Split('\n')[0].Trim(),
  };

  private static string QualifiedName(TypeDeclarationSyntax declaration) {
    var name = declaration.Identifier.Text;
    for (var parent = declaration.Parent; parent is not null; parent = parent.Parent)
      switch (parent) {
        case NamespaceDeclarationSyntax ns: name = ns.Name + "." + name; break;
        case FileScopedNamespaceDeclarationSyntax file: name = file.Name + "." + name; break;
        case TypeDeclarationSyntax type: name = type.Identifier.Text + "." + name; break;
      }
    return name;
  }

  /// <summary>Spelling differences that do not change meaning.</summary>
  private static readonly System.Text.RegularExpressions.Regex NullableForm =
    new(@"System\.Nullable<([^<>]+)>", System.Text.RegularExpressions.RegexOptions.Compiled);

  private static string Canonical(string text) => NullableForm.Replace(text, "$1?")
    .Replace("global::System.Boolean", "bool")
    .Replace("global::System.Int32", "int")
    .Replace("global::System.Int64", "long")
    .Replace("global::System.Single", "float")
    .Replace("global::System.Double", "double")
    .Replace("global::System.String", "string")
    .Replace("global::System.Object", "object")
    .Replace("System.Object", "object")
    .Replace("System.Boolean", "bool")
    .Replace("System.Int32", "int")
    .Replace("System.String", "string")
    .Replace("global::", "")
    .Replace("\r\n", "\n");
}
