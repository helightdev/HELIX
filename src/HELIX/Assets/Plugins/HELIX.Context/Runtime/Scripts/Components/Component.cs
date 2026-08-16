using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using HELIX.Prose;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HELIX.Context {

  public interface IScope { }

  public class RegistrarScope : IScope {
    public ComponentRegistrations registrations;
  }

  public class ApplicationScope : IScope { }
  public class SessionScope : IScope { }

  public class SceneScope : IScope {
    public Scene scene;
  }

  public class GameObjectScope : IScope {
    public GameObject gameObject;
  }

  public class ScopeRegistration {
    public Type type;
    public Type parentType;
    public readonly List<Type> parentTypes = new();
    public bool allowMultiple = true;

    public ScopeRegistration() { }

    public ScopeRegistration(Type type, params Type[] allowedParentTypes) {
      this.type = type;
      if (allowedParentTypes == null) return;
      parentTypes.AddRange(allowedParentTypes.Where(static x => x != null));
      parentType = parentTypes.FirstOrDefault();
    }

    public bool AllowsParent(Type candidate) {
      if (parentTypes.Count > 0) return parentTypes.Contains(candidate);
      return parentType == null || parentType == candidate;
    }
  }

  public enum ManagedScopeState {
    Created,
    Initializing,
    Active,
    Disposing,
    Disposed,
    Faulted
  }

  public sealed class ManagedScope {
    private readonly Dictionary<TypeKey, List<ScopeBinding>> _bindings = new();
    private readonly Dictionary<string, HashSet<RegistrationEntry>> _publishedWireKeys = new();
    private readonly HashSet<string> _anonymousWireKeys = new(StringComparer.Ordinal);
    private readonly List<ManagedScope> _managedChildren = new();
    private readonly List<LoadedComponent> _loadedComponents = new();
    private readonly HashSet<IScriptedDependency> _loadedScriptedDependencies =
      new(ReferenceComparer<IScriptedDependency>.Instance);
    private readonly HashSet<UnityEngine.Object> _ownedUnityObjects =
      new(ReferenceComparer<UnityEngine.Object>.Instance);
    private readonly HashSet<IDisposable> _ownedResources =
      new(ReferenceComparer<IDisposable>.Instance);
    private readonly CancellationTokenSource _cancellation = new();

    public readonly IScope scope;
    public ManagedScope parent;
    public readonly List<IScope> children = new();
    public ManagedScopeState State { get; internal set; } = ManagedScopeState.Created;

    public IReadOnlyList<ManagedScope> ManagedChildren => _managedChildren;
    public CancellationToken CancellationToken => _cancellation.Token;
    public bool IsActive => State == ManagedScopeState.Active;

    public ManagedScope(IScope scope) {
      this.scope = scope ?? throw new ArgumentNullException(nameof(scope));
    }

    public ManagedScope(ManagedScope parent, IScope scope) : this(scope) {
      this.parent = parent;
    }

    public object Resolve(TypeKey key) {
      EnsureResolvable();
      if (TryResolveValue(key, out var value)) return value;
      throw new ComponentResolutionException(
        $"No component is registered for '{key}' in scope {scope.GetType().FullName} or its ancestors."
      );
    }

    public bool TryResolve(TypeKey key, out object value) {
      EnsureResolvable();
      return TryResolveValue(key, out value);
    }

    public IReadOnlyList<object> ResolveAll(TypeKey key) {
      EnsureResolvable();
      ValidateKey(key);
      for (var current = this; current != null; current = current.parent) {
        if (!current._bindings.TryGetValue(key, out var bindings) || bindings.Count == 0) continue;
        return bindings.Select(static x => x.value).ToArray();
      }
      return Array.Empty<object>();
    }

    internal void AddChild(ManagedScope child) {
      _managedChildren.Add(child);
      children.Add(child.scope);
    }

    internal void RemoveChild(ManagedScope child) {
      _managedChildren.Remove(child);
      var index = children.FindIndex(x => ReferenceEquals(x, child.scope));
      if (index >= 0) children.RemoveAt(index);
    }

    internal void RecordComponent(RegistrationEntry registration, object instance) {
      _loadedComponents.Add(new LoadedComponent(registration, instance));
    }

    internal void BindComponent(RegistrationEntry registration, object instance) {
      foreach (var key in registration.keys.Distinct()) AddBinding(registration, key, instance);
    }

    internal void Publish(RegistrationEntry owner, TypeKey key, object value) {
      AddBinding(owner, key, value);
    }

    internal void Publish(RegistrationEntry owner, string wireKey) {
      EnsureCanPublish();
      if (string.IsNullOrWhiteSpace(wireKey)) {
        throw new ArgumentException("A publication wire key cannot be null or empty.", nameof(wireKey));
      }
      if (owner == null) {
        _anonymousWireKeys.Add(wireKey);
        return;
      }
      if (!_publishedWireKeys.TryGetValue(wireKey, out var owners)) {
        owners = new HashSet<RegistrationEntry>(ReferenceComparer<RegistrationEntry>.Instance);
        _publishedWireKeys[wireKey] = owners;
      }
      owners.Add(owner);
    }

    internal bool HasDependency(ComponentDependency dependency) {
      if (!dependency.flags.HasFlag(DependencyFlags.Required)) return true;
      return IsDependencyAvailable(dependency);
    }

    internal bool IsDependencyAvailable(ComponentDependency dependency) {
      if (dependency.IsTyped) return TryResolveValue(dependency.key, out _);
      if (!dependency.flags.HasFlag(DependencyFlags.Wirable)) {
        return dependency.scripted != null && _loadedScriptedDependencies.Contains(dependency.scripted);
      }
      return HasWireKey(dependency.wireKey);
    }

    internal bool HasWireKey(string wireKey) {
      if (string.IsNullOrEmpty(wireKey)) return false;
      for (var current = this; current != null; current = current.parent) {
        if (current._anonymousWireKeys.Contains(wireKey)) return true;
        if (current._publishedWireKeys.TryGetValue(wireKey, out var owners) && owners.Count > 0) return true;
      }
      return false;
    }

    internal bool HasLocalDependency(ComponentDependency dependency) {
      if (dependency.IsTyped) return TryResolveLocalValue(dependency.key, out _);
      if (!dependency.flags.HasFlag(DependencyFlags.Wirable)) {
        return dependency.scripted != null && _loadedScriptedDependencies.Contains(dependency.scripted);
      }
      return HasLocalWireKey(dependency.wireKey);
    }

    internal bool WasProvidedBy(RegistrationEntry registration, ComponentDependency dependency) {
      if (dependency.IsTyped) {
        return _bindings.TryGetValue(dependency.key, out var bindings) &&
               bindings.Any(x => ReferenceEquals(x.owner, registration));
      }
      return dependency.wireKey != null &&
             _publishedWireKeys.TryGetValue(dependency.wireKey, out var owners) && owners.Contains(registration);
    }

    internal void MarkScriptedLoaded(IScriptedDependency dependency) {
      _loadedScriptedDependencies.Add(dependency);
      if (dependency.Flags.HasFlag(DependencyFlags.Wirable)) Publish(null, dependency.CreateWireKey());
    }

    internal bool IsScriptedLoaded(IScriptedDependency dependency) => _loadedScriptedDependencies.Contains(dependency);

    internal IEnumerable<LoadedComponent> LoadedComponents => _loadedComponents;

    internal IEnumerable<TypeKey> BoundKeys(RegistrationEntry registration) => _bindings
      .Where(pair => pair.Value.Any(binding => ReferenceEquals(binding.owner, registration)))
      .Select(static pair => pair.Key);

    internal IEnumerable<string> PublishedWireKeys(RegistrationEntry registration) => _publishedWireKeys
      .Where(pair => pair.Value.Contains(registration))
      .Select(static pair => pair.Key);

    internal void Own(object value) {
      EnsureCanPublish();
      switch (value) {
        case null:
          return;
        case UnityEngine.Object unityObject:
          _ownedUnityObjects.Add(unityObject);
          return;
        case IDisposable disposable:
          _ownedResources.Add(disposable);
          return;
        default:
          throw new ArgumentException(
            $"Scope-owned values must be a {nameof(UnityEngine.Object)} or {nameof(IDisposable)}.", nameof(value)
          );
      }
    }

    internal void Cancel(List<Exception> failures) {
      if (_cancellation.IsCancellationRequested) return;
      try {
        _cancellation.Cancel();
      } catch (Exception exception) {
        failures.Add(new ComponentDeinitializationException("Scope cancellation failed.", exception));
      }
    }

    internal void UnloadComponents(List<Exception> failures) {
      for (var i = _loadedComponents.Count - 1; i >= 0; i--) {
        var loaded = _loadedComponents[i];
        try {
          if (loaded.instance is IComponent component) component.UnloadComponent();
        } catch (Exception exception) {
          failures.Add(new ComponentDeinitializationException(
            $"Failed to unload component '{loaded.registration.name}'.", exception
          ));
        }

        try {
          if (loaded.instance is IEventListener listener) listener.HandlerList.UnregisterAll();
        } catch (Exception exception) {
          failures.Add(new ComponentDeinitializationException(
            $"Failed to unregister handlers for component '{loaded.registration.name}'.", exception
          ));
        }

        try {
          if (loaded.instance is IDisposable disposable) disposable.Dispose();
        } catch (Exception exception) {
          failures.Add(new ComponentDeinitializationException(
            $"Failed to dispose component '{loaded.registration.name}'.", exception
          ));
        }
      }
      _loadedComponents.Clear();
      _bindings.Clear();
      _publishedWireKeys.Clear();
      _anonymousWireKeys.Clear();
      _loadedScriptedDependencies.Clear();
    }

    internal void DisposeOwnedResources(List<Exception> failures) {
      foreach (var resource in _ownedResources) {
        try {
          resource.Dispose();
        } catch (Exception exception) {
          failures.Add(new ComponentDeinitializationException("Failed to dispose a scope-owned resource.", exception));
        }
      }
      _ownedResources.Clear();
    }

    internal void DestroyOwnedUnityObjects(List<Exception> failures) {
      foreach (var owned in _ownedUnityObjects) {
        if (owned == null) continue;
        try {
          if (Application.isPlaying) UnityEngine.Object.Destroy(owned);
          else UnityEngine.Object.DestroyImmediate(owned);
        } catch (Exception exception) {
          failures.Add(new ComponentDeinitializationException(
            $"Failed to destroy Unity object '{owned.name}' owned by {scope.GetType().Name}.", exception
          ));
        }
      }
      _ownedUnityObjects.Clear();
      _cancellation.Dispose();
    }

    private void AddBinding(RegistrationEntry owner, TypeKey key, object value) {
      EnsureCanPublish();
      ValidateKey(key);
      if (value == null) throw new ComponentResolutionException($"Cannot publish null for '{key}'.");
      if (!key.type.IsInstanceOfType(value)) {
        throw new ComponentResolutionException(
          $"Value of type {value.GetType().FullName} cannot be published as '{key}'."
        );
      }
      if (!_bindings.TryGetValue(key, out var bindings)) {
        bindings = new List<ScopeBinding>();
        _bindings[key] = bindings;
      }
      if (bindings.Any(x => ReferenceEquals(x.owner, owner) && ReferenceEquals(x.value, value))) return;
      bindings.Add(new ScopeBinding(owner, value));
      Publish(owner, key.CreateWireKey());
    }

    private bool TryResolveValue(TypeKey key, out object value) {
      ValidateKey(key);
      for (var current = this; current != null; current = current.parent) {
        if (current.TryResolveLocalValue(key, out value)) return true;
      }
      value = null;
      return false;
    }

    private bool TryResolveLocalValue(TypeKey key, out object value) {
      ValidateKey(key);
      if (!_bindings.TryGetValue(key, out var bindings) || bindings.Count == 0) {
        value = null;
        return false;
      }
      if (bindings.Count > 1) {
        throw new ComponentResolutionException(
          $"Multiple components are registered for '{key}' in scope {scope.GetType().FullName}."
        );
      }
      value = bindings[0].value;
      return true;
    }

    private bool HasLocalWireKey(string wireKey) {
      if (string.IsNullOrEmpty(wireKey)) return false;
      return _anonymousWireKeys.Contains(wireKey) ||
             _publishedWireKeys.TryGetValue(wireKey, out var owners) && owners.Count > 0;
    }

    private static void ValidateKey(TypeKey key) {
      if (key.type == null) throw new ArgumentException("A component key must have a type.", nameof(key));
    }

    private void EnsureResolvable() {
      if (State is ManagedScopeState.Initializing or ManagedScopeState.Active) return;
      throw new ScopeLifecycleException(
        $"Scope {scope.GetType().FullName} cannot resolve components while it is {State}."
      );
    }

    private void EnsureCanPublish() {
      if (State is ManagedScopeState.Initializing or ManagedScopeState.Active) return;
      throw new ScopeLifecycleException(
        $"Scope {scope.GetType().FullName} cannot accept publications while it is {State}."
      );
    }

    private readonly struct ScopeBinding {
      public readonly RegistrationEntry owner;
      public readonly object value;

      public ScopeBinding(RegistrationEntry owner, object value) {
        this.owner = owner;
        this.value = value;
      }
    }

    internal readonly struct LoadedComponent {
      public readonly RegistrationEntry registration;
      public readonly object instance;

      public LoadedComponent(RegistrationEntry registration, object instance) {
        this.registration = registration;
        this.instance = instance;
      }
    }
  }

  public sealed class HXContainer : IDisposable {
    public readonly Dictionary<IScope, ManagedScope> scopes =
      new(ReferenceComparer<IScope>.Instance);
    public readonly RegistrarScope registrarScope = new();
    public readonly ApplicationScope applicationScope = new();

    private bool _registrarPrepared;
    private bool _applicationStarted;
    private bool _disposed;
    private readonly HashSet<IScope> _disposedScopes = new(ReferenceComparer<IScope>.Instance);
    private readonly HashSet<IScope> _creatingScopes = new(ReferenceComparer<IScope>.Instance);

    public HXContainer() {
      SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    public HXContainer(RegistrationDiscoveryProvider discoveryProvider) : this() {
      if (discoveryProvider == null) throw new ArgumentNullException(nameof(discoveryProvider));
      try {
        PrepareRegistrar(discoveryProvider());
      } catch {
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
        ThrowWithCleanupFailures(exception, cleanupFailures, "Registrar initialization failed and rollback had failures.");
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
      if (managed != null && (managed.State is ManagedScopeState.Disposed or ManagedScopeState.Disposing)) return;
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
      if (_creatingScopes.Contains(scope)) throw new ScopeLifecycleException("This scope instance is already initializing.");
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
      if (!_registrarPrepared) throw new ScopeLifecycleException("Prepare the registrar before starting the application.");
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
      var parentType = parent.scope.GetType();
      var childType = child.GetType();

      if (child is RegistrarScope) throw new ScopeLifecycleException("A registrar scope cannot be created as a child.");
      if (child is ApplicationScope &&
          (!ReferenceEquals(child, applicationScope) || parent.scope is not RegistrarScope)) {
        throw new ScopeLifecycleException("The container application scope must be a direct child of the registrar.");
      }
      if (child is SessionScope && parent.scope is not ApplicationScope) {
        throw new ScopeLifecycleException("A session scope must be a child of the application scope.");
      }
      if (child is SceneScope && parent.scope is not ApplicationScope && parent.scope is not SessionScope) {
        throw new ScopeLifecycleException("A scene scope must be a child of an application or session scope.");
      }
      if (child is SceneScope sceneScope && (!sceneScope.scene.IsValid() || !sceneScope.scene.isLoaded)) {
        throw new ScopeLifecycleException("A scene scope requires a valid, loaded scene.");
      }
      if (child is GameObjectScope gameObjectScope) {
        if (gameObjectScope.gameObject == null) {
          throw new ScopeLifecycleException("A GameObject scope requires a GameObject.");
        }
        if (parent.scope is not ApplicationScope && parent.scope is not SessionScope && parent.scope is not SceneScope) {
          throw new ScopeLifecycleException("A GameObject scope must be a child of an application, session or scene scope.");
        }
        if (parent.scope is SceneScope parentScene && gameObjectScope.gameObject.scene != parentScene.scene) {
          throw new ScopeLifecycleException("A GameObject scope beneath a scene scope must belong to that scene.");
        }
      }

      var registrations = registrarScope.registrations;
      if (registrations.scopes.TryGetValue(childType, out var scopeRegistration)) {
        if (!scopeRegistration.AllowsParent(parentType)) {
          throw new ScopeLifecycleException(
            $"Scope {childType.FullName} cannot be created below {parentType.FullName}."
          );
        }
        if (!scopeRegistration.allowMultiple && scopes.Keys.Any(x => x.GetType() == childType)) {
          throw new ScopeLifecycleException($"Scope {childType.FullName} does not allow multiple instances.");
        }
      }
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
                   x.IsTyped && x.flags.HasFlag(DependencyFlags.Required))) {
          AddProvider(providers, publication.key, entry);
        }
      }

      foreach (var pair in providers.Where(static x => x.Value.Distinct().Count() > 1)) {
        throw new ComponentGraphException(
          $"Multiple components in {managed.scope.GetType().Name} guarantee the key '{pair.Key}': " +
          string.Join(", ", pair.Value.Select(static x => x.name).Distinct())
        );
      }

      foreach (var entry in entries) {
        foreach (var dependency in entry.dependencies.Where(static x => x.flags.HasFlag(DependencyFlags.Required))) {
          if (dependency.IsTyped) {
            if (providers.ContainsKey(dependency.key)) continue;
            var visibleProviders = managed.ResolveAll(dependency.key);
            if (visibleProviders.Count > 1) {
              throw new ComponentGraphException(
                $"Component '{entry.name}' has multiple visible providers for '{dependency.key}'."
              );
            }
            if (visibleProviders.Count > 0) continue;
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
      var entries = SelectRegistrations(managed);
      ValidateRegistrationGraph(managed, entries);
      EnsureSynchronous(entries);
      LoadStagesSync(managed, entries);
    }

    private async UniTask LoadScopeAsync(ManagedScope managed) {
      var entries = SelectRegistrations(managed);
      ValidateRegistrationGraph(managed, entries);
      await LoadStagesAsync(managed, entries);
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
        var result = dependency.scripted.Load(new ComponentLoadContext(this, managed));
        if (!result.success) {
          if (dependency.flags.HasFlag(DependencyFlags.Required)) {
            throw new ComponentInitializationException(
              $"Scripted dependency '{dependency.wireKey}' failed during {stage}."
            );
          }
          continue;
        }
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
        var result = await dependency.scripted.LoadAsync(new ComponentLoadContext(this, managed));
        if (!result.success) {
          if (dependency.flags.HasFlag(DependencyFlags.Required)) {
            throw new ComponentInitializationException(
              $"Scripted dependency '{dependency.wireKey}' failed during {stage}."
            );
          }
          continue;
        }
        managed.MarkScriptedLoaded(dependency.scripted);
      }
    }

    private static IEnumerable<ComponentDependency> EnumerateImplicitScripted(
      IEnumerable<RegistrationEntry> entries,
      InitializationStage stage
    ) => entries
      .SelectMany(static x => x.dependencies)
      .Where(x => x.IsScripted && x.scripted.Stage == stage &&
                  x.flags.HasFlag(DependencyFlags.ImplicitLoadable))
      .OrderBy(static x => x.scripted.Order);

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
    });

    private void LoadRegistrationSync(ManagedScope managed, RegistrationEntry entry) {
      var context = new ComponentLoadContext(this, managed, entry);
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
      var context = new ComponentLoadContext(this, managed, entry);
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
                 x.flags.HasFlag(DependencyFlags.Required))) {
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
      });
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
        publication.flags.HasFlag(DependencyFlags.Required) && publication.wireKey == wireKey));

    private static bool HasGuaranteedLocalProvider(
      IEnumerable<RegistrationEntry> entries,
      ComponentDependency dependency
    ) {
      if (dependency.IsTyped) {
        return entries.Any(entry => entry.keys.Contains(dependency.key) || entry.publications.Any(publication =>
          publication.IsTyped && publication.key.Equals(dependency.key) &&
          publication.flags.HasFlag(DependencyFlags.Required)));
      }
      return dependency.flags.HasFlag(DependencyFlags.Wirable) &&
             HasPublicationProvider(entries, dependency.wireKey);
    }

    private void ThrowIfDisposed() {
      if (_disposed) throw new ObjectDisposedException(nameof(HXContainer));
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

  internal sealed class GameObjectScopeObserver : MonoBehaviour {
    private HXContainer _container;
    private IScope _scope;

    internal void Attach(HXContainer container, IScope scope) {
      _container = container;
      _scope = scope;
    }

    internal bool Observes(IScope scope) => ReferenceEquals(_scope, scope);

    internal void Detach() {
      _container = null;
      _scope = null;
    }

    private void OnDestroy() {
      var container = _container;
      var scope = _scope;
      Detach();
      container?.DisposeScopeFromUnity(scope);
    }
  }

  internal sealed class ReferenceComparer<T> : IEqualityComparer<T> where T : class {
    public static readonly ReferenceComparer<T> Instance = new();
    public bool Equals(T x, T y) => ReferenceEquals(x, y);
    public int GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);
  }

  /// <summary>Renders declared registrations or live scopes as a printable Prose dependency graph.</summary>
  public static class ComponentGraphProse {
    public static void WriteDeclared(IProseWriter writer, ComponentRegistrations registrations) {
      if (writer == null) throw new ArgumentNullException(nameof(writer));
      if (registrations == null) throw new ArgumentNullException(nameof(registrations));

      using (writer.Tree()) {
        writer.Name("Declared dependency graph");
        foreach (var group in registrations.components.Values
                   .GroupBy(static entry => entry.scope ?? typeof(ApplicationScope))
                   .OrderBy(static group => TypeName(group.Key), StringComparer.Ordinal)) {
          using (writer.Tree()) {
            writer.Name(TypeName(group.Key));
            foreach (var entry in group.OrderBy(static entry => entry.name, StringComparer.Ordinal)) {
              WriteDeclaredEntry(writer, entry);
            }
          }
        }
      }
    }

    public static void WriteLive(IProseWriter writer, HXContainer container) {
      if (writer == null) throw new ArgumentNullException(nameof(writer));
      if (container == null) throw new ArgumentNullException(nameof(container));

      using (writer.Tree()) {
        writer.Name("Live dependency graph");
        if (container.TryGetScope(container.registrarScope, out var registrar)) {
          WriteLiveScope(writer, registrar);
        } else {
          writer.Property("state", "not prepared", ProseFormatters.String);
        }
      }
    }

    private static void WriteDeclaredEntry(IProseWriter writer, RegistrationEntry entry) {
      using (writer.Tree()) {
        writer.Name($"{entry.name} : {TypeName(entry.type)}");
        writer.Property("keys", Join(entry.keys.Select(FormatKey)), ProseFormatters.String);
        WriteDependencies(writer, "requires", entry.dependencies);
        WriteDependencies(writer, "publishes", entry.publications);
      }
    }

    private static void WriteLiveScope(IProseWriter writer, ManagedScope scope) {
      using (writer.Tree()) {
        writer.Name($"{TypeName(scope.scope.GetType())} [{scope.State}]");
        foreach (var loaded in scope.LoadedComponents) {
          using (writer.Tree()) {
            writer.Name($"{loaded.registration.name} : {TypeName(loaded.instance.GetType())}");
            writer.Property("keys", Join(scope.BoundKeys(loaded.registration).Select(FormatKey)), ProseFormatters.String);
            var publications = scope.PublishedWireKeys(loaded.registration)
              .Where(key => !loaded.registration.keys.Any(componentKey => componentKey.CreateWireKey() == key));
            writer.Property("publications", Join(publications), ProseFormatters.String);
          }
        }
        foreach (var child in scope.ManagedChildren) WriteLiveScope(writer, child);
      }
    }

    private static void WriteDependencies(
      IProseWriter writer, string name, IEnumerable<ComponentDependency> dependencies
    ) => writer.Property(name, Join(dependencies.Select(FormatDependency)), ProseFormatters.String);

    private static string FormatDependency(ComponentDependency dependency) => dependency.IsTyped
      ? FormatKey(dependency.key)
      : dependency.wireKey ?? dependency.scripted?.GetType().FullName ?? "<unwired>";

    private static string FormatKey(TypeKey key) => key.qualifier == null
      ? TypeName(key.type)
      : $"{TypeName(key.type)} | {key.qualifier}";

    private static string TypeName(Type type) => type?.Name ?? "<unknown>";

    private static string Join(IEnumerable<string> values) {
      var text = string.Join(", ", values);
      return text.Length == 0 ? "—" : text;
    }
  }

  public class ComponentContainerException : Exception {
    public ComponentContainerException(string message) : base(message) { }
    public ComponentContainerException(string message, Exception innerException) : base(message, innerException) { }
  }

  public sealed class ComponentGraphException : ComponentContainerException {
    public ComponentGraphException(string message) : base(message) { }
    public ComponentGraphException(string message, Exception innerException) : base(message, innerException) { }
  }

  public sealed class ComponentActivationException : ComponentContainerException {
    public ComponentActivationException(string message) : base(message) { }
  }

  public sealed class ComponentInitializationException : ComponentContainerException {
    public ComponentInitializationException(string message) : base(message) { }
    public ComponentInitializationException(string message, Exception innerException) : base(message, innerException) { }
  }

  public sealed class ComponentDeinitializationException : ComponentContainerException {
    public ComponentDeinitializationException(string message, Exception innerException) : base(message, innerException) { }
  }

  public sealed class ComponentResolutionException : ComponentContainerException {
    public ComponentResolutionException(string message) : base(message) { }
  }

  public sealed class ScopeLifecycleException : ComponentContainerException {
    public ScopeLifecycleException(string message) : base(message) { }
  }

  public sealed class AsyncScopeInitializationException : ComponentContainerException {
    public AsyncScopeInitializationException(string message) : base(message) { }
  }

  public static class HX {
    public static HXContainer container;

    public static object Resolve(TypeKey key) {
      if (container == null) throw new ScopeLifecycleException("No active HXContainer is installed.");
      return container.Application.Resolve(key);
    }
  }

  public interface IComponent {
    ManagedScope Scope { get; set; }
    void LoadComponent() { }
    void UnloadComponent() { }
  }

  // Stereotype attributes
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
  [MixinDefineTarget(MixinOn.ConfigureComponent, MixinOn.RegistrationConfiguratorDelegate)]
  [MixinExpression(
    new[] { MixinOn.ConfigureComponent, MixinOn.ComponentLoad, MixinOn.ComponentUnload },
    new[] { -100_000, 0, 0 },
    @"
@USING UnityEngine;
@USING HELIX.Context;
@CODE<$ConfigureComponent> registration.name = ""@this:name"";
@CODE<IMPLEMENTS> IComponent
@CODE<CLASS> public ManagedScope Scope { get; set; }
@VAR<IsComponent> true

@SCOPE
  @MATCH @this:?is<MonoBehaviour>
  @CODE<$ConfigureComponent> registration.activator = DefaultComponentActivators.MonoBehaviour<@this:type>();
  @RETURN
@SCOPE
  @CODE<$ConfigureComponent> registration.activator = DefaultComponentActivators.PlainObject<@this:type>();
  @RETURN
"
  )]
  public class ComponentAttribute : Attribute { }

  public class ServiceAttribute : ComponentAttribute { }
}
