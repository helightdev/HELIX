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
    ) {
      var fields = InstanceFields(type);
      if (!TryAnalyzeFields(fields, out model, out diagnostic)) return false;
      if (!TryAnalyzeEquality(type, fields, out var equality, out diagnostic)) {
        model = null;
        return false;
      }
      model = new PropStructModel(
        model.ParameterParts,
        model.ArgumentParts,
        model.Assignments,
        model.RequiresUnsafe,
        equality
      );
      return true;
    }

    private static bool TryAnalyzeFields(
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
        var propAttribute = Attribute(field, Attributes.Prop);

        if (propAttribute is null) {
          if (encounteredOptional) {
            model = null;
            diagnostic = Diagnostic.Create(RequiredFieldAfterOptionalField, LocationOf(field), field.Name);
            return false;
          }
          AddDirect(fieldType, fieldName, parameters, arguments, assignments);
          continue;
        }

        if (!TryReadDefault(propAttribute, out var defaultValue, out var error)) {
          model = null;
          diagnostic = InvalidDefault(field, error);
          return false;
        }
        if (defaultValue.Mode == PropInitMode.None) {
          if (encounteredOptional) {
            model = null;
            diagnostic = Diagnostic.Create(RequiredFieldAfterOptionalField, LocationOf(field), field.Name);
            return false;
          }
          AddDirect(fieldType, fieldName, parameters, arguments, assignments);
          continue;
        }

        encounteredOptional = true;
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

    private static bool TryAnalyzeEquality(
      INamedTypeSymbol type,
      IReadOnlyList<IFieldSymbol> fields,
      out PropEqualityModel equality,
      out Diagnostic diagnostic
    ) {
      diagnostic = null;
      if (!ImplementsEquatable(type)) {
        equality = PropEqualityModel.None;
        return true;
      }

      var comparisons = new List<string>(fields.Count);
      var hashValues = new List<string>(fields.Count);
      foreach (var field in fields) {
        var attribute = Attribute(field, Attributes.Prop);
        if (attribute is not null && !BooleanArgument(attribute, "Equatable", true)) continue;

        var fieldName = EscapeIdentifier(field.Name);
        var current = "this." + fieldName;
        var other = "other." + fieldName;
        if (IsPrimitive(field.Type)) {
          comparisons.Add(current + " == " + other);
          hashValues.Add(current);
          continue;
        }
        if (field.Type.TypeKind is TypeKind.Pointer or TypeKind.FunctionPointer) {
          comparisons.Add(current + " == " + other);
          hashValues.Add("((global::System.IntPtr)" + current + ")");
          continue;
        }

        var equalitySyntax = attribute is null
          ? Templates.ProxyEquality
          : StringArgument(attribute, "EqualitySyntax", Templates.ProxyEquality);
        var hashCodeSyntax = attribute is null
          ? Templates.ProxyHashCode
          : StringArgument(attribute, "HashCodeSyntax", Templates.ProxyHashCode);
        if (string.IsNullOrWhiteSpace(equalitySyntax) || string.IsNullOrWhiteSpace(hashCodeSyntax)) {
          equality = null;
          diagnostic = InvalidEqualitySyntax(field, "the syntax must be a non-empty format string");
          return false;
        }

        try {
          comparisons.Add(string.Format(CultureInfo.InvariantCulture, equalitySyntax, current, other));
          hashValues.Add(string.Format(CultureInfo.InvariantCulture, hashCodeSyntax, current));
        } catch (FormatException exception) {
          equality = null;
          diagnostic = InvalidEqualitySyntax(field, exception.Message);
          return false;
        }
      }

      var hasTypedEquals = HasTypedEquals(type);
      equality = new PropEqualityModel(
        type.ToDisplayString(TypeDisplayFormat),
        comparisons,
        hashValues,
        generateTypedEquals: !hasTypedEquals,
        callTypedEqualsDirectly: !hasTypedEquals || HasOrdinaryTypedEquals(type),
        generateObjectEquals: !HasObjectEquals(type),
        generateHashCode: !HasHashCode(type),
        requiresUnsafe: fields.Any(field => ContainsPointer(field.Type))
      );
      return true;
    }

    private static bool ImplementsEquatable(INamedTypeSymbol type) => type.AllInterfaces.Any(candidate =>
      candidate.OriginalDefinition.MetadataName == "IEquatable`1" &&
      candidate.OriginalDefinition.ContainingNamespace.ToDisplayString() == "System" &&
      candidate.TypeArguments.Length == 1 &&
      SymbolEqualityComparer.Default.Equals(candidate.TypeArguments[0], type)
    );

    private static bool HasTypedEquals(INamedTypeSymbol type) => type.GetMembers().OfType<IMethodSymbol>().Any(method =>
      IsTypedEquals(method, type) &&
      (method is { Name: "Equals", DeclaredAccessibility: Accessibility.Public } ||
       method.ExplicitInterfaceImplementations.Any())
    );

    private static bool HasOrdinaryTypedEquals(INamedTypeSymbol type) => type.GetMembers("Equals")
      .OfType<IMethodSymbol>()
      .Any(method => IsTypedEquals(method, type));

    private static bool IsTypedEquals(IMethodSymbol method, INamedTypeSymbol type) => !method.IsStatic &&
      !method.ReturnsByRef &&
      !method.ReturnsByRefReadonly &&
      method.ReturnType.SpecialType == SpecialType.System_Boolean &&
      method.Parameters.Length == 1 &&
      method.Parameters[0].RefKind == RefKind.None &&
      SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, type);

    private static bool HasObjectEquals(INamedTypeSymbol type) => type.GetMembers("Equals")
      .OfType<IMethodSymbol>()
      .Any(method =>
        !method.IsStatic &&
        method.ReturnType.SpecialType == SpecialType.System_Boolean &&
        method.Parameters.Length == 1 &&
        method.Parameters[0].Type.SpecialType == SpecialType.System_Object
      );

    private static bool HasHashCode(INamedTypeSymbol type) => type.GetMembers("GetHashCode")
      .OfType<IMethodSymbol>()
      .Any(method =>
        !method.IsStatic &&
        method.ReturnType.SpecialType == SpecialType.System_Int32 &&
        method.Parameters.Length == 0
      );

    private static bool IsPrimitive(ITypeSymbol type) => type.TypeKind == TypeKind.Enum || type.SpecialType is
      SpecialType.System_Boolean or SpecialType.System_Byte or SpecialType.System_SByte or SpecialType.System_Int16 or
      SpecialType.System_UInt16 or SpecialType.System_Int32 or SpecialType.System_UInt32 or
      SpecialType.System_Int64 or SpecialType.System_UInt64 or SpecialType.System_Char or
      SpecialType.System_Single or SpecialType.System_Double or SpecialType.System_Decimal;

    private static Diagnostic InvalidEqualitySyntax(IFieldSymbol field, string error) => Diagnostic.Create(
      GeneratorDiagnostics.PropStruct.InvalidEqualitySyntax,
      LocationOf(field),
      field.Name,
      error
    );

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
      out PropInitializerValue value,
      out string error
    ) {
      value = default;
      error = null;
      if (attribute.ConstructorArguments.Length == 0) {
        value = new PropInitializerValue(PropInitMode.None, null);
        return true;
      }

      var mode = PropInitMode.Literal;
      if (attribute.ConstructorArguments.Length > 1) {
        var rawMode = attribute.ConstructorArguments[1].Value;
        if (rawMode is null ||
            !TryConvertToInt32(rawMode, out var modeValue) ||
            modeValue < (int)PropInitMode.Literal ||
            modeValue > (int)PropInitMode.None) {
          error = "the PropInit value is not recognized";
          return false;
        }
        mode = (PropInitMode)modeValue;
      }

      if (mode == PropInitMode.None) {
        value = new PropInitializerValue(mode, null);
        return true;
      }

      var argument = attribute.ConstructorArguments[0];
      if (mode == PropInitMode.Literal) {
        if (!TryFormatLiteral(argument, out var expression)) {
          error = "the literal value cannot be used as a constructor parameter default";
          return false;
        }
        value = new PropInitializerValue(mode, expression);
        return true;
      }

      if (argument.Value is not string code || string.IsNullOrWhiteSpace(code)) {
        error = mode + " initialization requires a non-empty string value";
        return false;
      }
      value = new PropInitializerValue(mode, code);
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

    private enum PropInitMode { Literal, Constant, Deferred, None }

    private readonly struct PropInitializerValue {
      internal PropInitializerValue(PropInitMode mode, string expression) {
        Mode = mode;
        Expression = expression;
      }

      internal PropInitMode Mode { get; }
      internal string Expression { get; }
    }
  }

  internal sealed class PropStructModel {
    internal static PropStructModel Empty { get; } = new([], [], [], false, PropEqualityModel.None);

    internal PropStructModel(
      IReadOnlyList<string> parameterParts,
      IReadOnlyList<string> arguments,
      IReadOnlyList<PropAssignment> assignments,
      bool requiresUnsafe,
      PropEqualityModel equality = null
    ) {
      ParameterParts = parameterParts;
      ArgumentParts = arguments;
      Assignments = assignments;
      RequiresUnsafe = requiresUnsafe;
      Equality = equality ?? PropEqualityModel.None;
    }

    internal IReadOnlyList<string> ParameterParts { get; }
    internal IReadOnlyList<string> ArgumentParts { get; }
    internal IReadOnlyList<PropAssignment> Assignments { get; }
    internal bool RequiresUnsafe { get; }
    internal PropEqualityModel Equality { get; }

    internal void AppendAssignments(SharpStringBuilder builder, string target) {
      if (builder is null) throw new ArgumentNullException(nameof(builder));
      if (target is null) throw new ArgumentNullException(nameof(target));
      var prefix = target.Length == 0 ? "" : target + ".";
      foreach (var assignment in Assignments) {
        builder.Assignment(prefix + assignment.FieldName, assignment.ValueExpression);
      }
    }
  }

  internal sealed class PropEqualityModel {
    internal static PropEqualityModel None { get; } = new(null, [], [], false, false, false, false, false);

    internal PropEqualityModel(
      string typeName,
      IReadOnlyList<string> comparisons,
      IReadOnlyList<string> hashValues,
      bool generateTypedEquals,
      bool callTypedEqualsDirectly,
      bool generateObjectEquals,
      bool generateHashCode,
      bool requiresUnsafe
    ) {
      TypeName = typeName;
      Comparisons = comparisons;
      HashValues = hashValues;
      GenerateTypedEquals = generateTypedEquals;
      CallTypedEqualsDirectly = callTypedEqualsDirectly;
      GenerateObjectEquals = generateObjectEquals;
      GenerateHashCode = generateHashCode;
      RequiresUnsafe = requiresUnsafe;
    }

    internal string TypeName { get; }
    internal IReadOnlyList<string> Comparisons { get; }
    internal IReadOnlyList<string> HashValues { get; }
    internal bool GenerateTypedEquals { get; }
    internal bool CallTypedEqualsDirectly { get; }
    internal bool GenerateObjectEquals { get; }
    internal bool GenerateHashCode { get; }
    internal bool RequiresUnsafe { get; }
    internal bool HasMembers => GenerateTypedEquals || GenerateObjectEquals || GenerateHashCode;

    internal void AppendMembers(SharpStringBuilder builder) {
      if (!HasMembers) return;
      var unsafeModifier = RequiresUnsafe ? " unsafe" : "";
      if (GenerateTypedEquals) {
        builder.BlankLine()
          .Append($"public{unsafeModifier} bool Equals")
          .Parameters([TypeName + " other"], multiline: false)
          .Append(" => ");
        if (Comparisons.Count == 0) builder.Append("true");
        else builder.AppendList(Comparisons, " && ");
        builder.AppendLine(";");
      }
      if (GenerateObjectEquals) {
        var equalsCall = CallTypedEqualsDirectly
          ? "Equals(other)"
          : $"((global::System.IEquatable<{TypeName}>)this).Equals(other)";
        builder.BlankLine()
          .Append("public override bool Equals")
          .Parameters(["object obj"], multiline: false)
          .AppendLine($" => obj is {TypeName} other && {equalsCall};");
      }
      if (!GenerateHashCode) return;

      builder.BlankLine();
      using (builder.Method($"public override{unsafeModifier} int GetHashCode", [], multiline: false)) {
        if (HashValues.Count == 0) {
          builder.Return("0");
          return;
        }
        builder.Statement("var hashCode = new global::System.HashCode()");
        foreach (var value in HashValues) builder.Statement("hashCode.Add(" + value + ")");
        builder.Return("hashCode.ToHashCode()");
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