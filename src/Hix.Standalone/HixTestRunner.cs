using System.Runtime.InteropServices;
using Hix.Compiler;
using Hix.Runtime;

namespace Hix.Standalone;

public sealed record HixTestResult(string File, string Name, bool Success, string Message = "");

public sealed class HixTestRunResult(IReadOnlyList<HixTestResult> tests) {
  public IReadOnlyList<HixTestResult> Tests { get; } = tests;
  public int Passed => Tests.Count(test => test.Success);
  public int Failed => Tests.Count - Passed;
  public bool Success => Failed == 0;
}

public static class HixTestRunner {
  public static HixTestRunResult Run(IEnumerable<string> paths, TextWriter output = null, Action<HixTestResult> onResult = null) {
    output ??= Console.Out;
    var results = new List<HixTestResult>();
    foreach (var input in paths.DefaultIfEmpty(Directory.GetCurrentDirectory())) {
      var path = Path.GetFullPath(input);
      var manifest = Directory.Exists(path) ? Path.Combine(path, "manifest.hal") : path.EndsWith(".hal", StringComparison.OrdinalIgnoreCase) ? path : null;
      if (manifest != null && File.Exists(manifest)) results.AddRange(RunProject(HixProject.Load(manifest), output, onResult).Tests);
      else foreach (var file in Discover([path])) results.AddRange(RunFile(file, output, onResult));
    }
    output.WriteLine($"{results.Count} tests: {results.Count(result => result.Success)} passed, " +
      $"{results.Count(result => !result.Success)} failed");
    return new HixTestRunResult(results);
  }

  public static HixTestRunResult RunProject(HixProject project, TextWriter output = null, Action<HixTestResult> onResult = null) {
    output ??= Console.Out;
    var results = new List<HixTestResult>();
    var main = project.MainSources();
    foreach (var testPath in HixProject.MatchFiles(project.Manifest.Directory, project.Manifest.Sources.Test)) {
      var files = new List<HixSourceFile>(main); var known = new HashSet<string>(main.Select(file => file.Path), PathComparer);
      foreach (var file in HixFileImports.LoadFiles(testPath)) if (known.Add(file.Path)) files.Add(file);
      results.AddRange(RunFiles(files, new HashSet<string>([testPath], PathComparer), output, onResult));
    }
    return new(results);
  }

  private static IReadOnlyList<HixTestResult> RunFile(string entryFile, TextWriter output, Action<HixTestResult> onResult = null) {
    IReadOnlyList<HixSourceFile> files;
    try {
      files = HixFileImports.LoadFiles(entryFile);
    } catch (Exception exception) when (exception is ArgumentException or IOException or UnauthorizedAccessException) {
      return [Report(new(entryFile, "compile", false, exception.Message), output, onResult)];
    }
    return RunFiles(files, new HashSet<string>([entryFile], PathComparer), output, onResult);
  }

  private static IReadOnlyList<HixTestResult> RunFiles(IReadOnlyList<HixSourceFile> files, ISet<string> testPaths, TextWriter output, Action<HixTestResult> onResult = null) {
    var results = new List<HixTestResult>();
    var directory = files.Count == 0 ? Directory.GetCurrentDirectory() : Path.GetDirectoryName(files[0].Path);
    var backend = new HixTestBackend(output, directory);

    HixProgramImage program;
    CompilationUnitIr[] units;
    try {
      units = files.Select(file => AntlrSyntax.Parse(file.Source, backend)).ToArray();
      var diagnostic = units.SelectMany((unit, index) => unit.Diagnostics.Select(item =>
        files[index].Path.Replace('\\', '/') + ":" + item.Line + ": " + item.Message)).FirstOrDefault();
      if (diagnostic != null) throw new ArgumentException(diagnostic);
      var sources = files.Select(file => file.Source).ToArray();
      backend.SetCompilationSources(sources);
      program = HixCompiler.CompileFunctions(sources, backend);
    } catch (Exception exception) when (exception is ArgumentException) {
      results.Add(Report(new(files.FirstOrDefault()?.Path ?? "manifest.hal", "compile", false, exception.Message), output, onResult));
      return results;
    }

    var vm = new HixVM([program]);
    for (var fileIndex = 0; fileIndex < files.Count; fileIndex++) {
      if (!testPaths.Contains(files[fileIndex].Path)) continue;
      foreach (var function in units[fileIndex].Declarations.OfType<FunctionDeclarationIr>()) {
        if (!function.Metadata.Any(metadata => metadata.Name == "test")) continue;
        var cases = function.Metadata.Where(metadata => metadata.Name == "testcase").ToArray();
        if (cases.Length == 0) {
          results.Add(Report(Invoke(vm, program, backend, files[fileIndex].Path, function.Name, null, 0), output, onResult));
          continue;
        }
        for (var caseIndex = 0; caseIndex < cases.Length; caseIndex++)
          results.Add(Report(Invoke(vm, program, backend, files[fileIndex].Path, function.Name,
            cases[caseIndex], caseIndex + 1), output, onResult));
      }
    }
    return results;
  }

  private static HixTestResult Invoke(HixVM vm, HixProgramImage program, HixTestBackend backend,
    string file, string function, MetadataIr testCase, int caseIndex) {
    var name = function + (caseIndex == 0 ? "" : "[" + caseIndex + "]");
    try {
      var metadata = testCase?.Values.Select(Constant).ToArray() ?? [];
      var arguments = metadata.Length == 0 ? [] : metadata[0] is TupleHixValue tuple ? tuple.Values.ToArray() : [metadata[0]];
      var thread = backend.CreateThread();
      var invocation = vm.Invoke(program, thread, function, arguments);
      if (!invocation.Success) return new(file, name, false, invocation.Error.Resolve(invocation.Strings));
      if (metadata.Length == 2) {
        if (!thread.Equal(invocation.Value, metadata[1]))
          return new(file, name, false, "expected " + thread.Render(metadata[1]).Resolve(thread.Strings) +
            " but received " + invocation.Value.Render(thread).Resolve(invocation.Strings));
      }
      return new(file, name, true);
    } catch (ArgumentException exception) { return new(file, name, false, exception.Message); }
  }

  private static IHixValue Constant(ExpressionIr expression) => expression switch {
    StringExpressionIr text => HixThread.String(text.Value), NumberExpressionIr number => new NumberHixValue(number.Value),
    BooleanExpressionIr boolean => BooleanHixValue.From(boolean.Value), NullExpressionIr => NullHixValue.Instance,
    MissingExpressionIr => MissingHixValue.Instance,
    TupleExpressionIr tuple => new TupleHixValue(tuple.Values.Select(Constant).ToArray()),
    TableExpressionIr table => new HixTableValue(table.Entries.Select(entry =>
      new KeyValuePair<HixString, IHixValue>(HixString.Dynamic(entry.Key), Constant(entry.Value)))),
    _ => throw new ArgumentException("test metadata values must be constant")
  };

  private static HixTestResult Report(HixTestResult result, TextWriter output, Action<HixTestResult> onResult) {
    output.WriteLine((result.Success ? "PASS " : "FAIL ") + result.File.Replace('\\', '/') + "::" + result.Name +
      (string.IsNullOrEmpty(result.Message) ? "" : ": " + result.Message));
    onResult?.Invoke(result); return result;
  }

  private static IEnumerable<string> Discover(IEnumerable<string> paths) {
    foreach (var input in paths.DefaultIfEmpty(Directory.GetCurrentDirectory())) {
      var path = Path.GetFullPath(input);
      if (File.Exists(path)) { yield return path; continue; }
      if (!Directory.Exists(path)) throw new FileNotFoundException("test path does not exist", path);
      foreach (var file in Directory.EnumerateFiles(path, "*.hix", SearchOption.AllDirectories)
        .OrderBy(file => file, StringComparer.Ordinal)) {
        var unit = AntlrSyntax.Parse(File.ReadAllText(file), new HixTestBackend(TextWriter.Null, Path.GetDirectoryName(file)), true);
        var declaredTestBackend = unit.Metadata.Any(metadata => metadata.Name == "backend" &&
          metadata.Values.FirstOrDefault() is StringExpressionIr value &&
          string.Equals(value.Value, "Test", StringComparison.OrdinalIgnoreCase));
        var containsTests = unit.Declarations.OfType<FunctionDeclarationIr>().Any(function =>
            function.Metadata.Any(metadata => metadata.Name == "test"));
        if (declaredTestBackend && containsTests) yield return Path.GetFullPath(file);
      }
    }
  }

  private static bool PathEquals(string left, string right) => string.Equals(Path.GetFullPath(left), Path.GetFullPath(right),
    RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
  private static StringComparer PathComparer => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
}
