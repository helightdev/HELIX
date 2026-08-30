using System;
using System.Collections.Generic;
using System.Linq;
using HelixSourceGenerator.Language.Compiler;
using HelixSourceGenerator.Shared;
using Microsoft.CodeAnalysis;

namespace HelixSourceGenerator.Language.Functions;

internal sealed class TypeFunction() : EvaluatedFunctionDefinition("type", 0, 0) {
  protected override IMixinValue Apply(
    ExecutionContext context, IMixinValue value,
    IReadOnlyList<IMixinValue> arguments
  ) {
    return value is RoslynMixinValue typed &&
      RoslynMixinContext.TypeOf(typed.Value) is { } type
        ? new RoslynMixinValue(type)
        : value is DetachedSemanticMixinValue detached
          ? detached
          : NullMixinValue.Instance;
  }
}

internal sealed class FullNameFunction() : EvaluatedFunctionDefinition("fullName", 0, 0) {
  protected override IMixinValue Apply(
    ExecutionContext context, IMixinValue value,
    IReadOnlyList<IMixinValue> arguments
  ) {
    return value is RoslynMixinValue typed &&
      RoslynMixinContext.TypeOf(typed.Value) is { } type
        ? new LiteralMixinValue(
          ExecutionContext.Dynamic(
            type.ToDisplayString(
              SymbolDisplayFormat.MinimallyQualifiedFormat.WithGenericsOptions(
                SymbolDisplayGenericsOptions.IncludeTypeParameters
              )
            )
          )
        )
        : value is DetachedSemanticMixinValue detached
          ? new LiteralMixinValue(detached.Render(context))
          : NullMixinValue.Instance;
  }
}

internal sealed class VisibilityFunction() : EvaluatedFunctionDefinition("visibility", 0, 0) {
  protected override IMixinValue Apply(
    ExecutionContext context, IMixinValue value,
    IReadOnlyList<IMixinValue> arguments
  ) {
    if (value is RoslynMixinValue visible &&
      (visible.Value as ISymbol ?? RoslynMixinContext.TypeOf(visible.Value)) is { } symbol) {
      return new LiteralMixinValue(
        ExecutionContext.Dynamic(
          symbol.DeclaredAccessibility.ToString().ToLowerInvariant()
        )
      );
    }
    return context.Error("property ':visibility' is not available for this value");
  }
}

internal sealed class MakeGenericFunction() : EvaluatedFunctionDefinition("makeGeneric", 1, 1) {
  protected override IMixinValue Apply(
    ExecutionContext context, IMixinValue value,
    IReadOnlyList<IMixinValue> arguments
  ) {
    var generic = context.Unwrap(value).Render(context).Resolve(context.Strings);
    if (!generic.StartsWith("global::", StringComparison.Ordinal)) generic = "global::" + generic;
    var types = string.Join(
      ", ", arguments.Select(item => {
          var argument = item.Render(context).Resolve(context.Strings);
          return argument.Contains(".") && !argument.StartsWith("global::", StringComparison.Ordinal)
            ? "global::" + argument
            : argument;
        }
      )
    );
    var marker = generic.IndexOf('<');
    return new LiteralMixinValue(
      ExecutionContext.Dynamic(
        marker < 0
          ? generic + "<" + types + ">"
          : generic.Substring(0, marker) + "<" + types + ">"
      )
    );
  }
}

internal abstract class AttributeFunction(string name, int arguments)
  : EvaluatedFunctionDefinition(name, arguments, arguments) {
  protected sealed override IMixinValue Apply(
    ExecutionContext context, IMixinValue value,
    IReadOnlyList<IMixinValue> arguments
  ) {
    return Select(
      context, value,
      arguments.Count == 0 ? default : arguments[0].Render(context)
    );
  }

  protected abstract IMixinValue Select(ExecutionContext context, IMixinValue value, MixinString type);
}

internal sealed class AttributesFunction() : AttributeFunction("attributes", 0) {
  protected override IMixinValue Select(ExecutionContext context, IMixinValue value, MixinString type) {
    return context.Attributes(value, default, false, false);
  }
}

internal sealed class AttributesOfFunction() : AttributeFunction("attributesOf", 1) {
  protected override IMixinValue Select(ExecutionContext context, IMixinValue value, MixinString type) {
    return context.Attributes(value, type, false, false);
  }
}

internal sealed class AttributesOfExactFunction() : AttributeFunction("attributesOfExact", 1) {
  protected override IMixinValue Select(ExecutionContext context, IMixinValue value, MixinString type) {
    return context.Attributes(value, type, true, false);
  }
}

internal sealed class AttributeOfFunction() : AttributeFunction("attributeOf", 1) {
  protected override IMixinValue Select(ExecutionContext context, IMixinValue value, MixinString type) {
    return context.Attributes(value, type, false, true);
  }
}

internal sealed class MembersFunction() : EvaluatedFunctionDefinition("members", 0, 0) {
  protected override IMixinValue Apply(
    ExecutionContext context, IMixinValue value, IReadOnlyList<IMixinValue> arguments
  ) {
    if (value is not RoslynMixinValue { Value: INamedTypeSymbol type })
      return context.Error(":members requires a named type");
    var members = type.GetMembers().Where(item => !item.IsImplicitlyDeclared && item is not IMethodSymbol {
        MethodKind: MethodKind.PropertyGet or MethodKind.PropertySet or MethodKind.EventAdd or
        MethodKind.EventRemove or MethodKind.EventRaise
      })
      .OrderBy(item => item.Locations.FirstOrDefault(location => location.IsInSource)?.SourceTree?.FilePath ?? "",
        StringComparer.Ordinal)
      .ThenBy(item => item.Locations.FirstOrDefault(location => location.IsInSource)?.SourceSpan.Start ?? int.MaxValue)
      .ThenBy(item => item.MetadataName, StringComparer.Ordinal)
      .ThenBy(item => item.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), StringComparer.Ordinal)
      .Select((item, index) => new KeyValuePair<MixinString, IMixinValue>(
        context.ResolveString(index.ToString()), new RoslynMixinValue(item)
      ));
    return new MixinTableValue(members.ToArray());
  }
}

internal sealed class ParametersFunction() : EvaluatedFunctionDefinition("parameters", 0, 0) {
  protected override IMixinValue Apply(
    ExecutionContext context, IMixinValue value, IReadOnlyList<IMixinValue> arguments
  ) {
    IReadOnlyList<IParameterSymbol> parameters = value switch {
      RoslynMixinValue { Value: IMethodSymbol method } => method.Parameters,
      RoslynMixinValue { Value: INamedTypeSymbol { TypeKind: TypeKind.Delegate, DelegateInvokeMethod: { } invoke } } =>
        invoke.Parameters,
      _ => null
    };
    if (parameters is null) return context.Error(":parameters requires a method or delegate");
    return new MixinTableValue(parameters.Select((item, index) =>
      new KeyValuePair<MixinString, IMixinValue>(
        context.ResolveString(index.ToString()), new RoslynMixinValue(item)
      )
    ).ToArray());
  }
}

internal sealed class NullableTypeFunction() : EvaluatedFunctionDefinition("nullableType", 0, 0) {
  protected override IMixinValue Apply(
    ExecutionContext context, IMixinValue value, IReadOnlyList<IMixinValue> arguments
  ) {
    if (value is not RoslynMixinValue roslyn || RoslynMixinContext.TypeOf(roslyn.Value) is not ITypeSymbol type)
      return context.Error(":nullableType requires a typed semantic value");
    var text = type.ToDisplayString(GeneratorAnalysis.TypeDisplayFormat);
    if (type.IsReferenceType || type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T })
      return new LiteralMixinValue(context.ResolveString(text));
    if (type.IsValueType || type is ITypeParameterSymbol { HasValueTypeConstraint: true })
      return new LiteralMixinValue(context.ResolveString("global::System.Nullable<" + text + ">"));
    return context.Error(":nullableType does not support an unconstrained type parameter");
  }
}

internal sealed class CSharpLiteralFunction() : EvaluatedFunctionDefinition("csharpLiteral", 0, 0) {
  protected override IMixinValue Apply(
    ExecutionContext context, IMixinValue value, IReadOnlyList<IMixinValue> arguments
  ) {
    return value is RoslynMixinValue { Value: TypedConstant constant }
      ? new LiteralMixinValue(context.ResolveString(RoslynMixinValue.RenderCSharpConstant(constant)))
      : value;
  }
}
