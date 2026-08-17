using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HELIX.SourceGen.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static HELIX.SourceGen.GeneratorAnalysis;
using static HELIX.SourceGen.GeneratorDiagnostics.Mixins;
using static HELIX.SourceGen.GeneratorSource;
using static HELIX.SourceGen.GeneratorStrings;

namespace HELIX.SourceGen;

public sealed partial class MixinGenerator {
private static void AppendProperty(SharpStringBuilder builder, IPropertySymbol property) {
    builder.Append("public ")
      .Append(property.Type.ToDisplayString(TypeDisplayFormat))
      .Append(" ")
      .Append(EscapeIdentifier(property.Name))
      .Append(" { ");
    if (property.GetMethod is not null) builder.Append("get; ");
    if (property.SetMethod is not null) builder.Append(property.SetMethod.IsInitOnly ? "init; " : "set; ");
    builder.AppendLine("}");
  }

  private static void AppendMethod(SharpStringBuilder builder, GeneratedMethod method) {
    var declaration = method.Declaration;
    if (method.TypeParameters.Count != 0 && !declaration.EndsWith(
      ">", StringComparison.Ordinal
    )) declaration += "<" + string.Join(", ", method.TypeParameters) + ">";
    var parameters = method.Parameters.Select(item => item.Declaration).ToArray();
    builder.Append(declaration).Parameters(parameters, parameters.Length > 1);
    foreach (var constraint in method.Constraints) builder.Append(" ").Append(constraint);
    using (builder.Block()) {
      if (method.ReturnType is not null) builder.Statement(method.ReturnType + " __mixinReturnValue = default");

      var baseEmitted = !method.CallBase;
      var invocationIndex = 0;
      foreach (var contribution in method.Contributions) {
        if (!baseEmitted && contribution.Order >= 0) {
          AppendBaseCall(builder, method);
          baseEmitted = true;
        }
        AppendInvocation(builder, method, contribution, invocationIndex++);
      }
      if (!baseEmitted) AppendBaseCall(builder, method);
      if (method.ReturnType is not null) builder.Return("__mixinReturnValue");
    }
  }

  private static void AppendBaseCall(SharpStringBuilder builder, GeneratedMethod method) {
    var call = "base." + EscapeIdentifier(method.Name) + "(" +
      string.Join(", ", method.Parameters.Select(item => item.Argument)) + ")";
    builder.Statement(method.ReturnType is null ? call : "__mixinReturnValue = " + call);
  }

  private static void AppendInvocation(
    SharpStringBuilder builder,
    GeneratedMethod target,
    MixinContribution contribution,
    int invocationIndex
  ) {
    if (contribution.ExpressionResult is not null) {
      foreach (var output in contribution.ExpressionResult.Outputs) {
        if (output.Target == MixinExpressionOutputTarget.Target) builder.Statement(output.Text);
      }
      return;
    }
    List<string> before = null, after = null;
    var arguments = new List<string>(contribution.Parameters.Count);
    foreach (var parameter in contribution.Parameters) {
      var expression = BuildArgument(
        target, contribution, parameter, invocationIndex, ref before, ref after
      );
      arguments.Add(RefPrefix(parameter.Symbol.RefKind) + expression);
    }

    if (before is not null) foreach (var statement in before) builder.Statement(statement);
    var call = InvocationTarget(contribution) + "(" + string.Join(", ", arguments) + ")";
    if (contribution.Method.ReturnType.SpecialType == SpecialType.System_Boolean) {
      if (after is null)
        using (builder.If("!" + call))
          AppendEarlyReturn(builder, target);
      else {
        var continueName = "__mixinContinue" + invocationIndex.ToString(CultureInfo.InvariantCulture);
        builder.Statement("var " + continueName + " = " + call);
        foreach (var statement in after) builder.Statement(statement);
        using (builder.If("!" + continueName)) AppendEarlyReturn(builder, target);
      }
    } else {
      builder.Statement(call);
      if (after is not null) foreach (var statement in after) builder.Statement(statement);
    }
  }

  private static void AppendEarlyReturn(SharpStringBuilder builder, GeneratedMethod target) {
    if (target.ReturnType is null) builder.Statement("return");
    else builder.Return("__mixinReturnValue");
  }

  private static string BuildArgument(
    GeneratedMethod target,
    MixinContribution contribution,
    MixinParameter parameter,
    int invocationIndex,
    ref List<string> before,
    ref List<string> after
  ) {
    switch (parameter.Injection) {
      case Unmarked:
        return EscapeIdentifier(target.Parameters[parameter.Position].Name);
      case InjectThis:
        return target.IsStatic ? "null" : "this";
      case InjectReturnValue:
        return "__mixinReturnValue";
      case InjectAttribute:
        return AttributeArgument(contribution, parameter);
      case InjectDelegate:
        return MemberReference((IMethodSymbol)contribution.Source);
      case InjectTarget:
        return TargetArgument(target, contribution, parameter, invocationIndex, ref before, ref after);
      case InjectMember:
        return LocalMemberArgument(target, parameter, invocationIndex, ref before, ref after);
      case InjectParameter:
        return EscapeIdentifier(
          FindTargetParameter(
            target.Parameters, parameter.Name ?? parameter.Symbol.Name
          ).Name
        );
      default:
        return "default";
    }
  }

  private static string TargetArgument(
    GeneratedMethod target,
    MixinContribution contribution,
    MixinParameter parameter,
    int invocationIndex,
    ref List<string> before,
    ref List<string> after
  ) {
    if (contribution.Kind != ContributionKind.Attribute) return target.IsStatic ? "null" : "this";
    switch (contribution.Source) {
      case IFieldSymbol field:
        return MemberReference(field);
      case IPropertySymbol property when parameter.Symbol.RefKind == RefKind.Ref:
        var temporary = "__mixinTarget" + invocationIndex.ToString(CultureInfo.InvariantCulture) +
          "_" + parameter.PositionInMethod.ToString(CultureInfo.InvariantCulture);
        (before ??= new List<string>()).Add("var " + temporary + " = " + MemberReference(property));
        (after ??= new List<string>()).Add(MemberReference(property) + " = " + temporary);
        return temporary;
      case IPropertySymbol property:
        return MemberReference(property);
      case IMethodSymbol method:
        return Literal(method.Name);
      case INamedTypeSymbol type when parameter.Symbol.Type.SpecialType == SpecialType.System_String:
        return Literal(type.Name);
      case INamedTypeSymbol:
        return target.IsStatic ? "null" : "this";
      default:
        return "default";
    }
  }

  private static string LocalMemberArgument(
    GeneratedMethod target,
    MixinParameter parameter,
    int invocationIndex,
    ref List<string> before,
    ref List<string> after
  ) {
    var resource = FindResource(target.Resources, parameter.Name ?? parameter.Symbol.Name);
    if (resource is null) return "default";
    if (!resource.IsProperty || parameter.Symbol.RefKind != RefKind.Ref) return resource.Expression;

    var temporary = "__mixinMember" + invocationIndex.ToString(CultureInfo.InvariantCulture) +
      "_" + parameter.PositionInMethod.ToString(CultureInfo.InvariantCulture);
    (before ??= new List<string>()).Add("var " + temporary + " = " + resource.Expression);
    (after ??= new List<string>()).Add(resource.Expression + " = " + temporary);
    return temporary;
  }

  private static MixinResource FindResource(
    IReadOnlyList<MixinResource> resources,
    string name
  ) {
    MixinResource insensitive = null;
    foreach (var resource in resources) {
      if (string.Equals(resource.Name, name, StringComparison.Ordinal)) return resource;
      if (insensitive is null && string.Equals(
        resource.Name, name, StringComparison.OrdinalIgnoreCase
      )) insensitive = resource;
    }
    return insensitive;
  }

  private static string AttributeArgument(MixinContribution contribution, MixinParameter parameter) {
    var name = parameter.Name ?? parameter.Symbol.Name;
    if (TryFindAttributeConstant(
      contribution.AppliedAttribute, name, -1, out var constant
    )) return TypedConstantExpression(constant);
    return contribution.ImplicitAttribute is not null &&
      contribution.ImplicitAttribute.Values.TryGetValue(name, out var value)
        ? RoslynMixinExpressionContext.ConstantExpression(value.Value, value.Type)
        : "default";
  }

  private static bool TryFindAttributeConstant(
    AttributeData attribute,
    string name,
    int index,
    out TypedConstant constant
  ) {
    constant = default;
    if (attribute is null) return false;
    if (!string.IsNullOrEmpty(name)) {
      foreach (var argument in attribute.NamedArguments) {
        if (!string.Equals(argument.Key, name, StringComparison.OrdinalIgnoreCase)) continue;
        constant = argument.Value;
        return true;
      }
      var constructor = attribute.AttributeConstructor;
      if (constructor is not null) {
        for (var argumentIndex = 0; argumentIndex < constructor.Parameters.Length &&
          argumentIndex < attribute.ConstructorArguments.Length; argumentIndex++) {
          if (!string.Equals(
            constructor.Parameters[argumentIndex].Name, name, StringComparison.OrdinalIgnoreCase
          )) continue;
          constant = attribute.ConstructorArguments[argumentIndex];
          return true;
        }
      }
    }
    if (index < 0 || index >= attribute.ConstructorArguments.Length) return false;
    constant = attribute.ConstructorArguments[index];
    return true;
  }

  private static ITypeSymbol AttributeSourceType(
    MixinContribution contribution,
    string name,
    int index
  ) {
    var attribute = contribution.AppliedAttribute;
    if (TryFindAttributeConstant(attribute, name, index, out var constant)) return constant.Type;
    if (contribution.ImplicitAttribute is { } implicitAttribute) {
      if (!string.IsNullOrEmpty(name) && implicitAttribute.Values.TryGetValue(name, out var value)) return value.Type;
      return implicitAttribute.Type.GetMembers().FirstOrDefault(item =>
        string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase)
      ) switch {
        IFieldSymbol field => field.Type,
        IPropertySymbol property => property.Type,
        _ => null
      };
    }
    if (attribute?.AttributeClass is null || string.IsNullOrEmpty(name)) return null;
    foreach (var member in attribute.AttributeClass.GetMembers()) {
      if (!string.Equals(member.Name, name, StringComparison.OrdinalIgnoreCase)) continue;
      if (member is IFieldSymbol field) return field.Type;
      if (member is IPropertySymbol property) return property.Type;
    }
    return null;
  }

  private static string TypedConstantExpression(TypedConstant constant) {
    if (constant.IsNull) return "null";
    if (constant.Kind == TypedConstantKind.Type && constant.Value is ITypeSymbol type)
      return "typeof(" + type.ToDisplayString(TypeDisplayFormat) + ")";
    if (constant.Kind == TypedConstantKind.Array) {
      var element = (constant.Type as IArrayTypeSymbol)?.ElementType.ToDisplayString(TypeDisplayFormat) ?? "object";
      return "new " + element + "[] { " +
        string.Join(", ", constant.Values.Select(TypedConstantExpression)) + " }";
    }
    var literal = CSharpLiteral(constant.Value);
    return constant.Kind == TypedConstantKind.Enum && constant.Type is not null
      ? "(" + constant.Type.ToDisplayString(TypeDisplayFormat) + ")" + literal
      : literal;
  }

  private static string CSharpLiteral(object value) {
    if (value is null) return "null";
    return value switch {
      string text => Literal(text),
      char character => SyntaxFactory.Literal(character).ToFullString(),
      bool boolean => boolean ? "true" : "false",
      float single when float.IsNaN(single) => "global::System.Single.NaN",
      float single when float.IsPositiveInfinity(single) => "global::System.Single.PositiveInfinity",
      float single when float.IsNegativeInfinity(single) => "global::System.Single.NegativeInfinity",
      float single => single.ToString("R", CultureInfo.InvariantCulture) + "F",
      double number when double.IsNaN(number) => "global::System.Double.NaN",
      double number when double.IsPositiveInfinity(number) => "global::System.Double.PositiveInfinity",
      double number when double.IsNegativeInfinity(number) => "global::System.Double.NegativeInfinity",
      double number => number.ToString("R", CultureInfo.InvariantCulture) + "D",
      decimal number => number.ToString(CultureInfo.InvariantCulture) + "M",
      uint number => number.ToString(CultureInfo.InvariantCulture) + "U",
      long number => number.ToString(CultureInfo.InvariantCulture) + "L",
      ulong number => number.ToString(CultureInfo.InvariantCulture) + "UL",
      _ => Convert.ToString(value, CultureInfo.InvariantCulture)
    };
  }

  private static string Literal(string value) {
    return SyntaxFactory.Literal(value).ToFullString();
  }

  private static string InvocationTarget(MixinContribution contribution) {
    var genericArguments = contribution.GenericArguments.Count == 0
      ? ""
      : "<" + string.Join(
        ", ", contribution.GenericArguments.Select(item =>
          item.ToDisplayString(TypeDisplayFormat)
        )
      ) + ">";
    if (contribution.Kind == ContributionKind.Local) {
      return contribution.Method.IsStatic
        ? contribution.Method.ContainingType.ToDisplayString(TypeDisplayFormat) + "." +
        EscapeIdentifier(contribution.Method.Name) + genericArguments
        : EscapeIdentifier(contribution.Method.Name) + genericArguments;
    }
    return contribution.Method.ContainingType.ToDisplayString(TypeDisplayFormat) + "." +
      EscapeIdentifier(contribution.Method.Name) + genericArguments;
  }

  private static string MemberReference(ISymbol member) {
    var name = EscapeIdentifier(member.Name);
    return member.IsStatic
      ? member.ContainingType.ToDisplayString(TypeDisplayFormat) + "." + name
      : "this." + name;
  }

  private static string RefPrefix(RefKind kind) {
    return kind switch {
      RefKind.Ref => "ref ",
      RefKind.Out => "out ",
      RefKind.In => "in ",
      _ => ""
    };
  }

  private static IReadOnlyList<ISymbol> OrderedMembers(INamedTypeSymbol type) {
    return type.GetMembers()
      .OrderBy(
        item => item.Locations.FirstOrDefault(location => location.IsInSource)?.SourceTree?.FilePath,
        StringComparer.Ordinal
      )
      .ThenBy(SourceOrder)
      .ToArray();
  }

  private static IReadOnlyList<AttributeData> OrderedAttributes(ISymbol symbol) {
    return symbol.GetAttributes()
      .OrderBy(item => item.ApplicationSyntaxReference?.SyntaxTree.FilePath, StringComparer.Ordinal)
      .ThenBy(item => item.ApplicationSyntaxReference?.Span.Start ?? int.MaxValue)
      .ToArray();
  }

  private static IReadOnlyList<AttributeData> InheritedAttributes(
    INamedTypeSymbol type,
    string metadataName,
    bool allowMultiple,
    bool baseFirst = false
  ) {
    var result = new List<AttributeData>();
    var hierarchy = new List<INamedTypeSymbol>();
    for (var current = type; current is not null; current = current.BaseType) hierarchy.Add(current);
    if (baseFirst) hierarchy.Reverse();
    foreach (var current in hierarchy) {
      var declared = OrderedAttributes(current)
        .Where(item => IsAttribute(item, metadataName))
        .ToArray();
      if (declared.Length == 0) continue;
      result.AddRange(declared);
      if (!allowMultiple) break;
    }
    return result;
  }

  private static bool IsAttribute(AttributeData attribute, string metadataName) {
    return attribute.AttributeClass?.ToDisplayString() == metadataName;
  }

  private static IReadOnlyList<MixinResource> CollectResources(
    INamedTypeSymbol target,
    IReadOnlyList<MixinVariable> variables,
    IReadOnlyList<IPropertySymbol> properties
  ) {
    var result = new List<MixinResource>();
    var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    for (var current = target; current is not null; current = current.BaseType) {
      foreach (var member in OrderedMembers(current)) {
        if (!SymbolEqualityComparer.Default.Equals(current, target) &&
          member.DeclaredAccessibility == Accessibility.Private) continue;
        if (member.IsImplicitlyDeclared || member is not (IFieldSymbol or IPropertySymbol) ||
          !names.Add(member.Name)) continue;
        result.Add(
          new MixinResource(
            member.Name,
            MemberReference(member),
            member is IPropertySymbol property ? property.Type : ((IFieldSymbol)member).Type,
            member is IPropertySymbol,
            member.IsStatic
          )
        );
      }
    }
    foreach (var variable in variables) {
      if (names.Add(variable.Name)) {
        result.Add(
          new MixinResource(
            variable.Name, "this." + EscapeIdentifier(variable.Name), variable.Type, false, false
          )
        );
      }
    }
    foreach (var property in properties) {
      if (names.Add(property.Name)) {
        result.Add(
          new MixinResource(
            property.Name, "this." + EscapeIdentifier(property.Name), property.Type, true, false
          )
        );
      }
    }
    return result;
  }
}
