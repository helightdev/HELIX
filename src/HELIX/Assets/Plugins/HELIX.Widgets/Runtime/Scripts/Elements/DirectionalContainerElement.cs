using System.Collections.Generic;
using System.Linq;
using HELIX.Extensions;
using HELIX.Types;
using HELIX.Widgets.Universal;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Elements {
  [UxmlElement]
  public abstract partial class
    DirectionalContainerElement : MultiChildWidgetBaseElement<DirectionalContainerWidget>, IPreferExplicitFlex {
    private Align _crossAxisAlign;
    private float _gap;
    private Justify _mainAxisAlign;
    private bool _reverse;

    protected DirectionalContainerElement() {
      style.flexWrap = Wrap.NoWrap;
      style.justifyContent = MainAxisAlign;
      style.alignItems = CrossAxisAlign;

      RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
      RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
    }

    [UxmlAttribute]
    public float Gap {
      get => _gap;
      set {
        var previousGap = _gap;
        _gap = value;
        var hasNoGap = Mathf.Approximately(_gap, 0f);
        if (hasNoGap) _gap = 0f;
        var hadNoGap = Mathf.Approximately(previousGap, 0f);

        if (hasNoGap != hadNoGap) {
          RebuildGaps();
          return;
        }

        if (hasNoGap) return;
        foreach (var visualElement in Children()) {
          if (visualElement.ClassListContains("generated-gap") && visualElement is SpacerElement spacer)
            spacer.Width = _gap;
        }
      }
    }

    [UxmlAttribute]
    public Justify MainAxisAlign {
      get => _mainAxisAlign;
      set {
        if (_mainAxisAlign == value) return;
        _mainAxisAlign = value;
        style.justifyContent = _mainAxisAlign;
        RebuildGaps();
      }
    }

    [UxmlAttribute]
    public Align CrossAxisAlign {
      get => _crossAxisAlign;
      set {
        if (_crossAxisAlign == value) return;
        _crossAxisAlign = value;
        style.alignItems = _crossAxisAlign;
      }
    }

    [UxmlAttribute]
    public bool Reverse {
      get => _reverse;
      set {
        _reverse = value;
        style.flexDirection = GetFlexDirection(_reverse);
      }
    }

    public override IEnumerable<VisualElement> Childs {
      get => base.Childs.Where(child => !child.ClassListContains("generated-gap"));
      set {
        base.Childs = value;
        RebuildGaps(false);
      }
    }

    private void OnAttachToPanel(AttachToPanelEvent evt) {
      HierarchyDepth = this.GetDepth();
      style.flexDirection = GetFlexDirection(Reverse);
      RebuildGaps();
    }

    private void OnGeometryChanged(GeometryChangedEvent evt) {
      RebuildGaps();
    }

    protected abstract FlexDirection GetFlexDirection(bool reverse);
    protected abstract Axis GetAxis();

    private void RebuildGaps(bool clear = true) {
      if (clear) RemoveGaps();

      if (Mathf.Approximately(_gap, 0f) || _mainAxisAlign is Justify.SpaceBetween or Justify.SpaceEvenly) return;

      // Insert gaps between all children
      var count = childCount - 1;
      for (var i = 0; i < count; i++) {
        var gapElement = new SpacerElement {
          Width = _gap,
          Axis = GetAxis()
        };
        gapElement.AddToClassList("generated-gap");
        Insert(2 * i + 1, gapElement);
      }
    }

    private void RemoveGaps() {
      for (var i = childCount - 1; i >= 0; i--) {
        var element = ElementAt(i);
        if (element.ClassListContains("generated-gap")) Remove(element);
      }
    }

    public override void Apply(DirectionalContainerWidget previous, DirectionalContainerWidget widget) {
      base.Apply(previous, widget);
      Gap = widget.gap;
      MainAxisAlign = widget.mainAxisAlign;
      CrossAxisAlign = widget.crossAxisAlign;
      Reverse = widget.reverse;
    }

    public override void LoadWidgetElements(List<IWidgetElement> elements) {
      for (var i = 0; i < contentContainer.childCount; i++) {
        var child = contentContainer[i];
        if (child.ClassListContains("generated-gap")) continue;
        elements.Add(Reconciler.ExpandElement(child));
      }
    }

    public override void UpdateWidgetElements(IWidgetElement[] result, ReconcilerCollectionDelta[] deltas) {
      RemoveGaps();
      base.UpdateWidgetElements(result, deltas);
      RebuildGaps(false);
    }
  }
}