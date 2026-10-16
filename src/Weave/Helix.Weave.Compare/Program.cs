using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Helix.Weave.Lowering;

namespace Helix.Weave.Compare;

/// <summary>
/// Runs the old Hix generator and the new weave generator over the same real Unity compilation
/// and reports where their output differs.
/// </summary>
public static class Program {
  public static int Main(string[] args) {
    if (args.Length < 3) {
      Console.Error.WriteLine(
        "usage: compare <unity-project-dir> <mixin-project-dir> <hix-generator.dll> [--out <dir>] [csproj-name ...]");
      return 2;
    }

    var unityDirectory = Path.GetFullPath(args[0]);
    var mixinDirectory = Path.GetFullPath(args[1]);
    var hixGeneratorPath = Path.GetFullPath(args[2]);
    var rest = args.Skip(3).ToList();

    string outputDirectory = null;
    var outIndex = rest.IndexOf("--out");
    if (outIndex >= 0 && outIndex + 1 < rest.Count) {
      outputDirectory = Path.GetFullPath(rest[outIndex + 1]);
      rest.RemoveRange(outIndex, 2);
    }

    var projects = rest.Count > 0
      ? rest.Select(name => Path.Combine(unityDirectory, name.EndsWith(".csproj") ? name : name + ".csproj")).ToList()
      : Directory.EnumerateFiles(unityDirectory, "*.csproj")
        .Where(path => !path.Contains(".Player.") && !path.Contains("-testable"))
        .OrderBy(path => path, StringComparer.Ordinal)
        .ToList();

    var weave = BuildWeaveGenerator(mixinDirectory);
    if (weave is null) return 1;
    var hix = LoadGenerators(hixGeneratorPath);
    Console.WriteLine($"hix generators: {hix.Count}, projects: {projects.Count}\n");

    var catalogs = UnityProject.FindAdditionalFiles(unityDirectory);
    var totals = new Totals();

    foreach (var path in projects) {
      if (!File.Exists(path)) { Console.WriteLine($"missing: {path}"); continue; }
      CompareProject(path, weave, hix, catalogs, outputDirectory, totals);
    }

    Console.WriteLine();
    Console.WriteLine($"types: {totals.Same} identical, {totals.Different} differing, " +
                      $"{totals.OnlyHix} only Hix, {totals.OnlyWeave} only weave");
    return 0;
  }

  private static void Write(string root, string project, string type, string hix, string weave) {
    var directory = Path.Combine(root, project);
    Directory.CreateDirectory(directory);
    File.WriteAllText(Path.Combine(directory, type + ".hix.cs"), hix);
    File.WriteAllText(Path.Combine(directory, type + ".weave.cs"), weave);
  }

  private sealed class Totals {
    public int Same, Different, OnlyHix, OnlyWeave;
  }

  private static void CompareProject(string csprojPath, IIncrementalGenerator weave,
                                     IReadOnlyList<object> hixGenerators,
                                     IReadOnlyList<AdditionalText> catalogs, string outputDirectory,
                                     Totals totals) {
    var loaded = UnityProject.Load(csprojPath);
    if (loaded.Compilation.SyntaxTrees.Length == 0) return;

    var clock = System.Diagnostics.Stopwatch.StartNew();
    var weaveSources = Run(loaded.Compilation, new[] { (object)weave }, catalogs, out var weaveDiagnostics);
    var weaveMs = clock.ElapsedMilliseconds;
    clock.Restart();
    Run(loaded.Compilation, new[] { (object)weave }, catalogs, out _);
    var weaveAgainMs = clock.ElapsedMilliseconds;
    clock.Restart();
    var hixSources = Run(loaded.Compilation, hixGenerators, catalogs, out var hixDiagnostics);
    var hixMs = clock.ElapsedMilliseconds;
    if (weaveSources.Count == 0 && hixSources.Count == 0) {
      Console.WriteLine($"--- {loaded.Name}: no output from either " +
                        $"({loaded.Compilation.SyntaxTrees.Length} files, " +
                        $"{loaded.MissingReferences.Count} missing refs)");
      return;
    }

    Console.WriteLine($"=== {loaded.Name} ({loaded.Compilation.SyntaxTrees.Length} files, " +
                      $"{loaded.MissingReferences.Count} missing refs) ===");
    Console.WriteLine($"  hix: {hixSources.Count} files in {hixMs}ms, " +
                      $"weave: {weaveSources.Count} files in {weaveMs}ms ({weaveAgainMs}ms warm)");

    // Hix and weave name their outputs differently; compare by the type they are for.
    var hixByType = ByType(hixSources);
    var weaveByType = ByType(weaveSources);

    foreach (var type in hixByType.Keys.Union(weaveByType.Keys).OrderBy(key => key, StringComparer.Ordinal)) {
      var inHix = hixByType.TryGetValue(type, out var hixText);
      var inWeave = weaveByType.TryGetValue(type, out var weaveText);

      if (inHix && !inWeave) { totals.OnlyHix++; Console.WriteLine($"  - only hix:   {type}"); continue; }
      if (!inHix && inWeave) { totals.OnlyWeave++; Console.WriteLine($"  + only weave: {type}"); continue; }

      if (outputDirectory is not null) Write(outputDirectory, loaded.Name, type, hixText, weaveText);

      var differences = Semantic.Compare(hixText, weaveText);
      if (differences.Count == 0) { totals.Same++; continue; }

      totals.Different++;
      Console.WriteLine($"  ~ differs:    {type}");
      foreach (var difference in differences.Take(6)) Console.WriteLine($"      {difference}");
      if (differences.Count > 6) Console.WriteLine($"      ... and {differences.Count - 6} more");

    }

    foreach (var diagnostic in weaveDiagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Take(10))
      Console.WriteLine($"  weave error: {diagnostic}");
    foreach (var diagnostic in hixDiagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Take(5))
      Console.WriteLine($"  hix error:   {diagnostic}");
  }

  /// <summary>
  /// Keys generated sources by the type they declare, since the two generators name their
  /// output files differently.
  /// </summary>
  private static Dictionary<string, string> ByType(IReadOnlyDictionary<string, string> sources) {
    var result = new Dictionary<string, string>(StringComparer.Ordinal);
    foreach (var pair in sources) {
      var key = DeclaredType(pair.Value) ?? Path.GetFileName(pair.Key);
      result[key] = result.TryGetValue(key, out var existing) ? existing + "\n" + pair.Value : pair.Value;
    }
    return result;
  }

  private static string DeclaredType(string source) {
    var root = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview)).GetRoot();
    var declaration = root.DescendantNodes()
      .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.BaseTypeDeclarationSyntax>()
      .FirstOrDefault();
    if (declaration is null) return null;

    var name = declaration.Identifier.Text;
    for (var parent = declaration.Parent; parent is not null; parent = parent.Parent)
      switch (parent) {
        case Microsoft.CodeAnalysis.CSharp.Syntax.NamespaceDeclarationSyntax ns:
          name = ns.Name + "." + name; break;
        case Microsoft.CodeAnalysis.CSharp.Syntax.FileScopedNamespaceDeclarationSyntax file:
          name = file.Name + "." + name; break;
        case Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax type:
          name = type.Identifier.Text + "." + name; break;
      }
    return name;
  }

  /// <summary>Whitespace and comments differ by construction; compare the code.</summary>
  private static string Normalize(string source) {
    if (source is null) return "";
    var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview));
    return tree.GetRoot().NormalizeWhitespace().ToFullString();
  }

  private static IReadOnlyDictionary<string, string> Run(Compilation compilation,
                                                         IReadOnlyList<object> generators,
                                                         IReadOnlyList<AdditionalText> additional,
                                                         out IReadOnlyList<Diagnostic> diagnostics) {
    diagnostics = Array.Empty<Diagnostic>();
    if (generators.Count == 0) return new Dictionary<string, string>();

    var wrapped = generators.Select(generator => generator switch {
      IIncrementalGenerator incremental => incremental.AsSourceGenerator(),
      ISourceGenerator source => source,
      _ => null,
    }).Where(generator => generator is not null).ToArray();

    var driver = CSharpGeneratorDriver.Create(
      wrapped, additional, new CSharpParseOptions(LanguageVersion.Preview));

    var result = driver.RunGenerators(compilation).GetRunResult();
    diagnostics = result.Diagnostics;
    return result.Results
      .SelectMany(item => item.GeneratedSources)
      .GroupBy(source => source.HintName, StringComparer.Ordinal)
      .ToDictionary(group => group.Key, group => group.First().SourceText.ToString(), StringComparer.Ordinal);
  }

  private static IReadOnlyList<object> LoadGenerators(string assemblyPath) {
    var assembly = Assembly.LoadFrom(assemblyPath);
    return assembly.GetTypes()
      .Where(type => !type.IsAbstract && type.GetCustomAttributes()
        .Any(attribute => attribute.GetType().Name == "GeneratorAttribute"))
      .Select(Activator.CreateInstance)
      .ToList();
  }

  /// <summary>Lowers the mixin project and compiles the generator it produces, in memory.</summary>
  private static IIncrementalGenerator BuildWeaveGenerator(string mixinDirectory) {
    var references = DefaultReferences();
    var lowered = Lowerer.LowerDirectory(mixinDirectory, references);
    if (!lowered.Success) {
      foreach (var error in lowered.Errors) Console.Error.WriteLine(error);
      return null;
    }

    var trees = lowered.Files
      .OrderBy(file => file.Key, StringComparer.Ordinal)
      .Select(file => CSharpSyntaxTree.ParseText(file.Value,
        new CSharpParseOptions(LanguageVersion.Preview), file.Key));

    var compilation = CSharpCompilation.Create("Helix.Weave.Generated", trees, references,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    using var stream = new MemoryStream();
    var emitted = compilation.Emit(stream);
    if (!emitted.Success) {
      foreach (var diagnostic in emitted.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Take(20))
        Console.Error.WriteLine(diagnostic);
      return null;
    }

    var type = Assembly.Load(stream.ToArray()).GetType("Helix.Weave.Generated.WeaveGenerator");
    return (IIncrementalGenerator)Activator.CreateInstance(type!);
  }

  private static IReadOnlyList<MetadataReference> DefaultReferences() {
    var runtime = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
    return new[] {
        typeof(object).Assembly.Location,
        Path.Combine(runtime, "System.Runtime.dll"),
        Path.Combine(runtime, "System.Collections.dll"),
        Path.Combine(runtime, "System.Linq.dll"),
        Path.Combine(runtime, "netstandard.dll"),
        typeof(System.Collections.Immutable.ImmutableArray).Assembly.Location,
        typeof(SyntaxTree).Assembly.Location,
        typeof(CSharpSyntaxTree).Assembly.Location,
        typeof(Mixin).Assembly.Location,
      }.Where(File.Exists).Distinct()
      .Select(path => (MetadataReference)MetadataReference.CreateFromFile(path))
      .ToList();
  }
}
