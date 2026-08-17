using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HELIX.SourceGen.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static HELIX.SourceGen.GeneratorAnalysis;
using static HELIX.SourceGen.GeneratorDiagnostics.Mixins;
using static HELIX.SourceGen.GeneratorSource;
using static HELIX.SourceGen.GeneratorStrings;

namespace HELIX.SourceGen;

public sealed partial class MixinGenerator {
  private enum ContributionKind { Local, Interface, Attribute }

  private sealed class ExpressionOutputs {
    private List<string> _annotations, _class, _file, _implements, _usings;
    private HashSet<string> _annotationSet, _implementSet, _usingSet;
    internal IReadOnlyList<string> Annotations => _annotations ?? (IReadOnlyList<string>)Array.Empty<string>();
    internal IReadOnlyList<string> Class => _class ?? (IReadOnlyList<string>)Array.Empty<string>();
    internal IReadOnlyList<string> File => _file ?? (IReadOnlyList<string>)Array.Empty<string>();
    internal IReadOnlyList<string> Implements => _implements ?? (IReadOnlyList<string>)Array.Empty<string>();
    internal IReadOnlyList<string> Usings => _usings ?? (IReadOnlyList<string>)Array.Empty<string>();
    internal bool Any { get; private set; }

    internal void AddAnnotation(string text) => AddUnique(
      ref _annotations, ref _annotationSet, text
    );

    internal void Add(MixinExpressionOutput output) {
      if (string.IsNullOrEmpty(output.Text)) return;
      Any = true;
      var text = output.Text.Trim();
      switch (output.Target) {
        case MixinExpressionOutputTarget.Implements:
          AddUnique(ref _implements, ref _implementSet, text); break;
        case MixinExpressionOutputTarget.Annotation:
          AddUnique(ref _annotations, ref _annotationSet, text); break;
        case MixinExpressionOutputTarget.Using:
          text = text.TrimEnd(';');
          AddUnique(ref _usings, ref _usingSet, text.Length == 0 ? "" : "using " + text + ";");
          break;
        case MixinExpressionOutputTarget.Class: (_class ??= new List<string>()).Add(output.Text); break;
        case MixinExpressionOutputTarget.File: (_file ??= new List<string>()).Add(output.Text); break;
      }
    }

    private static void AddUnique(ref List<string> values, ref HashSet<string> seen, string value) {
      if (value.Length == 0 || !(seen ??= new HashSet<string>(StringComparer.Ordinal)).Add(value)) return;
      (values ??= new List<string>()).Add(value);
    }
  }

  private sealed class MixinContribution {
    internal MixinContribution(
      IMethodSymbol method,
      string target,
      int order,
      string expression,
      ContributionKind kind,
      int sequence,
      ISymbol source,
      AttributeData appliedAttribute,
      IReadOnlyList<MixinParameter> parameters,
      IReadOnlyDictionary<string, string> targetDefinitions
    ) {
      var targetSyntax = ParseTarget(target, targetDefinitions);
      Method = method;
      Target = target;
      EmittedTarget = targetSyntax.Name;
      IsStaticTarget = targetSyntax.IsStatic;
      IsPublicTarget = targetSyntax.IsPublic;
      DelegateTarget = targetSyntax.DelegateType;
      Order = order;
      Expression = expression;
      Kind = kind;
      Sequence = sequence;
      Source = source;
      AppliedAttribute = appliedAttribute;
      Parameters = parameters;
      TargetDefinitions = targetDefinitions;
      GenericArguments = Array.Empty<ITypeSymbol>();
      PositionalCount = string.IsNullOrWhiteSpace(expression)
        ? parameters.Count(item => item.Injection == Unmarked)
        : 0;
    }

    internal IMethodSymbol Method { get; }
    internal string Target { get; }
    internal string EmittedTarget { get; }
    internal bool IsStaticTarget { get; }
    internal bool IsPublicTarget { get; }
    internal string DelegateTarget { get; }
    internal int Order { get; }
    internal string Expression { get; }
    internal ContributionKind Kind { get; }
    internal int Sequence { get; }
    internal ISymbol Source { get; }
    internal AttributeData AppliedAttribute { get; }
    internal ImplicitMixinAttribute ImplicitAttribute { get; private set; }
    internal IReadOnlyList<MixinParameter> Parameters { get; }
    internal IReadOnlyList<ITypeSymbol> GenericArguments { get; private set; }
    internal MixinExpressionResult ExpressionResult { get; private set; }
    internal int PositionalCount { get; }
    internal IReadOnlyDictionary<string, string> TargetDefinitions { get; }

    internal MixinContribution WithMethod(
      IMethodSymbol method,
      IReadOnlyList<ITypeSymbol> genericArguments
    ) {
      var parameters = new MixinParameter[Parameters.Count];
      for (var index = 0; index < parameters.Length; index++) {
        var source = Parameters[index];
        parameters[index] = new MixinParameter(
          method.Parameters[index], source.Injection, source.Name, source.Position,
          source.CheckAssignment
        );
      }
      var result = new MixinContribution(
        method, Target, Order, Expression, Kind, Sequence, Source,
        AppliedAttribute, parameters, TargetDefinitions
      );
      result.GenericArguments = genericArguments;
      result.ImplicitAttribute = ImplicitAttribute;
      return result;
    }

    internal MixinContribution WithImplicitAttribute(ImplicitMixinAttribute implicitAttribute) {
      ImplicitAttribute = implicitAttribute;
      return this;
    }

    internal MixinContribution WithExpressionResult(MixinExpressionResult expressionResult) {
      ExpressionResult = expressionResult;
      return this;
    }
  }

  private sealed record MixinParameter(
    IParameterSymbol Symbol, int Injection, string Name, int Position, bool CheckAssignment
  ) {
    internal int PositionInMethod => Symbol.Ordinal;
  }

  private sealed record MixinTargetParameter(
    ITypeSymbol TypeSymbol, string Name, RefKind RefKind, bool IsParams
  ) {
    internal string Type => TypeSymbol.ToDisplayString(TypeDisplayFormat);
    internal string Declaration => (IsParams ? "params " : RefPrefix(RefKind)) + Type + " " + EscapeIdentifier(Name);
    internal string Argument => RefPrefix(RefKind) + EscapeIdentifier(Name);

    internal static MixinTargetParameter FromSymbol(IParameterSymbol parameter) {
      return new MixinTargetParameter(parameter.Type, parameter.Name, parameter.RefKind, parameter.IsParams);
    }
  }

  private sealed record GeneratedMethod(
    string Declaration,
    IReadOnlyList<string> TypeParameters,
    IReadOnlyList<string> Constraints,
    IReadOnlyList<MixinTargetParameter> Parameters,
    string ReturnType,
    bool CallBase,
    bool IsStatic,
    string Name,
    IReadOnlyList<MixinContribution> Contributions,
    IReadOnlyList<MixinResource> Resources
  );

  private sealed record MixinVariable(ITypeSymbol Type, string Name);

  private sealed record MixinResource(
    string Name, string Expression, ITypeSymbol Type, bool IsProperty, bool IsStatic
  );

  private sealed class AttributeExpressionTarget {
    internal AttributeExpressionTarget(
      string target,
      int order,
      IReadOnlyDictionary<string, string> targetDefinitions
    ) {
      var targetSyntax = ParseTarget(target, targetDefinitions);
      Target = target;
      EmittedTarget = targetSyntax.Name;
      Order = order;
    }

    internal string Target { get; }
    internal string EmittedTarget { get; }
    internal int Order { get; }
    internal List<MixinExpressionOutput> Outputs { get; set; }
  }

  private sealed record TargetSyntax(string Name, bool IsStatic, bool IsPublic, string DelegateType);

  private sealed class ImplicitMixinAttribute {
    private ImplicitMixinAttribute(
      INamedTypeSymbol type,
      IReadOnlyDictionary<string, ImplicitMixinValue> values
    ) {
      Type = type;
      Values = values;
    }

    internal INamedTypeSymbol Type { get; }
    internal IReadOnlyDictionary<string, ImplicitMixinValue> Values { get; }

    internal static bool TryCreate(
      INamedTypeSymbol type,
      out ImplicitMixinAttribute attribute,
      out string failure
    ) {
      attribute = null;
      failure = null;
      if (type.IsAbstract || type.IsGenericType) {
        failure = "implicit attribute '" + type.ToDisplayString() +
          "' must be a non-abstract, non-generic attribute type";
        return false;
      }
      var usage = Attribute(type, "System.AttributeUsageAttribute");
      if (usage is not null &&
        (usage.ConstructorArguments.Length == 0 ||
          !TryConvertToInt32(usage.ConstructorArguments[0].Value, out var targets) ||
          (targets & 4) == 0)) {
        failure = "implicit attribute '" + type.ToDisplayString() +
          "' does not allow class-level application";
        return false;
      }
      var constructor = type.InstanceConstructors
        .Where(item => item.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal &&
          item.Parameters.All(parameter => parameter.HasExplicitDefaultValue)
        )
        .OrderBy(item => item.Parameters.Length)
        .FirstOrDefault();
      if (constructor is null) {
        failure = "implicit attribute '" + type.ToDisplayString() +
          "' has no accessible constructor whose parameters all have defaults";
        return false;
      }
      var values = new Dictionary<string, ImplicitMixinValue>(StringComparer.OrdinalIgnoreCase);
      foreach (var parameter in constructor.Parameters) {
        values[parameter.Name] = new ImplicitMixinValue(
          parameter.Type, parameter.ExplicitDefaultValue
        );
      }
      attribute = new ImplicitMixinAttribute(type, values);
      return true;
    }
  }

  private sealed record MixinTarget(INamedTypeSymbol Type, CSharpCompilation Compilation);

  private sealed record PreparedMixinExpression(
    string Provider,
    string Expression,
    Location Location,
    MixinExpressionValidationResult Validation
  );

  private sealed record PreparedMixinExpressions(
    IReadOnlyList<PreparedMixinExpression> Items, MixinExpressionPreparedState State
  );
}
