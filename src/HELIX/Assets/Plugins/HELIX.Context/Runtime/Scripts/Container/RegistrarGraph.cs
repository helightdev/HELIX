using System;
using System.Collections.Generic;
using System.Linq;

namespace HELIX.Context {
  /// <summary>Container-owned registrar index and structural dependency graph.</summary>
  internal sealed class RegistrarGraph {
    private ComponentRegistrations _registrations;
    private readonly Dictionary<Type, List<ComponentRegistration>> _scopePlans = new();

    internal static bool PublicationMatches(
      ComponentDependency publication,
      ComponentDependency dependency
    ) {
      if (dependency.IsTyped)
        return publication.IsTyped && publication.key.Equals(dependency.key);
      return dependency.flags.HasFlag(DependencyFlags.Wirable) &&
        publication.flags.HasFlag(DependencyFlags.Wirable) &&
        publication.wireKey == dependency.wireKey;
    }

    internal static bool CanProvide(ComponentRegistration entry, ComponentDependency dependency) {
      return dependency.IsTyped && entry.keys.Contains(dependency.key) ||
        entry.publications.Any(publication => PublicationMatches(publication, dependency));
    }

    internal static int TransformerPriority(ComponentRegistration entry) {
      var transformed = entry.dependencies.Where(dependency =>
        entry.publications.Any(publication => PublicationMatches(publication, dependency))
      ).ToList();
      if (transformed.Count == 0) return 0;
      return transformed.Any(static dependency => !dependency.IsCollection) ? 2 : 1;
    }

    internal static IOrderedEnumerable<ComponentRegistration> OrderFallbackCandidates(
      IEnumerable<ComponentRegistration> entries
    ) {
      return entries
        .OrderBy(static entry => entry.phase)
        // Transformers always precede terminal consumers, regardless of component order.
        .ThenByDescending(static entry => TransformerPriority(entry) > 0)
        // Component order is authoritative among transformers (and among consumers).
        .ThenBy(static entry => entry.order)
        // At equal order, scalar transformers precede collection transformers.
        .ThenByDescending(TransformerPriority)
        .ThenBy(static entry => entry.name, StringComparer.Ordinal);
    }

    /// <summary>Produces the statically knowable plan while keeping phases as hard barriers.</summary>
    internal static List<ComponentRegistration> Plan(IEnumerable<ComponentRegistration> registrations) {
      return registrations.Distinct()
        .GroupBy(static entry => entry.phase)
        .OrderBy(static group => group.Key)
        .SelectMany(PlanInnerPhase)
        .ToList();
    }

    private static List<ComponentRegistration> PlanInnerPhase(IEnumerable<ComponentRegistration> registrations) {
      var all = registrations.Distinct().ToList();
      if (all.Count == 0) return new List<ComponentRegistration>();
      var phase = all[0].phase;
      var pending = all
        .OrderBy(static entry => entry.order)
        .ThenBy(static entry => entry.name, StringComparer.Ordinal)
        .ToList();
      var loaded = new List<ComponentRegistration>();

      bool IsGuaranteedAvailable(ComponentDependency dependency) {
        return loaded.Any(entry =>
          dependency.IsTyped && entry.keys.Contains(dependency.key) ||
          entry.publications.Any(publication =>
            publication.flags.HasFlag(DependencyFlags.Required) &&
            PublicationMatches(publication, dependency)
          )
        );
      }

      bool RequiredDependenciesSatisfied(ComponentRegistration entry) {
        return entry.dependencies.All(dependency => {
          if (!dependency.flags.HasFlag(DependencyFlags.Required)) return true;
          if (dependency.IsScripted && dependency.flags.HasFlag(DependencyFlags.ImplicitLoadable))
            return dependency.scripted.Phase <= phase;
          var hasDeclaredProvider = all.Any(provider => CanProvide(provider, dependency));
          return !hasDeclaredProvider || IsGuaranteedAvailable(dependency);
        });
      }

      bool HasFutureScriptedDependency(ComponentRegistration entry, int phase) {
        return entry.dependencies.Any(dependency =>
          dependency.IsScripted && dependency.flags.HasFlag(DependencyFlags.ImplicitLoadable) &&
          dependency.scripted.Phase > phase
        );
      }

      bool HasPendingProvider(ComponentRegistration consumer, ComponentDependency dependency) {
        var count = pending.Count(entry => CanProvide(entry, dependency));
        return count > (CanProvide(consumer, dependency) ? 1 : 0);
      }

      while (pending.Count > 0) {
        var next = pending.FirstOrDefault(entry =>
          RequiredDependenciesSatisfied(entry) &&
          !HasFutureScriptedDependency(entry, phase) &&
          entry.dependencies.All(dependency => !HasPendingProvider(entry, dependency))
        );
        next ??= OrderFallbackCandidates(pending.Where(entry =>
          RequiredDependenciesSatisfied(entry) && !HasFutureScriptedDependency(entry, phase)
        )).FirstOrDefault();
        if (next == null) break;
        pending.Remove(next);
        loaded.Add(next);
      }

      // Retain a deterministic declaration for structurally unresolved cycles. The runtime
      // loader remains responsible for rejecting them if no scope binding can break the cycle.
      while (pending.Count > 0) {
        var next = OrderFallbackCandidates(pending).First();
        pending.Remove(next);
        loaded.Add(next);
      }

      return loaded;
    }

    private static void InsertIntoPlan(List<ComponentRegistration> plan, ComponentRegistration entry) {
      if (plan.Contains(entry)) return;
      var samePhase = plan.Where(candidate => candidate.phase == entry.phase).ToList();
      if (samePhase.Count == 0) {
        var phaseIndex = plan.FindIndex(candidate => candidate.phase > entry.phase);
        plan.Insert(phaseIndex < 0 ? plan.Count : phaseIndex, entry);
        return;
      }

      var proposed = PlanInnerPhase(samePhase.Append(entry));
      var proposedIndex = proposed.IndexOf(entry);
      ComponentRegistration preceding = null;
      ComponentRegistration following = null;
      for (var i = proposedIndex - 1; i >= 0; i--) {
        if (!samePhase.Contains(proposed[i])) continue;
        preceding = proposed[i];
        break;
      }
      for (var i = proposedIndex + 1; i < proposed.Count; i++) {
        if (!samePhase.Contains(proposed[i])) continue;
        following = proposed[i];
        break;
      }

      if (preceding != null) plan.Insert(plan.IndexOf(preceding) + 1, entry);
      else if (following != null) plan.Insert(plan.IndexOf(following), entry);
      else plan.Add(entry);
    }

    public ComponentRegistrations Prepare(ComponentRegistrations registrations) {
      if (registrations == null) throw new ArgumentNullException(nameof(registrations));
      foreach (var pair in registrations.components) {
        var entry = pair.Value;
        if (entry == null || pair.Key != entry.type)
          throw new ComponentGraphException("The component registration index contains an invalid entry.");
        if (entry.scope != null && !typeof(IScope).IsAssignableFrom(entry.scope)) {
          throw new ComponentGraphException(
            $"Component '{entry.name}' is assigned to {entry.scope.FullName}, which is not an IScope."
          );
        }
        if (entry.keys.Any(static key => key.type == null))
          throw new ComponentGraphException($"Component '{entry.name}' exposes an untyped key.");
        if (entry.conditions.Any(static condition => condition == null))
          throw new ComponentGraphException($"Component '{entry.name}' contains a null condition.");
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
        if (registration == null || pair.Key != registration.type || !typeof(IScope).IsAssignableFrom(pair.Key))
          throw new ComponentGraphException("The scope registration index contains an invalid entry.");
        if (registration.parentTypes.Any(static type => !typeof(IScope).IsAssignableFrom(type)) ||
          (registration.parentType != null && !typeof(IScope).IsAssignableFrom(registration.parentType)))
          throw new ComponentGraphException($"Scope {pair.Key.FullName} contains a parent type that is not an IScope.");
      }

      _scopePlans.Clear();
      foreach (var group in registrations.components.Values
        .Where(static entry => entry.scope != null)
        .GroupBy(static entry => entry.scope)) {
        _scopePlans.Add(group.Key, Plan(group));
      }

      return _registrations = registrations;
    }

    public List<ComponentRegistration> For(
      ManagedScope managed,
      IEnumerable<ComponentRegistration> contributions = null,
      Func<ComponentRegistration, bool> include = null
    ) {
      if (_registrations == null) throw new ScopeLifecycleException("The registrar graph has not been prepared.");
      var scopeType = managed.scope.GetType();
      var entries = _scopePlans.TryGetValue(scopeType, out var prepared)
        ? prepared.Where(entry => include?.Invoke(entry) ?? true).ToList()
        : new List<ComponentRegistration>();
      foreach (var contribution in OrderFallbackCandidates(
        (contributions ?? Enumerable.Empty<ComponentRegistration>()).Distinct()
      )) {
        if (entries.Contains(contribution) || !(include?.Invoke(contribution) ?? true)) continue;
        InsertIntoPlan(entries, contribution);
      }
      var providers = new HashSet<TypeKey>();
      foreach (var entry in entries) {
        foreach (var key in entry.keys.Distinct()) providers.Add(key);
        foreach (var publication in entry.publications.Where(static publication =>
          publication.IsTyped && publication.flags.HasFlag(DependencyFlags.Required)
        )) providers.Add(publication.key);
      }

      foreach (var entry in entries) {
        if (entry.optional) continue;
        foreach (var dependency in entry.dependencies.Where(static dependency =>
          dependency.flags.HasFlag(DependencyFlags.Required)
        )) {
          if (dependency.IsTyped) {
            if (providers.Contains(dependency.key) || managed.ResolveAll(dependency.key).Count > 0) continue;
            throw new ComponentGraphException(
              $"Component '{entry.name}' requires '{dependency.key}', but no visible component guarantees that key."
            );
          }
          if (dependency.scripted == null)
            throw new ComponentGraphException($"Component '{entry.name}' contains an invalid untyped dependency.");
          if (dependency.flags.HasFlag(DependencyFlags.ImplicitLoadable)) continue;
          if (dependency.flags.HasFlag(DependencyFlags.Wirable) &&
            HasPublication(entries, dependency.wireKey)) continue;
          throw new ComponentGraphException(
            $"Component '{entry.name}' requires scripted dependency '{dependency.wireKey}', but it is not implicitly " +
            "loadable and has no guaranteed publication provider."
          );
        }
      }
      return entries;
    }

    public bool HasLocalProvider(IEnumerable<ComponentRegistration> entries, ComponentDependency dependency) {
      if (dependency.IsTyped) {
        return entries.Any(entry => entry.keys.Contains(dependency.key) || entry.publications.Any(publication =>
            publication.IsTyped && publication.key.Equals(dependency.key) &&
            publication.flags.HasFlag(DependencyFlags.Required)
          )
        );
      }
      return dependency.flags.HasFlag(DependencyFlags.Wirable) && HasPublication(entries, dependency.wireKey);
    }

    private static bool HasPublication(IEnumerable<ComponentRegistration> entries, string wireKey) {
      return entries.Any(entry => entry.publications.Any(publication =>
          publication.flags.HasFlag(DependencyFlags.Required) && publication.wireKey == wireKey
        )
      );
    }
  }
}
