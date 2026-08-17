using System;

namespace HELIX.Context {
  // TODO

  [MixinExpression(
    new[] { MixinOn.Init, MixinOn.Dispose, MixinOn.ConfigureComponent },
    new[] { -1, 1, 0 },
    "@CALL<InjectImpl>"
  )]
  public abstract class InjectAttributeBase : Attribute {
    public readonly Source source;
    public readonly string qualifier;

    protected InjectAttributeBase(Source source, string qualifier) {
      this.source = source;
      this.qualifier = qualifier;
    }
  }

  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class InjectAttribute : InjectAttributeBase {
    public InjectAttribute(Source source = Source.Container, string qualifier = null) : base(source, qualifier) { }
  }

  public enum Source { Container = 0, Components = 1, Addressables = 2, Resources = 3 }
}