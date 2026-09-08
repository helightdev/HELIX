using System.Collections.Generic;
using System.IO;
using System.Linq;
using Antlr4.Runtime;
using Xunit;
using Lexer = Mixins.Compiler.Generated.HixLexer;
using Parser = Mixins.Compiler.Generated.HixParser;

namespace HELIX.SourceGen.Tests;

public sealed class HixAntlrGrammarTests {
  [Theory]
  [InlineData("Core")]
  [InlineData("Boot")]
  [InlineData("Compose")]
  [InlineData("Context")]
  public void CanonicalHelixLibrariesUseCurrentHixSyntax(string name) {
    var path = Path.GetFullPath(Path.Combine(
      "../../../../../../HELIX/Assets/Mixins", name + ".HelixSourceGenerator.additionalfile"));

    var unit = Mixins.Compiler.AntlrSyntax.Parse(File.ReadAllText(path));

    Assert.Empty(unit.Diagnostics);
    Assert.NotEmpty(unit.Declarations);
  }

  [Theory]
  [InlineData("  mixin Example {\r\n\t expression { emit<😀> }\r\n}  ")]
  [InlineData("mixin Example { expression { emit<\\x5bescaped\\x5d> } }\n")]
  [InlineData("// comment\nmixin Example { expression { emit @> text\n  @+ continued\n} }\n")]
  public void CanonicalTokensCoverTheOriginalUtf16Source(string source) {
    var unit = Mixins.Compiler.AntlrSyntax.Parse(source);
    Assert.Empty(unit.Diagnostics);
    Assert.Equal(source, string.Concat(unit.Tokens.Select(token => token.Text)));
    var position = 0;
    foreach (var token in unit.Tokens) {
      Assert.Equal(position, token.Start);
      Assert.Equal(source.Substring(token.Start, token.End - token.Start), token.Text);
      position = token.End;
    }
    Assert.Equal(source.Length, position);
  }

  [Theory]
  [InlineData("mixin Example { expression { emit<hello> } }")]
  [InlineData("mixin Example { expression { local x = @[<a>, <b>] } }")]
  [InlineData("mixin Example { expression { local x = @{name=<a>, enabled=true} } }")]
  [InlineData("mixin Example { prelude expression { carry local name @= target:name; } expression { emit<[local#name]> } }")]
  [InlineData("pure func describe sig @{name=string} -> string do { return(<Name: [param#name]>) }")]
  [InlineData("mixin Example { expression { local x = when [true] {\n[true] -> <yes>\nelse -> <no>\n} } }")]
  [InlineData("mixin Example { expression { emit @> Hello {{this:name}}\n@+!\n} }")]
  [InlineData("mixin Example { expression { local x @= null ?: <fallback>; } }")]
  [InlineData("mixin Example { expression { emit(number<2>) } }")]
  [InlineData("mixin Example { expression { emit(12.5) } }")]
  [InlineData("mixin Example { expression { emit(-12.5) } }")]
  [InlineData("pure func empty sig null -> null { return(null) }")]
  public void ParsesLanguageFeatures(string source) {
    var errors = new Errors();
    var lexer = new Lexer(new AntlrInputStream(source));
    lexer.RemoveErrorListeners();
    lexer.AddErrorListener(errors);
    var tokens = new CommonTokenStream(lexer);
    var parser = new Parser(tokens);
    parser.RemoveErrorListeners();
    parser.AddErrorListener(errors);
    parser.compilationUnit();
    Assert.Empty(errors.Messages);
    Assert.DoesNotContain(tokens.GetTokens(), token => token.Type == Lexer.ERROR_TOKEN);
    Assert.Equal(TokenConstants.EOF, parser.CurrentToken.Type);
    var semantic = Mixins.Compiler.AntlrSyntax.Parse(source);
    Assert.Empty(semantic.Diagnostics);
    Assert.NotEmpty(semantic.Declarations);
  }

  [Fact]
  public void NumberLiteralProducesANumericAstAndToken() {
    var semantic = Mixins.Compiler.AntlrSyntax.Parse("mixin Example { expression { emit(-12.5) } }");

    Assert.Empty(semantic.Diagnostics);
    var number = Assert.Single(semantic.Children.SelectMany(Descendants).OfType<Mixins.Compiler.NumberExpressionAst>());
    Assert.Equal(-12.5, number.Value);
    Assert.Contains(semantic.Tokens, token => token.Kind == Mixins.Compiler.HixTokenKind.Number && token.Text == "-12.5");
  }

  [Theory]
  [InlineData("true", true)]
  [InlineData("false", false)]
  public void BooleanLiteralProducesAPrimitiveAstAndToken(string source, bool expected) {
    var semantic = Mixins.Compiler.AntlrSyntax.Parse(
      "mixin Example { expression { emit(" + source + ") } }");

    Assert.Empty(semantic.Diagnostics);
    var boolean = Assert.Single(semantic.Children.SelectMany(Descendants)
      .OfType<Mixins.Compiler.BooleanExpressionAst>());
    Assert.Equal(expected, boolean.Value);
    Assert.Contains(semantic.Tokens, token =>
      token.Kind == Mixins.Compiler.HixTokenKind.Boolean && token.Text == source);
  }

  [Fact]
  public void NullLiteralProducesAPrimitiveAstAndRemainsAValidSignatureKind() {
    var semantic = Mixins.Compiler.AntlrSyntax.Parse("""
      pure func empty sig null -> null { return(null) }
      """);

    Assert.Empty(semantic.Diagnostics);
    Assert.Single(semantic.Children.SelectMany(Descendants).OfType<Mixins.Compiler.NullExpressionAst>());
    Assert.Contains(semantic.Tokens, token =>
      token.Kind == Mixins.Compiler.HixTokenKind.Null && token.Text == "null");
    var function = Assert.Single(semantic.Declarations.OfType<Mixins.Compiler.FunctionDeclarationAst>());
    Assert.Equal("null", Assert.Single(function.Signatures).InputKind);
    Assert.Equal("null", Assert.Single(function.Signatures).OutputKind);
  }

  private static IEnumerable<Mixins.Compiler.HixAst> Descendants(Mixins.Compiler.HixAst node) {
    yield return node;
    foreach (var child in node.Children)
      foreach (var descendant in Descendants(child))
        yield return descendant;
  }

  private sealed class Errors : BaseErrorListener, IAntlrErrorListener<int> {
    public readonly List<string> Messages = [];
    public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol,
      int line, int charPositionInLine, string msg, RecognitionException e) =>
      Messages.Add($"{line}:{charPositionInLine}: {msg}");
    public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol,
      int line, int charPositionInLine, string msg, RecognitionException e) =>
      Messages.Add($"{line}:{charPositionInLine}: {msg}");
  }
}
