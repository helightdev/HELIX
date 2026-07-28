using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HELIX.SourceGen {
  [Generator(LanguageNames.CSharp)]
  public sealed class PropStructGenerator : IIncrementalGenerator {
    private const string PropStructAttributeName = "HELIX.Compose.PropStructAttribute";
    private const string PropDefaultAttributeName = "HELIX.Compose.PropDefaultAttribute";

    private static readonly DiagnosticDescriptor MustBePartial = new DiagnosticDescriptor(
      "HLXP00",
      "Prop struct must be partial",
      "Struct '{0}' is marked [PropStruct] but is not declared partial",
      "HELIX",
      DiagnosticSeverity.Error,
      true
    );

    private static readonly DiagnosticDescriptor ContainingTypeMustBePartial = new DiagnosticDescriptor(
      "HLXP01",
      "Containing type must be partial",
      "Struct '{0}' is marked [PropStruct], but containing type '{1}' is not declared partial",
      "HELIX",
      DiagnosticSeverity.Error,
      true
    );

    private static readonly DiagnosticDescriptor RequiredFieldAfterOptionalField = new DiagnosticDescriptor(
      "HLXP02",
      "Required prop must precede optional props",
      "Field '{0}' has no [PropDefault] but follows a field with a default value",
      "HELIX",
      DiagnosticSeverity.Error,
      true
    );

    private static readonly DiagnosticDescriptor InvalidDefault = new DiagnosticDescriptor(
      "HLXP03",
      "Invalid prop default",
      "The [PropDefault] on field '{0}' is invalid: {1}",
      "HELIX",
      DiagnosticSeverity.Error,
      true
    );

    private static readonly SymbolDisplayFormat TypeDisplayFormat =
      SymbolDisplayFormat.FullyQualifiedFormat
        .WithMiscellaneousOptions(
          SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
          SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
        );

    public void Initialize(IncrementalGeneratorInitializationContext context) {
      var structs = context.SyntaxProvider.ForAttributeWithMetadataName(
        PropStructAttributeName,
        predicate: static (node, _) => node is StructDeclarationSyntax,
        transform: static (ctx, _) => (INamedTypeSymbol)ctx.TargetSymbol
      );

      context.RegisterSourceOutput(structs, static (spc, type) => Generate(spc, type));
    }

    private static void Generate(SourceProductionContext context, INamedTypeSymbol type) {
      if (!IsPartial(type)) {
        context.ReportDiagnostic(Diagnostic.Create(
          MustBePartial,
          type.Locations.FirstOrDefault() ?? Location.None,
          type.Name
        ));
        return;
      }

      for (var containing = type.ContainingType; containing is not null; containing = containing.ContainingType) {
        if (IsPartial(containing)) continue;
        context.ReportDiagnostic(Diagnostic.Create(
          ContainingTypeMustBePartial,
          type.Locations.FirstOrDefault() ?? Location.None,
          type.Name,
          containing.Name
        ));
        return;
      }

      var fields = type.GetMembers()
        .OfType<IFieldSymbol>()
        .Where(field => !field.IsStatic && !field.IsImplicitlyDeclared)
        .OrderBy(SourceOrder)
        .ToArray();

      if (!TryBuild(context, fields, out var generated)) return;

      var wrapper = BuildWrapper(type, CollectUsings(type), out var hintName);
      var indent = new string(' ', wrapper.MemberDepth * 2);
      var parameterIndent = indent + "  ";
      var body = new StringBuilder();
      body.Append('\n');
      body.Append(indent);
      body.Append(AccessibilityText(type.DeclaredAccessibility));
      body.Append(generated.RequiresUnsafe ? " unsafe " : " ");
      body.Append(EscapeIdentifier(type.Name));
      body.Append('(');

      if (generated.ParameterParts.Count > 0) {
        body.Append('\n');
        body.Append(parameterIndent);
        body.Append(string.Join(",\n" + parameterIndent, generated.ParameterParts));
        body.Append('\n');
        body.Append(indent);
      }

      body.Append(") {\n");
      body.Append(generated.RenderAssignments("this", indent + "  "));
      body.Append(indent);
      body.Append("}\n");

      context.AddSource(hintName, wrapper.Header + body + wrapper.Footer);
    }

    /// <summary>
    /// Builds the reusable prop surface for a struct. Parameters includes defaults and can be
    /// placed directly in a constructor or method signature. RenderAssignments accepts any
    /// assignment target, for example "this", "props", or "state.props".
    /// </summary>
    internal static bool TryBuild(
      SourceProductionContext context,
      INamedTypeSymbol type,
      out PropStructCode generated
    ) {
      var fields = type.GetMembers()
        .OfType<IFieldSymbol>()
        .Where(field => !field.IsStatic && !field.IsImplicitlyDeclared)
        .OrderBy(SourceOrder)
        .ToArray();
      return TryBuild(context, fields, out generated);
    }

    private static bool TryBuild(
      SourceProductionContext context,
      IReadOnlyList<IFieldSymbol> fields,
      out PropStructCode generated
    ) {
      generated = null;
      var parameters = new List<string>(fields.Count);
      var arguments = new List<string>(fields.Count);
      var assignments = new List<PropAssignment>(fields.Count);
      var encounteredOptional = false;
      var requiresUnsafe = false;

      foreach (var field in fields) {
        var fieldName = EscapeIdentifier(field.Name);
        var fieldType = field.Type.ToDisplayString(TypeDisplayFormat);
        requiresUnsafe |= ContainsPointer(field.Type);

        var defaultAttribute = field.GetAttributes()
          .FirstOrDefault(attribute =>
            attribute.AttributeClass?.ToDisplayString() == PropDefaultAttributeName);

        if (defaultAttribute is null) {
          if (encounteredOptional) {
            context.ReportDiagnostic(Diagnostic.Create(
              RequiredFieldAfterOptionalField,
              field.Locations.FirstOrDefault() ?? Location.None,
              field.Name
            ));
            return false;
          }

          parameters.Add(fieldType + " " + fieldName);
          arguments.Add(fieldName);
          assignments.Add(new PropAssignment(fieldName, fieldName));
          continue;
        }

        encounteredOptional = true;
        if (!TryReadDefault(defaultAttribute, field, out var defaultValue, out var error)) {
          context.ReportDiagnostic(Diagnostic.Create(
            InvalidDefault,
            field.Locations.FirstOrDefault() ?? Location.None,
            field.Name,
            error
          ));
          return false;
        }

        switch (defaultValue.Mode) {
          case PropInitMode.Literal:
          case PropInitMode.Constant:
            parameters.Add(fieldType + " " + fieldName + " = " + defaultValue.Expression);
            arguments.Add(fieldName);
            assignments.Add(new PropAssignment(fieldName, fieldName));
            break;

          case PropInitMode.Deferred:
            if (!TryMakeNullableParameterType(field.Type, out var parameterType, out error)) {
              context.ReportDiagnostic(Diagnostic.Create(
                InvalidDefault,
                field.Locations.FirstOrDefault() ?? Location.None,
                field.Name,
                error
              ));
              return false;
            }

            parameters.Add(parameterType + " " + fieldName + " = null");
            arguments.Add(fieldName);
            assignments.Add(new PropAssignment(
              fieldName,
              fieldName + " ?? " + defaultValue.Expression
            ));
            break;
        }
      }
      generated = new PropStructCode(parameters, arguments, assignments, requiresUnsafe);
      return true;
    }

    private static bool TryReadDefault(
      AttributeData attribute,
      IFieldSymbol field,
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
            modeValue < 0 ||
            modeValue > 2) {
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

      if (constant.Kind == TypedConstantKind.Enum &&
          constant.Type is INamedTypeSymbol enumType) {
        var matchingMember = enumType.GetMembers()
          .OfType<IFieldSymbol>()
          .FirstOrDefault(member =>
            member.HasConstantValue &&
            Equals(member.ConstantValue, constant.Value));
        var enumName = enumType.ToDisplayString(TypeDisplayFormat);
        if (matchingMember is not null) {
          expression = enumName + "." + EscapeIdentifier(matchingMember.Name);
          return true;
        }

        if (TryFormatPrimitive(constant.Value, out var underlying)) {
          expression = "(" + enumName + ")" + underlying;
          return true;
        }
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
      if (type.IsReferenceType) {
        parameterType = type.ToDisplayString(TypeDisplayFormat);
        return true;
      }

      if (type is INamedTypeSymbol named &&
          named.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T) {
        parameterType = type.ToDisplayString(TypeDisplayFormat);
        return true;
      }

      if (type.IsValueType ||
          type is ITypeParameterSymbol { HasValueTypeConstraint: true }) {
        parameterType = "global::System.Nullable<" +
                        type.ToDisplayString(TypeDisplayFormat) + ">";
        return true;
      }

      parameterType = null;
      error = "Deferred initialization is not supported for an unconstrained type parameter";
      return false;
    }

    private static bool TryConvertToInt32(object value, out int converted) {
      try {
        converted = Convert.ToInt32(value, CultureInfo.InvariantCulture);
        return true;
      } catch (Exception) {
        converted = 0;
        return false;
      }
    }

    private static int SourceOrder(IFieldSymbol field) {
      var location = field.Locations.FirstOrDefault(item => item.IsInSource);
      return location?.SourceSpan.Start ?? int.MaxValue;
    }

    private static bool ContainsPointer(ITypeSymbol type) {
      if (type.TypeKind == TypeKind.Pointer || type.TypeKind == TypeKind.FunctionPointer) return true;
      if (type is IArrayTypeSymbol array) return ContainsPointer(array.ElementType);
      if (type is INamedTypeSymbol named) return named.TypeArguments.Any(ContainsPointer);
      return false;
    }

    private static bool IsPartial(INamedTypeSymbol type) =>
      type.DeclaringSyntaxReferences.Any(reference =>
        reference.GetSyntax() is TypeDeclarationSyntax declaration &&
        declaration.Modifiers.Any(SyntaxKind.PartialKeyword));

    private static string AccessibilityText(Accessibility accessibility) => accessibility switch {
      Accessibility.Public => "public",
      Accessibility.Private => "private",
      Accessibility.Protected => "protected",
      Accessibility.Internal => "internal",
      Accessibility.ProtectedAndInternal => "private protected",
      Accessibility.ProtectedOrInternal => "protected internal",
      _ => "internal"
    };

    private static string EscapeIdentifier(string identifier) =>
      SyntaxFacts.GetKeywordKind(identifier) != SyntaxKind.None ||
      SyntaxFacts.GetContextualKeywordKind(identifier) != SyntaxKind.None
        ? "@" + identifier
        : identifier;

    public static IReadOnlyList<string> CollectUsings(INamedTypeSymbol type) {
      var result = new List<string>();
      var seen = new HashSet<string>(StringComparer.Ordinal);

      foreach (var syntaxReference in type.DeclaringSyntaxReferences) {
        if (syntaxReference.GetSyntax() is not TypeDeclarationSyntax declaration) continue;

        if (declaration.SyntaxTree.GetRoot() is CompilationUnitSyntax compilationUnit) {
          foreach (var usingDirective in compilationUnit.Usings) {
            AddUsing(usingDirective, seen, result);
          }
        }

        for (SyntaxNode node = declaration.Parent; node is not null; node = node.Parent) {
          if (node is BaseNamespaceDeclarationSyntax namespaceDeclaration) {
            foreach (var usingDirective in namespaceDeclaration.Usings) {
              AddUsing(usingDirective, seen, result);
            }
          }
        }
      }

      return result;
    }

    private static void AddUsing(
      UsingDirectiveSyntax usingDirective,
      ISet<string> seen,
      ICollection<string> result
    ) {
      var text = usingDirective.WithoutTrivia().NormalizeWhitespace().ToFullString();
      if (seen.Add(text)) result.Add(text);
    }

    private static Wrapper BuildWrapper(
      INamedTypeSymbol type,
      IReadOnlyList<string> usings,
      out string hintName
    ) {
      var chain = new List<INamedTypeSymbol>();
      for (var current = type; current is not null; current = current.ContainingType) {
        chain.Add(current);
      }
      chain.Reverse();

      var namespaceName = type.ContainingNamespace is { IsGlobalNamespace: false } ns
        ? ns.ToDisplayString()
        : null;
      var header = new StringBuilder("// <auto-generated/>\n");
      foreach (var usingDirective in usings) {
        header.Append(usingDirective);
        header.Append('\n');
      }
      if (usings.Count > 0) header.Append('\n');

      var depth = 0;
      if (namespaceName is not null) {
        header.Append("namespace ");
        header.Append(namespaceName);
        header.Append(" {\n");
        depth++;
      }

      foreach (var current in chain) {
        header.Append(' ', depth * 2);
        header.Append(current.IsStatic ? "static partial " : "partial ");
        header.Append(TypeKeyword(current));
        header.Append(' ');
        header.Append(EscapeIdentifier(current.Name));
        header.Append(TypeParameters(current));
        header.Append(" {\n");
        depth++;
      }

      var footer = new StringBuilder();
      for (var closeDepth = depth - 1; closeDepth >= 0; closeDepth--) {
        footer.Append(' ', closeDepth * 2);
        footer.Append("}\n");
      }

      var metadataPath = string.Join(".", chain.Select(item => item.MetadataName));
      hintName = Sanitize((namespaceName ?? "global") + "." + metadataPath) + ".prop-struct.g.cs";
      return new Wrapper(header.ToString(), footer.ToString(), depth);
    }

    private static string TypeKeyword(INamedTypeSymbol type) {
      if (type.IsRecord) return type.TypeKind == TypeKind.Struct ? "record struct" : "record";
      return type.TypeKind == TypeKind.Struct ? "struct" :
             type.TypeKind == TypeKind.Interface ? "interface" : "class";
    }

    private static string TypeParameters(INamedTypeSymbol type) => type.TypeParameters.Length == 0
      ? ""
      : "<" + string.Join(", ", type.TypeParameters.Select(parameter =>
        EscapeIdentifier(parameter.Name))) + ">";

    private static string Sanitize(string value) {
      var characters = value.ToCharArray();
      for (var index = 0; index < characters.Length; index++) {
        if (!char.IsLetterOrDigit(characters[index])) characters[index] = '_';
      }
      return new string(characters);
    }

    private enum PropInitMode {
      Literal,
      Constant,
      Deferred
    }

    private readonly struct PropDefaultValue {
      public PropDefaultValue(PropInitMode mode, string expression) {
        Mode = mode;
        Expression = expression;
      }

      public PropInitMode Mode { get; }
      public string Expression { get; }
    }

    internal sealed class PropStructCode {
      internal static PropStructCode Empty { get; } = new PropStructCode(
        new string[0],
        new string[0],
        new PropAssignment[0],
        false
      );

      internal PropStructCode(
        IReadOnlyList<string> parameterParts,
        IReadOnlyList<string> arguments,
        IReadOnlyList<PropAssignment> assignments,
        bool requiresUnsafe
      ) {
        ParameterParts = parameterParts;
        Parameters = string.Join(", ", parameterParts);
        Arguments = string.Join(", ", arguments);
        Assignments = assignments;
        RequiresUnsafe = requiresUnsafe;
      }

      internal IReadOnlyList<string> ParameterParts { get; }
      internal string Parameters { get; }
      internal string Arguments { get; }
      internal IReadOnlyList<PropAssignment> Assignments { get; }
      internal bool RequiresUnsafe { get; }

      internal string RenderAssignments(string target, string indent = "") {
        if (target is null) throw new ArgumentNullException(nameof(target));
        var prefix = target.Length == 0 ? "" : target + ".";
        var result = new StringBuilder();
        foreach (var assignment in Assignments) {
          result.Append(indent);
          result.Append(prefix);
          result.Append(assignment.FieldName);
          result.Append(" = ");
          result.Append(assignment.ValueExpression);
          result.Append(";\n");
        }
        return result.ToString();
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

    private readonly struct Wrapper {
      public Wrapper(string header, string footer, int memberDepth) {
        Header = header;
        Footer = footer;
        MemberDepth = memberDepth;
      }

      public string Header { get; }
      public string Footer { get; }
      public int MemberDepth { get; }
    }
  }
}
