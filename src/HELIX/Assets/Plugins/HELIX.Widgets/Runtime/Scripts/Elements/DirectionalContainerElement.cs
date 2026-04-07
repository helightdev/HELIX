using System.Collections.Generic;
using System.Linq;
using HELIX.Abstractions;
using HELIX.Extensions;
using HELIX.Types;
using HELIX.Widgets.Universal;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Elements {
    [UxmlElement]
    public abstract partial class DirectionalContainerElement : VisualElement, IMultiChildContainer, IWidgetElement,
        IWidgetElementCollection {
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
                    if (visualElement.ClassListContains("generated-gap")) visualElement.style.width = _gap;
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

        public virtual IEnumerable<VisualElement> Childs {
            get => Children().Where(child => !child.ClassListContains("generated-gap"));
            set {
                Clear();
                if (value == null) return;
                foreach (var child in value) Add(child);
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
            if (clear)
                foreach (var child in Children().ToList()) {
                    if (child.ClassListContains("generated-gap")) Remove(child);
                }

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

        public VisualElement Element => this;
        public Widget Descriptor { get; set; }
        public int HierarchyDepth { get; set; }

        public bool CanReconcile(Widget updated) {
            return updated is DirectionalContainerWidget;
        }

        public bool Reconcile(Widget updated) {
            if (updated is not DirectionalContainerWidget widget) return false;
            Gap = widget.gap;
            MainAxisAlign = widget.mainAxisAlign;
            CrossAxisAlign = widget.crossAxisAlign;
            Reverse = widget.reverse;
            Modifier.ApplyDelta(Descriptor, updated, this);
            Descriptor = updated;
            DefaultReconciler.ReconcileCollection(this, widget.children);
            return true;
        }

        public IEnumerable<IWidgetElement> Elements => Childs.Select(DefaultReconciler.ExpandElement);

        public void FillElements(List<IWidgetElement> elements) {
            for (int i = 0; i < hierarchy.childCount; i++) {
                var child = hierarchy[i];
                if (child.ClassListContains("generated-gap")) continue;
                elements.Add(DefaultReconciler.ExpandElement(child));
            }
        }

        public void Update(IWidgetElement[] result, ReconcilerCollectionDelta[] deltas) {
            // Remove Gaps
            foreach (var child in Children().ToList()) {
                if (child.ClassListContains("generated-gap")) Remove(child);
            }
            new HierarchyDescriptionCollection(this).Update(result, deltas);
            RebuildGaps(false);
        }

        public void Update(IEnumerable<IWidgetElement> updated) {
            Childs = updated?.Select(e => e.Element) ?? Enumerable.Empty<VisualElement>();
        }
    }
}