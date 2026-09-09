using System;
using System.Collections.Generic;
using System.Linq;

namespace Hix.Compiler;

public static class LanguageValidation {
  public static void Validate(IReadOnlyList<HixAst> declarations, List<HixParseDiagnostic> diagnostics, HixBackend backend = null) {
    backend ??= HixCoreBackend.Instance;
    void Error(HixAst node, string message) => diagnostics.Add(new HixParseDiagnostic(node.Line, message));
    var patterns = declarations.OfType<TypeDeclarationAst>().GroupBy(type => type.Name, StringComparer.Ordinal)
      .ToDictionary(group => group.Key, group => group.First().Pattern, StringComparer.Ordinal);
    void Signature(FunctionDeclarationAst function, FunctionSignature signature) {
      void Fields(IReadOnlyList<SignatureField> fields, string kind, bool output) {
        if (fields == null) {
          ValidatePattern(output ? signature.OutputPattern : signature.InputPattern, function, new HashSet<string>());
          return;
        }
        var names = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < fields.Count; index++) {
          var field = fields[index];
          if (!names.Add(field.Name)) Error(function, "duplicate signature field '" + field.Name + "'");
          ValidatePattern(field.Pattern, function, new HashSet<string>());
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
    foreach (var group in declarations.OfType<TypeDeclarationAst>().GroupBy(type => type.Name, StringComparer.Ordinal))
      if (group.Count() > 1) Error(group.First(), "duplicate pattern '" + group.Key + "'");
    foreach (var type in declarations.OfType<TypeDeclarationAst>()) ValidatePattern(type.Pattern, type, new HashSet<string>());
    Scope(declarations, []);

    void ValidatePattern(HixPattern pattern, HixAst owner, ISet<string> path) {
      switch (pattern) {
        case NamedHixPattern named when !patterns.ContainsKey(named.Name):
          Error(owner, "unknown pattern '" + named.Name + "'"); break;
        case NamedHixPattern named when !path.Add(named.Name):
          Error(owner, "cyclic pattern alias '" + named.Name + "'"); break;
        case NamedHixPattern named:
          ValidatePattern(patterns[named.Name], owner, path); path.Remove(named.Name); break;
        case UnionHixPattern union:
          foreach (var member in union.Patterns) ValidatePattern(member, owner, new HashSet<string>(path)); break;
        case TupleHixPattern tuple:
          foreach (var field in tuple.Fields) ValidatePattern(field.Pattern, owner, new HashSet<string>(path)); break;
        case TableHixPattern table:
          foreach (var field in table.Fields) ValidatePattern(field.Pattern, owner, new HashSet<string>(path)); break;
        case ManyHixPattern many: ValidatePattern(many.Element, owner, path); break;
        case MapHixPattern map:
          ValidatePattern(map.Key, owner, new HashSet<string>(path));
          ValidatePattern(map.Value, owner, new HashSet<string>(path)); break;
        case ConstantHixPattern constant: ValidatePattern(constant.Underlying, owner, path); break;
        case ConstrainedHixPattern constrained: ValidatePattern(constrained.Underlying, owner, path); break;
        case DelegateHixPattern callable:
          foreach (var field in callable.Parameters) ValidatePattern(field.Pattern, owner, new HashSet<string>(path));
          ValidatePattern(callable.Result, owner, path); break;
      }
    }
  }

  private static IEnumerable<HixAst> Descendants(HixAst node) {
    yield return node;
    foreach (var child in node.SemanticChildren)
      foreach (var descendant in Descendants(child)) yield return descendant;
  }
}
