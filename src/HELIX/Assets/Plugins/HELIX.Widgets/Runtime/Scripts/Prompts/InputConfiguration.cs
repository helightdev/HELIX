using System;

namespace HELIX.Widgets.Prompts {
  [Serializable]
  public struct InputConfiguration : IEquatable<InputConfiguration> {
    public static readonly InputConfiguration Default = new(InputDeviceType.KeyboardMouse, GamepadVariant.Generic);

    public InputDeviceType deviceType;
    public GamepadVariant gamepadVariant;
    public int flag;

    public WidgetState InputMask => deviceType switch {
      InputDeviceType.KeyboardMouse => WidgetState.InputKeyboardMouse,
      InputDeviceType.Gamepad => WidgetState.InputGamepad,
      InputDeviceType.Touchscreen => WidgetState.InputTouch,
      _ => throw new ArgumentOutOfRangeException()
    };

    public InputConfiguration(InputDeviceType deviceType, GamepadVariant gamepadVariant, int flag = 0) {
      this.deviceType = deviceType;
      this.gamepadVariant = gamepadVariant;
      this.flag = flag;
    }

    public bool Equals(InputConfiguration other) {
      return deviceType == other.deviceType && gamepadVariant == other.gamepadVariant && flag == other.flag;
    }

    public override bool Equals(object obj) {
      return obj is InputConfiguration other && Equals(other);
    }

    public override int GetHashCode() {
      return HashCode.Combine((int)deviceType, (int)gamepadVariant, flag);
    }
  }
}