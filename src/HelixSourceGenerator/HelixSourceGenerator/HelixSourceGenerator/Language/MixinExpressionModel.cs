using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HelixSourceGenerator.Language.Compiler;

namespace HelixSourceGenerator.Language;

public enum MixinExpressionOutputTarget {
  Target,
  Class,
  File,
  Implements,
  Injection,
  Annotation,
  Using,
  Mixin
}

public sealed record MixinExpressionOutput {
  private readonly MixinString _injectionTarget;
  private readonly MixinStringPool _pool;

  public MixinExpressionOutput(
    MixinExpressionOutputTarget target,
    string text,
    string injectionTarget = null,
    int injectionPriority = 0
  ) : this(
    target, [MixinString.Dynamic(text)], new MixinStringPoolBuilder().Freeze(),
    MixinString.Dynamic(injectionTarget), injectionPriority
  ) { }

  internal MixinExpressionOutput(
    MixinExpressionOutputTarget target,
    IReadOnlyList<MixinString> segments,
    MixinStringPool pool,
    MixinString injectionTarget,
    int injectionPriority = 0
  ) {
    Target = target;
    Segments = segments ?? [];
    _pool = pool ?? throw new ArgumentNullException(nameof(pool));
    _injectionTarget = injectionTarget;
    InjectionPriority = injectionPriority;
  }

  public MixinExpressionOutputTarget Target { get; }
  public string Text => string.Concat(Segments.Select(segment => segment.Resolve(_pool)));
  public string InjectionTarget => _injectionTarget.Resolve(_pool);
  public int InjectionPriority { get; }
  internal IReadOnlyList<MixinString> Segments { get; }
  internal bool IsEmpty => Segments.All(segment => string.IsNullOrEmpty(segment.Resolve(_pool)));
  internal string Resolve(MixinString segment) => segment.Resolve(_pool);
  internal MixinExpressionOutput Retarget(MixinExpressionOutputTarget target) {
    return new MixinExpressionOutput(target, Segments, _pool, _injectionTarget, InjectionPriority);
  }
}

public record MixinExpressionLog(string Text = "", int Line = -1, bool IsHint = false);

public record MixinExpressionPreparedLog(string Text, int Line, int ProgramIndex);

public sealed class MixinExpressionProperty : FunctionInvocation {
  public MixinExpressionProperty(
    string name, string argument = null, bool negated = false
  ) : base(name, argument is null ? Array.Empty<string>() : new[] { argument }, negated) {
    Values = [.. Arguments];
  }

  public MixinExpressionProperty(
    string name, IReadOnlyList<string> arguments, bool negated = false
  ) : base(name, arguments, negated) {
    Values = [.. Arguments];
  }

  internal MixinExpressionProperty(
    string name,
    IReadOnlyList<string> arguments,
    IReadOnlyList<object> values,
    bool negated
  ) : base(name, arguments, negated) {
    Values = values ?? [];
    ParsedArguments = [];
  }

  internal MixinExpressionProperty(
    string name, IReadOnlyList<string> arguments,
    IReadOnlyList<MixinPropertyArgumentSyntax> parsedArguments, bool negated
  ) : base(name, arguments, negated) {
    Values = [.. Arguments];
    ParsedArguments = parsedArguments ?? [];
  }

  internal IReadOnlyList<object> Values { get; }
  internal IReadOnlyList<MixinPropertyArgumentSyntax> ParsedArguments { get; } = [];
}

/// <summary>An immutable expression table. Mutating operations return a new table.</summary>
public sealed class MixinExpressionTable : MixinValue {
  private readonly Dictionary<string, IMixinValue> _values;

  public MixinExpressionTable() : this(new Dictionary<string, IMixinValue>(StringComparer.Ordinal)) { }

  private MixinExpressionTable(Dictionary<string, IMixinValue> values) {
    _values = values;
  }

  public int Count => _values.Count;
  public bool IsClosed { get; private set; }

  internal IEnumerable<IMixinValue> Values => _values.Values;
  internal IEnumerable<KeyValuePair<string, IMixinValue>> Entries => _values;

  public override object Value => this;
  public override bool IsTruthy => true;
  public override string Render() => ToString();

  public override void Fingerprint(MixinFingerprintBuilder builder) {
    builder.Append(nameof(MixinExpressionTable));
    builder.Append(Count);
    foreach (var item in _values.OrderBy(item => item.Key, StringComparer.Ordinal)) {
      builder.Append(item.Key);
      item.Value.Fingerprint(builder);
    }
  }

  public override bool TryGetText(out string text) {
    text = null;
    return false;
  }

  public override object Select(string path) => TryGetValue(path, out var value) ? value : null;

  public override bool Has(object member) => _values.ContainsKey(Convert.ToString(member));
  internal bool ContainsValue(object expected) => Values.Any(item => MixinValue.RelaxedEquals(item, expected));

  public override IMixinValue Unlink() {
    var result = new MixinExpressionTable();
    result = _values.Aggregate(result, (current, item) => current.Put(item.Key, item.Value.Unlink().Value));
    return result.Close();
  }

  public override bool Equals(object obj) {
    if (ReferenceEquals(this, obj)) return true;
    if (obj is not MixinExpressionTable other || Count != other.Count) return false;
    foreach (var item in _values) {
      if (!other._values.TryGetValue(item.Key, out var value) || !Equals(item.Value, value)) return false;
    }
    return true;
  }

  public override int GetHashCode() {
    var hash = 17;
    foreach (var item in _values.OrderBy(item => item.Key, StringComparer.Ordinal)) {
      hash = unchecked(hash * 31 + StringComparer.Ordinal.GetHashCode(item.Key));
      hash = unchecked(hash * 31 + (item.Value?.GetHashCode() ?? 0));
    }
    return hash;
  }

  public bool TryGetValue(string key, out object value) {
    if (_values.TryGetValue(key ?? "", out var typed)) {
      value = typed.Value;
      return true;
    }
    value = null;
    return false;
  }

  internal bool TryGetMixinValue(string key, out IMixinValue value) => _values.TryGetValue(key ?? "", out value);

  internal MixinExpressionTable Put(string key, object value) {
    var result = Writable();
    var typed = From(value);
    if (typed.Value is MixinExpressionTable nested) nested.Close();
    result._values[key ?? ""] = typed;
    return result;
  }

  internal MixinExpressionTable Remove(string key) {
    var result = Writable();
    result._values.Remove(key ?? "");
    return result;
  }

  internal MixinExpressionTable Close() {
    if (IsClosed) return this;
    IsClosed = true;
    return this;
  }

  private MixinExpressionTable Writable() => !IsClosed
    ? this
    : new MixinExpressionTable(_values.ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal));

  public override string ToString() => "table[" + Count.ToString(CultureInfo.InvariantCulture) + "]";
}

public enum MixinExpressionRoot {
  Target,
  This,
  Attribute,
  Argument,
  Variable,
  Local,
  True,
  False,
  Null,
  Table,
  Parameter,
  Carry
}

public sealed class MixinExpressionReference(
  MixinExpressionRoot root,
  string member,
  IReadOnlyList<MixinExpressionProperty> properties
) {
  public MixinExpressionRoot Root { get; } = root;
  public string Member { get; } = member;
  public IReadOnlyList<MixinExpressionProperty> Properties { get; } = properties ?? [];

  internal void CollectConstants(MixinStringPoolBuilder pool) {
    if (Member is not null) pool.Intern(Member);
    foreach (var property in Properties) {
      pool.Intern(property.Name);
      foreach (var argument in property.Arguments) pool.Intern(argument);
    }
  }
}

/// <summary>Resolves host-specific values and predicates used by a mixin expression.</summary>
public interface IMixinExpressionContext {
  bool TryResolve(MixinExpressionReference reference, out string value, out string error);
  bool TryEvaluate(MixinExpressionReference reference, out bool value, out string error);
}

/// <summary>Resolves a host value without forcing it through string rendering.</summary>
public interface IMixinExpressionValueContext : IMixinExpressionContext {
  bool TryResolveValue(MixinExpressionReference reference, out object value, out string error);
  bool TryRenderValue(object value, MixinExpressionRoot? root, out string text, out string error);
}

/// <summary>Optional host support for resolving generated targets and callable signatures.</summary>
public interface IMixinExpressionSignatureContext {
  bool TryResolveMixin(string target, out string callable, out string error);
  bool TryWire(string from, string to, out string arguments, out string error);
  bool TryHaveSameSignature(string first, string second, out bool value, out string error);
  bool TryWireable(string from, string to, out bool value, out string error);
}

/// <summary>Optional host support for creating and consuming generated prop structs.</summary>
public interface IMixinExpressionPropStructContext {
  bool TryCreatePropStruct(
    string structName,
    MixinExpressionReference syntaxTarget,
    out object handle, out string declaration, out string error
  );

  bool TryApplyPropStructProperty(
    object handle,
    MixinExpressionProperty property,
    out object value, out string error
  );
}

/// <summary>Optional host support for configuring prop struct declaration generation.</summary>
public interface IMixinExpressionConfigurablePropStructContext : IMixinExpressionPropStructContext {
  bool TryCreatePropStruct(
    string structName, MixinExpressionReference syntaxTarget,
    bool generateDatatype, bool generateDeclaration,
    out object handle, out string declaration, out string error
  );
}

/// <summary>Optional host support for augmenting an existing prop struct.</summary>
public interface IMixinExpressionStructAugmentationContext {
  bool TryAugmentPropStruct(
    MixinExpressionReference syntaxTarget,
    out object handle, out string declaration, out string error
  );
}

public sealed record MixinExpressionResult {
  internal MixinExpressionResult(
    bool success,
    string error,
    int errorLine,
    IReadOnlyList<MixinExpressionOutput> outputs,
    IReadOnlyList<MixinExpressionLog> logs = null,
    IReadOnlyDictionary<string, object> variables = null,
    int executedOperations = 0,
    double executionMilliseconds = 0
  ) {
    Success = success;
    Error = error;
    ErrorLine = errorLine;
    Outputs = outputs ?? [];
    Logs = logs ?? [];
    Variables = variables ?? new Dictionary<string, object>();
    ExecutedOperations = executedOperations;
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

public sealed record MixinExpressionValidationResult(bool Success, string Error, int ErrorLine);

/// <summary>
///   Immutable, context-free result of compiling and evaluating prepared mixins.  The generator
///   may safely retain this object in an incremental value and share it between target runs.
/// </summary>
public sealed record MixinExpressionPreparedState(
  MixinStringPool StringPool,
  IReadOnlyList<MixinProgramSyntax> Programs,
  IReadOnlyDictionary<string, object> Variables,
  IReadOnlyList<DirectiveInstruction> Instructions,
  IReadOnlyDictionary<string, int> Labels,
  IReadOnlyDictionary<int, int> InstructionScopes,
  IReadOnlyDictionary<string, MixinExpressionCompiler.FunctionDefinition> Functions,
  IReadOnlyDictionary<int, int> FunctionStarts,
  ISet<int> FunctionEnds,
  ISet<int> Initializers,
  IReadOnlyList<MixinExpressionPreparedLog> Logs,
  int ExecutedOperations
);

internal sealed record MixinExpressionExecutionProgram(
  MixinStringPool StringPool,
  IReadOnlyList<DirectiveInstruction> Instructions,
  IReadOnlyDictionary<string, object> Variables,
  IReadOnlyDictionary<string, int> Labels,
  IReadOnlyDictionary<int, int> InstructionScopes,
  IReadOnlyDictionary<string, MixinExpressionCompiler.FunctionDefinition> Functions,
  IReadOnlyDictionary<int, int> FunctionStarts,
  ISet<int> FunctionEnds,
  ISet<int> Initializers
);
