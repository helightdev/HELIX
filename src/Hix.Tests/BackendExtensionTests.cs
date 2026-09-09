using System.Collections.Generic;
using System.Linq;
using Hix;
using Hix.Compiler;
using Hix.Functions;
using Hix.Runtime;
using Xunit;

namespace HELIX.SourceGen.Tests;

// This assembly references Hix; it does not link its sources or use InternalsVisibleTo.
public sealed class BackendExtensionTests {
  [Fact]
  public void ExternalBackendCanRegisterFunctionsRootsAndCompilerTransforms() {
    var backend = new ExtensionBackend();
    var program = HixCompiler.CompileFunctions(["pure func main { return(publish(plus(7, extension))) }"], backend);
    var thread = backend.CreateThread();
    var result = new HixVM([program]).Invoke(program, thread);
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(11, Assert.IsType<NumberHixValue>(result.Value).Value);
    Assert.Equal("11", Assert.Single(result.Execution.Outputs).ReadText());
    Assert.Equal("extension", Assert.Single(result.Execution.Logs).Text.Resolve(result.Strings));
    Assert.Same(backend, program.Backend);
    Assert.Contains("publish", program.Disassemble());
    Assert.Equal(HixValueKind.Number, KindDefinitions.Get(HixValueKind.Number).ValueKind);
  }

  private sealed class ExtensionBackend : HixBackend {
    private static readonly IReadOnlyList<HixCompilerStep> steps =
      new HixCompilerStep[] {new ReplaceSevenStep()}.Concat(HixCompiler.DefaultSteps).ToArray();
    public override IReadOnlyList<HixCompilerStep> CompilerSteps => steps;

    protected override void RegisterRoots(IDictionary<string, HixBackendRoot> roots) {
      base.RegisterRoots(roots);
      roots.Add("extension", new("extension", HixValueKind.Number, HasEffects: false));
    }
    public override IHixValue ResolveRoot(HixThread thread, string name) => name == "extension"
      ? new NumberHixValue(3) : base.ResolveRoot(thread, name);

    protected override void RegisterFunctions(FunctionSignatureRegistryBuilder functions) {
      base.RegisterFunctions(functions);
      functions.Add(new SimpleFunction("publish", [new(HixValueKind.Number, [HixValueKind.Number])],
        (thread, arguments) => {
          thread.Emit(arguments[0]);
          thread.Logs.Add(new(HixString.Dynamic("extension")));
          return arguments[0];
        }));
    }
  }

  private sealed class ReplaceSevenStep : HixCompilerStep {
    public override HixCompilerSyntax Transform(HixCompilerSyntax input, HixExpressionPreparedState globals) {
      var rewriter = new ReplaceSeven();
      return input with {Functions = input.Functions.Select(rewriter.Rewrite).ToArray()};
    }
  }

  private sealed class ReplaceSeven : HixAstRewriter {
    protected override HixAst RewriteNode(HixAst node) => node is NumberExpressionAst {Value: 7}
      ? CopyLocation(node, new NumberExpressionAst(8)) : base.RewriteNode(node);
  }
}
