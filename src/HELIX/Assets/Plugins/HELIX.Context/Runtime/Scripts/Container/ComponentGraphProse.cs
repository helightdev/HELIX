using System;
using System.Collections.Generic;
using System.Linq;
using HELIX.Prose;

namespace HELIX.Context {
  /// <summary>Renders declared registrations or live scopes as a printable Prose dependency graph.</summary>
  public static class ComponentGraphProse {
    public static void WriteDeclared(IProseWriter writer, ComponentRegistrations registrations) {
      if (writer == null) throw new ArgumentNullException(nameof(writer));
      if (registrations == null) throw new ArgumentNullException(nameof(registrations));

      using (writer.Tree()) {
        writer.Name("Declared dependency graph");
        foreach (var group in registrations.components.Values
          .GroupBy(static entry => entry.scope ?? typeof(ApplicationScope))
          .OrderBy(static group => TypeName(group.Key), StringComparer.Ordinal)) {
          using (writer.Tree()) {
            writer.Name(TypeName(group.Key));
            foreach (var entry in group.OrderBy(static entry => entry.name, StringComparer.Ordinal))
              WriteDeclaredEntry(writer, entry);
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
        else writer.Property("state", "not prepared", ProseFormatters.String);
      }
    }

    private static void WriteDeclaredEntry(IProseWriter writer, RegistrationEntry entry) {
      using (writer.Tree()) {
        writer.Name($"{entry.name} : {TypeName(entry.type)}");
        writer.Property("keys", Join(entry.keys.Select(FormatKey)), ProseFormatters.String);
        WriteDependencies(writer, "requires", entry.dependencies);
        WriteDependencies(writer, "publishes", entry.publications);
      }
    }

    private static void WriteLiveScope(IProseWriter writer, ManagedScope scope) {
      using (writer.Tree()) {
        writer.Name($"{TypeName(scope.scope.GetType())} [{scope.State}]");
        foreach (var loaded in scope.LoadedComponents) {
          using (writer.Tree()) {
            writer.Name($"{loaded.registration.name} : {TypeName(loaded.instance.GetType())}");
            writer.Property(
              "keys",
              Join(scope.BoundKeys(loaded.registration).Select(FormatKey)),
              ProseFormatters.String
            );
            var publications = scope.PublishedWireKeys(loaded.registration).Where(key =>
              !loaded.registration.keys.Any(componentKey => componentKey.CreateWireKey() == key)
            );
            writer.Property("publications", Join(publications), ProseFormatters.String);
          }
        }
        foreach (var child in scope.ManagedChildren) WriteLiveScope(writer, child);
      }
    }

    private static void WriteDependencies(
      IProseWriter writer,
      string name,
      IEnumerable<ComponentDependency> dependencies
    ) {
      writer.Property(name, Join(dependencies.Select(FormatDependency)), ProseFormatters.String);
    }

    private static string FormatDependency(ComponentDependency dependency) {
      return dependency.IsTyped
        ? FormatKey(dependency.key)
        : dependency.wireKey ?? dependency.scripted?.GetType().FullName ?? "<unwired>";
    }

    private static string FormatKey(TypeKey key) {
      return key.qualifier == null
        ? TypeName(key.type)
        : $"{TypeName(key.type)} | {key.qualifier}";
    }

    private static string TypeName(Type type) {
      return type?.Name ?? "<unknown>";
    }

    private static string Join(IEnumerable<string> values) {
      var text = string.Join(", ", values);
      return text.Length == 0 ? "—" : text;
    }
  }
}