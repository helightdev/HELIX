using System.Collections.Generic;
using System.Linq;
using System.Text;
using HelixSourceGenerator.Language.Compiler;
using HelixSourceGenerator.Language.Functions;

namespace HelixSourceGenerator.Language;

public static partial class MixinExpressionVirtualMachine {
  internal static bool TryResolveDirectiveArgument(
    DirectiveArgumentSyntax argument,
    IMixinExpressionContext context,
    MixinValueDictionary locals,
    MixinValueDictionary variables,
    out string value,
    out string error
  ) {
    if (!TryResolveArgumentValue(argument, context, locals, variables, out var resolved, out error)) {
      value = null;
      return false;
    }
    if (context is IMixinExpressionValueContext valueContext)
      return valueContext.TryRenderValue(resolved, null, out value, out error);
    value = resolved.Render();
    return true;
  }

  private static bool TryResolveArgumentValue(
    DirectiveArgumentSyntax argument,
    IMixinExpressionContext context,
    MixinValueDictionary locals,
    MixinValueDictionary variables,
    out IMixinValue value,
    out string error
  ) {
    if (argument?.Expression is not null)
      return TryEvaluateExpression(argument.Expression, context, locals, variables, out value, out error);
    value = MixinValue.From(argument?.Literal);
    error = null;
    return true;
  }

  internal static bool TryEvaluateAll(
    IReadOnlyList<MixinExpressionReference> expression,
    IMixinExpressionContext context,
    MixinValueDictionary locals,
    MixinValueDictionary variables,
    out bool result,
    out string error,
    out string failure
  ) {
    result = true;
    error = null;
    failure = null;
    var found = false;
    foreach (var reference in expression) {
      found = true;
      if (!TryEvaluate(reference, context, locals, variables, out var value, out error)) return false;
      result &= value;
      if (!value && failure is null) {
        failure = MixinSyntaxRenderer.DescribeFailedCondition(
          SelectFailedCondition(reference, context, locals, variables)
        );
      }
    }
    if (found) return true;
    error = "boolean expression is empty";
    return false;
  }

  private static MixinExpressionReference SelectFailedCondition(
    MixinExpressionReference reference,
    IMixinExpressionContext context,
    MixinValueDictionary locals,
    MixinValueDictionary variables
  ) {
    var valueProperties = reference.Properties.Where(property => !IsBooleanProperty(property)).ToArray();
    foreach (var predicate in reference.Properties.Where(IsBooleanProperty)) {
      var properties = valueProperties.Concat([predicate]).ToArray();
      var candidate = new MixinExpressionReference(reference.Root, reference.Member, properties);
      if (TryEvaluate(candidate, context, locals, variables, out var value, out _) && !value) return candidate;
    }
    return reference;
  }

  private static bool IsBooleanProperty(MixinExpressionProperty property) {
    return FunctionLibrary.IsPredicate(property.Name);
  }

  internal static bool TryInterpolate(
    IReadOnlyList<ValueExpressionPart> expression,
    IMixinExpressionContext context,
    MixinValueDictionary locals,
    MixinValueDictionary variables,
    out string result,
    out string error
  ) {
    error = null;
    var builder = new StringBuilder();
    foreach (var part in expression) {
      if (part.Reference is null) {
        builder.Append(part.Literal);
        continue;
      }
      if (!TryResolve(part.Reference, context, locals, variables, out var value, out error)) {
        result = null;
        return false;
      }
      if (context is IMixinExpressionValueContext valueContext) {
        MixinExpressionRoot? renderRoot = part.Reference.Properties.Any(IsTypeFunction)
          ? null
          : part.Reference.Root;
        if (!valueContext.TryRenderValue(value, renderRoot, out var rendered, out error)) {
          result = null;
          return false;
        }
        builder.Append(rendered);
      } else builder.Append(value.Render());
    }
    result = builder.ToString();
    return true;
  }

  internal static bool TryInterpolateSegments(
    IReadOnlyList<ValueExpressionPart> expression,
    IMixinExpressionContext context,
    MixinValueDictionary locals,
    MixinValueDictionary variables,
    MixinStringPool pool,
    out IReadOnlyList<MixinString> result,
    out string error
  ) {
    error = null;
    var segments = new List<MixinString>(expression.Count);
    foreach (var part in expression) {
      if (part.Reference is null) {
        segments.Add(pool.Get(part.Literal));
        continue;
      }
      if (!TryResolve(part.Reference, context, locals, variables, out var value, out error)) {
        result = null;
        return false;
      }
      string rendered;
      if (context is IMixinExpressionValueContext valueContext) {
        MixinExpressionRoot? renderRoot = part.Reference.Properties.Any(IsTypeFunction)
          ? null
          : part.Reference.Root;
        if (!valueContext.TryRenderValue(value, renderRoot, out rendered, out error)) {
          result = null;
          return false;
        }
      } else rendered = value.Render();
      segments.Add(MixinString.Dynamic(rendered));
    }
    result = segments;
    return true;
  }

  internal static bool TryEvaluateExpression(
    IReadOnlyList<ValueExpressionPart> expression,
    IMixinExpressionContext context,
    MixinValueDictionary locals,
    MixinValueDictionary variables,
    out IMixinValue result,
    out string error
  ) {
    if (expression is { Count: 1 } && expression[0].Reference is not null)
      return TryResolve(expression[0].Reference, context, locals, variables, out result, out error);
    if (TryInterpolate(expression, context, locals, variables, out var text, out error)) {
      result = MixinValue.From(text);
      return true;
    }
    result = null;
    return false;
  }

  internal static void RestoreCallParameter(
    MixinValueDictionary locals,
    CallFrame frame
  ) {
    if (frame.hadParameter) locals[ParameterLocalKey] = frame.parameter;
    else locals.Remove(ParameterLocalKey);
  }

  private static bool TryResolve(
    MixinExpressionReference reference,
    IMixinExpressionContext context,
    MixinValueDictionary locals,
    MixinValueDictionary variables,
    out IMixinValue value,
    out string error
  ) {
    if (!TryPrepareReference(reference, context, locals, variables, out reference, out error) ||
      !TryReduceLogicalProperties(reference, context, locals, variables, out reference, out error)) {
      value = null;
      return false;
    }
    var tableOperationIndex = reference.Properties.ToList().FindIndex(IsTableMutation);
    if (tableOperationIndex >= 0) {
      var prefix = new MixinExpressionReference(
        reference.Root, reference.Member,
        [.. reference.Properties.Take(tableOperationIndex)]
      );
      if (!TryResolveCore(prefix, context, locals, variables, out value, out error)) return false;
      var operations = new MixinExpressionReference(
        MixinExpressionRoot.Table, null, [.. reference.Properties.Skip(tableOperationIndex)]
      );
      if (!FunctionLibrary.TryApply(operations, context, null, ref value, out error)) return false;
      var predicate = operations.Properties.FirstOrDefault(IsBooleanProperty);
      if (predicate is null) return true;
      if (!TryEvaluateValuePredicate(value, predicate, context, operations, out var matched, out error)) return false;
      value = MixinValue.From(predicate.Negated ? !matched : matched);
      return true;
    }
    if (reference.Properties.Any(IsBooleanProperty)) {
      if (!TryEvaluateCore(reference, context, locals, variables, out var boolean, out error)) {
        value = null;
        return false;
      }
      value = MixinValue.From(boolean);
      return true;
    }
    return TryResolveCore(reference, context, locals, variables, out value, out error);
  }

  private static bool TryResolveCore(
    MixinExpressionReference reference,
    IMixinExpressionContext context,
    MixinValueDictionary locals,
    MixinValueDictionary variables,
    out IMixinValue value,
    out string error
  ) {
    if (reference.Root is MixinExpressionRoot.True or MixinExpressionRoot.False or
      MixinExpressionRoot.Null or MixinExpressionRoot.Table or MixinExpressionRoot.Parameter or
      MixinExpressionRoot.Carry) {
      value = reference.Root switch {
        MixinExpressionRoot.True => MixinValue.From(true),
        MixinExpressionRoot.False => MixinValue.From(false),
        MixinExpressionRoot.Null => NullMixinValue.Instance,
        MixinExpressionRoot.Table => new MixinExpressionTable(),
        MixinExpressionRoot.Parameter => locals.TryGetValue(ParameterLocalKey, out var parameter)
          ? MixinValue.From(parameter, context)
          : NullMixinValue.Instance,
        MixinExpressionRoot.Carry => variables.TryGetValue(CarryLocalPrefix + (reference.Member ?? ""), out var carried)
          ? MixinValue.From(carried, context)
          : NullMixinValue.Instance,
        _ => NullMixinValue.Instance
      };
      if (reference.Root == MixinExpressionRoot.Carry || string.IsNullOrEmpty(reference.Member))
        return FunctionLibrary.TryApply(reference, context, null, ref value, out error);
      if (value is MixinExpressionTable table) {
        value = table.TryGetValue(reference.Member, out var selected)
          ? MixinValue.From(selected, context)
          : NullMixinValue.Instance;
      } else value = value.Select(reference.Member);
      return FunctionLibrary.TryApply(reference, context, null, ref value, out error);
    }
    if (TryStored(reference, context, locals, variables, out value, out error)) return error is null;
    if (context is IMixinExpressionValueContext valueContext) {
      if (valueContext.TryResolveValue(reference, out var resolvedValue, out error)) {
        value = resolvedValue;
        return true;
      }
      value = null;
      return false;
    }
    if (context.TryResolve(reference, out var resolved, out error)) {
      value = MixinValue.From(resolved);
      return true;
    }
    value = null;
    return false;
  }

  private static bool TryEvaluate(
    MixinExpressionReference reference,
    IMixinExpressionContext context,
    MixinValueDictionary locals,
    MixinValueDictionary variables,
    out bool value,
    out string error
  ) {
    if (!TryPrepareReference(reference, context, locals, variables, out reference, out error) ||
      !TryReduceLogicalProperties(reference, context, locals, variables, out reference, out error)) {
      value = false;
      return false;
    }
    var tableOperationIndex = reference.Properties.ToList().FindIndex(IsTableMutation);
    if (tableOperationIndex < 0) return TryEvaluateCore(reference, context, locals, variables, out value, out error);
    var prefix = new MixinExpressionReference(
      reference.Root, reference.Member,
      [.. reference.Properties.Take(tableOperationIndex)]
    );
    if (!TryResolveCore(prefix, context, locals, variables, out var tableValue, out error)) {
      value = false;
      return false;
    }
    var operations = new MixinExpressionReference(
      MixinExpressionRoot.Table, null, [.. reference.Properties.Skip(tableOperationIndex)]
    );
    if (!FunctionLibrary.TryApply(operations, context, null, ref tableValue, out error)) {
      value = false;
      return false;
    }
    var predicate = operations.Properties.FirstOrDefault(IsBooleanProperty);
    if (predicate is null) {
      value = tableValue.IsTruthy;
      return true;
    }
    if (!TryEvaluateValuePredicate(tableValue, predicate, context, operations, out var matched, out error)) {
      value = false;
      return false;
    }
    value = predicate.Negated ? !matched : matched;
    return true;
  }

  private static bool TryEvaluateCore(
    MixinExpressionReference reference,
    IMixinExpressionContext context,
    MixinValueDictionary locals,
    MixinValueDictionary variables,
    out bool value,
    out string error
  ) {
    switch (reference.Root) {
      case MixinExpressionRoot.True or MixinExpressionRoot.False or MixinExpressionRoot.Null or
        MixinExpressionRoot.Table or MixinExpressionRoot.Parameter or MixinExpressionRoot.Carry: {
        if (!TryResolveCore(reference, context, locals, variables, out var atom, out error)) {
          value = false;
          return false;
        }
        var predicates = reference.Properties.Where(IsBooleanProperty).ToArray();
        if (predicates.Length == 0) {
          value = atom.IsTruthy;
          return true;
        }
        value = true;
        foreach (var predicate in predicates) {
          if (!TryEvaluateValuePredicate(atom, predicate, context, reference, out var item, out error)) return false;
          value &= predicate.Negated ? !item : item;
        }
        return true;
      }
      case MixinExpressionRoot.Local:
      case MixinExpressionRoot.Variable: {
        TryStored(reference, context, locals, variables, out var stored, out error);
        var predicates = reference.Properties.Where(IsBooleanProperty).ToArray();
        if (error is not null) {
          if (predicates.Length == 0) {
            value = false;
            return false;
          }
          error = null;
          value = true;
          foreach (var predicate in predicates) {
            if (!TryEvaluateValuePredicate(
              NullMixinValue.Instance, predicate, context, reference, out var item, out error
            )) return false;
            value &= predicate.Negated ? !item : item;
          }
          return true;
        }
        if (predicates.Length == 0) {
          value = stored.IsTruthy;
          return true;
        }
        value = true;
        foreach (var predicate in predicates) {
          if (!TryEvaluateValuePredicate(stored, predicate, context, reference, out var item, out error)) return false;
          if (error is not null) return false;
          value &= predicate.Negated ? !item : item;
        }
        return true;
      }
      default: {
        var predicates = reference.Properties.Where(IsBooleanProperty).ToArray();
        if (predicates.Length == 0) return context.TryEvaluate(reference, out value, out error);
        var valueReference = new MixinExpressionReference(
          reference.Root, reference.Member, [.. reference.Properties.Where(item => !IsBooleanProperty(item))]
        );
        if (!TryResolveCore(valueReference, context, locals, variables, out var subject, out error)) {
          if (predicates.Length == 0) {
            value = false;
            return false;
          }
          subject = NullMixinValue.Instance;
          error = null;
        }
        value = true;
        foreach (var predicate in predicates) {
          if (!TryEvaluateValuePredicate(subject, predicate, context, reference, out var item, out error)) return false;
          value &= predicate.Negated ? !item : item;
        }
        return true;
      }
    }
  }

  private static bool TryEvaluateValuePredicate(
    IMixinValue subject,
    MixinExpressionProperty property,
    IMixinExpressionContext context,
    MixinExpressionReference reference,
    out bool value,
    out string error
  ) {
    if (FunctionLibrary.TryGet(property.Name, out var function) &&
      function is PredicateFunctionDefinition predicate) {
      return predicate.EvaluateReference(
        subject, reference, property, context, out value, out error
      );
    }
    value = false;
    error = "unknown boolean pseudo-property ':?" + property.Name + "'";
    return false;
  }

  private static bool IsTableMutation(MixinExpressionProperty property) {
    return FunctionLibrary.TryGet(property.Name, out var function) && function is TableMutationFunction;
  }

  private static bool TryPrepareReference(
    MixinExpressionReference reference,
    IMixinExpressionContext context,
    MixinValueDictionary locals,
    MixinValueDictionary variables,
    out MixinExpressionReference prepared,
    out string error
  ) {
    var properties = new List<MixinExpressionProperty>(reference.Properties.Count);
    foreach (var property in reference.Properties) {
      var arguments = new List<string>(property.Arguments.Count);
      var values = new List<IMixinValue>(property.Arguments.Count);
      for (var argumentIndex = 0; argumentIndex < property.Arguments.Count; argumentIndex++) {
        var argument = property.Arguments[argumentIndex];
        var syntax = property.ParsedArguments.Count > argumentIndex
          ? property.ParsedArguments[argumentIndex]
          : new MixinPropertyArgumentSyntax(argument, null, null);
        if (syntax.BooleanExpression is not null) {
          arguments.Add(argument);
          values.Add(new StringMixinValue(argument));
          continue;
        }
        var parsedArgument = new DirectiveArgumentSyntax(syntax.Literal, syntax.ValueExpression);
        if (!TryResolveArgumentValue(parsedArgument, context, locals, variables, out var resolvedValue, out error)) {
          prepared = null;
          return false;
        }
        if (context is IMixinExpressionValueContext valueContext) {
          if (!valueContext.TryRenderValue(resolvedValue, null, out var rendered, out error)) {
            prepared = null;
            return false;
          }
          arguments.Add(rendered);
        } else arguments.Add(resolvedValue.Render());
        values.Add(resolvedValue);
      }
      properties.Add(
        new MixinExpressionProperty(
          property.Name, arguments.AsReadOnly(), values.AsReadOnly(), property.Negated,
          property.ParsedArguments
        )
      );
    }
    prepared = new MixinExpressionReference(reference.Root, reference.Member, properties.AsReadOnly());
    error = null;
    return true;
  }

  private static bool TryReduceLogicalProperties(
    MixinExpressionReference reference,
    IMixinExpressionContext context,
    MixinValueDictionary locals,
    MixinValueDictionary variables,
    out MixinExpressionReference reduced,
    out string error
  ) {
    var properties = reference.Properties.ToList();
    var root = reference.Root;
    var member = reference.Member;
    while (true) {
      var index = properties.FindIndex(IsLogicalFunction);
      if (index < 0) break;
      var operation = properties[index];
      FunctionLibrary.TryGet(operation.Name, out var operationDefinition);
      var logical = (LogicalFunctionDefinition)operationDefinition;
      if (operation.Arguments.Count == 0) {
        reduced = null;
        error = ":" + operation.Name + " requires at least one boolean expression";
        return false;
      }
      var prefix = new MixinExpressionReference(root, member, [.. properties.Take(index)]);
      if (!TryEvaluateCore(prefix, context, locals, variables, out var result, out error)) {
        reduced = null;
        return false;
      }
      foreach (var argument in operation.ParsedArguments) {
        if (argument.BooleanExpression is not { } parsed ||
          !TryEvaluateAll(parsed, context, locals, variables, out var item, out error, out _)) {
          reduced = null;
          error ??= ":" + operation.Name + " has an invalid boolean expression";
          return false;
        }
        result = logical.Combine(result, item);
      }
      root = result ? MixinExpressionRoot.True : MixinExpressionRoot.False;
      member = null;
      properties = [.. properties.Skip(index + 1)];
    }
    reduced = new MixinExpressionReference(root, member, properties.AsReadOnly());
    error = null;
    return true;
  }

  private static bool TryStored(
    MixinExpressionReference reference,
    IMixinExpressionContext context,
    MixinValueDictionary locals,
    MixinValueDictionary variables,
    out IMixinValue value,
    out string error
  ) {
    value = null;
    error = null;
    if (reference.Root != MixinExpressionRoot.Local &&
      reference.Root != MixinExpressionRoot.Variable) return false;
    if (string.IsNullOrEmpty(reference.Member)) {
      error = "@" + reference.Root.Keyword() + " requires a member name";
      return true;
    }
    var values = reference.Root == MixinExpressionRoot.Local ? locals : variables;
    if (!values.TryGetValue(reference.Member, out var stored)) {
      error = "unknown @" + reference.Root.Keyword() + " value '" + reference.Member + "'";
      return true;
    }
    value = MixinValue.From(stored, context);
    FunctionLibrary.TryApply(reference, context, reference.Member, ref value, out error);
    return true;
  }

  private static bool IsLogicalFunction(MixinExpressionProperty property) {
    return FunctionLibrary.IsLogical(property);
  }

  private static bool IsTypeFunction(MixinExpressionProperty property) {
    return FunctionLibrary.TryGet(property.Name, out var function) && function is TypeFunction;
  }

  internal static bool TryResumeTransform(
    MixinTransformRequest request,
    IMixinExpressionContext context,
    IMixinValue accumulated,
    out IMixinValue value,
    out string error
  ) {
    value = accumulated;
    return FunctionLibrary.TryResume(request, context, ref value, out error);
  }
}