using System;
using System.Collections.Generic;
using System.Linq;
using Mixins.Compiler;

namespace Mixins.Runtime;

public static class MixinVirtualMachine {
  internal const string CarryLocalPrefix = "\0carry:";

  public static MixinExpressionResult Execute(string source, string mixinName, ExecutionContext context,
    IDictionary<string, object> variables = null) => Execute(AntlrSyntax.Parse(source), mixinName, context, variables);

  public static MixinExpressionResult Execute(CompilationUnitAst unit, string mixinName, ExecutionContext context,
    IDictionary<string, object> variables = null) {
    if (unit is null) return Failure("the compilation unit is null", 0);
    if (unit.Diagnostics.Count != 0) return Failure(unit.Diagnostics[0].Message, unit.Diagnostics[0].Line);
    var declarations = unit.Declarations.OfType<MixinDeclarationAst>().Where(mixin => mixin.Name == mixinName).ToArray();
    if (declarations.Length != 1) return Failure("expected exactly one mixin named '" + mixinName + "'", 0);
    var declaration = declarations[0];
    try {
      return Execute(MixinCompiler.Prepare(declaration, MixinCompiler.PrepareGlobals(new[] {unit}), null), context, variables);
    } catch (ArgumentException exception) { return Failure(exception.Message, declaration.Line); }
  }

  internal static MixinExpressionResult Execute(MixinExpressionExecutionProgram program, ExecutionContext context,
    IDictionary<string, object> variables, bool importCarries = true) {
    context.Strings = program.StringPool;
    var imported = variables == null ? null : new Dictionary<string, object>(
      variables.Where(item => importCarries || !item.Key.StartsWith(CarryLocalPrefix, StringComparison.Ordinal))
        .ToDictionary(item => item.Key, item => item.Value), StringComparer.Ordinal);
    var result = new LanguageExecution(context, program)
      .Execute(program.Expressions, imported);
    if (variables != null)
      foreach (var item in result.Variables) variables[item.Key] = item.Value;
    return result;
  }

  private static MixinExpressionResult Failure(string error, int line) => new(false, error, line, []);
}
