using System.Globalization;
using System.Linq;
using HelixSourceGenerator.Language.Compiler;
using static HelixSourceGenerator.Language.Functions.FunctionResults;

namespace HelixSourceGenerator.Language.Functions;

internal sealed class NameFunction : FunctionDefinition {
  internal NameFunction() : base("name", 0, 0) { }

  internal override bool Invoke(
    FunctionInvocation invocation, IMixinExpressionContext context,
    string root, string member, ref IMixinValue value, out string error
  ) {
    var typed = MixinValue.From(value, context);
    value = typed is DetachedSemanticValue or DetachedTypeValue ||
      typed.RoslynSymbol is not null || typed.RoslynType is not null
        ? MixinValue.From(typed.Name)
        : MixinValue.From(member ?? root);
    error = null;
    return true;
  }
}

internal sealed class PathFunction : FunctionDefinition {
  internal PathFunction() : base("path", 1, 1) { }

  internal override bool Invoke(
    FunctionInvocation invocation, IMixinExpressionContext context,
    string root, string member, ref IMixinValue value, out string error
  ) {
    var typed = MixinValue.From(value, context);
    value = typed.Select(invocation.Argument);
    return FunctionResult(Name, value, out error);
  }
}

internal sealed class UnwrapFunction : FunctionDefinition {
  internal UnwrapFunction() : base("unwrap", 0, 0) { }

  internal override bool Invoke(
    FunctionInvocation invocation, IMixinExpressionContext context,
    string root, string member, ref IMixinValue value, out string error
  ) {
    value = MixinValue.From(value, context).Unwrap();
    error = null;
    return true;
  }
}

internal sealed class SwitchFunction : FunctionDefinition {
  internal SwitchFunction() : base("switch", 2, 2) { }

  internal override bool Invoke(
    FunctionInvocation invocation, IMixinExpressionContext context,
    string root, string member, ref IMixinValue value, out string error
  ) {
    var property = (MixinExpressionProperty)invocation;
    var truthy = MixinValue.From(value, context).IsTruthy;
    value = property.Values[truthy ? 0 : 1];
    error = null;
    return true;
  }
}

internal sealed class SizeFunction : FunctionDefinition {
  internal SizeFunction() : base("size", 0, 0) { }

  internal override bool Invoke(
    FunctionInvocation invocation, IMixinExpressionContext context,
    string root, string member, ref IMixinValue value, out string error
  ) {
    var typed = MixinValue.From(value, context);
    value = MixinValue.From(
      typed.TryGetText(out var text)
        ? text.Length.ToString(CultureInfo.InvariantCulture)
        : typed.Value switch {
          MixinExpressionTable table => table.Count.ToString(CultureInfo.InvariantCulture),
          string rawText => rawText.Length.ToString(CultureInfo.InvariantCulture),
          _ => "0"
        }
    );
    error = null;
    return true;
  }
}

internal sealed class LogicalFunctionDefinition : FunctionDefinition {
  internal LogicalFunctionDefinition(string name) : base(name, 1, int.MaxValue) { }

  internal override bool Validate(FunctionInvocation invocation, out string error) {
    if (!base.Validate(invocation, out error)) return false;
    if (invocation is MixinExpressionProperty property &&
      property.ParsedArguments.All(item => item.BooleanExpression is not null)) return true;
    error = ":" + Name + " arguments must be dynamic boolean expressions";
    return false;
  }

  internal bool Combine(bool left, bool right) {
    return Name == "and" ? left && right : left || right;
  }
}
