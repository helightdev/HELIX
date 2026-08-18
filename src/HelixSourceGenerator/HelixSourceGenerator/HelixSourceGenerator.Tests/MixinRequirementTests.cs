using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using HELIX.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace HELIX.SourceGen.Tests;

public sealed class MixinRequirementTests {
  [Fact]
  public void ImplicitInterfaceRequirementAddsInterfaceAndItsExpression() {
    var result = Run(
      """
      using System;
      namespace HELIX.Context {
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)] public sealed class EnableMixinsAttribute : Attribute { }
        [AttributeUsage(AttributeTargets.Interface)] public sealed class MixinAttribute : Attribute { }
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
        public sealed class RequireMixinAttribute : Attribute {
          public RequireMixinAttribute(Type target, bool declareImplicit = false) { }
        }
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
        public sealed class MixinExpressionAttribute : Attribute {
          public MixinExpressionAttribute(string target, int order, string expression) { }
        }
        [Mixin, EnableMixins] public interface IMixin { }
      }
      [HELIX.Context.MixinExpression("$Init", 0, "@CODE Required()")]
      public interface IBaseMixin : HELIX.Context.IMixin { }
      [HELIX.Context.RequireMixin(typeof(IBaseMixin), true)]
      public interface IFeatureMixin : HELIX.Context.IMixin { }
      [HELIX.Context.EnableMixins]
      public partial class Demo : IFeatureMixin {
        private void Required() { }
      }
      """
    );

    Assert.Empty(result.Diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    var generated = Assert.Single(result.GeneratedSources).SourceText.ToString();
    Assert.Contains("partial class Demo : global::IBaseMixin", generated);
    Assert.Contains("Required();", generated);
  }

  [Fact]
  public void MissingExplicitRequirementReportsDiagnostic() {
    var result = Run(
      """
      using System;
      namespace HELIX.Context {
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)] public sealed class EnableMixinsAttribute : Attribute { }
        [AttributeUsage(AttributeTargets.Interface)] public sealed class MixinAttribute : Attribute { }
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
        public sealed class RequireMixinAttribute : Attribute {
          public RequireMixinAttribute(Type target, bool declareImplicit = false) { }
        }
        [Mixin, EnableMixins] public interface IMixin { }
      }
      public interface IBaseMixin : HELIX.Context.IMixin { }
      [HELIX.Context.RequireMixin(typeof(IBaseMixin))]
      public interface IFeatureMixin : HELIX.Context.IMixin { }
      [HELIX.Context.EnableMixins]
      public partial class Demo : IFeatureMixin { }
      """
    );

    Assert.Contains(
      result.Diagnostics,
      item => item.Id == "HLXM09" &&
        item.GetMessage().Contains("IBaseMixin")
    );
  }

  [Fact]
  public void ExplicitInterfaceRequirementAcceptsAnAlreadyDeclaredMixin() {
    var result = Run(
      """
      using System;
      namespace HELIX.Context {
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)] public sealed class EnableMixinsAttribute : Attribute { }
        [AttributeUsage(AttributeTargets.Interface)] public sealed class MixinAttribute : Attribute { }
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
        public sealed class RequireMixinAttribute : Attribute {
          public RequireMixinAttribute(Type target, bool declareImplicit = false) { }
        }
        [Mixin, EnableMixins] public interface IMixin { }
      }
      public interface IBaseMixin : HELIX.Context.IMixin { }
      [HELIX.Context.RequireMixin(typeof(IBaseMixin))]
      public interface IFeatureMixin : HELIX.Context.IMixin { }
      [HELIX.Context.EnableMixins]
      public partial class Demo : IFeatureMixin, IBaseMixin { }
      """
    );

    Assert.DoesNotContain(result.Diagnostics, item => item.Id == "HLXM09");
  }

  [Fact]
  public void ImplicitAttributeRequirementUsesDefaultConstructorValues() {
    var result = Run(
      """
      using System;
      namespace HELIX.Context {
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)] public sealed class EnableMixinsAttribute : Attribute { }
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
        public sealed class RequireMixinAttribute : Attribute {
          public RequireMixinAttribute(Type target, bool declareImplicit = false) { }
        }
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
        public sealed class MixinExpressionAttribute : Attribute {
          public MixinExpressionAttribute(string target, int order, string expression) { }
        }
      }
      [HELIX.Context.MixinExpression("$Init", 0, "@CODE Required(@attr#value)")]
      [AttributeUsage(AttributeTargets.Class)]
      public sealed class RequiredAttribute : Attribute {
        public RequiredAttribute(int value = 42) { }
      }
      [HELIX.Context.MixinExpression("$Dispose", 0, "@CODE Triggered()")]
      [HELIX.Context.RequireMixin(typeof(RequiredAttribute), true)]
      [AttributeUsage(AttributeTargets.Class)]
      public sealed class TriggerAttribute : Attribute { }
      [HELIX.Context.EnableMixins, Trigger]
      public partial class Demo {
        private void Required(int value) { }
        private void Triggered() { }
      }
      """
    );

    Assert.Empty(result.Diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    var generated = Assert.Single(result.GeneratedSources).SourceText.ToString();
    Assert.Contains("[global::RequiredAttribute]", generated);
    Assert.Contains("Required(42);", generated);
    Assert.Contains("Triggered();", generated);
  }

  private static GeneratorRunResult Run(string source) {
    var compilation = CSharpCompilation.Create(
      "RequirementTest",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = CSharpGeneratorDriver.Create(new MixinGenerator());
    return Assert.Single(driver.RunGenerators(compilation).GetRunResult().Results);
  }

  private static ImmutableArray<MetadataReference> PlatformReferences { get; } =
    ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))!
    .Split(Path.PathSeparator)
    .Select(path => MetadataReference.CreateFromFile(path))
    .ToImmutableArray<MetadataReference>();
}
