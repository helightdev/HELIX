// Port of Core.hix: the datatype mapping, the property derivations and the structure mixins.
using System;
using System.Linq;
using Helix.Weave;
using Microsoft.CodeAnalysis;

namespace HELIX.Weave;

/// <summary>
/// The HELIX datatype a property maps to. Lives here rather than in the weave core, which has no
/// opinion about what a datatype is. Hix's <c>SeedProp</c> datatype table.
/// </summary>
public static class Datatypes {
  public static string For(PureTypeRef type, string self) => type.MetadataName switch {
    "System.String" => "global::HELIX.Datatypes.String",
    "System.Int32" => "global::HELIX.Datatypes.Int",
    "System.Int64" => "global::HELIX.Datatypes.Long",
    "System.Single" => "global::HELIX.Datatypes.Float",
    "System.Double" => "global::HELIX.Datatypes.Double",
    "System.Boolean" => "global::HELIX.Datatypes.Bool",
    "UnityEngine.Color" => "global::HELIX.Datatypes.Color",
    "UnityEngine.Vector2" => "global::HELIX.Datatypes.Vector2",
    "UnityEngine.Vector3" => "global::HELIX.Datatypes.Vector3",
    "UnityEngine.Vector4" => "global::HELIX.Datatypes.Vector4",
    _ => type.IsEnum
      ? $"global::HELIX.Datatypes.Enum<{self}>()"
      : $"global::HELIX.Datatypes.Object<{self}>()",
  };
}

/// <summary>Port of <c>derivation mixin HELIX.PropertyDatatypeAttribute</c>.</summary>
[Mixin(Unit.Derivation, Order = -50, Attribute = "HELIX.PropertyDatatypeAttribute")]
public sealed class PropertyDatatype : Attribute {
  public PropertyDatatype(string datatype = null) => this.datatype = datatype;
  public string datatype;

  [Weave]
  static DerivedProperty Derive(DerivationTarget t, PropertyDatatype a, DerivedProperty p) =>
    string.IsNullOrWhiteSpace(a.datatype) ? p : p with { Datatype = a.datatype };
}

/// <summary>How a [Prop] supplies its default. Mirrors HELIX.PropInit exactly, values included.</summary>
public enum PropInit {
  /// <summary>The literal value is used as the constructor parameter initializer.</summary>
  Literal = 0,
  /// <summary>The string content is inserted as a constant initializer expression.</summary>
  Constant = 1,
  /// <summary>The parameter is nullable and defaults to null; the content is used when it is null.</summary>
  Deferred = 2,
  None = 3,
}

/// <summary>
/// Port of <c>derivation mixin HELIX.PropAttribute</c>. The member names mirror HELIX's attribute
/// exactly — including the camelCase public fields — because that is what attribute arguments
/// bind to by name.
///
/// The tangle of <c>:eq(3)</c> / <c>:eq&lt;None&gt;</c> / <c>:eq&lt;HELIX.PropInit.None&gt;</c>
/// in the original was Hix failing to decode an enum; here it is one comparison.
/// </summary>
[Mixin(Unit.Derivation, Attribute = "HELIX.PropAttribute")]
public sealed class Prop : Attribute {
  public Literal defaultValue;
  public PropInit defaultInit = PropInit.None;

  public bool Equatable { get; set; } = true;
  public string EqualitySyntax { get; set; } = "{0} == {1}";
  public string HashCodeSyntax { get; set; } = "{0}";
  public string Datatype { get; set; }

  private static readonly Slot<bool> SawOptional = new(Scope.Type);

  [Weave]
  static DerivedProperty Derive(DerivationTarget t, Prop a, DerivedProperty p) {
    if (!a.Equatable) p = p with { Equatable = false };
    if (!string.IsNullOrWhiteSpace(a.Datatype)) p = p with { Datatype = a.Datatype };
    p = WithEqualityOverrides(a, p);

    if (a.defaultInit == PropInit.None) {
      // Hix asserted this: once a property has a default, the rest must have one too.
      if (t.Get(SawOptional))
        t.Report("HX0101", $"required property '{p.Name}' cannot follow one with a default");
      return p;
    }

    if (t.Symbol is IParameterSymbol { RefKind: not RefKind.None }) {
      t.Report("HX0102", $"'{p.Name}' cannot have a default and a by-reference modifier");
      return p;
    }

    t.Set(SawOptional, true);

    // Literal: the value as a C# literal. Constant and Deferred: the string content is source.
    var value = a.defaultInit == PropInit.Literal
      ? a.defaultValue.Exists ? a.defaultValue.Text : "null"
      : a.defaultValue.Source ?? "null";

    if (a.defaultInit == PropInit.Deferred)
      return p with {
        Required = false,
        Default = value,
        Parameter = $"{Nullable(p)} {p.Name} = null",
        Assignment = $"this.{p.Name} = {p.Name} ?? {value};",
      };

    return p with {
      Required = false,
      Default = value,
      Parameter = $"{p.Type.Qualified} {p.Name} = {value}",
    };
  }

  /// <summary>A deferred default needs somewhere to put "not supplied".</summary>
  private static string Nullable(DerivedProperty p) =>
    p.Type.Facts.HasFlag(TypeFacts.ValueType) && !p.Type.Qualified.EndsWith("?", StringComparison.Ordinal)
      ? p.Type.Qualified + "?"
      : p.Type.Qualified;

  private static DerivedProperty WithEqualityOverrides(Prop a, DerivedProperty p) {
    if (p.Type.IsPrimitive || p.Type.IsPointer) return p;

    // The defaults are "{0} == {1}" and "{0}", which is what the seed already produced for
    // comparable types; only a real override is worth applying.
    if (!string.IsNullOrWhiteSpace(a.EqualitySyntax) && a.EqualitySyntax != "{0} == {1}")
      p = p with {
        Equality = a.EqualitySyntax.Replace("{0}", $"this.{p.Name}").Replace("{1}", $"other.{p.Name}"),
      };

    if (!string.IsNullOrWhiteSpace(a.HashCodeSyntax) && a.HashCodeSyntax != "{0}")
      p = p with { Hash = a.HashCodeSyntax.Replace("{0}", $"this.{p.Name}") };

    return p;
  }
}

/// <summary>Port of <c>mixin HELIX.StructureAttribute</c>.</summary>
[Mixin(Unit.Type, Attribute = "HELIX.StructureAttribute")]
public sealed class Structure : Attribute {
  public Structure(bool datatype = false) => Datatype = datatype;
  public bool Datatype { get; init; }

  [Weave]
  static void Apply(TypeTarget t, Structure a) {
    var props = t.DeriveFields();
    var self = t.TypeRef.Qualified;

    if (props.Length > 0)
      t.Class($$"""
        {{t.Visibility}} {{t.SelfName}}({{Code.Join(props, p => p.Parameter)}}) {
          {{Code.Join(props, p => p.Assignment, "\n  ")}}
        }
        """);

    var equatable = props.Where(p => p.Equatable).ToList();
    var unsafeness = equatable.Any(p => p.Unsafe) ? "unsafe " : "";

    // Equality is opt-in: the type declares IEquatable<Self> and we fill in whatever parts of it
    // the author did not write. Hix called this equatableSelf / typedEqualsSelf / objectEquals.
    if (t.DeclaresInterface($"System.IEquatable<{t.SelfName}>") || t.DeclaresInterface("System.IEquatable")) {
      var comparisons = equatable.Count == 0 ? "true" : Code.Join(equatable, p => p.Equality, " && ");

      if (!t.DeclaresMember("Equals"))
        t.Class($$"""
          public {{unsafeness}}bool Equals({{self}} other) => {{comparisons}};
          public override {{unsafeness}}bool Equals(object obj) => obj is {{self}} other && Equals(other);
          """);

      if (!t.DeclaresMember("GetHashCode"))
        t.Class($$"""
          public override {{unsafeness}}int GetHashCode() {
            var hashCode = new global::System.HashCode();
            {{Code.Join(equatable, p => $"hashCode.Add({p.Hash});", "\n  ")}}
            return hashCode.ToHashCode();
          }
          """);
    }

    if (a.Datatype)
      t.Class(Shared.BuildDatatype(self, t.SelfName, Shared.DatatypeEntries(self, props)));
  }
}

/// <summary>
/// Port of <c>mixin HELIX.HookAttribute</c>. The delegate lookup is Hix's
/// <c>resolveMixin</c> + <c>wire</c>.
/// </summary>
[Mixin(Unit.Method, Attribute = "HELIX.HookAttribute")]
public sealed class Hook : Attribute {
  public Hook(string target = null, int order = 0) {
    this.target = target;
    this.order = order;
  }

  public readonly string target;
  public readonly int order;

  [Weave]
  static void Apply(MethodTarget t, Hook a) {
    var name = a.target;
    if (name is null) {
      if (!t.Name.StartsWith("On", StringComparison.Ordinal)) {
        t.Report("HX0110", "a hook without an explicit target must be named On<Something>");
        return;
      }
      name = "$" + t.Name.Substring(2);
    }

    var target = t.TargetFor(name);
    if (target is null) {
      t.Report("HX0110", $"injection target '{name}' is not defined on this type");
      return;
    }

    // The generated method's parameters come from the target's delegate; wire them through.
    var arguments = t.Wire(target, t.Method);
    if (arguments is null) {
      t.Report("HX0111",
        $"'{t.Name}' does not match the signature of the hook target '{name}'" +
        (target.DelegateType is null ? "" : $" ({target.DelegateType})"));
      return;
    }

    t.Inject(target, a.order, $"{t.Name}({arguments});");
  }
}

/// <summary>Port of <c>mixin HELIX.MixinUsingAttribute</c>.</summary>
[Mixin(Unit.Type, Attribute = "HELIX.MixinUsingAttribute")]
public sealed class MixinUsing : Attribute {
  public MixinUsing(string statement = null) => Statement = statement;
  public string Statement { get; init; }

  [Weave]
  static void Apply(TypeTarget t, MixinUsing a) {
    if (!string.IsNullOrWhiteSpace(a.Statement)) t.Using(a.Statement);
  }
}
