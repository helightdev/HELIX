using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace HELIX.Context {
  /// <summary>Reusable container service that bounds transient dependency evidence to one scope load.</summary>
  internal sealed partial class ScopeLoader {
    private static readonly AsyncLocal<ScopeLoader> _active = new();
    private readonly Dictionary<string, HashSet<ComponentRegistration>> _publications = new();
    private readonly HashSet<string> _anonymousPublications = new(StringComparer.Ordinal);
    private readonly HashSet<IScriptedDependency> _scripted = new(ReferenceComparer<IScriptedDependency>.Instance);
    private readonly ManagedContainer _container;
    private readonly RegistrarGraph _graph;
    private readonly ScopeRules _rules;
    private ManagedScope _scope;
    private Dictionary<ComponentRegistration, Queue<object>> _injected;

    public ScopeLoader(ManagedContainer container, RegistrarGraph graph, ScopeRules rules) {
      _container = container ?? throw new ArgumentNullException(nameof(container));
      _graph = graph ?? throw new ArgumentNullException(nameof(graph));
      _rules = rules ?? throw new ArgumentNullException(nameof(rules));
    }

    public static ScopeLoader Active => _active.Value ??
      throw new ScopeLifecycleException("No scope is currently loading.");
    public static ScopeLoader ActiveOrNull => _active.Value;

    public void Record(IScriptedDependency dependency) => _scripted.Add(dependency);
    public bool Contains(IScriptedDependency dependency) => _scripted.Contains(dependency);
    public void Publish(string wireKey) => _anonymousPublications.Add(wireKey);

    public void Publish(ComponentRegistration owner, string wireKey) {
      if (!_publications.TryGetValue(wireKey, out var owners))
        _publications.Add(wireKey, owners = new HashSet<ComponentRegistration>());
      owners.Add(owner);
    }

    public bool HasPublication(string wireKey) {
      return !string.IsNullOrEmpty(wireKey) &&
        (_anonymousPublications.Contains(wireKey) ||
          (_publications.TryGetValue(wireKey, out var owners) && owners.Count > 0));
    }

    public bool WasPublishedBy(ComponentRegistration owner, string wireKey) {
      return wireKey != null && _publications.TryGetValue(wireKey, out var owners) && owners.Contains(owner);
    }

    public IEnumerable<string> PublicationsBy(ComponentRegistration owner) {
      return _publications.Where(pair => pair.Value.Contains(owner)).Select(static pair => pair.Key);
    }

    internal void ValidateScope(ManagedScope parent, IScope child) {
      _rules.Validate(ScopeValidationContext.Create(_container, parent, child));
    }

    private static IEnumerable<ComponentDependency> EnumerateImplicitScripted(
      IEnumerable<ComponentRegistration> entries,
      InitializationStage stage
    ) => entries
      .SelectMany(static x => x.dependencies)
      .Where(x => x.IsScripted && x.scripted.Stage == stage && x.flags.HasFlag(DependencyFlags.ImplicitLoadable))
      .OrderBy(static x => x.scripted.Order);

    private static bool CanProvide(ComponentRegistration entry, ComponentDependency dependency) {
      if (dependency.IsTyped) {
        return entry.keys.Contains(dependency.key) || entry.publications.Any(publication =>
          publication.IsTyped && publication.key.Equals(dependency.key)
        );
      }
      return dependency.flags.HasFlag(DependencyFlags.Wirable) && entry.publications.Any(publication =>
        publication.flags.HasFlag(DependencyFlags.Wirable) && publication.wireKey == dependency.wireKey
      );
    }

    private static bool HasPendingProvider(
      ComponentRegistration consumer,
      ComponentDependency dependency,
      IReadOnlyCollection<ComponentRegistration> pending
    ) {
      var count = pending.Count(entry => CanProvide(entry, dependency));
      return count > (CanProvide(consumer, dependency) ? 1 : 0);
    }

    private static bool CanLoadWithoutDelay(
      ManagedScope managed,
      ComponentRegistration entry,
      IReadOnlyCollection<ComponentRegistration> pending,
      InitializationStage stage
    ) {
      if (!RequiredDependenciesSatisfied(managed, entry, pending)) return false;
      return entry.dependencies.All(dependency =>
        !HasPendingProvider(entry, dependency, pending) &&
        !(dependency.IsScripted && dependency.flags.HasFlag(DependencyFlags.ImplicitLoadable) &&
          dependency.scripted.Stage > stage && !managed.IsDependencyAvailable(dependency))
      );
    }

    private static bool RequiredDependenciesSatisfied(
      ManagedScope managed,
      ComponentRegistration entry,
      IReadOnlyCollection<ComponentRegistration> pending
    ) => entry.dependencies.All(dependency => {
        if (!dependency.flags.HasFlag(DependencyFlags.Required)) return true;
        var hasLocalProvider = pending.Any(provider => CanProvide(provider, dependency)) ||
          managed.HasLocalDependency(dependency);
        return hasLocalProvider ? managed.HasLocalDependency(dependency) : managed.HasDependency(dependency);
      }
    );

    private static bool RemoveUnavailableOptionalComponents(
      ManagedScope managed,
      List<ComponentRegistration> pending
    ) {
      var unavailable = pending.Where(entry =>
          entry.optional && !RequiredDependenciesSatisfied(managed, entry, pending)
        )
        .ToList();
      foreach (var entry in unavailable) pending.Remove(entry);
      return unavailable.Count > 0;
    }

    private void EnsureIterationAvailable(int iteration, ManagedScope managed) {
      if (_container.maxLoadingIterations <= 0) {
        throw new ScopeLifecycleException(
          $"{nameof(ManagedContainer.maxLoadingIterations)} must be greater than zero."
        );
      }
      if (iteration < _container.maxLoadingIterations) return;
      throw new ComponentGraphException(
        $"Scope {managed.scope.GetType().FullName} exceeded the maximum of " +
        $"{_container.maxLoadingIterations} dependency-loading iterations. " +
        "The graph may contain a publication cycle that continues to make artificial progress."
      );
    }

    private static void ValidateAndCompleteComponent(
      ManagedScope managed,
      ComponentRegistration entry,
      object instance
    ) {
      foreach (var publication in entry.publications.Where(static x =>
        x.flags.HasFlag(DependencyFlags.Required)
      )) {
        if (managed.WasProvidedBy(entry, publication)) continue;
        throw new ComponentInitializationException(
          $"Component '{entry.name}' did not provide required publication '{publication.wireKey}'."
        );
      }

      if (instance is IComponent component) {
        component.ComponentBinding.SetLoaded(true);
      }
    }

    private static void EnsureFullyLoaded(ManagedScope managed, IReadOnlyCollection<ComponentRegistration> pending) {
      if (pending.Count == 0) return;
      var details = pending.Select(entry => {
          var missing = entry.dependencies.Where(x => !managed.HasDependency(x))
            .Select(static x => x.wireKey ?? x.scripted?.GetType().FullName ?? "<unwired>");
          return $"{entry.name} -> [{string.Join(", ", missing)}]";
        }
      );
      throw new ComponentGraphException(
        "The dependency graph cannot advance. It contains a cycle or unresolved dynamic publications: " +
        string.Join("; ", details)
      );
    }

    private void Reset() {
      _scope = null;
      _injected = null;
      _active.Value = null;
      _publications.Clear();
      _anonymousPublications.Clear();
      _scripted.Clear();
    }

    private void RestoreActiveContext() {
      _active.Value = this;
    }

    private IReadOnlyList<ComponentRegistration> EntriesFor(
      ManagedScope managed,
      IEnumerable<IComponent> contributions,
      IEnumerable<Type> componentTypes
    ) {
      _injected = _container.DiscoverInjectedComponents(managed, contributions);
      var selected = new List<ComponentRegistration>(_injected.Keys);
      foreach (var type in componentTypes ?? Enumerable.Empty<Type>()) {
        if (!_container.registrarScope.registrations.components.TryGetValue(type, out var registration)) {
          throw new ComponentGraphException($"Component type {type.FullName} is not registered.");
        }
        if (registration.scope != null && registration.scope != managed.scope.GetType()) {
          throw new ComponentGraphException(
            $"Component '{registration.name}' is assigned to {registration.scope.FullName} and cannot be added to " +
            $"{managed.scope.GetType().FullName}."
          );
        }
        selected.Add(registration);
      }
      var entries = _graph.For(managed, selected, entry => ConditionsSatisfied(managed, entry));
      if (_injected.Count == 0) return entries;

      var expanded = new List<ComponentRegistration>();
      foreach (var entry in entries) {
        if (_injected.TryGetValue(entry, out var instances)) {
          for (var i = 0; i < instances.Count; i++) expanded.Add(entry);
        } else expanded.Add(entry);
      }
      return expanded;
    }

    private bool ConditionsSatisfied(ManagedScope managed, ComponentRegistration entry) {
      if (entry.conditions.Count == 0) return true;
      var context = new ComponentLoadContext(_container, managed, entry, this);
      try {
        return entry.conditions.All(condition => condition(context));
      } catch (Exception exception) {
        if (exception is ComponentContainerException) throw;
        throw new ComponentGraphException($"Failed to evaluate conditions for component '{entry.name}'.", exception);
      }
    }

    private object Activate(ComponentRegistration entry, ComponentLoadContext context) {
      if (_injected != null && _injected.TryGetValue(entry, out var instances) && instances.Count > 0) {
        var instance = instances.Dequeue();
        if (instance is IComponent component) {
          var runtimeData = component.ComponentBinding;
          runtimeData.scope = context.scope;
          runtimeData.container = context.container;
        }
        ;
        return instance;
      }
      return entry.Activate(context);
    }
  }

  internal sealed partial class ScopeLoader {
    // Sync
    internal void LoadSync(
      ManagedScope managed,
      IEnumerable<IComponent> contributions = null,
      IEnumerable<Type> componentTypes = null,
      IEnumerable<ScopeBinding> bindings = null
    ) {
      if (_scope != null) throw new ScopeLifecycleException("The scope loader is already loading a scope.");
      _scope = managed;
      _active.Value = this;
      try {
        managed.AddBindings(bindings);
        var entries = EntriesFor(managed, contributions, componentTypes);
        foreach (var entry in entries) {
          if (entry.IsAsync) {
            throw new AsyncScopeInitializationException(
              $"Component '{entry.name}' requires asynchronous initialization."
            );
          }
          foreach (var dependency in entry.dependencies) {
            if (dependency.IsScripted && dependency.flags.HasFlag(DependencyFlags.Async)) {
              throw new AsyncScopeInitializationException(
                $"Component '{entry.name}' requires asynchronous dependency '{dependency.wireKey}'."
              );
            }
          }
        }
        var pending = new List<ComponentRegistration>(entries);
        foreach (InitializationStage stage in Enum.GetValues(typeof(InitializationStage))) {
          LoadScriptedDependenciesSync(managed, entries, stage);
          if (stage != InitializationStage.PreInit) LoadEligibleSync(managed, pending, stage);
        }
        EnsureFullyLoaded(managed, pending);
      } finally {
        Reset();
      }
    }

    private void LoadScriptedDependenciesSync(
      ManagedScope managed,
      IEnumerable<ComponentRegistration> entries,
      InitializationStage stage
    ) {
      foreach (var dependency in EnumerateImplicitScripted(entries, stage)) {
        if (managed.IsScriptedLoaded(dependency.scripted) || managed.IsDependencyAvailable(dependency)) continue;
        var result = dependency.scripted.Load(new ComponentLoadContext(_container, managed, null, this));
        if (!result.success) {
          if (dependency.flags.HasFlag(DependencyFlags.Required)) {
            throw new ComponentInitializationException(
              $"Scripted dependency '{dependency.wireKey}' failed during {stage}."
            );
          }
          continue;
        }
        if (result.value != null) managed.Publish(null, result.value.GetType(), result.value);
        managed.MarkScriptedLoaded(dependency.scripted);
      }
    }

    private void LoadEligibleSync(
      ManagedScope managed,
      List<ComponentRegistration> pending,
      InitializationStage stage
    ) {
      var iteration = 0;
      while (true) {
        if (pending.Count == 0) return;
        EnsureIterationAvailable(iteration++, managed);
        var eligible = pending.Where(entry => CanLoadWithoutDelay(managed, entry, pending, stage))
          .OrderBy(static x => x.order)
          .ThenBy(static x => x.name, StringComparer.Ordinal)
          .ToList();
        if (eligible.Count > 0) {
          foreach (var entry in eligible) {
            LoadRegistrationSync(managed, entry);
            pending.Remove(entry);
          }
          continue;
        }
        if (stage != InitializationStage.PostInit) return;
        if (RemoveUnavailableOptionalComponents(managed, pending)) continue;
        var fallback = pending.Where(entry => RequiredDependenciesSatisfied(managed, entry, pending))
          .OrderBy(static x => x.order)
          .ThenBy(static x => x.name, StringComparer.Ordinal)
          .FirstOrDefault();
        if (fallback == null) return;
        LoadRegistrationSync(managed, fallback);
        pending.Remove(fallback);
      }
    }

    private void LoadRegistrationSync(ManagedScope managed, ComponentRegistration entry) {
      var context = new ComponentLoadContext(_container, managed, entry, this);
      try {
        var instance = Activate(entry, context);
        managed.RecordComponent(entry, instance);
        managed.BindComponent(entry, instance);
        entry.InitializeSync(instance, context);
        entry.InitializeLate(instance, context);
        ValidateAndCompleteComponent(managed, entry, instance);
      } catch (Exception exception) {
        if (exception is ComponentContainerException) throw;
        throw new ComponentInitializationException($"Failed to initialize component '{entry.name}'.", exception);
      }
    }
  }

  internal sealed partial class ScopeLoader {
    // Async
    internal async UniTask LoadAsync(
      ManagedScope managed,
      IEnumerable<IComponent> contributions = null,
      IEnumerable<Type> componentTypes = null,
      IEnumerable<ScopeBinding> bindings = null
    ) {
      if (_scope != null) throw new ScopeLifecycleException("The scope loader is already loading a scope.");
      _scope = managed;
      _active.Value = this;
      try {
        managed.AddBindings(bindings);
        var entries = EntriesFor(managed, contributions, componentTypes);
        var pending = new List<ComponentRegistration>(entries);
        foreach (InitializationStage stage in Enum.GetValues(typeof(InitializationStage))) {
          await LoadScriptedDependenciesAsync(managed, entries, stage);
          RestoreActiveContext();
          if (stage != InitializationStage.PreInit) {
            await LoadEligibleAsync(managed, pending, stage);
            RestoreActiveContext();
          }
        }
        EnsureFullyLoaded(managed, pending);
      } finally {
        Reset();
      }
    }

    private async UniTask LoadScriptedDependenciesAsync(
      ManagedScope managed,
      IEnumerable<ComponentRegistration> entries,
      InitializationStage stage
    ) {
      foreach (var dependency in EnumerateImplicitScripted(entries, stage)) {
        if (managed.IsScriptedLoaded(dependency.scripted) || managed.IsDependencyAvailable(dependency)) continue;
        var result = await dependency.scripted.LoadAsync(new ComponentLoadContext(_container, managed, null, this));
        RestoreActiveContext();
        if (!result.success) {
          if (dependency.flags.HasFlag(DependencyFlags.Required)) {
            throw new ComponentInitializationException(
              $"Scripted dependency '{dependency.wireKey}' failed during {stage}."
            );
          }
          continue;
        }
        if (result.value != null) managed.Publish(null, result.value.GetType(), result.value);
        managed.MarkScriptedLoaded(dependency.scripted);
      }
    }

    private async UniTask LoadEligibleAsync(
      ManagedScope managed,
      List<ComponentRegistration> pending,
      InitializationStage stage
    ) {
      var iteration = 0;
      while (true) {
        if (pending.Count == 0) return;
        EnsureIterationAvailable(iteration++, managed);
        var eligible = pending.Where(entry => CanLoadWithoutDelay(managed, entry, pending, stage))
          .OrderBy(static x => x.order)
          .ThenBy(static x => x.name, StringComparer.Ordinal)
          .ToList();
        if (eligible.Count > 0) {
          foreach (var entry in eligible) {
            await LoadRegistrationAsync(managed, entry);
            RestoreActiveContext();
            pending.Remove(entry);
          }
          continue;
        }
        if (stage != InitializationStage.PostInit) return;
        if (RemoveUnavailableOptionalComponents(managed, pending)) continue;
        var fallback = pending.Where(entry => RequiredDependenciesSatisfied(managed, entry, pending))
          .OrderBy(static x => x.order)
          .ThenBy(static x => x.name, StringComparer.Ordinal)
          .FirstOrDefault();
        if (fallback == null) return;
        await LoadRegistrationAsync(managed, fallback);
        RestoreActiveContext();
        pending.Remove(fallback);
      }
    }

    private async UniTask LoadRegistrationAsync(ManagedScope managed, ComponentRegistration entry) {
      RestoreActiveContext();
      var context = new ComponentLoadContext(_container, managed, entry, this);
      try {
        var instance = Activate(entry, context);
        managed.RecordComponent(entry, instance);
        managed.BindComponent(entry, instance);
        entry.InitializeSync(instance, context);
        await entry.InitializeAsync(instance, context);
        RestoreActiveContext();
        entry.InitializeLate(instance, context);
        ValidateAndCompleteComponent(managed, entry, instance);
      } catch (Exception exception) {
        if (exception is ComponentContainerException) throw;
        throw new ComponentInitializationException($"Failed to initialize component '{entry.name}'.", exception);
      }
    }
  }
}
