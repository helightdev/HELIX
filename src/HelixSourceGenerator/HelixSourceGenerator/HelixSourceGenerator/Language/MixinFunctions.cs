using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace HELIX.SourceGen.Expressions;

public abstract class FunctionInvocation {
  protected FunctionInvocation(string name, IReadOnlyList<string> arguments, bool negated) {
    Name = name ?? throw new ArgumentNullException(nameof(name));
    Arguments = arguments ?? Array.Empty<string>();
    Negated = negated;
  }

  public string Name { get; }
  public string Argument => Arguments.Count == 0 ? null : Arguments[0];
  public IReadOnlyList<string> Arguments { get; }
  public bool Negated { get; }
}

internal abstract class FunctionDefinition {
  protected FunctionDefinition(string name, int minimumArguments, int maximumArguments, bool predicate = false) {
    Name = name;
    MinimumArguments = minimumArguments;
    MaximumArguments = maximumArguments;
    IsPredicate = predicate;
  }

  internal string Name { get; }
  internal int MinimumArguments { get; }
  internal int MaximumArguments { get; }
  internal bool IsPredicate { get; }

  internal virtual bool Validate(FunctionInvocation invocation, out string error) {
    var count = invocation.Arguments.Count;
    if (count >= MinimumArguments && count <= MaximumArguments) {
      error = null;
      return true;
    }
    if (MinimumArguments == MaximumArguments) {
      error = ":" + Name + " requires " + MinimumArguments +
        (MinimumArguments == 1 ? " argument" : " arguments");
    } else {
      error = ":" + Name + " accepts at most " + MaximumArguments +
        (MaximumArguments == 1 ? " argument" : " arguments");
    }
    return false;
  }

  internal virtual bool Invoke(
    FunctionInvocation invocation, string root, string member, ref object value, out string error
  ) {
    error = "function ':" + Name + "' cannot be used as a value transformation";
    return false;
  }
}

internal sealed class ValueFunctionDefinition : FunctionDefinition {
  internal ValueFunctionDefinition(string name, int arguments) : base(name, arguments, arguments) { }
  internal ValueFunctionDefinition(string name, int minimum, int maximum) : base(name, minimum, maximum) { }
}

internal sealed class NameFunction : FunctionDefinition {
  internal NameFunction() : base("name", 0, 0) { }

  internal override bool Invoke(
    FunctionInvocation invocation, string root, string member, ref object value, out string error
  ) {
    value = member ?? root;
    error = null;
    return true;
  }
}

internal sealed class PathFunction : FunctionDefinition {
  internal PathFunction() : base("path", 1, 1) { }

  internal override bool Invoke(
    FunctionInvocation invocation, string root, string member, ref object value, out string error
  ) {
    value = value is MixinExpressionTable table && table.TryGetValue(invocation.Argument, out var selected)
      ? selected
      : null;
    error = null;
    return true;
  }
}

internal sealed class UnwrapFunction : FunctionDefinition {
  internal UnwrapFunction() : base("unwrap", 0, 0) { }

  internal override bool Invoke(
    FunctionInvocation invocation, string root, string member, ref object value, out string error
  ) {
    value = MixinExpressionEvaluator.UnwrapValue(MixinExpressionEvaluator.Render(value));
    error = null;
    return true;
  }
}

internal sealed class RegexFunction : FunctionDefinition {
  private readonly bool _firstOnly;

  internal RegexFunction(string name, bool firstOnly) : base(name, 2, 2) {
    _firstOnly = firstOnly;
  }

  internal override bool Invoke(
    FunctionInvocation invocation, string root, string member, ref object value, out string error
  ) {
    try {
      var text = MixinExpressionEvaluator.Render(value);
      value = _firstOnly
        ? new Regex(invocation.Arguments[0]).Replace(text, invocation.Arguments[1], 1)
        : Regex.Replace(text, invocation.Arguments[0], invocation.Arguments[1]);
      error = null;
      return true;
    } catch (ArgumentException exception) {
      error = "invalid regular expression: " + exception.Message;
      return false;
    }
  }
}

internal enum TableFunctionKind { Put, Remove, Push, Pop }

internal sealed class TableFunction : FunctionDefinition {
  private readonly TableFunctionKind _kind;

  internal TableFunction(string name, int arguments, TableFunctionKind kind) : base(name, arguments, arguments) {
    _kind = kind;
  }

  internal override bool Invoke(
    FunctionInvocation invocation, string root, string member, ref object value, out string error
  ) {
    var property = (MixinExpressionProperty)invocation;
    var table = value as MixinExpressionTable ?? new MixinExpressionTable();
    switch (_kind) {
      case TableFunctionKind.Put:
        value = table.Put(MixinExpressionEvaluator.Render(property.Values[0]), property.Values[1]); break;
      case TableFunctionKind.Remove: value = table.Remove(invocation.Argument); break;
      case TableFunctionKind.Push:
        value = table.Put(table.Count.ToString(CultureInfo.InvariantCulture), property.Values[0]); break;
      case TableFunctionKind.Pop: value = table.Remove((table.Count - 1).ToString(CultureInfo.InvariantCulture)); break;
    }
    error = null;
    return true;
  }
}

internal sealed class SwitchFunction : FunctionDefinition {
  internal SwitchFunction() : base("switch", 2, 2) { }

  internal override bool Invoke(
    FunctionInvocation invocation, string root, string member, ref object value, out string error
  ) {
    var property = (MixinExpressionProperty)invocation;
    value = property.Values[MixinExpressionEvaluator.IsTruthy(value) ? 0 : 1];
    error = null;
    return true;
  }
}

internal sealed class SizeFunction : FunctionDefinition {
  internal SizeFunction() : base("size", 0, 0) { }

  internal override bool Invoke(
    FunctionInvocation invocation, string root, string member, ref object value, out string error
  ) {
    value = value switch {
      MixinExpressionTable table => table.Count.ToString(CultureInfo.InvariantCulture),
      string text => text.Length.ToString(CultureInfo.InvariantCulture), _ => "0"
    };
    error = null;
    return true;
  }
}

internal sealed class FloatTimeFunction : FunctionDefinition {
  internal FloatTimeFunction() : base("floatTime", 0, 0) { }

  internal override bool Invoke(
    FunctionInvocation invocation, string root, string member, ref object value, out string error
  ) {
    if (!(MixinExpressionEvaluator.TryConvertFloat(value, out var seconds) ||
      MixinExpressionEvaluator.TryParseFloatTime(
        value is null ? null : MixinExpressionEvaluator.UnwrapValue(MixinExpressionEvaluator.Render(value)), out seconds
      ))) {
      error = "cannot parse '" + MixinExpressionEvaluator.Render(value) + "' as a float time";
      return false;
    }
    value = MixinExpressionEvaluator.FormatFloatTime(seconds);
    error = null;
    return true;
  }
}

internal sealed class PredicateFunctionDefinition : FunctionDefinition {
  internal PredicateFunctionDefinition(string name, int arguments) : base(name, arguments, arguments, true) { }
}

internal sealed class LogicalFunctionDefinition : FunctionDefinition {
  internal LogicalFunctionDefinition(string name) : base(name, 1, int.MaxValue) { }

  internal override bool Validate(FunctionInvocation invocation, out string error) {
    if (!base.Validate(invocation, out error)) return false;
    if (invocation.Arguments.All(MixinExpressionParser.IsDynamicArgument)) return true;
    error = ":" + Name + " arguments must be dynamic boolean expressions";
    return false;
  }
}

internal static class FunctionLibrary {
  private static readonly IReadOnlyDictionary<string, FunctionDefinition> Definitions =
    new FunctionDefinition[] {
      new NameFunction(), new PathFunction(), new UnwrapFunction(),
      new RegexFunction("replace", false), new RegexFunction("replaceFirst", true),
      new SwitchFunction(), new TableFunction("put", 2, TableFunctionKind.Put),
      new ValueFunctionDefinition("makeGeneric", 1), new ValueFunctionDefinition("wire", 1),
      new TableFunction("remove", 1, TableFunctionKind.Remove),
      new TableFunction("push", 1, TableFunctionKind.Push),
      new ValueFunctionDefinition("structParams", 0, 1), new ValueFunctionDefinition("structArgs", 0, 1),
      new ValueFunctionDefinition("visibility", 0), new TableFunction("pop", 0, TableFunctionKind.Pop),
      new SizeFunction(), new FloatTimeFunction(),
      new PredicateFunctionDefinition("is", 1), new PredicateFunctionDefinition("has", 1),
      new PredicateFunctionDefinition("eq", 1), new PredicateFunctionDefinition("matches", 1),
      new PredicateFunctionDefinition("signature", 1), new PredicateFunctionDefinition("wireable", 2),
      new PredicateFunctionDefinition("exists", 0), new PredicateFunctionDefinition("isSelf", 0),
      new PredicateFunctionDefinition("ref", 0), new PredicateFunctionDefinition("in", 0),
      new PredicateFunctionDefinition("out", 0), new PredicateFunctionDefinition("inout", 0),
      new PredicateFunctionDefinition("argument", 0), new PredicateFunctionDefinition("static", 0),
      new PredicateFunctionDefinition("async", 0), new PredicateFunctionDefinition("public", 0),
      new PredicateFunctionDefinition("exposed", 0), new PredicateFunctionDefinition("top", 0),
      new PredicateFunctionDefinition("concrete", 0), new PredicateFunctionDefinition("partial", 0),
      new PredicateFunctionDefinition("generic", 0), new PredicateFunctionDefinition("struct", 0),
      new PredicateFunctionDefinition("class", 0), new PredicateFunctionDefinition("structHasEquality", 0),
      new PredicateFunctionDefinition("structNoArgs", 0), new PredicateFunctionDefinition("structAugment", 0),
      new LogicalFunctionDefinition("and"), new LogicalFunctionDefinition("or")
    }.ToDictionary(definition => definition.Name, StringComparer.Ordinal);

  internal static bool TryGet(string name, out FunctionDefinition definition) {
    return Definitions.TryGetValue(name ?? "", out definition);
  }

  internal static bool IsPredicate(string name) {
    return TryGet(name, out var definition) && definition.IsPredicate;
  }

  internal static bool TryInvoke(
    MixinExpressionProperty invocation, string root, string member, ref object value, out string error
  ) {
    if (!TryGet(invocation.Name, out var function)) {
      error = "property '" + invocation.Name + "' is not valid for @" + root;
      return false;
    }
    return function.Invoke(invocation, root, member, ref value, out error);
  }
}