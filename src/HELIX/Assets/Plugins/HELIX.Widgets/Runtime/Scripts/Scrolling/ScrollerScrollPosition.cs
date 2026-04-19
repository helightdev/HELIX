using HELIX.Animation;
using HELIX.Extensions;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Scrolling {
    public class ScrollerScrollPosition : ScrollPosition {
        public readonly Scroller scroller;
        public readonly ScrollView scrollView;

        private float _lastValue;
        private float _lastExtentTotal;
        private DebouncedScheduler _debouncedScheduler;

        public ScrollerScrollPosition(Scroller scroller, ScrollView scrollView) {
            this.scroller = scroller;
            this.scrollView = scrollView;
            _debouncedScheduler = new DebouncedScheduler(scroller);
            scroller.value = 0;
            scroller.valueChanged += OnValueChanged;
            scrollView.contentContainer.RegisterCallback<GeometryChangedEvent>(_ => {
                OnValueChanged(scroller.value);
            });
        }

        public override float Min => scroller.lowValue;
        public override float Max => scroller.highValue;
        public override float ExtentInside =>
            scroller.direction == SliderDirection.Horizontal ? scrollView.layout.width : scrollView.layout.height;

        public override float ExtentTotal => ExtentInside + scroller.highValue - scroller.lowValue;

        public override float Extent {
            get => scroller.value;
            set {
                if(!Mathf.Approximately(scroller.value, value)) _debouncedScheduler.Stop();
                _lastValue = value;
                scroller.value = value;
            }
        }

        private void OnValueChanged(float obj) {
            if (Mathf.Approximately(obj, _lastValue) && Mathf.Approximately(_lastExtentTotal, ExtentTotal)) return;
            _lastValue = obj;
            _lastExtentTotal = ExtentTotal;
            NotifyObservers();
        }

        public override void Dispose() {
            scroller.valueChanged -= OnValueChanged;
            base.Dispose();
        }

        public override void Restore(float offset) {
            _debouncedScheduler.Stop();
            _lastValue = offset;
            scroller.value = offset;
            scroller.schedule.Execute(() => {
                scroller.value = offset;
            }).ExecuteLater(1);
        }

        public override void AnimateTo(float offset, TimeValue duration, EasingMode easing) {
            var startValue = scroller.value;
            _debouncedScheduler.Tween(
                duration.ToMilliseconds(),
                t => {
                    var value = math.lerp(startValue, offset, easing.Eval(t));
                    scroller.value = value;
                }
            );
        }

        public override void ScrollTo(VisualElement element) {
            _debouncedScheduler.Stop();
            if (scrollView == null) throw new System.InvalidOperationException("ScrollView reference is null.");
            scrollView.ScrollTo(element);
        }
    }
}