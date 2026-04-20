using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Properties;
using HELIX.Widgets.Modifiers;
using HELIX.Widgets.Theming;
using HELIX.Widgets.Universal.Styles;
using HELIX.Widgets.Universal.Theme;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Universal {
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

    public HText(
      string text,
      bool enableRichText = false,
      bool emojiFallbackSupport = true,
      bool parseEscapeSequences = true,
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
    public static HText Body(this HText text, IThemeProvider theme, int level = 1) {
      var typography = theme.GetThemed(PrimitiveBaseTheme.Typography);
      text.style ??= new TextStyle();
      text.style.fontSize = level switch {
        1 => typography.FontSize3,
        2 => typography.FontSize4,
        3 => typography.FontSize5,
        _ => typography.FontSize3
      };
      return text;
    }

    public static HText Heading(this HText text, IThemeProvider theme, int level = 1) {
      var typography = theme.GetThemed(PrimitiveBaseTheme.Typography);
      text.style ??= new TextStyle();
      text.style.fontSize = level switch {
        1 => typography.FontSize6,
        2 => typography.FontSize7,
        3 => typography.FontSize8,
        _ => typography.FontSize6
      };
      return text;
    }

    public static HText Caption(this HText text, IThemeProvider theme, int level = 1) {
      var typography = theme.GetThemed(PrimitiveBaseTheme.Typography);
      text.style ??= new TextStyle();
      text.style.fontSize = level switch {
        1 => typography.FontSize1,
        2 => typography.FontSize2,
        _ => typography.FontSize1
      };
      return text;
    }

    public static HText Display(this HText text, IThemeProvider theme, int level = 1) {
      var typography = theme.GetThemed(PrimitiveBaseTheme.Typography);
      text.style ??= new TextStyle();
      text.style.fontSize = level switch {
        1 => typography.FontSize9,
        2 => typography.FontSize8,
        3 => typography.FontSize7,
        _ => typography.FontSize9
      };
      return text;
    }
  }
}