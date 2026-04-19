using System.Collections.Generic;
using HELIX.Types;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Properties;
using HELIX.Widgets.Elements;
using HELIX.Widgets.Modifiers;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Universal {
    public class HStack : MultiChildWidget {
        public readonly Axis axis;
        public readonly Align crossAxisAlign;
        public readonly Justify mainAxisAlign;
        public readonly Align? wrapAlign;
        public readonly bool reverse;
        public readonly bool wrapReverse;
        public readonly bool wrap;

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

        public override IWidgetElement CreateElement() => ReconcileInto(new HStackElement());

        public override void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.DebugFillProperties(properties);
            properties.Add(new EnumProperty<Axis>("axis", axis, defaultValue: Axis.Vertical));
            properties.Add(new EnumProperty<Align>("crossAxisAlign", crossAxisAlign, defaultValue: Align.FlexStart));
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