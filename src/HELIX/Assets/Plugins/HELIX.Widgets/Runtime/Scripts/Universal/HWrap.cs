using HELIX.Types;
using HELIX.Widgets;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Properties;
using HELIX.Widgets.Elements;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Universal {
    
    public class HWrap : MultiChildWidget {
        public Axis axis = Axis.Horizontal;
        public Align crossAxisAlign = Align.FlexStart;
        public Justify mainAxisAlign = Justify.FlexStart;
        public Align? wrapAlign = null;
        public bool reverse = false;
        public bool wrapReverse = false;
        public bool wrap = true;
        public override IWidgetElement CreateElement() => ReconcileInto(new FlexWrapElement());

        public override void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.DebugFillProperties(properties);
            properties.Add(new EnumProperty<Axis>("axis", axis, defaultValue: Axis.Horizontal));
            properties.Add(new EnumProperty<Align>("crossAxisAlign", crossAxisAlign, defaultValue: Align.Center));
            properties.Add(new EnumProperty<Justify>("mainAxisAlign", mainAxisAlign, defaultValue: Justify.FlexStart));
            properties.Add(
                new EnumProperty<Align>("wrapAlign", wrapAlign ?? crossAxisAlign, defaultValue: crossAxisAlign)
            );
            properties.Add(new FlagProperty("reverse", reverse, ifTrue: "Reverse"));
            properties.Add(new FlagProperty("wrapReverse", wrapReverse, ifTrue: "Wrap Reverse"));
        }
    }

    public class FlexWrapElement : MultiChildWidgetBaseElement<HWrap>, IPreferExplicitFlex {
        public override void Apply(HWrap previous, HWrap widget) {
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