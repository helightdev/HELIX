using System.Collections.Generic;
using HELIX.Types;
using HELIX.Widgets.Elements;
using HELIX.Widgets.Universal.Theme;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Universal {
  public class HGap : Widget {
    public readonly Axis? axis;
    public readonly StyleLength? size;
    public readonly int level;

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