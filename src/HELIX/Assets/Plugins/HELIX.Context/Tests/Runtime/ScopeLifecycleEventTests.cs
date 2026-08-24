using System.Collections.Generic;
using System.Threading.Tasks;
using HELIX.Context.Tests.Fixtures;
using NUnit.Framework;

namespace HELIX.Context.Tests {
  public class ScopeLifecycleEventTests : ManagedContainerTestFixture {
    [TestCase(false)]
    [TestCase(true)]
    public async Task ScopeEventsWrapSyncAndAsyncLoadingInLifecycleOrder(bool async) {
      var trace = new List<string>();
      var registrations = new ManagedRegistrations();
      registrations.Add(_ => new RecordingComponent(trace, "component"));
      var container = CreateContainer(registrations);
      ManagedScope observedScope = null;

      using var create = Evt.Subscribe<ScopeCreateEvent>((ScopeCreateEvent evt) => {
        if (!ReferenceEquals(evt.Scope.scope, container.applicationScope)) return;
        Assert.That(evt.Container, Is.SameAs(container));
        Assert.That(evt.Scope.State, Is.EqualTo(ManagedScopeState.Initializing));
        observedScope = evt.Scope;
        trace.Add("scope:create");
      });
      using var activate = Evt.Subscribe<ScopeActivateEvent>((ScopeActivateEvent evt) => {
        if (!ReferenceEquals(evt.Scope.scope, container.applicationScope)) return;
        AssertPayload(evt.Container, evt.Scope, container, observedScope, ManagedScopeState.Active);
        trace.Add("scope:activate");
      });
      using var started = Evt.Subscribe<ScopeStartedEvent>((ScopeStartedEvent evt) => {
        if (!ReferenceEquals(evt.Scope.scope, container.applicationScope)) return;
        AssertPayload(evt.Container, evt.Scope, container, observedScope, ManagedScopeState.Active);
        trace.Add("scope:started");
      });
      using var deactivate = Evt.Subscribe<ScopeDeactivateEvent>((ScopeDeactivateEvent evt) => {
        if (!ReferenceEquals(evt.Scope.scope, container.applicationScope)) return;
        AssertPayload(evt.Container, evt.Scope, container, observedScope, ManagedScopeState.Disposing);
        trace.Add("scope:deactivate");
      });
      using var disposed = Evt.Subscribe<ScopeDisposedEvent>((ScopeDisposedEvent evt) => {
        if (!ReferenceEquals(evt.Scope.scope, container.applicationScope)) return;
        AssertPayload(evt.Container, evt.Scope, container, observedScope, ManagedScopeState.Disposed);
        trace.Add("scope:disposed");
      });

      ManagedScope scope;
      if (async) {
        await container.StartApplication();
        scope = container.Application;
      } else scope = container.StartApplicationSync();
      container.DisposeScope(scope.scope);

      Assert.That(scope, Is.SameAs(observedScope));
      Assert.That(trace, Is.EqualTo(new[] {
        "scope:create",
        "component:load",
        "scope:activate",
        "scope:started",
        "scope:deactivate",
        "component:unload",
        "scope:disposed"
      }));
    }

    private static void AssertPayload(
      ManagedContainer actualContainer,
      ManagedScope actualScope,
      ManagedContainer expectedContainer,
      ManagedScope expectedScope,
      ManagedScopeState expectedState
    ) {
      Assert.That(actualContainer, Is.SameAs(expectedContainer));
      Assert.That(actualScope, Is.SameAs(expectedScope));
      Assert.That(actualScope.State, Is.EqualTo(expectedState));
    }
  }
}
