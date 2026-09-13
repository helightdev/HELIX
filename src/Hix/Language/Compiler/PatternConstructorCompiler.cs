using System;
using System.Collections.Generic;
using System.Linq;

namespace Hix.Compiler;

/// <summary>Generates ordinary pure language functions for declared type-name calls.</summary>
internal static class PatternConstructorCompiler {
  internal static IEnumerable<FunctionDeclarationIr> Compile(string name, HixPattern pattern,
    IReadOnlyDictionary<string, HixPattern> patterns) {
    HixPatterns.TryStructural(pattern, patterns, out var structural);
    var result = new NamedHixPattern(name);
    ExpressionIr identity = new RootExpressionIr("param");
    if (pattern is TaggedHixPattern tagged) {
      var key = new StringExpressionIr(TaggedHixPattern.FieldName);
      identity = new CallExpressionIr("put", [identity, key,
        new CallExpressionIr("get", [new RootExpressionIr("param"),
          new StringExpressionIr(TaggedHixPattern.FieldName), new StringExpressionIr(tagged.Discriminator)])]);
    }
    yield return Method(name, new FunctionSignature(structural ?? HixPattern.Any, null, result, null), identity);
    if (structural == null) yield break;
    var fields = structural is TableHixPattern table ? table.Fields : ((TupleHixPattern)structural).Fields;
    var inputs = fields.Select(field => new SignatureField(field.Name, field.Pattern, optional: field.Optional,
      defaultValue: field.HasDefault ? Literal(field.DefaultValue) : null)).ToArray();
    ExpressionIr construction;
    if (structural is TableHixPattern) {
      var statements = new List<StatementIr> {
        new AssignmentStatementIr(StorageSpace.Local, "__value", new TableExpressionIr(fields
          .Select((field, index) => new KeyValuePair<string, ExpressionIr>(field.Name, Argument(index))).ToArray()))
      };
      foreach (var (field, index) in fields.Select((field, index) => (field, index))) {
        if (!field.AllowsMissing) continue;
        statements.Add(new AssignmentStatementIr(StorageSpace.Local, "__value",
          new SelectionExpressionIr(null, [new SelectionBranchIr([IsMissing(index)],
            new CallExpressionIr("remove", [Local(), new StringExpressionIr(field.Name)]))], Local())));
      }
      ExpressionIr value = Local();
      if (pattern is TaggedHixPattern tag) value = new CallExpressionIr("put", [value,
        new StringExpressionIr(TaggedHixPattern.FieldName), new CallExpressionIr("get", [Local(),
          new StringExpressionIr(TaggedHixPattern.FieldName), new StringExpressionIr(tag.Discriminator)])]);
      statements.Add(new ControlFlowStatementIr(ControlFlowKind.Return, null, [value]));
      yield return new FunctionDeclarationIr(name, true, false, true,
        [new FunctionSignature(null, inputs, result, null)], new BlockStatementIr(statements), [HixMetadataCompiler.Synthetic()]);
    } else {
      var branches = Enumerable.Range(0, fields.Count).Reverse().Select(index =>
        new SelectionBranchIr([new UnaryExpressionIr(UnaryOperation.Not, IsMissing(index))],
          new TupleExpressionIr(Enumerable.Range(0, index + 1).Select(Argument).ToArray()))).ToArray();
      construction = new SelectionExpressionIr(null, branches, new TupleExpressionIr([]));
      yield return Method(name, new FunctionSignature(null, inputs, result, null), construction);
    }
  }

  private static FunctionDeclarationIr Method(string name, FunctionSignature signature, ExpressionIr result) =>
    new(name, true, false, true, [signature],
      new BlockStatementIr([new ControlFlowStatementIr(ControlFlowKind.Return, null, [result])]), [HixMetadataCompiler.Synthetic()]);
  private static ExpressionIr Argument(int index) => new RootExpressionIr(index.ToString(), true);
  private static ExpressionIr Local() => new MemberExpressionIr(new RootExpressionIr("local"), "__value");
  private static ExpressionIr IsMissing(int index) => new CallExpressionIr("is", [Argument(index), new RootExpressionIr("missing")]);

  private static ExpressionIr Literal(object value) => value switch {
    null or NullHixValue => new NullExpressionIr(),
    MissingHixValue => new MissingExpressionIr(),
    string text => new StringExpressionIr(text),
    LiteralHixValue text => new StringExpressionIr(text.Value.Resolve(null)),
    bool boolean => new BooleanExpressionIr(boolean),
    BooleanHixValue boolean => new BooleanExpressionIr(boolean.Value),
    NumberHixValue number => new NumberExpressionIr(number.Value),
    double number => new NumberExpressionIr(number),
    int number => new NumberExpressionIr(number),
    long number => new NumberExpressionIr(number),
    HixTableValue table => new TableExpressionIr(table.Entries.Select(entry =>
      new KeyValuePair<string, ExpressionIr>(entry.Key.Resolve(null), Literal(entry.Value))).ToArray()),
    TupleHixValue tuple => new TupleExpressionIr(tuple.Values.Select(value => Literal(value)).ToArray()),
    IReadOnlyDictionary<string, object> table => new TableExpressionIr(table.Select(entry =>
      new KeyValuePair<string, ExpressionIr>(entry.Key, Literal(entry.Value))).ToArray()),
    IEnumerable<object> tuple => new TupleExpressionIr(tuple.Select(Literal).ToArray()),
    _ => throw new ArgumentException("pattern defaults must be constant values")
  };
}
