using System;
using System.Collections.Generic;
using System.Linq;

namespace Hix.Compiler.Steps;

/// <summary>Flow-sensitive local/parameter inference and static overload binding.</summary>
public sealed class PatternBindingStep : HixCompilerStep {
  public override void Run(HixCompilation compilation) {
    var input = compilation.Module;
    var globals = compilation.Catalog;
    var functions = input.Functions.Concat(globals.Functions).GroupBy(function => function.Name, StringComparer.Ordinal)
      .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);
    new Binder(functions, globals.Patterns, globals.Backend, compilation.Diagnostics,
      input.Parameters ?? []).Visit(input);
  }

  private sealed class Binder(IReadOnlyDictionary<string, FunctionDeclarationIr[]> functions,
    IReadOnlyDictionary<string, HixPattern> patterns, HixBackend backend,
    ICollection<HixParseDiagnostic> diagnostics, IReadOnlyList<SignatureField> inputParameters) : HixIrVisitor {
    private Dictionary<string, HixPattern> locals = new(StringComparer.Ordinal);
    private IReadOnlyList<SignatureField> parameters = [];
    private Dictionary<(StorageSpace, string), HixVariableSymbol> symbols = new();
    private HashSet<string> declaredLocals = new(StringComparer.Ordinal);
    private HashSet<string> uncertainLocals = new(StringComparer.Ordinal);
    private HixVariableSymbol Symbol(StorageSpace storage, string name) {
      if (!symbols.TryGetValue((storage, name), out var symbol)) symbols.Add((storage, name), symbol = new(name, storage));
      return symbol;
    }

    public override void Visit(HixIrNode node) {
      if (node is FallbackExpressionIr fallback) {
        Visit(fallback.Value);
        var before = new Dictionary<string, HixPattern>(locals, StringComparer.Ordinal);
        Visit(fallback.Fallback);
        Merge([before, locals]);
      } else base.Visit(node);
      if (node is ExpressionIr expression) expression.InferredPattern = Infer(expression);
    }

    protected override void VisitExpressionDeclaration(ExpressionDeclarationIr expression) =>
      InScope(inputParameters, expression.Body);

    protected override void VisitFunction(FunctionDeclarationIr function) =>
      InScope(function.Signatures.Count == 1 ? function.Signatures[0].Inputs ?? [] : [], function.Body);

    private void InScope(IReadOnlyList<SignatureField> fields, BlockStatementIr body) {
      var previousLocals = locals; var previousParameters = parameters;
      var previousSymbols = symbols;
      var previousDeclaredLocals = declaredLocals;
      declaredLocals = new(Descendants(body).OfType<AssignmentStatementIr>()
        .Where(assignment => assignment.Storage == StorageSpace.Local && assignment.IsDeclaration)
        .Select(assignment => assignment.Name), StringComparer.Ordinal);
      locals = new(StringComparer.Ordinal); parameters = fields; symbols = new();
      foreach (var assignment in Descendants(body).OfType<AssignmentStatementIr>())
        assignment.Symbol = Symbol(assignment.Storage, assignment.Name);
      try { Visit(body); }
      finally { locals = previousLocals; parameters = previousParameters; symbols = previousSymbols; declaredLocals = previousDeclaredLocals; }
    }

    protected override void VisitAssignment(AssignmentStatementIr assignment) {
      Visit(assignment.Value);
      if (assignment.Storage != StorageSpace.Local) return;
      if (assignment.IsDeclaration) locals[assignment.Name] = assignment.DeclaredPattern ??
        (assignment.Value == null || uncertainLocals.Contains(assignment.Name) ? HixPattern.Any : assignment.Value.InferredPattern);
    }

    protected override void VisitRoot(RootExpressionIr root) {
      if (!root.IsSmart) {
        root.Binding = new(functions.ContainsKey(root.Name) ? HixReferenceKind.Function : HixReferenceKind.Root, root.Name);
      } else if (root.Name == "it") root.Binding = new(HixReferenceKind.Parameter, "param");
      else if (declaredLocals.Contains(root.Name) && symbols.TryGetValue((StorageSpace.Local, root.Name), out var local))
        root.Binding = new(HixReferenceKind.Local, local.Name, local);
      else if (int.TryParse(root.Name, out var index) && index >= 0)
        root.Binding = new(HixReferenceKind.Parameter, root.Name, ParameterIndex: index);
      else if (parameters.Select((field, position) => (field, position))
                 .FirstOrDefault(item => item.field.Name == root.Name) is {field: not null} parameter)
        root.Binding = new(HixReferenceKind.Parameter, root.Name, ParameterIndex: parameter.position);
      else root.Binding = new(HixReferenceKind.Invalid, root.Name);
    }

    protected override void VisitMember(MemberExpressionIr member) {
      Visit(member.Receiver);
      member.Binding = member.Receiver is RootExpressionIr { IsSmart: false } root ? root.Name switch {
        "local" => new(HixReferenceKind.Local, member.Member, Symbol(StorageSpace.Local, member.Member)),
        "var" => new(HixReferenceKind.Variable, member.Member, Symbol(StorageSpace.Variable, member.Member)),
        "tar" => new(HixReferenceKind.TargetVariable, member.Member, Symbol(StorageSpace.Target, member.Member)),
        "param" => new(HixReferenceKind.Parameter, member.Member),
        _ => new(HixReferenceKind.Member, member.Member)
      } : new(HixReferenceKind.Member, member.Member);
    }

    protected override void VisitBlock(BlockStatementIr block) {
      // Jumps can revisit writes or skip them. Until a control-flow graph proves
      // otherwise, never specialize reads of those locals inside or after this block.
      var nodes = Descendants(block).ToArray();
      var jumps = nodes.OfType<ControlFlowStatementIr>().Any(flow =>
        flow.Operation is ControlFlowKind.Continue or ControlFlowKind.Goto or ControlFlowKind.Break);
      var assigned = jumps ? nodes.OfType<AssignmentStatementIr>().Where(value => value.Storage == StorageSpace.Local)
        .Select(value => value.Name).Distinct().ToArray() : Array.Empty<string>();
      var previous = uncertainLocals;
      uncertainLocals = new(previous, StringComparer.Ordinal);
      uncertainLocals.UnionWith(assigned);
      foreach (var name in assigned) locals[name] = HixPattern.Any;
      try { base.VisitBlock(block); }
      finally { uncertainLocals = previous; }
      foreach (var name in assigned) locals[name] = HixPattern.Any;
    }

    private static IEnumerable<HixIrNode> Descendants(HixIrNode node) {
      yield return node;
      foreach (var child in node.SemanticChildren)
        foreach (var nested in Descendants(child)) yield return nested;
    }

    protected override void VisitSelection(SelectionExpressionIr selection) {
      Visit(selection.Selector);
      var before = new Dictionary<string, HixPattern>(locals, StringComparer.Ordinal);
      var branches = new List<Dictionary<string, HixPattern>>();
      foreach (var branch in selection.Branches) {
        locals = new(before, StringComparer.Ordinal);
        foreach (var condition in branch.Conditions) Visit(condition);
        Visit(branch.Result);
        branches.Add(locals);
      }
      locals = new(before, StringComparer.Ordinal);
      Visit(selection.Fallback);
      branches.Add(locals);
      Merge(branches);
    }

    private void Merge(IEnumerable<Dictionary<string, HixPattern>> environments) {
      var branches = environments.ToArray();
      locals = branches.SelectMany(branch => branch.Keys).Distinct().ToDictionary(name => name,
        name => Union(branches.Select(branch => branch.TryGetValue(name, out var pattern) ? pattern : HixPattern.Any)),
        StringComparer.Ordinal);
    }

    protected override void VisitCall(CallExpressionIr call) {
      foreach (var argument in call.Arguments) Visit(argument);
      var arguments = call.Arguments;
      var argumentPatterns = arguments.Select(argument => argument.InferredPattern).ToArray();
      var selected = patterns.ContainsKey(call.Name) ? HixCallBinding.Pattern : HixCallBinding.Dynamic;
      if (functions.TryGetValue(call.Name, out var declarations)) {
        var hasDynamicFallback = declarations.Any(function => function.Signatures.Count == 0);
        var candidates = declarations.SelectMany(function => function.Signatures.Select(signature => (function, signature)))
          .Select(candidate => TryArrange(call, candidate.signature, out var arranged)
            ? (candidate.function, candidate.signature, arranged, relations:
              Relations(arranged.Select(InferBound).ToArray(), candidate.signature).ToArray())
            : (candidate.function, candidate.signature, arranged: (ExpressionIr[])null,
              relations: Array.Empty<HixPatternRelation>()))
          .Where(candidate => candidate.arranged != null)
          .Where(candidate => candidate.relations.All(relation => relation != HixPatternRelation.Never)).ToArray();
        if (candidates.Length != 0) {
          var score = candidates.Max(candidate => candidate.relations.Count(value => value == HixPatternRelation.Always));
          var best = candidates.Where(candidate => candidate.relations.Count(value => value == HixPatternRelation.Always) == score).ToArray();
          if (best.Length == 1 && (!hasDynamicFallback || best[0].relations.All(value => value == HixPatternRelation.Always))) {
            selected = HixCallBinding.Language(best[0].signature.Constant(call.Name), best[0].function);
            call.BoundArguments = best[0].arranged;
          }
        } else if (call.ArgumentNames.Any(name => name != null))
          diagnostics.Add(new(call.Line, "no overload of '" + call.Name + "' accepts the supplied named arguments"));
      } else {
        if (call.ArgumentNames.Any(name => name != null)) {
          diagnostics.Add(new(call.Line, "named arguments require a statically declared language function"));
          call.Binding = selected;
          return;
        }
        var candidates = backend.Functions.Resolve(call.Name, arguments.Count)
          .SelectMany(definition => definition.Signatures
            .Where(signature => signature.MatchesArgumentCount(arguments.Count))
            .Select(signature => (definition, signature, relations: argumentPatterns.Select((argument, index) =>
              HixPatternRelations.Relate(argument, KindPattern(signature.GetArgumentType(index)), patterns)).ToArray())))
          .Where(candidate => candidate.relations.All(relation => relation != HixPatternRelation.Never)).ToArray();
        if (candidates.Length != 0) {
          var score = candidates.Max(candidate => candidate.relations.Count(value => value == HixPatternRelation.Always));
          var best = candidates.Where(candidate => candidate.relations.Count(value => value == HixPatternRelation.Always) == score).ToArray();
          if (best.Length == 1) selected = HixCallBinding.Host(BuiltinSignature(call.Name, best[0].signature), best[0].definition);
        }
      }
      call.Binding = selected;
    }

    private HixPattern InferBound(ExpressionIr expression) {
      if (expression.InferredPattern == null) Visit(expression);
      return expression.InferredPattern ?? HixPattern.Any;
    }

    private bool TryArrange(CallExpressionIr call, FunctionSignature signature, out ExpressionIr[] arranged) {
      arranged = null;
      if (signature.Inputs == null) return !call.ArgumentNames.Any(name => name != null) &&
        (signature.InputPattern == null || call.Arguments.Count == 1) && (arranged = call.Arguments.ToArray()) != null;
      var fields = signature.Inputs;
      if (fields.Any(field => field.Variadic) || call.Arguments.Count > fields.Count) return false;
      var values = new ExpressionIr[fields.Count];
      var positional = 0;
      var sawNamed = false;
      for (var index = 0; index < call.Arguments.Count; index++) {
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
        if (values[target] != null) return false;
        values[target] = call.Arguments[index];
      }
      for (var index = 0; index < fields.Count; index++) {
        if (values[index] != null) continue;
        if (fields[index].DefaultValue != null)
          values[index] = new HixIrRewriter().Rewrite(fields[index].DefaultValue);
        else if (fields[index].Optional) values[index] = new NullExpressionIr();
        else return false;
      }
      arranged = values;
      return true;
    }

    private IEnumerable<HixPatternRelation> Relations(IReadOnlyList<HixPattern> supplied, FunctionSignature signature) {
      if (signature.Inputs == null) {
        if (signature.InputPattern == null) yield break;
        yield return supplied.Count == 1
          ? HixPatternRelations.Relate(supplied[0], signature.InputPattern, patterns) : HixPatternRelation.Never;
        yield break;
      }
      for (var index = 0; index < supplied.Count; index++) {
        var field = signature.Inputs[Math.Min(index, signature.Inputs.Count - 1)];
        yield return field.Optional && supplied[index] is KindHixPattern {ValueKind: HixValueKind.Null}
          ? HixPatternRelation.Always
          : HixPatternRelations.Relate(supplied[index], field.Pattern, patterns);
      }
    }

    private static bool AcceptsCount(FunctionSignature signature, int count) {
      if (signature.Inputs == null) return signature.InputPattern == null || count == 1;
      var required = signature.Inputs.Count(field => !field.Optional && !field.Variadic);
      return count >= required && (signature.Inputs.Any(field => field.Variadic) || count <= signature.Inputs.Count);
    }

    private static HixPattern KindPattern(HixValueKind kind) => kind == HixValueKind.Any
      ? HixPattern.Any : new KindHixPattern(kind);
    private static SignatureHixPattern BuiltinSignature(string name, global::Hix.FunctionSignature signature) =>
      new(name, signature.ArgumentTypes.Select((kind, index) =>
        new HixPatternField(index.ToString(), KindPattern(kind))).ToArray(), KindPattern(signature.ResultType));

    private HixPattern Infer(ExpressionIr expression) => expression switch {
      StringExpressionIr or InterpolationExpressionIr => new KindHixPattern(HixValueKind.String),
      NumberExpressionIr => new KindHixPattern(HixValueKind.Number),
      BooleanExpressionIr => new KindHixPattern(HixValueKind.Bool),
      NullExpressionIr => new KindHixPattern(HixValueKind.Null),
      TupleExpressionIr tuple => new TupleHixPattern(tuple.Values.Select((value, index) =>
        new HixPatternField(index.ToString(), value.InferredPattern)).ToArray()),
      TableExpressionIr table => new TableHixPattern(table.Entries.Select(entry =>
        new HixPatternField(entry.Key, entry.Value.InferredPattern)).ToArray()),
      CallExpressionIr {CoerceBoolean: true} => KindPattern(HixValueKind.Bool),
      CallExpressionIr call when patterns.ContainsKey(call.Name) => new NamedHixPattern(call.Name),
      CallExpressionIr {Binding.Signature: { } signature} => signature.Result,
      MemberExpressionIr {Receiver: RootExpressionIr {Name: "local"}} member when locals.TryGetValue(member.Member, out var local) => local,
      MemberExpressionIr {Receiver: RootExpressionIr {Name: "param"}} member =>
        parameters.FirstOrDefault(field => field.Name == member.Member)?.Pattern ?? HixPattern.Any,
      RootExpressionIr {Binding.Kind: HixReferenceKind.Local, Name: var localName} when locals.TryGetValue(localName, out var localPattern) => localPattern,
      InlineExpressionIr inline when locals.TryGetValue(inline.ResultLocal, out var inlineResult) => inlineResult,
      UnaryExpressionIr unary => unary.Operation == UnaryOperation.Not ? KindPattern(HixValueKind.Bool) : unary.Value.InferredPattern,
      RootExpressionIr {IsSmart: true, Name: var name} when int.TryParse(name, out var index) && index >= 0 && index < parameters.Count =>
        parameters[index].Pattern,
      FallbackExpressionIr fallback => Union([fallback.Value.InferredPattern, fallback.Fallback.InferredPattern]),
      SelectionExpressionIr selection => Union(selection.Branches.Select(branch => InferResult(branch.Result))
        .Concat([InferResult(selection.Fallback)])),
      _ => HixPattern.Any
    };

    private HixPattern InferResult(HixIrNode node) => node is ExpressionIr expression ? expression.InferredPattern :
      new KindHixPattern(HixValueKind.Null);
    private static HixPattern Union(IEnumerable<HixPattern> values) {
      return HixPatterns.Union(values);
    }
  }
}
