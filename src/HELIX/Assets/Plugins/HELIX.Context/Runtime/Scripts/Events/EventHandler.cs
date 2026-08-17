using System;

namespace HELIX.Context {
  [AttributeUsage(AttributeTargets.Method)]
  // [RequireMixin(typeof(IEventHandlersMixin), true)]
  [MixinExpression(
    new[] { MixinOn.Init, MixinOn.ConfigureComponent },
    new[] { 1, -90_000 },
    @"
@CALL<RequireEventHandler>

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

//   [MixinExpression(
//     new[] { MixinOn.Dispose },
//     new[] { 1 },
//     @"
// @CALL<RequireEventHandler>
// "
//   )]
//   [Mixin] public interface IEventHandlersMixin : IMixin { }
}