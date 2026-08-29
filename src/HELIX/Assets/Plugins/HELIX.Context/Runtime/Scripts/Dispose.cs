using System;

namespace HELIX.Context {
  [AttributeUsage(AttributeTargets.Field)]
  public class AutoDisposeAttribute : Attribute { }

  public static class AutoDisposeMethods {
    public static void DisposeDisposable<T>(ref T target) where T : IDisposable {
      if (target == null) return;
      target.Dispose();
      target = default;
    }
  }
}
