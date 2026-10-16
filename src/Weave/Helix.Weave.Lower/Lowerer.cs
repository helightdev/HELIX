using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Helix.Weave.Lowering;

public sealed class LoweringResult {
  public Dictionary<string, string> Files { get; } = new(StringComparer.Ordinal);
  public List<LoweringError> Errors { get; } = new();
  public List<MixinDeclaration> Mixins { get; } = new();
  public bool Success => Errors.Count == 0;
}

/// <summary>
/// Reads an authoring project, relocates its mixins and writes the generator's sources.
/// <c>*.lib.cs</c> is copied straight through.
/// </summary>
public static class Lowerer {
  /// <param name="lineBase">
  /// Directory the generated files will be written to; #line paths are made relative to it so
  /// the output carries no machine-specific absolute paths.
  /// </param>
  public static LoweringResult Lower(IEnumerable<(string Path, string Text)> sources,
                                     IEnumerable<MetadataReference> references,
                                     string lineBase = null) {
    var result = new LoweringResult();
    // Preview: raw string literals are how mixins write emitted code, and the Roslyn version
    // pinned here still gates them behind preview.
    var trees = sources
      .Select(source => CSharpSyntaxTree.ParseText(source.Text,
        new CSharpParseOptions(LanguageVersion.Preview), path: source.Path))
      .ToList();

    var compilation = CSharpCompilation.Create("Helix.Weave.Authoring", trees, references,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

    foreach (var diagnostic in compilation.GetDiagnostics())
      if (diagnostic.Severity == DiagnosticSeverity.Error)
        result.Errors.Add(new LoweringError("authoring project does not compile: " + diagnostic.GetMessage(),
          diagnostic.Location));

    if (!result.Success) return result;

    var declarations = MixinDiscovery.Discover(compilation, result.Errors);
    result.Mixins.AddRange(declarations);
    if (!result.Success) return result;

    foreach (var declaration in declarations)
      result.Files[declaration.FileName] = LoweringEmitter.EmitMixin(declaration, lineBase);

    // Whatever else a mixin file declared travels with it, verbatim.
    var mixinTypeNames = new HashSet<string>(
      declarations.Select(d => (d.Namespace is null ? "" : d.Namespace + ".") + d.Name), StringComparer.Ordinal);

    foreach (var tree in compilation.SyntaxTrees) {
      if (tree.FilePath.EndsWith(".lib.cs", StringComparison.OrdinalIgnoreCase)) continue;
      var companions = LoweringEmitter.EmitCompanions(tree, mixinTypeNames);
      if (companions is not null)
        result.Files[Path.GetFileNameWithoutExtension(tree.FilePath) + ".rest.g.cs"] = companions;
    }

    result.Files["MixinRegistry.g.cs"] = LoweringEmitter.EmitRegistry(declarations);

    // Library sources bypass lowering entirely; they are already in runtime form.
    foreach (var tree in compilation.SyntaxTrees) {
      if (!tree.FilePath.EndsWith(".lib.cs", StringComparison.OrdinalIgnoreCase)) continue;
      result.Files[Path.GetFileName(tree.FilePath)] = tree.ToString();
    }

    return result;
  }

  public static LoweringResult LowerDirectory(string directory, IEnumerable<MetadataReference> references,
                                              string lineBase = null) {
    var root = Path.GetFullPath(directory);
    var sources = Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
      .Where(path => !IsBuildOutput(root, path))
      .OrderBy(path => path, StringComparer.Ordinal)
      .Select(path => (path, File.ReadAllText(path)));
    return Lower(sources, references, lineBase);
  }

  /// <summary>Excludes build output, judged relative to the project root rather than by path text.</summary>
  private static bool IsBuildOutput(string root, string path) {
    var relative = Path.GetFullPath(path).Substring(root.Length).TrimStart(Path.DirectorySeparatorChar);
    foreach (var segment in relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
      if (segment is "obj" or "bin") return true;
    return false;
  }

  public static void Write(LoweringResult result, string outputDirectory) {
    Directory.CreateDirectory(outputDirectory);
    foreach (var file in result.Files)
      File.WriteAllText(Path.Combine(outputDirectory, file.Key), file.Value);
  }
}
