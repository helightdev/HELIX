using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MixinLanguage.Compiler;

public static partial class MixinExpressionCompiler {
  private sealed class InlineExpansionStep : MixinExpressionCompilerStep {
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
      output = new MixinCompilerSyntax(new MixinProgramSyntax(prelude), new MixinProgramSyntax(expression));
      return true;
    }

    private static bool TryExpandInlines(
      MixinProgramSyntax explicitPrelude, MixinProgramSyntax expression,
      MixinExpressionPreparedState preparedState, out IReadOnlyList<DirectiveInstruction> expandedPrelude,
      out IReadOnlyList<DirectiveInstruction> expandedExpression, out string error, out int errorLine
    ) {
      var functions = new Dictionary<string, IReadOnlyList<DirectiveInstruction>>(StringComparer.Ordinal);
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
      MixinProgramSyntax program,
      IDictionary<string, IReadOnlyList<DirectiveInstruction>> functions, out string error, out int errorLine
    ) {
      var instructions = Enumerable.Range(0, program.Count).Select(program.Get).ToArray();
      var localFunctions = new Dictionary<string, FunctionDefinition>(StringComparer.Ordinal);
      if (!TryIndexSymbols(
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
      IEnumerable<DirectiveInstruction> source,
      IReadOnlyDictionary<string, IReadOnlyList<DirectiveInstruction>> functions,
      IReadOnlyDictionary<MixinString, int> importedFunctions, MixinStringPool importedStrings,
      ISet<string> activeFunctions, ref int sequence, out IReadOnlyList<DirectiveInstruction> expanded,
      out string error, out int errorLine
    ) {
      var result = new List<DirectiveInstruction>();
      foreach (var instruction in source) {
        if (instruction is not InlineDirectiveSyntax inline) {
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
        var bodyNodes = new List<DirectiveInstruction>();
        foreach (var item in body) {
          switch (item) {
            case ReturnDirectiveSyntax returned:
              if (!IsEmpty(returned.Expression))
                bodyNodes.Add(new LocalDirectiveSyntax(item.Line, suffix + "_return", returned.Expression));
              bodyNodes.Add(new GotoDirectiveSyntax(item.Line, endLabel));
              continue;
            case ScopeDirectiveSyntax or LabelDirectiveSyntax or GotoDirectiveSyntax or MatchDirectiveSyntax
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
        result.Add(new ScopeDirectiveSyntax(inline.Line, endLabel));
      }
      expanded = result.AsReadOnly();
      error = null;
      errorLine = 0;
      return true;
    }

    private static string LabelOf(DirectiveInstruction instruction) {
      return instruction switch {
        ScopeDirectiveSyntax item => item.Label, LabelDirectiveSyntax item => item.Name,
        GotoDirectiveSyntax item => item.Label, MatchDirectiveSyntax item => item.FailureLabel, _ => null
      };
    }

    private static DirectiveInstruction RenameLabel(DirectiveInstruction instruction, string label) {
      return instruction switch {
        ScopeDirectiveSyntax item => new ScopeDirectiveSyntax(item.Line, label),
        LabelDirectiveSyntax item => new LabelDirectiveSyntax(item.Line, label),
        GotoDirectiveSyntax item => new GotoDirectiveSyntax(item.Line, label),
        MatchDirectiveSyntax item => new MatchDirectiveSyntax(item.Line, label, item.Expression), _ => instruction
      };
    }

    private static bool IsEmpty(IReadOnlyList<IMixinValue> expression) {
      return expression is null || expression.All(item => item.Reference is null && string.IsNullOrEmpty(item.Literal));
    }
  }
}