using System.Collections.Generic;
using HELIX.Types;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Properties;
using HELIX.Widgets.Elements;
using HELIX.Widgets.Modifiers;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Universal {
  /// <summary>
  /// Represents a flexible layout widget that arranges its child widgets
  /// along a specified axis with various alignment and wrapping properties.
  /// </summary>
  public class HFlex : MultiChildWidget {
    public readonly Axis axis;
    public readonly Align crossAxisAlign;
    public readonly Justify mainAxisAlign;
    public readonly bool reverse;
    public readonly bool wrap;
    public readonly Align? wrapAlign;
    public readonly bool wrapReverse;

    /// <summary>
    /// Represents a flexible layout widget that arranges its child widgets
    /// along a specified axis, with additional alignment, wrapping, and
    /// layout options.
    /// </summary>
    /// <param name="axis">The axis along which the child widgets will be arranged.</param>
    /// <param name="crossAxisAlign">The alignment along the cross-axis.</param>
    /// <param name="mainAxisAlign">The alignment along the main axis.</param>
    /// <param name="wrapAlign">
    /// The cross-axis alignment used when wrapping.
    /// Defaults to <paramref name="crossAxisAlign"/>.
    /// </param>
    /// <param name="reverse">Whether to reverse the order of the children.</param>
    /// <param name="wrapReverse">Whether to wrap the child widgets in reverse order.</param>
    /// <param name="wrap">Whether to enable wrapping.</param>
    /// <param name="children">The children of this container.</param>
    /// <param name="key">Passed on to <see cref="Widget.key"/>.</param>
    /// <param name="constants">Passed on to <see cref="Widget.constants"/>.</param>
    /// <param name="modifiers">Passed on to <see cref="Widget.modifiers"/>.</param>
    /// <seealso cref="IPreferExplicitFlex"/>
    /// <seealso cref="ModifierFallbacks.ImplicitFlexFill"/>
    /// <inheritdoc/>
    public HFlex(
      Axis axis = Axis.Vertical,
      Align crossAxisAlign = Align.FlexStart,
      Justify mainAxisAlign = Justify.FlexStart,
      Align? wrapAlign = null,
      bool reverse = false,
      bool wrapReverse = false,
      bool wrap = false,
      IReadOnlyList<Widget> children = null,
      Key key = default,
      object[] constants = null,
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
      return ReconcileInto(new HFlexElement());
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

  public class HFlexElement : MultiChildWidgetBaseElement<HFlex>, IPreferExplicitFlex {
    public Axis PreferredFlexAxis => TypedDescriptor?.axis ?? Axis.Vertical;

    public override void Apply(HFlex previous, HFlex widget) {
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