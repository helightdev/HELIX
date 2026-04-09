using System;
using Unity.Mathematics;
using UnityEngine.UIElements;

namespace HELIX.Types {
    
    public struct StyleInt4 : IStyleValue<int4>, IEquatable<StyleInt4> {
        private int4 _value;
        private StyleKeyword _keyword;

        public int4 value {
            readonly get => _value;
            set => _value = value;
        } //LTRB

        public StyleKeyword keyword {
            readonly get => _keyword;
            set => _keyword = value;
        }

        public StyleInt4(int4 value) : this() {
            _value = value;
            _keyword = StyleKeyword.Undefined;
        }

        public StyleInt4(StyleKeyword keyword) : this() {
            _keyword = keyword;
        }

        public StyleInt L => _keyword == StyleKeyword.Undefined ? new StyleInt(_value.x) : new StyleInt(_keyword);
        public StyleInt T => _keyword == StyleKeyword.Undefined ? new StyleInt(_value.y) : new StyleInt(_keyword);
        public StyleInt R => _keyword == StyleKeyword.Undefined ? new StyleInt(_value.z) : new StyleInt(_keyword);
        public StyleInt B => _keyword == StyleKeyword.Undefined ? new StyleInt(_value.w) : new StyleInt(_keyword);

        public bool Equals(StyleInt4 other) {
            return _value.Equals(other._value) && _keyword == other._keyword;
        }

        public override string ToString() {
            return HelixFormattingHelper.FormatStyleValue(_keyword, HelixFormattingHelper.BuildQuadruple("Int4", L, T, R, B));
        }

        public static implicit operator StyleInt4(StyleKeyword k) {
            return new StyleInt4 {
                _value = default,
                _keyword = k
            };
        }

        public static implicit operator StyleInt4(int4 v) {
            return new StyleInt4 {
                _value = v,
                _keyword = StyleKeyword.Undefined
            };
        }
    }
}