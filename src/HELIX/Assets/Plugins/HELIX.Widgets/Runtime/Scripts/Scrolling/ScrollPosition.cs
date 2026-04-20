using HELIX.Widgets.Signals;
using Unity.Mathematics;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Scrolling {
    public abstract class ScrollPosition : Signal {

        public abstract float Min { get; }
        public abstract float Max { get; }
        public abstract float Extent { get; set; }
        public abstract float ExtentInside { get; }
        public abstract float ExtentTotal { get; }

        public virtual float NormalizedOffset {
            get => math.unlerp(Min, Max, Extent);
            set => Extent = math.lerp(Min, Max, value);
        }

        public virtual void Restore(float offset) {
            Extent = offset;
        }

        public virtual void AnimateTo(float offset, TimeValue durationMs, EasingMode easing) {
            Extent = offset;
        }

        public virtual void ScrollTo(VisualElement element) {
            // Implement in subclasses if needed
        }

    }
}