namespace HELIX.SourceGen;

internal static class GeneratorStrings {
  internal const string DiagnosticCategory = "HELIX";

  internal static class Attributes {
    internal const string
      Managed = "HELIX.Context.ManagedAttribute",
      HelixApplication = "HELIX.Context.HelixApplicationAttribute",
      HelixModule = "HELIX.Context.HelixModuleAttribute",
      Mixable = "HELIX.MixableAttribute",
      Prop = "HELIX.PropAttribute",
      Structure = "HELIX.StructureAttribute";
  }

  internal static class Types {
    internal const string
      CompositionId = "HELIX.Compose.CompositionId",
      Datatypes = "HELIX.Datatypes",
      StructureDatatype = "HELIX.StructureDatatype",
      ConfigurableStructureDatatype = "HELIX.ConfigurableStructureDatatype",
      StructurePropertyDatatype = "HELIX.StructurePropertyDatatype",
      UnityColor = "UnityEngine.Color",
      UnityVector2 = "UnityEngine.Vector2",
      UnityVector3 = "UnityEngine.Vector3",
      UnityVector4 = "UnityEngine.Vector4"
      ;
  }

  internal static class Members {
    internal const string
      ConfigureDatatype = "ConfigureDatatype",
      Datatype = "Datatype";
  }

  internal static class PropArguments {
    internal const string
      Datatype = "Datatype",
      Equatable = "Equatable",
      EqualitySyntax = "EqualitySyntax",
      HashCodeSyntax = "HashCodeSyntax";
  }

  internal static class DatatypeMembers {
    internal const string
      String = "String",
      Int = "Int",
      Long = "Long",
      Float = "Float",
      Double = "Double",
      Bool = "Bool",
      Color = "Color",
      Vector2 = "Vector2",
      Vector3 = "Vector3",
      Vector4 = "Vector4",
      Enum = "Enum",
      Object = "Object";
  }

  internal static class Templates {
    internal const string
      Equality = "{0} == {1}",
      HashCode = "{0}";
  }
}

internal static class MixinGeneratorCandidates {
  internal static readonly string[] AttributeMetadataNames = [
    GeneratorStrings.Attributes.Mixable
  ];
}
