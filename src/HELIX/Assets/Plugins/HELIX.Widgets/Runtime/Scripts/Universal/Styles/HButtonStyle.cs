using System.Collections.Generic;
using HELIX.Types;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Universal.Theme;

namespace HELIX.Widgets.Universal.Styles {
    public class HButtonStyle : DiagnosticableBase {
        public static HButtonStyle Default = new();

        public WidgetStateProperty<BackgroundStyle> backgroundStyle = WidgetStateProperties.Never<BackgroundStyle>();
        public WidgetStateProperty<Border> border = WidgetStateProperties.Never<Border>();
        public WidgetStateProperty<BorderRadius> borderRadius = WidgetStateProperties.Never<BorderRadius>();
        public WidgetStateProperty<BoxShadowStyle> boxShadow = WidgetStateProperties.Never<BoxShadowStyle>();
        public WidgetStateProperty<float> opacity = WidgetStateProperties.Never<float>();
        public WidgetStateProperty<TextStyle> textStyle = WidgetStateProperties.Never<TextStyle>();
        public SubstanceLayers layers = default;
        public WidgetStateProperty<Transition[]> transitions = WidgetStateProperties.Never<Transition[]>();

        public WidgetStateProperty<Alignment> alignment = WidgetStateProperties.Never<Alignment>();
        public WidgetStateProperty<BoxConstraints> constraints = WidgetStateProperties.Never<BoxConstraints>();
        public WidgetStateProperty<StyleLength4> padding = WidgetStateProperties.Never<StyleLength4>();
        public WidgetStateProperty<ModifierSet> modifiers = WidgetStateProperties.Never<ModifierSet>();

        public override void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.DebugFillProperties(properties);
            properties.Add(new DiagnosticsProperty<object>("alignment", alignment, showName: false));
            properties.Add(new DiagnosticsProperty<object>("backgroundStyle", backgroundStyle, showName: false));
            properties.Add(new DiagnosticsProperty<object>("border", border, showName: false));
            properties.Add(new DiagnosticsProperty<object>("borderRadius", borderRadius, showName: false));
            properties.Add(new DiagnosticsProperty<object>("boxShadow", boxShadow, showName: false));
            properties.Add(new DiagnosticsProperty<object>("constraints", constraints, showName: false));
            properties.Add(new DiagnosticsProperty<object>("padding", padding, showName: false));
            properties.Add(new DiagnosticsProperty<object>("opacity", opacity, showName: false));
            properties.Add(new DiagnosticsProperty<object>("textStyle", textStyle, showName: false));
            properties.Add(new DiagnosticsProperty<object>("layers", layers, showName: false));
            properties.Add(new DiagnosticsProperty<object>("transitions", transitions, showName: false));
            properties.Add(new DiagnosticsProperty<object>("modifiers", modifiers, showName: false));
        }
    }

    public enum HButtonVariant {
        Default,
        Flat,
        FlatTwoState,
        Soft,
        Outline,
        Ghost
    }

    public enum HButtonSize {
        Small,
        Regular,
        Medium,
        Large
    }

    public enum HInputRadius {
        None,
        Small,
        Medium,
        Large,
        Full
    }
}