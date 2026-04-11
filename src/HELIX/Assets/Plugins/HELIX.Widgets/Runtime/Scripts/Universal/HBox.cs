using HELIX.Types;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Properties;
using HELIX.Widgets.Elements;
using HELIX.Widgets.Modifiers;

namespace HELIX.Widgets.Universal {
    public class HBox : SingleChildWidget {
        public Alignment alignment = Alignment.Center;
        public BackgroundStyle backgroundStyle;
        public Border border = Border.None;
        public BorderRadius borderRadius = BorderRadius.None;

        public HBox() {
            AddModifier(ModifierFallbacks.ImplicitFlexFill);
        }

        // ReSharper disable once InconsistentNaming

        public override IWidgetElement CreateElement() {
            return ReconcileInto(new ContainerElement());
        }

        public class ContainerElement : SingleChildWidgetBaseElement<HBox> {
            public override void Apply(HBox previous, HBox widget) {
                if (previous == null || !Equals(previous.backgroundStyle, widget.backgroundStyle))
                    (widget.backgroundStyle ?? BackgroundStyle.Default).Apply(this);

                widget.border.Apply(this);
                widget.borderRadius.Apply(this);
                widget.alignment.AlignAsColumn(this);
            }
        }

        public override void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.DebugFillProperties(properties);
            properties.Add(new DiagnosticsProperty<Alignment>("alignment", alignment));
            properties.Add(new BackgroundStyleProperty("backgroundStyle", backgroundStyle));
            properties.Add(new DiagnosticsProperty<Border>("border", border, defaultValue: Border.None));
            properties.Add(
                new DiagnosticsProperty<BorderRadius>("borderRadius", borderRadius, defaultValue: BorderRadius.None)
            );
        }
    }
}