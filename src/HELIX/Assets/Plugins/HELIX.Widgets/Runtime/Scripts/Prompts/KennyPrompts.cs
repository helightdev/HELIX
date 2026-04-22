using System.Collections.Generic;

namespace HELIX.Widgets.Prompts {
  public class KennyKeyboardMousePromptProvider : KennyResourcePromptProvider {
    public KennyKeyboardMousePromptProvider() : base(
      "helix/input/KeyboardMouse/SVGS",
      new Dictionary<string, string> {
        // Number Bindings
        ["<Keyboard>/0"] = "keyboard_0+outline",
        ["<Keyboard>/1"] = "keyboard_1+outline",
        ["<Keyboard>/2"] = "keyboard_2+outline",
        ["<Keyboard>/3"] = "keyboard_3+outline",
        ["<Keyboard>/4"] = "keyboard_4+outline",
        ["<Keyboard>/5"] = "keyboard_5+outline",
        ["<Keyboard>/6"] = "keyboard_6+outline",
        ["<Keyboard>/7"] = "keyboard_7+outline",
        ["<Keyboard>/8"] = "keyboard_8+outline",
        ["<Keyboard>/9"] = "keyboard_9+outline",

        // Numpad Bindings (reusing the same keys as the number keys)
        ["<Keyboard>/numpad0"] = "keyboard_0+outline",
        ["<Keyboard>/numpad1"] = "keyboard_1+outline",
        ["<Keyboard>/numpad2"] = "keyboard_2+outline",
        ["<Keyboard>/numpad3"] = "keyboard_3+outline",
        ["<Keyboard>/numpad4"] = "keyboard_4+outline",
        ["<Keyboard>/numpad5"] = "keyboard_5+outline",
        ["<Keyboard>/numpad6"] = "keyboard_6+outline",
        ["<Keyboard>/numpad7"] = "keyboard_7+outline",
        ["<Keyboard>/numpad8"] = "keyboard_8+outline",
        ["<Keyboard>/numpad9"] = "keyboard_9+outline",

        // Alphabet Bindings
        ["<Keyboard>/a"] = "keyboard_a+outline",
        ["<Keyboard>/b"] = "keyboard_b+outline",
        ["<Keyboard>/c"] = "keyboard_c+outline",
        ["<Keyboard>/d"] = "keyboard_d+outline",
        ["<Keyboard>/e"] = "keyboard_e+outline",
        ["<Keyboard>/f"] = "keyboard_f+outline",
        ["<Keyboard>/g"] = "keyboard_g+outline",
        ["<Keyboard>/h"] = "keyboard_h+outline",
        ["<Keyboard>/i"] = "keyboard_i+outline",
        ["<Keyboard>/j"] = "keyboard_j+outline",
        ["<Keyboard>/k"] = "keyboard_k+outline",
        ["<Keyboard>/l"] = "keyboard_l+outline",
        ["<Keyboard>/m"] = "keyboard_m+outline",
        ["<Keyboard>/n"] = "keyboard_n+outline",
        ["<Keyboard>/o"] = "keyboard_o+outline",
        ["<Keyboard>/p"] = "keyboard_p+outline",
        ["<Keyboard>/q"] = "keyboard_q+outline",
        ["<Keyboard>/r"] = "keyboard_r+outline",
        ["<Keyboard>/s"] = "keyboard_s+outline",
        ["<Keyboard>/t"] = "keyboard_t+outline",
        ["<Keyboard>/u"] = "keyboard_u+outline",
        ["<Keyboard>/v"] = "keyboard_v+outline",
        ["<Keyboard>/w"] = "keyboard_w+outline",
        ["<Keyboard>/x"] = "keyboard_x+outline",
        ["<Keyboard>/y"] = "keyboard_y+outline",
        ["<Keyboard>/z"] = "keyboard_z+outline",

        // Arrow Bindings
        ["<Keyboard>/upArrow"] = "keyboard_arrow_up+outline",
        ["<Keyboard>/downArrow"] = "keyboard_arrow_down+outline",
        ["<Keyboard>/leftArrow"] = "keyboard_arrow_left+outline",
        ["<Keyboard>/rightArrow"] = "keyboard_arrow_right+outline",

        // Function Key Bindings
        ["<Keyboard>/f1"] = "keyboard_f1+outline",
        ["<Keyboard>/f2"] = "keyboard_f2+outline",
        ["<Keyboard>/f3"] = "keyboard_f3+outline",
        ["<Keyboard>/f4"] = "keyboard_f4+outline",
        ["<Keyboard>/f5"] = "keyboard_f5+outline",
        ["<Keyboard>/f6"] = "keyboard_f6+outline",
        ["<Keyboard>/f7"] = "keyboard_f7+outline",
        ["<Keyboard>/f8"] = "keyboard_f8+outline",
        ["<Keyboard>/f9"] = "keyboard_f9+outline",
        ["<Keyboard>/f10"] = "keyboard_f10+outline",
        ["<Keyboard>/f11"] = "keyboard_f11+outline",
        ["<Keyboard>/f12"] = "keyboard_f12+outline",

        // Special Characters Bindings
        ["<Keyboard>/space"] = "keyboard_space+outline+icon",
        ["<Keyboard>/enter"] = "keyboard_enter+outline",
        ["<Keyboard>/backspace"] = "keyboard_backspace+outline+alternative+icon",
        ["<Keyboard>/tab"] = "keyboard_tab+outline+icon+alternative",
        ["<Keyboard>/shift"] = "keyboard_shift+outline+icon",
        ["<Keyboard>/leftShift"] = "keyboard_shift+outline+icon",
        ["<Keyboard>/rightShift"] = "keyboard_shift+outline+icon",
        ["<Keyboard>/ctrl"] = "keyboard_ctrl+outline",
        ["<Keyboard>/leftCtrl"] = "keyboard_ctrl+outline",
        ["<Keyboard>/rightCtrl"] = "keyboard_ctrl+outline",
        ["<Keyboard>/alt"] = "keyboard_alt+outline",
        ["<Keyboard>/leftAlt"] = "keyboard_alt+outline",
        ["<Keyboard>/rightAlt"] = "keyboard_alt+outline",
        ["<Keyboard>/escape"] = "keyboard_escape+outline",
        ["<Keyboard>/capsLock"] = "keyboard_capslock+outline+icon",
        ["<Keyboard>/numLock"] = "keyboard_numlock+outline",
        ["<Keyboard>/pageUp"] = "keyboard_page_up+outline",
        ["<Keyboard>/pageDown"] = "keyboard_page_down+outline",
        ["<Keyboard>/home"] = "keyboard_home+outline",
        ["<Keyboard>/end"] = "keyboard_end+outline",
        ["<Keyboard>/insert"] = "keyboard_insert+outline",
        ["<Keyboard>/delete"] = "keyboard_delete+outline",
        ["<Keyboard>/printScreen"] = "keyboard_printscreen+outline",

        // Symbol Bindings
        ["<Keyboard>/minus"] = "keyboard_minus+outline",
        ["<Keyboard>/equals"] = "keyboard_equals+outline",
        ["<Keyboard>/leftBracket"] = "keyboard_bracket_open+outline",
        ["<Keyboard>/rightBracket"] = "keyboard_bracket_close+outline",
        ["<Keyboard>/semicolon"] = "keyboard_semicolon+outline",
        ["<Keyboard>/quote"] = "keyboard_quote+outline",
        ["<Keyboard>/comma"] = "keyboard_comma+outline",
        ["<Keyboard>/period"] = "keyboard_period+outline",
        ["<Keyboard>/slash"] = "keyboard_slash_forward+outline",
        ["<Keyboard>/backslash"] = "keyboard_slash_back+outline",

        // Meta-Bindings
        ["<Keyboard>/anyKey"] = "keyboard_any+outline",

        // Mouse Bindings
        ["<Mouse>/leftButton"] = "mouse_left+outline",
        ["<Mouse>/rightButton"] = "mouse_right+outline",
        ["<Mouse>/middleButton"] = "mouse_scroll+outline",
        ["<Mouse>/delta"] = "mouse_move+outline",
        ["<Mouse>/delta/left"] = "mouse_left+outline",
        ["<Mouse>/delta/right"] = "mouse_right+outline",
        ["<Mouse>/delta/horizontal"] = "mouse_horizontal+outline",
        ["<Mouse>/delta/vertical"] = "mouse_vertical+outline",
        ["<Mouse>/position"] = "mouse+outline",
        ["<Mouse>/scroll"] = "mouse_scroll_vertical+outline"
      }
    ) { }
  }

  public class KennyPlaystationSeriesPromptProvider : KennyResourcePromptProvider {
    public KennyPlaystationSeriesPromptProvider() : base(
      "helix/input/PlayStationSeries/SVGS",
      new Dictionary<string, string>() {
        // Stick Bindings
        ["<Gamepad>/leftStick"] = "playstation_stick_l",
        ["<Gamepad>/leftStick/up"] = "playstation_stick_l_up",
        ["<Gamepad>/leftStick/down"] = "playstation_stick_l_down",
        ["<Gamepad>/leftStick/left"] = "playstation_stick_l_left",
        ["<Gamepad>/leftStick/right"] = "playstation_stick_l_right",
        ["<Gamepad>/leftStick/vertical"] = "playstation_stick_l_vertical",
        ["<Gamepad>/leftStick/horizontal"] = "playstation_stick_l_horizontal",
        ["<Gamepad>/leftStickPress"] = "playstation_stick_l_press+outline",

        ["<Gamepad>/rightStick"] = "playstation_stick_r",
        ["<Gamepad>/rightStick/up"] = "playstation_stick_r_up",
        ["<Gamepad>/rightStick/down"] = "playstation_stick_r_down",
        ["<Gamepad>/rightStick/left"] = "playstation_stick_r_left",
        ["<Gamepad>/rightStick/right"] = "playstation_stick_r_right",
        ["<Gamepad>/rightStick/vertical"] = "playstation_stick_r_vertical",
        ["<Gamepad>/rightStick/horizontal"] = "playstation_stick_r_horizontal",
        ["<Gamepad>/rightStickPress"] = "playstation_stick_r_press+outline",

        // Trigger Bindings
        ["<Gamepad>/leftShoulder"] = "playstation_trigger_l1+outline+alternative",
        ["<Gamepad>/rightShoulder"] = "playstation_trigger_r1+outline+alternative",
        ["<Gamepad>/leftTrigger"] = "playstation_trigger_l2+outline+alternative",
        ["<Gamepad>/rightTrigger"] = "playstation_trigger_r2+outline+alternative",

        // D-Pad Bindings
        ["<Gamepad>/dpad"] = "playstation_dpad",
        ["<Gamepad>/dpad/up"] = "playstation_dpad_up+outline",
        ["<Gamepad>/dpad/down"] = "playstation_dpad_down+outline",
        ["<Gamepad>/dpad/left"] = "playstation_dpad_left+outline",
        ["<Gamepad>/dpad/right"] = "playstation_dpad_right+outline",
        ["<Gamepad>/dpad/vertical"] = "playstation_dpad_vertical+outline",
        ["<Gamepad>/dpad/horizontal"] = "playstation_dpad_horizontal+outline",

        // Button Bindings
        ["<Gamepad>/buttonNorth"] = "playstation_button_triangle+outline+color",
        ["<Gamepad>/buttonSouth"] = "playstation_button_cross+outline+color",
        ["<Gamepad>/buttonWest"] = "playstation_button_square+outline+color",
        ["<Gamepad>/buttonEast"] = "playstation_button_circle+outline+color",

        // Special Button Bindings
        ["<Gamepad>/buttonSelect"] = "playstation5_button_create+outline+alternative",
        ["<Gamepad>/buttonStart"] = "playstation5_button_options+outline+alternative",
        ["<Gamepad>/touchpadButton"] = "playstation5_touchpad_press+outline"
      }
    ) { }
  }

  public class KennyXboxSeriesPromptProvider : KennyResourcePromptProvider {
    public KennyXboxSeriesPromptProvider() : base(
      "helix/input/XboxSeries/SVGS",
      new Dictionary<string, string>() {
        // Stick Bindings
        ["<Gamepad>/leftStick"] = "xbox_stick_l",
        ["<Gamepad>/leftStick/up"] = "xbox_stick_l_up",
        ["<Gamepad>/leftStick/down"] = "xbox_stick_l_down",
        ["<Gamepad>/leftStick/left"] = "xbox_stick_l_left",
        ["<Gamepad>/leftStick/right"] = "xbox_stick_l_right",
        ["<Gamepad>/leftStick/vertical"] = "xbox_stick_l_vertical",
        ["<Gamepad>/leftStick/horizontal"] = "xbox_stick_l_horizontal",
        ["<Gamepad>/leftStickPress"] = "xbox_stick_l_press+outline",

        ["<Gamepad>/rightStick"] = "xbox_stick_r",
        ["<Gamepad>/rightStick/up"] = "xbox_stick_r_up",
        ["<Gamepad>/rightStick/down"] = "xbox_stick_r_down",
        ["<Gamepad>/rightStick/left"] = "xbox_stick_r_left",
        ["<Gamepad>/rightStick/right"] = "xbox_stick_r_right",
        ["<Gamepad>/rightStick/vertical"] = "xbox_stick_r_vertical",
        ["<Gamepad>/rightStick/horizontal"] = "xbox_stick_r_horizontal",
        ["<Gamepad>/rightStickPress"] = "xbox_stick_r_press+outline",

        // Trigger Bindings
        ["<Gamepad>/leftShoulder"] = "xbox_lb+outline",
        ["<Gamepad>/rightShoulder"] = "xbox_rb+outline",
        ["<Gamepad>/leftTrigger"] = "xbox_lt+outline",
        ["<Gamepad>/rightTrigger"] = "xbox_rt+outline",

        // D-Pad Bindings
        ["<Gamepad>/dpad"] = "xbox_dpad",
        ["<Gamepad>/dpad/up"] = "xbox_dpad_up+outline",
        ["<Gamepad>/dpad/down"] = "xbox_dpad_down+outline",
        ["<Gamepad>/dpad/left"] = "xbox_dpad_left+outline",
        ["<Gamepad>/dpad/right"] = "xbox_dpad_right+outline",
        ["<Gamepad>/dpad/vertical"] = "xbox_dpad_vertical+outline",
        ["<Gamepad>/dpad/horizontal"] = "xbox_dpad_horizontal+outline",

        // Button Bindings
        ["<Gamepad>/buttonNorth"] = "xbox_button_y+outline+color",
        ["<Gamepad>/buttonSouth"] = "xbox_button_a+outline+color",
        ["<Gamepad>/buttonWest"] = "xbox_button_x+outline+color",
        ["<Gamepad>/buttonEast"] = "xbox_button_b+outline+color",

        // Special Button Bindings
        ["<Gamepad>/buttonSelect"] = "xbox_button_back+outline+icon",
        ["<Gamepad>/buttonStart"] = "xbox_button_start+outline+icon"
      }
    ) { }
  }

  public class KennySteamControllerPromptProvider : KennyResourcePromptProvider {
    public KennySteamControllerPromptProvider() : base(
      "helix/input/SteamController/SVGS",
      new Dictionary<string, string>() {
        // Stick Bindings
        ["<Gamepad>/leftStick"] = "steam_stick",
        ["<Gamepad>/leftStick/up"] = "steam_stick_up",
        ["<Gamepad>/leftStick/down"] = "steam_stick_down",
        ["<Gamepad>/leftStick/left"] = "steam_stick_left",
        ["<Gamepad>/leftStick/right"] = "steam_stick_right",
        ["<Gamepad>/leftStick/vertical"] = "steam_stick_vertical",
        ["<Gamepad>/leftStick/horizontal"] = "steam_stick_horizontal",
        ["<Gamepad>/leftStickPress"] = "steam_stick_l_press",

        ["<Gamepad>/rightStick"] = "steam_pad",
        ["<Gamepad>/rightStick/up"] = "steam_pad_up",
        ["<Gamepad>/rightStick/down"] = "steam_pad_down",
        ["<Gamepad>/rightStick/left"] = "steam_pad_left",
        ["<Gamepad>/rightStick/right"] = "steam_pad_right",
        ["<Gamepad>/rightStickPress"] = "steam_pad_center",

        // Trigger Bindings
        ["<Gamepad>/leftShoulder"] = "steam_lb",
        ["<Gamepad>/rightShoulder"] = "steam_rb",
        ["<Gamepad>/leftTrigger"] = "steam_lt",
        ["<Gamepad>/rightTrigger"] = "steam_rt",

        // D-Pad Bindings
        ["<Gamepad>/dpad"] = "steam_dpad",
        ["<Gamepad>/dpad/up"] = "steam_dpad_up+outline",
        ["<Gamepad>/dpad/down"] = "steam_dpad_down+outline",
        ["<Gamepad>/dpad/left"] = "steam_dpad_left+outline",
        ["<Gamepad>/dpad/right"] = "steam_dpad_right+outline",
        ["<Gamepad>/dpad/vertical"] = "steam_dpad_vertical+outline",
        ["<Gamepad>/dpad/horizontal"] = "steam_dpad_horizontal+outline",

        // Button Bindings
        ["<Gamepad>/buttonNorth"] = "steam_button_y+outline+color",
        ["<Gamepad>/buttonSouth"] = "steam_button_a+outline+color",
        ["<Gamepad>/buttonWest"] = "steam_button_x+outline+color",
        ["<Gamepad>/buttonEast"] = "steam_button_b+outline+color",

        // Special Button Bindings
        ["<Gamepad>/buttonSelect"] = "steam_button_back_icon+outline",
        ["<Gamepad>/buttonStart"] = "steam_button_start_icon+outline"
      }
    ) { }
  }

  public class KennySteamDeckPromptProvider : KennyResourcePromptProvider {
    public KennySteamDeckPromptProvider() : base(
      "helix/input/SteamDeck/SVGS",
      new Dictionary<string, string>() {
        // Stick Bindings
        ["<Gamepad>/leftStick"] = "steamdeck_stick_l",
        ["<Gamepad>/leftStick/up"] = "steamdeck_stick_l_up",
        ["<Gamepad>/leftStick/down"] = "steamdeck_stick_l_down",
        ["<Gamepad>/leftStick/left"] = "steamdeck_stick_l_left",
        ["<Gamepad>/leftStick/right"] = "steamdeck_stick_l_right",
        ["<Gamepad>/leftStick/vertical"] = "steamdeck_stick_l_vertical",
        ["<Gamepad>/leftStick/horizontal"] = "steamdeck_stick_l_horizontal",
        ["<Gamepad>/leftStickPress"] = "steamdeck_stick_l_press",

        ["<Gamepad>/rightStick"] = "steamdeck_stick_r",
        ["<Gamepad>/rightStick/up"] = "steamdeck_stick_r_up",
        ["<Gamepad>/rightStick/down"] = "steamdeck_stick_r_down",
        ["<Gamepad>/rightStick/left"] = "steamdeck_stick_r_left",
        ["<Gamepad>/rightStick/right"] = "steamdeck_stick_r_right",
        ["<Gamepad>/rightStick/vertical"] = "steamdeck_stick_r_vertical",
        ["<Gamepad>/rightStick/horizontal"] = "steamdeck_stick_r_horizontal",
        ["<Gamepad>/rightStickPress"] = "steamdeck_stick_r_press",

        // Trigger Bindings
        ["<Gamepad>/leftShoulder"] = "steamdeck_button_l1",
        ["<Gamepad>/rightShoulder"] = "steamdeck_button_r1",
        ["<Gamepad>/leftTrigger"] = "steamdeck_button_l2",
        ["<Gamepad>/rightTrigger"] = "steamdeck_button_r2",

        // D-Pad Bindings
        ["<Gamepad>/dpad"] = "steamdeck_dpad",
        ["<Gamepad>/dpad/up"] = "steamdeck_dpad_up+outline",
        ["<Gamepad>/dpad/down"] = "steamdeck_dpad_down+outline",
        ["<Gamepad>/dpad/left"] = "steamdeck_dpad_left+outline",
        ["<Gamepad>/dpad/right"] = "steamdeck_dpad_right+outline",
        ["<Gamepad>/dpad/vertical"] = "steamdeck_dpad_vertical+outline",
        ["<Gamepad>/dpad/horizontal"] = "steamdeck_dpad_horizontal+outline",

        // Button Bindings
        ["<Gamepad>/buttonNorth"] = "steamdeck_button_y+outline",
        ["<Gamepad>/buttonSouth"] = "steamdeck_button_a+outline",
        ["<Gamepad>/buttonWest"] = "steamdeck_button_x+outline",
        ["<Gamepad>/buttonEast"] = "steamdeck_button_b+outline",

        // Special Button Bindings
        ["<Gamepad>/buttonSelect"] = "steamdeck_button_view+outline",
        ["<Gamepad>/buttonStart"] = "steamdeck_button_options+outline"
      }
    ) { }
  }

  public class KennyNintendoSwitch2PromptProvider : KennyResourcePromptProvider {

    public KennyNintendoSwitch2PromptProvider() : base(
      "helix/input/NintendoSwitch2/SVGS",
      new Dictionary<string, string>() {
        // Stick Bindings
        ["<Gamepad>/leftStick"] = "switch_stick_l",
        ["<Gamepad>/leftStick/up"] = "switch_stick_l_up",
        ["<Gamepad>/leftStick/down"] = "switch_stick_l_down",
        ["<Gamepad>/leftStick/left"] = "switch_stick_l_left",
        ["<Gamepad>/leftStick/right"] = "switch_stick_l_right",
        ["<Gamepad>/leftStick/vertical"] = "switch_stick_l_vertical",
        ["<Gamepad>/leftStick/horizontal"] = "switch_stick_l_horizontal",
        ["<Gamepad>/leftStickPress"] = "switch_stick_l_press",

        ["<Gamepad>/rightStick"] = "switch_stick_r",
        ["<Gamepad>/rightStick/up"] = "switch_stick_r_up",
        ["<Gamepad>/rightStick/down"] = "switch_stick_r_down",
        ["<Gamepad>/rightStick/left"] = "switch_stick_r_left",
        ["<Gamepad>/rightStick/right"] = "switch_stick_r_right",
        ["<Gamepad>/rightStick/vertical"] = "switch_stick_r_vertical",
        ["<Gamepad>/rightStick/horizontal"] = "switch_stick_r_horizontal",
        ["<Gamepad>/rightStickPress"] = "switch_stick_r_press",

        // Trigger Bindings
        ["<Gamepad>/leftShoulder"] = "switch_button_l+outline",
        ["<Gamepad>/rightShoulder"] = "switch_button_r+outline",
        ["<Gamepad>/leftTrigger"] = "switch_button_zl+outline",
        ["<Gamepad>/rightTrigger"] = "switch_button_zr+outline",

        // D-Pad Bindings
        ["<Gamepad>/dpad"] = "switch_dpad",
        ["<Gamepad>/dpad/up"] = "switch_dpad_up+outline",
        ["<Gamepad>/dpad/down"] = "switch_dpad_down+outline",
        ["<Gamepad>/dpad/left"] = "switch_dpad_left+outline",
        ["<Gamepad>/dpad/right"] = "switch_dpad_right+outline",
        ["<Gamepad>/dpad/vertical"] = "switch_dpad_vertical+outline",
        ["<Gamepad>/dpad/horizontal"] = "switch_dpad_horizontal+outline",

        // Button Bindings
        ["<Gamepad>/buttonNorth"] = "switch_button_y+outline",
        ["<Gamepad>/buttonSouth"] = "switch_button_a+outline",
        ["<Gamepad>/buttonWest"] = "switch_button_x+outline",
        ["<Gamepad>/buttonEast"] = "switch_button_b+outline",

        // Special Button Bindings
        ["<Gamepad>/buttonSelect"] = "switch_button_minus+outline",
        ["<Gamepad>/buttonStart"] = "switch_button_plus+outline"
      }
    ) { }

  }
}