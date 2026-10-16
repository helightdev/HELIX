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
  // A shared key rather than cross-symbol state: each handler emits the block and the merge
  // keeps one copy, which is what lets every prelude be cached on its own.
  public static void RequireHandler(LateTarget t) => t.Shared("Fixtures.EventHandlerList", target => {
    target.Implements("global::Fixtures.IEventListener");
    target.Class(@"
      private readonly global::Fixtures.EventHandlerList eventHandlerList = new();
      global::Fixtures.EventHandlerList global::Fixtures.IEventListener.HandlerList => eventHandlerList;");
    target.Inject(Targets.Dispose, 1, "eventHandlerList.UnregisterAll();");
  });
}
