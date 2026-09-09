using System.Collections.Generic;
using Hix;
using Hix.Compiler;
using Hix.Runtime;
namespace HELIX.SourceGen.Tests;
internal static class TestCompiler {
  internal static HixExpressionExecutionProgram Compile(string source, string name, HixBackend backend = null) => HixCompiler.Compile(source, name, backend ?? TestBackend.Instance);
  internal static HixExpressionExecutionProgram Compile(CompilationUnitAst unit, string name, HixBackend backend = null) => HixCompiler.Compile(unit, name, backend ?? TestBackend.Instance);
}
