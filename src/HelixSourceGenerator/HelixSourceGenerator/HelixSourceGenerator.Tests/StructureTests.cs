using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using HELIX.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace HELIX.SourceGen.Tests;

public sealed class StructureTests {

  [Fact]
  public void DatatypeIsNotGeneratedWithoutOptIn() {
    var result = Run(
      Runtime +
      """
      [HELIX.Mixable, HELIX.Structure]
      public partial struct Settings {
        public int count;
      }
      """
    );

    Assert.Empty(result.Diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    Assert.Empty(result.OutputDiagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    Assert.DoesNotContain("StructureDatatype", result.Generated);
    Assert.DoesNotContain(" Datatype =", result.Generated);
  }

  [Fact]
  public void DatatypeConfigurationIsOptional() {
    var result = Run(
      Runtime +
      """
      [HELIX.Mixable, HELIX.Structure(datatype: true)]
      public partial struct Settings {
        public int count;
      }
      """
    );

    Assert.Empty(result.Diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    Assert.Empty(result.OutputDiagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    Assert.Contains("static void ConfigureDatatype", result.Generated);
  }

  [Fact]
  public void EmptyStructureDoesNotGenerateConstructor() {
    var result = Run(
      Runtime +
      """
      [HELIX.Mixable, HELIX.Structure]
      public partial struct Settings { }
      """
    );

    Assert.Empty(result.Diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    Assert.Empty(result.OutputDiagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    Assert.DoesNotContain(" Settings(", result.Generated);
  }

  [Fact]
  public void StructureDerivationPreservesCustomEqualityAndHashFormats() {
    var result = Run(
      Runtime +
      """
      [HELIX.Mixable, HELIX.Structure]
      public partial struct Settings : System.IEquatable<Settings> {
        [HELIX.Prop(null,
          EqualitySyntax = "global::System.String.Equals({0}, {1})",
          HashCodeSyntax = "{0}.Length")]
        public string text;
      }
      """
    );

    Assert.Empty(result.Diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    Assert.Empty(result.OutputDiagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    Assert.Contains("global::System.String.Equals(this.text, other.text)", result.Generated);
    Assert.Contains("hashCode.Add(this.text.Length)", result.Generated);
  }

  [Fact]
  public void StructureDerivationDoesNotDuplicateExistingEqualityMembers() {
    var result = Run(
      Runtime +
      """
      [HELIX.Mixable, HELIX.Structure]
      public partial struct Settings : System.IEquatable<Settings> {
        public int value;
        public bool Equals(Settings other) => value == other.value;
        public override bool Equals(object obj) => obj is Settings other && Equals(other);
        public override int GetHashCode() => value;
      }
      """
    );

    Assert.Empty(result.Diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    Assert.Empty(result.OutputDiagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    Assert.DoesNotContain("bool Equals", result.Generated);
    Assert.DoesNotContain("GetHashCode", result.Generated);
  }

  private static TestResult Run(string source) {
    var compilation = CSharpCompilation.Create(
      "FeatureAssembly",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = MixinTestDriver.Create(
      compilation,
      additionalTexts: new AdditionalText[] {
        new TestAdditionalText(
          "/tests/Core.HelixSourceGenerator.additionalfile", File.ReadAllText(Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../../../HELIX/Assets/Mixins/Core.HelixSourceGenerator.additionalfile"
          )))
        )
      }
    );
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);
    var run = driver.GetRunResult();
    var generated = Assert.Single(run.Results.SelectMany(item => item.GeneratedSources)).SourceText.ToString();
    return new TestResult(generated, diagnostics.AddRange(run.Diagnostics), output.GetDiagnostics());
  }

  private sealed record TestResult(
    string Generated,
    ImmutableArray<Diagnostic> Diagnostics,
    ImmutableArray<Diagnostic> OutputDiagnostics
  );

  private const string Runtime = """
                                 using System;
                                 using System.Collections.Generic;
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
                                     public PropAttribute(object defaultValue, PropInit defaultInit = PropInit.Literal) {
                                       this.defaultValue = defaultValue;
                                       this.defaultInit = defaultInit;
                                     }
                                     public PropAttribute() { defaultInit = PropInit.None; }
                                     public bool Equatable { get; set; } = true;
                                     public string EqualitySyntax { get; set; } = "{0} == {1}";
                                     public string HashCodeSyntax { get; set; } = "{0}";
                                     public string Datatype { get; set; }
                                   }
                                   public sealed class PropertyDatatypeAttribute : Attribute {
                                     public PropertyDatatypeAttribute(string datatype) { }
                                   }
                                   [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
                                   public sealed class MixinExpressionAttribute : Attribute {
                                     public MixinExpressionAttribute(string expression) { }
                                     public MixinExpressionAttribute(string target, int order, string expression) { }
                                     public MixinExpressionAttribute(string[] target, int[] order, string expression) { }
                                   }
                                   [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
                                   public sealed class MixinImportAttribute : Attribute {
                                     public MixinImportAttribute(Type library) { }
                                   }
                                   [AttributeUsage(AttributeTargets.Class)]
                                   public sealed class MixinLibraryAttribute : Attribute {
                                     public MixinLibraryAttribute(string content) { }
                                   }
                                   [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
                                   public sealed class MixinDefineTargetAttribute : Attribute {
                                     public MixinDefineTargetAttribute(string key, string target) { }
                                   }
                                   public interface IDatatype<T> { }
                                   public sealed class PrimitiveDatatype<T> : IDatatype<T> { }
                                   public static class Datatypes {
                                     public static readonly IDatatype<string> String = new PrimitiveDatatype<string>();
                                     public static readonly IDatatype<int> Int = new PrimitiveDatatype<int>();
                                     public static IDatatype<T> Enum<T>() where T : struct, Enum => new PrimitiveDatatype<T>();
                                     public static IDatatype<T> Object<T>() => new PrimitiveDatatype<T>();
                                   }
                                   public abstract class StructurePropertyDatatype<T> { }
                                   public sealed class StructurePropertyDatatype<T, TValue> : StructurePropertyDatatype<T> {
                                     public delegate TValue Getter(ref T value);
                                     public delegate void Setter(ref T value, TValue propertyValue);
                                     public StructurePropertyDatatype(
                                       string name, IDatatype<TValue> datatype, Getter getter, Setter setter,
                                       IList<object> modifiers = null, bool required = true,
                                       object defaultValue = null
                                     ) { }
                                   }
                                   public sealed class StructureDatatype<T> {
                                     public StructureDatatype(
                                       string name, IList<StructurePropertyDatatype<T>> properties
                                     ) { }
                                     public bool Configured { get; set; }
                                     public List<string> ConfiguredProperties { get; } = new List<string>();
                                   }
                                   public sealed class ConfigurableStructureDatatype<T> {
                                     private readonly StructureDatatype<T> datatype;
                                     public ConfigurableStructureDatatype(
                                       StructureDatatype<T> datatype, Action<StructureDatatype<T>> configure
                                     ) {
                                       this.datatype = datatype;
                                       configure(datatype);
                                     }
                                     public StructureDatatype<T> Datatype => datatype;
                                   }
                                 }
                                 """;

  private static ImmutableArray<MetadataReference> PlatformReferences { get; } =
    ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))!
    .Split(Path.PathSeparator)
    .Select(path => MetadataReference.CreateFromFile(path))
    .ToImmutableArray<MetadataReference>();
}
