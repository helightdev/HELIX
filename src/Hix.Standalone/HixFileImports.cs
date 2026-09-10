using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Hix.Compiler;

namespace Hix.Standalone;

public static class HixFileImports {
  public static IReadOnlyList<string> Load(string entryFile) {
    var sources = new List<string>();
    var visited = new HashSet<string>(PathComparer);
    Visit(Path.GetFullPath(entryFile), sources, visited);
    return sources;
  }

  private static void Visit(string file, ICollection<string> sources, ISet<string> visited) {
    file = Path.GetFullPath(file);
    if (!visited.Add(file)) return;
    var source = File.ReadAllText(file);
    var unit = AntlrSyntax.Parse(source, new HixStandaloneBackend(TextWriter.Null, Path.GetDirectoryName(file)), true);
    foreach (var metadata in unit.Metadata.Where(item => item.Name == "import")) {
      if (metadata.Values.Count != 1 || metadata.Values[0] is not StringExpressionIr value)
        throw new ArgumentException("%import requires one literal path glob");
      foreach (var imported in Match(Path.GetDirectoryName(file), value.Value)) Visit(imported, sources, visited);
    }
    sources.Add(source);
  }

  private static IEnumerable<string> Match(string directory, string pattern) {
    pattern = pattern.Replace('\\', '/');
    if (Path.IsPathRooted(pattern)) throw new ArgumentException("%import paths must be relative");
    var regex = new Regex("^" + Regex.Escape(pattern)
      .Replace(@"\*\*", ".*").Replace(@"\*", "[^/]*") + "$", RegexOptions.CultureInvariant);
    var matches = Directory.EnumerateFiles(directory, "*.hix", SearchOption.AllDirectories)
      .Where(path => regex.IsMatch(Relative(directory, path)))
      .OrderBy(path => Relative(directory, path), StringComparer.Ordinal).ToArray();
    if (matches.Length == 0) throw new FileNotFoundException("%import matched no Hix files: " + pattern);
    return matches;
  }

  private static string Relative(string directory, string path) {
    var root = Path.GetFullPath(directory).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
    var full = Path.GetFullPath(path);
    return full.StartsWith(root, PathComparison) ? full.Substring(root.Length).Replace('\\', '/') : full.Replace('\\', '/');
  }

  private static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
  private static StringComparer PathComparer => IsWindows ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
  private static StringComparison PathComparison => IsWindows ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
}
