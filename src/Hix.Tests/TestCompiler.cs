using System.Collections.Generic;
using Hix;
using Hix.Compiler;
using Hix.Runtime;
namespace HELIX.SourceGen.Tests;
internal static class TestCompiler {
  internal static HixProgramImage Compile(string source, string name, HixBackend backend = null) => HixCompiler.Compile(source, name, backend ?? TestBackend.Instance);
  internal static HixProgramImage Compile(CompilationUnitIr unit, string name, HixBackend backend = null) => HixCompiler.Compile(unit, name, backend ?? TestBackend.Instance);
}

internal static class TestOutputText {
  internal static string ReadText(this HixOutput output) => new HixThread().Text(output.Value).Resolve(null);
}
