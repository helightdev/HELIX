using System;
using System.Linq;
using Helix.Weave;

namespace Fixtures;

/// <summary>
/// Consumes the derivation pipeline: derives the type's fields, then joins the fragments into a
/// constructor and value equality. The shape of Hix's <c>StructureAttribute</c>.
/// </summary>
[Mixin(Unit.Type)]
public sealed class StructureAttribute : Attribute {
  public bool Equality { get; init; } = true;

  [Weave]
  static void Apply(TypeTarget t, StructureAttribute a) {
    var props = t.DeriveFields();
    if (props.Length == 0) return;

    // Required parameters cannot follow optional ones; say so instead of emitting broken code.
    var firstOptional = props.ToList().FindIndex(p => !p.Required);
    if (firstOptional >= 0 && props.Skip(firstOptional).Any(p => p.Required)) {
      t.Report("FX010", "a required property cannot follow one with a default");
      return;
    }

    var unsafeness = props.Any(p => p.Unsafe) ? "unsafe " : "";
    t.Class($$"""
      public {{unsafeness}}{{t.SelfName}}({{Code.Join(props, p => p.Parameter)}}) {
        {{Code.Join(props, p => p.Assignment, "\n  ")}}
      }
      """);

    if (!a.Equality) return;

    var equatable = props.Where(p => p.Equatable).ToList();
    var comparisons = equatable.Count == 0 ? "true" : Code.Join(equatable, p => p.Equality, " && ");

    t.Implements($"global::System.IEquatable<{t.TypeRef.Qualified}>");
    t.Class($$"""
      public {{unsafeness}}bool Equals({{t.TypeRef.Qualified}} other) => {{comparisons}};
      public override {{unsafeness}}bool Equals(object obj) =>
        obj is {{t.TypeRef.Qualified}} other && Equals(other);
      public override {{unsafeness}}int GetHashCode() {
        var hashCode = new global::System.HashCode();
        {{Code.Join(equatable, p => $"hashCode.Add({p.Hash});", "\n  ")}}
        return hashCode.ToHashCode();
      }
      """);
  }
}
