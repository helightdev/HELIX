using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace HELIX.SourceGen.Expressions;

public enum MixinExpressionOutputTarget { Target, Class, File, Implements, Injection, Annotation, Using, Mixin }

public sealed class MixinExpressionOutput {
  public MixinExpressionOutput(
    MixinExpressionOutputTarget target,
    string text,
    string injectionTarget = null,
    int injectionPriority = 0
  ) {
    Target = target;
    Text = text ?? "";
    InjectionTarget = injectionTarget;
    InjectionPriority = injectionPriority;
  }

  public MixinExpressionOutputTarget Target { get; }
  public string Text { get; }
  public string InjectionTarget { get; }
  public int InjectionPriority { get; }
}

public sealed class MixinExpressionLog {
  internal MixinExpressionLog(string text, int line, bool isHint = false) {
    Text = text ?? "";
    Line = line;
    IsHint = isHint;
  }

  public string Text { get; }
  public int Line { get; }
  public bool IsHint { get; }
}

public sealed class MixinExpressionPreparedLog {
  internal MixinExpressionPreparedLog(string text, int line, int programIndex) {
    Text = text ?? "";
    Line = line;
    ProgramIndex = programIndex;
  }

  public string Text { get; }
  public int Line { get; }
  public int ProgramIndex { get; }
}

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
public sealed class MixinExpressionTable {
  private readonly IReadOnlyDictionary<string, object> _values;

  public MixinExpressionTable() : this(new Dictionary<string, object>(StringComparer.Ordinal)) { }

  private MixinExpressionTable(IReadOnlyDictionary<string, object> values) {
    _values = values;
  }

  public int Count => _values.Count;

  internal IEnumerable<object> Values => _values.Values;

  public bool TryGetValue(string key, out object value) {
    return _values.TryGetValue(key ?? "", out value);
  }

  internal MixinExpressionTable Put(string key, object value) {
    var result = _values.ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
    result[key ?? ""] = value;
    return new MixinExpressionTable(result);
  }

  internal MixinExpressionTable Remove(string key) {
    var result = _values.ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
    result.Remove(key ?? "");
    return new MixinExpressionTable(result);
  }

  public override string ToString() {
    return "table[" + Count.ToString(CultureInfo.InvariantCulture) + "]";
  }
}

public sealed class MixinExpressionReference {
  public MixinExpressionReference(
    string root,
    string member,
    IReadOnlyList<MixinExpressionProperty> properties
  ) {
    Root = root ?? throw new ArgumentNullException(nameof(root));
    Member = member;
    Properties = properties ?? Array.Empty<MixinExpressionProperty>();
  }

  public string Root { get; }
  public string Member { get; }
  public IReadOnlyList<MixinExpressionProperty> Properties { get; }
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

public sealed class MixinExpressionResult {
  internal MixinExpressionResult(
    bool success,
    string error,
    int errorLine,
    IReadOnlyList<MixinExpressionOutput> outputs,
    IReadOnlyList<MixinExpressionLog> logs = null
  ) {
    Success = success;
    Error = error;
    ErrorLine = errorLine;
    Outputs = outputs ?? Array.Empty<MixinExpressionOutput>();
    Logs = logs ?? Array.Empty<MixinExpressionLog>();
  }

  public bool Success { get; }
  public string Error { get; }
  public int ErrorLine { get; }
  public IReadOnlyList<MixinExpressionOutput> Outputs { get; }
  public IReadOnlyList<MixinExpressionLog> Logs { get; }
}

public sealed class MixinExpressionValidationResult {
  internal MixinExpressionValidationResult(bool success, string error, int errorLine) {
    Success = success;
    Error = error;
    ErrorLine = errorLine;
  }

  public bool Success { get; }
  public string Error { get; }
  public int ErrorLine { get; }
}

/// <summary>
///   Immutable, context-free result of compiling and evaluating prepared mixins.  The generator
///   may safely retain this object in an incremental value and share it between target runs.
/// </summary>
public sealed class MixinExpressionPreparedState {
  internal MixinExpressionPreparedState(
    IReadOnlyList<MixinProgramSyntax> programs,
    IReadOnlyDictionary<string, object> variables,
    IReadOnlyList<DirectiveInstruction> instructions,
    IReadOnlyDictionary<string, int> labels,
    IReadOnlyDictionary<int, int> instructionScopes,
    IReadOnlyDictionary<string, MixinExpressionCompiler.FunctionDefinition> functions,
    IReadOnlyDictionary<int, int> functionStarts,
    ISet<int> functionEnds,
    ISet<int> initializers,
    IReadOnlyList<MixinExpressionPreparedLog> logs,
    int executedOperations
  ) {
    Programs = programs;
    Variables = variables;
    Instructions = instructions;
    Labels = labels;
    InstructionScopes = instructionScopes;
    Functions = functions;
    FunctionStarts = functionStarts;
    FunctionEnds = functionEnds;
    Initializers = initializers;
    Logs = logs;
    ExecutedOperations = executedOperations;
  }

  internal IReadOnlyList<MixinProgramSyntax> Programs { get; }
  internal IReadOnlyDictionary<string, object> Variables { get; }
  internal IReadOnlyList<DirectiveInstruction> Instructions { get; }
  internal IReadOnlyDictionary<string, int> Labels { get; }
  internal IReadOnlyDictionary<int, int> InstructionScopes { get; }
  internal IReadOnlyDictionary<string, MixinExpressionCompiler.FunctionDefinition> Functions { get; }
  internal IReadOnlyDictionary<int, int> FunctionStarts { get; }
  internal ISet<int> FunctionEnds { get; }
  internal ISet<int> Initializers { get; }
  public IReadOnlyList<MixinExpressionPreparedLog> Logs { get; }
  public int ExecutedOperations { get; }
}