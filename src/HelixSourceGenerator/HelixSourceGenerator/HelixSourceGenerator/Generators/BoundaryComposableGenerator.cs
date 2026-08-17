using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static HELIX.SourceGen.GeneratorAnalysis;
using static HELIX.SourceGen.GeneratorDiagnostics.BoundaryComposable;
using static HELIX.SourceGen.GeneratorSource;
using static HELIX.SourceGen.GeneratorStrings;

namespace HELIX.SourceGen;

[Generator(LanguageNames.CSharp)]
public sealed class BoundaryComposableGenerator : IIncrementalGenerator {
  public void Initialize(IncrementalGeneratorInitializationContext context) {
    var composables = context.SyntaxProvider.ForAttributeWithMetadataName(
      Attributes.BoundaryComposable,
      static (node, _) => node is ClassDeclarationSyntax,
      static (ctx, _) => (INamedTypeSymbol)ctx.TargetSymbol
    );

    context.RegisterSourceOutput(composables, static (spc, composable) => Generate(spc, composable));
  }

  private static void Generate(SourceProductionContext context, INamedTypeSymbol composable) {
    var location = LocationOf(composable);
    if (!IsPartial(composable)) {
      context.ReportDiagnostic(Diagnostic.Create(MustBePartial, location, composable.Name));
      return;
    }

    if (composable.TypeParameters.Length != 0) {
      context.ReportDiagnostic(Diagnostic.Create(GenericNotSupported, location, composable.Name));
      return;
    }

    var containing = FirstNonPartialContainingType(composable);
    if (containing is not null) {
      context.ReportDiagnostic(
        Diagnostic.Create(ContainingTypeMustBePartial, location, composable.Name, containing.Name)
      );
      return;
    }

    var props = composable.GetTypeMembers("Props").FirstOrDefault(type => type.Arity == 0);
    if (props is not null && (props.TypeKind != TypeKind.Struct || !IsPartial(props))) {
      context.ReportDiagnostic(Diagnostic.Create(InvalidProps, location, composable.Name));
      return;
    }

    var propCode = PropStructModel.Empty;
    if (props is not null && !PropStructApi.TryAnalyze(props, out propCode, out var diagnostic)) {
      context.ReportDiagnostic(diagnostic);
      return;
    }
    var hasPropFields = propCode.ParameterParts.Count > 0;

    var attribute = composable.GetAttributes()
      .First(item => item.AttributeClass?.ToDisplayString() == Attributes.BoundaryComposable);
    var extension = BooleanArgument(attribute, "Extension");
    var useLookupCache = BooleanArgument(attribute, "UseLookupCache");
    var baseArgument = TypeArgument(attribute, "Base");
    var composableName = StringArgument(attribute, "Name") ?? composable.Name;
    if (!IsValidIdentifier(composableName)) {
      context.ReportDiagnostic(Diagnostic.Create(InvalidName, location, composable.Name, composableName));
      return;
    }
    if (!TryResolveBase(composable, baseArgument, context, location, out var baseType)) return;

    var extensionType = FullyQualifiedExtensionType(composable, composableName);
    var wrapper = WrapType(composable, "boundary-composable", CollectUsings(props ?? composable), baseType: baseType);
    var source = wrapper.Build(
      builder => {
        AppendCompositionMethod(builder, composable, extensionType, propCode);
        if (!hasPropFields) AppendBakedComposable(builder);

        builder.BlankLine();
        using (builder.Method(
          "public override void OnRecompose",
          [
            $"ref global::{Types.Composition} cx", $"global::{Types.BoundaryData} state",
            $"global::{Types.Boundary} boundary"
          ]
        )) {
          builder.Statement($"var transfer = new global::{Types.CompositionTransfer}()")
            .Append($"global::{Types.CompositionInternals}.EnterComposition")
            .Arguments(["ref cx", extensionType + ".compositionId", "ref transfer"])
            .AppendLine(";");
          using (builder.Try()) {
            AppendContextPulls(builder, composable);
            if (useLookupCache) builder.Statement("boundary.UseLookupCache()");
            builder.Statement("base.OnRecompose(ref cx, state, boundary)");
          }
          using (builder.Finally())
            builder.Statement($"global::{Types.CompositionInternals}.ExitComposition(ref cx, ref transfer)");
        }

        if (props is null) AppendEmptyPropsStruct(builder);
        else if (hasPropFields || propCode.Equality.HasMembers) AppendPropsMembers(builder, props, propCode);
      },
      builder => AppendExtensionClass(builder, composable, composableName, propCode, extension)
    );

    context.AddSource(wrapper.HintName, source);
  }

  private static void AppendCompositionMethod(
    SharpStringBuilder builder,
    INamedTypeSymbol composable,
    string extensionType,
    PropStructModel props
  ) {
    var composableType = composable.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

    builder.BlankLine();
    using (builder.Method(
      $"public static ref global::{Types.ElementRef} ComposeBoundary",
      props.ParameterParts.Prepend($"ref global::{Types.Composition} cx")
    )) {
      builder.Append($"cx.AUTHORING.PropsBoundaryStateComposable<{composableType}, Props>")
        .AppendDelimitedList(
          [extensionType + ".typeId", "out var node", "out _", "out var attachment"],
          multiline: true
        )
        .AppendLine(";")
        .Append("var props = ");
      if (props.ParameterParts.Count > 0) builder.Append("new Props").Arguments(props.ArgumentParts);
      else builder.Append("default(Props)");
      builder.AppendLine(";")
        .Statement("attachment.ReceiveProps(props)")
        .Assignment("node.composable", "null")
        .Return("cx.AUTHORING.YieldBoundary(ref cx, node)", true);
    }
  }

  private static void AppendBakedComposable(SharpStringBuilder builder) {
    builder.BlankLine()
      .Append($"public static readonly global::{Types.Composable} BakedComposable = static ")
      .Parameters([$"ref global::{Types.Composition} cx"], false)
      .Append(" =>");
    using (builder.Block(suffix: ";")) builder.Statement("ComposeBoundary(ref cx)");
  }

  private static void AppendEmptyPropsStruct(SharpStringBuilder builder) {
    builder.BlankLine().AppendLine("public struct Props { }");
  }

  private static void AppendPropsMembers(
    SharpStringBuilder builder,
    INamedTypeSymbol props,
    PropStructModel code
  ) {
    var accessibility = AccessibilityText(props.DeclaredAccessibility);
    var unsafeModifier = code.RequiresUnsafe ? " unsafe" : "";

    builder.BlankLine();
    using (builder.Type("partial struct Props")) {
      if (code.ParameterParts.Count > 0) {
        using (builder.Method(
          $"{accessibility}{unsafeModifier} Props",
          code.ParameterParts,
          true
        )) code.AppendAssignments(builder, "this");
      }
      code.Equality.AppendMembers(builder);
    }
  }

  private static void AppendExtensionClass(
    SharpStringBuilder builder,
    INamedTypeSymbol composable,
    string methodName,
    PropStructModel props,
    bool generateExtensionMethod
  ) {
    var namespaceName = composable.ContainingNamespace is { IsGlobalNamespace: false } ns
      ? ns.ToDisplayString()
      : null;
    var accessibility = EffectiveAccessibility(composable) == Accessibility.Public ? "public" : "internal";
    var escapedName = EscapeIdentifier(methodName);

    builder.BlankLine();
    using (builder.Namespace(namespaceName)) {
      using (builder.Type($"{accessibility} static class {escapedName}Extensions")) {
        builder.Field("public static readonly", "ushort", "compositionId", GetCompositionId(methodName))
          .Field("public static readonly", "ushort", "typeId", GetTypeId(methodName));
        if (generateExtensionMethod) AppendExtensionMethod(builder, composable, methodName, props);
      }
    }
  }

  private static void AppendExtensionMethod(
    SharpStringBuilder builder,
    INamedTypeSymbol composable,
    string methodName,
    PropStructModel props
  ) {
    var composableType = composable.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

    builder.BlankLine()
      .Append($"public static ref global::{Types.ElementRef} {EscapeIdentifier(methodName)}")
      .Parameters(props.ParameterParts.Prepend($"ref this global::{Types.Composition} cx"))
      .Append(" => ref ")
      .Append(composableType)
      .Append(".ComposeBoundary")
      .Arguments(props.ArgumentParts.Prepend("ref cx"))
      .AppendLine(";");
  }

  private static string FullyQualifiedExtensionType(
    INamedTypeSymbol composable,
    string composableName
  ) {
    var namespacePrefix = composable.ContainingNamespace is { IsGlobalNamespace: false } ns
      ? ns.ToDisplayString() + "."
      : "";
    return "global::" + namespacePrefix + EscapeIdentifier(composableName) + "Extensions";
  }

  private static bool TryResolveBase(
    INamedTypeSymbol composable,
    ITypeSymbol baseArgument,
    SourceProductionContext context,
    Location location,
    out string baseType
  ) {
    var propsType = composable.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) + ".Props";

    if (baseArgument is null) {
      baseType = "global::" + Types.PropsBoundaryComposable + "<" + propsType + ">";
      return true;
    }

    if (baseArgument is not INamedTypeSymbol { IsUnboundGenericType: true } named ||
      named.TypeParameters.Length != 1) {
      context.ReportDiagnostic(
        Diagnostic.Create(
          InvalidBaseType,
          location,
          composable.Name,
          baseArgument.ToDisplayString(),
          "it must be an unbound generic type of arity 1"
        )
      );
      baseType = null;
      return false;
    }

    var openType = named.ConstructedFrom.ToDisplayString(
      SymbolDisplayFormat.FullyQualifiedFormat.WithGenericsOptions(SymbolDisplayGenericsOptions.None)
    );
    baseType = openType + "<" + propsType + ">";
    return true;
  }

  private static void AppendContextPulls(SharpStringBuilder builder, INamedTypeSymbol composable) {
    foreach (var field in composable.GetMembers()
      .OfType<IFieldSymbol>()
      .Where(field => field
        .GetAttributes().Any(attribute => attribute.AttributeClass?.ToDisplayString() == Attributes.Context)
      )) builder.Statement(EscapeIdentifier(field.Name) + ".Pull()");
  }
}