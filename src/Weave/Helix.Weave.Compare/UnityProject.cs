using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Helix.Weave.Compare;

/// <summary>
/// Builds a real Roslyn compilation from one of Unity's generated .csproj files — the same
/// sources, references and defines Unity compiles with.
/// </summary>
public static class UnityProject {
  public sealed record Loaded(
    string Name,
    CSharpCompilation Compilation,
    IReadOnlyList<AdditionalText> AdditionalFiles,
    IReadOnlyList<string> MissingReferences);

  public static Loaded Load(string csprojPath) {
    var document = XDocument.Load(csprojPath);
    var directory = Path.GetDirectoryName(Path.GetFullPath(csprojPath))!;
    var ns = document.Root!.Name.Namespace;

    var defines = document.Descendants(ns + "DefineConstants").FirstOrDefault()?.Value
      ?.Split(';', StringSplitOptions.RemoveEmptyEntries)
      .Select(value => value.Trim())
      .Where(value => value.Length > 0)
      .Distinct()
      .ToArray() ?? Array.Empty<string>();

    var parseOptions = new CSharpParseOptions(LanguageVersion.Preview, preprocessorSymbols: defines);

    var trees = document.Descendants(ns + "Compile")
      .Select(element => element.Attribute("Include")?.Value)
      .Where(value => !string.IsNullOrEmpty(value))
      .Select(value => Path.GetFullPath(Path.Combine(directory, value.Replace('\\', '/'))))
      .Where(File.Exists)
      .OrderBy(path => path, StringComparer.Ordinal)
      .Select(path => CSharpSyntaxTree.ParseText(File.ReadAllText(path), parseOptions, path))
      .ToList();

    var missing = new List<string>();
    var references = new List<MetadataReference>();
    foreach (var hint in document.Descendants(ns + "HintPath").Select(element => element.Value)) {
      var path = Path.GetFullPath(Path.Combine(directory, hint.Replace('\\', '/')));
      if (File.Exists(path)) references.Add(MetadataReference.CreateFromFile(path));
      else missing.Add(path);
    }

    // Sibling assemblies come through as ProjectReference, not HintPath. Without them the HELIX
    // attributes do not resolve at all and every mixin silently does nothing.
    var scriptAssemblies = Path.Combine(directory, "Library", "ScriptAssemblies");
    foreach (var project in document.Descendants(ns + "ProjectReference")
               .Select(element => element.Attribute("Include")?.Value)
               .Where(value => !string.IsNullOrEmpty(value))) {
      var assembly = Path.Combine(scriptAssemblies,
        Path.GetFileNameWithoutExtension(project!.Replace('\\', '/')) + ".dll");
      if (File.Exists(assembly)) references.Add(MetadataReference.CreateFromFile(assembly));
      else missing.Add(assembly);
    }

    // The mixin catalog Hix's generator reads.
    var additional = document.Descendants(ns + "AdditionalFiles")
      .Concat(document.Descendants(ns + "AdditionalFile"))
      .Select(element => element.Attribute("Include")?.Value)
      .Where(value => !string.IsNullOrEmpty(value))
      .Select(value => Path.GetFullPath(Path.Combine(directory, value!.Replace('\\', '/'))))
      .Where(File.Exists)
      .Select(path => (AdditionalText)new FileText(path))
      .ToList();

    var compilation = CSharpCompilation.Create(
      Path.GetFileNameWithoutExtension(csprojPath), trees, references,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

    return new Loaded(Path.GetFileNameWithoutExtension(csprojPath), compilation, additional, missing);
  }

  /// <summary>Every .additionalfile under the project, for generators that read a catalog.</summary>
  public static IReadOnlyList<AdditionalText> FindAdditionalFiles(string root) =>
    Directory.EnumerateFiles(root, "*.additionalfile", SearchOption.AllDirectories)
      .OrderBy(path => path, StringComparer.Ordinal)
      .Select(path => (AdditionalText)new FileText(path))
      .ToList();

  private sealed class FileText : AdditionalText {
    private readonly string _path;
    public FileText(string path) => _path = path;
    public override string Path => _path;
    public override SourceText GetText(System.Threading.CancellationToken cancellationToken = default) =>
      SourceText.From(File.ReadAllText(_path));
  }
}
