using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HELIX.Context {
  /// <summary>Container identity, retained state, and construction surface.</summary>
  public sealed class ManagedContainer : IDisposable {
    public readonly Dictionary<IScope, ManagedScope> scopes = new(ReferenceComparer<IScope>.Instance);
    public readonly RegistrarScope registrarScope = new();
    public readonly ApplicationScope applicationScope = new();
    private readonly ScopeRules _scopeRules;
    private readonly ScopeLoader _scopeLoader = new();
    private readonly HashSet<IScope> _disposedScopes = new(ReferenceComparer<IScope>.Instance);
    private readonly HashSet<IScope> _creatingScopes = new(ReferenceComparer<IScope>.Instance);
    private bool _registrarPrepared, _applicationStarted, _disposed;

    public ManagedContainer() : this(ScopeRules.Default) { }

    public ManagedContainer(ScopeRules scopeRules) {
      _scopeRules = scopeRules ?? throw new ArgumentNullException(nameof(scopeRules));
      SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    public ManagedContainer(RegistrationDiscoveryProvider discoveryProvider) : this() {
      if (discoveryProvider == null) throw new ArgumentNullException(nameof(discoveryProvider));
      try { PrepareRegistrar(discoveryProvider()); } catch {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        _disposed = true;
        throw;
      }
    }

    public ManagedScope Registrar => GetScope(registrarScope);
    public ManagedScope Application => GetScope(applicationScope);

    public void PrepareRegistrar(ComponentRegistrations registrations) {
      ThrowIfDisposed();
      if (_registrarPrepared) throw new ScopeLifecycleException("The registrar has already been prepared.");
      registrarScope.registrations = registrations ?? throw new ArgumentNullException(nameof(registrations));
      ValidateRegistrations(registrations);

      var registrar = new ManagedScope(registrarScope) { State = ManagedScopeState.Initializing };
      scopes.Add(registrarScope, registrar);
      try {
        LoadScopeSync(registrar);
        registrar.State = ManagedScopeState.Active;
        _registrarPrepared = true;
      } catch (Exception exception) {
        var cleanupFailures = RollbackScope(registrar);
        scopes.Remove(registrarScope);
        ThrowWithCleanupFailures(
          exception,
          cleanupFailures,
          "Registrar initialization failed and rollback had failures."
        );
        throw;
      }
    }

    public async UniTask StartApplication() {
      EnsureApplicationCanStart();
      await CreateScope(Registrar, applicationScope);
      _applicationStarted = true;
      HX.container = this;
    }

    public ManagedScope StartApplicationSync() {
      EnsureApplicationCanStart();
      var scope = CreateScopeSync(Registrar, applicationScope);
      _applicationStarted = true;
      HX.container = this;
      return scope;
    }

    public async UniTask<ManagedScope> CreateScope(ManagedScope parent, IScope scope) {
      var managed = BeginScopeCreation(parent, scope);
      try {
        await LoadScopeAsync(managed);
        CommitScope(managed);
        return managed;
      } catch (Exception exception) {
        var cleanupFailures = RollbackScope(managed);
        ThrowWithCleanupFailures(exception, cleanupFailures, "Scope initialization failed and rollback had failures.");
        throw;
      }
    }

    public ManagedScope CreateScopeSync(ManagedScope parent, IScope scope) {
      var managed = BeginScopeCreation(parent, scope);
      try {
        LoadScopeSync(managed);
        CommitScope(managed);
        return managed;
      } catch (Exception exception) {
        var cleanupFailures = RollbackScope(managed);
        ThrowWithCleanupFailures(exception, cleanupFailures, "Scope initialization failed and rollback had failures.");
        throw;
      }
    }

    public ManagedScope GetScope(IScope scope) {
      if (scope != null && scopes.TryGetValue(scope, out var managed)) return managed;
      throw new ScopeLifecycleException("The requested scope is not managed by this container.");
    }

    public bool TryGetScope(IScope scope, out ManagedScope managed) {
      if (scope != null) return scopes.TryGetValue(scope, out managed);
      managed = null;
      return false;
    }

    public void DisposeScope(IScope scope) {
      if (scope != null && _disposedScopes.Contains(scope)) return;
      ThrowIfDisposed();
      var managed = GetScope(scope);
      if (managed is null or { State: ManagedScopeState.Disposed or ManagedScopeState.Disposing }) return;
      ValidateManagedScope(managed);
      if (ReferenceEquals(managed.scope, registrarScope)) {
        throw new ScopeLifecycleException("Dispose the registrar through HXContainer.Dispose().");
      }
      var failures = new List<Exception>();
      DisposeScopeCore(managed, failures);
      if (failures.Count > 0) {
        throw new AggregateException($"Scope {managed.scope.GetType().Name} was disposed with failures.", failures);
      }
    }

    public void Dispose() {
      if (_disposed) return;
      var failures = new List<Exception>();
      if (scopes.TryGetValue(registrarScope, out var registrar)) {
        foreach (var child in registrar.ManagedChildren.ToArray()) DisposeScopeCore(child, failures);
        registrar.State = ManagedScopeState.Disposing;
        registrar.Cancel(failures);
        registrar.UnloadComponents(failures);
        registrar.DisposeOwnedResources(failures);
        registrar.DestroyOwnedUnityObjects(failures);
        registrar.State = ManagedScopeState.Disposed;
      }
      scopes.Clear();
      SceneManager.sceneUnloaded -= OnSceneUnloaded;
      if (ReferenceEquals(HX.container, this)) HX.container = null;
      _disposed = true;
      if (failures.Count > 0) throw new AggregateException("Container disposal completed with failures.", failures);
    }

    internal void DisposeScopeFromUnity(IScope scope) {
      if (_disposed || scope == null || !scopes.TryGetValue(scope, out var managed)) return;
      var failures = new List<Exception>();
      DisposeScopeCore(managed, failures);
      foreach (var failure in failures) Debug.LogException(failure);
    }

    private ManagedScope BeginScopeCreation(ManagedScope parent, IScope scope) {
      ThrowIfDisposed();
      if (!_registrarPrepared) throw new ScopeLifecycleException("Prepare the registrar before creating scopes.");
      ValidateRegistrations(registrarScope.registrations);
      if (parent == null) throw new ArgumentNullException(nameof(parent));
      if (scope == null) throw new ArgumentNullException(nameof(scope));
      ValidateManagedScope(parent);
      if (parent.State != ManagedScopeState.Active) {
        throw new ScopeLifecycleException("A child can only be created beneath an active scope.");
      }
      if (scopes.ContainsKey(scope)) throw new ScopeLifecycleException("This scope instance is already managed.");
      if (_creatingScopes.Contains(scope))
        throw new ScopeLifecycleException("This scope instance is already initializing.");
      if (_disposedScopes.Contains(scope)) {
        throw new ScopeLifecycleException("A disposed scope instance cannot be reused. Create a new scope instance.");
      }
      ValidateScopeRelationship(parent, scope);
      _creatingScopes.Add(scope);
      return new ManagedScope(parent, scope) { State = ManagedScopeState.Initializing };
    }

    private void CommitScope(ManagedScope managed) {
      AttachUnityLifetime(managed);
      managed.parent.AddChild(managed);
      scopes.Add(managed.scope, managed);
      _creatingScopes.Remove(managed.scope);
      managed.State = ManagedScopeState.Active;
    }

    private void EnsureApplicationCanStart() {
      ThrowIfDisposed();
      if (!_registrarPrepared) {
        throw new ScopeLifecycleException("Prepare the registrar before starting the application.");
      }
      ValidateRegistrations(registrarScope.registrations);
      if (_applicationStarted || scopes.ContainsKey(applicationScope) || _creatingScopes.Contains(applicationScope)) {
        throw new ScopeLifecycleException("The application scope can only be started once per container.");
      }
    }

    private void ValidateManagedScope(ManagedScope managed) {
      if (managed == null || !scopes.TryGetValue(managed.scope, out var stored) || !ReferenceEquals(stored, managed)) {
        throw new ScopeLifecycleException("The supplied managed scope does not belong to this container.");
      }
    }

    private void ValidateScopeRelationship(ManagedScope parent, IScope child) {
      _scopeRules.Validate(
        new ScopeValidationContext(
          parent,
          child,
          registrarScope.registrations,
          scopes.Keys,
          applicationScope
        )
      );
    }

    private void ValidateRegistrationGraph(ManagedScope managed, IReadOnlyList<RegistrationEntry> entries) {
      var providers = new Dictionary<TypeKey, List<RegistrationEntry>>();
      foreach (var entry in entries) {
        if (entry.scope != null && !typeof(IScope).IsAssignableFrom(entry.scope)) {
          throw new ComponentGraphException(
            $"Component '{entry.name}' is assigned to {entry.scope.FullName}, which is not an IScope."
          );
        }
        foreach (var key in entry.keys.Distinct()) AddProvider(providers, key, entry);
        foreach (var publication in entry.publications.Where(static x =>
          x.IsTyped && x.flags.HasFlag(DependencyFlags.Required)
        )) {
          AddProvider(providers, publication.key, entry);
        }
      }

      foreach (var entry in entries) {
        foreach (var dependency in entry.dependencies.Where(static x => x.flags.HasFlag(DependencyFlags.Required))) {
          if (dependency.IsTyped) {
            if (providers.ContainsKey(dependency.key)) continue;
            if (managed.ResolveAll(dependency.key).Count > 0) continue;
            throw new ComponentGraphException(
              $"Component '{entry.name}' requires '{dependency.key}', but no visible component guarantees that key."
            );
          }
          if (dependency.scripted == null) {
            throw new ComponentGraphException($"Component '{entry.name}' contains an invalid untyped dependency.");
          }
          if (dependency.flags.HasFlag(DependencyFlags.ImplicitLoadable)) continue;
          if (dependency.flags.HasFlag(DependencyFlags.Wirable) &&
              HasPublicationProvider(entries, dependency.wireKey)) continue;
          throw new ComponentGraphException(
            $"Component '{entry.name}' requires scripted dependency '{dependency.wireKey}', but it is not implicitly " +
            "loadable and has no guaranteed publication provider."
          );
        }
      }
    }

    private static void ValidateRegistrations(ComponentRegistrations registrations) {
      foreach (var pair in registrations.components) {
        var entry = pair.Value;
        if (entry == null || pair.Key != entry.type) {
          throw new ComponentGraphException("The component registration index contains an invalid entry.");
        }
        if (entry.scope != null && !typeof(IScope).IsAssignableFrom(entry.scope)) {
          throw new ComponentGraphException(
            $"Component '{entry.name}' is assigned to {entry.scope.FullName}, which is not an IScope."
          );
        }
        if (entry.keys.Any(static x => x.type == null)) {
          throw new ComponentGraphException($"Component '{entry.name}' exposes an untyped key.");
        }
        foreach (var key in entry.keys) {
          if (!key.type.IsAssignableFrom(entry.type)) {
            throw new ComponentGraphException(
              $"Component '{entry.name}' of type {entry.type.FullName} cannot expose {key.type.FullName}."
            );
          }
        }
      }

      foreach (var pair in registrations.scopes) {
        var registration = pair.Value;
        if (registration == null || pair.Key != registration.type ||
            !typeof(IScope).IsAssignableFrom(pair.Key)) {
          throw new ComponentGraphException("The scope registration index contains an invalid entry.");
        }
        if (registration.parentTypes.Any(static x => !typeof(IScope).IsAssignableFrom(x)) ||
            registration.parentType != null && !typeof(IScope).IsAssignableFrom(registration.parentType)) {
          throw new ComponentGraphException($"Scope {pair.Key.FullName} contains a parent type that is not an IScope.");
        }
      }
    }

    private void LoadScopeSync(ManagedScope managed) {
      _scopeLoader.Load(
        managed,
        () => {
          var entries = SelectRegistrations(managed);
          ValidateRegistrationGraph(managed, entries);
          EnsureSynchronous(entries);
          LoadStagesSync(managed, entries);
        }
      );
    }

    private async UniTask LoadScopeAsync(ManagedScope managed) {
      await _scopeLoader.LoadAsync(
        managed,
        async () => {
          var entries = SelectRegistrations(managed);
          ValidateRegistrationGraph(managed, entries);
          await LoadStagesAsync(managed, entries);
        }
      );
    }

    private List<RegistrationEntry> SelectRegistrations(ManagedScope managed) {
      var scopeType = managed.scope.GetType();
      return registrarScope.registrations.components.Values
        .Where(entry => (entry.scope ?? typeof(ApplicationScope)) == scopeType)
        .OrderBy(static x => x.name, StringComparer.Ordinal)
        .ToList();
    }

    private static void EnsureSynchronous(IEnumerable<RegistrationEntry> entries) {
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
    }

    private void LoadStagesSync(ManagedScope managed, List<RegistrationEntry> entries) {
      var pending = new List<RegistrationEntry>(entries);
      foreach (InitializationStage stage in Enum.GetValues(typeof(InitializationStage))) {
        LoadScriptedDependenciesSync(managed, entries, stage);
        if (stage != InitializationStage.PreInit) LoadEligibleSync(managed, entries, pending);
      }
      EnsureFullyLoaded(managed, pending);
    }

    private async UniTask LoadStagesAsync(ManagedScope managed, List<RegistrationEntry> entries) {
      var pending = new List<RegistrationEntry>(entries);
      foreach (InitializationStage stage in Enum.GetValues(typeof(InitializationStage))) {
        await LoadScriptedDependenciesAsync(managed, entries, stage);
        if (stage != InitializationStage.PreInit) await LoadEligibleAsync(managed, entries, pending);
      }
      EnsureFullyLoaded(managed, pending);
    }

    private void LoadScriptedDependenciesSync(
      ManagedScope managed,
      IEnumerable<RegistrationEntry> entries,
      InitializationStage stage
    ) {
      foreach (var dependency in EnumerateImplicitScripted(entries, stage)) {
        if (managed.IsScriptedLoaded(dependency.scripted) || managed.IsDependencyAvailable(dependency)) continue;
        var result = dependency.scripted.Load(new ComponentLoadContext(this, managed, null, _scopeLoader));
        if (!result.success) {
          if (dependency.flags.HasFlag(DependencyFlags.Required)) {
            throw new ComponentInitializationException(
              $"Scripted dependency '{dependency.wireKey}' failed during {stage}."
            );
          }
          continue;
        }
        MaterializeScriptedResult(managed, result);
        managed.MarkScriptedLoaded(dependency.scripted);
      }
    }

    private async UniTask LoadScriptedDependenciesAsync(
      ManagedScope managed,
      IEnumerable<RegistrationEntry> entries,
      InitializationStage stage
    ) {
      foreach (var dependency in EnumerateImplicitScripted(entries, stage)) {
        if (managed.IsScriptedLoaded(dependency.scripted) || managed.IsDependencyAvailable(dependency)) continue;
        var result = await dependency.scripted.LoadAsync(new ComponentLoadContext(this, managed, null, _scopeLoader));
        if (!result.success) {
          if (dependency.flags.HasFlag(DependencyFlags.Required)) {
            throw new ComponentInitializationException(
              $"Scripted dependency '{dependency.wireKey}' failed during {stage}."
            );
          }
          continue;
        }
        MaterializeScriptedResult(managed, result);
        managed.MarkScriptedLoaded(dependency.scripted);
      }
    }

    private static IEnumerable<ComponentDependency> EnumerateImplicitScripted(
      IEnumerable<RegistrationEntry> entries,
      InitializationStage stage
    ) => entries
      .SelectMany(static x => x.dependencies)
      .Where(x => x.IsScripted && x.scripted.Stage == stage && x.flags.HasFlag(DependencyFlags.ImplicitLoadable))
      .OrderBy(static x => x.scripted.Order);

    private static void MaterializeScriptedResult(ManagedScope scope, ComponentLoadResult result) {
      if (result.value != null) scope.Publish(null, result.value.GetType(), result.value);
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

    private static bool DependenciesSatisfied(
      ManagedScope managed,
      RegistrationEntry entry,
      IReadOnlyList<RegistrationEntry> allEntries
    ) => entry.dependencies.All(dependency => {
        if (!dependency.flags.HasFlag(DependencyFlags.Required)) return true;
        return HasGuaranteedLocalProvider(allEntries, dependency)
          ? managed.HasLocalDependency(dependency)
          : managed.HasDependency(dependency);
      }
    );

    private void LoadRegistrationSync(ManagedScope managed, RegistrationEntry entry) {
      var context = new ComponentLoadContext(this, managed, entry, _scopeLoader);
      object instance = null;
      try {
        instance = entry.Activate(context);
        managed.RecordComponent(entry, instance);
        managed.BindComponent(entry, instance);
        entry.InitializeSync(instance, context);
        ValidateRequiredPublications(managed, entry);
      } catch (Exception exception) {
        if (exception is ComponentContainerException) throw;
        throw new ComponentInitializationException($"Failed to initialize component '{entry.name}'.", exception);
      }
    }

    private async UniTask LoadRegistrationAsync(ManagedScope managed, RegistrationEntry entry) {
      var context = new ComponentLoadContext(this, managed, entry, _scopeLoader);
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

    private List<Exception> RollbackScope(ManagedScope managed) {
      managed.State = ManagedScopeState.Faulted;
      var failures = new List<Exception>();
      TryDetachUnityLifetime(managed, failures);
      managed.Cancel(failures);
      managed.UnloadComponents(failures);
      managed.DisposeOwnedResources(failures);
      managed.DestroyOwnedUnityObjects(failures);
      managed.parent?.RemoveChild(managed);
      scopes.Remove(managed.scope);
      _creatingScopes.Remove(managed.scope);
      return failures;
    }

    private void DisposeScopeCore(ManagedScope managed, List<Exception> failures) {
      if (managed.State is ManagedScopeState.Disposed or ManagedScopeState.Disposing) return;
      managed.State = ManagedScopeState.Disposing;
      managed.Cancel(failures);
      foreach (var child in managed.ManagedChildren.ToArray()) DisposeScopeCore(child, failures);
      TryDetachUnityLifetime(managed, failures);
      managed.UnloadComponents(failures);
      managed.DisposeOwnedResources(failures);
      managed.DestroyOwnedUnityObjects(failures);
      managed.parent?.RemoveChild(managed);
      scopes.Remove(managed.scope);
      _disposedScopes.Add(managed.scope);
      managed.State = ManagedScopeState.Disposed;
    }

    private void AttachUnityLifetime(ManagedScope managed) {
      if (managed.scope is not GameObjectScope gameObjectScope || gameObjectScope.gameObject == null) return;
      var observer = gameObjectScope.gameObject.AddComponent<GameObjectScopeObserver>();
      observer.Attach(this, managed.scope);
    }

    private static void DetachUnityLifetime(ManagedScope managed) {
      if (managed.scope is not GameObjectScope gameObjectScope || gameObjectScope.gameObject == null) return;
      foreach (var observer in gameObjectScope.gameObject.GetComponents<GameObjectScopeObserver>()) {
        if (!observer.Observes(managed.scope)) continue;
        observer.Detach();
        if (UnityEngine.Application.isPlaying) UnityEngine.Object.Destroy(observer);
        else UnityEngine.Object.DestroyImmediate(observer);
      }
    }

    private static void TryDetachUnityLifetime(ManagedScope managed, List<Exception> failures) {
      try {
        DetachUnityLifetime(managed);
      } catch (Exception exception) {
        failures.Add(new ComponentDeinitializationException("Failed to detach a Unity scope lifetime.", exception));
      }
    }

    private void OnSceneUnloaded(Scene scene) {
      var matching = scopes.Values.Where(x => x.scope is SceneScope sceneScope && sceneScope.scene == scene).ToArray();
      foreach (var managed in matching) DisposeScopeFromUnity(managed.scope);
    }

    private static void AddProvider(
      IDictionary<TypeKey, List<RegistrationEntry>> providers,
      TypeKey key,
      RegistrationEntry entry
    ) {
      if (key.type == null) throw new ComponentGraphException($"Component '{entry.name}' exposes an untyped key.");
      if (!providers.TryGetValue(key, out var values)) {
        values = new List<RegistrationEntry>();
        providers[key] = values;
      }
      values.Add(entry);
    }

    private static bool HasPublicationProvider(IEnumerable<RegistrationEntry> entries, string wireKey) =>
      entries.Any(entry => entry.publications.Any(publication =>
          publication.flags.HasFlag(DependencyFlags.Required) && publication.wireKey == wireKey
        )
      );

    private static bool HasGuaranteedLocalProvider(
      IEnumerable<RegistrationEntry> entries,
      ComponentDependency dependency
    ) {
      if (dependency.IsTyped) {
        return entries.Any(entry => entry.keys.Contains(dependency.key) || entry.publications.Any(publication =>
            publication.IsTyped && publication.key.Equals(dependency.key) &&
            publication.flags.HasFlag(DependencyFlags.Required)
          )
        );
      }
      return dependency.flags.HasFlag(DependencyFlags.Wirable) &&
             HasPublicationProvider(entries, dependency.wireKey);
    }

    private void ThrowIfDisposed() {
      if (_disposed) throw new ObjectDisposedException(nameof(ManagedContainer));
    }

    private static void ThrowWithCleanupFailures(
      Exception original,
      List<Exception> cleanupFailures,
      string message
    ) {
      if (cleanupFailures.Count == 0) return;
      cleanupFailures.Insert(0, original);
      throw new AggregateException(message, cleanupFailures);
    }
  }
}