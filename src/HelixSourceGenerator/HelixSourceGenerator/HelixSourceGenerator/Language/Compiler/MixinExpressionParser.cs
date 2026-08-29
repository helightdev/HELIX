using System;
using System.Collections.Generic;
using System.Linq;

namespace HelixSourceGenerator.Language.Compiler;

internal sealed record MixinParseDiagnostic(int Line, string Message);

internal sealed record MixinProgramParseResult(DirectiveInstruction[] Instructions,
  IReadOnlyList<MixinParseDiagnostic> Diagnostics
);

internal sealed record MixinDirectiveParseResult(DirectiveInstruction Node, string Error);

/// <summary>Builds the mixin AST from lexer tokens.</summary>
internal static class MixinExpressionParser {
  internal static string[] SplitLines(string expression) {
    return MixinExpressionLexer.SplitLogicalLines(expression);
  }

  internal static bool IsDynamicArgument(string value) {
    return MixinExpressionLexer.TryUnwrapDynamicArgument(value, out _);
  }

  internal static bool TryParseReference(
    string text, out MixinExpressionReference reference, out string error
  ) {
    reference = null;
    error = null;
    if (text is null) {
      error = "reference is null";
      return false;
    }
    var tokens = MixinExpressionLexer.LexExpression(text);
    var position = 0;
    if (TryParseReferenceTokens(tokens, ref position, out reference, out error) && position == tokens.Count)
      return true;
    error ??= "unexpected text after expression reference";
    reference = null;
    return false;
  }

  private static bool TryParseReferenceTokens(
    IReadOnlyList<MixinExpressionToken> tokens, ref int position,
    out MixinExpressionReference reference, out string error
  ) {
    reference = null;
    error = null;
    if (!Take(tokens, ref position, MixinExpressionTokenKind.At, out _)) {
      error = "expected '@' expression reference";
      return false;
    }
    var parenthesized = Take(tokens, ref position, MixinExpressionTokenKind.OpenParenthesis, out _);
    MixinExpressionRoot root;
    if (Take(tokens, ref position, MixinExpressionTokenKind.NullRoot, out _)) root = MixinExpressionRoot.Null;
    else if (!Take(tokens, ref position, MixinExpressionTokenKind.Identifier, out var rootToken)) {
      error = "expression reference has no root";
      return false;
    } else if (!TryParseRoot(rootToken.Text, out root)) {
      error = "unknown expression root '@" + rootToken.Text + "'";
      return false;
    }
    var member = Take(tokens, ref position, MixinExpressionTokenKind.Member, out var memberToken)
      ? memberToken.Text
      : null;
    var properties = new List<MixinExpressionProperty>();
    while (position < tokens.Count) {
      if (Take(tokens, ref position, MixinExpressionTokenKind.Path, out var path)) {
        properties.Add(new MixinExpressionProperty("path", path.Text));
        continue;
      }
      var negated = Take(tokens, ref position, MixinExpressionTokenKind.Negation, out _);
      Take(tokens, ref position, MixinExpressionTokenKind.Predicate, out _);
      if (!Take(tokens, ref position, MixinExpressionTokenKind.Property, out var propertyToken)) {
        if (negated) {
          error = "property name is empty";
          return false;
        }
        break;
      }
      var arguments = new List<string>();
      while (Take(tokens, ref position, MixinExpressionTokenKind.Argument, out var argument))
        arguments.Add(argument.Text);
      var parsed = arguments.Select(argument => ParsePropertyArgument(propertyToken.Text, argument)).ToArray();
      var property = new MixinExpressionProperty(
        propertyToken.Text, arguments.AsReadOnly(), parsed, negated
      );
      if (FunctionLibrary.TryGet(property.Name, out var function) && !function.Validate(property, out error))
        return false;
      properties.Add(property);
    }
    for (var index = 0; index + 1 < properties.Count; index++) {
      if (!FunctionLibrary.IsPredicate(properties[index].Name)) continue;
      error = "boolean operation ':" + properties[index].Name + "' must be terminal";
      return false;
    }
    Take(tokens, ref position, MixinExpressionTokenKind.CloseParenthesis, out _);
    if (position < tokens.Count && tokens[position].Kind == MixinExpressionTokenKind.Invalid) {
      error = string.IsNullOrEmpty(tokens[position].Text) ? "invalid expression reference" : tokens[position].Text;
      return false;
    }
    reference = new MixinExpressionReference(root, member, properties.AsReadOnly(), parenthesized);
    return true;
  }

  private static MixinPropertyArgumentSyntax ParsePropertyArgument(string property, string argument) {
    if (property is "and" or "or" && MixinExpressionLexer.TryUnwrapDynamicArgument(argument, out var boolean)) {
      return new MixinPropertyArgumentSyntax(
        null, null, ParseBooleanExpression(boolean)
      );
    }
    if (MixinExpressionLexer.TryUnwrapDynamicArgument(argument, out var value)) {
      return new MixinPropertyArgumentSyntax(
        null, ParseValueExpression(value), null
      );
    }
    return new MixinPropertyArgumentSyntax(argument, null, null);
  }

  private static bool Take(
    IReadOnlyList<MixinExpressionToken> tokens, ref int position,
    MixinExpressionTokenKind kind, out MixinExpressionToken token
  ) {
    if (position < tokens.Count && tokens[position].Kind == kind) {
      token = tokens[position++];
      return true;
    }
    token = null;
    return false;
  }

  private static bool TryParseRoot(string keyword, out MixinExpressionRoot root) {
    switch (keyword) {
      case "target":
        root = MixinExpressionRoot.Target;
        return true;
      case "this":
        root = MixinExpressionRoot.This;
        return true;
      case "attr":
        root = MixinExpressionRoot.Attribute;
        return true;
      case "arg":
        root = MixinExpressionRoot.Argument;
        return true;
      case "var":
        root = MixinExpressionRoot.Variable;
        return true;
      case "local":
        root = MixinExpressionRoot.Local;
        return true;
      case "true":
        root = MixinExpressionRoot.True;
        return true;
      case "false":
        root = MixinExpressionRoot.False;
        return true;
      case "null":
        root = MixinExpressionRoot.Null;
        return true;
      case "table":
        root = MixinExpressionRoot.Table;
        return true;
      case "param":
        root = MixinExpressionRoot.Parameter;
        return true;
      case "carry":
        root = MixinExpressionRoot.Carry;
        return true;
      default:
        root = default;
        return false;
    }
  }

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
    if (tokens.Any(token => token.Kind is MixinTokenKind.Invalid or MixinTokenKind.OrphanContinuation)) {
      return new MixinDirectiveParseResult(
        new EmptyDirectiveSyntax(line), tokens.Any(token => token.Kind == MixinTokenKind.OrphanContinuation)
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
    if (!DirectiveLibrary.TryGet(name, out var directive)) {
      return new MixinDirectiveParseResult(
        new UnknownDirectiveSyntax(line, name), "unknown directive '@" + name + "'"
      );
    }
    var invocationValid = directive.Validate(arguments, operand, out var invocationError) &&
      ValidateOperand(directive.OperandKind, operand, out invocationError) &&
      ValidateDynamicArguments(arguments, out invocationError);
    var parsedArguments = arguments.Select(ParseDirectiveArgument).ToArray();
    return new MixinDirectiveParseResult(
      new DirectiveInvocationSyntax(line, directive, parsedArguments, ParseValueExpression(operand)),
      invocationValid ? null : invocationError
    );
  }

  private static DirectiveInstruction CreateBuiltinSyntax(string name, int l, IReadOnlyList<string> a, string o) {
    return name switch {
      "SCOPE" => new ScopeDirectiveSyntax(l, a.FirstOrDefault()),
      "LABEL" => new LabelDirectiveSyntax(l, a.FirstOrDefault()),
      "FUNC" => new FunctionDirectiveSyntax(l, a.FirstOrDefault()),
      "CALL" => new CallDirectiveSyntax(
        l, a.Count == 2 ? a[1] : a.FirstOrDefault(), a.Count == 2 ? a[0] : null, ParseValueExpression(o)
      ),
      "INLINE" => new InlineDirectiveSyntax(l, a.FirstOrDefault()), "END" => new EndDirectiveSyntax(l),
      "MATCH" => new MatchDirectiveSyntax(l, a.FirstOrDefault(), ParseBooleanExpression(o)),
      "ASSERT" => new AssertDirectiveSyntax(l, ParseBooleanExpression(o)),
      "CODE" => CreateCodeSyntax(l, a.FirstOrDefault(), o),
      "MIXIN" => new MixinDirectiveSyntax(
        l, ParseDirectiveArgument(a.FirstOrDefault()), a.Count > 1 ? ParseDirectiveArgument(a[1]) : null,
        ParseValueExpression(o)
      ),
      "USING" => new UsingDirectiveSyntax(l, ParseValueExpression(o)),
      "LOG" => new LogDirectiveSyntax(l, ParseValueExpression(o)),
      "LOCAL" => new LocalDirectiveSyntax(l, a.FirstOrDefault(), ParseValueExpression(o)),
      "VAR" => new VariableDirectiveSyntax(l, a.FirstOrDefault(), ParseValueExpression(o)),
      "CARRY" => new CarryDirectiveSyntax(l, a.FirstOrDefault(), ParseValueExpression(o)),
      "RETURN" => new ReturnDirectiveSyntax(l, ParseValueExpression(o)),
      "GOTO" => new GotoDirectiveSyntax(l, a.FirstOrDefault()), "SKIP" => new SkipDirectiveSyntax(l),
      "FAIL" => new FailDirectiveSyntax(l, ParseValueExpression(o)),
      "ANNOTATION" => new AnnotationDirectiveSyntax(l, a.FirstOrDefault()),
      "PRELUDE" => new PreludeDirectiveSyntax(l),
      "DEFINE_TARGET" => new DefineTargetDirectiveSyntax(l, a.FirstOrDefault(), a.Count > 1 ? a[1] : null),
      _ => null
    };
  }

  private static DirectiveArgumentSyntax ParseDirectiveArgument(string source) {
    if (!MixinExpressionLexer.TryUnwrapDynamicArgument(source, out var expression))
      return new DirectiveArgumentSyntax(source, null);
    return new DirectiveArgumentSyntax(null, ParseValueExpression(expression));
  }

  private static bool ValidateDynamicArguments(IReadOnlyList<string> arguments, out string error) {
    foreach (var argument in arguments) {
      if (!MixinExpressionLexer.TryUnwrapDynamicArgument(argument, out var expression)) continue;
      if (!ValidateValueExpressionSyntax(expression, out error)) return false;
    }
    error = null;
    return true;
  }

  private static CodeDirectiveSyntax CreateCodeSyntax(int line, string argument, string code) {
    switch ((argument ?? "TARGET").ToUpperInvariant()) {
      case "TARGET":
        return new CodeDirectiveSyntax(line, MixinExpressionOutputTarget.Target, null, ParseValueExpression(code));
      case "CLASS":
        return new CodeDirectiveSyntax(line, MixinExpressionOutputTarget.Class, null, ParseValueExpression(code));
      case "FILE":
        return new CodeDirectiveSyntax(line, MixinExpressionOutputTarget.File, null, ParseValueExpression(code));
      case "EXTENDS":
        return new CodeDirectiveSyntax(line, MixinExpressionOutputTarget.Extends, null, ParseValueExpression(code));
      case "IMPLEMENTS":
        return new CodeDirectiveSyntax(line, MixinExpressionOutputTarget.Implements, null, ParseValueExpression(code));
      case "ANNOTATION":
        return new CodeDirectiveSyntax(line, MixinExpressionOutputTarget.Annotation, null, ParseValueExpression(code));
      default:
        return new CodeDirectiveSyntax(
          line, MixinExpressionOutputTarget.Injection, argument, ParseValueExpression(code)
        );
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
    var tokens = MixinExpressionLexer.LexExpression(text);
    var position = 0;
    while (position < tokens.Count) {
      if (tokens[position].Kind == MixinExpressionTokenKind.Literal &&
        string.IsNullOrWhiteSpace(tokens[position].Text)) {
        position++;
        continue;
      }
      if (!TryParseReferenceTokens(tokens, ref position, out var reference, out _)) {
        result.Add(null);
        break;
      }
      result.Add(reference);
    }
    return result.AsReadOnly();
  }

  internal static IReadOnlyList<ValueExpressionPart> ParseValueExpression(string text) {
    var result = new List<ValueExpressionPart>();
    var tokens = MixinExpressionLexer.LexExpression(text);
    var position = 0;
    while (position < tokens.Count) {
      if (Take(tokens, ref position, MixinExpressionTokenKind.Literal, out var literal)) {
        result.Add(new ValueExpressionPart(literal.Text, null));
        continue;
      }
      var referenceStart = position;
      if (!TryParseReferenceTokens(tokens, ref position, out var reference, out _)) {
        position = referenceStart + 1;
        while (position < tokens.Count && tokens[position].Kind is not (
          MixinExpressionTokenKind.At or MixinExpressionTokenKind.Literal
          )) position++;
        var end = position < tokens.Count ? tokens[position].Start : text.Length;
        result.Add(
          new ValueExpressionPart(
            text.Substring(tokens[referenceStart].Start, end - tokens[referenceStart].Start), null, true
          )
        );
        continue;
      }
      result.Add(new ValueExpressionPart(null, reference));
    }
    if (result.Count == 0) result.Add(new ValueExpressionPart("", null));
    return result.AsReadOnly();
  }

  internal static bool ValidateBooleanExpressionSyntax(string text, out string error) {
    error = null;
    var tokens = MixinExpressionLexer.LexExpression(text);
    var position = 0;
    var found = false;
    while (position < tokens.Count) {
      if (tokens[position].Kind == MixinExpressionTokenKind.Literal &&
        string.IsNullOrWhiteSpace(tokens[position].Text)) {
        position++;
        continue;
      }
      found = true;
      if (!TryParseReferenceTokens(tokens, ref position, out _, out error)) return false;
    }
    if (found) return true;
    error = "boolean expression is empty";
    return false;
  }

  internal static bool ValidateValueExpressionSyntax(string text, out string error) {
    error = null;
    var tokens = MixinExpressionLexer.LexExpression(text);
    var position = 0;
    while (position < tokens.Count) {
      if (Take(tokens, ref position, MixinExpressionTokenKind.Literal, out _)) continue;
      if (!TryParseReferenceTokens(tokens, ref position, out _, out error)) return false;
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

  private static MixinExpressionValidationResult ValidationFailure(string error, int line) {
    return new MixinExpressionValidationResult(false, error, line);
  }

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