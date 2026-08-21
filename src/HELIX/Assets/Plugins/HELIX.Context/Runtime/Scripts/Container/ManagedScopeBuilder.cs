using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.Assertions;

namespace HELIX.Context {
  /// <summary>Configures and initializes one managed scope.</summary>
  public sealed class ManagedScopeBuilder {
    private readonly ManagedContainer _container;
    private readonly ManagedScope _parent;
    private IScope _scope;
    private readonly List<IManaged> _managed = new();
    private readonly HashSet<Type> _managedTypes = new();
    private readonly List<ScopeBinding> _bindings = new();
    private bool _built;

    internal ManagedScopeBuilder(ManagedContainer container, ManagedScope parent) {
      _container = container ?? throw new ArgumentNullException(nameof(container));
      _parent = parent ?? throw new ArgumentNullException(nameof(parent));
    }

    public ManagedScopeBuilder From(IScope scope) {
      Assert.IsFalse(_built, "A managed scope builder can only build one scope.");
      _scope = scope ?? throw new ArgumentNullException(nameof(scope));
      return this;
    }

    public ManagedScopeBuilder AddComponent(IManaged component) {
      Assert.IsFalse(_built, "A managed scope builder can only build one scope.");
      if (component == null) throw new ArgumentNullException(nameof(component));
      _managed.Add(component);
      return this;
    }

    public ManagedScopeBuilder AddComponent(Type componentType) {
      Assert.IsFalse(_built, "A managed scope builder can only build one scope.");
      if (componentType == null) throw new ArgumentNullException(nameof(componentType));
      _managedTypes.Add(componentType);
      return this;
    }

    public ManagedScopeBuilder AddComponent<T>() => AddComponent(typeof(T));

    public ManagedScopeBuilder AddComponents(IEnumerable<IManaged> components) {
      Assert.IsFalse(_built, "A managed scope builder can only build one scope.");
      if (components == null) throw new ArgumentNullException(nameof(components));
      foreach (var component in components) AddComponent(component);
      return this;
    }

    public ManagedScopeBuilder AddBinding(TypeKey key, object value) {
      Assert.IsFalse(_built, "A managed scope builder can only build one scope.");
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

    public ManagedScopeBuilder AddProxyBinding(TypeKey key, Func<object> supplier) {
      Assert.IsFalse(_built, "A managed scope builder can only build one scope.");
      if (key.type == null) throw new ArgumentException("A binding key must have a type.", nameof(key));
      if (supplier == null) throw new ArgumentNullException(nameof(supplier));
      _bindings.Add(new ScopeBinding(key, supplier));
      return this;
    }

    public ManagedScopeBuilder AddProxyBinding<T>(Func<T> supplier, string qualifier = null) where T : class {
      if (supplier == null) throw new ArgumentNullException(nameof(supplier));
      return AddProxyBinding(new TypeKey(typeof(T), qualifier), supplier);
    }

    public ManagedScope StartSync() {
      BeginBuild();
      return _container.StartScopeSync(_parent, _scope, _managed, _managedTypes, _bindings);
    }

    public UniTask<ManagedScope> StartAsync() {
      BeginBuild();
      return _container.StartScopeAsync(_parent, _scope, _managed, _managedTypes, _bindings);
    }

    private void BeginBuild() {
      Assert.IsFalse(_built, "A managed scope builder can only build one scope.");
      Assert.IsNotNull(_scope, "Select a scope before building a managed scope.");
      _built = true;
    }
  }

  internal readonly struct ScopeBinding {
    public readonly TypeKey key;
    public readonly object value;
    public readonly Func<object> supplier;

    public ScopeBinding(TypeKey key, object value) {
      this.key = key;
      this.value = value;
      supplier = null;
    }

    public ScopeBinding(TypeKey key, Func<object> supplier) {
      this.key = key;
      value = null;
      this.supplier = supplier;
    }
  }
}
