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
    public partial class BoxShadowElement : BaseElement, IWidgetElement {
        private readonly VisualElement _shadowElement;
        private float _blurRadius = 4f;
        private Vector4 _borderRadius = new(0, 0, 0, 0);
        private Vector2 _offset = Vector2.zero;
        private Color _shadowColor = new(0, 0, 0, 0.25f);
        private float _spreadRadius;
        private TimeValue _transitionDuration = new(0);
        private EasingMode _easingFunction = EasingMode.Linear;
        private readonly TweenStateArena<Color> _colorArena;

        public BoxShadowElement() {
            _shadowElement = new Element("Shadow") {
                style = {
                    backgroundColor = _shadowColor,
                    position = Position.Absolute
                }
            };
            _colorArena = TweenStateArena.ColorArena(
                this,
                value => {
                    _shadowElement.style.backgroundColor = value;
                },
                durationMs: 0,
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

        public VisualElement Element => this;
        public Widget Descriptor { get; private set; }

        public bool CanReconcile(Widget updated) {
            return updated is BoxShadow;
        }

        public bool Reconcile(Widget updated) {
            if (updated is not BoxShadow descriptor) return false;
            BlurRadius = descriptor.blurRadius;
            BorderRadius = descriptor.borderRadius;
            Offset = descriptor.offset;
            ShadowColor = descriptor.shadowColor;
            SpreadRadius = descriptor.spreadRadius;
            TransitionDuration = descriptor.transitionDuration;
            EasingFunction = descriptor.easingFunction;
            
            DefaultReconciler.ReconcileSingleDirectKeeping(this, descriptor.child, 1);
            Modifier.ApplyDelta(Descriptor, updated, this);
            Descriptor = updated;
            return true;
        }
    }
}