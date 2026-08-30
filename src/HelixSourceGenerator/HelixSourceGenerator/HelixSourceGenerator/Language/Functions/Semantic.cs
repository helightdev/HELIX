using System;
using System.Collections.Generic;
using System.Linq;
using HelixSourceGenerator.Language.Compiler;
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
          ? detached with { Rendered = detached.TypeName, Name = detached.TypeName }
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
          ? new LiteralMixinValue(detached.FullName)
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
    return value is DetachedSemanticMixinValue detached
      ? new LiteralMixinValue(detached.Visibility)
      : context.Error("property ':visibility' is not available for this value");
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