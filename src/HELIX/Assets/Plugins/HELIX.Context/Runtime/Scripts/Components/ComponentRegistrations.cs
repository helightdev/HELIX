using System;
using System.Collections.Generic;
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

  public class ScopeRegistrations {
    public readonly HashSet<Type> declared = new();
    public readonly HashSet<Type> required = new();
  }

  public class ComponentGraph {
    public readonly Dictionary<Type, RegistrationEntry> components = new();
    public readonly Dictionary<Type, ScopeRegistrations> scopes = new();
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
  }

  public readonly struct TypeDependency {
    public readonly TypeKey key;
    public readonly bool required;

    public TypeDependency(TypeKey key, bool required) {
      this.key = key;
      this.required = required;
    }
  }

  public sealed class RegistrationEntry {
    public readonly Type type;
    public readonly List<RegistrationHandlerBinding> handlers = new();
    public readonly List<TypeDependency> dependencies = new();
    public readonly List<TypeKey> keys = new();
    public Type scopeType;

    public string name;

    public RegistrationEntry(Type type) {
      this.type = type;
    }

    public void RegisterHandlerBinding<T>(int priority = 0) {
      handlers.Add(new RegistrationHandlerBinding(typeof(T), priority));
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

  public delegate ComponentRegistrations RegistrationDiscoveryProvider();
}