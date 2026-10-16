using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Helix.Weave.Lowering;

public static class Program {
  public static int Main(string[] args) {
    if (args.Length < 2) {
      Console.Error.WriteLine("usage: weave-lower <mixin-project-dir> <output-dir> [reference.dll ...]");
      return 2;
    }

    var references = args.Skip(2)
      .Concat(DefaultReferences())
      .Where(File.Exists)
      .Select(path => (MetadataReference)MetadataReference.CreateFromFile(path))
      .ToList();

    var result = Lowerer.LowerDirectory(args[0], references, lineBase: args[1]);
    foreach (var error in result.Errors) Console.Error.WriteLine(error);
    if (!result.Success) return 1;

    Lowerer.Write(result, args[1]);
    Console.WriteLine($"lowered {result.Mixins.Count} mixin(s) into {result.Files.Count} file(s)");
    return 0;
  }

  private static string[] DefaultReferences() {
    var runtime = Path.GetDirectoryName(typeof(object).Assembly.Location);
    return new[] {
      typeof(object).Assembly.Location,
      Path.Combine(runtime!, "System.Runtime.dll"),
      Path.Combine(runtime, "System.Collections.dll"),
      Path.Combine(runtime, "System.Linq.dll"),
      Path.Combine(runtime, "netstandard.dll"),
      typeof(System.Collections.Immutable.ImmutableArray).Assembly.Location,
      typeof(Microsoft.CodeAnalysis.SyntaxTree).Assembly.Location,
      typeof(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree).Assembly.Location,
      typeof(Helix.Weave.Mixin).Assembly.Location,
    };
  }
}
