using System;
using HELIX.Types;
using UnityEngine.UIElements;

namespace HELIX.NW {
  public readonly struct LabelSpec : ISpec {
    public readonly Composable icon;
    public readonly Composable textContent;
    public readonly string text;

    public bool IsEmpty => icon == null && textContent == null && text == null;

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
          if (hadPrevious) cx.Space(content.gap);
          var label = content.label.Value;
          cx.Label(in label);
          hadPrevious = true;
        }
        if (content.suffix.HasValue) {
          if (hadPrevious) cx.Space(content.gap);
          var suffix = content.suffix.Value;
          cx.Label(in suffix);
        }
      }
    }
  }
}