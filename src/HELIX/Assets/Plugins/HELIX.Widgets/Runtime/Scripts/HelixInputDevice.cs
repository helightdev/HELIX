using HELIX.Widgets.Signals;

namespace HELIX.Widgets {
    public class HelixInputDevice : ValueSignal<InputDeviceBase> {

        public static readonly HelixInputDevice Instance = new(new UnknownKeyboardDevice());

        private HelixInputDevice(InputDeviceBase value = null, bool equality = true) : base(value, equality) { }

    }

    public abstract class InputDeviceBase {

        public WidgetState stateInputs;

    }

    public class UnknownKeyboardDevice : InputDeviceBase {

        public UnknownKeyboardDevice() {
            stateInputs = WidgetState.InputKeyboard | WidgetState.InputMouse;
        }

    }
}