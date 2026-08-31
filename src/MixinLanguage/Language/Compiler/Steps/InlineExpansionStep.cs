using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Mixins.Compiler.Steps;

public sealed class InlineExpansionStep : MixinExpressionCompilerStep {
  public override bool TryTransform(
    MixinCompilerSyntax input, MixinExpressionPreparedState preparedState,
    out MixinCompilerSyntax output, out string error, out int errorLine
  ) {
    if (!TryExpandInlines(
      input.Prelude, input.Expression, preparedState,
      out var prelude, out var expression, out error, out errorLine
    )) {
      output = input;
      return false;
    }
    output = new MixinCompilerSyntax(new ProgramAst(prelude), new ProgramAst(expression));
    return true;
  }

  private static bool TryExpandInlines(
    ProgramAst explicitPrelude, ProgramAst expression,
    MixinExpressionPreparedState preparedState, out IReadOnlyList<InstructionAst> expandedPrelude,
    out IReadOnlyList<InstructionAst> expandedExpression, out string error, out int errorLine
  ) {
    var functions = new Dictionary<string, IReadOnlyList<InstructionAst>>(StringComparer.Ordinal);
    if (!TryCollectFunctions(explicitPrelude, functions, out error, out errorLine) ||
      !TryCollectFunctions(expression, functions, out error, out errorLine)) {
      expandedPrelude = [];
      expandedExpression = [];
      return false;
    }
    var sequence = 0;
    if (!TryExpandProgram(
      explicitPrelude.AvailableInstructions(), functions,
      preparedState?.FunctionEntries, preparedState?.StringPool, new HashSet<string>(StringComparer.Ordinal),
      ref sequence, out expandedPrelude, out error, out errorLine
    )) {
      expandedExpression = [];
      return false;
    }
    return TryExpandProgram(
      expression.AvailableInstructions(), functions,
      preparedState?.FunctionEntries, preparedState?.StringPool, new HashSet<string>(StringComparer.Ordinal),
      ref sequence, out expandedExpression, out error, out errorLine
    );
  }

  private static bool TryCollectFunctions(
    ProgramAst program,
    IDictionary<string, IReadOnlyList<InstructionAst>> functions, out string error, out int errorLine
  ) {
    var instructions = Enumerable.Range(0, program.Count).Select(program.Get).ToArray();
    var localFunctions =
      new Dictionary<string, Compiler.MixinCompiler.FunctionDefinition>(StringComparer.Ordinal);
    if (!Compiler.MixinCompiler.TryIndexSymbols(
      instructions, 0, instructions.Length,
      new Dictionary<string, int>(), new Dictionary<int, int>(), localFunctions,
      new Dictionary<int, int>(), new HashSet<int>(), out error, out errorLine
    )) return false;
    foreach (var function in localFunctions) {
      if (functions.ContainsKey(function.Key)) {
        error = "duplicate function '" + function.Key + "'";
        errorLine = instructions[function.Value.Start - 1].Line;
        return false;
      }
      functions.Add(
        function.Key,
        [.. instructions.Skip(function.Value.Start).Take(function.Value.End - function.Value.Start)]
      );
    }
    error = null;
    errorLine = 0;
    return true;
  }

  private static bool TryExpandProgram(
    IEnumerable<InstructionAst> source,
    IReadOnlyDictionary<string, IReadOnlyList<InstructionAst>> functions,
    IReadOnlyDictionary<MixinString, int> importedFunctions, MixinStringPool importedStrings,
    ISet<string> activeFunctions, ref int sequence, out IReadOnlyList<InstructionAst> expanded,
    out string error, out int errorLine
  ) {
    var result = new List<InstructionAst>();
    foreach (var instruction in source) {
      if (instruction is not InlineAst inline) {
        result.Add(instruction);
        continue;
      }
      var name = inline.Name ?? "";
      if (!functions.TryGetValue(name, out var body)) {
        if (importedFunctions is not null && importedFunctions.Keys.Any(key =>
          string.Equals(key.Resolve(importedStrings), name, StringComparison.Ordinal)
        )) {
          result.Add(instruction);
          continue;
        }
        expanded = [];
        error = "unknown inline function '" + name + "'";
        errorLine = instruction.Line;
        return false;
      }
      if (!activeFunctions.Add(name)) {
        expanded = [];
        error = "recursive inline function '" + name + "'";
        errorLine = instruction.Line;
        return false;
      }
      var suffix = "__inline_" + sequence++.ToString(CultureInfo.InvariantCulture);
      var endLabel = suffix + "_end";
      var labels = body.Select(LabelOf).Where(item => !string.IsNullOrEmpty(item))
        .Distinct(StringComparer.Ordinal).ToDictionary(item => item, item => item + suffix, StringComparer.Ordinal);
      var bodyNodes = new List<InstructionAst>();
      foreach (var item in body) {
        switch (item) {
          case ReturnAst returned:
            if (!IsEmpty(returned.Expression))
              bodyNodes.Add(new LocalAst(suffix + "_return", returned.Expression).InheritFrom(item));
            bodyNodes.Add(new GotoAst(endLabel).InheritFrom(item));
            continue;
          case ScopeAst or LabelAst or GotoAst or MatchAst
            when LabelOf(item) is { Length: > 0 } label && labels.TryGetValue(label, out var renamed):
            bodyNodes.Add(RenameLabel(item, renamed));
            continue;
          default: bodyNodes.Add(item); break;
        }
      }
      if (!TryExpandProgram(
        bodyNodes, functions, importedFunctions, importedStrings, activeFunctions,
        ref sequence, out var expandedBody, out error, out errorLine
      )) {
        expanded = [];
        activeFunctions.Remove(name);
        return false;
      }
      activeFunctions.Remove(name);
      result.AddRange(expandedBody);
      result.Add(new ScopeAst(endLabel).InheritFrom(inline));
    }
    expanded = result.AsReadOnly();
    error = null;
    errorLine = 0;
    return true;
  }

  private static string LabelOf(InstructionAst ast) {
    return ast switch {
      ScopeAst item => item.Label, LabelAst item => item.Name,
      GotoAst item => item.Label, MatchAst item => item.FailureLabel, _ => null
    };
  }

  private static InstructionAst RenameLabel(InstructionAst ast, string label) {
    return ast switch {
      ScopeAst item => new ScopeAst(label).InheritFrom(item),
      LabelAst item => new LabelAst(label).InheritFrom(item),
      GotoAst item => new GotoAst(label).InheritFrom(item),
      MatchAst item => new MatchAst(label, item.Expression).InheritFrom(item), _ => ast
    };
  }

  private static bool IsEmpty(IReadOnlyList<ValueAst> expression) {
    return expression is null || expression.All(item => item.Reference is null && string.IsNullOrEmpty(item.Literal));
  }
}