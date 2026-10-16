using Microsoft.CodeAnalysis;

namespace Helix.Weave;

/// <summary>
/// Builds the initial <see cref="DerivedProperty"/> for a field or parameter, before any
/// derivation step runs. Port of Hix's <c>SeedProp</c>, minus the HELIX-specific datatype table —
/// that belongs to the mixins that consume this, not to the core.
/// </summary>
public static class PropertySeed {
  public static DerivedProperty Seed(ISymbol symbol, SymbolFacts facts, int order = 0) {
    var type = symbol switch {
      IFieldSymbol field => field.Type,
      IParameterSymbol parameter => parameter.Type,
      IPropertySymbol property => property.Type,
      _ => null,
    };

    var refKind = symbol is IParameterSymbol parameterSymbol ? parameterSymbol.RefKind : RefKind.None;
    var pure = facts.Detach(type);
    var name = symbol.Name;
    var qualified = pure.Qualified ?? "object";

    // `in` is the only modifier that belongs on both sides; ref and out are call-site only.
    var modifier = refKind switch {
      RefKind.In => "in ",
      RefKind.Ref => "ref ",
      RefKind.Out => "out ",
      _ => "",
    };
    var parameterModifier = refKind == RefKind.In ? "in " : "";

    // Enums compare with == directly, like primitives; only genuinely opaque types need the
    // comparer.
    var equality = pure.IsPrimitive || pure.IsPointer || pure.IsEnum
      ? $"this.{name} == other.{name}"
      : $"global::System.Collections.Generic.EqualityComparer<{qualified}>.Default" +
        $".Equals(this.{name}, other.{name})";

    var hash = pure.IsPointer ? $"((global::System.IntPtr)this.{name})" : $"this.{name}";

    return new DerivedProperty {
      Name = name,
      Type = pure,
      Modifier = modifier,
      ParameterModifier = parameterModifier,
      Parameter = $"{parameterModifier}{qualified} {name}",
      Argument = $"{parameterModifier}{name}",
      CallArgument = $"{modifier}{name}",
      Assignment = $"this.{name} = {name};",
      Unsafe = pure.ContainsPointer,
      Equality = equality,
      Hash = hash,
      Order = order,
    };
  }

  /// <summary>Instance fields, in declaration order — Hix's <c>KeepInstanceField</c>.</summary>
  public static bool IsInstanceField(ISymbol symbol) =>
    symbol is IFieldSymbol { IsStatic: false, IsImplicitlyDeclared: false, AssociatedSymbol: null };
}
