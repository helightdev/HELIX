using System.Collections.Generic;
using System.IO;
using System.Linq;
using Antlr4.Runtime;
using Xunit;
using Lexer = Hix.Compiler.Generated.HixLexer;
using Parser = Hix.Compiler.Generated.HixParser;

namespace HELIX.SourceGen.Tests;

public sealed class HixAntlrGrammarTests {
  [Fact]
  public void RoslynCallsInsideGlobalFunctionsAndDerivationsArePreparedStatically() {
    var unit = Hix.Compiler.AntlrSyntax.Parse("""
      func inspect(symbol value) -> symbol { return(param#value:type) }
      derivation mixin Base { expression { local inspected @= param:type; } }
      mixin Example { expression { } }
      """, Hix.HixMixinBackend.Instance);
    Assert.True(unit.Diagnostics.Count == 0, string.Join("\n", unit.Diagnostics));

    var program = Hix.Compiler.HixCompiler.Compile(unit, "Example", Hix.HixMixinBackend.Instance);
    var dump = program.Disassemble();

    Assert.Contains("static type(symbol 0) -> symbol", dump);
    Assert.DoesNotContain("dynamic type(", dump);
  }

  [Theory]
  [InlineData("Core")]
  [InlineData("Boot")]
  [InlineData("Compose")]
  [InlineData("Context")]
  public void CanonicalHelixLibrariesUseCurrentHixSyntax(string name) {
    var path = Path.GetFullPath(Path.Combine(
      "../../../../HELIX/Assets/Mixins", name + ".HelixSourceGenerator.additionalfile"));

    var unit = Hix.Compiler.AntlrSyntax.Parse(File.ReadAllText(path));

    Assert.True(unit.Diagnostics.Count == 0, string.Join("\n", unit.Diagnostics));
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
      Assert.Equal(position, token.StartIndex);
      Assert.Equal(source.Substring(token.StartIndex, (token.StopIndex + 1) - token.StartIndex), token.Text);
      position = (token.StopIndex + 1);
    }
    Assert.Equal(source.Length, position);
  }

  [Theory]
  [InlineData("mixin Example { expression { emit<hello> } }")]
  [InlineData("mixin Example { expression { local x = @[<a>, <b>] } }")]
  [InlineData("mixin Example { expression { local x = @{name=<a>, enabled=true} } }")]
  [InlineData("mixin Example { prelude expression { carry local name @= target:name; } expression { emit<[local#name]> } }")]
  [InlineData("pure func describe(string name) -> string do { return(<Name: [$name]>) }")]
  [InlineData("mixin Example { expression { local x = when [true] {\n[true] -> <yes>\nelse -> <no>\n} } }")]
  [InlineData("mixin Example { expression { emit @> Hello {{this:name}}\n@+!\n} }")]
  [InlineData("mixin Example { expression { local x @= null ?: <fallback>; } }")]
  [InlineData("mixin Example { expression { emit(number<2>) } }")]
  [InlineData("mixin Example { expression { emit(12.5) } }")]
  [InlineData("mixin Example { expression { emit(-12.5) } }")]
  [InlineData("pure func empty(null value) -> null { return(null) }")]
  [InlineData("pure func answer => 42\n")]
  [InlineData("pure func <answer with spaces> => 42\n")]
  [InlineData("mixin <HELIX.Example-Type> { expression { emit(<ok>) } }")]
  [InlineData("%deprecated\n%since(<2.0>)\n%[<future>]\npure func annotated => 42\n")]
  [InlineData("pure func typed(%optional string value) -> string { return($value) }")]
  [InlineData("pure func typed(%many<string> tuple values, %const<test> string another) -> string { return($another) }")]
  [InlineData("mixin AnnotatedTable { expression { local value = @{%[<field-note>] name=<Ada>} } }")]
  [InlineData("%type<Item>\n%guid<12345678-1234-1234-1234-123456789012>\n%name<My Custom Item>\n---\nmixin Test { }")]
  [InlineData("mixin Example { expression { local mapper = func => <[$0]>; emit(call(local#mapper, <x>)) } }")]
  [InlineData("mixin Example { expression { local mapper = func { return(<[$0]>) } } }")]
  [InlineData("pure func trailing(string first, string second,) -> string { return(join(<a>, <b>,)) }")]
  [InlineData("mixin Trailing { expression { local tuple = @[<a>, <b>,]; local table = @{first=<a>, second=<b>,}; emit(tuple) } }")]
  [InlineData("type NullableString = %union<string><null>\ntype Pair = @[string left, number right]\ntype Handler = delegate(string value) -> null\npure func typed(string value) -> null { return(null) }")]
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
    Assert.Equal("answer with spaces", semantic.Declarations.OfType<Hix.Compiler.FunctionDeclarationIr>().Single().Name);
    Assert.Equal("HELIX.Example-Type", semantic.Declarations.OfType<Hix.Compiler.MixinDeclarationIr>().Single().Name);
  }

  [Fact]
  public void MetadataIsRetainedOnDeclarationsAndTableFields() {
    var semantic = Hix.Compiler.AntlrSyntax.Parse("""
      %deprecated
      %since(<2.0>)
      %[<future>]
      pure func annotated(%optional string value) -> @{string value} {
        return(@{%[<field-note>] value=$value})
      }
      """);

    Assert.Empty(semantic.Diagnostics);
    var function = semantic.Declarations.OfType<Hix.Compiler.FunctionDeclarationIr>().Single();
    Assert.Equal(new[] {"deprecated", "since", null}, function.Metadata.Select(metadata => metadata.Name));
    Assert.Equal("optional", function.Signatures.Single().Inputs.Single().Metadata.Single().Name);
    var table = function.Body.Children.SelectMany(Descendants).OfType<Hix.Compiler.TableExpressionIr>().Single();
    Assert.Equal("field-note", Assert.IsType<Hix.Compiler.StringExpressionIr>(
      table.FieldMetadata.Single().Value.Single().Values.Single()).Value);
  }

  [Fact]
  public void TablePatternFieldsAcceptAssignedDefaults() {
    var semantic = Hix.Compiler.AntlrSyntax.Parse(
      "type Person = @{string name, number age = [18], %default<draft> string state}");

    Assert.Empty(semantic.Diagnostics);
    var fields = Assert.IsType<Hix.TableHixPattern>(semantic.Declarations.OfType<Hix.Compiler.TypeDeclarationIr>()
      .Single().Pattern).Fields;
    Assert.False(fields[0].HasDefault);
    Assert.Equal(18d, fields[1].DefaultValue);
    Assert.Equal("draft", fields[2].DefaultValue);
  }

  [Theory]
  [InlineData("%deprecated func Build { return(null) }", 1)]
  [InlineData("%deprecated %since(<2.0>) func Build { return(null) }", 2)]
  public void MetadataCanAppearInlineBeforeADeclaration(string source, int expectedCount) {
    var semantic = Hix.Compiler.AntlrSyntax.Parse(source);

    Assert.Empty(semantic.Diagnostics);
    var function = semantic.Declarations.OfType<Hix.Compiler.FunctionDeclarationIr>().Single();
    Assert.Equal(expectedCount, function.Metadata.Count);
  }

  [Fact]
  public void NamedFieldMetadataEndsAtWhitespace() {
    var semantic = Hix.Compiler.AntlrSyntax.Parse("""
      pure func typed(%many<string> tuple test, %const<test> string another) -> @{string test, string another} {
        return(@{%anyOf<string><test> test=<yes>, %something(123) another=<yes>})
      }
      """);

    Assert.Empty(semantic.Diagnostics);
    var function = Assert.Single(semantic.Declarations.OfType<Hix.Compiler.FunctionDeclarationIr>());
    var fields = Assert.Single(function.Signatures).Inputs;
    Assert.Equal(new[] {"string"}, fields[0].Metadata.Single().Values
      .Cast<Hix.Compiler.StringExpressionIr>().Select(value => value.Value));
    Assert.Equal("test", Assert.IsType<Hix.Compiler.StringExpressionIr>(fields[1].Metadata.Single().Values.Single()).Value);
    var table = Assert.Single(function.Body.Children.SelectMany(Descendants).OfType<Hix.Compiler.TableExpressionIr>());
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
    Assert.Empty(Assert.Single(semantic.Declarations.OfType<Hix.Compiler.MixinDeclarationIr>()).Metadata);
  }

  [Fact]
  public void NumberLiteralProducesANumericAstAndToken() {
    var semantic = Hix.Compiler.AntlrSyntax.Parse("mixin Example { expression { emit(-12.5) } }");

    Assert.Empty(semantic.Diagnostics);
    var number = Assert.Single(semantic.Children.SelectMany(Descendants).OfType<Hix.Compiler.NumberExpressionIr>());
    Assert.Equal(-12.5, number.Value);
    Assert.Contains(semantic.Tokens, token => token.Type == Hix.Compiler.Generated.HixLexer.NUMBER && token.Text == "-12.5");
  }

  [Theory]
  [InlineData("true", true)]
  [InlineData("false", false)]
  public void BooleanLiteralProducesAPrimitiveAstAndToken(string source, bool expected) {
    var semantic = Hix.Compiler.AntlrSyntax.Parse(
      "mixin Example { expression { emit(" + source + ") } }");

    Assert.Empty(semantic.Diagnostics);
    var boolean = Assert.Single(semantic.Children.SelectMany(Descendants)
      .OfType<Hix.Compiler.BooleanExpressionIr>());
    Assert.Equal(expected, boolean.Value);
    Assert.Contains(semantic.Tokens, token =>
      token.Type == Hix.Compiler.Generated.HixLexer.BOOLEAN && token.Text == source);
  }

  [Fact]
  public void NullLiteralProducesAPrimitiveAstAndRemainsAValidSignatureKind() {
    var semantic = Hix.Compiler.AntlrSyntax.Parse("""
      pure func empty(null value) -> null { return(null) }
      """);

    Assert.Empty(semantic.Diagnostics);
    Assert.Single(semantic.Children.SelectMany(Descendants).OfType<Hix.Compiler.NullExpressionIr>());
    Assert.Contains(semantic.Tokens, token =>
      token.Type == Hix.Compiler.Generated.HixLexer.NULL && token.Text == "null");
    var function = Assert.Single(semantic.Declarations.OfType<Hix.Compiler.FunctionDeclarationIr>());
    Assert.Equal("null", Assert.Single(Assert.Single(function.Signatures).Inputs).Kind);
    Assert.Equal("null", Assert.Single(function.Signatures).OutputKind);
  }

  [Fact]
  public void ArrowWithoutParametersDeclaresDynamicInput() {
    var semantic = Hix.Compiler.AntlrSyntax.Parse(
      "pure func inspect -> string { return(<[$it]:[param#name]>) }");

    Assert.Empty(semantic.Diagnostics);
    var signature = Assert.Single(Assert.Single(semantic.Declarations
      .OfType<Hix.Compiler.FunctionDeclarationIr>()).Signatures);
    Assert.Null(signature.Inputs);
    Assert.Null(signature.InputPattern);
    Assert.Equal("string", signature.OutputKind);
  }

  [Fact]
  public void LegacySigFunctionSyntaxIsRejected() {
    var semantic = Hix.Compiler.AntlrSyntax.Parse(
      "pure func inspect sig any -> string { return(<old>) }");

    Assert.NotEmpty(semantic.Diagnostics);
  }

  [Theory]
  [InlineData("value:eq(<test>); emit(<done>)")]
  [InlineData("value:eq(<test>) emit(<done>)")]
  public void IdentifierColonStartsAStatementDerivation(string statements) {
    var semantic = Hix.Compiler.AntlrSyntax.Parse(
      "mixin Example { expression { " + statements + " } }");

    Assert.Empty(semantic.Diagnostics);
    var expression = Assert.Single(Assert.Single(semantic.Declarations
      .OfType<Hix.Compiler.MixinDeclarationIr>()).Declarations.OfType<Hix.Compiler.ExpressionDeclarationIr>());
    Assert.IsType<Hix.Compiler.ExpressionStatementIr>(expression.Body.Statements[0]);
  }

  [Fact]
  public void LocalDeclarationsCarryOptionalPatternsAndInitializers() {
    var semantic = Hix.Compiler.AntlrSyntax.Parse("""
      type Label = string
      mixin Example { expression {
        local string typed
        local Label named = <value>
        local %optional string annotated
        local inferred = 42
        $inferred = 7
      } }
      """);

    Assert.Empty(semantic.Diagnostics);
    var locals = semantic.Children.SelectMany(Descendants).OfType<Hix.Compiler.AssignmentStatementIr>().ToArray();
    Assert.Equal(5, locals.Length);
    Assert.Null(locals[0].Value);
    Assert.Equal("string", locals[0].DeclaredPattern.Display);
    Assert.Equal("Label", locals[1].DeclaredPattern.Display);
    Assert.False(locals[^1].IsDeclaration);
  }

  private static IEnumerable<Hix.Compiler.HixIrNode> Descendants(Hix.Compiler.HixIrNode node) {
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
