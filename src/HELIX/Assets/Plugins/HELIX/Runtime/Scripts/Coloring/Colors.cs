using System;
using System.Collections.Generic;
using System.Linq;
using HELIX.Coloring.Material;
using UnityEngine;

namespace HELIX.Coloring {
  public static class Colors {
    public static readonly RadixSwatch Gray = new(
      "gray",
      new[] {
        "#fcfcfc", "#f9f9f9", "#f0f0f0", "#e8e8e8", "#e0e0e0", "#d9d9d9", "#cecece", "#bbbbbb", "#8d8d8d",
        "#838383", "#646464", "#202020"
      },
      new[] {
        "#111111", "#191919", "#222222", "#2a2a2a", "#313131", "#3a3a3a", "#484848", "#606060", "#6e6e6e",
        "#7b7b7b", "#b4b4b4", "#eeeeee"
      }
    );

    public static readonly RadixSwatch Mauve = new(
      "mauve",
      new[] {
        "#fdfcfd", "#faf9fb", "#f2eff3", "#eae7ec", "#e3dfe6", "#dbd8e0", "#d0cdd7", "#bcbac7", "#8e8c99",
        "#84828e", "#65636d", "#211f26"
      },
      new[] {
        "#121113", "#1a191b", "#232225", "#2b292d", "#323035", "#3c393f", "#49474e", "#625f69", "#6f6d78",
        "#7c7a85", "#b5b2bc", "#eeeef0"
      }
    );

    public static readonly RadixSwatch Slate = new(
      "slate",
      new[] {
        "#fcfcfd", "#f9f9fb", "#f0f0f3", "#e8e8ec", "#e0e1e6", "#d9d9e0", "#cdced6", "#b9bbc6", "#8b8d98",
        "#80838d", "#60646c", "#1c2024"
      },
      new[] {
        "#111113", "#18191b", "#212225", "#272a2d", "#2e3135", "#363a3f", "#43484e", "#5a6169", "#696e77",
        "#777b84", "#b0b4ba", "#edeef0"
      }
    );

    public static readonly RadixSwatch Sage = new(
      "sage",
      new[] {
        "#fbfdfc", "#f7f9f8", "#eef1f0", "#e6e9e8", "#dfe2e0", "#d7dad9", "#cbcfcd", "#b8bcba", "#868e8b",
        "#7c8481", "#5f6563", "#1a211e"
      },
      new[] {
        "#101211", "#171918", "#202221", "#272a29", "#2e3130", "#373b39", "#444947", "#5b625f", "#63706b",
        "#717d79", "#adb5b2", "#eceeed"
      }
    );

    public static readonly RadixSwatch Olive = new(
      "olive",
      new[] {
        "#fcfdfc", "#f8faf8", "#eff1ef", "#e7e9e7", "#dfe2df", "#d7dad7", "#cccfcc", "#b9bcb8", "#898e87",
        "#7f847d", "#60655f", "#1d211c"
      },
      new[] {
        "#111210", "#181917", "#212220", "#282a27", "#2f312e", "#383a36", "#454843", "#5c625b", "#687066",
        "#767d74", "#afb5ad", "#eceeec"
      }
    );

    public static readonly RadixSwatch Sand = new(
      "sand",
      new[] {
        "#fdfdfc", "#f9f9f8", "#f1f0ef", "#e9e8e6", "#e2e1de", "#dad9d6", "#cfceca", "#bcbbb5", "#8d8d86",
        "#82827c", "#63635e", "#21201c"
      },
      new[] {
        "#111110", "#191918", "#222221", "#2a2a28", "#31312e", "#3b3a37", "#494844", "#62605b", "#6f6d66",
        "#7c7b74", "#b5b3ad", "#eeeeec"
      }
    );

    public static readonly RadixSwatch Tomato = new(
      "tomato",
      new[] {
        "#fffcfc", "#fff8f7", "#feebe7", "#ffdcd3", "#ffcdc2", "#fdbdaf", "#f5a898", "#ec8e7b", "#e54d2e",
        "#dd4425", "#d13415", "#5c271f"
      },
      new[] {
        "#181111", "#1f1513", "#391714", "#4e1511", "#5e1c16", "#6e2920", "#853a2d", "#ac4d39", "#e54d2e",
        "#ec6142", "#ff977d", "#fbd3cb"
      }
    );

    public static readonly RadixSwatch Red = new(
      "red",
      new[] {
        "#fffcfc", "#fff7f7", "#feebec", "#ffdbdc", "#ffcdce", "#fdbdbe", "#f4a9aa", "#eb8e90", "#e5484d",
        "#dc3e42", "#ce2c31", "#641723"
      },
      new[] {
        "#191111", "#201314", "#3b1219", "#500f1c", "#611623", "#72232d", "#8c333a", "#b54548", "#e5484d",
        "#ec5d5e", "#ff9592", "#ffd1d9"
      }
    );

    public static readonly RadixSwatch Ruby = new(
      "ruby",
      new[] {
        "#fffcfd", "#fff7f8", "#feeaed", "#ffdce1", "#ffced6", "#f8bfc8", "#efacb8", "#e592a3", "#e54666",
        "#dc3b5d", "#ca244d", "#64172b"
      },
      new[] {
        "#191113", "#1e1517", "#3a141e", "#4e1325", "#5e1a2e", "#6f2539", "#883447", "#b3445a", "#e54666",
        "#ec5a72", "#ff949d", "#fed2e1"
      }
    );

    public static readonly RadixSwatch Crimson = new(
      "crimson",
      new[] {
        "#fffcfd", "#fef7f9", "#ffe9f0", "#fedce7", "#facedd", "#f3bed1", "#eaacc3", "#e093b2", "#e93d82",
        "#df3478", "#cb1d63", "#621639"
      },
      new[] {
        "#191114", "#201318", "#381525", "#4d122f", "#5c1839", "#6d2545", "#873356", "#b0436e", "#e93d82",
        "#ee518a", "#ff92ad", "#fdd3e8"
      }
    );

    public static readonly RadixSwatch Pink = new(
      "pink",
      new[] {
        "#fffcfe", "#fef7fb", "#fee9f5", "#fbdcef", "#f6cee7", "#efbfdd", "#e7acd0", "#dd93c2", "#d6409f",
        "#cf3897", "#c2298a", "#651249"
      },
      new[] {
        "#191117", "#21121d", "#37172f", "#4b143d", "#591c47", "#692955", "#833869", "#a84885", "#d6409f",
        "#de51a8", "#ff8dcc", "#fdd1ea"
      }
    );

    public static readonly RadixSwatch Plum = new(
      "plum",
      new[] {
        "#fefcff", "#fff8ff", "#fceffc", "#f9e5f9", "#f3d8f3", "#ebc8ed", "#dfb1e1", "#cf91d8", "#ab4aba",
        "#a144af", "#953ea3", "#53195d"
      },
      new[] {
        "#181118", "#201320", "#351a35", "#451d47", "#512454", "#5e3061", "#734079", "#92549c", "#ab4aba",
        "#b658c4", "#e796f3", "#f4d4f4"
      }
    );

    public static readonly RadixSwatch Purple = new(
      "purple",
      new[] {
        "#fefcfe", "#fbf7fe", "#f7edfe", "#f2e2fc", "#ead5f9", "#e0c4f4", "#d1afec", "#be93e4", "#8e4ec6",
        "#8347b9", "#8145b5", "#402060"
      },
      new[] {
        "#18111b", "#1e1523", "#301c3b", "#3d224e", "#48295c", "#54346b", "#664282", "#8457aa", "#8e4ec6",
        "#9a5cd0", "#d19dff", "#ecd9fa"
      }
    );

    public static readonly RadixSwatch Violet = new(
      "violet",
      new[] {
        "#fdfcfe", "#faf8ff", "#f4f0fe", "#ebe4ff", "#e1d9ff", "#d4cafe", "#c2b5f5", "#aa99ec", "#6e56cf",
        "#654dc4", "#6550b9", "#2f265f"
      },
      new[] {
        "#14121f", "#1b1525", "#291f43", "#33255b", "#3c2e69", "#473876", "#56468b", "#6958ad", "#6e56cf",
        "#7d66d9", "#baa7ff", "#e2ddfe"
      }
    );

    public static readonly RadixSwatch Iris = new(
      "iris",
      new[] {
        "#fdfdff", "#f8f8ff", "#f0f1fe", "#e6e7ff", "#dadcff", "#cbcdff", "#b8baf8", "#9b9ef0", "#5b5bd6",
        "#5151cd", "#5753c6", "#272962"
      },
      new[] {
        "#13131e", "#171625", "#202248", "#262a65", "#303374", "#3d3e82", "#4a4a95", "#5958b1", "#5b5bd6",
        "#6e6ade", "#b1a9ff", "#e0dffe"
      }
    );

    public static readonly RadixSwatch Indigo = new(
      "indigo",
      new[] {
        "#fdfdfe", "#f7f9ff", "#edf2fe", "#e1e9ff", "#d2deff", "#c1d0ff", "#abbdf9", "#8da4ef", "#3e63dd",
        "#3358d4", "#3a5bc7", "#1f2d5c"
      },
      new[] {
        "#11131f", "#141726", "#182449", "#1d2e62", "#253974", "#304384", "#3a4f97", "#435db1", "#3e63dd",
        "#5472e4", "#9eb1ff", "#d6e1ff"
      }
    );

    public static readonly RadixSwatch Blue = new(
      "blue",
      new[] {
        "#fbfdff", "#f4faff", "#e6f4fe", "#d5efff", "#c2e5ff", "#acd8fc", "#8ec8f6", "#5eb1ef", "#0090ff",
        "#0588f0", "#0d74ce", "#113264"
      },
      new[] {
        "#0d1520", "#111927", "#0d2847", "#003362", "#004074", "#104d87", "#205d9e", "#2870bd", "#0090ff",
        "#3b9eff", "#70b8ff", "#c2e6ff"
      }
    );

    public static readonly RadixSwatch Cyan = new(
      "cyan",
      new[] {
        "#fafdfe", "#f2fafb", "#def7f9", "#caf1f6", "#b5e9f0", "#9ddde7", "#7dcedc", "#3db9cf", "#00a2c7",
        "#0797b9", "#107d98", "#0d3c48"
      },
      new[] {
        "#0b161a", "#101b20", "#082c36", "#003848", "#004558", "#045468", "#12677e", "#11809c", "#00a2c7",
        "#23afd0", "#4ccce6", "#b6ecf7"
      }
    );

    public static readonly RadixSwatch Teal = new(
      "teal",
      new[] {
        "#fafefd", "#f3fbf9", "#e0f8f3", "#ccf3ea", "#b8eae0", "#a1ded2", "#83cdc1", "#53b9ab", "#12a594",
        "#0d9b8a", "#008573", "#0d3d38"
      },
      new[] {
        "#0d1514", "#111c1b", "#0d2d2a", "#023b37", "#084843", "#145750", "#1c6961", "#207e73", "#12a594",
        "#0eb39e", "#0bd8b6", "#adf0dd"
      }
    );

    public static readonly RadixSwatch Jade = new(
      "jade",
      new[] {
        "#fbfefd", "#f4fbf7", "#e6f7ed", "#d6f1e3", "#c3e9d7", "#acdec8", "#8bceb6", "#56ba9f", "#29a383",
        "#26997b", "#208368", "#1d3b31"
      },
      new[] {
        "#0d1512", "#121c18", "#0f2e22", "#0b3b2c", "#114837", "#1b5745", "#246854", "#2a7e68", "#29a383",
        "#27b08b", "#1fd8a4", "#adf0d4"
      }
    );

    public static readonly RadixSwatch Green = new(
      "green",
      new[] {
        "#fbfefc", "#f4fbf6", "#e6f6eb", "#d6f1df", "#c4e8d1", "#adddc0", "#8eceaa", "#5bb98b", "#30a46c",
        "#299764", "#18794e", "#193b2d"
      },
      new[] {
        "#0e1512", "#121b17", "#132d21", "#113b29", "#174933", "#20573e", "#28684a", "#2f7c57", "#30a46c",
        "#33b074", "#3dd68c", "#b1f1cb"
      }
    );

    public static readonly RadixSwatch Grass = new(
      "grass",
      new[] {
        "#fbfefb", "#f5fbf5", "#e9f6e9", "#daf1db", "#c9e8ca", "#b2ddb5", "#94ce9a", "#65ba74", "#46a758",
        "#3e9b4f", "#2a7e3b", "#203c25"
      },
      new[] {
        "#0e1511", "#141a15", "#1b2a1e", "#1d3a24", "#25482d", "#2d5736", "#366740", "#3e7949", "#46a758",
        "#53b365", "#71d083", "#c2f0c2"
      }
    );

    public static readonly RadixSwatch Bronze = new(
      "bronze",
      new[] {
        "#fdfcfc", "#fdf8f6", "#f8edea", "#f2e1db", "#eaddd7", "#e0cec7", "#d2bab0", "#bfa094", "#977669",
        "#8a6a5e", "#6f4e44", "#3b2f2b"
      },
      new[] {
        "#141110", "#1f1917", "#2a211f", "#332824", "#40302b", "#503c35", "#644a41", "#7c5f54", "#9c6f62",
        "#ad7f72", "#d4c3b7", "#faf2ed"
      }
    );

    public static readonly RadixSwatch Gold = new(
      "gold",
      new[] {
        "#fdfdfc", "#faf9f2", "#f2f0e7", "#eae6db", "#e1dccf", "#d8d0bf", "#cbc0aa", "#b9a88d", "#978365",
        "#8c7a5e", "#71624b", "#3b352b"
      },
      new[] {
        "#121211", "#1b1a17", "#24231f", "#2d2b26", "#36332d", "#3f3b34", "#4d4639", "#625748", "#746a5c",
        "#837a6f", "#d4cfc2", "#f7f4ea"
      }
    );

    public static readonly RadixSwatch Brown = new(
      "brown",
      new[] {
        "#fefdfc", "#fcf9f6", "#f6eee7", "#f0e4d9", "#ebdaca", "#e4cdb7", "#dcbc9f", "#c9a88c", "#a18072",
        "#957468", "#7d5e54", "#3e332e"
      },
      new[] {
        "#12110f", "#1c1816", "#28211d", "#322922", "#3e3128", "#4d3c2f", "#614a39", "#7c5f46", "#9c6f54",
        "#ad7f61", "#d6c3b1", "#faf0e5"
      }
    );

    public static readonly RadixSwatch Orange = new(
      "orange",
      new[] {
        "#fefcfb", "#fff7ed", "#ffefd6", "#ffdfb5", "#ffd19a", "#ffc182", "#f5ae73", "#ec9455", "#f76b15",
        "#ef5f00", "#cc4e00", "#582d1d"
      },
      new[] {
        "#17120e", "#1e160f", "#331e0b", "#462100", "#562800", "#66350c", "#7e451d", "#a35829", "#f76b15",
        "#ff801f", "#ffa057", "#ffe0c2"
      }
    );

    public static readonly RadixSwatch Amber = new(
      "amber",
      new[] {
        "#fefdfb", "#fefbe9", "#fff7c2", "#ffee9c", "#fbe577", "#f3d673", "#e9c162", "#e2a336", "#ffc53d",
        "#ffba18", "#ab6400", "#4f3422"
      },
      new[] {
        "#16120c", "#1d180f", "#302008", "#3f2700", "#4d3000", "#5c3d05", "#714f19", "#8f6424", "#ffc53d",
        "#ffd60a", "#ffca16", "#ffe7b3"
      }
    );

    public static readonly RadixSwatch Yellow = new(
      "yellow",
      new[] {
        "#fdfdf9", "#fefce9", "#fffab8", "#fff394", "#ffe770", "#f3d768", "#e4c767", "#d5ae39", "#ffe629",
        "#ffdc00", "#9e6c00", "#473b1f"
      },
      new[] {
        "#14120b", "#1b180f", "#2d2305", "#362b00", "#433500", "#524202", "#665417", "#836a21", "#ffe629",
        "#ffff57", "#f5e147", "#f6eeb4"
      }
    );

    public static readonly RadixSwatch Lime = new(
      "lime",
      new[] {
        "#fcfdfa", "#f8faf3", "#eef6d6", "#e2f0bd", "#d3e7a6", "#c2da91", "#abc978", "#8db654", "#bdee63",
        "#b0e64c", "#5c7c2f", "#37401c"
      },
      new[] {
        "#11130c", "#151a10", "#1f2917", "#29371d", "#334423", "#3d522a", "#496231", "#577538", "#bdee63",
        "#d4ff70", "#bde56c", "#e3f7ba"
      }
    );

    public static readonly RadixSwatch Mint = new(
      "mint",
      new[] {
        "#f9fefd", "#f2fbf9", "#ddf9f2", "#c8f4e9", "#b3ecde", "#9ce0d0", "#7ecfbd", "#4cbba5", "#86ead4",
        "#7de0cb", "#027864", "#16433c"
      },
      new[] {
        "#0e1515", "#0f1b1b", "#092c2b", "#003a38", "#004744", "#105650", "#1e685f", "#277f70", "#86ead4",
        "#a8f5e5", "#58d5ba", "#c4f5e1"
      }
    );

    public static readonly RadixSwatch Sky = new(
      "sky",
      new[] {
        "#f9feff", "#f1fafd", "#e1f6fd", "#d1f0fa", "#bee7f5", "#a9daed", "#8dcae3", "#60b3d7", "#7ce2fe",
        "#74daf8", "#00749e", "#1d3e56"
      },
      new[] {
        "#0d141f", "#111a27", "#112840", "#113555", "#154467", "#1b537b", "#1f6692", "#197cae", "#7ce2fe",
        "#a8eeff", "#75c7f0", "#c2f3ff"
      }
    );

    public static readonly RadixSwatch[] All = {
      Gray, Mauve, Slate, Sage, Olive, Sand, Tomato, Red, Ruby, Crimson, Pink, Plum, Purple, Violet, Iris, Indigo,
      Blue, Cyan, Teal, Jade, Green, Grass, Bronze, Gold, Brown, Orange, Amber, Yellow, Lime, Mint, Sky
    };

    public static readonly RadixSwatch[] Accents = {
      Tomato, Red, Ruby, Crimson, Pink, Plum, Purple, Violet, Iris, Indigo, Blue, Cyan, Teal, Jade, Green, Grass,
      Bronze, Gold, Brown, Orange, Amber, Yellow, Lime, Mint, Sky
    };

    public static readonly RadixSwatch[] Neutrals = { Gray, Mauve, Slate, Sage, Olive, Sand };

    public static readonly Color Transparent = new(0, 0, 0, 0);

    public static readonly Color Black = new(0, 0, 0);
    public static readonly Color Black05 = new(0, 0, 0, 0.05f);
    public static readonly Color Black10 = new(0, 0, 0, 0.10f);
    public static readonly Color Black15 = new(0, 0, 0, 0.15f);
    public static readonly Color Black20 = new(0, 0, 0, 0.20f);
    public static readonly Color Black30 = new(0, 0, 0, 0.30f);
    public static readonly Color Black40 = new(0, 0, 0, 0.40f);
    public static readonly Color Black50 = new(0, 0, 0, 0.50f);
    public static readonly Color Black60 = new(0, 0, 0, 0.60f);
    public static readonly Color Black70 = new(0, 0, 0, 0.70f);
    public static readonly Color Black80 = new(0, 0, 0, 0.80f);
    public static readonly Color Black90 = new(0, 0, 0, 0.90f);
    public static readonly Color Black95 = new(0, 0, 0, 0.95f);

    public static readonly Color White = new(1, 1, 1);
    public static readonly Color White05 = new(1, 1, 1, 0.05f);
    public static readonly Color White10 = new(1, 1, 1, 0.10f);
    public static readonly Color White15 = new(1, 1, 1, 0.15f);
    public static readonly Color White20 = new(1, 1, 1, 0.20f);
    public static readonly Color White30 = new(1, 1, 1, 0.30f);
    public static readonly Color White40 = new(1, 1, 1, 0.40f);
    public static readonly Color White50 = new(1, 1, 1, 0.50f);
    public static readonly Color White60 = new(1, 1, 1, 0.60f);
    public static readonly Color White70 = new(1, 1, 1, 0.70f);
    public static readonly Color White80 = new(1, 1, 1, 0.80f);
    public static readonly Color White90 = new(1, 1, 1, 0.90f);
    public static readonly Color White95 = new(1, 1, 1, 0.95f);

    public static readonly Dictionary<string, RadixSwatch> Named = All.ToDictionary(
      swatch => swatch.name,
      swatch => swatch
    );

    public static RadixSwatch Neutral => Gray;

    public static Color Hex(string hex) {
      if (ColorUtility.TryParseHtmlString(hex, out var color)) return color;
      throw new ArgumentException($"Invalid hex color: {hex}");
    }

    public static Color Rgb(int r, int g, int b) {
      return new Color(r / 255f, g / 255f, b / 255f);
    }

    public static Color Argb(int a, int r, int g, int b) {
      return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
    }

    public static Color Argb(int argb) {
      return argb.ArgbToColor();
    }

    public static Color Argb(uint argb) {
      return argb.ArgbToColor();
    }

    public static Color OkLch(float l, float c, float h) {
      var lch = new OkLchColor(l, c, h);
      var color = (Color)lch;
      return color.gamma;
    }

    public static Color OkLab(float l, float a, float b) {
      var lab = new OkLabColor(l, a, b);
      var color = (Color)lab;
      return color.gamma;
    }

    public static Color Hsv(float h, float s, float v) {
      return Color.HSVToRGB(h, s, v);
    }

    public static Color AlphaBlend(Color background, Color foreground) {
      var alpha = foreground.a;
      var backAlpha = background.a;
      var invAlpha = 1 - alpha;
      if (alpha == 0) return background;
      if (Mathf.Approximately(backAlpha, 1)) {
        return new Color(
          foreground.r * alpha + background.r * invAlpha,
          foreground.g * alpha + background.g * invAlpha,
          foreground.b * alpha + background.b * invAlpha,
          1
        );
      }

      backAlpha *= invAlpha;
      var outAlpha = alpha + backAlpha;
      return new Color(
        (foreground.r * alpha + background.r * backAlpha) / outAlpha,
        (foreground.g * alpha + background.g * backAlpha) / outAlpha,
        (foreground.b * alpha + background.b * backAlpha) / outAlpha,
        outAlpha
      );
    }

    public static Color WithOpacity(this Color color, float alpha) {
      return new Color(color.r, color.g, color.b, alpha);
    }

    public static Color MultiplyOpacity(this Color color, float alpha) {
      return new Color(color.r, color.g, color.b, color.a * alpha);
    }

    public static float ComputeLuminance(this Color gamma) {
      var linear = gamma.linear;
      return 0.2126f * linear.r + 0.7152f * linear.g + 0.0722f * linear.b;
    }

    public static string ToHex(this Color color) {
      var r = Mathf.Clamp01(color.r);
      var g = Mathf.Clamp01(color.g);
      var b = Mathf.Clamp01(color.b);
      var a = Mathf.Clamp01(color.a);
      return Mathf.Approximately(a, 1f)
        ? $"#{Mathf.RoundToInt(r * 255):X2}{Mathf.RoundToInt(g * 255):X2}{Mathf.RoundToInt(b * 255):X2}"
        : $"#{Mathf.RoundToInt(r * 255):X2}{Mathf.RoundToInt(g * 255):X2}{Mathf.RoundToInt(b * 255):X2}{Mathf.RoundToInt(a * 255):X2}";
    }
  }
}