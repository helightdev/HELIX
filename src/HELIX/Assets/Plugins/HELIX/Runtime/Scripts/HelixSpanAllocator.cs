using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace HELIX {
  public static class HelixSpanAllocator {

    // Just to be sure, we put a smaller limit for mobile and console platforms.
#if UNITY_STANDALONE || UNITY_EDITOR
    private const int MaxStackBytes = 4096; // 4KB
#else
    private const int MaxStackBytes = 1024; // 1KB
#endif

    public static void Lease<T, R>(ReferenceSpanAction<T, R> action, int length, ref R reference)
      where T : unmanaged {
      var totalBytes = length * Unsafe.SizeOf<T>();
      if (totalBytes < MaxStackBytes) {
        Span<T> span = stackalloc T[length];
        action(span, ref reference);
      } else {
        var array = ArrayPool<T>.Shared.Rent(length);
        try {
          var span = array.AsSpan(0, length);
          action(span, ref reference);
        } finally {
          ArrayPool<T>.Shared.Return(array);
        }
      }
    }

    public static void Lease<T, V>(ValueSpanAction<T, V> action, int length, V value) where T : unmanaged {
      var totalBytes = length * Unsafe.SizeOf<T>();
      if (totalBytes < MaxStackBytes) {
        Span<T> span = stackalloc T[length];
        action(span, value);
      } else {
        var array = ArrayPool<T>.Shared.Rent(length);
        try {
          var span = array.AsSpan(0, length);
          action(span, value);
        } finally {
          ArrayPool<T>.Shared.Return(array);
        }
      }
    }
  }

  public delegate void ReferenceSpanAction<T, R>(Span<T> span, ref R reference) where T : unmanaged;

  public delegate void ValueSpanAction<T, in V>(Span<T> span, V value) where T : unmanaged;
}