using System;
using Unity.Mathematics;
using UnityEngine.UIElements;

namespace HELIX.Types {
  public struct StyleInt4 : IStyleValue<int4>, IEquatable<StyleInt4> {
    private int4 _value;

    public int4 value {
      readonly get => _value;
      set => _value = value;
    } //LTRB

    public StyleKeyword keyword { get; set; }

    public StyleInt4(int4 value) : this() {
      _value = value;
      keyword = StyleKeyword.Undefined;
    }

    public StyleInt4(StyleKeyword keyword) : this() {
      this.keyword = keyword;
    }

    public StyleInt L => keyword == StyleKeyword.Undefined ? new StyleInt(_value.x) : new StyleInt(keyword);
    public StyleInt T => keyword == StyleKeyword.Undefined ? new StyleInt(_value.y) : new StyleInt(keyword);
    public StyleInt R => keyword == StyleKeyword.Undefined ? new StyleInt(_value.z) : new StyleInt(keyword);
    public StyleInt B => keyword == StyleKeyword.Undefined ? new StyleInt(_value.w) : new StyleInt(keyword);

    public bool Equals(StyleInt4 other) {
      return _value.Equals(other._value) && keyword == other.keyword;
    }

    public override string ToString() {
      return HelixFormattingHelper.FormatStyleValue(keyword, HelixFormattingHelper.BuildQuadruple("Int4", L, T, R, B));
    }

    public static implicit operator StyleInt4(StyleKeyword k) {
      return new StyleInt4 {
        _value = default,
        keyword = k
      };
    }

    public static implicit operator StyleInt4(int4 v) {
      return new StyleInt4 {
        _value = v,
        keyword = StyleKeyword.Undefined
      };
    }
  }
}