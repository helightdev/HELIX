using System;
using System.Collections.Generic;
using System.Linq;

namespace Hix.Compiler.Steps;

public sealed class SignatureParameterBindingStep : HixLoweringStep {
  public override HixModuleIr Lower(HixModuleIr input, HixCompilerCatalog globals) =>
    new(input.Prelude, input.Late, input.Functions.Select(Rewrite).ToArray(), input.Parameters);

  public static FunctionDeclarationIr Rewrite(FunctionDeclarationIr function) {
    var positions = new Dictionary<string, int>(StringComparer.Ordinal);
    var ambiguous = new HashSet<string>(StringComparer.Ordinal);
    foreach (var signature in function.Signatures.Where(signature => signature.Inputs != null)) {
      for (var index = 0; index < signature.Inputs.Count; index++) {
        var name = signature.Inputs[index].Name;
        var position = index;
        if (positions.TryGetValue(name, out var existing) && existing != position) ambiguous.Add(name);
        else positions[name] = position;
      }
    }
    foreach (var name in ambiguous) positions.Remove(name);
    var locals = new HashSet<string>(Descendants(function.Body).OfType<AssignmentStatementIr>()
      .Where(assignment => assignment.Storage == StorageSpace.Local)
      .Select(assignment => assignment.Name), StringComparer.Ordinal);
    foreach (var local in locals) positions.Remove(local);
    if (positions.Count == 0) return function;
    var body = new ParameterRewriter(positions).Rewrite(function.Body);
    return Copy(function, new FunctionDeclarationIr(function.Name, function.IsPure, function.IsInline,
      function.IsNoinline, function.Signatures, body, function.Metadata.Select(item => new HixIrRewriter().Rewrite(item)).ToArray()));
  }

  private static IEnumerable<HixIrNode> Descendants(HixIrNode node) {
    yield return node;
    foreach (var child in node.SemanticChildren)
      foreach (var descendant in Descendants(child)) yield return descendant;
  }

  private static T Copy<T>(HixIrNode source, T target) where T : HixIrNode {
    target.SourceRange = source.SourceRange;
    target.Tokens = source.Tokens;
    return target;
  }

  private sealed class ParameterRewriter(IReadOnlyDictionary<string, int> positions) : HixIrRewriter {
    protected override HixIrNode RewriteNode(HixIrNode node) => node is LambdaExpressionIr ? node : base.RewriteNode(node);

    protected override ExpressionIr RewriteRoot(RootExpressionIr root) =>
      root.IsSmart && positions.TryGetValue(root.Name, out var position)
        ? CopyLocation(root, new RootExpressionIr(position.ToString(), true))
        : base.RewriteRoot(root);
  }
}
