using System;
using HELIX.Widgets.Universal;
using HELIX.Widgets.Universal.Styles;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Prompts.Kenny {
  public class KennyPromptProvider : IPromptProvider {
    private KennyVariant _variant;
    private GamepadVariant _gamepadVariant;
    private IPromptLayerProvider _gamepadProvider;
    private IPromptLayerProvider _keyboardMouseProvider;

    public bool TryResolvePrompt(
      BuildContext context,
      InputAction action,
      InputConfiguration configuration,
      out SubstanceLayers layers
    ) {
      if (configuration.deviceType == InputDeviceType.Gamepad && _gamepadVariant != configuration.gamepadVariant) {
        LoadVariant(configuration.gamepadVariant);
      }

      _keyboardMouseProvider ??= new KennyKeyboardMousePromptLayerProvider {
        Variant = _variant
      };

      var used = configuration.deviceType switch {
        InputDeviceType.KeyboardMouse => _keyboardMouseProvider,
        InputDeviceType.Gamepad => _gamepadProvider,
        InputDeviceType.Touchscreen => _keyboardMouseProvider,
        _ => throw new ArgumentOutOfRangeException()
      };

      layers = default;

      var label = action.name;
      if (HelixInputHelper.TryFindBinding(action, configuration.deviceType, out var binding)) {
        var display = binding.ToDisplayString(out var layoutName, out var controlName);
        if (used.TryResolvePromptLayer(context, controlName, out layers)) return true;
        label = display;
      }

      layers = Substance.Builder((_, _) => new HText(
          label,
          style: new TextStyle {
            color = Color.white,
            align = TextAnchor.MiddleCenter,
            generator = TextGeneratorType.Advanced,
            autoSize = new TextAutoSize(TextAutoSizeMode.BestFit, Length.Percent(20), Length.Percent(50))
          }
        ).Stretch()
      );
      return true;
    }

    private void LoadVariant(GamepadVariant variant) {
      _gamepadProvider = variant switch {
        GamepadVariant.Generic or GamepadVariant.Xbox => new KennyXboxSeriesPromptLayerProvider { Variant = _variant },
        GamepadVariant.PlayStation => new KennyPlaystationSeriesPromptLayerProvider { Variant = _variant },
        GamepadVariant.NintendoSwitch => new KennyNintendoSwitch2PromptLayerProvider { Variant = _variant },
        GamepadVariant.SteamDeck => new KennySteamDeckPromptLayerProvider { Variant = _variant },
        GamepadVariant.SteamController => new KennySteamControllerPromptLayerProvider { Variant = _variant },
        _ => throw new ArgumentOutOfRangeException(nameof(variant), variant, null)
      };
      _gamepadVariant = variant;
    }
  }
}