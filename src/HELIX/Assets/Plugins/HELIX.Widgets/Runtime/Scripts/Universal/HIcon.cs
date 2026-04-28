using System.Collections.Generic;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Modifiers;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Universal {
  /// <summary>
  /// A wrapper widget around <see cref="Label"/> that is optimized for displaying font based icons.
  /// </summary>
  public class HIcon : WrappingBaseWidget<HIcon, Label> {
    private readonly ModifierSet _defaultModifiers = new ModifierSet {
      ModifierFallbacks.PaddingZero,
      ModifierFallbacks.MarginZero
    }.Sealed();

    public StyleFontDefinition font;
    public StyleLength fontSize = StyleKeyword.Null;
    public StyleColor color = StyleKeyword.Null;
    public StyleEnum<TextAnchor> align = StyleKeyword.Null;

    public readonly string text;

    /// <summary>
    /// Creates a wrapper widget around <see cref="Label"/> that is optimized for displaying font based icons.
    /// </summary>
    /// <param name="text">The textual content to display.</param>
    /// <param name="font">The font to use for the icon.</param>
    /// <param name="size">The font size to use for the icon.</param>
    /// <param name="color">The color to use for the icon.</param>
    /// <param name="align">The alignment to use for the icon.</param>
    /// <param name="key">Passed on to <see cref="Widget.key"/>.</param>
    /// <param name="constants">Passed on to <see cref="Widget.constants"/>.</param>
    /// <param name="modifiers">Passed on to <see cref="Widget.modifiers"/>.</param>
    public HIcon(
      string text,
      FontDefinition font,
      StyleLength? size = null,
      StyleColor? color = null,
      TextAnchor? align = null,
      Key key = default,
      object[] constants = null,
      IReadOnlyCollection<Modifier> modifiers = null
    ) : base(key, constants) {
      this.text = text;
      this.font = font;
      if (size != null) this.fontSize = size.Value;
      if (color != null) this.color = color.Value;
      if (align != null) this.align = align.Value;

      DefaultModifiers(_defaultModifiers, modifiers);
    }

    /// <summary>
    /// Creates a wrapper widget around <see cref="Label"/> that is optimized for displaying font based icons.
    /// </summary>
    /// <param name="icon">The Unicode character of the icon.</param>
    /// <param name="font">The font to use for the icon.</param>
    /// <param name="size">The font size to use for the icon.</param>
    /// <param name="color">The color to use for the icon.</param>
    /// <param name="align">The alignment to use for the icon.</param>
    /// <param name="key">Passed on to <see cref="Widget.key"/>.</param>
    /// <param name="constants">Passed on to <see cref="Widget.constants"/>.</param>
    /// <param name="modifiers">Passed on to <see cref="Widget.modifiers"/>.</param>
    public HIcon(
      char icon,
      FontDefinition font,
      StyleLength? size = null,
      StyleColor? color = null,
      TextAnchor? align = null,
      Key key = default,
      object[] constants = null,
      IReadOnlyCollection<Modifier> modifiers = null
    ) : base(key, constants) {
      text = icon.ToString();
      this.font = font;
      if (size != null) this.fontSize = size.Value;
      if (color != null) this.color = color.Value;
      if (align != null) this.align = align.Value;

      DefaultModifiers(_defaultModifiers, modifiers);
    }

    public override Label Create() {
      return new Label();
    }

    public override void Apply(HIcon previous, Label element) {
      if (text != element.text) element.text = text;
      element.style.unityFontDefinition = font;
      element.style.fontSize = fontSize;
      element.style.color = color;
      element.style.unityTextAlign = align;
    }

    public override void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
      base.DebugFillProperties(properties);
      properties.Add(new DiagnosticsProperty<string>("text", text));
      properties.Add(new DiagnosticsProperty<StyleFontDefinition>("font", font));
      properties.Add(new DiagnosticsProperty<StyleColor>("color", color));
      properties.Add(new DiagnosticsProperty<StyleLength>("fontSize", fontSize));
      properties.Add(new DiagnosticsProperty<StyleEnum<TextAnchor>>("align", align));
    }
  }
}