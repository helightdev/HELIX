using HELIX.Types;
using HELIX.Widgets.Diagnostics;

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
            properties.Add(new DiagnosticsProperty<object>("alignment", alignment));
            properties.Add(new DiagnosticsProperty<object>("backgroundStyle", backgroundStyle));
            properties.Add(new DiagnosticsProperty<object>("border", border));
            properties.Add(new DiagnosticsProperty<object>("borderRadius", borderRadius));
            properties.Add(new DiagnosticsProperty<object>("boxShadow", boxShadow));
            properties.Add(new DiagnosticsProperty<object>("constraints", constraints));
            properties.Add(new DiagnosticsProperty<object>("padding", padding));
            properties.Add(new DiagnosticsProperty<object>("opacity", opacity));
            properties.Add(new DiagnosticsProperty<object>("textStyle", textStyle));
            properties.Add(new DiagnosticsProperty<object>("layers", layers));
            properties.Add(new DiagnosticsProperty<object>("transitions", transitions));
            properties.Add(new DiagnosticsProperty<object>("modifiers", modifiers));
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