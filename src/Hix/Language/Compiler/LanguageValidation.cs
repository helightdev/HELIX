using System;
using System.Collections.Generic;
using System.Linq;

namespace Hix.Compiler;

internal static class LanguageValidation {
  internal static void Validate(IReadOnlyList<HixAst> declarations, List<HixParseDiagnostic> diagnostics, HixBackend backend = null) {
    backend ??= HixCoreBackend.Instance;
    void Error(HixAst node, string message) => diagnostics.Add(new HixParseDiagnostic(node.Line, message));
    bool KnownKind(string name) => name == "any" || name != null && KindHixValue.TryGet(name, out _);

    void Signature(FunctionDeclarationAst function, FunctionSignature signature) {
      void Fields(IReadOnlyList<SignatureField> fields, string kind, bool output) {
        if (fields == null) {
          if (!KnownKind(kind)) Error(function, "unknown signature kind '" + kind + "'");
          return;
        }
        var names = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < fields.Count; index++) {
          var field = fields[index];
          if (!names.Add(field.Name)) Error(function, "duplicate signature field '" + field.Name + "'");
          if (!KnownKind(field.Kind)) Error(function, "unknown signature kind '" + field.Kind + "'");
          if (field.Variadic && (output || index != fields.Count - 1))
            Error(function, "only the last input signature field may be variadic");
        }
      }
      Fields(signature.Inputs, signature.InputKind, false);
      Fields(signature.Outputs, signature.OutputKind, true);
    }

    void Scope(IReadOnlyList<HixAst> nodes, IReadOnlyList<FunctionDeclarationAst> imported) {
      var functions = nodes.OfType<FunctionDeclarationAst>().ToArray();
      var localNames = new HashSet<string>(functions.Select(function => function.Name), StringComparer.Ordinal);
      var visible = functions.Concat(imported.Where(function => !localNames.Contains(function.Name))).ToArray();
      var signatures = new HashSet<string>(StringComparer.Ordinal);
      void Pure(BlockStatementAst body) {
        foreach (var node in Descendants(body)) {
          if (node is AssignmentStatementAst {Storage: not StorageSpace.Local} or AssignmentStatementAst {IsCarried: true})
            Error(node, "pure functions cannot mutate shared storage");
          if (node is RootExpressionAst {IsSmart: false} root && (root.Name is "var" or "tar" || backend.Roots.TryGetValue(root.Name, out var hostRoot) && hostRoot.HasEffects))
            Error(node, "pure functions cannot read host or shared storage");
          if (node is CallExpressionAst call && !visible.Any(candidate => candidate.Name == call.Name) &&
              backend.Functions.Resolve(call.Name, call.Arguments.Count).Any(definition => definition.HasEffects))
            Error(node, "pure functions cannot perform '" + call.Name + "'");
          if (node is CallExpressionAst invocation && visible.Any(candidate => candidate.Name == invocation.Name && !candidate.IsPure))
            Error(node, "pure functions cannot invoke impure functions");
        }
      }
      foreach (var function in functions) {
        if (function.IsInline && function.IsNoinline) Error(function, "inline and noinline cannot be combined");
        if (function.Signatures.Count == 0 && !signatures.Add(function.Name + "(*)"))
          Error(function, "duplicate function '" + function.Name + "'");
        foreach (var signature in function.Signatures) {
          Signature(function, signature);
          var key = function.Name + "(" + (signature.Inputs == null ? signature.InputKind :
            string.Join(",", signature.Inputs.Select(field => (field.Variadic ? "..." : "") + field.Kind))) + ")";
          if (!signatures.Add(key)) Error(function, "duplicate function signature '" + key + "'");
        }
        if (function.IsPure) Pure(function.Body);
      }
      foreach (var lambda in nodes.SelectMany(Descendants).OfType<LambdaExpressionAst>()) Pure(lambda.Body);
      foreach (var function in nodes.OfType<FunctionDeclarationAst>())
        foreach (var assignment in Descendants(function.Body).OfType<AssignmentStatementAst>().Where(item => item.IsCarried))
          Error(assignment, "carry local is only valid in top-level prelude expressions");
      foreach (var lambda in nodes.SelectMany(Descendants).OfType<LambdaExpressionAst>())
        foreach (var assignment in Descendants(lambda.Body).OfType<AssignmentStatementAst>().Where(item => item.IsCarried))
          Error(assignment, "carry local is only valid in top-level prelude expressions");
      foreach (var expression in nodes.OfType<ExpressionDeclarationAst>().Where(item => !item.IsPrelude))
        foreach (var assignment in Descendants(expression.Body).OfType<AssignmentStatementAst>().Where(item => item.IsCarried))
          Error(assignment, "carry local is only valid in prelude expressions");
      foreach (var boundary in nodes.SelectMany(node => node switch {
        FunctionDeclarationAst function => new[] {function.Body},
        ExpressionDeclarationAst expression => new[] {expression.Body},
        _ => Array.Empty<BlockStatementAst>()
      })) {
        var labels = new HashSet<string>(StringComparer.Ordinal);
        foreach (var node in Descendants(boundary)) {
          var label = node switch {
            BlockStatementAst block => block.Label,
            ControlFlowStatementAst {Operation: ControlFlowKind.Label} marker => marker.Label, _ => null
          };
          if (label != null && !labels.Add(label)) Error(node, "duplicate label '" + label + "'");
          if (node is TableExpressionAst table && table.Entries.Select(entry => entry.Key).Distinct(StringComparer.Ordinal).Count() != table.Entries.Count)
            Error(node, "duplicate table literal key");
        }
      }
      foreach (var mixin in nodes.OfType<MixinDeclarationAst>()) Scope(mixin.Declarations, visible);
    }

    foreach (var group in declarations.OfType<MixinDeclarationAst>().GroupBy(mixin => mixin.Name, StringComparer.Ordinal))
      if (group.Count() > 1) Error(group.First(), "duplicate mixin '" + group.Key + "'");
    Scope(declarations, []);
  }

  private static IEnumerable<HixAst> Descendants(HixAst node) {
    yield return node;
    foreach (var child in node.SemanticChildren)
      foreach (var descendant in Descendants(child)) yield return descendant;
  }
}
