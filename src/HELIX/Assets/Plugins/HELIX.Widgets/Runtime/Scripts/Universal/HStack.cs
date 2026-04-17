using HELIX.Types;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Properties;
using HELIX.Widgets.Elements;
using HELIX.Widgets.Modifiers;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Universal {
    public class HStack : MultiChildWidget {
        public Axis axis = Axis.Vertical;
        public Align crossAxisAlign = Align.FlexStart;
        public Justify mainAxisAlign = Justify.FlexStart;
        public Align? wrapAlign = null;
        public bool reverse = false;
        public bool wrapReverse = false;
        public bool wrap = false;

        public HStack() {
            AddModifier(ModifierFallbacks.ImplicitFlexFill);
        }

        public override IWidgetElement CreateElement() => ReconcileInto(new HStackElement());

        public override void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.DebugFillProperties(properties);
            properties.Add(new EnumProperty<Axis>("axis", axis, defaultValue: Axis.Horizontal));
            properties.Add(new EnumProperty<Align>("crossAxisAlign", crossAxisAlign, defaultValue: Align.Center));
            properties.Add(new EnumProperty<Justify>("mainAxisAlign", mainAxisAlign, defaultValue: Justify.FlexStart));
            properties.Add(
                new EnumProperty<Align>("wrapAlign", wrapAlign ?? crossAxisAlign, defaultValue: crossAxisAlign)
            );
            properties.Add(new FlagProperty("wrap", wrap, ifTrue: "Wrap", ifFalse: "No Wrap"));
            properties.Add(new FlagProperty("reverse", reverse, ifTrue: "Reverse"));
            properties.Add(new FlagProperty("wrapReverse", wrapReverse, ifTrue: "Wrap Reverse"));
        }
    }

    public class HStackElement : MultiChildWidgetBaseElement<HStack>, IPreferExplicitFlex, IPreferStacking {
        public override void Apply(HStack previous, HStack widget) {
            base.Apply(previous, widget);
            if (widget.axis == Axis.Horizontal) {
                style.flexDirection = widget.reverse ? FlexDirection.RowReverse : FlexDirection.Row;
            } else { style.flexDirection = widget.reverse ? FlexDirection.ColumnReverse : FlexDirection.Column; }

            style.flexWrap = widget.wrap ? widget.wrapReverse ? Wrap.WrapReverse : Wrap.Wrap : Wrap.NoWrap;
            style.alignContent = widget.wrapAlign ?? widget.crossAxisAlign;
            style.alignItems = widget.crossAxisAlign;
            style.justifyContent = widget.mainAxisAlign;
        }
    }
}