using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static HELIX.SourceGen.GeneratorAnalysis;
using static HELIX.SourceGen.GeneratorDiagnostics.BoundaryComposable;
using static HELIX.SourceGen.GeneratorSource;
using static HELIX.SourceGen.GeneratorStrings;

namespace HELIX.SourceGen {
  [Generator(LanguageNames.CSharp)]
  public sealed class BoundaryComposableGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
      var composables = context.SyntaxProvider.ForAttributeWithMetadataName(
        Attributes.BoundaryComposable,
        predicate: static (node, _) => node is ClassDeclarationSyntax,
        transform: static (ctx, _) => (INamedTypeSymbol)ctx.TargetSymbol
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
      if (!TryResolveBase(composable, baseArgument, context, location, out var baseType)) return;

      var composableName = composable.Name;
      var extensionType = FullyQualifiedExtensionType(composable, composableName);
      var wrapper = WrapType(composable, "boundary-composable", CollectUsings(props ?? composable), baseType: baseType);
      var pullLines = BuildContextPullLines(composable);
      var lookupCacheLine = useLookupCache ? $"{IndentL4}boundary.UseLookupCache();\n" : "";
      var compositionMethod = BuildCompositionMethod(composable, extensionType, propCode, hasPropFields);
      var bakedComposable = hasPropFields ? "" : BuildBakedComposable();
      var propsDeclaration = props is null
        ? BuildEmptyPropsStruct()
        : hasPropFields
          ? BuildPropsConstructor(props, propCode)
          : "";
      var extensionClass = BuildExtensionClass(composable, composableName, propCode, hasPropFields, extension);

      var body = $@"
{compositionMethod}
{bakedComposable}
    public override void OnRecompose(
      ref global::{Types.Composition} cx,
      global::{Types.BoundaryData} state,
      global::{Types.Boundary} boundary
    ) {{
      var transfer = new global::{Types.CompositionTransfer}();
      global::{Types.CompositionInternals}.EnterComposition(
        ref cx,
        {extensionType}.compositionId,
        ref transfer
      );
      try {{
{pullLines}{lookupCacheLine}{IndentL4}base.OnRecompose(ref cx, state, boundary);
      }}
      finally {{
        global::{Types.CompositionInternals}.ExitComposition(ref cx, ref transfer);
      }}
    }}
{propsDeclaration}";

      context.AddSource(wrapper.HintName, wrapper.Enclose(body) + extensionClass);
    }

    private static string BuildCompositionMethod(
      INamedTypeSymbol composable,
      string extensionType,
      PropStructModel props,
      bool hasPropFields
    ) {
      var composableType = composable.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
      var parameters = hasPropFields ? $",\n{IndentL3}" + props.Parameters.Replace("\n", $"\n{IndentL3}") : "";
      var propsInitializer = hasPropFields
        ? $"new Props({props.Arguments})"
        : "default(Props)";

      return $@"
    public static ref global::{Types.ElementRef} ComposeBoundary(
      ref global::{Types.Composition} cx{parameters}
    ) {{
      cx.AUTHORING.PropsBoundaryStateComposable<{composableType}, Props>(
        {extensionType}.typeId,
        out var node,
        out _,
        out var attachment
      );
      var props = {propsInitializer};
      attachment.ReceiveProps(props);
      node.composable = null;
      return ref cx.AUTHORING.YieldBoundary(ref cx, node);
    }}
";
    }

    private static string BuildBakedComposable() => $@"
    public static readonly global::{Types.Composable} BakedComposable =
      static (ref global::{Types.Composition} cx) => {{ ComposeBoundary(ref cx); }};
";

    private static string BuildEmptyPropsStruct() => $@"
    public struct Props {{ }}
";

    private static string BuildPropsConstructor(
      INamedTypeSymbol props,
      PropStructModel code
    ) {
      var accessibility = AccessibilityText(props.DeclaredAccessibility);
      var unsafeModifier = code.RequiresUnsafe ? " unsafe" : "";
      var assignments = code.RenderAssignments("this", "        ");

      return $@"
    partial struct Props {{
      {accessibility}{unsafeModifier} Props({code.Parameters}) {{
{assignments}      }}
    }}
";
    }

    private static string BuildExtensionClass(
      INamedTypeSymbol composable,
      string methodName,
      PropStructModel props,
      bool hasPropFields,
      bool generateExtensionMethod
    ) {
      var namespaceName = composable.ContainingNamespace is { IsGlobalNamespace: false } ns
        ? ns.ToDisplayString()
        : null;
      var namespaceOpen = namespaceName is null ? "" : $"namespace {namespaceName} {{\n";
      var namespaceClose = namespaceName is null ? "" : "}\n";
      var accessibility = EffectiveAccessibility(composable) == Accessibility.Public ? "public" : "internal";
      var escapedName = EscapeIdentifier(methodName);
      var extensionMethod = generateExtensionMethod
        ? BuildExtensionMethod(composable, methodName, props, hasPropFields)
        : "";

      return $@"
{namespaceOpen}  {accessibility} static class {escapedName}Extensions {{
    public static readonly ushort compositionId = {GetCompositionId(methodName)};
    public static readonly ushort typeId = {GetTypeId(methodName)};
{extensionMethod}  }}
{namespaceClose}";
    }

    private static string BuildExtensionMethod(
      INamedTypeSymbol composable,
      string methodName,
      PropStructModel props,
      bool hasPropFields
    ) {
      var parameters = hasPropFields ? $",\n{IndentL3}" + props.Parameters.Replace("\n", $"\n{IndentL3}") : "";
      var arguments = hasPropFields ? ", " + props.Arguments : "";
      var composableType = composable.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

      return $@"
    public static ref global::{Types.ElementRef} {EscapeIdentifier(methodName)}(
      ref this global::{Types.Composition} cx{parameters}
    ) => ref {composableType}.ComposeBoundary(ref cx{arguments});
";
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

    private static string BuildContextPullLines(INamedTypeSymbol composable) => string.Concat(
      composable.GetMembers()
        .OfType<IFieldSymbol>()
        .Where(field => field
          .GetAttributes().Any(attribute => attribute.AttributeClass?.ToDisplayString() == Attributes.Context)
        )
        .Select(field => $"{IndentL4}{EscapeIdentifier(field.Name)}.Pull();\n")
    );
  }
}