using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HELIX.SourceGen {
  [Generator(LanguageNames.CSharp)]
  public sealed class ComposableProxyGenerator : IIncrementalGenerator {
    private const string AttributeMetadataName = "HELIX.Compose.ComposableProxyAttribute";
    private const string PropProxyAttributeMetadataName = "HELIX.Compose.PropProxyAttribute";
    private const string CompositionTypeName = "HELIX.Compose.Composition";
    private const string CompositionIdTypeName = "HELIX.Compose.CompositionId";
    private const string ElementRefTypeName = "HELIX.Compose.ElementRef";
    private const string ScopeHandleTypeName = "HELIX.Compose.ScopeHandle";
    private const string BoundaryCellTypeName = "HELIX.Compose.BoundaryCell";

    private const string DefaultCreateSyntax = "instance = new {TYPE}();";
    private const string DefaultPrepareSyntax = "/* Skip Prepare */";
    private const string DefaultPreYieldSyntax = "/* Skip Before Yield */";
    private const string DefaultPostYieldSyntax = "/* Skip Post Yield */";
    private const string DefaultScopeCallbackSyntax = "cell.TrimChildren();";
    private const string DefaultEqualitySyntax = "{0} == {1}";

    private static readonly DiagnosticDescriptor MissingTarget = new DiagnosticDescriptor(
      "HLXCP00",
      "Composable proxy target is required",
      "Struct '{0}' is marked [ComposableProxy] but does not specify a Target type",
      "HELIX",
      DiagnosticSeverity.Error,
      true
    );

    private static readonly DiagnosticDescriptor GenericNotSupported = new DiagnosticDescriptor(
      "HLXCP01",
      "Generic composable proxies are not supported",
      "Struct '{0}' is marked [ComposableProxy], but it or its containing type is generic",
      "HELIX",
      DiagnosticSeverity.Error,
      true
    );

    private static readonly DiagnosticDescriptor InvalidName = new DiagnosticDescriptor(
      "HLXCP02",
      "Composable proxy name is invalid",
      "Struct '{0}' specifies '{1}' as its composable name, but it is not a valid C# identifier",
      "HELIX",
      DiagnosticSeverity.Error,
      true
    );

    private static readonly DiagnosticDescriptor InvalidKind = new DiagnosticDescriptor(
      "HLXCP03",
      "Composable proxy kind is invalid",
      "Struct '{0}' specifies an unrecognized ComposableKind value",
      "HELIX",
      DiagnosticSeverity.Error,
      true
    );

    private static readonly DiagnosticDescriptor InvalidPropProxy = new DiagnosticDescriptor(
      "HLXCP04",
      "Proxy prop configuration is invalid",
      "Field '{0}' has an invalid [PropProxy]: {1}",
      "HELIX",
      DiagnosticSeverity.Error,
      true
    );

    private static readonly DiagnosticDescriptor MustBePartial = new DiagnosticDescriptor(
      "HLXCP05",
      "Composable proxy must be partial",
      "Struct '{0}' is marked [ComposableProxy] but is not declared partial",
      "HELIX",
      DiagnosticSeverity.Error,
      true
    );

    private static readonly DiagnosticDescriptor ContainingTypeMustBePartial = new DiagnosticDescriptor(
      "HLXCP06",
      "Containing type must be partial",
      "Struct '{0}' is marked [ComposableProxy], but containing type '{1}' is not declared partial",
      "HELIX",
      DiagnosticSeverity.Error,
      true
    );

    private static readonly SymbolDisplayFormat TypeDisplayFormat =
      SymbolDisplayFormat.FullyQualifiedFormat
        .WithMiscellaneousOptions(
          SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
          SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
        );

    public void Initialize(IncrementalGeneratorInitializationContext context) {
      var proxies = context.SyntaxProvider.ForAttributeWithMetadataName(
        AttributeMetadataName,
        predicate: static (node, _) => node is StructDeclarationSyntax,
        transform: static (ctx, _) => (INamedTypeSymbol)ctx.TargetSymbol
      );

      context.RegisterSourceOutput(proxies, static (spc, proxy) => Generate(spc, proxy));
    }

    private static void Generate(SourceProductionContext context, INamedTypeSymbol proxy) {
      var location = proxy.Locations.FirstOrDefault() ?? Location.None;
      var attribute = proxy.GetAttributes().First(item =>
        item.AttributeClass?.ToDisplayString() == AttributeMetadataName);
      var target = GetTypeArgument(attribute, "Target");
      if (target is null) {
        context.ReportDiagnostic(Diagnostic.Create(MissingTarget, location, proxy.Name));
        return;
      }

      if (!IsPartial(proxy)) {
        context.ReportDiagnostic(Diagnostic.Create(MustBePartial, location, proxy.Name));
        return;
      }

      for (var containing = proxy.ContainingType;
           containing is not null;
           containing = containing.ContainingType) {
        if (IsPartial(containing)) continue;
        context.ReportDiagnostic(Diagnostic.Create(
          ContainingTypeMustBePartial,
          location,
          proxy.Name,
          containing.Name
        ));
        return;
      }

      if (HasTypeParameters(proxy)) {
        context.ReportDiagnostic(Diagnostic.Create(GenericNotSupported, location, proxy.Name));
        return;
      }

      var methodName = GetStringArgument(attribute, "Name", target.Name);
      if (!IsValidMethodName(methodName)) {
        context.ReportDiagnostic(Diagnostic.Create(
          InvalidName,
          location,
          proxy.Name,
          methodName ?? "null"
        ));
        return;
      }

      var kind = GetIntArgument(attribute, "Kind", 1);
      if (kind < 0 || kind > 1) {
        context.ReportDiagnostic(Diagnostic.Create(InvalidKind, location, proxy.Name));
        return;
      }

      if (!PropStructGenerator.TryBuild(context, proxy, out var props)) return;
      var fields = proxy.GetMembers()
        .OfType<IFieldSymbol>()
        .Where(field => !field.IsStatic && !field.IsImplicitlyDeclared)
        .OrderBy(SourceOrder)
        .ToArray();
      var targetType = target.ToDisplayString(TypeDisplayFormat);
      if (!TryBuildPropUpdates(
            context,
            fields,
            props.Assignments,
            targetType,
            out var propUpdates
          )) return;

      var requiresTracking = GetBooleanArgument(attribute, "RequiresTracking", true);
      var generateExtension = GetBooleanArgument(attribute, "Extension", true);
      var createSyntax = GetStringArgument(attribute, "CreateSyntax", DefaultCreateSyntax);
      var prepareSyntax = GetStringArgument(attribute, "PrepareSyntax", DefaultPrepareSyntax);
      var preYieldSyntax = GetStringArgument(attribute, "PreYieldSyntax", DefaultPreYieldSyntax);
      var postYieldSyntax = GetStringArgument(attribute, "PostYieldSyntax", DefaultPostYieldSyntax);
      var scopeCallbackSyntax = GetStringArgument(
        attribute,
        "ScopeCallbackSyntax",
        DefaultScopeCallbackSyntax
      );

      createSyntax = createSyntax.Replace("{TYPE}", targetType);
      var source = BuildSource(
        proxy,
        methodName,
        targetType,
        kind == 0,
        requiresTracking,
        props,
        propUpdates,
        createSyntax,
        prepareSyntax,
        preYieldSyntax,
        postYieldSyntax,
        scopeCallbackSyntax,
        generateExtension,
        out var hintName
      );
      context.AddSource(hintName, source);
    }

    private static bool TryBuildPropUpdates(
      SourceProductionContext context,
      IReadOnlyList<IFieldSymbol> fields,
      IReadOnlyList<PropStructGenerator.PropAssignment> assignments,
      string targetType,
      out string updates
    ) {
      var result = new StringBuilder();
      for (var index = 0; index < fields.Count; index++) {
        var field = fields[index];
        var assignment = assignments[index];
        var attribute = field.GetAttributes().FirstOrDefault(item =>
          item.AttributeClass?.ToDisplayString() == PropProxyAttributeMetadataName);
        var setter = attribute is null
          ? EscapeIdentifier(field.Name)
          : GetConstructorStringArgument(attribute, 0);
        if (string.IsNullOrWhiteSpace(setter)) {
          context.ReportDiagnostic(Diagnostic.Create(
            InvalidPropProxy,
            field.Locations.FirstOrDefault() ?? Location.None,
            field.Name,
            "the setter must be a non-empty member name or function template"
          ));
          updates = null;
          return false;
        }

        var value = assignment.ValueExpression;
        var isFunction = attribute is not null &&
                         GetBooleanArgument(attribute, "Function", false);
        var setterCode = isFunction
          ? setter
            .Replace("{VALUE}", value)
            .Replace("{TYPE}", targetType)
          : $"instance.{setter} = {value};";
        var checkEquality = attribute is not null &&
                            GetBooleanArgument(attribute, "CheckEquality", false);
        if (!checkEquality) {
          result.AppendLine(setterCode);
          continue;
        }

        var getter = GetStringArgument(attribute, "Getter") ?? setter;
        if (string.IsNullOrWhiteSpace(getter)) {
          context.ReportDiagnostic(Diagnostic.Create(
            InvalidPropProxy,
            field.Locations.FirstOrDefault() ?? Location.None,
            field.Name,
            "the getter must be a non-empty member name when equality checking is enabled"
          ));
          updates = null;
          return false;
        }

        var equalitySyntax =
          GetStringArgument(attribute, "EqualitySyntax") ?? DefaultEqualitySyntax;
        string equality;
        try {
          equality = string.Format(
            CultureInfo.InvariantCulture,
            equalitySyntax,
            "instance." + getter,
            value
          );
        } catch (FormatException exception) {
          context.ReportDiagnostic(Diagnostic.Create(
            InvalidPropProxy,
            field.Locations.FirstOrDefault() ?? Location.None,
            field.Name,
            "EqualitySyntax could not be formatted: " + exception.Message
          ));
          updates = null;
          return false;
        }

        result.Append($@"if (!({equality})) {{
{IndentCode(setterCode, "  ")}}}
");
      }

      updates = result.ToString();
      return true;
    }

    private static string BuildSource(
      INamedTypeSymbol proxy,
      string methodName,
      string targetType,
      bool isScope,
      bool requiresTracking,
      PropStructGenerator.PropStructCode props,
      string propUpdates,
      string createSyntax,
      string prepareSyntax,
      string preYieldSyntax,
      string postYieldSyntax,
      string scopeCallbackSyntax,
      bool generateExtension,
      out string hintName
    ) {
      var wrapper = BuildTypeWrapper(
        proxy,
        PropStructGenerator.CollectUsings(proxy),
        out hintName
      );
      var structMembers = BuildStructMembers(
        targetType,
        isScope,
        requiresTracking,
        props,
        propUpdates,
        createSyntax,
        prepareSyntax,
        preYieldSyntax,
        postYieldSyntax,
        scopeCallbackSyntax
      );
      var extensionClass = generateExtension
        ? BuildExtensionClass(proxy, methodName, isScope, props)
        : "";

      return wrapper.Header +
             IndentCode(structMembers, new string(' ', wrapper.MemberDepth * 2)) +
             wrapper.Footer +
             extensionClass;
    }

    private static string BuildStructMembers(
      string targetType,
      bool isScope,
      bool requiresTracking,
      PropStructGenerator.PropStructCode props,
      string propUpdates,
      string createSyntax,
      string prepareSyntax,
      string preYieldSyntax,
      string postYieldSyntax,
      string scopeCallbackSyntax
    ) {
      var unsafeModifier = props.RequiresUnsafe ? " unsafe" : "";
      var parameters = props.ParameterParts.Count == 0 ? "" : ", " + props.Parameters;
      var requireMethod = requiresTracking ? "RequireTracked" : "RequireComposable";
      var returnType = isScope
        ? $"global::{ScopeHandleTypeName}"
        : $"ref global::{ElementRefTypeName}";
      var yieldCode = isScope
        ? BuildScopeYield(preYieldSyntax, postYieldSyntax, scopeCallbackSyntax)
        : BuildElementYield(preYieldSyntax, postYieldSyntax);

      return $@"private static readonly ushort _typeId =
  global::{CompositionIdTypeName}.GetTypeId();

public static{unsafeModifier} {returnType} Compose(
  ref global::{CompositionTypeName} cx{parameters}
) {{
  if (!cx.AUTHORING.{requireMethod}<{targetType}>(
    _typeId,
    out var instance,
    out var retained
  )) {{
{IndentCode(createSyntax, "    ")}  }}
  if (!retained) {{
{IndentCode(prepareSyntax, "    ")}  }}
{IndentCode(propUpdates, "  ")}{yieldCode}}}
";
    }

    private static string BuildElementYield(
      string preYieldSyntax,
      string postYieldSyntax
    ) => $@"{IndentCode(preYieldSyntax, "  ")}  ref var elementRef = ref cx.AUTHORING.YieldElement(ref cx, instance);
{IndentCode(postYieldSyntax, "  ")}  return ref elementRef;
";

    private static string BuildScopeYield(
      string preYieldSyntax,
      string postYieldSyntax,
      string scopeCallbackSyntax
    ) => $@"{IndentCode(preYieldSyntax, "  ")}  var scopeHandle = cx.AUTHORING.YieldScope(
    ref cx,
    instance,
    static (
      global::{BoundaryCellTypeName} cell,
      in global::{ScopeHandleTypeName} handle
    ) => {{
{IndentCode(scopeCallbackSyntax, "      ")}    }}
  );
{IndentCode(postYieldSyntax, "  ")}  return scopeHandle;
";

    private static string BuildExtensionClass(
      INamedTypeSymbol proxy,
      string methodName,
      bool isScope,
      PropStructGenerator.PropStructCode props
    ) {
      var namespaceName = proxy.ContainingNamespace is { IsGlobalNamespace: false } ns
        ? ns.ToDisplayString()
        : null;
      var namespaceOpen = namespaceName is null ? "" : $"namespace {namespaceName} {{\n";
      var namespaceClose = namespaceName is null ? "" : "}\n";
      var className = EscapeIdentifier(methodName) + "Extensions";
      var accessibility = EffectiveAccessibility(proxy) == Accessibility.Public
        ? "public"
        : "internal";
      var unsafeModifier = props.RequiresUnsafe ? " unsafe" : "";
      var parameters = props.ParameterParts.Count == 0 ? "" : ", " + props.Parameters;
      var returnType = isScope
        ? $"global::{ScopeHandleTypeName}"
        : $"ref global::{ElementRefTypeName}";
      var returnRef = isScope ? "" : "ref ";
      var proxyType = proxy.ToDisplayString(TypeDisplayFormat);
      var arguments = props.ParameterParts.Count == 0 ? "" : ", " + props.Arguments;

      return $@"
{namespaceOpen}  {accessibility} static class {className} {{
    public static{unsafeModifier} {returnType} {EscapeIdentifier(methodName)}(
      ref this global::{CompositionTypeName} cx{parameters}
    ) => {returnRef}{proxyType}.Compose(ref cx{arguments});
  }}
{namespaceClose}";
    }

    private static string IndentCode(string code, string indent) {
      var normalized = (code ?? "").Replace("\r\n", "\n").Replace('\r', '\n');
      var lines = normalized.Split(new[] { '\n' }, StringSplitOptions.None);
      var result = new StringBuilder();
      foreach (var line in lines) {
        result.Append(indent);
        result.Append(line);
        result.Append('\n');
      }
      return result.ToString();
    }

    private static Wrapper BuildTypeWrapper(
      INamedTypeSymbol type,
      IReadOnlyList<string> usings,
      out string hintName
    ) {
      var chain = new List<INamedTypeSymbol>();
      for (var current = type; current is not null; current = current.ContainingType) {
        chain.Add(current);
      }
      chain.Reverse();

      var namespaceName = type.ContainingNamespace is { IsGlobalNamespace: false } ns
        ? ns.ToDisplayString()
        : null;
      var header = new StringBuilder("// <auto-generated/>\n");
      foreach (var usingDirective in usings) header.AppendLine(usingDirective);
      if (usings.Count > 0) header.AppendLine();

      var depth = 0;
      if (namespaceName is not null) {
        header.AppendLine($"namespace {namespaceName} {{");
        depth++;
      }

      foreach (var current in chain) {
        var indent = new string(' ', depth * 2);
        var staticModifier = current.IsStatic ? "static " : "";
        header.AppendLine(
          $"{indent}{staticModifier}partial {TypeKeyword(current)} " +
          $"{EscapeIdentifier(current.Name)}{TypeParameters(current)} {{"
        );
        depth++;
      }

      var footer = new StringBuilder();
      for (var closeDepth = depth - 1; closeDepth >= 0; closeDepth--) {
        footer.AppendLine(new string(' ', closeDepth * 2) + "}");
      }

      var metadataPath = GetMetadataPath(type);
      hintName = Sanitize((namespaceName ?? "global") + "." + metadataPath) +
                 ".composable-proxy.g.cs";
      return new Wrapper(header.ToString(), footer.ToString(), depth);
    }

    private static ITypeSymbol GetTypeArgument(AttributeData attribute, string name) =>
      attribute.NamedArguments.FirstOrDefault(item => item.Key == name).Value.Value as ITypeSymbol;

    private static string GetStringArgument(
      AttributeData attribute,
      string name,
      string defaultValue = null
    ) {
      foreach (var argument in attribute.NamedArguments) {
        if (argument.Key == name) return argument.Value.Value as string;
      }
      return defaultValue;
    }

    private static string GetConstructorStringArgument(AttributeData attribute, int index) =>
      attribute.ConstructorArguments.Length > index
        ? attribute.ConstructorArguments[index].Value as string
        : null;

    private static bool GetBooleanArgument(
      AttributeData attribute,
      string name,
      bool defaultValue
    ) {
      foreach (var argument in attribute.NamedArguments) {
        if (argument.Key == name && argument.Value.Value is bool value) return value;
      }
      return defaultValue;
    }

    private static int GetIntArgument(AttributeData attribute, string name, int defaultValue) {
      object value = null;
      foreach (var argument in attribute.NamedArguments) {
        if (argument.Key != name) continue;
        value = argument.Value.Value;
        break;
      }
      if (value is null) return defaultValue;
      try {
        return Convert.ToInt32(value, CultureInfo.InvariantCulture);
      } catch (Exception) {
        return int.MinValue;
      }
    }

    private static bool HasTypeParameters(INamedTypeSymbol type) {
      for (var current = type; current is not null; current = current.ContainingType) {
        if (current.TypeParameters.Length != 0) return true;
      }
      return false;
    }

    private static bool IsPartial(INamedTypeSymbol type) =>
      type.DeclaringSyntaxReferences.Any(reference =>
        reference.GetSyntax() is TypeDeclarationSyntax declaration &&
        declaration.Modifiers.Any(SyntaxKind.PartialKeyword));

    private static bool IsValidMethodName(string name) =>
      !string.IsNullOrWhiteSpace(name) &&
      (SyntaxFacts.IsValidIdentifier(name) ||
       SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None ||
       SyntaxFacts.GetContextualKeywordKind(name) != SyntaxKind.None);

    private static Accessibility EffectiveAccessibility(INamedTypeSymbol type) {
      for (var current = type; current is not null; current = current.ContainingType) {
        if (current.DeclaredAccessibility != Accessibility.Public) return Accessibility.Internal;
      }
      return Accessibility.Public;
    }

    private static int SourceOrder(IFieldSymbol field) {
      var location = field.Locations.FirstOrDefault(item => item.IsInSource);
      return location?.SourceSpan.Start ?? int.MaxValue;
    }

    private static string GetMetadataPath(INamedTypeSymbol type) {
      var chain = new List<string>();
      for (var current = type; current is not null; current = current.ContainingType) {
        chain.Add(current.MetadataName);
      }
      chain.Reverse();
      return string.Join(".", chain);
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

    private static string EscapeIdentifier(string identifier) =>
      SyntaxFacts.GetKeywordKind(identifier) != SyntaxKind.None ||
      SyntaxFacts.GetContextualKeywordKind(identifier) != SyntaxKind.None
        ? "@" + identifier
        : identifier;

    private static string Sanitize(string value) {
      var characters = value.ToCharArray();
      for (var index = 0; index < characters.Length; index++) {
        if (!char.IsLetterOrDigit(characters[index])) characters[index] = '_';
      }
      return new string(characters);
    }

    private readonly struct Wrapper {
      internal Wrapper(string header, string footer, int memberDepth) {
        Header = header;
        Footer = footer;
        MemberDepth = memberDepth;
      }

      internal string Header { get; }
      internal string Footer { get; }
      internal int MemberDepth { get; }
    }
  }
}
