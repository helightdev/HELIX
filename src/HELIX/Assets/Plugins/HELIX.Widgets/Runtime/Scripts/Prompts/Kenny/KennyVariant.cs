using System;

namespace HELIX.Widgets.Prompts.Kenny {
  [Flags]
  public enum KennyVariant {
    None = 0,
    Icon = 1 << 1,
    Alternative = 1 << 2,
    Color = 1 << 3,
    Outline = 1 << 4,
  }
}