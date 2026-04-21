using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HELIX.Abstractions;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Theming;
using UnityEngine.UIElements;

namespace HELIX.Widgets {
  // ReSharper disable once InconsistentNaming
  public interface BuildContext : IDiagnosticableTree, IElement, IThemeProvider {
    static BuildContext Current = null;
    static BuildContext ReconcilerCurrent = null;
    Widget Descriptor { get; }
    BuildContext ParentContext { get; set; }

    protected bool IsUserWidget => this is IStatelessWidget || this is IStatefulWidget;

    static BuildContext GetUserTarget(BuildContext start, BuildContext except = null) {
      return GetAncestorChain(start).LastOrDefault(context => context.IsUserWidget && context != except) ?? start;
    }

    [SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
    static IEnumerable<BuildContext> GetAncestorChain(BuildContext start) {
      var current = start;
      while (current != null) {
        yield return current;
        var next = current.ParentContext;
        if (next == null) {
          var parent = current.Element.parent;
          if (parent is BuildContext asElement) next = asElement;
          else if (parent is ITreeAncestorTraversalHint hint) next = hint.Owner;
          else if (parent.userData is ITreeAncestorTraversalHint hint2) next = hint2.Owner; // 
          else next = parent.GetFirstAncestorOfType<BuildContext>(); //
        }

        current = next;
      }
    }

    [SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
    static BuildContext GetDirectParent(VisualElement current) {
      if (current is BuildContext currentContext && currentContext.ParentContext != null) {
        return currentContext.ParentContext;
      }

      var parent = current.parent;
      if (parent is BuildContext context) return context;
      if (parent is ITreeAncestorTraversalHint hint) return hint.Owner;
      if (parent.userData is ITreeAncestorTraversalHint hint2) return hint2.Owner;
      return null;
    }

    static BuildContext FindParent<T>(BuildContext context) where T : Widget {
      var current = context;
      while (current != null) {
        if (current is IWidgetElement { Descriptor: T }) return current;
        current = current.ParentContext;
      }

      return null;
    }
  }
}