using System.Collections.Generic;
using System.IO;
using System.Linq;
using Antlr4.Runtime;
using Xunit;
using Lexer = Hix.Compiler.Generated.HixLexer;
using Parser = Hix.Compiler.Generated.HixParser;

namespace HELIX.SourceGen.Tests;

public sealed class HixAntlrGrammarTests {
  [Theory]
  [InlineData("Core")]
  [InlineData("Boot")]
  [InlineData("Compose")]
  [InlineData("Context")]
  public void CanonicalHelixLibrariesUseCurrentHixSyntax(string name) {
    var path = Path.GetFullPath(Path.Combine(
      "../../../../HELIX/Assets/Mixins", name + ".HelixSourceGenerator.additionalfile"));

    var unit = Hix.Compiler.AntlrSyntax.Parse(File.ReadAllText(path));

    Assert.Empty(unit.Diagnostics);
    Assert.NotEmpty(unit.Declarations);
  }

  [Theory]
  [InlineData("  mixin Example {\r\n\t expression { emit<😀> }\r\n}  ")]
  [InlineData("mixin Example { expression { emit<\\x5bescaped\\x5d> } }\n")]
  [InlineData("// comment\nmixin Example { expression { emit @> text\n  @+ continued\n} }\n")]
  public void CanonicalTokensCoverTheOriginalUtf16Source(string source) {
    var unit = Hix.Compiler.AntlrSyntax.Parse(source);
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
  [InlineData("pure func answer => 42\n")]
  [InlineData("pure func <answer with spaces> => 42\n")]
  [InlineData("mixin <HELIX.Example-Type> { expression { emit(<ok>) } }")]
  [InlineData("%deprecated\n%since(<2.0>)\n%[<future>]\npure func annotated => 42\n")]
  [InlineData("pure func typed sig @{%[<native-type>] value=string} -> string { return(param#value) }")]
  [InlineData("pure func typed sig @{%anyOf<string><test> value=string, %something(123) another=string} -> string { return(param#value) }")]
  [InlineData("mixin AnnotatedTable { expression { local value = @{%[<field-note>] name=<Ada>} } }")]
  [InlineData("%type<Item>\n%guid<12345678-1234-1234-1234-123456789012>\n%name<My Custom Item>\n---\nmixin Test { }")]
  [InlineData("mixin Example { expression { local mapper = func => <[$0]>; emit(call(local#mapper, <x>)) } }")]
  [InlineData("mixin Example { expression { local mapper = func { return(<[$0]>) } } }")]
  [InlineData("pure func trailing sig @{first=string, second=string,} -> string { return(join(<a>, <b>,)) }")]
  [InlineData("mixin Trailing { expression { local tuple = @[<a>, <b>,]; local table = @{first=<a>, second=<b>,}; emit(tuple) } }")]
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
    var semantic = Hix.Compiler.AntlrSyntax.Parse(source);
    Assert.Empty(semantic.Diagnostics);
    Assert.NotEmpty(semantic.Declarations);
  }

  [Fact]
  public void ArgumentBasedDeclarationNamesBecomeLiteralAstNames() {
    var semantic = Hix.Compiler.AntlrSyntax.Parse("""
      pure func <answer with spaces> => 42;
      mixin <HELIX.Example-Type> { expression { emit(<ok>) } }
      """);

    Assert.Empty(semantic.Diagnostics);
    Assert.Equal("answer with spaces", semantic.Declarations.OfType<Hix.Compiler.FunctionDeclarationAst>().Single().Name);
    Assert.Equal("HELIX.Example-Type", semantic.Declarations.OfType<Hix.Compiler.MixinDeclarationAst>().Single().Name);
  }

  [Fact]
  public void MetadataIsRetainedOnDeclarationsAndTableFields() {
    var semantic = Hix.Compiler.AntlrSyntax.Parse("""
      %deprecated
      %since(<2.0>)
      %[<future>]
      pure func annotated sig @{%[<native-type>] value=string} -> string {
        return(@{%[<field-note>] value=param#value})
      }
      """);

    Assert.Empty(semantic.Diagnostics);
    var function = semantic.Declarations.OfType<Hix.Compiler.FunctionDeclarationAst>().Single();
    Assert.Equal(new[] {"deprecated", "since", null}, function.Metadata.Select(metadata => metadata.Name));
    Assert.Equal("native-type", Assert.IsType<Hix.Compiler.StringExpressionAst>(
      function.Signatures.Single().Inputs.Single().Metadata.Single().Values.Single()).Value);
    var table = function.Body.Children.SelectMany(Descendants).OfType<Hix.Compiler.TableExpressionAst>().Single();
    Assert.Equal("field-note", Assert.IsType<Hix.Compiler.StringExpressionAst>(
      table.FieldMetadata.Single().Value.Single().Values.Single()).Value);
  }

  [Theory]
  [InlineData("%deprecated func Build { return(null) }", 1)]
  [InlineData("%deprecated %since(<2.0>) func Build { return(null) }", 2)]
  public void MetadataCanAppearInlineBeforeADeclaration(string source, int expectedCount) {
    var semantic = Hix.Compiler.AntlrSyntax.Parse(source);

    Assert.Empty(semantic.Diagnostics);
    var function = semantic.Declarations.OfType<Hix.Compiler.FunctionDeclarationAst>().Single();
    Assert.Equal(expectedCount, function.Metadata.Count);
  }

  [Fact]
  public void NamedFieldMetadataEndsAtWhitespace() {
    var semantic = Hix.Compiler.AntlrSyntax.Parse("""
      pure func typed sig @{%anyOf<string><test> test=string, %something(123) another=string} -> string {
        return(@{%anyOf<string><test> test=<yes>, %something(123) another=<yes>})
      }
      """);

    Assert.Empty(semantic.Diagnostics);
    var function = Assert.Single(semantic.Declarations.OfType<Hix.Compiler.FunctionDeclarationAst>());
    var fields = Assert.Single(function.Signatures).Inputs;
    Assert.Equal(new[] {"string", "test"}, fields[0].Metadata.Single().Values
      .Cast<Hix.Compiler.StringExpressionAst>().Select(value => value.Value));
    Assert.Equal(123, Assert.IsType<Hix.Compiler.NumberExpressionAst>(fields[1].Metadata.Single().Values.Single()).Value);
    var table = Assert.Single(function.Body.Children.SelectMany(Descendants).OfType<Hix.Compiler.TableExpressionAst>());
    Assert.Equal(new[] {"test", "another"}, table.FieldMetadata.Select(field => field.Key));
  }

  [Fact]
  public void SectionDelimiterSeparatesFileMetadataFromDeclarations() {
    var semantic = Hix.Compiler.AntlrSyntax.Parse("""
      %type<Item>
      %guid<12345678-1234-1234-1234-123456789012>
      %name<My Custom Item>
      %interaction<something>
      ---
      mixin Test {
      }
      """);

    Assert.Empty(semantic.Diagnostics);
    Assert.Equal(new[] {"type", "guid", "name", "interaction"}, semantic.Metadata.Select(value => value.Name));
    Assert.Empty(Assert.Single(semantic.Declarations.OfType<Hix.Compiler.MixinDeclarationAst>()).Metadata);
  }

  [Fact]
  public void NumberLiteralProducesANumericAstAndToken() {
    var semantic = Hix.Compiler.AntlrSyntax.Parse("mixin Example { expression { emit(-12.5) } }");

    Assert.Empty(semantic.Diagnostics);
    var number = Assert.Single(semantic.Children.SelectMany(Descendants).OfType<Hix.Compiler.NumberExpressionAst>());
    Assert.Equal(-12.5, number.Value);
    Assert.Contains(semantic.Tokens, token => token.Kind == Hix.Compiler.HixTokenKind.Number && token.Text == "-12.5");
  }

  [Theory]
  [InlineData("true", true)]
  [InlineData("false", false)]
  public void BooleanLiteralProducesAPrimitiveAstAndToken(string source, bool expected) {
    var semantic = Hix.Compiler.AntlrSyntax.Parse(
      "mixin Example { expression { emit(" + source + ") } }");

    Assert.Empty(semantic.Diagnostics);
    var boolean = Assert.Single(semantic.Children.SelectMany(Descendants)
      .OfType<Hix.Compiler.BooleanExpressionAst>());
    Assert.Equal(expected, boolean.Value);
    Assert.Contains(semantic.Tokens, token =>
      token.Kind == Hix.Compiler.HixTokenKind.Boolean && token.Text == source);
  }

  [Fact]
  public void NullLiteralProducesAPrimitiveAstAndRemainsAValidSignatureKind() {
    var semantic = Hix.Compiler.AntlrSyntax.Parse("""
      pure func empty sig null -> null { return(null) }
      """);

    Assert.Empty(semantic.Diagnostics);
    Assert.Single(semantic.Children.SelectMany(Descendants).OfType<Hix.Compiler.NullExpressionAst>());
    Assert.Contains(semantic.Tokens, token =>
      token.Kind == Hix.Compiler.HixTokenKind.Null && token.Text == "null");
    var function = Assert.Single(semantic.Declarations.OfType<Hix.Compiler.FunctionDeclarationAst>());
    Assert.Equal("null", Assert.Single(function.Signatures).InputKind);
    Assert.Equal("null", Assert.Single(function.Signatures).OutputKind);
  }

  private static IEnumerable<Hix.Compiler.HixAst> Descendants(Hix.Compiler.HixAst node) {
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
