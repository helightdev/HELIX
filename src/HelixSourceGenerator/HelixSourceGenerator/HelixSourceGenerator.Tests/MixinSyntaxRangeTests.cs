using System.Linq;
using Mixins;
using Mixins.Compiler;
using Xunit;

namespace HELIX.SourceGen.Tests;

public sealed class MixinSyntaxRangeTests {
  [Fact]
  public void ProgramAstCarriesAbsoluteRangesAcrossContinuationLines() {
    const string source =
      "  @LOCAL<Name> @table:put<first><(@param#value)>\n" +
      "    @+:put<second><(@local#Name)>\n";

    var instruction = Assert.IsAssignableFrom<ValueStatementAst>(
      MixinParser.Parse(source).Instructions[0]
    );
    var reference = instruction.Expression.Single().Reference;

    Assert.Equal("@", Slice(source, instruction.MarkerRange));
    Assert.Equal("LOCAL", Slice(source, instruction.NameRange));
    Assert.Equal("<Name>", Slice(source, instruction.ArgumentRanges.Single()));
    Assert.Equal("Name", Slice(source, instruction.ArgumentContentRanges.Single()));
    Assert.Equal("table", Slice(source, reference.RootRange));
    Assert.Equal("put", Slice(source, reference.Properties[0].NameRange));
    Assert.Equal("put", Slice(source, reference.Properties[1].NameRange));
    Assert.True(instruction.SourceRange.End >= reference.SourceRange.End);
  }

  [Fact]
  public void StandaloneExpressionsCarryRootMemberFunctionAndArgumentRanges() {
    const string source = "@(local#Value:replace<Old><(@param#value)>)";
    var reference = MixinParser.ParseExpressionValues(source).Single().Reference;

    Assert.Equal("local", Slice(source, reference.RootRange));
    Assert.Equal("Value", Slice(source, reference.MemberRange));
    Assert.Equal("replace", Slice(source, reference.Properties.Single().NameRange));
    Assert.Equal("<Old>", Slice(source, reference.Properties.Single().ParsedArguments[0].SourceRange));
    Assert.Equal("<(@param#value)>", Slice(source, reference.Properties.Single().ParsedArguments[1].SourceRange));
  }

  [Fact]
  public void AstUsesParserRootRangesForNamedNullRoot() {
    const string source = "@RETURN @null:eq<null>";
    var root = Descendants(MixinParser.Parse(source)).Single(child =>
      child.Kind == MixinSyntaxKind.Root && Slice(source, child.SourceRange) == "null"
    );

    Assert.Equal("null", Slice(source, root.SourceRange));
  }

  [Fact]
  public void AstExposesNamedRootsThroughParenthesesAndFunctionChains() {
    const string source =
      "@LOCAL<Value> seed\n" +
      "@RETURN @(local#Value:replace<x><y>)\n";

    var program = MixinParser.Parse(source);
    var reference = Assert.Single(Descendants(program).Where(item =>
      item.Kind == MixinSyntaxKind.Member && Slice(source, item.SourceRange) == "Value"));

    Assert.Equal("Value", Slice(source, reference.SourceRange));
  }

  [Fact]
  public void ParserAttachesTheCanonicalTokensWithLineAndColumnLocations() {
    const string source = "  @RETURN @table:put<name><value>";
    var program = MixinParser.Parse(source);
    var operation = Assert.Single(program.Tokens.Where(token => token.Kind == MixinTokenKind.FunctionOperator));
    var identifier = program.Tokens.Single(token => token.Kind == MixinTokenKind.Identifier && token.Text == "put");

    Assert.Equal(1, operation.SourceRange.Line);
    Assert.Equal(source.IndexOf(':'), operation.SourceRange.Column);
    Assert.Equal(":", operation.Text);
    Assert.Equal("put", identifier.Text);
    Assert.Contains(program.Instructions[0].Tokens, token => ReferenceEquals(token, operation));
  }

  [Fact]
  public void StandaloneExpressionsUseTheCanonicalLexerWithoutSyntheticDirectiveContext() {
    const string source = "@local#Value:replace<Old><New>";
    var tokens = MixinLexer.Lex(source);
    var reference = MixinParser.ParseExpressionValues(source).Single().Reference;

    Assert.Equal(MixinTokenKind.At, tokens[0].Kind);
    Assert.Equal("local", tokens[1].Text);
    Assert.Equal(2, tokens.Count(token => token.Kind == MixinTokenKind.OpenArgument));
    Assert.Equal(2, tokens.Count(token => token.Kind == MixinTokenKind.CloseArgument));
    Assert.Equal("Value", reference.Member);
    Assert.Equal(0, reference.SourceRange.Start);
    Assert.Equal(source.Length, reference.SourceRange.End);
  }

  [Fact]
  public void DirectiveAndExpressionIdentifiersShareTheSameTokenKind() {
    var directive = MixinLexer.Lex("@RETURN @local#Value");

    Assert.Equal(MixinTokenKind.At, directive[0].Kind);
    Assert.Equal(2, directive.Count(token => token.Kind == MixinTokenKind.At));
  }

  [Fact]
  public void LexerKeepsWhitespaceInsideTextAndArguments() {
    var operand = MixinLexer.Lex("@RETURN hello world");
    var argument = MixinLexer.Lex("@MIXIN<hello world> body");

    Assert.Equal(" hello world", Assert.Single(operand.Where(token =>
      token.Kind == MixinTokenKind.Text)).Text);
    Assert.Contains(argument, token => token.Kind == MixinTokenKind.Text && token.Text == "hello world");
    Assert.DoesNotContain(argument, token => token.Kind == MixinTokenKind.Whitespace);
  }

  [Fact]
  public void RegisteredSyntaxIsBackedByItsDefinition() {
    Assert.True(DirectiveLibrary.TryGet("RETURN", out var definition));
    var syntax = Assert.IsType<ReturnAst>(
      MixinParser.Parse("@RETURN value").Instructions.Single()
    );

    Assert.Same(definition, syntax.Definition);
    Assert.Contains(definition, DirectiveLibrary.Enumerate());
  }

  [Theory]
  [InlineData("@LOCAL<Name> value", MixinSymbolKind.Local, MixinSymbolUsage.Declaration)]
  [InlineData("@GOTO<Done>", MixinSymbolKind.Label, MixinSymbolUsage.Reference)]
  [InlineData("@CALL<Render>", MixinSymbolKind.Function, MixinSymbolUsage.Reference)]
  public void DirectiveArgumentsCarryDefinitionMetadata(
    string source, MixinSymbolKind symbol, MixinSymbolUsage usage
  ) {
    var argument = Assert.IsType<DirectiveArgumentAst>(MixinParser.Parse(source)
      .Children.Single().Children.Single(child => child.Kind is
        MixinSyntaxKind.DeclarationDirectiveArgument or MixinSyntaxKind.ReferenceDirectiveArgument));

    Assert.Equal(symbol, argument.ArgumentMetadata.SymbolKind);
    Assert.Equal(usage, argument.ArgumentMetadata.SymbolUsage);
  }

  [Fact]
  public void AstHasOneParentedSemanticTreeAndTypedTrivia() {
    const string source = "@RETURN @table\n@+:put<name><value>";
    var program = MixinParser.Parse(source);
    var continuation = Assert.Single(Descendants(program).OfType<TriviaAst>());

    Assert.Equal(MixinSyntaxKind.Continuation, continuation.Kind);
    Assert.True(continuation.IsTrivia);
    Assert.DoesNotContain(continuation, continuation.Parent.SemanticChildren);
    Assert.All(Descendants(program), node => {
      Assert.NotNull(node.Parent);
      Assert.Same(program, node.Program);
    });
  }

  [Fact]
  public void StatementsAndExecutableDirectivesUseSeparateAstBranches() {
    Assert.IsAssignableFrom<StatementAst>(MixinParser.Parse("@CALL<Render>").Instructions.Single());
    Assert.IsAssignableFrom<ExecutableDirectiveAst>(
      MixinParser.Parse("@PUSH<value> @table").Instructions.Single()
    );
  }

  [Fact]
  public void EveryRegisteredDefinitionProvidesCompleteLanguageMetadata() {
    Assert.All(FunctionLibrary.Enumerate(), definition => {
      Assert.NotNull(definition.Metadata);
      Assert.False(string.IsNullOrWhiteSpace(definition.Documentation));
      Assert.True(definition.ArgumentTypes.Count >= definition.MinimumArguments);
    });
    Assert.All(DirectiveLibrary.EnumerateLanguageDefinitions(), definition => {
      Assert.NotNull(definition.Metadata);
      Assert.False(string.IsNullOrWhiteSpace(definition.Documentation));
      Assert.True(definition.ArgumentTypes.Count >= definition.MinimumArguments);
    });
  }

  private static string Slice(string source, MixinSourceRange range) =>
    source.Substring(range.Start, range.Length);

  private static System.Collections.Generic.IEnumerable<MixinAst> Descendants(MixinAst node) {
    foreach (var child in node.Children) {
      yield return child;
      foreach (var descendant in Descendants(child)) yield return descendant;
    }
  }
}
