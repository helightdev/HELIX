using System;
using System.Runtime.CompilerServices;
using HELIX.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Extensions {
    public static partial class VisualElementExtensions {
        public static IVisualElementScheduledItem Tween(this IVisualElementScheduler scheduler, long durationMs,
            Action<float> onUpdate) {
            var isActive = true;
            var progress = 0f;
            return scheduler.Execute(state => {
                var stepValue = (float)state.deltaTime / durationMs;
                progress = Mathf.Clamp01(progress + stepValue);
                onUpdate(progress);
                if (progress >= 1f) isActive = false;
            }).Until(() => !isActive);
        }

        public static IVisualElementScheduledItem Tween(this IVisualElementScheduler scheduler, long durationMs,
            float from, float to, Action<float> onUpdate) {
            return scheduler.Tween(durationMs, progress => {
                var value = Mathf.LerpUnclamped(from, to, progress);
                onUpdate(value);
            });
        }
        
        public static IVisualElementScheduledItem Tween(this IVisualElementScheduler scheduler, long durationMs,
            Color from, Color to, Action<Color> onUpdate) {
            return scheduler.Tween(durationMs, progress => {
                var value = Color.LerpUnclamped(from, to, progress);
                onUpdate(value);
            });
        }
    }
}