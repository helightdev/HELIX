namespace HELIX.SourceGen {
  internal static class GeneratorStrings {
    internal const string DiagnosticCategory = "HELIX";

    internal static class Attributes {
      internal const string
        BoundaryComposable = "HELIX.Compose.BoundaryComposableAttribute",
        ComposableProxy = "HELIX.Compose.ComposableProxyAttribute",
        Composition = "HELIX.Compose.CompositionAttribute",
        Context = "HELIX.Compose.ContextAttribute",
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
        Composition = "HELIX.Compose.Composition",
        CompositionId = "HELIX.Compose.CompositionId",
        CompositionInternals = "HELIX.Compose.CompositionInternals",
        CompositionTransfer = "HELIX.Compose.CompositionInternals.TransferData",
        ElementRef = "HELIX.Compose.ElementRef",
        LocalId = "HELIX.Compose.LocalId",
        PropsBoundaryComposable = "HELIX.Compose.PropsBoundaryComposable",
        ReadComposable = "HELIX.Compose.ReadComposable",
        ScopeHandle = "HELIX.Compose.ScopeHandle";
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
}
