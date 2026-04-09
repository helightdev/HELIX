using HELIX.Types;
using HELIX.Widgets.Elements;

namespace HELIX.Widgets.Universal {
    public class Container : SingleChildWidget {
        public Alignment alignment = Alignment.Center;
        public BackgroundStyle backgroundStyle;
        public Border border = Border.None;
        public BorderRadius borderRadius = BorderRadius.None;
        public BoxConstraints constraints = BoxConstraints.Initial;

        // ReSharper disable once InconsistentNaming
        public StyleLength2 size {
            set => constraints = BoxConstraints.Preferred(value);
        }

        public override IWidgetElement CreateElement() {
            return ReconcileInto(new ContainerElement());
        }

        public class ContainerElement : SingleChildWidgetBaseElement<Container> {
            public override void Apply(Container previous, Container widget) {
                if (previous == null || !Equals(previous.backgroundStyle, widget.backgroundStyle))
                    (widget.backgroundStyle ?? BackgroundStyle.Default).Apply(this);

                widget.border.Apply(this);
                widget.borderRadius.Apply(this);
                widget.constraints.Apply(this);
                widget.alignment.AlignAsColumn(this);
            }
        }
    }
}