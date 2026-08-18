using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace HELIX.Context {
  public class ComponentRegistrations {
    public readonly Dictionary<Type, ComponentRegistration> components = new();
    public readonly Dictionary<Type, ScopeRegistration> scopes = new();

    public void Register(Type type, RegistrationConfigurator configurator) {
      if (type == null) throw new ArgumentNullException(nameof(type));
      if (configurator == null) throw new ArgumentNullException(nameof(configurator));
      if (components.ContainsKey(type))
        throw new ComponentGraphException($"Component type {type.FullName} is already registered.");
      var entry = new ComponentRegistration(type);
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
      return type.FullName + (qualifier != null ? $"|{qualifier}" : "");
    }

    public override string ToString() {
      return qualifier == null ? type?.FullName ?? "<untyped>" : $"{type?.FullName}|{qualifier}";
    }
  }

  [Flags]
  public enum DependencyFlags {
    None = 0,
    Wirable = 1 << 0,
    Async = 1 << 1,
    Scripted = 1 << 2,
    Required = 1 << 3,
    ImplicitLoadable = 1 << 4,
    Collection = 1 << 5
  }

  public readonly struct ComponentDependency {
    public readonly TypeKey key;
    public readonly IScriptedDependency scripted;
    public readonly DependencyFlags flags;
    public readonly string wireKey;

    public bool IsScripted => scripted != null;
    public bool IsTyped => key.type != null;
    public bool IsCollection => flags.HasFlag(DependencyFlags.Collection);

    public ComponentDependency(TypeKey key, bool required, bool collection = false) {
      if (key.type == null) throw new ArgumentException("A typed dependency requires a type.", nameof(key));
      this.key = key;
      wireKey = key.CreateWireKey();
      scripted = null;
      flags = DependencyFlags.Wirable;
      if (required) flags |= DependencyFlags.Required;
      if (collection) flags |= DependencyFlags.Collection;
    }

    public ComponentDependency(IScriptedDependency scripted, bool required) {
      this.scripted = scripted ?? throw new ArgumentNullException(nameof(scripted));
      key = default;
      flags = scripted.Flags;
      if (required) flags |= DependencyFlags.Required;
      wireKey = flags.HasFlag(DependencyFlags.Wirable) ? scripted.WireKey : null;
    }

    public static implicit operator ComponentDependency(TypeKey key) {
      return new ComponentDependency(key, true);
    }

    public static implicit operator ComponentDependency(ScriptedDependency scripted) {
      return new ComponentDependency(scripted, true);
    }
  }

  public readonly struct ComponentLoadContext {
    public readonly ManagedContainer container;
    public readonly ManagedScope scope;
    public readonly ComponentRegistration registration;
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
      ComponentRegistration registration,
      ScopeLoader loader
    ) {
      this.container = container;
      this.scope = scope;
      this.registration = registration;
      this.loader = loader;
    }

    public CancellationToken CancellationToken => scope.CancellationToken;

    public object Resolve(TypeKey key) => scope.Resolve(key);

    public bool TryResolve(TypeKey key, out object value) => scope.TryResolve(key, out value);

    public void Publish(TypeKey key, object value) => scope.Publish(registration, key, value, loader);

    public void Publish<T>(T value, string qualifier = null) => Publish(new TypeKey(typeof(T), qualifier), value);

    public void Publish(object value, Type type, string qualifier = null) =>
      Publish(new TypeKey(type, qualifier), value);

    public void PublishProxy(TypeKey key, Func<object> supplier) =>
      scope.PublishProxy(registration, key, supplier, loader);

    public void PublishProxy<T>(Func<T> supplier, string qualifier = null) where T : class {
      if (supplier == null) throw new ArgumentNullException(nameof(supplier));
      PublishProxy(new TypeKey(typeof(T), qualifier), supplier);
    }

    public void PublishKey(string wireKey) {
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
    int Phase { get; }
    string WireKey { get; }
  }

  public abstract class ScriptedDependency : IScriptedDependency {
    public static string SimpleKey(Type type, Source source, string qualifier, bool list = false) {
      var src = $"{type.FullName}|{(int)source}|{qualifier ?? "null"}";
      return list ? $"list|{src}" : src;
    } // This matches (and MUST match) the roslyn generated name from [Inject]

    public static string SimpleKey(Type type, Source source, string qualifier, string kind) {
      return $"{kind}|{type.FullName}|{(int)source}|{qualifier ?? "null"}";
    }

    public int Order { get; }
    public DependencyFlags Flags { get; }
    public int Phase { get; }
    public string WireKey { get; protected set; }

    protected ScriptedDependency(
      int order = 0,
      DependencyFlags flags = DependencyFlags.Wirable | DependencyFlags.ImplicitLoadable,
      int phase = InitPhase.PreInit
    ) {
      Order = order;
      Flags = flags | DependencyFlags.Scripted;
      Phase = phase;
    }

    protected ScriptedDependency(
      string wireKey,
      int order = 0,
      DependencyFlags flags = DependencyFlags.Wirable | DependencyFlags.ImplicitLoadable,
      int phase = InitPhase.PreInit
    ) {
      Order = order;
      Flags = flags | DependencyFlags.Scripted;
      Phase = phase;
      WireKey = wireKey;
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

  public static class InitPhase {
    public const int PreInit = -1000;
    public const int Early = -100;
    public const int Configuration = -50;
    public const int Normal = 0;
    public const int Late = 100;
    public const int PostInit = 1000;
  }

  public sealed class ComponentRegistration : IComponentLoadable {
    public readonly Type type;
    public readonly List<RegistrationHandlerBinding> handlers = new();
    public readonly List<ComponentDependency> dependencies = new(); // Requirements
    public readonly List<TypeKey> keys = new(); // Keys this registration itself will be bound to, guaranteed

    // Additional publications the registration may provide
    public readonly List<ComponentDependency> publications = new();

    public Type scope; // Associated scope type
    public string name;
    public int phase = InitPhase.Normal;
    public int order = 0;
    public bool optional;
    public ComponentActivator activator;
    public readonly List<ComponentCondition> conditions = new();

    public ComponentRegistration(Type type) {
      this.type = type ?? throw new ArgumentNullException(nameof(type));
      name = type.Name;
      keys.Add(type);
    }

    public ComponentRegistration InScope(Type scopeType) {
      if (scopeType == null || !typeof(IScope).IsAssignableFrom(scopeType))
        throw new ArgumentException("A component scope must implement IScope.", nameof(scopeType));
      scope = scopeType;
      return this;
    }

    /// <summary>
    /// Makes this component conditional on all of its required dependencies being available.
    /// An unavailable optional component is omitted instead of failing the scope.
    /// </summary>
    public ComponentRegistration Optional(bool value = true) {
      optional = value;
      return this;
    }

    public ComponentRegistration Condition(ComponentCondition condition) {
      conditions.Add(condition ?? throw new ArgumentNullException(nameof(condition)));
      return this;
    }

    public ComponentRegistration Key(TypeKey key) {
      if (key.type == null) throw new ArgumentException("A component key requires a type.", nameof(key));
      keys.Add(key);
      return this;
    }

    public ComponentRegistration Dependency(ComponentDependency dependency) {
      dependencies.Add(dependency);
      return this;
    }

    public ComponentRegistration Dependency(Type bindingType, string qualifier) =>
      Dependency(new TypeKey(bindingType, qualifier));

    public ComponentRegistration Publication(ComponentDependency publication) {
      publications.Add(publication);
      return this;
    }

    public void RegisterHandlerBinding<T>(int priority = 0) {
      handlers.Add(new RegistrationHandlerBinding(typeof(T), priority));
    }

    public bool IsAsync => handlers.Any(static x => x.eventType == typeof(AsyncComponentLoadEvent));

    public async UniTask<ComponentLoadResult> LoadAsync(ComponentLoadContext context) {
      var instance = Activate(context);
      InitializeSync(instance, context);
      await InitializeAsync(instance, context);
      InitializeLate(instance, context);
      return new ComponentLoadResult(true, instance);
    }

    public ComponentLoadResult Load(ComponentLoadContext context) {
      var instance = Activate(context);
      InitializeSync(instance, context);
      InitializeLate(instance, context);
      return new ComponentLoadResult(true, instance);
    }

    internal object Activate(ComponentLoadContext context) {
      if (activator == null)
        throw new ComponentActivationException($"Component '{name}' ({type.FullName}) has no activator.");
      var instance = activator(context);
      if (instance is IComponent component) {
        var runtimeData = component.ComponentBinding;
        runtimeData.scope = context.scope;
        runtimeData.container = context.container;
      }

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
      if (instance is IComponent component) component.LoadComponent(context);
      if (instance is IEventListener listener) {
        var initEvent = new ComponentLoadEvent(context);
        listener.HandlerList.RaiseLocal(initEvent);
      }
    }

    internal async UniTask InitializeAsync(object instance, ComponentLoadContext context) {
      if (instance is not IEventListener listener) return;
      var initEvent = new AsyncComponentLoadEvent();
      initEvent.Reset(context);
      await listener.HandlerList.RaiseLocalAsync(initEvent);
    }

    internal void InitializeLate(object instance, ComponentLoadContext context) {
      if (instance is IComponent component) component.LoadComponentLate(context);
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

  public delegate void RegistrationConfigurator(ComponentRegistration registration);
  public delegate bool ComponentCondition(ComponentLoadContext context);

  public class AsyncComponentLoadEvent : AsyncChainEvt<AsyncComponentLoadEvent> {
    // Pooling capable
    private ComponentLoadContext _context;
    public AsyncComponentLoadEvent() { }

    public ManagedContainer Container => _context.container;
    public ManagedScope Scope => _context.scope;
    public ComponentLoadContext Context => _context;

    public void Reset(ComponentLoadContext updated) {
      Reset();
      _context = updated;
    }
  }

  public readonly struct ComponentLoadEvent : Evt<ComponentLoadEvent> {
    private readonly ComponentLoadContext _context;

    public ComponentLoadEvent(ComponentLoadContext context) {
      _context = context;
    }

    public ManagedContainer Container => _context.container;
    public ManagedScope Scope => _context.scope;
    public ComponentLoadContext Context => _context;
  }

  public delegate ComponentRegistrations RegistrationDiscoveryProvider();
}
