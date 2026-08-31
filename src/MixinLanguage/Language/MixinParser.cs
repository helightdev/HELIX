using System;
using System.Collections.Generic;
using System.Linq;
using Mixins.Compiler;
using Mixins.Runtime;

namespace Mixins;

public sealed record MixinParseDiagnostic(int Line, string Message);

internal sealed record MixinProgramParseResult(InstructionAst[] Instructions,
  IReadOnlyList<MixinParseDiagnostic> Diagnostics, IReadOnlyList<MixinToken> Tokens
);

internal sealed record MixinDirectiveParseResult(InstructionAst Node, string Error);

internal sealed record MixinDirectiveTokenGroup(
  IReadOnlyList<MixinToken> Tokens, IReadOnlyList<MixinSourceRange> Continuations,
  int ContinuedFromLine, int SourceEnd
);

/// <summary>Builds the mixin AST from lexer tokens.</summary>
public static class MixinParser {
  private static IReadOnlyList<MixinToken> TokensForExpression(string text, int offset = 0) =>
    OffsetTokens(MixinLexer.Lex(text ?? ""), offset);

  public static ProgramAst Parse(string expression) {
    return new ProgramAst(expression);
  }

  internal static bool IsDynamicArgument(string value) {
    return MixinLexer.TryUnwrapDynamicArgument(value, out _);
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
    var cursor = new MixinTokenStream(TokensForExpression(text));
    if (TryParseReference(cursor, out reference, out error) && cursor.AtEnd)
      return true;
    error ??= "unexpected text after expression reference";
    reference = null;
    return false;
  }

  private static bool TryParseReference(
    MixinTokenStream cursor,
    out MixinExpressionReference reference, out string error
  ) {
    reference = null;
    error = null;
    if (!cursor.Take(MixinTokenKind.At, out var atToken)) {
      error = "expected '@' expression reference";
      return false;
    }
    var parenthesized = cursor.Take(MixinTokenKind.OpenParenthesis, out _);
    cursor.Take(MixinTokenKind.Identifier, out var rootToken);
    var rootText = rootToken?.Text ?? "";
    MixinExpressionRoot root;
    if (rootText.Length == 0) root = MixinExpressionRoot.Null;
    else if (!MixinRootLibrary.TryGet(rootText, out var rootDefinition)) {
      error = "unknown expression root '@" + rootText + "'";
      return false;
    } else root = rootDefinition.Root;
    if (parenthesized && rootToken is null) {
      error = "expression reference has no root";
      return false;
    }
    MixinToken memberToken = null;
    string member = null;
    var properties = new List<MixinExpressionProperty>();
    while (!cursor.AtEnd) {
      if (cursor.Take(MixinTokenKind.Hash, out var hash)) {
        if (!cursor.Take(MixinTokenKind.Identifier, out var path)) {
          error = "path name is empty";
          return false;
        }
        var pathName = path.Text;
        if (member is null && properties.Count == 0) {
          member = pathName;
          memberToken = path;
          continue;
        }
        properties.Add(new MixinExpressionProperty(
          "path", [new MixinPropertyArgumentAst(pathName, null, null, path.SourceRange)],
          sourceRange: new MixinSourceRange(hash.Start, path.End, hash.SourceRange.Line, hash.SourceRange.Column),
          nameRange: path.SourceRange
        ));
        continue;
      }
      if (!TryTakeOperator(cursor, out var operatorToken)) break;
      var negated = operatorToken.Kind == MixinTokenKind.NegatedPredicateOperator;
      var predicate = operatorToken.Kind is MixinTokenKind.PredicateOperator or
        MixinTokenKind.NegatedPredicateOperator;
      if (!cursor.Take(MixinTokenKind.Identifier, out var propertyToken)) {
        error = "property name is empty";
        return false;
      }
      FunctionLibrary.TryResolve(propertyToken.Text, predicate, out var function);
      var arguments = new List<MixinPropertyArgumentAst>();
      while (cursor.At(MixinTokenKind.OpenArgument)) {
        if (!TryParsePropertyArgument(
          cursor, function?.GetArgumentType(arguments.Count) == MixinLanguageValueKind.Boolean,
          out var argument
        )) {
          error = "invalid argument for property ':" + propertyToken.Text + "'";
          return false;
        }
        arguments.Add(argument);
      }
      var propertyName = function?.Name ?? propertyToken.Text;
      var propertyStart = operatorToken.Start;
      var propertyEnd = cursor.Previous?.End ?? propertyToken.End;
      var property = new MixinExpressionProperty(
        propertyName, arguments.AsReadOnly(), negated,
        sourceRange: new MixinSourceRange(
          propertyStart, propertyEnd, operatorToken.SourceRange.Line, operatorToken.SourceRange.Column
        ),
        nameRange: propertyToken.SourceRange
      );
      if (function is not null && (property.Arguments.Count < function.MinimumArguments ||
          property.Arguments.Count > function.MaximumArguments)) {
        error = function.MinimumArguments == function.MaximumArguments
          ? ":" + property.Name + " requires " + function.MinimumArguments +
          (function.MinimumArguments == 1 ? " argument" : " arguments")
          : ":" + property.Name + " accepts at most " + function.MaximumArguments + " arguments";
        return false;
      }
      if (function is not null)
        for (var index = 0; index < arguments.Count; index++)
          if (function.GetArgumentType(index) == MixinLanguageValueKind.Boolean &&
              arguments[index].BooleanExpression is null) {
            error = ":" + property.Name + " arguments must be dynamic boolean expressions";
            return false;
          }
      properties.Add(property);
    }
    for (var index = 0; index + 1 < properties.Count; index++) {
      if (!FunctionLibrary.IsPredicate(properties[index].Name)) continue;
      error = "boolean operation ':" + properties[index].Name + "' must be terminal";
      return false;
    }
    if (parenthesized && !cursor.Take(MixinTokenKind.CloseParenthesis, out _)) {
      error = "parenthesized expression reference is not closed";
      return false;
    }
    if (cursor.At(MixinTokenKind.Invalid)) {
      error = string.IsNullOrEmpty(cursor.Current.Text) ? "invalid expression reference" : cursor.Current.Text;
      return false;
    }
    var referenceEnd = cursor.Previous?.End ?? atToken.End;
    reference = new MixinExpressionReference(
      root, member, properties.AsReadOnly(), parenthesized,
      new MixinSourceRange(
        atToken.Start, referenceEnd, atToken.SourceRange.Line, atToken.SourceRange.Column
      ),
      rootToken?.SourceRange ?? new MixinSourceRange(atToken.End, atToken.End,
        atToken.SourceRange.Line, atToken.SourceRange.Column + 1),
      memberToken?.SourceRange ?? default
    );
    return true;
  }

  private static bool TryParsePropertyArgument(
    MixinTokenStream cursor, bool booleanExpression,
    out MixinPropertyArgumentAst argument
  ) {
    if (!cursor.Take(MixinTokenKind.OpenArgument, out var open)) {
      argument = null;
      return false;
    }
    var start = cursor.Position;
    var depth = 1;
    while (!cursor.AtEnd && depth != 0) {
      if (cursor.Current.Kind == MixinTokenKind.OpenArgument) depth++;
      else if (cursor.Current.Kind == MixinTokenKind.CloseArgument) depth--;
      if (depth != 0) cursor.Consume();
    }
    if (depth != 0) {
      argument = null;
      return false;
    }
    var close = cursor.Consume();
    var content = cursor.Slice(start, cursor.Position - 1);
    var raw = string.Concat(content.Select(token => token.Text));
    var range = new MixinSourceRange(open.Start, close.End, open.SourceRange.Line, open.SourceRange.Column);
    if (content.Count >= 2 && content[0].Kind == MixinTokenKind.OpenParenthesis &&
        content[content.Count - 1].Kind == MixinTokenKind.CloseParenthesis) {
      var expression = TrimTokens(content, 1, 1);
      argument = booleanExpression
        ? new MixinPropertyArgumentAst(null, null, ParseBooleanExpression(expression), range)
        : new MixinPropertyArgumentAst(null, ParseValueExpression(expression), null, range);
    } else argument = new MixinPropertyArgumentAst(raw, null, null, range);
    return true;
  }

  private static bool TryTakeOperator(MixinTokenStream cursor, out MixinToken token) {
    if (cursor.Current?.Kind is MixinTokenKind.FunctionOperator or
        MixinTokenKind.PredicateOperator or MixinTokenKind.NegatedPredicateOperator) {
      token = cursor.Consume();
      return true;
    }
    token = null;
    return false;
  }

  internal static MixinProgramParseResult ParseProgram(string source) {
    source ??= "";
    var sourceTokens = MixinLexer.Lex(source);
    var result = new List<InstructionAst>();
    var diagnostics = new List<MixinParseDiagnostic>();
    var groups = BuildDirectiveTokenGroups(sourceTokens);
    var lineStarts = SourceLineStarts(source);
    for (var index = 0; index < groups.Count; index++) {
      var group = groups[index];
      var line = index + 1;
      var parsed = group.ContinuedFromLine >= 0
        ? new MixinDirectiveParseResult(new EmptyDirectiveAst { SourceRange = new MixinSourceRange(
          lineStarts[index], Math.Max(lineStarts[index], group.SourceEnd), line, 0
        ) }, null)
        : ParseSharedTokens(source, group.Tokens, line);
      if (group.ContinuedFromLine < 0) {
        parsed.Node.SourceRange = new MixinSourceRange(
          lineStarts[index], Math.Max(lineStarts[index], group.SourceEnd),
          line, 0
        );
        AttachSyntaxChildren(parsed.Node, group.Continuations);
        AttachTokens(parsed.Node, sourceTokens);
      }
      result.Add(parsed.Node);
      if (parsed.Error is not null) diagnostics.Add(new MixinParseDiagnostic(line, parsed.Error));
    }
    return new MixinProgramParseResult([.. result], diagnostics.AsReadOnly(), sourceTokens);
  }

  private static IReadOnlyList<MixinDirectiveTokenGroup> BuildDirectiveTokenGroups(
    IReadOnlyList<MixinToken> sourceTokens
  ) {
    var lineCount = sourceTokens.Count(token => token.Kind == MixinTokenKind.NewLine) + 1;
    var tokens = Enumerable.Range(0, lineCount).Select(_ => new List<MixinToken>()).ToArray();
    foreach (var token in sourceTokens)
      if (token.Kind != MixinTokenKind.NewLine && token.Line > 0 && token.Line <= lineCount)
        tokens[token.Line - 1].Add(token);
    var continuations = Enumerable.Range(0, lineCount)
      .Select(_ => new List<MixinSourceRange>()).ToArray();
    var continuedFrom = Enumerable.Repeat(-1, lineCount).ToArray();
    var sourceEnds = new int[lineCount];
    foreach (var token in sourceTokens) {
      if (token.Line <= 0 || token.Line > lineCount) continue;
      sourceEnds[token.Line - 1] = Math.Max(sourceEnds[token.Line - 1], token.End);
    }
    var active = -1;
    for (var line = 0; line < lineCount; line++) {
      var first = tokens[line].FirstOrDefault(token => token.Kind != MixinTokenKind.Whitespace);
      if (first?.Kind is MixinTokenKind.DirectContinuation or MixinTokenKind.NewLineContinuation) {
        if (active >= 0) {
          continuedFrom[line] = active;
          continuations[active].Add(first.SourceRange);
          if (first.Kind == MixinTokenKind.NewLineContinuation) tokens[active].Add(
            new MixinToken(MixinTokenKind.Text, "\n", first.Start, first.End)
              .WithLocation(first.Line, first.SourceRange.Column)
          );
          var markerIndex = tokens[line].IndexOf(first);
          tokens[active].AddRange(tokens[line].Skip(markerIndex + 1));
          sourceEnds[active] = Math.Max(sourceEnds[active], sourceEnds[line]);
        } else active = -1;
        continue;
      }
      active = first is { Kind: MixinTokenKind.At } ? line : -1;
    }
    return tokens.Select((items, line) => new MixinDirectiveTokenGroup(
      items.AsReadOnly(), continuations[line].AsReadOnly(), continuedFrom[line], sourceEnds[line]
    )).ToArray();
  }

  private static int[] SourceLineStarts(string source) {
    var starts = new List<int> { 0 };
    for (var index = 0; index < source.Length; index++) {
      if (source[index] == '\r' && index + 1 < source.Length && source[index + 1] == '\n') index++;
      else if (source[index] != '\n' && source[index] != '\r') continue;
      starts.Add(index + 1);
    }
    return starts.ToArray();
  }

  internal static MixinDirectiveParseResult ParseDirective(string text, int line) {
    var tokens = MixinLexer.Lex(text ?? "");
    var parsed = ParseSharedTokens(text ?? "", tokens, line);
    parsed.Node.SourceRange = new MixinSourceRange(0, (text ?? string.Empty).Length, line, 0);
    AttachSyntaxChildren(parsed.Node, []);
    AttachTokens(parsed.Node, tokens);
    return parsed;
  }

  private static MixinDirectiveParseResult ParseSharedTokens(
    string source, IReadOnlyList<MixinToken> tokens, int line
  ) {
    var cursor = new MixinTokenStream(tokens);
    var sourceRange = new MixinSourceRange(
      tokens.Count == 0 ? 0 : tokens[0].Start,
      tokens.Count == 0 ? source.Length : tokens[tokens.Count - 1].End,
      line, tokens.Count == 0 ? 0 : tokens[0].SourceRange.Column
    );
    while (cursor.At(MixinTokenKind.Whitespace)) cursor.Consume();
    if (cursor.AtEnd || cursor.At(MixinTokenKind.Comment))
      return new MixinDirectiveParseResult(new EmptyDirectiveAst { SourceRange = sourceRange }, null);
    if (cursor.Current?.Kind is MixinTokenKind.DirectContinuation or MixinTokenKind.NewLineContinuation)
      return new MixinDirectiveParseResult(
        new EmptyDirectiveAst { SourceRange = sourceRange }, "continuation requires an immediately preceding directive");
    if (!cursor.Take(MixinTokenKind.At, out var marker) ||
        !cursor.Take(MixinTokenKind.Identifier, out var directive))
      return new MixinDirectiveParseResult(new EmptyDirectiveAst { SourceRange = sourceRange }, "expected an expression directive");
    var name = directive.Text.ToUpperInvariant();
    var arguments = new List<string>();
    var argumentTokens = new List<IReadOnlyList<MixinToken>>();
    var argumentRanges = new List<MixinSourceRange>();
    var argumentContentRanges = new List<MixinSourceRange>();
    var previousEnd = directive.End;
    while (cursor.At(MixinTokenKind.OpenArgument) && cursor.Current.Start == previousEnd) {
      var open = cursor.Consume();
      var start = open.Start;
      var depth = 1;
      var content = new List<MixinToken>();
      MixinToken close = null;
      while (!cursor.AtEnd && depth != 0) {
        var token = cursor.Consume();
        if (token.Kind == MixinTokenKind.OpenArgument) depth++;
        else if (token.Kind == MixinTokenKind.CloseArgument) depth--;
        if (depth == 0) close = token;
        else content.Add(token);
      }
      if (depth != 0)
        return new MixinDirectiveParseResult(
          new EmptyDirectiveAst { SourceRange = sourceRange }, "expected an expression directive");
      var raw = string.Concat(content.Select(token => token.Text));
      var trimmed = raw.Trim();
      var leading = raw.Length - raw.TrimStart().Length;
      var trailing = raw.Length - raw.TrimEnd().Length;
      var contentStart = content.Count == 0 ? open.End : content[0].Start + leading;
      var contentEnd = content.Count == 0 ? open.End : content[content.Count - 1].End - trailing;
      arguments.Add(trimmed);
      argumentTokens.Add(TrimTokens(content, leading, trailing));
      argumentRanges.Add(new MixinSourceRange(start, close.End, line, open.SourceRange.Column));
      argumentContentRanges.Add(new MixinSourceRange(
        contentStart, Math.Max(contentStart, contentEnd), line,
        content.Count == 0 ? open.SourceRange.Column + 1 : content[0].SourceRange.Column + leading
      ));
      previousEnd = close.End;
    }
    while (cursor.At(MixinTokenKind.Whitespace)) cursor.Consume();
    var operandTokens = cursor.Slice(cursor.Position, cursor.Tokens.Count).ToList();
    if (operandTokens.Count > 0 && operandTokens[0].Kind == MixinTokenKind.Text) {
      var token = operandTokens[0];
      var leading = token.Text.Length - token.Text.TrimStart(' ', '\t').Length;
      if (leading == token.Text.Length) operandTokens.RemoveAt(0);
      else if (leading > 0) operandTokens[0] = new MixinToken(
        MixinTokenKind.Text, token.Text.Substring(leading), token.Start + leading, token.End
      ).WithLocation(token.Line, token.SourceRange.Column + leading);
    }
    var operandStart = operandTokens.Count == 0 ? previousEnd : operandTokens[0].Start;
    var operandEnd = operandTokens.Count == 0 ? operandStart : operandTokens[operandTokens.Count - 1].End;
    var operandRange = new MixinSourceRange(operandStart, operandEnd, line,
      operandTokens.Count == 0 ? directive.SourceRange.Column + directive.SourceRange.Length : operandTokens[0].SourceRange.Column);
    var markerRange = marker.SourceRange;
    var nameRange = directive.SourceRange;
    var operandText = string.Concat(operandTokens.Select(token => token.Text));
    if (!DirectiveLibrary.TryGet(name, out var definition)) {
      var unknown = new UnknownDirectiveAst(name) { SourceRange = sourceRange };
      CompleteDirective(unknown, null, markerRange, nameRange, argumentRanges,
        argumentContentRanges, operandRange);
      return new MixinDirectiveParseResult(unknown, "unknown directive '@" + name + "'");
    }
    var valid = definition.Validate(arguments, operandText, out var error) &&
      ValidateOperand(definition.OperandKind, operandTokens, out error) &&
      ValidateExpressionArguments(argumentTokens, out error);
    var parsedArguments = arguments.Select((argument, index) =>
      ParseDirectiveArgument(argument, argumentTokens[index], argumentContentRanges[index])).ToArray();
    var valueOperand = definition.OperandKind == DirectiveOperandKind.Value
      ? ParseValueExpression(operandTokens) : [];
    var booleanOperand = definition.OperandKind == DirectiveOperandKind.Boolean
      ? ParseBooleanExpression(operandTokens) : [];
    var node = definition.CreateSyntax(new MixinDirectiveSyntaxData(
      arguments, parsedArguments, valueOperand, booleanOperand
    ));
    node.SourceRange = sourceRange;
    node.ParsedArguments = parsedArguments;
    CompleteDirective(node, definition, markerRange, nameRange, argumentRanges,
      argumentContentRanges, operandRange);
    return new MixinDirectiveParseResult(node, valid ? null : error);
  }

  private static void CompleteDirective(
    InstructionAst node, DirectiveDefinition definition,
    MixinSourceRange markerRange, MixinSourceRange nameRange,
    IReadOnlyList<MixinSourceRange> argumentRanges,
    IReadOnlyList<MixinSourceRange> argumentContentRanges,
    MixinSourceRange operandRange
  ) {
    node.Definition = definition;
    node.MarkerRange = markerRange;
    node.NameRange = nameRange;
    node.ArgumentRanges = argumentRanges;
    node.ArgumentContentRanges = argumentContentRanges;
    node.OperandRange = operandRange;
  }

  private static IReadOnlyList<MixinToken> TrimTokens(
    IReadOnlyList<MixinToken> tokens, int leading, int trailing
  ) {
    var result = tokens.ToList();
    while (result.Count > 0 && leading >= result[0].Text.Length) {
      leading -= result[0].Text.Length;
      result.RemoveAt(0);
    }
    if (result.Count > 0 && leading > 0) result[0] = SliceToken(
      result[0], leading, result[0].Text.Length
    );
    while (result.Count > 0 && trailing >= result[result.Count - 1].Text.Length) {
      trailing -= result[result.Count - 1].Text.Length;
      result.RemoveAt(result.Count - 1);
    }
    if (result.Count > 0 && trailing > 0) {
      var last = result.Count - 1;
      result[last] = SliceToken(result[last], 0, result[last].Text.Length - trailing);
    }
    return result.AsReadOnly();
  }

  private static MixinToken SliceToken(MixinToken token, int start, int end) =>
    new MixinToken(token.Kind, token.Text.Substring(start, end - start),
      token.Start + start, token.Start + end).WithLocation(
      token.Line, token.SourceRange.Column + start
    );

  private static void AttachSyntaxChildren(
    InstructionAst node, IReadOnlyList<MixinSourceRange> continuations
  ) {
    var children = new List<MixinAst>();
    if (!node.NameRange.IsEmpty) children.Add(new LeafAst(
      MixinSyntaxKind.DirectiveName,
      new MixinSourceRange(
        node.MarkerRange.Start, node.NameRange.End,
        node.MarkerRange.Line, node.MarkerRange.Column
      )
    ));
    MixinLanguageCatalog.TryGetDirective(MixinSyntaxFacts.Command(node), out var directive);
    for (var index = 0; index < node.ArgumentRanges.Count; index++) {
      var metadata = directive?.Definition.GetArgumentMetadata(node.ArgumentRanges.Count, index);
      var argument = index < node.ParsedArguments.Count
        ? node.ParsedArguments[index]
        : new DirectiveArgumentAst(null, null, node.ArgumentRanges[index]);
      argument.Kind = DirectiveArgumentKind(metadata);
      argument.ArgumentMetadata = metadata;
      children.Add(argument);
    }
    var operandChildren = new List<MixinAst>();
    if (node is ValueStatementAst value)
      operandChildren.AddRange(value.Expression);
    if (node is DirectiveInvocationAst invocation)
      operandChildren.AddRange(invocation.Expression);
    if (node is BooleanStatementAst boolean)
      foreach (var reference in boolean.Expression)
        if (reference is not null) operandChildren.Add(reference);
    if (!node.OperandRange.IsEmpty || operandChildren.Count != 0)
      children.Add(new LeafAst(
        MixinSyntaxKind.Operand, node.OperandRange, operandChildren.AsReadOnly()
      ));
    foreach (var continuation in continuations)
      children.Add(new TriviaAst(MixinSyntaxKind.Continuation, continuation));
    node.Kind = node is EmptyDirectiveAst
      ? MixinSyntaxKind.Operand
      : MixinSyntaxKind.Directive;
    node.Children = children.AsReadOnly();
  }

  private static void AttachTokens(
    MixinAst node, IReadOnlyList<MixinToken> tokens
  ) {
    node.Tokens = tokens.Where(token =>
      token.Start >= node.SourceRange.Start && token.End <= node.SourceRange.End
    ).ToArray();
    foreach (var child in node.Children) AttachTokens(child, node.Tokens);
  }

  private static MixinSyntaxKind DirectiveArgumentKind(MixinDirectiveArgumentMetadata metadata) =>
    metadata?.SymbolUsage switch {
      MixinSymbolUsage.Declaration => MixinSyntaxKind.DeclarationDirectiveArgument,
      MixinSymbolUsage.Reference => MixinSyntaxKind.ReferenceDirectiveArgument,
      MixinSymbolUsage.Declaration | MixinSymbolUsage.Reference =>
        MixinSyntaxKind.DeclarationReferenceDirectiveArgument,
      _ => MixinSyntaxKind.DirectiveArgument
    };

  private static DirectiveArgumentAst ParseDirectiveArgument(
    string source, IReadOnlyList<MixinToken> tokens, MixinSourceRange range
  ) {
    if (!IsDynamicArgument(tokens))
      return new DirectiveArgumentAst(source, null, range);
    var inner = TrimTokens(tokens, 1, 1);
    return new DirectiveArgumentAst(null, ParseValueExpression(inner), range);
  }

  private static bool IsDynamicArgument(IReadOnlyList<MixinToken> tokens) =>
    tokens.Count >= 2 && tokens[0].Kind == MixinTokenKind.OpenParenthesis &&
    tokens[tokens.Count - 1].Kind == MixinTokenKind.CloseParenthesis;

  private static bool ValidateExpressionArguments(
    IReadOnlyList<IReadOnlyList<MixinToken>> arguments, out string error
  ) {
    foreach (var argument in arguments) {
      if (!IsDynamicArgument(argument)) continue;
      if (!ValidateValueExpressionSyntax(TrimTokens(argument, 1, 1), out error)) return false;
    }
    error = null;
    return true;
  }

  private static bool ValidateOperand(
    DirectiveOperandKind kind, IReadOnlyList<MixinToken> operand, out string error
  ) {
    switch (kind) {
      case DirectiveOperandKind.Boolean: return ValidateBooleanExpressionSyntax(operand, out error);
      case DirectiveOperandKind.Value: return ValidateValueExpressionSyntax(operand, out error);
      default:
        error = null;
        return true;
    }
  }

  internal static IReadOnlyList<MixinExpressionReference> ParseBooleanExpression(
    string text, int sourceOffset = 0
  ) {
    return ParseBooleanExpression(OffsetTokens(TokensForExpression(text), sourceOffset));
  }

  private static IReadOnlyList<MixinExpressionReference> ParseBooleanExpression(
    IReadOnlyList<MixinToken> tokens
  ) {
    var result = new List<MixinExpressionReference>();
    var cursor = new MixinTokenStream(tokens);
    while (!cursor.AtEnd) {
      if (cursor.Current.Kind is MixinTokenKind.Whitespace or MixinTokenKind.Text &&
        string.IsNullOrWhiteSpace(cursor.Current.Text)) {
        cursor.Consume();
        continue;
      }
      if (!TryParseReference(cursor, out var reference, out _)) {
        result.Add(null);
        break;
      }
      result.Add(reference);
    }
    return result.AsReadOnly();
  }

  internal static IReadOnlyList<ValueAst> ParseValueExpression(string text, int sourceOffset = 0) {
    return ParseValueExpression(
      OffsetTokens(TokensForExpression(text), sourceOffset), text, sourceOffset
    );
  }

  /// <summary>Parses a standalone value expression using the compiler grammar.</summary>
  public static IReadOnlyList<ValueAst> ParseExpressionValues(string text) =>
    ParseValueExpression(text);

  private static IReadOnlyList<ValueAst> ParseValueExpression(
    IReadOnlyList<MixinToken> tokens
  ) {
    return ParseValueExpression(tokens, null, 0);
  }

  private static IReadOnlyList<ValueAst> ParseValueExpression(
    IReadOnlyList<MixinToken> tokens, string source, int sourceOffset
  ) {
    var result = new List<ValueAst>();
    var cursor = new MixinTokenStream(tokens);
    while (!cursor.AtEnd) {
      if (!cursor.At(MixinTokenKind.At)) {
        var literal = cursor.Consume();
        result.Add(new ValueAst(
          literal.Kind == MixinTokenKind.Escape ? "@" : literal.Text, null,
          sourceRange: literal.SourceRange
        ));
        continue;
      }
      var referenceStart = cursor.Position;
      if (!TryParseReference(cursor, out var reference, out _)) {
        cursor.Position = referenceStart + 1;
        while (!cursor.AtEnd && !cursor.At(MixinTokenKind.At)) cursor.Consume();
        var end = !cursor.AtEnd
          ? cursor.Current.Start
          : source is null ? tokens[referenceStart].End : sourceOffset + source.Length;
        result.Add(
          new ValueAst(
            source is null
              ? string.Concat(cursor.Slice(referenceStart, cursor.Position).Select(token => token.Text))
              : source.Substring(tokens[referenceStart].Start - sourceOffset,
                end - tokens[referenceStart].Start),
            null, true, new MixinSourceRange(tokens[referenceStart].Start, end)
          )
        );
        continue;
      }
      result.Add(new ValueAst(null, reference, sourceRange: reference.SourceRange));
    }
    if (result.Count == 0) result.Add(new ValueAst("", null));
    return result.AsReadOnly();
  }

  private static IReadOnlyList<MixinToken> OffsetTokens(
    IReadOnlyList<MixinToken> tokens, int offset
  ) {
    if (offset == 0) return tokens;
    return tokens.Select(token => token.Offset(offset)).ToArray();
  }

  internal static bool ValidateBooleanExpressionSyntax(string text, out string error) {
    return ValidateBooleanExpressionSyntax(TokensForExpression(text), out error);
  }

  private static bool ValidateBooleanExpressionSyntax(
    IReadOnlyList<MixinToken> tokens, out string error
  ) {
    error = null;
    var cursor = new MixinTokenStream(tokens);
    var found = false;
    while (!cursor.AtEnd) {
      if (cursor.Current.Kind is MixinTokenKind.Whitespace or MixinTokenKind.Text &&
        string.IsNullOrWhiteSpace(cursor.Current.Text)) {
        cursor.Consume();
        continue;
      }
      found = true;
      if (!TryParseReference(cursor, out _, out error)) return false;
    }
    if (found) return true;
    error = "boolean expression is empty";
    return false;
  }

  internal static bool ValidateValueExpressionSyntax(string text, out string error) {
    return ValidateValueExpressionSyntax(TokensForExpression(text), out error);
  }

  private static bool ValidateValueExpressionSyntax(
    IReadOnlyList<MixinToken> tokens, out string error
  ) {
    error = null;
    var cursor = new MixinTokenStream(tokens);
    while (!cursor.AtEnd) {
      if (!cursor.At(MixinTokenKind.At)) {
        cursor.Consume();
        continue;
      }
      if (!TryParseReference(cursor, out _, out error)) return false;
    }
    return true;
  }

  internal static MixinExpressionValidationResult ValidateSyntax(string expression, bool functionsOnly) {
    if (expression is null) return ValidationFailure("the expression is null", 0);
    var program = new ProgramAst(expression);
    if (program.Diagnostics.Count != 0)
      return ValidationFailure(program.Diagnostics[0].Message, program.Diagnostics[0].Line);
    string activeFunction = null;
    var functionLine = 0;
    var functionScopeOpen = false;
    var functions = new HashSet<string>(StringComparer.Ordinal);
    for (var index = 0; index < program.Count; index++) {
      var parsed = program.Get(index);
      if (parsed is EmptyDirectiveAst) continue;
      if (activeFunction is null) {
        if (parsed is not FunctionAst) {
          if (functionsOnly)
            return ValidationFailure("mixin libraries may only contain function declarations", parsed.Line);
          continue;
        }
        var function = (FunctionAst)parsed;
        if (!functions.Add(function.Name))
          return ValidationFailure("duplicate function '" + function.Name + "'", parsed.Line);
        activeFunction = function.Name;
        functionLine = parsed.Line;
        functionScopeOpen = false;
        continue;
      }
      if (parsed is FunctionAst) return ValidationFailure("functions may not be nested", parsed.Line);
      if (parsed is ScopeAst) functionScopeOpen = true;
      if (parsed is not EndAst) continue;
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
    var parsed = ParseDirective(line ?? "", 0).Node;
    command = MixinSyntaxFacts.Command(parsed);
    if (command is null) {
      command = null;
      arguments = [];
      operand = null;
      return false;
    }
    arguments = MixinSyntaxFacts.Arguments(parsed);
    operand = MixinSyntaxFacts.Operand(parsed);
    return true;
  }
}
