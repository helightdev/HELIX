using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;

namespace HELIX.Widgets.Prompts {
  public static class HelixInputHelper {

    public static bool TryFindBinding(InputAction action, InputDeviceType type, out InputBinding binding) {
      var groupName = type switch {
        InputDeviceType.KeyboardMouse => "Keyboard&Mouse",
        InputDeviceType.Gamepad => "Gamepad",
        InputDeviceType.Touchscreen => "Touch",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
      };
      binding = default;

      var mask = InputBinding.MaskByGroup(groupName);
      foreach (var actionBinding in action.bindings) {
        if (!mask.Matches(actionBinding)) continue;
        binding = actionBinding;
        return true;
      }

      return false;
    }

    public static InputConfiguration DetectDefaultInputDeviceType() {
      var gamepad = DetectPlatformGamepadVariant();
      var deviceType = SystemInfo.deviceType;
      if (gamepad != GamepadVariant.Generic || deviceType == DeviceType.Console)
        return new InputConfiguration(InputDeviceType.Gamepad, gamepad);

      var currentGamepad = Gamepad.current;
      if (currentGamepad != null) gamepad = currentGamepad.DetectGamepadVariant();

      if (deviceType == DeviceType.Handheld) {
        if (currentGamepad != null) return new InputConfiguration(InputDeviceType.Gamepad, gamepad);
        return new InputConfiguration(InputDeviceType.Touchscreen, GamepadVariant.Generic);
      }

      return new InputConfiguration(InputDeviceType.KeyboardMouse, GamepadVariant.Generic);
    }

    public static InputConfiguration DetectInputFromAction(InputControl control) {
      var gamepad = DetectPlatformGamepadVariant();
      if (gamepad != GamepadVariant.Generic) return new InputConfiguration(InputDeviceType.Gamepad, gamepad);

      if (control.device is Gamepad gamepadDevice) {
        gamepad = gamepadDevice.DetectGamepadVariant();
        return new InputConfiguration(InputDeviceType.Gamepad, gamepad);
      }

      return new InputConfiguration(
        control.device is Touchscreen ? InputDeviceType.Touchscreen : InputDeviceType.KeyboardMouse,
        GamepadVariant.Generic
      );
    }

    public static GamepadVariant DetectGamepadVariant(this Gamepad gamepad) {
      return gamepad switch {
        DualShockGamepad => GamepadVariant.PlayStation,
        XInputController => GamepadVariant.Xbox,
        _ => GamepadVariant.Generic
      };
    }

    public static GamepadVariant DetectPlatformGamepadVariant() {
      return Application.platform switch {
        RuntimePlatform.Switch or RuntimePlatform.Switch2
          => GamepadVariant.NintendoSwitch,
        RuntimePlatform.PS4 or RuntimePlatform.PS5
          => GamepadVariant.PlayStation,
        RuntimePlatform.XboxOne or RuntimePlatform.GameCoreXboxOne or RuntimePlatform.GameCoreXboxSeries
          => GamepadVariant.Xbox,
        _ => GamepadVariant.Generic
      };
    }
  }
}