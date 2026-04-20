using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace HELIX.Widgets.Utilities {
  public class ReferenceEqualityComparer<T> : IEqualityComparer<T> where T : class {
    bool IEqualityComparer<T>.Equals(T x, T y) {
      return ReferenceEquals(x, y);
    }

    int IEqualityComparer<T>.GetHashCode(T obj) {
      return RuntimeHelpers.GetHashCode(obj);
    }
  }
}