using System;
using System.Collections.Generic;
using System.Linq;
using HELIX.Prose;
using NUnit.Framework;
using UnityEngine;

namespace HELIX.Context.Tests {
  public class ManagedContainerTests {
    private readonly List<ManagedContainer> _containers = new();

    [TearDown]
    public void TearDown() {
      foreach (var container in _containers) {
        try { container.Dispose(); } catch (AggregateException) { }
      }
      _containers.Clear();
    }

    [Test]
    public void LoadsComponentsInDependencyOrderAndResolvesExposedKeys() {
      var order = new List<string>();
      var registrations = new ComponentRegistrations();
      registrations.Register(typeof(Provider), entry => {
        entry.activator = _ => new Provider(order);
        entry.Key(typeof(IProvider));
      });
      registrations.Register(typeof(Consumer), entry => {
        entry.activator = context => new Consumer((IProvider)context.Resolve(typeof(IProvider)), order);
        entry.Dependency(Required(typeof(IProvider)));
      });

      var container = CreateContainer(registrations);
      var application = container.StartApplicationSync();

      Assert.That(order, Is.EqualTo(new[] { "provider", "consumer" }));
      Assert.That(application.Resolve(typeof(IProvider)), Is.SameAs(application.Resolve(typeof(Provider))));
      Assert.That(((Consumer)application.Resolve(typeof(Consumer))).provider, Is.SameAs(application.Resolve(typeof(IProvider))));
    }

    [Test]
    public void ChildScopeResolvesAncestorButParentCannotResolveChild() {
      var registrations = new ComponentRegistrations();
      registrations.Register(typeof(Provider), entry => {
        entry.activator = _ => new Provider(null);
        entry.Key(typeof(IProvider));
      });
      registrations.Register(typeof(SessionConsumer), entry => {
        entry.scope = typeof(SessionScope);
        entry.activator = context => new SessionConsumer((IProvider)context.Resolve(typeof(IProvider)));
        entry.Dependency(Required(typeof(IProvider)));
      });

      var container = CreateContainer(registrations);
      var application = container.StartApplicationSync();
      var session = container.CreateScopeSync(application, new SessionScope());

      Assert.That(session.Resolve(typeof(IProvider)), Is.SameAs(application.Resolve(typeof(IProvider))));
      Assert.That(((SessionConsumer)session.Resolve(typeof(SessionConsumer))).provider, Is.SameAs(application.Resolve(typeof(IProvider))));
      Assert.Throws<ComponentResolutionException>(() => application.Resolve(typeof(SessionConsumer)));
    }

    [Test]
    public void GuaranteedChildProviderShadowsAncestorBeforeChildConsumerLoads() {
      var registrations = new ComponentRegistrations();
      registrations.Register(typeof(Provider), entry => {
        entry.activator = _ => new Provider(null);
        entry.Key(typeof(IProvider));
      });
      registrations.Register(typeof(SessionProvider), entry => {
        entry.scope = typeof(SessionScope);
        entry.activator = _ => new SessionProvider();
        entry.Key(typeof(IProvider));
      });
      registrations.Register(typeof(SessionConsumer), entry => {
        entry.scope = typeof(SessionScope);
        entry.activator = context => new SessionConsumer((IProvider)context.Resolve(typeof(IProvider)));
        entry.Dependency(Required(typeof(IProvider)));
      });

      var container = CreateContainer(registrations);
      var application = container.StartApplicationSync();
      var session = container.CreateScopeSync(application, new SessionScope());

      Assert.That(((SessionConsumer)session.Resolve(typeof(SessionConsumer))).provider, Is.SameAs(session.Resolve(typeof(SessionProvider))));
      Assert.That(session.Resolve(typeof(IProvider)), Is.Not.SameAs(application.Resolve(typeof(IProvider))));
    }

    [Test]
    public void InitializationFailureRollsBackInReverseOrder() {
      var lifecycle = new List<string>();
      var registrations = new ComponentRegistrations();
      registrations.Register(typeof(RollbackComponent), entry => {
        entry.order = -1;
        entry.activator = _ => new RollbackComponent(lifecycle);
      });
      registrations.Register(typeof(FailingComponent), entry => {
        entry.order = 1;
        entry.activator = _ => throw new InvalidOperationException("failure");
      });

      var container = CreateContainer(registrations);

      Assert.Throws<ComponentInitializationException>(() => container.StartApplicationSync());
      Assert.That(lifecycle, Is.EqualTo(new[] { "load", "unload" }));
      Assert.That(container.TryGetScope(container.applicationScope, out _), Is.False);
    }

    [Test]
    public void DetectsDependencyCycles() {
      var registrations = new ComponentRegistrations();
      registrations.Register(typeof(CycleA), entry => {
        entry.activator = _ => new CycleA();
        entry.Dependency(Required(typeof(CycleB)));
      });
      registrations.Register(typeof(CycleB), entry => {
        entry.activator = _ => new CycleB();
        entry.Dependency(Required(typeof(CycleA)));
      });

      var container = CreateContainer(registrations);
      var exception = Assert.Throws<ComponentGraphException>(() => container.StartApplicationSync());

      Assert.That(exception.Message, Does.Contain("cannot advance"));
      Assert.That(container.TryGetScope(container.applicationScope, out _), Is.False);
    }

    [Test]
    public void StopsLoadingWhenDependencyPassLimitIsExceeded() {
      var registrations = new ComponentRegistrations();
      registrations.Register(typeof(Provider), entry => {
        entry.activator = _ => new Provider(null);
        entry.Key(typeof(IProvider));
      });
      registrations.Register(typeof(Consumer), entry => {
        entry.activator = context => new Consumer((IProvider)context.Resolve(typeof(IProvider)), null);
        entry.Dependency(Required(typeof(IProvider)));
      });
      var container = CreateContainer(registrations);
      container.maxLoadingIterations = 1;

      var exception = Assert.Throws<ComponentGraphException>(() => container.StartApplicationSync());

      Assert.That(exception.Message, Does.Contain("maximum of 1"));
      Assert.That(container.TryGetScope(container.applicationScope, out _), Is.False);
    }

    [Test]
    public void ResolvesLatestBindingAndReturnsAllBindingsForAKey() {
      var activations = 0;
      var registrations = new ComponentRegistrations();
      registrations.Register(typeof(Provider), entry => {
        entry.activator = _ => {
          activations++;
          return new Provider(null);
        };
        entry.Key(typeof(IProvider));
      });
      registrations.Register(typeof(SecondProvider), entry => {
        entry.activator = _ => {
          activations++;
          return new SecondProvider();
        };
        entry.Key(typeof(IProvider));
      });

      var container = CreateContainer(registrations);
      var application = container.StartApplicationSync();

      Assert.That(activations, Is.EqualTo(2));
      Assert.That(application.Resolve(typeof(IProvider)), Is.TypeOf<SecondProvider>());
      Assert.That(application.ResolveAll(typeof(IProvider)), Has.Count.EqualTo(2));
      Assert.That(application.ResolveAll(typeof(IProvider))[0], Is.TypeOf<Provider>());
      Assert.That(application.ResolveAll(typeof(IProvider))[1], Is.TypeOf<SecondProvider>());
    }

    [Test]
    public void RequiredPublicationMustBeProvidedByDeclaringComponent() {
      var registrations = new ComponentRegistrations();
      registrations.Register(typeof(Publisher), entry => {
        entry.activator = _ => new Publisher();
        entry.Publication(Required(typeof(IProvider)));
      });

      var container = CreateContainer(registrations);

      Assert.Throws<ComponentInitializationException>(() => container.StartApplicationSync());
    }

    [Test]
    public void ContextPublicationSatisfiesRequiredPublicationAndConsumers() {
      var registrations = new ComponentRegistrations();
      registrations.Register(typeof(Publisher), entry => {
        entry.activator = context => {
          var instance = new Publisher();
          context.Publish(typeof(IProvider), instance);
          return instance;
        };
        entry.Publication(Required(typeof(IProvider)));
      });
      registrations.Register(typeof(Consumer), entry => {
        entry.activator = context => new Consumer((IProvider)context.Resolve(typeof(IProvider)), null);
        entry.Dependency(Required(typeof(IProvider)));
      });

      var container = CreateContainer(registrations);
      var application = container.StartApplicationSync();

      Assert.That(application.Resolve(typeof(IProvider)), Is.SameAs(application.Resolve(typeof(Publisher))));
    }

    [Test]
    public void SyncInitializationRejectsAsyncHandlerMetadataBeforeActivation() {
      var activations = 0;
      var registrations = new ComponentRegistrations();
      registrations.Register(typeof(Provider), entry => {
        entry.activator = _ => {
          activations++;
          return new Provider(null);
        };
        entry.RegisterHandlerBinding<ComponentAsyncInitEvent>();
      });

      var container = CreateContainer(registrations);

      Assert.Throws<AsyncScopeInitializationException>(() => container.StartApplicationSync());
      Assert.That(activations, Is.Zero);
    }

    [Test]
    public void ScopeDisposalUnloadsComponentsInReverseDependencyOrder() {
      var lifecycle = new List<string>();
      var registrations = new ComponentRegistrations();
      registrations.Register(typeof(OrderedProvider), entry => entry.activator = _ => new OrderedProvider(lifecycle));
      registrations.Register(typeof(OrderedConsumer), entry => {
        entry.activator = _ => new OrderedConsumer(lifecycle);
        entry.Dependency(Required(typeof(OrderedProvider)));
      });

      var container = CreateContainer(registrations);
      var application = container.StartApplicationSync();
      container.DisposeScope(application.scope);
      container.DisposeScope(application.scope);
      container.DisposeScope(application.scope);

      Assert.That(lifecycle, Is.EqualTo(new[] {
        "provider-load", "consumer-load", "consumer-unload", "provider-unload"
      }));
      Assert.That(application.State, Is.EqualTo(ManagedScopeState.Disposed));
    }

    [Test]
    public void WritesDeclaredAndLiveDependencyGraphsWithProse() {
      var registrations = new ComponentRegistrations();
      registrations.Register(typeof(Provider), entry => {
        entry.activator = _ => new Provider(null);
        entry.Key(typeof(IProvider));
      });
      registrations.Register(typeof(Consumer), entry => {
        entry.activator = context => new Consumer((IProvider)context.Resolve(typeof(IProvider)), null);
        entry.Dependency(Required(typeof(IProvider)));
      });

      var declaredWriter = new ProseTextWriter();
      ComponentGraphProse.WriteDeclared(declaredWriter, registrations);

      Assert.That(declaredWriter.ToString(), Does.Contain("Declared dependency graph"));
      Assert.That(declaredWriter.ToString(), Does.Contain("IProvider"));

      var container = CreateContainer(registrations);
      container.StartApplicationSync();
      var liveWriter = new ProseTextWriter();
      ComponentGraphProse.WriteLive(liveWriter, container);

      Assert.That(liveWriter.ToString(), Does.Contain("Live dependency graph"));
      Assert.That(liveWriter.ToString(), Does.Contain("ApplicationScope [Active]"));
      Assert.That(liveWriter.ToString(), Does.Contain("Provider"));
    }

    [Test]
    public void GameObjectScopeAdoptsExistingComponentsInsteadOfActivatingNewOnes() {
      var registrations = new ComponentRegistrations();
      registrations.Register(typeof(InjectedTestComponent), entry => {
        entry.activator = _ => throw new AssertionException("The existing component should be adopted.");
      });
      GameObject gameObject = null;
      try {
        var container = CreateContainer(registrations, false);
        var application = container.StartApplicationSync();
        gameObject = new GameObject("Injected component test");
        var existing = gameObject.AddComponent<InjectedTestComponent>();
        var scope = container.CreateScopeSync(application, new GameObjectScope { gameObject = gameObject });

        Assert.That(scope.Resolve(typeof(InjectedTestComponent)), Is.SameAs(existing));
        Assert.That(existing.loadCount, Is.EqualTo(1));
        Assert.That(existing.RuntimeComponentData.scope, Is.SameAs(scope));
        Assert.That(existing.RuntimeComponentData.isLoaded, Is.True);
      } finally {
        if (gameObject != null) UnityEngine.Object.DestroyImmediate(gameObject);
      }
    }

    [Test]
    public void ApplicationStartupCreatesSceneScopeAndAdoptsExistingSceneComponents() {
      var registrations = new ComponentRegistrations();
      registrations.Register(typeof(InjectedTestComponent), entry => {
        entry.activator = _ => throw new AssertionException("The existing component should be adopted.");
      });
      var gameObject = new GameObject("Scene injected component test");
      try {
        var existing = gameObject.AddComponent<InjectedTestComponent>();
        var container = CreateContainer(registrations, false);
        container.StartApplicationSync();
        var sceneScope = container.scopes.Values.Single(scope =>
          scope.scope is SceneScope scene && scene.scene == gameObject.scene
        );

        Assert.That(sceneScope.Resolve(typeof(InjectedTestComponent)), Is.SameAs(existing));
        Assert.That(existing.loadCount, Is.EqualTo(1));
      } finally {
        UnityEngine.Object.DestroyImmediate(gameObject);
      }
    }

    [Test]
    public void UnscopedComponentsOnlyLoadWhenContributed() {
      var activations = 0;
      var registrations = new ComponentRegistrations();
      registrations.Register(typeof(ManuallyContributedComponent), entry =>
        entry.activator = _ => {
          activations++;
          return new ManuallyContributedComponent();
        }
      );
      var container = CreateContainer(registrations, false);
      var application = container.StartApplicationSync();

      Assert.That(activations, Is.Zero);
      Assert.Throws<ComponentResolutionException>(() => application.Resolve(typeof(ManuallyContributedComponent)));

      var contributed = new ManuallyContributedComponent();
      var session = container.CreateScopeSync(
        application,
        new SessionScope(),
        new[] { contributed }
      );
      Assert.That(session.Resolve(typeof(ManuallyContributedComponent)), Is.SameAs(contributed));
      Assert.That(contributed.RuntimeComponentData.scope, Is.SameAs(session));
      Assert.That(contributed.RuntimeComponentData.isLoaded, Is.True);
      Assert.That(activations, Is.Zero);
    }

    private ManagedContainer CreateContainer(
      ComponentRegistrations registrations,
      bool assignUnscopedToApplication = true
    ) {
      if (assignUnscopedToApplication) {
        foreach (var registration in registrations.components.Values) {
          if (registration.scope == null) registration.scope = typeof(ApplicationScope);
        }
      }
      var container = new ManagedContainer();
      _containers.Add(container);
      container.PrepareRegistrar(registrations);
      return container;
    }

    private static ComponentDependency Required(Type type) => new((TypeKey)type, true);

    private interface IProvider { }

    private sealed class Provider : IProvider {
      public Provider(ICollection<string> order) => order?.Add("provider");
    }

    private sealed class SecondProvider : IProvider { }
    private sealed class Publisher : IProvider { }
    private sealed class SessionProvider : IProvider { }

    private sealed class Consumer {
      public readonly IProvider provider;
      public Consumer(IProvider provider, ICollection<string> order) {
        this.provider = provider;
        order?.Add("consumer");
      }
    }

    private sealed class SessionConsumer {
      public readonly IProvider provider;
      public SessionConsumer(IProvider provider) => this.provider = provider;
    }

    private sealed class RollbackComponent : IComponent {
      private readonly ICollection<string> _lifecycle;
      public RollbackComponent(ICollection<string> lifecycle) => _lifecycle = lifecycle;
      public RuntimeComponentData RuntimeComponentData { get; } = new();
      public void LoadComponent() => _lifecycle.Add("load");
      public void UnloadComponent() => _lifecycle.Add("unload");
    }

    private sealed class FailingComponent { }
    private sealed class CycleA { }
    private sealed class CycleB { }

    private sealed class OrderedProvider : IComponent {
      private readonly ICollection<string> _lifecycle;
      public OrderedProvider(ICollection<string> lifecycle) => _lifecycle = lifecycle;
      public RuntimeComponentData RuntimeComponentData { get; } = new();
      public void LoadComponent() => _lifecycle.Add("provider-load");
      public void UnloadComponent() => _lifecycle.Add("provider-unload");
    }

    private sealed class OrderedConsumer : IComponent {
      private readonly ICollection<string> _lifecycle;
      public OrderedConsumer(ICollection<string> lifecycle) => _lifecycle = lifecycle;
      public RuntimeComponentData RuntimeComponentData { get; } = new();
      public void LoadComponent() => _lifecycle.Add("consumer-load");
      public void UnloadComponent() => _lifecycle.Add("consumer-unload");
    }
  }

  public sealed class InjectedTestComponent : MonoBehaviour, IComponent {
    public int loadCount;
    public RuntimeComponentData RuntimeComponentData { get; } = new();
    public void LoadComponent() => loadCount++;
  }

  public sealed class ManuallyContributedComponent : IComponent {
    public RuntimeComponentData RuntimeComponentData { get; } = new();
  }
}
