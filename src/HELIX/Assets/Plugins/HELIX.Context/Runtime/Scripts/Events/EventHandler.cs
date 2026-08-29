using System;

namespace HELIX.Context {
  [AttributeUsage(AttributeTargets.Method)]
  // [RequireMixin(typeof(IEventHandlersMixin), true)]
  [MixinImport(typeof(ContextMixinLibrary))]
  public class EventHandlerAttribute : Attribute {
    public readonly int priority;

    public EventHandlerAttribute(int priority = EventPriority.Normal) {
      this.priority = priority;
    }
  }

//   [Mixin] public interface IEventHandlersMixin : IMixin { }
}
