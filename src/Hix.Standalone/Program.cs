using Hix.Compiler;
using Hix.Runtime;
namespace Hix.Standalone;
public static class Program {
  public static int Main(string[] args) => Run(args, Console.Out, Console.Error);
  public static int Run(string[] args, TextWriter output, TextWriter error) {
    if (args.Length == 0) { Usage(error); return 2; }
    if (args[0] == "test") {
      try {
        var ideEvents = args.Contains("--ide-events");
        var paths = args.Skip(1).Where(argument => argument != "--ide-events");
        return HixTestRunner.Run(paths, output, ideEvents ? result => WriteTestEvent(output, result) : null).Success ? 0 : 1;
      }
      catch (Exception exception) when (exception is ArgumentException or IOException or UnauthorizedAccessException) {
        error.WriteLine(exception.Message); return 1;
      }
    }
    var entry = (string)null;
    var arguments = new List<IHixValue>();
    for (var i = 1; i < args.Length; i++) {
      if (args[i] == "--") {
        for (i++; i < args.Length; i++) arguments.Add(new LiteralHixValue(HixString.Dynamic(args[i])));
        break;
      }
      if (args[i] == "--entry" && i + 1 < args.Length) { entry = args[++i]; continue; }
      error.WriteLine("Unknown or incomplete option: " + args[i]); return 2;
    }
    try {
      var input = Path.GetFullPath(args[0]);
      var projectInput = Directory.Exists(input) || input.EndsWith(".hal", StringComparison.OrdinalIgnoreCase);
      var project = projectInput ? HixProject.Load(input) : null;
      var entryFile = project?.Manifest.Path ?? input;
      entry ??= project?.Manifest.ApplicationMain ?? "main";
      var backend = new HixStandaloneBackend(output, project?.Manifest.Directory ?? Path.GetDirectoryName(entryFile));
      var sources = project == null ? HixFileImports.Load(entryFile) : project.MainSources().Select(file => file.Source).ToArray();
      backend.SetCompilationSources(sources);
      var program = HixCompiler.CompileFunctions(sources, backend);
      var result = new HixVM([program]).Invoke(program, backend.CreateThread(), entry, arguments.ToArray());
      if (result.Success) return 0;
      error.WriteLine(result.Error.Resolve(result.Strings)); return 1;
    } catch (Exception exception) when (exception is ArgumentException or IOException or UnauthorizedAccessException) {
      error.WriteLine(exception.Message); return 1;
    }
  }
  private static void Usage(TextWriter error) {
    error.WriteLine("Usage: hix <manifest-or-project-directory> [--entry <name>] [-- <arguments...>]");
    error.WriteLine("       hix test [<manifest-or-project-directory>]");
  }
  private static void WriteTestEvent(TextWriter output, HixTestResult result) {
    string Escape(string value) => (value ?? "").Replace("|", "||").Replace("'", "|'").Replace("\n", "|n").Replace("\r", "|r").Replace("[", "|[").Replace("]", "|]");
    var name = Escape(result.Name); var location = "file://" + Escape(Path.GetFullPath(result.File).Replace('\\', '/'));
    output.WriteLine($"##teamcity[testStarted name='{name}' locationHint='{location}']");
    if (!result.Success) output.WriteLine($"##teamcity[testFailed name='{name}' message='{Escape(result.Message)}']");
    output.WriteLine($"##teamcity[testFinished name='{name}']");
  }
}
