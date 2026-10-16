using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using HelixSourceGenerator.Generators;

var root = Path.GetFullPath(args.Length == 0 ? "." : args[0]);
var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
  .Select(path => (MetadataReference)MetadataReference.CreateFromFile(path)).ToArray();
var parse = new CSharpParseOptions(LanguageVersion.Latest);
var runtime = CSharpCompilation.Create("Runtime", new[] {CSharpSyntaxTree.ParseText(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Runtime.txt")), parse)}, references,
  new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
var text = new FileText(Path.Combine(root, "src/HELIX/Assets/Mixins/Core.HelixSourceGenerator.additionalfile"));
var source = string.Join("\n", Enumerable.Range(0, 71).Select(i =>
  $"[HELIX.Mixable, HELIX.Structure] public partial struct Demo{i} : System.IEquatable<Demo{i}> {{ " +
  string.Join(" ", Enumerable.Range(0, 4).Select(j => $"[HELIX.Prop(0)] public int Value{j};")) + " }"));
for (var run = 0; run < 6; run++) {
  var compilation = CSharpCompilation.Create("Benchmark" + run, new[] {CSharpSyntaxTree.ParseText(source, parse)},
    references.Append(runtime.ToMetadataReference()), new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
  GeneratorDriver driver = CSharpGeneratorDriver.Create(new[] {new MixinGenerator().AsSourceGenerator()}, new[] {text}, parse);
  var before = GC.GetAllocatedBytesForCurrentThread(); var timer = Stopwatch.StartNew();
  driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);
  timer.Stop(); var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
  var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
  if (errors.Length != 0) throw new Exception(string.Join("\n", errors.Select(e => e.ToString())));
  var generated = driver.GetRunResult().Results.SelectMany(r => r.GeneratedSources).ToArray();
  if (generated.Length != 71 || generated.Any(g => !g.SourceText.ToString().Contains("hashCode.Add(this.Value0)")))
    throw new Exception("Missing generated structure members");
  var compilationErrors = output.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
  if (compilationErrors.Length != 0) throw new Exception(string.Join("\n", compilationErrors.Select(e => e.ToString())));
  Console.WriteLine($"run={run} elapsed_ms={timer.Elapsed.TotalMilliseconds:F3} allocated_bytes={allocated} generated={generated.Length}");
}
sealed class FileText(string path) : AdditionalText {
  private readonly SourceText text = SourceText.From(File.ReadAllText(path));
  public override string Path => path;
  public override SourceText GetText(CancellationToken cancellationToken = default) => text;
}
