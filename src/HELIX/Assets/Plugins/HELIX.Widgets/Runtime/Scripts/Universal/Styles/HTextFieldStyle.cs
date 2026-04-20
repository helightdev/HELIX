using HELIX.Types;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Elements;
using HELIX.Widgets.Theming;
using HELIX.Widgets.Universal.Substances;
using HELIX.Widgets.Universal.Theme;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Universal.Styles {
  public class HTextFieldStyle : DiagnosticableBase {
    public static HTextFieldStyle Default = new();
    public WidgetStateProperty<Alignment> alignment = WidgetStateProperties.Never<Alignment>();
    public WidgetStateProperty<BoxConstraints> constraints = WidgetStateProperties.Never<BoxConstraints>();
    public WidgetStateProperty<Color> cursorColor = WidgetStateProperties.Never<Color>();

    public WidgetStateProperty<GenericTextInputStyle> inputStyle =
      WidgetStateProperties.Never<GenericTextInputStyle>();

    public SubstanceLayers layers;
    public WidgetStateProperty<ModifierSet> modifiers = WidgetStateProperties.Never<ModifierSet>();
    public WidgetStateProperty<StyleLength4> padding = WidgetStateProperties.Never<StyleLength4>();
    public WidgetStateProperty<Color> selectionColor = WidgetStateProperties.Never<Color>();

    public WidgetStateProperty<TextStyle> textStyle = WidgetStateProperties.Never<TextStyle>();

    public override void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
      properties.Add(new DiagnosticsProperty<object>("alignment", alignment));
      properties.Add(new DiagnosticsProperty<object>("constraints", constraints));
      properties.Add(new DiagnosticsProperty<object>("layers", layers));
      properties.Add(new DiagnosticsProperty<object>("modifiers", modifiers));
      properties.Add(new DiagnosticsProperty<object>("padding", padding));
      properties.Add(new DiagnosticsProperty<object>("textStyle", textStyle));
    }

    public static HTextFieldStyle DefaultStyleOf(IThemeProvider context) {
      var typography = PrimitiveBaseTheme.Typography.Get(context);
      var spacing = PrimitiveBaseTheme.Spacing.Get(context);
      var colors = PrimitiveBaseTheme.Colors.Get(context);

      return new HTextFieldStyle {
        textStyle = new TextStyle {
          color = colors.surface.onMain,
          fontSize = typography.FontSize2
        },
        constraints = BoxConstraints.Tight(StyleKeyword.Auto, typography.LineHeight2),
        padding = EdgeInsets.Symmetric(spacing.Space2, 0),
        layers = new SubstanceBuilder(context)
          .Outline()
          .Append(ctx => PrimitiveTheme.ButtonFocusLayer.Get(ctx))
          .Build()
      };
    }
  }
}