using System.IO;
using System.Linq;
using HelixRider.Protocol;
using NUnit.Framework;

namespace HelixRider.Tests.MixinLanguage;

[TestFixture]
public sealed class HelixMixinProtocolHostTests
{
    [Test]
    public void DirectoryFunctionResolutionRemovesSingleFileUnresolvedDiagnostic()
    {
        var response = HelixMixinLanguageHost.Parse(new MixinParseRequest(new[]
        {
            new MixinFileInput("/project/Mixins/Library.HelixSourceGenerator.additionalfile",
                "@FUNC<Build>\n@RETURN @null\n@END\n", 1),
            new MixinFileInput("/project/Mixins/Use.HelixSourceGenerator.additionalfile",
                "@CALL<Build> value\n", 1)
        }));

        var use = response.Files.Single(file => file.FilePath.EndsWith("Use.HelixSourceGenerator.additionalfile"));
        Assert.That(use.References.Single(reference => reference.Name == "Build").TargetFilePath,
            Does.EndWith("Library.HelixSourceGenerator.additionalfile"));
        Assert.That(use.Diagnostics.Any(diagnostic => diagnostic.Message.Contains("unresolved function")),
            Is.False);
    }

    [Test]
    public void DirectoryDuplicateFunctionsAreReportedAtEveryDeclaration()
    {
        var response = HelixMixinLanguageHost.Parse(new MixinParseRequest(new[]
        {
            new MixinFileInput("/project/Mixins/A.HelixSourceGenerator.additionalfile",
                "@FUNC<Build>\n@END\n", 1),
            new MixinFileInput("/project/Mixins/B.HelixSourceGenerator.additionalfile",
                "@FUNC<Build>\n@END\n", 1)
        }));

        Assert.That(response.Files, Has.All.Matches<MixinFileSnapshot>(file =>
            file.Diagnostics.Any(diagnostic => diagnostic.Message ==
                "duplicate function 'Build' in mixin directory")));
    }

    [Test]
    public void RepositoryMixinBatchCanBeSerialized()
    {
        var root = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (root != null && !Directory.Exists(Path.Combine(root.FullName, "src", "HELIX", "Assets", "Mixins")))
            root = root.Parent;
        Assert.That(root, Is.Not.Null, "could not locate the repository root");

        var directory = Path.Combine(root!.FullName, "src", "HELIX", "Assets", "Mixins");
        var files = Directory.GetFiles(directory, "*.HelixSourceGenerator.additionalfile")
            .OrderBy(path => path)
            .Select((path, index) => new MixinFileInput(path, File.ReadAllText(path), index + 1))
            .ToArray();

        var response = HelixMixinLanguageHost.Parse(new MixinParseRequest(files));

        Assert.That(response.Files, Has.Length.EqualTo(files.Length));
        Assert.That(HelixMixinLanguageHost.LanguageCatalog().Definitions, Is.Not.Empty);
        Assert.That(response.Files.Sum(file => file.Declarations.Length), Is.GreaterThan(10));
        Assert.That(response.Files.Sum(file => file.References.Length), Is.GreaterThan(10));
    }
}
