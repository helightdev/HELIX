using System;

namespace Helix.Weave;

/// <summary>What a mixin applies to.</summary>
public enum Unit {
  Type,
  Method,
  Field,
  Property,
  /// <summary>A field or a property — most member attributes accept either.</summary>
  Member,
  Parameter,
  /// <summary>A step in the property-derivation pipeline rather than a direct emission.</summary>
  Derivation,
}

/// <summary>
/// Marks a dataclass as a mixin: its properties are the attribute's arguments, its weave
/// methods are the implementation. This declaration is the entire meta surface.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class MixinAttribute : Attribute {
  public MixinAttribute(Unit unit) => Unit = unit;
  public Unit Unit { get; }
  /// <summary>Relative order against other mixins on the same symbol.</summary>
  public int Order { get; set; }
  /// <summary>
  /// Metadata name of the attribute that triggers this mixin. Defaults to the dataclass's own
  /// fully qualified name, which is the case that needs no thought.
  /// </summary>
  public string Attribute { get; set; }

  /// <summary>
  /// Set when the mixin queries the whole compilation (module discovery). Such a mixin is not
  /// cached per symbol, because an unrelated type elsewhere changes its answer.
  /// </summary>
  public bool WholeCompilation { get; set; }
}

/// <summary>
/// Declares the injection-target aliases this mixin's attribute implies for a type. Runs before
/// any prelude on that type, and only ever reads the type — so it is a pure function of the
/// type's attributes rather than state one mixin leaves for another.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class AliasesAttribute : Attribute { }

/// <summary>A single-pass weave method: prelude only.</summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class WeaveAttribute : Attribute { }

/// <summary>The prelude of a two-pass mixin. Returns the carry record.</summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class PreludeAttribute : Attribute { }

/// <summary>
/// The late pass. Takes the carry record and nothing else — no attribute, no symbols —
/// so that it stays pure with respect to the analyzed source.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class LateAttribute : Attribute { }

/// <summary>Declares an injection target constant, lowered to an <see cref="InjectionTarget"/>.</summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class InjectionTargetAttribute : Attribute {
  public InjectionTargetAttribute(string name = null) => Name = name;
  public string Name { get; }
  public Type Delegate { get; set; }
  public bool Static { get; set; }
  public bool Public { get; set; }
}
