using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Lexer = Hix.Compiler.Generated.HixLexer;
using Parser = Hix.Compiler.Generated.HixParser;

namespace Hix.Compiler;

/// <summary>ANTLR recognition followed by semantic construction, with no second syntax recognizer.</summary>
public static class AntlrSyntax {
  public static CompilationUnitIr Parse(string source, HixBackend backend = null,
    bool recoverValidDeclarations = false) {
    source ??= "";
    var parseSource = recoverValidDeclarations ? RecoverIncompleteHeader(source) : source;
    var diagnostics = new List<HixParseDiagnostic>();
    var errors = new ErrorListener(diagnostics);
    var lexer = new Lexer(new AntlrInputStream(parseSource));
    lexer.RemoveErrorListeners();
    lexer.AddErrorListener(errors);
    var stream = new CommonTokenStream(lexer);
    var parser = new Parser(stream);
    parser.RemoveErrorListeners();
    parser.AddErrorListener(errors);
    var tree = parser.compilationUnit();
    stream.Fill();
    var tokens = stream.GetTokens().Where(token => token.Type != TokenConstants.EOF).ToArray();
    foreach (var token in stream.GetTokens())
      if (token.Type == Lexer.ERROR_TOKEN)
        diagnostics.Add(new HixParseDiagnostic(token.Line, "invalid token '" + token.Text + "'"));
    // Error recovery trees must never produce executable partial programs during compilation.
    // Editor analysis may, however, retain complete declarations surrounding an incomplete edit.
    var builder = new IrBuilder(tokens, diagnostics);
    var declarationContexts = tree.topLevelDeclaration()
      .Where(context => !recoverValidDeclarations || !HasSyntaxError(context)).ToArray();
    var declarations = diagnostics.Count == 0 || recoverValidDeclarations
      ? declarationContexts.Select(builder.Visit).ToArray()
      : Array.Empty<HixIrNode>();
    var metadata = (diagnostics.Count == 0 || recoverValidDeclarations) &&
      tree.fileMetadataSection() is { } section && !HasSyntaxError(section)
      ? section.metadataList().metadata().Select(value => (MetadataIr)builder.Visit(value)).ToArray()
      : Array.Empty<MetadataIr>();
    var patterns = declarations.OfType<TypeDeclarationIr>().GroupBy(type => type.Name, StringComparer.Ordinal)
      .ToDictionary(group => group.Key, group => group.First().Pattern, StringComparer.Ordinal);
    LanguageValidation.Validate(declarations, diagnostics, backend);
    if (diagnostics.Count == 0) PatternTypeAnalysis.Validate(declarations, patterns, diagnostics, backend);
    return new CompilationUnitIr(source, declarations, diagnostics, tokens, metadata);
  }

  private static string RecoverIncompleteHeader(string source) {
    var delimiter = source.IndexOf("---", StringComparison.Ordinal);
    if (delimiter < 0) return source;
    var header = source.Substring(0, delimiter);
    var hasIncompleteMetadata = header.Split('\n').Any(line => line.Trim('\r', ' ', '\t') == "%");
    if (!hasIncompleteMetadata) return source;

    // Keep offsets stable while preventing an unfinished header item from making ANTLR consume
    // the section delimiter and every following declaration as part of one recovery context.
    var recovered = source.ToCharArray();
    for (var index = 0; index < delimiter + 3; index++)
      if (recovered[index] is not '\r' and not '\n') recovered[index] = ' ';
    return new string(recovered);
  }

  private static bool HasSyntaxError(IParseTree node) {
    if (node is IErrorNode) return true;
    if (node is ITerminalNode terminal && terminal.Symbol.TokenIndex < 0) return true;
    if (node is Parser.MetadataContext metadata &&
      metadata.metadataValue() is null && metadata.IDENTIFIER() is null) return true;
    for (var index = 0; index < node.ChildCount; index++)
      if (HasSyntaxError(node.GetChild(index))) return true;
    return false;
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

  private sealed class IrBuilder(IReadOnlyList<IToken> tokens, List<HixParseDiagnostic> diagnostics)
    : Generated.HixParserBaseVisitor<HixIrNode> {
    private IReadOnlyList<MetadataIr> declarationMetadata = [];
    private IToken[] TokensIn(HixSourceRange range) {
      var low = 0;
      var high = tokens.Count;
      while (low < high) {
        var middle = low + (high - low) / 2;
        if (tokens[middle].StartIndex < range.Start) low = middle + 1;
        else high = middle;
      }
      var end = low;
      while (end < tokens.Count && tokens[end].StopIndex + 1 <= range.End) end++;
      var result = new IToken[end - low];
      for (var index = 0; index < result.Length; index++) result[index] = tokens[low + index];
      return result;
    }

    private T At<T>(T node, ParserRuleContext context) where T : HixIrNode {
      node.SourceRange = new HixSourceRange(context.Start.StartIndex, context.Stop.StopIndex + 1,
        context.Start.Line, context.Start.Column);
      node.Tokens = TokensIn(node.SourceRange);
      return node;
    }

    private void Modifiers(IEnumerable<ParserRuleContext> modifiers) {
      foreach (var duplicate in modifiers.GroupBy(modifier => modifier.GetText()).Where(group => group.Count() > 1))
        diagnostics.Add(new HixParseDiagnostic(duplicate.First().Start.Line, "duplicate modifier '" + duplicate.Key + "'"));
    }

    private ExpressionIr Value(IParseTree context) => (ExpressionIr)Visit(context);
    private StatementIr Statement(IParseTree context) => (StatementIr)Visit(context);
    private ExpressionIr[] Arguments(Parser.ValueListContext context) => context is null ? [] :
      context.children.OfType<ParserRuleContext>().Select(Value).ToArray();
    private BlockStatementIr Block(Parser.StatementBlockContext context, string label = null) =>
      At(new BlockStatementIr(context.statement().Select(Statement).ToArray(), label), context);
    private BlockStatementIr FunctionBody(Parser.FunctionBodyContext context) {
      if (context.statementBlock() is { } block) return Block(block);
      var returned = At(new ControlFlowStatementIr(ControlFlowKind.Return, null, [Value(context.value())]), context);
      return At(new BlockStatementIr([returned]), context);
    }

    private string DeclarationName(ITerminalNode identifier, Parser.ArgumentValueContext argument) {
      if (identifier != null) return identifier.GetText();
      if (Value(argument) is StringExpressionIr literal) return literal.Value;
      diagnostics.Add(new HixParseDiagnostic(argument.Start.Line, "declaration name must be a literal string"));
      return argument.GetText();
    }

    public override HixIrNode VisitTopLevelDeclaration(Parser.TopLevelDeclarationContext context) {
      var previous = declarationMetadata;
      declarationMetadata = context.metadataList()?.metadata()
        .Select(metadata => (MetadataIr)Visit(metadata)).ToArray() ?? [];
      try {
        return At(Visit(context.mixinDeclaration() ?? (ParserRuleContext)context.funcDeclaration() ??
          context.typeDeclaration()), context);
      } finally {
        declarationMetadata = previous;
      }
    }

    public override HixIrNode VisitMetadata(Parser.MetadataContext context) {
      if (context.metadataValue() is { } anonymous) return Visit(anonymous);
      return At(new MetadataIr(context.IDENTIFIER().GetText(), Arguments(context.valueList())), context);
    }

    public override HixIrNode VisitMetadataValue(Parser.MetadataValueContext context) =>
      At(new MetadataIr(null, [Value(context.value())]), context);

    private HixPattern Pattern(Parser.PatternExpressionContext context) {
      var pattern = context.patternPrimary() == null ? HixPattern.Any : Pattern(context.patternPrimary());
      if (context.metadataList() == null) return pattern;
      return ApplyPatternMetadata(pattern, context.metadataList().metadata().Select(item => (MetadataIr)Visit(item)), out _);
    }

    private HixPattern Pattern(Parser.PatternPrimaryContext context) {
      if (context.patternIdentifier() != null) return HixPatterns.Named(context.patternIdentifier().GetText());
      if (context.tablePattern() != null)
        return new TableHixPattern(context.tablePattern().patternField().Select(PatternField).ToArray());
      if (context.tuplePattern() != null)
        return new TupleHixPattern(context.tuplePattern().patternField().Select(PatternField).ToArray());
      var delegatePattern = context.delegatePattern();
      return new DelegateHixPattern(delegatePattern.patternParameterList().patternField().Select(PatternField).ToArray(),
        Pattern(delegatePattern.patternExpression()));
    }

    private HixPatternField PatternField(Parser.PatternFieldContext context) {
      var explicitName = context.ROOT_IDENTIFIER()?.GetText();
      var pattern = explicitName == null ? HixPattern.Any : Pattern(context.patternPrimary());
      var name = explicitName ?? context.patternPrimary().GetText();
      var metadata = context.metadataList()?.metadata().Select(item => (MetadataIr)Visit(item)) ?? [];
      pattern = ApplyPatternMetadata(pattern, metadata, out var optional);
      return new HixPatternField(name, pattern, optional);
    }

    private HixPattern ApplyPatternMetadata(HixPattern pattern, IEnumerable<MetadataIr> values, out bool optional) {
      optional = false;
      foreach (var metadata in values) {
        object Argument(int index) => index >= metadata.Values.Count ? null : metadata.Values[index] switch {
          StringExpressionIr text => text.Value, NumberExpressionIr number => number.Value,
          BooleanExpressionIr boolean => boolean.Value, NullExpressionIr => null, _ => null
        };
        HixPattern PatternArgument(int index) => Argument(index) is string name ? HixPatterns.Named(name) : HixPattern.Any;
        if (!HixPatternMetadata.TryGet(metadata.Name, out _)) {
          diagnostics.Add(new HixParseDiagnostic(metadata.Line, "unknown pattern directive '%" + metadata.Name + "'"));
          continue;
        }
        switch (metadata.Name) {
          case "optional": optional = true; break;
          case "many": pattern = new ManyHixPattern(metadata.Values.Count == 0 ? pattern : PatternArgument(0)); break;
          case "map":
            pattern = new MapHixPattern(PatternArgument(0), PatternArgument(1)); break;
          case "union":
            pattern = HixPatterns.Union(Enumerable.Range(0, metadata.Values.Count).Select(PatternArgument)); break;
          case "const": pattern = new ConstantHixPattern(Argument(0), pattern); break;
          case "min": pattern = new ConstrainedHixPattern(pattern, HixPatternConstraintKind.Minimum, Argument(0)); break;
          case "max": pattern = new ConstrainedHixPattern(pattern, HixPatternConstraintKind.Maximum, Argument(0)); break;
          case "length": pattern = new ConstrainedHixPattern(pattern, HixPatternConstraintKind.Length, Argument(0)); break;
          case "matches": pattern = new ConstrainedHixPattern(pattern, HixPatternConstraintKind.Matches, Argument(0)); break;
        }
      }
      return pattern;
    }

    public override HixIrNode VisitTypeDeclaration(Parser.TypeDeclarationContext context) =>
      At(new TypeDeclarationIr(context.IDENTIFIER().GetText(), Pattern(context.patternExpression()), declarationMetadata), context);

    public override HixIrNode VisitMixinDeclaration(Parser.MixinDeclarationContext context) {
      Modifiers(context.mixinModifier());
      var identifier = context.mixinIdentifier();
      return At(new MixinDeclarationIr(DeclarationName(identifier.IDENTIFIER() ?? identifier.NAMESPACE_IDENTIFIER(),
          identifier.argumentValue()), context.mixinModifier().Length != 0,
        context.mixinBody().children.OfType<ParserRuleContext>()
          .Where(child => child is not Parser.TriviaContext).Select(Visit).ToArray(), declarationMetadata), context);
    }

    public override HixIrNode VisitExpressionDeclaration(Parser.ExpressionDeclarationContext context) {
      Modifiers(context.expressionModifier());
      return At(new ExpressionDeclarationIr(context.expressionModifier().Any(modifier => modifier.KEYWORD_PRELUDE() != null),
        context.expressionModifier().Any(modifier => modifier.KEYWORD_STRICT() != null), Block(context.statementBlock())),
      context);
    }

    public override HixIrNode VisitFuncDeclaration(Parser.FuncDeclarationContext context) {
      Modifiers(context.funcModifier());
      SignatureField[] Fields(Parser.SignatureContext signature) => signature.tableSignature()?.tableSignatureEntry()
        .Select(field => new SignatureField(field.ROOT_IDENTIFIER().GetText(), field.kindIdentifier().GetText(),
          field.VALUE_EXPAND() != null, field.metadata().Select(metadata => (MetadataIr)Visit(metadata)).ToArray())).ToArray();
      var signatures = context.functionMetadata().functionSignatureVariant().Select(signature =>
        new FunctionSignature(signature.signature(0).kindIdentifier()?.GetText(), Fields(signature.signature(0)),
          signature.signature(1).kindIdentifier()?.GetText(), Fields(signature.signature(1)))).ToList();
      if (context.directFunctionSignature() is { } direct) {
        var parameters = direct.patternParameterList().patternField().Select(PatternField)
          .Select(field => new SignatureField(field.Name, field.Pattern, optional: field.Optional)).ToArray();
        signatures.Insert(0, new FunctionSignature(null, parameters, Pattern(direct.patternExpression()), null));
      }
      var identifier = context.functionDeclarationIdentifier();
      return At(new FunctionDeclarationIr(DeclarationName(identifier.IDENTIFIER(), identifier.argumentValue()),
        context.funcModifier().Any(modifier => modifier.KEYWORD_PURE() != null),
        context.funcModifier().Any(modifier => modifier.KEYWORD_INLINE() != null),
        context.funcModifier().Any(modifier => modifier.KEYWORD_NOINLINE() != null), signatures.ToArray(),
        FunctionBody(context.functionBody()), declarationMetadata), context);
    }

    public override HixIrNode VisitLambdaValue(Parser.LambdaValueContext context) {
      BlockStatementIr body;
      if (context.BEGIN_LAMBDA_BLOCK() != null)
        body = At(new BlockStatementIr(context.statement().Select(Statement).ToArray()), context);
      else {
        var returned = At(new ControlFlowStatementIr(ControlFlowKind.Return, null, [Value(context.value())]), context);
        body = At(new BlockStatementIr([returned]), context);
      }
      return At(new LambdaExpressionIr(body), context);
    }

    public override HixIrNode VisitStatementBlock(Parser.StatementBlockContext context) => Block(context);
    public override HixIrNode VisitStatement(Parser.StatementContext context) {
      if (context.statementBlock() is { } block)
        return Block(block, context.labelIdentifier()?.LABEL_IDENTIFIER().GetText());
      if (context.labelIdentifier() is { } label)
        return At(new ControlFlowStatementIr(ControlFlowKind.Label, label.LABEL_IDENTIFIER().GetText(), []), context);
      var child = context.children.OfType<ParserRuleContext>().Single();
      var node = Visit(child);
      return node is SelectionExpressionIr selection ? At(new SelectionStatementIr(selection), context) : node;
    }

    public override HixIrNode VisitAssignmentStatement(Parser.AssignmentStatementContext context) {
      var specifier = context.variableSpecifiers();
      var storage = specifier.KEYWORD_LOCAL() != null ? StorageSpace.Local :
        specifier.KEYWORD_TARGET() != null ? StorageSpace.Target : StorageSpace.Variable;
      return At(new AssignmentStatementIr(storage, context.variableIdentifier().GetText(),
        Value(context.assignedValue()), specifier.KEYWORD_CARRY() != null), context);
    }

    public override HixIrNode VisitAssignedValue(Parser.AssignedValueContext context) {
      if (context.invocationStatement() is { } invocation) return Invocation(invocation);
      return Visit(context.children.OfType<ParserRuleContext>().Single());
    }

    private CallExpressionIr Invocation(Parser.InvocationStatementContext context) => At(
      new CallExpressionIr(context.IDENTIFIER().GetText(), Arguments(context.valueList())
        .Concat(context.tailValue() is { } tail ? [Value(tail)] : []).ToArray()), context);
    public override HixIrNode VisitInvocationStatement(Parser.InvocationStatementContext context) =>
      At(new InvocationStatementIr(Invocation(context)), context);

    public override HixIrNode VisitControlflowStatement(Parser.ControlflowStatementContext context) => At(
      new ControlFlowStatementIr(context.KEYWORD_RETURN() != null ? ControlFlowKind.Return :
        context.KEYWORD_GOTO() != null ? ControlFlowKind.Goto :
        context.KEYWORD_BREAK() != null ? ControlFlowKind.Break : ControlFlowKind.Continue,
        context.IDENTIFIER()?.GetText(), Arguments(context.valueList())), context);

    public override HixIrNode VisitValue(Parser.ValueContext context) =>
      Visit(context.children.OfType<ParserRuleContext>().Single());
    public override HixIrNode VisitPrimaryValue(Parser.PrimaryValueContext context) =>
      context.NUMBER() is { } number
        ? At(new NumberExpressionIr(double.Parse(number.GetText(),
          NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
          CultureInfo.InvariantCulture)), context)
        : context.BOOLEAN() is { } boolean
          ? At(new BooleanExpressionIr(boolean.GetText() == "true"), context)
        : context.NULL() is not null
          ? At(new NullExpressionIr(), context)
        : Visit(context.children.OfType<ParserRuleContext>().Single());
    public override HixIrNode VisitTailValue(Parser.TailValueContext context) =>
      Visit(context.children.OfType<ParserRuleContext>().Single());
    public override HixIrNode VisitInlineValue(Parser.InlineValueContext context) => Visit(context.value());
    public override HixIrNode VisitValueExpression(Parser.ValueExpressionContext context) => Visit(context.value());

    public override HixIrNode VisitNonArgumentValue(Parser.NonArgumentValueContext context) {
      if (context.prefixOperators() != null)
        return At(new UnaryExpressionIr(UnaryOperation.Not, Value(context.nonArgumentValue())), context);
      if (context.postfixOperators() != null)
        return At(new UnaryExpressionIr(UnaryOperation.Check, Value(context.nonArgumentValue())), context);
      var value = Value(context.primaryValue());
      return context.elvisValue() is { } fallback
        ? At(new FallbackExpressionIr(value, Value(fallback.value())), context) : value;
    }

    public override HixIrNode VisitValueStatement(Parser.ValueStatementContext context) => At(
      new CallExpressionIr(context.functionIdentifier().GetText(), Arguments(context.valueList())), context);

    public override HixIrNode VisitTupleValue(Parser.TupleValueContext context) => At(
      new TupleExpressionIr(context.value().Select(Value).ToArray()), context);
    public override HixIrNode VisitTableValue(Parser.TableValueContext context) {
      var entries = context.tableKeyedEntry();
      return At(new TableExpressionIr(entries.Select(entry =>
          new KeyValuePair<string, ExpressionIr>(entry.ROOT_IDENTIFIER().GetText(), Value(entry.value()))).ToArray(),
        entries.Select(entry => new KeyValuePair<string, IReadOnlyList<MetadataIr>>(
          entry.ROOT_IDENTIFIER().GetText(), entry.metadata().Select(metadata =>
            (MetadataIr)Visit(metadata)).ToArray())).ToArray()), context);
    }

    public override HixIrNode VisitDerivationRoot(Parser.DerivationRootContext context) => At(
      new RootExpressionIr((context.ROOT_IDENTIFIER()?.GetText() ?? context.NUMBER().GetText()),
        context.VALUE_SMART_ROOT() != null), context);
    public override HixIrNode VisitDerivation(Parser.DerivationContext context) =>
      Transform(Value(context.derivationRoot()), context.transformationPart());
    private ExpressionIr Transform(ExpressionIr value, Parser.TransformationPartContext[] parts) {
      foreach (var part in parts) {
        var start = value.SourceRange;
        if (part.memberIdentifier() is { } member)
          value = At(new MemberExpressionIr(value, member.MEMBER_IDENTIFIER().GetText()), part);
        else if (part.functionIdentifier() is { } function)
          value = At(new CallExpressionIr(function.GetText(), new[] {value}.Concat(Arguments(part.valueList())).ToArray(),
            part.functionChainType().VALUE_PREDICATE() != null), part);
        if (part.VALUE_WRAP() == null && !start.IsEmpty) {
          value.SourceRange = value.SourceRange with {Start = start.Start, Line = start.Line, Column = start.Column};
          value.Tokens = TokensIn(value.SourceRange);
        }
      }
      return value;
    }

    private static ExpressionIr Interpolate(IEnumerable<ExpressionIr> source) {
      var parts = new List<ExpressionIr>();
      foreach (var part in source) {
        if (part is StringExpressionIr text && parts.LastOrDefault() is StringExpressionIr previous)
          parts[parts.Count - 1] = new StringExpressionIr(previous.Value + text.Value);
        else parts.Add(part);
      }
      return parts.Count == 0 ? new StringExpressionIr("") : parts.Count == 1 && parts[0] is StringExpressionIr
        ? parts[0] : new InterpolationExpressionIr(parts.ToArray());
    }

    public override HixIrNode VisitArgumentValue(Parser.ArgumentValueContext context) => At(
      Interpolate(context.argumentBody().children?.Select(child => child is ITerminalNode text
        ? new StringExpressionIr(text.GetText()) : Value(child)) ?? []), context);
    public override HixIrNode VisitEscaped(Parser.EscapedContext context) {
      var text = context.GetText().Substring(1);
      var decoded = context.ESCAPE_HEX() != null
        ? ((char)int.Parse(text.Substring(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture)).ToString()
        : text switch {"a" => "\a", "b" => "\b", "f" => "\f", "n" => "\n", "r" => "\r", "t" => "\t", "v" => "\v", _ => text};
      return At(new StringExpressionIr(decoded), context);
    }

    public override HixIrNode VisitContentBlock(Parser.ContentBlockContext context) => At(
      Interpolate(context.contentBody().children.Select(child => child is ITerminalNode terminal
        ? new StringExpressionIr(terminal.Symbol.Type switch {
          Lexer.CONTENT_WRAP => "", Lexer.CONTENT_LINEBREAK => "\n", _ => terminal.GetText()
        }) : Value(child))), context);
    public override HixIrNode VisitContentInterpolate(Parser.ContentInterpolateContext context) =>
      Visit(context.derivation());

    public override HixIrNode VisitWhenResult(Parser.WhenResultContext context) {
      var result = Visit(context.children.OfType<ParserRuleContext>().Single());
      return result is InvocationStatementIr invocation ? invocation.Call : result;
    }
    public override HixIrNode VisitWhenConditionStatement(Parser.WhenConditionStatementContext context) => At(
      new SelectionExpressionIr(null,
        [At(new SelectionBranchIr(Arguments(context.whenChainCondition().valueList()), Block(context.statementBlock())), context)],
        context.whenResult() is { } fallback ? Visit(fallback) : null), context);
    public override HixIrNode VisitWhenChainStatement(Parser.WhenChainStatementContext context) => At(
      new SelectionExpressionIr(null, context.whenChainBody().whenChainBranch().Select(branch => At(
        new SelectionBranchIr(Arguments(branch.whenChainCondition().valueList()), Visit(branch.whenResult())), branch)).ToArray(),
        context.whenChainBody().whenElseBranch() is { } fallback ? Visit(fallback.whenResult()) : null), context);
    public override HixIrNode VisitWhenValueStatement(Parser.WhenValueStatementContext context) {
      var branches = context.whenValueBody().whenValueBranch().Select(branch => {
        var condition = branch.whenValueCondition();
        var transformation = condition.inlineTransformation();
        var value = transformation != null
          ? Transform(At(new SelectorExpressionIr(), condition), transformation.transformationPart()) : Value(condition.value());
        return At(new SelectionBranchIr([value], Visit(branch.whenResult()), transformation != null), branch);
      }).ToArray();
      return At(new SelectionExpressionIr(context.value() is { } selector ? Value(selector) : null, branches,
        context.whenValueBody().whenElseBranch() is { } fallback ? Visit(fallback.whenResult()) : null), context);
    }
  }
}
