using System.Collections.Generic;
using HELIX.Widgets.Signals;

namespace HELIX.Widgets.Universal.Controllers {
    public class WidgetStateController : ValueSignal<WidgetState>, ISignalObserver {
        public static bool LastNavigated;

        private readonly HashSet<WidgetStateController> _children = new();
        public readonly WidgetState inheritance;
        public readonly WidgetState mask;

        public WidgetStateController(
            WidgetState mask = WidgetState.MetaAll,
            WidgetState inheritanceMask = WidgetState.MetaAll
        ) {
            this.mask = mask;
            inheritance = inheritanceMask;
            HelixInputDevice.Instance.AddObserver(new WeakSignalObserver(this));
        }

        public IEnumerable<WidgetStateController> Children => _children;

        public void OnSignalChanged(Signal signal) {
            SetValue(value);
        }

        public void AddInherited(WidgetStateController child) {
            _children.Add(child);
            child.AddObserver(new WeakSignalObserver(this));
        }

        public void Enable(WidgetState state) {
            SetValue(value | state);
        }

        public void Disable(WidgetState state) {
            SetValue(value & ~state);
        }

        public void DisableEnable(WidgetState disable, WidgetState enable) {
            SetValue((value & ~disable) | enable);
        }

        public void Toggle(WidgetState state) {
            SetValue(value ^ state);
        }

        public void Toggle(WidgetState state, bool toggle) {
            if (toggle) Enable(state);
            else Disable(state);
        }

        public void Clear() {
            SetValue(WidgetState.None);
        }

        public override void SetValue(WidgetState newValue) {
            var next = newValue | HelixInputDevice.Instance.Value.stateInputs;
            foreach (var child in _children) next |= child.Value & inheritance;
            next &= mask;

            var hasChangedNavigated = (next & WidgetState.Navigated) != (value & WidgetState.Navigated);
            if (hasChangedNavigated) LastNavigated = (next & WidgetState.Navigated) != 0;

            base.SetValue(next);
        }
    }
}