using System;
using System.Collections.Generic;
using System.Linq;
using MixinLanguage.Compiler;

namespace MixinLanguage;

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

  internal virtual DirectiveAst CreateSyntax(MixinDirectiveSyntaxData data) =>
    new DirectiveInvocationAst(data.SourceRange, this, data.ParsedArguments, data.ValueOperand);

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

internal enum MixinDirectiveSyntaxForm {
  Invocation, Scope, Label, Function, Call, Inline, End, Match, Assert, Code, Mixin,
  Using, Log, Local, Variable, TargetVariable, Carry, Return, Goto, Skip, Fail,
  Annotation, Prelude, DefineTarget
}

internal sealed class SyntaxDirectiveDefinition(
  string name, DirectiveOperandKind operandKind, int maximumArguments,
  int minimumArguments, MixinDirectiveSyntaxForm syntaxForm
) : DirectiveDefinition(name, operandKind, maximumArguments, minimumArguments) {
  internal override DirectiveAst CreateSyntax(MixinDirectiveSyntaxData data) {
    var arguments = data.Arguments;
    return syntaxForm switch {
      MixinDirectiveSyntaxForm.Scope => new ScopeAst(data.SourceRange, arguments.FirstOrDefault()),
      MixinDirectiveSyntaxForm.Label => new LabelAst(data.SourceRange, arguments[0]),
      MixinDirectiveSyntaxForm.Function => new FunctionAst(data.SourceRange, arguments[0]),
      MixinDirectiveSyntaxForm.Call => new CallDirectiveAst(
        data.SourceRange, arguments.Count == 2 ? arguments[1] : arguments[0],
        arguments.Count == 2 ? arguments[0] : null, data.ValueOperand
      ),
      MixinDirectiveSyntaxForm.Inline => new InlineDirectiveAst(data.SourceRange, arguments[0]),
      MixinDirectiveSyntaxForm.End => new EndAst(data.SourceRange),
      MixinDirectiveSyntaxForm.Match => new MatchDirectiveAst(
        data.SourceRange, arguments.FirstOrDefault(), data.BooleanOperand
      ),
      MixinDirectiveSyntaxForm.Assert => new AssertDirectiveAst(data.SourceRange, data.BooleanOperand),
      MixinDirectiveSyntaxForm.Code => CreateCodeSyntax(data),
      MixinDirectiveSyntaxForm.Mixin => new TargetedCodeDirectiveAst(
        data.SourceRange, data.ParsedArguments[0],
        data.ParsedArguments.Count > 1 ? data.ParsedArguments[1] : null, data.ValueOperand
      ),
      MixinDirectiveSyntaxForm.Using => new UsingDirectiveSyntax(data.SourceRange, data.ValueOperand),
      MixinDirectiveSyntaxForm.Log => new LogDirectiveSyntax(data.SourceRange, data.ValueOperand),
      MixinDirectiveSyntaxForm.Local => new LocalDirectiveSyntax(data.SourceRange, arguments[0], data.ValueOperand),
      MixinDirectiveSyntaxForm.Variable => new VariableDirectiveAst(data.SourceRange, arguments[0], data.ValueOperand),
      MixinDirectiveSyntaxForm.TargetVariable => new TargetVariableDirectiveAst(data.SourceRange, arguments[0], data.ValueOperand),
      MixinDirectiveSyntaxForm.Carry => new CarryDirectiveAst(data.SourceRange, arguments[0], data.ValueOperand),
      MixinDirectiveSyntaxForm.Return => new ReturnDirectiveAst(data.SourceRange, data.ValueOperand),
      MixinDirectiveSyntaxForm.Goto => new GotoDirectiveAst(data.SourceRange, arguments[0]),
      MixinDirectiveSyntaxForm.Skip => new SkipDirectiveAst(data.SourceRange),
      MixinDirectiveSyntaxForm.Fail => new FailDirectiveAst(data.SourceRange, data.ValueOperand),
      MixinDirectiveSyntaxForm.Annotation => new AnnotationAst(data.SourceRange, arguments[0]),
      MixinDirectiveSyntaxForm.Prelude => new PreludeAst(data.SourceRange),
      MixinDirectiveSyntaxForm.DefineTarget => new DefineTargetAst(
        data.SourceRange, arguments[0], arguments[1]
      ),
      _ => base.CreateSyntax(data)
    };
  }

  private static DirectiveAst CreateCodeSyntax(MixinDirectiveSyntaxData data) {
    var argument = data.Arguments.FirstOrDefault();
    if (Enum.TryParse<MixinExpressionOutputTarget>(argument ?? "TARGET", true, out var target))
      return new CodeDirectiveAst(data.SourceRange, target, null, data.ValueOperand);
    return new CodeDirectiveAst(
      data.SourceRange, MixinExpressionOutputTarget.Injection, argument, data.ValueOperand
    );
  }
}

public static class DirectiveLibrary {
  private static readonly IReadOnlyDictionary<string, DirectiveDefinition> Definitions =
    CreateDefinitions();

  private static IReadOnlyDictionary<string, DirectiveDefinition> CreateDefinitions() {
    using var profile = MixinProfiler.Measure("static.directive_library");
    var definitions = new Dictionary<string, DirectiveDefinition>(StringComparer.Ordinal) {
      ["RESOLVE_MIXIN"] = FunctionDirective(
        new ResolveMixinDirectiveFunction(), [Arg(MixinLanguageValueKind.Text)], 0,
        "Resolves and applies a named mixin."
      ),
      ["PUSH"] = FunctionDirective(
        new PushDirectiveFunction(),
        [Arg(MixinLanguageValueKind.Identifier, MixinSymbolKind.Local, MixinSymbolUsage.Reference)],
        documentation: "Appends the operand to a local table."
      ),
      ["PUT"] = FunctionDirective(
        new PutDirectiveFunction(), [
          Arg(MixinLanguageValueKind.Identifier, MixinSymbolKind.Local, MixinSymbolUsage.Reference),
          Arg(MixinLanguageValueKind.Any)
        ], documentation: "Stores the operand under a key in a local table."
      ),
      ["SCOPE"] = Define("SCOPE", 0, 1, DirectiveOperandKind.None, [Arg(MixinLanguageValueKind.Label, MixinSymbolKind.Label, MixinSymbolUsage.Declaration)],
        MixinDirectiveSyntaxForm.Scope),
      ["LABEL"] = Define("LABEL", 1, 1, DirectiveOperandKind.None, [Arg(MixinLanguageValueKind.Label, MixinSymbolKind.Label, MixinSymbolUsage.Declaration)],
        MixinDirectiveSyntaxForm.Label),
      ["FUNC"] = Define("FUNC", 1, 1, DirectiveOperandKind.None, [Arg(MixinLanguageValueKind.Function, MixinSymbolKind.Function, MixinSymbolUsage.Declaration)],
        MixinDirectiveSyntaxForm.Function),
      ["CALL"] = Define("CALL", 1, 2, DirectiveOperandKind.Value,
        [Arg(MixinLanguageValueKind.Identifier, MixinSymbolKind.Local, MixinSymbolUsage.Declaration), Arg(MixinLanguageValueKind.Function, MixinSymbolKind.Function, MixinSymbolUsage.Reference)],
        MixinDirectiveSyntaxForm.Call,
        new Dictionary<int, IReadOnlyList<MixinDirectiveArgumentMetadata>> { [1] = [Arg(MixinLanguageValueKind.Function, MixinSymbolKind.Function, MixinSymbolUsage.Reference)] }),
      ["INLINE"] = Define("INLINE", 1, 1, DirectiveOperandKind.None, [Arg(MixinLanguageValueKind.Function, MixinSymbolKind.Function, MixinSymbolUsage.Reference)],
        MixinDirectiveSyntaxForm.Inline),
      ["END"] = Define("END", 0, 0, DirectiveOperandKind.None, [], MixinDirectiveSyntaxForm.End),
      ["MATCH"] = Define("MATCH", 0, 1, DirectiveOperandKind.Boolean, [Arg(MixinLanguageValueKind.Label, MixinSymbolKind.Label, MixinSymbolUsage.Reference)],
        MixinDirectiveSyntaxForm.Match),
      ["ASSERT"] = Define("ASSERT", 0, 0, DirectiveOperandKind.Boolean, [], MixinDirectiveSyntaxForm.Assert),
      ["CODE"] = Define("CODE", 0, 1, DirectiveOperandKind.Value, [Arg(MixinLanguageValueKind.OutputTarget)], MixinDirectiveSyntaxForm.Code),
      ["MIXIN"] = Define("MIXIN", 1, 2, DirectiveOperandKind.Value, [Arg(MixinLanguageValueKind.Any), Arg(MixinLanguageValueKind.Any)],
        MixinDirectiveSyntaxForm.Mixin),
      ["USING"] = Define("USING", 0, 0, DirectiveOperandKind.Value, [], MixinDirectiveSyntaxForm.Using),
      ["LOG"] = Define("LOG", 0, 0, DirectiveOperandKind.Value, [], MixinDirectiveSyntaxForm.Log),
      ["LOCAL"] = NamedValue("LOCAL", MixinSymbolKind.Local, MixinDirectiveSyntaxForm.Local),
      ["VAR"] = NamedValue("VAR", MixinSymbolKind.Variable, MixinDirectiveSyntaxForm.Variable),
      ["TAR"] = NamedValue("TAR", MixinSymbolKind.TargetVariable, MixinDirectiveSyntaxForm.TargetVariable),
      ["CARRY"] = NamedValue("CARRY", MixinSymbolKind.Carry, MixinDirectiveSyntaxForm.Carry),
      ["RETURN"] = Define("RETURN", 0, 0, DirectiveOperandKind.Value, [], MixinDirectiveSyntaxForm.Return),
      ["GOTO"] = Define("GOTO", 1, 1, DirectiveOperandKind.None, [Arg(MixinLanguageValueKind.Label, MixinSymbolKind.Label, MixinSymbolUsage.Reference)], MixinDirectiveSyntaxForm.Goto),
      ["SKIP"] = Define("SKIP", 0, 0, DirectiveOperandKind.None, [], MixinDirectiveSyntaxForm.Skip),
      ["FAIL"] = Define("FAIL", 0, 0, DirectiveOperandKind.Value, [], MixinDirectiveSyntaxForm.Fail),
      ["ANNOTATION"] = Define("ANNOTATION", 1, 1, DirectiveOperandKind.None, [Arg(MixinLanguageValueKind.CSharpType, MixinSymbolKind.Annotation, MixinSymbolUsage.Declaration | MixinSymbolUsage.Reference)], MixinDirectiveSyntaxForm.Annotation),
      ["DERIVATION"] = Define("DERIVATION", 1, 1, DirectiveOperandKind.None, [Arg(MixinLanguageValueKind.CSharpType, MixinSymbolKind.Derivation, MixinSymbolUsage.Declaration | MixinSymbolUsage.Reference)]),
      ["PRELUDE"] = Define("PRELUDE", 0, 0, DirectiveOperandKind.None, [], MixinDirectiveSyntaxForm.Prelude),
      ["DEFINE_TARGET"] = Define("DEFINE_TARGET", 2, 2, DirectiveOperandKind.None,
        [Arg(MixinLanguageValueKind.Identifier, MixinSymbolKind.Target, MixinSymbolUsage.Declaration), Arg(MixinLanguageValueKind.Any)],
        MixinDirectiveSyntaxForm.DefineTarget),
      ["CONFIG"] = Define("CONFIG", 1, 1, DirectiveOperandKind.Value, [Arg(MixinLanguageValueKind.Identifier)])
    };
    ValidateMetadata(definitions.Values);
    return definitions;
  }

  public static bool TryGet(string name, out DirectiveDefinition definition) {
    return Definitions.TryGetValue(name ?? "", out definition);
  }

  public static IEnumerable<DirectiveDefinition> Enumerate() => Definitions.Values;
  public static IEnumerable<DirectiveDefinition> EnumerateLanguageDefinitions() => Definitions.Values;

  private static DirectiveDefinition Define(string name, int minimum, int maximum,
    DirectiveOperandKind operand, IReadOnlyList<MixinDirectiveArgumentMetadata> arguments,
    MixinDirectiveSyntaxForm syntaxForm = MixinDirectiveSyntaxForm.Invocation,
    IReadOnlyDictionary<int, IReadOnlyList<MixinDirectiveArgumentMetadata>> arityArguments = null) {
    return new SyntaxDirectiveDefinition(name, operand, maximum, minimum, syntaxForm).WithLanguageSignature(
      arguments, documentation: "@" + name + " directive.", arityArguments: arityArguments);
  }

  private static DirectiveDefinition NamedValue(string name, MixinSymbolKind symbol,
    MixinDirectiveSyntaxForm syntaxForm) => Define(
    name, 1, 1, DirectiveOperandKind.Value,
    [Arg(MixinLanguageValueKind.Identifier, symbol, MixinSymbolUsage.Declaration)], syntaxForm
  );

  private static DirectiveDefinition FunctionDirective(
    FunctionDefinition function, IReadOnlyList<MixinDirectiveArgumentMetadata> arguments,
    int hoistedLocalArgumentIndex = -1, string documentation = null
  ) {
    function.WithLanguageSignature(
      MixinLanguageValueKind.Any, MixinLanguageValueKind.Any,
      arguments.Select(argument => argument.ValueKind).ToArray(), documentation
    );
    return new DirectiveDefinition(
      function.Name, DirectiveOperandKind.Value,
      function.MaximumArguments, function.MinimumArguments
    ).WithLanguageSignature(arguments, documentation: documentation)
      .WithFunction(function, hoistedLocalArgumentIndex);
  }

  private static MixinDirectiveArgumentMetadata Arg(MixinLanguageValueKind kind,
    MixinSymbolKind symbol = MixinSymbolKind.None, MixinSymbolUsage usage = MixinSymbolUsage.None) =>
    new(kind, symbol, usage);

  private static void ValidateMetadata(IEnumerable<DirectiveDefinition> definitions) {
    foreach (var definition in definitions) {
      if (definition.Metadata is null)
        throw new InvalidOperationException("Directive '" + definition.Name + "' must provide language metadata.");
      if (definition.ArgumentTypes.Count < definition.MinimumArguments ||
          definition.MaximumArguments != int.MaxValue && definition.ArgumentTypes.Count != definition.MaximumArguments)
        throw new InvalidOperationException("Directive '" + definition.Name + "' has an incomplete argument descriptor.");
    }
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

internal sealed class ResolveMixinDirectiveFunction() : EvaluatedDirectiveFunction("RESOLVE_MIXIN", 1) {
  protected override IMixinValue Apply(
    ExecutionContext context, IReadOnlyList<IMixinValue> arguments,
    IMixinValue operand
  ) {
    return context.ResolveMixin(arguments[0].Render(context), operand);
  }
}

internal abstract class TableDirectiveFunction(string name, int arguments)
  : FunctionDefinition(name, arguments, arguments) {
  public override IMixinValue Invoke(
    ExecutionContext context, IMixinValue instance,
    IReadOnlyList<IMixinValue> arguments, bool negated
  ) {
    var values = arguments.Select(context.Evaluate).ToArray();
    var error = values.OfType<ErrorMixinValue>().FirstOrDefault();
    if (error is not null) return error;
    instance = context.Evaluate(instance);
    if (instance is ErrorMixinValue) return instance;
    var local = values[0].Render(context);
    var table = context.Locals.TryGetValue(local, out var existing) && existing is MixinTableValue typed
      ? typed
      : MixinTableValue.Empty;
    table = Mutate(context, table, values, instance);
    context.Locals.StoreIsolated(local, table);
    return table;
  }

  protected abstract MixinTableValue Mutate(
    ExecutionContext context, MixinTableValue table,
    IReadOnlyList<IMixinValue> arguments, IMixinValue operand
  );
}

internal sealed class PushDirectiveFunction() : TableDirectiveFunction("PUSH", 1) {
  protected override MixinTableValue Mutate(
    ExecutionContext context, MixinTableValue table,
    IReadOnlyList<IMixinValue> arguments, IMixinValue operand
  ) {
    return table.Push(context, operand);
  }
}

internal sealed class PutDirectiveFunction() : TableDirectiveFunction("PUT", 2) {
  protected override MixinTableValue Mutate(
    ExecutionContext context, MixinTableValue table,
    IReadOnlyList<IMixinValue> arguments, IMixinValue operand
  ) {
    return table.Put(context, arguments[1].Render(context), operand);
  }
}
