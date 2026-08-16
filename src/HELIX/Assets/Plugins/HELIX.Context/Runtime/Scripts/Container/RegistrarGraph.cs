using System;
using System.Collections.Generic;
using System.Linq;

namespace HELIX.Context {
  /// <summary>Container-owned registrar index and structural dependency graph.</summary>
  internal sealed class RegistrarGraph {
    private ComponentRegistrations _registrations;

    public ComponentRegistrations Prepare(ComponentRegistrations registrations) {
      if (registrations == null) throw new ArgumentNullException(nameof(registrations));
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
        if (entry.keys.Any(static key => key.type == null)) {
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
        if (registration == null || pair.Key != registration.type || !typeof(IScope).IsAssignableFrom(pair.Key)) {
          throw new ComponentGraphException("The scope registration index contains an invalid entry.");
        }
        if (registration.parentTypes.Any(static type => !typeof(IScope).IsAssignableFrom(type)) ||
            registration.parentType != null && !typeof(IScope).IsAssignableFrom(registration.parentType)) {
          throw new ComponentGraphException($"Scope {pair.Key.FullName} contains a parent type that is not an IScope.");
        }
      }

      return _registrations = registrations;
    }

    public List<RegistrationEntry> For(ManagedScope managed) {
      if (_registrations == null) throw new ScopeLifecycleException("The registrar graph has not been prepared.");
      var scopeType = managed.scope.GetType();
      var entries = _registrations.components.Values
        .Where(entry => (entry.scope ?? typeof(ApplicationScope)) == scopeType)
        .OrderBy(static entry => entry.name, StringComparer.Ordinal)
        .ToList();
      var providers = new HashSet<TypeKey>();
      foreach (var entry in entries) {
        foreach (var key in entry.keys.Distinct()) providers.Add(key);
        foreach (var publication in entry.publications.Where(static publication =>
          publication.IsTyped && publication.flags.HasFlag(DependencyFlags.Required)
        )) providers.Add(publication.key);
      }

      foreach (var entry in entries) {
        foreach (var dependency in entry.dependencies.Where(static dependency =>
          dependency.flags.HasFlag(DependencyFlags.Required)
        )) {
          if (dependency.IsTyped) {
            if (providers.Contains(dependency.key) || managed.ResolveAll(dependency.key).Count > 0) continue;
            throw new ComponentGraphException(
              $"Component '{entry.name}' requires '{dependency.key}', but no visible component guarantees that key."
            );
          }
          if (dependency.scripted == null) {
            throw new ComponentGraphException($"Component '{entry.name}' contains an invalid untyped dependency.");
          }
          if (dependency.flags.HasFlag(DependencyFlags.ImplicitLoadable)) continue;
          if (dependency.flags.HasFlag(DependencyFlags.Wirable) && HasPublication(entries, dependency.wireKey)) continue;
          throw new ComponentGraphException(
            $"Component '{entry.name}' requires scripted dependency '{dependency.wireKey}', but it is not implicitly " +
            "loadable and has no guaranteed publication provider."
          );
        }
      }
      return entries;
    }

    public bool HasLocalProvider(IEnumerable<RegistrationEntry> entries, ComponentDependency dependency) {
      if (dependency.IsTyped) {
        return entries.Any(entry => entry.keys.Contains(dependency.key) || entry.publications.Any(publication =>
          publication.IsTyped && publication.key.Equals(dependency.key) &&
          publication.flags.HasFlag(DependencyFlags.Required)
        ));
      }
      return dependency.flags.HasFlag(DependencyFlags.Wirable) && HasPublication(entries, dependency.wireKey);
    }

    private static bool HasPublication(IEnumerable<RegistrationEntry> entries, string wireKey) =>
      entries.Any(entry => entry.publications.Any(publication =>
        publication.flags.HasFlag(DependencyFlags.Required) && publication.wireKey == wireKey
      ));
  }
}
