using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace HELIX.Context {
  /// <summary>Reusable container service that bounds transient dependency evidence to one scope load.</summary>
  internal sealed partial class ScopeLoader {
    private static readonly AsyncLocal<ScopeLoader> _active = new();
    public readonly Dictionary<string, HashSet<ComponentRegistration>> publications = new();
    public readonly HashSet<string> anonymousPublications = new(StringComparer.Ordinal);
    public readonly HashSet<IScriptedDependency> scripted = new(ReferenceComparer<IScriptedDependency>.Instance);
    private readonly ManagedContainer _container;
    private readonly RegistrarGraph _graph;
    private ManagedScope _scope;
    private Dictionary<ComponentRegistration, Queue<object>> _injected;

    public ScopeLoader(ManagedContainer container, RegistrarGraph graph) {
      _container = container ?? throw new ArgumentNullException(nameof(container));
      _graph = graph ?? throw new ArgumentNullException(nameof(graph));
    }

    public static ScopeLoader Active => _active.Value ??
      throw new ScopeLifecycleException("No scope is currently loading.");
    public static ScopeLoader ActiveOrNull => _active.Value;

    public void Publish(ComponentRegistration owner, string wireKey) {
      if (!publications.TryGetValue(wireKey, out var owners))
        publications.Add(wireKey, owners = new HashSet<ComponentRegistration>());
      owners.Add(owner);
    }

    private static IEnumerable<ComponentDependency> EnumerateImplicitScripted(
      IEnumerable<ComponentRegistration> entries,
      int phase
    ) => entries
      .SelectMany(static x => x.dependencies)
      .Where(x => x.IsScripted && x.scripted.Phase == phase && x.flags.HasFlag(DependencyFlags.ImplicitLoadable))
      .OrderBy(static x => x.scripted.Order);

    private static IEnumerable<int> EnumeratePhases(IEnumerable<ComponentRegistration> entries) {
      return entries
        .Select(static entry => entry.phase)
        .Concat(entries.SelectMany(static entry => entry.dependencies)
          .Where(static dependency => dependency.IsScripted)
          .Select(static dependency => dependency.scripted.Phase))
        .Append(InitPhase.PreInit)
        .Append(InitPhase.Normal)
        .Append(InitPhase.PostInit)
        .Distinct()
        .OrderBy(static phase => phase);
    }

    private sealed class PendingProviderIndex {
      private readonly Dictionary<string, int> _counts = new(StringComparer.Ordinal);

      public PendingProviderIndex(IEnumerable<ComponentRegistration> entries) {
        foreach (var entry in entries) Add(entry, 1);
      }

      public bool Contains(ComponentDependency dependency) {
        return dependency.wireKey != null && _counts.TryGetValue(dependency.wireKey, out var count) && count > 0;
      }

      public void Remove(ComponentRegistration entry) => Add(entry, -1);

      private void Add(ComponentRegistration entry, int amount) {
        foreach (var wireKey in entry.keys.Select(static key => key.CreateWireKey())
          .Concat(entry.publications.Select(static publication => publication.wireKey))
          .Where(static wireKey => wireKey != null)
          .Distinct(StringComparer.Ordinal)) {
          _counts.TryGetValue(wireKey, out var count);
          _counts[wireKey] = count + amount;
        }
      }
    }

    private static bool RequiredDependenciesSatisfied(
      ManagedScope managed,
      ComponentRegistration entry,
      PendingProviderIndex pendingProviders
    ) => entry.dependencies.All(dependency => {
        if (!dependency.flags.HasFlag(DependencyFlags.Required)) return true;
        var hasLocalProvider = pendingProviders.Contains(dependency) || managed.HasLocalDependency(dependency);
        return hasLocalProvider ? managed.HasLocalDependency(dependency) : managed.HasDependency(dependency);
      }
    );

    private static bool RemoveUnavailableOptionalComponents(
      ManagedScope managed,
      List<ComponentRegistration> pending,
      PendingProviderIndex pendingProviders
    ) {
      var unavailable = pending.FirstOrDefault(entry =>
        entry.optional && !RequiredDependenciesSatisfied(managed, entry, pendingProviders)
      );
      if (unavailable == null) return false;
      pending.Remove(unavailable);
      pendingProviders.Remove(unavailable);
      return true;
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

    private static void EnsureFullyLoaded(
      ManagedScope managed,
      IReadOnlyCollection<ComponentRegistration> pending,
      int phase
    ) {
      if (pending.Count == 0) return;
      var details = pending.Select(entry => {
          var missing = entry.dependencies.Where(x => !managed.HasDependency(x))
            .Select(static x => x.wireKey ?? x.scripted?.GetType().FullName ?? "<unwired>");
          return $"{entry.name} -> [{string.Join(", ", missing)}]";
        }
      );
      throw new ComponentGraphException(
        $"Initialization phase {phase} cannot advance. It contains a cycle, a dependency assigned to a later " +
        "phase, or unresolved dynamic publications: " +
        string.Join("; ", details)
      );
    }

    private void Reset() {
      _scope = null;
      _injected = null;
      _active.Value = null;
      publications.Clear();
      anonymousPublications.Clear();
      scripted.Clear();
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
        foreach (var phase in EnumeratePhases(entries)) {
          LoadScriptedDependenciesSync(managed, entries, phase);
          LoadPhaseSync(managed, entries.Where(entry => entry.phase == phase).ToList(), phase);
        }
      } finally {
        Reset();
      }
    }

    private void LoadScriptedDependenciesSync(
      ManagedScope managed,
      IEnumerable<ComponentRegistration> entries,
      int phase
    ) {
      foreach (var dependency in EnumerateImplicitScripted(entries, phase)) {
        if (scripted.Contains(dependency.scripted) || managed.IsDependencyAvailable(dependency)) continue;
        var result = dependency.scripted.Load(new ComponentLoadContext(_container, managed, null, this));
        if (!result.success) {
          if (dependency.flags.HasFlag(DependencyFlags.Required)) {
            throw new ComponentInitializationException(
              $"Scripted dependency '{dependency.wireKey}' failed during phase {phase}."
            );
          }
          continue;
        }
        if (result.value != null) managed.Publish(null, result.value.GetType(), result.value);
        scripted.Add(dependency.scripted);
        if (dependency.flags.HasFlag(DependencyFlags.Wirable)) anonymousPublications.Add(dependency.wireKey);
      }
    }

    private void LoadPhaseSync(
      ManagedScope managed,
      List<ComponentRegistration> pending,
      int phase
    ) {
      if (pending.Count == 0) return;
      var pendingProviders = new PendingProviderIndex(pending);
      var iteration = 0;
      while (pending.Count > 0) {
        EnsureIterationAvailable(iteration++, managed);
        var loadIndex = RequiredDependenciesSatisfied(managed, pending[0], pendingProviders)
          ? 0
          : pending.FindIndex(1, entry => RequiredDependenciesSatisfied(managed, entry, pendingProviders));
        if (loadIndex >= 0) {
          var entry = pending[loadIndex];
          LoadRegistrationSync(managed, entry);
          pending.RemoveAt(loadIndex);
          pendingProviders.Remove(entry);
          continue;
        }
        if (RemoveUnavailableOptionalComponents(managed, pending, pendingProviders)) continue;
        EnsureFullyLoaded(managed, pending, phase);
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
        foreach (var phase in EnumeratePhases(entries)) {
          await LoadScriptedDependenciesAsync(managed, entries, phase);
          RestoreActiveContext();
          await LoadPhaseAsync(managed, entries.Where(entry => entry.phase == phase).ToList(), phase);
          RestoreActiveContext();
        }
      } finally {
        Reset();
      }
    }

    private async UniTask LoadScriptedDependenciesAsync(
      ManagedScope managed,
      IEnumerable<ComponentRegistration> entries,
      int phase
    ) {
      foreach (var dependency in EnumerateImplicitScripted(entries, phase)) {
        if (scripted.Contains(dependency.scripted) || managed.IsDependencyAvailable(dependency)) continue;
        var result = await dependency.scripted.LoadAsync(new ComponentLoadContext(_container, managed, null, this));
        RestoreActiveContext();
        if (!result.success) {
          if (dependency.flags.HasFlag(DependencyFlags.Required)) {
            throw new ComponentInitializationException(
              $"Scripted dependency '{dependency.wireKey}' failed during phase {phase}."
            );
          }
          continue;
        }
        if (result.value != null) managed.Publish(null, result.value.GetType(), result.value);
        scripted.Add(dependency.scripted);
        if (dependency.flags.HasFlag(DependencyFlags.Wirable)) anonymousPublications.Add(dependency.wireKey);
      }
    }

    private async UniTask LoadPhaseAsync(
      ManagedScope managed,
      List<ComponentRegistration> pending,
      int phase
    ) {
      if (pending.Count == 0) return;
      var pendingProviders = new PendingProviderIndex(pending);
      var iteration = 0;
      while (pending.Count > 0) {
        EnsureIterationAvailable(iteration++, managed);
        var loadIndex = RequiredDependenciesSatisfied(managed, pending[0], pendingProviders)
          ? 0
          : pending.FindIndex(1, entry => RequiredDependenciesSatisfied(managed, entry, pendingProviders));
        if (loadIndex >= 0) {
          var entry = pending[loadIndex];
          await LoadRegistrationAsync(managed, entry);
          RestoreActiveContext();
          pending.RemoveAt(loadIndex);
          pendingProviders.Remove(entry);
          continue;
        }
        if (RemoveUnavailableOptionalComponents(managed, pending, pendingProviders)) continue;
        EnsureFullyLoaded(managed, pending, phase);
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
