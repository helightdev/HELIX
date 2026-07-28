using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HELIX.SourceGen {
  [Generator(LanguageNames.CSharp)]
  public sealed class BoundaryComposableGenerator : IIncrementalGenerator {
    private const string AttributeMetadataName = "HELIX.Compose.BoundaryComposableAttribute";
    private const string CompositionTypeName = "HELIX.Compose.Composition";
    private const string ComposableTypeName = "HELIX.Compose.Composable";
    private const string ElementRefTypeName = "HELIX.Compose.ElementRef";
    private const string CompositionIdTypeName = "HELIX.Compose.CompositionId";
    private const string BoundaryDataTypeName = "HELIX.Compose.BoundaryData";
    private const string BoundaryTypeName = "HELIX.Compose.IBoundary";
    private const string CompositionInternalsTypeName = "HELIX.Compose.CompositionInternals";
    private const string TransferDataTypeName = "HELIX.Compose.CompositionInternals.TransferData";
    private const string PropsBaseTypeName = "HELIX.Compose.PropsBoundaryComposable";
    private const string ContextAttributeName = "HELIX.Compose.ContextAttribute";

    private static readonly DiagnosticDescriptor MustBePartial = new DiagnosticDescriptor(
      "HLXC00",
      "Boundary composable must be partial",
      "Class '{0}' is marked [BoundaryComposable] but is not declared partial",
      "HELIX",
      DiagnosticSeverity.Error,
      true
    );

    private static readonly DiagnosticDescriptor InvalidProps = new DiagnosticDescriptor(
      "HLXC02",
      "Boundary composable Props must be a partial struct",
      "Nested type '{0}.Props' must be a partial struct",
      "HELIX",
      DiagnosticSeverity.Error,
      true
    );

    private static readonly DiagnosticDescriptor InvalidBaseType = new DiagnosticDescriptor(
      "HLXC03",
      "Boundary composable Base is invalid",
      "Class '{0}' specifies Base = '{1}', but {2}",
      "HELIX",
      DiagnosticSeverity.Error,
      true
    );

    private static readonly DiagnosticDescriptor GenericNotSupported = new DiagnosticDescriptor(
      "HLXC04",
      "Generic boundary composables are not supported",
      "Class '{0}' is marked [BoundaryComposable] but is generic",
      "HELIX",
      DiagnosticSeverity.Error,
      true
    );

    private static readonly DiagnosticDescriptor ContainingTypeMustBePartial = new DiagnosticDescriptor(
      "HLXC05",
      "Containing type must be partial",
      "Class '{0}' is marked [BoundaryComposable], but containing type '{1}' is not declared partial",
      "HELIX",
      DiagnosticSeverity.Error,
      true
    );

    public void Initialize(IncrementalGeneratorInitializationContext context) {
      var composables = context.SyntaxProvider.ForAttributeWithMetadataName(
        AttributeMetadataName,
        predicate: static (node, _) => node is ClassDeclarationSyntax,
        transform: static (ctx, _) => (INamedTypeSymbol)ctx.TargetSymbol
      );

      context.RegisterSourceOutput(composables, static (spc, composable) => Generate(spc, composable));
    }

    private static void Generate(SourceProductionContext context, INamedTypeSymbol composable) {
      var location = composable.Locations.FirstOrDefault() ?? Location.None;
      if (!IsPartial(composable)) {
        context.ReportDiagnostic(Diagnostic.Create(MustBePartial, location, composable.Name));
        return;
      }

      if (composable.TypeParameters.Length != 0) {
        context.ReportDiagnostic(Diagnostic.Create(GenericNotSupported, location, composable.Name));
        return;
      }

      for (var containing = composable.ContainingType;
           containing is not null;
           containing = containing.ContainingType) {
        if (IsPartial(containing)) continue;
        context.ReportDiagnostic(Diagnostic.Create(
          ContainingTypeMustBePartial,
          location,
          composable.Name,
          containing.Name
        ));
        return;
      }

      var props = composable.GetTypeMembers("Props").FirstOrDefault(type => type.Arity == 0);
      if (props is not null && (props.TypeKind != TypeKind.Struct || !IsPartial(props))) {
        context.ReportDiagnostic(Diagnostic.Create(InvalidProps, location, composable.Name));
        return;
      }

      var propCode = PropStructGenerator.PropStructCode.Empty;
      if (props is not null && !PropStructGenerator.TryBuild(context, props, out propCode)) return;
      var hasPropFields = propCode.ParameterParts.Count > 0;

      var attribute = composable.GetAttributes().First(item =>
        item.AttributeClass?.ToDisplayString() == AttributeMetadataName);
      var extension = GetBooleanArgument(attribute, "Extension");
      var useLookupCache = GetBooleanArgument(attribute, "UseLookupCache");
      var baseArgument = GetTypeArgument(attribute, "Base");
      if (!TryResolveBase(
            composable,
            baseArgument,
            context,
            location,
            out var baseType
          )) return;

      var composableName = composable.Name;
      var extensionType = FullyQualifiedExtensionType(composable, composableName);
      var wrapper = BuildTypeWrapper(
        composable,
        baseType,
        PropStructGenerator.CollectUsings(props ?? composable),
        out var hintName
      );
      var pullLines = BuildContextPullLines(composable);
      var lookupCacheLine = useLookupCache ? "        boundary.UseLookupCache();\n" : "";
      var compositionMethod = BuildCompositionMethod(
        composable,
        extensionType,
        propCode,
        hasPropFields
      );
      var bakedComposable = hasPropFields ? "" : BuildBakedComposable();
      var propsDeclaration = props is null
        ? BuildEmptyPropsStruct()
        : hasPropFields
          ? BuildPropsConstructor(props, propCode)
          : "";
      var extensionClass = BuildExtensionClass(
        composable,
        composableName,
        propCode,
        hasPropFields,
        extension
      );

      var body = $@"
{compositionMethod}
{bakedComposable}
    public override void OnRecompose(
      ref global::{CompositionTypeName} cx,
      global::{BoundaryDataTypeName} state,
      global::{BoundaryTypeName} boundary
    ) {{
      var transfer = new global::{TransferDataTypeName}();
      global::{CompositionInternalsTypeName}.EnterComposition(
        ref cx,
        {extensionType}.compositionId,
        ref transfer
      );
      try {{
{pullLines}{lookupCacheLine}        base.OnRecompose(ref cx, state, boundary);
      }}
      finally {{
        global::{CompositionInternalsTypeName}.ExitComposition(ref cx, ref transfer);
      }}
    }}
{propsDeclaration}";

      context.AddSource(
        hintName,
        wrapper.Header + body + wrapper.Footer + extensionClass
      );
    }

    private static string BuildCompositionMethod(
      INamedTypeSymbol composable,
      string extensionType,
      PropStructGenerator.PropStructCode props,
      bool hasPropFields
    ) {
      var composableType = composable.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
      var parameters = hasPropFields ? ", " + props.Parameters : "";
      var propsInitializer = hasPropFields
        ? $"new Props({props.Arguments})"
        : "default(Props)";

      return $@"    public static ref global::{ElementRefTypeName} ComposeBoundary(
      ref global::{CompositionTypeName} cx{parameters}
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

    private static string BuildBakedComposable() =>
      $@"    public static readonly global::{ComposableTypeName} BakedComposable =
      static (ref global::{CompositionTypeName} cx) => {{
        ComposeBoundary(ref cx);
      }};

";

    private static string BuildEmptyPropsStruct() =>
      $@"
    public struct Props {{ }}
";

    private static string BuildPropsConstructor(
      INamedTypeSymbol props,
      PropStructGenerator.PropStructCode code
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
      PropStructGenerator.PropStructCode props,
      bool hasPropFields,
      bool generateExtensionMethod
    ) {
      var namespaceName = composable.ContainingNamespace is { IsGlobalNamespace: false } ns
        ? ns.ToDisplayString()
        : null;
      var namespaceOpen = namespaceName is null ? "" : $"namespace {namespaceName} {{\n";
      var namespaceClose = namespaceName is null ? "" : "}\n";
      var accessibility = EffectiveAccessibility(composable) == Accessibility.Public
        ? "public"
        : "internal";
      var escapedName = EscapeIdentifier(methodName);
      var extensionMethod = generateExtensionMethod
        ? BuildExtensionMethod(composable, methodName, props, hasPropFields)
        : "";

      return $@"
{namespaceOpen}  {accessibility} static class {escapedName}Extensions {{
    public static readonly ushort compositionId =
      global::{CompositionIdTypeName}.GetCompositionId();
    public static readonly ushort typeId =
      global::{CompositionIdTypeName}.GetTypeId();
{extensionMethod}  }}
{namespaceClose}";
    }

    private static string BuildExtensionMethod(
      INamedTypeSymbol composable,
      string methodName,
      PropStructGenerator.PropStructCode props,
      bool hasPropFields
    ) {
      var parameters = hasPropFields ? ", " + props.Parameters : "";
      var arguments = hasPropFields ? ", " + props.Arguments : "";
      var composableType = composable.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

      return $@"
    public static ref global::{ElementRefTypeName} {EscapeIdentifier(methodName)}(
      ref this global::{CompositionTypeName} cx{parameters}
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
        baseType = "global::" + PropsBaseTypeName + "<" + propsType + ">";
        return true;
      }

      if (baseArgument is not INamedTypeSymbol named ||
          !named.IsUnboundGenericType ||
          named.TypeParameters.Length != 1) {
        context.ReportDiagnostic(Diagnostic.Create(
          InvalidBaseType,
          location,
          composable.Name,
          baseArgument.ToDisplayString(),
          "it must be an unbound generic type of arity 1"
        ));
        baseType = null;
        return false;
      }

      var openType = named.ConstructedFrom.ToDisplayString(
        SymbolDisplayFormat.FullyQualifiedFormat
          .WithGenericsOptions(SymbolDisplayGenericsOptions.None));
      baseType = openType + "<" + propsType + ">";
      return true;
    }

    private static string BuildContextPullLines(INamedTypeSymbol composable) =>
      string.Concat(
        composable.GetMembers()
          .OfType<IFieldSymbol>()
          .Where(field => field.GetAttributes().Any(attribute =>
            attribute.AttributeClass?.ToDisplayString() == ContextAttributeName))
          .Select(field => $"        {EscapeIdentifier(field.Name)}.Pull();\n")
      );

    private static ITypeSymbol GetTypeArgument(AttributeData attribute, string name) =>
      attribute.NamedArguments.FirstOrDefault(argument => argument.Key == name).Value.Value as ITypeSymbol;

    private static bool GetBooleanArgument(AttributeData attribute, string name) {
      var argument = attribute.NamedArguments.FirstOrDefault(item => item.Key == name);
      return argument.Value.Value is bool value && value;
    }

    private static bool IsPartial(INamedTypeSymbol type) =>
      type.DeclaringSyntaxReferences.Any(reference =>
        reference.GetSyntax() is TypeDeclarationSyntax declaration &&
        declaration.Modifiers.Any(SyntaxKind.PartialKeyword));

    private static Accessibility EffectiveAccessibility(INamedTypeSymbol type) {
      for (var current = type; current is not null; current = current.ContainingType) {
        if (current.DeclaredAccessibility != Accessibility.Public) return Accessibility.Internal;
      }
      return Accessibility.Public;
    }

    private static string AccessibilityText(Accessibility accessibility) => accessibility switch {
      Accessibility.Public => "public",
      Accessibility.Private => "private",
      Accessibility.Protected => "protected",
      Accessibility.Internal => "internal",
      Accessibility.ProtectedAndInternal => "private protected",
      Accessibility.ProtectedOrInternal => "protected internal",
      _ => "internal"
    };

    private static string EscapeIdentifier(string identifier) =>
      SyntaxFacts.GetKeywordKind(identifier) != SyntaxKind.None ||
      SyntaxFacts.GetContextualKeywordKind(identifier) != SyntaxKind.None
        ? "@" + identifier
        : identifier;

    private static Wrapper BuildTypeWrapper(
      INamedTypeSymbol type,
      string baseType,
      IReadOnlyList<string> usings,
      out string hintName
    ) {
      var chain = new List<INamedTypeSymbol>();
      for (var current = type; current is not null; current = current.ContainingType) chain.Add(current);
      chain.Reverse();

      var namespaceName = type.ContainingNamespace is { IsGlobalNamespace: false } ns
        ? ns.ToDisplayString()
        : null;
      var header = new StringBuilder("// <auto-generated/>\n");
      foreach (var usingDirective in usings) {
        header.Append(usingDirective);
        header.Append('\n');
      }
      if (usings.Count > 0) header.Append('\n');
      var depth = 0;
      if (namespaceName is not null) {
        header.Append("namespace ");
        header.Append(namespaceName);
        header.Append(" {\n");
        depth++;
      }

      foreach (var current in chain) {
        header.Append(' ', depth * 2);
        header.Append(current.IsStatic ? "static partial " : "partial ");
        header.Append(TypeKeyword(current));
        header.Append(' ');
        header.Append(EscapeIdentifier(current.Name));
        header.Append(TypeParameters(current));
        if (SymbolEqualityComparer.Default.Equals(current, type)) {
          header.Append(" : ");
          header.Append(baseType);
        }
        header.Append(" {\n");
        depth++;
      }

      var footer = new StringBuilder();
      for (var closeDepth = depth - 1; closeDepth >= 0; closeDepth--) {
        footer.Append(' ', closeDepth * 2);
        footer.Append("}\n");
      }

      var metadataPath = string.Join(".", chain.Select(item => item.MetadataName));
      hintName = Sanitize((namespaceName ?? "global") + "." + metadataPath) +
                 ".boundary-composable.g.cs";
      return new Wrapper(header.ToString(), footer.ToString());
    }

    private static string TypeKeyword(INamedTypeSymbol type) {
      if (type.IsRecord) return type.TypeKind == TypeKind.Struct ? "record struct" : "record";
      return type.TypeKind == TypeKind.Struct ? "struct" :
             type.TypeKind == TypeKind.Interface ? "interface" : "class";
    }

    private static string TypeParameters(INamedTypeSymbol type) => type.TypeParameters.Length == 0
      ? ""
      : "<" + string.Join(", ", type.TypeParameters.Select(parameter =>
        EscapeIdentifier(parameter.Name))) + ">";

    private static string Sanitize(string value) {
      var characters = value.ToCharArray();
      for (var index = 0; index < characters.Length; index++) {
        if (!char.IsLetterOrDigit(characters[index])) characters[index] = '_';
      }
      return new string(characters);
    }

    private readonly struct Wrapper {
      internal Wrapper(string header, string footer) {
        Header = header;
        Footer = footer;
      }

      internal string Header { get; }
      internal string Footer { get; }
    }
  }
}
