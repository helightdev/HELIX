using System;
using System.Collections.Generic;
using System.Linq;
using Mixins.Compiler;
using Mixins.Env;
using Mixins.Runtime;

namespace Mixins;

public enum DirectiveOperandKind { None, Value, Boolean }

/// <summary>
/// Coarse language types used by editors and other analysis clients. They are intentionally
/// broader than runtime <see cref="IMixinValue"/> implementations: dynamic expressions remain
/// valid while statically meaningful call sites can still offer completion and diagnostics.
/// </summary>
public enum MixinLanguageValueKind {
  None,
  Any,
  Text,
  Table,
  Symbol,
  Type,
  Boolean,
  Function,
  CSharpType,
  Identifier,
  Label,
  OutputTarget,
  Expression
}

[Flags]
public enum MixinSymbolUsage { None = 0, Declaration = 1, Reference = 2 }

public enum MixinSymbolKind {
  None,
  Function,
  Label,
  Local,
  Variable,
  TargetVariable,
  Carry,
  Annotation,
  Derivation,
  Target,
  CSharpType
}

internal enum MixinDirectiveSyntaxForm {
  Invocation,
  Scope,
  Label,
  Function,
  Call,
  Inline,
  End,
  Match,
  Assert,
  Code,
  Mixin,
  Using,
  Log,
  Local,
  Variable,
  TargetVariable,
  Carry,
  Return,
  Goto,
  Skip,
  Fail,
  Annotation,
  Prelude,
  DefineTarget
}

public enum MixinExpressionRoot {
  Target,
  This,
  Attribute,
  Argument,
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

public sealed record MixinDirectiveArgumentMetadata(
  MixinLanguageValueKind ValueKind,
  MixinSymbolKind SymbolKind = MixinSymbolKind.None,
  MixinSymbolUsage SymbolUsage = MixinSymbolUsage.None
);

public sealed record MixinRootDefinition(
  string Name, MixinExpressionRoot Root, string Documentation
);

public class DirectiveDefinition(string name, DirectiveOperandKind operandKind, int argumentCount) {
  public string Name { get; } = name;
  public DirectiveOperandKind OperandKind { get; } = operandKind;
  public int ArgumentCount { get; } = argumentCount;
  public MixinLanguageValueKind OperandType { get; private set; }
  public IReadOnlyList<MixinDirectiveArgumentMetadata> Arguments { get; private set; }
  public IReadOnlyList<MixinLanguageValueKind> ArgumentTypes =>
    Arguments.Select(argument => argument.ValueKind).ToArray();
  public string Documentation { get; private set; }
  public FunctionDefinition Function { get; private set; }
  public int HoistedLocalArgumentIndex { get; private set; } = -1;

  internal DirectiveDefinition WithLanguageSignature(
    IReadOnlyList<MixinDirectiveArgumentMetadata> arguments,
    MixinLanguageValueKind? operandType = null,
    string documentation = null
  ) {
    if (arguments is null) throw new ArgumentNullException(nameof(arguments));
    if (string.IsNullOrWhiteSpace(documentation))
      throw new ArgumentException("Directive documentation is required.", nameof(documentation));
    if (arguments.Count != ArgumentCount)
      throw new ArgumentException("Directive argument signature does not match its arity.", nameof(arguments));
    OperandType = operandType ?? OperandKind switch {
      DirectiveOperandKind.Boolean => MixinLanguageValueKind.Boolean,
      DirectiveOperandKind.Value => MixinLanguageValueKind.Expression,
      _ => MixinLanguageValueKind.None
    };
    Arguments = arguments;
    Documentation = documentation;
    return this;
  }

  public MixinDirectiveArgumentMetadata GetArgumentMetadata(int index) =>
    index >= 0 && index < Arguments.Count ? Arguments[index] : null;

  public string ArgumentCountError() =>
    $"@{Name} requires {ArgumentCount}{(ArgumentCount == 1 ? " argument" : " arguments")}";

  internal virtual InstructionAst CreateSyntax(MixinDirectiveSyntaxData data) =>
    new DirectiveInvocationAst(this, data.ParsedArguments, data.ValueOperand);

  internal DirectiveDefinition WithFunction(
    FunctionDefinition function, int hoistedLocalArgumentIndex = -1
  ) {
    Function = function ?? throw new ArgumentNullException(nameof(function));
    if (!function.MatchesArgumentCount(ArgumentCount))
      throw new ArgumentException("Directive and function argument descriptors must match.", nameof(function));
    if (!function.ArgumentTypes.SequenceEqual(ArgumentTypes))
      throw new ArgumentException("Directive and function argument types must match.", nameof(function));
    HoistedLocalArgumentIndex = hoistedLocalArgumentIndex;
    return this;
  }
}

internal sealed class SyntaxDirectiveDefinition(
  string name, DirectiveOperandKind operandKind, int argumentCount,
  MixinDirectiveSyntaxForm syntaxForm
) : DirectiveDefinition(name, operandKind, argumentCount) {
  internal override InstructionAst CreateSyntax(MixinDirectiveSyntaxData data) {
    var arguments = data.Arguments;
    return syntaxForm switch {
      MixinDirectiveSyntaxForm.Scope => new ScopeAst(arguments.FirstOrDefault()),
      MixinDirectiveSyntaxForm.Label => new LabelAst(arguments[0]),
      MixinDirectiveSyntaxForm.Function => new FunctionAst(arguments[0]),
      MixinDirectiveSyntaxForm.Call => new CallAst(
        arguments.Count == 2 ? arguments[1] : arguments[0],
        arguments.Count == 2 ? arguments[0] : null, data.ValueOperand
      ),
      MixinDirectiveSyntaxForm.Inline => new InlineAst(arguments[0]),
      MixinDirectiveSyntaxForm.End => new EndAst(),
      MixinDirectiveSyntaxForm.Match => new MatchAst(arguments.FirstOrDefault(), data.BooleanOperand),
      MixinDirectiveSyntaxForm.Assert => new AssertAst(data.BooleanOperand),
      MixinDirectiveSyntaxForm.Code => CreateCodeSyntax(data),
      MixinDirectiveSyntaxForm.Mixin => new TargetedCodeAst(
        data.ParsedArguments[0], data.ParsedArguments.Count > 1 ? data.ParsedArguments[1] : null, data.ValueOperand
      ),
      MixinDirectiveSyntaxForm.Using => new UsingAst(data.ValueOperand),
      MixinDirectiveSyntaxForm.Log => new LogAst(data.ValueOperand),
      MixinDirectiveSyntaxForm.Local => new LocalAst(arguments[0], data.ValueOperand),
      MixinDirectiveSyntaxForm.Variable => new VariableAst(arguments[0], data.ValueOperand),
      MixinDirectiveSyntaxForm.TargetVariable => new TargetVariableAst(arguments[0], data.ValueOperand),
      MixinDirectiveSyntaxForm.Carry => new CarryAst(arguments[0], data.ValueOperand),
      MixinDirectiveSyntaxForm.Return => new ReturnAst(data.ValueOperand),
      MixinDirectiveSyntaxForm.Goto => new GotoAst(arguments[0]),
      MixinDirectiveSyntaxForm.Skip => new SkipAst(),
      MixinDirectiveSyntaxForm.Fail => new FailAst(data.ValueOperand),
      MixinDirectiveSyntaxForm.Annotation => new AnnotationAst(arguments[0]),
      MixinDirectiveSyntaxForm.Prelude => new PreludeAst(),
      MixinDirectiveSyntaxForm.DefineTarget => new DefineTargetAst(arguments[0], arguments[1]),
      _ => base.CreateSyntax(data)
    };
  }

  private static InstructionAst CreateCodeSyntax(MixinDirectiveSyntaxData data) {
    var argument = data.Arguments.FirstOrDefault();
    if (Enum.TryParse<MixinExpressionOutputTarget>(argument ?? "TARGET", true, out var target))
      return new CodeAst(target, null, data.ValueOperand);
    return new CodeAst(
      MixinExpressionOutputTarget.Injection, argument, data.ValueOperand
    );
  }
}

public abstract class FunctionDefinition {
  private string _predicateAlias;

  protected FunctionDefinition(
    string name, int argumentCount,
    MixinLanguageValueKind receiverType = MixinLanguageValueKind.Any,
    MixinLanguageValueKind resultType = MixinLanguageValueKind.Any,
    IReadOnlyList<MixinLanguageValueKind> argumentTypes = null,
    string documentation = null, bool variadic = false
  ) {
    Name = name;
    ArgumentCount = argumentCount;
    IsVariadic = variadic;
    ReceiverType = receiverType;
    ResultType = resultType;
    ArgumentTypes = argumentTypes ?? [
      .. Enumerable.Repeat(MixinLanguageValueKind.Any, argumentCount + (variadic ? 1 : 0))
    ];
    Documentation = documentation ?? "Transforms the current value.";
    if (ArgumentTypes.Count != argumentCount + (variadic ? 1 : 0))
      throw new ArgumentException("Function argument signature does not match its arity.", nameof(argumentTypes));
  }

  public string Name { get; }
  public int ArgumentCount { get; }
  public bool IsVariadic { get; }
  public virtual bool IsPredicate => false;
  public MixinLanguageValueKind ReceiverType { get; private set; }
  public MixinLanguageValueKind ResultType { get; private set; }
  public IReadOnlyList<MixinLanguageValueKind> ArgumentTypes { get; private set; }
  public string Documentation { get; private set; }

  public bool MatchesArgumentCount(int count) => IsVariadic ? count >= ArgumentCount : count == ArgumentCount;

  public string ArgumentCountError() => IsVariadic
    ? $":{Name} requires at least {ArgumentCount}{(ArgumentCount == 1 ? " argument" : " arguments")}"
    : $":{Name} requires {ArgumentCount}{(ArgumentCount == 1 ? " argument" : " arguments")}";

  public MixinLanguageValueKind GetArgumentType(int index) {
    if (index < 0 || ArgumentTypes.Count == 0) return MixinLanguageValueKind.None;
    if (index < ArgumentTypes.Count) return ArgumentTypes[index];
    return IsVariadic ? ArgumentTypes[ArgumentTypes.Count - 1] : MixinLanguageValueKind.None;
  }

  internal FunctionDefinition WithPredicateAlias(string alias) {
    _predicateAlias = string.IsNullOrWhiteSpace(alias) ? null : alias;
    return this;
  }

  internal bool MatchesPredicate(string name) => IsPredicate && (
    string.Equals(Name, name, StringComparison.Ordinal) ||
    string.Equals(_predicateAlias, name, StringComparison.Ordinal)
  );

  public abstract IMixinValue Invoke(
    ExecutionContext context, IMixinValue instance, IReadOnlyList<IMixinValue> arguments, bool negated
  );
}

internal abstract class EvaluatedFunctionDefinition(
  string name, int argumentCount,
  MixinLanguageValueKind receiverType = MixinLanguageValueKind.Any,
  MixinLanguageValueKind resultType = MixinLanguageValueKind.Any,
  IReadOnlyList<MixinLanguageValueKind> argumentTypes = null,
  bool predicate = false, bool variadic = false
) : FunctionDefinition(
  name, argumentCount, receiverType,
  predicate ? MixinLanguageValueKind.Boolean : resultType, argumentTypes,
  predicate ? "Tests the current value." : "Transforms the current value.", variadic
) {
  private readonly string _cacheKey = ":" + name;
  public override bool IsPredicate => predicate;

  public sealed override IMixinValue Invoke(
    ExecutionContext context, IMixinValue instance,
    IReadOnlyList<IMixinValue> arguments, bool negated
  ) {
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
    if (!predicate) return negated ? context.Error($"value function ':{Name}' cannot be negated") : result;
    var truth = result.IsTruthy(context);
    return truth != negated ? BooleanMixinValue.True : BooleanMixinValue.False;
  }

  protected abstract IMixinValue Apply(
    ExecutionContext context, IMixinValue value,
    IReadOnlyList<IMixinValue> arguments
  );
}

internal abstract class PredicateFunctionDefinition(
  string name, int argumentCount,
  MixinLanguageValueKind receiverType = MixinLanguageValueKind.Any,
  IReadOnlyList<MixinLanguageValueKind> argumentTypes = null, bool variadic = false
) : EvaluatedFunctionDefinition(
  name, argumentCount, receiverType, argumentTypes: argumentTypes, predicate: true, variadic: variadic
) {
  protected static BooleanMixinValue Result(bool value) {
    return value ? BooleanMixinValue.True : BooleanMixinValue.False;
  }

  protected static string Comparable(IMixinValue value, ExecutionContext context) {
    var text = value.Render(context).Resolve(context.Strings) ?? "";
    if (text.StartsWith("global::", StringComparison.Ordinal)) text = text.Substring(8);
    return text.Length >= 2 && text[0] == '"' && text[text.Length - 1] == '"'
      ? text.Substring(1, text.Length - 2)
      : text;
  }
}

internal abstract class EvaluatedDirectiveFunction(
  string name, int arguments, IReadOnlyList<MixinLanguageValueKind> argumentTypes
) : FunctionDefinition(name, arguments, argumentTypes: argumentTypes) {
  protected virtual bool EvaluateOperand => true;

  public sealed override IMixinValue Invoke(
    ExecutionContext context, IMixinValue instance,
    IReadOnlyList<IMixinValue> arguments, bool negated
  ) {
    var values = arguments.Select(context.Evaluate).ToArray();
    var error = values.OfType<ErrorMixinValue>().FirstOrDefault();
    if (error is not null) return error;
    if (EvaluateOperand) instance = context.Evaluate(instance);
    return instance is ErrorMixinValue ? instance : Apply(context, values, instance);
  }

  protected abstract IMixinValue Apply(
    ExecutionContext context, IReadOnlyList<IMixinValue> arguments,
    IMixinValue operand
  );
}