using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace HELIX.Context {
  public class ComponentRegistrations {
    public readonly Dictionary<Type, RegistrationEntry> components = new();

    public void Register(Type type, RegistrationConfigurator configurator) {
      var entry = new RegistrationEntry(type);
      try {
        configurator(entry);
        components[type] = entry;
      } catch (Exception e) {
        Debug.LogException(e);
      }
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

    public string CreateWireKey() => type.AssemblyQualifiedName + (qualifier != null ? $"|{qualifier}" : "");
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
      this.key = key;
      wireKey = key.CreateWireKey();
      scripted = null;
      flags = DependencyFlags.Wirable;
      if (required) flags |= DependencyFlags.Required;
    }

    public ComponentDependency(IScriptedDependency scripted, bool required) {
      this.scripted = scripted;
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
    public readonly HXContainer container;
    public readonly ManagedScope scope;

    public ComponentLoadContext(HXContainer container, ManagedScope scope) {
      this.container = container;
      this.scope = scope;
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

    public virtual string CreateWireKey() => GetType().AssemblyQualifiedName;

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

    public ComponentLoadResult(bool success) {
      this.success = success;
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
      this.type = type;
      name = type.Name;
    }

    public void RegisterHandlerBinding<T>(int priority = 0) {
      handlers.Add(new RegistrationHandlerBinding(typeof(T), priority));
    }

    public bool IsAsync => handlers.Any(static x => x.eventType == typeof(IAsyncChainEvt));
    public async UniTask<ComponentLoadResult> LoadAsync(ComponentLoadContext context) {
      var instance = ActivateAndLoadSync(context);
      if (instance is IEventListener listener) {
        var initEvent = new ComponentAsyncInitEvent();
        initEvent.Reset(context);
        await listener.HandlerList.RaiseLocalAsync(initEvent);
      }
      return true;
    }

    public ComponentLoadResult Load(ComponentLoadContext context) {
      ActivateAndLoadSync(context);
      return true;
    }

    private object ActivateAndLoadSync(ComponentLoadContext context) {
      var instance = activator(context);
      if (instance is IComponent component) {
       component.LoadComponent(); // Trigger mixin injectable load method
      }
      if (instance is IEventListener listener) {
        var initEvent = new ComponentInitEvent(context);
        listener.HandlerList.RaiseLocal(initEvent); // trigger proper
      }
      return instance;
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

    public HXContainer Container => _context.container;
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

    public HXContainer Container => _context.container;
    public ManagedScope Scope => _context.scope;
    public ComponentLoadContext Context => _context;
  }

  public delegate ComponentRegistrations RegistrationDiscoveryProvider();
}