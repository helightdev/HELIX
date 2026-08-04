using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static HELIX.SourceGen.GeneratorAnalysis;
using static HELIX.SourceGen.GeneratorDiagnostics.ComposableProxy;
using static HELIX.SourceGen.GeneratorSource;
using static HELIX.SourceGen.GeneratorStrings;

namespace HELIX.SourceGen {
  [Generator(LanguageNames.CSharp)]
  public sealed class ComposableProxyGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
      var proxies = context.SyntaxProvider.ForAttributeWithMetadataName(
        Attributes.ComposableProxy,
        predicate: static (node, _) => node is StructDeclarationSyntax,
        transform: static (ctx, _) => (INamedTypeSymbol)ctx.TargetSymbol
      );

      context.RegisterSourceOutput(proxies, static (spc, proxy) => Generate(spc, proxy));
    }

    private static void Generate(SourceProductionContext context, INamedTypeSymbol proxy) {
      var location = LocationOf(proxy);
      var attribute = proxy.GetAttributes().First(item =>
        item.AttributeClass?.ToDisplayString() == Attributes.ComposableProxy
      );
      var target = TypeArgument(attribute, "Target");
      if (target is null) {
        context.ReportDiagnostic(Diagnostic.Create(MissingTarget, location, proxy.Name));
        return;
      }

      if (!IsPartial(proxy)) {
        context.ReportDiagnostic(Diagnostic.Create(MustBePartial, location, proxy.Name));
        return;
      }

      var containing = FirstNonPartialContainingType(proxy);
      if (containing is not null) {
        context.ReportDiagnostic(Diagnostic.Create(ContainingTypeMustBePartial, location, proxy.Name, containing.Name));
        return;
      }

      if (HasTypeParameters(proxy)) {
        context.ReportDiagnostic(Diagnostic.Create(GenericNotSupported, location, proxy.Name));
        return;
      }

      var methodName = StringArgument(attribute, "Name", target.Name);
      if (!IsValidIdentifier(methodName)) {
        context.ReportDiagnostic(Diagnostic.Create(InvalidName, location, proxy.Name, methodName ?? "null"));
        return;
      }

      var kind = Int32Argument(attribute, "Kind", 1);
      if (kind is < 0 or > 1) {
        context.ReportDiagnostic(Diagnostic.Create(InvalidKind, location, proxy.Name));
        return;
      }

      if (!PropStructApi.TryAnalyze(proxy, out var props, out var diagnostic)) {
        context.ReportDiagnostic(diagnostic);
        return;
      }
      var fields = InstanceFields(proxy);
      var targetType = target.ToDisplayString(TypeDisplayFormat);
      if (!TryBuildPropUpdates(
        context,
        fields,
        props.Assignments,
        targetType,
        out var propUpdates
      )) return;

      var requiresTracking = BooleanArgument(attribute, "RequiresTracking", true);
      var generateExtension = BooleanArgument(attribute, "Extension", true);
      var createSyntax = StringArgument(attribute, "CreateSyntax", Templates.ProxyCreate);
      var prepareSyntax = StringArgument(attribute, "PrepareSyntax", Templates.ProxyPrepare);
      var preYieldSyntax = StringArgument(attribute, "PreYieldSyntax", Templates.ProxyPreYield);
      var postYieldSyntax = StringArgument(attribute, "PostYieldSyntax", Templates.ProxyPostYield);
      var scopeCallbackSyntax = StringArgument(attribute, "ScopeCallbackSyntax", Templates.ProxyScopeCallback);

      createSyntax = createSyntax.Replace("{TYPE}", targetType);
      var source = BuildSource(
        proxy, methodName, targetType, kind == 0, requiresTracking, props, propUpdates,
        createSyntax, prepareSyntax, preYieldSyntax, postYieldSyntax, scopeCallbackSyntax,
        generateExtension,
        out var hintName
      );
      context.AddSource(hintName, source);
    }

    private static bool TryBuildPropUpdates(
      SourceProductionContext context,
      IReadOnlyList<IFieldSymbol> fields,
      IReadOnlyList<PropAssignment> assignments,
      string targetType,
      out string updates
    ) {
      var result = new StringBuilder();
      for (var index = 0; index < fields.Count; index++) {
        var field = fields[index];
        var assignment = assignments[index];
        var attribute = field.GetAttributes().FirstOrDefault(item =>
          item.AttributeClass?.ToDisplayString() == Attributes.PropProxy
        );
        var setter = attribute is null
          ? EscapeIdentifier(field.Name)
          : ConstructorStringArgument(attribute, 0);
        if (string.IsNullOrWhiteSpace(setter)) {
          context.ReportDiagnostic(
            Diagnostic.Create(
              InvalidPropProxy,
              field.Locations.FirstOrDefault() ?? Location.None,
              field.Name,
              "the setter must be a non-empty member name or function template"
            )
          );
          updates = null;
          return false;
        }

        var value = assignment.ValueExpression;
        var isFunction = attribute is not null && BooleanArgument(attribute, "Function");
        var setterCode = isFunction
          ? setter.Replace("{VALUE}", value).Replace("{TYPE}", targetType)
          : $"instance.{setter} = {value};";
        var checkEquality = attribute is not null && BooleanArgument(attribute, "CheckEquality");
        if (!checkEquality) {
          result.AppendLine(setterCode);
          continue;
        }

        var getter = StringArgument(attribute, "Getter") ?? setter;
        if (string.IsNullOrWhiteSpace(getter)) {
          context.ReportDiagnostic(
            Diagnostic.Create(
              InvalidPropProxy,
              field.Locations.FirstOrDefault() ?? Location.None,
              field.Name,
              "the getter must be a non-empty member name when equality checking is enabled"
            )
          );
          updates = null;
          return false;
        }

        var equalitySyntax =
          StringArgument(attribute, "EqualitySyntax") ??
          Templates.ProxyEquality;
        string equality;
        try {
          equality = string.Format(
            CultureInfo.InvariantCulture,
            equalitySyntax,
            "instance." + getter,
            value
          );
        } catch (FormatException exception) {
          context.ReportDiagnostic(
            Diagnostic.Create(
              InvalidPropProxy,
              field.Locations.FirstOrDefault() ?? Location.None,
              field.Name,
              "EqualitySyntax could not be formatted: " + exception.Message
            )
          );
          updates = null;
          return false;
        }

        result.Append(
          $@"if (!({equality})) {{
{Indent(setterCode, "  ")}}}
"
        );
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
      PropStructModel props,
      string propUpdates,
      string createSyntax,
      string prepareSyntax,
      string preYieldSyntax,
      string postYieldSyntax,
      string scopeCallbackSyntax,
      bool generateExtension,
      out string hintName
    ) {
      var wrapper = WrapType(
        proxy,
        "composable-proxy",
        CollectUsings(proxy)
      );
      hintName = wrapper.HintName;
      var structMembers = BuildStructMembers(
        methodName, targetType, isScope, requiresTracking, props, propUpdates, createSyntax, prepareSyntax, preYieldSyntax,
        postYieldSyntax, scopeCallbackSyntax
      );
      var extensionClass = generateExtension ? BuildExtensionClass(proxy, methodName, isScope, props) : "";
      return wrapper.Header +
             Indent(structMembers, new string(' ', wrapper.MemberDepth * 2)) +
             wrapper.Footer +
             extensionClass;
    }

    private static string BuildStructMembers(
      string name,
      string targetType,
      bool isScope,
      bool requiresTracking,
      PropStructModel props,
      string propUpdates,
      string createSyntax,
      string prepareSyntax,
      string preYieldSyntax,
      string postYieldSyntax,
      string scopeCallbackSyntax
    ) {
      var unsafeModifier = props.RequiresUnsafe ? " unsafe" : "";
      var parameters = props.ParameterParts.Count == 0 ? "" : $",\n{IndentL3}" + props.Parameters.Replace("\n", $"\n{IndentL3}");
      var requireMethod = requiresTracking ? "RequireTracked" : "RequireComposable";
      var returnType = isScope ? $"global::{Types.ScopeHandle}" : $"ref global::{Types.ElementRef}";
      var yieldCode = isScope
        ? BuildScopeYield(preYieldSyntax, postYieldSyntax, scopeCallbackSyntax)
        : BuildElementYield(preYieldSyntax, postYieldSyntax);

      return $@"private static readonly ushort _typeId =
  {GetTypeId(name)};

public static{unsafeModifier} {returnType} Compose(
  ref global::{Types.Composition} cx{parameters}
) {{
  if (!cx.AUTHORING.{requireMethod}<{targetType}>(
    _typeId,
    out var instance,
    out var retained
  )) {{
{Indent(createSyntax, "    ")}  }}
  if (!retained) {{
{Indent(prepareSyntax, "    ")}  }}
{Indent(propUpdates, "  ")}{yieldCode}}}
";
    }

    private static string BuildElementYield(
      string preYieldSyntax,
      string postYieldSyntax
    ) => $@"{Indent(preYieldSyntax, "  ")}  ref var elementRef = ref cx.AUTHORING.YieldElement(ref cx, instance);
{Indent(postYieldSyntax, "  ")}  return ref elementRef;
";

    private static string BuildScopeYield(
      string preYieldSyntax,
      string postYieldSyntax,
      string scopeCallbackSyntax
    ) => $@"{Indent(preYieldSyntax, "  ")}  var scopeHandle = cx.AUTHORING.YieldScope(
    ref cx,
    instance,
    static (
      global::{Types.BoundaryCell} cell,
      in global::{Types.ScopeHandle} handle
    ) => {{
{Indent(scopeCallbackSyntax, "      ")}    }}
  );
{Indent(postYieldSyntax, "  ")}  return scopeHandle;
";

    private static string BuildExtensionClass(
      INamedTypeSymbol proxy,
      string methodName,
      bool isScope,
      PropStructModel props
    ) {
      var namespaceName = proxy.ContainingNamespace is { IsGlobalNamespace: false } ns
        ? ns.ToDisplayString()
        : null;
      var namespaceOpen = namespaceName is null ? "" : $"namespace {namespaceName} {{\n";
      var namespaceClose = namespaceName is null ? "" : "}\n";
      var className = EscapeIdentifier(methodName) + "Extensions";
      var accessibility = EffectiveAccessibility(proxy) == Accessibility.Public ? "public" : "internal";
      var unsafeModifier = props.RequiresUnsafe ? " unsafe" : "";
      var parameters = props.ParameterParts.Count == 0 ? "" : $",\n{IndentL3}" + props.Parameters.Replace("\n",$"\n{IndentL3}");
      var returnType = isScope ? $"global::{Types.ScopeHandle}" : $"ref global::{Types.ElementRef}";
      var returnRef = isScope ? "" : "ref ";
      var proxyType = proxy.ToDisplayString(TypeDisplayFormat);
      var arguments = props.ParameterParts.Count == 0 ? "" : ", " + props.Arguments;

      return $@"
{namespaceOpen}  {accessibility} static class {className} {{
    public static{unsafeModifier} {returnType} {EscapeIdentifier(methodName)}(
      ref this global::{Types.Composition} cx{parameters}
    ) => {returnRef}{proxyType}.Compose(ref cx{arguments});
  }}
{namespaceClose}";
    }
  }
}
