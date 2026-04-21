using System.Collections.Generic;
using HELIX.Types;
using HELIX.Widgets.Elements;
using HELIX.Widgets.Universal.Theme;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Universal {
  /// <summary>
  /// A widget used to easily add spacing between other widgets.
  /// </summary>
  public class HGap : Widget {
    public readonly Axis? axis;
    public readonly StyleLength? size;
    public readonly int level;

    /// <summary>
    /// Creates a widget to add spacing between other widgets using a theme-defined size.
    /// </summary>
    /// <param name="level">
    /// The level of the gap [1, 9], defaulting to <c>1</c>.
    /// Sizes are resolved using <see cref="PrimitiveBaseTheme.Spacing"/>.
    /// </param>
    /// <param name="axis">
    /// The axis of the gap. If not specified, the axis of the parent flex container will be used,
    /// or vertical if there is no parent flex container.
    /// </param>
    /// <param name="key">Passed on to <see cref="Widget.key"/>.</param>
    /// <param name="constants">Passed on to <see cref="Widget.constants"/>.</param>
    /// <param name="modifiers">Passed on to <see cref="Widget.modifiers"/>.</param>
    public HGap(
      int level = 1,
      Axis? axis = null,
      Key key = default,
      object[] constants = null,
      IReadOnlyCollection<Modifier> modifiers = null
    ) : base(key, constants, modifiers) {
      this.axis = axis;
      size = null;
      this.level = level;
    }

    /// <summary>
    /// Creates a widget to add spacing between other widgets using a custom size.
    /// </summary>
    /// <param name="size">
    /// The size of the gap. Falls back to <see cref="PrimitiveSpacingScheme.Space1"/> resolved
    /// using <see cref="PrimitiveBaseTheme.Spacing"/> when null.
    /// </param>
    /// <param name="axis">
    /// The axis of the gap. If not specified, the axis of the parent flex container will be used,
    /// or vertical if there is no parent flex container.
    /// </param>
    /// <param name="key">Passed on to <see cref="Widget.key"/>.</param>
    /// <param name="constants">Passed on to <see cref="Widget.constants"/>.</param>
    /// <param name="modifiers">Passed on to <see cref="Widget.modifiers"/>.</param>
    public HGap(
      StyleLength? size,
      Axis? axis = null,
      Key key = default,
      object[] constants = null,
      IReadOnlyCollection<Modifier> modifiers = null
    ) : base(key, constants, modifiers) {
      this.axis = axis;
      this.size = size;
    }

    public override IWidgetElement CreateElement() => ReconcileInto(new HGapElement());
  }

  public class HGapElement : WidgetBaseElement<HGap> {
    public override void Apply(HGap previous, HGap widget) {
      var spacing = PrimitiveBaseTheme.Spacing.Get(this);
      var size = widget.size ?? spacing.Level(widget.level);

      Axis axis;
      var directParent = BuildContext.GetDirectParent(this);
      if (directParent is IPreferExplicitFlex flexParent) { axis = widget.axis ?? flexParent.PreferredFlexAxis; } else {
        axis = widget.axis ?? Axis.Vertical;
      }

      if (axis == Axis.Vertical) {
        style.height = size;
        style.width = new StyleLength(Length.Auto());
      } else {
        style.width = size;
        style.height = new StyleLength(Length.Auto());
      }
    }
  }
}