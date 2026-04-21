using HELIX.Types;
using HELIX.Widgets.Elements;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Modifiers {
  public static class ModifierFallbacks {
    /// <summary>
    /// This fallback modifier causes the widget to use flex filling to fill available space unless placed under
    /// a parent using <see cref="IPreferExplicitFlex"/>.
    /// </summary>
    /// <remarks>
    /// Default behavior:
    /// <list type="bullet">
    /// <item><see cref="IStyle.flexGrow"/>: <c>1</c></item>
    /// <item><see cref="IStyle.flexShrink"/>: <c>1</c></item>
    /// <item><see cref="IStyle.alignSelf"/>: <see cref="Align.Stretch"/></item>
    /// </list>
    ///
    /// When placed under a parent using <see cref="IPreferExplicitFlex"/>:
    /// <list type="bullet">
    /// <item><see cref="IStyle.flexGrow"/>: <c>0</c></item>
    /// <item><see cref="IStyle.flexShrink"/>: <c>0</c></item>
    /// <item><see cref="IStyle.alignSelf"/>: <see cref="Align.Auto"/></item>
    /// </list>
    /// </remarks>
    public static readonly FlexibleModifier ImplicitFlexFill = new(1f, 1f, Align.Stretch) {
      isFallback = true,
      isImplicit = true
    };

    /// <summary>
    /// This fallback modifier causes the widget to use absolute positioning to fill available space when placed under
    /// a parent using <see cref="IPreferStacking"/>.
    /// </summary>
    public static readonly PositionModifier StackingStretch = new(new StyleLength4(0), Position.Absolute) {
      isFallback = true,
      isStackingOnly = true
    };

    /// <summary>
    /// This fallback modifier causes the widget to use no flex layouting, forcing the implicit preferred size of its
    /// content to be used.
    /// </summary>
    public static readonly FlexibleModifier FlexTight = new(0f, 0f, Align.Auto) { isFallback = true };

    public static readonly FlexibleModifier FlexFill = new(1f, 1f, Align.Stretch) { isFallback = true };
    public static readonly FlexibleModifier FlexExpand = new(1f, 1f, Align.Auto) { isFallback = true };
    public static readonly FlexibleModifier FlexTightStretch = new(0f, 0f, Align.Stretch) { isFallback = true };

    public static readonly PositionModifier PosStretch = new(new StyleLength4(0), Position.Absolute)
      { isFallback = true };

    public static readonly PaddingModifier PaddingZero = new(StyleLength4.Zero) { isFallback = true };
    public static readonly MarginModifier MarginZero = new(StyleLength4.Zero) { isFallback = true };
  }
}