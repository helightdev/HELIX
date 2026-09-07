using System;
using System.Collections.Generic;
using System.Linq;
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
  Parameter,
  Carry
}

public enum MixinExpressionOutputTarget {
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

public interface IMixinSignature {
  string Name { get; }
  int ArgumentCount { get; }
  bool MatchesArgumentCount(int count);
}

public sealed class MixinSignatureRegistry<T> where T : IMixinSignature {
  private readonly IReadOnlyDictionary<string, T> _definitions;

  internal MixinSignatureRegistry(IReadOnlyDictionary<string, T> definitions) {
    _definitions = definitions;
  }

  public bool TryGet(string name, int argumentCount, out T definition) {
    definition = _definitions.Values.FirstOrDefault(item =>
      string.Equals(item.Name, name, StringComparison.Ordinal) && item.MatchesArgumentCount(argumentCount)
    );
    return definition is not null;
  }

  public IEnumerable<T> Enumerate() => _definitions.Values;
}

internal static class MixinSignatureRegistry {
  internal static MixinSignatureRegistry<T> Build<T>(
    Action<MixinSignatureRegistryBuilder<T>> configure,
    Action<IEnumerable<T>> validate = null
  ) where T : IMixinSignature {
    var builder = new MixinSignatureRegistryBuilder<T>();
    configure(builder);
    return validate is null ? builder.Build() : builder.Build(validate);
  }
}

internal sealed class MixinSignatureRegistryBuilder<T> where T : IMixinSignature {
  private readonly Dictionary<string, T> _definitions = new(StringComparer.Ordinal);

  internal MixinSignatureRegistryBuilder<T> Add(T definition) {
    var key = definition.Name;
    while (_definitions.ContainsKey(key)) key += "/";
    _definitions.Add(key, definition);
    return this;
  }

  internal MixinSignatureRegistryBuilder<T> Add(params T[] definitions) => AddRange(definitions);

  internal MixinSignatureRegistryBuilder<T> AddRange(IEnumerable<T> definitions) {
    foreach (var definition in definitions) Add(definition);
    return this;
  }

  internal IEnumerable<T> Enumerate() => _definitions.Values;

  internal T Get(string name, int argumentCount) => _definitions.Values.First(item =>
    string.Equals(item.Name, name, StringComparison.Ordinal) && item.MatchesArgumentCount(argumentCount)
  );

  internal MixinSignatureRegistryBuilder<T> Configure(
    string name, int argumentCount, Action<T> configure
  ) {
    configure(Get(name, argumentCount));
    return this;
  }

  internal MixinSignatureRegistry<T> Build() => new(_definitions);

  internal MixinSignatureRegistry<T> Build(Action<IEnumerable<T>> validate) {
    validate(_definitions.Values);
    return Build();
  }
}

public abstract class FunctionDefinition : IMixinSignature {

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
  }

  public string Name { get; }
  public int ArgumentCount { get; }
  public bool IsVariadic { get; }
  public MixinValueKind ReceiverType { get; private set; }
  public MixinValueKind ResultType { get; private set; }
  public IReadOnlyList<MixinValueKind> ArgumentTypes { get; private set; }
  public string Documentation { get; private set; }

  public bool MatchesArgumentCount(int count) => IsVariadic ? count >= ArgumentCount : count == ArgumentCount;

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
    if (!MatchesArgumentCount(values.Length)) return false;
    for (var index = 0; index < values.Length; index++) {
      var expected = GetArgumentType(index);
      if (expected != MixinValueKind.Any && KindMixinValue.Of(values[index]).ValueKind != expected) return false;
    }
    return true;
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
