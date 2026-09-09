using System.Collections.Generic;
using Hix;
using Hix.Compiler;
using Hix.Runtime;
namespace HELIX.SourceGen.Tests;
internal static class TestCompiler {
  internal static HixProgramImage Compile(string source, string name, HixBackend backend = null) => HixCompiler.Compile(source, name, backend ?? TestBackend.Instance);
  internal static HixProgramImage Compile(CompilationUnitAst unit, string name, HixBackend backend = null) => HixCompiler.Compile(unit, name, backend ?? TestBackend.Instance);
}
