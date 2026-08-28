using HELIX.Prose;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  /// <summary>
  ///   Overrides selected <see cref="Flex" /> arguments when a Compose prose frame is baked.
  ///   Multiple modifiers may be applied; later non-null values take precedence.
  /// </summary>
  public sealed class ComposeFlexModifier : IProseModifier {
    public ComposeFlexModifier(
      Axis? axis = null,
      Justify? main = null,
      Align? cross = null,
      float? gap = null,
      bool? reverse = null,
      bool? clear = null
    ) {
      Axis = axis;
      Main = main;
      Cross = cross;
      Gap = gap;
      Reverse = reverse;
      Clear = clear;
    }

    public Axis? Axis { get; }
    public Justify? Main { get; }
    public Align? Cross { get; }
    public float? Gap { get; }
    public bool? Reverse { get; }
    public bool? Clear { get; }
  }

  public static class ComposeProseModifiers {
    public static ComposeFlexModifier Flex(
      Axis? axis = null,
      Justify? main = null,
      Align? cross = null,
      float? gap = null,
      bool? reverse = null,
      bool? clear = null
    ) {
      return new ComposeFlexModifier(axis, main, cross, gap, reverse, clear);
    }
  }
}