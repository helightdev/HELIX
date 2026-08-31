using System.Collections.Generic;
using System.Linq;
using RuntimeValue = MixinLanguage.IMixinValue;

namespace MixinLanguage.Compiler;

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

  private static RuntimeValue LowerArgument(MixinPropertyArgumentAst ast, MixinStringPool strings) {
    if (ast is null) return new LiteralMixinValue(strings.Get(null));
    if (ast.BooleanExpression is { } boolean)
      return new AllMixinValue([.. boolean.Select(item => LowerReference(item, strings))]);
    if (ast.ValueExpression is { } expression) return LowerValue(expression, strings);
    return LowerLiteral(ast.Literal, strings);
  }

  private static RuntimeValue LowerArgument(DirectiveArgumentAst ast, MixinStringPool strings) {
    if (ast is null) return new LiteralMixinValue(strings.Get(null));
    return ast.Expression is null
      ? LowerLiteral(ast.Literal, strings)
      : LowerValue(ast.Expression, strings);
  }

  private static RuntimeValue LowerValue(IReadOnlyList<ValueAst> syntax, MixinStringPool strings) {
    if (syntax is { Count: 1 } && syntax[0].Reference is { } reference) return LowerReference(reference, strings);
    if (syntax is { Count: 1 } && syntax[0].Reference is null) return LowerLiteral(syntax[0].Literal, strings);
    return new InterpolationMixinValue(
      [
        .. syntax.Select(part => part.Reference is null
          ? LowerLiteral(part.Literal, strings)
          : LowerReference(part.Reference, strings)
        )
      ]
    );
  }

  private static RuntimeValue LowerLiteral(string value, MixinStringPool strings) {
    return value switch {
      "true" => BooleanMixinValue.True,
      "false" => BooleanMixinValue.False,
      _ => new LiteralMixinValue(strings.Get(value))
    };
  }

  private static AllMixinValue LowerBoolean(
    IReadOnlyList<MixinExpressionReference> syntax,
    MixinStringPool strings
  ) {
    return new AllMixinValue([.. syntax.Select(item => LowerReference(item, strings))]);
  }

  private static MixinInstruction LowerInstruction(
    IReadOnlyList<DirectiveAst> syntaxInstructions,
    DirectiveAst syntax, int index,
    MixinStringPool strings, IReadOnlyDictionary<string, int> labels,
    IReadOnlyDictionary<int, int> scopes, IReadOnlyDictionary<string, FunctionDefinition> functions,
    IReadOnlyDictionary<int, int> functionStarts, ISet<int> functionEnds,
    IReadOnlyDictionary<MixinString, int> importedFunctions = null, int instructionOffset = 0
  ) {
    var location = new MixinSourceLocation(0, syntax.Line);

    MixinString Name(string value) {
      return strings.Get(value);
    }

    int Label(string value) {
      return value is not null && scopes.TryGetValue(index, out var scope) &&
        labels.TryGetValue(ScopeLabelKey(scope, value), out var target)
          ? target + 1
          : -1;
    }

    int Next() {
      var target = FindNextScopeOrEnd(
        syntaxInstructions, index + 1,
        scopes.TryGetValue(index, out var scope) ? scope : 0, scopes, functionStarts, functionEnds
      );
      return target >= 0 && target < syntaxInstructions.Count &&
        syntaxInstructions[target] is ScopeAst or LabelAst
          ? target + 1
          : target;
    }

    var lowered = syntax switch {
      EmptyDirectiveAst => new MixinInstruction(MixinOpcode.Empty, location),
      ScopeAst => new MixinInstruction(MixinOpcode.Scope, location),
      LabelAst => new MixinInstruction(MixinOpcode.Scope, location),
      FunctionAst function => new MixinInstruction(
        MixinOpcode.Function, location, Name: Name(function.Name),
        Destination: functionStarts.TryGetValue(index, out var end) ? end + 1 : -1
      ),
      EndAst => new MixinInstruction(
        MixinOpcode.End, location,
        SecondaryDestination: functionEnds.Contains(index) ? 1 : 0
      ),
      MatchDirectiveAst match => new MixinInstruction(
        MixinOpcode.Match, location, LowerBoolean(match.Expression, strings),
        Name: Name(match.FailureLabel), Destination: Label(match.FailureLabel),
        SecondaryDestination: Next()
      ),
      AssertDirectiveAst assertion => new MixinInstruction(
        MixinOpcode.Assert, location, LowerBoolean(assertion.Expression, strings),
        Message: Name(DescribeAssertion(assertion.Expression, strings))
      ),
      CodeDirectiveAst code => new MixinInstruction(
        MixinOpcode.Emit, location, LowerValue(code.Expression, strings),
        Name: Name(code.InjectionTarget), OutputTarget: code.Target
      ),
      TargetedCodeDirectiveAst mixin => new MixinInstruction(
        MixinOpcode.Mixin, location, LowerValue(mixin.Expression, strings),
        [LowerArgument(mixin.Target, strings), LowerArgument(mixin.Priority, strings)]
      ),
      UsingDirectiveSyntax use => new MixinInstruction(
        MixinOpcode.Using, location, LowerValue(use.Expression, strings)
      ),
      LogDirectiveSyntax log => new MixinInstruction(MixinOpcode.Log, location, LowerValue(log.Expression, strings)),
      LocalDirectiveSyntax local => new MixinInstruction(
        MixinOpcode.StoreLocal, location, LowerValue(local.Expression, strings), Name: Name(local.Name)
      ),
      VariableDirectiveAst variable => new MixinInstruction(
        MixinOpcode.StoreVariable, location, LowerValue(variable.Expression, strings), Name: Name(variable.Name)
      ),
      TargetVariableDirectiveAst variable => new MixinInstruction(
        MixinOpcode.StoreTargetVariable, location, LowerValue(variable.Expression, strings), Name: Name(variable.Name)
      ),
      CarryDirectiveAst carry => new MixinInstruction(
        MixinOpcode.Carry, location, LowerValue(carry.Expression, strings),
        Name: Name(carry.Label)
      ),
      ReturnDirectiveAst returned => new MixinInstruction(
        MixinOpcode.Return, location, LowerValue(returned.Expression, strings),
        SecondaryDestination: string.IsNullOrEmpty(MixinSyntaxRenderer.RenderValue(returned.Expression)) ? 0 : 1
      ),
      CallDirectiveAst call => new MixinInstruction(
        MixinOpcode.Call, location, LowerValue(call.Expression, strings),
        Name: Name(call.ReturnLocal), Destination: functions.TryGetValue(call.Function ?? "", out var target)
          ? target.Start
          : importedFunctions is not null && importedFunctions.TryGetValue(Name(call.Function), out var imported)
            ? imported
            : -1
      ),
      InlineDirectiveAst inline => new MixinInstruction(
        MixinOpcode.Call, location, NullMixinValue.Instance,
        Destination: functions.TryGetValue(inline.Name ?? "", out var inlineTarget)
          ? inlineTarget.Start
          : importedFunctions is not null &&
          importedFunctions.TryGetValue(Name(inline.Name), out var importedInline)
            ? importedInline
            : -1
      ),
      GotoDirectiveAst go => new MixinInstruction(
        MixinOpcode.Goto, location, Name: Name(go.Label), Destination: Label(go.Label)
      ),
      SkipDirectiveAst => new MixinInstruction(
        MixinOpcode.Skip, location,
        Destination: Next()
      ),
      FailDirectiveAst fail => new MixinInstruction(
        MixinOpcode.Fail, location, LowerValue(fail.Expression, strings)
      ),
      DirectiveInvocationAst directive => new MixinInstruction(
        MixinOpcode.Directive, location,
        LowerValue(directive.Expression, strings), [
          .. directive.ParsedArguments.Select(item =>
            item.Expression is null ? LowerLiteral(item.Literal, strings) : LowerValue(item.Expression, strings)
          )
        ], Directive: directive.Definition
      ),
      _ => new MixinInstruction(
        MixinOpcode.Fail, location, new ErrorMixinValue(strings.Get("invalid compiled instruction"))
      )
    };
    return lowered with {
      Operand = ResolveProgramFunctions(lowered.Operand),
      Arguments = lowered.Arguments?.Select(ResolveProgramFunctions).ToArray()
    };

    RuntimeValue ResolveProgramFunctions(RuntimeValue value) {
      if (value is not InvokeMixinValue invocation) {
        return value switch {
          InterpolationMixinValue interpolation => new InterpolationMixinValue(
            [.. interpolation.Parts.Select(ResolveProgramFunctions)]
          ),
          AllMixinValue all => new AllMixinValue([.. all.Values.Select(ResolveProgramFunctions)]),
          _ => value
        };
      }
      var instance = ResolveProgramFunctions(invocation.Instance);
      var arguments = invocation.Arguments.Select(ResolveProgramFunctions).ToArray();
      var callbackIndex = invocation.Function.Name == "reduce" ? 1 : 0;
      if (invocation.Function.Name is "mapValues" or "map" or "filter" or "reduce" &&
        arguments.Length > callbackIndex && arguments[callbackIndex] is LiteralMixinValue literal) {
        var functionName = literal.Value.Resolve(strings);
        var entry = functions.TryGetValue(functionName, out var local)
          ? local.Start + instructionOffset
          : importedFunctions is not null && importedFunctions.TryGetValue(strings.Get(functionName), out var imported)
            ? imported
            : -1;
        arguments[callbackIndex] = entry < 0
          ? new ErrorMixinValue(strings.Get("unknown function '" + functionName + "'"))
          : new ProgramFunctionMixinValue(entry);
      }
      return new InvokeMixinValue(invocation.Function, instance, arguments, invocation.Negated);
    }
  }

  private static string DescribeAssertion(
    IReadOnlyList<MixinExpressionReference> expression,
    MixinStringPool strings
  ) {
    return string.Join(
      " and ",
      (expression ?? []).Select(MixinSyntaxRenderer.DescribeFailedCondition)
    );
  }
}