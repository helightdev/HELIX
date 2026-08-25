namespace HELIX.SourceGen;

internal static class GeneratorStrings {
  internal static readonly string[] BuiltinMixinStereotypes = {
    "HELIX.Context.ManagedAttribute",
    "HELIX.Context.ServiceAttribute"
  };

  internal const string DiagnosticCategory = "HELIX";

  internal static class Attributes {
    internal const string
      Managed = "HELIX.Context.ManagedAttribute",
      HelixApplication = "HELIX.Context.HelixApplicationAttribute",
      HelixModule = "HELIX.Context.HelixModuleAttribute",
      MixinExpression = "HELIX.MixinExpressionAttribute",
      MixinPrepareGlobal = "HELIX.MixinPrepareGlobalAttribute",
      MixinDefineTarget = "HELIX.MixinDefineTargetAttribute",
      BoundaryComposable = "HELIX.Compose.BoundaryComposableAttribute",
      ComposableProxy = "HELIX.Compose.ComposableProxyAttribute",
      Composition = "HELIX.Compose.CompositionAttribute",
      Context = "HELIX.Compose.ContextAttribute",
      EnableMixins = "HELIX.EnableMixinsAttribute",
      Mixin = "HELIX.MixinAttribute",
      RequireMixin = "HELIX.RequireMixinAttribute",
      Prop = "HELIX.PropAttribute",
      PropStruct = "HELIX.PropStructAttribute",
      UxmlElement = "UnityEngine.UIElements.UxmlElementAttribute";
  }

  internal static class Types {
    internal const string
      BoundaryCell = "HELIX.Compose.BoundaryCell",
      BoundaryData = "HELIX.Compose.BoundaryData",
      Boundary = "HELIX.Compose.IBoundary",
      BoundaryVisualElement = "HELIX.Compose.BoundaryVisualElement",
      Composable = "HELIX.Compose.Composable",
      IComposable = "HELIX.Compose.IComposable",
      Composition = "HELIX.Compose.Composition",
      CompositionId = "HELIX.Compose.CompositionId",
      CompositionInternals = "HELIX.Compose.CompositionInternals",
      CompositionTransfer = "HELIX.Compose.CompositionInternals.TransferData",
      ElementRef = "HELIX.Compose.ElementRef",
      LocalId = "HELIX.Compose.LocalId",
      Mixin = "HELIX.IMixin",
      PropsBoundaryComposable = "HELIX.Compose.PropsBoundaryComposable",
      ReadComposable = "HELIX.Compose.ReadComposable",
      ScopeHandle = "HELIX.Compose.ScopeHandle",
      Datatypes = "HELIX.Datatypes",
      StructureDatatype = "HELIX.StructureDatatype",
      ConfigurableStructureDatatype = "HELIX.ConfigurableStructureDatatype",
      StructurePropertyDatatype = "HELIX.StructurePropertyDatatype",
      UnityColor = "UnityEngine.Color",
      UnityVector2 = "UnityEngine.Vector2",
      UnityVector3 = "UnityEngine.Vector3",
      UnityVector4 = "UnityEngine.Vector4",
      VisualElement = "UnityEngine.UIElements.VisualElement";
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
      HashCodeSyntax = "HashCodeSyntax",
      ProxyFunction = "ProxyFunction",
      ProxySetter = "ProxySetter",
      ProxyGetter = "ProxyGetter",
      ProxyEquality = "ProxyEquality";
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
      ProxyCreate = "instance = new {TYPE}();",
      ProxyPrepare = "/* Skip Prepare */",
      ProxyPreYield = "/* Skip Before Yield */",
      ProxyPostYield = "/* Skip Post Yield */",
      ProxyScopeCallback = "cell.TrimChildren();",
      ProxyEquality = "{0} == {1}",
      ProxyHashCode = "{0}";
  }
}
