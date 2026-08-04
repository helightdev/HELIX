using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
      out IReadOnlyList<PropUpdate> updates
    ) {
      var result = new List<PropUpdate>(fields.Count);
      for (var index = 0; index < fields.Count; index++) {
        var field = fields[index];
        var assignment = assignments[index];
        var attribute = Attribute(field, Attributes.Prop);
        var defaultSetter = EscapeIdentifier(field.Name);
        var function = attribute is null ? null : StringArgument(attribute, "ProxyFunction");
        var setter = attribute is null
          ? defaultSetter
          : StringArgument(attribute, "ProxySetter") ?? defaultSetter;
        if (function is null && string.IsNullOrWhiteSpace(setter)) {
          context.ReportDiagnostic(
            Diagnostic.Create(
              InvalidProp,
              field.Locations.FirstOrDefault() ?? Location.None,
              field.Name,
              "ProxySetter must be a non-empty member name when ProxyFunction is not defined"
            )
          );
          updates = null;
          return false;
        }

        var value = assignment.ValueExpression;
        var setterCode = function is not null
          ? function.Replace("{VALUE}", value).Replace("{TYPE}", targetType)
          : $"instance.{setter} = {value};";
        var checkEquality = attribute is not null && BooleanArgument(attribute, "ProxyEquality");
        if (!checkEquality) {
          result.Add(new PropUpdate(setterCode, null));
          continue;
        }

        var getter = StringArgument(attribute, "ProxyGetter") ?? setter;
        if (string.IsNullOrWhiteSpace(getter)) {
          context.ReportDiagnostic(
            Diagnostic.Create(
              InvalidProp,
              field.Locations.FirstOrDefault() ?? Location.None,
              field.Name,
              "ProxyGetter must be a non-empty member name when ProxyEquality is enabled"
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
              InvalidProp,
              field.Locations.FirstOrDefault() ?? Location.None,
              field.Name,
              "EqualitySyntax could not be formatted: " + exception.Message
            )
          );
          updates = null;
          return false;
        }

        result.Add(new PropUpdate(setterCode, equality));
      }

      updates = result;
      return true;
    }

    private static string BuildSource(
      INamedTypeSymbol proxy,
      string methodName, string targetType, bool isScope, bool requiresTracking,
      PropStructModel props, IReadOnlyList<PropUpdate> propUpdates,
      string createSyntax, string prepareSyntax, string preYieldSyntax, string postYieldSyntax,
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
      return wrapper.Build(
        builder => AppendStructMembers(
          builder, methodName, targetType, isScope, requiresTracking, props, propUpdates, createSyntax, prepareSyntax,
          preYieldSyntax, postYieldSyntax, scopeCallbackSyntax
        ),
        generateExtension
          ? builder => AppendExtensionClass(builder, proxy, methodName, isScope, props)
          : null
      );
    }

    private static void AppendStructMembers(
      SharpStringBuilder builder,
      string name, string targetType, bool isScope, bool requiresTracking,
      PropStructModel props, IReadOnlyList<PropUpdate> propUpdates,
      string createSyntax, string prepareSyntax, string preYieldSyntax, string postYieldSyntax,
      string scopeCallbackSyntax
    ) {
      var unsafeModifier = props.RequiresUnsafe ? " unsafe" : "";
      var requireMethod = requiresTracking ? "RequireTracked" : "RequireComposable";
      var returnType = isScope ? $"global::{Types.ScopeHandle}" : $"ref global::{Types.ElementRef}";

      builder.Field("private static readonly", "ushort", "_typeId", GetTypeId(name))
        .BlankLine();
      using (builder.Method(
        $"public static{unsafeModifier} {returnType} Compose",
        props.ParameterParts.Prepend($"ref global::{Types.Composition} cx")
      )) {
        builder.Append($"if (!cx.AUTHORING.{requireMethod}<{targetType}>")
          .AppendDelimitedList(["_typeId", "out var instance", "out var retained"], multiline: true)
          .Append(")");
        using (builder.Block()) builder.AppendCode(createSyntax);
        using (builder.If("!retained")) builder.AppendCode(prepareSyntax);
        AppendPropUpdates(builder, propUpdates);
        if (isScope) AppendScopeYield(builder, preYieldSyntax, postYieldSyntax, scopeCallbackSyntax);
        else AppendElementYield(builder, preYieldSyntax, postYieldSyntax);
      }
      props.Equality.AppendMembers(builder);
    }

    private static void AppendElementYield(SharpStringBuilder builder, string preYieldSyntax, string postYieldSyntax) {
      builder.AppendCode(preYieldSyntax)
        .Statement("ref var elementRef = ref cx.AUTHORING.YieldElement(ref cx, instance)")
        .AppendCode(postYieldSyntax)
        .Return("elementRef", byRef: true);
    }

    private static void AppendScopeYield(
      SharpStringBuilder builder, string preYieldSyntax, string postYieldSyntax, string scopeCallbackSyntax
    ) {
      builder.AppendCode(preYieldSyntax)
        .Append("var scopeHandle = cx.AUTHORING.YieldScope");
      using (builder.Delimited("(", ")", closeLine: false)) {
        builder.AppendLine("ref cx,")
          .AppendLine("instance,")
          .Append("static ")
          .Parameters(
            [$"global::{Types.BoundaryCell} cell", $"in global::{Types.ScopeHandle} handle"]
          )
          .Append(" =>");
        using (builder.Block()) builder.AppendCode(scopeCallbackSyntax);
      }
      builder.AppendLine(";")
        .AppendCode(postYieldSyntax)
        .Return("scopeHandle");
    }

    private static void AppendExtensionClass(
      SharpStringBuilder builder,
      INamedTypeSymbol proxy,
      string methodName,
      bool isScope,
      PropStructModel props
    ) {
      var namespaceName = proxy.ContainingNamespace is { IsGlobalNamespace: false } ns
        ? ns.ToDisplayString()
        : null;
      var className = EscapeIdentifier(methodName) + "Extensions";
      var accessibility = EffectiveAccessibility(proxy) == Accessibility.Public ? "public" : "internal";
      var unsafeModifier = props.RequiresUnsafe ? " unsafe" : "";
      var returnType = isScope ? $"global::{Types.ScopeHandle}" : $"ref global::{Types.ElementRef}";
      var returnRef = isScope ? "" : "ref ";
      var proxyType = proxy.ToDisplayString(TypeDisplayFormat);

      builder.BlankLine();
      using (builder.Namespace(namespaceName)) {
        using (builder.Type($"{accessibility} static class {className}")) {
          builder.Append($"public static{unsafeModifier} {returnType} {EscapeIdentifier(methodName)}")
            .Parameters(props.ParameterParts.Prepend($"ref this global::{Types.Composition} cx"))
            .Append(" => ")
            .Append(returnRef)
            .Append(proxyType)
            .Append(".Compose")
            .Arguments(props.ArgumentParts.Prepend("ref cx"))
            .AppendLine(";");
        }
      }
    }

    private static void AppendPropUpdates(SharpStringBuilder builder, IEnumerable<PropUpdate> updates) {
      foreach (var update in updates) {
        if (update.Equality is null) builder.AppendCode(update.SetterCode);
        else
          using (builder.If($"!({update.Equality})"))
            builder.AppendCode(update.SetterCode);
      }
    }

    private readonly struct PropUpdate {
      internal PropUpdate(string setterCode, string equality) {
        SetterCode = setterCode;
        Equality = equality;
      }

      internal string SetterCode { get; }
      internal string Equality { get; }
    }
  }
}
