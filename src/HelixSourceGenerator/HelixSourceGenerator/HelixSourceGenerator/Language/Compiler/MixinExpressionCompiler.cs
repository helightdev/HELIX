using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using HelixSourceGenerator.Language.Functions;

namespace HelixSourceGenerator.Language.Compiler;

using static MixinExpressionVirtualMachine;

public static class MixinExpressionCompiler {
  internal static MixinProgramSyntax GetProgram(string expression) => new(expression);

  public static MixinExpressionValidationResult ValidateSyntax(string expression) =>
    MixinExpressionParser.ValidateSyntax(expression, false);

  internal static MixinExpressionValidationResult ValidateFunctionLibrary(string expression) =>
    MixinExpressionParser.ValidateSyntax(expression, true);

  public static bool TryParseReference(string text, out MixinExpressionReference reference, out string error) {
    reference = null;
    error = null;
    if (text is null) { error = "reference is null"; return false; }
    var position = 0;
    if (TryReadReferenceNode(text, ref position, out reference, out error) &&
      position == text.Length) return true;
    error ??= "unexpected text after expression reference";
    reference = null;
    return false;
  }

  internal static bool TryReadReferenceNode(
    string text, ref int position, out MixinExpressionReference reference, out string error
  ) {
    reference = null;
    error = null;
    if (position >= text.Length || text[position] != '@') {
      error = "expected '@' expression reference";
      return false;
    }
    position++;
    var parenthesized = position < text.Length && text[position] == '(';
    if (parenthesized) position++;
    MixinExpressionRoot root;
    if (position < text.Length && text[position] == ':') root = MixinExpressionRoot.Null;
    else {
      var rootStart = position;
      while (position < text.Length && (char.IsLetterOrDigit(text[position]) || text[position] == '_')) position++;
      if (position == rootStart) { error = "expression reference has no root"; return false; }
      var keyword = text.Substring(rootStart, position - rootStart);
      if (!TryParseRoot(keyword, out root)) { error = "unknown expression root '@" + keyword + "'"; return false; }
    }
    string member = null;
    if (position < text.Length && text[position] == '#') {
      position++;
      var start = position;
      while (position < text.Length && (char.IsLetterOrDigit(text[position]) || text[position] == '_')) position++;
      if (position == start) { error = "member reference is empty"; return false; }
      member = text.Substring(start, position - start);
    }
    var properties = new List<MixinExpressionProperty>();
    while (position < text.Length && (text[position] == ':' || text[position] == '#')) {
      if (text[position] == '#') {
        position++;
        var start = position;
        while (position < text.Length && (char.IsLetterOrDigit(text[position]) || text[position] == '_')) position++;
        if (position == start) { error = "type argument path is empty"; return false; }
        properties.Add(new MixinExpressionProperty("path", text.Substring(start, position - start)));
        continue;
      }
      position++;
      var negated = false;
      if (position < text.Length && text[position] == '!') { negated = true; position++; }
      if (position < text.Length && text[position] == '?') position++;
      var propertyStart = position;
      while (position < text.Length && (char.IsLetterOrDigit(text[position]) || text[position] == '_')) position++;
      if (position == propertyStart) { error = "property name is empty"; return false; }
      var name = text.Substring(propertyStart, position - propertyStart);
      var arguments = new List<string>();
      while (position < text.Length && text[position] == '<') {
        position++;
        var argumentStart = position;
        var depth = 1;
        while (position < text.Length && depth != 0) {
          if (text[position] == '<') depth++;
          else if (text[position] == '>') depth--;
          if (depth != 0) position++;
        }
        if (depth != 0) { error = "unterminated property argument"; return false; }
        arguments.Add(text.Substring(argumentStart, position - argumentStart));
        position++;
      }
      var parsedArguments = arguments.Select(argument => ParsePropertyArgument(name, argument)).ToArray();
      var property = new MixinExpressionProperty(name, arguments.AsReadOnly(), parsedArguments, negated);
      if (FunctionLibrary.TryGet(name, out var function) && !function.Validate(property, out error)) return false;
      properties.Add(property);
    }
    for (var index = 0; index + 1 < properties.Count; index++) {
      if (!FunctionLibrary.IsPredicate(properties[index].Name)) continue;
      error = "boolean operation ':" + properties[index].Name + "' must be terminal";
      return false;
    }
    if (parenthesized) {
      if (position >= text.Length || text[position] != ')') { error = "unterminated parenthesized reference"; return false; }
      position++;
    }
    reference = new MixinExpressionReference(root, member, properties.AsReadOnly());
    return true;
  }

  private static MixinPropertyArgumentSyntax ParsePropertyArgument(string property, string argument) {
    if (property is "and" or "or" && MixinExpressionParser.IsDynamicArgument(argument))
      return new MixinPropertyArgumentSyntax(
        null, null,
        MixinExpressionParser.ParseBooleanExpression(argument.Substring(1, argument.Length - 2))
      );
    if (MixinExpressionParser.IsDynamicArgument(argument))
      return new MixinPropertyArgumentSyntax(
        null, MixinExpressionParser.ParseValueExpression(argument.Substring(1, argument.Length - 2)), null
      );
    return new MixinPropertyArgumentSyntax(argument, null, null);
  }

  private static bool TryParseRoot(string keyword, out MixinExpressionRoot root) {
    switch (keyword) {
      case "target": root = MixinExpressionRoot.Target; return true;
      case "this": root = MixinExpressionRoot.This; return true;
      case "attr": root = MixinExpressionRoot.Attribute; return true;
      case "arg": root = MixinExpressionRoot.Argument; return true;
      case "var": root = MixinExpressionRoot.Variable; return true;
      case "local": root = MixinExpressionRoot.Local; return true;
      case "true": root = MixinExpressionRoot.True; return true;
      case "false": root = MixinExpressionRoot.False; return true;
      case "null": root = MixinExpressionRoot.Null; return true;
      case "table": root = MixinExpressionRoot.Table; return true;
      case "param": root = MixinExpressionRoot.Parameter; return true;
      case "carry": root = MixinExpressionRoot.Carry; return true;
      default: root = default; return false;
    }
  }

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
      if (candidate.Error is null && candidate.Node is FunctionDirectiveSyntax)
        localFunctions.Add(candidate.Node.Argument);
    }
    for (var index = 0; index < lines.Length; index++) {
      var line = lines[index];
      var parseResult = MixinExpressionParser.ParseDirective(line, index + 1);
      var parsed = parseResult.Node;
      if (parseResult.Error is not null) {
        prelude = explicitPrelude ?? "";
        lateExpression = expression ?? "";
        error = parseResult.Error;
        errorLine = index + 1;
        return false;
      }
      if (parsed is CallDirectiveSyntax && (
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
      if (parsed is DirectiveInvocationSyntax {
        Definition: DirectiveFunctionDefinition { HoistedLocalArgumentIndex: >= 0 }
      }) {
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
      late.Add(MixinSyntaxRenderer.RenderLogicalLine(rewritten));
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
    var program = GetProgram(source ?? "");
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
    var program = GetProgram(source ?? "");
    if (program.Diagnostics.Count != 0) {
      expanded = source ?? "";
      error = program.Diagnostics[0].Message;
      errorLine = program.Diagnostics[0].Line;
      return false;
    }
    for (var index = 0; index < program.Count; index++) {
      var instruction = program.Get(index);
      if (instruction is not InlineDirectiveSyntax) {
        result.Add(MixinSyntaxRenderer.RenderInstruction(instruction));
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
      var labels = body.Where(item =>
          item is ScopeDirectiveSyntax or LabelDirectiveSyntax &&
          !string.IsNullOrEmpty(item.Argument)
        )
        .Select(item => item.Argument).Distinct(StringComparer.Ordinal)
        .ToDictionary(item => item, item => item + suffix, StringComparer.Ordinal);
      var bodyLines = new List<string>();
      foreach (var item in body) {
        switch (item) {
          case ReturnDirectiveSyntax: {
            if (!string.IsNullOrEmpty(item.Operand))
              bodyLines.Add("@LOCAL<" + suffix + "_return> " + item.Operand);
            bodyLines.Add("@GOTO<" + endLabel + ">");
            continue;
          }
          case ScopeDirectiveSyntax or LabelDirectiveSyntax or GotoDirectiveSyntax or MatchDirectiveSyntax
            when !string.IsNullOrEmpty(item.Argument) &&
            labels.TryGetValue(item.Argument, out var renamed):
            bodyLines.Add(MixinSyntaxRenderer.RenderInstruction(item, renamed));
            continue;
          default: bodyLines.Add(MixinSyntaxRenderer.RenderInstruction(item)); break;
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

  private static bool TryHoistStructuralDirective(
    DirectiveInstruction instruction,
    string line,
    ICollection<string> generated,
    IDictionary<string, string> labels,
    ISet<string> structuralLocals,
    out string error
  ) {
    var localIndex = (instruction as DirectiveInvocationSyntax)?.Definition is DirectiveFunctionDefinition function
      ? function.HoistedLocalArgumentIndex
      : -1;
    var local = localIndex >= 0 && instruction.Arguments.Count > localIndex
      ? instruction.Arguments[localIndex]
      : null;
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
        var parseResult = MixinExpressionParser.ParseDirective(line, index + 1);
        var parsed = parseResult.Node;
        if (parseResult.Error is null && parsed is CarryDirectiveSyntax) {
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

  public static MixinExpressionPreparedState PrepareGlobals(IEnumerable<string> expressions) {
    var programs = new List<MixinProgramSyntax>();
    foreach (var expression in expressions ?? []) {
      var validation = MixinExpressionParser.ValidateSyntax(expression, false);
      if (!validation.Success) {
        throw new ArgumentException(
          "invalid prepared expression at line " + validation.ErrorLine + ": " + validation.Error,
          nameof(expressions)
        );
      }
      programs.Add(GetProgram(expression));
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

  internal static bool TryCompileExecution(
    MixinProgramSyntax program,
    MixinExpressionPreparedState prepared,
    out MixinExpressionExecutionProgram compiled,
    out string error,
    out int errorLine
  ) {
    compiled = null;
    error = null;
    errorLine = 0;
    if (program is null) {
      error = "the expression is null";
      return false;
    }
    if (program.Diagnostics.Count != 0) {
      error = program.Diagnostics[0].Message;
      errorLine = program.Diagnostics[0].Line;
      return false;
    }
    var preparedInstructions = prepared?.Instructions ?? [];
    var localInstructions = Enumerable.Range(0, program.Count).Select(program.Get).ToArray();
    var instructions = preparedInstructions.Concat(localInstructions).ToArray();
    var pool = prepared?.StringPool;
    if (pool is null) {
      var poolBuilder = new MixinStringPoolBuilder();
      program.CollectConstants(poolBuilder);
      pool = poolBuilder.Freeze();
    }
    var labels = prepared is null
      ? new Dictionary<string, int>(StringComparer.Ordinal)
      : prepared.Labels.ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
    var instructionScopes = prepared?.InstructionScopes.ToDictionary(item => item.Key, item => item.Value)
      ?? new Dictionary<int, int>();
    var functions = prepared is null
      ? new Dictionary<string, FunctionDefinition>(StringComparer.Ordinal)
      : prepared.Functions.ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
    var functionStarts = prepared?.FunctionStarts.ToDictionary(item => item.Key, item => item.Value)
      ?? new Dictionary<int, int>();
    var functionEnds = prepared is null ? new HashSet<int>() : new HashSet<int>(prepared.FunctionEnds);
    if (!TryIndexSymbols(
      instructions, preparedInstructions.Count, instructions.Length,
      labels, instructionScopes, functions, functionStarts, functionEnds,
      out error, out errorLine
    )) return false;
    compiled = new MixinExpressionExecutionProgram(
      pool, instructions, prepared?.Variables ?? new Dictionary<string, object>(),
      labels, instructionScopes, functions, functionStarts, functionEnds,
      prepared is null ? new HashSet<int>() : new HashSet<int>(prepared.Initializers)
    );
    return true;
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
        switch (instruction.Command) {
          case "SCOPE": scope = true; break;
          case "LABEL":
          case "END" when scope: scope = false; break;
          case "END": depth--; break;
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
    MixinValueDictionary variables,
    ICollection<MixinExpressionPreparedLog> logs,
    ref int executedOperations,
    out string error,
    out int line
  ) {
    error = null;
    line = 0;
    if (program.Diagnostics.Count != 0) {
      error = program.Diagnostics[0].Message;
      line = program.Diagnostics[0].Line;
      return false;
    }
    var functionDepth = 0;
    var functionScope = false;
    for (var i = 0; i < program.Count; i++) {
      var instruction = program.Get(i);
      if (instruction.Command == "FUNC") {
        functionDepth++;
        functionScope = false;
        continue;
      }
      if (functionDepth != 0) {
        switch (instruction.Command) {
          case "SCOPE": functionScope = true; break;
          case "LABEL":
          case "END" when functionScope: functionScope = false; break;
          case "END": functionDepth--; break;
        }
        continue;
      }
      switch (instruction.Command) {
        case "VAR": {
          executedOperations++;
          if (!TryInterpolatePrepared(instruction.ValueExpression, variables, out var value, out error)) {
            line = instruction.Line;
            return false;
          }
          variables[instruction.Argument] = value;
          break;
        }
        case "LOG": {
          executedOperations++;
          if (!TryInterpolatePrepared(instruction.ValueExpression, variables, out var value, out error)) {
            line = instruction.Line;
            return false;
          }
          logs.Add(new MixinExpressionPreparedLog(value, instruction.Line, programIndex));
          break;
        }
      }
    }
    return true;
  }

  private static bool TryInterpolatePrepared(
    IReadOnlyList<ValueExpressionPart> expression,
    MixinValueDictionary variables,
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
      builder.Append(Render(value));
    }
    result = builder.ToString();
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
    if (command is not ("SCOPE" or "LABEL") || string.IsNullOrEmpty(argument)) return;
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
      switch (command) {
        case "SCOPE" or "LABEL": return index;
        case "END": return functionEnds.Contains(index) ? index : index + 1;
      }
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
        else if (command == "LABEL") functionScopeOpen = false;
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
