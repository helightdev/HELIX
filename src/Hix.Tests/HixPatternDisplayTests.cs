using System.Collections.Generic;
using Hix;
using Hix.Compiler;
using Hix.Runtime;
using Xunit;

namespace HELIX.SourceGen.Tests;

public sealed class HixPatternDisplayTests {
  private static readonly HixPattern Number = HixPatterns.Named("number");
  private static readonly HixPattern Text = HixPatterns.Named("string");
  public static IEnumerable<object[]> Displays() {
    yield return [new ManyHixPattern(new ManyHixPattern(Text)), "many(many(string))"];
    yield return [HixPatterns.Union([Number, Text]), "number|string"];
    yield return [HixPatterns.Nullable(Number), "number?"];
    yield return [HixPatterns.Nullable(HixPatterns.Union([Number, Text])), "(number|string)?"];
    yield return [new TableHixPattern([new("value", Number)]), "{}"];
    yield return [new TupleHixPattern([new("value", Number)]), "[]"];
    yield return [new MapHixPattern(Text, new ManyHixPattern(Number)), "map(string, many(number))"];
    yield return [new DocumentedHixPattern(new NamedHixPattern("path.Record"), "Title", "Description"), "path.Record"];
    yield return [new DelegateHixPattern([new("value", Number, Optional: true)], Text), "delegate(number value?) -> string"];
    yield return [new DelegateHixPattern([new("value", HixPatterns.Nullable(Number))], Text), "delegate(number? value) -> string"];
    yield return [new DelegateHixPattern([new("value", HixPatterns.Nullable(Number), Optional: true)], Text), "delegate(number? value?) -> string"];
    yield return [new DelegateHixPattern([new("value", Number, HasDefault: true)], Text), "delegate(number value?) -> string"];
    yield return [new DelegateHixPattern([new SignatureField("values", Number, variadic: true).AsPatternField()], Text),
      "delegate(...number values) -> string"];
  }
  [Theory]
  [MemberData(nameof(Displays))]
  public void PatternsUseCompactDisplay(HixPattern pattern, string expected) => Assert.Equal(expected, pattern.Display);

  [Fact]
  public void NamedPatternDocumentationUsesOnlyTheName() {
    var analysis = new LanguageAnalysis("""
      type Address = @{string street};
      %tagged<dog>
      type Dog = @{string name};
      """);
    Assert.Empty(analysis.Program.Diagnostics);
    Assert.Contains(analysis.TypeFacts, fact => fact.Type == "Address" && fact.Documentation == "Address");
    Assert.Contains(analysis.TypeFacts, fact => fact.Type == "Dog" && fact.Documentation == "Dog");
  }

  [Fact]
  public void NamedPatternsRemainNamedThroughGenericInferenceInHints() {
    var analysis = new LanguageAnalysis("""
      type Record = @{number value};
      %generic<T>
      pure func identity(T value) -> T => $value;
      func main {
        local Record record = @{value = 42}
        local copied = identity($record)
      }
      """);
    Assert.Empty(analysis.Program.Diagnostics);
    Assert.Contains(analysis.TypeFacts, fact => fact.Inlay && fact.Documentation == "local copied: Record");
  }

  [Fact]
  public void ShortenedDisplaysDoNotMergeStructuralOverloads() {
    var program = HixCompiler.CompileFunctions(["""
      pure noinline func select(@{number value} input) -> string => <number>;
      pure noinline func select(@{string value} input) -> string => <string>;
      func main() -> string { return(<[select(@{value = 42})]/[select(@{value = <text>})]>) }
      """]);
    var restored = HixCompiler.CompileFunctions([HixDecompiler.Decompile(program)]);
    using var vm = new HixVM([restored]);
    var context = HixCoreBackend.Instance.CreateCoroutine();
    var result = vm.Invoke(restored, context);
    Assert.True(result.Success);
    Assert.Equal("number/string", result.Value.Render(context).Resolve(context.Strings));
    Assert.NotEqual(HixGenericTypes.PatternIdentity(new TableHixPattern([new("value", Number)])),
      HixGenericTypes.PatternIdentity(new TableHixPattern([new("value", Text)])));
  }
}
