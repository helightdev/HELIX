using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace HELIX.Context {
  /// <summary>Configures and initializes one managed scope.</summary>
  public sealed class ManagedScopeBuilder {
    private readonly ManagedContainer _container;
    private readonly ManagedScope _parent;
    private IScope _scope;
    private readonly List<IComponent> _components = new();
    private readonly HashSet<Type> _componentTypes = new();
    private readonly List<ScopeBinding> _bindings = new();
    private bool _built;

    internal ManagedScopeBuilder(ManagedContainer container, ManagedScope parent) {
      _container = container ?? throw new ArgumentNullException(nameof(container));
      _parent = parent ?? throw new ArgumentNullException(nameof(parent));
    }

    public ManagedScopeBuilder From(IScope scope) {
      EnsureNotBuilt();
      _scope = scope ?? throw new ArgumentNullException(nameof(scope));
      return this;
    }

    public ManagedScopeBuilder AddComponent(IComponent component) {
      EnsureNotBuilt();
      if (component == null) throw new ArgumentNullException(nameof(component));
      _components.Add(component);
      return this;
    }

    public ManagedScopeBuilder AddComponent(Type componentType) {
      EnsureNotBuilt();
      if (componentType == null) throw new ArgumentNullException(nameof(componentType));
      _componentTypes.Add(componentType);
      return this;
    }

    public ManagedScopeBuilder AddComponent<T>() => AddComponent(typeof(T));

    public ManagedScopeBuilder AddComponents(IEnumerable<IComponent> components) {
      EnsureNotBuilt();
      if (components == null) throw new ArgumentNullException(nameof(components));
      foreach (var component in components) AddComponent(component);
      return this;
    }

    public ManagedScopeBuilder AddBinding(TypeKey key, object value) {
      EnsureNotBuilt();
      if (key.type == null) throw new ArgumentException("A binding key must have a type.", nameof(key));
      if (value == null) throw new ArgumentNullException(nameof(value));
      if (!key.type.IsInstanceOfType(value)) {
        throw new ArgumentException(
          $"Value of type {value.GetType().FullName} cannot be bound as '{key}'.",
          nameof(value)
        );
      }
      _bindings.Add(new ScopeBinding(key, value));
      return this;
    }

    public ManagedScopeBuilder AddBinding<T>(T value, string qualifier = null) where T : class {
      return AddBinding(new TypeKey(typeof(T), qualifier), value);
    }

    public ManagedScope StartSync() {
      BeginBuild();
      return _container.StartScopeSync(_parent, _scope, _components, _componentTypes, _bindings);
    }

    public UniTask<ManagedScope> StartAsync() {
      BeginBuild();
      return _container.StartScopeAsync(_parent, _scope, _components, _componentTypes, _bindings);
    }

    private void BeginBuild() {
      EnsureNotBuilt();
      if (_scope == null) throw new ScopeLifecycleException("Select a scope before building a managed scope.");
      _built = true;
    }

    private void EnsureNotBuilt() {
      if (_built) throw new ScopeLifecycleException("A managed scope builder can only build one scope.");
    }
  }

  internal readonly struct ScopeBinding {
    public readonly TypeKey key;
    public readonly object value;

    public ScopeBinding(TypeKey key, object value) {
      this.key = key;
      this.value = value;
    }
  }
}
