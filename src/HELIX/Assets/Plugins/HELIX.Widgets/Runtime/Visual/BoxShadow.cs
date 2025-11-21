using System.Collections.Generic;
using HELIX.Extensions;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Visual {
    [UxmlElement]
    public partial class BoxShadow : BaseWidget {
        private float _spreadRadius = 0f;
        private float _blurRadius = 4f;
        private Vector2 _offset = Vector2.zero;
        private Color _shadowColor = new(0, 0, 0, 0.25f);
        private Vector4 _borderRadius = new(0, 0, 0, 0);
        private readonly VisualElement _shadowElement;

        public BoxShadow() {
            _shadowElement = new VisualElement {
                style = {
                    backgroundColor = _shadowColor,
                    position = Position.Absolute
                }
            };
            ApplyBlurFunction();
            ApplyTransformation();
            Add(_shadowElement);
        }

        [UxmlAttribute]
        public float SpreadRadius {
            get => _spreadRadius;
            set {
                _spreadRadius = value;
                ApplyTransformation();
            }
        }

        [UxmlAttribute]
        public float BlurRadius {
            get => _blurRadius;
            set {
                _blurRadius = value;
                ApplyBlurFunction();
            }
        }

        [UxmlAttribute]
        public Vector2 Offset {
            get => _offset;
            set {
                _offset = value;
                ApplyTransformation();
            }
        }

        [UxmlAttribute]
        public Color ShadowColor {
            get => _shadowColor;
            set {
                _shadowColor = value;
                _shadowElement.style.backgroundColor = _shadowColor;
            }
        }

        public Vector4 BorderRadius {
            get => _borderRadius;
            set {
                _borderRadius = value;
                _shadowElement.BorderRadius(value);
            }
        }
        
        [UxmlAttribute]
        public Rect Corners {
            get => EditorUtilities.SwizzleToRect(BorderRadius);
            set => BorderRadius = EditorUtilities.SwizzleToVector4(value);
        }

        private void ApplyBlurFunction() {
            var function = new FilterFunction(FilterFunctionType.Blur);
            function.AddParameter(new FilterParameter(_blurRadius));
            _shadowElement.style.filter = new List<FilterFunction> { function };
        }

        private void ApplyTransformation() {
            var offsets = new Vector4(-_spreadRadius, -_spreadRadius, -_spreadRadius, -_spreadRadius);
            offsets += new Vector4(_offset.y, -_offset.x, -_offset.y, _offset.x);
            _shadowElement.Position(offsets);
        }
    }
}