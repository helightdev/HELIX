using System;
using System.Collections.Generic;
using System.Linq;
using Hix.Functions;
using Hix.Compiler;
using Hix.Env;
using Hix.Runtime;

namespace Hix;

/// <summary>Flat runtime kinds. Any is a signature wildcard, not a parent kind.</summary>
public enum HixValueKind { Any, Null, String, Bool, Number, Tuple, Table, Symbol, Function, Error, Kind, Pattern }

public enum HixExpressionRoot {
  Target,
  This,
  Attribute,
  Variable,
  TargetVariable,
  Local,
  True,
  False,
  Null,
  Table,
  Parameter
}

public enum HixEmissionTarget {
  Target,
  Class,
  File,
  Extends,
  Implements,
  Injection,
  Annotation,
  Using,
  Mixin
}

public readonly record struct HixSourceLocation(int Program, int Line);



public sealed record HixRootDefinition(
  string Name, HixExpressionRoot Root, HixValueKind Kind, string Documentation
);

public sealed record FunctionSignature(
  HixValueKind ResultType, IReadOnlyList<HixValueKind> ArgumentTypes, bool IsVariadic = false
) {
  public int ArgumentCount => ArgumentTypes.Count - (IsVariadic ? 1 : 0);
  public bool MatchesArgumentCount(int count) => IsVariadic ? count >= ArgumentCount : count == ArgumentCount;
  public HixValueKind GetArgumentType(int index) => index < ArgumentTypes.Count
    ? ArgumentTypes[index] : IsVariadic ? ArgumentTypes[ArgumentTypes.Count - 1] : HixValueKind.Any;
}

public sealed class FunctionSignatureRegistry {
  private readonly IReadOnlyDictionary<string, FunctionDefinition[]> _definitions;
  private readonly FunctionDefinition[] _all;

  internal FunctionSignatureRegistry(IEnumerable<FunctionDefinition> definitions) {
    _all = definitions.ToArray();
    _definitions = _all.GroupBy(definition => definition.Name, StringComparer.Ordinal)
      .ToDictionary(group => group.Key, group => group
        .OrderBy(definition => definition.Signatures.Min(signature => signature.ArgumentTypes.Count(kind => kind == HixValueKind.Any))).ToArray(),
        StringComparer.Ordinal);
  }

  public bool TryGet(string name, int argumentCount, out FunctionDefinition definition) {
    definition = Resolve(name, argumentCount).FirstOrDefault();
    return definition is not null;
  }

  public IReadOnlyList<FunctionDefinition> Resolve(string name, int argumentCount) =>
    _definitions.TryGetValue(name, out var definitions)
      ? definitions.Where(definition => definition.MatchesArgumentCount(argumentCount)).ToArray()
      : Array.Empty<FunctionDefinition>();

  public IEnumerable<FunctionDefinition> Enumerate() => _all;
}

internal delegate IHixValue InlineFunction(LanguageExecution execution, IHixValue[] arguments);

public sealed class FunctionSignatureRegistryBuilder {
  private readonly List<FunctionDefinition> _definitions = [];

  public FunctionSignatureRegistryBuilder Add(params FunctionDefinition[] definitions) {
    _definitions.AddRange(definitions);
    return this;
  }

  public FunctionSignatureRegistry Build(Action<IEnumerable<FunctionDefinition>> validate = null) {
    validate?.Invoke(_definitions);
    return new FunctionSignatureRegistry(_definitions);
  }
}

internal sealed class SimpleFunction(
  string name,
  IReadOnlyList<FunctionSignature> signatures,
  InlineFunction implementation,
  bool effects = false,
  bool acceptsErrors = false,
  bool requiresPrelude = false
) : FunctionDefinition(name, signatures) {
  public override bool HasEffects => effects;
  public override bool RequiresPrelude => requiresPrelude;
  public override bool AcceptsErrors => acceptsErrors;
  public override IHixValue Execute(HixExecutionContext invocation, IHixValue[] arguments, int line) { var execution = invocation.Execution; return implementation(execution, arguments); }
}

public abstract class FunctionDefinition {

  protected FunctionDefinition(
    string name, int argumentCount,
    HixValueKind receiverType = HixValueKind.Any,
    HixValueKind resultType = HixValueKind.Any,
    IReadOnlyList<HixValueKind> argumentTypes = null,
    string documentation = null, bool variadic = false
  ) {
    Name = name;
    ArgumentCount = argumentCount;
    IsVariadic = variadic;
    ReceiverType = receiverType;
    ResultType = resultType;
    ArgumentTypes = argumentTypes ?? [
      .. Enumerable.Repeat(HixValueKind.Any, argumentCount + (variadic ? 1 : 0))
    ];
    Documentation = documentation ?? "Transforms the current value.";
    if (ArgumentTypes.Count != argumentCount + (variadic ? 1 : 0))
      throw new ArgumentException("Function argument signature does not match its arity.", nameof(argumentTypes));
    Signatures = [new FunctionSignature(resultType, ArgumentTypes, variadic)];
  }

  protected FunctionDefinition(string name, IReadOnlyList<FunctionSignature> signatures) {
    if (signatures is null || signatures.Count == 0) throw new ArgumentException("A function requires at least one signature.", nameof(signatures));
    Name = name; Signatures = signatures;
    var primary = signatures[0]; ArgumentCount = primary.ArgumentCount; IsVariadic = primary.IsVariadic;
    ResultType = primary.ResultType; ArgumentTypes = primary.ArgumentTypes;
    ReceiverType = ArgumentTypes.Count == 0 ? HixValueKind.Any : ArgumentTypes[0];
    Documentation = "Transforms the current value.";
  }

  public string Name { get; }
  public int ArgumentCount { get; }
  public bool IsVariadic { get; }
  public HixValueKind ReceiverType { get; private set; }
  public HixValueKind ResultType { get; private set; }
  public IReadOnlyList<HixValueKind> ArgumentTypes { get; private set; }
  public string Documentation { get; private set; }
  public IReadOnlyList<FunctionSignature> Signatures { get; }

  public bool MatchesArgumentCount(int count) => Signatures.Any(signature => signature.MatchesArgumentCount(count));

  public string ArgumentCountError() => IsVariadic
    ? $":{Name} requires at least {ArgumentCount}{(ArgumentCount == 1 ? " argument" : " arguments")}"
    : $":{Name} requires {ArgumentCount}{(ArgumentCount == 1 ? " argument" : " arguments")}";

  public HixValueKind GetArgumentType(int index) {
    if (index < 0 || ArgumentTypes.Count == 0) return HixValueKind.Any;
    if (index < ArgumentTypes.Count) return ArgumentTypes[index];
    return IsVariadic ? ArgumentTypes[ArgumentTypes.Count - 1] : HixValueKind.Any;
  }

  public virtual bool HasEffects => false;
  public virtual bool AcceptsErrors => false;
  public virtual IReadOnlyDictionary<int, string> ArgumentReferences => new Dictionary<int, string>();
  public virtual bool RequiresPrelude => false;
  internal bool MatchesValues(IHixValue[] values) {
    return Signatures.Any(signature => signature.MatchesArgumentCount(values.Length) &&
      values.Select((value, index) => signature.GetArgumentType(index) is var expected &&
        (expected == HixValueKind.Any || value.Kind == expected)).All(match => match));
  }
  internal bool TryConvertValues(LanguageExecution execution, IHixValue[] values,
    out IHixValue[] converted, out int conversionCount) {
    converted = null;
    conversionCount = int.MaxValue;
    foreach (var signature in Signatures) {
      if (!signature.MatchesArgumentCount(values.Length)) continue;
      var candidate = values;
      var count = 0;
      var valid = true;
      for (var index = 0; index < values.Length; index++) {
        var expected = signature.GetArgumentType(index);
        if (!KindDefinitions.TryImplicitConvert(execution, values[index], expected, out var value)) {
          valid = false;
          break;
        }
        if (!ReferenceEquals(value, values[index])) {
          if (ReferenceEquals(candidate, values)) candidate = (IHixValue[])values.Clone();
          candidate[index] = value;
          count++;
        }
      }
      if (valid && count < conversionCount) { converted = candidate; conversionCount = count; }
      if (conversionCount == 0) break;
    }
    return converted != null;
  }
  internal static bool TryConvertValues(LanguageExecution execution, FunctionSignature signature, IHixValue[] values,
    out IHixValue[] converted) {
    converted = values;
    if (!signature.MatchesArgumentCount(values.Length)) return false;
    for (var index = 0; index < values.Length; index++) {
      if (!KindDefinitions.TryImplicitConvert(execution, values[index], signature.GetArgumentType(index), out var value))
        return false;
      if (ReferenceEquals(value, values[index])) continue;
      if (ReferenceEquals(converted, values)) converted = (IHixValue[])values.Clone();
      converted[index] = value;
    }
    return true;
  }
  public abstract IHixValue Execute(HixExecutionContext context, IHixValue[] arguments, int line);
}

public abstract class EvaluatedFunctionDefinition(
  string name, int argumentCount,
  HixValueKind receiverType = HixValueKind.Any,
  HixValueKind resultType = HixValueKind.Any,
  IReadOnlyList<HixValueKind> argumentTypes = null,
  bool variadic = false
) : FunctionDefinition(
  name, argumentCount + 1, receiverType,
  resultType, new[] {receiverType}.Concat(argumentTypes ?? Enumerable.Repeat(HixValueKind.Any, argumentCount + (variadic ? 1 : 0))).ToArray(),
  "Transforms the current value.", variadic
) {

  public sealed override IHixValue Execute(HixExecutionContext invocation, IHixValue[] supplied, int line) {
    var execution = invocation.Execution;
    var context = execution.Context;
    var instance = supplied[0];
    IReadOnlyList<IHixValue> arguments = supplied.Skip(1).ToArray();
    using var profile = HixProfiler.MeasureFunction(Name);
    instance = context.Evaluate(instance);
    if (instance is ErrorHixValue) return instance;
    var values = arguments.Count == 0 ? Array.Empty<IHixValue>() : arguments.Select(context.Evaluate).ToArray();
    if (values.OfType<ErrorHixValue>().FirstOrDefault() is { } error) return error;
    IHixValue result;
    try {
      result = EvaluateValue(context, instance, values);
    } catch (ArgumentException exception) {
      return context.Error($"function ':{Name}' failed: {exception.Message}");
    }
    return result;
  }

  protected virtual IHixValue EvaluateValue(HixExecutionContext context, IHixValue value, IReadOnlyList<IHixValue> arguments) => Apply(context, value, arguments);

  protected abstract IHixValue Apply(
    HixExecutionContext context, IHixValue value,
    IReadOnlyList<IHixValue> arguments
  );
}
