using System;
using System.Collections.Generic;
using System.Linq;
using HELIX.Prose;

namespace HELIX.Context {
  /// <summary>Renders declared registrations or live scopes as a printable Prose dependency graph.</summary>
  public static class ManagedGraphProse {
    public static void WriteDeclared(IProseWriter writer, ManagedRegistrations registrations) {
      if (writer == null) throw new ArgumentNullException(nameof(writer));
      if (registrations == null) throw new ArgumentNullException(nameof(registrations));

      using (writer.Tree()) {
        writer.Name("Declared dependency graph");
        foreach (var group in registrations.components.Values
          .GroupBy(static entry => entry.scope)
          .OrderBy(static group => group.Key == null ? "" : TypeName(group.Key), StringComparer.Ordinal)) {
          using (writer.Tree()) {
            writer.Name(group.Key == null ? "Unscoped" : TypeName(group.Key));
            var plan = RegistrarGraph.Plan(group);
            var scripted = plan
              .SelectMany(static entry => entry.dependencies)
              .Where(static dependency => dependency.IsScripted &&
                dependency.flags.HasFlag(DependencyFlags.ImplicitLoadable))
              .Select(static dependency => dependency.scripted)
              .Distinct(ReferenceComparer<IScriptedDependency>.Instance)
              .ToList();
            var phases = plan.Select(static entry => entry.phase)
              .Concat(scripted.Select(static dependency => dependency.Phase))
              .Distinct()
              .OrderBy(static phase => phase);
            foreach (var phase in phases) {
              using (writer.Tree()) {
                writer.Name(PhaseName(phase));
                foreach (var dependency in scripted
                  .Where(dependency => dependency.Phase == phase)
                  .OrderBy(static dependency => dependency.Order)
                  .ThenBy(static dependency => dependency.WireKey, StringComparer.Ordinal))
                  WriteDeclaredScriptedDependency(writer, dependency);
                foreach (var entry in plan.Where(entry => entry.phase == phase)) WriteDeclaredEntry(writer, entry);
              }
            }
          }
        }
      }
    }

    public static void WriteLive(IProseWriter writer, ManagedContainer container) {
      if (writer == null) throw new ArgumentNullException(nameof(writer));
      if (container == null) throw new ArgumentNullException(nameof(container));

      using (writer.Tree()) {
        writer.Name("Live dependency graph");
        if (container.TryGetScope(container.registrarScope, out var registrar)) WriteLiveScope(writer, registrar);
        else writer.Property("state", "not prepared", ProseDatatypes.String);
      }
    }

    private static void WriteDeclaredEntry(IProseWriter writer, ManagedRegistration entry) {
      using (writer.Tree()) {
        writer.Name($"{entry.name} : {TypeName(entry.type)}");
        if (entry.optional) writer.Property("optional", true, ProseDatatypes.Bool);
        WriteJoinedProperty(writer, "keys", entry.keys.Select(FormatKey));
        WriteDependencies(writer, "requires", entry.dependencies);
        WriteDependencies(writer, "publishes", entry.publications);
      }
    }

    private static void WriteDeclaredScriptedDependency(IProseWriter writer, IScriptedDependency dependency) {
      using (writer.Tree()) {
        writer.Name($"{dependency.WireKey ?? "<unwired>"} : {TypeName(dependency.GetType())}");
        writer.Property("scripted", true, ProseDatatypes.Bool);
      }
    }

    private static void WriteLiveScope(IProseWriter writer, ManagedScope scope) {
      using (writer.Tree()) {
        writer.Name($"{TypeName(scope.scope.GetType())} [{scope.State}]");
        foreach (var phase in scope.loadedComponents.GroupBy(static loaded => loaded.registration.phase)) {
          using (writer.Tree()) {
            writer.Name(PhaseName(phase.Key));
            foreach (var loaded in phase) {
              using (writer.Tree()) {
                writer.Name($"{loaded.registration.name} : {TypeName(loaded.instance.GetType())}");
                WriteJoinedProperty(
                  writer,
                  "keys",
                  scope.bindings.Where(pair => pair.Value.Any(binding =>
                    ReferenceEquals(binding.owner, loaded.registration)
                  )).Select(pair => FormatKey(pair.Key))
                );
                var loader = ScopeLoader.ActiveOrNull;
                var publications = loader == null
                  ? Enumerable.Empty<string>()
                  : loader.publications.Where(pair => pair.Value.Contains(loaded.registration))
                    .Select(static pair => pair.Key)
                    .Where(key => loaded.registration.keys.All(componentKey =>
                      componentKey.CreateWireKey() != key
                    ));
                WriteJoinedProperty(writer, "publications", publications);
              }
            }
          }
        }
        foreach (var child in scope.managedChildren) WriteLiveScope(writer, child);
      }
    }

    private static void WriteDependencies(
      IProseWriter writer,
      string name,
      IEnumerable<ComponentDependency> dependencies
    ) {
      WriteJoinedProperty(writer, name, dependencies.Select(FormatDependency));
    }

    private static void WriteJoinedProperty(IProseWriter writer, string name, IEnumerable<string> values) {
      var items = values.ToList();
      writer.Property(name, Join(items), ProseDatatypes.String, hidden: items.Count == 0);
    }

    private static string FormatDependency(ComponentDependency dependency) {
      return dependency.wireKey ?? "<unwired>";
    }

    private static string FormatKey(TypeKey key) {
      return key.CreateWireKey();
    }

    private static string TypeName(Type type) {
      return type?.Name ?? "<unknown>";
    }

    private static string PhaseName(int phase) => $"Phase {phase}";

    private static string Join(IEnumerable<string> values) {
      var text = string.Join(", ", values);
      return text.Length == 0 ? "—" : text;
    }
  }
}
