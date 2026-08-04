using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static HELIX.SourceGen.GeneratorAnalysis;
using static HELIX.SourceGen.GeneratorDiagnostics.PropStruct;
using static HELIX.SourceGen.GeneratorSource;
using static HELIX.SourceGen.GeneratorStrings;

using StringBuilder = System.Text.StringBuilder;

namespace HELIX.SourceGen {
  [Generator(LanguageNames.CSharp)]
  public sealed class PropStructGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
      var structs = context.SyntaxProvider.ForAttributeWithMetadataName(
        Attributes.PropStruct,
        predicate: static (node, _) => node is StructDeclarationSyntax,
        transform: static (ctx, _) => (INamedTypeSymbol)ctx.TargetSymbol
      );
      context.RegisterSourceOutput(structs, static (production, type) => Generate(production, type));
    }

    private static void Generate(SourceProductionContext context, INamedTypeSymbol type) {
      var location = LocationOf(type);
      if (!IsPartial(type)) {
        context.ReportDiagnostic(Diagnostic.Create(MustBePartial, location, type.Name));
        return;
      }

      var invalidContainer = FirstNonPartialContainingType(type);
      if (invalidContainer is not null) {
        context.ReportDiagnostic(
          Diagnostic.Create(ContainingTypeMustBePartial, location, type.Name, invalidContainer.Name)
        );
        return;
      }

      if (!PropStructApi.TryAnalyze(type, out var props, out var diagnostic)) {
        context.ReportDiagnostic(diagnostic);
        return;
      }

      var wrapper = WrapType(type, "prop-struct", CollectUsings(type));
      var indent = new string(' ', wrapper.MemberDepth * 2);
      var parameterIndent = indent + "  ";
      var body = new StringBuilder()
        .Append('\n').Append(indent)
        .Append(AccessibilityText(type.DeclaredAccessibility))
        .Append(props.RequiresUnsafe ? " unsafe " : " ")
        .Append(EscapeIdentifier(type.Name)).Append('(');

      if (props.ParameterParts.Count > 0) {
        body.Append('\n').Append(parameterIndent)
          .Append(string.Join(",\n" + parameterIndent, props.ParameterParts))
          .Append('\n').Append(indent);
      }
      body.Append(") {\n")
        .Append(props.RenderAssignments("this", indent + "  "))
        .Append(indent).Append("}\n");

      context.AddSource(wrapper.HintName, wrapper.Enclose(body.ToString()));
    }
  }
}