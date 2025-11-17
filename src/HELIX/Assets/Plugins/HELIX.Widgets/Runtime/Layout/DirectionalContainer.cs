using System.Collections.Generic;
using System.Linq;
using HELIX.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Layout {
    [UxmlElement]
    public abstract partial class DirectionalContainer : VisualElement, IMultiChildContainer {
        private float _gap;
        private Justify _mainAxisAlign;
        private Align _crossAxisAlign;
        private bool _reverse;

        protected DirectionalContainer() {
            style.flexWrap = Wrap.NoWrap;
            style.justifyContent = MainAxisAlign;
            style.alignItems = CrossAxisAlign;

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        private void OnAttachToPanel(AttachToPanelEvent evt) {
            style.flexDirection = GetFlexDirection(Reverse);
            RebuildGaps();
        }

        private void OnGeometryChanged(GeometryChangedEvent evt) {
            RebuildGaps();
        }

        protected abstract FlexDirection GetFlexDirection(bool reverse);
        protected abstract Axis GetAxis();

        [UxmlAttribute]
        public float Gap {
            get => _gap;
            set {
                _gap = value;
                if (Mathf.Approximately(_gap, 0f)) {
                    _gap = 0f;
                    RebuildGaps();
                } else {
                    foreach (var visualElement in Children()) {
                        if (visualElement.ClassListContains("generated-gap")) {
                            visualElement.style.width = _gap;
                        }
                    }
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

        private void RebuildGaps() {
            foreach (var child in Children().ToList()) {
                if (child.ClassListContains("generated-gap")) Remove(child);
            }

            if (Mathf.Approximately(_gap, 0f) || _mainAxisAlign is Justify.SpaceBetween or Justify.SpaceEvenly) return;

            // Insert gaps between all children
            var children = Children().ToList();
            for (var i = 0; i < children.Count - 1; i++) {
                var gapElement = new Spacer {
                    Width = _gap,
                    Axis = GetAxis()
                };
                gapElement.AddToClassList("generated-gap");
                Insert(2 * i + 1, gapElement);
            }
        }

        public virtual IEnumerable<VisualElement> Childs {
            get => Children();
            set {
                Clear();
                if (value == null) return;
                foreach (var child in value) {
                    Add(child);
                }
            }
        }
    }
}