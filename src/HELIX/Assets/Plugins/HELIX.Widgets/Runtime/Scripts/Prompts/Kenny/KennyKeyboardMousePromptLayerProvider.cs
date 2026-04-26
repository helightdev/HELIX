using System.Collections.Generic;

namespace HELIX.Widgets.Prompts.Kenny {
  public class KennyKeyboardMousePromptLayerProvider : KennyResourcePromptLayerProvider {
    public KennyKeyboardMousePromptLayerProvider() : base(
      "helix/input/KeyboardMouse/SVGS",
      new Dictionary<string, string> {
        // Number Bindings
        ["0"] = "keyboard_0+outline",
        ["1"] = "keyboard_1+outline",
        ["2"] = "keyboard_2+outline",
        ["3"] = "keyboard_3+outline",
        ["4"] = "keyboard_4+outline",
        ["5"] = "keyboard_5+outline",
        ["6"] = "keyboard_6+outline",
        ["7"] = "keyboard_7+outline",
        ["8"] = "keyboard_8+outline",
        ["9"] = "keyboard_9+outline",

        // Numpad Bindings (reusing the same keys as the number keys)
        ["numpad0"] = "keyboard_0+outline",
        ["numpad1"] = "keyboard_1+outline",
        ["numpad2"] = "keyboard_2+outline",
        ["numpad3"] = "keyboard_3+outline",
        ["numpad4"] = "keyboard_4+outline",
        ["numpad5"] = "keyboard_5+outline",
        ["numpad6"] = "keyboard_6+outline",
        ["numpad7"] = "keyboard_7+outline",
        ["numpad8"] = "keyboard_8+outline",
        ["numpad9"] = "keyboard_9+outline",

        // Alphabet Bindings
        ["a"] = "keyboard_a+outline",
        ["b"] = "keyboard_b+outline",
        ["c"] = "keyboard_c+outline",
        ["d"] = "keyboard_d+outline",
        ["e"] = "keyboard_e+outline",
        ["f"] = "keyboard_f+outline",
        ["g"] = "keyboard_g+outline",
        ["h"] = "keyboard_h+outline",
        ["i"] = "keyboard_i+outline",
        ["j"] = "keyboard_j+outline",
        ["k"] = "keyboard_k+outline",
        ["l"] = "keyboard_l+outline",
        ["m"] = "keyboard_m+outline",
        ["n"] = "keyboard_n+outline",
        ["o"] = "keyboard_o+outline",
        ["p"] = "keyboard_p+outline",
        ["q"] = "keyboard_q+outline",
        ["r"] = "keyboard_r+outline",
        ["s"] = "keyboard_s+outline",
        ["t"] = "keyboard_t+outline",
        ["u"] = "keyboard_u+outline",
        ["v"] = "keyboard_v+outline",
        ["w"] = "keyboard_w+outline",
        ["x"] = "keyboard_x+outline",
        ["y"] = "keyboard_y+outline",
        ["z"] = "keyboard_z+outline",

        // Arrow Bindings
        ["upArrow"] = "keyboard_arrow_up+outline",
        ["downArrow"] = "keyboard_arrow_down+outline",
        ["leftArrow"] = "keyboard_arrow_left+outline",
        ["rightArrow"] = "keyboard_arrow_right+outline",

        // Function Key Bindings
        ["f1"] = "keyboard_f1+outline",
        ["f2"] = "keyboard_f2+outline",
        ["f3"] = "keyboard_f3+outline",
        ["f4"] = "keyboard_f4+outline",
        ["f5"] = "keyboard_f5+outline",
        ["f6"] = "keyboard_f6+outline",
        ["f7"] = "keyboard_f7+outline",
        ["f8"] = "keyboard_f8+outline",
        ["f9"] = "keyboard_f9+outline",
        ["f10"] = "keyboard_f10+outline",
        ["f11"] = "keyboard_f11+outline",
        ["f12"] = "keyboard_f12+outline",

        // Special Characters Bindings
        ["space"] = "keyboard_space+outline+icon",
        ["enter"] = "keyboard_enter+outline",
        ["backspace"] = "keyboard_backspace+outline+alternative+icon",
        ["tab"] = "keyboard_tab+outline+icon+alternative",
        ["shift"] = "keyboard_shift+outline+icon",
        ["leftShift"] = "keyboard_shift+outline+icon",
        ["rightShift"] = "keyboard_shift+outline+icon",
        ["ctrl"] = "keyboard_ctrl+outline",
        ["leftCtrl"] = "keyboard_ctrl+outline",
        ["rightCtrl"] = "keyboard_ctrl+outline",
        ["alt"] = "keyboard_alt+outline",
        ["leftAlt"] = "keyboard_alt+outline",
        ["rightAlt"] = "keyboard_alt+outline",
        ["escape"] = "keyboard_escape+outline",
        ["capsLock"] = "keyboard_capslock+outline+icon",
        ["numLock"] = "keyboard_numlock+outline",
        ["pageUp"] = "keyboard_page_up+outline",
        ["pageDown"] = "keyboard_page_down+outline",
        ["home"] = "keyboard_home+outline",
        ["end"] = "keyboard_end+outline",
        ["insert"] = "keyboard_insert+outline",
        ["delete"] = "keyboard_delete+outline",
        ["printScreen"] = "keyboard_printscreen+outline",

        // Symbol Bindings
        ["minus"] = "keyboard_minus+outline",
        ["equals"] = "keyboard_equals+outline",
        ["leftBracket"] = "keyboard_bracket_open+outline",
        ["rightBracket"] = "keyboard_bracket_close+outline",
        ["semicolon"] = "keyboard_semicolon+outline",
        ["quote"] = "keyboard_quote+outline",
        ["comma"] = "keyboard_comma+outline",
        ["period"] = "keyboard_period+outline",
        ["slash"] = "keyboard_slash_forward+outline",
        ["backslash"] = "keyboard_slash_back+outline",

        // Meta-Bindings
        ["anyKey"] = "keyboard_any+outline",

        // Mouse Bindings
        ["leftButton"] = "mouse_left+outline",
        ["rightButton"] = "mouse_right+outline",
        ["middleButton"] = "mouse_scroll+outline",
        ["delta"] = "mouse_move+outline",
        ["delta/left"] = "mouse_left+outline",
        ["delta/right"] = "mouse_right+outline",
        ["delta/horizontal"] = "mouse_horizontal+outline",
        ["delta/vertical"] = "mouse_vertical+outline",
        ["position"] = "mouse+outline",
        ["scroll"] = "mouse_scroll_vertical+outline"
      }
    ) {

    }
  }
}