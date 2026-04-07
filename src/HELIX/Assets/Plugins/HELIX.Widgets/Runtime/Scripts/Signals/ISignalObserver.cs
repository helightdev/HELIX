namespace HELIX.Widgets.Signals {
    public interface ISignalObserver {
        void OnSignalChanged(Signal signal) { }
        void OnSignalRemoved(Signal signal) { }
        void OnSignalAdded(Signal signal) { }
        void OnSignalDirty(Signal signal) { }
    }
}