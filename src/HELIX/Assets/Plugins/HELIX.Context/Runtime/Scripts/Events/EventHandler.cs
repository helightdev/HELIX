using System;

namespace HELIX.Context {
  [AttributeUsage(AttributeTargets.Method)]
  // [RequireMixin(typeof(IEventHandlersMixin), true)]
  [MixinImport(typeof(ContextMixinLibrary))]
  [MixinExpression(
    new[] { MixinOn.Init, MixinOn.ConfigureManaged },
    new[] { 1, -90_000 },
    "@CALL<EventHandlerImpl>"
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
