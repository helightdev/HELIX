using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace HELIX.SourceGen.Expressions;

public enum MixinExpressionOutputTarget { Target, Class, File, Implements, Injection, Annotation, Using, Mixin }

public sealed record MixinExpressionOutput {
  private readonly IReadOnlyList<MixinString> _segments;
  private readonly MixinStringPool _pool;
  private readonly MixinString _injectionTarget;

  public MixinExpressionOutput(
    MixinExpressionOutputTarget target,
    string text,
    string injectionTarget = null,
    int injectionPriority = 0
  ) : this(
    target, new[] { MixinString.Dynamic(text) }, new MixinStringPoolBuilder().Freeze(),
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
    _segments = segments ?? Array.Empty<MixinString>();
    _pool = pool ?? throw new ArgumentNullException(nameof(pool));
    _injectionTarget = injectionTarget;
    InjectionPriority = injectionPriority;
  }

  public MixinExpressionOutputTarget Target { get; }
  public string Text => string.Concat(_segments.Select(segment => segment.Resolve(_pool)));
  public string InjectionTarget => _injectionTarget.Resolve(_pool);
  public int InjectionPriority { get; }
  internal IReadOnlyList<MixinString> Segments => _segments;
  internal string Resolve(MixinString segment) => segment.Resolve(_pool);
  internal bool IsEmpty => _segments.All(segment => string.IsNullOrEmpty(segment.Resolve(_pool)));
  internal MixinExpressionOutput Retarget(MixinExpressionOutputTarget target) => new(
    target, _segments, _pool, _injectionTarget, InjectionPriority
  );
}

public record MixinExpressionLog(string Text = "", int Line = -1, bool IsHint = false);

public record MixinExpressionPreparedLog(string Text, int Line, int ProgramIndex);

public sealed class MixinExpressionProperty : FunctionInvocation {
  public MixinExpressionProperty(
    string name, string argument = null, bool negated = false
  ) : base(name, argument is null ? Array.Empty<string>() : new[] { argument }, negated) {
    Values = Arguments.Cast<object>().ToArray();
  }

  public MixinExpressionProperty(
    string name, IReadOnlyList<string> arguments, bool negated = false
  ) : base(name, arguments, negated) {
    Values = Arguments.Cast<object>().ToArray();
  }

  internal MixinExpressionProperty(
    string name,
    IReadOnlyList<string> arguments,
    IReadOnlyList<object> values,
    bool negated
  ) : base(name, arguments, negated) {
    Values = values ?? Array.Empty<object>();
  }

  internal IReadOnlyList<object> Values { get; }
}

/// <summary>An immutable expression table. Mutating operations return a new table.</summary>
public sealed class MixinExpressionTable : MixinValue {
  private readonly Dictionary<string, IMixinValue> _values;
  private bool _closed;

  public MixinExpressionTable() : this(new Dictionary<string, IMixinValue>(StringComparer.Ordinal)) { }

  private MixinExpressionTable(Dictionary<string, IMixinValue> values) {
    _values = values;
  }

  public int Count => _values.Count;
  public bool IsClosed => _closed;

  internal IEnumerable<IMixinValue> Values => _values.Values;
  internal IEnumerable<KeyValuePair<string, IMixinValue>> Entries => _values;

  public override object BackingValue => this;
  public override bool IsTruthy => true;
  public override string Render() => ToString();

  public override bool TryGetText(out string text) {
    text = null;
    return false;
  }

  public override object Select(string path) => TryGetValue(path, out var value) ? value : null;
  public override bool Has(object member) => _values.ContainsKey(Convert.ToString(member) ?? "");

  public override IMixinValue Unlink() {
    var result = new MixinExpressionTable();
    foreach (var item in _values) result = result.Put(item.Key, item.Value.Unlink().BackingValue);
    return result.Close();
  }

  public override bool Equals(object obj) {
    if (ReferenceEquals(this, obj)) return true;
    if (obj is not MixinExpressionTable other || Count != other.Count) return false;
    foreach (var item in _values)
      if (!other._values.TryGetValue(item.Key, out var value) || !Equals(item.Value, value))
        return false;
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
      value = typed.BackingValue;
      return true;
    }
    value = null;
    return false;
  }

  internal bool TryGetMixinValue(string key, out IMixinValue value) => _values.TryGetValue(key ?? "", out value);

  internal MixinExpressionTable Put(string key, object value) {
    var result = Writable();
    var typed = MixinValue.From(value);
    if (typed.BackingValue is MixinExpressionTable nested) nested.Close();
    result._values[key ?? ""] = typed;
    return result;
  }

  internal MixinExpressionTable Remove(string key) {
    var result = Writable();
    result._values.Remove(key ?? "");
    return result;
  }

  internal MixinExpressionTable Close() {
    if (_closed) return this;
    _closed = true;
    return this;
  }

  private MixinExpressionTable Writable() {
    if (!_closed) return this;
    return new MixinExpressionTable(
      _values.ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal)
    );
  }

  public override string ToString() {
    return "table[" + Count.ToString(CultureInfo.InvariantCulture) + "]";
  }
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

public sealed class MixinExpressionReference {
  public MixinExpressionReference(
    MixinExpressionRoot root,
    string member,
    IReadOnlyList<MixinExpressionProperty> properties
  ) {
    Root = root;
    Member = member;
    Properties = properties ?? Array.Empty<MixinExpressionProperty>();
  }

  public MixinExpressionRoot Root { get; }
  public string Member { get; }
  public IReadOnlyList<MixinExpressionProperty> Properties { get; }

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
  bool TryResolve(
    MixinExpressionReference reference,
    out string value,
    out string error
  );

  bool TryEvaluate(
    MixinExpressionReference reference,
    out bool value,
    out string error
  );
}

/// <summary>Resolves a host value without forcing it through string rendering.</summary>
public interface IMixinExpressionValueContext : IMixinExpressionContext {
  bool TryResolveValue(
    MixinExpressionReference reference,
    out object value,
    out string error
  );

  bool TryRenderValue(
    object value, MixinExpressionRoot? root, out string text, out string error
  );
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
    out object handle,
    out string declaration,
    out string error
  );

  bool TryApplyPropStructProperty(
    object handle,
    MixinExpressionProperty property,
    out object value,
    out string error
  );
}

/// <summary>Optional host support for configuring prop struct declaration generation.</summary>
public interface IMixinExpressionConfigurablePropStructContext : IMixinExpressionPropStructContext {
  bool TryCreatePropStruct(
    string structName,
    MixinExpressionReference syntaxTarget,
    bool generateDatatype,
    bool generateDeclaration,
    out object handle,
    out string declaration,
    out string error
  );
}

/// <summary>Optional host support for augmenting an existing prop struct.</summary>
public interface IMixinExpressionStructAugmentationContext {
  bool TryAugmentPropStruct(
    MixinExpressionReference syntaxTarget,
    out object handle,
    out string declaration,
    out string error
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
    Outputs = outputs ?? Array.Empty<MixinExpressionOutput>();
    Logs = logs ?? Array.Empty<MixinExpressionLog>();
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
