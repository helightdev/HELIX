using System;
using System.Collections.Generic;
using System.Linq;
using Mixins.Functions;
using Mixins.Compiler;
using Mixins.Env;
using Mixins.Runtime;

namespace Mixins;

/// <summary>Flat runtime kinds. Any is a signature wildcard, not a parent kind.</summary>
public enum MixinValueKind { Any, Null, String, Bool, Number, Tuple, Table, Symbol, Function, Error, Kind }

public enum MixinExpressionRoot {
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

public enum MixinEmissionTarget {
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

public readonly record struct MixinSourceLocation(int Program, int Line);

internal sealed record MixinTargetDescriptor(string Name, bool IsStatic, bool IsPublic, string DelegateType);

public sealed record MixinRootDefinition(
  string Name, MixinExpressionRoot Root, MixinValueKind Kind, string Documentation
);

public sealed record FunctionSignature(
  MixinValueKind ResultType, IReadOnlyList<MixinValueKind> ArgumentTypes, bool IsVariadic = false
) {
  public int ArgumentCount => ArgumentTypes.Count - (IsVariadic ? 1 : 0);
  public bool MatchesArgumentCount(int count) => IsVariadic ? count >= ArgumentCount : count == ArgumentCount;
  public MixinValueKind GetArgumentType(int index) => index < ArgumentTypes.Count
    ? ArgumentTypes[index] : IsVariadic ? ArgumentTypes[ArgumentTypes.Count - 1] : MixinValueKind.Any;
}

public sealed class FunctionSignatureRegistry {
  private readonly IReadOnlyDictionary<string, FunctionDefinition[]> _definitions;
  private readonly FunctionDefinition[] _all;

  internal FunctionSignatureRegistry(IEnumerable<FunctionDefinition> definitions) {
    _all = definitions.ToArray();
    _definitions = _all.GroupBy(definition => definition.Name, StringComparer.Ordinal)
      .ToDictionary(group => group.Key, group => group
        .OrderBy(definition => definition.Signatures.Min(signature => signature.ArgumentTypes.Count(kind => kind == MixinValueKind.Any))).ToArray(),
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

internal delegate IMixinValue InlineFunction(LanguageExecution execution, IMixinValue[] arguments);

internal sealed class FunctionSignatureRegistryBuilder {
  private readonly List<FunctionDefinition> _definitions = [];

  internal FunctionSignatureRegistryBuilder Add(params FunctionDefinition[] definitions) {
    _definitions.AddRange(definitions);
    return this;
  }

  internal FunctionSignatureRegistry Build(Action<IEnumerable<FunctionDefinition>> validate = null) {
    validate?.Invoke(_definitions);
    return new FunctionSignatureRegistry(_definitions);
  }
}

internal sealed class SimpleFunction(
  string name,
  IReadOnlyList<FunctionSignature> signatures,
  InlineFunction implementation,
  bool effects = false,
  bool acceptsErrors = false
) : FunctionDefinition(name, signatures) {
  public override bool HasEffects => effects;
  public override bool AcceptsErrors => acceptsErrors;
  internal override IMixinValue Execute(LanguageExecution execution, IMixinValue[] arguments, int line) =>
    implementation(execution, arguments);
}

public abstract class FunctionDefinition {

  protected FunctionDefinition(
    string name, int argumentCount,
    MixinValueKind receiverType = MixinValueKind.Any,
    MixinValueKind resultType = MixinValueKind.Any,
    IReadOnlyList<MixinValueKind> argumentTypes = null,
    string documentation = null, bool variadic = false
  ) {
    Name = name;
    ArgumentCount = argumentCount;
    IsVariadic = variadic;
    ReceiverType = receiverType;
    ResultType = resultType;
    ArgumentTypes = argumentTypes ?? [
      .. Enumerable.Repeat(MixinValueKind.Any, argumentCount + (variadic ? 1 : 0))
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
    ReceiverType = ArgumentTypes.Count == 0 ? MixinValueKind.Any : ArgumentTypes[0];
    Documentation = "Transforms the current value.";
  }

  public string Name { get; }
  public int ArgumentCount { get; }
  public bool IsVariadic { get; }
  public MixinValueKind ReceiverType { get; private set; }
  public MixinValueKind ResultType { get; private set; }
  public IReadOnlyList<MixinValueKind> ArgumentTypes { get; private set; }
  public string Documentation { get; private set; }
  public IReadOnlyList<FunctionSignature> Signatures { get; }

  public bool MatchesArgumentCount(int count) => Signatures.Any(signature => signature.MatchesArgumentCount(count));

  public string ArgumentCountError() => IsVariadic
    ? $":{Name} requires at least {ArgumentCount}{(ArgumentCount == 1 ? " argument" : " arguments")}"
    : $":{Name} requires {ArgumentCount}{(ArgumentCount == 1 ? " argument" : " arguments")}";

  public MixinValueKind GetArgumentType(int index) {
    if (index < 0 || ArgumentTypes.Count == 0) return MixinValueKind.Any;
    if (index < ArgumentTypes.Count) return ArgumentTypes[index];
    return IsVariadic ? ArgumentTypes[ArgumentTypes.Count - 1] : MixinValueKind.Any;
  }

  public virtual bool HasEffects => false;
  public virtual bool AcceptsErrors => false;
  public virtual IReadOnlyList<int> CSharpTypeArguments => Array.Empty<int>();
  internal bool MatchesValues(IMixinValue[] values) {
    return Signatures.Any(signature => signature.MatchesArgumentCount(values.Length) &&
      values.Select((value, index) => signature.GetArgumentType(index) is var expected &&
        (expected == MixinValueKind.Any || value.Kind == expected)).All(match => match));
  }
  internal bool TryConvertValues(LanguageExecution execution, IMixinValue[] values,
    out IMixinValue[] converted, out int conversionCount) {
    converted = null;
    conversionCount = int.MaxValue;
    foreach (var signature in Signatures.Where(signature => signature.MatchesArgumentCount(values.Length))) {
      var candidate = new IMixinValue[values.Length];
      var count = 0;
      var valid = true;
      for (var index = 0; index < values.Length; index++) {
        var expected = signature.GetArgumentType(index);
        if (!KindDefinitions.TryImplicitConvert(execution, values[index], expected, out candidate[index])) {
          valid = false;
          break;
        }
        if (!ReferenceEquals(candidate[index], values[index])) count++;
      }
      if (valid && count < conversionCount) { converted = candidate; conversionCount = count; }
    }
    return converted != null;
  }
  internal abstract IMixinValue Execute(LanguageExecution execution, IMixinValue[] arguments, int line);
}

internal abstract class EvaluatedFunctionDefinition(
  string name, int argumentCount,
  MixinValueKind receiverType = MixinValueKind.Any,
  MixinValueKind resultType = MixinValueKind.Any,
  IReadOnlyList<MixinValueKind> argumentTypes = null,
  bool variadic = false
) : FunctionDefinition(
  name, argumentCount + 1, receiverType,
  resultType, new[] {receiverType}.Concat(argumentTypes ?? Enumerable.Repeat(MixinValueKind.Any, argumentCount + (variadic ? 1 : 0))).ToArray(),
  "Transforms the current value.", variadic
) {
  private readonly string _cacheKey = ":" + name;

  internal sealed override IMixinValue Execute(LanguageExecution execution, IMixinValue[] supplied, int line) {
    var context = execution.Context;
    var instance = supplied[0];
    IReadOnlyList<IMixinValue> arguments = supplied.Skip(1).ToArray();
    using var profile = MixinProfiler.MeasureFunction(Name);
    instance = context.Evaluate(instance);
    if (instance is ErrorMixinValue) return instance;
    var values = arguments.Count == 0 ? Array.Empty<IMixinValue>() : arguments.Select(context.Evaluate).ToArray();
    if (values.OfType<ErrorMixinValue>().FirstOrDefault() is { } error) return error;
    IMixinValue result;
    try {
      if (context is RoslynMixinContext roslyn && instance is RoslynMixinValue source) {
        string key;
        using (MixinProfiler.Measure("cache.derived.key")) {
          key = values.Length == 0
            ? _cacheKey
            : _cacheKey + "\u001f" + string.Join(
              "\u001f",
              values.Select(item => $"{item.GetType().FullName}={item.Render(context).Resolve(context.Strings)}")
            );
        }
        if (!roslyn.TryGetDerived(source, key, out result)) {
          result = Apply(context, instance, values);
          roslyn.StoreDerived(source, key, result);
        }
      } else result = Apply(context, instance, values);
    } catch (ArgumentException exception) {
      return context.Error($"function ':{Name}' failed: {exception.Message}");
    }
    return result;
  }

  protected abstract IMixinValue Apply(
    ExecutionContext context, IMixinValue value,
    IReadOnlyList<IMixinValue> arguments
  );
}
