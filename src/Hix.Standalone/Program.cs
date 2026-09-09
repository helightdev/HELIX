using Hix.Compiler;
using Hix.Runtime;
namespace Hix.Standalone;
public static class Program {
  public static int Main(string[] args) => Run(args, Console.Out, Console.Error);
  public static int Run(string[] args, TextWriter output, TextWriter error) {
    if (args.Length == 0) { error.WriteLine("Usage: hix <file> [--entry <name>] [-- <arguments...>]"); return 2; }
    var entry = "main";
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
      var backend = new HixStandaloneBackend(output);
      var program = HixCompiler.CompileFunctions([File.ReadAllText(args[0])], backend);
      var result = new HixVM([program]).Invoke(program, backend.CreateThread(), entry, arguments.ToArray());
      if (result.Success) return 0;
      error.WriteLine(result.Error.Resolve(result.Strings)); return 1;
    } catch (Exception exception) when (exception is ArgumentException or IOException or UnauthorizedAccessException) {
      error.WriteLine(exception.Message); return 1;
    }
  }
}
