using System.Collections.Generic;
using HELIX.Abstractions;
using HELIX.Animation;
using HELIX.Extensions;
using HELIX.Widgets.Universal;
using HELIX.Widgets.Utilities;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Elements {
    [UxmlElement]
    public partial class BoxShadowElement : SingleChildWidgetBaseElement<BoxShadow> {
        private readonly TweenStateArena<Color> _colorArena;
        private readonly VisualElement _shadowElement;
        private float _blurRadius = 4f;
        private Vector4 _borderRadius = new(0, 0, 0, 0);
        private EasingMode _easingFunction = EasingMode.Linear;
        private Vector2 _offset = Vector2.zero;
        private Color _shadowColor = new(0, 0, 0, 0.25f);
        private float _spreadRadius;
        private TimeValue _transitionDuration = new(0);

        public BoxShadowElement() {
            _shadowElement = new Element("Shadow") {
                style = {
                    backgroundColor = _shadowColor,
                    position = Position.Absolute
                },
                usageHints = UsageHints.DynamicColor | UsageHints.DynamicPostProcessing
            };
            _colorArena = TweenStateArena.ColorArena(
                this,
                value => { _shadowElement.style.backgroundColor = value; },
                0,
                easingMode: _easingFunction,
                startValue: _shadowColor
            );
            ApplyBlurFunction();
            ApplyTransformation();
            Add(_shadowElement);
        }

        [UxmlAttribute]
        public float SpreadRadius {
            get => _spreadRadius;
            set {
                if (Mathf.Approximately(_spreadRadius, value)) return;
                _spreadRadius = value;
                ApplyTransformation();
            }
        }

        [UxmlAttribute]
        public float BlurRadius {
            get => _blurRadius;
            set {
                if (Mathf.Approximately(_blurRadius, value)) return;
                _blurRadius = value;
                ApplyBlurFunction();
            }
        }

        [UxmlAttribute]
        public Vector2 Offset {
            get => _offset;
            set {
                if (Vector2.Distance(_offset, value) < 0.01f) return;
                _offset = value;
                ApplyTransformation();
            }
        }

        [UxmlAttribute]
        public Color ShadowColor {
            get => _shadowColor;
            set {
                if (_shadowColor == value) return;
                _shadowColor = value;
                _colorArena.Push(_shadowColor);
            }
        }

        public TimeValue TransitionDuration {
            get => _transitionDuration;
            set {
                _transitionDuration = value;
                _colorArena.Duration = _transitionDuration;
            }
        }

        public EasingMode EasingFunction {
            get => _easingFunction;
            set {
                _easingFunction = value;
                _colorArena.EasingMode = _easingFunction;
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
            get => EditorUtilities.SwizzleCorners(BorderRadius);
            set => BorderRadius = EditorUtilities.UnswizzleCorners(value);
        }

        public override VisualElement Child {
            get => ISingleChildContainer.LastGet(this, 1);
            set => ISingleChildContainer.LastSet(this, 1, value);
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

        public override void Apply(BoxShadow previous, BoxShadow widget) {
            base.Apply(previous, widget);
            BlurRadius = widget.blurRadius;
            BorderRadius = widget.borderRadius;
            Offset = widget.offset;
            ShadowColor = widget.shadowColor;
            SpreadRadius = widget.spreadRadius;
            TransitionDuration = widget.transitionDuration;
            EasingFunction = widget.easingFunction;
        }
    }
}