using System.IO;
using System.Linq;
using MixinLanguage;
using MixinLanguage.Analysis;
using Xunit;

namespace HELIX.SourceGen.Tests;

public sealed class MixinRepositoryAnalysisTests {
  [Fact]
  public void EditorCatalogUsesRuntimeTypeSignatures() {
    var members = FunctionLibrary.Enumerate().Single(item => item.Name == "members");
    Assert.Equal(MixinLanguageValueKind.Type, members.ReceiverType);
    Assert.Equal(MixinLanguageValueKind.Table, members.ResultType);

    var reduce = FunctionLibrary.Enumerate().Single(item => item.Name == "reduce");
    Assert.Equal(MixinLanguageValueKind.Table, reduce.ReceiverType);
    Assert.Equal([
      MixinLanguageValueKind.Any, MixinLanguageValueKind.Function
    ], reduce.ArgumentTypes);

    var makeGeneric = FunctionLibrary.Enumerate().Single(item => item.Name == "makeGeneric");
    Assert.Equal(MixinLanguageValueKind.Type, makeGeneric.ResultType);
    Assert.Equal([MixinLanguageValueKind.CSharpType], makeGeneric.ArgumentTypes);

    var annotation = DirectiveLibrary.EnumerateLanguageDefinitions()
      .Single(item => item.Name == "ANNOTATION");
    Assert.Equal([MixinLanguageValueKind.CSharpType], annotation.ArgumentTypes);
  }

  [Fact]
  public void EveryRepositoryMixinFileProducesAnEditorSnapshot() {
    var root = new DirectoryInfo(Directory.GetCurrentDirectory());
    while (root is not null && !Directory.Exists(Path.Combine(
      root.FullName, "src", "HELIX", "Assets", "Mixins"
    ))) root = root.Parent;
    Assert.NotNull(root);

    var files = Directory.GetFiles(
      Path.Combine(root!.FullName, "src", "HELIX", "Assets", "Mixins"),
      "*.HelixSourceGenerator.additionalfile"
    );
    Assert.NotEmpty(files);

    var snapshots = files.Select(path => MixinEditorAnalyzer.Analyze(File.ReadAllText(path))).ToArray();
    var completionSites = files.Select(path =>
      MixinEditorAnalyzer.GetCompletionSites(File.ReadAllText(path))).ToArray();
    Assert.All(snapshots, snapshot => Assert.NotNull(snapshot.Syntax.Root));
    Assert.True(snapshots.Sum(snapshot => snapshot.Syntax.Root.Children.Count) > 100);
    Assert.True(completionSites.Sum(sites => sites.Count) > 100);
  }
}
