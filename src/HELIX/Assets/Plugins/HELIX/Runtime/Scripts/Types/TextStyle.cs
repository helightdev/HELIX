using System;
using HELIX.NW;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Types {
  public struct TextStyle : IEquatable<TextStyle> {
    private static readonly TextStyle _fallback = new() {
      align = TextAnchor.MiddleLeft,
      color = Color.black,
      size = 14,
      letterSpacing = StyleKeyword.Null,
      style = FontStyle.Normal,
      wrap = WhiteSpace.Normal,
      overflow = TextOverflow.Clip,
      font = StyleKeyword.Null
    };

    private static readonly TextStyle _null = new() {
      align = StyleKeyword.Null,
      color = StyleKeyword.Null,
      size = StyleKeyword.Null,
      letterSpacing = StyleKeyword.Null,
      style = StyleKeyword.Null,
      wrap = StyleKeyword.Null,
      overflow = StyleKeyword.Null,
      font = StyleKeyword.Null
    };

    public static readonly ContextKey<TextStyle> Context = new("TextStyle", _fallback);

    public StyleEnum<TextAnchor> align;
    public StyleColor color;
    public StyleLength size;
    public StyleLength letterSpacing;
    public StyleEnum<FontStyle> style;
    public StyleEnum<WhiteSpace> wrap;
    public StyleEnum<TextOverflow> overflow;
    public StyleFontDefinition font;

    public TextStyle(
      StyleEnum<TextAnchor>? align = null,
      StyleColor? color = null,
      StyleLength? size = null,
      StyleLength? letterSpacing = null,
      StyleEnum<FontStyle>? style = null,
      StyleEnum<WhiteSpace>? wrap = null,
      StyleEnum<TextOverflow>? overflow = null,
      StyleFontDefinition? font = null
    ) {
      this.align = align ?? StyleKeyword.Null;
      this.color = color ?? StyleKeyword.Null;
      this.size = size ?? StyleKeyword.Null;
      this.letterSpacing = letterSpacing ?? StyleKeyword.Null;
      this.style = style ?? StyleKeyword.Null;
      this.wrap = wrap ?? StyleKeyword.Null;
      this.overflow = overflow ?? StyleKeyword.Null;
      this.font = font ?? StyleKeyword.Null;
    }

    public readonly void Apply(IComposable composable) {
      var element = composable.Element;
      composable.Flag |= UssFlag.Text;

      element.style.unityTextAlign = align;
      element.style.color = color;
      element.style.fontSize = size;
      element.style.letterSpacing = letterSpacing;
      element.style.unityFontStyleAndWeight = style;
      element.style.whiteSpace = wrap;
      element.style.textOverflow = overflow;
      element.style.unityFontDefinition = font;
    }

    public void Merge(TextStyle overrides) {
      if (overrides.align.keyword != StyleKeyword.Null) align = overrides.align;
      if (overrides.color.keyword != StyleKeyword.Null) color = overrides.color;
      if (overrides.size.keyword != StyleKeyword.Null) size = overrides.size;
      if (overrides.letterSpacing.keyword != StyleKeyword.Null) letterSpacing = overrides.letterSpacing;
      if (overrides.style.keyword != StyleKeyword.Null) style = overrides.style;
      if (overrides.wrap.keyword != StyleKeyword.Null) wrap = overrides.wrap;
      if (overrides.overflow.keyword != StyleKeyword.Null) overflow = overrides.overflow;
      if (overrides.font.keyword != StyleKeyword.Null) font = overrides.font;
    }

    public readonly bool Equals(TextStyle other) {
      return align.Equals(other.align) && color.Equals(other.color) && size.Equals(other.size) &&
             letterSpacing.Equals(other.letterSpacing) && style.Equals(other.style) &&
             wrap.Equals(other.wrap) && overflow.Equals(other.overflow) && font.Equals(other.font);
    }

    public readonly override bool Equals(object obj) {
      return obj is TextStyle other && Equals(other);
    }

    public readonly override int GetHashCode() {
      return HashCode.Combine(align, color, size, letterSpacing, style, wrap, overflow, font);
    }

    public static void Merge(
      ref TextStyle target,
      in TextStyle basis,
      in TextStyle overrides
    ) {
      target.align = overrides.align.keyword != StyleKeyword.Null ? overrides.align : basis.align;
      target.color = overrides.color.keyword != StyleKeyword.Null ? overrides.color : basis.color;
      target.size = overrides.size.keyword != StyleKeyword.Null ? overrides.size : basis.size;
      target.letterSpacing = overrides.letterSpacing.keyword != StyleKeyword.Null
        ? overrides.letterSpacing
        : basis.letterSpacing;
      target.style = overrides.style.keyword != StyleKeyword.Null ? overrides.style : basis.style;
      target.wrap = overrides.wrap.keyword != StyleKeyword.Null ? overrides.wrap : basis.wrap;
      target.overflow = overrides.overflow.keyword != StyleKeyword.Null ? overrides.overflow : basis.overflow;
      target.font = overrides.font.keyword != StyleKeyword.Null ? overrides.font : basis.font;
    }

    public static ref TextStyle WriteMerged(
      ref Composition cx,
      in TextStyle overrides
    ) {
      var hasBasis = RecompositionScope.TryGetContext(Context, out var basis);
      cx.WritableContext(Context, out var data);
      ref var target = ref data.GetValueRef();
      data.IncrementContextVersion(ContextFlags.None);

      if (hasBasis) {
        Merge(ref target, in basis.GetValueRef(), in overrides);
      } else {
        Merge(ref target, in _fallback, in overrides);
      }
      return ref target;
    }

    public static ref TextStyle WriteMerged(
      ref Composition cx,
      StateProperty<TextStyle> property,
      StateFlag flag
    ) {
      if (property.HasValueFor(flag)) {
        ref var overrides = ref property.GetValueRef(flag);
        return ref WriteMerged(ref cx, in overrides);
      } else {
        return ref WriteMerged(ref cx, in _null);
      }
    }
  }
}