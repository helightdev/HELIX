using System;

namespace HELIX.Context {
  [AttributeUsage(AttributeTargets.Field)]
  [AttributeMixinMethodProxy(typeof(AutoDisposeMethods), nameof(AutoDisposeMethods.DisposeDisposable))]
  public class AutoDisposeAttribute : Attribute { }

  public static class AutoDisposeMethods {
    [MixinMethod(MixinOn.Dispose)]
    public static void DisposeDisposable<T>(
      [MixinInject(MixinInject.Target)] ref T target
    ) where T : IDisposable {
      if (target == null) return;
      target.Dispose();
      target = default;
    }
  }
}