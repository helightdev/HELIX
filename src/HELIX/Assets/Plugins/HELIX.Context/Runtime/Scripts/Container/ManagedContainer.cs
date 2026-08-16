using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HELIX.Context {
  /// <summary>Container identity, retained state, and construction surface.</summary>
  public sealed class ManagedContainer : IDisposable {
    public const int DefaultMaxLoadingIterations = 1024;
    public readonly Dictionary<IScope, ManagedScope> scopes = new(ReferenceComparer<IScope>.Instance);
    public readonly RegistrarScope registrarScope = new();
    public readonly ApplicationScope applicationScope = new();
    private readonly RegistrarGraph _registrarGraph = new();
    private readonly ScopeLoader _scopeLoader;
    private readonly HashSet<IScope> _disposedScopes = new(ReferenceComparer<IScope>.Instance);
    private readonly HashSet<IScope> _creatingScopes = new(ReferenceComparer<IScope>.Instance);
    private bool _registrarPrepared, _applicationStarted, _disposed;

    /// <summary>Maximum dependency-resolution passes allowed while initializing one scope.</summary>
    public int maxLoadingIterations = DefaultMaxLoadingIterations;

    public ManagedContainer() : this(ScopeRules.Default) { }

    public ManagedContainer(ScopeRules scopeRules) {
      _scopeLoader = new ScopeLoader(this, _registrarGraph, scopeRules);
      SceneManager.sceneLoaded += OnSceneLoaded;
      SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    public ManagedContainer(RegistrationDiscoveryProvider discoveryProvider) : this() {
      if (discoveryProvider == null) throw new ArgumentNullException(nameof(discoveryProvider));
      try { PrepareRegistrar(discoveryProvider()); } catch {
        UnsubscribeSceneEvents();
        _disposed = true;
        throw;
      }
    }

    public ManagedScope Registrar => GetScope(registrarScope);
    public ManagedScope Application => GetScope(applicationScope);

    public void PrepareRegistrar(ComponentRegistrations registrations) {
      ThrowIfDisposed();
      if (_registrarPrepared) throw new ScopeLifecycleException("The registrar has already been prepared.");
      registrarScope.registrations = _registrarGraph.Prepare(registrations);
      var registrar = new ManagedScope(registrarScope);
      registrar.BeginInitialization();
      scopes.Add(registrarScope, registrar);
      try {
        _scopeLoader.LoadSync(registrar);
        registrar.Activate();
        _registrarPrepared = true;
      } catch (Exception exception) {
        var cleanupFailures = registrar.Rollback();
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
      var application = await CreateScope(Registrar, applicationScope);
      _applicationStarted = true;
      HX.container = this;
      foreach (var scene in LoadedScenesWithoutScope()) await CreateSceneScope(application, scene);
    }

    public ManagedScope StartApplicationSync() {
      EnsureApplicationCanStart();
      var scope = CreateScopeSync(Registrar, applicationScope);
      _applicationStarted = true;
      HX.container = this;
      foreach (var scene in LoadedScenesWithoutScope()) CreateSceneScopeSync(scope, scene);
      return scope;
    }

    public async UniTask<ManagedScope> CreateScope(ManagedScope parent, IScope scope) {
      return await CreateScope(parent, scope, null);
    }

    public async UniTask<ManagedScope> CreateScope(
      ManagedScope parent,
      IScope scope,
      IEnumerable<IComponent> components
    ) {
      var managed = BeginScopeCreation(parent, scope);
      try {
        await _scopeLoader.LoadAsync(managed, components);
        CommitScope(managed);
        return managed;
      } catch (Exception exception) {
        var cleanupFailures = managed.Rollback();
        ForgetCreatingScope(managed);
        ThrowWithCleanupFailures(exception, cleanupFailures, "Scope initialization failed and rollback had failures.");
        throw;
      }
    }

    public ManagedScope CreateScopeSync(ManagedScope parent, IScope scope) {
      return CreateScopeSync(parent, scope, null);
    }

    public ManagedScope CreateScopeSync(
      ManagedScope parent,
      IScope scope,
      IEnumerable<IComponent> components
    ) {
      var managed = BeginScopeCreation(parent, scope);
      try {
        _scopeLoader.LoadSync(managed, components);
        CommitScope(managed);
        return managed;
      } catch (Exception exception) {
        var cleanupFailures = managed.Rollback();
        ForgetCreatingScope(managed);
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
      if (ReferenceEquals(managed.scope, registrarScope))
        throw new ScopeLifecycleException("Dispose the registrar through HXContainer.Dispose().");
      var failures = managed.Dispose(ForgetDisposedScope);
      if (failures.Count > 0)
        throw new AggregateException($"Scope {managed.scope.GetType().Name} was disposed with failures.", failures);
    }

    public void Dispose() {
      if (_disposed) return;
      List<Exception> failures = null;
      if (scopes.TryGetValue(registrarScope, out var registrar)) failures = registrar.Dispose(ForgetDisposedScope);
      scopes.Clear();
      UnsubscribeSceneEvents();
      if (ReferenceEquals(HX.container, this)) HX.container = null;
      _disposed = true;
      if (failures is { Count: > 0 })
        throw new AggregateException("Container disposal completed with failures.", failures);
    }

    internal void DisposeScopeFromUnity(IScope scope) {
      if (_disposed || scope == null || !scopes.TryGetValue(scope, out var managed)) return;
      var failures = managed.Dispose(ForgetDisposedScope);
      foreach (var failure in failures) Debug.LogException(failure);
    }

    private ManagedScope BeginScopeCreation(ManagedScope parent, IScope scope) {
      ThrowIfDisposed();
      if (!_registrarPrepared) throw new ScopeLifecycleException("Prepare the registrar before creating scopes.");
      if (parent == null) throw new ArgumentNullException(nameof(parent));
      if (scope == null) throw new ArgumentNullException(nameof(scope));
      ValidateManagedScope(parent);
      if (parent.State != ManagedScopeState.Active)
        throw new ScopeLifecycleException("A child can only be created beneath an active scope.");
      if (scopes.ContainsKey(scope)) throw new ScopeLifecycleException("This scope instance is already managed.");
      if (_creatingScopes.Contains(scope))
        throw new ScopeLifecycleException("This scope instance is already initializing.");
      if (_disposedScopes.Contains(scope))
        throw new ScopeLifecycleException("A disposed scope instance cannot be reused. Create a new scope instance.");
      _scopeLoader.ValidateScope(parent, scope);
      _creatingScopes.Add(scope);
      var managed = new ManagedScope(parent, scope);
      managed.BeginInitialization();
      return managed;
    }

    private void CommitScope(ManagedScope managed) {
      managed.Activate(this);
      scopes.Add(managed.scope, managed);
      _creatingScopes.Remove(managed.scope);
    }

    private void EnsureApplicationCanStart() {
      ThrowIfDisposed();
      if (!_registrarPrepared)
        throw new ScopeLifecycleException("Prepare the registrar before starting the application.");
      if (_applicationStarted || scopes.ContainsKey(applicationScope) || _creatingScopes.Contains(applicationScope))
        throw new ScopeLifecycleException("The application scope can only be started once per container.");
    }

    private void ValidateManagedScope(ManagedScope managed) {
      if (managed == null || !scopes.TryGetValue(managed.scope, out var stored) || !ReferenceEquals(stored, managed))
        throw new ScopeLifecycleException("The supplied managed scope does not belong to this container.");
    }

    private void ForgetCreatingScope(ManagedScope managed) {
      scopes.Remove(managed.scope);
      _creatingScopes.Remove(managed.scope);
    }

    private void ForgetDisposedScope(ManagedScope managed) {
      scopes.Remove(managed.scope);
      _disposedScopes.Add(managed.scope);
    }

    private void OnSceneUnloaded(Scene scene) {
      var matching = scopes.Values.Where(x => x.scope is SceneScope sceneScope && sceneScope.scene == scene).ToArray();
      foreach (var managed in matching) DisposeScopeFromUnity(managed.scope);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
      if (!_applicationStarted || _disposed || HasSceneScope(scene)) return;
      CreateLoadedSceneScope(scene).Forget();
    }

    private async UniTaskVoid CreateLoadedSceneScope(Scene scene) {
      try {
        await CreateSceneScope(Application, scene);
      } catch (Exception exception) {
        Debug.LogException(exception);
      }
    }

    private UniTask<ManagedScope> CreateSceneScope(ManagedScope parent, Scene scene) {
      return CreateScope(parent, new SceneScope { scene = scene });
    }

    private ManagedScope CreateSceneScopeSync(ManagedScope parent, Scene scene) {
      return CreateScopeSync(parent, new SceneScope { scene = scene });
    }

    private IEnumerable<Scene> LoadedScenesWithoutScope() {
      for (var i = 0; i < SceneManager.sceneCount; i++) {
        var scene = SceneManager.GetSceneAt(i);
        if (scene.IsValid() && scene.isLoaded && !HasSceneScope(scene)) yield return scene;
      }
    }

    private bool HasSceneScope(Scene scene) {
      return scopes.Keys.Any(scope => scope is SceneScope sceneScope && sceneScope.scene == scene);
    }

    internal Dictionary<RegistrationEntry, Queue<object>> DiscoverInjectedComponents(
      ManagedScope managed,
      IEnumerable<IComponent> contributions = null
    ) {
      IEnumerable<IComponent> discovered = managed.scope switch {
        SceneScope sceneScope => SceneComponents(sceneScope.scene),
        GameObjectScope gameObjectScope when gameObjectScope.gameObject != null =>
          gameObjectScope.gameObject.GetComponents<MonoBehaviour>().OfType<IComponent>(),
        _ => Enumerable.Empty<IComponent>()
      };
      var result = new Dictionary<RegistrationEntry, Queue<object>>();
      var seen = new HashSet<IComponent>(ReferenceComparer<IComponent>.Instance);
      foreach (var component in (contributions ?? Enumerable.Empty<IComponent>()).Concat(discovered)) {
        if (component == null || !seen.Add(component)) continue;
        var runtime = component.RuntimeComponentData;
        if (runtime == null || runtime.isLoaded || runtime.isDisposed) continue;
        if (!registrarScope.registrations.components.TryGetValue(component.GetType(), out var registration)) continue;
        if (registration.scope != null && registration.scope != managed.scope.GetType()) continue;
        if (!result.TryGetValue(registration, out var instances))
          result.Add(registration, instances = new Queue<object>());
        instances.Enqueue(component);
      }
      return result;
    }

    private static IEnumerable<IComponent> SceneComponents(Scene scene) {
      if (!scene.IsValid() || !scene.isLoaded) yield break;
      foreach (var root in scene.GetRootGameObjects())
      foreach (var component in root.GetComponentsInChildren<MonoBehaviour>(true).OfType<IComponent>())
        yield return component;
    }

    private void UnsubscribeSceneEvents() {
      SceneManager.sceneLoaded -= OnSceneLoaded;
      SceneManager.sceneUnloaded -= OnSceneUnloaded;
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
