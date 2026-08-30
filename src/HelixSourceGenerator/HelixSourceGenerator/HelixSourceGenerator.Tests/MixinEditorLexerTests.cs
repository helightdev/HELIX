using System.Linq;
using MixinLanguage.Compiler;
using Xunit;

namespace HELIX.SourceGen.Tests;

public sealed class MixinEditorLexerTests {
  [Fact]
  public void TokenizesAdditionalFileSyntaxFromSharedLanguage() {
    const string source = "@# comment\n@LOCAL<Name> @param#symbol:name:replace<Old><New>\n@+ :members";

    var kinds = MixinEditorLexer.Lex(source).Select(token => token.Kind).ToArray();

    Assert.Contains(MixinEditorTokenKind.Comment, kinds);
    Assert.Contains(MixinEditorTokenKind.Directive, kinds);
    Assert.Contains(MixinEditorTokenKind.Value, kinds);
    Assert.Contains(MixinEditorTokenKind.Path, kinds);
    Assert.Contains(MixinEditorTokenKind.Function, kinds);
    Assert.Contains(MixinEditorTokenKind.Continuation, kinds);
  }

  [Fact]
  public void PreservesEverySourceCharacter() {
    const string source = "  @RETURN (@param#value:mapValues<(@v)>)\r\n";
    var tokens = MixinEditorLexer.Lex(source);

    Assert.Equal(0, tokens.First().Start);
    Assert.Equal(source.Length, tokens.Last().End);
    for (var index = 1; index < tokens.Count; index++)
      Assert.Equal(tokens[index - 1].End, tokens[index].Start);
  }

  [Fact]
  public void HighlightsEnclosedRootAndAtSignSeparately() {
    const string source = "@(this:type)";
    var tokens = MixinEditorLexer.Lex(source).ToArray();

    Assert.Equal(new[] {
      MixinEditorTokenKind.Value,
      MixinEditorTokenKind.EnclosedReferenceParenthesis,
      MixinEditorTokenKind.Value,
      MixinEditorTokenKind.Operator,
      MixinEditorTokenKind.Function,
      MixinEditorTokenKind.EnclosedReferenceParenthesis
    }, tokens.Select(token => token.Kind));
    Assert.Equal("@", source[tokens[0].Start..tokens[0].End]);
    Assert.Equal("this", source[tokens[2].Start..tokens[2].End]);
  }

  [Fact]
  public void OnlyEnclosedReferenceParenthesesAreLanguagePunctuation() {
    const string source = "@MIXIN Foo(bar); @(var#CompanionName)";
    var tokens = MixinEditorLexer.Lex(source);

    Assert.Equal(2, tokens.Count(token => token.Kind == MixinEditorTokenKind.EnclosedReferenceParenthesis));
    Assert.DoesNotContain(tokens, token => token.Kind == MixinEditorTokenKind.Parenthesis);
  }

  [Fact]
  public void HighlightsLiteralArgumentContent() {
    const string source = "@value:replace<Old><New>";
    var tokens = MixinEditorLexer.Lex(source);

    Assert.Equal(new[] { "Old", "New" }, tokens
      .Where(token => token.Kind == MixinEditorTokenKind.Argument)
      .Select(token => source[token.Start..token.End]));
  }

  [Theory]
  [InlineData("@MIXIN<$PostConstruct><0> global::UnityEngine.UIElements.VisualElementExtensions")]
  [InlineData("@ANNOTATION<HELIX.Compose.ClickHandlerAttribute>")]
  [InlineData("@RETURN @value:replace<global::Old><global::New>")]
  public void DoesNotTreatLiteralColonsAsFunctions(string source) {
    var tokens = MixinEditorLexer.Lex(source);

    Assert.DoesNotContain(tokens, token =>
      token.Kind == MixinEditorTokenKind.Function &&
      source[token.Start..token.End] == "UnityEngine");
    Assert.DoesNotContain(tokens, token =>
      token.Kind == MixinEditorTokenKind.Function &&
      source[token.Start..token.End] is "Old" or "New");
  }

  [Fact]
  public void DistinguishesDirectiveAndExpressionArgumentDelimiters() {
    const string source = "@ANNOTATION<HELIX.PropAttribute>\n@RETURN @value:replace<Old><New>";
    var tokens = MixinEditorLexer.Lex(source);

    Assert.Equal(2, tokens.Count(token => token.Kind == MixinEditorTokenKind.DirectiveArgumentDelimiter));
    Assert.Equal(4, tokens.Count(token => token.Kind == MixinEditorTokenKind.ExpressionArgumentDelimiter));
  }
}
