using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HelixSourceGenerator.Language.Compiler;
using Microsoft.CodeAnalysis;
using System.Text.RegularExpressions;

namespace HelixSourceGenerator.Language;

public enum MixinExpressionRoot {
  Target, This, Attribute, Argument, Variable, Local, True, False, Null, Table, Parameter, Carry
}

public enum MixinExpressionOutputTarget {
  Target, Class, File, Extends, Implements, Injection, Annotation, Using, Mixin
}

public readonly record struct MixinSourceLocation(int Program, int Line);

public sealed record MixinExpressionLog(string Text = "", int Line = -1, bool IsHint = false);
public sealed record MixinExpressionPreparedLog(string Text, int Line, int ProgramIndex);
public sealed record MixinExpressionValidationResult(bool Success, string Error, int ErrorLine);

public sealed class MixinExpressionOutput {
  internal MixinExpressionOutput(
    MixinExpressionOutputTarget target, IReadOnlyList<MixinString> segments,
    MixinStringPool strings, MixinString injectionTarget = default, int injectionPriority = 0
  ) {
    Target = target;
    Segments = segments ?? [];
    Strings = strings;
    InjectionTargetValue = injectionTarget;
    InjectionPriority = injectionPriority;
  }

  public MixinExpressionOutput(MixinExpressionOutputTarget target, string text, string injectionTarget = null,
    int injectionPriority = 0) {
    var strings = new MixinStringPoolBuilder().Freeze();
    Target = target;
    Strings = strings;
    Segments = [MixinString.Dynamic(text ?? "")];
    InjectionTargetValue = MixinString.Dynamic(injectionTarget ?? "");
    InjectionPriority = injectionPriority;
  }

  public MixinExpressionOutputTarget Target { get; }
  public string Text => string.Concat(Segments.Select(item => item.Resolve(Strings)));
  public string InjectionTarget => InjectionTargetValue.Resolve(Strings);
  public int InjectionPriority { get; }
  internal IReadOnlyList<MixinString> Segments { get; }
  internal MixinStringPool Strings { get; }
  internal MixinString InjectionTargetValue { get; }
  internal bool IsEmpty => Segments.All(item => string.IsNullOrEmpty(item.Resolve(Strings)));
  internal string Resolve(MixinString value) => value.Resolve(Strings);
  internal MixinExpressionOutput Retarget(MixinExpressionOutputTarget target) =>
    new(target, Segments, Strings, InjectionTargetValue, InjectionPriority);
}

public sealed class MixinExpressionResult {
  internal MixinExpressionResult(bool success, string error, int errorLine,
    IReadOnlyList<MixinExpressionOutput> outputs, IReadOnlyList<MixinExpressionLog> logs = null,
    IReadOnlyDictionary<string, object> variables = null, int executedOperations = 0,
    double executionMilliseconds = 0) {
    Success = success; Error = error; ErrorLine = errorLine; Outputs = outputs ?? []; Logs = logs ?? [];
    Variables = variables ?? new Dictionary<string, object>(); ExecutedOperations = executedOperations;
    ExecutionMilliseconds = executionMilliseconds;
  }
  public bool Success { get; }
  public string Error { get; }
  public int ErrorLine { get; }
  public IReadOnlyList<MixinExpressionOutput> Outputs { get; }
  public IReadOnlyList<MixinExpressionLog> Logs { get; }
  public IReadOnlyDictionary<string, object> Variables { get; }
  public int ExecutedOperations { get; }
  public double ExecutionMilliseconds { get; }
}

public abstract class ExecutionContext {
  protected ExecutionContext(MixinStringPool strings) {
    Strings = strings ?? throw new ArgumentNullException(nameof(strings));
  }
  public MixinStringPool Strings { get; internal set; }
  internal MixinValueDictionary Locals { get; } = new();
  internal MixinValueDictionary Variables { get; } = new();
  internal MixinValueDictionary Carries { get; } = new();
  internal IMixinValue Parameter { get; set; } = NullMixinValue.Instance;
  internal Func<ProgramFunctionMixinValue, IMixinValue, IMixinValue> ProgramInvoker { get; set; }

  public IMixinValue Resolve(MixinExpressionRoot root, MixinString member) {
    switch (root) {
      case MixinExpressionRoot.True: return BooleanMixinValue.True;
      case MixinExpressionRoot.False: return BooleanMixinValue.False;
      case MixinExpressionRoot.Null: return NullMixinValue.Instance;
      case MixinExpressionRoot.Local:
        return Locals.TryGetValue(member, out var local)
          ? local : NullMixinValue.Instance;
      case MixinExpressionRoot.Variable:
        return Variables.TryGetValue(member, out var variable)
          ? variable : NullMixinValue.Instance;
      case MixinExpressionRoot.Parameter:
        return string.IsNullOrEmpty(member.Resolve(Strings))
          ? Parameter : Parameter.Select(this, member);
      case MixinExpressionRoot.Table: return MixinTableValue.Empty;
      case MixinExpressionRoot.Carry:
        return Carries.TryGetValue(member, out var carry)
          ? carry : Error("unknown carried value");
      default: return ResolveHost(root, member);
    }
  }

  protected abstract IMixinValue ResolveHost(MixinExpressionRoot root, MixinString member);
  public IMixinValue Invoke(FunctionDefinition function, IMixinValue instance,
    IReadOnlyList<IMixinValue> arguments, bool negated) =>
    function.Invoke(this, instance, arguments, negated);
  public IMixinValue Evaluate(IMixinValue value) => value switch {
    RootMixinValue root => Evaluate(root.Resolve(this)),
    InvokeMixinValue invocation => Evaluate(invocation.Resolve(this)),
    InterpolationMixinValue interpolation => EvaluateInterpolation(interpolation),
    AllMixinValue all => EvaluateAll(all),
    TableTransformMixinValue transform => EvaluateTransform(transform),
    _ => value ?? NullMixinValue.Instance
  };
  private IMixinValue EvaluateInterpolation(InterpolationMixinValue interpolation) {
    var parts = interpolation.Parts.Select(Evaluate).ToArray();
    var error = parts.OfType<ErrorMixinValue>().FirstOrDefault();
    return error is not null ? error : new LiteralMixinValue(Dynamic(string.Concat(
      parts.Select(item => item.Render(this).Resolve(Strings))
    )));
  }
  private IMixinValue EvaluateTransform(TableTransformMixinValue transform) {
    if (ProgramInvoker is null) return Error("program function invocation is not available");
    var entries = new List<KeyValuePair<MixinString, IMixinValue>>();
    foreach (var item in transform.Table.Entries) {
      IMixinValue parameter = transform.Kind == TableTransformKind.MapValues
        ? item.Value
        : new MixinTableValue([
          new KeyValuePair<MixinString, IMixinValue>(Intern("k"), new LiteralMixinValue(item.Key)),
          new KeyValuePair<MixinString, IMixinValue>(Intern("v"), item.Value)
        ]);
      var result = ProgramInvoker(transform.Function, parameter);
      if (result is ErrorMixinValue) return result;
      if (transform.Kind == TableTransformKind.Filter) {
        if (result.IsTruthy(this)) entries.Add(item);
      } else entries.Add(new KeyValuePair<MixinString, IMixinValue>(item.Key, result));
    }
    return new MixinTableValue(entries);
  }
  private IMixinValue EvaluateAll(AllMixinValue all) {
    foreach (var item in all.Values) {
      var value = Evaluate(item);
      if (value is ErrorMixinValue) return value;
      if (!value.IsTruthy(this)) return BooleanMixinValue.False;
    }
    return all.Values.Count == 0 ? BooleanMixinValue.False : BooleanMixinValue.True;
  }
  public virtual MixinString Render(IMixinValue value) => value.Render(this);
  public virtual MixinString NameOf(IMixinValue value) => value is DetachedSemanticMixinValue detached
    ? detached.Name : value.Render(this);
  public virtual IMixinValue Unwrap(IMixinValue value) => value;
  public virtual bool IsType(IMixinValue value, MixinString type) {
    if (value is not DetachedSemanticMixinValue detached) return false;
    var expected = type.Resolve(Strings).Replace("global::", "");
    return detached.AssignableTypes.Any(item => {
      var candidate = item.Resolve(Strings).Replace("global::", "");
      return candidate == expected || candidate.Split('.', '+').LastOrDefault() == expected;
    });
  }
  public virtual IMixinValue Attributes(IMixinValue value, MixinString type, bool exact, bool first) =>
    first ? NullMixinValue.Instance : MixinTableValue.Empty;
  public virtual IMixinValue InvokeHostDirective(string name, IReadOnlyList<IMixinValue> arguments,
    IMixinValue operand) => Error("directive runtime is not installed: " + name);
  public virtual IMixinValue InvokeHostFunction(string name, IMixinValue value,
    IReadOnlyList<IMixinValue> arguments) => Error("function implementation is not installed: " + name);
  public virtual bool HasTrait(IMixinValue value, MixinString trait) =>
    value is DetachedSemanticMixinValue detached && detached.Traits.Any(item =>
      string.Equals(item.Resolve(Strings), trait.Resolve(Strings), StringComparison.Ordinal));
  internal virtual object UnlinkSnapshot(IMixinValue value, bool includeMembers) => value.Unlink(this);
  public MixinString Intern(string value) => Strings.Intern(value);
  public static MixinString Dynamic(string value) => MixinString.Dynamic(value ?? "");
  public ErrorMixinValue Error(string value) => new(Dynamic(value));
}

public interface IMixinValue : IEquatable<IMixinValue> {
  bool IsTruthy(ExecutionContext context);
  MixinString Render(ExecutionContext context);
  void Fingerprint(MixinFingerprintBuilder builder, ExecutionContext context);
  IMixinValue Select(ExecutionContext context, MixinString member);
  object Unlink(ExecutionContext context);
}

public sealed record ErrorMixinValue(MixinString Message) : IMixinValue {
  public bool IsTruthy(ExecutionContext context) => false;
  public MixinString Render(ExecutionContext context) => Message;
  public void Fingerprint(MixinFingerprintBuilder builder, ExecutionContext context) {
    builder.Append(nameof(ErrorMixinValue)); builder.Append(Message.Resolve(context.Strings));
  }
  public IMixinValue Select(ExecutionContext context, MixinString member) => this;
  public object Unlink(ExecutionContext context) => Message.Resolve(context.Strings);
  public bool Equals(IMixinValue other) => other is ErrorMixinValue value && Equals(value);
}

public sealed class NullMixinValue : IMixinValue {
  public static readonly NullMixinValue Instance = new();
  private NullMixinValue() { }
  public bool IsTruthy(ExecutionContext context) => false;
  public MixinString Render(ExecutionContext context) => ExecutionContext.Dynamic("null");
  public void Fingerprint(MixinFingerprintBuilder builder, ExecutionContext context) =>
    builder.Append(nameof(NullMixinValue));
  public IMixinValue Select(ExecutionContext context, MixinString member) => this;
  public object Unlink(ExecutionContext context) => null;
  public bool Equals(IMixinValue other) => other is NullMixinValue;
  public override bool Equals(object obj) => obj is NullMixinValue;
  public override int GetHashCode() => 0;
}

public sealed record BooleanMixinValue(bool Value) : IMixinValue {
  public static readonly BooleanMixinValue True = new(true), False = new(false);
  public bool IsTruthy(ExecutionContext context) => Value;
  public MixinString Render(ExecutionContext context) => ExecutionContext.Dynamic(Value ? "true" : "false");
  public void Fingerprint(MixinFingerprintBuilder builder, ExecutionContext context) {
    builder.Append(nameof(BooleanMixinValue)); builder.Append(Value);
  }
  public IMixinValue Select(ExecutionContext context, MixinString member) => NullMixinValue.Instance;
  public object Unlink(ExecutionContext context) => Value;
  public bool Equals(IMixinValue other) => other is BooleanMixinValue value && Equals(value);
}

public sealed record ObjectMixinValue(object Value) : IMixinValue {
  public bool IsTruthy(ExecutionContext context) => Value is not null && Value is not false;
  public MixinString Render(ExecutionContext context) => ExecutionContext.Dynamic(Convert.ToString(Value));
  public void Fingerprint(MixinFingerprintBuilder builder, ExecutionContext context) {
    builder.Append(nameof(ObjectMixinValue)); builder.Append(Convert.ToString(Value));
  }
  public IMixinValue Select(ExecutionContext context, MixinString member) => NullMixinValue.Instance;
  public object Unlink(ExecutionContext context) => Value;
  public bool Equals(IMixinValue other) => other is ObjectMixinValue value && Equals(Value, value.Value);
}

public sealed record MixinTableValue(IReadOnlyList<KeyValuePair<MixinString, IMixinValue>> Entries) : IMixinValue {
  public static readonly MixinTableValue Empty = new([]);
  public int Count => Entries.Count;
  public bool IsTruthy(ExecutionContext context) => Count != 0;
  public MixinString Render(ExecutionContext context) => ExecutionContext.Dynamic(
    string.Join(", ", Entries.Select(item =>
      item.Key.Resolve(context.Strings) + "=" + item.Value.Render(context).Resolve(context.Strings)))
  );
  public void Fingerprint(MixinFingerprintBuilder builder, ExecutionContext context) {
    builder.Append(nameof(MixinTableValue)); builder.Append(Count);
    foreach (var item in Entries) {
      builder.Append(item.Key.Resolve(context.Strings)); item.Value.Fingerprint(builder, context);
    }
  }
  public IMixinValue Select(ExecutionContext context, MixinString member) =>
    Entries.FirstOrDefault(item => item.Key == member).Value ?? NullMixinValue.Instance;
  public object Unlink(ExecutionContext context) => Entries.ToDictionary(
    item => item.Key.Resolve(context.Strings), item => item.Value.Unlink(context), StringComparer.Ordinal
  );
  public bool Equals(IMixinValue other) => other is MixinTableValue table && Entries.SequenceEqual(table.Entries);
  public MixinTableValue Put(MixinString key, IMixinValue value) => new(
    [.. Entries.Where(item => item.Key != key), new KeyValuePair<MixinString, IMixinValue>(key, value)]
  );
  public MixinTableValue Remove(MixinString key) => new([.. Entries.Where(item => item.Key != key)]);
  public MixinTableValue Push(ExecutionContext context, IMixinValue value) =>
    Put(context.Intern(Count.ToString()), value);
  public MixinTableValue Pop() => Count == 0 ? this : new([.. Entries.Take(Count - 1)]);
}

public abstract class FunctionDefinition {
  protected FunctionDefinition(string name, int minimumArguments, int maximumArguments) {
    Name = name; MinimumArguments = minimumArguments; MaximumArguments = maximumArguments;
  }
  public string Name { get; }
  public int MinimumArguments { get; }
  public int MaximumArguments { get; }
  public virtual bool IsPredicate => false;
  public abstract IMixinValue Invoke(ExecutionContext context, IMixinValue instance,
    IReadOnlyList<IMixinValue> arguments, bool negated);
}

internal sealed class BuiltinFunctionDefinition(string name, int minimumArguments, int maximumArguments,
  bool predicate = false) : FunctionDefinition(name, minimumArguments, maximumArguments) {
  private readonly string _cacheKey = ":" + name;
  public override bool IsPredicate => predicate;
  public override IMixinValue Invoke(ExecutionContext context, IMixinValue instance,
    IReadOnlyList<IMixinValue> arguments, bool negated) {
    instance = context.Evaluate(instance);
    if (instance is ErrorMixinValue) return instance;
    IReadOnlyList<IMixinValue> values = arguments;
    if (arguments.Count != 0) {
      var evaluated = new IMixinValue[arguments.Count];
      for (var index = 0; index < arguments.Count; index++) {
        evaluated[index] = context.Evaluate(arguments[index]);
        if (evaluated[index] is ErrorMixinValue error) return error;
      }
      values = evaluated;
    }
    IMixinValue result;
    try { result = Apply(context, instance, values); }
    catch (ArgumentException exception) { return context.Error("invalid regular expression: " + exception.Message); }
    if (predicate) {
      var truth = result.IsTruthy(context);
      return truth != negated ? BooleanMixinValue.True : BooleanMixinValue.False;
    }
    return negated ? context.Error("value function ':" + Name + "' cannot be negated") : result;
  }

  private IMixinValue Apply(ExecutionContext context, IMixinValue value, IReadOnlyList<IMixinValue> args) {
    IMixinValue result;
    if (context is RoslynMixinContext roslynContext && value is RoslynMixinValue roslynValue) {
      var key = args.Count == 0 ? _cacheKey : _cacheKey + "\u001f" + string.Join("\u001f", args.Select(item =>
        item.GetType().FullName + "=" + item.Render(context).Resolve(context.Strings)));
      result = roslynContext.Derive(roslynValue, key, () => ApplyCore(context, value, args));
    } else result = ApplyCore(context, value, args);
    return result;
  }

  private IMixinValue ApplyCore(ExecutionContext context, IMixinValue value, IReadOnlyList<IMixinValue> args) {
    string Text(IMixinValue item) => item.Render(context).Resolve(context.Strings);
    string TextValue() => Text(value);
    switch (Name) {
      case "path": return value.Select(context, args[0].Render(context));
      case "unwrap": return context.Unwrap(value);
      case "replace": return new LiteralMixinValue(ExecutionContext.Dynamic(Regex.Replace(TextValue(), Text(args[0]), Text(args[1]))));
      case "replaceFirst": return new LiteralMixinValue(ExecutionContext.Dynamic(new Regex(Text(args[0])).Replace(TextValue(), Text(args[1]), 1)));
      case "floatTime": return FloatTime(context, value);
      case "makeGeneric": {
        var generic = context.Unwrap(value).Render(context).Resolve(context.Strings);
        if (!generic.StartsWith("global::", StringComparison.Ordinal)) generic = "global::" + generic;
        var arguments = string.Join(", ", args.Select(item => {
          var argument = Text(item);
          return argument.Contains(".") && !argument.StartsWith("global::", StringComparison.Ordinal)
            ? "global::" + argument : argument;
        }));
        var marker = generic.IndexOf('<');
        return new LiteralMixinValue(ExecutionContext.Dynamic(marker < 0
          ? generic + "<" + arguments + ">" : generic.Substring(0, marker) + "<" + arguments + ">"));
      }
      case "switch": return value.IsTruthy(context) ? args[0] : args[1];
      case "size": return new LiteralMixinValue(ExecutionContext.Dynamic(value is MixinTableValue table
        ? table.Count.ToString() : TextValue().Length.ToString()));
      case "table": return value is MixinTableValue ? value : new MixinTableValue(
        value is NullMixinValue
          ? Array.Empty<KeyValuePair<MixinString, IMixinValue>>()
          : new[] { new KeyValuePair<MixinString, IMixinValue>(context.Intern("0"), value) });
      case "put": return (value as MixinTableValue ?? MixinTableValue.Empty).Put(args[0].Render(context), args[1]);
      case "remove": return (value as MixinTableValue ?? MixinTableValue.Empty).Remove(args[0].Render(context));
      case "push": return (value as MixinTableValue ?? MixinTableValue.Empty).Push(context, args[0]);
      case "pop": return (value as MixinTableValue ?? MixinTableValue.Empty).Pop();
      case "joinKeys": return Join(context, value, args, 0);
      case "joinValues": return Join(context, value, args, 1);
      case "join": return Join(context, value, args, 2);
      case "mapValues": case "map": case "filter":
        if (value is not MixinTableValue transformTable)
          return context.Error(":" + Name + " requires a table");
        if (args[0] is not ProgramFunctionMixinValue callback)
          return context.Error(":" + Name + " requires a resolved function");
        return new TableTransformMixinValue(transformTable, Name switch {
          "mapValues" => TableTransformKind.MapValues,
          "map" => TableTransformKind.Map,
          _ => TableTransformKind.Filter
        }, callback);
      case "name": return new LiteralMixinValue(context.NameOf(value));
      case "type": return value is RoslynMixinValue typed && RoslynMixinContext.TypeOf(typed.Value) is { } type
        ? new RoslynMixinValue(type) : value is DetachedSemanticMixinValue detachedType
          ? detachedType with { Rendered = detachedType.TypeName, Name = detachedType.TypeName }
          : NullMixinValue.Instance;
      case "fullName": return value is RoslynMixinValue full && RoslynMixinContext.TypeOf(full.Value) is { } fullType
        ? new LiteralMixinValue(ExecutionContext.Dynamic(fullType.ToDisplayString(
          SymbolDisplayFormat.MinimallyQualifiedFormat.WithGenericsOptions(
            SymbolDisplayGenericsOptions.IncludeTypeParameters))))
        : value is DetachedSemanticMixinValue detachedFull
          ? new LiteralMixinValue(detachedFull.FullName) : NullMixinValue.Instance;
      case "visibility": return value is RoslynMixinValue visible &&
        (visible.Value as ISymbol ?? RoslynMixinContext.TypeOf(visible.Value)) is { } symbol
        ? new LiteralMixinValue(ExecutionContext.Dynamic(symbol.DeclaredAccessibility.ToString().ToLowerInvariant()))
        : value is DetachedSemanticMixinValue detachedVisibility
          ? new LiteralMixinValue(detachedVisibility.Visibility)
          : context.Error("property ':visibility' is not available for this value");
      case "exists": return Bool(value is not NullMixinValue and not ErrorMixinValue);
      case "eq": return Bool(string.Equals(Comparable(TextValue()), Comparable(Text(args[0])),
        StringComparison.OrdinalIgnoreCase));
      case "matches": return Bool(Regex.IsMatch(TextValue(), Text(args[0])));
      case "has": return Bool(value is MixinTableValue hasTable
        ? hasTable.Entries.Any(item => item.Value.Equals(args[0]) ||
          string.Equals(Comparable(Text(item.Value)), Comparable(Text(args[0])), StringComparison.OrdinalIgnoreCase))
        : value.Select(context, args[0].Render(context)) is not NullMixinValue);
      case "and": return Bool(value.IsTruthy(context) && args.All(item => item.IsTruthy(context)));
      case "or": return Bool(value.IsTruthy(context) || args.Any(item => item.IsTruthy(context)));
      case "is": return Bool(context.IsType(value, args[0].Render(context)));
      case "attributes": return context.Attributes(value, default, false, false);
      case "attributesOf": return context.Attributes(value, args[0].Render(context), false, false);
      case "attributesOfExact": return context.Attributes(value, args[0].Render(context), true, false);
      case "attributeOf": return context.Attributes(value, args[0].Render(context), false, true);
      case "wire": case "structParams": case "structArgs": case "propStructCall":
      case "signature": case "wireable": case "structHasEquality": case "structNoArgs": case "structAugment":
        return context.InvokeHostFunction(Name, value, args);
      case var _ when predicate: return Bool(Trait(Name, value, context));
      default: return context.Error("function implementation is not installed: " + Name);
    }
  }

  private static BooleanMixinValue Bool(bool value) => value ? BooleanMixinValue.True : BooleanMixinValue.False;
  private static string Comparable(string value) {
    value ??= "";
    if (value.StartsWith("global::", StringComparison.Ordinal)) value = value.Substring(8);
    if (value.Length >= 2 && value[0] == '"' && value[value.Length - 1] == '"')
      value = value.Substring(1, value.Length - 2);
    return value;
  }
  private static IMixinValue FloatTime(ExecutionContext context, IMixinValue value) {
    var text = context.Unwrap(value).Render(context).Resolve(context.Strings);
    text = value is NullMixinValue ? "" : (text ?? "").Trim();
    double seconds;
    if (text.Length == 0 || string.Equals(text, "null", StringComparison.OrdinalIgnoreCase) ||
      string.Equals(text, "tick", StringComparison.OrdinalIgnoreCase) ||
      string.Equals(text, "ticks", StringComparison.OrdinalIgnoreCase)) seconds = 0;
    else if (text.StartsWith("%", StringComparison.Ordinal) &&
      double.TryParse(text.Substring(1), NumberStyles.Float, CultureInfo.InvariantCulture, out var frequency) && frequency > 0)
      seconds = 1d / frequency;
    else {
      var match = Regex.Match(text, @"^(?<n>[+-]?(?:\d+(?:\.\d*)?|\.\d+)(?:[eE][+-]?\d+)?)\s*(?<u>ms|millis|s|seconds?|t|ticks?|m|minutes?)?$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
      if (!match.Success || !double.TryParse(match.Groups["n"].Value, NumberStyles.Float,
        CultureInfo.InvariantCulture, out seconds)) return context.Error("cannot parse '" + text + "' as a float time");
      var unit = match.Groups["u"].Value.ToLowerInvariant();
      if (unit is "t" or "tick" or "ticks" && seconds != Math.Truncate(seconds))
        return context.Error("cannot parse '" + text + "' as a float time");
      switch (unit) {
        case "t": case "tick": case "ticks": seconds = -seconds; break;
        case "ms": case "millis": seconds /= 1000d; break;
        case "m": case "minute": case "minutes": seconds *= 60d; break;
      }
    }
    var result = Math.Abs(seconds) <= 1e-6 ? "-0f" : ((float)seconds).ToString("R", CultureInfo.InvariantCulture) + "f";
    return new LiteralMixinValue(ExecutionContext.Dynamic(result));
  }
  private static IMixinValue FromObject(ExecutionContext context, object value) => value switch {
    null => NullMixinValue.Instance, IMixinValue typed => typed,
    bool boolean => Bool(boolean), string text => new LiteralMixinValue(ExecutionContext.Dynamic(text)),
    _ => new ObjectMixinValue(value)
  };
  private static bool Trait(string name, IMixinValue value, ExecutionContext context) {
    if (context.HasTrait(value, context.Intern(name))) return true;
    if (value is not RoslynMixinValue roslyn) return false;
    var symbol = roslyn.Value as ISymbol; var type = RoslynMixinContext.TypeOf(roslyn.Value);
    return name switch {
      "isSelf" => context is RoslynMixinContext rc && SymbolEqualityComparer.Default.Equals(type, rc.CurrentType),
      "ref" => symbol is IParameterSymbol { RefKind: RefKind.Ref },
      "in" => symbol is IParameterSymbol { RefKind: RefKind.In },
      "out" => symbol is IParameterSymbol { RefKind: RefKind.Out },
      "inout" => symbol is IParameterSymbol { RefKind: RefKind.In or RefKind.Out },
      "argument" => symbol is IParameterSymbol { RefKind: RefKind.None },
      "static" => symbol?.IsStatic == true, "async" => symbol is IMethodSymbol { IsAsync: true },
      "public" => symbol?.DeclaredAccessibility == Accessibility.Public,
      "exposed" => symbol?.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal,
      "top" => type?.ContainingType is null, "generic" => type is INamedTypeSymbol { Arity: > 0 },
      "struct" => type?.TypeKind == TypeKind.Struct, "class" => type?.IsReferenceType == true,
      "concrete" => type is not { TypeKind: TypeKind.Interface } && type?.IsAbstract != true,
      _ => false
    };
  }
  private static IMixinValue Join(ExecutionContext context, IMixinValue value, IReadOnlyList<IMixinValue> args, int kind) {
    if (value is not MixinTableValue table) return context.Error("join requires a table");
    string Text(IMixinValue item) => item.Render(context).Resolve(context.Strings);
    var joined = kind switch {
      0 => string.Join(Text(args[0]), table.Entries.Select(item => item.Key.Resolve(context.Strings))),
      1 => string.Join(Text(args[0]), table.Entries.Select(item => Text(item.Value))),
      _ => string.Join(Text(args[1]), table.Entries.Select(item =>
        item.Key.Resolve(context.Strings) + Text(args[0]) + Text(item.Value)))
    };
    return new LiteralMixinValue(ExecutionContext.Dynamic(joined));
  }
}

internal static class FunctionLibrary {
  private static readonly IReadOnlyDictionary<string, FunctionDefinition> Definitions = Build();
  private static IReadOnlyDictionary<string, FunctionDefinition> Build() {
    var result = new Dictionary<string, FunctionDefinition>(StringComparer.Ordinal);
    foreach (var name in new[] { "name", "type", "fullName", "makeGeneric", "visibility", "path", "unwrap",
      "replace", "replaceFirst", "switch", "table", "put", "wire", "remove", "push", "structParams",
      "structArgs", "propStructCall", "pop", "size", "floatTime", "joinKeys", "joinValues", "join",
      "mapValues", "map", "filter", "attributes", "attributesOf", "attributesOfExact", "attributeOf" })
      result[name] = new BuiltinFunctionDefinition(name, 0, int.MaxValue);
    foreach (var name in new[] { "and", "or", "is", "has", "eq", "matches", "signature", "wireable",
      "exists", "isSelf", "ref", "in", "out", "inout", "argument", "static", "async", "public",
      "exposed", "top", "concrete", "partial", "generic", "struct", "class", "structHasEquality",
      "structNoArgs", "structAugment" })
      result[name] = new BuiltinFunctionDefinition(name, 0, int.MaxValue, true);
    void Set(string name, int minimum, int maximum, bool predicate = false) =>
      result[name] = new BuiltinFunctionDefinition(name, minimum, maximum, predicate);
    foreach (var name in new[] { "name", "type", "fullName", "visibility", "unwrap", "table", "pop", "size",
      "floatTime", "attributes" }) Set(name, 0, 0);
    foreach (var name in new[] { "path", "makeGeneric", "remove", "push", "mapValues", "map", "filter",
      "attributesOf", "attributesOfExact", "attributeOf", "wire", "joinKeys", "joinValues" }) Set(name, 1, 1);
    foreach (var name in new[] { "replace", "replaceFirst", "switch", "put", "join" }) Set(name, 2, 2);
    foreach (var name in new[] { "exists", "isSelf", "ref", "in", "out", "inout", "argument", "static",
      "async", "public", "exposed", "top", "concrete", "partial", "generic", "struct", "class",
      "structHasEquality", "structNoArgs", "structAugment" }) Set(name, 0, 0, true);
    foreach (var name in new[] { "is", "has", "eq", "matches", "signature" }) Set(name, 1, 1, true);
    Set("wireable", 2, 2, true); Set("and", 1, int.MaxValue, true); Set("or", 1, int.MaxValue, true);
    Set("structParams", 0, 1); Set("structArgs", 0, 1); Set("propStructCall", 2, 2);
    return result;
  }
  internal static bool TryGet(string name, out FunctionDefinition definition) =>
    Definitions.TryGetValue(name ?? "", out definition);
  internal static bool IsPredicate(string name) => TryGet(name, out var definition) && definition.IsPredicate;
  internal static void CollectConstants(MixinStringPoolBuilder pool) {
    foreach (var name in Definitions.Keys) pool.Intern(name);
  }
}
