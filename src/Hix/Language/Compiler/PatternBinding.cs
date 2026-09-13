using System;
using System.Collections.Generic;
using System.Linq;

namespace Hix.Compiler;

/// <summary>Shared source-pattern inference, generic overload selection, and executable IR binding.</summary>
public static class PatternBinding {
  private static HixPattern Primitive(HixValueKind kind) => HixPatterns.Primitive(kind).Pattern;

  // Value-mode primitive names are patterns; null and missing remain singleton values.
  internal static HixPattern RootValuePattern(string name,
    IReadOnlyDictionary<string, FunctionDeclarationIr[]> functions,
    IReadOnlyDictionary<string, HixPattern> patterns, HixBackend backend) {
    if (name == "null") return Primitive(HixValueKind.Null);
    if (name == "missing") return Primitive(HixValueKind.Missing);
    if (HixPatterns.TryPrimitive(name, out var primitive)) return new PatternValueHixPattern(primitive.Pattern);
    if (patterns.ContainsKey(name)) return new PatternValueHixPattern(new NamedHixPattern(name));
    if (functions.TryGetValue(name, out var methods)) return CallablePattern(methods);
    if (backend.Functions.Resolve(name).Count != 0) return Primitive(HixValueKind.Function);
    return backend.Roots.TryGetValue(name, out var root) ? root.Pattern : HixPattern.Any;
  }

  private static DelegateHixPattern CallablePattern(FunctionDeclarationIr[] methods) {
    var method = methods.FirstOrDefault();
    var signature = method?.Signatures.FirstOrDefault()?.Constant(method.Name);
    return signature == null ? new DelegateHixPattern([], HixPattern.Any) :
      new DelegateHixPattern(signature.Parameters, signature.Result);
  }

  // Executable IR: local scopes and control flow.
  public static void Bind(HixCompilation compilation) {
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
    private HixPattern parameterValue = HixPattern.Any;
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
      InScope(inputParameters, expression.Body, HixPattern.Any);

    protected override void VisitFunction(FunctionDeclarationIr function) =>
      InScope(function.Signatures.Count == 1 ? function.Signatures[0].Inputs ?? [] : [], function.Body,
        function.Signatures.Count == 1 ? function.Signatures[0].InputPattern ??
          (function.Signatures[0].Inputs == null ? HixPattern.Any : null) : HixPattern.Any);

    private void InScope(IReadOnlyList<SignatureField> fields, BlockStatementIr body, HixPattern collective = null) {
      if (body == null) return;
      var previousLocals = locals; var previousParameters = parameters; var previousParameterValue = parameterValue;
      var previousSymbols = symbols;
      var previousDeclaredLocals = declaredLocals;
      declaredLocals = new(Descendants(body).OfType<AssignmentStatementIr>()
        .Where(assignment => assignment.Storage == StorageSpace.Local && assignment.IsDeclaration)
        .Select(assignment => assignment.Name), StringComparer.Ordinal);
      locals = new(StringComparer.Ordinal); parameters = fields; symbols = new();
      parameterValue = collective ?? new TableHixPattern(fields.Select(field => field.AsPatternField() with {
        Pattern = field.Variadic ? new ManyHixPattern(field.Pattern) : field.Pattern
      }).ToArray());
      foreach (var assignment in Descendants(body).OfType<AssignmentStatementIr>())
        assignment.Symbol = Symbol(assignment.Storage, assignment.Name);
      try { Visit(body); }
      finally { locals = previousLocals; parameters = previousParameters; parameterValue = previousParameterValue; symbols = previousSymbols; declaredLocals = previousDeclaredLocals; }
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
          .FirstOrDefault(name => functions.ContainsKey(name) || patterns.ContainsKey(name) || backend.Functions.Resolve(name).Count != 0) ?? root.Name;
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
      // otherwise, never specialize locals with multiple writes inside or after this block.
      // A local written only by its declaration keeps the initializer type.
      var nodes = FlowDescendants(block).ToArray();
      var jumps = nodes.OfType<ControlFlowStatementIr>().Any(flow =>
        flow.Operation is ControlFlowKind.Continue or ControlFlowKind.Goto or ControlFlowKind.Break);
      var assigned = jumps ? nodes.OfType<AssignmentStatementIr>().Where(value => value.Storage == StorageSpace.Local)
        .GroupBy(value => value.Name).Where(group => group.Any(value => !value.IsDeclaration) || group.Count() > 1)
        .Select(group => group.Key).ToArray() : Array.Empty<string>();
      var previous = uncertainLocals;
      uncertainLocals = new(previous, StringComparer.Ordinal);
      uncertainLocals.UnionWith(assigned);
      foreach (var name in assigned) locals[name] = HixPattern.Any;
      try { base.VisitBlock(block); }
      finally { uncertainLocals = previous; }
      foreach (var name in assigned) locals[name] = HixPattern.Any;
    }

    private static IEnumerable<HixIrNode> FlowDescendants(HixIrNode node) {
      yield return node;
      // Inline return jumps stay inside their own block and cannot revisit outer writes.
      if (node is InlineExpressionIr) yield break;
      foreach (var child in node.SemanticChildren)
        foreach (var nested in FlowDescendants(child)) yield return nested;
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
        if (constraint.Kind != HixCallKind.Static || constraint.Signature == null) {
          diagnostics.Add(new(call.Line, "invalid explicit call dispatch")); return;
        }
        var bindings = new List<HixCallBinding>();
        if (functions.TryGetValue(call.Name, out var available))
          foreach (var function in available)
            foreach (var signature in function.Signatures)
              if ((HixGenericTypes.SignatureIdentity(signature.Constant(call.Name)) == constraint.Signature || signature.Constant(call.Name).Display == constraint.Signature) && AcceptsCount(signature, call.Arguments.Count))
                if (bindings.Count == 0) bindings.Add(HixCallBinding.Language(signature.Constant(call.Name), function));
        if (bindings.Count == 0)
          foreach (var definition in backend.Functions.Resolve(call.Name, call.Arguments.Count))
            foreach (var signature in definition.Signatures)
              if ((HixGenericTypes.SignatureIdentity(BuiltinSignature(call.Name, signature)) == constraint.Signature || BuiltinSignature(call.Name, signature).Display == constraint.Signature))
                bindings.Add(HixCallBinding.Host(BuiltinSignature(call.Name, signature), definition));
        if (bindings.Count != 1) diagnostics.Add(new(call.Line, "cannot bind explicit signature '" + constraint.Signature + "'"));
        else call.Binding = bindings[0];
        return;
      }
      var selected = HixCallBinding.Unbound;
      var resolution = PatternBinding.Resolve(call, functions, backend, patterns, InferBound, out var ambiguous);
      call.Resolution = resolution;
      if (resolution != null) {
        selected = resolution.Binding;
        call.BoundArguments = resolution.Arguments;
      } else if (ambiguous)
        diagnostics.Add(new(call.Line, "ambiguous overload of '" + call.Name + "'"));
      else if (call.ArgumentNames.Any(name => name != null))
        diagnostics.Add(new(call.Line, "no overload of '" + call.Name + "' accepts the supplied named arguments"));
      if (selected.Kind == HixCallKind.Unbound && !ambiguous && !call.ArgumentNames.Any(name => name != null))
        diagnostics.Add(new(call.Line, "cannot bind signature of '" + call.Name + "'; ordinary calls require a declared matching signature"));
      if (functions.TryGetValue(call.Name, out var resolvedDeclarations) && resolvedDeclarations.All(function => function.IsExternal))
        diagnostics.Add(new(call.Line, "External function '" + call.Name + "' has no implementation."));
      call.Binding = selected;
      if (call.UsingImports.Count != 0 && InPureScope(call)) {
        var impure = selected.Function is {IsPure: false} || selected.Backend is {HasEffects: true};
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

    private static bool AcceptsCount(FunctionSignature signature, int count) {
      if (signature.Inputs == null) return signature.InputPattern == null || count == 1;
      var required = signature.Inputs.Count(field => !field.AllowsMissing && !field.Variadic);
      return count >= required && (signature.Inputs.Any(field => field.Variadic) || count <= signature.Inputs.Count);
    }

    private static SignatureHixPattern BuiltinSignature(string name, global::Hix.FunctionSignature signature) =>
      new(name, signature.ArgumentPatterns.Select((pattern, index) =>
        new HixPatternField(signature.GetArgumentName(index) ?? index.ToString(), Primitive(HixGenericTypes.Erase(pattern)),
          DefaultValue: signature.GetArgumentDefault(index),
          HasDefault: signature.GetArgumentDefault(index) != null)).ToArray(), Primitive(HixGenericTypes.Erase(signature.ResultPattern)));

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
      CallExpressionIr {CoerceBoolean: true} => Primitive(HixValueKind.Bool),
      CallExpressionIr {Resolution: { } resolution} => resolution.Signature.Result,
      CallExpressionIr {Binding.Signature: { } signature} => signature.Result,
      CallExpressionIr call when patterns.ContainsKey(call.Name) => new NamedHixPattern(call.Name),
      MemberExpressionIr {Receiver: RootExpressionIr {Name: "local"}} member when locals.TryGetValue(member.Member, out var local) => local,
      MemberExpressionIr {Receiver: RootExpressionIr {Name: "param"}} member =>
        parameters.FirstOrDefault(field => field.Name == member.Member)?.Pattern ?? HixPattern.Any,
      MemberExpressionIr member => PatternBinding.Member(member.Receiver.InferredPattern ?? HixPattern.Any, member.Member, patterns),
      RootExpressionIr {Binding.Kind: HixReferenceKind.Local, Name: var localName} when locals.TryGetValue(localName, out var localPattern) => localPattern,
      InlineExpressionIr {ResultPattern: not null} inline => inline.ResultPattern,
      InlineExpressionIr inline when locals.TryGetValue(inline.ResultLocal, out var inlineResult) => inlineResult,
      UnaryExpressionIr unary => unary.Operation == UnaryOperation.Not ? Primitive(HixValueKind.Bool) : unary.Value.InferredPattern,
      RootExpressionIr {Binding.Kind: HixReferenceKind.Parameter, Binding.ParameterIndex: var parameterIndex} when parameterIndex >= 0 && parameterIndex < parameters.Count =>
        parameters[parameterIndex].Pattern,
      RootExpressionIr {IsSmart: true, Name: var name} when int.TryParse(name, out var index) && index >= 0 && index < parameters.Count =>
        parameters[index].Pattern,
      FallbackExpressionIr fallback => Union([fallback.Value.InferredPattern, fallback.Fallback.InferredPattern]),
      SelectionExpressionIr selection => Union(selection.Branches.Select(branch => InferResult(branch.Result))
        .Concat([InferResult(selection.Fallback)])),
      RootExpressionIr {IsSmart: false, Name: "param"} => parameterValue,
      RootExpressionIr {IsSmart: true, Name: "it"} => parameterValue,
      RootExpressionIr {IsSmart: false, Name: "args"} => new TupleHixPattern(parameters.Select(field => field.AsPatternField()).ToArray()),
      RootExpressionIr {IsSmart: false, Name: "local" or "var" or "tar"} => HixPattern.Any,
      RootExpressionIr {IsSmart: false} root => RootValuePattern(root.Name, functions, patterns, backend),
      LambdaExpressionIr lambda => new DelegateHixPattern([], InferResult(lambda.Body)),
      _ => HixPattern.Any
    };

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

  // Calls: argument arrangement, generic inference, and VM signature projection.
  public static HixCallResolution Resolve(CallExpressionIr call,
    IReadOnlyDictionary<string, FunctionDeclarationIr[]> functions, HixBackend backend,
    IReadOnlyDictionary<string, HixPattern> patterns, Func<ExpressionIr, HixPattern> infer,
    out bool ambiguous) {
    if (call.UsingImports.Count != 0) {
      foreach (var name in HixUsingImports.Candidates(call.OriginalName, call.UsingImports).Distinct()) {
        var qualified = new CallExpressionIr(name, call.Arguments, call.CoerceBoolean,
          argumentNames: call.ArgumentNames) { TypeArguments = call.TypeArguments };
        var resolution = Resolve(qualified, functions, backend, patterns, infer, out ambiguous);
        if (resolution != null || ambiguous) { call.Name = name; return resolution; }

      }
      ambiguous = false;
      return null;
    }
    var candidates = new List<HixCallResolution>();
    var patternFallbacks = new List<Action>();
    var languageSignatures = new HashSet<string>(StringComparer.Ordinal);
    if (functions.TryGetValue(call.Name, out var declarations)) {
      foreach (var declaration in declarations)
        foreach (var signature in declaration.Signatures) {
          if (signature.Inputs == null && signature.InputPattern == null) continue;
          var key = HixGenericTypes.SignatureIdentity(signature.Constant(call.Name) with { Result = HixPattern.Any });
          if (!languageSignatures.Add(key)) continue;
          var defaults = signature.Inputs?.Select(field => field.DefaultValue ??
            (field.Optional ? new MissingExpressionIr() : null)).ToArray();
          Add(signature.Constant(call.Name), declaration.TypeParameters.Concat(signature.TypeParameters).Distinct().ToArray(), defaults,
            signature.Inputs?.Any(field => field.Variadic) == true, declaration, null, null, null);
        }
    } else foreach (var definition in backend.Functions.Resolve(call.Name))
      foreach (var signature in definition.Signatures)
        Add(new SignatureHixPattern(call.Name, signature.ArgumentPatterns.Select((pattern, index) =>
          new HixPatternField(signature.GetArgumentName(index) ?? index.ToString(), pattern,
            HasDefault: signature.GetArgumentDefault(index) != null)).ToArray(), signature.ResultPattern),
          signature.TypeParameters, signature.ArgumentDefaults, signature.IsVariadic, null, definition, signature.InferResult,
          signature.CallableArguments);
    if (!candidates.Any(candidate => candidate.ExactMatch))
      foreach (var fallback in patternFallbacks) fallback();
    ambiguous = false;
    if (candidates.Count == 0) return null;
    var ordered = candidates.OrderByDescending(candidate => candidate.Score).ThenBy(candidate => candidate.Generic).ToArray();
    var best = ordered[0];
    ambiguous = ordered.Skip(1).Any(candidate => candidate.Score == best.Score && candidate.Generic == best.Generic);
    return ambiguous ? null : best;

    void Add(SignatureHixPattern signature, IReadOnlyList<HixTypeParameter> parameters,
      IReadOnlyList<ExpressionIr> defaults, bool variadic, FunctionDeclarationIr language,
      HixFunctionEntry host, Func<IReadOnlyList<HixPattern>, HixPattern> inferResult,
      IReadOnlyDictionary<int, int> callableArguments) {
      TryAdd(signature, parameters, defaults, variadic, language, host, inferResult, callableArguments, false);
      patternFallbacks.Add(() => TryAdd(signature, parameters, defaults, variadic, language, host, inferResult, callableArguments, true));
    }

    void TryAdd(SignatureHixPattern signature, IReadOnlyList<HixTypeParameter> parameters,
      IReadOnlyList<ExpressionIr> defaults, bool variadic, FunctionDeclarationIr language,
      HixFunctionEntry host, Func<IReadOnlyList<HixPattern>, HixPattern> inferResult,
      IReadOnlyDictionary<int, int> callableArguments, bool convertPatternNames) {
      var arrangedCall = call;
      if (language != null && patterns.ContainsKey(call.Name.EndsWith(".new", StringComparison.Ordinal)
            ? call.Name.Substring(0, call.Name.Length - 4) : call.Name) &&
          call.ArgumentNames.Any(name => name != null && !signature.Parameters.Any(field => field.Name == name) &&
            signature.Parameters.Any(field => field.Name == "$" + name))) {
        arrangedCall = new CallExpressionIr(call.Name, call.Arguments, argumentNames: call.ArgumentNames.Select(name =>
          name != null && !signature.Parameters.Any(field => field.Name == name) &&
            signature.Parameters.Any(field => field.Name == "$" + name) ? "$" + name : name).ToArray());
      }
      if (!Arrange(arrangedCall, signature.Parameters, defaults, variadic, out var arguments)) return;
      if (call.Name == "new" && arguments.FirstOrDefault() is RootExpressionIr traitTarget &&
          HixPatterns.TryTrait(new NamedHixPattern(traitTarget.Name), patterns, out _)) return;
      if (call.TypeArguments.Count != 0 && call.TypeArguments.Count != parameters.Count) return;
      var substitutions = new Dictionary<HixTypeParameter, HixPattern>();
      for (var index = 0; index < call.TypeArguments.Count; index++) substitutions[parameters[index]] = call.TypeArguments[index];
      var supplied = arguments.Select(infer).ToArray();
      var converted = new List<int>();
      if (convertPatternNames) {
        for (var index = 0; index < arguments.Length; index++) {
          var expected = signature.Parameters[Math.Min(index, signature.Parameters.Count - 1)].Pattern;
          if (!AcceptsPatternValue(expected) || arguments[index] is not StringExpressionIr literal ||
              HixPatternRelations.Relate(supplied[index], HixGenericTypes.ErasePattern(ResolvePattern(expected, patterns)), patterns) == HixPatternRelation.Always) continue;
          var name = HixUsingImports.Candidates(literal.Value, literal.UsingImports)
            .FirstOrDefault(candidate => patterns.ContainsKey(candidate) || HixPatterns.TryPrimitive(candidate, out _));
          if (name == null) continue;
          var represented = HixPatterns.TryPrimitive(name, out var primitive) ? primitive.Pattern : new NamedHixPattern(name);
          var witness = new PatternValueHixPattern(represented);
          arguments[index] = new RootExpressionIr(name) {
            Binding = new HixReferenceBinding(HixReferenceKind.Root, name), InferredPattern = witness,
            SourceRange = literal.SourceRange, Tokens = literal.Tokens, UsingImports = literal.UsingImports
          };
          supplied[index] = witness;
          converted.Add(index);
        }
        if (converted.Count == 0) return;
      }
      if (call.Name == "new" && supplied.FirstOrDefault() is PatternValueHixPattern constructionTarget &&
          HixPatterns.TryTrait(constructionTarget.ValuePattern, patterns, out _)) return;
      // Infer data arguments first so callable parameters receive their concrete context.
      for (var index = 0; index < supplied.Length; index++) {
        var expected = ResolvePattern(signature.Parameters[Math.Min(index, signature.Parameters.Count - 1)].Pattern, patterns);
        if (expected is not DelegateHixPattern && !Unify(expected, supplied[index], substitutions, patterns)) return;
      }
      ComputedGenericPath.Infer(parameters, signature.Parameters, supplied, substitutions, patterns);
      var invocationArguments = ComputedGenericPath.Invocations(parameters, signature.Parameters);
      for (var index = 0; index < arguments.Length; index++) {
        var expected = ResolvePattern(signature.Parameters[Math.Min(index, signature.Parameters.Count - 1)].Pattern, patterns);
        if (expected is not DelegateHixPattern callable) continue;
        var context = HixGenericTypes.Substitute(callable, substitutions, true) as DelegateHixPattern;
        TupleHixPattern invocation = null;
        if (invocationArguments.TryGetValue(index, out var tupleIndex) || callableArguments != null && callableArguments.TryGetValue(index, out tupleIndex)) {
          invocation = supplied[tupleIndex] as TupleHixPattern;
          if (invocation != null) context = new DelegateHixPattern(invocation.Fields, context.Result);
        }
        supplied[index] = PatternBinding.InferCallable(arguments[index], context, functions, patterns, backend,
          out var validCallable, invocationContext: invocation != null) ?? supplied[index];
        if (!validCallable) return;
        if (invocation != null && supplied[index] is DelegateHixPattern actualCallable &&
            (actualCallable.Parameters.Count != invocation.Fields.Count || actualCallable.Parameters.Select((field, position) =>
              HixPatternRelations.Relate(invocation.Fields[position].Pattern, field.Pattern, patterns)).Any(relation => relation == HixPatternRelation.Never))) return;
      }
      ComputedGenericPath.Infer(parameters, signature.Parameters, supplied, substitutions, patterns);
      for (var index = 0; index < supplied.Length; index++) {
        var expected = signature.Parameters[Math.Min(index, signature.Parameters.Count - 1)].Pattern;
        if (!Unify(expected, supplied[index], substitutions, patterns)) return;
      }
      ComputedGenericPath.Infer(parameters, signature.Parameters, supplied, substitutions, patterns);
      foreach (var parameter in parameters)
        if (substitutions.TryGetValue(parameter, out var value) &&
            !HixGenericTypes.SatisfiesBounds(value, parameter, substitutions, patterns)) return;
      var instantiated = new SignatureHixPattern(signature.Name, signature.Parameters.Select(field => field with {
        Pattern = HixGenericTypes.Substitute(field.Pattern, substitutions, true) }).ToArray(),
        HixGenericTypes.Substitute(signature.Result, substitutions, true));
      if (inferResult != null) instantiated = instantiated with { Result = inferResult(supplied) };
      var relations = supplied.Select((value, index) => HixPatternRelations.Relate(value,
        instantiated.Parameters[Math.Min(index, instantiated.Parameters.Count - 1)].Pattern, patterns)).ToArray();
      if (relations.Any(relation => relation == HixPatternRelation.Never)) return;
      var erased = new SignatureHixPattern(signature.Name, signature.Parameters.Select((field, fieldIndex) => field with {
        Pattern = host != null ? Primitive(HixGenericTypes.Erase(field.Pattern)) : HixGenericTypes.ErasePattern(field.Pattern),
        DefaultValue = host != null && defaults != null && fieldIndex < defaults.Count ?
          ConstantData(defaults[fieldIndex]) : field.DefaultValue,
        HasDefault = host == null ? field.HasDefault : defaults != null && fieldIndex < defaults.Count &&
          defaults[fieldIndex] != null
      }).ToArray(), host != null ? Primitive(HixGenericTypes.Erase(signature.Result)) : HixGenericTypes.ErasePattern(signature.Result));
      var binding = language != null ? HixCallBinding.Language(erased, language) : HixCallBinding.Host(erased, host);
      candidates.Add(new(instantiated, substitutions, arguments, binding,
        relations.Count(relation => relation == HixPatternRelation.Always) * (supplied.Length + 1) +
        supplied.Where((value, index) => HixGenericTypes.Equivalent(value,
          instantiated.Parameters[Math.Min(index, instantiated.Parameters.Count - 1)].Pattern)).Count(), parameters.Count != 0) {
        DeclarationSignature = parameters.Count != 0 || signature.Parameters.Any(field => HixGenericTypes.RequiresErasure(field.Pattern)) ||
          HixGenericTypes.RequiresErasure(signature.Result) ? HixGenericTypes.SignatureIdentity(signature) : null,
        ExactMatch = converted.Count == 0 && relations.All(relation => relation == HixPatternRelation.Always),
        PatternConversions = converted.ToArray()
      });
    }

    bool AcceptsPatternValue(HixPattern expected) {
      expected = ResolvePattern(expected, patterns);
      return expected switch {
        KindHixPattern {ValueKind: HixValueKind.Pattern} or PatternValueHixPattern => true,
        TypeParameterHixPattern parameter => parameter.Parameter.Bound != null && AcceptsPatternValue(parameter.Parameter.Bound) ||
          parameter.Parameter.AdditionalBounds.Any(AcceptsPatternValue),
        UnionHixPattern union => union.Patterns.Any(AcceptsPatternValue),
        _ => false
      };
    }
  }


  private static HixPattern ResolvePattern(HixPattern pattern, IReadOnlyDictionary<string, HixPattern> definitions) {
    pattern = PatternComposition.Resolve(pattern, definitions);
    var active = new HashSet<string>(StringComparer.Ordinal);
    while (true) {
      if (pattern is DocumentedHixPattern documented) { pattern = documented.Underlying; continue; }
      if (pattern is NamedHixPattern named && active.Add(named.Name) && definitions.TryGetValue(named.Name, out var resolved)) {
        pattern = resolved; continue;
      }
      return pattern;
    }
  }

  public static bool Unify(HixPattern expected, HixPattern actual,
    IDictionary<HixTypeParameter, HixPattern> substitutions, IReadOnlyDictionary<string, HixPattern> patterns) {
    var referencedPattern = actual;
    expected = ResolvePattern(expected, patterns);
    actual = ResolvePattern(actual, patterns);
    if (actual is AnyHixPattern) return true;
    if (expected is KindHixPattern {ValueKind: HixValueKind.Pattern} &&
        HixGenericTypes.Erase(actual) is not (HixValueKind.Pattern or HixValueKind.Any)) return false;
    if (expected is TypeParameterHixPattern parameter) {
      if (parameter.Parameter.ComputedPath != null)
        return !substitutions.TryGetValue(parameter.Parameter, out var computed) ||
          HixPatternRelations.Relate(actual, ResolvePattern(computed, patterns), patterns) != HixPatternRelation.Never;
      if (!substitutions.TryGetValue(parameter.Parameter, out var existing)) {
        substitutions[parameter.Parameter] = referencedPattern is NamedHixPattern ? referencedPattern : actual; return true;
      }
      return HixGenericTypes.Equivalent(ResolvePattern(existing, patterns), actual);
    }
    if (expected is OpaqueHixPattern opaque && actual is OpaqueHixPattern supplied)
      return opaque.Definition.Identity == supplied.Definition.Identity && opaque.Arguments.Count == supplied.Arguments.Count &&
        opaque.Arguments.Zip(supplied.Arguments, (a, b) => Unify(a, b, substitutions, patterns)).All(value => value);
    if (expected is ManyHixPattern many && actual is TupleHixPattern tuple)
      return Unify(many.Element, tuple.Fields.Count == 0 ? HixPattern.Any : HixPatterns.Union(tuple.Fields.Select(field => field.Pattern)), substitutions, patterns);
    if (expected is ManyHixPattern expectedMany && actual is ManyHixPattern actualMany)
      return Unify(expectedMany.Element, actualMany.Element, substitutions, patterns);
    if (expected is TupleHixPattern expectedTuple && actual is TupleHixPattern actualTuple)
      return expectedTuple.Fields.Count == actualTuple.Fields.Count && expectedTuple.Fields.Zip(actualTuple.Fields,
        (a, b) => Unify(a.Pattern, b.Pattern, substitutions, patterns)).All(value => value);
    if (expected is TableHixPattern expectedTable && actual is TableHixPattern actualTable)
      return expectedTable.Fields.All(field => actualTable.Fields.FirstOrDefault(value => value.Name == field.Name) is { } found ?
        Unify(field.Pattern, found.Pattern, substitutions, patterns) : field.AllowsMissing);
    if (expected is MapHixPattern tableMap && actual is TableHixPattern mappedTable)
      return Unify(tableMap.Key, Primitive(HixValueKind.String), substitutions, patterns) &&
        Unify(tableMap.Value, mappedTable.Fields.Count == 0 ? HixPattern.Any : HixPatterns.Union(mappedTable.Fields.Select(field => field.Pattern)), substitutions, patterns);
    if (expected is MapHixPattern map && actual is MapHixPattern actualMap)
      return Unify(map.Key, actualMap.Key, substitutions, patterns) && Unify(map.Value, actualMap.Value, substitutions, patterns);
    if (expected is DelegateHixPattern callable && actual is DelegateHixPattern actualCallable)
      return (callable.Parameters.Count == 0 || callable.Parameters.Count == actualCallable.Parameters.Count &&
        callable.Parameters.Zip(actualCallable.Parameters, (a, b) => Unify(a.Pattern, b.Pattern, substitutions, patterns)).All(value => value)) &&
        Unify(callable.Result, actualCallable.Result, substitutions, patterns);
    if (expected is UnionHixPattern union) {
      foreach (var member in union.Patterns) {
        var trial = new Dictionary<HixTypeParameter, HixPattern>(substitutions);
        if (!Unify(member, actual, trial, patterns)) continue;
        foreach (var pair in trial) substitutions[pair.Key] = pair.Value;
        return true;
      }
      return false;
    }
    return HixPatternRelations.Relate(actual, expected, patterns) != HixPatternRelation.Never;
  }

  private static object ConstantData(ExpressionIr value) => value switch {
    null or NullExpressionIr => null,
    MissingExpressionIr => MissingHixValue.Instance,
    StringExpressionIr text => text.Value,
    NumberExpressionIr number => number.Value,
    BooleanExpressionIr boolean => boolean.Value,
    TupleExpressionIr tuple => tuple.Values.Select(ConstantData).ToArray(),
    TableExpressionIr table => table.Entries.ToDictionary(entry => entry.Key, entry => ConstantData(entry.Value)),
    _ => throw new ArgumentException("parameter defaults must be constant values")
  };

  public static bool Arrange(CallExpressionIr call, IReadOnlyList<HixPatternField> fields,
    IReadOnlyList<ExpressionIr> defaults, bool variadic, out ExpressionIr[] arguments) {
    arguments = null;
    if (variadic) {
      if (call.ArgumentNames.Any(name => name != null) || call.Arguments.Count < fields.Count - 1) return false;
      arguments = call.Arguments.ToArray(); return true;
    }
    if (call.Arguments.Count > fields.Count) return false;
    var values = new ExpressionIr[fields.Count];
    var positional = 0; var named = false;
    for (var index = 0; index < call.Arguments.Count; index++) {
      var name = call.ArgumentNames[index];
      var target = name == null ? positional++ : fields.ToList().FindIndex(field => field.Name == name);
      if (name == null && named || target < 0 || target >= fields.Count || values[target] != null) return false;
      named |= name != null;
      values[target] = call.Arguments[index];
    }
    for (var index = 0; index < values.Length; index++) {
      if (values[index] != null) continue;
      if (defaults == null || index >= defaults.Count || defaults[index] == null) return false;
      values[index] = new HixIrRewriter().Rewrite(defaults[index]);
    }
    arguments = values; return true;
  }

  // Source validation and contextual callable inference.
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
        var resolution = PatternBinding.Resolve(new CallExpressionIr(root.Name, arguments), functions, backend, patterns,
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
            if (!PatternBinding.Unify(signature.Parameters[index].Pattern, context.Parameters[index].Pattern, substitutions, patterns)) {
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
    patterns = backend.Functions.Types.Select(type => new KeyValuePair<string, HixPattern>(type.Name, type.Pattern))
      .Concat(patterns).Concat(declarations.OfType<TypeDeclarationIr>()
        .Select(type => new KeyValuePair<string, HixPattern>(type.Name, type.Pattern)))
      .GroupBy(item => item.Key, StringComparer.Ordinal).ToDictionary(group => group.Key, group => group.Last().Value, StringComparer.Ordinal);
    try { patterns = PatternComposition.ResolveDefinitions(patterns); }
    catch (ArgumentException error) { diagnostics.Add(new(0, error.Message)); return; }
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
      case RootExpressionIr {IsSmart: true, Name: var name} when int.TryParse(name, out var index) && index >= 0 && index < parameters.Count:
        return parameters[index].Pattern;
      case RootExpressionIr {IsSmart: true} smart:
        return locals.TryGetValue(smart.Name, out var smartLocal) ? smartLocal :
          parameters.FirstOrDefault(parameter => parameter.Name == smart.Name)?.Pattern ?? HixPattern.Any;
      case RootExpressionIr {IsSmart: false, Name: "args"}: return new TupleHixPattern(parameters.ToArray());
      case RootExpressionIr {IsSmart: false, Name: "local" or "var" or "tar" or "param"}: return HixPattern.Any;
      case RootExpressionIr {IsSmart: false} root: return RootValuePattern(root.Name, functions, patterns, backend);
      case LambdaExpressionIr lambda:
        return new DelegateHixPattern([], InferResult(lambda.Body, new Dictionary<string, HixPattern>(),
          [], functions, patterns, diagnostics, backend));
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
          var constructors = functions.ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal);
          constructors[call.Name] = PatternConstructorCompiler.Compile(call.Name, checkedPattern, patterns)
            .Where(method => method.Name == call.Name).ToArray();
          var constructorResolution = Resolve(call, constructors, backend, patterns,
            value => { var position = call.Arguments.ToList().IndexOf(value);
              return position >= 0 ? supplied[position] : Infer(value, locals, parameters, functions, patterns, diagnostics, backend); },
            out var constructorAmbiguous);
          if (constructorResolution != null) return constructorResolution.Signature.Result;
          if (constructorAmbiguous) diagnostics.Add(new(call.Line, "ambiguous constructor '" + call.Name + "'"));
          else if (HixPatterns.TryTrait(checkedPattern, patterns, out _))
            diagnostics.Add(new(call.Line, "no matching trait constructor '" + call.Name + "'"));
          else if (HixPatterns.TryStructural(checkedPattern, patterns, out var structural))
            diagnostics.Add(new(call.Line, PatternConstructorDiagnostics.Describe(call.Name,
              call.ArgumentNames, supplied, structural is TableHixPattern table ? table.Fields : ((TupleHixPattern)structural).Fields, patterns)));
          else diagnostics.Add(new(call.Line, "pattern '" + call.Name + "' has no constructor; use cast(value, " + call.Name + ")"));
          return HixPattern.Any;
        }
        var resolution = PatternBinding.Resolve(call, functions, backend, patterns,
          value => { var position = call.Arguments.ToList().IndexOf(value);
            return position >= 0 ? supplied[position] : Infer(value, locals, parameters, functions, patterns, diagnostics, backend); }, out var ambiguous);
        call.Resolution = resolution;
        if (resolution != null) return resolution.Signature.Result;
        if (ambiguous) {
          if (supplied.All(value => value is not AnyHixPattern))
            diagnostics.Add(new(call.Line, "ambiguous overload of '" + call.Name + "'"));
          return HixPattern.Any;
        }
        if (functions.TryGetValue(call.Name, out var declarations) && declarations.Any(function =>
            function.Signatures.Count == 0 || function.Signatures.Any(signature => signature.Inputs == null && signature.InputPattern == null))) {
          diagnostics.Add(new(call.Line, "cannot bind signature of '" + call.Name + "'; ordinary calls require a declared matching signature"));
          return HixPattern.Any;
        }
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
    if (receiver is TraitHixPattern trait) receiver = trait.Requirement;
    if (receiver is IntersectionHixPattern intersection) return new IntersectionHixPattern(Member(intersection.Left, name, patterns), Member(intersection.Right, name, patterns));
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
}
