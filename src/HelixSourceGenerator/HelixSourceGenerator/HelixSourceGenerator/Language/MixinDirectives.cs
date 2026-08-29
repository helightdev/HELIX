using System.Collections.Generic;

namespace HelixSourceGenerator.Language;

public enum DirectiveOperandKind { None, Value, Boolean }

/// <summary>Definition and compiler contract for a source-language directive.</summary>
public abstract class DirectiveDefinition {
  protected DirectiveDefinition(string name, DirectiveOperandKind operandKind) {
    Name = name;
    OperandKind = operandKind;
  }

  internal string Name { get; }
  internal DirectiveOperandKind OperandKind { get; }

  protected virtual int MaximumArguments => 1;

  internal virtual bool Validate(
    IReadOnlyList<string> arguments, string operand, out string error
  ) {
    error = null;
    if (arguments.Count <= MaximumArguments) return true;
    error = Name + " accepts at most " + MaximumArguments +
      (MaximumArguments == 1 ? " argument" : " arguments");
    return false;
  }
}

public static class DirectiveLibrary {
  public static bool TryGet(string name, out DirectiveDefinition definition) {
    if (DirectiveFunctionLibrary.TryGet(name, out var function)) {
      definition = function;
      return true;
    }
    definition = null;
    return false;
  }
}