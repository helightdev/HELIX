using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Hix.Roslyn;
using Hix.Runtime;

namespace Hix.Functions;

public sealed class TypeFunction() : RoslynFunctionDefinition("type", 0,
  HixValueKind.Symbol, HixValueKind.Symbol) {
  public override string Documentation => "Returns the type symbol associated with a semantic value, or null if unavailable.";
  protected override IHixValue Apply(
    HixThread context, IHixValue value,
    IReadOnlyList<IHixValue> arguments
  ) {
    return value is RoslynHixValue typed &&
      HixRoslynContext.TypeOf(typed.Value) is { } type
        ? new RoslynHixValue(type)
        : value is DetachedSemanticHixValue detached
          ? detached
          : NullHixValue.Instance;
  }
}

public sealed class FullNameFunction() : RoslynFunctionDefinition("fullName", 0,
  HixValueKind.Symbol, HixValueKind.String) {
  public override string Documentation => "Renders the associated type name with generic type parameters.";
  protected override IHixValue Apply(
    HixThread context, IHixValue value,
    IReadOnlyList<IHixValue> arguments
  ) {
    return value is RoslynHixValue typed &&
      HixRoslynContext.TypeOf(typed.Value) is { } type
        ? new LiteralHixValue(
          HixString.Dynamic(
            type.ToDisplayString(
              SymbolDisplayFormat.MinimallyQualifiedFormat.WithGenericsOptions(
                SymbolDisplayGenericsOptions.IncludeTypeParameters
              )
            )
          )
        )
        : value is DetachedSemanticHixValue detached
          ? new LiteralHixValue(detached.Render(context))
          : NullHixValue.Instance;
  }
}

public sealed class VisibilityFunction() : RoslynFunctionDefinition("visibility", 0,
  HixValueKind.Symbol, HixValueKind.String) {
  public override string Documentation => "Returns the declared accessibility of a symbol as lowercase text.";
  protected override IHixValue Apply(
    HixThread context, IHixValue value,
    IReadOnlyList<IHixValue> arguments
  ) {
    if (value is RoslynHixValue visible &&
      (visible.Value as ISymbol ?? HixRoslynContext.TypeOf(visible.Value)) is { } symbol) {
      return new LiteralHixValue(
        HixString.Dynamic(
          symbol.DeclaredAccessibility.ToString().ToLowerInvariant()
        )
      );
    }
    return context.Error("property ':visibility' is not available for this value");
  }
}

public sealed class MakeGenericFunction() : RoslynFunctionDefinition("makeGeneric", 1,
  HixValueKind.Symbol, HixValueKind.Symbol, [HixValueKind.Symbol]) {
  public override string Documentation => "Renders a generic type name using the supplied type arguments.";
  protected override IHixValue Apply(
    HixThread context, IHixValue value,
    IReadOnlyList<IHixValue> arguments
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
    return new LiteralHixValue(
      HixString.Dynamic(
        marker < 0
          ? generic + "<" + types + ">"
          : generic.Substring(0, marker) + "<" + types + ">"
      )
    );
  }
}

public abstract class AttributeFunction(string name, int arguments)
  : RoslynFunctionDefinition(name, arguments, HixValueKind.Symbol,
    name == "attributeOf" ? HixValueKind.Any : HixValueKind.Tuple,
    arguments == 0 ? [] : [HixValueKind.String]) {
  public override IReadOnlyDictionary<int, string> ArgumentReferences => ArgumentCount == 1 ? new Dictionary<int, string>() : new Dictionary<int, string> {{1, "CSharpType"}};
  protected sealed override IHixValue Apply(
    HixThread context, IHixValue value,
    IReadOnlyList<IHixValue> arguments
  ) {
    return Select(
      context, value,
      arguments.Count == 0 ? default : arguments[0].Render(context)
    );
  }

  protected abstract IHixValue Select(HixThread context, IHixValue value, HixString type);
}

public sealed class AttributesFunction() : AttributeFunction("attributes", 0) {
  public override string Documentation => "Returns attributes attached to a symbol.";
  protected override IHixValue Select(HixThread context, IHixValue value, HixString type) {
    return context.Attributes(value, default, false, false);
  }
}

public sealed class AttributesOfFunction() : AttributeFunction("attributesOf", 1) {
  public override string Documentation => "Returns attributes assignable to the specified attribute type.";
  protected override IHixValue Select(HixThread context, IHixValue value, HixString type) {
    return context.Attributes(value, type, false, false);
  }
}

public sealed class AttributesOfExactFunction() : AttributeFunction("attributesOfExact", 1) {
  public override string Documentation => "Returns attributes with exactly the specified attribute type.";
  protected override IHixValue Select(HixThread context, IHixValue value, HixString type) {
    return context.Attributes(value, type, true, false);
  }
}

public sealed class AttributeOfFunction() : AttributeFunction("attributeOf", 1) {
  public override string Documentation => "Returns the first matching attribute, or null when absent.";
  protected override IHixValue Select(HixThread context, IHixValue value, HixString type) {
    return context.Attributes(value, type, false, true);
  }
}

public sealed class MembersFunction() : RoslynFunctionDefinition("members", 0,
  HixValueKind.Symbol, HixValueKind.Tuple) {
  public override string Documentation => "Returns the members of a named type as a tuple of semantic values.";
  protected override IHixValue Apply(
    HixThread context, IHixValue value, IReadOnlyList<IHixValue> arguments
  ) {
    if (value is not RoslynHixValue { Value: INamedTypeSymbol type })
      return context.Error(":members requires a named type");
    var members = type.GetMembers().Where(item => !item.IsImplicitlyDeclared && item is not IMethodSymbol {
      MethodKind: MethodKind.PropertyGet or MethodKind.PropertySet or MethodKind.EventAdd or
          MethodKind.EventRemove or MethodKind.EventRaise
    }
      )
      .OrderBy(
        item => item.Locations.FirstOrDefault(location => location.IsInSource)?.SourceTree?.FilePath ?? "",
        StringComparer.Ordinal
      )
      .ThenBy(item => item.Locations.FirstOrDefault(location => location.IsInSource)?.SourceSpan.Start ?? int.MaxValue)
      .ThenBy(item => item.MetadataName, StringComparer.Ordinal)
      .ThenBy(item => item.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), StringComparer.Ordinal)
      .Select(item => (IHixValue)new RoslynHixValue(item));
    return new TupleHixValue(members.ToArray());
  }
}

public sealed class ParametersFunction() : RoslynFunctionDefinition("parameters", 0,
  HixValueKind.Symbol, HixValueKind.Tuple) {
  public override string Documentation => "Returns the parameters of a method or delegate as a tuple.";
  protected override IHixValue Apply(
    HixThread context, IHixValue value, IReadOnlyList<IHixValue> arguments
  ) {
    IReadOnlyList<IParameterSymbol> parameters = value switch {
      RoslynHixValue { Value: IMethodSymbol method } => method.Parameters,
      RoslynHixValue { Value: INamedTypeSymbol { TypeKind: TypeKind.Delegate, DelegateInvokeMethod: { } invoke } } =>
        invoke.Parameters,
      _ => null
    };
    if (parameters is null) return context.Error(":parameters requires a method or delegate");
    return new TupleHixValue(parameters.Select(item => (IHixValue)new RoslynHixValue(item)).ToArray());
  }
}

public sealed class NullableTypeFunction() : RoslynFunctionDefinition("nullableType", 0,
  HixValueKind.Symbol, HixValueKind.Symbol) {
  public override string Documentation => "Renders a nullable form of the type represented by the value.";
  protected override IHixValue Apply(
    HixThread context, IHixValue value, IReadOnlyList<IHixValue> arguments
  ) {
    if (value is not RoslynHixValue roslyn || HixRoslynContext.TypeOf(roslyn.Value) is not ITypeSymbol type)
      return context.Error(":nullableType requires a typed semantic value");
    var text = type.ToDisplayString(GeneratorAnalysis.TypeDisplayFormat);
    if (type.IsReferenceType ||
      type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T })
      return new LiteralHixValue(context.ResolveString(text));
    if (type.IsValueType || type is ITypeParameterSymbol { HasValueTypeConstraint: true })
      return new LiteralHixValue(context.ResolveString("global::System.Nullable<" + text + ">"));
    return context.Error(":nullableType does not support an unconstrained type parameter");
  }
}

public sealed class CSharpLiteralFunction() : RoslynFunctionDefinition("csharpLiteral", 0) {
  public override string Documentation => "Renders a constant or scalar value as a valid C# literal.";
  protected override IHixValue Apply(
    HixThread context, IHixValue value, IReadOnlyList<IHixValue> arguments
  ) {
    var text = value switch {
      NullHixValue => "null",
      RoslynHixValue { Value: TypedConstant constant } => RoslynHixValue.RenderCSharpConstant(constant),
      LiteralHixValue => Microsoft.CodeAnalysis.CSharp.SymbolDisplay.FormatLiteral(value.Render(context).Resolve(context.Strings), true),
      BooleanHixValue or NumberHixValue => value.Render(context).Resolve(context.Strings),
      _ => null
    };
    return text is null ? context.Error("csharpLiteral requires a C# constant or scalar value")
      : new LiteralHixValue(context.ResolveString(text));
  }
}
