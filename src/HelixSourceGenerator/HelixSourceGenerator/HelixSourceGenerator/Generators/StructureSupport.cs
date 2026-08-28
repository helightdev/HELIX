using System.Linq;
using HELIX.SourceGen.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static HELIX.SourceGen.GeneratorAnalysis;
using static HELIX.SourceGen.GeneratorDiagnostics.Structure;
using static HELIX.SourceGen.GeneratorSource;
using static HELIX.SourceGen.GeneratorStrings;

namespace HELIX.SourceGen;

internal static class StructureSupport {
  internal static void Register(IncrementalGeneratorInitializationContext context) {
    var structs = context.SyntaxProvider.ForAttributeWithMetadataName(
      Attributes.Structure,
      static (node, _) => node is StructDeclarationSyntax,
      static (ctx, _) => CreateTarget(ctx)
    );

    context.RegisterSourceOutput(
      structs.Where(static target => !target.GenerateDatatype),
      static (production, target) => Generate(
        production, target.Type, null, null, false
      )
    );
    context.RegisterSourceOutput(
      structs.Where(static target => target.GenerateDatatype),
      static (production, target) => Generate(
        production,
        target.Type,
        target.Compilation,
        null,
        true
      )
    );
  }

  private static PropStructTarget CreateTarget(GeneratorAttributeSyntaxContext context) {
    var type = (INamedTypeSymbol)context.TargetSymbol;
    var attribute = Attribute(type, Attributes.Structure);
    var generateDatatype = attribute is { ConstructorArguments.Length: > 0 } &&
      attribute.ConstructorArguments[0].Value is true;
    return new PropStructTarget(
      type,
      generateDatatype ? (CSharpCompilation)context.SemanticModel.Compilation : null,
      generateDatatype
    );
  }

  private static void Generate(
    SourceProductionContext context,
    INamedTypeSymbol type,
    CSharpCompilation compilation,
    MixinExpressionPreparedState preparedExpressions,
    bool generateDatatype
  ) {
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

    if (!PropStructApi.TryAnalyze(type, out var props, out var diagnostic, generateDatatype)) {
      context.ReportDiagnostic(diagnostic);
      return;
    }

    var mixins = generateDatatype
      ? PropStructMixinApi.Analyze(
        context, type, props.PropertySymbols, compilation,
        MixinLibraryApi.Prepare(
          context,
          MixinLibraryApi.AttributeOwners(
            new ISymbol[] { type }.Concat(props.PropertySymbols)
          )
        )
      )
      : new PropStructMixinModel();
    var usings = CollectUsings(type).Concat(mixins.Usings).Distinct().ToArray();
    var wrapper = WrapType(
      type, "prop-struct", usings,
      baseType: mixins.Implements.Count == 0 ? null : string.Join(", ", mixins.Implements),
      typeAttributes: mixins.Annotations
    );
    var source = wrapper.Build(builder => {
        if (props.ParameterParts.Count > 0) {
          builder.BlankLine();
          using (builder.Method(
            AccessibilityText(type.DeclaredAccessibility) +
            (props.RequiresUnsafe ? " unsafe " : " ") +
            EscapeIdentifier(type.Name),
            props.ParameterParts,
            true
          )) props.AppendAssignments(builder, "this");
        }
        props.Equality.AppendMembers(builder);
        if (props.Datatype is not null) {
          builder.BlankLine();
          props.Datatype.AppendMember(builder, mixins.Configuration);
        }
        foreach (var output in mixins.Class) {
          builder.BlankLine();
          builder.AppendCode(output);
        }
      }, builder => {
        foreach (var output in mixins.File) builder.AppendCode(output);
      },
      afterInNamespace: true
    );
    context.AddSource(wrapper.HintName, source);
  }

  private sealed record PropStructTarget(
    INamedTypeSymbol Type,
    CSharpCompilation Compilation,
    bool GenerateDatatype
  );
}
