using Newtonsoft.Json.Linq;

namespace Hix.Standalone.Analysis;

/// Compiler-only view of a C# host's declarations. Execution remains in the actual C# host.
internal sealed class HixInteropBackend(JObject schema) : HixBackend {
  protected override void RegisterFunctions(FunctionSignatureRegistryBuilder functions) {
    foreach (var type in schema["opaqueTypes"] ?? new JArray())
      functions.DefineOpaqueType((string)type["Identity"], (string)type["Name"], (int?)type["Arity"] ?? 0, (string)type["Documentation"] ?? "");
    foreach (var library in schema["libraries"] ?? new JArray()) functions.AddLibrary((string)library);
    foreach (var handler in schema["handlers"] ?? new JArray())
      functions.Add(new SimpleFunction((string)handler, (thread, values) => thread.Error("C# interop declarations are analysis-only")));
  }
  protected override void RegisterRoots(IDictionary<string, HixBackendRoot> roots) {
    foreach (var root in schema["roots"] ?? new JArray()) {
      var name = (string)root["Name"];
      roots[name] = new HixBackendRoot(name, (HixValueKind)Enum.Parse(typeof(HixValueKind), (string)root["kind"]),
        (bool?)root["RequiresPrelude"] ?? false, (bool?)root["HasEffects"] ?? true);
      if ((string)root["opaqueType"] is { } identity) {
        var type = schema["opaqueTypes"].First(x => (string)x["Identity"] == identity);
        roots[name] = roots[name] with { Pattern = new HixOpaqueTypeDefinition(identity, (string)type["Name"], (int?)type["Arity"] ?? 0).Construct() };
      }
    }
  }
}
