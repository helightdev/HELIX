using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace HELIX.SourceGen.Tests;

public sealed class MixinGeneratorExpressionTests {

  [Fact]
  public void LateBackendPreservesPreparedInjectionIdentity() {
    var context = new Hix.Runtime.HixExecutionContext(Hix.HixMixinBackend.Instance);
    Assert.Equal("$Recompose", context.ResolveInjectionTarget("$Recompose"));
    Assert.Equal("^*Apply:SomeDelegate", context.ResolveInjectionTarget("^*Apply:SomeDelegate"));
  }

  [Fact]
  public void DerivationReadsAttributeDefaultsFromCompilationReferences() {
    const string runtimeSource = """
                                 using System;
                                 namespace HELIX {
                                   [AttributeUsage(AttributeTargets.Struct)]
                                   public sealed class MixableAttribute : Attribute { }
                                   [AttributeUsage(AttributeTargets.Struct)]
                                   public sealed class StructureAttribute : Attribute {
                                     public StructureAttribute(bool datatype = false) { }
                                   }
                                   public enum PropInit { Literal, Constant, Deferred, None }
                                   [AttributeUsage(AttributeTargets.Field)]
                                   public sealed class PropAttribute : Attribute {
                                     public object defaultValue;
                                     public PropInit defaultInit;
                                     public PropAttribute(object value, PropInit mode = PropInit.Literal) {
                                       defaultValue = value;
                                       defaultInit = mode;
                                     }
                                     public bool Equatable { get; set; } = true;
                                     public string EqualitySyntax { get; set; } = "{0} == {1}";
                                     public string HashCodeSyntax { get; set; } = "{0}";
                                     public string Datatype { get; set; }
                                   }
                                   public sealed class PropertyDatatypeAttribute : Attribute {
                                     public PropertyDatatypeAttribute(string datatype) { }
                                   }
                                 }
                                 """;
    var parseOptions = new CSharpParseOptions(LanguageVersion.Latest);
    var runtime = CSharpCompilation.Create(
      "ReferencedHelixRuntime",
      [CSharpSyntaxTree.ParseText(runtimeSource, parseOptions, "/runtime/Annotations.cs")],
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    Assert.Empty(runtime.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
    var compilation = CSharpCompilation.Create(
      "ReferencedAttributeDefaultsTest",
      [CSharpSyntaxTree.ParseText(
        "[HELIX.Mixable, HELIX.Structure] public partial struct Demo : System.IEquatable<Demo> { " +
        "[HELIX.Prop(0)] public int Value; }",
        parseOptions
      )],
      PlatformReferences.Add(runtime.ToMetadataReference()),
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    var path = Path.GetFullPath(Path.Combine(
      AppContext.BaseDirectory,
      "../../../../HELIX/Assets/Mixins/Core.HelixSourceGenerator.additionalfile"
    ));
    GeneratorDriver driver = MixinTestDriver.Create(
      compilation, additionalTexts: [new TestAdditionalText(path, File.ReadAllText(path))]
    );

    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out var diagnostics);

    Assert.DoesNotContain(diagnostics, item => item.Id == "HLXM08");
    var generated = Assert.Single(driver.GetRunResult().Results.SelectMany(item => item.GeneratedSources))
      .SourceText.ToString();
    Assert.Contains("this.Value == other.Value", generated);
    Assert.Contains("hashCode.Add(this.Value)", generated);
  }

  [Fact]
  public void HixDebugConfigurationDoesNotPolluteGeneratedSource() {
    const string source = """
                          using System;
                          namespace HELIX {
                            [AttributeUsage(AttributeTargets.Class)] public sealed class MixableAttribute : Attribute { }
                          }
                          public sealed class DebugAttribute : Attribute { }
                          [HELIX.Mixable, Debug] public partial class Demo { }
                          """;
    var compilation = CSharpCompilation.Create(
      "HixDebugConfigurationTest",
      [CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest))],
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = MixinTestDriver.Create(compilation, additionalTexts: [
      new TestAdditionalText("/project/Debug.HelixSourceGenerator.additionalfile", """
        mixin Configuration {
          prelude expression { config(<DEBUG>, <enabled>) }
        }
        mixin DebugAttribute {
          expression { local DebugValue = 1 }
        }
        """)
    ]);

    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out var diagnostics);

    Assert.Empty(diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    var generated = Assert.Single(Assert.Single(driver.GetRunResult().Results).GeneratedSources)
      .SourceText.ToString();
    Assert.DoesNotContain("HELIX MIXIN PROGRAM DUMP", generated);
    Assert.DoesNotContain("PRELUDE BYTECODE", generated);
    Assert.DoesNotContain(".constant", generated);
    Assert.DoesNotContain("local DebugValue = 1", generated);
  }

  private static ImmutableArray<MetadataReference> PlatformReferences { get; } =
    ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))!
    .Split(Path.PathSeparator)
    .Select(path => MetadataReference.CreateFromFile(path))
    .ToImmutableArray<MetadataReference>();

}

internal sealed class TestAdditionalText : AdditionalText {
  private readonly SourceText _text;
  internal TestAdditionalText(string path, string text) {
    Path = path;
    _text = SourceText.From(text);
  }
  public override string Path { get; }
  public override SourceText GetText(System.Threading.CancellationToken cancellationToken = default) => _text;
}
