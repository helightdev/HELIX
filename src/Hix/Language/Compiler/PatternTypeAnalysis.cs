using System;
using System.Collections.Generic;
using System.Linq;

namespace Hix.Compiler;

/// <summary>Conservative flow analysis: locals and parameters are typed; shared variables remain any.</summary>
public static class PatternTypeAnalysis {
  [ThreadStatic] private static HashSet<FunctionDeclarationIr> activeCallables;

  public static DelegateHixPattern InferCallable(ExpressionIr expression, DelegateHixPattern context,
    IReadOnlyDictionary<string, FunctionDeclarationIr[]> functions, IReadOnlyDictionary<string, HixPattern> patterns,
    HixBackend backend, out bool valid, bool invocationContext = false) {
    valid = true;
    BlockStatementIr body;
    FunctionDeclarationIr function = null;
    if (expression is LambdaExpressionIr lambda) body = lambda.Body;
    else if (expression is RootExpressionIr {IsSmart: false} root && functions.TryGetValue(root.Name, out var methods)) {
      if (invocationContext && methods.Any(method => method.Signatures.Count != 0)) {
        var arguments = context.Parameters.Select((field, index) => (ExpressionIr)new RootExpressionIr(index.ToString()) {
          InferredPattern = field.Pattern
        }).ToArray();
        var resolution = HixCallResolver.Resolve(new CallExpressionIr(root.Name, arguments), functions, backend, patterns,
          argument => argument.InferredPattern, out var ambiguous);
        if (resolution == null) {
          valid = !ambiguous && methods.Any(method => method.Signatures.Count == 0);
          return valid ? new DelegateHixPattern(context.Parameters, HixPattern.Any) : null;
        }
        return new DelegateHixPattern(context.Parameters, resolution.Signature.Result);
      }
      if (methods.Length != 1) return null;
      function = methods[0];
      if (function.Signatures.Count == 1) {
        var signature = function.Signatures[0].Constant(function.Name);
        var parameters = function.TypeParameters.Concat(function.Signatures[0].TypeParameters).Distinct().ToArray();
        if (parameters.Length != 0 && context != null && context.Parameters.Count == signature.Parameters.Count) {
          var substitutions = new Dictionary<HixTypeParameter, HixPattern>();
          for (var index = 0; index < signature.Parameters.Count; index++)
            if (!HixCallResolver.Unify(signature.Parameters[index].Pattern, context.Parameters[index].Pattern, substitutions, patterns)) {
              valid = false; return null;
            }
          foreach (var parameter in parameters)
            if (substitutions.TryGetValue(parameter, out var value) &&
                !HixGenericTypes.SatisfiesBounds(value, parameter, substitutions, patterns)) {
              valid = false; return null;
            }
          return new DelegateHixPattern(signature.Parameters.Select(field => field with {
            Pattern = HixGenericTypes.Substitute(field.Pattern, substitutions, true) }).ToArray(),
            HixGenericTypes.Substitute(signature.Result, substitutions, true));
        }
        return new DelegateHixPattern(signature.Parameters, signature.Result);
      }
      body = function.Body;
      if (body == null) return new DelegateHixPattern(context?.Parameters ?? [], HixPattern.Any);
    } else return null;
    activeCallables ??= new();
    if (function != null && !activeCallables.Add(function)) return new DelegateHixPattern(context?.Parameters ?? [], HixPattern.Any);
    try {
      var diagnostics = new List<HixParseDiagnostic>();
      var result = InferResult(body, LocalEnvironment(body), context?.Parameters ?? [], functions, patterns, diagnostics, backend);
      return new DelegateHixPattern(context?.Parameters ?? [], result);
    } finally { if (function != null) activeCallables.Remove(function); }
  }

  public static void Validate(IReadOnlyList<HixIrNode> declarations, IReadOnlyDictionary<string, HixPattern> patterns,
    ICollection<HixParseDiagnostic> diagnostics, HixBackend backend = null) {
    backend ??= HixCoreBackend.Instance;
    declarations = DeclarationExports.Expand(declarations);
    patterns = backend.Functions.Types.Concat(declarations.OfType<TypeDeclarationIr>()).GroupBy(type => type.Name)
      .ToDictionary(group => group.Key, group => group.Last().Pattern, StringComparer.Ordinal);
    var functions = declarations.OfType<FunctionDeclarationIr>().GroupBy(value => value.Name, StringComparer.Ordinal)
      .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);
    foreach (var function in declarations.OfType<FunctionDeclarationIr>().Where(function => !function.IsExternal)) {
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
      foreach (var function in mixin.Declarations.OfType<FunctionDeclarationIr>().Where(function => !function.IsExternal))
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
      case RootExpressionIr {IsSmart: true} smart:
        return locals.TryGetValue(smart.Name, out var smartLocal) ? smartLocal :
          parameters.FirstOrDefault(parameter => parameter.Name == smart.Name)?.Pattern ?? HixPattern.Any;
      case RootExpressionIr {IsSmart: false, Name: "args"}: return new TupleHixPattern(parameters.ToArray());
      case RootExpressionIr {IsSmart: false, Name: "local" or "var" or "tar" or "param"}: return HixPattern.Any;
      case RootExpressionIr {IsSmart: false, Name: "null"}: return new KindHixPattern(HixValueKind.Null);
      case RootExpressionIr {IsSmart: false, Name: "missing"}: return new KindHixPattern(HixValueKind.Missing);
      case RootExpressionIr {IsSmart: false} root when HixPatterns.TryPrimitive(root.Name, out _): return new KindHixPattern(HixValueKind.Pattern);
      case RootExpressionIr {IsSmart: false} root when patterns.ContainsKey(root.Name): return new KindHixPattern(HixValueKind.Pattern);
      case RootExpressionIr {IsSmart: false} root when backend.Functions.Resolve(root.Name).Count != 0 && !functions.ContainsKey(root.Name): return new KindHixPattern(HixValueKind.Function);
      case RootExpressionIr {IsSmart: false} root when functions.TryGetValue(root.Name, out var methods):
        var method = methods.FirstOrDefault();
        var signature = method?.Signatures.FirstOrDefault();
        return signature == null ? new DelegateHixPattern([], HixPattern.Any) :
          new DelegateHixPattern(signature.Constant(method.Name).Parameters, signature.Constant(method.Name).Result);
      case LambdaExpressionIr lambda:
        return new DelegateHixPattern([], InferResult(lambda.Body, new Dictionary<string, HixPattern>(),
          [], functions, patterns, diagnostics, backend));
      case RootExpressionIr {IsSmart: false} root when backend.Roots.TryGetValue(root.Name, out var hostRoot): return hostRoot.Pattern;
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
        if (backend.Functions.Resolve(call.Name).Count == 0 && patterns.TryGetValue(call.Name, out var checkedPattern)) {
          if (!HixPatterns.TryStructural(checkedPattern, patterns, out var structural) ||
              !AcceptsConstructor(call, supplied, structural is TableHixPattern table ? table.Fields :
                ((TupleHixPattern)structural).Fields, patterns)) {
            if (structural != null) diagnostics.Add(new(call.Line, PatternConstructorDiagnostics.Describe(call.Name,
              call.ArgumentNames, supplied, structural is TableHixPattern mismatchTable ? mismatchTable.Fields :
                ((TupleHixPattern)structural).Fields, patterns)));
            else diagnostics.Add(new(call.Line, "pattern '" + call.Name + "' has no constructor; use cast(value, " + call.Name + ")"));
          }
          return new NamedHixPattern(call.Name);
        }
        var resolution = HixCallResolver.Resolve(call, functions, backend, patterns,
          value => { var position = call.Arguments.ToList().IndexOf(value);
            return position >= 0 ? supplied[position] : Infer(value, locals, parameters, functions, patterns, diagnostics, backend); }, out var ambiguous);
        call.Resolution = resolution;
        if (resolution != null) return resolution.Signature.Result;
        if (ambiguous) {
          if (supplied.All(value => value is not AnyHixPattern))
            diagnostics.Add(new(call.Line, "ambiguous overload of '" + call.Name + "'"));
          return HixPattern.Any;
        }
        if (functions.TryGetValue(call.Name, out var dynamicFunctions) && dynamicFunctions.Any(value => value.Signatures.Count == 0 || value.Signatures.Any(signature => signature.Inputs == null && signature.InputPattern == null)))
          return HixPattern.Any;
        // A namespace may be supplied by another source file when the global catalog is prepared.
        if (call.UsingImports.Count != 0 && HixUsingImports.Candidates(call.OriginalName, call.UsingImports)
            .Any(name => name != call.OriginalName && !functions.ContainsKey(name) && backend.Functions.Resolve(name).Count == 0))
          return HixPattern.Any;
        if (!functions.ContainsKey(call.Name) && backend.Functions.Resolve(call.Name).Count != 0 &&
            !backend.Functions.Resolve(call.Name).Any(definition => definition.MatchesArgumentCount(supplied.Length))) {
          diagnostics.Add(new(call.Line, "no overload of '" + call.Name + "' accepts " + supplied.Length + " argument(s)"));
          return HixPattern.Any;
        }
        if (backend.Functions.Resolve(call.Name).Count != 0 || functions.ContainsKey(call.Name))
          diagnostics.Add(new(call.Line, "no overload of '" + call.Name + "' accepts (" +
            string.Join(", ", supplied.Select(value => value.Display)) + ")"));
        return HixPattern.Any;
      default: return HixPattern.Any;
    }
  }

  private static bool AcceptsConstructor(CallExpressionIr call, IReadOnlyList<HixPattern> supplied,
    IReadOnlyList<HixPatternField> fields, IReadOnlyDictionary<string, HixPattern> patterns) {
    if (supplied.Count > fields.Count) return false;
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
        var returns = block.Statements.OfType<ControlFlowStatementIr>().Where(value => value.Operation == ControlFlowKind.Return)
          .SelectMany(value => value.Values).Select(value => Infer(value, locals, parameters, functions, patterns, diagnostics, backend)).ToArray();
        return returns.Length == 0 ? new KindHixPattern(HixValueKind.Null) : HixPatterns.Union(returns);
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

  internal static HixPattern Member(HixPattern receiver, string name, IReadOnlyDictionary<string, HixPattern> patterns) {
    var active = new HashSet<string>();
    while (receiver is NamedHixPattern named && active.Add(named.Name) && patterns.TryGetValue(named.Name, out var resolved)) receiver = resolved;
    if (receiver is TaggedHixPattern tagged && name == TaggedHixPattern.FieldName)
      return new ConstantHixPattern(tagged.Discriminator, new KindHixPattern(HixValueKind.String));
    if (receiver is UnionHixPattern union) return HixPatterns.Union(union.Patterns.Select(pattern => Member(pattern, name, patterns)));
    if (HixPatterns.TryStructural(receiver, patterns, out var structural)) receiver = structural;
    return receiver switch {
      TableHixPattern table => table.Fields.FirstOrDefault(field => field.Name == name)?.Pattern ?? HixPattern.Any,
      TupleHixPattern tuple when int.TryParse(name, out var index) && index >= 0 && index < tuple.Fields.Count => tuple.Fields[index].Pattern,
      ManyHixPattern many when int.TryParse(name, out _) => many.Element,
      MapHixPattern map => map.Value,
      _ => HixPattern.Any
    };
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
