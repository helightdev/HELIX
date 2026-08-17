namespace HELIX.SourceGen;

internal static class GeneratorStrings {
  internal static readonly string[] BuiltinMixinStereotypes = {
    "HELIX.Context.ComponentAttribute",
    "HELIX.Context.ServiceAttribute"
  };

  internal const string DiagnosticCategory = "HELIX";

  internal static class Attributes {
    internal const string
      Component = "HELIX.Context.ComponentAttribute",
      MixinExpression = "HELIX.Context.MixinExpressionAttribute",
      MixinPrepareGlobal = "HELIX.Context.MixinPrepareGlobalAttribute",
      MixinDefineTarget = "HELIX.Context.MixinDefineTargetAttribute",
      BoundaryComposable = "HELIX.Compose.BoundaryComposableAttribute",
      ComposableProxy = "HELIX.Compose.ComposableProxyAttribute",
      Composition = "HELIX.Compose.CompositionAttribute",
      Context = "HELIX.Compose.ContextAttribute",
      EnableMixins = "HELIX.Context.EnableMixinsAttribute",
      Mixin = "HELIX.Context.MixinAttribute",
      RequireMixin = "HELIX.Context.RequireMixinAttribute",
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
      Mixin = "HELIX.Context.IMixin",
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
