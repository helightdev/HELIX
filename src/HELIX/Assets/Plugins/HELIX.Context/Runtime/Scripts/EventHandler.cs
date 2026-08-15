using System;
using HELIX.Context.Events;

namespace HELIX.Context {
  [AttributeUsage(AttributeTargets.Method)]
  [RequireMixin(typeof(IEventHandlersMixin), declareImplicit: true)]
  [MixinExpression(
    MixinOn.Init,
    1,
    @"
@SCOPE
  @MATCH @arg#0:?is<global::HELIX.Context.Events.IAsyncChainEvt>
  @ASSERT @arg#0:?argument
  @CODE<$Init> eventHandlerList.RegisterAsync<@arg#0:type>(@target, @attr#priority);
  @RETURN
@SCOPE
  @MATCH @arg#0:?is<global::HELIX.Context.Events.Evt>
  @MATCH @arg#0:?ref
  @CODE<$Init> eventHandlerList.Register<@arg#0:type>(@target, @attr#priority);
  @RETURN
@SCOPE
  @MATCH @arg#0:?is<global::HELIX.Context.Events.Evt>
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