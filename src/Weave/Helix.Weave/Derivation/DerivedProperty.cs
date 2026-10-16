namespace Helix.Weave;

/// <summary>
/// One property of a generated structure: the syntax fragments that constructors, equality and
/// datatype tables are joined out of. Every member is detached, so a DerivedProperty rides in a
/// carry record and is safe as a cache key.
/// </summary>
public sealed record DerivedProperty {
  public string Name { get; init; } = "";
  public PureTypeRef Type { get; init; }

  /// <summary>Call-site modifier with its trailing space, e.g. <c>"ref "</c>.</summary>
  public string Modifier { get; init; } = "";
  /// <summary>Declaration-site modifier with its trailing space; only <c>in</c> survives here.</summary>
  public string ParameterModifier { get; init; } = "";

  /// <summary>Parameter declaration, e.g. <c>in global::S.Thing value</c>.</summary>
  public string Parameter { get; init; } = "";
  /// <summary>Argument at a declaration-shaped call site, e.g. <c>in value</c>.</summary>
  public string Argument { get; init; } = "";
  /// <summary>Argument preserving the full modifier, e.g. <c>ref value</c>.</summary>
  public string CallArgument { get; init; } = "";
  /// <summary>Field assignment in a generated constructor.</summary>
  public string Assignment { get; init; } = "";

  /// <summary>Opaque to the core: whatever the consuming mixins mean by a datatype.</summary>
  public string Datatype { get; init; } = "";
  /// <summary>Extra configuration statements a derivation wants emitted alongside.</summary>
  public string Configuration { get; init; } = "";

  public bool Required { get; init; } = true;
  public string Default { get; init; } = "null";
  public bool Equatable { get; init; } = true;
  public bool Unsafe { get; init; }

  public string Equality { get; init; } = "";
  public string Hash { get; init; } = "";

  /// <summary>The symbol this was seeded from, for diagnostics only — never emitted.</summary>
  public int Order { get; init; }

  public override string ToString() => Parameter;
}
