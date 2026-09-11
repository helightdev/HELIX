using System;
using System.Collections.Generic;
using System.Linq;

namespace Hix.Compiler;

/// <summary>Conservative flow analysis: locals and parameters are typed; shared variables remain any.</summary>
public static class PatternTypeAnalysis {
  public static void Validate(IReadOnlyList<HixIrNode> declarations, IReadOnlyDictionary<string, HixPattern> patterns,
    ICollection<HixParseDiagnostic> diagnostics, HixBackend backend = null) {
    backend ??= HixCoreBackend.Instance;
    var functions = declarations.OfType<FunctionDeclarationIr>().GroupBy(value => value.Name, StringComparer.Ordinal)
      .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);
    foreach (var function in declarations.OfType<FunctionDeclarationIr>()) {
      var parameters = ParameterPatterns(function);
      var expected = function.Signatures.Count == 1 && function.Signatures[0].IsPatternSyntax
        ? ReturnPattern(function.Signatures[0]) : HixPattern.Any;
      AnalyzeBlock(function.Body, LocalEnvironment(function.Body), parameters, expected,
        functions, patterns, diagnostics, backend);
    }
    foreach (var mixin in declarations.OfType<MixinDeclarationIr>()) {
      var nested = mixin.Declarations.OfType<FunctionDeclarationIr>().Concat(declarations.OfType<FunctionDeclarationIr>())
        .GroupBy(value => value.Name, StringComparer.Ordinal)
        .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);
      foreach (var function in mixin.Declarations.OfType<FunctionDeclarationIr>())
        AnalyzeBlock(function.Body, LocalEnvironment(function.Body), ParameterPatterns(function),
          function.Signatures.Count == 1 && function.Signatures[0].IsPatternSyntax
            ? ReturnPattern(function.Signatures[0]) : HixPattern.Any,
          nested, patterns, diagnostics, backend);
      foreach (var expression in mixin.Declarations.OfType<ExpressionDeclarationIr>())
        AnalyzeBlock(expression.Body, LocalEnvironment(expression.Body),
          mixin.Parameters.Select(field => field.AsPatternField()).ToArray(), HixPattern.Any,
          nested, patterns, diagnostics, backend);
    }
  }

  private static Dictionary<string, HixPattern> LocalEnvironment(BlockStatementIr body) {
    var locals = new Dictionary<string, HixPattern>(StringComparer.Ordinal);
    void Collect(HixIrNode node) {
      if (node is AssignmentStatementIr {Storage: StorageSpace.Local, IsDeclaration: true} declaration)
        locals[declaration.Name] = declaration.DeclaredPattern ?? HixPattern.Any;
      foreach (var child in node.SemanticChildren) Collect(child);
    }
    Collect(body);
    return locals;
  }

  private static IReadOnlyList<HixPatternField> ParameterPatterns(FunctionDeclarationIr function) {
    if (function.Signatures.Count == 0) return [];
    var maximum = function.Signatures.Where(value => value.Inputs != null).Select(value => value.Inputs.Count).DefaultIfEmpty().Max();
    var result = new List<HixPatternField>();
    for (var index = 0; index < maximum; index++) {
      var fields = function.Signatures.Where(value => value.Inputs != null && index < value.Inputs.Count)
        .Select(value => value.Inputs[index]).ToArray();
      if (fields.Length == 0) continue;
      result.Add(new(fields[0].Name, Union(fields.Select(value => value.Pattern)),
        fields.All(value => value.AllowsMissing)));
    }
    return result;
  }

  private static void AnalyzeBlock(BlockStatementIr block, IDictionary<string, HixPattern> locals,
    IReadOnlyList<HixPatternField> parameters, HixPattern expectedReturn,
    IReadOnlyDictionary<string, FunctionDeclarationIr[]> functions, IReadOnlyDictionary<string, HixPattern> patterns,
    ICollection<HixParseDiagnostic> diagnostics, HixBackend backend) {
    foreach (var statement in block.Statements) {
      switch (statement) {
        case AssignmentStatementIr assignment:
          var assigned = assignment.Value == null ? HixPattern.Any :
            Infer(assignment.Value, locals, parameters, functions, patterns, diagnostics, backend);
          if (assignment.Storage == StorageSpace.Local) {
            if (assignment.IsDeclaration) {
              var declared = assignment.DeclaredPattern ?? assigned;
              if (assignment.DeclaredPattern != null && assignment.Value != null &&
                  HixPatternRelations.Relate(assigned, declared, patterns) == HixPatternRelation.Never)
                diagnostics.Add(new(statement.Line, "initial value pattern '" + assigned.Display +
                  "' does not match local '" + assignment.Name + "' pattern '" + declared.Display + "'"));
              locals[assignment.Name] = declared;
            } else if (!locals.TryGetValue(assignment.Name, out var declared)) {
              diagnostics.Add(new(statement.Line, "cannot assign undeclared local '" + assignment.Name + "'"));
            } else if (HixPatternRelations.Relate(assigned, declared, patterns) == HixPatternRelation.Never) {
              diagnostics.Add(new(statement.Line, "assigned pattern '" + assigned.Display +
                "' does not match local '" + assignment.Name + "' pattern '" + declared.Display + "'"));
            }
          }
          break;
        case InvocationStatementIr invocation:
          Infer(invocation.Call, locals, parameters, functions, patterns, diagnostics, backend); break;
        case ControlFlowStatementIr {Operation: ControlFlowKind.Return} returned:
          var actual = returned.Values.Count switch {
            0 => new KindHixPattern(HixValueKind.Null),
            1 => Infer(returned.Values[0], locals, parameters, functions, patterns, diagnostics, backend),
            _ => new TupleHixPattern(returned.Values.Select((value, index) => new HixPatternField(index.ToString(),
              Infer(value, locals, parameters, functions, patterns, diagnostics, backend))).ToArray())
          };
          if (expectedReturn is not AnyHixPattern && HixPatternRelations.Relate(actual, expectedReturn, patterns) == HixPatternRelation.Never)
            diagnostics.Add(new(statement.Line, "return pattern '" + actual.Display + "' does not match '" + expectedReturn.Display + "'"));
          break;
        case BlockStatementIr nested:
          AnalyzeBlock(nested, new Dictionary<string, HixPattern>(locals, StringComparer.Ordinal), parameters,
            expectedReturn, functions, patterns, diagnostics, backend); break;
        default:
          foreach (var expression in statement.SemanticChildren.OfType<ExpressionIr>())
            Infer(expression, locals, parameters, functions, patterns, diagnostics, backend);
          break;
      }
    }
  }

  private static HixPattern Infer(ExpressionIr expression, IDictionary<string, HixPattern> locals,
    IReadOnlyList<HixPatternField> parameters, IReadOnlyDictionary<string, FunctionDeclarationIr[]> functions,
    IReadOnlyDictionary<string, HixPattern> patterns, ICollection<HixParseDiagnostic> diagnostics, HixBackend backend) {
    switch (expression) {
      case StringExpressionIr:
      case InterpolationExpressionIr:
        return new KindHixPattern(HixValueKind.String);
      case NumberExpressionIr: return new KindHixPattern(HixValueKind.Number);
      case BooleanExpressionIr: return new KindHixPattern(HixValueKind.Bool);
      case NullExpressionIr: return new KindHixPattern(HixValueKind.Null);
      case MissingExpressionIr: return new KindHixPattern(HixValueKind.Missing);
      case TupleExpressionIr tuple: return new TupleHixPattern(tuple.Values.Select((value, index) =>
        new HixPatternField(index.ToString(), Infer(value, locals, parameters, functions, patterns, diagnostics, backend))).ToArray());
      case TableExpressionIr table: return new TableHixPattern(table.Entries.Select(entry =>
        new HixPatternField(entry.Key, Infer(entry.Value, locals, parameters, functions, patterns, diagnostics, backend))).ToArray());
      case MemberExpressionIr {Receiver: RootExpressionIr {Name: "local"}} member:
        return locals.TryGetValue(member.Member, out var local) ? local : HixPattern.Any;
      case MemberExpressionIr {Receiver: RootExpressionIr {Name: "param"}} member:
        return parameters.FirstOrDefault(value => value.Name == member.Member)?.Pattern ?? HixPattern.Any;
      case MemberExpressionIr member:
        var receiver = Infer(member.Receiver, locals, parameters, functions, patterns, diagnostics, backend);
        return Member(receiver, member.Member, patterns);
      case RootExpressionIr {IsSmart: true, Name: var name} when int.TryParse(name, out var index) && index < parameters.Count:
        return parameters[index].Pattern;
      case RootExpressionIr: return HixPattern.Any; // var, tar and host roots deliberately remain dynamic.
      case FallbackExpressionIr fallback:
        return Union([Infer(fallback.Value, locals, parameters, functions, patterns, diagnostics, backend),
          Infer(fallback.Fallback, locals, parameters, functions, patterns, diagnostics, backend)]);
      case SelectionExpressionIr selection:
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
      case CallExpressionIr call:
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
          .Where(signature => Accepts(call, signature, supplied, patterns)).ToArray();
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

  private static HixPattern InferResult(HixIrNode node, IDictionary<string, HixPattern> locals,
    IReadOnlyList<HixPatternField> parameters, IReadOnlyDictionary<string, FunctionDeclarationIr[]> functions,
    IReadOnlyDictionary<string, HixPattern> patterns, ICollection<HixParseDiagnostic> diagnostics,
    HixBackend backend) {
    switch (node) {
      case ExpressionIr expression:
        return Infer(expression, locals, parameters, functions, patterns, diagnostics, backend);
      case InvocationStatementIr invocation:
        return Infer(invocation.Call, locals, parameters, functions, patterns, diagnostics, backend);
      case BlockStatementIr block:
        AnalyzeBlock(block, locals, parameters, HixPattern.Any, functions, patterns, diagnostics, backend);
        return HixPattern.Any;
      default:
        foreach (var child in node.SemanticChildren)
          InferResult(child, locals, parameters, functions, patterns, diagnostics, backend);
        return HixPattern.Any;
    }
  }

  private static bool Accepts(CallExpressionIr call, FunctionSignature signature, IReadOnlyList<HixPattern> supplied,
    IReadOnlyDictionary<string, HixPattern> patterns) {
    if (signature.Inputs == null && signature.InputPattern == null)
      return !call.ArgumentNames.Any(name => name != null);
    if (signature.Inputs == null)
      return !call.ArgumentNames.Any(name => name != null) && supplied.Count == 1 &&
        HixPatternRelations.Relate(supplied[0], signature.InputPattern, patterns) != HixPatternRelation.Never;
    var fields = signature.Inputs;
    if (fields.Any(field => field.Variadic) || supplied.Count > fields.Count) return false;
    var assigned = new bool[fields.Count];
    var positional = 0;
    var sawNamed = false;
    for (var index = 0; index < supplied.Count; index++) {
      var name = call.ArgumentNames[index];
      int target;
      if (name == null) {
        if (sawNamed || positional >= fields.Count) return false;
        target = positional++;
      } else {
        sawNamed = true;
        target = fields.ToList().FindIndex(field => field.Name == name);
        if (target < 0) return false;
      }
      if (assigned[target] || HixPatternRelations.Relate(supplied[index], fields[target].Pattern, patterns) ==
          HixPatternRelation.Never) return false;
      assigned[target] = true;
    }
    return fields.Select((field, index) => assigned[index] || field.AllowsMissing).All(value => value);
  }

  private static HixPattern Member(HixPattern receiver, string name, IReadOnlyDictionary<string, HixPattern> patterns) {
    if (receiver is NamedHixPattern named && patterns.TryGetValue(named.Name, out var resolved)) receiver = resolved;
    if (receiver is TaggedHixPattern tagged) {
      if (name == TaggedHixPattern.FieldName)
        return new ConstantHixPattern(tagged.Discriminator, new KindHixPattern(HixValueKind.String));
      receiver = tagged.Underlying;
    }
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
