using System.Linq;
using MixinLanguage.Compiler;
using MixinLanguage.Analysis;
using Xunit;

namespace HELIX.SourceGen.Tests;

public sealed class MixinSyntaxRangeTests {
  [Fact]
  public void ProgramAstCarriesAbsoluteRangesAcrossContinuationLines() {
    const string source =
      "  @LOCAL<Name> @table:put<first><(@param#value)>\n" +
      "    @+:put<second><(@local#Name)>\n";

    var instruction = Assert.IsAssignableFrom<ValueDirectiveSyntax>(
      MixinExpressionParser.Parse(source).Instructions[0]
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
    var reference = MixinExpressionParser.ParseExpressionValues(source).Single().Reference;

    Assert.Equal("local", Slice(source, reference.RootRange));
    Assert.Equal("Value", Slice(source, reference.MemberRange));
    Assert.Equal("replace", Slice(source, reference.Properties.Single().NameRange));
    Assert.Equal("<Old>", Slice(source, reference.Properties.Single().ParsedArguments[0].SourceRange));
    Assert.Equal("<(@param#value)>", Slice(source, reference.Properties.Single().ParsedArguments[1].SourceRange));
  }

  [Fact]
  public void EditorProjectionUsesParserRootRangesForNamedNullRoot() {
    const string source = "@RETURN @null:eq<null>";
    var root = MixinEditorSyntaxParser.Parse(source).Root.Children.Single()
      .Children.Single(child => child.Kind == MixinEditorSyntaxKind.Operand)
      .Children.Single().Children.Single(child => child.Kind == MixinEditorSyntaxKind.Root);

    Assert.Equal("null", Slice(source, root.SourceRange));
  }

  [Fact]
  public void EditorAnalysisFindsNamedRootsThroughParenthesesAndFunctionChains() {
    const string source =
      "@LOCAL<Value> seed\n" +
      "@RETURN @(local#Value:replace<x><y>)\n";

    var analysis = MixinEditorAnalyzer.Analyze(source);
    var reference = Assert.Single(analysis.References.Where(item =>
      item.Kind == MixinEditorReferenceKind.Local && item.Name == "Value"));

    Assert.Equal("Value", Slice(source, reference.Range));
  }

  private static string Slice(string source, MixinSourceRange range) =>
    source.Substring(range.Start, range.Length);
}
