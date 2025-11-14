using System;
using UnityEngine.UIElements;

namespace HELIX.Animation {
    /// <summary>
    /// Represents a scheduler that executes tasks in a debounced manner, ensuring that only one task
    /// is scheduled at a time within the Unity UIElements system.
    /// </summary>
    public class DebouncedScheduler : IVisualElementScheduler {
        private IVisualElementScheduledItem _scheduledItem;
        private readonly IVisualElementScheduler _scheduler;

        public DebouncedScheduler(IVisualElementScheduler scheduler) {
            _scheduler = scheduler;
        }

        public DebouncedScheduler(VisualElement element) : this(element.schedule) { }

        public void Stop() {
            _scheduledItem?.Pause();
            _scheduledItem = null;
        }

        public void Replace(IVisualElementScheduledItem newItem) {
            _scheduledItem?.Pause();
            _scheduledItem = newItem;
        }

        public IVisualElementScheduledItem Execute(Action<TimerState> timerUpdateEvent) {
            _scheduledItem?.Pause();
            _scheduledItem = _scheduler.Execute(timerUpdateEvent);
            return _scheduledItem;
        }

        public IVisualElementScheduledItem Execute(Action updateEvent) {
            _scheduledItem?.Pause();
            _scheduledItem = _scheduler.Execute(updateEvent);
            return _scheduledItem;
        }
    }
}