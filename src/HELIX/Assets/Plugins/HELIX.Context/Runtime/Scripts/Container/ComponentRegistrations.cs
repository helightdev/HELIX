using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace HELIX.Context {
  public class ComponentRegistrations {
    public readonly Dictionary<Type, RegistrationEntry> components = new();
    public readonly Dictionary<Type, ScopeRegistration> scopes = new();

    public void Register(Type type, RegistrationConfigurator configurator) {
      if (type == null) throw new ArgumentNullException(nameof(type));
      if (configurator == null) throw new ArgumentNullException(nameof(configurator));
      if (components.ContainsKey(type))
        throw new ComponentGraphException($"Component type {type.FullName} is already registered.");
      var entry = new RegistrationEntry(type);
      try {
        configurator(entry);
        components[type] = entry;
      } catch (Exception e) {
        throw new ComponentGraphException($"Failed to configure component type {type.FullName}.", e);
      }
    }

    public ScopeRegistration RegisterScope(Type type, params Type[] allowedParentTypes) {
      if (type == null) throw new ArgumentNullException(nameof(type));
      if (!typeof(IScope).IsAssignableFrom(type))
        throw new ArgumentException($"{type.FullName} does not implement {nameof(IScope)}.", nameof(type));
      var registration = new ScopeRegistration(type, allowedParentTypes);
      scopes[type] = registration;
      return registration;
    }
  }

  public readonly struct TypeKey : IEquatable<TypeKey> {
    public readonly Type type;
    public readonly string qualifier;

    public TypeKey(Type type, string qualifier) {
      this.type = type;
      this.qualifier = qualifier;
    }

    public static implicit operator TypeKey(Type type) {
      return new TypeKey(type, null);
    }

    public bool Equals(TypeKey other) {
      return type == other.type && qualifier == other.qualifier;
    }

    public override bool Equals(object obj) {
      return obj is TypeKey other && Equals(other);
    }

    public override int GetHashCode() {
      return HashCode.Combine(type, qualifier);
    }

    public string CreateWireKey() {
      return type.AssemblyQualifiedName + (qualifier != null ? $"|{qualifier}" : "");
    }

    public override string ToString() {
      return qualifier == null ? type?.FullName ?? "<untyped>" : $"{type?.FullName}|{qualifier}";
    }
  }

  [Flags]
  public enum DependencyFlags {
    None = 0, Wirable = 1 << 0, Async = 1 << 1, Scripted = 1 << 2, Required = 1 << 3, ImplicitLoadable = 1 << 4
  }

  public readonly struct ComponentDependency {
    public readonly TypeKey key;
    public readonly IScriptedDependency scripted;
    public readonly DependencyFlags flags;
    public readonly string wireKey;

    public bool IsScripted => scripted != null;
    public bool IsTyped => key.type != null;

    public ComponentDependency(TypeKey key, bool required) {
      if (key.type == null) throw new ArgumentException("A typed dependency requires a type.", nameof(key));
      this.key = key;
      wireKey = key.CreateWireKey();
      scripted = null;
      flags = DependencyFlags.Wirable;
      if (required) flags |= DependencyFlags.Required;
    }

    public ComponentDependency(IScriptedDependency scripted, bool required) {
      this.scripted = scripted ?? throw new ArgumentNullException(nameof(scripted));
      key = default;
      flags = scripted.Flags;
      if (required) flags |= DependencyFlags.Required;
      wireKey = flags.HasFlag(DependencyFlags.Wirable) ? scripted.CreateWireKey() : null;
    }

    public static implicit operator ComponentDependency(TypeKey key) {
      return new ComponentDependency(key, true);
    }

    public static implicit operator ComponentDependency(ScriptedDependency scripted) {
      return new ComponentDependency(scripted, true);
    }
  }

  public struct ComponentLoadContext {
    public readonly ManagedContainer container;
    public readonly ManagedScope scope;
    public readonly RegistrationEntry registration;
    internal readonly ScopeLoader loader;

    public ComponentLoadContext(ManagedContainer container, ManagedScope scope) {
      this.container = container;
      this.scope = scope;
      registration = null;
      loader = null;
    }

    internal ComponentLoadContext(
      ManagedContainer container,
      ManagedScope scope,
      RegistrationEntry registration,
      ScopeLoader loader
    ) {
      this.container = container;
      this.scope = scope;
      this.registration = registration;
      this.loader = loader;
    }

    public CancellationToken CancellationToken => scope.CancellationToken;

    public object Resolve(TypeKey key) {
      return scope.Resolve(key);
    }

    public bool TryResolve(TypeKey key, out object value) {
      return scope.TryResolve(key, out value);
    }

    public void Publish(TypeKey key, object value) {
      scope.Publish(registration, key, value);
    }

    public void Publish(string wireKey) {
      (loader ?? throw new ScopeLifecycleException(
        "Wire publications are only available during component loading."
      )).Publish(registration, wireKey);
    }

    public void Own(object value) {
      scope.Own(value);
    }
  }

  public interface IScriptedDependency : IComponentLoadable {
    int Order { get; }
    DependencyFlags Flags { get; }
    InitializationStage Stage { get; }
    string CreateWireKey();
  }

  public abstract class ScriptedDependency : IScriptedDependency {
    public int Order { get; }
    public DependencyFlags Flags { get; }
    public InitializationStage Stage { get; }

    protected ScriptedDependency(
      int order = 0,
      DependencyFlags flags = DependencyFlags.Wirable | DependencyFlags.ImplicitLoadable,
      InitializationStage stage = InitializationStage.PreInit
    ) {
      Order = order;
      Flags = flags | DependencyFlags.Scripted;
      Stage = stage;
    }

    public virtual string CreateWireKey() {
      return GetType().AssemblyQualifiedName;
    }

    public virtual UniTask<ComponentLoadResult> LoadAsync(ComponentLoadContext context) {
      var result = Load(context);
      return UniTask.FromResult(result);
    }

    public virtual ComponentLoadResult Load(ComponentLoadContext context) {
      throw new NotImplementedException();
    }
  }

  public interface IComponentLoadable {
    UniTask<ComponentLoadResult> LoadAsync(ComponentLoadContext context);
    ComponentLoadResult Load(ComponentLoadContext context);
  }

  public readonly struct ComponentLoadResult {
    public readonly bool success;
    public readonly object value;

    public ComponentLoadResult(bool success, object value = null) {
      this.success = success;
      this.value = value;
    }

    public static implicit operator ComponentLoadResult(bool success) {
      return new ComponentLoadResult(success);
    }
  }

  public enum InitializationStage { PreInit, Init, PostInit }

  public sealed class RegistrationEntry : IComponentLoadable {
    public readonly Type type;
    public readonly List<RegistrationHandlerBinding> handlers = new();
    public readonly List<ComponentDependency> dependencies = new(); // Requirements
    public readonly List<TypeKey> keys = new(); // Keys this registration itself will be bound to, guaranteed

    // Additional publications the registration may provide
    public readonly List<ComponentDependency> publications = new();

    public Type scope; // Associated scope type
    public string name;
    public int order = 0;
    public ComponentActivator activator;

    public RegistrationEntry(Type type) {
      this.type = type ?? throw new ArgumentNullException(nameof(type));
      name = type.Name;
      keys.Add(type);
    }

    public RegistrationEntry InScope(Type scopeType) {
      if (scopeType == null || !typeof(IScope).IsAssignableFrom(scopeType))
        throw new ArgumentException("A component scope must implement IScope.", nameof(scopeType));
      scope = scopeType;
      return this;
    }

    public RegistrationEntry Key(TypeKey key) {
      if (key.type == null) throw new ArgumentException("A component key requires a type.", nameof(key));
      keys.Add(key);
      return this;
    }

    public RegistrationEntry Dependency(ComponentDependency dependency) {
      dependencies.Add(dependency);
      return this;
    }

    public RegistrationEntry Publication(ComponentDependency publication) {
      publications.Add(publication);
      return this;
    }

    public void RegisterHandlerBinding<T>(int priority = 0) {
      handlers.Add(new RegistrationHandlerBinding(typeof(T), priority));
    }

    public bool IsAsync => handlers.Any(static x => x.eventType == typeof(ComponentAsyncInitEvent));

    public async UniTask<ComponentLoadResult> LoadAsync(ComponentLoadContext context) {
      var instance = Activate(context);
      InitializeSync(instance, context);
      await InitializeAsync(instance, context);
      return new ComponentLoadResult(true, instance);
    }

    public ComponentLoadResult Load(ComponentLoadContext context) {
      var instance = Activate(context);
      InitializeSync(instance, context);
      return new ComponentLoadResult(true, instance);
    }

    internal object Activate(ComponentLoadContext context) {
      if (activator == null)
        throw new ComponentActivationException($"Component '{name}' ({type.FullName}) has no activator.");
      var instance = activator(context);
      if (instance is IComponent component) component.Scope = context.scope;

      if (instance == null)
        throw new ComponentActivationException($"Activator for component '{name}' ({type.FullName}) returned null.");
      if (!type.IsInstanceOfType(instance)) {
        throw new ComponentActivationException(
          $"Activator for component '{name}' returned {instance.GetType().FullName}, expected {type.FullName}."
        );
      }
      return instance;
    }

    internal void InitializeSync(object instance, ComponentLoadContext context) {
      if (instance is IComponent component) component.LoadComponent();
      if (instance is IEventListener listener) {
        var initEvent = new ComponentInitEvent(context);
        listener.HandlerList.RaiseLocal(initEvent);
      }
    }

    internal async UniTask InitializeAsync(object instance, ComponentLoadContext context) {
      if (instance is not IEventListener listener) return;
      var initEvent = new ComponentAsyncInitEvent();
      initEvent.Reset(context);
      await listener.HandlerList.RaiseLocalAsync(initEvent);
    }
  }

  public struct RegistrationHandlerBinding {
    public readonly Type eventType;
    public readonly int priority;

    public RegistrationHandlerBinding(Type eventType, int priority) {
      this.eventType = eventType;
      this.priority = priority;
    }
  }

  public delegate void RegistrationConfigurator(RegistrationEntry registration);

  public class ComponentAsyncInitEvent : AsyncChainEvt<ComponentAsyncInitEvent> {
    // Pooling capable
    private ComponentLoadContext _context;
    public ComponentAsyncInitEvent() { }

    public ManagedContainer Container => _context.container;
    public ManagedScope Scope => _context.scope;
    public ComponentLoadContext Context => _context;

    public void Reset(ComponentLoadContext updated) {
      Reset();
      _context = updated;
    }
  }

  public readonly struct ComponentInitEvent : Evt<ComponentInitEvent> {
    private readonly ComponentLoadContext _context;

    public ComponentInitEvent(ComponentLoadContext context) {
      _context = context;
    }

    public ManagedContainer Container => _context.container;
    public ManagedScope Scope => _context.scope;
    public ComponentLoadContext Context => _context;
  }

  public delegate ComponentRegistrations RegistrationDiscoveryProvider();
}