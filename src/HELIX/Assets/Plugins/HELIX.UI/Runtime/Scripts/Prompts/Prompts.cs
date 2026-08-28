using System;
using System.Collections.Generic;
using HELIX.Coloring;
using HELIX.Compose;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;
using UnityEngine.UIElements;

namespace HELIX.UI.Prompts {
  public enum InputDeviceType { KeyboardMouse, Gamepad, Touchscreen }

  public enum GamepadVariant { Generic, Xbox, PlayStation, NintendoSwitch, SteamDeck, SteamController }

  [Serializable]
  public struct InputConfiguration : IEquatable<InputConfiguration> {
    public static readonly InputConfiguration Default = new(InputDeviceType.KeyboardMouse);
    public InputDeviceType deviceType;
    public GamepadVariant gamepadVariant;
    public int flag;

    public InputConfiguration(
      InputDeviceType deviceType,
      GamepadVariant gamepadVariant = GamepadVariant.Generic,
      int flag = 0
    ) {
      this.deviceType = deviceType;
      this.gamepadVariant = gamepadVariant;
      this.flag = flag;
    }

    public bool Equals(InputConfiguration other) => deviceType == other.deviceType &&
      gamepadVariant == other.gamepadVariant && flag == other.flag;

    public override bool Equals(object obj) => obj is InputConfiguration other && Equals(other);
    public override int GetHashCode() => HashCode.Combine((int)deviceType, (int)gamepadVariant, flag);
  }

  public sealed class HelixInputController : ValueSignal<InputConfiguration> {
    public static readonly ContextKey<HelixInputController> Key = new("HelixInputController");
    public HelixInputController(InputConfiguration value) : base(value) { }
  }

  public static class HelixInputHelper {
    public static bool TryFindBinding(InputAction action, InputDeviceType type, out InputBinding binding) {
      binding = default;
      if (action == null) return false;
      var group = type switch {
        InputDeviceType.KeyboardMouse => "Keyboard&Mouse",
        InputDeviceType.Gamepad => "Gamepad",
        InputDeviceType.Touchscreen => "Touch",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
      };
      var mask = InputBinding.MaskByGroup(group);
      foreach (var candidate in action.bindings) {
        if (!mask.Matches(candidate) || candidate.isComposite) continue;
        binding = candidate;
        return true;
      }
      return false;
    }

    public static InputConfiguration DetectDefaultInputDeviceType() {
      var variant = DetectPlatformGamepadVariant();
      if (variant != GamepadVariant.Generic || SystemInfo.deviceType == DeviceType.Console)
        return new InputConfiguration(InputDeviceType.Gamepad, variant);
      if (Gamepad.current != null)
        return new InputConfiguration(InputDeviceType.Gamepad, Gamepad.current.DetectGamepadVariant());
      return SystemInfo.deviceType == DeviceType.Handheld
        ? new InputConfiguration(InputDeviceType.Touchscreen)
        : InputConfiguration.Default;
    }

    public static InputConfiguration DetectInputFromAction(InputControl control) {
      if (control == null) return InputConfiguration.Default;
      var platform = DetectPlatformGamepadVariant();
      if (platform != GamepadVariant.Generic) return new InputConfiguration(InputDeviceType.Gamepad, platform);
      if (control.device is Gamepad gamepad)
        return new InputConfiguration(InputDeviceType.Gamepad, gamepad.DetectGamepadVariant());
      return new InputConfiguration(
        control.device is Touchscreen ? InputDeviceType.Touchscreen : InputDeviceType.KeyboardMouse
      );
    }

    public static GamepadVariant DetectGamepadVariant(this Gamepad gamepad) => gamepad switch {
      DualShockGamepad => GamepadVariant.PlayStation,
      XInputController => GamepadVariant.Xbox,
      _ => GamepadVariant.Generic
    };

    public static GamepadVariant DetectPlatformGamepadVariant() => Application.platform switch {
      RuntimePlatform.Switch or RuntimePlatform.Switch2 => GamepadVariant.NintendoSwitch,
      RuntimePlatform.PS4 or RuntimePlatform.PS5 => GamepadVariant.PlayStation,
      RuntimePlatform.XboxOne or RuntimePlatform.GameCoreXboxOne or RuntimePlatform.GameCoreXboxSeries
        => GamepadVariant.Xbox,
      _ => GamepadVariant.Generic
    };
  }

  public readonly struct PromptVisualSpec : ISpec<PromptVisualSpec> {
    public readonly BackgroundImage? image;
    public readonly string label;
    public readonly Color tint;
    public readonly BoxConstraints constraints;
    public readonly TextRole textRole;

    public PromptVisualSpec(
      BackgroundImage? image = null,
      string label = null,
      Color? tint = null,
      BoxConstraints? constraints = null,
      TextRole textRole = TextRole.LabelMedium
    ) {
      this.image = image;
      this.label = label;
      this.tint = tint ?? Colors.White;
      this.constraints = constraints ?? BoxConstraints.Null;
      this.textRole = textRole;
    }

    public ReadComposable<PromptVisualSpec> GetDefault(in PromptVisualSpec spec) => ComposeDefault;

    private static void ComposeDefault(ref Composition cx, in PromptVisualSpec spec) {
      if (spec.image.HasValue) {
        cx.DrawImage(spec.image, tint: spec.tint, constraints: spec.constraints);
      } else {
        cx.Text(spec.label ?? string.Empty, spec.textRole)
          .Size(spec.constraints)
          .TextAlign(TextAnchor.MiddleCenter);
      }
    }
  }

  public interface IPromptProvider {
    bool TryResolvePrompt(
      InputAction action,
      in InputConfiguration configuration,
      in PromptSpec prompt,
      out PromptVisualSpec visual
    );
  }

  public readonly struct PromptSpec : ISpec<PromptSpec> {
    public readonly InputAction action;
    public readonly string actionName;
    public readonly IPromptProvider provider;
    public readonly InputConfiguration? configuration;
    public readonly BoxConstraints constraints;
    public readonly Color tint;
    public readonly TextRole textRole;

    public PromptSpec(
      InputAction action,
      IPromptProvider provider = null,
      InputConfiguration? configuration = null,
      BoxConstraints? constraints = null,
      Color? tint = null,
      TextRole textRole = TextRole.LabelMedium
    ) : this(null, action, provider, configuration, constraints, tint, textRole) { }

    public PromptSpec(
      string actionName,
      IPromptProvider provider = null,
      InputConfiguration? configuration = null,
      BoxConstraints? constraints = null,
      Color? tint = null,
      TextRole textRole = TextRole.LabelMedium
    ) : this(actionName, null, provider, configuration, constraints, tint, textRole) { }

    private PromptSpec(
      string actionName,
      InputAction action,
      IPromptProvider provider,
      InputConfiguration? configuration,
      BoxConstraints? constraints,
      Color? tint,
      TextRole textRole
    ) {
      this.actionName = actionName;
      this.action = action;
      this.provider = provider;
      this.configuration = configuration;
      this.constraints = constraints ?? BoxConstraints.Null;
      this.tint = tint ?? Colors.White;
      this.textRole = textRole;
    }

    public ReadComposable<PromptSpec> GetDefault(in PromptSpec spec) => ComposeDefault;

    private static void ComposeDefault(ref Composition cx, in PromptSpec spec) {
      var configuration = spec.configuration;
      var controller = HelixInputController.Key[in cx];
      if (!configuration.HasValue) {
        cx.SubscribeTo(controller);
        configuration = controller.PeekValue();
      }
      var action = spec.action ?? InputSystem.actions?.FindAction(spec.actionName);
      var provider = spec.provider ?? KennyPromptProvider.Default;
      var resolvedConfiguration = configuration.Value;
      if (!provider.TryResolvePrompt(action, in resolvedConfiguration, in spec, out var visual)) {
        visual = new PromptVisualSpec(
          label: action?.name ?? spec.actionName,
          tint: spec.tint,
          constraints: spec.constraints,
          textRole: spec.textRole
        );
      }
      cx.Spec(in visual);
    }
  }

  public sealed class KennyPromptProvider : IPromptProvider {
    public static readonly KennyPromptProvider Default = new();
    private readonly Dictionary<string, VectorImage> _images = new(StringComparer.OrdinalIgnoreCase);

    public bool TryResolvePrompt(
      InputAction action,
      in InputConfiguration configuration,
      in PromptSpec prompt,
      out PromptVisualSpec visual
    ) {
      var label = action?.name ?? prompt.actionName;
      if (!HelixInputHelper.TryFindBinding(action, configuration.deviceType, out var binding)) {
        visual = new PromptVisualSpec(
          label: label,
          tint: prompt.tint,
          constraints: prompt.constraints,
          textRole: prompt.textRole
        );
        return true;
      }
      var display = binding.ToDisplayString(out _, out var controlName);
      if (TryResolveImage(configuration, controlName, out var image)) {
        visual = new PromptVisualSpec(image, tint: prompt.tint, constraints: prompt.constraints);
        return true;
      }
      visual = new PromptVisualSpec(
        label: string.IsNullOrEmpty(display) ? label : display,
        tint: prompt.tint,
        constraints: prompt.constraints,
        textRole: prompt.textRole
      );
      return true;
    }

    private bool TryResolveImage(InputConfiguration configuration, string control, out BackgroundImage image) {
      image = default;
      if (string.IsNullOrEmpty(control)) return false;
      var folder = configuration.deviceType == InputDeviceType.KeyboardMouse
        ? "KeyboardMouse"
        : configuration.gamepadVariant switch {
          GamepadVariant.PlayStation => "PlayStationSeries",
          GamepadVariant.NintendoSwitch => "NintendoSwitch2",
          GamepadVariant.SteamDeck => "SteamDeck",
          GamepadVariant.SteamController => "SteamController",
          _ => "XboxSeries"
        };
      var resource = PromptResourceName(configuration.deviceType, configuration.gamepadVariant, control);
      if (resource == null) return false;
      var path = $"helix/input/{folder}/Vector/{resource}";
      if (!_images.TryGetValue(path, out var vector)) {
        vector = Resources.Load<VectorImage>(path);
        _images[path] = vector;
      }
      if (!vector) return false;
      image = BackgroundImage.VectorImage(
        vector,
        new ImageScaling(
          new BackgroundSize(BackgroundSizeType.Contain),
          new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat)
        )
      );
      return true;
    }

    private static string PromptResourceName(
      InputDeviceType device,
      GamepadVariant variant,
      string control
    ) {
      if (device != InputDeviceType.Gamepad) return KeyboardResource(control);
      return variant switch {
        GamepadVariant.PlayStation => GamepadResource("playstation", control, "triangle", "cross", "square", "circle"),
        GamepadVariant.NintendoSwitch => GamepadResource("switch", control, "y", "a", "x", "b"),
        GamepadVariant.SteamDeck => GamepadResource("steamdeck", control, "y", "a", "x", "b"),
        GamepadVariant.SteamController => SteamControllerResource(control),
        _ => GamepadResource("xbox", control, "y", "a", "x", "b")
      };
    }

    private static string KeyboardResource(string control) {
      var key = control switch {
        "upArrow" => "arrow_up", "downArrow" => "arrow_down",
        "leftArrow" => "arrow_left", "rightArrow" => "arrow_right",
        "space" => "space", "enter" => "enter", "backspace" => "backspace",
        "leftShift" or "rightShift" or "shift" => "shift",
        "leftCtrl" or "rightCtrl" or "ctrl" => "ctrl",
        "leftAlt" or "rightAlt" or "alt" => "alt",
        "capsLock" => "capslock", "numLock" => "numlock",
        "pageUp" => "page_up", "pageDown" => "page_down",
        "leftBracket" => "bracket_open", "rightBracket" => "bracket_close",
        "printScreen" => "printscreen", "equals" => "equals",
        "minus" => "minus", "semicolon" => "semicolon", "quote" => "quote",
        "comma" => "comma", "period" => "period", "slash" => "slash_forward",
        "backslash" => "slash_back", "anyKey" => "any",
        "leftButton" => null, "rightButton" => null, "middleButton" => null,
        _ => control.StartsWith("numpad", StringComparison.Ordinal)
          ? control.Substring(6)
          : control.ToLowerInvariant()
      };
      if (control is "leftButton" or "rightButton")
        return $"mouse_{(control == "leftButton" ? "left" : "right")}_outline";
      if (control == "middleButton") return "mouse_scroll_outline";
      return key == null ? null : $"keyboard_{key}_outline";
    }

    private static string GamepadResource(
      string family,
      string control,
      string north,
      string south,
      string west,
      string east
    ) {
      var token = control switch {
        "leftStick" => "stick_l", "rightStick" => "stick_r",
        "leftStickPress" => "stick_l_press", "rightStickPress" => "stick_r_press",
        "leftShoulder" => family == "playstation" ? "trigger_l1" : family == "switch" ? "button_l" : "lb",
        "rightShoulder" => family == "playstation" ? "trigger_r1" : family == "switch" ? "button_r" : "rb",
        "leftTrigger" => family == "playstation" ? "trigger_l2" : family == "switch" ? "button_zl" : "lt",
        "rightTrigger" => family == "playstation" ? "trigger_r2" : family == "switch" ? "button_zr" : "rt",
        "buttonNorth" => "button_" + north, "buttonSouth" => "button_" + south,
        "buttonWest" => "button_" + west, "buttonEast" => "button_" + east,
        "buttonSelect" => family switch {
          "playstation" => "5_button_create", "switch" => "button_minus", _ => "button_back"
        },
        "buttonStart" => family switch {
          "playstation" => "5_button_options", "switch" => "button_plus", _ => "button_start"
        },
        "touchpadButton" when family == "playstation" => "5_touchpad_press",
        _ when control.StartsWith("dpad/", StringComparison.Ordinal) => "dpad_" + control.Substring(5),
        _ when control.StartsWith("leftStick/", StringComparison.Ordinal) => "stick_l_" + control.Substring(10),
        _ when control.StartsWith("rightStick/", StringComparison.Ordinal) => "stick_r_" + control.Substring(11),
        "dpad" => "dpad",
        _ => null
      };
      if (token == null) return null;
      var suffix = control is "dpad" or "leftStick" or "rightStick" ? "" : "_outline";
      if (family == "steamdeck") suffix = "";
      return family + "_" + token + suffix;
    }

    private static string SteamControllerResource(string control) {
      var token = control switch {
        "leftStick" => "stick", "leftStickPress" => "stick_l_press",
        "rightStick" => "pad", "rightStickPress" => "pad_center",
        "leftShoulder" => "lb", "rightShoulder" => "rb",
        "leftTrigger" => "lt", "rightTrigger" => "rt",
        "buttonNorth" => "button_y", "buttonSouth" => "button_a",
        "buttonWest" => "button_x", "buttonEast" => "button_b",
        "buttonSelect" => "button_back_icon", "buttonStart" => "button_start_icon",
        _ when control.StartsWith("dpad/", StringComparison.Ordinal) => "dpad_" + control.Substring(5),
        _ when control.StartsWith("leftStick/", StringComparison.Ordinal) => "stick_" + control.Substring(10),
        _ when control.StartsWith("rightStick/", StringComparison.Ordinal) => "pad_" + control.Substring(11),
        "dpad" => "dpad_all",
        _ => null
      };
      return token == null ? null : "steam_" + token + "_outline";
    }
  }
}