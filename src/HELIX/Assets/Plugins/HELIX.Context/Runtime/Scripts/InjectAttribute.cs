using System;

namespace HELIX.Context {

  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  [MixinExpression(
    new[] { MixinOn.Init, MixinOn.Dispose, MixinOn.ConfigureComponent },
    new[] { -1, 1, 0 },
    "@CALL<InjectDiImpl>"
  )]
  public class InjectAttribute : Attribute {
    public InjectAttribute(string qualifier = null, bool required = true) { }
  }

  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  [MixinExpression(
    new[] { MixinOn.Init, MixinOn.Dispose, MixinOn.ConfigureComponent },
    new[] { -1, 1, 0 },
    "@CALL<ResourceImpl>"
  )]
  public class ResourceAttribute : Attribute {
    public ResourceAttribute(Source source, string qualifier = null) { }
  }

  public enum Source { Container = 0, Components = 1, Addressables = 2, Resources = 3 }
}