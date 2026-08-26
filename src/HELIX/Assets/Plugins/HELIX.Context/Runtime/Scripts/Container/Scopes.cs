using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HELIX.Context {
  public interface IScope { }

  public class RegistrarScope : IScope {
    public ManagedRegistrations registrations;
  }

  public class ApplicationScope : IScope { }

  public class SessionScope : IScope { }

  public class SceneScope : IScope {
    public Scene scene;
  }

  public class GameObjectScope : IScope {
    public GameObject gameObject;

    public GameObjectScope(GameObject gameObject = null) {
      this.gameObject = gameObject;
    }
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
      parentTypes.AddRange(allowedParentTypes.Where(static value => value != null));
      parentType = parentTypes.FirstOrDefault();
    }

    public bool AllowsParent(Type candidate) {
      return parentTypes.Count > 0
        ? parentTypes.Contains(candidate)
        : parentType == null || parentType == candidate;
    }
  }

  public enum ManagedScopeState { Created, Initializing, Active, Disposing, Disposed, Faulted }

  public readonly struct ManagedId : IEquatable<ManagedId> {
    public const ulong MaxOwner = 0x0000FFFFFFFFFFFF;
    public static readonly ManagedId Invalid = default;

    public readonly ulong value;
    public ulong owner => value >> 16;
    public ushort binding => (ushort)value;
    public ManagedId Owner => new(owner, 0);
    public bool IsValid => value != 0;

    public ManagedId(ulong owner, ushort binding) {
      if (owner > MaxOwner) throw new ArgumentOutOfRangeException(nameof(owner));
      value = owner << 16 | binding;
    }

    public bool Equals(ManagedId other) => value == other.value;
    public override bool Equals(object obj) => obj is ManagedId other && Equals(other);
    public override int GetHashCode() => value.GetHashCode();
    public override string ToString() => $"{owner}:{binding}";
    public static bool operator ==(ManagedId left, ManagedId right) => left.Equals(right);
    public static bool operator !=(ManagedId left, ManagedId right) => !left.Equals(right);
  }

  internal sealed class ManagedScopeCompanion : IManaged {
    public RuntimeManagedData managed { get; } = new();
  }

  public readonly struct ManagedBinding {
    public readonly ManagedId id;
    public readonly object value;
    public readonly Func<object> supplier;

    public ManagedBinding(ManagedId id, object value) {
      this.id = id;
      this.value = value;
      supplier = null;
    }

    public ManagedBinding(ManagedId id, Func<object> supplier) {
      this.id = id;
      value = null;
      this.supplier = supplier;
    }

    public object Resolve() {
      return supplier != null ? supplier() : value;
    }
  }

  public sealed class ManagedScope {
    public readonly Dictionary<TypeKey, List<ManagedBinding>> bindings = new();
    private readonly List<BindingObserver> _bindingObservers = new();
    private readonly CancellationTokenSource _cancellation = new();
    public readonly List<ManagedScope> managedChildren = new();
    public readonly List<IManaged> loadedComponents = new();
    private readonly HashSet<object> _owned = new(ReferenceComparer<object>.Instance);
    private ManagedContainer _container;

    public readonly IScope scope;
    public readonly IManaged companion = new ManagedScopeCompanion();
    public ManagedScope parent;
    public readonly List<IScope> children = new();
    public ManagedScopeState State { get; internal set; } = ManagedScopeState.Created;

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
      var values = new List<object>();
      for (var current = this; current != null; current = current.parent) {
        if (!current.bindings.TryGetValue(key, out var bindings)) continue;
        foreach (var binding in bindings) {
          if (ResolveBinding(key, binding, out var value)) values.Add(value);
        }
      }
      return values;
    }

    /// <summary>
    /// Registers an observer for bindings in this scope and its descendants. Existing bindings in
    /// the subtree are reported before this method returns.
    /// </summary>
    public void RegisterBindingObserver(BindingObserver observer) {
      if (observer == null) throw new ArgumentNullException(nameof(observer));
      EnsureBindingObserversMutable();
      if (_bindingObservers.Contains(observer)) return;
      _bindingObservers.Add(observer);
      NotifyExistingBindings(observer);
    }

    /// <summary>Stops an observer from receiving binding notifications from this scope.</summary>
    public void UnregisterBindingObserver(BindingObserver observer) {
      if (observer == null) throw new ArgumentNullException(nameof(observer));
      _bindingObservers.Remove(observer);
    }

    internal void BindComponent(ManagedId ownerId, ManagedRegistration registration, object instance) {
      foreach (var key in registration.keys.Distinct()) AddBinding(ownerId, key, instance, owner: registration);
    }

    internal void Publish(
      ManagedId ownerId,
      TypeKey key,
      object value,
      ScopeLoader loader = null,
      ManagedRegistration owner = null
    ) {
      AddBinding(ownerId, key, value, loader, owner);
    }

    internal void PublishProxy(
      ManagedId ownerId,
      TypeKey key,
      Func<object> supplier,
      ScopeLoader loader = null,
      ManagedRegistration owner = null
    ) {
      AddProxyBinding(ownerId, key, supplier, loader, owner);
    }

    internal bool HasDependency(ComponentDependency dependency) {
      if (!dependency.flags.HasFlag(DependencyFlags.Required)) return true;
      return IsDependencyAvailable(dependency);
    }

    internal bool IsDependencyAvailable(ComponentDependency dependency) {
      if (dependency.IsTyped) return HasBinding(dependency.key);
      if (!dependency.flags.HasFlag(DependencyFlags.Wirable))
        return dependency.scripted != null && ScopeLoader.Active.scripted.Contains(dependency.scripted);
      return HasWireKey(dependency.wireKey);
    }

    internal bool HasWireKey(string wireKey) {
      if (string.IsNullOrEmpty(wireKey)) return false;
      var loader = ScopeLoader.ActiveOrNull;
      return loader != null && (loader.anonymousPublications.Contains(wireKey) ||
        loader.publications.TryGetValue(wireKey, out var owners) && owners.Count > 0);
    }

    internal bool HasLocalDependency(ComponentDependency dependency) {
      if (dependency.IsTyped) return HasLocalBinding(dependency.key);
      if (!dependency.flags.HasFlag(DependencyFlags.Wirable))
        return dependency.scripted != null && ScopeLoader.Active.scripted.Contains(dependency.scripted);
      return HasWireKey(dependency.wireKey);
    }

    internal bool WasProvidedBy(ManagedId ownerId, ManagedRegistration registration, ComponentDependency dependency) {
      if (dependency.IsTyped) {
        return bindings.TryGetValue(dependency.key, out var keyBindings) &&
          keyBindings.Any(x => x.id.owner == ownerId.owner);
      }
      return dependency.wireKey != null && ScopeLoader.Active.publications.TryGetValue(
        dependency.wireKey,
        out var owners
      ) && owners.Contains(registration);
    }

    private void AddBinding(
      ManagedId ownerId,
      TypeKey key,
      object value,
      ScopeLoader loader = null,
      ManagedRegistration owner = null
    ) {
      EnsureCanPublish();
      ValidateKey(key);
      if (value == null) throw new ComponentResolutionException($"Cannot publish null for '{key}'.");
      if (!key.type.IsInstanceOfType(value)) {
        throw new ComponentResolutionException(
          $"Value of type {value.GetType().FullName} cannot be published as '{key}'."
        );
      }
      if (!bindings.TryGetValue(key, out var keyBindings))
        bindings.Add(key, keyBindings = new List<ManagedBinding>());
      if (!keyBindings.Any(binding => binding.id.owner == ownerId.owner && ReferenceEquals(binding.value, value))) {
        var binding = new ManagedBinding((loader ?? ScopeLoader.Active).ReserveBindingId(ownerId), value);
        keyBindings.Add(binding);
        NotifyBindingAdded(key, binding);
      }
      (loader ?? ScopeLoader.Active).Publish(owner, key.CreateWireKey());
    }

    private void AddProxyBinding(
      ManagedId ownerId,
      TypeKey key,
      Func<object> supplier,
      ScopeLoader loader = null,
      ManagedRegistration owner = null
    ) {
      EnsureCanPublish();
      ValidateKey(key);
      if (supplier == null) throw new ArgumentNullException(nameof(supplier));
      if (!bindings.TryGetValue(key, out var keyBindings))
        bindings.Add(key, keyBindings = new List<ManagedBinding>());
      if (!keyBindings.Any(binding => binding.id.owner == ownerId.owner &&
        ReferenceEquals(binding.supplier, supplier)
      )) {
        var binding = new ManagedBinding((loader ?? ScopeLoader.Active).ReserveBindingId(ownerId), supplier);
        keyBindings.Add(binding);
        NotifyBindingAdded(key, binding);
      }
      (loader ?? ScopeLoader.Active).Publish(owner, key.CreateWireKey());
    }

    internal void AddBindings(IEnumerable<ScopeBinding> bindings) {
      foreach (var binding in bindings ?? Enumerable.Empty<ScopeBinding>()) {
        if (binding.supplier != null) AddProxyBinding(companion.managed.id, binding.key, binding.supplier);
        else AddBinding(companion.managed.id, binding.key, binding.value);
      }
    }


    private bool HasBinding(TypeKey key) {
      ValidateKey(key);
      for (var current = this; current != null; current = current.parent) {
        if (current.HasLocalBinding(key)) return true;
      }
      return false;
    }

    private bool HasLocalBinding(TypeKey key) {
      ValidateKey(key);
      return bindings.TryGetValue(key, out var keyBindings) && keyBindings.Count > 0;
    }

    private bool TryResolveValue(TypeKey key, out object value) {
      ValidateKey(key);
      for (var current = this; current != null; current = current.parent) {
        if (current.TryResolveLocalValue(key, out value))
          return true;
      }
      value = null;
      return false;
    }

    private bool TryResolveLocalValue(TypeKey key, out object value) {
      ValidateKey(key);
      if (bindings.TryGetValue(key, out var keyBindings)) {
        for (var i = keyBindings.Count - 1; i >= 0; i--) {
          if (ResolveBinding(key, keyBindings[i], out value)) return true;
        }
      }
      value = null;
      return false;
    }

    private static bool ResolveBinding(TypeKey key, ManagedBinding binding, out object value) {
      try {
        value = binding.supplier != null ? binding.supplier() : binding.value;
      } catch (Exception exception) {
        throw new ComponentResolutionException($"Supplier for '{key}' failed.", exception);
      }
      if (value == null) return false;
      if (!key.type.IsInstanceOfType(value)) {
        throw new ComponentResolutionException(
          $"Supplier for '{key}' returned incompatible type {value.GetType().FullName}."
        );
      }
      return true;
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

    private void EnsureBindingObserversMutable() {
      if (State is ManagedScopeState.Initializing or ManagedScopeState.Active) return;
      throw new ScopeLifecycleException(
        $"Scope {scope.GetType().FullName} cannot register a registry while it is {State}."
      );
    }

    private void NotifyExistingBindings(BindingObserver registry) {
      foreach (var pair in bindings) {
        foreach (var binding in pair.Value) registry.BindingAdded(this, pair.Key, binding);
      }
      foreach (var child in managedChildren) child.NotifyExistingBindings(registry);
    }

    private void NotifyBindingAdded(TypeKey key, ManagedBinding binding) {
      for (var current = this; current != null; current = current.parent) {
        foreach (var registry in current._bindingObservers.ToArray()) registry.BindingAdded(this, key, binding);
      }
    }

    private void NotifyBindingRemoved(TypeKey key, ManagedBinding binding, List<Exception> failures) {
      for (var current = this; current != null; current = current.parent) {
        foreach (var registry in current._bindingObservers.ToArray()) {
          try { registry.BindingRemoved(this, key, binding); } catch (Exception exception) {
            failures.Add(
              new ComponentDeinitializationException("A binding registry failed during removal.", exception)
            );
          }
        }
      }
    }

    internal void AddChild(ManagedScope child) {
      managedChildren.Add(child);
      children.Add(child.scope);
    }

    internal void RemoveChild(ManagedScope child) {
      managedChildren.Remove(child);
      var index = children.FindIndex(x => ReferenceEquals(x, child.scope));
      if (index >= 0) children.RemoveAt(index);
    }

    internal void RecordComponent(ManagedContainer container, IManaged instance) {
      _container ??= container;
      loadedComponents.Add(instance);
    }

    internal void Own(object value) {
      EnsureCanPublish();
      switch (value) {
        case null: return;
        case UnityEngine.Object or IDisposable:
          _owned.Add(value);
          return;
        default:
          throw new ArgumentException(
            $"Scope-owned values must be a {nameof(UnityEngine.Object)} or {nameof(IDisposable)}.",
            nameof(value)
          );
      }
    }

    internal void BeginInitialization() {
      if (State != ManagedScopeState.Created)
        throw new ScopeLifecycleException($"Scope {scope.GetType().FullName} cannot initialize while it is {State}.");
      State = ManagedScopeState.Initializing;
    }

    internal void Attach(ManagedContainer container) {
      _container = container ?? throw new ArgumentNullException(nameof(container));
      var data = companion.managed;
      data.scope = this;
      data.container = container;
      container.RegisterManaged(container.ReserveManagedId(), null, companion);
    }

    internal void Activate(ManagedContainer container = null) {
      _container = container ?? _container;
      parent?.AddChild(this);
      State = ManagedScopeState.Active;
      container?.NotifyScopeActivated(this);
    }

    internal List<Exception> Rollback() {
      State = ManagedScopeState.Faulted;
      var failures = new List<Exception>();
      Teardown(failures);
      return failures;
    }

    internal List<Exception> Dispose(Action<ManagedScope> released = null) {
      var failures = new List<Exception>();
      Dispose(failures, released);
      return failures;
    }

    private void Dispose(List<Exception> failures, Action<ManagedScope> released) {
      if (State is ManagedScopeState.Disposed or ManagedScopeState.Disposing) return;
      State = ManagedScopeState.Disposing;
      Cancel(failures);
      foreach (var child in managedChildren.ToArray()) child.Dispose(failures, released);
      Teardown(failures, false);
      State = ManagedScopeState.Disposed;
      NotifyScopeDisposed(failures);
      released?.Invoke(this);
    }

    private void Teardown(List<Exception> failures, bool cancel = true) {
      if (cancel) Cancel(failures);
      NotifyScopeDisposing(failures);
      RemoveBindings(failures);
      UnloadManageds(failures);
      DisposeOwnedResources(failures);
      DestroyOwnedUnityObjects(failures);
      parent?.RemoveChild(this);
      if (companion.managed.id.IsValid) {
        _container?.UnregisterManaged(companion.managed.id);
        companion.managed.SetDisposed(true);
      }
    }

    private void NotifyScopeDisposing(List<Exception> failures) {
      if (_container == null) return;
      try {
        _container.NotifyScopeDisposing(this);
      } catch (Exception exception) {
        failures.Add(new ComponentDeinitializationException("A scope handler failed during disposal.", exception));
      }
    }

    private void NotifyScopeDisposed(List<Exception> failures) {
      if (_container == null) return;
      try {
        new ScopeDisposedEvent { Container = _container, Scope = this }.Raise();
      } catch (Exception exception) {
        failures.Add(new ComponentDeinitializationException("A scope disposal event handler failed.", exception));
      }
      _container = null;
    }

    private void Cancel(List<Exception> failures) {
      if (_cancellation.IsCancellationRequested) return;
      try { _cancellation.Cancel(); } catch (Exception exception) {
        failures.Add(new ComponentDeinitializationException("Scope cancellation failed.", exception));
      }
    }

    private void UnloadManageds(List<Exception> failures) {
      foreach (var loaded in loadedComponents.AsEnumerable().Reverse()) {
        Unload(loaded, failures);
        _container?.UnregisterManaged(loaded.managed.id);
      }
      loadedComponents.Clear();
    }

    private void RemoveBindings(List<Exception> failures) {
      foreach (var pair in bindings) {
        foreach (var binding in pair.Value) NotifyBindingRemoved(pair.Key, binding, failures);
      }
      bindings.Clear();
      _bindingObservers.Clear();
    }

    private void DisposeOwnedResources(List<Exception> failures) {
      foreach (var resource in _owned.OfType<IDisposable>()) {
        try { resource.Dispose(); } catch (Exception exception) {
          failures.Add(new ComponentDeinitializationException("Failed to dispose a scope-owned resource.", exception));
        }
      }
      _owned.RemoveWhere(value => value is IDisposable);
    }

    private void DestroyOwnedUnityObjects(List<Exception> failures) {
      foreach (var owned in _owned.OfType<UnityEngine.Object>()) {
        if (owned == null) continue;
        try {
          if (Application.isPlaying) UnityEngine.Object.Destroy(owned);
          else UnityEngine.Object.DestroyImmediate(owned);
        } catch (Exception exception) {
          failures.Add(
            new ComponentDeinitializationException(
              $"Failed to destroy Unity object '{owned.name}' owned by {scope.GetType().Name}.",
              exception
            )
          );
        }
      }
      _owned.RemoveWhere(value => value is UnityEngine.Object);
      _cancellation.Dispose();
    }

    private static void Unload(IManaged loaded, List<Exception> failures) {
      try {
        loaded.UnloadManaged();
        loaded.managed.SetDisposed(true);
      } catch (Exception exception) {
        failures.Add(
          new ComponentDeinitializationException(
            $"Failed to unload component '{loaded.managed.registration.name}'.", exception
          )
        );
      }
      try {
        if (loaded is IEventListener listener) listener.HandlerList.UnregisterAll();
      } catch (Exception exception) {
        failures.Add(
          new ComponentDeinitializationException(
            $"Failed to unregister handlers for component '{loaded.managed.registration.name}'.",
            exception
          )
        );
      }
      try {
        if (loaded is IDisposable disposable) disposable.Dispose();
      } catch (Exception exception) {
        failures.Add(
          new ComponentDeinitializationException(
            $"Failed to dispose component '{loaded.managed.registration.name}'.",
            exception
          )
        );
      }
    }
  }

  public interface ContextEvt<T> : Evt<T> where T : ContextEvt<T> {
    public ManagedContainer Container { get; set; }
  }

  public interface ContextScopeEvt<T> : ContextEvt<T> where T : ContextScopeEvt<T> {
    public ManagedScope Scope { get; set; }
  }

  /// <summary>
  /// Called directly after a scope is created and before any managed bindings are loaded or initialized.
  /// </summary>
  public struct ScopeCreateEvent : ContextScopeEvt<ScopeCreateEvent> {
    public ManagedContainer Container { get; set; }
    public ManagedScope Scope { get; set; }
  }

  /// <summary>
  /// Called directly after all managed bindings have been loaded and initialized in a scope.
  /// </summary>
  public struct ScopeActivateEvent : ContextScopeEvt<ScopeActivateEvent> {
    public ManagedContainer Container { get; set; }
    public ManagedScope Scope { get; set; }
  }

  /// <summary>
  /// Called after all managed bindings have been initialized and after <see cref="ScopeActivateEvent"/>.
  /// </summary>
  public struct ScopeStartedEvent : ContextScopeEvt<ScopeStartedEvent> {
    public ManagedContainer Container { get; set; }
    public ManagedScope Scope { get; set; }
  }

  /// <summary>
  /// Called before a scope and its bindings are disposed.
  /// </summary>
  public struct ScopeDeactivateEvent : ContextScopeEvt<ScopeDeactivateEvent> {
    public ManagedContainer Container { get; set; }
    public ManagedScope Scope { get; set; }
  }

  /// <summary>
  /// Called after a scope and its bindings have been disposed.
  /// </summary>
  public struct ScopeDisposedEvent : ContextScopeEvt<ScopeDisposedEvent> {
    public ManagedContainer Container { get; set; }
    public ManagedScope Scope { get; set; }
  }
}
