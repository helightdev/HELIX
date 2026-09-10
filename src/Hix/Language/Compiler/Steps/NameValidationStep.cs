using System.Collections.Generic;
using System.Linq;

namespace Hix.Compiler.Steps;

/// <summary>Validate names without rebuilding the IR.</summary>
public sealed class NameValidationStep : HixCompilerStep {
  public override void Run(HixCompilation compilation) => new Validator(compilation).Visit(compilation.Module);

  private sealed class Validator : HixIrVisitor {
    private readonly HixCompilation compilation;
    private readonly HashSet<string> functions;
    public Validator(HixCompilation compilation) {
      this.compilation = compilation;
      functions = new(compilation.Module.Functions.Concat(compilation.Catalog.Functions).Select(function => function.Name));
    }
    protected override void VisitRoot(RootExpressionIr root) {
      if (!root.IsSmart && root.Name is not ("local" or "var" or "tar" or "args" or "param") &&
          !KindHixValue.TryGet(root.Name, out _) && !functions.Contains(root.Name) &&
          !compilation.Catalog.Patterns.ContainsKey(root.Name) && !compilation.Backend.Roots.ContainsKey(root.Name))
        compilation.Diagnostics.Add(new(root.Line, "unknown root '" + root.Name + "'"));
    }
    protected override void VisitCall(CallExpressionIr call) {
      if (call.Name is not ("var" or "tar" or "local") &&
          !compilation.Catalog.Patterns.ContainsKey(call.Name) && !functions.Contains(call.Name) &&
          compilation.Backend.Functions.Resolve(call.Name, call.Arguments.Count).Count == 0)
        compilation.Diagnostics.Add(new(call.Line, "unknown function '" + call.Name + "'"));
      base.VisitCall(call);
    }
  }
}
