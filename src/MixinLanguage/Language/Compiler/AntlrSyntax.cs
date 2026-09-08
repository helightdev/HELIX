using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Lexer = Mixins.Compiler.Generated.HixLexer;
using Parser = Mixins.Compiler.Generated.HixParser;

namespace Mixins.Compiler;

/// <summary>ANTLR recognition followed by semantic construction, with no second syntax recognizer.</summary>
public static class AntlrSyntax {
  public static CompilationUnitAst Parse(string source) {
    source ??= "";
    var diagnostics = new List<HixParseDiagnostic>();
    var errors = new ErrorListener(diagnostics);
    var lexer = new Lexer(new AntlrInputStream(source));
    lexer.RemoveErrorListeners();
    lexer.AddErrorListener(errors);
    var stream = new CommonTokenStream(lexer);
    var parser = new Parser(stream);
    parser.RemoveErrorListeners();
    parser.AddErrorListener(errors);
    var tree = parser.compilationUnit();
    stream.Fill();
    var tokens = CompleteTokens(source, stream.GetTokens());
    foreach (var token in stream.GetTokens())
      if (token.Type == Lexer.ERROR_TOKEN)
        diagnostics.Add(new HixParseDiagnostic(token.Line, "invalid token '" + token.Text + "'"));
    // Error recovery trees are useful to ANTLR, but must never produce executable partial programs.
    var builder = new Builder(tokens, diagnostics);
    var declarations = diagnostics.Count == 0
      ? tree.children.OfType<ParserRuleContext>().Where(child => child is not Parser.TriviaContext)
        .Select(builder.Visit).ToArray()
      : Array.Empty<LanguageAst>();
    LanguageValidation.Validate(declarations, diagnostics);
    return new CompilationUnitAst(source, declarations, diagnostics, tokens);
  }

  private static HixToken[] CompleteTokens(string source, IList<IToken> recognized) {
    var result = new List<HixToken>();
    var cursor = 0;
    var line = 1;
    var column = 0;
    void Advance(int end) {
      while (cursor < end) {
        var character = source[cursor++];
        if (character == '\r' || character == '\n' && (cursor < 2 || source[cursor - 2] != '\r')) {
          line++;
          column = 0;
        } else if (character != '\n') column++;
      }
    }
    void Gap(int end) {
      if (end <= cursor) return;
      var text = source.Substring(cursor, end - cursor);
      var range = new HixSourceRange(cursor, end, line, column);
      var kind = text.All(char.IsWhiteSpace) ? HixTokenKind.Whitespace
        : text == "\\" ? HixTokenKind.Escape : HixTokenKind.Invalid;
      result.Add(new HixToken(kind, text, line, range, range));
      Advance(end);
    }
    foreach (var token in recognized) {
      if (token.Type == TokenConstants.EOF) continue;
      Gap(token.StartIndex);
      result.Add(Token(token));
      Advance(token.StopIndex + 1);
    }
    Gap(source.Length);
    return result.ToArray();
  }

  private static HixToken Token(IToken token) {
    var kind = token.Type switch {
      Lexer.COMMENT or Lexer.SLASH_COMMENT => HixTokenKind.Comment,
      Lexer.NEWLINE or Lexer.TERMINATOR => HixTokenKind.NewLine,
      Lexer.CONTENT_LINEBREAK => HixTokenKind.NewLineContinuation,
      Lexer.CONTENT_WRAP or Lexer.VALUE_WRAP => HixTokenKind.DirectContinuation,
      Lexer.ERROR_TOKEN => HixTokenKind.Invalid,
      Lexer.ARGUMENT_TEXT or Lexer.CONTENT_TEXT => HixTokenKind.Text,
      Lexer.NUMBER => HixTokenKind.Number,
      Lexer.VALUE_MEMBER => HixTokenKind.Hash,
      Lexer.VALUE_FUNCTION => HixTokenKind.FunctionOperator,
      Lexer.VALUE_PREDICATE => HixTokenKind.BooleanCallOperator,
      Lexer.BEGIN_ARGUMENT => HixTokenKind.OpenArgument,
      Lexer.ARGUMENT_END => HixTokenKind.CloseArgument,
      Lexer.BEGIN_PARAMETERS => HixTokenKind.OpenParenthesis,
      Lexer.END_PARAMETERS => HixTokenKind.CloseParenthesis,
      Lexer.ESCAPE or Lexer.ESCAPE_HEX or Lexer.ESCAPE_LITERAL or Lexer.ESCAPE_MACRO => HixTokenKind.Escape,
      _ => HixTokenKind.Identifier
    };
    var range = new HixSourceRange(token.StartIndex, token.StopIndex + 1, token.Line, token.Column);
    return new HixToken(kind, token.Text, token.Line, range, range);
  }

  private sealed class ErrorListener(List<HixParseDiagnostic> diagnostics)
    : BaseErrorListener, IAntlrErrorListener<int> {
    public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol,
      int line, int charPositionInLine, string msg, RecognitionException e) =>
      diagnostics.Add(new HixParseDiagnostic(line, $"column {charPositionInLine}: {msg}"));
    public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol,
      int line, int charPositionInLine, string msg, RecognitionException e) =>
      diagnostics.Add(new HixParseDiagnostic(line, $"column {charPositionInLine}: {msg}"));
  }

  private sealed class Builder(IReadOnlyList<HixToken> tokens, List<HixParseDiagnostic> diagnostics)
    : Generated.HixParserBaseVisitor<LanguageAst> {
    private HixToken[] TokensIn(HixSourceRange range) {
      var low = 0;
      var high = tokens.Count;
      while (low < high) {
        var middle = low + (high - low) / 2;
        if (tokens[middle].Start < range.Start) low = middle + 1;
        else high = middle;
      }
      var end = low;
      while (end < tokens.Count && tokens[end].End <= range.End) end++;
      var result = new HixToken[end - low];
      for (var index = 0; index < result.Length; index++) result[index] = tokens[low + index];
      return result;
    }

    private T At<T>(T node, ParserRuleContext context) where T : LanguageAst {
      node.SourceRange = new HixSourceRange(context.Start.StartIndex, context.Stop.StopIndex + 1,
        context.Start.Line, context.Start.Column);
      node.Tokens = TokensIn(node.SourceRange);
      return node;
    }

    private void Modifiers(IEnumerable<ParserRuleContext> modifiers) {
      foreach (var duplicate in modifiers.GroupBy(modifier => modifier.GetText()).Where(group => group.Count() > 1))
        diagnostics.Add(new HixParseDiagnostic(duplicate.First().Start.Line, "duplicate modifier '" + duplicate.Key + "'"));
    }

    private ExpressionAst Value(IParseTree context) => (ExpressionAst)Visit(context);
    private StatementAst Statement(IParseTree context) => (StatementAst)Visit(context);
    private ExpressionAst[] Arguments(Parser.ValueListContext context) => context is null ? [] :
      context.children.OfType<ParserRuleContext>().Select(Value).ToArray();
    private BlockStatementAst Block(Parser.StatementBlockContext context, string label = null) =>
      At(new BlockStatementAst(context.statement().Select(Statement).ToArray(), label), context);

    public override LanguageAst VisitMixinDeclaration(Parser.MixinDeclarationContext context) {
      Modifiers(context.mixinModifier());
      return At(new MixinDeclarationAst(context.mixinIdentifier().GetText(), context.mixinModifier().Length != 0,
        context.mixinBody().children.OfType<ParserRuleContext>()
          .Where(child => child is not Parser.TriviaContext).Select(Visit).ToArray()), context);
    }

    public override LanguageAst VisitExpressionDeclaration(Parser.ExpressionDeclarationContext context) {
      Modifiers(context.expressionModifier());
      return At(new ExpressionDeclarationAst(context.expressionModifier().Any(modifier => modifier.KEYWORD_PRELUDE() != null),
        context.expressionModifier().Any(modifier => modifier.KEYWORD_STRICT() != null), Block(context.statementBlock())),
      context);
    }

    public override LanguageAst VisitFuncDeclaration(Parser.FuncDeclarationContext context) {
      Modifiers(context.funcModifier());
      SignatureField[] Fields(Parser.SignatureContext signature) => signature.tableSignature()?.tableSignatureEntry()
        .Select(field => new SignatureField(field.ROOT_IDENTIFIER(0).GetText(), field.ROOT_IDENTIFIER(1).GetText(),
          field.VALUE_EXPAND() != null)).ToArray();
      var signatures = context.functionMetadata().functionSignatureVariant().Select(signature =>
        new FunctionSignature(signature.signature(0).IDENTIFIER()?.GetText(), Fields(signature.signature(0)),
          signature.signature(1).IDENTIFIER()?.GetText(), Fields(signature.signature(1)))).ToArray();
      return At(new FunctionDeclarationAst(context.IDENTIFIER().GetText(),
        context.funcModifier().Any(modifier => modifier.KEYWORD_PURE() != null),
        context.funcModifier().Any(modifier => modifier.KEYWORD_INLINE() != null),
        context.funcModifier().Any(modifier => modifier.KEYWORD_NOINLINE() != null), signatures,
        Block(context.statementBlock())), context);
    }

    public override LanguageAst VisitStatementBlock(Parser.StatementBlockContext context) => Block(context);
    public override LanguageAst VisitStatement(Parser.StatementContext context) {
      if (context.statementBlock() is { } block)
        return Block(block, context.labelIdentifier()?.LABEL_IDENTIFIER().GetText());
      if (context.labelIdentifier() is { } label)
        return At(new ControlFlowStatementAst(ControlFlowKind.Label, label.LABEL_IDENTIFIER().GetText(), []), context);
      var child = context.children.OfType<ParserRuleContext>().Single();
      var node = Visit(child);
      return node is SelectionExpressionAst selection ? At(new SelectionStatementAst(selection), context) : node;
    }

    public override LanguageAst VisitAssignmentStatement(Parser.AssignmentStatementContext context) {
      var specifier = context.variableSpecifiers();
      var storage = specifier.KEYWORD_LOCAL() != null ? StorageSpace.Local :
        specifier.KEYWORD_CARRY() != null ? StorageSpace.Carry :
        specifier.KEYWORD_TARGET() != null ? StorageSpace.Target : StorageSpace.Variable;
      return At(new AssignmentStatementAst(storage, context.variableIdentifier().GetText(),
        Value(context.assignedValue())), context);
    }

    public override LanguageAst VisitAssignedValue(Parser.AssignedValueContext context) {
      if (context.invocationStatement() is { } invocation) return Invocation(invocation);
      return Visit(context.children.OfType<ParserRuleContext>().Single());
    }

    private CallExpressionAst Invocation(Parser.InvocationStatementContext context) => At(
      new CallExpressionAst(context.IDENTIFIER().GetText(), Arguments(context.valueList())
        .Concat(context.tailValue() is { } tail ? [Value(tail)] : []).ToArray()), context);
    public override LanguageAst VisitInvocationStatement(Parser.InvocationStatementContext context) =>
      At(new InvocationStatementAst(Invocation(context)), context);

    public override LanguageAst VisitControlflowStatement(Parser.ControlflowStatementContext context) => At(
      new ControlFlowStatementAst(context.KEYWORD_RETURN() != null ? ControlFlowKind.Return :
        context.KEYWORD_GOTO() != null ? ControlFlowKind.Goto :
        context.KEYWORD_BREAK() != null ? ControlFlowKind.Break : ControlFlowKind.Continue,
        context.IDENTIFIER()?.GetText(), Arguments(context.valueList())), context);

    public override LanguageAst VisitValue(Parser.ValueContext context) =>
      Visit(context.children.OfType<ParserRuleContext>().Single());
    public override LanguageAst VisitPrimaryValue(Parser.PrimaryValueContext context) =>
      context.NUMBER() is { } number
        ? At(new NumberExpressionAst(double.Parse(number.GetText(),
          NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
          CultureInfo.InvariantCulture)), context)
        : Visit(context.children.OfType<ParserRuleContext>().Single());
    public override LanguageAst VisitTailValue(Parser.TailValueContext context) =>
      Visit(context.children.OfType<ParserRuleContext>().Single());
    public override LanguageAst VisitInlineValue(Parser.InlineValueContext context) => Visit(context.value());
    public override LanguageAst VisitValueExpression(Parser.ValueExpressionContext context) => Visit(context.value());

    public override LanguageAst VisitNonArgumentValue(Parser.NonArgumentValueContext context) {
      if (context.prefixOperators() != null)
        return At(new UnaryExpressionAst(UnaryOperation.Not, Value(context.nonArgumentValue())), context);
      if (context.postfixOperators() != null)
        return At(new UnaryExpressionAst(UnaryOperation.Check, Value(context.nonArgumentValue())), context);
      var value = Value(context.primaryValue());
      return context.elvisValue() is { } fallback
        ? At(new FallbackExpressionAst(value, Value(fallback.value())), context) : value;
    }

    public override LanguageAst VisitValueStatement(Parser.ValueStatementContext context) => At(
      new CallExpressionAst(context.functionIdentifier().GetText(), Arguments(context.valueList())), context);

    public override LanguageAst VisitTupleValue(Parser.TupleValueContext context) => At(
      new TupleExpressionAst(context.value().Select(Value).ToArray()), context);
    public override LanguageAst VisitTableValue(Parser.TableValueContext context) => At(
      new TableExpressionAst(context.tableKeyedEntry().Select(entry =>
        new KeyValuePair<string, ExpressionAst>(entry.ROOT_IDENTIFIER().GetText(), Value(entry.value()))).ToArray()),
      context);

    public override LanguageAst VisitDerivationRoot(Parser.DerivationRootContext context) => At(
      new RootExpressionAst(context.ROOT_IDENTIFIER().GetText(), context.VALUE_SMART_ROOT() != null), context);
    public override LanguageAst VisitDerivation(Parser.DerivationContext context) =>
      Transform(Value(context.derivationRoot()), context.transformationPart());
    private ExpressionAst Transform(ExpressionAst value, Parser.TransformationPartContext[] parts) {
      foreach (var part in parts) {
        var start = value.SourceRange;
        if (part.memberIdentifier() is { } member)
          value = At(new MemberExpressionAst(value, member.MEMBER_IDENTIFIER().GetText()), part);
        else if (part.functionIdentifier() is { } function)
          value = At(new CallExpressionAst(function.GetText(), new[] {value}.Concat(Arguments(part.valueList())).ToArray(),
            part.functionChainType().VALUE_PREDICATE() != null), part);
        if (part.VALUE_WRAP() == null && !start.IsEmpty) {
          value.SourceRange = value.SourceRange with {Start = start.Start, Line = start.Line, Column = start.Column};
          value.Tokens = TokensIn(value.SourceRange);
        }
      }
      return value;
    }

    private static ExpressionAst Interpolate(IEnumerable<ExpressionAst> source) {
      var parts = new List<ExpressionAst>();
      foreach (var part in source) {
        if (part is StringExpressionAst text && parts.LastOrDefault() is StringExpressionAst previous)
          parts[parts.Count - 1] = new StringExpressionAst(previous.Value + text.Value);
        else parts.Add(part);
      }
      return parts.Count == 0 ? new StringExpressionAst("") : parts.Count == 1 && parts[0] is StringExpressionAst
        ? parts[0] : new InterpolationExpressionAst(parts.ToArray());
    }

    public override LanguageAst VisitArgumentValue(Parser.ArgumentValueContext context) => At(
      Interpolate(context.argumentBody().children?.Select(child => child is ITerminalNode text
        ? new StringExpressionAst(text.GetText()) : Value(child)) ?? []), context);
    public override LanguageAst VisitEscaped(Parser.EscapedContext context) {
      var text = context.GetText().Substring(1);
      var decoded = context.ESCAPE_HEX() != null
        ? ((char)int.Parse(text.Substring(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture)).ToString()
        : text switch {"a" => "\a", "b" => "\b", "f" => "\f", "n" => "\n", "r" => "\r", "t" => "\t", "v" => "\v", _ => text};
      return At(new StringExpressionAst(decoded), context);
    }

    public override LanguageAst VisitContentBlock(Parser.ContentBlockContext context) => At(
      Interpolate(context.contentBody().children.Select(child => child is ITerminalNode terminal
        ? new StringExpressionAst(terminal.Symbol.Type switch {
          Lexer.CONTENT_WRAP => "", Lexer.CONTENT_LINEBREAK => "\n", _ => terminal.GetText()
        }) : Value(child))), context);
    public override LanguageAst VisitContentInterpolate(Parser.ContentInterpolateContext context) =>
      Visit(context.derivation());

    public override LanguageAst VisitWhenResult(Parser.WhenResultContext context) {
      var result = Visit(context.children.OfType<ParserRuleContext>().Single());
      return result is InvocationStatementAst invocation ? invocation.Call : result;
    }
    public override LanguageAst VisitWhenConditionStatement(Parser.WhenConditionStatementContext context) => At(
      new SelectionExpressionAst(null,
        [At(new SelectionBranchAst(Arguments(context.whenChainCondition().valueList()), Block(context.statementBlock())), context)],
        context.whenResult() is { } fallback ? Visit(fallback) : null), context);
    public override LanguageAst VisitWhenChainStatement(Parser.WhenChainStatementContext context) => At(
      new SelectionExpressionAst(null, context.whenChainBody().whenChainBranch().Select(branch => At(
        new SelectionBranchAst(Arguments(branch.whenChainCondition().valueList()), Visit(branch.whenResult())), branch)).ToArray(),
        context.whenChainBody().whenElseBranch() is { } fallback ? Visit(fallback.whenResult()) : null), context);
    public override LanguageAst VisitWhenValueStatement(Parser.WhenValueStatementContext context) {
      var branches = context.whenValueBody().whenValueBranch().Select(branch => {
        var condition = branch.whenValueCondition();
        var transformation = condition.inlineTransformation();
        var value = transformation != null
          ? Transform(new RootExpressionAst("\0selector"), transformation.transformationPart()) : Value(condition.value());
        return At(new SelectionBranchAst([value], Visit(branch.whenResult()), transformation != null), branch);
      }).ToArray();
      return At(new SelectionExpressionAst(context.value() is { } selector ? Value(selector) : null, branches,
        context.whenValueBody().whenElseBranch() is { } fallback ? Visit(fallback.whenResult()) : null), context);
    }
  }
}
