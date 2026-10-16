using System;
using Helix.Weave;

namespace Fixtures;

/// <summary>
/// A derivation step: runs for every seeded property whose field carries [Prop], and returns
/// the property the rest of the pipeline sees. Port of Hix's <c>derivation mixin</c>.
/// </summary>
[Mixin(Unit.Derivation, Order = 10)]
public sealed class PropAttribute : Attribute {
  public PropAttribute(string defaultValue = null) => DefaultValue = defaultValue;

  /// <summary>Literal C# for the parameter's default; null keeps the property required.</summary>
  public string DefaultValue { get; init; }
  public bool Equatable { get; init; } = true;

  [Weave]
  static DerivedProperty Derive(DerivationTarget t, PropAttribute a, DerivedProperty p) {
    if (!a.Equatable) p = p with { Equatable = false };
    if (a.DefaultValue is null) return p;

    return p with {
      Required = false,
      Default = a.DefaultValue,
      Parameter = $"{p.Type.Qualified} {p.Name} = {a.DefaultValue}",
    };
  }
}
