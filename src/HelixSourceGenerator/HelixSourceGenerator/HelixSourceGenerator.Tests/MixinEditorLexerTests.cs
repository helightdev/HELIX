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
}
