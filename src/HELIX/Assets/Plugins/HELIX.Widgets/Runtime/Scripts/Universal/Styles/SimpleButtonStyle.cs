using HELIX.Types;

namespace HELIX.Widgets.Universal.Styles {
    public class SimpleButtonStyle {
        public IWidgetStateProperty<Alignment> alignment = WidgetStateProperties.Never<Alignment>();
        public IWidgetStateProperty<BackgroundStyle> backgroundStyle = WidgetStateProperties.Never<BackgroundStyle>();
        public IWidgetStateProperty<Border> border = WidgetStateProperties.Never<Border>();
        public IWidgetStateProperty<BorderRadius> borderRadius = WidgetStateProperties.Never<BorderRadius>();
        public IWidgetStateProperty<BoxShadowStyle> boxShadow = WidgetStateProperties.Never<BoxShadowStyle>();
        public IWidgetStateProperty<BoxConstraints> constraints = WidgetStateProperties.Never<BoxConstraints>();
        public IWidgetStateProperty<StyleLength4> padding = WidgetStateProperties.Never<StyleLength4>();
        public IWidgetStateProperty<TextStyle> textStyle = WidgetStateProperties.Never<TextStyle>();
        public IWidgetStateProperty<Transition[]> transitions = WidgetStateProperties.Never<Transition[]>();
    }
}