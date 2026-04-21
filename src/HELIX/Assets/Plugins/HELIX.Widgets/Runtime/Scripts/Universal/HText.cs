using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Properties;
using HELIX.Widgets.Modifiers;
using HELIX.Widgets.Theming;
using HELIX.Widgets.Universal.Styles;
using HELIX.Widgets.Universal.Theme;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Universal {
  /// <summary>
  /// A wrapper widget around <see cref="Label"/> that allows for easy configuration of the textual content.
  /// </summary>
  public class HText : WrappingBaseWidget<HText, Label> {
    private readonly ModifierSet _defaultModifiers = new ModifierSet {
      ModifierFallbacks.PaddingZero,
      ModifierFallbacks.MarginZero
    }.Sealed();

    public readonly bool doubleClickSelectsWords;
    public readonly bool emojiFallbackSupport;
    public readonly bool enableRichText;
    public readonly LanguageDirection languageDirection;
    public readonly bool parseEscapeSequences;
    public readonly bool selectable;

    public readonly string text;
    public readonly bool tripleClickSelectsLine;
    public TextStyle style;

    /// <summary>
    /// Creates a wrapper widget around <see cref="Label"/> that allows for easy configuration of the textual content.
    /// </summary>
    /// <param name="text">The textual content to display.</param>
    /// <param name="enableRichText">Whether the text supports unity's rich text formatting.</param>
    /// <param name="emojiFallbackSupport">Whether to use emoji fallbacks for unsupported characters.</param>
    /// <param name="parseEscapeSequences">Whether to parse escape sequences like <c>\n</c>in the text.</param>
    /// <param name="selectable">Whether the text is selectable by the user.</param>
    /// <param name="doubleClickSelectsWords">Whether double-clicking selects words.</param>
    /// <param name="tripleClickSelectsLine">Whether triple-clicking selects entire lines.</param>
    /// <param name="languageDirection">The language direction for the text.</param>
    /// <param name="style">Style overrides to apply to the text.</param>
    public HText(
      string text,
      bool enableRichText = false,
      bool emojiFallbackSupport = true,
      bool parseEscapeSequences = false,
      bool selectable = false,
      bool doubleClickSelectsWords = true,
      bool tripleClickSelectsLine = true,
      LanguageDirection languageDirection = LanguageDirection.Inherit,
      TextStyle style = null
    ) {
      this.text = text;
      this.enableRichText = enableRichText;
      this.emojiFallbackSupport = emojiFallbackSupport;
      this.parseEscapeSequences = parseEscapeSequences;
      this.selectable = selectable;
      this.doubleClickSelectsWords = doubleClickSelectsWords;
      this.tripleClickSelectsLine = tripleClickSelectsLine;
      this.languageDirection = languageDirection;
      this.style = style;

      DefaultModifiers(_defaultModifiers, null);
    }

    public override Label Create() {
      return new Label();
    }

    public override void Apply(HText previous, Label element) {
      if (text != element.text) element.text = text;
      element.enableRichText = enableRichText;
      element.emojiFallbackSupport = emojiFallbackSupport;
      element.parseEscapeSequences = parseEscapeSequences;
      element.languageDirection = languageDirection;
      element.selection.isSelectable = selectable;
      element.selection.doubleClickSelectsWord = doubleClickSelectsWords;
      element.selection.tripleClickSelectsLine = tripleClickSelectsLine;

      if (previous == null || !Equals(style, previous.style)) (style ?? TextStyle.Default).Apply(element);
    }

    public override void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
      base.DebugFillProperties(properties);
      properties.Add(new StringProperty("text", text));
      properties.Add(new TextStyleProperty("style", style));

      properties.Add(new FlagProperty("enableRichText", enableRichText, "RichText"));
      properties.Add(new FlagProperty("emojiFallbackSupport", emojiFallbackSupport, ifFalse: "NoEmojiFallback"));
      properties.Add(
        new FlagProperty("parseEscapeSequences", parseEscapeSequences, ifFalse: "NoEscapeSequenceParsing")
      );
      properties.Add(new FlagProperty("selectable", selectable, "Selectable"));
      properties.Add(
        new FlagProperty(
          "doubleClickSelectsWords",
          doubleClickSelectsWords,
          ifFalse: "NoDoubleClickWordSelection"
        )
      );
      properties.Add(
        new FlagProperty(
          "tripleClickSelectsLine",
          tripleClickSelectsLine,
          ifFalse: "NoTripleClickLineSelection"
        )
      );
      properties.Add(
        new EnumProperty<LanguageDirection>(
          "languageDirection",
          languageDirection,
          LanguageDirection.Inherit
        )
      );
    }
  }

  public static class HTextExtensions {
    /// <summary>
    /// Applies the current theme's <b>body</b> font size at the specified level.
    /// </summary>
    /// <remarks>
    /// Resolves the sizes from <see cref="PrimitiveBaseTheme.Typography"/> with level 1 being linked to
    /// <see cref="PrimitiveTypographyScheme.FontSize3"/> and level 3 being the last.
    /// </remarks>
    public static HText Body(this HText text, IThemeProvider theme, int level = 1) {
      var typography = theme.GetThemed(PrimitiveBaseTheme.Typography);
      text.style ??= new TextStyle();
      text.style.fontSize = level switch {
        <= 1 => typography.FontSize3,
        2 => typography.FontSize4,
        _ => typography.FontSize5
      };
      return text;
    }

    /// <summary>
    /// Applies the current theme's <b>heading</b> font size at the specified level.
    /// </summary>
    /// <remarks>
    /// Resolves the sizes from <see cref="PrimitiveBaseTheme.Typography"/> with level 1 being linked to
    /// <see cref="PrimitiveTypographyScheme.FontSize6"/> and level 3 being the last.
    /// </remarks>
    public static HText Heading(this HText text, IThemeProvider theme, int level = 1) {
      var typography = theme.GetThemed(PrimitiveBaseTheme.Typography);
      text.style ??= new TextStyle();
      text.style.fontSize = level switch {
        <= 1 => typography.FontSize6,
        2 => typography.FontSize7,
        _ => typography.FontSize8
      };
      return text;
    }


    /// <summary>
    /// Applies the current theme's <b>caption</b> font size at the specified level.
    /// </summary>
    /// <remarks>
    /// Resolves the sizes from <see cref="PrimitiveBaseTheme.Typography"/> with level 1 being linked to
    /// <see cref="PrimitiveTypographyScheme.FontSize1"/> and level 2 being the last.
    /// </remarks>
    public static HText Caption(this HText text, IThemeProvider theme, int level = 1) {
      var typography = theme.GetThemed(PrimitiveBaseTheme.Typography);
      text.style ??= new TextStyle();
      text.style.fontSize = level switch {
        <= 1 => typography.FontSize1,
        _ => typography.FontSize2
      };
      return text;
    }

    /// <summary>
    /// Applies the current theme's <b>display</b> font size.
    /// </summary>
    /// <remarks>
    /// Resolves the sizes from <see cref="PrimitiveBaseTheme.Typography"/> being linked to
    /// <see cref="PrimitiveTypographyScheme.FontSize9"/>.
    /// </remarks>
    public static HText Display(this HText text, IThemeProvider theme) {
      var typography = theme.GetThemed(PrimitiveBaseTheme.Typography);
      text.style ??= new TextStyle();
      text.style.fontSize = typography.FontSize9;
      return text;
    }
  }
}