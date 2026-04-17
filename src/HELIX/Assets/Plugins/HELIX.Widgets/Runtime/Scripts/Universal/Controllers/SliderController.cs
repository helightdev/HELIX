using System;
using HELIX.Widgets.Signals;
using UnityEngine;

namespace HELIX.Widgets.Universal.Controllers {
    public class SliderController : ValueSignal<float> {
        public readonly WidgetStateController widgetState;
        public bool enabled = true;
        private float _thumbRange = 0.8f;

        public float ThumbRange {
            get => _thumbRange;
            set {
                if (Mathf.Approximately(_thumbRange, value)) return;
                _thumbRange = Mathf.Clamp01(value);
                NotifyObservers();
            }
        }

        public bool Enabled => enabled && (widgetState?.PeekValue().Enabled() ?? true);
        public Action<float> onChanged;

        public SliderController(WidgetStateController widgetState = null, float initialValue = 0f)
            :
            base(Mathf.Clamp01(initialValue)) {
            this.widgetState = widgetState;
        }

        public override void SetValue(float newValue) {
            var oldValue = PeekValue();
            base.SetValue(Mathf.Clamp01(newValue));
            var current = PeekValue();
            if (!Mathf.Approximately(oldValue, current)) onChanged?.Invoke(current);
        }

        public override void SetWithoutNotify(float newValue) {
            base.SetWithoutNotify(Mathf.Clamp01(newValue));
        }
    }
}