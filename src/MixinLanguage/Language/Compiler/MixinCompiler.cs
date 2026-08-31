using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mixins.Env;
using Mixins.Runtime;
using Mixins.Compiler.Steps;

namespace Mixins.Compiler;

public static partial class MixinCompiler {
  private static readonly IReadOnlyList<MixinExpressionCompilerStep> Steps = [
    new InlineExpansionStep(), // Replace inline calls with their rewritten bodies.
    new PreludeHoistingStep(), // Try moving non-late compatible instructions to the prelude.
    new FunctionBindingStep() // Associate function calls with their definitions
  ];

  public static ValidationResult ValidateSyntax(string expression) {
    return MixinParser.ValidateSyntax(expression, false);
  }

  internal static ValidationResult ValidateFunctionLibrary(string expression) {
    return MixinParser.ValidateSyntax(expression, true);
  }

  public static bool TryParseReference(string text, out MixinExpressionReference reference, out string error) {
    return MixinParser.TryParseReference(text, out reference, out error);
  }

  public static ProgramAst RewriteTargetAsThis(ProgramAst program) {
    MixinExpressionReference Rewrite(MixinExpressionReference reference) {
      var properties = reference.Properties
        .Select(property => FunctionBindingStep.RewriteProperty(property, Rewrite))
        .ToArray();
      return new MixinExpressionReference(
        reference.Root == MixinExpressionRoot.Target ? MixinExpressionRoot.This : reference.Root,
        reference.Member, properties, reference.Parenthesized, reference.SourceRange
      );
    }

    return new ProgramAst(
      program.AvailableInstructions().Select(instruction => FunctionBindingStep.RewriteReferences(instruction, Rewrite))
    );
  }

  public static bool TryCompileSyntax(
    ProgramAst explicitPrelude,
    ProgramAst expression,
    MixinExpressionPreparedState preparedState,
    out ProgramAst prelude,
    out ProgramAst lateExpression,
    out string error,
    out int errorLine
  ) {
    var syntax = new MixinCompilerSyntax(explicitPrelude, expression);
    foreach (var step in Steps) {
      using var profile = MixinProfiler.Measure($"compiler.step.{step.GetType().Name}");
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

  public static MixinExpressionPreparedState PrepareGlobals(IEnumerable<string> expressions) {
    using var profile = MixinProfiler.Measure("compiler.prepare_globals.text");
    var programs = new List<ProgramAst>();
    foreach (var expression in expressions ?? []) {
      var validation = MixinParser.ValidateSyntax(expression, false);
      if (!validation.Success) {
        throw new ArgumentException(
          $"invalid prepared expression at line {validation.ErrorLine}: {validation.Error}",
          nameof(expressions)
        );
      }
      programs.Add(MixinParser.Parse(expression));
    }
    return PrepareGlobals(programs);
  }

  internal static MixinExpressionPreparedState PrepareGlobals(
    IReadOnlyList<ProgramAst> programs, IEnumerable<string> additionalConstants = null
  ) {
    using var profile = MixinProfiler.Measure("compiler.prepare_globals.syntax");
    programs = [.. (programs ?? []).Select(FunctionBindingStep.Bind)];
    var poolBuilder = new MixinStringPoolBuilder();
    foreach (var program in programs) program.CollectConstants(poolBuilder);
    foreach (var constant in additionalConstants ?? []) poolBuilder.Intern(constant);
    var stringPool = poolBuilder.Freeze();
    var variables = new MixinValueDictionary();
    var logs = new List<MixinExpressionPreparedLog>();
    var executedOperations = 0;
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
          $"invalid prepared expression at line {symbolLine}: {symbolError}",
          nameof(programs)
        );
      }
      instructionOffset += program.Count;
    }
    var initializers = FindPreparedInitializers(instructions);
    foreach (var index in initializers) {
      switch (instructions[index]) {
        case VariableAst variable:
          if (!TryInterpolatePrepared(variable.Expression, variables, stringPool, out var value, out var error)) {
            throw new ArgumentException(
              $"invalid prepared expression at line {variable.Line}: {error}",
              nameof(programs)
            );
          }
          variables.StoreIsolated(stringPool.Get(variable.Name), new LiteralMixinValue(stringPool.Get(value)));
          executedOperations++;
          break;
        case LogAst log:
          if (!TryInterpolatePrepared(log.Expression, variables, stringPool, out var text, out var logError)) {
            throw new ArgumentException(
              $"invalid prepared expression at line {log.Line}: {logError}",
              nameof(programs)
            );
          }
          logs.Add(new MixinExpressionPreparedLog(text, log.Line, 0));
          executedOperations++;
          break;
      }
    }
    var lowered = instructions.Select((instruction, index) => initializers.Contains(index)
      ? new MixinInstruction(MixinOpcode.Empty, new MixinSourceLocation(0, instruction.Line))
      : LowerInstruction(
        instructions, instruction, index, stringPool, labels, instructionScopes,
        functions, functionStarts, functionEnds
      )
    ).ToArray();
    var functionEntries = functions.ToDictionary(
      item => stringPool.Get(item.Key), item => item.Value.Start
    );
    return new MixinExpressionPreparedState(
      stringPool, lowered, variables, functionEntries, logs.AsReadOnly(), executedOperations
    );
  }

  internal static bool TryCompileExecution(
    ProgramAst program,
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
    program = FunctionBindingStep.Bind(program);
    var localInstructions = Enumerable.Range(0, program.Count).Select(program.Get).ToArray();
    var poolBuilder = new MixinStringPoolBuilder(prepared?.StringPool);
    program.CollectConstants(poolBuilder);
    var pool = poolBuilder.Freeze();
    var labels = new Dictionary<string, int>(StringComparer.Ordinal);
    var instructionScopes = new Dictionary<int, int>();
    var functions = new Dictionary<string, FunctionDefinition>(StringComparer.Ordinal);
    var functionStarts = new Dictionary<int, int>();
    var functionEnds = new HashSet<int>();
    if (!TryIndexSymbols(
      localInstructions, 0, localInstructions.Length,
      labels, instructionScopes, functions, functionStarts, functionEnds,
      out error, out errorLine
    )) return false;
    var offset = prepared?.Instructions.Count ?? 0;
    var lowered = localInstructions.Select((instruction, index) => {
        var item = LowerInstruction(
          localInstructions, instruction, index, pool, labels, instructionScopes,
          functions, functionStarts, functionEnds, prepared?.FunctionEntries, offset
        );
        var importedCall = (instruction is CallAst call &&
          !functions.ContainsKey(call.Function ?? "")) || (instruction is InlineAst inline &&
          !functions.ContainsKey(inline.Name ?? ""));
        return item with {
          Destination = item.Destination < 0 || importedCall
            ? item.Destination
            : item.Destination + offset,
          SecondaryDestination = item.SecondaryDestination < 0
            ? item.SecondaryDestination
            : item.SecondaryDestination + offset
        };
      }
    ).ToArray();
    var instructions = (prepared?.Instructions ?? []).Concat(lowered).ToArray();
    compiled = new MixinExpressionExecutionProgram(
      pool, instructions, prepared?.Variables ?? new MixinValueDictionary()
    );
    return true;
  }

  internal static ISet<int> FindPreparedInitializers(IReadOnlyList<InstructionAst> instructions) {
    var result = new HashSet<int>();
    var depth = 0;
    var scope = false;
    for (var index = 0; index < instructions.Count; index++) {
      var instruction = instructions[index];
      if (instruction is FunctionAst) {
        depth++;
        scope = false;
        continue;
      }
      if (depth != 0) {
        switch (instruction) {
          case ScopeAst: scope = true; break;
          case LabelAst:
          case EndAst when scope: scope = false; break;
          case EndAst: depth--; break;
        }
        continue;
      }
      if (instruction is VariableAst or LogAst) result.Add(index);
    }
    return result;
  }

  internal static bool TryInterpolatePrepared(
    IReadOnlyList<ValueAst> expression,
    IReadOnlyDictionary<MixinString, IMixinValue> variables,
    MixinStringPool strings,
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
        !variables.TryGetValue(strings.Get(reference.Member), out var value)) {
        result = null;
        error = "prepared global initializers may only reference an existing @var value";
        return false;
      }
      if (reference.Properties.Count != 0) {
        result = null;
        error = "prepared global initializer references cannot have properties";
        return false;
      }
      if (value is not LiteralMixinValue literal) {
        result = null;
        error = "prepared global initializer value is not literal";
        return false;
      }
      builder.Append(literal.Value.Resolve(strings));
    }
    result = builder.ToString();
    return true;
  }

  internal static void AddScopeLabel(
    InstructionAst ast,
    int index,
    int scope,
    IDictionary<string, int> labels,
    out string error
  ) {
    error = null;
    var argument = ast switch {
      ScopeAst item => item.Label,
      LabelAst item => item.Name,
      _ => null
    };
    if (string.IsNullOrEmpty(argument)) return;
    var key = ScopeLabelKey(scope, argument);
    if (labels.ContainsKey(key)) {
      error = $"duplicate scope label '{argument}'";
      return;
    }
    labels.Add(key, index);
  }

  internal static string ScopeLabelKey(int scope, string label) => $"{scope}\0{label}";

  internal static int FindNextScopeOrEnd(
    IReadOnlyList<InstructionAst> lines,
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
        case ScopeAst or LabelAst: return index;
        case EndAst: return functionEnds.Contains(index) ? index : index + 1;
      }
    }
    return -1;
  }

  internal static bool TryIndexSymbols(
    IReadOnlyList<InstructionAst> lines,
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
      if (instruction is EmptyDirectiveAst) continue;
      if (activeFunction is not null) {
        if (instruction is FunctionAst) {
          error = "functions may not be nested";
          errorLine = instruction.Line;
          return false;
        }
        if (instruction is ScopeAst) functionScopeOpen = true;
        else if (instruction is LabelAst) functionScopeOpen = false;
        if (instruction is not EndAst) {
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
      if (instruction is FunctionAst function) {
        if (functions.ContainsKey(function.Name)) {
          error = $"duplicate function '{function.Name}'";
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
    error = $"unterminated function '{activeFunction}'";
    errorLine = lines[functionStart].Line;
    return false;
  }

  public sealed record FunctionDefinition(int Start, int End);
}

/// <summary>The syntax state passed between ordered compiler transformations.</summary>
public sealed record MixinCompilerSyntax(
  ProgramAst Prelude,
  ProgramAst Expression
);

/// <summary>An AST-to-AST transformation in the mixin compiler pipeline.</summary>
public abstract class MixinExpressionCompilerStep {
  public abstract bool TryTransform(
    MixinCompilerSyntax input,
    MixinExpressionPreparedState preparedState,
    out MixinCompilerSyntax output,
    out string error,
    out int errorLine
  );
}