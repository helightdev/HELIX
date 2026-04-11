using HELIX.Widgets.Signals;
using Unity.Mathematics;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Scrolling
{
    public abstract class ScrollPosition : Signal {
        public abstract float Min { get; }
        public abstract float Max { get; }
        public abstract float Offset { get; set; }

        public virtual float NormalizedOffset {
            get => math.unlerp(Min, Max, Offset);
            set => Offset = math.lerp(Min, Max, value);
        }

        public virtual void Restore(float offset) {
            Offset = offset;
        }

        public virtual void AnimateTo(float offset, TimeValue durationMs, EasingMode easing) {
            Offset = offset;
        }
        
        public virtual void ScrollTo(VisualElement element) {
            // Implement in subclasses if needed
        }
    }
}