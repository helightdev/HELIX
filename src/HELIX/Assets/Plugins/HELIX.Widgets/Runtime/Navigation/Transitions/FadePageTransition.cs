using HELIX.Extensions;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Navigation.Transitions {
    [UxmlObject]
    public partial class FadePageTransition : PageTransition {
        public FadePageTransition() { }

        public FadePageTransition(int durationMs) {
            Duration = durationMs;
        }

        private FadePageTransition(int durationMs, EasingMode easing) {
            Duration = durationMs;
            Easing = easing;
        }

        [UxmlAttribute]
        public int Duration { get; set; } = 100;

        [UxmlAttribute]
        public EasingMode Easing { get; set; } = EasingMode.Linear;

        public override void Start(PageTransitionContext context) {
            var added = context.modificationResult.AddedPages;
            var removed = context.modificationResult.RemovedPages;
            var mmdi = context.modificationResult.MinMaxDeltaIndices;
            if (mmdi.w < mmdi.x && mmdi.z != -1) {
                foreach (var page in added) {
                    page.style.opacity = 1f;
                    page.style.display = DisplayStyle.Flex;
                }

                added = NavPageBuffer.Empty;
            }

            if (mmdi.y < mmdi.z && mmdi.x != -1) {
                foreach (var page in removed) {
                    page.style.opacity = 1f;
                    page.style.display = DisplayStyle.Flex;
                }

                removed = NavPageBuffer.Empty;
            }

            foreach (var page in added) {
                page.style.display = DisplayStyle.Flex;
                page.style.opacity = 0f;
            }

            foreach (var page in removed) {
                page.style.opacity = 1f;
                page.style.display = DisplayStyle.Flex;
            }

            context.ScheduledTransition(scheduler => scheduler.Tween(
                    Duration,
                    t => {
                        t = Easing.Eval(t);

                        foreach (var page in added) page.style.opacity = t;

                        foreach (var page in removed) page.style.opacity = 1f - t;
                    }
                )
            );
        }

        public override void Finish(PageTransitionContext context) {
            foreach (var page in context.modificationResult.AddedPages) page.style.opacity = 1f;

            foreach (var page in context.modificationResult.RemovedPages) page.style.opacity = 1f;
        }
    }
}