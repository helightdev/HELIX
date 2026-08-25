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

namespace HELIX.SourceGen;

[Generator(LanguageNames.CSharp)]
public sealed class ComposableProxyGenerator : IIncrementalGenerator {
  public void Initialize(IncrementalGeneratorInitializationContext context) {
    var proxies = context.SyntaxProvider.ForAttributeWithMetadataName(
      Attributes.ComposableProxy,
      static (node, _) => node is StructDeclarationSyntax or ClassDeclarationSyntax,
      static (ctx, _) => (INamedTypeSymbol)ctx.TargetSymbol
    );

    context.RegisterSourceOutput(proxies, static (spc, proxy) => Generate(spc, proxy));
  }

  private static void Generate(SourceProductionContext context, INamedTypeSymbol proxy) {
    var location = LocationOf(proxy);
    var attribute = proxy.GetAttributes().First(item =>
      item.AttributeClass?.ToDisplayString() == Attributes.ComposableProxy
    );
    var classProxy = proxy.TypeKind == TypeKind.Class;
    var target = classProxy ? proxy : TypeArgument(attribute, "Target");
    if (!classProxy && target is null) {
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

    if (classProxy && !InheritsFrom(proxy, Types.VisualElement)) {
      context.ReportDiagnostic(Diagnostic.Create(ClassMustBeVisualElement, location, proxy.Name));
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

    var targetType = target.ToDisplayString(TypeDisplayFormat);
    PropStructModel props;
    IReadOnlyList<PropUpdate> propUpdates;
    if (classProxy) {
      if (!TryAnalyzeClassProxy(context, proxy, targetType, out props, out propUpdates)) return;
    } else {
      if (!PropStructApi.TryAnalyze(proxy, out props, out var diagnostic)) {
        context.ReportDiagnostic(diagnostic);
        return;
      }
      if (!TryBuildPropUpdates(
        context,
        InstanceFields(proxy),
        props.Assignments,
        targetType,
        out propUpdates
      )) return;
    }

    var requiresTracking = classProxy
      ? !Implements(proxy, Types.IComposable)
      : BooleanArgument(attribute, "RequiresTracking", true);
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

  private static bool TryAnalyzeClassProxy(
    SourceProductionContext context,
    INamedTypeSymbol proxy,
    string targetType,
    out PropStructModel props,
    out IReadOnlyList<PropUpdate> updates
  ) {
    var definitions = new List<PropDefinition>();
    var actions = new List<ClassPropAction>();
    var names = new HashSet<string>(StringComparer.Ordinal);

    foreach (var member in proxy.GetMembers().OrderBy(SourceOrder)) {
      if (member is IFieldSymbol { IsImplicitlyDeclared: false } field) {
        var attribute = Attribute(field, Attributes.Prop);
        if (attribute is null) continue;
        if (field.IsStatic) {
          ReportInvalidProp(context, field, "fields on a composable proxy class must be instance fields");
          props = null;
          updates = null;
          return false;
        }
        if (!TryAddProp(context, proxy, field, field.Type, attribute, RefKind.None, names, definitions)) {
          props = null;
          updates = null;
          return false;
        }
        actions.Add(ClassPropAction.ForMember(field, attribute, definitions.Count - 1));
        continue;
      }

      if (member is IPropertySymbol property) {
        var attribute = Attribute(property, Attributes.Prop);
        if (attribute is null) continue;
        if (property.IsStatic || property.IsIndexer) {
          ReportInvalidProp(
            context,
            property,
            property.IsIndexer
              ? "indexers cannot define composable props"
              : "properties on a composable proxy class must be instance properties"
          );
          props = null;
          updates = null;
          return false;
        }
        if (!TryAddProp(context, proxy, property, property.Type, attribute, RefKind.None, names, definitions)) {
          props = null;
          updates = null;
          return false;
        }
        actions.Add(ClassPropAction.ForMember(property, attribute, definitions.Count - 1));
        continue;
      }

      if (member is not IMethodSymbol method) continue;
      var parameterAttributes = method.Parameters
        .Select(parameter => Attribute(parameter, Attributes.Prop))
        .ToArray();
      if (!parameterAttributes.Any(attribute => attribute is not null)) continue;
      if (method.MethodKind != MethodKind.Ordinary) {
        ReportInvalidMethod(context, method, "it is not an ordinary method");
        props = null;
        updates = null;
        return false;
      }
      if (method.IsStatic) {
        ReportInvalidMethod(context, method, "it is static; prop methods must be instance methods");
        props = null;
        updates = null;
        return false;
      }
      if (method.IsGenericMethod) {
        ReportInvalidMethod(context, method, "generic prop methods are not supported");
        props = null;
        updates = null;
        return false;
      }
      if (method.ExplicitInterfaceImplementations.Length != 0) {
        ReportInvalidMethod(context, method, "explicit interface implementations are not supported");
        props = null;
        updates = null;
        return false;
      }
      if (parameterAttributes.Any(attribute => attribute is null)) {
        ReportInvalidMethod(context, method, "every parameter must be marked [Prop]");
        props = null;
        updates = null;
        return false;
      }
      if (method.Parameters.Any(parameter => parameter.RefKind is not (RefKind.None or RefKind.In))) {
        ReportInvalidMethod(context, method, "ref and out parameters are not supported");
        props = null;
        updates = null;
        return false;
      }

      var start = definitions.Count;
      for (var index = 0; index < method.Parameters.Length; index++) {
        var parameter = method.Parameters[index];
        if (!TryAddProp(
          context,
          proxy,
          parameter,
          parameter.Type,
          parameterAttributes[index],
          parameter.RefKind,
          names,
          definitions
        )) {
          props = null;
          updates = null;
          return false;
        }
      }
      actions.Add(ClassPropAction.ForMethod(method, start, method.Parameters.Length));
    }

    if (!PropStructApi.TryAnalyzeProps(definitions, out props, out var diagnostic)) {
      context.ReportDiagnostic(diagnostic);
      updates = null;
      return false;
    }

    var result = new List<PropUpdate>(actions.Count);
    foreach (var action in actions) {
      if (action.Method is not null) {
        var arguments = new string[action.Count];
        for (var index = 0; index < action.Count; index++) {
          var parameter = action.Method.Parameters[index];
          arguments[index] =
            (parameter.RefKind == RefKind.In ? "in " : "") +
            props.Assignments[action.Start + index].ValueExpression;
        }
        result.Add(
          new PropUpdate(
            $"instance.{EscapeIdentifier(action.Method.Name)}({string.Join(", ", arguments)});",
            null
          )
        );
        continue;
      }

      if (!TryBuildPropUpdate(
        context,
        action.Member,
        action.Attribute,
        props.Assignments[action.Start],
        targetType,
        true,
        out var update
      )) {
        updates = null;
        return false;
      }
      result.Add(update);
    }

    updates = result;
    return true;
  }

  private static bool TryAddProp(
    SourceProductionContext context,
    INamedTypeSymbol proxy,
    ISymbol symbol,
    ITypeSymbol type,
    AttributeData attribute,
    RefKind refKind,
    ISet<string> names,
    ICollection<PropDefinition> definitions
  ) {
    if (IsGeneratedName(symbol.Name)) {
      ReportInvalidProp(context, symbol, "the name is reserved by the generated composition method");
      return false;
    }
    if (!names.Add(symbol.Name)) {
      context.ReportDiagnostic(
        Diagnostic.Create(
          DuplicatePropName,
          LocationOf(symbol),
          symbol.Name,
          proxy.Name
        )
      );
      return false;
    }
    definitions.Add(new PropDefinition(symbol, type, symbol.Name, attribute, refKind));
    return true;
  }

  private static bool IsGeneratedName(string name) {
    return name is
      "cx" or "instance" or "retained" or "elementRef" or "scopeHandle" or "cell" or "handle" or "_typeId";
  }

  private static void ReportInvalidProp(
    SourceProductionContext context,
    ISymbol symbol,
    string reason
  ) {
    context.ReportDiagnostic(Diagnostic.Create(InvalidProp, LocationOf(symbol), symbol.Name, reason));
  }

  private static void ReportInvalidMethod(
    SourceProductionContext context,
    IMethodSymbol method,
    string reason
  ) {
    context.ReportDiagnostic(Diagnostic.Create(InvalidMethod, LocationOf(method), method.Name, reason));
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
      var attribute = Attribute(field, Attributes.Prop);
      if (!TryBuildPropUpdate(
        context,
        field,
        attribute,
        assignments[index],
        targetType,
        false,
        out var update
      )) {
        updates = null;
        return false;
      }
      result.Add(update);
    }

    updates = result;
    return true;
  }

  private static bool TryBuildPropUpdate(
    SourceProductionContext context,
    ISymbol member,
    AttributeData attribute,
    PropAssignment assignment,
    string targetType,
    bool validateClassMember,
    out PropUpdate update
  ) {
    var defaultSetter = EscapeIdentifier(member.Name);
    var function = attribute is null ? null : StringArgument(attribute, PropArguments.ProxyFunction);
    var setter = attribute is null
      ? defaultSetter
      : StringArgument(attribute, PropArguments.ProxySetter) ?? defaultSetter;
    if (function is null && string.IsNullOrWhiteSpace(setter)) {
      ReportInvalidProp(
        context,
        member,
        "ProxySetter must be a non-empty member name when ProxyFunction is not defined"
      );
      update = default;
      return false;
    }
    if (validateClassMember && function is null && setter == defaultSetter) {
      if (member is IFieldSymbol { IsReadOnly: true } or IFieldSymbol { IsConst: true }) {
        ReportInvalidProp(context, member, "a readonly field requires ProxySetter or ProxyFunction");
        update = default;
        return false;
      }
      if (member is IPropertySymbol property &&
        (property.SetMethod is null || property.SetMethod.IsInitOnly)) {
        ReportInvalidProp(context, member, "a property without a mutable setter requires ProxySetter or ProxyFunction");
        update = default;
        return false;
      }
    }

    var value = assignment.ValueExpression;
    var setterCode = function is not null
      ? function.Replace("{VALUE}", value).Replace("{TYPE}", targetType)
      : $"instance.{setter} = {value};";
    var checkEquality = attribute is not null && BooleanArgument(attribute, PropArguments.ProxyEquality);
    if (!checkEquality) {
      update = new PropUpdate(setterCode, null);
      return true;
    }

    var getter = StringArgument(attribute, PropArguments.ProxyGetter) ?? setter;
    if (string.IsNullOrWhiteSpace(getter)) {
      ReportInvalidProp(
        context,
        member,
        "ProxyGetter must be a non-empty member name when ProxyEquality is enabled"
      );
      update = default;
      return false;
    }

    var equalitySyntax =
      StringArgument(attribute, PropArguments.EqualitySyntax) ??
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
      ReportInvalidProp(
        context,
        member,
        "EqualitySyntax could not be formatted: " + exception.Message
      );
      update = default;
      return false;
    }

    update = new PropUpdate(setterCode, equality);
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
      builder => AppendProxyMembers(
        builder, methodName, targetType, isScope, requiresTracking, props, propUpdates, createSyntax, prepareSyntax,
        preYieldSyntax, postYieldSyntax, scopeCallbackSyntax
      ),
      generateExtension
        ? builder => AppendExtensionClass(builder, proxy, methodName, isScope, props)
        : null
    );
  }

  private static void AppendProxyMembers(
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
      .Return("elementRef", true);
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
      else {
        using (builder.If($"!({update.Equality})"))
          builder.AppendCode(update.SetterCode);
      }
    }
  }

  private readonly struct ClassPropAction {
    private ClassPropAction(
      ISymbol member,
      IMethodSymbol method,
      AttributeData attribute,
      int start,
      int count
    ) {
      Member = member;
      Method = method;
      Attribute = attribute;
      Start = start;
      Count = count;
    }

    internal static ClassPropAction ForMember(ISymbol member, AttributeData attribute, int index) {
      return new ClassPropAction(member, null, attribute, index, 1);
    }

    internal static ClassPropAction ForMethod(IMethodSymbol method, int start, int count) {
      return new ClassPropAction(null, method, null, start, count);
    }

    internal ISymbol Member { get; }
    internal IMethodSymbol Method { get; }
    internal AttributeData Attribute { get; }
    internal int Start { get; }
    internal int Count { get; }
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
