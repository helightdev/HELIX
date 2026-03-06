using System;
using HELIX.Extensions;
using HELIX.Painting;
using HELIX.Types;
using HELIX.Widgets.Theming;
using HELIX.Widgets.Visual;
using HELIX.Widgets.Visual.PathBuilders;
using HELIX.Widgets.Visual.PathDrawers;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Input {
    [UxmlElement]
    public partial class GenericSlider : BaseWidget {
        private readonly WidgetFactorySlot<GenericSliderThumb> _thumbSlot;
        private readonly WidgetFactorySlot<GenericSliderTrack> _trackSlot;
        private Axis _axis;
        private float _thumbRange; // 0: point, epsilon-1f: thumb width as percentage of total slider length
        private float _value; // 0f-1f

        public event Action<float> OnValueChanged;

        public GenericSlider() {
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            _trackSlot = WidgetFactorySlot(
                    track => {
                        track.Rebuild(this, contentRect);
                        track.SetValue(_value);
                    },
                    fallback: new SimpleGenericSliderTrack()
                )
                .Stretched()
                .AddTo(this);
            _thumbSlot = WidgetFactorySlot(
                    thumb => {
                        thumb.Rebuild(this, contentRect);
                        thumb.SetRange(_thumbRange);
                        thumb.SetValue(_value);
                    },
                    fallback: new SimpleGenericSliderThumb()
                )
                .Stretched()
                .AddTo(this);
        }
        
        public void SetValueWithoutNotify(float newValue) {
            _value = Mathf.Clamp01(newValue);
            _trackSlot.Element?.SetValue(_value);
            _thumbSlot.Element?.SetValue(_value);
        }
        
        [UxmlAttribute, Range(0f, 1f)]
        public float Value {
            get => _value;
            set {
                _value = Mathf.Clamp01(value);
                _trackSlot.Element?.SetValue(_value);
                _thumbSlot.Element?.SetValue(_value);
                OnValueChanged?.Invoke(_value);
            }
        }

        [UxmlAttribute, Range(0f, 1f)]
        public float ThumbRange {
            get => _thumbRange;
            set {
                _thumbRange = Mathf.Clamp01(value);
                _thumbSlot.Element?.SetRange(_thumbRange);
            }
        }

        [UxmlAttribute]
        public Axis Axis {
            get => _axis;
            set {
                _axis = value;
                Rebuild();
            }
        }

        [UxmlObjectReference("track-factory")]
        public GenericSliderTrackFactory TrackFactory {
            get => _trackSlot.GetMapped<GenericSliderTrackFactory>();
            set => _trackSlot.SetMapped(value);
        }

        [UxmlObjectReference("thumb-factory")]
        public GenericSliderThumbFactory ThumbFactory {
            get => _thumbSlot.GetMapped<GenericSliderThumbFactory>();
            set => _thumbSlot.SetMapped(value);
        }

        [UxmlObjectReference("base-test")]
        public VisualElementWidgetFactory TestFactory { get; set; }

        protected override void OnAttached(AttachToPanelEvent evt) {
            base.OnAttached(evt);
            Rebuild();
        }

        public void Rebuild() {
            _trackSlot.Element?.Rebuild(this, contentRect);
            _thumbSlot.Element?.Rebuild(this, contentRect);
        }

        private void OnGeometryChanged(GeometryChangedEvent evt) {
            _trackSlot.Element?.SetDimensions(contentRect);
            _thumbSlot.Element?.SetDimensions(contentRect);
        }
    }

    public abstract class GenericSliderTrack : BaseWidget {
        public Rect dimensions;
        public GenericSlider slider;
        public float value;

        public virtual void Rebuild(GenericSlider parentSlider, Rect sliderDimensions) {
            slider = parentSlider;
            dimensions = sliderDimensions;
        }

        public virtual void SetValue(float newValue) {
            value = newValue;
        }

        public virtual void SetDimensions(Rect newDimensions) {
            dimensions = newDimensions;
        }
    }

    public abstract class GenericSliderThumb : BaseWidget {
        public Rect dimensions;
        public float range;
        public GenericSlider slider;
        public float value;

        public virtual void Rebuild(GenericSlider parentSlider, Rect sliderDimensions) {
            slider = parentSlider;
            dimensions = sliderDimensions;
        }

        public virtual void SetValue(float newValue) {
            value = newValue;
        }

        public virtual void SetRange(float newRange) {
            range = newRange;
        }

        public virtual void SetDimensions(Rect newDimensions) {
            dimensions = newDimensions;
        }
    }

    [UxmlObject]
    public abstract partial class GenericSliderThumbFactory : WidgetFactory<GenericSliderThumb> { }

    [UxmlObject]
    public abstract partial class GenericSliderTrackFactory : WidgetFactory<GenericSliderTrack> { }

    [UxmlObject, Serializable]
    public partial class SimpleGenericSliderTrack : GenericSliderTrackFactory {
        [UxmlAttribute]
        public StyleLength trackHeight = 33.Percent();

        [UxmlObjectReference("paint")]
        public ScriptablePaint paint = new ScriptablePainter(
            new RoundedRectPathBuilder { Radii = new Vector4(4, 4, 4, 4) },
            new SolidFillPathDrawer { Color = Color.gray }
        );

        public override VisualElement Create(BaseWidget parentWidget) {
            return new Widget(paint, trackHeight);
        }

        public class Widget : GenericSliderTrack {
            private readonly ScriptablePaint _paint;
            private readonly StyleLength _trackHeight;

            public Widget(ScriptablePaint paint, StyleLength trackHeight) {
                _paint = paint;
                _trackHeight = trackHeight;
                this.Stretched();
                generateVisualContent = GenerateVisualContent;
            }

            private void GenerateVisualContent(MeshGenerationContext obj) {
                var canvas = new PaintCanvas(obj);
                var rect = canvas.canvasRect;
                var bounds = slider.Axis switch {
                    Axis.Horizontal => rect.LayoutSimple(Alignment.Center, Length.Auto(), _trackHeight),
                    Axis.Vertical   => rect.LayoutSimple(Alignment.Center, _trackHeight, Length.Auto()),
                    _               => throw new ArgumentOutOfRangeException()
                };
                _paint?.Draw(canvas, bounds);
            }
        }
    }

    [UxmlObject, Serializable]
    public partial class SimpleGenericSliderThumb : GenericSliderThumbFactory {
        [UxmlAttribute]
        public StyleLength trackHeight = Length.Auto();

        [UxmlObjectReference("paint")]
        public ScriptablePaint paint = new ScriptablePainter(
            new RoundedRectPathBuilder { Radii = new Vector4(4, 4, 4, 4) },
            new SolidFillPathDrawer { Color = Color.black }
        );

        public override VisualElement Create(BaseWidget parentWidget) {
            return new Widget(paint, trackHeight);
        }

        public class Widget : GenericSliderThumb {
            private readonly ScriptablePaint _paint;
            private readonly float _pointSize = 8f;
            private readonly StyleLength _trackHeight;
            private float _dragOffset;
            private int _pointerId = -1;

            public Widget(ScriptablePaint paint, StyleLength trackHeight) {
                _paint = paint;
                _trackHeight = trackHeight;

                RegisterCallback<PointerDownEvent>(OnPointerDown);
                RegisterCallback<PointerMoveEvent>(OnPointerMove);
                RegisterCallback<PointerUpEvent>(OnPointerUp);
                RegisterCallback<PointerCancelEvent>(OnPointerCancel);
                RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);

                generateVisualContent = GenerateVisualContent;
            }

            private void GenerateVisualContent(MeshGenerationContext obj) {
                var canvas = new PaintCanvas(obj);
                var rect = canvas.canvasRect;
                var bounds = slider.Axis switch {
                    Axis.Horizontal => rect.LayoutSimple(Alignment.Center, Length.Auto(), _trackHeight),
                    Axis.Vertical   => rect.LayoutSimple(Alignment.Center, _trackHeight, Length.Auto()),
                    _               => throw new ArgumentOutOfRangeException()
                };
                _paint?.Draw(canvas, bounds);
            }

            public override void Rebuild(GenericSlider parentSlider, Rect sliderDimensions) {
                base.Rebuild(parentSlider, sliderDimensions);
                ApplySizeAndPosition();
            }

            public override void SetValue(float newValue) {
                base.SetValue(newValue);
                ApplySizeAndPosition();
            }

            public override void SetRange(float newRange) {
                base.SetRange(newRange);
                ApplySizeAndPosition();
            }

            public override void SetDimensions(Rect newDimensions) {
                base.SetDimensions(newDimensions);
                ApplySizeAndPosition();
            }

            private void OnPointerDown(PointerDownEvent evt) {
                if (_pointerId != -1) return;

                _pointerId = evt.pointerId;
                this.CapturePointer(_pointerId);
                _dragOffset = slider.Axis.GetVectorComponent(evt.localPosition);
                evt.StopPropagation();
            }

            private void OnPointerMove(PointerMoveEvent evt) {
                if (evt.pointerId != _pointerId) return;
                evt.StopPropagation();
                var sliderLocal = slider.WorldToLocal(evt.position);
                var mainAxis = slider.Axis;
                var availableSize = mainAxis.GetRectSize(dimensions);
                var effectiveSize = Mathf.Approximately(range, 0f) ? _pointSize : availableSize * range;
                var leeway = Mathf.Max(0f, availableSize - effectiveSize);

                if (Mathf.Approximately(leeway, 0f)) {
                    slider.Value = 0f;
                    evt.StopPropagation();
                    return;
                }

                var pointerAxisPos = slider.Axis.GetVectorComponent(sliderLocal - dimensions.min);
                var newOffset = pointerAxisPos - _dragOffset;
                newOffset = Mathf.Clamp(newOffset, 0f, leeway);
                var newValue = newOffset / leeway;
                slider.Value = newValue;
            }

            private void OnPointerUp(PointerUpEvent evt) {
                if (evt.pointerId != _pointerId) return;
                EndDrag(evt.pointerId);
                evt.StopPropagation();
            }

            private void OnPointerCancel(PointerCancelEvent evt) {
                if (evt.pointerId != _pointerId) return;
                EndDrag(evt.pointerId);
                evt.StopPropagation();
            }

            private void OnPointerCaptureOut(PointerCaptureOutEvent evt) {
                if (_pointerId == -1) return;
                _pointerId = -1;
                _dragOffset = 0f;
            }

            private void EndDrag(int pointerId) {
                if (this.HasPointerCapture(pointerId)) this.ReleasePointer(pointerId);
                _pointerId = -1;
                _dragOffset = 0f;
            }

            private void ApplySizeAndPosition() {
                var mainAxis = slider.Axis;
                var crossAxis = mainAxis.Opposite();
                var availableSize = mainAxis.GetRectSize(dimensions);
                var effectiveSize = Mathf.Approximately(range, 0f) ? _pointSize : availableSize * range;
                var leeway = availableSize - effectiveSize;
                leeway = Mathf.Max(0f, leeway);
                var offset = Mathf.Approximately(leeway, 0f) ? 0f : value * leeway;

                mainAxis.SetBegin(this, offset);
                mainAxis.SetSize(this, effectiveSize);
                crossAxis.SetSize(this, crossAxis.GetRectSize(dimensions));
            }
        }
    }
}