using System.Collections.Generic;
using HELIX.Types;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Properties;
using HELIX.Widgets.Elements;
using HELIX.Widgets.Universal.Styles;

namespace HELIX.Widgets.Universal {
    public class HBox : SingleChildWidget {

        public readonly Alignment alignment;
        public readonly BackgroundStyle background;
        public readonly Border border;
        public readonly BorderRadius borderRadius;

        public HBox(
            Alignment? alignment = null,
            BackgroundStyle background = null,
            Border? border = null,
            BorderRadius? borderRadius = null,
            Widget child = null,
            Key key = default,
            object[] constants = null,
            IReadOnlyCollection<Modifier> modifiers = null
        ) : base(child, key, constants) {
            this.alignment = alignment ?? Alignment.Center;
            this.background = background;
            this.border = border ?? Border.None;
            this.borderRadius = borderRadius ?? BorderRadius.None;

            DefaultModifiers(ModifierSet.DefaultFlexFill, modifiers);
        }

        // ReSharper disable once InconsistentNaming

        public override IWidgetElement CreateElement() {
            return ReconcileInto(new ContainerElement());
        }

        public override void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.DebugFillProperties(properties);
            properties.Add(new DiagnosticsProperty<Alignment>("alignment", alignment));
            properties.Add(new BackgroundStyleProperty("backgroundStyle", background));
            properties.Add(new DiagnosticsProperty<Border>("border", border, defaultValue: Border.None));
            properties.Add(
                new DiagnosticsProperty<BorderRadius>("borderRadius", borderRadius, defaultValue: BorderRadius.None)
            );
        }

        public class ContainerElement : SingleChildWidgetBaseElement<HBox> {

            public override void Apply(HBox previous, HBox widget) {
                if (previous == null || !Equals(previous.background, widget.background))
                    (widget.background ?? BackgroundStyle.Default).Apply(this);

                widget.border.Apply(this);
                widget.borderRadius.Apply(this);
                widget.alignment.AlignAsColumn(this);
            }

        }

    }
}