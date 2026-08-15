using System;

namespace HELIX.Context {
  [AttributeUsage(AttributeTargets.Method)]
  [RequireMixin(typeof(IEventHandlersMixin), declareImplicit: true)]
  [MixinExpression(
    new[] { MixinOn.Init, MixinOn.ConfigureRegistration },
    new[] { 1, -90_000 },
    @"
@SCOPE
  @MATCH @var#IsComponent:?eq<true>
  @CODE<^*~HELIX.Context.RegistrationConfigurator> registration.RegisterHandlerBinding<@arg#0:type>(@attr#priority)

@SCOPE
  @MATCH @arg#0:?is<global::HELIX.Context.IAsyncChainEvt>
  @ASSERT @arg#0:?argument
  @CODE<$Init> eventHandlerList.RegisterAsync<@arg#0:type>(@target, @attr#priority);
  @RETURN
@SCOPE
  @MATCH @arg#0:?is<global::HELIX.Context.Evt>
  @MATCH @arg#0:?ref
  @CODE<$Init> eventHandlerList.Register<@arg#0:type>(@target, @attr#priority);
  @RETURN
@SCOPE
  @MATCH @arg#0:?is<global::HELIX.Context.Evt>
  @MATCH @arg#0:?argument
  @CODE<$Init> eventHandlerList.Register<@arg#0:type>(@target, @attr#priority);
  @RETURN
"
  )]
  public class EventHandlerAttribute : Attribute {
    public readonly int priority;

    public EventHandlerAttribute(int priority = EventPriority.Normal) {
      this.priority = priority;
    }
  }

  [MixinExpression(
    new[] { MixinOn.Dispose },
    new[] { 1 },
    @"
@CODE<CLASS> [global::System.NonSerializedAttribute]
@CODE<CLASS> protected readonly global::HELIX.Context.EventHandlerList eventHandlerList = global::HELIX.Context.EventHandlerList.Create();

@CODE<$Dispose> eventHandlerList.UnregisterAll();
"
  )]
  [Mixin] public interface IEventHandlersMixin : IMixin { }
}