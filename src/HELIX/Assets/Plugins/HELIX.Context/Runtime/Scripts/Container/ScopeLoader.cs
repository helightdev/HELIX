using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace HELIX.Context {
  /// <summary>Reusable container service that bounds transient dependency evidence to one scope load.</summary>
  internal sealed partial class ScopeLoader {
    private static readonly AsyncLocal<ScopeLoader> _active = new();
    private readonly Dictionary<string, HashSet<RegistrationEntry>> _publications = new();
    private readonly HashSet<string> _anonymousPublications = new(StringComparer.Ordinal);
    private readonly HashSet<IScriptedDependency> _scripted = new(ReferenceComparer<IScriptedDependency>.Instance);
    private readonly ManagedContainer _container;
    private readonly RegistrarGraph _graph;
    private readonly ScopeRules _rules;
    private ManagedScope _scope;

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

    public void Publish(RegistrationEntry owner, string wireKey) {
      if (!_publications.TryGetValue(wireKey, out var owners)) _publications.Add(wireKey, owners = new());
      owners.Add(owner);
    }

    public bool HasPublication(string wireKey) => !string.IsNullOrEmpty(wireKey) &&
      (_anonymousPublications.Contains(wireKey) ||
        _publications.TryGetValue(wireKey, out var owners) && owners.Count > 0);

    public bool WasPublishedBy(RegistrationEntry owner, string wireKey) => wireKey != null &&
      _publications.TryGetValue(wireKey, out var owners) && owners.Contains(owner);

    public IEnumerable<string> PublicationsBy(RegistrationEntry owner) => _publications
      .Where(pair => pair.Value.Contains(owner)).Select(static pair => pair.Key);

    internal void ValidateScope(ManagedScope parent, IScope child) {
      _rules.Validate(
        new ScopeValidationContext(
          parent,
          child,
          _container.registrarScope.registrations,
          _container.scopes.Keys,
          _container.applicationScope
        )
      );
    }

    private static IEnumerable<ComponentDependency> EnumerateImplicitScripted(
      IEnumerable<RegistrationEntry> entries,
      InitializationStage stage
    ) => entries
      .SelectMany(static x => x.dependencies)
      .Where(x => x.IsScripted && x.scripted.Stage == stage && x.flags.HasFlag(DependencyFlags.ImplicitLoadable))
      .OrderBy(static x => x.scripted.Order);

    private bool DependenciesSatisfied(
      ManagedScope managed,
      RegistrationEntry entry,
      IReadOnlyList<RegistrationEntry> allEntries
    ) => entry.dependencies.All(dependency => {
        if (!dependency.flags.HasFlag(DependencyFlags.Required)) return true;
        var localProvider = _graph.HasLocalProvider(allEntries, dependency);
        return localProvider
          ? managed.HasLocalDependency(dependency)
          : managed.HasDependency(dependency);
      }
    );

    private static void ValidateRequiredPublications(ManagedScope managed, RegistrationEntry entry) {
      foreach (var publication in entry.publications.Where(static x =>
        x.flags.HasFlag(DependencyFlags.Required)
      )) {
        if (managed.WasProvidedBy(entry, publication)) continue;
        throw new ComponentInitializationException(
          $"Component '{entry.name}' did not provide required publication '{publication.wireKey}'."
        );
      }
    }

    private static void EnsureFullyLoaded(ManagedScope managed, IReadOnlyCollection<RegistrationEntry> pending) {
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
      _active.Value = null;
      _publications.Clear();
      _anonymousPublications.Clear();
      _scripted.Clear();
    }
  }

  internal sealed partial class ScopeLoader { // Sync
    internal void LoadSync(ManagedScope managed) {
      if (_scope != null) throw new ScopeLifecycleException("The scope loader is already loading a scope.");
      _scope = managed;
      _active.Value = this;
      try {
        var entries = _graph.For(managed);
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
        var pending = new List<RegistrationEntry>(entries);
        foreach (InitializationStage stage in Enum.GetValues(typeof(InitializationStage))) {
          LoadScriptedDependenciesSync(managed, entries, stage);
          if (stage != InitializationStage.PreInit) LoadEligibleSync(managed, entries, pending);
        }
        EnsureFullyLoaded(managed, pending);
      } finally {
        Reset();
      }
    }

    private void LoadScriptedDependenciesSync(
      ManagedScope managed,
      IEnumerable<RegistrationEntry> entries,
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
      IReadOnlyList<RegistrationEntry> allEntries,
      List<RegistrationEntry> pending
    ) {
      while (true) {
        var eligible = pending.Where(entry => DependenciesSatisfied(managed, entry, allEntries))
          .OrderBy(static x => x.order)
          .ThenBy(static x => x.name, StringComparer.Ordinal)
          .ToList();
        if (eligible.Count == 0) return;
        foreach (var entry in eligible) {
          LoadRegistrationSync(managed, entry);
          pending.Remove(entry);
        }
      }
    }

    private void LoadRegistrationSync(ManagedScope managed, RegistrationEntry entry) {
      var context = new ComponentLoadContext(_container, managed, entry, this);
      try {
        var instance = entry.Activate(context);
        managed.RecordComponent(entry, instance);
        managed.BindComponent(entry, instance);
        entry.InitializeSync(instance, context);
        ValidateRequiredPublications(managed, entry);
      } catch (Exception exception) {
        if (exception is ComponentContainerException) throw;
        throw new ComponentInitializationException($"Failed to initialize component '{entry.name}'.", exception);
      }
    }
  }

  internal sealed partial class ScopeLoader { // Async
    internal async UniTask LoadAsync(ManagedScope managed) {
      if (_scope != null) throw new ScopeLifecycleException("The scope loader is already loading a scope.");
      _scope = managed;
      _active.Value = this;
      try {
        var entries = _graph.For(managed);
        var pending = new List<RegistrationEntry>(entries);
        foreach (InitializationStage stage in Enum.GetValues(typeof(InitializationStage))) {
          await LoadScriptedDependenciesAsync(managed, entries, stage);
          if (stage != InitializationStage.PreInit) await LoadEligibleAsync(managed, entries, pending);
        }
        EnsureFullyLoaded(managed, pending);
      } finally {
        Reset();
      }
    }

    private async UniTask LoadScriptedDependenciesAsync(
      ManagedScope managed,
      IEnumerable<RegistrationEntry> entries,
      InitializationStage stage
    ) {
      foreach (var dependency in EnumerateImplicitScripted(entries, stage)) {
        if (managed.IsScriptedLoaded(dependency.scripted) || managed.IsDependencyAvailable(dependency)) continue;
        var result = await dependency.scripted.LoadAsync(new ComponentLoadContext(_container, managed, null, this));
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
      IReadOnlyList<RegistrationEntry> allEntries,
      List<RegistrationEntry> pending
    ) {
      while (true) {
        var eligible = pending.Where(entry => DependenciesSatisfied(managed, entry, allEntries))
          .OrderBy(static x => x.order)
          .ThenBy(static x => x.name, StringComparer.Ordinal)
          .ToList();
        if (eligible.Count == 0) return;
        foreach (var entry in eligible) {
          await LoadRegistrationAsync(managed, entry);
          pending.Remove(entry);
        }
      }
    }

    private async UniTask LoadRegistrationAsync(ManagedScope managed, RegistrationEntry entry) {
      var context = new ComponentLoadContext(_container, managed, entry, this);
      try {
        var instance = entry.Activate(context);
        managed.RecordComponent(entry, instance);
        managed.BindComponent(entry, instance);
        entry.InitializeSync(instance, context);
        await entry.InitializeAsync(instance, context);
        ValidateRequiredPublications(managed, entry);
      } catch (Exception exception) {
        if (exception is ComponentContainerException) throw;
        throw new ComponentInitializationException($"Failed to initialize component '{entry.name}'.", exception);
      }
    }
  }
}
