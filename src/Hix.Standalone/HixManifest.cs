using Hix.Hal;

namespace Hix.Standalone;

public sealed record HixSourceSets(IReadOnlyList<string> Main, IReadOnlyList<string> Test, IReadOnlyList<string> Modules) {
  public static HixSourceSets Default { get; } = new(["src/**"], ["test/**"], ["modules/**"]);
}
public abstract record HixDependency(string Version);
public sealed record PathDependency(string Uri, string Version = null) : HixDependency(Version);
public sealed record GitDependency(string Repository, string Branch = null, string Rev = null, string Tag = null,
  string Version = null) : HixDependency(Version);
public sealed record ModuleDependency(string Version = null) : HixDependency(Version);
public sealed record RepositoryDependency(string Constraint) : HixDependency(Constraint);

public sealed record HixManifest(string Path, string Name, HixVersion Version, string Description, string Repository,
  string License, string Author, string ApplicationMain, HixSourceSets Sources,
  IReadOnlyDictionary<string, HixDependency> Dependencies) {
  public string Directory => System.IO.Path.GetDirectoryName(Path);
  public static HixManifest Find(string file) {
    var directory = System.IO.Directory.Exists(file) ? System.IO.Path.GetFullPath(file) : System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(file));
    while (directory != null) { var manifest = System.IO.Path.Combine(directory, "manifest.hal"); if (File.Exists(manifest)) return Load(manifest); directory = System.IO.Directory.GetParent(directory)?.FullName; }
    return null;
  }
  public static HixManifest Load(string file) {
    file = ResolveManifestPath(file); var document = HalDocumentSyntax.Parse(File.ReadAllText(file));
    if (document.Diagnostics.Count != 0) throw Error(file, document.Diagnostics[0].Message);
    if (document.Sections.Count == 0 || document.Sections[0].Type != "Manifest") throw Error(file, "root section must have type Manifest");
    if (document.Sections.Count != 1) throw Error(file, "a manifest must contain exactly one section");
    var fields = document.Sections[0].Fields; EnsureUnique(file, fields, "manifest");
    var allowed = new HashSet<string>(["name", "version", "description", "repository", "license", "author", "applicationMain", "sources", "dependencies"], StringComparer.Ordinal);
    var unknown = fields.FirstOrDefault(field => !allowed.Contains(field.Name));
    if (unknown != null) throw Error(file, "unknown manifest field '" + unknown.Name + "'");
    var name = OptionalText(fields, "name") ?? throw Error(file, "manifest field 'name' is required");
    var versionText = OptionalText(fields, "version");
    return new(file, name, versionText == null ? null : HixVersion.Parse(versionText), OptionalText(fields, "description"),
      OptionalText(fields, "repository"), OptionalText(fields, "license"), OptionalText(fields, "author"),
      OptionalText(fields, "applicationMain") ?? "main", ParseSources(file, fields.FirstOrDefault(x => x.Name == "sources")?.Value),
      ParseDependencies(file, fields.FirstOrDefault(x => x.Name == "dependencies")?.Value));
  }
  public static string ResolveManifestPath(string path) {
    path = System.IO.Path.GetFullPath(path); if (System.IO.Directory.Exists(path)) path = System.IO.Path.Combine(path, "manifest.hal");
    if (!File.Exists(path)) throw new FileNotFoundException("Hix manifest does not exist", path); return path;
  }
  private static HixSourceSets ParseSources(string file, HalValueSyntax value) {
    if (value == null) return HixSourceSets.Default;
    if (value is not HalTableSyntax table) throw Error(file, "Manifest.sources must be a table"); EnsureUnique(file, table.Fields, "sources");
    var unknown = table.Fields.FirstOrDefault(x => x.Name is not ("main" or "test" or "modules")); if (unknown != null) throw Error(file, "unknown sources field '" + unknown.Name + "'");
    return new(Patterns(table.Fields, "main", HixSourceSets.Default.Main), Patterns(table.Fields, "test", HixSourceSets.Default.Test), Patterns(table.Fields, "modules", HixSourceSets.Default.Modules));
  }
  private static IReadOnlyDictionary<string, HixDependency> ParseDependencies(string file, HalValueSyntax value) {
    if (value == null) return new Dictionary<string, HixDependency>(); if (value is not HalTableSyntax table) throw Error(file, "Manifest.dependencies must be a table"); EnsureUnique(file, table.Fields, "dependencies");
    return table.Fields.ToDictionary(x => x.Name, x => ParseDependency(file, x.Name, x.Value), StringComparer.Ordinal);
  }
  private static HixDependency ParseDependency(string file, string name, HalValueSyntax value) {
    if (value is HalScalarSyntax {Value: string text}) return text == "module" ? new ModuleDependency() : new RepositoryDependency(text);
    if (value is not HalTypedSyntax {Value: HalTableSyntax table} typed) throw Error(file, "dependency '" + name + "' must be a string or tagged descriptor");
    EnsureUnique(file, table.Fields, "dependency '" + name + "'"); string Field(string key) => OptionalText(table.Fields, key); var version = Field("version");
    return typed.Type switch {
      "ModuleDependency" => Only(file, name, table.Fields, ["version"], new ModuleDependency(version)),
      "PathDependency" => Only(file, name, table.Fields, ["uri", "version"], new PathDependency(Field("uri") ?? throw Error(file, "PathDependency.uri is required"), version)),
      "GitDependency" => Only(file, name, table.Fields, ["repository", "branch", "rev", "tag", "version"], new GitDependency(Field("repository") ?? throw Error(file, "GitDependency.repository is required"), Field("branch"), Field("rev"), Field("tag"), version)),
      _ => throw Error(file, "unknown dependency type '" + typed.Type + "'")};
  }
  private static T Only<T>(string file, string dependency, IReadOnlyList<HalFieldSyntax> fields, IReadOnlyCollection<string> allowed, T result) { var unknown = fields.FirstOrDefault(x => !allowed.Contains(x.Name)); if (unknown != null) throw Error(file, "unknown field '" + unknown.Name + "' in dependency '" + dependency + "'"); return result; }
  private static IReadOnlyList<string> Patterns(IReadOnlyList<HalFieldSyntax> fields, string name, IReadOnlyList<string> fallback) => fields.FirstOrDefault(x => x.Name == name)?.Value switch {null => fallback, HalListSyntax list => list.Values.Select(x => Text(x, "sources." + name)).ToArray(), var scalar => [Text(scalar, "sources." + name)]};
  private static string OptionalText(IReadOnlyList<HalFieldSyntax> fields, string name) => fields.FirstOrDefault(x => x.Name == name) is {Value: var value} ? Text(value, name) : null;
  private static string Text(HalValueSyntax value, string field) => value is HalScalarSyntax {Value: string text} ? text : throw new ArgumentException("Manifest." + field + " must be a string");
  private static void EnsureUnique(string file, IReadOnlyList<HalFieldSyntax> fields, string owner) { var duplicate = fields.GroupBy(x => x.Name, StringComparer.Ordinal).FirstOrDefault(x => x.Count() > 1); if (duplicate != null) throw Error(file, "duplicate " + owner + " field '" + duplicate.Key + "'"); }
  private static ArgumentException Error(string file, string message) => new(file.Replace('\\', '/') + ": " + message);
}
