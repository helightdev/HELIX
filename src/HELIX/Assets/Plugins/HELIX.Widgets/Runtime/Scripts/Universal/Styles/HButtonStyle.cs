using HELIX.Types;
using HELIX.Widgets.Diagnostics;

namespace HELIX.Widgets.Universal.Styles {
    public class HButtonStyle : DiagnosticableBase {
        public static HButtonStyle Default = new();
        
        public IWidgetStateProperty<Alignment> alignment = WidgetStateProperties.Never<Alignment>();
        public IWidgetStateProperty<BackgroundStyle> backgroundStyle = WidgetStateProperties.Never<BackgroundStyle>();
        public IWidgetStateProperty<Border> border = WidgetStateProperties.Never<Border>();
        public IWidgetStateProperty<BorderRadius> borderRadius = WidgetStateProperties.Never<BorderRadius>();
        public IWidgetStateProperty<BoxShadowStyle> boxShadow = WidgetStateProperties.Never<BoxShadowStyle>();
        public IWidgetStateProperty<BoxConstraints> constraints = WidgetStateProperties.Never<BoxConstraints>();
        public IWidgetStateProperty<StyleLength4> padding = WidgetStateProperties.Never<StyleLength4>();
        public IWidgetStateProperty<float> opacity = WidgetStateProperties.Never<float>();
        public IWidgetStateProperty<TextStyle> textStyle = WidgetStateProperties.Never<TextStyle>();
        public IWidgetStateProperty<Transition[]> transitions = WidgetStateProperties.Never<Transition[]>();
        public IWidgetStateProperty<Modifier[]> modifiers = WidgetStateProperties.Never<Modifier[]>();

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
            properties.Add(new DiagnosticsProperty<object>("transitions", transitions, showName: false));
            properties.Add(new DiagnosticsProperty<object>("modifiers", modifiers, showName: false));
        }
    }

    public enum HButtonVariant {
        Default,
        Solid,
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