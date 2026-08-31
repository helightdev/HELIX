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
  None, Any, Text, Table, Symbol, Type, Boolean, Function, CSharpType, Identifier, Label,
  OutputTarget, Expression
}

[Flags]
public enum MixinSymbolUsage { None = 0, Declaration = 1, Reference = 2 }

public enum MixinSymbolKind {
  None, Function, Label, Local, Variable, TargetVariable, Carry, Annotation, Derivation,
  Target, CSharpType
}

internal enum MixinDirectiveSyntaxForm {
  Invocation, Scope, Label, Function, Call, Inline, End, Match, Assert, Code, Mixin,
  Using, Log, Local, Variable, TargetVariable, Carry, Return, Goto, Skip, Fail,
  Annotation, Prelude, DefineTarget
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

public sealed record MixinDirectiveMetadata(
  MixinLanguageValueKind OperandType,
  IReadOnlyList<MixinDirectiveArgumentMetadata> Arguments,
  IReadOnlyDictionary<int, IReadOnlyList<MixinDirectiveArgumentMetadata>> ArityArguments,
  string Documentation
);

public sealed record MixinFunctionMetadata(
  MixinLanguageValueKind ReceiverType,
  MixinLanguageValueKind ResultType,
  IReadOnlyList<MixinLanguageValueKind> ArgumentTypes,
  string Documentation
);

public sealed record MixinRootDefinition(
  string Name, MixinExpressionRoot Root, string Documentation
);

public class DirectiveDefinition(string name, DirectiveOperandKind operandKind, int maximumArguments = 1,
  int minimumArguments = 0) {
  public string Name { get; } = name;
  public DirectiveOperandKind OperandKind { get; } = operandKind;
  public int MaximumArguments { get; } = maximumArguments;
  public int MinimumArguments { get; } = minimumArguments;
  public MixinDirectiveMetadata Metadata { get; private set; }
  public MixinLanguageValueKind OperandType => Metadata.OperandType;
  public IReadOnlyList<MixinLanguageValueKind> ArgumentTypes =>
    Metadata.Arguments.Select(argument => argument.ValueKind).ToArray();
  public IReadOnlyList<MixinDirectiveArgumentMetadata> ArgumentMetadata => Metadata.Arguments;
  public string Documentation => Metadata.Documentation;
  public FunctionDefinition Function { get; private set; }
  public int HoistedLocalArgumentIndex { get; private set; } = -1;

  internal DirectiveDefinition WithLanguageSignature(
    IReadOnlyList<MixinDirectiveArgumentMetadata> arguments,
    MixinLanguageValueKind? operandType = null,
    string documentation = null,
    IReadOnlyDictionary<int, IReadOnlyList<MixinDirectiveArgumentMetadata>> arityArguments = null
  ) {
    if (arguments is null) throw new ArgumentNullException(nameof(arguments));
    if (string.IsNullOrWhiteSpace(documentation))
      throw new ArgumentException("Directive documentation is required.", nameof(documentation));
    Metadata = new MixinDirectiveMetadata(
      operandType ?? OperandKind switch {
        DirectiveOperandKind.Boolean => MixinLanguageValueKind.Boolean,
        DirectiveOperandKind.Value => MixinLanguageValueKind.Expression,
        _ => MixinLanguageValueKind.None
      }, arguments, arityArguments ?? new Dictionary<int, IReadOnlyList<MixinDirectiveArgumentMetadata>>(), documentation
    );
    return this;
  }

  public MixinDirectiveArgumentMetadata GetArgumentMetadata(int argumentCount, int index) {
    var arguments = Metadata.ArityArguments.TryGetValue(argumentCount, out var exact)
      ? exact : Metadata.Arguments;
    return index >= 0 && index < arguments.Count ? arguments[index] : null;
  }

  internal virtual InstructionAst CreateSyntax(MixinDirectiveSyntaxData data) =>
    new DirectiveInvocationAst(this, data.ParsedArguments, data.ValueOperand);

  internal DirectiveDefinition WithFunction(
    FunctionDefinition function, int hoistedLocalArgumentIndex = -1
  ) {
    Function = function ?? throw new ArgumentNullException(nameof(function));
    if (function.MinimumArguments != MinimumArguments || function.MaximumArguments != MaximumArguments)
      throw new ArgumentException("Directive and function argument descriptors must match.", nameof(function));
    if (!function.ArgumentTypes.SequenceEqual(ArgumentTypes))
      throw new ArgumentException("Directive and function argument types must match.", nameof(function));
    HoistedLocalArgumentIndex = hoistedLocalArgumentIndex;
    return this;
  }

  internal virtual bool Validate(IReadOnlyList<string> arguments, string operand, out string error) {
    if (arguments.Count >= MinimumArguments && arguments.Count <= MaximumArguments) {
      error = null;
      return true;
    }
    error = arguments.Count < MinimumArguments
      ? Name + " requires at least " + MinimumArguments + " arguments"
      : Name + " accepts at most " + MaximumArguments + " arguments";
    return false;
  }
}

internal sealed class SyntaxDirectiveDefinition(
  string name, DirectiveOperandKind operandKind, int maximumArguments,
  int minimumArguments, MixinDirectiveSyntaxForm syntaxForm
) : DirectiveDefinition(name, operandKind, maximumArguments, minimumArguments) {
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
      MixinDirectiveSyntaxForm.Match => new MatchAst(
        arguments.FirstOrDefault(), data.BooleanOperand
      ),
      MixinDirectiveSyntaxForm.Assert => new AssertAst(data.BooleanOperand),
      MixinDirectiveSyntaxForm.Code => CreateCodeSyntax(data),
      MixinDirectiveSyntaxForm.Mixin => new TargetedCodeAst(
        data.ParsedArguments[0],
        data.ParsedArguments.Count > 1 ? data.ParsedArguments[1] : null, data.ValueOperand
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
      MixinDirectiveSyntaxForm.DefineTarget => new DefineTargetAst(
        arguments[0], arguments[1]
      ),
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
  protected FunctionDefinition(string name, int minimumArguments, int maximumArguments) {
    Name = name;
    MinimumArguments = minimumArguments;
    MaximumArguments = maximumArguments;
  }

  public string Name { get; }
  public int MinimumArguments { get; }
  public int MaximumArguments { get; }
  public virtual bool IsPredicate => false;
  public MixinFunctionMetadata Metadata { get; private set; }
  public MixinLanguageValueKind ReceiverType => Metadata.ReceiverType;
  public MixinLanguageValueKind ResultType => Metadata.ResultType;
  public IReadOnlyList<MixinLanguageValueKind> ArgumentTypes => Metadata.ArgumentTypes;
  public string Documentation => Metadata.Documentation;

  public MixinLanguageValueKind GetArgumentType(int index) {
    if (index < 0 || ArgumentTypes.Count == 0) return MixinLanguageValueKind.None;
    if (index < ArgumentTypes.Count) return ArgumentTypes[index];
    return MaximumArguments == int.MaxValue ? ArgumentTypes[ArgumentTypes.Count - 1] : MixinLanguageValueKind.None;
  }

  internal FunctionDefinition WithPredicateAlias(string alias) {
    _predicateAlias = string.IsNullOrWhiteSpace(alias) ? null : alias;
    return this;
  }

  internal bool MatchesPredicate(string name) => IsPredicate &&
    (string.Equals(Name, name, StringComparison.Ordinal) ||
      string.Equals(_predicateAlias, name, StringComparison.Ordinal));

  internal FunctionDefinition WithLanguageSignature(
    MixinLanguageValueKind receiver,
    MixinLanguageValueKind result,
    IReadOnlyList<MixinLanguageValueKind> arguments = null,
    string documentation = null
  ) {
    if (arguments is null) throw new ArgumentNullException(nameof(arguments));
    if (string.IsNullOrWhiteSpace(documentation))
      throw new ArgumentException("Function documentation is required.", nameof(documentation));
    Metadata = new MixinFunctionMetadata(receiver, result, arguments, documentation);
    return this;
  }

  public abstract IMixinValue Invoke(
    ExecutionContext context, IMixinValue instance,
    IReadOnlyList<IMixinValue> arguments, bool negated
  );
}


internal abstract class EvaluatedFunctionDefinition(string name, int minimumArguments, int maximumArguments,
  bool predicate = false
) : FunctionDefinition(name, minimumArguments, maximumArguments) {
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
              "\u001f", values.Select(item => item.GetType().FullName + "=" +
                item.Render(context).Resolve(context.Strings)
              )
            );
        }
        if (!roslyn.TryGetDerived(source, key, out result)) {
          result = Apply(context, instance, values);
          roslyn.StoreDerived(source, key, result);
        }
      } else result = Apply(context, instance, values);
    } catch (ArgumentException exception) {
      return context.Error("function ':" + Name + "' failed: " + exception.Message);
    }
    if (predicate) {
      var truth = result.IsTruthy(context);
      return truth != negated ? BooleanMixinValue.True : BooleanMixinValue.False;
    }
    return negated ? context.Error("value function ':" + Name + "' cannot be negated") : result;
  }

  protected abstract IMixinValue Apply(
    ExecutionContext context, IMixinValue value,
    IReadOnlyList<IMixinValue> arguments
  );
}

internal abstract class PredicateFunctionDefinition(string name, int minimumArguments, int maximumArguments)
  : EvaluatedFunctionDefinition(name, minimumArguments, maximumArguments, true) {
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

internal abstract class EvaluatedDirectiveFunction(string name, int arguments)
  : FunctionDefinition(name, arguments, arguments) {
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