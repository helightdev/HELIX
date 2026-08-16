using System;

namespace HELIX.Context {
  [AttributeUsage(AttributeTargets.Method)]
  [RequireMixin(typeof(IEventHandlersMixin), declareImplicit: true)]
  [MixinExpression(
    new[] { MixinOn.Init, MixinOn.ConfigureComponent },
    new[] { 1, -90_000 },
    @"
@VAR<IsEventHandler> true

@SCOPE
  @MATCH @var#IsComponent:?eq<true>
  @CODE<$ConfigureComponent> registration.RegisterHandlerBinding<@arg#0:type>(@attr#priority)
@END

@USING HELIX.Context;
@SCOPE
  @MATCH @arg#0:?is<IAsyncChainEvt>
  @ASSERT @arg#0:?argument
  @CODE<$Init> eventHandlerList.RegisterAsync<@arg#0:type>(@target, @attr#priority);
  @RETURN
@SCOPE
  @MATCH @arg#0:?is<Evt>
  @MATCH @arg#0:?ref
  @CODE<$Init> eventHandlerList.Register<@arg#0:type>(@target, @attr#priority);
  @RETURN
@SCOPE
  @MATCH @arg#0:?is<Evt>
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
@USING HELIX.Context;
@USING System;
@CODE<CLASS> [NonSerializedAttribute]
@CODE<CLASS> protected readonly EventHandlerList eventHandlerList = EventHandlerList.Create();
@CODE<CLASS> EventHandlerList IEventListener.HandlerList => eventHandlerList;

@CODE<$Dispose> eventHandlerList.UnregisterAll();
@CODE<IMPLEMENTS> IEventListener
"
  )]
  [Mixin] public interface IEventHandlersMixin : IMixin { }
}