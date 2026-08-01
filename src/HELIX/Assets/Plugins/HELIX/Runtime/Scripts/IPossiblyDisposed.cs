using System;
using UnityEngine;

namespace HELIX {
  public interface IPossiblyDisposed {
    bool IsDisposed { get; }
  }

  public static class DisposableExtensions {
    public static bool DisposeSafe<T>(this T disposable) where T : IDisposable {
      if (disposable == null) return false;
      try {
        disposable.Dispose();
        return true;
      } catch (Exception ex) {
        Debug.LogException(ex);
        return false;
      }
    }
  }
}