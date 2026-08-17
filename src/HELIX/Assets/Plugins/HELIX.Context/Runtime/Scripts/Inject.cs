using System;

namespace HELIX.Context {
  // TODO

  [MixinExpression(
    new[] { MixinOn.Init, MixinOn.Dispose, MixinOn.ConfigureComponent },
    new[] { -1, 1, 0 },
    @"
@SCOPE<SwitchIsList>
  @MATCH @target:type:?is<System.Collections.IEnumerable>
  @ASSERT @target:type#0:?exists
  @GOTO<Lists>
@END

@SCOPE<Single>
@LOCAL<WireKey> @@""@target:type:unwrap|@attr#source|@attr#qualifier:unwrap""
@SCOPE<Container>
  @MATCH @attr#source:?eq<0>
  @CODE<$ConfigureComponent> registration.Dependency(typeof(@target:type), @attr#qualifier);
  @CODE<$Init> @target:name = RuntimeComponentData.Resolve<@target:type>(@attr#qualifier);
  @RETURN
@SCOPE<Addressables>
  @MATCH @attr#source:?eq<2>
  @CODE<$ConfigureComponent> registration.Dependency(new ComponentDependency(new AddressableDependency<@target:type>(@attr#qualifier, @local#WireKey), true));
  @CODE<$Init> @target:name = RuntimeComponentData.Resolve<@target:type>(@local#WireKey);
  @RETURN
@SCOPE<Resources>
  @MATCH @attr#source:?eq<3>
  @CODE<$ConfigureComponent> registration.Dependency(new ComponentDependency(new ResourceDependency<@target:type>(@attr#qualifier, @local#WireKey), true));
  @CODE<$Init> @target:name = RuntimeComponentData.Resolve<@target:type>(@local#WireKey);
  @RETURN
@FAIL

@SCOPE<Lists>
@USING System.Collections.Generic
@USING System.Collections
@LOCAL<WireKey> @@""list|@target:type#0:unwrap|@attr#source|@attr#qualifier:unwrap""
@SCOPE<AddressableList>
  @MATCH @attr#source:?eq<2>
  @CODE<$ConfigureComponent> registration.Dependency(new ComponentDependency(new AddressableListDependency<@target:type#0>(@attr#qualifier, @local#WireKey), true));
  @CODE<$Init> @target:name = RuntimeComponentData.Resolve<List<@target:type#0>>(@local#WireKey) as @target:type;
  @RETURN
@SCOPE<ResourceList>
  @MATCH @attr#source:?eq<3>
  @CODE<$ConfigureComponent> registration.Dependency(new ComponentDependency(new ResourceListDependency<@target:type#0>(@attr#qualifier, @local#WireKey), true));
  @CODE<$Init> @target:name = RuntimeComponentData.Resolve<List<@target:type#0>>(@local#WireKey) as @target:type;
  @RETURN
"
  )]
  [RequireMixin(typeof(IEventHandlersMixin), true)]
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