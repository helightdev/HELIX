using HELIX.Types;
using HELIX.Widgets.Elements;

namespace HELIX.Widgets.Universal {
    public class Container : Widget {
        public Alignment alignment = Alignment.Center;
        public BackgroundStyle backgroundStyle;
        public Border border = Border.None;
        public BorderRadius borderRadius = BorderRadius.None;
        public Widget child;
        public BoxConstraints constraints = BoxConstraints.Initial;

        // ReSharper disable once InconsistentNaming
        public StyleLength2 size {
            set => constraints = BoxConstraints.Preferred(value);
        }

        public override IWidgetElement CreateElement() {
            var element = new ContainerElement();
            element.Reconcile(this);
            return element;
        }

        public class ContainerElement : SingleChildWidgetHostElement<Container> {
            public override Widget GetChild(Container widget) {
                return widget.child;
            }

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