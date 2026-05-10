using System.Collections.Generic;
using HELIX.Types;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Properties;
using HELIX.Widgets.Elements;
using HELIX.Widgets.Modifiers;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Universal {

  /// <summary>
  /// Represents a container that tries to arrange its children above each other using absolute positioning.
  /// </summary>
  public class HStack : MultiChildWidget {
    public readonly Axis axis;
    public readonly Align crossAxisAlign;
    public readonly Justify mainAxisAlign;
    public readonly bool reverse;
    public readonly bool wrap;
    public readonly Align? wrapAlign;
    public readonly bool wrapReverse;

    /// <summary>
    /// Creates a container that tries to arrange its children above each other using absolute positioning.
    /// If the children do not use absolute positioning, behavior will be similar to <see cref="HFlex"/>.
    /// </summary>
    /// <param name="axis">The axis along which non-stacking children will be arranged.</param>
    /// <param name="crossAxisAlign">The alignment along the cross-axis.</param>
    /// <param name="mainAxisAlign">The alignment along the main axis.</param>
    /// <param name="wrapAlign">
    /// The cross-axis alignment used when wrapping non-stacking children.
    /// Defaults to <paramref name="crossAxisAlign"/>.
    /// </param>
    /// <param name="reverse">Whether to reverse the order of the children.</param>
    /// <param name="wrapReverse">Whether to wrap non-stacking children in reverse order.</param>
    /// <param name="wrap">Whether to enable wrapping for non-stacking children.</param>
    /// <param name="children">The children of this container.</param>
    /// <param name="key">Passed on to <see cref="Widget.key"/>.</param>
    /// <param name="constants">Passed on to <see cref="Widget.constants"/>.</param>
    /// <param name="modifiers">Passed on to <see cref="Widget.modifiers"/>.</param>
    /// <seealso cref="IPreferExplicitFlex"/>
    /// <seealso cref="IPreferStacking"/>
    /// <inheritdoc/>
    public HStack(
      Axis axis = Axis.Vertical,
      Align crossAxisAlign = Align.FlexStart,
      Justify mainAxisAlign = Justify.FlexStart,
      Align? wrapAlign = null,
      bool reverse = false,
      bool wrapReverse = false,
      bool wrap = false,
      Key key = default,
      object[] constants = null,
      IReadOnlyList<Widget> children = null,
      IReadOnlyCollection<Modifier> modifiers = null
    ) : base(children, key, constants) {
      this.axis = axis;
      this.crossAxisAlign = crossAxisAlign;
      this.mainAxisAlign = mainAxisAlign;
      this.wrapAlign = wrapAlign;
      this.reverse = reverse;
      this.wrapReverse = wrapReverse;
      this.wrap = wrap;

      DefaultModifiers(ModifierSet.DefaultFlexFill, modifiers);
    }

    public override IWidgetElement CreateElement() {
      return ReconcileInto(new HStackElement());
    }

    public override void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
      base.DebugFillProperties(properties);
      properties.Add(new EnumProperty<Axis>("axis", axis, Axis.Vertical));
      properties.Add(new EnumProperty<Align>("crossAxisAlign", crossAxisAlign, Align.FlexStart));
      properties.Add(new EnumProperty<Justify>("mainAxisAlign", mainAxisAlign, Justify.FlexStart));
      properties.Add(new EnumProperty<Align>("wrapAlign", wrapAlign ?? crossAxisAlign, crossAxisAlign));
      properties.Add(new FlagProperty("wrap", wrap, "Wrap", "No Wrap"));
      properties.Add(new FlagProperty("reverse", reverse, "Reverse"));
      properties.Add(new FlagProperty("wrapReverse", wrapReverse, "Wrap Reverse"));
    }
  }

  public class HStackElement : MultiChildWidgetBaseElement<HStack>, IPreferExplicitFlex, IPreferStacking {
    public Axis PreferredFlexAxis => TypedDescriptor?.axis ?? Axis.Vertical;

    public override void Apply(HStack previous, HStack widget) {
      base.Apply(previous, widget);
      if (widget.axis == Axis.Horizontal)
        style.flexDirection = widget.reverse ? FlexDirection.RowReverse : FlexDirection.Row;
      else style.flexDirection = widget.reverse ? FlexDirection.ColumnReverse : FlexDirection.Column;

      style.flexWrap = widget.wrap ? widget.wrapReverse ? Wrap.WrapReverse : Wrap.Wrap : Wrap.NoWrap;
      style.alignContent = widget.wrapAlign ?? widget.crossAxisAlign;
      style.alignItems = widget.crossAxisAlign;
      style.justifyContent = widget.mainAxisAlign;
    }
  }
}