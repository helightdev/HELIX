using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Helix.Weave.Tests;

/// <summary>
/// The prelude keeps the symbol; everything downstream gets the detached ref. These cover the
/// conversion itself, since the type system is what enforces the boundary at authoring time.
/// </summary>
public class DetachmentTests {
  private static (SymbolFacts Facts, INamedTypeSymbol Type) Compile(string source, string typeName) {
    var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest));
    var compilation = CSharpCompilation.Create("Detachment", new[] { tree }, Harness.References,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
    var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
    Assert.Empty(errors);
    return (new SymbolFacts(), compilation.GetTypeByMetadataName(typeName));
  }

  private const string Source = """
    using System.Collections.Generic;

    namespace S {
      public interface IMarker { }
      public class Base { }
      public class Thing : Base, IMarker { }
      public enum Mode { Fast }
      public unsafe struct WithPointer { public int* Pointer; }
      public class Holder { public List<Thing> Items; public int Count; public Mode Mode; }
    }
    """;

  [Fact]
  public void BackedRefAnswersSymbolQueries() {
    var (facts, type) = Compile(Source, "S.Thing");
    var reference = facts.Ref(type);

    Assert.True(reference.Exists);
    Assert.True(reference.IsClass);
    Assert.True(reference.Implements("S.IMarker"));
    Assert.True(reference.InheritsFrom("S.Base"));
    Assert.False(reference.Implements("S.Missing"));
    Assert.Equal("global::S.Thing", reference.Qualified);
    Assert.Equal("S.Thing", reference.MetadataName);
    Assert.Same(type, reference.Symbol);
  }

  [Fact]
  public void DetachingKeepsTheFactsAndDropsTheSymbol() {
    var (facts, type) = Compile(Source, "S.Holder");
    var field = type.GetMembers("Items").OfType<IFieldSymbol>().Single();

    PureTypeRef pure = facts.Ref(field.Type); // implicit, one way only
    Assert.True(pure.Exists);
    Assert.True(pure.IsClass);
    Assert.Equal("global::System.Collections.Generic.List<global::S.Thing>", pure.Qualified);
    Assert.Equal("global::S.Thing", pure.TypeArguments.Single().Qualified);
  }

  [Fact]
  public void DetachedRefsAreEquatableAcrossDetachments() {
    var (facts, type) = Compile(Source, "S.Thing");
    Assert.Equal(facts.Detach(type), facts.Detach(type));
    Assert.True(facts.Detach(type) == facts.Ref(type).Pure());
  }

  [Fact]
  public void DetachedRefsOfDifferentTypesDiffer() {
    var (facts, _) = Compile(Source, "S.Thing");
    var (_, other) = Compile(Source, "S.Base");
    Assert.NotEqual(facts.Detach(facts.Ref(other).Symbol), default);
  }

  [Fact]
  public void CarriesPointerAndEnumFacts() {
    var (facts, holder) = Compile(Source, "S.Holder");
    var mode = holder.GetMembers("Mode").OfType<IFieldSymbol>().Single();
    var count = holder.GetMembers("Count").OfType<IFieldSymbol>().Single();

    Assert.True(facts.Detach(mode.Type).IsEnum);
    Assert.True(facts.Detach(count.Type).IsPrimitive);

    var (pointerFacts, withPointer) = Compile(Source, "S.WithPointer");
    Assert.True(pointerFacts.Detach(withPointer).ContainsPointer);
  }

  [Fact]
  public void MissingSymbolsDetachToNothingRatherThanThrowing() {
    var (facts, _) = Compile(Source, "S.Thing");
    Assert.False(facts.Detach(null).Exists);
    Assert.False(facts.Ref(null).Exists);
    Assert.Equal("", facts.Ref(null).ToString());
  }

  [Fact]
  public void FactsAreMemoized() {
    var (facts, type) = Compile(Source, "S.Thing");
    facts.Detach(type);
    var missesAfterFirst = facts.Misses;
    facts.Detach(type);
    facts.Detach(type);
    Assert.Equal(missesAfterFirst, facts.Misses);
    Assert.True(facts.Hits > 0);
  }
}
