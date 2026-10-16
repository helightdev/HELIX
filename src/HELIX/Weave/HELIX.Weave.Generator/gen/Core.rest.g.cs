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
