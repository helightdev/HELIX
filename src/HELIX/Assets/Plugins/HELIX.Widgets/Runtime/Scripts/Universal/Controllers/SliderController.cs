using System;
using HELIX.Widgets.Scrolling;
using HELIX.Widgets.Signals;
using UnityEngine;

namespace HELIX.Widgets.Universal.Controllers {
    public class SliderController : ValueSignal<float>, ISignalObserver {
        public readonly WidgetStateController widgetState;
        public bool enabled = true;
        private float _thumbRange = 0.1f;
        private ScrollController _linkedScrollController;

        public float ThumbRange {
            get => _thumbRange;
            set {
                if (Mathf.Approximately(_thumbRange, value)) return;
                _thumbRange = Mathf.Clamp01(value);
                NotifyObservers();
            }
        }

        public bool Enabled => enabled && (widgetState?.PeekValue().Enabled() ?? true);
        public ScrollController LinkedScrollController => _linkedScrollController;
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
            if (_linkedScrollController?.ScrollPosition != null) {
                _linkedScrollController.NormalizedScrollOffset = current;
            }
            if (!Mathf.Approximately(oldValue, current)) onChanged?.Invoke(current);
        }

        public override void SetWithoutNotify(float newValue) {
            base.SetWithoutNotify(Mathf.Clamp01(newValue));
        }

        public void LinkScrollController(ScrollController scrollController, bool syncValueFromScroll = true) {
            if (_linkedScrollController == scrollController) {
                RefreshFromLinkedScroll(syncValueFromScroll);
                return;
            }

            _linkedScrollController?.RemoveObserver(this);
            _linkedScrollController = scrollController;
            _linkedScrollController?.AddObserver(this);
            RefreshFromLinkedScroll(syncValueFromScroll);
        }

        public void UnlinkScrollController() {
            _linkedScrollController?.RemoveObserver(this);
            _linkedScrollController = null;
        }

        public void RefreshFromLinkedScroll(bool syncValue = true) {
            var position = _linkedScrollController?.ScrollPosition;
            if (position == null) return;

            var extentTotal = position.ExtentTotal;
            var extentInside = position.ExtentInside;
            ThumbRange = extentTotal <= 0f ? 1f : Mathf.Clamp01(extentInside / extentTotal);

            if (syncValue) {
                SetValue(_linkedScrollController.NormalizedScrollOffset);
            }
        }

        public void OnSignalChanged(Signal signal) {
            if (signal == _linkedScrollController) {
                RefreshFromLinkedScroll();
            }
        }

        public void OnSignalRemoved(Signal signal) {
            if (signal == _linkedScrollController) {
                _linkedScrollController = null;
            }
        }

        public override void Dispose() {
            UnlinkScrollController();
            base.Dispose();
        }
    }
}