using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace HelixSourceGenerator.Language;

internal sealed record MixinParseDiagnostic(int Line, string Message);
internal sealed record MixinProgramParseResult(DirectiveInstruction[] Instructions, IReadOnlyList<MixinParseDiagnostic> Diagnostics);
internal sealed record MixinDirectiveParseResult(DirectiveInstruction Node, string Error);

/// <summary>Builds the mixin AST from lexer tokens.</summary>
internal static class MixinExpressionParser {
  internal static string[] SplitLines(string expression) => MixinExpressionLexer.SplitLogicalLines(expression);
  internal static bool IsDynamicArgument(string value) => value is { Length: >= 2 } && value[0] == '(' && value[value.Length - 1] == ')';

  internal static MixinProgramParseResult ParseProgram(string source) {
    var tokens = MixinExpressionLexer.Lex(source); var result = new List<DirectiveInstruction>(); var diagnostics = new List<MixinParseDiagnostic>(); var line = new List<MixinToken>();
    foreach (var token in tokens) {
      if (token.Kind != MixinTokenKind.EndOfLine) { line.Add(token); continue; }
      var parsed = ParseTokens(line, token.Line); result.Add(parsed.Node);
      if (parsed.Error is not null) diagnostics.Add(new MixinParseDiagnostic(token.Line, parsed.Error));
      line.Clear();
    }
    return new MixinProgramParseResult(result.ToArray(), diagnostics.AsReadOnly());
  }

  internal static MixinDirectiveParseResult ParseDirective(string text, int line) {
    var tokens = MixinExpressionLexer.LexLogicalLine(text, line);
    return ParseTokens(tokens.Where(token => token.Kind != MixinTokenKind.EndOfLine).ToArray(), line);
  }

  private static MixinDirectiveParseResult ParseTokens(IReadOnlyList<MixinToken> tokens, int line) {
    if (tokens.Count == 0) return new MixinDirectiveParseResult(new EmptyDirectiveSyntax(line), null);
    if (tokens.Any(token => token.Kind == MixinTokenKind.Invalid)) {
      var raw = tokens.First(token => token.Kind == MixinTokenKind.Invalid).Text.TrimStart(' ', '\t');
      return new MixinDirectiveParseResult(new EmptyDirectiveSyntax(line), raw.StartsWith("@\\", StringComparison.Ordinal) || raw.StartsWith("@+", StringComparison.Ordinal)
        ? "continuation requires an immediately preceding directive" : "expected an expression directive");
    }
    if (tokens.Count < 3 || tokens[0].Kind != MixinTokenKind.At || tokens[1].Kind != MixinTokenKind.Identifier)
      return new MixinDirectiveParseResult(new EmptyDirectiveSyntax(line), "expected an expression directive");
    var name = tokens[1].Text.ToUpperInvariant();
    var arguments = tokens.Where(token => token.Kind == MixinTokenKind.Argument).Select(token => token.Text).ToArray();
    var operand = tokens.FirstOrDefault(token => token.Kind == MixinTokenKind.Operand)?.Text ?? "";
    var syntax = CreateBuiltinSyntax(name, line, arguments, operand);
    if (syntax is not null) {
      var valid = ValidateBuiltin(name, arguments, operand, out var error);
      return new MixinDirectiveParseResult(syntax, valid ? null : error);
    }
    if (!DirectiveLibrary.TryGet(name, out var directive))
      return new MixinDirectiveParseResult(new UnknownDirectiveSyntax(line, name, arguments, operand), "unknown directive '@" + name + "'");
    var invocationValid = directive.Validate(arguments, operand, out var invocationError) &&
      ValidateOperand(directive.OperandKind, operand, out invocationError);
    return new MixinDirectiveParseResult(
      new DirectiveInvocationSyntax(line, directive, arguments, operand), invocationValid ? null : invocationError
    );
  }

  private static DirectiveInstruction CreateBuiltinSyntax(string name, int l, IReadOnlyList<string> a, string o) => name switch {
    "SCOPE" => new ScopeDirectiveSyntax(l,a,o), "LABEL" => new LabelDirectiveSyntax(l,a,o),
    "FUNC" => new FunctionDirectiveSyntax(l,a,o), "CALL" => new CallDirectiveSyntax(l,a,o),
    "INLINE" => new InlineDirectiveSyntax(l,a,o), "END" => new EndDirectiveSyntax(l,a,o),
    "MATCH" => new MatchDirectiveSyntax(l,a,o), "ASSERT" => new AssertDirectiveSyntax(l,a,o),
    "CODE" => new CodeDirectiveSyntax(l,a,o), "MIXIN" => new MixinDirectiveSyntax(l,a,o),
    "USING" => new UsingDirectiveSyntax(l,a,o), "LOG" => new LogDirectiveSyntax(l,a,o),
    "LOCAL" => new LocalDirectiveSyntax(l,a,o), "VAR" => new VariableDirectiveSyntax(l,a,o),
    "CARRY" => new CarryDirectiveSyntax(l,a,o), "RETURN" => new ReturnDirectiveSyntax(l,a,o),
    "GOTO" => new GotoDirectiveSyntax(l,a,o), "SKIP" => new SkipDirectiveSyntax(l,a,o),
    "FAIL" => new FailDirectiveSyntax(l,a,o), "ANNOTATION" => new AnnotationDirectiveSyntax(l,a,o),
    "PRELUDE" => new PreludeDirectiveSyntax(l,a,o), "DEFINE_TARGET" => new DefineTargetDirectiveSyntax(l,a,o),
    _ => null
  };

  private static bool ValidateBuiltin(string name, IReadOnlyList<string> arguments, string operand, out string error) {
    if (name == "CALL") {
      if (arguments.Count is not (1 or 2) || arguments.Any(string.IsNullOrEmpty)) {
        error = "CALL requires a function label and optionally a return local"; return false;
      }
    } else if (name == "MIXIN") {
      if (arguments.Count is < 1 or > 2 || string.IsNullOrEmpty(arguments[0])) {
        error = "MIXIN requires a target"; return false;
      }
    } else if (name == "DEFINE_TARGET") {
      if (arguments.Count != 2 || arguments.Any(string.IsNullOrWhiteSpace)) {
        error = "DEFINE_TARGET requires a name and value"; return false;
      }
    } else {
      var named = name is "LABEL" or "FUNC" or "INLINE" or "LOCAL" or "VAR" or "CARRY" or "ANNOTATION" or "GOTO";
      if (named && (arguments.Count != 1 || string.IsNullOrEmpty(arguments[0]))) {
        error = name + " requires a name"; return false;
      }
      if (!named && arguments.Count > 1) {
        error = name + " accepts at most 1 argument"; return false;
      }
    }
    var kind = name is "MATCH" or "ASSERT" ? DirectiveOperandKind.Boolean
      : name is "CALL" or "CODE" or "MIXIN" or "USING" or "LOCAL" or "VAR" or "CARRY" or "LOG" or "RETURN" or "FAIL"
        ? DirectiveOperandKind.Value : DirectiveOperandKind.None;
    return ValidateOperand(kind, operand, out error);
  }

  private static bool ValidateOperand(DirectiveOperandKind kind, string operand, out string error) {
    switch (kind) {
      case DirectiveOperandKind.Boolean: return ValidateBooleanExpressionSyntax(operand, out error);
      case DirectiveOperandKind.Value: return ValidateValueExpressionSyntax(operand, out error);
      default: error = null; return true;
    }
  }

  internal static IReadOnlyList<MixinExpressionReference> ParseBooleanExpression(string text) {
    var result = new List<MixinExpressionReference>(); var position = 0;
    while (position < text.Length) {
      while (position < text.Length && char.IsWhiteSpace(text[position])) position++;
      if (position == text.Length) break;
      if (!MixinExpressionEvaluator.TryReadReferenceNode(text, ref position, out var reference, out _)) { result.Add(null); break; }
      result.Add(reference);
    }
    return result.AsReadOnly();
  }

  internal static IReadOnlyList<ValueExpressionPart> ParseValueExpression(string text) {
    var result = new List<ValueExpressionPart>(); var literal = new StringBuilder(); var position = 0;
    while (position < text.Length) {
      if (text[position] != '@') { literal.Append(text[position++]); continue; }
      if (position + 1 < text.Length && text[position + 1] == '@') { literal.Append('@'); position += 2; continue; }
      if (literal.Length != 0) { result.Add(new ValueExpressionPart(literal.ToString(), null)); literal.Clear(); }
      if (!MixinExpressionEvaluator.TryReadReferenceNode(text, ref position, out var reference, out _)) break;
      result.Add(new ValueExpressionPart(null, reference));
    }
    if (literal.Length != 0 || result.Count == 0) result.Add(new ValueExpressionPart(literal.ToString(), null));
    return result.AsReadOnly();
  }

  internal static bool ValidateBooleanExpressionSyntax(string text, out string error) {
    error = null; var position = 0; var found = false;
    while (position < text.Length) {
      while (position < text.Length && char.IsWhiteSpace(text[position])) position++;
      if (position == text.Length) break;
      found = true;
      if (!MixinExpressionEvaluator.TryReadReferenceNode(text, ref position, out _, out error)) return false;
    }
    if (found) return true;
    error = "boolean expression is empty";
    return false;
  }

  internal static bool ValidateValueExpressionSyntax(string text, out string error) {
    error = null; var position = 0;
    while (position < text.Length) {
      if (text[position] != '@') { position++; continue; }
      if (position + 1 < text.Length && text[position + 1] == '@') { position += 2; continue; }
      if (!MixinExpressionEvaluator.TryReadReferenceNode(text, ref position, out _, out error)) return false;
    }
    return true;
  }

  internal static MixinExpressionValidationResult ValidateSyntax(string expression, bool functionsOnly) {
    if (expression is null) return ValidationFailure("the expression is null", 0);
    var program = new MixinProgramSyntax(expression);
    if (program.Diagnostics.Count != 0)
      return ValidationFailure(program.Diagnostics[0].Message, program.Diagnostics[0].Line);
    string activeFunction = null; var functionLine = 0; var functionScopeOpen = false;
    var functions = new HashSet<string>(StringComparer.Ordinal);
    for (var index = 0; index < program.Count; index++) {
      var parsed = program.Get(index);
      if (parsed is EmptyDirectiveSyntax) continue;
      if (activeFunction is null) {
        if (parsed is not FunctionDirectiveSyntax) {
          if (functionsOnly)
            return ValidationFailure("mixin libraries may only contain function declarations", parsed.Line);
          continue;
        }
        if (!functions.Add(parsed.Argument))
          return ValidationFailure("duplicate function '" + parsed.Argument + "'", parsed.Line);
        activeFunction = parsed.Argument; functionLine = parsed.Line; functionScopeOpen = false;
        continue;
      }
      if (parsed is FunctionDirectiveSyntax) return ValidationFailure("functions may not be nested", parsed.Line);
      if (parsed is ScopeDirectiveSyntax) functionScopeOpen = true;
      if (parsed is not EndDirectiveSyntax) continue;
      if (functionScopeOpen) functionScopeOpen = false;
      else activeFunction = null;
    }
    return activeFunction is null
      ? new MixinExpressionValidationResult(true, null, 0)
      : ValidationFailure("unterminated function '" + activeFunction + "'", functionLine);
  }

  private static MixinExpressionValidationResult ValidationFailure(string error, int line) =>
    new(false, error, line);

  internal static bool TryReadDirective(string line, out string command, out IReadOnlyList<string> arguments, out string operand) {
    var tokens = MixinExpressionLexer.LexLogicalLine(line, 1);
    var identifier = tokens.FirstOrDefault(token => token.Kind == MixinTokenKind.Identifier);
    if (identifier is null || tokens.Any(token => token.Kind == MixinTokenKind.Invalid)) {
      command = null; arguments = []; operand = null; return false;
    }
    command = identifier.Text.ToUpperInvariant();
    arguments = tokens.Where(token => token.Kind == MixinTokenKind.Argument).Select(token => token.Text).ToArray();
    operand = tokens.FirstOrDefault(token => token.Kind == MixinTokenKind.Operand)?.Text ?? "";
    return true;
  }
}
