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
  public static HixTestRunResult Run(IEnumerable<string> paths, TextWriter output = null) {
    output ??= Console.Out;
    var files = Discover(paths).ToArray();
    var results = new List<HixTestResult>();
    foreach (var file in files) results.AddRange(RunFile(file, output));
    output.WriteLine($"{results.Count} tests: {results.Count(result => result.Success)} passed, " +
      $"{results.Count(result => !result.Success)} failed");
    return new HixTestRunResult(results);
  }

  private static IReadOnlyList<HixTestResult> RunFile(string entryFile, TextWriter output) {
    var results = new List<HixTestResult>();
    var directory = Path.GetDirectoryName(entryFile) ?? Directory.GetCurrentDirectory();
    var backend = new HixTestBackend(output, directory);
    IReadOnlyList<HixSourceFile> files;
    CompilationUnitIr entry;
    try {
      files = HixFileImports.LoadFiles(entryFile);
      entry = AntlrSyntax.Parse(files.Single(file => PathEquals(file.Path, entryFile)).Source, backend);
    } catch (Exception exception) when (exception is ArgumentException or IOException or UnauthorizedAccessException) {
      results.Add(Report(new(entryFile, "compile", false, exception.Message), output));
      return results;
    }

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
      results.Add(Report(new(entryFile, "compile", false, exception.Message), output));
      return results;
    }

    var vm = new HixVM([program]);
    for (var fileIndex = 0; fileIndex < files.Count; fileIndex++) {
      foreach (var function in units[fileIndex].Declarations.OfType<FunctionDeclarationIr>()) {
        if (!function.Metadata.Any(metadata => metadata.Name == "test")) continue;
        var cases = function.Metadata.Where(metadata => metadata.Name == "testcase").ToArray();
        if (cases.Length == 0) {
          results.Add(Report(Invoke(vm, program, backend, files[fileIndex].Path, function.Name, null, 0), output));
          continue;
        }
        for (var caseIndex = 0; caseIndex < cases.Length; caseIndex++)
          results.Add(Report(Invoke(vm, program, backend, files[fileIndex].Path, function.Name,
            cases[caseIndex], caseIndex + 1), output));
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

  private static HixTestResult Report(HixTestResult result, TextWriter output) {
    output.WriteLine((result.Success ? "PASS " : "FAIL ") + result.File.Replace('\\', '/') + "::" + result.Name +
      (string.IsNullOrEmpty(result.Message) ? "" : ": " + result.Message));
    return result;
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
        var testBackend = declaredTestBackend || string.Equals(HixManifest.Find(file)?.Backend, "Test",
          StringComparison.OrdinalIgnoreCase);
        var containsTests = unit.Declarations.OfType<FunctionDeclarationIr>().Any(function =>
            function.Metadata.Any(metadata => metadata.Name == "test"));
        if (testBackend && containsTests) yield return Path.GetFullPath(file);
      }
    }
  }

  private static bool PathEquals(string left, string right) => string.Equals(Path.GetFullPath(left), Path.GetFullPath(right),
    RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
}
