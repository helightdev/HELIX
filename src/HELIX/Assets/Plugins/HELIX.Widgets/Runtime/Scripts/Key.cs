using System;
using System.IO.Hashing;
using System.Text;

namespace HELIX.Widgets
{
    public readonly struct Key : IEquatable<Key> {
        private readonly ulong _value;
        public static readonly Key None = new Key(0);

        public Key(ulong value) {
            _value = value;
        }

        public bool IsNone => _value == 0;

        public bool Equals(Key other) => _value == other._value;
        public override bool Equals(object obj) => obj is Key other && Equals(other);
        public override int GetHashCode() => _value.GetHashCode();

        public static bool operator ==(Key a, Key b) => a._value == b._value;
        public static bool operator !=(Key a, Key b) => a._value != b._value;

        public override string ToString() => IsNone ? "None" : _value.ToString();

        public static implicit operator Key(ulong value) => new(value);
        public static implicit operator Key(uint value) => new(value);
        public static implicit operator Key(int value) => new(unchecked((uint)value));
        public static implicit operator Key(string value) => new(HashString(value));

        private static ulong HashString(string value) {
            var source = Encoding.UTF8.GetBytes(value);
            return XxHash3.HashToUInt64(source);
        }
    }
}