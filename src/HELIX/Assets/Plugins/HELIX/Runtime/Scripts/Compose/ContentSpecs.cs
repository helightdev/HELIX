using System;
using HELIX.Types;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;
using TextStyle = HELIX.Types.TextStyle;

namespace HELIX.Compose {
  public readonly struct ChevronSpec : ISpec {
    public readonly ArrowPosition position;
    public readonly float size;
    public readonly Color color;

    public ChevronSpec(ArrowPosition position, float size, Color color) {
      this.position = position;
      this.size = size;
      this.color = color;
    }

    public static void Default(ref Composition cx, in ChevronSpec spec) {
      switch (spec.position) {
        case ArrowPosition.Up:
          cx.Text("\u25B2").TextSize(spec.size).TextColor(spec.color);
          break;
        case ArrowPosition.Down:
          cx.Text("\u25BC").TextSize(spec.size).TextColor(spec.color);
          break;
        case ArrowPosition.Left:
          cx.Text("\u25C0").TextSize(spec.size).TextColor(spec.color);
          break;
        case ArrowPosition.Right:
          cx.Text("\u25B6").TextSize(spec.size).TextColor(spec.color);
          break;
        default: throw new ArgumentOutOfRangeException();
      }
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

    public static void Default(ref Composition cx, in IconRef spec) {
      if (spec.font != null) {
        cx.Text(spec.icon).TextFont(spec.font);
      } else {
        cx.Text(spec.icon).TextFont(new StyleFontDefinition(spec.fontAsset));
      }
    }
  }

  public readonly struct LabelSpec : ISpec {
    public static readonly ThemeProperty<Length> Gap = new(data => data[SpacingRole.Spacing1]);

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
      Default(ref cx, in spec, Gap.ReadScope());
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
        iconStyle.Apply(cx.APPLY.composable);
      }
      if (spec.HasIconAndText) cx.Gap(gap);

      if (!spec.HasText) return;
      if (spec.textContent != null) spec.textContent(ref cx);
      else cx.Text(spec.text);
      textStyle.Apply(cx.APPLY.composable);
    }
  }

  public readonly struct PrefixLabelSuffixSpec : ISpec {
    public readonly LabelSpec? prefix;
    public readonly LabelSpec? label;
    public readonly LabelSpec? suffix;
    public readonly float gap;
    public readonly Align alignment;

    public bool IsEmpty => !prefix.HasValue && !label.HasValue && !suffix.HasValue;

    public PrefixLabelSuffixSpec(
      string text,
      LabelSpec? prefix = null,
      LabelSpec? suffix = null,
      float gap = 4f,
      Align alignment = Align.Center
    ) {
      this.prefix = prefix;
      label = new LabelSpec(text);
      this.suffix = suffix;
      this.gap = gap;
      this.alignment = alignment;
    }

    public PrefixLabelSuffixSpec(
      LabelSpec label,
      LabelSpec? prefix = null,
      LabelSpec? suffix = null,
      float gap = 4f,
      Align alignment = Align.Center
    ) {
      this.prefix = prefix;
      this.label = label;
      this.suffix = suffix;
      this.gap = gap;
      this.alignment = alignment;
    }
  }

  public static class ContentSpecDefinitions {
    public static void Label(ref this Composition cx, in LabelSpec content) {
      using (cx.Flex(Axis.Horizontal, cross: Align.Center)) {
        content.icon?.Invoke(ref cx);
        if (content.textContent != null) content.textContent.Invoke(ref cx);
        else if (content.text != null) cx.Text(content.text);
      }
    }

    public static void PrefixLabelSuffix(
      ref this Composition cx,
      in PrefixLabelSuffixSpec content
    ) {
      using (cx.Flex(Axis.Horizontal, cross: content.alignment)) {
        var hadPrevious = false;
        if (content.prefix.HasValue) {
          var prefix = content.prefix.Value;
          cx.Label(in prefix);
          hadPrevious = true;
        }
        if (content.label.HasValue) {
          if (hadPrevious) cx.Gap(content.gap);
          var label = content.label.Value;
          cx.Label(in label);
          hadPrevious = true;
        }
        if (content.suffix.HasValue) {
          if (hadPrevious) cx.Gap(content.gap);
          var suffix = content.suffix.Value;
          cx.Label(in suffix);
        }
      }
    }
  }
}