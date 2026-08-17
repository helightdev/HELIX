using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static HELIX.SourceGen.GeneratorAnalysis;
using static HELIX.SourceGen.GeneratorSource;
using static HELIX.SourceGen.GeneratorStrings;

namespace HELIX.SourceGen;

[Generator(LanguageNames.CSharp)]
public sealed class BoundaryVisualElementGenerator : IIncrementalGenerator {
  public void Initialize(IncrementalGeneratorInitializationContext context) {
    var elements = context.SyntaxProvider.ForAttributeWithMetadataName(
      Attributes.UxmlElement,
      static (node, _) => node is ClassDeclarationSyntax,
      static (ctx, _) => (INamedTypeSymbol)ctx.TargetSymbol
    );

    context.RegisterSourceOutput(elements, static (spc, element) => Generate(spc, element));
  }

  private static void Generate(SourceProductionContext spc, INamedTypeSymbol element) {
    if (!InheritsFrom(element, Types.BoundaryVisualElement)) return;

    var containing = WrapType(element, "boundary-visual-element");
    var source = containing.Build(builder => {
        builder.BlankLine();
        using (builder.Block($"private static readonly global::{Types.CompositionId} _compositionId = new()", ";")) {
          builder.AppendLine($"composition = {GetCompositionId(element.Name)},")
            .AppendLine($"type = {GetTypeId(element.Name)},")
            .AppendLine($"local = global::{Types.LocalId}.Initial");
        }
        builder.BlankLine();
        using (builder.Method(
          "public override void PerformCompose",
          [$"ref global::{Types.Composition} cx"],
          false
        )) {
          builder.Statement("cx.AUTHORING.SetId(_compositionId)")
            .Statement("Compose(ref cx)");
        }
      }
    );

    spc.AddSource(containing.HintName, source);
  }
}