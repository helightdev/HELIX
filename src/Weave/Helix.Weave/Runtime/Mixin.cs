using Microsoft.CodeAnalysis;

namespace Helix.Weave;

/// <summary>
/// What lowering generates one of per mixin dataclass. The relocated weave methods are called
/// from here; the carry is opaque to the host, which only hands it back to the late pass.
/// </summary>
public abstract class Mixin {
  /// <summary>Metadata name of the triggering attribute, e.g. <c>HELIX.Context.InjectAttribute</c>.</summary>
  public abstract string AttributeName { get; }
  public abstract Unit Unit { get; }
  public virtual int Order => 0;
  public virtual bool HasLate => false;

  /// <summary>
  /// True when the prelude queries the whole compilation rather than just its own symbol, as
  /// module discovery does. Such a mixin cannot be cached per symbol, so it is declared rather
  /// than guessed.
  /// </summary>
  public virtual bool WholeCompilation => false;

  /// <summary>
  /// Defines the injection-target aliases this mixin's attribute implies, for any type carrying
  /// it. Called for every symbol on such a type, before its prelude.
  /// </summary>
  public virtual void DefineAliases(TypeTarget target) { }

  /// <summary>Runs the prelude. Returns the carry when there is a late pass, otherwise null.</summary>
  public abstract object Prelude(SymbolTarget target, AttributeData attribute);

  /// <summary>Runs the late pass against the carry the prelude returned.</summary>
  public virtual void Late(LateTarget target, object carry) { }

  /// <summary>
  /// Runs one step of the property-derivation pipeline. Only <see cref="Unit.Derivation"/>
  /// mixins override this; they are not driven by the attribute pipeline but consulted by
  /// DeriveFields/DeriveParameters from inside another mixin's prelude.
  /// </summary>
  public virtual DerivedProperty Derive(DerivationTarget target, AttributeData attribute,
                                        DerivedProperty property) => property;
}

/// <summary>The closed set of mixins, generated so dispatch needs no reflection.</summary>
public interface IMixinRegistry {
  System.Collections.Generic.IReadOnlyList<Mixin> Mixins { get; }
}
