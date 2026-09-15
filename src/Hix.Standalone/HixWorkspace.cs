namespace Hix.Standalone;

/// The nearest manifest supplies types; the uppermost manifest that owns it supplies storage.
public sealed class HixWorkspace {
  public string Root { get; }
  public string Directory { get; }
  public string CacheDirectory => Path.Combine(Root, ".hix");
  public HixManifest Manifest { get; }
  public HixProject Project { get; }
  private HixWorkspace(string root, string directory, HixManifest manifest, HixProject project) {
    Root = root; Directory = directory; Manifest = manifest; Project = project;
  }
  public static HixWorkspace Find(string path) {
    path = Path.GetFullPath(path);
    var nearest = HixManifest.Find(path);
    if (nearest == null) {
      var directory = System.IO.Directory.Exists(path) ? path : Path.GetDirectoryName(path);
      return new(directory, directory, null, null);
    }
    var project = HixProject.Load(nearest.Path);
    for (var parent = System.IO.Directory.GetParent(nearest.Directory); parent != null; parent = parent.Parent) {
      if (!File.Exists(Path.Combine(parent.FullName, "manifest.hal"))) continue;
      var candidate = HixProject.Load(parent.FullName);
      if (Associated(candidate).Any(m => SamePath(m.Path, nearest.Path))) project = candidate;
    }
    return new(project.Manifest.Directory, nearest.Directory, nearest, project);
  }
  private static IEnumerable<HixManifest> Associated(HixProject project) {
    var queue = new Queue<HixManifest>(project.Modules.Values.Select(m => m.Manifest).Prepend(project.Manifest));
    var visited = new HashSet<string>(PathComparer);
    while (queue.Count != 0) {
      var manifest = queue.Dequeue(); if (!visited.Add(manifest.Path)) continue;
      yield return manifest;
      foreach (var path in manifest.Dependencies.Values.OfType<PathDependency>()) {
        var dependency = HixProject.Load(Path.Combine(manifest.Directory, path.Uri));
        queue.Enqueue(dependency.Manifest);
        foreach (var module in dependency.Modules.Values) queue.Enqueue(module.Manifest);
      }
    }
  }
  public IReadOnlyList<string> AssetPaths() {
    if (Project == null) return Files(Root, "*.hal", directory => !File.Exists(Path.Combine(directory, "manifest.hal"))).Where(path => !Path.GetFileName(path).Equals("manifest.hal", StringComparison.OrdinalIgnoreCase)).ToArray();
    var owners = new Dictionary<string, HixManifest>(PathComparer);
    HixManifest Owner(string path) {
      var directory = Path.GetDirectoryName(path);
      if (!owners.TryGetValue(directory, out var owner)) owners[directory] = owner = HixManifest.Find(path);
      return owner;
    }
    return Associated(Project).Where(manifest => Contains(Root, manifest.Directory))
      .SelectMany(manifest => HixProject.MatchAssetFiles(manifest.Directory, manifest.Sources.Assets)
        .Where(path => SamePath(Owner(path).Path, manifest.Path)))
      .Distinct(PathComparer).OrderBy(path => path, PathComparer).ToArray();
  }
  public bool IncludesAsset(string path) {
    if (!Contains(Root, path) || Contains(CacheDirectory, path) || !path.EndsWith(".hal", StringComparison.OrdinalIgnoreCase)
        || Path.GetFileName(path).Equals("manifest.hal", StringComparison.OrdinalIgnoreCase)) return false;
    var owner = HixManifest.Find(path);
    return owner == null || (SamePath(Find(path).Root, Root) && HixProject.MatchesFile(owner.Directory, path, owner.Sources.Assets));
  }
  public IReadOnlyList<HixSourceFile> Sources() => Project != null ? Project.SourcesFor(Manifest) :
    System.IO.Directory.GetFiles(Directory, "*.hix").OrderBy(p => p, StringComparer.Ordinal).Select(p => new HixSourceFile(p, File.ReadAllText(p))).ToArray();
  internal static StringComparer PathComparer => System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows) ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
  internal static bool SamePath(string left, string right) => PathComparer.Equals(Path.GetFullPath(left), Path.GetFullPath(right));
  internal static bool Contains(string root, string path) => SamePath(root, path) || Path.GetFullPath(path).StartsWith(Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar,
    PathComparer == StringComparer.OrdinalIgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
  internal static IEnumerable<string> Directories(string root, Func<string,bool> include = null) {
    foreach (var directory in System.IO.Directory.EnumerateDirectories(root).OrderBy(p => p, StringComparer.Ordinal)) {
      if (Path.GetFileName(directory) is ".hix" or ".git" or "bin" or "obj" or "node_modules" || (File.GetAttributes(directory) & FileAttributes.ReparsePoint) != 0) continue;
      if (include != null && !include(directory)) continue;
      yield return directory;
      foreach (var child in Directories(directory, include)) yield return child;
    }
  }
  internal static IEnumerable<string> Files(string root, string pattern, Func<string,bool> includeDirectory = null) => Directories(root, includeDirectory).Prepend(root)
    .SelectMany(directory => System.IO.Directory.EnumerateFiles(directory, pattern)).Where(path => (File.GetAttributes(path) & FileAttributes.ReparsePoint) == 0);
}
