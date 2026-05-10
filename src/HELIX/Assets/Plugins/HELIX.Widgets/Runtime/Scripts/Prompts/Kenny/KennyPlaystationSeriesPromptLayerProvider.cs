using System.Collections.Generic;

namespace HELIX.Widgets.Prompts.Kenny {
  public class KennyPlaystationSeriesPromptLayerProvider : KennyResourcePromptLayerProvider {
    public KennyPlaystationSeriesPromptLayerProvider() : base(
      "helix/input/PlayStationSeries/SVGS",
      new Dictionary<string, string>() {
        // Stick Bindings
        ["leftStick"] = "playstation_stick_l",
        ["leftStick/up"] = "playstation_stick_l_up",
        ["leftStick/down"] = "playstation_stick_l_down",
        ["leftStick/left"] = "playstation_stick_l_left",
        ["leftStick/right"] = "playstation_stick_l_right",
        ["leftStick/vertical"] = "playstation_stick_l_vertical",
        ["leftStick/horizontal"] = "playstation_stick_l_horizontal",
        ["leftStickPress"] = "playstation_stick_l_press+outline",

        ["rightStick"] = "playstation_stick_r",
        ["rightStick/up"] = "playstation_stick_r_up",
        ["rightStick/down"] = "playstation_stick_r_down",
        ["rightStick/left"] = "playstation_stick_r_left",
        ["rightStick/right"] = "playstation_stick_r_right",
        ["rightStick/vertical"] = "playstation_stick_r_vertical",
        ["rightStick/horizontal"] = "playstation_stick_r_horizontal",
        ["rightStickPress"] = "playstation_stick_r_press+outline",

        // Trigger Bindings
        ["leftShoulder"] = "playstation_trigger_l1+outline+alternative",
        ["rightShoulder"] = "playstation_trigger_r1+outline+alternative",
        ["leftTrigger"] = "playstation_trigger_l2+outline+alternative",
        ["rightTrigger"] = "playstation_trigger_r2+outline+alternative",

        // D-Pad Bindings
        ["dpad"] = "playstation_dpad",
        ["dpad/up"] = "playstation_dpad_up+outline",
        ["dpad/down"] = "playstation_dpad_down+outline",
        ["dpad/left"] = "playstation_dpad_left+outline",
        ["dpad/right"] = "playstation_dpad_right+outline",
        ["dpad/vertical"] = "playstation_dpad_vertical+outline",
        ["dpad/horizontal"] = "playstation_dpad_horizontal+outline",

        // Button Bindings
        ["buttonNorth"] = "playstation_button_triangle+outline+color",
        ["buttonSouth"] = "playstation_button_cross+outline+color",
        ["buttonWest"] = "playstation_button_square+outline+color",
        ["buttonEast"] = "playstation_button_circle+outline+color",

        // Special Button Bindings
        ["buttonSelect"] = "playstation5_button_create+outline+alternative",
        ["buttonStart"] = "playstation5_button_options+outline+alternative",
        ["touchpadButton"] = "playstation5_touchpad_press+outline"
      }
    ) { }
  }
}