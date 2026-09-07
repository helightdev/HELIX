using System.Linq;
using Mixins;
using Mixins.Compiler;
using NUnit.Framework;

namespace HelixRider.Tests.MixinLanguage;

[TestFixture]
public sealed class MixinEditorModelTests {
    [Test]
    public void CanonicalAnalysisUsesAntlrTokensAndSemanticAst() {
        const string source = "mixin Example {\n  pure func Build { return(<ok>) }\n  expression { emit(Build()) }\n}";
        var analysis = new LanguageAnalysis(source);

        Assert.That(analysis.Program.Diagnostics, Is.Empty);
        Assert.That(string.Concat(analysis.Program.Tokens.Select(token => token.Text)), Is.EqualTo(source));
        Assert.That(analysis.Declarations.Any(item => item.Kind == "Mixin" && item.Name == "Example"), Is.True);
        Assert.That(analysis.Declarations.Any(item => item.Kind == "Function" && item.Name == "Build"), Is.True);
        Assert.That(analysis.References.Any(item => item.Kind == "Function" && item.Name == "Build"), Is.True);
    }

    [Test]
    public void AnalysisFindsStorageAndLabelReferences() {
        const string source = "mixin Example { expression { local value = <ok>; emit(local#value); goto done; :done\n } }";
        var analysis = new LanguageAnalysis(source);

        Assert.That(analysis.Program.Diagnostics, Is.Empty);
        Assert.That(analysis.Declarations.Any(item => item.Kind == "Local" && item.Name == "value"), Is.True);
        Assert.That(analysis.References.Any(item => item.Kind == "Local" && item.Name == "value"), Is.True);
        Assert.That(analysis.Declarations.Any(item => item.Kind == "Label" && item.Name == "done"), Is.True);
        Assert.That(analysis.References.Any(item => item.Kind == "Label" && item.Name == "done"), Is.True);
    }

    [Test]
    public void CatalogContainsFlatDefinitionSignatures() {
        var matches = FunctionLibrary.Enumerate().Single(item => item.Name == "matches");
        Assert.That(matches.ArgumentTypes, Is.EqualTo(new[] {MixinValueKind.String, MixinValueKind.String}));
        Assert.That(matches.ResultType, Is.EqualTo(MixinValueKind.Bool));
        Assert.That(FunctionLibrary.Enumerate().Any(item => item.Name == "format"), Is.False);
    }
}
