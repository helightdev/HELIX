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
  public void DatatypeOptInGeneratesConfigurableStructureDatatype() {
    var result = Run(
      Runtime +
      """
      namespace Feature {
        public enum Mode { First, Second }

        [Feature.ConfigureSettings]
        [HELIX.EnableMixins, HELIX.Structure(datatype: true)]
        public partial struct Settings {
          [Feature.ConfigureProperty]
          public int count;
          public Mode mode;

          [HELIX.Prop(18)]
          public int age;

          [HELIX.Prop(null, Datatype = "Custom.Text")]
          public string label;

        }

        public static class Custom {
          public static readonly HELIX.IDatatype<string> Text = HELIX.Datatypes.String;
        }

        [HELIX.MixinExpression("@CODE datatype.Configured = true")]
        [AttributeUsage(AttributeTargets.Struct)]
        public sealed class ConfigureSettingsAttribute : Attribute { }

        [HELIX.MixinExpression("@CODE datatype.ConfiguredProperties.Add(\"@target:name\")")]
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter)]
        public sealed class ConfigurePropertyAttribute : Attribute { }
      }
      """
    );

    Assert.Empty(result.Diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    Assert.Empty(result.OutputDiagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    Assert.Contains(
      "public static readonly global::HELIX.StructureDatatype<global::Feature.Settings> Datatype =",
      result.Generated
    );
    Assert.Contains("new global::HELIX.ConfigurableStructureDatatype<global::Feature.Settings>(", result.Generated);
    Assert.Contains("global::HELIX.Datatypes.Int,", result.Generated);
    Assert.Contains("global::HELIX.Datatypes.Enum<global::Feature.Mode>(),", result.Generated);
    Assert.Contains("Custom.Text,", result.Generated);
    Assert.Contains("datatype => ConfigureDatatype(datatype)", result.Generated);
    Assert.Contains("datatype.Configured = true;", result.Generated);
    Assert.Contains("datatype.ConfiguredProperties.Add(\"count\");", result.Generated);
    Assert.Contains("value.count = propertyValue", result.Generated);
    Assert.Contains("value.label = propertyValue", result.Generated);
    Assert.Contains("required: true,\n            defaultValue: null", result.Generated);
    Assert.Contains("required: false,\n            defaultValue: 18", result.Generated);
    Assert.Contains("required: false,\n            defaultValue: null", result.Generated);
  }

  [Fact]
  public void DatatypeIsNotGeneratedWithoutOptIn() {
    var result = Run(
      Runtime +
      """
      [HELIX.EnableMixins, HELIX.Structure]
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
      [HELIX.EnableMixins, HELIX.Structure(datatype: true)]
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
      [HELIX.EnableMixins, HELIX.Structure]
      public partial struct Settings { }
      """
    );

    Assert.Empty(result.Diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    Assert.Empty(result.OutputDiagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    Assert.DoesNotContain(" Settings(", result.Generated);
  }

  private void DatatypeMixinsImportPreparedFunctionLibraryFromMixinAttribute() {
    var result = Run(
      Runtime +
      """
      [HELIX.MixinLibrary("@FUNC<configure>\n@CODE datatype.Configured = true\n@END")]
      public static class DatatypeFunctions { }

      [HELIX.MixinImport(typeof(DatatypeFunctions))]
      [HELIX.MixinExpression("@CALL<configure>")]
      [AttributeUsage(AttributeTargets.Struct)]
      public sealed class ConfigureFromLibraryAttribute : Attribute { }

      [ConfigureFromLibrary]
      [HELIX.EnableMixins, HELIX.Structure(datatype: true)]
      public partial struct Settings {
        public int count;
      }
      """
    );

    Assert.Empty(result.Diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    Assert.Empty(result.OutputDiagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    Assert.Contains("datatype.Configured = true;", result.Generated);
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
          "/tests/Core.HelixSourceGenerator.additionalfile",
          "@ANNOTATION<HELIX.StructureAttribute>\n" +
          "@PRELUDE\n@AUGMENT_STRUCT<PropsModel> @this\n@END\n@END"
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
                                   public sealed class EnableMixinsAttribute : Attribute { }
                                   [AttributeUsage(AttributeTargets.Struct)]
                                   public sealed class StructureAttribute : Attribute {
                                     public StructureAttribute(bool datatype = false) { }
                                   }
                                   [AttributeUsage(AttributeTargets.Field)]
                                   public sealed class PropAttribute : Attribute {
                                     public PropAttribute(object value) { }
                                     public string Datatype { get; set; }
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
