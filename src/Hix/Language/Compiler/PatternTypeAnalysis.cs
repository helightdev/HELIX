using System;
using System.Collections.Generic;
using System.Linq;

namespace Hix.Compiler;

/// <summary>Conservative flow analysis: locals and parameters are typed; shared variables remain any.</summary>
public static class PatternTypeAnalysis {
  public static void Validate(IReadOnlyList<HixAst> declarations, IReadOnlyDictionary<string, HixPattern> patterns,
    ICollection<HixParseDiagnostic> diagnostics, HixBackend backend = null) {
    backend ??= HixCoreBackend.Instance;
    var functions = declarations.OfType<FunctionDeclarationAst>().GroupBy(value => value.Name, StringComparer.Ordinal)
      .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);
    foreach (var function in declarations.OfType<FunctionDeclarationAst>()) {
      var parameters = ParameterPatterns(function);
      var expected = function.Signatures.Count == 1 && function.Signatures[0].IsPatternSyntax
        ? ReturnPattern(function.Signatures[0]) : HixPattern.Any;
      AnalyzeBlock(function.Body, new Dictionary<string, HixPattern>(StringComparer.Ordinal), parameters, expected,
        functions, patterns, diagnostics, backend);
    }
    foreach (var mixin in declarations.OfType<MixinDeclarationAst>()) {
      var nested = mixin.Declarations.OfType<FunctionDeclarationAst>().Concat(declarations.OfType<FunctionDeclarationAst>())
        .GroupBy(value => value.Name, StringComparer.Ordinal)
        .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);
      foreach (var function in mixin.Declarations.OfType<FunctionDeclarationAst>())
        AnalyzeBlock(function.Body, new Dictionary<string, HixPattern>(StringComparer.Ordinal), ParameterPatterns(function),
          function.Signatures.Count == 1 && function.Signatures[0].IsPatternSyntax
            ? ReturnPattern(function.Signatures[0]) : HixPattern.Any,
          nested, patterns, diagnostics, backend);
      foreach (var expression in mixin.Declarations.OfType<ExpressionDeclarationAst>())
        AnalyzeBlock(expression.Body, new Dictionary<string, HixPattern>(StringComparer.Ordinal), [], HixPattern.Any,
          nested, patterns, diagnostics, backend);
    }
  }

  private static IReadOnlyList<HixPatternField> ParameterPatterns(FunctionDeclarationAst function) {
    if (function.Signatures.Count == 0) return [];
    var maximum = function.Signatures.Where(value => value.Inputs != null).Select(value => value.Inputs.Count).DefaultIfEmpty().Max();
    var result = new List<HixPatternField>();
    for (var index = 0; index < maximum; index++) {
      var fields = function.Signatures.Where(value => value.Inputs != null && index < value.Inputs.Count)
        .Select(value => value.Inputs[index]).ToArray();
      if (fields.Length == 0) continue;
      result.Add(new(fields[0].Name, Union(fields.Select(value => value.Pattern)), fields.All(value => value.Optional)));
    }
    return result;
  }

  private static void AnalyzeBlock(BlockStatementAst block, IDictionary<string, HixPattern> locals,
    IReadOnlyList<HixPatternField> parameters, HixPattern expectedReturn,
    IReadOnlyDictionary<string, FunctionDeclarationAst[]> functions, IReadOnlyDictionary<string, HixPattern> patterns,
    ICollection<HixParseDiagnostic> diagnostics, HixBackend backend) {
    foreach (var statement in block.Statements) {
      switch (statement) {
        case AssignmentStatementAst assignment:
          var assigned = Infer(assignment.Value, locals, parameters, functions, patterns, diagnostics, backend);
          if (assignment.Storage == StorageSpace.Local) locals[assignment.Name] = assigned;
          break;
        case InvocationStatementAst invocation:
          Infer(invocation.Call, locals, parameters, functions, patterns, diagnostics, backend); break;
        case ControlFlowStatementAst {Operation: ControlFlowKind.Return} returned:
          var actual = returned.Values.Count switch {
            0 => new KindHixPattern(HixValueKind.Null),
            1 => Infer(returned.Values[0], locals, parameters, functions, patterns, diagnostics, backend),
            _ => new TupleHixPattern(returned.Values.Select((value, index) => new HixPatternField(index.ToString(),
              Infer(value, locals, parameters, functions, patterns, diagnostics, backend))).ToArray())
          };
          if (expectedReturn is not AnyHixPattern && HixPatternRelations.Relate(actual, expectedReturn, patterns) == HixPatternRelation.Never)
            diagnostics.Add(new(statement.Line, "return pattern '" + actual.Display + "' does not match '" + expectedReturn.Display + "'"));
          break;
        case BlockStatementAst nested:
          AnalyzeBlock(nested, new Dictionary<string, HixPattern>(locals, StringComparer.Ordinal), parameters,
            expectedReturn, functions, patterns, diagnostics, backend); break;
        default:
          foreach (var expression in statement.SemanticChildren.OfType<ExpressionAst>())
            Infer(expression, locals, parameters, functions, patterns, diagnostics, backend);
          break;
      }
    }
  }

  private static HixPattern Infer(ExpressionAst expression, IDictionary<string, HixPattern> locals,
    IReadOnlyList<HixPatternField> parameters, IReadOnlyDictionary<string, FunctionDeclarationAst[]> functions,
    IReadOnlyDictionary<string, HixPattern> patterns, ICollection<HixParseDiagnostic> diagnostics, HixBackend backend) {
    switch (expression) {
      case StringExpressionAst: return new KindHixPattern(HixValueKind.String);
      case NumberExpressionAst: return new KindHixPattern(HixValueKind.Number);
      case BooleanExpressionAst: return new KindHixPattern(HixValueKind.Bool);
      case NullExpressionAst: return new KindHixPattern(HixValueKind.Null);
      case TupleExpressionAst tuple: return new TupleHixPattern(tuple.Values.Select((value, index) =>
        new HixPatternField(index.ToString(), Infer(value, locals, parameters, functions, patterns, diagnostics, backend))).ToArray());
      case TableExpressionAst table: return new TableHixPattern(table.Entries.Select(entry =>
        new HixPatternField(entry.Key, Infer(entry.Value, locals, parameters, functions, patterns, diagnostics, backend))).ToArray());
      case MemberExpressionAst {Receiver: RootExpressionAst {Name: "local"}} member:
        return locals.TryGetValue(member.Member, out var local) ? local : HixPattern.Any;
      case MemberExpressionAst {Receiver: RootExpressionAst {Name: "param"}} member:
        return parameters.FirstOrDefault(value => value.Name == member.Member)?.Pattern ?? HixPattern.Any;
      case MemberExpressionAst member:
        var receiver = Infer(member.Receiver, locals, parameters, functions, patterns, diagnostics, backend);
        return Member(receiver, member.Member, patterns);
      case RootExpressionAst {IsSmart: true, Name: var name} when int.TryParse(name, out var index) && index < parameters.Count:
        return parameters[index].Pattern;
      case RootExpressionAst: return HixPattern.Any; // var, tar and host roots deliberately remain dynamic.
      case FallbackExpressionAst fallback:
        return Union([Infer(fallback.Value, locals, parameters, functions, patterns, diagnostics, backend),
          Infer(fallback.Fallback, locals, parameters, functions, patterns, diagnostics, backend)]);
      case SelectionExpressionAst selection:
        if (selection.Selector != null)
          Infer(selection.Selector, locals, parameters, functions, patterns, diagnostics, backend);
        var results = new List<HixPattern>();
        foreach (var branch in selection.Branches) {
          var branchLocals = new Dictionary<string, HixPattern>(locals, StringComparer.Ordinal);
          foreach (var condition in branch.Conditions)
            Infer(condition, branchLocals, parameters, functions, patterns, diagnostics, backend);
          results.Add(InferResult(branch.Result, branchLocals, parameters, functions, patterns, diagnostics, backend));
        }
        if (selection.Fallback != null)
          results.Add(InferResult(selection.Fallback,
            new Dictionary<string, HixPattern>(locals, StringComparer.Ordinal), parameters,
            functions, patterns, diagnostics, backend));
        return Union(results);
      case CallExpressionAst call:
        var supplied = call.Arguments.Select(value => Infer(value, locals, parameters, functions, patterns, diagnostics, backend)).ToArray();
        if (patterns.TryGetValue(call.Name, out var checkedPattern)) {
          if (supplied.Length != 1) diagnostics.Add(new(call.Line, "pattern '" + call.Name + "' expects one value"));
          else if (HixPatternRelations.Relate(supplied[0], checkedPattern, patterns) == HixPatternRelation.Never)
            diagnostics.Add(new(call.Line, "pattern '" + supplied[0].Display + "' can never match '" + call.Name + "'"));
          return new NamedHixPattern(call.Name);
        }
        if (!functions.TryGetValue(call.Name, out var overloads)) {
          var builtins = backend.Functions.Enumerate().Where(definition => definition.Name == call.Name).ToArray();
          if (builtins.Length == 0) return HixPattern.Any;
          var signatures = builtins.SelectMany(definition => definition.Signatures).Where(signature =>
            signature.MatchesArgumentCount(supplied.Length)).ToArray();
          if (signatures.Length == 0) {
            diagnostics.Add(new(call.Line, "no overload of '" + call.Name + "' accepts " + supplied.Length + " argument(s)"));
            return HixPattern.Any;
          }
          var compatible = signatures.Where(signature => supplied.Select((value, index) =>
            HixPatternRelations.Relate(value, KindPattern(signature.GetArgumentType(index)), patterns))
            .All(relation => relation != HixPatternRelation.Never)).ToArray();
          if (compatible.Length == 0) {
            diagnostics.Add(new(call.Line, "no overload of '" + call.Name + "' accepts (" +
              string.Join(", ", supplied.Select(value => value.Display)) + ")"));
            return HixPattern.Any;
          }
          return Union(compatible.Select(signature => KindPattern(signature.ResultType)));
        }
        if (overloads.Any(function => function.Signatures.Count == 0)) return HixPattern.Any;
        var candidates = overloads.SelectMany(function => function.Signatures)
          .Where(signature => Accepts(signature, supplied, patterns)).ToArray();
        if (candidates.Length == 0) {
          diagnostics.Add(new(call.Line, "no overload of '" + call.Name + "' accepts (" +
            string.Join(", ", supplied.Select(value => value.Display)) + ")"));
          return HixPattern.Any;
        }
        var distinct = candidates.Select(ReturnPattern).Distinct().ToArray();
        return Union(distinct);
      default: return HixPattern.Any;
    }
  }

  private static HixPattern InferResult(HixAst node, IDictionary<string, HixPattern> locals,
    IReadOnlyList<HixPatternField> parameters, IReadOnlyDictionary<string, FunctionDeclarationAst[]> functions,
    IReadOnlyDictionary<string, HixPattern> patterns, ICollection<HixParseDiagnostic> diagnostics,
    HixBackend backend) {
    switch (node) {
      case ExpressionAst expression:
        return Infer(expression, locals, parameters, functions, patterns, diagnostics, backend);
      case InvocationStatementAst invocation:
        return Infer(invocation.Call, locals, parameters, functions, patterns, diagnostics, backend);
      case BlockStatementAst block:
        AnalyzeBlock(block, locals, parameters, HixPattern.Any, functions, patterns, diagnostics, backend);
        return HixPattern.Any;
      default:
        foreach (var child in node.SemanticChildren)
          InferResult(child, locals, parameters, functions, patterns, diagnostics, backend);
        return HixPattern.Any;
    }
  }

  private static bool Accepts(FunctionSignature signature, IReadOnlyList<HixPattern> supplied,
    IReadOnlyDictionary<string, HixPattern> patterns) {
    if (signature.Inputs == null)
      return supplied.Count == 1 && HixPatternRelations.Relate(supplied[0], signature.InputPattern, patterns) != HixPatternRelation.Never;
    var required = signature.Inputs.Count(value => !value.Optional && !value.Variadic);
    if (supplied.Count < required || !signature.Inputs.Any(value => value.Variadic) && supplied.Count > signature.Inputs.Count) return false;
    return supplied.Select((value, index) => HixPatternRelations.Relate(value,
      signature.Inputs[Math.Min(index, signature.Inputs.Count - 1)].Pattern, patterns))
      .All(value => value != HixPatternRelation.Never);
  }

  private static HixPattern Member(HixPattern receiver, string name, IReadOnlyDictionary<string, HixPattern> patterns) {
    if (receiver is NamedHixPattern named && patterns.TryGetValue(named.Name, out var resolved)) receiver = resolved;
    return receiver is TableHixPattern table
      ? table.Fields.FirstOrDefault(field => field.Name == name)?.Pattern ?? HixPattern.Any : HixPattern.Any;
  }

  private static HixPattern ReturnPattern(FunctionSignature signature) => signature.Outputs == null
    ? signature.OutputPattern ?? HixPattern.Any
    : new TableHixPattern(signature.Outputs.Select(field => field.AsPatternField()).ToArray());
  private static HixPattern Union(IEnumerable<HixPattern> values) {
    return HixPatterns.Union(values);
  }
  private static HixPattern KindPattern(HixValueKind kind) => kind == HixValueKind.Any
    ? HixPattern.Any : new KindHixPattern(kind);
}
