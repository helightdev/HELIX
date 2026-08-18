using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HELIX.Context.Tests.Fixtures;
using HELIX.Prose;
using NUnit.Framework;
using UnityEngine;

namespace HELIX.Context.Tests {
  public class ManagedContainerTests : ManagedContainerTestFixture {
    private const string Primary = "primary";
    private const string Secondary = "secondary";

    [Test]
    public void LoadsProviderBeforeConsumerAndExposesItsInterface() {
      var trace = new List<string>();
      var registrations = new ComponentRegistrations();
      registrations.Add(_ => new Provider(trace)).Exposes<IProvider>();
      registrations.Add(context => new ProviderConsumer(context.Resolve<IProvider>(), trace))
        .Requires<IProvider>();

      var scope = CreateContainer(registrations).StartApplicationSync();

      Assert.That(trace, Is.EqualTo(new[] { "primary", "consumer" }));
      Assert.That(scope.Resolve<IProvider>(), Is.SameAs(scope.Resolve<Provider>()));
      Assert.That(scope.Resolve<ProviderConsumer>().Provider, Is.SameAs(scope.Resolve<IProvider>()));
    }

    [Test]
    public void ChildScopeCanResolveAncestorWithoutPublishingBackToParent() {
      var registrations = new ComponentRegistrations();
      registrations.Add(_ => new Provider()).Exposes<IProvider>();
      registrations.Add(context => new ProviderConsumer(context.Resolve<IProvider>()))
        .In<SessionScope>()
        .Requires<IProvider>();

      var container = CreateContainer(registrations);
      var application = container.StartApplicationSync();
      var session = container.CreateScope(application).From(new SessionScope()).StartSync();

      Assert.That(session.Resolve<IProvider>(), Is.SameAs(application.Resolve<IProvider>()));
      Assert.That(session.Resolve<ProviderConsumer>().Provider, Is.SameAs(application.Resolve<IProvider>()));
      Assert.Throws<ComponentResolutionException>(() => application.Resolve<ProviderConsumer>());
    }

    [Test]
    public void LocalProviderShadowsAncestorBeforeChildConsumerLoads() {
      var registrations = new ComponentRegistrations();
      registrations.Add(_ => new Provider()).Exposes<IProvider>();
      registrations.Add(_ => new SessionProvider())
        .In<SessionScope>()
        .Exposes<IProvider>();
      registrations.Add(context => new ProviderConsumer(context.Resolve<IProvider>()))
        .In<SessionScope>()
        .Requires<IProvider>();

      var container = CreateContainer(registrations);
      var application = container.StartApplicationSync();
      var session = container.CreateScope(application).From(new SessionScope()).StartSync();

      Assert.That(session.Resolve<ProviderConsumer>().Provider, Is.SameAs(session.Resolve<SessionProvider>()));
      Assert.That(session.Resolve<IProvider>(), Is.Not.SameAs(application.Resolve<IProvider>()));
    }

    [Test]
    public void QualifiersKeepCompetingProvidersIsolated() {
      var registrations = new ComponentRegistrations();
      registrations.Add(_ => new Provider()).Exposes<IProvider>(Primary);
      registrations.Add(_ => new SecondaryProvider()).Exposes<IProvider>(Secondary);
      registrations.Add(context => new ProviderConsumer(context.Resolve<IProvider>(Primary)))
        .Requires<IProvider>(Primary);
      registrations.Add(context => new OptionalProviderConsumer(context.Resolve<IProvider>(Secondary)))
        .Requires<IProvider>(Secondary);

      var scope = CreateContainer(registrations).StartApplicationSync();

      Assert.That(scope.Resolve<ProviderConsumer>().Provider, Is.SameAs(scope.Resolve<IProvider>(Primary)));
      Assert.That(scope.Resolve<OptionalProviderConsumer>().Provider,
        Is.SameAs(scope.Resolve<IProvider>(Secondary)));
      Assert.That(scope.Resolve<IProvider>(Primary), Is.TypeOf<Provider>());
      Assert.That(scope.Resolve<IProvider>(Secondary), Is.TypeOf<SecondaryProvider>());
    }

    [Test]
    public void CollectedDependencyReturnsEmptyListWhenNoProviderExists() {
      var registrations = new ComponentRegistrations();
      registrations.Add(context => new ProviderListConsumer(context.ResolveAll<IProvider>()))
        .Collects<IProvider>();

      var scope = CreateContainer(registrations).StartApplicationSync();

      Assert.That(scope.Resolve<ProviderListConsumer>().Providers, Is.Empty);
    }

    [Test]
    public void CollectedDependencyWaitsForEveryProviderAndPreservesLoadOrder() {
      var trace = new List<string>();
      var registrations = new ComponentRegistrations();
      registrations.Add(context => new ProviderListConsumer(context.ResolveAll<IProvider>(), trace))
        .Collects<IProvider>()
        .order = -100;
      registrations.Add(_ => new Provider(trace))
        .Exposes<IProvider>()
        .order = 10;
      registrations.Add(_ => new SecondaryProvider(trace))
        .Exposes<IProvider>()
        .order = 20;

      var scope = CreateContainer(registrations).StartApplicationSync();
      var providers = scope.Resolve<ProviderListConsumer>().Providers;

      Assert.That(trace, Is.EqualTo(new[] { "primary", "secondary", "list-consumer" }));
      Assert.That(providers.Select(provider => provider.Name), Is.EqualTo(new[] { "primary", "secondary" }));
    }

    [Test]
    public void CollectedDependencyUsesElementQualifierAndIncludesVisibleAncestors() {
      var registrations = new ComponentRegistrations();
      registrations.Add(_ => new Provider()).Exposes<IProvider>(Primary);
      registrations.Add(_ => new SecondaryProvider())
        .In<SessionScope>()
        .Exposes<IProvider>(Primary);
      registrations.Add(context => new ProviderListConsumer(context.ResolveAll<IProvider>(Primary)))
        .In<SessionScope>()
        .Collects<IProvider>(Primary);

      var container = CreateContainer(registrations);
      var application = container.StartApplicationSync();
      var session = container.CreateScope(application).From(new SessionScope()).StartSync();
      var providers = session.Resolve<ProviderListConsumer>().Providers;

      Assert.That(providers, Has.Count.EqualTo(2));
      Assert.That(providers[0], Is.TypeOf<SecondaryProvider>());
      Assert.That(providers[1], Is.SameAs(application.Resolve<IProvider>(Primary)));
    }

    [Test]
    public void CollectedDependencyEvaluatesProxyProvidersOnlyWhenConsumerResolvesList() {
      var trace = new List<string>();
      var value = new PublishedProvider();
      var registrations = new ComponentRegistrations();
      registrations.Add(context => new ProviderListConsumer(context.ResolveAll<IProvider>(), trace))
        .Collects<IProvider>()
        .order = -100;
      registrations.Add(_ => new ProxyBindingComponent(() => value, trace))
        .Publishes<IProvider>();

      var scope = CreateContainer(registrations).StartApplicationSync();

      Assert.That(trace, Is.EqualTo(new[] { "proxy:load", "proxy:late", "supplier", "list-consumer" }));
      Assert.That(scope.Resolve<ProviderListConsumer>().Providers, Is.EqualTo(new[] { value }));
    }

    [Test]
    public void RegistrationConditionsAreAndedAndEvaluatedOncePerScope() {
      var firstCalls = 0;
      var secondCalls = 0;
      var registrations = new ComponentRegistrations();
      registrations.Add<Provider>(_ =>
          throw new AssertionException("Disabled component should not activate."))
        .Condition(_ => {
          firstCalls++;
          return true;
        })
        .Condition(_ => {
          secondCalls++;
          return false;
        });

      var scope = CreateContainer(registrations).StartApplicationSync();

      Assert.That(scope.TryResolve(typeof(Provider), out _), Is.False);
      Assert.That(firstCalls, Is.EqualTo(1));
      Assert.That(secondCalls, Is.EqualTo(1));
    }

    [Test]
    public void RegistrationConditionCanReadScopeBindingsIndependentlyForEachScope() {
      var registrations = new ComponentRegistrations();
      registrations.Add(_ => new Provider())
        .In<SessionScope>()
        .Condition(context => context.Resolve<FeatureFlag>().Enabled);
      var container = CreateContainer(registrations);
      var application = container.StartApplicationSync();

      var enabled = container.CreateScope(application)
        .From(new SessionScope())
        .AddBinding(new FeatureFlag(true))
        .StartSync();
      var disabled = container.CreateScope(application)
        .From(new SessionScope())
        .AddBinding(new FeatureFlag(false))
        .StartSync();

      Assert.That(enabled.Resolve<Provider>(), Is.Not.Null);
      Assert.That(disabled.TryResolve(typeof(Provider), out _), Is.False);
    }

    [Test]
    public void ConditionalProvidersAreRemovedBeforeCollectionDependencySettles() {
      var registrations = new ComponentRegistrations();
      registrations.Add(_ => new Provider())
        .Condition(_ => false)
        .Exposes<IProvider>();
      registrations.Add(_ => new SecondaryProvider())
        .Condition(_ => true)
        .Exposes<IProvider>();
      registrations.Add(context => new ProviderListConsumer(context.ResolveAll<IProvider>()))
        .Collects<IProvider>();

      var scope = CreateContainer(registrations).StartApplicationSync();
      var providers = scope.Resolve<ProviderListConsumer>().Providers;

      Assert.That(providers, Has.Count.EqualTo(1));
      Assert.That(providers[0], Is.TypeOf<SecondaryProvider>());
      Assert.That(scope.TryResolve(typeof(Provider), out _), Is.False);
    }

    [Test]
    public void DisabledOnlyProviderDoesNotSatisfyRequiredConsumerGraph() {
      var registrations = new ComponentRegistrations();
      registrations.Add(_ => new Provider())
        .Condition(_ => false)
        .Exposes<IProvider>();
      registrations.Add(context => new ProviderConsumer(context.Resolve<IProvider>()))
        .Requires<IProvider>();
      var container = CreateContainer(registrations);

      var exception = Assert.Throws<ComponentGraphException>(() => container.StartApplicationSync());

      Assert.That(exception.Message, Does.Contain("no visible component guarantees"));
    }

    [Test]
    public void ConditionFailureIsReportedAsGraphFailureBeforeActivation() {
      var activations = 0;
      var registrations = new ComponentRegistrations();
      registrations.Add(_ => {
          activations++;
          return new Provider();
        })
        .Condition(_ => throw new InvalidOperationException("condition failure"));
      var container = CreateContainer(registrations);

      var exception = Assert.Throws<ComponentGraphException>(() => container.StartApplicationSync());

      Assert.That(exception.Message, Does.Contain("Failed to evaluate conditions"));
      Assert.That(exception.InnerException?.Message, Is.EqualTo("condition failure"));
      Assert.That(activations, Is.Zero);
    }

    [Test]
    public void OptionalDependencyWaitsForAProviderEvenWhenConsumerHasEarlierOrder() {
      var registrations = new ComponentRegistrations();
      registrations.Add(context => new OptionalProviderConsumer(context.ResolveOptional<IProvider>()))
        .OptionallyRequires<IProvider>()
        .order = -100;
      registrations.Add(_ => new Provider())
        .Exposes<IProvider>()
        .order = 100;

      var scope = CreateContainer(registrations).StartApplicationSync();

      Assert.That(scope.Resolve<OptionalProviderConsumer>().Provider, Is.SameAs(scope.Resolve<IProvider>()));
    }

    [Test]
    public void OptionalDependencyWaitsThroughAnInterconnectedProviderChain() {
      var trace = new List<string>();
      var registrations = new ComponentRegistrations();
      registrations.Add(context => new OptionalProviderConsumer(context.ResolveOptional<IProvider>(), trace))
        .OptionallyRequires<IProvider>()
        .order = -100;
      registrations.Add(_ => new Provider(trace))
        .Requires<DependencyGate>()
        .Exposes<IProvider>()
        .order = -50;
      registrations.Add(_ => new DependencyGate(trace));

      CreateContainer(registrations).StartApplicationSync();

      Assert.That(trace, Is.EqualTo(new[] { "gate", "primary", "optional-consumer" }));
    }

    [Test]
    public void OptionalDependencyFallsBackToNullWhenNoProviderCanAppear() {
      var registrations = new ComponentRegistrations();
      registrations.Add(context => new OptionalProviderConsumer(context.ResolveOptional<IProvider>()))
        .OptionallyRequires<IProvider>();

      var scope = CreateContainer(registrations).StartApplicationSync();

      Assert.That(scope.Resolve<OptionalProviderConsumer>().Provider, Is.Null);
    }

    [Test]
    public void ConsumerWaitsForEveryMatchingProviderAndUsesLastOrderedBinding() {
      var registrations = new ComponentRegistrations();
      registrations.Add(context => new ProviderConsumer(context.Resolve<IProvider>()))
        .Requires<IProvider>()
        .order = -100;
      registrations.Add(_ => new Provider())
        .Exposes<IProvider>()
        .order = 10;
      registrations.Add(_ => new SecondaryProvider())
        .Exposes<IProvider>()
        .order = 20;

      var scope = CreateContainer(registrations).StartApplicationSync();

      Assert.That(scope.Resolve<ProviderConsumer>().Provider, Is.TypeOf<SecondaryProvider>());
    }

    [Test]
    public void OptionalDependencyCycleUsesEntryOrderAsFinalTieBreaker() {
      var trace = new List<string>();
      var registrations = new ComponentRegistrations();
      registrations.Add(_ => {
          trace.Add("A");
          return new CycleA();
        })
        .OptionallyRequires<CycleB>()
        .order = 20;
      registrations.Add(_ => {
          trace.Add("B");
          return new CycleB();
        })
        .OptionallyRequires<CycleA>()
        .order = 10;

      CreateContainer(registrations).StartApplicationSync();

      Assert.That(trace, Is.EqualTo(new[] { "B", "A" }));
    }

    [Test]
    public void PipelineTransformersRunBeforeAnAlreadyEligibleConsumer() {
      var trace = new List<string>();
      var registrations = PipelineRegistrations(trace);

      var scope = CreateContainer(registrations).StartApplicationSync();

      Assert.That(trace, Is.EqualTo(new[] {
        "fallback", "first-transformer", "second-transformer", "consumer"
      }));
      Assert.That(scope.Resolve<PipelineConsumer>().Value, Is.EqualTo("2;1;3;"));
    }

    [Test]
    public async Task AsyncPipelineTransformersRunBeforeAnAlreadyEligibleConsumer() {
      var trace = new List<string>();
      var container = CreateContainer(PipelineRegistrations(trace));

      await container.StartApplication();

      Assert.That(trace, Is.EqualTo(new[] {
        "fallback", "first-transformer", "second-transformer", "consumer"
      }));
      Assert.That(container.Application.Resolve<PipelineConsumer>().Value, Is.EqualTo("2;1;3;"));
    }

    [Test]
    public void EqualOrderCollectionTransformerRunsAfterScalarTransformersAndBeforeConsumers() {
      var trace = new List<string>();
      var registrations = CollectionPipelineRegistrations(trace);
      var declared = new ProseTextWriter(wrapWidth: 1000);

      ComponentGraphProse.WriteDeclared(declared, registrations);
      var scope = CreateContainer(registrations).StartApplicationSync();

      Assert.That(trace, Is.EqualTo(new[] {
        "fallback", "first-transformer", "collection-transformer", "collection-consumer"
      }));
      Assert.That(scope.Resolve<PipelineListConsumer>().Values, Is.EqualTo(new[] {
        "2;", "2;1;", "2;,2;1;;3"
      }));
      AssertInOrder(declared.ToString(),
        nameof(FallbackPipelineStage),
        nameof(FirstPipelineStage),
        nameof(CollectingPipelineStage),
        nameof(PipelineListConsumer));
    }

    [Test]
    public void LowerOrderCollectionTransformerRunsBeforeScalarTransformersButNotConsumers() {
      var trace = new List<string>();
      var registrations = CollectionPipelineRegistrations(trace, collectionOrder: -10);
      var declared = new ProseTextWriter(wrapWidth: 1000);

      ComponentGraphProse.WriteDeclared(declared, registrations);
      CreateContainer(registrations).StartApplicationSync();

      Assert.That(trace, Is.EqualTo(new[] {
        "collection-transformer", "fallback", "first-transformer", "collection-consumer"
      }));
      AssertInOrder(declared.ToString(),
        nameof(CollectingPipelineStage),
        nameof(FallbackPipelineStage),
        nameof(FirstPipelineStage),
        nameof(PipelineListConsumer));
    }

    [Test]
    public void GeneratedListInjectionIsMarkedAsACollectionDependency() {
      var registration = new ComponentRegistration(typeof(GeneratedListInjectionComponent));

      GeneratedListInjectionComponent.RegistrationConfigurator(registration);

      Assert.That(registration.dependencies, Has.Count.EqualTo(1));
      Assert.That(registration.dependencies[0].IsCollection, Is.True);
      Assert.That(registration.dependencies[0].flags.HasFlag(DependencyFlags.Required), Is.False);
    }

    [Test]
    public void EarlierPhaseCanForceAnOptionalConsumerAheadOfALaterProvider() {
      var trace = new List<string>();
      var registrations = new ComponentRegistrations();
      registrations.Add(context => new OptionalProviderConsumer(context.ResolveOptional<IProvider>(), trace))
        .OptionallyRequires<IProvider>()
        .phase = -100;
      registrations.Add(_ => new Provider(trace)).Exposes<IProvider>().phase = 100;

      var scope = CreateContainer(registrations).StartApplicationSync();

      Assert.That(trace, Is.EqualTo(new[] { "optional-consumer", "primary" }));
      Assert.That(scope.Resolve<OptionalProviderConsumer>().Provider, Is.Null);
    }

    [Test]
    public void EarlierPhaseCannotBypassARequiredDependencyFromALaterPhase() {
      var trace = new List<string>();
      var registrations = new ComponentRegistrations();
      registrations.Add(context => new ProviderConsumer(context.Resolve<IProvider>(), trace))
        .Requires<IProvider>()
        .phase = -100;
      registrations.Add(_ => new Provider(trace)).Exposes<IProvider>().phase = 100;

      CreateContainer(registrations).StartApplicationSync();

      Assert.That(trace, Is.EqualTo(new[] { "primary", "consumer" }));
    }

    [Test]
    public void ArbitraryScriptedDependencyPhaseParticipatesInLoading() {
      var trace = new List<string>();
      var dependency = new PhasedScriptedDependency(125, trace);
      var registrations = new ComponentRegistrations();
      registrations.Add(_ => new RecordingComponent(trace, "component"))
        .Dependency(new ComponentDependency(dependency, true));

      CreateContainer(registrations).StartApplicationSync();

      Assert.That(trace, Is.EqualTo(new[] { "scripted", "component:load" }));
    }

    [Test]
    public void GeneratedComponentAttributeConfiguresAnIntegerPhase() {
      var registration = new ComponentRegistration(typeof(GeneratedPhasedComponent));

      GeneratedPhasedComponent.RegistrationConfigurator(registration);

      Assert.That(registration.phase, Is.EqualTo(-250));
    }

    [Test]
    public void OptionalComponentIsOmittedWhenRequiredDependencyNeverAppears() {
      var registrations = new ComponentRegistrations();
      registrations.Add<OptionalProviderConsumer>(_ =>
          throw new AssertionException("Unsatisfied optional component should not activate."))
        .Optional()
        .Requires<IProvider>();

      var scope = CreateContainer(registrations).StartApplicationSync();

      Assert.That(scope.TryResolve(typeof(OptionalProviderConsumer), out _), Is.False);
    }

    [Test]
    public void OptionalComponentLoadsWhenItsDependencyBecomesAvailableAndFeedsDownstreamConsumer() {
      var trace = new List<string>();
      var registrations = new ComponentRegistrations();
      registrations.Add(_ => new DependencyGate(trace));
      registrations.Add(_ => new Provider(trace))
        .Optional()
        .Requires<DependencyGate>()
        .Exposes<IProvider>();
      registrations.Add(context => new ProviderConsumer(context.Resolve<IProvider>(), trace))
        .Requires<IProvider>();

      var scope = CreateContainer(registrations).StartApplicationSync();

      Assert.That(trace, Is.EqualTo(new[] { "gate", "primary", "consumer" }));
      Assert.That(scope.Resolve<ProviderConsumer>().Provider, Is.SameAs(scope.Resolve<Provider>()));
    }

    [Test]
    public void SkippedOptionalLocalProviderLetsChildConsumerFallBackToAncestor() {
      var registrations = new ComponentRegistrations();
      registrations.Add(_ => new Provider()).Exposes<IProvider>();
      registrations.Add(_ => new SessionProvider())
        .In<SessionScope>()
        .Optional()
        .Requires<MissingDependency>()
        .Exposes<IProvider>();
      registrations.Add(context => new ProviderConsumer(context.Resolve<IProvider>()))
        .In<SessionScope>()
        .Requires<IProvider>();

      var container = CreateContainer(registrations);
      var application = container.StartApplicationSync();
      var session = container.CreateScope(application).From(new SessionScope()).StartSync();

      Assert.That(session.TryResolve(typeof(SessionProvider), out _), Is.False);
      Assert.That(session.Resolve<ProviderConsumer>().Provider, Is.SameAs(application.Resolve<IProvider>()));
    }

    [Test]
    public void LateLoadPublishesBindingBeforeBufferedConsumerLoads() {
      var trace = new List<string>();
      var value = new PublishedProvider();
      var registrations = LateBindingRegistrations(trace, value);

      var scope = CreateContainer(registrations).StartApplicationSync();

      Assert.That(trace, Is.EqualTo(new[] { "publisher:load", "publisher:late", "consumer" }));
      Assert.That(scope.Resolve<ProviderConsumer>().Provider, Is.SameAs(value));
      Assert.That(scope.Resolve<IProvider>(), Is.SameAs(value));
    }

    [Test]
    public async Task AsyncInitializationCompletesBeforeLateLoadPublishes() {
      var trace = new List<string>();
      var value = new PublishedProvider();
      var registrations = new ComponentRegistrations();
      registrations.Add(_ => new AsyncLateBindingComponent(value, typeof(IProvider), trace))
        .Publishes<IProvider>()
        .Publishes<AsyncHandlerValue>()
        .RegisterHandlerBinding<AsyncComponentLoadEvent>();
      registrations.Add(context => new ProviderConsumer(context.Resolve<IProvider>(), trace))
        .Requires<IProvider>();
      var container = CreateContainer(registrations);

      await container.StartApplication();

      Assert.That(trace, Is.EqualTo(new[] {
        "load", "async:start", "async:end", "late", "consumer"
      }));
      Assert.That(container.Application.Resolve<ProviderConsumer>().Provider, Is.SameAs(value));
      Assert.That(container.Application.Resolve<AsyncHandlerValue>(), Is.Not.Null);
    }

    [Test]
    public void ConsumerWaitsForAllCompetingLatePublishers() {
      var trace = new List<string>();
      var first = new Provider(name: "first");
      var second = new SecondaryProvider();
      var registrations = new ComponentRegistrations();
      registrations.Add(_ => new LateBindingComponent(first, typeof(IProvider), trace))
        .Publishes<IProvider>()
        .order = 10;
      registrations.Add(_ => new SecondLateBindingComponent(second, trace))
        .Publishes<IProvider>()
        .order = 20;
      registrations.Add(context => new ProviderConsumer(context.Resolve<IProvider>(), trace))
        .Requires<IProvider>()
        .order = -100;

      var scope = CreateContainer(registrations).StartApplicationSync();

      Assert.That(trace, Is.EqualTo(new[] {
        "publisher:load", "publisher:late", "second-publisher:load", "second-publisher:late", "consumer"
      }));
      Assert.That(scope.Resolve<ProviderConsumer>().Provider, Is.SameAs(second));
    }

    [Test]
    public void ScopeProxyBindingIsLazyAndEvaluatedForEveryResolution() {
      var calls = 0;
      var first = new Provider(name: "first");
      var second = new SecondaryProvider();
      var registrations = new ComponentRegistrations();
      var container = CreateContainer(registrations);
      var application = container.StartApplicationSync();
      var session = container.CreateScope(application)
        .From(new SessionScope())
        .AddProxyBinding<IProvider>(() => ++calls == 1 ? first : second)
        .StartSync();

      Assert.That(calls, Is.Zero);
      Assert.That(session.Resolve<IProvider>(), Is.SameAs(first));
      Assert.That(session.Resolve<IProvider>(), Is.SameAs(second));
      Assert.That(calls, Is.EqualTo(2));
    }

    [Test]
    public void BufferedConsumerResolvesProxyOnlyAfterPublisherLateLoad() {
      var trace = new List<string>();
      var value = new PublishedProvider();
      var registrations = new ComponentRegistrations();
      registrations.Add(_ => new ProxyBindingComponent(() => value, trace))
        .Publishes<IProvider>();
      registrations.Add(context => new ProviderConsumer(context.Resolve<IProvider>(), trace))
        .Requires<IProvider>();

      var scope = CreateContainer(registrations).StartApplicationSync();

      Assert.That(trace, Is.EqualTo(new[] { "proxy:load", "proxy:late", "supplier", "consumer" }));
      Assert.That(scope.Resolve<ProviderConsumer>().Provider, Is.SameAs(value));
    }

    [Test]
    public void NullProxyFallsBackToOlderVisibleBindingAndIsOmittedFromResolveAll() {
      var registrations = new ComponentRegistrations();
      registrations.Add(_ => new Provider()).Exposes<IProvider>();
      var container = CreateContainer(registrations);
      var application = container.StartApplicationSync();
      var session = container.CreateScope(application)
        .From(new SessionScope())
        .AddProxyBinding<IProvider>(() => null)
        .StartSync();

      var all = session.ResolveAll(typeof(IProvider));

      Assert.That(session.Resolve<IProvider>(), Is.SameAs(application.Resolve<IProvider>()));
      Assert.That(all, Has.Count.EqualTo(1));
      Assert.That(all[0], Is.SameAs(application.Resolve<IProvider>()));
    }

    [Test]
    public void ProxyBindingHonorsQualifierAndRejectsIncompatibleSupplierValue() {
      var registrations = new ComponentRegistrations();
      var container = CreateContainer(registrations);
      var application = container.StartApplicationSync();
      var expected = new Provider();
      var session = container.CreateScope(application)
        .From(new SessionScope())
        .AddProxyBinding<IProvider>(() => expected, Primary)
        .AddProxyBinding(new TypeKey(typeof(IProvider), Secondary), () => new object())
        .StartSync();

      Assert.That(session.Resolve<IProvider>(Primary), Is.SameAs(expected));
      var exception = Assert.Throws<ComponentResolutionException>(() => session.Resolve<IProvider>(Secondary));
      Assert.That(exception.Message, Does.Contain("incompatible type"));
    }

    [Test]
    public void LateLoadFailureRollsBackAlreadyRecordedComponent() {
      var trace = new List<string>();
      var registrations = new ComponentRegistrations();
      registrations.Add(_ => new LateBindingComponent(
          new PublishedProvider(), typeof(IProvider), trace, failLate: true
        ))
        .Publishes<IProvider>();
      var container = CreateContainer(registrations);

      Assert.Throws<ComponentInitializationException>(() => container.StartApplicationSync());

      Assert.That(trace, Is.EqualTo(new[] { "publisher:load", "publisher:late", "publisher:unload" }));
      Assert.That(container.TryGetScope(container.applicationScope, out _), Is.False);
    }

    [Test]
    public void RequiredPublicationMustComeFromDeclaringComponent() {
      var registrations = new ComponentRegistrations();
      registrations.Add(_ => new PublishedProvider()).Publishes<IProvider>();

      var container = CreateContainer(registrations);

      Assert.Throws<ComponentInitializationException>(() => container.StartApplicationSync());
    }

    [Test]
    public void ActivationPublicationSatisfiesDeclarationAndConsumer() {
      var registrations = new ComponentRegistrations();
      registrations.Add(context => {
          var provider = new PublishedProvider();
          context.Publish<IProvider>(provider);
          return provider;
        })
        .Publishes<IProvider>();
      registrations.Add(context => new ProviderConsumer(context.Resolve<IProvider>()))
        .Requires<IProvider>();

      var scope = CreateContainer(registrations).StartApplicationSync();

      Assert.That(scope.Resolve<IProvider>(), Is.SameAs(scope.Resolve<PublishedProvider>()));
      Assert.That(scope.Resolve<ProviderConsumer>().Provider, Is.SameAs(scope.Resolve<IProvider>()));
    }

    [Test]
    public void RequiredDependencyCycleFailsWithoutActivatingEitherComponent() {
      var registrations = new ComponentRegistrations();
      registrations.Add(_ => new CycleA()).Requires<CycleB>();
      registrations.Add(_ => new CycleB()).Requires<CycleA>();
      var container = CreateContainer(registrations);

      var exception = Assert.Throws<ComponentGraphException>(() => container.StartApplicationSync());

      Assert.That(exception.Message, Does.Contain("cannot advance"));
      Assert.That(container.TryGetScope(container.applicationScope, out _), Is.False);
    }

    [Test]
    public void LoadingPassLimitStopsAnOtherwiseProgressingGraph() {
      var registrations = new ComponentRegistrations();
      registrations.Add(_ => new Provider()).Exposes<IProvider>();
      registrations.Add(context => new ProviderConsumer(context.Resolve<IProvider>()))
        .Requires<IProvider>();
      var container = CreateContainer(registrations, maxLoadingIterations: 1);

      var exception = Assert.Throws<ComponentGraphException>(() => container.StartApplicationSync());

      Assert.That(exception.Message, Does.Contain("maximum of 1"));
      Assert.That(container.TryGetScope(container.applicationScope, out _), Is.False);
    }

    [Test]
    public void ResolveAllPreservesProviderLoadOrderWhileResolveReturnsLatest() {
      var registrations = new ComponentRegistrations();
      registrations.Add(_ => new Provider()).Exposes<IProvider>().order = 10;
      registrations.Add(_ => new SecondaryProvider()).Exposes<IProvider>().order = 20;

      var scope = CreateContainer(registrations).StartApplicationSync();
      var all = scope.ResolveAll(typeof(IProvider));

      Assert.That(scope.Resolve<IProvider>(), Is.TypeOf<SecondaryProvider>());
      Assert.That(all, Has.Count.EqualTo(2));
      Assert.That(all[0], Is.TypeOf<Provider>());
      Assert.That(all[1], Is.TypeOf<SecondaryProvider>());
    }

    [Test]
    public void SyncStartRejectsAsyncComponentBeforeActivation() {
      var activations = 0;
      var registrations = new ComponentRegistrations();
      registrations.Add(_ => {
          activations++;
          return new Provider();
        })
        .RegisterHandlerBinding<AsyncComponentLoadEvent>();
      var container = CreateContainer(registrations);

      Assert.Throws<AsyncScopeInitializationException>(() => container.StartApplicationSync());
      Assert.That(activations, Is.Zero);
    }

    [Test]
    public void InitializationFailureRollsBackLoadedComponentsInReverseOrder() {
      var trace = new List<string>();
      var registrations = new ComponentRegistrations();
      registrations.Add(_ => new RecordingComponent(trace, "stable")).order = -1;
      registrations.Add<FailingComponent>(_ => throw new InvalidOperationException("failure")).order = 1;
      var container = CreateContainer(registrations);

      Assert.Throws<ComponentInitializationException>(() => container.StartApplicationSync());

      Assert.That(trace, Is.EqualTo(new[] { "stable:load", "stable:unload" }));
      Assert.That(container.TryGetScope(container.applicationScope, out _), Is.False);
    }

    [Test]
    public void ScopeDisposalUnloadsInReverseDependencyOrderAndIsIdempotent() {
      var trace = new List<string>();
      var registrations = new ComponentRegistrations();
      registrations.Add(_ => new RecordingComponent(trace, "provider"));
      registrations.Add(_ => new RecordingConsumerComponent(trace)).Requires<RecordingComponent>();
      var container = CreateContainer(registrations);
      var scope = container.StartApplicationSync();

      container.DisposeScope(scope.scope);
      container.DisposeScope(scope.scope);

      Assert.That(trace, Is.EqualTo(new[] {
        "provider:load", "consumer:load", "consumer:unload", "provider:unload"
      }));
      Assert.That(scope.State, Is.EqualTo(ManagedScopeState.Disposed));
    }

    [Test]
    public void ScopeBuilderBindingIsVisibleBeforeComponentActivation() {
      var registrations = new ComponentRegistrations();
      registrations.Add(context => new ProviderConsumer(context.Resolve<IProvider>()))
        .In<SessionScope>()
        .Requires<IProvider>();
      var container = CreateContainer(registrations);
      var application = container.StartApplicationSync();
      var provider = new Provider();

      var session = container.CreateScope(application)
        .From(new SessionScope())
        .AddBinding<IProvider>(provider)
        .StartSync();

      Assert.That(session.Resolve<IProvider>(), Is.SameAs(provider));
      Assert.That(session.Resolve<ProviderConsumer>().Provider, Is.SameAs(provider));
    }

    [Test]
    public void ExistingGameObjectComponentIsAdoptedInsteadOfActivated() {
      var registrations = new ComponentRegistrations();
      registrations.Add<InjectedTestComponent>(_ =>
        throw new AssertionException("Existing component should be adopted."));
      GameObject gameObject = null;

      try {
        var container = CreateContainer(registrations, assignUnscopedToApplication: false);
        var application = container.StartApplicationSync();
        gameObject = new GameObject("Injected component test");
        var existing = gameObject.AddComponent<InjectedTestComponent>();

        var scope = container.CreateScope(application)
          .From(new GameObjectScope { gameObject = gameObject })
          .StartSync();

        Assert.That(scope.Resolve<InjectedTestComponent>(), Is.SameAs(existing));
        Assert.That(existing.loadCount, Is.EqualTo(1));
        Assert.That(existing.ComponentBinding.scope, Is.SameAs(scope));
        Assert.That(existing.ComponentBinding.isLoaded, Is.True);
      } finally {
        if (gameObject != null) UnityEngine.Object.DestroyImmediate(gameObject);
      }
    }

    [Test]
    public void ApplicationStartupCreatesSceneScopeAndAdoptsSceneComponent() {
      var registrations = new ComponentRegistrations();
      registrations.Add<InjectedTestComponent>(_ =>
        throw new AssertionException("Existing component should be adopted."));
      var gameObject = new GameObject("Scene component test");

      try {
        var existing = gameObject.AddComponent<InjectedTestComponent>();
        var container = CreateContainer(registrations, assignUnscopedToApplication: false);
        container.StartApplicationSync();
        var scene = container.scopes.Values.Single(candidate =>
          candidate.scope is SceneScope scope && scope.scene == gameObject.scene
        );

        Assert.That(scene.Resolve<InjectedTestComponent>(), Is.SameAs(existing));
        Assert.That(existing.loadCount, Is.EqualTo(1));
      } finally {
        UnityEngine.Object.DestroyImmediate(gameObject);
      }
    }

    [Test]
    public void UnscopedComponentOnlyLoadsWhenExplicitlyContributed() {
      var activations = 0;
      var registrations = new ComponentRegistrations();
      registrations.Add(_ => {
        activations++;
        return new ContributedComponent();
      });
      var container = CreateContainer(registrations, assignUnscopedToApplication: false);
      var application = container.StartApplicationSync();
      var contributed = new ContributedComponent();

      var session = container.CreateScope(application)
        .From(new SessionScope())
        .AddComponent(contributed)
        .StartSync();

      Assert.That(application.TryResolve(typeof(ContributedComponent), out _), Is.False);
      Assert.That(session.Resolve<ContributedComponent>(), Is.SameAs(contributed));
      Assert.That(contributed.ComponentBinding.scope, Is.SameAs(session));
      Assert.That(contributed.ComponentBinding.isLoaded, Is.True);
      Assert.That(activations, Is.Zero);
    }

    [Test]
    public void ScopeBuilderCanActivateExplicitUnscopedComponentType() {
      var activations = 0;
      var registrations = new ComponentRegistrations();
      registrations.Add(_ => {
        activations++;
        return new ContributedComponent();
      });
      var container = CreateContainer(registrations, assignUnscopedToApplication: false);
      var application = container.StartApplicationSync();

      var session = container.CreateScope(application)
        .From(new SessionScope())
        .AddComponent<ContributedComponent>()
        .StartSync();

      Assert.That(activations, Is.EqualTo(1));
      Assert.That(session.Resolve<ContributedComponent>(), Is.TypeOf<ContributedComponent>());
    }

    [Test]
    public void BuilderInstalledScopeHandlerContributesComponent() {
      var contributed = new ContributedComponent();
      var registrations = new ComponentRegistrations();
      registrations.Add<ContributedComponent>(_ =>
        throw new AssertionException("Handler contribution should be adopted."));
      var container = CreateContainer(
        registrations,
        assignUnscopedToApplication: false,
        handlers: new IScopeHandler[] { new TestScopeHandler(contributed) }
      );
      var application = container.StartApplicationSync();

      var scope = container.CreateScope(application).From(new TestScope()).StartSync();

      Assert.That(scope.Resolve<ContributedComponent>(), Is.SameAs(contributed));
    }

    [Test]
    public void RegistrarComponentCanInstallScopeHandler() {
      var contributed = new ContributedComponent();
      var registrations = new ComponentRegistrations();
      registrations.Add(_ => new RegistrarScopeHandler(contributed)).In<RegistrarScope>();
      registrations.Add<ContributedComponent>(_ =>
        throw new AssertionException("Handler contribution should be adopted."));
      var container = CreateContainer(registrations, assignUnscopedToApplication: false);
      var application = container.StartApplicationSync();

      var scope = container.CreateScope(application).From(new TestScope()).StartSync();

      Assert.That(scope.Resolve<ContributedComponent>(), Is.SameAs(contributed));
    }

    [Test]
    public void DeclaredAndLiveGraphsDescribeDependenciesAndOptionalComponents() {
      var registrations = new ComponentRegistrations();
      registrations.Add(_ => new Provider()).Exposes<IProvider>();
      registrations.Add(context => new ProviderConsumer(context.Resolve<IProvider>()))
        .Optional()
        .Requires<IProvider>();
      var declared = new ProseTextWriter(wrapWidth: 1000);

      ComponentGraphProse.WriteDeclared(declared, registrations);
      var container = CreateContainer(registrations);
      container.StartApplicationSync();
      var live = new ProseTextWriter(wrapWidth: 1000);
      ComponentGraphProse.WriteLive(live, container);

      Assert.That(declared.ToString(), Does.Contain("Declared dependency graph"));
      Assert.That(declared.ToString(), Does.Contain(new TypeKey(typeof(IProvider), null).CreateWireKey()));
      Assert.That(declared.ToString(), Does.Contain("optional"));
      Assert.That(live.ToString(), Does.Contain("Live dependency graph"));
      Assert.That(live.ToString(), Does.Contain("ApplicationScope [Active]"));
      Assert.That(live.ToString(), Does.Contain(new TypeKey(typeof(IProvider), null).CreateWireKey()));
    }

    [Test]
    public void DeclaredGraphUsesThePlannedTransformerPipelineOrder() {
      var writer = new ProseTextWriter();

      ComponentGraphProse.WriteDeclared(writer, PipelineRegistrations(new List<string>()));

      var graph = writer.ToString();
      Assert.That(graph.IndexOf(nameof(FallbackPipelineStage), StringComparison.Ordinal),
        Is.LessThan(graph.IndexOf(nameof(FirstPipelineStage), StringComparison.Ordinal)));
      Assert.That(graph.IndexOf(nameof(FirstPipelineStage), StringComparison.Ordinal),
        Is.LessThan(graph.IndexOf(nameof(SecondPipelineStage), StringComparison.Ordinal)));
      Assert.That(graph.IndexOf(nameof(SecondPipelineStage), StringComparison.Ordinal),
        Is.LessThan(graph.IndexOf(nameof(PipelineConsumer), StringComparison.Ordinal)));
    }

    private static ComponentRegistrations LateBindingRegistrations(
      ICollection<string> trace,
      IProvider value
    ) {
      var registrations = new ComponentRegistrations();
      registrations.Add(_ => new LateBindingComponent(value, typeof(IProvider), trace))
        .Publishes<IProvider>();
      registrations.Add(context => new ProviderConsumer(context.Resolve<IProvider>(), trace))
        .Requires<IProvider>();
      return registrations;
    }

    private static ComponentRegistrations PipelineRegistrations(ICollection<string> trace) {
      const string pipeline = "pipeline";
      var registrations = new ComponentRegistrations();
      registrations.Add(context => new PipelineConsumer(context.Resolve<string>(pipeline), trace))
        .Requires<string>(pipeline)
        .order = -100;
      registrations.Add(context => new FallbackPipelineStage(context.ResolveOptional<string>(pipeline), trace))
        .OptionallyRequires<string>(pipeline)
        .Publishes<string>(pipeline)
        .order = 0;
      registrations.Add(context => new FirstPipelineStage(context.Resolve<string>(pipeline), trace))
        .Requires<string>(pipeline)
        .Publishes<string>(pipeline)
        .order = 10;
      registrations.Add(context => new SecondPipelineStage(context.Resolve<string>(pipeline), trace))
        .Requires<string>(pipeline)
        .Publishes<string>(pipeline)
        .order = 20;
      return registrations;
    }

    private static ComponentRegistrations CollectionPipelineRegistrations(
      ICollection<string> trace,
      int collectionOrder = 0
    ) {
      const string pipeline = "pipeline";
      var registrations = new ComponentRegistrations();
      registrations.Add(context => new PipelineListConsumer(context.ResolveAll<string>(pipeline), trace))
        .Collects<string>(pipeline)
        .order = -300;
      registrations.Add(context =>
          new CollectingPipelineStage(context.ResolveAll<string>(pipeline), trace))
        .Collects<string>(pipeline)
        .Publishes<string>(pipeline)
        .order = collectionOrder;
      registrations.Add(context => new FallbackPipelineStage(context.ResolveOptional<string>(pipeline), trace))
        .OptionallyRequires<string>(pipeline)
        .Publishes<string>(pipeline)
        .order = 0;
      registrations.Add(context => new FirstPipelineStage(context.Resolve<string>(pipeline), trace))
        .Requires<string>(pipeline)
        .Publishes<string>(pipeline)
        .order = 0;
      return registrations;
    }

    private static void AssertInOrder(string text, params string[] values) {
      var previous = -1;
      foreach (var value in values) {
        var current = text.IndexOf(value, StringComparison.Ordinal);
        Assert.That(current, Is.GreaterThan(previous), $"Expected '{value}' after the preceding graph entry.");
        previous = current;
      }
    }
  }
}
