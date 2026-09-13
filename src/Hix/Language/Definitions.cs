using System;
using System.Collections.Generic;
using System.Linq;
using Hix.Functions;
using Hix.Compiler;
using Hix.Env;
using Hix.Runtime;

namespace Hix;

/// <summary>Flat runtime kinds. Any is a signature wildcard, not a parent kind.</summary>
public enum HixValueKind { Any, Null, String, Bool, Number, Tuple, Table, Opaque, Function, Error, Kind, Pattern, Missing }

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

public readonly record struct HixSourceLocation(int Program, int Line);



public sealed record HixRootDefinition(
  string Name, HixExpressionRoot Root, HixValueKind Kind, string Documentation
);

[Flags]
public enum HixMetadataKind {
  None = 0, Pattern = 1, PatternField = 2, File = 4, TypeDefinition = 8,
  FunctionDefinition = 16, MixinDefinition = 32, Statement = 64, VariableDeclaration = 128
}

public static class HixMetadataTargets {
  private static readonly (string Name, HixMetadataKind Kind)[] targets = [
    ("pattern", HixMetadataKind.Pattern), ("field", HixMetadataKind.PatternField),
    ("file", HixMetadataKind.File), ("type", HixMetadataKind.TypeDefinition),
    ("function", HixMetadataKind.FunctionDefinition), ("mixin", HixMetadataKind.MixinDefinition),
    ("statement", HixMetadataKind.Statement), ("variable", HixMetadataKind.VariableDeclaration)
  ];
  public static bool TryParse(string name, out HixMetadataKind kind) {
    foreach (var target in targets)
      if (target.Name == name) { kind = target.Kind; return true; }
    kind = HixMetadataKind.None;
    return false;
  }
  public static IEnumerable<string> Names(HixMetadataKind kinds) => targets
    .Where(target => (kinds & target.Kind) != 0).Select(target => target.Name);
}

public sealed record FunctionSignature(
  HixValueKind ResultType, IReadOnlyList<HixValueKind> ArgumentTypes, bool IsVariadic = false,
  IReadOnlyList<string> ArgumentNames = null, IReadOnlyList<ExpressionIr> ArgumentDefaults = null
) {
  public FunctionSignature(HixPattern resultPattern, IReadOnlyList<HixPattern> argumentPatterns,
    bool IsVariadic = false, IReadOnlyList<string> ArgumentNames = null,
    IReadOnlyList<ExpressionIr> ArgumentDefaults = null, IReadOnlyList<HixTypeParameter> typeParameters = null)
    : this(HixGenericTypes.Erase(resultPattern), argumentPatterns.Select(HixGenericTypes.Erase).ToArray(),
      IsVariadic, ArgumentNames, ArgumentDefaults) {
    ResultPattern = resultPattern;
    ArgumentPatterns = argumentPatterns;
    TypeParameters = typeParameters ?? Array.Empty<HixTypeParameter>();
  }
  public HixPattern ResultPattern { get; init; } = ResultType == HixValueKind.Any ? HixPattern.Any : new KindHixPattern(ResultType);
  public IReadOnlyList<HixPattern> ArgumentPatterns { get; init; } = ArgumentTypes.Select(kind =>
    kind == HixValueKind.Any ? HixPattern.Any : new KindHixPattern(kind)).ToArray();
  public IReadOnlyList<HixTypeParameter> TypeParameters { get; init; } = Array.Empty<HixTypeParameter>();
  public Func<IReadOnlyList<HixPattern>, HixPattern> InferResult { get; init; }
  /// <summary>Maps callable argument positions to tuple arguments used to invoke those callables.</summary>
  public IReadOnlyDictionary<int, int> CallableArguments { get; init; }
  public HixPattern GetArgumentPattern(int index) => index < ArgumentPatterns.Count ? ArgumentPatterns[index] :
    IsVariadic ? ArgumentPatterns[ArgumentPatterns.Count - 1] : HixPattern.Any;
  public int MinimumArgumentCount => IsVariadic ? ArgumentCount : Enumerable.Range(0, ArgumentTypes.Count)
    .Count(index => GetArgumentDefault(index) == null);
  public int ArgumentCount => ArgumentTypes.Count - (IsVariadic ? 1 : 0);
  public bool MatchesArgumentCount(int count) => count >= MinimumArgumentCount &&
    (IsVariadic ? count >= ArgumentCount : count <= ArgumentCount);
  public HixValueKind GetArgumentType(int index) => index < ArgumentTypes.Count
    ? ArgumentTypes[index] : IsVariadic ? ArgumentTypes[ArgumentTypes.Count - 1] : HixValueKind.Any;
  public string GetArgumentName(int index) => ArgumentNames != null && index < ArgumentNames.Count
    ? ArgumentNames[index] : null;
  public ExpressionIr GetArgumentDefault(int index) => ArgumentDefaults != null && index < ArgumentDefaults.Count
    ? ArgumentDefaults[index] : null;
}

public sealed class FunctionSignatureRegistry {
  private readonly IReadOnlyDictionary<string, HixFunctionEntry[]> _definitions;
  private readonly HixFunctionEntry[] _all;

  public FunctionSignatureRegistry(IEnumerable<HixFunctionEntry> definitions) {
    _all = definitions.ToArray();
    _definitions = _all.GroupBy(definition => definition.Name, StringComparer.Ordinal)
      .ToDictionary(group => group.Key, group => group
        .OrderBy(definition => definition.Signatures.Select(signature => signature.ArgumentTypes.Count(kind => kind == HixValueKind.Any)).DefaultIfEmpty(int.MaxValue).Min()).ToArray(),
        StringComparer.Ordinal);
  }

  public bool TryGet(string name, int argumentCount, out HixFunctionEntry definition) {
    definition = Resolve(name, argumentCount).FirstOrDefault();
    return definition is not null;
  }

  public IReadOnlyList<HixFunctionEntry> Resolve(string name, int argumentCount) =>
    _definitions.TryGetValue(name, out var definitions)
      ? definitions.Where(definition => definition.MatchesArgumentCount(argumentCount)).ToArray()
      : Array.Empty<HixFunctionEntry>();

  public IReadOnlyList<HixFunctionEntry> Resolve(string name) =>
    _definitions.TryGetValue(name, out var definitions) ? definitions : Array.Empty<HixFunctionEntry>();

  public IEnumerable<HixFunctionEntry> Enumerate() => _all;
}

public delegate IHixValue InlineFunction(HixCoroutine execution, IHixValue[] arguments);

public sealed class FunctionSignatureRegistryBuilder {
  private readonly List<FunctionDefinition> _definitions = [];
  private readonly List<string> libraries = [];
  private readonly List<CompilationUnitIr> libraryUnits = [];
  private readonly Dictionary<string, HixOpaqueTypeDefinition> opaqueTypes = new(StringComparer.Ordinal);
  private readonly Dictionary<string, HixPatternConstructor> patternConstructors = new(StringComparer.Ordinal);
  private readonly Dictionary<string, Func<FunctionSignature, FunctionSignature>> inference = new(StringComparer.Ordinal);

  public IReadOnlyList<FunctionDefinition> Handlers => _definitions;
  public IReadOnlyList<string> Libraries => libraries;
  public IReadOnlyList<CompilationUnitIr> LibraryUnits => libraryUnits;
  public IReadOnlyCollection<HixOpaqueTypeDefinition> OpaqueTypes => opaqueTypes.Values;
  public IReadOnlyCollection<HixPatternConstructor> PatternConstructors => patternConstructors.Values;

  public FunctionSignatureRegistryBuilder AddLibrary(string source) {
    libraries.Add(source ?? throw new ArgumentNullException(nameof(source)));
    return this;
  }
  public HixPatternConstructor DefinePatternConstructor(string name, int arity,
    Func<IReadOnlyList<HixPattern>, HixPattern> implementation, string documentation = "") {
    var constructor = new HixPatternConstructor(name, arity, implementation, documentation);
    if (patternConstructors.ContainsKey(name))
      throw new ArgumentException("Duplicate pattern constructor '" + name + "'.");
    patternConstructors.Add(name, constructor);
    return constructor;
  }

  public HixOpaqueTypeDefinition DefineOpaqueType(string identity, string name, int arity = 0, string documentation = "") {
    if (string.IsNullOrWhiteSpace(identity) || string.IsNullOrWhiteSpace(name) || arity < 0)
      throw new ArgumentException("An opaque type requires an identity, name, and non-negative arity.");
    var definition = new HixOpaqueTypeDefinition(identity, name, arity, documentation);
    if (opaqueTypes.TryGetValue(identity, out var existing)) {
      if (existing != definition) throw new ArgumentException("Conflicting opaque type identity '" + identity + "'.");
      return existing;
    }
    opaqueTypes.Add(identity, definition);
    return definition;
  }

  public FunctionSignatureRegistryBuilder Add(params FunctionDefinition[] definitions) {
    _definitions.AddRange(definitions);
    return this;
  }

  public FunctionSignatureRegistryBuilder AddLibrary(CompilationUnitIr unit) {
    libraryUnits.Add(unit ?? throw new ArgumentNullException(nameof(unit)));
    return this;
  }
  /// <summary>Configures compiler inference without attaching declaration metadata to host handlers.</summary>
  public FunctionSignatureRegistryBuilder DefineInference(string name, Func<FunctionSignature, FunctionSignature> configure) {
    inference.Add(name, configure);
    return this;
  }
  internal FunctionSignature ConfigureInference(string name, FunctionSignature signature) =>
    inference.TryGetValue(name, out var configure) ? configure(signature) : signature;
}

/// <summary>A name-only host implementation. Its signature comes from an external Hix declaration.</summary>
public sealed class SimpleFunction(string name, InlineFunction implementation) : FunctionDefinition(name) {
  public override IHixValue Execute(HixCoroutine execution, IHixValue[] arguments, int line) =>
    implementation(execution, arguments);
}

public class FunctionDefinition {
  public FunctionDefinition(string name) {
    Name = name ?? throw new ArgumentNullException(nameof(name));
  }

  public string Name { get; }

  public virtual IHixValue Execute(HixCoroutine context, IHixValue[] arguments, int line) =>
    context.Error("host function '" + Name + "' has no synchronous implementation");

  /// <summary>Optional nonblocking implementation. Null selects the synchronous Execute path.</summary>
  public virtual HixTask<IHixValue> ExecuteAsync(HixCoroutine context, IHixValue[] arguments, int line) => null;
}

public class HixFunctionEntry {


  public HixFunctionEntry(string name, IReadOnlyList<FunctionSignature> signatures, string documentation = null,
    HixMetadataKind metadata = HixMetadataKind.None) {
    if (signatures is null || signatures.Count == 0) throw new ArgumentException("A function requires at least one signature.", nameof(signatures));
    Name = name; Signatures = signatures;
    var primary = signatures[0]; ArgumentCount = primary.ArgumentCount; IsVariadic = primary.IsVariadic;
    ResultType = primary.ResultType; ArgumentTypes = primary.ArgumentTypes;
    ReceiverType = ArgumentTypes.Count == 0 ? HixValueKind.Any : ArgumentTypes[0];
    Documentation = documentation ?? ""; Metadata = metadata;
  }

  public string Name { get; }
  public int ArgumentCount { get; }
  public bool IsVariadic { get; }
  public HixValueKind ReceiverType { get; private set; }
  public HixValueKind ResultType { get; private set; }
  public IReadOnlyList<HixValueKind> ArgumentTypes { get; private set; }
  public virtual string Documentation { get; }
  public IReadOnlyList<FunctionSignature> Signatures { get; protected set; }
  public HixMetadataKind Metadata { get; }

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
  public bool RequiresPrelude { get; protected set; }
  public IReadOnlyDictionary<int, string> ArgumentReferences { get; protected set; } =
    new Dictionary<int, string>();
  public bool MatchesValues(IHixValue[] values) {
    return Signatures.Any(signature => signature.MatchesArgumentCount(values.Length) &&
      values.Select((value, index) => signature.GetArgumentType(index) is var expected &&
        (expected == HixValueKind.Any || value.Kind == expected)).All(match => match));
  }
  public bool TryConvertValues(HixCoroutine execution, IHixValue[] values,
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
  public static bool TryConvertValues(HixCoroutine execution, FunctionSignature signature, IHixValue[] values,
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
  public FunctionDefinition Handler { get; protected set; }

}

public abstract class EvaluatedFunctionDefinition(string name) : FunctionDefinition(name) {
  public sealed override IHixValue Execute(HixCoroutine execution, IHixValue[] supplied, int line) {
    using var profile = HixProfiler.MeasureFunction(Name);
    if (supplied.Length == 0) return execution.Error("function ':" + Name + "' requires a receiver");
    var arguments = supplied.Length == 1 ? Array.Empty<IHixValue>() : supplied.Skip(1).ToArray();
    return EvaluateValue(execution, supplied[0], arguments);
  }

  protected virtual IHixValue EvaluateValue(HixCoroutine context, IHixValue value, IReadOnlyList<IHixValue> arguments) => Apply(context, value, arguments);

  protected abstract IHixValue Apply(
    HixCoroutine context, IHixValue value,
    IReadOnlyList<IHixValue> arguments
  );
}
