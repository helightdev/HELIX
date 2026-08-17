using System;

namespace HELIX.Context {
  [AttributeUsage(AttributeTargets.Field)]
  [MixinExpression(MixinOn.Dispose, 1, @"
@USING HELIX.Context
@CODE AutoDisposeMethods.DisposeDisposable(ref @target)
")]
  public class AutoDisposeAttribute : Attribute { }

  public static class AutoDisposeMethods {
    public static void DisposeDisposable<T>(ref T target) where T : IDisposable {
      if (target == null) return;
      target.Dispose();
      target = default;
    }
  }
}