using System;
using System.Collections.Generic;

namespace HELIX {
  public struct HXOptional<T> : IEquatable<HXOptional<T>> {
    public readonly T value;
    public readonly bool hasValue;

    public HXOptional(T value) : this() {
      this.value = value;
      this.hasValue = true;
    }

    public HXOptional(T value, bool hasValue) {
      this.value = value;
      this.hasValue = hasValue;
    }

    public static readonly HXOptional<T> None = new(default, false);
    public static implicit operator HXOptional<T>(T value) => new(value, true);

    public bool Equals(HXOptional<T> other) {
      return EqualityComparer<T>.Default.Equals(value, other.value) && hasValue == other.hasValue;
    }

    public override bool Equals(object obj) {
      return obj is HXOptional<T> other && Equals(other);
    }

    public override int GetHashCode() {
      return HashCode.Combine(value, hasValue);
    }
  }
}
