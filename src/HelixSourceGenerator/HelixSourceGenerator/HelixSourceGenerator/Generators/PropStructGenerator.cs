using System.Linq;
using HELIX.SourceGen.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static HELIX.SourceGen.GeneratorAnalysis;
using static HELIX.SourceGen.GeneratorDiagnostics.PropStruct;
using static HELIX.SourceGen.GeneratorSource;
using static HELIX.SourceGen.GeneratorStrings;

namespace HELIX.SourceGen;

[Generator(LanguageNames.CSharp)]
public sealed class PropStructGenerator : IIncrementalGenerator {
  public void Initialize(IncrementalGeneratorInitializationContext context) {
    var structs = context.SyntaxProvider.ForAttributeWithMetadataName(
      Attributes.PropStruct,
      static (node, _) => node is StructDeclarationSyntax,
      static (ctx, _) => (INamedTypeSymbol)ctx.TargetSymbol
    );
    var preparedExpressions = context.CompilationProvider.Select(
      static (compilation, _) => MixinGenerator.CollectPreparedExpressions(compilation)
    );
    context.RegisterSourceOutput(
      structs.Combine(context.CompilationProvider).Combine(preparedExpressions),
      static (production, input) => Generate(
        production,
        input.Left.Left,
        (CSharpCompilation)input.Left.Right,
        input.Right.State
      )
    );
  }

  private static void Generate(
    SourceProductionContext context,
    INamedTypeSymbol type,
    CSharpCompilation compilation,
    MixinExpressionPreparedState preparedExpressions
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

    var attribute = Attribute(type, Attributes.PropStruct);
    var generateDatatype = attribute is { ConstructorArguments.Length: > 0 } &&
      attribute.ConstructorArguments[0].Value is true;
    if (!PropStructApi.TryAnalyze(type, out var props, out var diagnostic, generateDatatype)) {
      context.ReportDiagnostic(diagnostic);
      return;
    }

    var mixins = generateDatatype
      ? PropStructMixinApi.Analyze(
        context, type, props.PropertySymbols, compilation, preparedExpressions
      )
      : new PropStructMixinModel();
    var usings = CollectUsings(type).Concat(mixins.Usings).Distinct().ToArray();
    var wrapper = WrapType(
      type, "prop-struct", usings,
      baseType: mixins.Implements.Count == 0 ? null : string.Join(", ", mixins.Implements),
      typeAttributes: mixins.Annotations
    );
    var source = wrapper.Build(builder => {
        builder.BlankLine();
        using (builder.Method(
          AccessibilityText(type.DeclaredAccessibility) +
          (props.RequiresUnsafe ? " unsafe " : " ") +
          EscapeIdentifier(type.Name),
          props.ParameterParts,
          props.ParameterParts.Count > 0
        )) props.AppendAssignments(builder, "this");
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
}
