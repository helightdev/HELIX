// Library code: copied past the lowering pass untouched, so it is already in runtime form.
using Helix.Weave;

namespace Fixtures;

public static class Targets {
  public static readonly InjectionTarget Init = new("Awake", isPublic: false);
  public static readonly InjectionTarget Dispose = new("OnDestroy", isPublic: false);
  public static readonly InjectionTarget Configure =
    new("Configure", "Fixtures.RegistrationConfigurator", isStatic: true, isPublic: true);
}

public static class Shared {
  private static readonly Slot<bool> HasHandler = new(Scope.Type);

  public static void RequireHandler(LateTarget t) {
    if (!t.Once(HasHandler)) return;
    t.Implements("global::Fixtures.IEventListener");
    t.Class(@"
      private readonly global::Fixtures.EventHandlerList eventHandlerList = new();
      global::Fixtures.EventHandlerList global::Fixtures.IEventListener.HandlerList => eventHandlerList;");
    t.Inject(Targets.Dispose, 1, "eventHandlerList.UnregisterAll();");
  }
}
