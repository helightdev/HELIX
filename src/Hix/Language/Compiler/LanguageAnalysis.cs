using System;
using System.Collections.Generic;
using System.Linq;
using Hix.Functions;

namespace Hix.Compiler;

/// <summary>Definition and reference analysis over the canonical semantic AST.</summary>
public sealed class LanguageAnalysis {
  public sealed record Symbol(string Name, string Kind, HixSourceRange Range, HixSourceRange Scope,
    HixIrNode Node);
  public sealed record Reference(string Name, string Kind, HixSourceRange Range, HixIrNode Node);
  public sealed record TypeFact(HixSourceRange Range, string Type, string Documentation, bool Inlay,
    string Kind = "Type") {
    public HixCallResolution CallResolution { get; init; }
    public IReadOnlyList<HixPatternField> Members { get; init; } = Array.Empty<HixPatternField>();
  }

  private readonly HixBackend backend;
  public LanguageAnalysis(string source, HixBackend backend = null,
    bool recoverValidDeclarations = false) {
    this.backend = backend ?? HixCoreBackend.Instance;
    Program = AntlrSyntax.Parse(source ?? "", this.backend, recoverValidDeclarations);
    var declarations = new List<Symbol>();
    var references = new List<Reference>();
    Visit(Program, declarations, references);
    Declarations = declarations;
    References = references;
    TypeFacts = CollectTypeFacts();
  }

  public CompilationUnitIr Program { get; }
  public IReadOnlyList<Symbol> Declarations { get; }
  public IReadOnlyList<Reference> References { get; }
  public IReadOnlyList<TypeFact> TypeFacts { get; }

  private IReadOnlyList<TypeFact> CollectTypeFacts() {
    var facts = new List<TypeFact>();
    IReadOnlyList<HixIrNode> exports;
    try { exports = DeclarationExports.Expand(Program.Declarations); }
    catch (ArgumentException) { exports = Program.Declarations; }
    var patterns = backend.Functions.Types.Concat(exports.OfType<TypeDeclarationIr>()).GroupBy(type => type.Name).Select(group => group.Last()).ToDictionary(type => type.Name,
      type => type.Pattern, StringComparer.Ordinal);
    try { patterns = PatternComposition.ResolveDefinitions(patterns).ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal); }
    catch (ArgumentException) { }
    var functions = exports.OfType<FunctionDeclarationIr>().Concat(
        exports.OfType<MixinDeclarationIr>().SelectMany(mixin => mixin.Declarations.OfType<FunctionDeclarationIr>()))
      .GroupBy(function => function.Name, StringComparer.Ordinal).ToDictionary(group => group.Key,
        group => group.ToArray(), StringComparer.Ordinal);
    foreach (var type in Program.Declarations.OfType<TypeDeclarationIr>())
      facts.Add(new(IdentifierRange(type, type.Name), type.Name,
        DeclarationDocumentation(type.Name, type.Metadata), false));
    foreach (var metadata in Program.Declarations.OfType<MetadataDeclarationIr>())
      facts.Add(new(IdentifierRange(metadata, metadata.Name), "metadata", DeclarationDocumentation(
        metadata.Retention.ToString().ToLowerInvariant() + " meta " + metadata.Name + "(" + string.Join(", ", metadata.Arguments.Select(argument =>
          argument.AsPatternField().Display)) + ") for " +
        string.Join(" ", HixMetadataTargets.Names(metadata.Targets)), metadata.Metadata), false));
    foreach (var mixin in Program.Declarations.OfType<MixinDeclarationIr>())
      facts.Add(new(IdentifierRange(mixin, mixin.Name), "mixin",
        DeclarationDocumentation("mixin " + mixin.Name + "(" + string.Join(", ", mixin.Parameters.Select(field =>
          field.AsPatternField().Display)) + ")", mixin.Metadata), false));
    foreach (var function in functions.Values.SelectMany(group => group))
      facts.Add(new(IdentifierRange(function, function.Name), FunctionResult(function), FunctionDocumentation(function), false));
    foreach (var function in functions.Values.SelectMany(group => group)) {
      foreach (var parameter in function.TypeParameters) {
        var bounds = (parameter.Bound == null ? Array.Empty<HixPattern>() : new[] {parameter.Bound})
          .Concat(parameter.AdditionalBounds).ToArray();
        var documentation = "generic " + parameter.Name + (parameter.ComputedPath == null ? "" : " computed from " + parameter.ComputedPath) + (bounds.Length == 0 ? " (unbounded)" :
          " bounded by " + string.Join(" and ", bounds.Select(bound => bound.Display)));
        foreach (var token in function.Tokens.Where(token => token.Text == parameter.Name))
          facts.Add(new(HixSourceRange.FromToken(token), parameter.Name, documentation, false));
        if (GenericParameterLocation(function, parameter.Name) is { } location)
          facts.Add(new(location.Range, parameter.Name, documentation, false));
      }
    }
    foreach (var function in functions.Values.SelectMany(group => group)) {
      var parameters = function.Signatures.Count == 1 ? function.Signatures[0].Inputs ?? [] : [];
      if (!function.IsExternal) Analyze(function.Body, new Dictionary<string, HixPattern>(StringComparer.Ordinal), parameters);
    }
    foreach (var expression in Program.Declarations.OfType<MixinDeclarationIr>()
               .SelectMany(mixin => mixin.Declarations.OfType<ExpressionDeclarationIr>()
                 .Select(expression => (mixin, expression))))
      Analyze(expression.expression.Body, new Dictionary<string, HixPattern>(StringComparer.Ordinal),
        expression.mixin.Parameters);
    return facts;

    void Analyze(BlockStatementIr block, IDictionary<string, HixPattern> locals,
      IReadOnlyList<SignatureField> parameters) {
      foreach (var statement in block.Statements) {
        if (statement is AssignmentStatementIr assignment) {
          var inferred = assignment.Value == null ? HixPattern.Any : Infer(assignment.Value, locals, parameters);
          if (assignment.Storage == StorageSpace.Local) {
            if (assignment.IsDeclaration) locals[assignment.Name] = assignment.DeclaredPattern ?? inferred;
            var local = locals.TryGetValue(assignment.Name, out var known) ? known : HixPattern.Any;
            facts.Add(new(IdentifierRange(assignment, assignment.Name), local.Display,
              "local " + assignment.Name + ": " + local.Display, assignment.IsDeclaration && assignment.DeclaredPattern == null && assignment.Value != null));
          }
        } else if (statement is BlockStatementIr nested) {
          Analyze(nested, new Dictionary<string, HixPattern>(locals, StringComparer.Ordinal), parameters);
        } else foreach (var expression in statement.SemanticChildren.OfType<ExpressionIr>()) Infer(expression, locals, parameters);
      }
    }

    HixPattern Infer(ExpressionIr expression, IDictionary<string, HixPattern> locals,
      IReadOnlyList<SignatureField> parameters) {
      HixPattern result;
      switch (expression) {
        case StringExpressionIr:
        case InterpolationExpressionIr:
          result = new KindHixPattern(HixValueKind.String); break;
        case NumberExpressionIr: result = new KindHixPattern(HixValueKind.Number); break;
        case BooleanExpressionIr: result = new KindHixPattern(HixValueKind.Bool); break;
        case NullExpressionIr: result = new KindHixPattern(HixValueKind.Null); break;
        case MissingExpressionIr: result = new KindHixPattern(HixValueKind.Missing); break;
        case TupleExpressionIr tuple: result = new TupleHixPattern(tuple.Values.Select((value, index) =>
          new HixPatternField(index.ToString(), Infer(value, locals, parameters))).ToArray()); break;
        case TableExpressionIr table: result = new TableHixPattern(table.Entries.Select(entry =>
          new HixPatternField(entry.Key, Infer(entry.Value, locals, parameters))).ToArray()); break;
        case MemberExpressionIr {Receiver: RootExpressionIr {Name: "local"}} member:
          result = locals.TryGetValue(member.Member, out var local) ? local : HixPattern.Any;
          facts.Add(new(TrailingIdentifierRange(member, member.Member), result.Display,
            "local " + member.Member + ": " + result.Display, false));
          break;
        case MemberExpressionIr {Receiver: RootExpressionIr {Name: "param"}} member:
          result = parameters.FirstOrDefault(parameter => parameter.Name == member.Member)?.Pattern ?? HixPattern.Any;
          facts.Add(new(TrailingIdentifierRange(member, member.Member), result.Display,
            "parameter " + member.Member + ": " + result.Display, false));
          break;
        case RootExpressionIr {IsSmart: true, Name: var position} when
          int.TryParse(position, out var index) && index >= 0 && index < parameters.Count:
          result = parameters[index].Pattern; break;
        case RootExpressionIr {IsSmart: true} root:
          result = locals.TryGetValue(root.Name, out var smartLocal) ? smartLocal :
            parameters.FirstOrDefault(parameter => parameter.Name == root.Name)?.Pattern ?? HixPattern.Any;
          break;
        case RootExpressionIr root when backend.Roots.TryGetValue(root.Name, out var hostRoot):
          result = hostRoot.Pattern; break;
        case SelectionExpressionIr selection:
          if (selection.Selector != null) Infer(selection.Selector, locals, parameters);
          var results = new List<HixPattern>();
          foreach (var branch in selection.Branches) {
            var branchLocals = new Dictionary<string, HixPattern>(locals, StringComparer.Ordinal);
            foreach (var condition in branch.Conditions) Infer(condition, branchLocals, parameters);
            results.Add(InferResult(branch.Result, branchLocals, parameters));
          }
          if (selection.Fallback != null)
            results.Add(InferResult(selection.Fallback,
              new Dictionary<string, HixPattern>(locals, StringComparer.Ordinal), parameters));
          result = Union(results);
          break;
        case RootExpressionIr {IsSmart: false, Name: "args"}: result = new TupleHixPattern(parameters.Select(field => field.AsPatternField()).ToArray()); break;
        case RootExpressionIr {IsSmart: false, Name: "local" or "var" or "tar" or "param"}: result = HixPattern.Any; break;
        case RootExpressionIr {IsSmart: false} root:
          result = PatternBinding.RootValuePattern(root.Name, functions, patterns, backend);
          break;
        case LambdaExpressionIr lambda:
          result = new DelegateHixPattern([], InferResult(lambda.Body, new Dictionary<string, HixPattern>(), []));
          break;
        case CallExpressionIr call:
          var argumentPatterns = call.Arguments.Select(argument => Infer(argument, locals, parameters)).ToArray();
          string documentation;
          var dynamic = false;
          {
            var callableFunctions = functions;
            if (backend.Functions.Resolve(call.Name).Count == 0 && patterns.TryGetValue(call.Name, out var constructorPattern)) {
              callableFunctions = functions.ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal);
              callableFunctions[call.Name] = PatternConstructorCompiler.Compile(call.Name, constructorPattern, patterns)
                .Where(method => method.Name == call.Name).ToArray();
            }
            var resolution = PatternBinding.Resolve(call, callableFunctions, backend, patterns,
              value => { var position = call.Arguments.ToList().IndexOf(value);
                return position >= 0 ? argumentPatterns[position] : Infer(value, locals, parameters); }, out var ambiguous);
            call.Resolution = resolution;
            result = resolution?.Signature.Result ?? HixPattern.Any;
            dynamic = call.Name == "call";
            documentation = resolution?.Signature.Display ?? call.Name;
            if (resolution?.Binding.Function is { } languageFunction)
              documentation = DeclarationDocumentation(documentation, languageFunction.Metadata);
            if (resolution?.Binding.Backend is { } definition)
              documentation = resolution.Signature.Name + "(" + string.Join(", ", resolution.Signature.Parameters.Select(field => field.Display)) +
                ") -> " + resolution.Signature.Result.Display + "\n" + definition.Documentation;
            else if (resolution == null && !functions.ContainsKey(call.Name))
              documentation = string.Join("\n", backend.Functions.Resolve(call.Name).SelectMany(definition =>
                definition.Signatures.Select(signature => DefinitionDocumentation(definition, signature))));
            if (resolution != null)
              for (var position = 0; position < resolution.Arguments.Length; position++) {
                var target = resolution.Signature.Parameters[Math.Min(position, resolution.Signature.Parameters.Count - 1)].Pattern;
                var sourcePosition = call.Arguments.ToList().IndexOf(resolution.Arguments[position]);
                var actual = sourcePosition >= 0 ? argumentPatterns[sourcePosition] : HixPattern.Any;
                if (resolution.PatternConversions.Contains(position))
                  facts.Add(new(resolution.Arguments[position].SourceRange, "pattern",
                    "Resolved literal pattern name '" + ((RootExpressionIr)resolution.Arguments[position]).Name + "' at compile time.", true, "Coercion"));
                else if (target is KindHixPattern targetKind && Conversion(actual, targetKind.ValueKind) > 0)
                  facts.Add(new(resolution.Arguments[position].SourceRange, target.Display,
                    "Implicitly coerced from " + actual.Display + " to " + target.Display + ".", true, "Coercion"));
              }
          }
          facts.Add(new(TrailingIdentifierRange(call, call.Name), result.Display, documentation, false, "Call") {
            CallResolution = dynamic ? null : call.Resolution
          });
          if (dynamic) facts.Add(new(TrailingIdentifierRange(call, call.Name), result.Display,
            "Overload is selected dynamically at runtime.\n" + documentation, false, "DynamicCall"));
          break;
        default:
          foreach (var child in expression.SemanticChildren.OfType<ExpressionIr>()) Infer(child, locals, parameters);
          result = HixPattern.Any; break;
      }
      facts.Add(new(expression.SourceRange, result.Display, "", false, "Expression") {
        Members = ExpressionMembers(result, patterns)
      });
      return result;
    }

    HixPattern InferResult(HixIrNode node, IDictionary<string, HixPattern> locals,
      IReadOnlyList<SignatureField> parameters) {
      switch (node) {
        case ExpressionIr expression: return Infer(expression, locals, parameters);
        case InvocationStatementIr invocation: return Infer(invocation.Call, locals, parameters);
        case BlockStatementIr block:
          Analyze(block, locals, parameters);
          var returns = block.Statements.OfType<ControlFlowStatementIr>().Where(value => value.Operation == ControlFlowKind.Return)
            .SelectMany(value => value.Values).Select(value => Infer(value, locals, parameters)).ToArray();
          return returns.Length == 0 ? KindPattern(HixValueKind.Null) : Union(returns);
        default:
          foreach (var child in node.SemanticChildren) InferResult(child, locals, parameters);
          return HixPattern.Any;
      }
    }
  }

  private static HixPattern Union(IEnumerable<HixPattern> values) {
    return HixPatterns.Union(values);
  }
  private static HixPattern KindPattern(HixValueKind kind) => kind == HixValueKind.Any
    ? HixPattern.Any : new KindHixPattern(kind);
  private static int Conversion(HixPattern pattern, HixValueKind target) {
    if (target == HixValueKind.Any) return 0;
    if (pattern is ConstantHixPattern constant) return Conversion(constant.Underlying, target);
    if (pattern is EnumHixPattern enumeration) return Conversion(enumeration.Underlying, target);
    if (pattern is ConstrainedHixPattern constrained) return Conversion(constrained.Underlying, target);
    if (pattern is DocumentedHixPattern documented) return Conversion(documented.Underlying, target);
    if (pattern is TaggedHixPattern tagged) return Conversion(tagged.Underlying, target);
    var source = pattern switch {
      KindHixPattern kind => kind.ValueKind,
      TupleHixPattern or ManyHixPattern => HixValueKind.Tuple,
      TableHixPattern or MapHixPattern => HixValueKind.Table,
      DelegateHixPattern or SignatureHixPattern => HixValueKind.Function,
      _ => HixValueKind.Any
    };
    if (source == HixValueKind.Any) return 0;
    if (source == target) return 0;
    return KindDefinitions.CanImplicitConvert(source, target) ? 1 : -1;
  }
  private static string FunctionResult(FunctionDeclarationIr function) => function.Signatures.Count == 1
    ? (function.Signatures[0].Outputs == null ? function.Signatures[0].OutputPattern?.Display :
      new TableHixPattern(function.Signatures[0].Outputs.Select(field => field.AsPatternField()).ToArray()).Display) ?? "any"
    : "any";
  private static string FunctionDocumentation(FunctionDeclarationIr function) => FunctionDocumentation(function, function.Signatures);
  private static string FunctionDocumentation(FunctionDeclarationIr function, IReadOnlyList<FunctionSignature> signatures) {
    var signature = signatures.Count == 0 ? "func " + function.Name + "(...) -> any"
      : string.Join("\n", signatures.Select(item => item.Constant(function.Name).Display));
    return DeclarationDocumentation(signature, function.Metadata);
  }
  private static string DeclarationDocumentation(string signature, IReadOnlyList<MetadataIr> metadata) {
    var title = metadata.Where(item => item.Name == "title").SelectMany(item => item.Values)
      .OfType<StringExpressionIr>().Select(value => value.Value).FirstOrDefault();
    var documentation = string.Join("\n", metadata.Where(item => item.Name is "doc" or "description")
      .SelectMany(item => item.Values).OfType<StringExpressionIr>().Select(value => value.Value));
    var heading = string.IsNullOrEmpty(title) ? signature : title + "\n" + signature;
    return string.IsNullOrEmpty(documentation) ? heading : heading + "\n" + documentation;
  }
  private static string DefinitionDocumentation(HixFunctionEntry definition, global::Hix.FunctionSignature signature) =>
    definition.Name + "(" + string.Join(", ", signature.ArgumentPatterns.Select((type, index) =>
      (signature.IsVariadic && index == signature.ArgumentPatterns.Count - 1 ? "..." : "") +
      new HixPatternField(signature.GetArgumentName(index), type, HasDefault: signature.GetArgumentDefault(index) != null).Display)) +
    ") -> " + signature.ResultPattern.Display +
    (string.IsNullOrEmpty(definition.Documentation) ? "" : "\n" + definition.Documentation);


  public Symbol Resolve(Reference reference, IEnumerable<LanguageAnalysis> analyses,
    out LanguageAnalysis owner) {
    owner = null;
    if (reference is null) return null;
    var available = (analyses ?? []).ToArray();
    var sourceName = reference.Node switch {
      CallExpressionIr call => call.OriginalName,
      RootExpressionIr root => root.OriginalName,
      _ => reference.Name
    };
    var names = reference.Kind is "Function" or "Pattern"
      ? HixUsingImports.Candidates(sourceName, reference.Node.UsingImports) : [reference.Name];
    var candidates = names.Select(name => available.SelectMany(analysis => analysis.Declarations
      .Where(symbol => symbol.Name == name && Compatible(symbol.Kind, reference.Kind))
      .Select(symbol => (Analysis: analysis, Symbol: symbol))).ToArray())
      .FirstOrDefault(matches => matches.Length != 0) ?? [];
    if (candidates.Length == 0) return null;

    // Prefer the innermost declaration visible at the reference, then declarations in this file,
    // followed by stable directory order supplied by the caller.
    var local = candidates.Where(candidate => ReferenceEquals(candidate.Analysis, this) &&
      Contains(candidate.Symbol.Scope, reference.Range)).OrderBy(candidate => candidate.Symbol.Scope.Length)
      .ThenByDescending(candidate => candidate.Symbol.Range.Start).FirstOrDefault();
    var selected = local.Symbol is not null ? local : candidates.FirstOrDefault(candidate =>
      ReferenceEquals(candidate.Analysis, this));
    if (selected.Symbol is null) selected = candidates[0];
    owner = selected.Analysis;
    return selected.Symbol;
  }

  public static HixSourceRange Scope(HixIrNode node) {
    for (var current = node; current is not null; current = current.Parent)
      if (current is FunctionDeclarationIr or MixinDeclarationIr or BlockStatementIr)
        return current.SourceRange;
    return node?.Program?.SourceRange ?? default;
  }

  private void Visit(HixIrNode node, ICollection<Symbol> declarations,
    ICollection<Reference> references) {
    switch (node) {
      case MixinDeclarationIr mixin:
        declarations.Add(new Symbol(mixin.Name, "Mixin", IdentifierRange(mixin, mixin.Name),
          Scope(mixin.Parent), mixin));
        break;
      case MetadataDeclarationIr metadata:
        declarations.Add(new Symbol(metadata.Name, "Metadata", IdentifierRange(metadata, metadata.Name), Scope(metadata.Parent), metadata));
        foreach (var argument in metadata.Arguments) AddPatternReferences(argument.Pattern, metadata, references);
        break;
      case FunctionDeclarationIr function:
        declarations.Add(new Symbol(function.Name, "Function", IdentifierRange(function, function.Name),
          Scope(function.Parent), function));
        foreach (var parameter in function.TypeParameters) {
          if (GenericParameterLocation(function, parameter.Name) is { } location)
            declarations.Add(new Symbol(parameter.Name, "Pattern", location.Range,
              function.SourceRange with {Start = function.Metadata.Min(item => item.SourceRange.Start)}, location.Metadata));
        }
        foreach (var signature in function.Signatures) {
          if (signature.InputPattern != null) AddPatternReferences(signature.InputPattern, function, references);
          foreach (var field in signature.Inputs ?? []) AddPatternReferences(field.Pattern, function, references);
          if (signature.OutputPattern != null) AddPatternReferences(signature.OutputPattern, function, references);
          foreach (var field in signature.Outputs ?? []) AddPatternReferences(field.Pattern, function, references);
        }
        break;
      case TypeDeclarationIr type:
        declarations.Add(new Symbol(type.Name, "Pattern", IdentifierRange(type, type.Name), Scope(type.Parent), type));
        AddPatternReferences(type.Pattern, type, references);
        break;
      case AssignmentStatementIr assignment:
        if (assignment.IsDeclaration)
          declarations.Add(new Symbol(assignment.Name, "Local", IdentifierRange(assignment, assignment.Name),
            Scope(assignment), assignment));
        else references.Add(new Reference(assignment.Name, "Local", IdentifierRange(assignment, assignment.Name), assignment));
        break;
      case ControlFlowStatementIr {Operation: ControlFlowKind.Label} label:
        declarations.Add(new Symbol(label.Label, "Label", IdentifierRange(label, label.Label),
          Scope(label), label));
        break;
      case ControlFlowStatementIr {Operation: ControlFlowKind.Goto} jump:
        references.Add(new Reference(jump.Label, "Label", IdentifierRange(jump, jump.Label), jump));
        break;
      case MemberExpressionIr member when StorageReference(member) is { } storage:
        references.Add(new Reference(member.Member, storage, TrailingIdentifierRange(member, member.Member), member));
        break;
      case CallExpressionIr call:
        AddCallReferences(call, references);
        break;
      case RootExpressionIr root when IsFunctionReference(root.Name):
        references.Add(new Reference(root.Name, "Function", IdentifierRange(root, root.Name), root));
        break;
      case MetadataIr metadata when metadata.Name == "computedGeneric":
        if (metadata.Values.ElementAtOrDefault(1) is StringExpressionIr pointer) {
          try {
            var parts = ComputedGenericPath.Parse(pointer.Value);
            if (parts[0] == "generics") {
              var range = StringContentRange(pointer);
              references.Add(new Reference(parts[1], "Pattern", range with {Start = range.Start + "/generics/".Length, End = range.Start + "/generics/".Length + parts[1].Length}, pointer));
            }
          } catch (ArgumentException) { }
        }
        foreach (var argument in metadata.Values.Skip(2)) AddTypeArgumentReferences(argument, references);
        return;
      case MetadataIr metadata when metadata.Name == "generic":
        foreach (var argument in metadata.Values.Skip(1)) AddTypeArgumentReferences(argument, references);
        return;
      case MetadataIr metadata when backend.Functions.PatternConstructors.Any(item => item.Name == metadata.Name) ||
          metadata.Name is "many" or "map" or "union":
        foreach (var argument in metadata.Values) AddTypeArgumentReferences(argument, references);
        return;
    }
    foreach (var child in node.Children) Visit(child, declarations, references);
  }

  private void AddTypeArgumentReferences(ExpressionIr expression, ICollection<Reference> references) {
    switch (expression) {
      case StringExpressionIr text when HixPatterns.Named(text.Value) is NamedHixPattern:
        references.Add(new Reference(text.Value, "Pattern", StringContentRange(text), text)); break;
      case RootExpressionIr root when HixPatterns.Named(root.Name) is NamedHixPattern:
        references.Add(new Reference(root.Name, "Pattern", IdentifierRange(root, root.Name), root)); break;
      case CallExpressionIr call:
        foreach (var argument in call.Arguments) AddTypeArgumentReferences(argument, references); break;
    }
  }

  private void AddPatternReferences(HixPattern pattern, HixIrNode owner, ICollection<Reference> references) {
    switch (pattern) {
      case TypeParameterHixPattern parameter:
        references.Add(new Reference(parameter.Parameter.Name, "Pattern", IdentifierRange(owner, parameter.Parameter.Name), owner)); break;
      case OpaqueHixPattern opaque:
        foreach (var argument in opaque.Arguments) AddPatternReferences(argument, owner, references); break;
      case NamedHixPattern named:
        references.Add(new Reference(named.Name, "Pattern", IdentifierRange(owner, named.Name), owner)); break;
      case UnionHixPattern union:
        foreach (var value in union.Patterns) AddPatternReferences(value, owner, references); break;
      case TupleHixPattern tuple:
        foreach (var field in tuple.Fields) AddPatternReferences(field.Pattern, owner, references); break;
      case TableHixPattern table:
        foreach (var field in table.Fields) AddPatternReferences(field.Pattern, owner, references); break;
      case ManyHixPattern many: AddPatternReferences(many.Element, owner, references); break;
      case MapHixPattern map:
        AddPatternReferences(map.Key, owner, references); AddPatternReferences(map.Value, owner, references); break;
      case ConstantHixPattern constant: AddPatternReferences(constant.Underlying, owner, references); break;
      case EnumHixPattern enumeration: AddPatternReferences(enumeration.Underlying, owner, references); break;
      case DocumentedHixPattern documented: AddPatternReferences(documented.Underlying, owner, references); break;
      case ConstrainedHixPattern constrained: AddPatternReferences(constrained.Underlying, owner, references); break;
      case TraitHixPattern trait: AddPatternReferences(trait.Underlying, owner, references); break;
      case InheritedHixPattern inherited: AddPatternReferences(inherited.Parent, owner, references); AddPatternReferences(inherited.Underlying, owner, references); break;
      case TraitUsageHixPattern usage: AddPatternReferences(usage.Trait, owner, references); AddPatternReferences(usage.Underlying, owner, references); break;
      case IntersectionHixPattern intersection: AddPatternReferences(intersection.Left, owner, references); AddPatternReferences(intersection.Right, owner, references); break;
      case TaggedHixPattern tagged: AddPatternReferences(tagged.Underlying, owner, references); break;
      case DelegateHixPattern callable:
        foreach (var field in callable.Parameters) AddPatternReferences(field.Pattern, owner, references);
        AddPatternReferences(callable.Result, owner, references); break;
    }
  }

  private void AddCallReferences(CallExpressionIr call, ICollection<Reference> references) {
    if (call.Resolution is { } resolution)
      foreach (var position in resolution.PatternConversions) {
        var target = (RootExpressionIr)resolution.Arguments[position];
        if (call.Arguments.FirstOrDefault(argument => argument.SourceRange == target.SourceRange) is StringExpressionIr literal &&
            HixPatterns.Named(target.Name) is NamedHixPattern)
          references.Add(new Reference(target.Name, "Pattern", StringContentRange(literal), literal));
      }
    if (Program.Declarations.OfType<TypeDeclarationIr>().Any(type => type.Name == call.Name)) {
      references.Add(new Reference(call.Name, "Pattern", TrailingIdentifierRange(call, call.Name), call));
      return;
    }
    if (!backend.Functions.TryGet(call.Name, call.Arguments.Count, out var definition))
      references.Add(new Reference(call.Name, "Function", TrailingIdentifierRange(call, call.Name), call));
    else foreach (var reference in definition.ArgumentReferences) {
      var index = reference.Key;
      if (index < 0 || index >= call.Arguments.Count || call.Arguments[index] is not StringExpressionIr text) continue;
      var range = StringContentRange(call.Arguments[index]);
      references.Add(new Reference(text.Value, reference.Value, range, call.Arguments[index]));
    }
  }

  private static string StorageReference(MemberExpressionIr member) {
    if (member.Receiver is not RootExpressionIr root) return null;
    return root.Name switch {
      "local" => "Local", "var" => "Variable", "tar" => "TargetVariable", _ => null
    };
  }

  private bool IsFunctionReference(string name) => !backend.Roots.ContainsKey(name) && name is not
    ("local" or "var" or "tar" or
     "param" or "true" or "false" or "null" or "string" or "bool" or "number" or
     "tuple" or "table" or "opaque" or "function" or "error" or "pattern");

  private HixSourceRange IdentifierRange(HixIrNode node, string name) {
    if (node is RootExpressionIr root && name == root.Name) name = root.OriginalName;
    var token = node.Tokens.FirstOrDefault(item => item.Channel == Antlr4.Runtime.TokenConstants.DefaultChannel && item.Text == name);
    return token != null ? HixSourceRange.FromToken(token) : TextRange(node.SourceRange, name, false);
  }

  private static IReadOnlyList<HixPatternField> ExpressionMembers(HixPattern pattern,
    IReadOnlyDictionary<string, HixPattern> definitions) {
    if (HixPatterns.TryStructural(pattern, definitions, out var structural))
      return structural is TableHixPattern table ? table.Fields.Where(field => field.Name != null).ToArray() : ((TupleHixPattern)structural).Fields
        .Select((field, index) => field with {Name = index.ToString()}).ToArray();
    if (pattern is UnionHixPattern union) {
      var variants = union.Patterns.Where(item => item is not KindHixPattern {ValueKind: HixValueKind.Null or HixValueKind.Missing})
        .Select(item => ExpressionMembers(item, definitions)).ToArray();
      if (variants.Length == 0) return Array.Empty<HixPatternField>();
      return variants[0].Where(field => variants.All(fields => fields.Any(item => item.Name == field.Name)))
        .Select(field => field with {Pattern = HixPatterns.Union(variants.Select(fields => fields.First(item => item.Name == field.Name).Pattern))}).ToArray();
    }
    return Array.Empty<HixPatternField>();
  }

  private HixSourceRange TrailingIdentifierRange(HixIrNode node, string name) {
    if (node is CallExpressionIr call && name == call.Name) name = call.OriginalName;
    var token = node.Tokens.LastOrDefault(item => item.Channel == Antlr4.Runtime.TokenConstants.DefaultChannel && item.Text == name);
    return token != null ? HixSourceRange.FromToken(token) : TextRange(node.SourceRange, name, true);
  }

  private HixSourceRange TextRange(HixSourceRange within, string text, bool last) {
    var source = Program.Source;
    var start = last
      ? source.LastIndexOf(text, Math.Max(within.Start, within.End - 1), within.Length, StringComparison.Ordinal)
      : source.IndexOf(text, within.Start, within.Length, StringComparison.Ordinal);
    return start < 0 ? within : new HixSourceRange(start, start + text.Length, within.Line, within.Column);
  }

  private static (MetadataIr Metadata, HixSourceRange Range)? GenericParameterLocation(FunctionDeclarationIr function, string name) {
    foreach (var metadata in function.Metadata.Where(item => item.Name is "generic" or "computedGeneric")) {
      foreach (var value in metadata.Values.Take(1).OfType<StringExpressionIr>()) {
        var offset = 0;
        foreach (var part in value.Value.Split(',')) {
          if (part.Trim() == name) {
            var range = StringContentRange(value);
            var start = range.Start + offset + part.IndexOf(name, StringComparison.Ordinal);
            return (metadata, range with {Start = start, End = start + name.Length});
          }
          offset += part.Length + 1;
        }
      }
    }
    return null;
  }

  private static HixSourceRange StringContentRange(ExpressionIr expression) {
    var range = expression.SourceRange;
    return range.Length >= 2 ? range with {Start = range.Start + 1, End = range.End - 1} : range;
  }

  private static bool Compatible(string declaration, string reference) => declaration == reference;
  private static bool Contains(HixSourceRange scope, HixSourceRange range) =>
    scope.Start <= range.Start && scope.End >= range.End;
}
