using System.Collections.Generic;

namespace HELIX.Widgets.Prompts.Kenny {
  public class KennySteamDeckPromptLayerProvider : KennyResourcePromptLayerProvider {
    public KennySteamDeckPromptLayerProvider() : base(
      "helix/input/SteamDeck/SVGS",
      new Dictionary<string, string>() {
        // Stick Bindings
        ["leftStick"] = "steamdeck_stick_l",
        ["leftStick/up"] = "steamdeck_stick_l_up",
        ["leftStick/down"] = "steamdeck_stick_l_down",
        ["leftStick/left"] = "steamdeck_stick_l_left",
        ["leftStick/right"] = "steamdeck_stick_l_right",
        ["leftStick/vertical"] = "steamdeck_stick_l_vertical",
        ["leftStick/horizontal"] = "steamdeck_stick_l_horizontal",
        ["leftStickPress"] = "steamdeck_stick_l_press",

        ["rightStick"] = "steamdeck_stick_r",
        ["rightStick/up"] = "steamdeck_stick_r_up",
        ["rightStick/down"] = "steamdeck_stick_r_down",
        ["rightStick/left"] = "steamdeck_stick_r_left",
        ["rightStick/right"] = "steamdeck_stick_r_right",
        ["rightStick/vertical"] = "steamdeck_stick_r_vertical",
        ["rightStick/horizontal"] = "steamdeck_stick_r_horizontal",
        ["rightStickPress"] = "steamdeck_stick_r_press",

        // Trigger Bindings
        ["leftShoulder"] = "steamdeck_button_l1",
        ["rightShoulder"] = "steamdeck_button_r1",
        ["leftTrigger"] = "steamdeck_button_l2",
        ["rightTrigger"] = "steamdeck_button_r2",

        // D-Pad Bindings
        ["dpad"] = "steamdeck_dpad",
        ["dpad/up"] = "steamdeck_dpad_up+outline",
        ["dpad/down"] = "steamdeck_dpad_down+outline",
        ["dpad/left"] = "steamdeck_dpad_left+outline",
        ["dpad/right"] = "steamdeck_dpad_right+outline",
        ["dpad/vertical"] = "steamdeck_dpad_vertical+outline",
        ["dpad/horizontal"] = "steamdeck_dpad_horizontal+outline",

        // Button Bindings
        ["buttonNorth"] = "steamdeck_button_y+outline",
        ["buttonSouth"] = "steamdeck_button_a+outline",
        ["buttonWest"] = "steamdeck_button_x+outline",
        ["buttonEast"] = "steamdeck_button_b+outline",

        // Special Button Bindings
        ["buttonSelect"] = "steamdeck_button_view+outline",
        ["buttonStart"] = "steamdeck_button_options+outline"
      }
    ) { }
  }
}