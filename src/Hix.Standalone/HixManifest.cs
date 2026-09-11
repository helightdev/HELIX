using Hix.Hal;

namespace Hix.Standalone;

public sealed record HixManifest(string Path, string Backend, IReadOnlyList<string> Imports) {
  public static HixManifest Find(string file) {
    var directory = Directory.Exists(file) ? System.IO.Path.GetFullPath(file)
      : System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(file));
    while (directory != null) {
      var manifest = System.IO.Path.Combine(directory, "manifest.hal");
      if (File.Exists(manifest)) return Load(manifest);
      directory = Directory.GetParent(directory)?.FullName;
    }
    return null;
  }

  public static HixManifest Load(string file) {
    file = System.IO.Path.GetFullPath(file);
    var document = HalDocumentSyntax.Parse(File.ReadAllText(file));
    if (document.Diagnostics.Count != 0)
      throw new ArgumentException(file.Replace('\\', '/') + ": " + document.Diagnostics[0].Message);
    if (document.Sections.Count == 0 || document.Sections[0].Type != "Manifest")
      throw new ArgumentException(file.Replace('\\', '/') + ": root section must have type Manifest");
    if (document.Sections.Count != 1)
      throw new ArgumentException(file.Replace('\\', '/') + ": a manifest must contain exactly one section");

    var fields = document.Sections[0].Fields;
    var duplicate = fields.GroupBy(field => field.Name, StringComparer.Ordinal).FirstOrDefault(group => group.Count() > 1);
    if (duplicate != null) throw new ArgumentException(file.Replace('\\', '/') + ": duplicate manifest field '" + duplicate.Key + "'");
    var unknown = fields.FirstOrDefault(field => field.Name is not ("backend" or "imports"));
    if (unknown != null) throw new ArgumentException(file.Replace('\\', '/') + ": unknown manifest field '" + unknown.Name + "'");

    var backend = fields.FirstOrDefault(field => field.Name == "backend") is { Value: var backendValue }
      ? Text(backendValue, "backend") : "Standalone";
    var imports = fields.FirstOrDefault(field => field.Name == "imports")?.Value switch {
      null => Array.Empty<string>(),
      HalListSyntax list => list.Values.Select(value => Text(value, "imports")).ToArray(),
      var value => new[] {Text(value, "imports")}
    };
    return new HixManifest(file, backend, imports);
  }

  private static string Text(HalValueSyntax value, string field) => value is HalScalarSyntax { Value: string text }
    ? text : throw new ArgumentException("Manifest." + field + " must contain string values");
}
