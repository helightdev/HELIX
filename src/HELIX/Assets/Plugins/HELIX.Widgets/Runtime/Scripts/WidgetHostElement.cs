using System;
using System.Collections.Generic;
using HELIX.Widgets.Elements;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets {
  /// <summary>
  /// Represents a <see cref="VisualElement"/> that hosts a <see cref="Widget"/> derived from an <see cref="IBuildable"/>.
  /// </summary>
  /// <remarks>
  /// This is the preferred entry point for integrating <see cref="Widget"/>s into a UI-Toolkit-based application.
  /// It may be used to integrate widgets into non-widget-based parts of the visual tree.
  /// </remarks>
  ///
  public class WidgetHostElement : BuildingWidgetBaseElement<WidgetHostElement.WidgetType>, IHierarchyDisposable {
    public static readonly HashSet<WidgetHostElement> Instances = new();

    private bool _hasState;

    public WidgetHostElement() {
      Descriptor = RootWidget.Instance;
    }

    public IBuildable Buildable { get; set; }

    public void Dispose() {
      _hasState = false;
      userData = null;
      ParentContext = null;
      Descriptor = null;

#if UNITY_EDITOR
      Instances.Remove(this);
#endif

      // Initially I ran Clear() here to instantly trigger child disposal, but that conflicted with hierarchy rules
      // I then allowed disposals to trickle down by polling until the set is empty and that worked, but just
      // doing the disposal using the object destructor was less error-prone and is probably more performant 
    }

    protected override void OnAttached(AttachToPanelEvent evt) {
      base.OnAttached(evt);

      if (_hasState) Debug.Log("Widget host hierarchy has moved before disposal!");

      var nearestWidget = GetFirstAncestorOfType<IWidgetElement>();
      if (nearestWidget == null) {
        Descriptor = RootWidget.Instance;
        ParentContext = null;
      } else {
        ParentContext = nearestWidget;
        Descriptor = GapWidget.Instance;
      }

      _hasState = true;

#if UNITY_EDITOR
      Instances.Add(this);
#endif

      ModificationBarrier.Rebuild(this);
    }

    protected override IBuildable GetBuildableForWidget(WidgetType previous, WidgetType widget) {
      return Buildable;
    }

    public abstract class WidgetType : Widget {
      public override IWidgetElement CreateElement() {
        throw new NotImplementedException();
      }
    }

    public class RootWidget : WidgetType {
      public static readonly RootWidget Instance = new();

      private RootWidget() { }

      public override string GetWidgetName() {
        return "[ROOT]";
      }
    }

    public class GapWidget : WidgetType {
      public static readonly GapWidget Instance = new();

      private GapWidget() { }

      public override string GetWidgetName() {
        return "[GAP]";
      }
    }
  }
}