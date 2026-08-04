using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static HELIX.SourceGen.GeneratorAnalysis;
using static HELIX.SourceGen.GeneratorDiagnostics.PropStruct;
using static HELIX.SourceGen.GeneratorStrings;

namespace HELIX.SourceGen {
  /// <summary>Analyzes a prop struct into reusable constructor and assignment fragments.</summary>
  internal static class PropStructApi {
    internal static bool TryAnalyze(
      INamedTypeSymbol type,
      out PropStructModel model,
      out Diagnostic diagnostic
    ) => TryAnalyze(InstanceFields(type), out model, out diagnostic);

    internal static bool TryAnalyze(
      IReadOnlyList<IFieldSymbol> fields,
      out PropStructModel model,
      out Diagnostic diagnostic
    ) {
      var parameters = new List<string>(fields.Count);
      var arguments = new List<string>(fields.Count);
      var assignments = new List<PropAssignment>(fields.Count);
      var encounteredOptional = false;
      var requiresUnsafe = false;

      foreach (var field in fields) {
        var fieldName = EscapeIdentifier(field.Name);
        var fieldType = field.Type.ToDisplayString(TypeDisplayFormat);
        requiresUnsafe |= ContainsPointer(field.Type);
        var defaultAttribute = Attribute(field, Attributes.PropDefault);

        if (defaultAttribute is null) {
          if (encounteredOptional) {
            model = null;
            diagnostic = Diagnostic.Create(RequiredFieldAfterOptionalField, LocationOf(field), field.Name);
            return false;
          }
          AddDirect(fieldType, fieldName, parameters, arguments, assignments);
          continue;
        }

        encounteredOptional = true;
        if (!TryReadDefault(defaultAttribute, out var defaultValue, out var error)) {
          model = null;
          diagnostic = InvalidDefault(field, error);
          return false;
        }

        if (defaultValue.Mode != PropInitMode.Deferred) {
          parameters.Add(fieldType + " " + fieldName + " = " + defaultValue.Expression);
          arguments.Add(fieldName);
          assignments.Add(new PropAssignment(fieldName, fieldName));
          continue;
        }

        if (!TryMakeNullableParameterType(field.Type, out var parameterType, out error)) {
          model = null;
          diagnostic = InvalidDefault(field, error);
          return false;
        }
        parameters.Add(parameterType + " " + fieldName + " = null");
        arguments.Add(fieldName);
        assignments.Add(new PropAssignment(fieldName, fieldName + " ?? " + defaultValue.Expression));
      }

      model = new PropStructModel(parameters, arguments, assignments, requiresUnsafe);
      diagnostic = null;
      return true;
    }

    private static void AddDirect(
      string fieldType,
      string fieldName,
      ICollection<string> parameters,
      ICollection<string> arguments,
      ICollection<PropAssignment> assignments
    ) {
      parameters.Add(fieldType + " " + fieldName);
      arguments.Add(fieldName);
      assignments.Add(new PropAssignment(fieldName, fieldName));
    }

    private static Diagnostic InvalidDefault(IFieldSymbol field, string error) => Diagnostic.Create(
      GeneratorDiagnostics.PropStruct.InvalidDefault,
      LocationOf(field),
      field.Name,
      error
    );

    private static bool TryReadDefault(
      AttributeData attribute,
      out PropDefaultValue value,
      out string error
    ) {
      value = default;
      error = null;
      if (attribute.ConstructorArguments.Length == 0) {
        error = "the attribute has no value";
        return false;
      }

      var mode = PropInitMode.Literal;
      if (attribute.ConstructorArguments.Length > 1) {
        var rawMode = attribute.ConstructorArguments[1].Value;
        if (rawMode is null ||
            !TryConvertToInt32(rawMode, out var modeValue) ||
            modeValue < (int)PropInitMode.Literal ||
            modeValue > (int)PropInitMode.Deferred) {
          error = "the PropInit value is not recognized";
          return false;
        }
        mode = (PropInitMode)modeValue;
      }

      var argument = attribute.ConstructorArguments[0];
      if (mode == PropInitMode.Literal) {
        if (!TryFormatLiteral(argument, out var expression)) {
          error = "the literal value cannot be used as a constructor parameter default";
          return false;
        }
        value = new PropDefaultValue(mode, expression);
        return true;
      }

      if (argument.Value is not string code || string.IsNullOrWhiteSpace(code)) {
        error = mode + " initialization requires a non-empty string value";
        return false;
      }
      value = new PropDefaultValue(mode, code);
      return true;
    }

    private static bool TryFormatLiteral(TypedConstant constant, out string expression) {
      expression = null;
      if (constant.IsNull) {
        expression = "null";
        return true;
      }
      if (constant.Kind == TypedConstantKind.Enum && constant.Type is INamedTypeSymbol enumType) {
        var matchingMember = enumType.GetMembers().OfType<IFieldSymbol>().FirstOrDefault(member =>
          member.HasConstantValue && Equals(member.ConstantValue, constant.Value)
        );
        var enumName = enumType.ToDisplayString(TypeDisplayFormat);
        if (matchingMember is not null) {
          expression = enumName + "." + EscapeIdentifier(matchingMember.Name);
          return true;
        }
        if (TryFormatPrimitive(constant.Value, out var underlying)) {
          expression = "(" + enumName + ")" + underlying;
          return true;
        }
        expression = null;
        return false;
      }
      return constant.Kind == TypedConstantKind.Primitive &&
             TryFormatPrimitive(constant.Value, out expression);
    }

    private static bool TryFormatPrimitive(object value, out string expression) {
      switch (value) {
        case bool boolean:
          expression = boolean ? "true" : "false";
          return true;
        case string text:
          expression = SymbolDisplay.FormatLiteral(text, true);
          return true;
        case char character:
          expression = SymbolDisplay.FormatLiteral(character, true);
          return true;
        case sbyte number:
          expression = number.ToString(CultureInfo.InvariantCulture);
          return true;
        case byte number:
          expression = number.ToString(CultureInfo.InvariantCulture);
          return true;
        case short number:
          expression = number.ToString(CultureInfo.InvariantCulture);
          return true;
        case ushort number:
          expression = number.ToString(CultureInfo.InvariantCulture);
          return true;
        case int number:
          expression = number.ToString(CultureInfo.InvariantCulture);
          return true;
        case uint number:
          expression = number.ToString(CultureInfo.InvariantCulture) + "U";
          return true;
        case long number:
          expression = number.ToString(CultureInfo.InvariantCulture) + "L";
          return true;
        case ulong number:
          expression = number.ToString(CultureInfo.InvariantCulture) + "UL";
          return true;
        case float number:
          expression = FormatSingle(number);
          return true;
        case double number:
          expression = FormatDouble(number);
          return true;
        default:
          expression = null;
          return false;
      }
    }

    private static string FormatSingle(float value) {
      if (float.IsNaN(value)) return "global::System.Single.NaN";
      if (float.IsPositiveInfinity(value)) return "global::System.Single.PositiveInfinity";
      if (float.IsNegativeInfinity(value)) return "global::System.Single.NegativeInfinity";
      return value.ToString("R", CultureInfo.InvariantCulture) + "F";
    }

    private static string FormatDouble(double value) {
      if (double.IsNaN(value)) return "global::System.Double.NaN";
      if (double.IsPositiveInfinity(value)) return "global::System.Double.PositiveInfinity";
      if (double.IsNegativeInfinity(value)) return "global::System.Double.NegativeInfinity";
      return value.ToString("R", CultureInfo.InvariantCulture) + "D";
    }

    private static bool TryMakeNullableParameterType(
      ITypeSymbol type,
      out string parameterType,
      out string error
    ) {
      error = null;
      if (type.IsReferenceType ||
          type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T }) {
        parameterType = type.ToDisplayString(TypeDisplayFormat);
        return true;
      }
      if (type.IsValueType || type is ITypeParameterSymbol { HasValueTypeConstraint: true }) {
        parameterType = "global::System.Nullable<" +
                        type.ToDisplayString(TypeDisplayFormat) + ">";
        return true;
      }
      parameterType = null;
      error = "Deferred initialization is not supported for an unconstrained type parameter";
      return false;
    }

    private enum PropInitMode { Literal, Constant, Deferred }

    private readonly struct PropDefaultValue {
      internal PropDefaultValue(PropInitMode mode, string expression) {
        Mode = mode;
        Expression = expression;
      }

      internal PropInitMode Mode { get; }
      internal string Expression { get; }
    }
  }

  internal sealed class PropStructModel {
    internal static PropStructModel Empty { get; } = new([], [], [], false);

    internal PropStructModel(
      IReadOnlyList<string> parameterParts,
      IReadOnlyList<string> arguments,
      IReadOnlyList<PropAssignment> assignments,
      bool requiresUnsafe
    ) {
      ParameterParts = parameterParts;
      ArgumentParts = arguments;
      Assignments = assignments;
      RequiresUnsafe = requiresUnsafe;
    }

    internal IReadOnlyList<string> ParameterParts { get; }
    internal IReadOnlyList<string> ArgumentParts { get; }
    internal IReadOnlyList<PropAssignment> Assignments { get; }
    internal bool RequiresUnsafe { get; }

    internal void AppendAssignments(SharpStringBuilder builder, string target) {
      if (builder is null) throw new ArgumentNullException(nameof(builder));
      if (target is null) throw new ArgumentNullException(nameof(target));
      var prefix = target.Length == 0 ? "" : target + ".";
      foreach (var assignment in Assignments) {
        builder.Assignment(prefix + assignment.FieldName, assignment.ValueExpression);
      }
    }
  }

  internal readonly struct PropAssignment {
    internal PropAssignment(string fieldName, string valueExpression) {
      FieldName = fieldName;
      ValueExpression = valueExpression;
    }

    internal string FieldName { get; }
    internal string ValueExpression { get; }
  }
}
