using System;
using Helix.Weave;

namespace Fixtures;

[Mixin(Unit.Method)]
public sealed class EventHandlerAttribute : Attribute {
  public EventHandlerAttribute(int priority = 0) => Priority = priority;
  public int Priority { get; init; }

  [Weave]
  static void Apply(MethodTarget t, EventHandlerAttribute a) {
    Shared.RequireHandler(t);

    if (t.Params.Length != 1) {
      t.Report("FX001", "expected exactly one event parameter");
      return;
    }

    var p = t.Param(0);
    var register =
      p.ByValue && p.Type.Implements("Fixtures.IAsyncEvt") ? "RegisterAsync" :
      (p.ByValue || p.ByRef) && p.Type.Implements("Fixtures.Evt") ? "Register" : null;

    if (register is null) {
      t.Report("FX002", "parameter must be an Evt or an IAsyncEvt");
      return;
    }

    t.Inject(Targets.Init, 1,
      $"eventHandlerList.{register}<{p.Type}>({t.Name}, {Lit.Of(a.Priority)});");
  }
}
