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

  /// <summary>Publishes a component field or property after its normal initialization has completed.</summary>
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  [MixinExpression(
    MixinOn.ConfigureComponent,
    0,
    @"
@USING HELIX.Context;
@ASSERT @target:type:?class
@CODE registration.Publication(new ComponentDependency(new TypeKey(typeof(@target:type), @attr#qualifier), @attr#required));
"
  )]
  [MixinExpression(
    MixinOn.ComponentLoadLate,
    0,
    @"
@USING HELIX.Context;
@SCOPE
  @MATCH @attr#proxied:?eq<true>
  @CODE context.PublishProxy(new TypeKey(typeof(@target:type), @attr#qualifier), () => @target:name);
  @RETURN
@END
@SCOPE
  @MATCH @attr#required:?eq<false>
  @CODE if (@target:name != null) context.Publish(@target:name, typeof(@target:type), @attr#qualifier);
  @RETURN
@END
@CODE context.Publish(@target:name, typeof(@target:type), @attr#qualifier);
"
  )]
  public class BindAttribute : Attribute {
    public BindAttribute(string qualifier = null, bool required = true, bool proxied = false) { }
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
