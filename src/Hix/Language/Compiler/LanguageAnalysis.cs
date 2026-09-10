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
    string Kind = "Type");

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
    var patterns = Program.Declarations.OfType<TypeDeclarationIr>().ToDictionary(type => type.Name,
      type => type.Pattern, StringComparer.Ordinal);
    var functions = Program.Declarations.OfType<FunctionDeclarationIr>().Concat(
        Program.Declarations.OfType<MixinDeclarationIr>().SelectMany(mixin => mixin.Declarations.OfType<FunctionDeclarationIr>()))
      .GroupBy(function => function.Name, StringComparer.Ordinal).ToDictionary(group => group.Key,
        group => group.ToArray(), StringComparer.Ordinal);
    foreach (var type in Program.Declarations.OfType<TypeDeclarationIr>())
      facts.Add(new(IdentifierRange(type, type.Name), type.Name,
        DeclarationDocumentation("pattern " + type.Name + " = " + type.Pattern.Display, type.Metadata), false));
    foreach (var mixin in Program.Declarations.OfType<MixinDeclarationIr>())
      facts.Add(new(IdentifierRange(mixin, mixin.Name), "mixin",
        DeclarationDocumentation("mixin " + mixin.Name, mixin.Metadata), false));
    foreach (var function in functions.Values.SelectMany(group => group))
      facts.Add(new(IdentifierRange(function, function.Name), FunctionResult(function), FunctionDocumentation(function), false));
    foreach (var function in functions.Values.SelectMany(group => group)) {
      var parameters = function.Signatures.Count == 1 ? function.Signatures[0].Inputs ?? [] : [];
      Analyze(function.Body, new Dictionary<string, HixPattern>(StringComparer.Ordinal), parameters);
    }
    foreach (var expression in Program.Declarations.OfType<MixinDeclarationIr>()
               .SelectMany(mixin => mixin.Declarations.OfType<ExpressionDeclarationIr>()))
      Analyze(expression.Body, new Dictionary<string, HixPattern>(StringComparer.Ordinal), []);
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
              "local " + assignment.Name + ": " + local.Display, assignment.IsDeclaration));
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
          result = KindPattern(hostRoot.Kind); break;
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
        case CallExpressionIr call:
          var argumentPatterns = call.Arguments.Select(argument => Infer(argument, locals, parameters)).ToArray();
          string documentation;
          var dynamic = false;
          if (patterns.ContainsKey(call.Name)) {
            result = new NamedHixPattern(call.Name);
            documentation = "Validate a value against pattern `" + call.Name + "`.";
          } else if (functions.TryGetValue(call.Name, out var overloads)) {
            documentation = string.Join("\n\n", overloads.Select(overload => {
              var applicable = overload.Signatures.Where(signature => {
                var fields = signature.Constant(call.Name).Parameters;
                return fields.Count >= argumentPatterns.Length && argumentPatterns.Select((argument, index) =>
                  fields[index].Pattern is KindHixPattern kind ? Conversion(argument, kind.ValueKind) >= 0 :
                    HixPatternRelations.Relate(argument, fields[index].Pattern, patterns) != HixPatternRelation.Never).All(matches => matches);
              }).ToArray();
              return overload.Signatures.Count == 0 || applicable.Length != 0
                ? FunctionDocumentation(overload, applicable) : null;
            }).Where(text => text != null));
            var candidates = overloads.SelectMany(overload => overload.Signatures).Where(signature =>
                signature.Inputs != null && signature.Inputs.Count == argumentPatterns.Length)
              .Select(signature => (Signature: signature, Relations: signature.Inputs.Select((field, index) =>
                HixPatternRelations.Relate(argumentPatterns[index], field.Pattern, patterns)).ToArray()))
              .Where(candidate => candidate.Relations.All(relation => relation != HixPatternRelation.Never)).ToArray();
            var score = candidates.Length == 0 ? -1 : candidates.Max(candidate =>
              candidate.Relations.Count(relation => relation == HixPatternRelation.Always));
            var best = candidates.Where(candidate => candidate.Relations.Count(relation =>
              relation == HixPatternRelation.Always) == score).ToArray();
            var fallback = overloads.Any(overload => overload.Signatures.Count == 0);
            var selected = best.Length == 1 && (!fallback || best[0].Relations.All(relation =>
              relation == HixPatternRelation.Always)) ? best[0].Signature : null;
            dynamic = selected == null;
            result = selected == null ? HixPattern.Any : selected.Outputs == null
              ? selected.OutputPattern ?? HixPattern.Any
              : new TableHixPattern(selected.Outputs.Select(field => field.AsPatternField()).ToArray());
            if (selected?.Inputs != null)
              for (var index = 0; index < argumentPatterns.Length; index++)
                if (selected.Inputs[index].Pattern is KindHixPattern target &&
                    Conversion(argumentPatterns[index], target.ValueKind) > 0)
                  facts.Add(new(call.Arguments[index].SourceRange, target.Display, "Implicitly coerced from " +
                    argumentPatterns[index].Display + " to " + target.Display + ".", true, "Coercion"));
          } else {
            var knownDefinitions = backend.Functions.Enumerate().Where(definition => definition.Name == call.Name).ToArray();
            var definitions = knownDefinitions.Where(definition => definition.MatchesArgumentCount(call.Arguments.Count)).ToArray();
            result = definitions.Length == 0 ? HixPattern.Any : Union(definitions.SelectMany(definition =>
              definition.Signatures.Where(signature => signature.MatchesArgumentCount(call.Arguments.Count))
                .Select(signature => KindPattern(signature.ResultType))));
            var documented = knownDefinitions.SelectMany(definition => definition.Signatures
                .Where(signature => (signature.IsVariadic || argumentPatterns.Length <= signature.ArgumentTypes.Count) &&
                  argumentPatterns.Select((argument, index) => Conversion(argument, signature.GetArgumentType(index)))
                    .All(conversion => conversion >= 0))
                .Select(signature => DefinitionDocumentation(definition, signature))).Distinct().ToArray();
            documentation = documented.Length == 0 ? call.Name : string.Join("\n\n", documented);
            var signatures = definitions.SelectMany(definition => definition.Signatures)
              .Where(signature => signature.MatchesArgumentCount(call.Arguments.Count)).ToArray();
            var selected = signatures.Select(signature => (Signature: signature, Conversions: argumentPatterns
                .Select((argument, index) => Conversion(argument, signature.GetArgumentType(index))).ToArray()))
              .Where(candidate => candidate.Conversions.All(conversion => conversion >= 0))
              .OrderBy(candidate => candidate.Conversions.Sum()).FirstOrDefault();
            if (selected.Signature != null)
              for (var index = 0; index < selected.Conversions.Length; index++)
                if (selected.Conversions[index] > 0) {
                  var target = selected.Signature.GetArgumentType(index).ToString().ToLowerInvariant();
                  facts.Add(new(call.Arguments[index].SourceRange, target, "Implicitly coerced from " +
                    argumentPatterns[index].Display + " to " + target + ".", true, "Coercion"));
                }
          }
          facts.Add(new(TrailingIdentifierRange(call, call.Name), result.Display, documentation, false, "Call"));
          if (dynamic) facts.Add(new(TrailingIdentifierRange(call, call.Name), result.Display,
            "Overload is selected dynamically at runtime.\n" + documentation, false, "DynamicCall"));
          break;
        default:
          foreach (var child in expression.SemanticChildren.OfType<ExpressionIr>()) Infer(child, locals, parameters);
          result = HixPattern.Any; break;
      }
      facts.Add(new(expression.SourceRange, result.Display, "", false, "Expression"));
      return result;
    }

    HixPattern InferResult(HixIrNode node, IDictionary<string, HixPattern> locals,
      IReadOnlyList<SignatureField> parameters) {
      switch (node) {
        case ExpressionIr expression: return Infer(expression, locals, parameters);
        case InvocationStatementIr invocation: return Infer(invocation.Call, locals, parameters);
        case BlockStatementIr block:
          Analyze(block, locals, parameters);
          return HixPattern.Any;
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
  private static string DefinitionDocumentation(FunctionDefinition definition, global::Hix.FunctionSignature signature) =>
    definition.Name + "(" + string.Join(", ", signature.ArgumentTypes.Select((type, index) =>
      (signature.IsVariadic && index == signature.ArgumentTypes.Count - 1 ? "..." : "") + type.ToString().ToLowerInvariant())) +
    ") -> " + signature.ResultType.ToString().ToLowerInvariant() +
    (string.IsNullOrEmpty(definition.Documentation) ? "" : "\n" + definition.Documentation);


  public Symbol Resolve(Reference reference, IEnumerable<LanguageAnalysis> analyses,
    out LanguageAnalysis owner) {
    owner = null;
    if (reference is null) return null;
    var candidates = (analyses ?? []).SelectMany(analysis => analysis.Declarations
      .Where(symbol => symbol.Name == reference.Name && Compatible(symbol.Kind, reference.Kind))
      .Select(symbol => (Analysis: analysis, Symbol: symbol))).ToArray();
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
      case FunctionDeclarationIr function:
        declarations.Add(new Symbol(function.Name, "Function", IdentifierRange(function, function.Name),
          Scope(function.Parent), function));
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
    }
    foreach (var child in node.Children) Visit(child, declarations, references);
  }

  private void AddPatternReferences(HixPattern pattern, HixIrNode owner, ICollection<Reference> references) {
    switch (pattern) {
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
      case TaggedHixPattern tagged: AddPatternReferences(tagged.Underlying, owner, references); break;
      case DelegateHixPattern callable:
        foreach (var field in callable.Parameters) AddPatternReferences(field.Pattern, owner, references);
        AddPatternReferences(callable.Result, owner, references); break;
    }
  }

  private void AddCallReferences(CallExpressionIr call, ICollection<Reference> references) {
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
     "tuple" or "table" or "symbol" or "function" or "error" or "kind" or "pattern");

  private HixSourceRange IdentifierRange(HixIrNode node, string name) {
    var token = node.Tokens.FirstOrDefault(item => item.Channel == Antlr4.Runtime.TokenConstants.DefaultChannel && item.Text == name);
    return token != null ? HixSourceRange.FromToken(token) : TextRange(node.SourceRange, name, false);
  }

  private HixSourceRange TrailingIdentifierRange(HixIrNode node, string name) {
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

  private static HixSourceRange StringContentRange(ExpressionIr expression) {
    var range = expression.SourceRange;
    return range.Length >= 2 ? range with {Start = range.Start + 1, End = range.End - 1} : range;
  }

  private static bool Compatible(string declaration, string reference) => declaration == reference;
  private static bool Contains(HixSourceRange scope, HixSourceRange range) =>
    scope.Start <= range.Start && scope.End >= range.End;
}
