using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static HelixSourceGenerator.Shared.GeneratorAnalysis;
using static HelixSourceGenerator.Shared.GeneratorDiagnostics.Structure;
using static HelixSourceGenerator.Shared.GeneratorStrings;

namespace HelixSourceGenerator.Shared;

/// <summary>Analyzes props into reusable parameter and assignment fragments.</summary>
internal static class PropStructApi {
  internal static bool TryAnalyze(
    INamedTypeSymbol type,
    out PropStructModel model,
    out Diagnostic diagnostic,
    bool datatype = false
  ) {
    var fields = InstanceFields(type);
    var props = fields.Select(field => new PropDefinition(
        field, field.Type, field.Name, Attribute(field, Attributes.Prop)
      )
    ).ToArray();
    if (!TryAnalyzeProps(props, out model, out diagnostic)) return false;
    if (!TryAnalyzeEquality(type, fields, out var equality, out diagnostic)) {
      model = null;
      return false;
    }
    model = new PropStructModel(
      model.ParameterParts, model.ArgumentParts, model.Assignments, model.RequiresUnsafe, equality,
      datatype ? AnalyzeDatatype(type.ToDisplayString(TypeDisplayFormat), type.Name, props) : null,
      model.PropertySymbols
    );
    return true;
  }

  internal static PropDatatypeModel AnalyzeDatatype(
    string structureType,
    string name,
    IReadOnlyList<PropDefinition> props
  ) {
    var properties = new List<PropDatatypeProperty>(props.Count);
    foreach (var prop in props) {
      var propType = prop.Type.ToDisplayString(TypeDisplayFormat);
      var attribute = prop.Attribute;
      var datatypeExpression = attribute is null ? null : StringArgument(attribute, PropArguments.Datatype);
      if (string.IsNullOrWhiteSpace(datatypeExpression))
        datatypeExpression = DefaultDatatypeExpression(prop.Type, propType);
      var required = true;
      var defaultValueExpression = "null";
      if (attribute is not null && TryReadDefault(attribute, out var defaultValue, out _) &&
        defaultValue.Mode != PropInitMode.None) {
        required = false;
        defaultValueExpression = defaultValue.Expression;
      }
      properties.Add(
        new PropDatatypeProperty(prop.Name, propType, datatypeExpression, required, defaultValueExpression)
      );
    }
    return new PropDatatypeModel(structureType, name, properties);
  }

  private static string DefaultDatatypeExpression(ITypeSymbol type, string typeName) {
    var datatypes = "global::" + Types.Datatypes + ".";
    switch (type.SpecialType) {
      case SpecialType.System_String: return datatypes + DatatypeMembers.String;
      case SpecialType.System_Int32: return datatypes + DatatypeMembers.Int;
      case SpecialType.System_Int64: return datatypes + DatatypeMembers.Long;
      case SpecialType.System_Single: return datatypes + DatatypeMembers.Float;
      case SpecialType.System_Double: return datatypes + DatatypeMembers.Double;
      case SpecialType.System_Boolean: return datatypes + DatatypeMembers.Bool;
    }
    if (type.TypeKind == TypeKind.Enum) return datatypes + DatatypeMembers.Enum + "<" + typeName + ">()";
    return type.ToDisplayString() switch {
      Types.UnityColor => datatypes + DatatypeMembers.Color,
      Types.UnityVector2 => datatypes + DatatypeMembers.Vector2,
      Types.UnityVector3 => datatypes + DatatypeMembers.Vector3,
      Types.UnityVector4 => datatypes + DatatypeMembers.Vector4,
      _ => datatypes + DatatypeMembers.Object + "<" + typeName + ">()"
    };
  }

  internal static bool TryAnalyzeProps(
    IReadOnlyList<PropDefinition> props,
    out PropStructModel model,
    out Diagnostic diagnostic
  ) {
    var parameters = new List<string>(props.Count);
    var arguments = new List<string>(props.Count);
    var assignments = new List<PropAssignment>(props.Count);
    var encounteredOptional = false;
    var requiresUnsafe = false;

    foreach (var prop in props) {
      var propName = EscapeIdentifier(prop.Name);
      var propType = prop.Type.ToDisplayString(TypeDisplayFormat);
      var refModifier = prop.RefKind == RefKind.In ? "in " : "";
      requiresUnsafe |= ContainsPointer(prop.Type);
      var propAttribute = prop.Attribute;

      if (propAttribute is null) {
        if (encounteredOptional) {
          model = null;
          diagnostic = Diagnostic.Create(RequiredPropAfterOptionalProp, LocationOf(prop.Symbol), prop.Name);
          return false;
        }
        AddDirect(propType, propName, refModifier, parameters, arguments, assignments);
        continue;
      }

      if (!TryReadDefault(propAttribute, out var defaultValue, out var error)) {
        model = null;
        diagnostic = InvalidDefault(prop.Symbol, prop.Name, error);
        return false;
      }
      if (defaultValue.Mode == PropInitMode.None) {
        if (encounteredOptional) {
          model = null;
          diagnostic = Diagnostic.Create(RequiredPropAfterOptionalProp, LocationOf(prop.Symbol), prop.Name);
          return false;
        }
        AddDirect(propType, propName, refModifier, parameters, arguments, assignments);
        continue;
      }

      if (prop.RefKind == RefKind.In) {
        model = null;
        diagnostic = InvalidDefault(prop.Symbol, prop.Name, "in props cannot declare default values");
        return false;
      }

      encounteredOptional = true;
      if (defaultValue.Mode != PropInitMode.Deferred) {
        parameters.Add(propType + " " + propName + " = " + defaultValue.Expression);
        arguments.Add(propName);
        assignments.Add(new PropAssignment(propName, propName));
        continue;
      }

      if (!TryMakeNullableParameterType(prop.Type, out var parameterType, out error)) {
        model = null;
        diagnostic = InvalidDefault(prop.Symbol, prop.Name, error);
        return false;
      }
      parameters.Add(parameterType + " " + propName + " = null");
      arguments.Add(propName);
      assignments.Add(new PropAssignment(propName, propName + " ?? " + defaultValue.Expression));
    }

    model = new PropStructModel(
      parameters, arguments, assignments, requiresUnsafe,
      propertySymbols: [.. props.Select(prop => prop.Symbol)]
    );
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
      if (attribute is not null && !BooleanArgument(attribute, PropArguments.Equatable, true)) continue;

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
        ? Templates.Equality
        : StringArgument(attribute, PropArguments.EqualitySyntax, Templates.Equality);
      var hashCodeSyntax = attribute is null
        ? Templates.HashCode
        : StringArgument(attribute, PropArguments.HashCodeSyntax, Templates.HashCode);
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
      !hasTypedEquals,
      !hasTypedEquals || HasOrdinaryTypedEquals(type),
      !HasObjectEquals(type),
      !HasHashCode(type),
      fields.Any(field => ContainsPointer(field.Type))
    );
    return true;
  }

  private static bool ImplementsEquatable(INamedTypeSymbol type) {
    return type.AllInterfaces.Any(candidate =>
      candidate.OriginalDefinition.MetadataName == "IEquatable`1" &&
      candidate.OriginalDefinition.ContainingNamespace.ToDisplayString() == "System" &&
      candidate.TypeArguments.Length == 1 &&
      SymbolEqualityComparer.Default.Equals(candidate.TypeArguments[0], type)
    );
  }

  private static bool HasTypedEquals(INamedTypeSymbol type) {
    return type.GetMembers().OfType<IMethodSymbol>().Any(method =>
      IsTypedEquals(method, type) &&
      (method is { Name: "Equals", DeclaredAccessibility: Accessibility.Public } ||
        method.ExplicitInterfaceImplementations.Any())
    );
  }

  private static bool HasOrdinaryTypedEquals(INamedTypeSymbol type) {
    return type.GetMembers("Equals")
      .OfType<IMethodSymbol>()
      .Any(method => IsTypedEquals(method, type));
  }

  private static bool IsTypedEquals(IMethodSymbol method, INamedTypeSymbol type) {
    return !method.IsStatic &&
      !method.ReturnsByRef &&
      !method.ReturnsByRefReadonly &&
      method.ReturnType.SpecialType == SpecialType.System_Boolean &&
      method.Parameters.Length == 1 &&
      method.Parameters[0].RefKind == RefKind.None &&
      SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, type);
  }

  private static bool HasObjectEquals(INamedTypeSymbol type) {
    return type.GetMembers("Equals")
      .OfType<IMethodSymbol>()
      .Any(method =>
        !method.IsStatic &&
        method.ReturnType.SpecialType == SpecialType.System_Boolean &&
        method.Parameters.Length == 1 &&
        method.Parameters[0].Type.SpecialType == SpecialType.System_Object
      );
  }

  private static bool HasHashCode(INamedTypeSymbol type) {
    return type.GetMembers("GetHashCode")
      .OfType<IMethodSymbol>()
      .Any(method =>
        !method.IsStatic &&
        method.ReturnType.SpecialType == SpecialType.System_Int32 &&
        method.Parameters.Length == 0
      );
  }

  private static bool IsPrimitive(ITypeSymbol type) {
    return type.TypeKind == TypeKind.Enum || type.SpecialType is
      SpecialType.System_Boolean or SpecialType.System_Byte or SpecialType.System_SByte or SpecialType.System_Int16 or
      SpecialType.System_UInt16 or SpecialType.System_Int32 or SpecialType.System_UInt32 or
      SpecialType.System_Int64 or SpecialType.System_UInt64 or SpecialType.System_Char or
      SpecialType.System_Single or SpecialType.System_Double or SpecialType.System_Decimal;
  }

  private static Diagnostic InvalidEqualitySyntax(IFieldSymbol field, string error) {
    return Diagnostic.Create(
      GeneratorDiagnostics.Structure.InvalidEqualitySyntax,
      LocationOf(field),
      field.Name,
      error
    );
  }

  private static void AddDirect(
    string fieldType,
    string fieldName,
    string refModifier,
    ICollection<string> parameters,
    ICollection<string> arguments,
    ICollection<PropAssignment> assignments
  ) {
    parameters.Add(refModifier + fieldType + " " + fieldName);
    arguments.Add(refModifier + fieldName);
    assignments.Add(new PropAssignment(fieldName, fieldName));
  }

  private static Diagnostic InvalidDefault(ISymbol symbol, string name, string error) {
    return Diagnostic.Create(GeneratorDiagnostics.Structure.InvalidDefault, LocationOf(symbol), name, error);
  }

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
    if (constant.Kind != TypedConstantKind.Enum || constant.Type is not INamedTypeSymbol enumType)
      return constant.Kind == TypedConstantKind.Primitive && TryFormatPrimitive(constant.Value, out expression);
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

internal readonly struct PropDefinition {
  internal PropDefinition(
    ISymbol symbol,
    ITypeSymbol type,
    string name,
    AttributeData attribute,
    RefKind refKind = RefKind.None
  ) {
    Symbol = symbol;
    Type = type;
    Name = name;
    Attribute = attribute;
    RefKind = refKind;
  }

  internal ISymbol Symbol { get; }
  internal ITypeSymbol Type { get; }
  internal string Name { get; }
  internal AttributeData Attribute { get; }
  internal RefKind RefKind { get; }
}

internal sealed class PropStructModel {
  internal PropStructModel(
    IReadOnlyList<string> parameterParts,
    IReadOnlyList<string> arguments,
    IReadOnlyList<PropAssignment> assignments,
    bool requiresUnsafe,
    PropEqualityModel equality = null,
    PropDatatypeModel datatype = null,
    IReadOnlyList<ISymbol> propertySymbols = null
  ) {
    ParameterParts = parameterParts;
    ArgumentParts = arguments;
    Assignments = assignments;
    RequiresUnsafe = requiresUnsafe;
    Equality = equality ?? PropEqualityModel.None;
    Datatype = datatype;
    PropertySymbols = propertySymbols ?? [];
  }

  internal static PropStructModel Empty { get; } = new([], [], [], false, PropEqualityModel.None);

  internal IReadOnlyList<string> ParameterParts { get; }
  internal IReadOnlyList<string> ArgumentParts { get; }
  internal IReadOnlyList<PropAssignment> Assignments { get; }
  internal bool RequiresUnsafe { get; }
  internal PropEqualityModel Equality { get; }
  internal PropDatatypeModel Datatype { get; }
  internal IReadOnlyList<ISymbol> PropertySymbols { get; }

  internal void AppendAssignments(SharpStringBuilder builder, string target) {
    if (builder is null) throw new ArgumentNullException(nameof(builder));
    if (target is null) throw new ArgumentNullException(nameof(target));
    var prefix = target.Length == 0 ? "" : target + ".";
    foreach (var assignment in Assignments)
      builder.Assignment(prefix + assignment.FieldName, assignment.ValueExpression);
  }
}

internal sealed class PropDatatypeModel {
  internal PropDatatypeModel(
    string structureType,
    string name,
    IReadOnlyList<PropDatatypeProperty> properties
  ) {
    StructureType = structureType;
    Name = name;
    Properties = properties;
  }

  internal string StructureType { get; }
  internal string Name { get; }
  internal IReadOnlyList<PropDatatypeProperty> Properties { get; }

  internal void AppendMember(
    SharpStringBuilder builder,
    IReadOnlyList<string> configuration
  ) {
    var structureDatatype = "global::" + Types.StructureDatatype + "<" + StructureType + ">";
    builder.Append("public static readonly ").Append(structureDatatype)
      .Append(" ").Append(Members.Datatype).AppendLine(" =");
    using (builder.Delimited(
      "new global::" + Types.ConfigurableStructureDatatype + "<" + StructureType + ">(",
      ")." + Members.Datatype + ";"
    )) {
      using (builder.Delimited("new " + structureDatatype + "(", "),", closeLine: true)) {
        builder.AppendLine(SymbolDisplay.FormatLiteral(Name, true) + ",");
        using (builder.Delimited(
          "new global::" + Types.StructurePropertyDatatype + "<" + StructureType + ">[] {",
          "}",
          closeLine: true
        )) {
          for (var index = 0; index < Properties.Count; index++) {
            AppendProperty(builder, Properties[index]);
            if (index != Properties.Count - 1) builder.AppendLine(",");
            else builder.AppendLine();
          }
        }
      }
      builder.Append("datatype => ").Append(Members.ConfigureDatatype).AppendLine("(datatype)");
    }
    builder.BlankLine();
    using (builder.Method(
      "static void " + Members.ConfigureDatatype,
      [structureDatatype + " datatype"],
      false
    ))
      foreach (var statement in configuration)
        builder.Statement(statement);
  }

  private void AppendProperty(SharpStringBuilder builder, PropDatatypeProperty property) {
    var fieldName = EscapeIdentifier(property.Name);
    using (builder.Delimited(
      "new global::" + Types.StructurePropertyDatatype + "<" + StructureType + ", " +
      property.Type + ">(",
      ")",
      closeLine: false
    )) {
      builder.AppendLine(SymbolDisplay.FormatLiteral(property.Name, true) + ",");
      builder.AppendLine(property.DatatypeExpression + ",");
      builder.AppendLine("(ref " + StructureType + " value) => value." + fieldName + ",");
      builder.Append("(ref ").Append(StructureType).Append(" value, ")
        .Append(property.Type).Append(" propertyValue) => value.")
        .Append(fieldName).AppendLine(" = propertyValue,");
      builder.AppendLine("required: " + (property.Required ? "true" : "false") + ",");
      builder.Append("defaultValue: ").AppendLine(property.DefaultValueExpression);
    }
  }
}

internal readonly struct PropDatatypeProperty {
  internal PropDatatypeProperty(
    string name, string type, string datatypeExpression,
    bool required, string defaultValueExpression
  ) {
    Name = name;
    Type = type;
    DatatypeExpression = datatypeExpression;
    Required = required;
    DefaultValueExpression = defaultValueExpression;
  }

  internal string Name { get; }
  internal string Type { get; }
  internal string DatatypeExpression { get; }
  internal bool Required { get; }
  internal string DefaultValueExpression { get; }
}

internal sealed class PropEqualityModel {
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

  internal static PropEqualityModel None { get; } = new(null, [], [], false, false, false, false, false);

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
        .Parameters([TypeName + " other"], false)
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
        .Parameters(["object obj"], false)
        .AppendLine($" => obj is {TypeName} other && {equalsCall};");
    }
    if (!GenerateHashCode) return;

    builder.BlankLine();
    using (builder.Method($"public override{unsafeModifier} int GetHashCode", [], false)) {
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