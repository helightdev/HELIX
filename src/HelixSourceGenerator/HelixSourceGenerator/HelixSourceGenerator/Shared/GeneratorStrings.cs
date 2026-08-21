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
      Prop = "HELIX.Compose.PropAttribute",
      PropStruct = "HELIX.Compose.PropStructAttribute",
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
      VisualElement = "UnityEngine.UIElements.VisualElement";
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
