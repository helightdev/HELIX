using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace HelixSourceGenerator.Language;

using static MixinExpressionEvaluator;
using static MixinExpressionInterpreter;

public static class MixinExpressionCompiler {
  private static readonly HashSet<MixinExpressionRoot> RoslynRoots = [
    MixinExpressionRoot.Target, MixinExpressionRoot.This, MixinExpressionRoot.Attribute, MixinExpressionRoot.Argument
  ];

  public static string RewriteTargetAsThis(string expression) {
    if (string.IsNullOrEmpty(expression)) return expression ?? "";
    var builder = new StringBuilder(expression.Length);
    for (var index = 0; index < expression.Length;) {
      var rootStart = -1;
      if (expression[index] == '@') {
        if (MatchesRoot(expression, index + 1, "target")) rootStart = index + 1;
        else if (index + 1 < expression.Length && expression[index + 1] == '(' &&
          MatchesRoot(expression, index + 2, "target")) rootStart = index + 2;
      }
      if (rootStart < 0) {
        builder.Append(expression[index++]);
        continue;
      }
      builder.Append(expression, index, rootStart - index).Append("this");
      index = rootStart + "target".Length;
    }
    return builder.ToString();
  }

  private static bool MatchesRoot(string expression, int start, string root) {
    if (start + root.Length > expression.Length ||
      string.CompareOrdinal(expression, start, root, 0, root.Length) != 0) return false;
    var end = start + root.Length;
    return end == expression.Length || !(char.IsLetterOrDigit(expression[end]) || expression[end] == '_');
  }

  public static bool TryHoistPrelude(
    string explicitPrelude,
    string expression,
    MixinExpressionPreparedState preparedState,
    out string prelude,
    out string lateExpression,
    out string error,
    out int errorLine
  ) {
    if (!TryExpandInlines(
      explicitPrelude, expression, preparedState,
      out explicitPrelude, out expression, out error, out errorLine
    )) {
      prelude = explicitPrelude ?? "";
      lateExpression = expression ?? "";
      return false;
    }
    var generated = new List<string>();
    var labels = new Dictionary<string, string>(StringComparer.Ordinal);
    var structuralLocals = new HashSet<string>(StringComparer.Ordinal);
    var late = new List<string>();
    error = null;
    errorLine = 0;
    var lines = MixinExpressionParser.SplitLines(expression ?? "");
    var localFunctions = new HashSet<string>(StringComparer.Ordinal);
    for (var index = 0; index < lines.Length; index++) {
      var candidate = MixinExpressionParser.ParseDirective(lines[index], index + 1);
      if (candidate.Error is null && candidate.Opcode == DirectiveOpcode.Function)
        localFunctions.Add(candidate.Argument);
    }
    for (var index = 0; index < lines.Length; index++) {
      var line = lines[index];
      var parsed = MixinExpressionParser.ParseDirective(line, index + 1);
      if (parsed.Error is not null) {
        prelude = explicitPrelude ?? "";
        lateExpression = expression ?? "";
        error = parsed.Error;
        errorLine = index + 1;
        return false;
      }
      if (parsed.Opcode == DirectiveOpcode.Call && (
        parsed.Arguments.Count == 0 ||
        MixinExpressionParser.IsDynamicArgument(parsed.Arguments[0]) ||
        !localFunctions.Contains(parsed.Arguments[0])
      )) {
        prelude = explicitPrelude ?? "";
        lateExpression = expression ?? "";
        error = "Prelude-model expressions may only call functions declared in the same expression; imported call '" +
          (parsed.Arguments.Count == 0 ? "" : parsed.Arguments[0]) + "' is not supported";
        errorLine = index + 1;
        return false;
      }
      if (parsed.Opcode is DirectiveOpcode.PropStruct or DirectiveOpcode.AugmentStruct
        or DirectiveOpcode.ResolveMixin) {
        if (!TryHoistStructuralDirective(parsed, line, generated, labels, structuralLocals, out error)) {
          prelude = explicitPrelude ?? "";
          lateExpression = expression ?? "";
          errorLine = index + 1;
          return false;
        }
        late.Add("");
        continue;
      }
      if (!TryRewriteRoslynReferences(line, generated, labels, structuralLocals, out var rewritten, out error)) {
        prelude = explicitPrelude ?? "";
        lateExpression = expression ?? "";
        errorLine = index + 1;
        return false;
      }
      late.Add(SerializeLogicalLine(rewritten));
    }
    var parts = new List<string>();
    if (!string.IsNullOrWhiteSpace(explicitPrelude)) parts.Add(explicitPrelude);
    if (generated.Count != 0) parts.Add(string.Join("\n", generated));
    prelude = string.Join("\n", parts);
    lateExpression = string.Join("\n", late);
    return true;
  }

  private static bool TryExpandInlines(
    string explicitPrelude,
    string expression,
    MixinExpressionPreparedState preparedState,
    out string expandedPrelude,
    out string expandedExpression,
    out string error,
    out int errorLine
  ) {
    var functions = new Dictionary<string, IReadOnlyList<DirectiveInstruction>>(StringComparer.Ordinal);
    if (preparedState is not null) {
      foreach (var function in preparedState.Functions) {
        functions[function.Key] = [
          .. preparedState.Instructions
            .Skip(function.Value.Start).Take(function.Value.End - function.Value.Start)
        ];
      }
    }
    if (!TryCollectInlineFunctions(explicitPrelude, functions, out error, out errorLine) ||
      !TryCollectInlineFunctions(expression, functions, out error, out errorLine)) {
      expandedPrelude = explicitPrelude ?? "";
      expandedExpression = expression ?? "";
      return false;
    }
    var inlineSequence = 0;
    if (!TryExpandInlineProgram(
      explicitPrelude, functions, new HashSet<string>(StringComparer.Ordinal), ref inlineSequence,
      out expandedPrelude, out error, out errorLine
    )) {
      expandedExpression = expression ?? "";
      return false;
    }
    return TryExpandInlineProgram(
      expression, functions, new HashSet<string>(StringComparer.Ordinal), ref inlineSequence,
      out expandedExpression, out error, out errorLine
    );
  }

  private static bool TryCollectInlineFunctions(
    string source,
    IDictionary<string, IReadOnlyList<DirectiveInstruction>> functions,
    out string error,
    out int errorLine
  ) {
    var program = GetProgram(source ?? "", true);
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

  private static bool TryExpandInlineProgram(
    string source,
    IReadOnlyDictionary<string, IReadOnlyList<DirectiveInstruction>> functions,
    ISet<string> activeFunctions,
    ref int inlineSequence,
    out string expanded,
    out string error,
    out int errorLine
  ) {
    var result = new List<string>();
    var program = GetProgram(source ?? "", true);
    for (var index = 0; index < program.Count; index++) {
      var instruction = program.Get(index);
      if (instruction.Error is not null) {
        expanded = source ?? "";
        error = instruction.Error;
        errorLine = instruction.Line;
        return false;
      }
      if (instruction.Opcode != DirectiveOpcode.Inline) {
        result.Add(SerializeInstruction(instruction));
        continue;
      }
      var name = instruction.Argument ?? "";
      if (!functions.TryGetValue(name, out var body)) {
        expanded = source ?? "";
        error = "unknown inline function '" + name + "'";
        errorLine = instruction.Line;
        return false;
      }
      if (!activeFunctions.Add(name)) {
        expanded = source ?? "";
        error = "recursive inline function '" + name + "'";
        errorLine = instruction.Line;
        return false;
      }
      var suffix = "__inline_" + inlineSequence++.ToString(CultureInfo.InvariantCulture);
      var endLabel = suffix + "_end";
      var labels = body.Where(item => item.Opcode == DirectiveOpcode.Scope && !string.IsNullOrEmpty(item.Argument))
        .Select(item => item.Argument).Distinct(StringComparer.Ordinal)
        .ToDictionary(item => item, item => item + suffix, StringComparer.Ordinal);
      var bodyLines = new List<string>();
      foreach (var item in body) {
        switch (item.Opcode) {
          case DirectiveOpcode.Return: {
            if (!string.IsNullOrEmpty(item.Operand))
              bodyLines.Add("@LOCAL<" + suffix + "_return> " + item.Operand);
            bodyLines.Add("@GOTO<" + endLabel + ">");
            continue;
          }
          case DirectiveOpcode.Scope or DirectiveOpcode.Goto or DirectiveOpcode.Match
            when !string.IsNullOrEmpty(item.Argument) &&
            labels.TryGetValue(item.Argument, out var renamed):
            bodyLines.Add(SerializeInstruction(item, renamed));
            continue;
          default: bodyLines.Add(SerializeInstruction(item)); break;
        }
      }
      var bodySource = string.Join("\n", bodyLines);
      if (!TryExpandInlineProgram(
        bodySource, functions, activeFunctions, ref inlineSequence,
        out var expandedBody, out error, out errorLine
      )) {
        expanded = source ?? "";
        activeFunctions.Remove(name);
        return false;
      }
      activeFunctions.Remove(name);
      if (!string.IsNullOrEmpty(expandedBody)) result.Add(expandedBody);
      result.Add("@SCOPE<" + endLabel + ">");
    }
    expanded = string.Join("\n", result);
    error = null;
    errorLine = 0;
    return true;
  }

  private static string SerializeInstruction(DirectiveInstruction instruction, string argument = null) {
    if (string.IsNullOrEmpty(instruction.Command)) return instruction.Operand ?? "";
    var builder = new StringBuilder("@").Append(instruction.Command);
    for (var index = 0; index < instruction.Arguments.Count; index++)
      builder.Append('<').Append(index == 0 && argument is not null ? argument : instruction.Arguments[index])
        .Append('>');
    if (!string.IsNullOrEmpty(instruction.Operand)) builder.Append(' ').Append(instruction.Operand);
    return SerializeLogicalLine(builder.ToString());
  }

  private static string SerializeLogicalLine(string line) {
    return (line ?? "").Replace("\n", "\n@\\");
  }

  private static bool TryHoistStructuralDirective(
    DirectiveInstruction instruction,
    string line,
    ICollection<string> generated,
    IDictionary<string, string> labels,
    ISet<string> structuralLocals,
    out string error
  ) {
    var local = instruction.Opcode switch {
      DirectiveOpcode.PropStruct when instruction.Arguments.Count >= 2 => instruction.Arguments[1],
      DirectiveOpcode.AugmentStruct when instruction.Arguments.Count >= 1 => instruction.Arguments[0],
      DirectiveOpcode.ResolveMixin when instruction.Arguments.Count >= 1 => instruction.Arguments[0],
      _ => null
    };
    if (string.IsNullOrEmpty(local) || MixinExpressionParser.IsDynamicArgument(local)) {
      error = "@" + instruction.Command + " cannot be hoisted because its result local is dynamic";
      return false;
    }
    if (!TryRewriteRoslynReferences(
      line, generated, labels, structuralLocals, out var rewritten, out error
    )) return false;
    generated.Add(rewritten);
    var access = "@local#" + local;
    var label = "__" + labels.Count.ToString(CultureInfo.InvariantCulture);
    labels[access] = label;
    generated.Add("@CARRY<" + label + "> " + access);
    structuralLocals.Add(local);
    return true;
  }

  private static bool TryRewriteRoslynReferences(
    string text,
    ICollection<string> generated,
    IDictionary<string, string> labels,
    ISet<string> structuralLocals,
    out string rewritten,
    out string error
  ) {
    var builder = new StringBuilder(text.Length);
    var position = 0;
    error = null;
    while (position < text.Length) {
      if (text[position] != '@' || (position + 1 < text.Length && text[position + 1] == '@')) {
        builder.Append(text[position++]);
        continue;
      }
      var start = position;
      if (!TryReadReferenceNode(text, ref position, out var reference, out _)) {
        builder.Append(text[start]);
        position = start + 1;
        continue;
      }
      var roslyn = RoslynRoots.Contains(reference.Root) ||
        (reference.Root == MixinExpressionRoot.Local &&
          structuralLocals?.Contains(reference.Member ?? "") == true);
      if (!roslyn) {
        builder.Append(text, start, position - start);
        continue;
      }
      var access = text.Substring(start, position - start);
      if (!labels.TryGetValue(access, out var label)) {
        label = "__" + labels.Count.ToString(CultureInfo.InvariantCulture);
        labels.Add(access, label);
        generated.Add("@CARRY<" + label + "> " + access);
      }
      builder.Append(access.StartsWith("@(", StringComparison.Ordinal) ? "@(carry#" + label + ")" : "@carry#" + label);
    }
    rewritten = builder.ToString();
    return true;
  }

  internal static void HoistLateCarries(
    string expression,
    string lateExpression,
    out string primary,
    out string late
  ) {
    primary = expression ?? "";
    late = lateExpression;
    if (string.IsNullOrEmpty(lateExpression)) return;

    var carries = new List<string>();
    var remaining = new List<string>();
    var lines = MixinExpressionParser.SplitLines(lateExpression);
    for (var index = 0; index < lines.Length; index++) {
      var line = lines[index];
      if (!string.IsNullOrWhiteSpace(line)) {
        var parsed = MixinExpressionParser.ParseDirective(line, index + 1);
        if (parsed.Error is null && parsed.Opcode == DirectiveOpcode.Carry) {
          carries.Add(line);
          continue;
        }
      }
      remaining.Add(line);
    }
    if (carries.Count == 0) return;
    primary = string.Join("\n", carries) + (primary.Length == 0 ? "" : "\n" + primary);
    late = string.Join("\n", remaining);
  }

  internal static MixinExpressionValidationResult ValidateSyntax(
    string expression, bool functionsOnly
  ) {
    if (expression is null) return ValidationFailure("the expression is null", 0);
    var lines = MixinExpressionParser.SplitLines(expression);
    string activeFunction = null;
    var functionLine = 0;
    var functionScopeOpen = false;
    var functions = new HashSet<string>(StringComparer.Ordinal);
    for (var index = 0; index < lines.Length; index++) {
      var line = lines[index];
      if (string.IsNullOrWhiteSpace(line)) continue;
      var parsed = MixinExpressionParser.ParseDirective(line, index + 1);
      if (parsed.Error is not null) return ValidationFailure(parsed.Error, index + 1);
      var command = parsed.Command;
      var argument = parsed.Argument;
      if (activeFunction is null) {
        if (command != "FUNC") {
          if (functionsOnly)
            return ValidationFailure("mixin libraries may only contain function declarations", index + 1);
          continue;
        }
        if (!functions.Add(argument))
          return ValidationFailure("duplicate function '" + argument + "'", index + 1);
        activeFunction = argument;
        functionLine = index + 1;
        functionScopeOpen = false;
        continue;
      }
      switch (command) {
        case "FUNC": return ValidationFailure("functions may not be nested", index + 1);
        case "SCOPE": functionScopeOpen = true; break;
      }
      if (command != "END") continue;
      if (functionScopeOpen) functionScopeOpen = false;
      else activeFunction = null;
    }
    return activeFunction is null
      ? new MixinExpressionValidationResult(true, null, 0)
      : ValidationFailure("unterminated function '" + activeFunction + "'", functionLine);
  }

  internal static MixinExpressionPreparedState PrepareGlobals(IEnumerable<string> expressions) {
    var programs = new List<MixinProgramSyntax>();
    foreach (var expression in expressions ?? []) {
      var validation = ValidateSyntax(expression, false);
      if (!validation.Success) {
        throw new ArgumentException(
          "invalid prepared expression at line " + validation.ErrorLine + ": " + validation.Error,
          nameof(expressions)
        );
      }
      programs.Add(GetProgram(expression, true));
    }
    return PrepareGlobals(programs);
  }

  internal static MixinExpressionPreparedState PrepareGlobals(
    IReadOnlyList<MixinProgramSyntax> programs
  ) {
    programs ??= [];
    var poolBuilder = new MixinStringPoolBuilder();
    foreach (var program in programs) program.CollectConstants(poolBuilder);
    var stringPool = poolBuilder.Freeze();
    var variables = new MixinValueDictionary(stringPool);
    var logs = new List<MixinExpressionPreparedLog>();
    var programIndex = 0;
    var executedOperations = 0;
    foreach (var program in programs) {
      if (!TryEvaluatePreparedInitializers(
        program, programIndex, variables, logs, ref executedOperations,
        out var error, out var line
      )) {
        throw new ArgumentException(
          "invalid prepared expression at line " + line + ": " + error, nameof(programs)
        );
      }
      programIndex++;
    }
    var instructions = programs.SelectMany(program =>
      Enumerable.Range(0, program.Count).Select(program.Get)
    ).ToArray();
    var labels = new Dictionary<string, int>(StringComparer.Ordinal);
    var instructionScopes = new Dictionary<int, int>();
    var functions = new Dictionary<string, FunctionDefinition>(StringComparer.Ordinal);
    var functionStarts = new Dictionary<int, int>();
    var functionEnds = new HashSet<int>();
    var instructionOffset = 0;
    foreach (var program in programs) {
      if (!TryIndexSymbols(
        instructions, instructionOffset, instructionOffset + program.Count,
        labels, instructionScopes, functions, functionStarts, functionEnds,
        out var symbolError, out var symbolLine
      )) {
        throw new ArgumentException(
          "invalid prepared expression at line " + symbolLine + ": " + symbolError,
          nameof(programs)
        );
      }
      instructionOffset += program.Count;
    }
    var initializers = FindPreparedInitializers(instructions);
    return new MixinExpressionPreparedState(
      stringPool,
      [.. programs],
      new MixinValueDictionary(variables, stringPool),
      instructions,
      new MixinStringDictionary<int>(labels, stringPool),
      new Dictionary<int, int>(instructionScopes),
      new MixinStringDictionary<FunctionDefinition>(functions, stringPool),
      new Dictionary<int, int>(functionStarts),
      new HashSet<int>(functionEnds),
      initializers,
      logs.AsReadOnly(),
      executedOperations
    );
  }

  private static ISet<int> FindPreparedInitializers(IReadOnlyList<DirectiveInstruction> instructions) {
    var result = new HashSet<int>();
    var depth = 0;
    var scope = false;
    for (var index = 0; index < instructions.Count; index++) {
      var instruction = instructions[index];
      if (instruction.Command == "FUNC") {
        depth++;
        scope = false;
        continue;
      }
      if (depth != 0) {
        if (instruction.Command == "SCOPE") scope = true;
        else if (instruction.Command == "END") {
          if (scope) scope = false;
          else depth--;
        }
        continue;
      }
      if (instruction.Command is "VAR" or "LOG") result.Add(index);
    }
    return result;
  }

  private static bool TryEvaluatePreparedInitializers(
    MixinProgramSyntax program,
    int programIndex,
    IDictionary<string, object> variables,
    ICollection<MixinExpressionPreparedLog> logs,
    ref int executedOperations,
    out string error,
    out int line
  ) {
    error = null;
    line = 0;
    var functionDepth = 0;
    var functionScope = false;
    for (var i = 0; i < program.Count; i++) {
      var instruction = program.Get(i);
      if (instruction.Error is not null) {
        error = instruction.Error;
        line = instruction.Line;
        return false;
      }
      if (instruction.Command == "FUNC") {
        functionDepth++;
        functionScope = false;
        continue;
      }
      if (functionDepth != 0) {
        if (instruction.Command == "SCOPE") functionScope = true;
        else if (instruction.Command == "END") {
          if (functionScope) functionScope = false;
          else functionDepth--;
        }
        continue;
      }
      if (instruction.Command == "VAR") {
        executedOperations++;
        if (!TryInterpolatePrepared(instruction.ValueExpression, variables, out var value, out error)) {
          line = instruction.Line;
          return false;
        }
        variables[instruction.Argument] = value;
      } else if (instruction.Command == "LOG") {
        executedOperations++;
        if (!TryInterpolatePrepared(instruction.ValueExpression, variables, out var value, out error)) {
          line = instruction.Line;
          return false;
        }
        logs.Add(new MixinExpressionPreparedLog(value, instruction.Line, programIndex));
      }
    }
    return true;
  }

  private static bool TryInterpolatePrepared(
    IReadOnlyList<ValueExpressionPart> expression,
    IDictionary<string, object> variables,
    out string result,
    out string error
  ) {
    var builder = new StringBuilder();
    error = null;
    foreach (var part in expression) {
      if (part.Reference is null) {
        builder.Append(part.Literal);
        continue;
      }
      var reference = part.Reference;
      if (reference.Root != MixinExpressionRoot.Variable || string.IsNullOrEmpty(reference.Member) ||
        !variables.TryGetValue(reference.Member, out var value)) {
        result = null;
        error = "prepared global initializers may only reference an existing @var value";
        return false;
      }
      if (reference.Properties.Count != 0) {
        result = null;
        error = "prepared global initializer references cannot have properties";
        return false;
      }
      builder.Append(RenderValue(value));
    }
    result = builder.ToString();
    return true;
  }

  internal static bool ValidateBooleanExpressionSyntax(string text, out string error) {
    error = null;
    var position = 0;
    var found = false;
    while (position < text.Length) {
      while (position < text.Length && char.IsWhiteSpace(text[position])) position++;
      if (position == text.Length) break;
      found = true;
      if (!TryReadReferenceNode(text, ref position, out _, out error)) return false;
    }
    if (found) return true;
    error = "boolean expression is empty";
    return false;
  }

  private static MixinExpressionValidationResult ValidationFailure(string error, int line) {
    return new MixinExpressionValidationResult(false, error, line);
  }

  internal static bool ValidateValueExpressionSyntax(string text, out string error) {
    error = null;
    var position = 0;
    while (position < text.Length) {
      if (text[position] != '@') {
        position++;
        continue;
      }
      if (position + 1 < text.Length && text[position + 1] == '@') {
        position += 2;
        continue;
      }
      if (!TryReadReferenceNode(text, ref position, out _, out error)) return false;
    }
    return true;
  }

  private static void AddScopeLabel(
    string command,
    string argument,
    int index,
    int scope,
    IDictionary<string, int> labels,
    out string error
  ) {
    error = null;
    if (command != "SCOPE" || string.IsNullOrEmpty(argument)) return;
    var key = ScopeLabelKey(scope, argument);
    if (labels.ContainsKey(key)) {
      error = "duplicate scope label '" + argument + "'";
      return;
    }
    labels.Add(key, index);
  }

  internal static string ScopeLabelKey(int scope, string label) {
    return scope + "\0" + label;
  }

  internal static int FindNextScopeOrEnd(
    IReadOnlyList<DirectiveInstruction> lines,
    int start,
    int scope,
    IReadOnlyDictionary<int, int> instructionScopes,
    IReadOnlyDictionary<int, int> functionStarts,
    ISet<int> functionEnds
  ) {
    for (var index = start; index < lines.Count; index++) {
      if (!instructionScopes.TryGetValue(index, out var candidateScope) || candidateScope != scope) return -1;
      if (functionStarts.TryGetValue(index, out var functionEnd)) {
        index = functionEnd;
        continue;
      }
      var command = lines[index].Command;
      if (string.IsNullOrEmpty(command)) continue;
      if (command == "SCOPE") return index;
      if (command == "END") return functionEnds.Contains(index) ? index : index + 1;
    }
    return -1;
  }

  internal static bool TryIndexSymbols(
    IReadOnlyList<DirectiveInstruction> lines,
    int start,
    int end,
    IDictionary<string, int> labels,
    IDictionary<int, int> instructionScopes,
    IDictionary<string, FunctionDefinition> functions,
    IDictionary<int, int> functionStarts,
    ISet<int> functionEnds,
    out string error,
    out int errorLine
  ) {
    error = null;
    errorLine = 0;
    string activeFunction = null;
    var functionStart = -1;
    var functionScopeOpen = false;
    for (var index = start; index < end; index++) {
      var instruction = lines[index];
      // Expression and function regions use disjoint ID ranges, including when both begin at 0.
      var scope = activeFunction is null ? -start - 1 : functionStart + 1;
      instructionScopes[index] = scope;
      if (instruction.Error is not null) {
        error = instruction.Error;
        errorLine = instruction.Line;
        return false;
      }
      var command = instruction.Command;
      var argument = instruction.Argument;
      if (string.IsNullOrEmpty(command)) continue;
      if (activeFunction is not null) {
        if (command == "FUNC") {
          error = "functions may not be nested";
          errorLine = instruction.Line;
          return false;
        }
        if (command == "SCOPE") functionScopeOpen = true;
        if (command != "END") {
          AddScopeLabel(command, argument, index, scope, labels, out error);
          if (error is not null) {
            errorLine = instruction.Line;
            return false;
          }
          continue;
        }
        if (functionScopeOpen) {
          functionScopeOpen = false;
          continue;
        }
        functions.Add(activeFunction, new FunctionDefinition(functionStart + 1, index));
        functionStarts.Add(functionStart, index);
        functionEnds.Add(index);
        activeFunction = null;
        continue;
      }
      if (command == "FUNC") {
        if (functions.ContainsKey(argument)) {
          error = "duplicate function '" + argument + "'";
          errorLine = instruction.Line;
          return false;
        }
        activeFunction = argument;
        functionStart = index;
        functionScopeOpen = false;
        continue;
      }
      AddScopeLabel(command, argument, index, scope, labels, out error);
      if (error is null) continue;
      errorLine = instruction.Line;
      return false;
    }
    if (activeFunction is null) return true;
    error = "unterminated function '" + activeFunction + "'";
    errorLine = lines[functionStart].Line;
    return false;
  }

  public sealed record FunctionDefinition(int Start, int End);
}