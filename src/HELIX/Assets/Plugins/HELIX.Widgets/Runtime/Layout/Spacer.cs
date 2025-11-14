using HELIX.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Layout {
    [UxmlElement]
    public partial class Spacer : VisualElement {
        private float _width = 8f;
        private Axis _axis = Axis.Vertical;
        private bool _expands = false;
        
        public Spacer() { }

        [UxmlAttribute]
        public float Width {
            get => _width;
            set {
                if (Mathf.Approximately(_width, value)) return;
                _width = value;
                if (Axis == Axis.Horizontal) {
                    style.width = value;
                } else {
                    style.height = value;
                }
            }
        }

        [UxmlAttribute]
        public Axis Axis {
            get => _axis;
            set {
                if (_axis == value) return;
                _axis = value;
                if (value == Axis.Horizontal) {
                    style.width = _width;
                    style.height = 0;
                } else {
                    style.height = _width;
                    style.width = 0;
                }
            }
        }

        [UxmlAttribute]
        public bool Expands {
            get => _expands;
            set {
                if (_expands == value) return;
                _expands = value;
                style.flexGrow = value ? 1 : 0;
            }
        }
    }
}