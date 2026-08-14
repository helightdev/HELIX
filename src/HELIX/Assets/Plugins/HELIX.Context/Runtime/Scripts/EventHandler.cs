using System;

namespace HELIX.Context {
  [AttributeUsage(AttributeTargets.Method)]
  public class EventHandlerAttribute : Attribute {
    public int Priority { get; set; } = 0;
  }
}