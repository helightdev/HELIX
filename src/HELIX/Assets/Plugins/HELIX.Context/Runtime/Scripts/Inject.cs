using System;
using System.Collections.Generic;
using System.Reflection;
using HELIX.Context.Events;
using UnityEngine;

namespace HELIX.Context {
  // TODO

  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class InjectAttribute : Attribute {
    public readonly Source source;
    public readonly string qualifier;

    public InjectAttribute() {
      source = Source.Container;
      qualifier = null;
    }

    public InjectAttribute(string qualifier) {
      source = Source.Container;
      this.qualifier = qualifier;
    }

    public InjectAttribute(Source source) {
      this.source = source;
      qualifier = null;
    }

    public InjectAttribute(Source source, string qualifier) {
      this.source = source;
      this.qualifier = qualifier;
    }
  }

  public enum Source { Container, Components, Addressables, Resources }

  [MixinExpression(
    new[] { MixinOn.ConfigureRegistration },
    new[] { -100_000 },
    @"
@CODE<^*~HELIX.Context.RegistrationConfigurator> registration.name = ""@this:name"";
"
  )]
  [Mixin] public interface IComponentMixin : IMixin { }

  public delegate void RegistrationConfigurator(RegistrationEntry registration);

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

  public sealed class RegistrationEntry {
    public readonly Type type;
    public RegistrationEntry(Type type) {
      this.type = type;
    }

    public string name;
  }
}