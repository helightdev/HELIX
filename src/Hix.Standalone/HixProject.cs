using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Hix.Standalone;

public sealed record HixModule(string QualifiedName, HixManifest Manifest);
public sealed class HixProject {
  private readonly Dictionary<string, HixModule> modules;
  private readonly HashSet<string> discoveredPaths = new(PathComparer);
  private HixProject(HixManifest manifest, Dictionary<string, HixModule> modules) { Manifest = manifest; this.modules = modules; discoveredPaths.Add(manifest.Path); }
  public HixManifest Manifest { get; }
  public IReadOnlyDictionary<string, HixModule> Modules => modules;
  public static HixProject Load(string path) {
    var manifest = HixManifest.Load(path); var modules = new Dictionary<string, HixModule>(StringComparer.Ordinal);
    Discover(manifest, manifest.Name, modules, new HashSet<string>(PathComparer)); return new(manifest, modules);
  }
  public IReadOnlyList<string> AssetFiles() => MatchAssetFiles(Manifest.Directory, Manifest.Sources.Assets);
  public IReadOnlyList<HixSourceFile> MacroSources() => MatchFiles(Manifest.Directory, Manifest.Sources.Macros)
    .Select(path => new HixSourceFile(path, File.ReadAllText(path))).ToArray();
  public IReadOnlyList<HixSourceFile> MainSources() => Sources(false);
  public IReadOnlyList<HixSourceFile> TestSources() => Sources(true);
  public IReadOnlyList<HixSourceFile> Sources(bool includeTests) {
    return SourcesFor(Manifest, includeTests);
  }
  public IReadOnlyList<HixSourceFile> SourcesFor(HixManifest manifest, bool includeTests = false) {
    var result = new List<HixSourceFile>(); var files = new HashSet<string>(PathComparer); var visited = new HashSet<string>(PathComparer);
    var name = modules.Values.FirstOrDefault(m => PathComparer.Equals(m.Manifest.Path, manifest.Path))?.QualifiedName ?? manifest.Name;
    Visit(manifest, name, includeTests, result, files, visited, []); return result;
  }
  private void Visit(HixManifest manifest, string qualifiedName, bool tests, ICollection<HixSourceFile> result,
    ISet<string> files, ISet<string> visited, IReadOnlyList<string> chain) {
    if (chain.Contains(manifest.Path, PathComparer)) throw new ArgumentException("Hix dependency cycle: " + string.Join(" -> ", chain.Append(manifest.Path).Select(x => x.Replace('\\', '/'))));
    if (!visited.Add(manifest.Path)) return;
    var next = chain.Append(manifest.Path).ToArray();
    foreach (var dependency in manifest.Dependencies) {
      var resolved = Resolve(manifest, qualifiedName, dependency.Key, dependency.Value);
      ValidateVersion(manifest, dependency.Key, dependency.Value.Version, resolved.Manifest);
      Visit(resolved.Manifest, resolved.QualifiedName, false, result, files, visited, next);
    }
    foreach (var file in MatchFiles(manifest.Directory, manifest.Sources.Main).Concat(tests ? MatchFiles(manifest.Directory, manifest.Sources.Test) : []))
      if (files.Add(file)) result.Add(new(file, File.ReadAllText(file)));
  }
  private HixModule Resolve(HixManifest owner, string ownerName, string dependencyName, HixDependency dependency) => dependency switch {
    ModuleDependency => ResolveModule(ownerName, dependencyName),
    PathDependency path => ResolvePath(owner, dependencyName, path),
    GitDependency => throw new NotSupportedException("Git dependency '" + dependencyName + "' is not supported yet"),
    RepositoryDependency => throw new NotSupportedException("Repository dependency '" + dependencyName + "' is not supported yet"),
    _ => throw new ArgumentException("Unknown dependency '" + dependencyName + "'")
  };
  private HixModule ResolvePath(HixManifest owner, string name, PathDependency dependency) {
    var path = System.IO.Path.IsPathRooted(dependency.Uri) ? dependency.Uri : System.IO.Path.Combine(owner.Directory, dependency.Uri);
    HixManifest manifest; try { manifest = HixManifest.Load(path); } catch (FileNotFoundException) { throw new FileNotFoundException("Path dependency '" + name + "' does not contain a manifest", path); }
    if (discoveredPaths.Add(manifest.Path))
      Discover(manifest, manifest.Name, modules, new HashSet<string>(PathComparer));
    return new(manifest.Name, manifest);
  }
  private HixModule ResolveModule(string ownerName, string name) {
    if (modules.TryGetValue(name, out var absolute)) return absolute;
    var parts = ownerName.Split('/');
    for (var count = parts.Length; count > 0; count--) { var candidate = string.Join("/", parts.Take(count).Append(name)); if (modules.TryGetValue(candidate, out var module)) return module; }
    throw new ArgumentException("Module dependency '" + name + "' could not be resolved from '" + ownerName + "'");
  }
  private static void ValidateVersion(HixManifest owner, string name, string constraint, HixManifest dependency) {
    if (constraint == null) return; if (dependency.Version == null) throw new ArgumentException(owner.Path.Replace('\\', '/') + ": dependency '" + name + "' has constraint '" + constraint + "' but declares no version");
    if (!HixVersionConstraint.Parse(constraint).Matches(dependency.Version)) throw new ArgumentException(owner.Path.Replace('\\', '/') + ": dependency '" + name + "' version " + dependency.Version + " does not satisfy " + constraint);
  }
  private static void Discover(HixManifest manifest, string qualified, IDictionary<string, HixModule> modules, ISet<string> visited) {
    if (!visited.Add(manifest.Path)) return;
    var candidates = MatchDirectories(manifest.Directory, manifest.Sources.Modules)
      .Where(directory => File.Exists(System.IO.Path.Combine(directory, "manifest.hal"))).ToArray();
    foreach (var directory in candidates.Where(candidate => !candidates.Any(parent => !PathComparer.Equals(parent, candidate) && IsWithin(parent, candidate)))) {
      var path = System.IO.Path.Combine(directory, "manifest.hal"); var child = HixManifest.Load(path);
      var name = qualified + "/" + child.Name; if (modules.ContainsKey(name)) throw new ArgumentException("Duplicate Hix module '" + name + "'");
      modules.Add(name, new(name, child)); Discover(child, name, modules, visited);
    }
  }
  private static bool IsWithin(string parent, string child) {
    var prefix = System.IO.Path.GetFullPath(parent).TrimEnd(System.IO.Path.DirectorySeparatorChar) + System.IO.Path.DirectorySeparatorChar;
    return System.IO.Path.GetFullPath(child).StartsWith(prefix, IsWindows ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
  }
  public static IReadOnlyList<string> MatchFiles(string directory, IEnumerable<string> patterns) => Match(directory, patterns, false).Where(File.Exists).Where(x => x.EndsWith(".hix", StringComparison.OrdinalIgnoreCase)).ToArray();
  public static IReadOnlyList<string> MatchAssetFiles(string directory, IEnumerable<string> patterns) =>
    Match(directory, patterns, false, "*.hal").Where(path => !Path.GetFileName(path).Equals("manifest.hal", StringComparison.OrdinalIgnoreCase)).ToArray();
  internal static bool MatchesFile(string directory, string path, IEnumerable<string> patterns) => patterns.Any(raw => {
    var pattern = raw.Replace('\\', '/').TrimEnd('/');
    if (Path.IsPathRooted(pattern)) throw new ArgumentException("Hix source patterns must be relative: " + raw);
    return Glob(pattern).IsMatch(Relative(directory, path));
  });
  private static IReadOnlyList<string> MatchDirectories(string directory, IEnumerable<string> patterns) => Match(directory, patterns, true).Where(Directory.Exists).ToArray();
  private static IReadOnlyList<string> Match(string directory, IEnumerable<string> patterns, bool directories, string filePattern = "*.hix") {
    if (!Directory.Exists(directory)) return [];
    var entries = directories ? HixWorkspace.Directories(directory).Prepend(directory) : HixWorkspace.Files(directory, filePattern);
    var all = entries.Select(System.IO.Path.GetFullPath).ToArray(); var result = new HashSet<string>(PathComparer);
    foreach (var raw in patterns) { var pattern = raw.Replace('\\', '/').TrimEnd('/'); if (System.IO.Path.IsPathRooted(pattern)) throw new ArgumentException("Hix source patterns must be relative: " + raw); var regex = Glob(pattern); foreach (var entry in all) { var relative = Relative(directory, entry); if (regex.IsMatch(relative) || directories && pattern.EndsWith("/**") && regex.IsMatch(relative + "/")) result.Add(entry); } }
    return result.OrderBy(x => Relative(directory, x), StringComparer.Ordinal).ToArray();
  }
  private static Regex Glob(string pattern) => new("^" + Regex.Escape(pattern).Replace(@"\*\*", ".*").Replace(@"\*", "[^/]*") + "$", RegexOptions.CultureInvariant);
  private static string Relative(string root, string path) => System.IO.Path.GetFullPath(path).Substring(System.IO.Path.GetFullPath(root).TrimEnd(System.IO.Path.DirectorySeparatorChar).Length).TrimStart(System.IO.Path.DirectorySeparatorChar).Replace('\\', '/');
  private static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
  private static StringComparer PathComparer => IsWindows ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
}
