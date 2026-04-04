using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Navigation {
    [UxmlObject]
    public abstract partial class PageTransition {
        public abstract void Start(PageTransitionContext context);
        public virtual void Finish(PageTransitionContext context) { }
    }

    public abstract class PageTransitionHandle {
        public abstract bool IsComplete { get; }
    }

    public class PageTransitionContext {
        public readonly List<PageTransitionHandle> handles = new();
        public readonly NavStackModificationResult modificationResult;
        public readonly NavStackElement stack;
        public readonly PageTransition transition;

        public PageTransitionContext(
            PageTransition transition,
            NavStackElement stack,
            NavStackModificationResult modificationResult
        ) {
            this.transition = transition;
            this.stack = stack;
            this.modificationResult = modificationResult;
        }

        public void ScheduledTransition(
            Func<IVisualElementScheduler, IVisualElementScheduledItem> func
        ) {
            var item = func(stack.schedule);
            var handle = new ScheduledItemPageTransitionHandle(item);
            handles.Add(handle);
        }
    }

    public class ScheduledItemPageTransitionHandle : PageTransitionHandle {
        private readonly IVisualElementScheduledItem _scheduledItem;

        public ScheduledItemPageTransitionHandle(IVisualElementScheduledItem scheduledItem) {
            _scheduledItem = scheduledItem;
        }

        public override bool IsComplete => !_scheduledItem.isActive;
    }
}