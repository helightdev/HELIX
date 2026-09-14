using Newtonsoft.Json.Linq;

namespace Hix.Standalone.Analysis;

/// Shared JSON request dispatcher for stdio and HTTP. HAL sessions are owned by this server.
public sealed class HixAnalysisServer {
  public HixAnalyzerService Analyzer { get; } = new(new HixStandaloneBackend(TextWriter.Null));
  private readonly HixAnalyzerService testAnalyzer = new(new HixTestBackend(TextWriter.Null));
  private HixAnalyzerService interopAnalyzer;
  public HalGraphService Hal { get; } = new();
  public object Dispatch(string method, JObject args) {
    string Text(string key) => (string)args[key];
    if (method?.StartsWith("assets/", StringComparison.Ordinal) == true) {
      using var database = AssetDatabase.Open(Text("path") ?? throw new ArgumentException("Missing workspace or asset path"));
      return method switch {
        "assets/index" => new { count = database.Refresh(), database = database.DatabasePath },
        "assets/query" => database.Query(Text("type"), (int?)args["offset"] ?? 0, (int?)args["limit"] ?? 100, Text("tag")),
        "assets/tags" => database.Tags(Text("assetId")),
        "assets/patterns" => JToken.FromObject(database.Patterns(Text("name"), Text("kind"), (int?)args["offset"] ?? 0, (int?)args["limit"] ?? 100), HixImageFile.Serializer()),
        "assets/build" => Build(database, Text("path"), Text("processorVersion") ?? "hal-values-v1"),
        "assets/dependencies" => database.Dependencies(Text("path"), (bool?)args["reverse"] ?? false),
        "assets/analyze" => Analysis(database),
        "assets/document" => database.Document(Text("path")),
        "assets/symbols" => database.Symbols(Text("name"), (int?)args["limit"] ?? 100),
        _ => throw new ArgumentException("Unknown method: " + method)
      };
    }
    if (method == "hix/configureInterop") {
      var schema = args["schema"] as JObject ?? throw new ArgumentException("Missing C# schema");
      interopAnalyzer = new HixAnalyzerService(new HixInteropBackend(schema),
        additionalDefinitions: schema["additionalDefinitions"]?.ToObject<HixDefinition[]>());
      return true;
    }
    var analyzer = Text("backend")?.ToLowerInvariant() switch {
      "test" => testAnalyzer,
      "unity" => interopAnalyzer ?? throw new InvalidOperationException("C# interop host schema is not configured"),
      _ => Analyzer
    };
    var session = Text("session"); var revision = (long?)args["revision"] ?? 0;
    return method switch {
      "hal/open" => Hal.Open(Text("path")),
      "hal/get" => Hal.Get(session),
      "hal/change" => Hal.Change(session, revision, Text("text")),
      "hal/candidates" => Hal.Candidates(session, revision, Text("node"), Text("port"), Text("direction")),
      "hal/edit" => Hal.Edit(session, revision, args["edit"]?.ToObject<HalGraphEdit>() ?? throw new ArgumentException("Missing edit")),
      "hal/save" => Hal.Save(session, revision),
      "hal/undo" => Hal.Undo(session, revision),
      "hal/redo" => Hal.Undo(session, revision, true),
      "hal/close" => Close(session, revision),
      "hix/synchronize" => Synchronize(analyzer, args["documents"]?.ToObject<HixDocument[]>()),
      "hix/format" => Hix.Compiler.HixFormatter.Format(Text("text")),
      "hal/format" => Hix.Hal.HalFormatter.Format(Text("text")),
      "hix/complete" => analyzer.Complete(Text("kind"), Text("prefix")),
      "hix/get" => analyzer.Get(Text("path")) ?? DiskDocument(Text("path")),
      "hix/remove" => analyzer.Remove(Text("path")),
      "hix/definitions" => analyzer.QueryDefinitions(Text("kind"), Text("receiverType"), Text("operandType"), Text("prefix")),
      _ => throw new ArgumentException("Unknown method: " + method)
    };
  }
  private static IReadOnlyList<HixDocumentSnapshot> Synchronize(HixAnalyzerService analyzer, HixDocument[] documents) {
    // Persist only saved files. Unsaved buffers stay in the session overlay.
    var roots = new HashSet<string>(HixWorkspace.PathComparer);
    foreach (var document in documents ?? []) {
      if (!File.Exists(document.Path)) continue;
      var workspace = HixWorkspace.Find(document.Path);
      if (!roots.Add(workspace.Directory)) continue;
      using var database = AssetDatabase.Open(document.Path);
      database.Analyze();
    }
    return analyzer.Synchronize(documents);
  }
  private static object Build(AssetDatabase database, string path, string version) {
    var build = database.Build(path, version);
    return new { build.AssetId, build.Path, build.Fingerprint, build.CacheHit, Values = JToken.FromObject(build.Values, HixImageFile.Serializer()) };
  }
  private static object Analysis(AssetDatabase database) {
    var analysis = database.Analyze();
    return new { analysis.Workspace, analysis.Fingerprint, analysis.Diagnostics, analysis.CacheHit, Patterns = JToken.FromObject(analysis.Patterns, HixImageFile.Serializer()) };
  }
  private static HixDocumentSnapshot DiskDocument(string path) {
    if (string.IsNullOrEmpty(path) || !File.Exists(path)) return null;
    using var database = AssetDatabase.Open(path); return database.Document(path);
  }
  private object Close(string id, long revision) { Hal.Close(id, revision); return new { closed = true }; }
}
