using System;

namespace HELIX.Context {
  // TODO

  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  [MixinExpression(
    new[] { MixinOn.Init, MixinOn.Dispose, MixinOn.ConfigureComponent },
    new[] { -1, 1, 0 },
    @"
@SCOPE<Container>
  @MATCH @attr#source:?eq<0>
  @CODE<$ConfigureComponent> registration.Dependency(typeof(@target:type), @attr#qualifier);
  @CODE<$Init> @target:name = RuntimeComponentData.Resolve<@target:type>(@attr#qualifier);
"
  )]
  [RequireMixin(typeof(IEventHandlersMixin), true)]
  public class InjectAttribute : Attribute {
    public readonly Source source;
    public readonly string qualifier;

    public InjectAttribute(Source source = Source.Container, string qualifier = null) {
      this.source = source;
      this.qualifier = qualifier;
    }
  }

  public enum Source { Container = 0, Components = 1, Addressables = 2, Resources = 3 }
}