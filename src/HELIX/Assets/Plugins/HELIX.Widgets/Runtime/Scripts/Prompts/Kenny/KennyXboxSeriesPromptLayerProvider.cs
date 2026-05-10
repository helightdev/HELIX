using System.Collections.Generic;

namespace HELIX.Widgets.Prompts.Kenny {
  public class KennyXboxSeriesPromptLayerProvider : KennyResourcePromptLayerProvider {
    public KennyXboxSeriesPromptLayerProvider() : base(
      "helix/input/XboxSeries/SVGS",
      new Dictionary<string, string>() {
        // Stick Bindings
        ["leftStick"] = "xbox_stick_l",
        ["leftStick/up"] = "xbox_stick_l_up",
        ["leftStick/down"] = "xbox_stick_l_down",
        ["leftStick/left"] = "xbox_stick_l_left",
        ["leftStick/right"] = "xbox_stick_l_right",
        ["leftStick/vertical"] = "xbox_stick_l_vertical",
        ["leftStick/horizontal"] = "xbox_stick_l_horizontal",
        ["leftStickPress"] = "xbox_stick_l_press+outline",

        ["rightStick"] = "xbox_stick_r",
        ["rightStick/up"] = "xbox_stick_r_up",
        ["rightStick/down"] = "xbox_stick_r_down",
        ["rightStick/left"] = "xbox_stick_r_left",
        ["rightStick/right"] = "xbox_stick_r_right",
        ["rightStick/vertical"] = "xbox_stick_r_vertical",
        ["rightStick/horizontal"] = "xbox_stick_r_horizontal",
        ["rightStickPress"] = "xbox_stick_r_press+outline",

        // Trigger Bindings
        ["leftShoulder"] = "xbox_lb+outline",
        ["rightShoulder"] = "xbox_rb+outline",
        ["leftTrigger"] = "xbox_lt+outline",
        ["rightTrigger"] = "xbox_rt+outline",

        // D-Pad Bindings
        ["dpad"] = "xbox_dpad",
        ["dpad/up"] = "xbox_dpad_up+outline",
        ["dpad/down"] = "xbox_dpad_down+outline",
        ["dpad/left"] = "xbox_dpad_left+outline",
        ["dpad/right"] = "xbox_dpad_right+outline",
        ["dpad/vertical"] = "xbox_dpad_vertical+outline",
        ["dpad/horizontal"] = "xbox_dpad_horizontal+outline",

        // Button Bindings
        ["buttonNorth"] = "xbox_button_y+outline+color",
        ["buttonSouth"] = "xbox_button_a+outline+color",
        ["buttonWest"] = "xbox_button_x+outline+color",
        ["buttonEast"] = "xbox_button_b+outline+color",

        // Special Button Bindings
        ["buttonSelect"] = "xbox_button_back+outline+icon",
        ["buttonStart"] = "xbox_button_start+outline+icon"
      }
    ) { }
  }
}