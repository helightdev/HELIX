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
      if (body == null) return;
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
        root.Name = HixUsingImports.Candidates(root.OriginalName, root.UsingImports)
          .FirstOrDefault(name => functions.ContainsKey(name) || backend.Functions.Resolve(name).Count != 0) ?? root.Name;
        if (functions.TryGetValue(root.Name, out var declarations) && declarations.All(function => function.IsExternal))
          diagnostics.Add(new(root.Line, "External function '" + root.Name + "' has no implementation."));
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
      if (functions.TryGetValue(call.Name, out var declarations) && declarations.All(function => function.IsExternal)) {
        diagnostics.Add(new(call.Line, "External function '" + call.Name + "' has no implementation."));
        return;
      }
      if (call.DispatchConstraint is { } constraint) {
        if (call.ArgumentNames.Any(name => name != null)) {
          diagnostics.Add(new(call.Line, "explicit dispatch requires positional arguments")); return;
        }
        if (constraint.Kind == HixCallKind.Dynamic) { call.Binding = HixCallBinding.Dynamic; return; }
        if (constraint.Kind != HixCallKind.Static || constraint.Signature == null) {
          diagnostics.Add(new(call.Line, "invalid explicit call dispatch")); return;
        }
        var bindings = new List<HixCallBinding>();
        if (functions.TryGetValue(call.Name, out var available))
          foreach (var function in available)
            foreach (var signature in function.Signatures)
              if (signature.Constant(call.Name).Display == constraint.Signature && AcceptsCount(signature, call.Arguments.Count))
                if (bindings.Count == 0) bindings.Add(HixCallBinding.Language(signature.Constant(call.Name), function));
        if (bindings.Count == 0)
          foreach (var definition in backend.Functions.Resolve(call.Name, call.Arguments.Count))
            foreach (var signature in definition.Signatures)
              if (BuiltinSignature(call.Name, signature).Display == constraint.Signature)
                bindings.Add(HixCallBinding.Host(BuiltinSignature(call.Name, signature), definition));
        if (bindings.Count != 1) diagnostics.Add(new(call.Line, "cannot bind explicit signature '" + constraint.Signature + "'"));
        else call.Binding = bindings[0];
        return;
      }
      var arguments = call.Arguments;
      var argumentPatterns = arguments.Select(argument => argument.InferredPattern).ToArray();
      var selected = HixCallBinding.Dynamic;
      if (patterns.TryGetValue(call.Name, out var declaredPattern)) {
        var methods = functions.TryGetValue(call.Name, out var namedMethods) ? namedMethods
          .Where(method => method.Signatures.Any(signature => signature.OutputPattern is NamedHixPattern named && named.Name == call.Name))
          .ToArray() : [];
        var identityMethod = methods.FirstOrDefault(method => method.Signatures.Any(signature => signature.Inputs == null));
        var constructorMethod = methods.FirstOrDefault(method => method.Signatures.Any(signature => signature.Inputs != null));
        var identityPattern = identityMethod?.Signatures[0].InputPattern ?? declaredPattern;
        var relation = call.Arguments.Count == 1
          ? HixPatternRelations.Relate(argumentPatterns[0], identityPattern, patterns) : HixPatternRelation.Never;
        if (call.ArgumentNames.All(name => name == null) && call.Arguments.Count == 1 &&
            relation != HixPatternRelation.Never) {
          call.Binding = identityMethod == null || relation == HixPatternRelation.Maybe && constructorMethod != null
            ? HixCallBinding.Dynamic
            : HixCallBinding.Language(identityMethod.Signatures[0].Constant(call.Name), identityMethod);
          return;
        }
        if (constructorMethod != null && TryArrange(call, constructorMethod.Signatures[0], out var arranged)) {
          call.BoundArguments = arranged;
          call.Binding = HixCallBinding.Language(constructorMethod.Signatures[0].Constant(call.Name), constructorMethod);
          return;
        }
        if (call.ArgumentNames.Any(name => name != null) || call.Arguments.Count != 1) {
          if (HixPatterns.TryStructural(declaredPattern, patterns, out var mismatch))
            diagnostics.Add(new(call.Line, PatternConstructorDiagnostics.Describe(call.Name, call.ArgumentNames,
              argumentPatterns, mismatch is TableHixPattern mismatchTable ? mismatchTable.Fields :
                ((TupleHixPattern)mismatch).Fields, patterns)));
          else diagnostics.Add(new(call.Line, "pattern '" + call.Name + "' only accepts " + call.Name + "(value: " +
            call.Name + ")"));
        }
        call.Binding = identityMethod == null ? HixCallBinding.Dynamic :
          HixCallBinding.Language(identityMethod.Signatures[0].Constant(call.Name), identityMethod);
        return;
      }
      var resolution = HixCallResolver.Resolve(call, functions, backend, patterns, InferBound, out var ambiguous);
      call.Resolution = resolution;
      if (resolution != null) {
        selected = resolution.Binding;
        call.BoundArguments = resolution.Arguments;
      } else if (ambiguous && call.Arguments.All(argument => argument.InferredPattern is not AnyHixPattern))
        diagnostics.Add(new(call.Line, "ambiguous overload of '" + call.Name + "'"));
      else if (call.ArgumentNames.Any(name => name != null))
        diagnostics.Add(new(call.Line, "no overload of '" + call.Name + "' accepts the supplied named arguments"));
      if (functions.TryGetValue(call.Name, out var resolvedDeclarations) && resolvedDeclarations.All(function => function.IsExternal))
        diagnostics.Add(new(call.Line, "External function '" + call.Name + "' has no implementation."));
      call.Binding = selected;
      if (call.UsingImports.Count != 0 && InPureScope(call)) {
        var impure = selected.Function is {IsPure: false} || selected.Backend is {HasEffects: true} ||
          selected.Kind == HixCallKind.Dynamic &&
          (functions.TryGetValue(call.Name, out var possibleFunctions) && possibleFunctions.Any(function => !function.IsPure) ||
           backend.Functions.Resolve(call.Name).Any(function => function.HasEffects));
        if (impure) diagnostics.Add(new(call.Line, "pure functions cannot invoke impure functions"));
      }
    }

    private static bool InPureScope(HixIrNode node) {
      for (var parent = node.Parent; parent != null; parent = parent.Parent) {
        if (parent is LambdaExpressionIr) return true;
        if (parent is FunctionDeclarationIr function) return function.IsPure;
      }
      return false;
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
        else if (fields[index].Optional)
          values[index] = new MissingExpressionIr {InferredPattern = new KindHixPattern(HixValueKind.Missing)};
        else return false;
      }
      arranged = values;
      return true;
    }

    private static bool AcceptsCount(FunctionSignature signature, int count) {
      if (signature.Inputs == null) return signature.InputPattern == null || count == 1;
      var required = signature.Inputs.Count(field => !field.AllowsMissing && !field.Variadic);
      return count >= required && (signature.Inputs.Any(field => field.Variadic) || count <= signature.Inputs.Count);
    }

    private static HixPattern KindPattern(HixValueKind kind) => kind == HixValueKind.Any
      ? HixPattern.Any : new KindHixPattern(kind);
    private static SignatureHixPattern BuiltinSignature(string name, global::Hix.FunctionSignature signature) =>
      new(name, signature.ArgumentTypes.Select((kind, index) =>
        new HixPatternField(signature.GetArgumentName(index) ?? index.ToString(), KindPattern(kind),
          DefaultValue: signature.GetArgumentDefault(index),
          HasDefault: signature.GetArgumentDefault(index) != null)).ToArray(), KindPattern(signature.ResultType));

    private HixPattern Infer(ExpressionIr expression) => expression switch {
      StringExpressionIr or InterpolationExpressionIr => new KindHixPattern(HixValueKind.String),
      NumberExpressionIr => new KindHixPattern(HixValueKind.Number),
      BooleanExpressionIr => new KindHixPattern(HixValueKind.Bool),
      NullExpressionIr => new KindHixPattern(HixValueKind.Null),
      MissingExpressionIr => new KindHixPattern(HixValueKind.Missing),
      TupleExpressionIr tuple => new TupleHixPattern(tuple.Values.Select((value, index) =>
        new HixPatternField(index.ToString(), value.InferredPattern)).ToArray()),
      TableExpressionIr table => new TableHixPattern(table.Entries.Select(entry =>
        new HixPatternField(entry.Key, entry.Value.InferredPattern)).ToArray()),
      CallExpressionIr {CoerceBoolean: true} => KindPattern(HixValueKind.Bool),
      CallExpressionIr call when patterns.ContainsKey(call.Name) => new NamedHixPattern(call.Name),
      CallExpressionIr {Resolution: { } resolution} => resolution.Signature.Result,
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
      RootExpressionIr {IsSmart: false, Name: var functionName} when functions.TryGetValue(functionName, out var methods) => Callable(methods),
      RootExpressionIr root when backend.Roots.TryGetValue(root.Name, out var hostRoot) => hostRoot.Pattern,
      LambdaExpressionIr lambda => new DelegateHixPattern([], InferResult(lambda.Body)),
      _ => HixPattern.Any
    };

    private static HixPattern Callable(FunctionDeclarationIr[] methods) {
      var method = methods.FirstOrDefault();
      var signature = method?.Signatures.FirstOrDefault();
      if (signature != null) return new DelegateHixPattern(signature.Constant(method.Name).Parameters,
        signature.Constant(method.Name).Result);
      return new DelegateHixPattern([], HixPattern.Any);
    }

    private HixPattern InferResult(HixIrNode node) => node switch {
      ExpressionIr expression => expression.InferredPattern,
      BlockStatementIr block => HixPatterns.Union(block.Statements.OfType<ControlFlowStatementIr>()
        .Where(value => value.Operation == ControlFlowKind.Return).SelectMany(value => value.Values)
        .Select(value => value.InferredPattern)),
      _ => new KindHixPattern(HixValueKind.Null)
    };
    private static HixPattern Union(IEnumerable<HixPattern> values) {
      return HixPatterns.Union(values);
    }
  }
}
