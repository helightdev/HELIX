using System;
using System.Collections.Generic;
using System.Linq;
using RuntimeValue = HelixSourceGenerator.Language.IMixinValue;

namespace HelixSourceGenerator.Language.Compiler;

public static partial class MixinExpressionCompiler {
  private static RuntimeValue LowerReference(MixinExpressionReference syntax, MixinStringPool strings) {
    RuntimeValue value = new RootMixinValue(syntax.Root, strings.Get(syntax.Member));
    foreach (var property in syntax.Properties) {
      if (property.Definition is null)
        return new ErrorMixinValue(strings.Get("unknown function '" + property.Name + "'"));
      var arguments = property.ParsedArguments.Select(argument => LowerArgument(argument, strings)).ToArray();
      value = new InvokeMixinValue(property.Definition, value, arguments, property.Negated);
    }
    return value;
  }

  private static RuntimeValue LowerArgument(MixinPropertyArgumentSyntax syntax, MixinStringPool strings) {
    if (syntax is null) return new LiteralMixinValue(strings.Get(null));
    if (syntax.BooleanExpression is { } boolean)
      return new AllMixinValue(boolean.Select(item => LowerReference(item, strings)).ToArray());
    if (syntax.ValueExpression is { } expression) return LowerValue(expression, strings);
    return LowerLiteral(syntax.Literal, strings);
  }

  private static RuntimeValue LowerArgument(DirectiveArgumentSyntax syntax, MixinStringPool strings) {
    if (syntax is null) return new LiteralMixinValue(strings.Get(null));
    return syntax.Expression is null
      ? LowerLiteral(syntax.Literal, strings)
      : LowerValue(syntax.Expression, strings);
  }

  private static RuntimeValue LowerValue(IReadOnlyList<Compiler.IMixinValue> syntax, MixinStringPool strings) {
    if (syntax is { Count: 1 } && syntax[0].Reference is { } reference) return LowerReference(reference, strings);
    if (syntax is { Count: 1 } && syntax[0].Reference is null) return LowerLiteral(syntax[0].Literal, strings);
    return new InterpolationMixinValue(syntax.Select(part => part.Reference is null
      ? LowerLiteral(part.Literal, strings)
      : LowerReference(part.Reference, strings)).ToArray());
  }

  private static RuntimeValue LowerLiteral(string value, MixinStringPool strings) => value switch {
    "true" => BooleanMixinValue.True,
    "false" => BooleanMixinValue.False,
    _ => new LiteralMixinValue(strings.Get(value))
  };

  private static AllMixinValue LowerBoolean(IReadOnlyList<MixinExpressionReference> syntax,
    MixinStringPool strings) => new(syntax.Select(item => LowerReference(item, strings)).ToArray());

  private static MixinInstruction LowerInstruction(IReadOnlyList<DirectiveInstruction> syntaxInstructions,
    DirectiveInstruction syntax, int index,
    MixinStringPool strings, IReadOnlyDictionary<string, int> labels,
    IReadOnlyDictionary<int, int> scopes, IReadOnlyDictionary<string, FunctionDefinition> functions,
    IReadOnlyDictionary<int, int> functionStarts, ISet<int> functionEnds,
    IReadOnlyDictionary<MixinString, int> importedFunctions = null, int instructionOffset = 0) {
    var location = new MixinSourceLocation(0, syntax.Line);
    MixinString Name(string value) => strings.Get(value);
    int Label(string value) => value is not null && scopes.TryGetValue(index, out var scope) &&
      labels.TryGetValue(ScopeLabelKey(scope, value), out var target) ? target + 1 : -1;
    int Next() {
      var target = FindNextScopeOrEnd(syntaxInstructions, index + 1,
        scopes.TryGetValue(index, out var scope) ? scope : 0, scopes, functionStarts, functionEnds);
      return target >= 0 && target < syntaxInstructions.Count &&
        syntaxInstructions[target] is ScopeDirectiveSyntax or LabelDirectiveSyntax ? target + 1 : target;
    }
    MixinInstruction lowered = syntax switch {
      EmptyDirectiveSyntax => new(MixinOpcode.Empty, location),
      ScopeDirectiveSyntax => new(MixinOpcode.Scope, location),
      LabelDirectiveSyntax => new(MixinOpcode.Scope, location),
      FunctionDirectiveSyntax function => new(MixinOpcode.Function, location, Name: Name(function.Name),
        Destination: functionStarts.TryGetValue(index, out var end) ? end + 1 : -1),
      EndDirectiveSyntax => new(MixinOpcode.End, location,
        SecondaryDestination: functionEnds.Contains(index) ? 1 : 0),
      MatchDirectiveSyntax match => new(MixinOpcode.Match, location, LowerBoolean(match.Expression, strings),
        Name: Name(match.FailureLabel), Destination: Label(match.FailureLabel),
        SecondaryDestination: Next()),
      AssertDirectiveSyntax assertion => new(MixinOpcode.Assert, location, LowerBoolean(assertion.Expression, strings),
        Message: Name(DescribeAssertion(assertion.Expression, strings))),
      CodeDirectiveSyntax code => new(MixinOpcode.Emit, location, LowerValue(code.Expression, strings),
        Name: Name(code.InjectionTarget), OutputTarget: code.Target),
      MixinDirectiveSyntax mixin => new(MixinOpcode.Mixin, location, LowerValue(mixin.Expression, strings),
        Arguments: new RuntimeValue[] { LowerArgument(mixin.Target, strings), LowerArgument(mixin.Priority, strings) }),
      UsingDirectiveSyntax use => new(MixinOpcode.Using, location, LowerValue(use.Expression, strings)),
      LogDirectiveSyntax log => new(MixinOpcode.Log, location, LowerValue(log.Expression, strings)),
      LocalDirectiveSyntax local => new(MixinOpcode.StoreLocal, location, LowerValue(local.Expression, strings), Name: Name(local.Name)),
      VariableDirectiveSyntax variable => new(MixinOpcode.StoreVariable, location, LowerValue(variable.Expression, strings), Name: Name(variable.Name)),
      CarryDirectiveSyntax carry => new(MixinOpcode.Carry, location, LowerValue(carry.Expression, strings), Name: Name(carry.Label)),
      ReturnDirectiveSyntax returned => new(MixinOpcode.Return, location, LowerValue(returned.Expression, strings)),
      CallDirectiveSyntax call => new(MixinOpcode.Call, location, LowerValue(call.Expression, strings),
        Name: Name(call.ReturnLocal), Destination: functions.TryGetValue(call.Function ?? "", out var target)
          ? target.Start : importedFunctions is not null && importedFunctions.TryGetValue(Name(call.Function), out var imported)
            ? imported : -1),
      InlineDirectiveSyntax inline => new(MixinOpcode.Call, location, NullMixinValue.Instance,
        Destination: functions.TryGetValue(inline.Name ?? "", out var inlineTarget)
          ? inlineTarget.Start : importedFunctions is not null &&
            importedFunctions.TryGetValue(Name(inline.Name), out var importedInline)
              ? importedInline : -1),
      GotoDirectiveSyntax go => new(MixinOpcode.Goto, location, Name: Name(go.Label), Destination: Label(go.Label)),
      SkipDirectiveSyntax => new(MixinOpcode.Skip, location,
        Destination: Next()),
      FailDirectiveSyntax fail => new(MixinOpcode.Fail, location, LowerValue(fail.Expression, strings)),
      DirectiveInvocationSyntax directive => new(MixinOpcode.Directive, location,
        LowerValue(directive.Expression, strings), directive.ParsedArguments.Select(item =>
          item.Expression is null ? LowerLiteral(item.Literal, strings) : LowerValue(item.Expression, strings)
        ).ToArray(), Directive: directive.Definition),
      _ => new(MixinOpcode.Fail, location, new ErrorMixinValue(strings.Get("invalid compiled instruction")))
    };
    return lowered with {
      Operand = ResolveProgramFunctions(lowered.Operand),
      Arguments = lowered.Arguments?.Select(ResolveProgramFunctions).ToArray()
    };

    RuntimeValue ResolveProgramFunctions(RuntimeValue value) {
      if (value is not InvokeMixinValue invocation) return value switch {
        InterpolationMixinValue interpolation => new InterpolationMixinValue(
          interpolation.Parts.Select(ResolveProgramFunctions).ToArray()),
        AllMixinValue all => new AllMixinValue(all.Values.Select(ResolveProgramFunctions).ToArray()),
        _ => value
      };
      var instance = ResolveProgramFunctions(invocation.Instance);
      var arguments = invocation.Arguments.Select(ResolveProgramFunctions).ToArray();
      if (invocation.Function.Name is "mapValues" or "map" or "filter" &&
        arguments.Length == 1 && arguments[0] is LiteralMixinValue literal) {
        var functionName = literal.Value.Resolve(strings);
        var entry = functions.TryGetValue(functionName, out var local)
          ? local.Start + instructionOffset
          : importedFunctions is not null && importedFunctions.TryGetValue(strings.Get(functionName), out var imported)
            ? imported : -1;
        arguments[0] = entry < 0
          ? new ErrorMixinValue(strings.Get("unknown function '" + functionName + "'"))
          : new ProgramFunctionMixinValue(entry);
      }
      return new InvokeMixinValue(invocation.Function, instance, arguments, invocation.Negated);
    }
  }

  private static string DescribeAssertion(IReadOnlyList<MixinExpressionReference> expression,
    MixinStringPool strings) => string.Join(" and ",
    (expression ?? []).Select(MixinSyntaxRenderer.DescribeFailedCondition));
}
