using System.Linq;
using Hix;
using Hix.Compiler;
using NUnit.Framework;

namespace HelixRider.Tests.MixinLanguage;

[TestFixture]
public sealed class MixinEditorModelTests {
    [Test]
    public void EditorAnalysisRetainsValidDeclarationsAroundIncompleteMetadata() {
        var analysis = new LanguageAnalysis("type Existing = string\n%", recoverValidDeclarations: true);

        Assert.That(analysis.Program.Diagnostics, Is.Not.Empty);
        Assert.That(analysis.Declarations.Any(item =>
            item.Kind == "Pattern" && item.Name == "Existing"), Is.True);
    }

    [Test]
    public void CompilerAnalysisRemainsStrictAroundIncompleteMetadata() {
        var analysis = new LanguageAnalysis("type Existing = string\n%");

        Assert.That(analysis.Program.Diagnostics, Is.Not.Empty);
        Assert.That(analysis.Declarations, Is.Empty);
    }

    [Test]
    public void EditorAnalysisRetainsDeclarationsBelowIncompleteHeaderMetadata() {
        var analysis = new LanguageAnalysis(
            "%\n---\ntype Existing = string\nmixin Example { expression { emit(<ok>) } }",
            recoverValidDeclarations: true);

        Assert.That(analysis.Declarations.Any(item =>
            item.Kind == "Pattern" && item.Name == "Existing"), Is.True);
        Assert.That(analysis.Declarations.Any(item =>
            item.Kind == "Mixin" && item.Name == "Example"), Is.True);
    }

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
        var matches = Hix.HixMixinBackend.Instance.Functions.Enumerate().Single(item => item.Name == "matches");
        Assert.That(matches.ArgumentTypes, Is.EqualTo(new[] {HixValueKind.String, HixValueKind.String}));
        Assert.That(matches.ResultType, Is.EqualTo(HixValueKind.Bool));
        Assert.That(Hix.HixMixinBackend.Instance.Functions.Enumerate().Any(item => item.Name == "format"), Is.False);
    }

    [Test]
    public void AnalysisDocumentsHostFunctionDespiteIncompleteArguments() {
        var analysis = new LanguageAnalysis(
            "mixin Example { expression { inject() } }",
            Hix.HixMixinBackend.Instance);

        Assert.That(analysis.TypeFacts.Any(fact =>
            fact.Kind == "Call" && fact.Documentation.Contains("inject(")), Is.True);
    }
}
