using System.Collections.Generic;
using System.Linq;
using HELIX.Abstractions;
using HELIX.Types;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Modifiers;
using HELIX.Widgets.Universal;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Elements {
  public abstract class MultiChildWidgetBaseElement<T> : WidgetBaseElement<T>, IMultiChildContainer,
    IWidgetElementCollection where T : MultiChildWidget {
    public virtual IEnumerable<VisualElement> Childs {
      get => Children();
      set {
        Clear();
        if (value == null) return;
        foreach (var child in value) Add(child);
      }
    }

    public virtual void LoadWidgetElements(List<IWidgetElement> elements) {
      new HierarchyDescriptionCollection(contentContainer).LoadWidgetElements(elements);
    }

    public virtual void UpdateWidgetElements(IWidgetElement[] result, ReconcilerCollectionDelta[] deltas) {
      new HierarchyDescriptionCollection(contentContainer).UpdateWidgetElements(result, deltas);
    }

    public override bool CanReconcile(Widget updated) {
      return updated is T;
    }

    public override bool Reconcile(Widget updated) {
      if (updated is not T widget) return false;
      var previous = TypedDescriptor;
      Apply(previous, widget);
      Modifier.ApplyDelta(Descriptor, updated, this);
      Descriptor = updated;
      Reconciler.ReconcileCollection(this, widget, this);
      return true;
    }

    public override List<DiagnosticsNode> DebugDescribeChildren() {
      return new HierarchyDescriptionCollection(contentContainer).Elements
        .Select(x => x.ToDiagnosticsNodeSafe())
        .ToList();
    }
  }

  /// <summary>
  /// <para>
  /// Indicates that widgets using implicit flex filling (e.g. <see cref="ModifierFallbacks.FlexFill"/>)
  /// should use tight constraints by default, preventing them and their children from expanding beyond their
  /// intrinsic preferred size.
  /// </para>
  /// <para>
  /// Also provides the preferred flex-axis for widgets such as <see cref="HGap"/> that need to know what their
  /// main-axis is going to be.
  /// </para>
  /// </summary>
  /// <remarks>
  /// <para>
  /// By default, many non-layouting widgets (e.g. <see cref="StatelessWidget{T}"/>,
  /// <see cref="StatefulWidget{T}"/>, <see cref="HThemeProvider"/>) adopt
  /// <see cref="ModifierFallbacks.FlexFill"/>, causing them to expand along both axes.
  /// </para>
  /// <para>
  /// This is convenient in isolation, but can lead to unexpected behavior in layouts
  /// like <see cref="HRow"/>, where children are typically expected to tightly size to their
  /// content unless wrapped in a <see cref="FlexibleModifier"/>. Applying a flex fill here would
  /// create unintended whitespace not occupied by the child if it is not fully flexing.
  /// </para>
  /// <para>
  /// Implementing <see cref="IPreferExplicitFlex"/> changes this default by applying
  /// tight constraints to direct children when possible, preventing the widget from expanding beyond
  /// its intrinsic size unless explicitly instructed.
  /// </para>
  /// </remarks>
  public interface IPreferExplicitFlex {
    Axis PreferredFlexAxis => Axis.Vertical;
  }

  /// <summary>
  /// Indicates that widgets should automatically use absolute positioning instead of the flex layout.
  /// </summary>
  /// <remarks>
  /// This is useful for alignment widgets (e.g. <see cref="HAlign"/>, <see cref="HCenter"/>) that are supposed to also
  /// be usable inside stacking layouts such as an <see cref="HStack"/> without specifying
  /// <see cref="ModifierExtensions.Positioned"/> or <see cref="ModifierExtensions.Stretch"/>.
  /// </remarks>
  public interface IPreferStacking { }
}