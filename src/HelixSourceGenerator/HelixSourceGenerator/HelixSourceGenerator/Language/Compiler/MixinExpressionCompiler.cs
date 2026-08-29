using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace HelixSourceGenerator.Language.Compiler;

public static class MixinExpressionCompiler {
  private static readonly HashSet<MixinExpressionRoot> RoslynRoots = [
    MixinExpressionRoot.Target, MixinExpressionRoot.This, MixinExpressionRoot.Attribute, MixinExpressionRoot.Argument
  ];
  private static readonly IReadOnlyList<MixinExpressionCompilerStep> Steps = [
    new InlineExpansionStep(),
    new PreludeHoistingStep()
  ];

  public static MixinExpressionValidationResult ValidateSyntax(string expression) {
    return MixinExpressionParser.ValidateSyntax(expression, false);
  }

  internal static MixinExpressionValidationResult ValidateFunctionLibrary(string expression) {
    return MixinExpressionParser.ValidateSyntax(expression, true);
  }

  public static bool TryParseReference(string text, out MixinExpressionReference reference, out string error) {
    return MixinExpressionParser.TryParseReference(text, out reference, out error);
  }

  public static MixinProgramSyntax RewriteTargetAsThis(MixinProgramSyntax program) {
    MixinExpressionReference Rewrite(MixinExpressionReference reference) {
      var properties = reference.Properties.Select(property => RewriteProperty(property, Rewrite)).ToArray();
      return new MixinExpressionReference(
        reference.Root == MixinExpressionRoot.Target ? MixinExpressionRoot.This : reference.Root,
        reference.Member, properties, reference.Parenthesized
      );
    }

    return new MixinProgramSyntax(
      program.AvailableInstructions().Select(instruction => RewriteReferences(instruction, Rewrite))
    );
  }

  public static bool TryCompileSyntax(
    MixinProgramSyntax explicitPrelude,
    MixinProgramSyntax expression,
    MixinExpressionPreparedState preparedState,
    out MixinProgramSyntax prelude,
    out MixinProgramSyntax lateExpression,
    out string error,
    out int errorLine
  ) {
    var syntax = new MixinCompilerSyntax(explicitPrelude, expression);
    foreach (var step in Steps) {
      if (step.TryTransform(syntax, preparedState, out var transformed, out error, out errorLine)) {
        syntax = transformed;
        continue;
      }
      prelude = syntax.Prelude;
      lateExpression = syntax.Expression;
      return false;
    }
    prelude = syntax.Prelude;
    lateExpression = syntax.Expression;
    error = null;
    errorLine = 0;
    return true;
  }

  private static bool TryHoistRoslynReferences(
    MixinProgramSyntax explicitPrelude,
    MixinProgramSyntax expression,
    out MixinProgramSyntax prelude,
    out MixinProgramSyntax lateExpression,
    out string error,
    out int errorLine
  ) {
    var generated = new List<DirectiveInstruction>();
    var labels = new Dictionary<MixinExpressionReference, string>();
    var structuralLocals = new HashSet<string>(StringComparer.Ordinal);
    var late = new List<DirectiveInstruction>();
    error = null;
    errorLine = 0;
    var localFunctions = new HashSet<string>(
      expression.AvailableInstructions().OfType<FunctionDirectiveSyntax>().Select(item => item.Name),
      StringComparer.Ordinal
    );
    foreach (var parsed in expression.AvailableInstructions()) {
      if (parsed is CallDirectiveSyntax call && !localFunctions.Contains(call.Function ?? "")) {
        prelude = explicitPrelude;
        lateExpression = expression;
        error = "Prelude-model expressions may only call functions declared in the same expression; imported call '" +
          (call.Function ?? "") + "' is not supported";
        errorLine = parsed.Line;
        return false;
      }
      if (parsed is DirectiveInvocationSyntax {
        Definition: DirectiveFunctionDefinition { HoistedLocalArgumentIndex: >= 0 }
      }) {
        if (!TryHoistStructuralDirective(parsed, generated, labels, structuralLocals, out error)) {
          prelude = explicitPrelude;
          lateExpression = expression;
          errorLine = parsed.Line;
          return false;
        }
        late.Add(new EmptyDirectiveSyntax(parsed.Line));
        continue;
      }
      late.Add(RewriteRoslynReferences(parsed, generated, labels, structuralLocals));
    }
    prelude = new MixinProgramSyntax(explicitPrelude.AvailableInstructions().Concat(generated));
    lateExpression = new MixinProgramSyntax(late);
    return true;
  }

  private sealed class InlineExpansionStep : MixinExpressionCompilerStep {
    public override bool TryTransform(
      MixinCompilerSyntax input,
      MixinExpressionPreparedState preparedState,
      out MixinCompilerSyntax output,
      out string error,
      out int errorLine
    ) {
      if (!TryExpandInlines(
        input.Prelude, input.Expression, preparedState,
        out var prelude, out var expression, out error, out errorLine
      )) {
        output = input;
        return false;
      }
      output = new MixinCompilerSyntax(
        new MixinProgramSyntax(prelude), new MixinProgramSyntax(expression)
      );
      return true;
    }
  }

  private sealed class PreludeHoistingStep : MixinExpressionCompilerStep {
    public override bool TryTransform(
      MixinCompilerSyntax input,
      MixinExpressionPreparedState preparedState,
      out MixinCompilerSyntax output,
      out string error,
      out int errorLine
    ) {
      if (!TryHoistRoslynReferences(
        input.Prelude, input.Expression,
        out var prelude, out var late, out error, out errorLine
      )) {
        output = input;
        return false;
      }
      output = new MixinCompilerSyntax(prelude, late);
      return true;
    }
  }

  private static bool TryExpandInlines(
    MixinProgramSyntax explicitPrelude,
    MixinProgramSyntax expression,
    MixinExpressionPreparedState preparedState,
    out IReadOnlyList<DirectiveInstruction> expandedPrelude,
    out IReadOnlyList<DirectiveInstruction> expandedExpression,
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
      expandedPrelude = [];
      expandedExpression = [];
      return false;
    }
    var inlineSequence = 0;
    if (!TryExpandInlineProgram(
      explicitPrelude.AvailableInstructions(), functions,
      new HashSet<string>(StringComparer.Ordinal), ref inlineSequence,
      out expandedPrelude, out error, out errorLine
    )) {
      expandedExpression = [];
      return false;
    }
    return TryExpandInlineProgram(
      expression.AvailableInstructions(), functions,
      new HashSet<string>(StringComparer.Ordinal), ref inlineSequence,
      out expandedExpression, out error, out errorLine
    );
  }

  private static bool TryCollectInlineFunctions(
    MixinProgramSyntax program,
    IDictionary<string, IReadOnlyList<DirectiveInstruction>> functions,
    out string error,
    out int errorLine
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

  private static bool TryExpandInlineProgram(
    IEnumerable<DirectiveInstruction> source,
    IReadOnlyDictionary<string, IReadOnlyList<DirectiveInstruction>> functions,
    ISet<string> activeFunctions,
    ref int inlineSequence,
    out IReadOnlyList<DirectiveInstruction> expanded,
    out string error,
    out int errorLine
  ) {
    var result = new List<DirectiveInstruction>();
    foreach (var instruction in source) {
      if (instruction is not InlineDirectiveSyntax inline) {
        result.Add(instruction);
        continue;
      }
      var name = inline.Name ?? "";
      if (!functions.TryGetValue(name, out var body)) {
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
      var suffix = "__inline_" + inlineSequence++.ToString(CultureInfo.InvariantCulture);
      var endLabel = suffix + "_end";
      var labels = body.Select(LabelOf).Where(item => !string.IsNullOrEmpty(item))
        .Distinct(StringComparer.Ordinal)
        .ToDictionary(item => item, item => item + suffix, StringComparer.Ordinal);
      var bodyNodes = new List<DirectiveInstruction>();
      foreach (var item in body) {
        switch (item) {
          case ReturnDirectiveSyntax returned: {
            if (!IsEmptyValue(returned.Expression))
              bodyNodes.Add(new LocalDirectiveSyntax(item.Line, suffix + "_return", returned.Expression));
            bodyNodes.Add(new GotoDirectiveSyntax(item.Line, endLabel));
            continue;
          }
          case ScopeDirectiveSyntax or LabelDirectiveSyntax or GotoDirectiveSyntax or MatchDirectiveSyntax
            when LabelOf(item) is { Length: > 0 } label && labels.TryGetValue(label, out var renamed):
            bodyNodes.Add(RenameLabel(item, renamed));
            continue;
          default: bodyNodes.Add(item); break;
        }
      }
      if (!TryExpandInlineProgram(
        bodyNodes, functions, activeFunctions, ref inlineSequence,
        out var expandedBody, out error, out errorLine
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

  private static bool TryHoistStructuralDirective(
    DirectiveInstruction instruction,
    ICollection<DirectiveInstruction> generated,
    IDictionary<MixinExpressionReference, string> labels,
    ISet<string> structuralLocals,
    out string error
  ) {
    var localIndex = (instruction as DirectiveInvocationSyntax)?.Definition is DirectiveFunctionDefinition function
      ? function.HoistedLocalArgumentIndex
      : -1;
    var invocation = (DirectiveInvocationSyntax)instruction;
    var localArgument = localIndex >= 0 && invocation.ParsedArguments.Count > localIndex
      ? invocation.ParsedArguments[localIndex]
      : null;
    var local = localArgument?.Literal;
    if (string.IsNullOrEmpty(local) || localArgument.IsDynamic) {
      error = "@" + invocation.Definition.Name + " cannot be hoisted because its result local is dynamic";
      return false;
    }
    // Structural directives execute in the Roslyn-backed prelude and consume their syntax target
    // directly. Rewriting that target to @carry would make the host context receive a runtime root.
    generated.Add(instruction);
    var localReference = new MixinExpressionReference(MixinExpressionRoot.Local, local, []);
    var label = "__" + labels.Count.ToString(CultureInfo.InvariantCulture);
    labels[localReference] = label;
    generated.Add(
      new CarryDirectiveSyntax(
        instruction.Line, label, [new ValueExpressionPart(null, localReference)]
      )
    );
    structuralLocals.Add(local);
    error = null;
    return true;
  }

  private static DirectiveInstruction RewriteRoslynReferences(
    DirectiveInstruction instruction,
    ICollection<DirectiveInstruction> generated,
    IDictionary<MixinExpressionReference, string> labels,
    ISet<string> structuralLocals
  ) {
    MixinExpressionReference RewriteReference(MixinExpressionReference reference) {
      var roslyn = RoslynRoots.Contains(reference.Root) ||
        (reference.Root == MixinExpressionRoot.Local &&
          structuralLocals?.Contains(reference.Member ?? "") == true);
      if (!roslyn) return RewriteNested(reference);
      if (!labels.TryGetValue(reference, out var label)) {
        label = "__" + labels.Count.ToString(CultureInfo.InvariantCulture);
        labels.Add(reference, label);
        generated.Add(
          new CarryDirectiveSyntax(
            instruction.Line, label, [new ValueExpressionPart(null, reference)]
          )
        );
      }
      return new MixinExpressionReference(
        MixinExpressionRoot.Carry, label, [], reference.Parenthesized
      );
    }

    MixinExpressionReference RewriteNested(MixinExpressionReference reference) {
      var properties = reference.Properties.Select(property => RewriteProperty(property, RewriteReference)).ToArray();
      return new MixinExpressionReference(
        reference.Root, reference.Member, properties, reference.Parenthesized
      );
    }

    return RewriteReferences(instruction, RewriteReference);
  }

  private static DirectiveInstruction RewriteReferences(
    DirectiveInstruction instruction,
    Func<MixinExpressionReference, MixinExpressionReference> rewrite
  ) {
    IReadOnlyList<ValueExpressionPart> Value(IReadOnlyList<ValueExpressionPart> value) {
      return [
        .. (value ?? []).Select(part => part.Reference is null
          ? part
          : new ValueExpressionPart(null, RewriteComplete(part.Reference))
        )
      ];
    }

    IReadOnlyList<MixinExpressionReference> Boolean(IReadOnlyList<MixinExpressionReference> value) {
      return [.. (value ?? []).Select(item => item is null ? null : RewriteBoolean(item))];
    }

    DirectiveArgumentSyntax Argument(DirectiveArgumentSyntax value) {
      return value is null
        ? null
        : value.IsDynamic
          ? new DirectiveArgumentSyntax(null, Value(value.Expression))
          : value;
    }

    MixinExpressionReference RewriteComplete(MixinExpressionReference reference) {
      var properties = reference.Properties.Select(property => RewriteProperty(property, RewriteComplete)).ToArray();
      return rewrite(
        new MixinExpressionReference(
          reference.Root, reference.Member, properties, reference.Parenthesized
        )
      );
    }

    MixinExpressionReference RewriteBoolean(MixinExpressionReference reference) {
      var properties = reference.Properties.Select(property => RewriteProperty(property, RewriteComplete)).ToArray();
      var predicateIndex = Array.FindIndex(properties, property => FunctionLibrary.IsPredicate(property.Name));
      if (predicateIndex < 0) {
        return rewrite(
          new MixinExpressionReference(
            reference.Root, reference.Member, properties, reference.Parenthesized
          )
        );
      }
      var subject = rewrite(
        new MixinExpressionReference(
          reference.Root, reference.Member, [.. properties.Take(predicateIndex)]
        )
      );
      return new MixinExpressionReference(
        subject.Root, subject.Member,
        [.. subject.Properties, .. properties.Skip(predicateIndex)], reference.Parenthesized
      );
    }

    return instruction switch {
      DirectiveInvocationSyntax item => new DirectiveInvocationSyntax(
        item.Line, item.Definition, [.. item.ParsedArguments.Select(Argument)], Value(item.Expression)
      ),
      CallDirectiveSyntax item => new CallDirectiveSyntax(
        item.Line, item.Function, item.ReturnLocal, Value(item.Expression)
      ),
      MatchDirectiveSyntax item => new MatchDirectiveSyntax(item.Line, item.FailureLabel, Boolean(item.Expression)),
      AssertDirectiveSyntax item => new AssertDirectiveSyntax(item.Line, Boolean(item.Expression)),
      CodeDirectiveSyntax item => new CodeDirectiveSyntax(
        item.Line, item.Target, item.InjectionTarget, Value(item.Expression)
      ),
      MixinDirectiveSyntax item => new MixinDirectiveSyntax(
        item.Line, Argument(item.Target), Argument(item.Priority), Value(item.Expression)
      ),
      UsingDirectiveSyntax item => new UsingDirectiveSyntax(item.Line, Value(item.Expression)),
      LogDirectiveSyntax item => new LogDirectiveSyntax(item.Line, Value(item.Expression)),
      LocalDirectiveSyntax item => new LocalDirectiveSyntax(item.Line, item.Name, Value(item.Expression)),
      VariableDirectiveSyntax item => new VariableDirectiveSyntax(item.Line, item.Name, Value(item.Expression)),
      CarryDirectiveSyntax item => new CarryDirectiveSyntax(item.Line, item.Label, Value(item.Expression)),
      ReturnDirectiveSyntax item => new ReturnDirectiveSyntax(item.Line, Value(item.Expression)),
      FailDirectiveSyntax item => new FailDirectiveSyntax(item.Line, Value(item.Expression)),
      _ => instruction
    };
  }

  private static MixinExpressionProperty RewriteProperty(
    MixinExpressionProperty property,
    Func<MixinExpressionReference, MixinExpressionReference> rewrite
  ) {
    if (property.ParsedArguments.Count == 0)
      return new MixinExpressionProperty(property.Name, property.Arguments, property.Negated);
    var parsed = property.ParsedArguments.Select(argument => {
        var value = argument.ValueExpression is null
          ? null
          : argument.ValueExpression.Select(part =>
            part.Reference is null ? part : new ValueExpressionPart(null, rewrite(part.Reference))
          ).ToArray();
        var boolean = argument.BooleanExpression is null ? null : argument.BooleanExpression.Select(rewrite).ToArray();
        return new MixinPropertyArgumentSyntax(argument.Literal, value, boolean);
      }
    ).ToArray();
    return new MixinExpressionProperty(property.Name, parsed, property.Negated);
  }

  private static string LabelOf(DirectiveInstruction instruction) {
    return instruction switch {
      ScopeDirectiveSyntax item => item.Label,
      LabelDirectiveSyntax item => item.Name,
      GotoDirectiveSyntax item => item.Label,
      MatchDirectiveSyntax item => item.FailureLabel,
      _ => null
    };
  }

  private static DirectiveInstruction RenameLabel(DirectiveInstruction instruction, string label) {
    return instruction switch {
      ScopeDirectiveSyntax item => new ScopeDirectiveSyntax(item.Line, label),
      LabelDirectiveSyntax item => new LabelDirectiveSyntax(item.Line, label),
      GotoDirectiveSyntax item => new GotoDirectiveSyntax(item.Line, label),
      MatchDirectiveSyntax item => new MatchDirectiveSyntax(item.Line, label, item.Expression),
      _ => instruction
    };
  }

  private static bool IsEmptyValue(IReadOnlyList<ValueExpressionPart> expression) {
    return expression is null || expression.All(item => item.Reference is null && string.IsNullOrEmpty(item.Literal));
  }

  internal static void HoistLateCarries(
    MixinProgramSyntax expression,
    MixinProgramSyntax lateExpression,
    out MixinProgramSyntax primary,
    out MixinProgramSyntax late
  ) {
    primary = expression;
    late = lateExpression;
    if (lateExpression is null || lateExpression.Count == 0) return;
    var carries = lateExpression.AvailableInstructions().OfType<CarryDirectiveSyntax>()
      .Cast<DirectiveInstruction>().ToArray();
    var remaining = lateExpression.AvailableInstructions().Where(item => item is not CarryDirectiveSyntax).ToArray();
    if (carries.Length == 0) return;
    primary = new MixinProgramSyntax(carries.Concat(expression.AvailableInstructions()));
    late = new MixinProgramSyntax(remaining);
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
      programs.Add(MixinExpressionParser.Parse(expression));
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
      if (instruction is FunctionDirectiveSyntax) {
        depth++;
        scope = false;
        continue;
      }
      if (depth != 0) {
        switch (instruction) {
          case ScopeDirectiveSyntax: scope = true; break;
          case LabelDirectiveSyntax:
          case EndDirectiveSyntax when scope: scope = false; break;
          case EndDirectiveSyntax: depth--; break;
        }
        continue;
      }
      if (instruction is VariableDirectiveSyntax or LogDirectiveSyntax) result.Add(index);
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
      if (instruction is FunctionDirectiveSyntax) {
        functionDepth++;
        functionScope = false;
        continue;
      }
      if (functionDepth != 0) {
        switch (instruction) {
          case ScopeDirectiveSyntax: functionScope = true; break;
          case LabelDirectiveSyntax:
          case EndDirectiveSyntax when functionScope: functionScope = false; break;
          case EndDirectiveSyntax: functionDepth--; break;
        }
        continue;
      }
      switch (instruction) {
        case VariableDirectiveSyntax variable: {
          executedOperations++;
          if (!TryInterpolatePrepared(variable.Expression, variables, out var value, out error)) {
            line = instruction.Line;
            return false;
          }
          variables[variable.Name] = value;
          break;
        }
        case LogDirectiveSyntax log: {
          executedOperations++;
          if (!TryInterpolatePrepared(log.Expression, variables, out var value, out error)) {
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
      builder.Append(MixinValue.From(value).Render());
    }
    result = builder.ToString();
    return true;
  }

  private static void AddScopeLabel(
    DirectiveInstruction instruction,
    int index,
    int scope,
    IDictionary<string, int> labels,
    out string error
  ) {
    error = null;
    var argument = instruction switch {
      ScopeDirectiveSyntax item => item.Label,
      LabelDirectiveSyntax item => item.Name,
      _ => null
    };
    if (string.IsNullOrEmpty(argument)) return;
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
      switch (lines[index]) {
        case ScopeDirectiveSyntax or LabelDirectiveSyntax: return index;
        case EndDirectiveSyntax: return functionEnds.Contains(index) ? index : index + 1;
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
      if (instruction is EmptyDirectiveSyntax) continue;
      if (activeFunction is not null) {
        if (instruction is FunctionDirectiveSyntax) {
          error = "functions may not be nested";
          errorLine = instruction.Line;
          return false;
        }
        if (instruction is ScopeDirectiveSyntax) functionScopeOpen = true;
        else if (instruction is LabelDirectiveSyntax) functionScopeOpen = false;
        if (instruction is not EndDirectiveSyntax) {
          AddScopeLabel(instruction, index, scope, labels, out error);
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
      if (instruction is FunctionDirectiveSyntax function) {
        if (functions.ContainsKey(function.Name)) {
          error = "duplicate function '" + function.Name + "'";
          errorLine = instruction.Line;
          return false;
        }
        activeFunction = function.Name;
        functionStart = index;
        functionScopeOpen = false;
        continue;
      }
      AddScopeLabel(instruction, index, scope, labels, out error);
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
