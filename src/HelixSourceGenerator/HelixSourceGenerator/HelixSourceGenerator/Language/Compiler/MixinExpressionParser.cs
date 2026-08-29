using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HelixSourceGenerator.Language.Compiler;

internal sealed record MixinParseDiagnostic(int Line, string Message);

internal sealed record MixinProgramParseResult(DirectiveInstruction[] Instructions,
  IReadOnlyList<MixinParseDiagnostic> Diagnostics
);

internal sealed record MixinDirectiveParseResult(DirectiveInstruction Node, string Error);

/// <summary>Builds the mixin AST from lexer tokens.</summary>
internal static class MixinExpressionParser {
  internal static string[] SplitLines(string expression) => MixinExpressionLexer.SplitLogicalLines(expression);

  internal static bool IsDynamicArgument(string value) =>
    value is { Length: >= 2 } && value[0] == '(' && value[value.Length - 1] == ')';

  internal static MixinProgramParseResult ParseProgram(string source) {
    var tokens = MixinExpressionLexer.Lex(source);
    var result = new List<DirectiveInstruction>();
    var diagnostics = new List<MixinParseDiagnostic>();
    var line = new List<MixinToken>();
    foreach (var token in tokens) {
      if (token.Kind != MixinTokenKind.EndOfLine) {
        line.Add(token);
        continue;
      }
      var parsed = ParseTokens(line, token.Line);
      result.Add(parsed.Node);
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
      return new MixinDirectiveParseResult(
        new EmptyDirectiveSyntax(line), raw.StartsWith("@\\", StringComparison.Ordinal) ||
        raw.StartsWith("@+", StringComparison.Ordinal)
          ? "continuation requires an immediately preceding directive"
          : "expected an expression directive"
      );
    }
    if (tokens.Count < 3 || tokens[0].Kind != MixinTokenKind.At || tokens[1].Kind != MixinTokenKind.Identifier)
      return new MixinDirectiveParseResult(new EmptyDirectiveSyntax(line), "expected an expression directive");
    var name = tokens[1].Text.ToUpperInvariant();
    var arguments = tokens.Where(token => token.Kind == MixinTokenKind.Argument).Select(token => token.Text).ToArray();
    var operand = tokens.FirstOrDefault(token => token.Kind == MixinTokenKind.Operand)?.Text ?? "";
    var syntax = CreateBuiltinSyntax(name, line, arguments, operand);
    if (syntax is not null) {
      var valid = ValidateBuiltin(name, arguments, operand, out var error) &&
        ValidateDynamicArguments(arguments, out error);
      return new MixinDirectiveParseResult(syntax, valid ? null : error);
    }
    if (!DirectiveLibrary.TryGet(name, out var directive))
      return new MixinDirectiveParseResult(
        new UnknownDirectiveSyntax(line, name), "unknown directive '@" + name + "'"
      );
    var invocationValid = directive.Validate(arguments, operand, out var invocationError) &&
      ValidateOperand(directive.OperandKind, operand, out invocationError) &&
      ValidateDynamicArguments(arguments, out invocationError);
    var parsedArguments = arguments.Select(ParseDirectiveArgument).ToArray();
    return new MixinDirectiveParseResult(
      new DirectiveInvocationSyntax(line, directive, parsedArguments, ParseValueExpression(operand)), invocationValid ? null : invocationError
    );
  }

  private static DirectiveInstruction CreateBuiltinSyntax(string name, int l, IReadOnlyList<string> a, string o) =>
    name switch {
      "SCOPE" => new ScopeDirectiveSyntax(l, a.FirstOrDefault()),
      "LABEL" => new LabelDirectiveSyntax(l, a.FirstOrDefault()),
      "FUNC" => new FunctionDirectiveSyntax(l, a.FirstOrDefault()),
      "CALL" => new CallDirectiveSyntax(l, a.Count == 2 ? a[1] : a.FirstOrDefault(), a.Count == 2 ? a[0] : null, ParseValueExpression(o)),
      "INLINE" => new InlineDirectiveSyntax(l, a.FirstOrDefault()), "END" => new EndDirectiveSyntax(l),
      "MATCH" => new MatchDirectiveSyntax(l, a.FirstOrDefault(), ParseBooleanExpression(o)), "ASSERT" => new AssertDirectiveSyntax(l, ParseBooleanExpression(o)),
      "CODE" => CreateCodeSyntax(l, a.FirstOrDefault(), o),
      "MIXIN" => new MixinDirectiveSyntax(l, ParseDirectiveArgument(a.FirstOrDefault()), a.Count > 1 ? ParseDirectiveArgument(a[1]) : null, ParseValueExpression(o)),
      "USING" => new UsingDirectiveSyntax(l, ParseValueExpression(o)), "LOG" => new LogDirectiveSyntax(l, ParseValueExpression(o)),
      "LOCAL" => new LocalDirectiveSyntax(l, a.FirstOrDefault(), ParseValueExpression(o)),
      "VAR" => new VariableDirectiveSyntax(l, a.FirstOrDefault(), ParseValueExpression(o)),
      "CARRY" => new CarryDirectiveSyntax(l, a.FirstOrDefault(), ParseValueExpression(o)), "RETURN" => new ReturnDirectiveSyntax(l, ParseValueExpression(o)),
      "GOTO" => new GotoDirectiveSyntax(l, a.FirstOrDefault()), "SKIP" => new SkipDirectiveSyntax(l),
      "FAIL" => new FailDirectiveSyntax(l, ParseValueExpression(o)), "ANNOTATION" => new AnnotationDirectiveSyntax(l, a.FirstOrDefault()),
      "PRELUDE" => new PreludeDirectiveSyntax(l),
      "DEFINE_TARGET" => new DefineTargetDirectiveSyntax(l, a.FirstOrDefault(), a.Count > 1 ? a[1] : null),
      _ => null
    };

  private static DirectiveArgumentSyntax ParseDirectiveArgument(string source) {
    if (!IsDynamicArgument(source)) return new DirectiveArgumentSyntax(source, null);
    return new DirectiveArgumentSyntax(null, ParseValueExpression(source.Substring(1, source.Length - 2)));
  }

  private static bool ValidateDynamicArguments(IReadOnlyList<string> arguments, out string error) {
    foreach (var argument in arguments) {
      if (!IsDynamicArgument(argument)) continue;
      if (!ValidateValueExpressionSyntax(argument.Substring(1, argument.Length - 2), out error)) return false;
    }
    error = null;
    return true;
  }

  private static CodeDirectiveSyntax CreateCodeSyntax(int line, string argument, string code) {
    switch ((argument ?? "TARGET").ToUpperInvariant()) {
      case "TARGET": return new CodeDirectiveSyntax(line, MixinExpressionOutputTarget.Target, null, ParseValueExpression(code));
      case "CLASS": return new CodeDirectiveSyntax(line, MixinExpressionOutputTarget.Class, null, ParseValueExpression(code));
      case "FILE": return new CodeDirectiveSyntax(line, MixinExpressionOutputTarget.File, null, ParseValueExpression(code));
      case "IMPLEMENTS": return new CodeDirectiveSyntax(line, MixinExpressionOutputTarget.Implements, null, ParseValueExpression(code));
      case "ANNOTATION": return new CodeDirectiveSyntax(line, MixinExpressionOutputTarget.Annotation, null, ParseValueExpression(code));
      default: return new CodeDirectiveSyntax(line, MixinExpressionOutputTarget.Injection, argument, ParseValueExpression(code));
    }
  }

  private static bool ValidateBuiltin(string name, IReadOnlyList<string> arguments, string operand, out string error) {
    if (name == "CALL") {
      if (arguments.Count is not (1 or 2) || arguments.Any(string.IsNullOrEmpty)) {
        error = "CALL requires a function label and optionally a return local";
        return false;
      }
    } else if (name == "MIXIN") {
      if (arguments.Count is < 1 or > 2 || string.IsNullOrEmpty(arguments[0])) {
        error = "MIXIN requires a target";
        return false;
      }
    } else if (name == "DEFINE_TARGET") {
      if (arguments.Count != 2 || arguments.Any(string.IsNullOrWhiteSpace)) {
        error = "DEFINE_TARGET requires a name and value";
        return false;
      }
    } else {
      var named = name is "LABEL" or "FUNC" or "INLINE" or "LOCAL" or "VAR" or "CARRY" or "ANNOTATION" or "GOTO";
      if (named && (arguments.Count != 1 || string.IsNullOrEmpty(arguments[0]))) {
        error = name + " requires a name";
        return false;
      }
      if (!named && arguments.Count > 1) {
        error = name + " accepts at most 1 argument";
        return false;
      }
    }
    var kind = name is "MATCH" or "ASSERT"
      ? DirectiveOperandKind.Boolean
      : name is "CALL" or "CODE" or "MIXIN" or "USING" or "LOCAL" or "VAR" or "CARRY" or "LOG" or "RETURN" or "FAIL"
        ? DirectiveOperandKind.Value
        : DirectiveOperandKind.None;
    return ValidateOperand(kind, operand, out error);
  }

  private static bool ValidateOperand(DirectiveOperandKind kind, string operand, out string error) {
    switch (kind) {
      case DirectiveOperandKind.Boolean: return ValidateBooleanExpressionSyntax(operand, out error);
      case DirectiveOperandKind.Value: return ValidateValueExpressionSyntax(operand, out error);
      default:
        error = null;
        return true;
    }
  }

  internal static IReadOnlyList<MixinExpressionReference> ParseBooleanExpression(string text) {
    var result = new List<MixinExpressionReference>();
    var position = 0;
    while (position < text.Length) {
      while (position < text.Length && char.IsWhiteSpace(text[position])) position++;
      if (position == text.Length) break;
      if (!MixinExpressionCompiler.TryReadReferenceNode(text, ref position, out var reference, out _)) {
        result.Add(null);
        break;
      }
      result.Add(reference);
    }
    return result.AsReadOnly();
  }

  internal static IReadOnlyList<ValueExpressionPart> ParseValueExpression(string text) {
    var result = new List<ValueExpressionPart>();
    var literal = new StringBuilder();
    var position = 0;
    while (position < text.Length) {
      if (text[position] != '@') {
        literal.Append(text[position++]);
        continue;
      }
      if (position + 1 < text.Length && text[position + 1] == '@') {
        literal.Append('@');
        position += 2;
        continue;
      }
      if (literal.Length != 0) {
        result.Add(new ValueExpressionPart(literal.ToString(), null));
        literal.Clear();
      }
      if (!MixinExpressionCompiler.TryReadReferenceNode(text, ref position, out var reference, out _)) break;
      result.Add(new ValueExpressionPart(null, reference));
    }
    if (literal.Length != 0 || result.Count == 0) result.Add(new ValueExpressionPart(literal.ToString(), null));
    return result.AsReadOnly();
  }

  internal static bool ValidateBooleanExpressionSyntax(string text, out string error) {
    error = null;
    var position = 0;
    var found = false;
    while (position < text.Length) {
      while (position < text.Length && char.IsWhiteSpace(text[position])) position++;
      if (position == text.Length) break;
      found = true;
      if (!MixinExpressionCompiler.TryReadReferenceNode(text, ref position, out _, out error)) return false;
    }
    if (found) return true;
    error = "boolean expression is empty";
    return false;
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
      if (!MixinExpressionCompiler.TryReadReferenceNode(text, ref position, out _, out error)) return false;
    }
    return true;
  }

  internal static MixinExpressionValidationResult ValidateSyntax(string expression, bool functionsOnly) {
    if (expression is null) return ValidationFailure("the expression is null", 0);
    var program = new MixinProgramSyntax(expression);
    if (program.Diagnostics.Count != 0)
      return ValidationFailure(program.Diagnostics[0].Message, program.Diagnostics[0].Line);
    string activeFunction = null;
    var functionLine = 0;
    var functionScopeOpen = false;
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
        var function = (FunctionDirectiveSyntax)parsed;
        if (!functions.Add(function.Name))
          return ValidationFailure("duplicate function '" + function.Name + "'", parsed.Line);
        activeFunction = function.Name;
        functionLine = parsed.Line;
        functionScopeOpen = false;
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

  private static MixinExpressionValidationResult ValidationFailure(string error, int line) => new(false, error, line);

  internal static bool TryReadDirective(
    string line, out string command, out IReadOnlyList<string> arguments, out string operand
  ) {
    var tokens = MixinExpressionLexer.LexLogicalLine(line, 1);
    var identifier = tokens.FirstOrDefault(token => token.Kind == MixinTokenKind.Identifier);
    if (identifier is null || tokens.Any(token => token.Kind == MixinTokenKind.Invalid)) {
      command = null;
      arguments = [];
      operand = null;
      return false;
    }
    command = identifier.Text.ToUpperInvariant();
    arguments = tokens.Where(token => token.Kind == MixinTokenKind.Argument).Select(token => token.Text).ToArray();
    operand = tokens.FirstOrDefault(token => token.Kind == MixinTokenKind.Operand)?.Text ?? "";
    return true;
  }
}
