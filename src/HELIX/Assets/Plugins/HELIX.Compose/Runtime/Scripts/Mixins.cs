using System;

namespace HELIX.Compose {

  public interface IRecomposeMixinTargets {
    void MixinInit() { }
    void MixinDispose() { }
    void MixinReset() { }
    void MixinRecompose(ref Composition cx) { }
  }

  public interface IBoundaryHooks {
    void MixinPostConstruct() { }
    void MixinRecompose(ref Composition cx) { }
    void MixinCompose(ref Composition cx) { }
    void MixinInit() { }
    void MixinReset() { }
    void MixinDispose() { }
  }

  public static class RecomposeMixinTargets {
    public const string Init = "^MixinInit";
    public const string Dispose = "^MixinDispose";
    public const string Reset = "^MixinReset";
    public const string Recompose = "^MixinRecompose:HELIX.Compose.Composable";
  }

  [AttributeUsage(AttributeTargets.Method)]
  public sealed class ComposableMethodAttribute : Attribute {
    public ComposableMethodAttribute(
      bool requiresTracking = false,
      bool scope = false,
      bool extension = false,
      string name = null,
      string scopeCallback = "cell.TrimChildren()"
    ) { }
  }

  [AttributeUsage(AttributeTargets.Method)]
  public sealed class ComposableDelegateAttribute : Attribute {
    public ComposableDelegateAttribute(
      string name = null
    ) { }
  }

  [AttributeUsage(AttributeTargets.Class)]
  public class CustomBoundaryElementAttribute : Attribute {
    public CustomBoundaryElementAttribute(
      bool constructor = true,
      bool composable = true,
      bool extension = false,
      bool cacheLookups = false,
      bool trimChildren = true,
      string name = null
    ) { }
  }


  [AttributeUsage(AttributeTargets.Class)]
  public class BoundaryElementMixinAttribute : Attribute {
    public BoundaryElementMixinAttribute(
      bool composable = true,
      bool extension = false,
      bool cacheLookups = false,
      bool trimChildren = true,
      string name = null
    ) { }
  }

  public class WriteContextHandlerAttribute : Attribute { }
}
