using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace HELIX.Context {
  /// <summary>Container identity, retained state, and construction surface.</summary>
  public sealed class ManagedContainer : IDisposable {
    public const int DefaultMaxLoadingIterations = 1024;
    public readonly Dictionary<IScope, ManagedScope> scopes = new(ReferenceComparer<IScope>.Instance);
    public readonly RegistrarScope registrarScope = new();
    public readonly ApplicationScope applicationScope = new();
    public readonly Dictionary<ManagedId, IManaged> managedObjects = new();
    public readonly ScopeRules scopeRules;
    private readonly RegistrarGraph _registrarGraph = new();
    private readonly ScopeLoader _scopeLoader;
    private readonly List<IScopeHandler> _scopeHandlers = new();
    private readonly HashSet<IScope> _disposedScopes = new(ReferenceComparer<IScope>.Instance);
    private readonly HashSet<IScope> _creatingScopes = new(ReferenceComparer<IScope>.Instance);
    private readonly Dictionary<ManagedId, int> _pendingBindingIds = new();
    private ulong _nextManagedOwner = 1;
    private bool _registrarPrepared, _applicationStarted, _disposed;

    /// <summary>Maximum dependency-resolution passes allowed while initializing one scope.</summary>
    public int MaxLoadingIterations { get; }

    internal ManagedContainer(
      ScopeRules scopeRules,
      IEnumerable<IScopeHandler> scopeHandlers,
      int maxLoadingIterations
    ) {
      this.scopeRules = scopeRules ?? throw new ArgumentNullException(nameof(scopeRules));
      _scopeLoader = new ScopeLoader(this, _registrarGraph);
      MaxLoadingIterations = maxLoadingIterations;
      InstallScopeHandler(new SceneScopeHandler());
      InstallScopeHandler(new GameObjectScopeHandler());
      foreach (var handler in scopeHandlers ?? Enumerable.Empty<IScopeHandler>()) InstallScopeHandler(handler);
    }

    public ManagedScope Registrar => GetScope(registrarScope);
    public ManagedScope Application => GetScope(applicationScope);

    public void PrepareRegistrar(ManagedRegistrations registrations) {
      ThrowIfDisposed();
      if (_registrarPrepared) throw new ScopeLifecycleException("The registrar has already been prepared.");
      registrarScope.registrations = _registrarGraph.Prepare(registrations);
      var registrar = new ManagedScope(registrarScope);
      registrar.Attach(this);
      registrar.BeginInitialization();
      scopes.Add(registrarScope, registrar);
      try {
        _scopeLoader.LoadSync(registrar);
        registrar.Activate();
        foreach (var handler in registrar.loadedComponents.OfType<IScopeHandler>())
          InstallScopeHandler(handler);
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
      var application = await CreateScope(Registrar).From(applicationScope).StartAsync();
      _applicationStarted = true;
      HX.container = this;
      foreach (var handler in _scopeHandlers.ToArray()) await handler.ApplicationStarted(this, application);
    }

    public ManagedScope StartApplicationSync() {
      EnsureApplicationCanStart();
      var scope = CreateScope(Registrar).From(applicationScope).StartSync();
      _applicationStarted = true;
      HX.container = this;
      foreach (var handler in _scopeHandlers.ToArray()) handler.ApplicationStartedSync(this, scope);
      return scope;
    }

    public ManagedScopeBuilder CreateScope(ManagedScope parent) => new(this, parent);

    internal async UniTask<ManagedScope> StartScopeAsync(
      ManagedScope parent,
      IScope scope,
      IEnumerable<IManaged> components,
      IEnumerable<Type> componentTypes,
      IEnumerable<ScopeBinding> bindings
    ) {
      var managed = BeginScopeCreation(parent, scope);
      try {
        new ScopeCreateEvent { Container = this, Scope = managed }.Raise();
        await _scopeLoader.LoadAsync(managed, components, componentTypes, bindings);
        CommitScope(managed);
        return managed;
      } catch (Exception exception) {
        var cleanupFailures = managed.Rollback();
        ForgetCreatingScope(managed);
        ThrowWithCleanupFailures(exception, cleanupFailures, "Scope initialization failed and rollback had failures.");
        throw;
      }
    }

    internal ManagedScope StartScopeSync(
      ManagedScope parent,
      IScope scope,
      IEnumerable<IManaged> components,
      IEnumerable<Type> componentTypes,
      IEnumerable<ScopeBinding> bindings
    ) {
      var managed = BeginScopeCreation(parent, scope);
      try {
        new ScopeCreateEvent { Container = this, Scope = managed }.Raise();
        _scopeLoader.LoadSync(managed, components, componentTypes, bindings);
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
      DetachScopeHandlers();
      if (scopes.TryGetValue(registrarScope, out var registrar)) failures = registrar.Dispose(ForgetDisposedScope);
      scopes.Clear();
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

    internal ManagedId ReserveManagedId() => new(_nextManagedOwner++, 0);

    internal ManagedId ReserveBindingId(ManagedId owner) {
      owner = owner.Owner;
      if (!owner.IsValid) throw new ScopeLifecycleException("A binding owner must be managed by the container.");
      if (managedObjects.TryGetValue(owner, out var instance)) return instance.managed.ReserveBindingId();
      if (!_pendingBindingIds.TryGetValue(owner, out var next)) next = 1;
      if (next > ushort.MaxValue)
        throw new ScopeLifecycleException($"Managed object {owner} exceeded the binding ID limit.");
      _pendingBindingIds[owner] = next + 1;
      return new ManagedId(owner.owner, (ushort)next);
    }

    internal void RegisterManaged(ManagedId id, ManagedRegistration registration, IManaged instance) {
      var data = instance.managed;
      data.id = id;
      data.registration = registration;
      if (_pendingBindingIds.TryGetValue(id.Owner, out var next)) {
        data.ContinueBindingIdsAt(next);
        _pendingBindingIds.Remove(id.Owner);
      }
      managedObjects.Add(id, instance);
    }

    internal void UnregisterManaged(ManagedId id) {
      managedObjects.Remove(id.Owner);
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
      scopeRules.Validate(ScopeValidationContext.Create(this, parent, scope));
      _creatingScopes.Add(scope);
      var managed = new ManagedScope(parent, scope);
      managed.Attach(this);
      managed.BeginInitialization();
      return managed;
    }

    private void CommitScope(ManagedScope managed) {
      managed.Activate(this);
      scopes.Add(managed.scope, managed);
      _creatingScopes.Remove(managed.scope);
      new ScopeStartedEvent { Container = this, Scope = managed }.Raise();
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

    internal Dictionary<ManagedRegistration, Queue<object>> DiscoverInjectedComponents(
      ManagedScope managed,
      IEnumerable<IManaged> contributions = null
    ) {
      var discovered = _scopeHandlers.Where(handler => handler.Handles(managed.scope))
        .SelectMany(handler => handler.DiscoverComponents(this, managed) ?? Enumerable.Empty<IManaged>());
      var result = new Dictionary<ManagedRegistration, Queue<object>>();
      var seen = new HashSet<IManaged>(ReferenceComparer<IManaged>.Instance);
      foreach (var component in (contributions ?? Enumerable.Empty<IManaged>()).Concat(discovered)) {
        if (component == null || !seen.Add(component)) continue;
        var runtime = component.managed;
        if (runtime == null || runtime.isLoaded || runtime.isDisposed) continue;
        if (!registrarScope.registrations.components.TryGetValue(component.GetType(), out var registration)) continue;
        if (registration.scope != null && registration.scope != managed.scope.GetType()) continue;
        if (!result.TryGetValue(registration, out var instances))
          result.Add(registration, instances = new Queue<object>());
        instances.Enqueue(component);
      }
      return result;
    }

    internal bool IsApplicationStarted => _applicationStarted && !_disposed;

    internal bool HasScope<TScope>(Func<TScope, bool> predicate) where TScope : class, IScope {
      return scopes.Keys.OfType<TScope>().Any(predicate);
    }

    internal IEnumerable<ManagedScope> FindScopes<TScope>(Func<TScope, bool> predicate)
      where TScope : class, IScope {
      return scopes.Values.Where(managed => managed.scope is TScope scope && predicate(scope));
    }

    internal void DisposeScopeFromHandler(IScope scope) => DisposeScopeFromUnity(scope);

    internal void NotifyScopeActivated(ManagedScope scope) {
      new ScopeActivateEvent { Container = this, Scope = scope }.Raise();
      foreach (var handler in _scopeHandlers.Where(handler => handler.Handles(scope.scope)).ToArray())
        handler.ScopeActivated(this, scope);
    }

    internal void NotifyScopeDisposing(ManagedScope scope) {
      new ScopeDeactivateEvent { Container = this, Scope = scope }.Raise();
      foreach (var handler in _scopeHandlers.Where(handler => handler.Handles(scope.scope)).Reverse().ToArray())
        handler.ScopeDisposing(this, scope);
    }

    private void InstallScopeHandler(IScopeHandler handler) {
      if (handler == null || _scopeHandlers.Any(existing => ReferenceEquals(existing, handler))) return;
      handler.Attach(this);
      _scopeHandlers.Add(handler);
    }

    private void DetachScopeHandlers() {
      if (_scopeHandlers == null) return;
      foreach (var handler in _scopeHandlers.AsEnumerable().Reverse().ToArray()) {
        try { handler.Detach(this); } catch (Exception exception) { Debug.LogException(exception); }
      }
      _scopeHandlers.Clear();
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
