using System.Linq;
using Hix.Compiler;
using Xunit;

namespace HELIX.SourceGen.Tests;

public sealed class MixinSyntaxRangeTests {
  [Fact]
  public void SemanticAstUsesAbsoluteUtf16Ranges() {
    const string source = "mixin Example {\n  expression { local value = <😀>; emit(<[local#value]>) }\n}";
    var unit = AntlrSyntax.Parse(source);
    Assert.Empty(unit.Diagnostics);
    Assert.Equal(0, unit.SourceRange.Start);
    Assert.Equal(source.Length, unit.SourceRange.End);
    Assert.All(Descendants(unit), node => {
      Assert.InRange(node.SourceRange.Start, 0, source.Length);
      Assert.InRange(node.SourceRange.End, node.SourceRange.Start, source.Length);
      Assert.NotNull(node.Parent);
      Assert.Same(unit, node.Program);
    });
  }

  [Fact]
  public void FunctionCallsCarryReceiverAndArgumentsInGrammarOrder() {
    const string source = "mixin Example { expression { local value = <text>; assert(local#value:matches<\\\\S>) } }";
    var unit = AntlrSyntax.Parse(source);
    Assert.Empty(unit.Diagnostics);
    var matches = Descendants(unit).OfType<CallExpressionIr>().Single(call => call.Name == "matches");
    Assert.Equal(2, matches.Arguments.Count);
    Assert.Equal("value", Assert.IsType<MemberExpressionIr>(matches.Arguments[0]).Member);
    Assert.Equal("\\S", Assert.IsType<StringExpressionIr>(matches.Arguments[1]).Value);
  }

  [Fact]
  public void BooleanCallOperatorIsOrdinaryCallCoercion() {
    const string source = "mixin Example { expression { local value = <text>; when(local#value:?matches<^t>) { emit<yes> } } }";
    var unit = AntlrSyntax.Parse(source);
    Assert.Empty(unit.Diagnostics);
    var matches = Descendants(unit).OfType<CallExpressionIr>().Single(call => call.Name == "matches");
    Assert.True(matches.CoerceBoolean);
  }

  [Fact]
  public void ContinuationsRemainNonSemanticTokens() {
    const string source = "mixin Example { expression { emit @> first\n@+ second\n} }";
    var unit = AntlrSyntax.Parse(source);
    Assert.Empty(unit.Diagnostics);
    Assert.Single(unit.Tokens.Where(token => token.Type is Hix.Compiler.Generated.HixLexer.CONTENT_WRAP or Hix.Compiler.Generated.HixLexer.VALUE_WRAP));
  }

  private static System.Collections.Generic.IEnumerable<HixIrNode> Descendants(HixIrNode node) {
    foreach (var child in node.Children) {
      yield return child;
      foreach (var descendant in Descendants(child)) yield return descendant;
    }
  }
}
