using System.Collections.Generic;

namespace HELIX.Widgets.Prompts.Kenny {
  public class KennyNintendoSwitch2PromptLayerProvider : KennyResourcePromptLayerProvider {

    public KennyNintendoSwitch2PromptLayerProvider() : base(
      "helix/input/NintendoSwitch2/SVGS",
      new Dictionary<string, string>() {
        // Stick Bindings
        ["leftStick"] = "switch_stick_l",
        ["leftStick/up"] = "switch_stick_l_up",
        ["leftStick/down"] = "switch_stick_l_down",
        ["leftStick/left"] = "switch_stick_l_left",
        ["leftStick/right"] = "switch_stick_l_right",
        ["leftStick/vertical"] = "switch_stick_l_vertical",
        ["leftStick/horizontal"] = "switch_stick_l_horizontal",
        ["leftStickPress"] = "switch_stick_l_press",

        ["rightStick"] = "switch_stick_r",
        ["rightStick/up"] = "switch_stick_r_up",
        ["rightStick/down"] = "switch_stick_r_down",
        ["rightStick/left"] = "switch_stick_r_left",
        ["rightStick/right"] = "switch_stick_r_right",
        ["rightStick/vertical"] = "switch_stick_r_vertical",
        ["rightStick/horizontal"] = "switch_stick_r_horizontal",
        ["rightStickPress"] = "switch_stick_r_press",

        // Trigger Bindings
        ["leftShoulder"] = "switch_button_l+outline",
        ["rightShoulder"] = "switch_button_r+outline",
        ["leftTrigger"] = "switch_button_zl+outline",
        ["rightTrigger"] = "switch_button_zr+outline",

        // D-Pad Bindings
        ["dpad"] = "switch_dpad",
        ["dpad/up"] = "switch_dpad_up+outline",
        ["dpad/down"] = "switch_dpad_down+outline",
        ["dpad/left"] = "switch_dpad_left+outline",
        ["dpad/right"] = "switch_dpad_right+outline",
        ["dpad/vertical"] = "switch_dpad_vertical+outline",
        ["dpad/horizontal"] = "switch_dpad_horizontal+outline",

        // Button Bindings
        ["buttonNorth"] = "switch_button_y+outline",
        ["buttonSouth"] = "switch_button_a+outline",
        ["buttonWest"] = "switch_button_x+outline",
        ["buttonEast"] = "switch_button_b+outline",

        // Special Button Bindings
        ["buttonSelect"] = "switch_button_minus+outline",
        ["buttonStart"] = "switch_button_plus+outline"
      }
    ) {

    }

  }
}