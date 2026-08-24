using System;
using HELIX.Theming;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;
using TextStyle = HELIX.Types.TextStyle;

namespace HELIX.Compose {
  public readonly struct ChevronSpec : ISpec {
    public readonly ArrowPosition position;
    public readonly StyleLength size;
    public readonly StyleColor? color;

    public ChevronSpec(ArrowPosition position, StyleLength size, StyleColor? color = null) {
      this.position = position;
      this.size = size;
      this.color = color;
    }

    public static void Default(ref Composition cx, in ChevronSpec spec) {
      ref var text = ref cx.CURSOR;
      switch (spec.position) {
        case ArrowPosition.Up: text = ref cx.Text("\u25B2"); break;
        case ArrowPosition.Down: text = ref cx.Text("\u25BC"); break;
        case ArrowPosition.Left: text = ref cx.Text("\u25C4"); break;
        case ArrowPosition.Right: text = ref cx.Text("\u25BA"); break;
        default: throw new ArgumentOutOfRangeException();
      }
      text.TextSize(spec.size);
      if (spec.color.HasValue) text.TextColor(spec.color.Value);
    }
  }

  public enum ArrowPosition { Up, Down, Left, Right }

  public readonly struct IconRef : ISpec {
    public readonly string icon;
    public readonly Font font;
    public readonly FontAsset fontAsset;

    public IconRef(string icon, Font font) {
      this.icon = icon;
      this.font = font;
      fontAsset = null;
    }

    public IconRef(string icon, FontAsset fontAsset) {
      this.icon = icon;
      this.fontAsset = fontAsset;
      font = null;
    }

    public IconRef(char icon, Font font) : this(icon.ToString(), font) { }
    public IconRef(char icon, FontAsset fontAsset) : this(icon.ToString(), fontAsset) { }

    public static void Default(ref Composition cx, in IconRef spec) {
      if (spec.font != null) {
        cx.Text(spec.icon).TextFont(spec.font);
      } else {
        cx.Text(spec.icon).TextFont(new StyleFontDefinition(spec.fontAsset));
      }
    }
  }

  public readonly struct LabelSpec : ISpec {
    public readonly Composable icon;
    public readonly Composable textContent;
    public readonly string text;

    public bool IsEmpty => icon == null && textContent == null && text == null;
    public bool HasIcon => icon != null;
    public bool HasText => textContent != null || text != null;
    public bool HasIconAndText => HasIcon && HasText;

    public LabelSpec(
      string text,
      Composable icon = null
    ) {
      this.icon = icon;
      textContent = null;
      this.text = text;
    }

    public LabelSpec(
      Composable textContent,
      Composable icon = null
    ) {
      this.icon = icon;
      this.textContent = textContent;
      text = null;
    }

    public void ComposeText(ref Composition cx) {
      if (textContent != null) textContent(ref cx);
      else cx.Text(text);
    }

    public void ComposeIcon(ref Composition cx) {
      if (icon != null) icon(ref cx);
    }


    public static void Default(ref Composition cx, in LabelSpec spec) {
      Default(ref cx, in spec, ThemeProperties.TextGap[in cx]);
    }

    public static void Default(ref Composition cx, in LabelSpec spec, Length gap) {
      if (spec.HasIcon) spec.icon(ref cx);
      if (spec.HasIconAndText) cx.Gap(gap);

      if (!spec.HasText) return;
      if (spec.textContent != null) spec.textContent(ref cx);
      else cx.Text(spec.text);
    }

    public static void Default(
      ref Composition cx,
      in LabelSpec spec,
      Length gap,
      in TextStyle iconStyle,
      in TextStyle textStyle
    ) {
      if (spec.HasIcon) {
        spec.icon(ref cx);
        iconStyle.Apply(cx.CURSOR.composable);
      }
      if (spec.HasIconAndText) cx.Gap(gap);

      if (!spec.HasText) return;
      if (spec.textContent != null) spec.textContent(ref cx);
      else cx.Text(spec.text);
      textStyle.Apply(cx.CURSOR.composable);
    }
  }
}
