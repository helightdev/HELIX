using System;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Navigation.Transitions {
    [UxmlObject]
    public partial class ModePageTransition : PageTransition {

        [UxmlObjectReference("Default")]
        public PageTransition DefaultTransition { get; set; } = new InstantPageTransition();

        [UxmlObjectReference("Push")]
        public PageTransition PushTransition { get; set; }

        [UxmlObjectReference("Pop")]
        public PageTransition PopTransition { get; set; }

        [UxmlObjectReference("Replace")]
        public PageTransition ReplaceTransition { get; set; }

        public override void Start(PageTransitionContext context) {
            switch (context.modificationResult.Type) {
                case NavModificationType.Push: (PushTransition ?? DefaultTransition)?.Start(context); break;
                case NavModificationType.Pop: (PopTransition ?? DefaultTransition)?.Start(context); break;
                case NavModificationType.Replace: (ReplaceTransition ?? DefaultTransition)?.Start(context); break;
                case NavModificationType.Complex: DefaultTransition?.Start(context); break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        public override void Finish(PageTransitionContext context) {
            switch (context.modificationResult.Type) {
                case NavModificationType.Push: (PushTransition ?? DefaultTransition)?.Finish(context); break;
                case NavModificationType.Pop: (PopTransition ?? DefaultTransition)?.Finish(context); break;
                case NavModificationType.Replace: (ReplaceTransition ?? DefaultTransition)?.Finish(context); break;
                case NavModificationType.Complex: DefaultTransition?.Finish(context); break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

    }
}