using System;
using System.Collections.Generic;
using System.Linq;

namespace Hix.Compiler.Steps;

/// <summary>Flow-sensitive local/parameter inference and static overload binding.</summary>
internal sealed class PatternBindingStep : HixCompilerStep {
  internal override HixCompilerSyntax Transform(HixCompilerSyntax input, HixExpressionPreparedState globals) {
    var functions = input.Functions.Concat(globals.Functions).GroupBy(function => function.Name, StringComparer.Ordinal)
      .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);
    var rewriter = new PatternRewriter(functions, globals.Patterns, globals.Backend);
    return new(input.Prelude.Select(rewriter.RewriteExpression).ToArray(),
      input.Late.Select(rewriter.RewriteExpression).ToArray(), input.Functions.Select(rewriter.RewriteFunction).ToArray());
  }

  private sealed class PatternRewriter(IReadOnlyDictionary<string, FunctionDeclarationAst[]> functions,
    IReadOnlyDictionary<string, HixPattern> patterns, HixBackend backend) : HixAstRewriter {
    private Dictionary<string, HixPattern> locals = new(StringComparer.Ordinal);
    private IReadOnlyList<SignatureField> parameters = [];

    internal ExpressionDeclarationAst RewriteExpression(ExpressionDeclarationAst expression) {
      var previousLocals = locals; var previousParameters = parameters;
      locals = new(StringComparer.Ordinal); parameters = [];
      var result = CopyLocation(expression, new ExpressionDeclarationAst(expression.IsPrelude, expression.IsStrict,
        RewriteBlock(expression.Body)));
      locals = previousLocals; parameters = previousParameters; return result;
    }

    internal FunctionDeclarationAst RewriteFunction(FunctionDeclarationAst function) {
      var previousLocals = locals; var previousParameters = parameters;
      locals = new(StringComparer.Ordinal);
      parameters = function.Signatures.Count == 1 ? function.Signatures[0].Inputs ?? [] : [];
      var result = CopyLocation(function, new FunctionDeclarationAst(function.Name, function.IsPure, function.IsInline,
        function.IsNoinline, function.Signatures, RewriteBlock(function.Body), function.Metadata));
      locals = previousLocals; parameters = previousParameters; return result;
    }

    private BlockStatementAst RewriteBlock(BlockStatementAst block) {
      var statements = new List<StatementAst>();
      foreach (var statement in block.Statements) {
        if (statement is AssignmentStatementAst assignment) {
          var value = Rewrite(assignment.Value);
          var rewritten = CopyLocation(assignment, new AssignmentStatementAst(assignment.Storage, assignment.Name,
            value, assignment.IsCarried));
          statements.Add(rewritten);
          if (assignment.Storage == StorageSpace.Local) locals[assignment.Name] = Infer(value);
          continue;
        }
        statements.Add(Rewrite(statement));
      }
      return CopyLocation(block, new BlockStatementAst(statements, block.Label));
    }

    protected override HixAst RewriteNode(HixAst node) => node switch {
      BlockStatementAst block => RewriteBlock(block),
      FunctionDeclarationAst function => RewriteFunction(function),
      _ => base.RewriteNode(node)
    };

    protected override ExpressionAst RewriteCall(CallExpressionAst call) {
      var arguments = call.Arguments.Select(Rewrite).ToArray();
      var argumentPatterns = arguments.Select(Infer).ToArray();
      SignatureHixPattern selected = null;
      if (functions.TryGetValue(call.Name, out var declarations)) {
        var hasDynamicFallback = declarations.Any(function => function.Signatures.Count == 0);
        var candidates = declarations.SelectMany(function => function.Signatures.Select(signature => (function, signature)))
          .Where(candidate => AcceptsCount(candidate.signature, arguments.Length))
          .Select(candidate => (candidate.function, candidate.signature,
            relations: Relations(argumentPatterns, candidate.signature).ToArray()))
          .Where(candidate => candidate.relations.All(relation => relation != HixPatternRelation.Never)).ToArray();
        if (candidates.Length != 0) {
          var score = candidates.Max(candidate => candidate.relations.Count(value => value == HixPatternRelation.Always));
          var best = candidates.Where(candidate => candidate.relations.Count(value => value == HixPatternRelation.Always) == score).ToArray();
          if (best.Length == 1 && (!hasDynamicFallback || best[0].relations.All(value => value == HixPatternRelation.Always)))
            selected = best[0].signature.Constant(call.Name);
        }
      } else {
        var candidates = backend.Functions.Resolve(call.Name, arguments.Length)
          .SelectMany(definition => definition.Signatures
            .Where(signature => signature.MatchesArgumentCount(arguments.Length))
            .Select(signature => (definition, signature, relations: argumentPatterns.Select((argument, index) =>
              HixPatternRelations.Relate(argument, KindPattern(signature.GetArgumentType(index)), patterns)).ToArray())))
          .Where(candidate => candidate.relations.All(relation => relation != HixPatternRelation.Never)).ToArray();
        if (candidates.Length != 0) {
          var score = candidates.Max(candidate => candidate.relations.Count(value => value == HixPatternRelation.Always));
          var best = candidates.Where(candidate => candidate.relations.Count(value => value == HixPatternRelation.Always) == score).ToArray();
          if (best.Length == 1) selected = BuiltinSignature(call.Name, best[0].signature);
        }
      }
      return CopyLocation(call, new CallExpressionAst(call.Name, arguments, call.CoerceBoolean, selected));
    }

    private IEnumerable<HixPatternRelation> Relations(IReadOnlyList<HixPattern> supplied, FunctionSignature signature) {
      if (signature.Inputs == null) {
        yield return supplied.Count == 1
          ? HixPatternRelations.Relate(supplied[0], signature.InputPattern, patterns) : HixPatternRelation.Never;
        yield break;
      }
      for (var index = 0; index < supplied.Count; index++) {
        var field = signature.Inputs[Math.Min(index, signature.Inputs.Count - 1)];
        yield return HixPatternRelations.Relate(supplied[index], field.Pattern, patterns);
      }
    }

    private static bool AcceptsCount(FunctionSignature signature, int count) {
      if (signature.Inputs == null) return count == 1;
      var required = signature.Inputs.Count(field => !field.Optional && !field.Variadic);
      return count >= required && (signature.Inputs.Any(field => field.Variadic) || count <= signature.Inputs.Count);
    }

    private static HixPattern KindPattern(HixValueKind kind) => kind == HixValueKind.Any
      ? HixPattern.Any : new KindHixPattern(kind);
    private static SignatureHixPattern BuiltinSignature(string name, global::Hix.FunctionSignature signature) =>
      new(name, signature.ArgumentTypes.Select((kind, index) =>
        new HixPatternField(index.ToString(), KindPattern(kind))).ToArray(), KindPattern(signature.ResultType));

    private HixPattern Infer(ExpressionAst expression) => expression switch {
      StringExpressionAst => new KindHixPattern(HixValueKind.String),
      NumberExpressionAst => new KindHixPattern(HixValueKind.Number),
      BooleanExpressionAst => new KindHixPattern(HixValueKind.Bool),
      NullExpressionAst => new KindHixPattern(HixValueKind.Null),
      TupleExpressionAst tuple => new TupleHixPattern(tuple.Values.Select((value, index) =>
        new HixPatternField(index.ToString(), Infer(value))).ToArray()),
      TableExpressionAst table => new TableHixPattern(table.Entries.Select(entry =>
        new HixPatternField(entry.Key, Infer(entry.Value))).ToArray()),
      CallExpressionAst call when patterns.ContainsKey(call.Name) => new NamedHixPattern(call.Name),
      CallExpressionAst {Signature: { } signature} => signature.Result,
      MemberExpressionAst {Receiver: RootExpressionAst {Name: "local"}} member when locals.TryGetValue(member.Member, out var local) => local,
      MemberExpressionAst {Receiver: RootExpressionAst {Name: "param"}} member =>
        parameters.FirstOrDefault(field => field.Name == member.Member)?.Pattern ?? HixPattern.Any,
      RootExpressionAst {IsSmart: true, Name: var name} when int.TryParse(name, out var index) && index >= 0 && index < parameters.Count =>
        parameters[index].Pattern,
      FallbackExpressionAst fallback => Union([Infer(fallback.Value), Infer(fallback.Fallback)]),
      SelectionExpressionAst selection => Union(selection.Branches.Select(branch => InferResult(branch.Result))
        .Concat([InferResult(selection.Fallback)])),
      _ => HixPattern.Any
    };

    private HixPattern InferResult(HixAst node) => node is ExpressionAst expression ? Infer(expression) :
      new KindHixPattern(HixValueKind.Null);
    private static HixPattern Union(IEnumerable<HixPattern> values) {
      return HixPatterns.Union(values);
    }
  }
}
