using HELIX.Types;
using HELIX.Widgets.Diagnostics;

namespace HELIX.Widgets.Universal.Styles {
  /// <summary>
  /// Defines the visual appearance of a <see cref="HButton"/>.
  /// </summary>
  public class HButtonStyle : DiagnosticableBase {
    public static HButtonStyle Default = new();

    /// <summary>
    /// Controls <see cref="HSubstanceBox.alignment"/>.
    /// </summary>
    public WidgetStateProperty<Alignment> alignment = WidgetStateProperties.Never<Alignment>();

    /// <summary>
    /// Controls the background <see cref="HSubstanceBox.substances"/>.
    /// </summary>
    public SubstanceLayers layers = default;

    /// <summary>
    /// Defines constraints applied using <see cref="Modifiers.SizeModifier"/>.
    /// </summary>
    public WidgetStateProperty<BoxConstraints> constraints = WidgetStateProperties.Never<BoxConstraints>();

    /// <summary>
    /// Defines the opacity of the button applied using <see cref="Modifiers.OpacityModifier"/>.
    /// </summary>
    public WidgetStateProperty<float> opacity = WidgetStateProperties.Never<float>();

    /// <summary>
    /// Defines additional modifiers that may override the button's default modifiers.
    /// </summary>
    /// <remarks>
    /// Modifiers are merged using the <see cref="MergingModifiersWidgetState"/>.
    /// </remarks>
    public WidgetStateProperty<ModifierSet> modifiers = WidgetStateProperties.Never<ModifierSet>();

    /// <summary>
    /// Defines padding between the button's background and its content.
    /// </summary>
    public WidgetStateProperty<StyleLength4> padding = WidgetStateProperties.Never<StyleLength4>();

    /// <summary>
    /// Define the default text applied using <see cref="Modifiers.TextStyleModifier"/>.
    /// </summary>
    public WidgetStateProperty<TextStyle> textStyle = WidgetStateProperties.Never<TextStyle>();


    public override void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
      base.DebugFillProperties(properties);
      properties.Add(new DiagnosticsProperty<object>("alignment", alignment));
      properties.Add(new DiagnosticsProperty<object>("constraints", constraints));
      properties.Add(new DiagnosticsProperty<object>("padding", padding));
      properties.Add(new DiagnosticsProperty<object>("opacity", opacity));
      properties.Add(new DiagnosticsProperty<object>("textStyle", textStyle));
      properties.Add(new DiagnosticsProperty<object>("layers", layers));
      properties.Add(new DiagnosticsProperty<object>("modifiers", modifiers));
    }
  }

  public enum HButtonVariant {
    Default,
    Flat,
    FlatTwoState,
    Soft,
    SoftTwoState,
    Outline,
    Ghost,
    TwoState
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