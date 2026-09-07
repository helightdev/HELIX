using System.Collections.Generic;
using System.IO;
using System.Linq;
using Antlr4.Runtime;
using Xunit;
using Lexer = Mixins.Compiler.Generated.MixinLexer;
using Parser = Mixins.Compiler.Generated.MixinParser;

namespace HELIX.SourceGen.Tests;

public sealed class MixinAntlrGrammarTests {
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
  [InlineData("mixin Example { prelude expression { carry name @= target:name; } expression { emit<[carry#name]> } }")]
  [InlineData("pure func describe sig @{name=string} -> string do { return(<Name: [param#name]>) }")]
  [InlineData("mixin Example { expression { local x = when [true] {\n[true] -> <yes>\nelse -> <no>\n} } }")]
  [InlineData("mixin Example { expression { emit @> Hello {{this:name}}\n@+!\n} }")]
  [InlineData("mixin Example { expression { local x @= null ?: <fallback>; } }")]
  [InlineData("mixin Example { expression { emit(number<2>) } }")]
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
