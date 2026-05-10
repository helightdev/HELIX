using System.Collections.Generic;

namespace HELIX.Widgets.Prompts.Kenny {
  public class KennySteamControllerPromptLayerProvider : KennyResourcePromptLayerProvider {
    public KennySteamControllerPromptLayerProvider() : base(
      "helix/input/SteamController/SVGS",
      new Dictionary<string, string>() {
        // Stick Bindings
        ["leftStick"] = "steam_stick",
        ["leftStick/up"] = "steam_stick_up",
        ["leftStick/down"] = "steam_stick_down",
        ["leftStick/left"] = "steam_stick_left",
        ["leftStick/right"] = "steam_stick_right",
        ["leftStick/vertical"] = "steam_stick_vertical",
        ["leftStick/horizontal"] = "steam_stick_horizontal",
        ["leftStickPress"] = "steam_stick_l_press",

        ["rightStick"] = "steam_pad",
        ["rightStick/up"] = "steam_pad_up",
        ["rightStick/down"] = "steam_pad_down",
        ["rightStick/left"] = "steam_pad_left",
        ["rightStick/right"] = "steam_pad_right",
        ["rightStickPress"] = "steam_pad_center",

        // Trigger Bindings
        ["leftShoulder"] = "steam_lb",
        ["rightShoulder"] = "steam_rb",
        ["leftTrigger"] = "steam_lt",
        ["rightTrigger"] = "steam_rt",

        // D-Pad Bindings
        ["dpad"] = "steam_dpad",
        ["dpad/up"] = "steam_dpad_up+outline",
        ["dpad/down"] = "steam_dpad_down+outline",
        ["dpad/left"] = "steam_dpad_left+outline",
        ["dpad/right"] = "steam_dpad_right+outline",
        ["dpad/vertical"] = "steam_dpad_vertical+outline",
        ["dpad/horizontal"] = "steam_dpad_horizontal+outline",

        // Button Bindings
        ["buttonNorth"] = "steam_button_y+outline+color",
        ["buttonSouth"] = "steam_button_a+outline+color",
        ["buttonWest"] = "steam_button_x+outline+color",
        ["buttonEast"] = "steam_button_b+outline+color",

        // Special Button Bindings
        ["buttonSelect"] = "steam_button_back_icon+outline",
        ["buttonStart"] = "steam_button_start_icon+outline"
      }
    ) { }
  }
}