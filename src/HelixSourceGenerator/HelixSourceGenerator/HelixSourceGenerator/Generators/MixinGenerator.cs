using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using HELIX.SourceGen.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static HELIX.SourceGen.GeneratorAnalysis;
using static HELIX.SourceGen.GeneratorDiagnostics.Mixins;
using static HELIX.SourceGen.GeneratorSource;
using static HELIX.SourceGen.GeneratorStrings;

namespace HELIX.SourceGen;

[Generator(LanguageNames.CSharp)]
public sealed partial class MixinGenerator : IIncrementalGenerator {
  private const int Unmarked = -1;
  private const int InjectThis = 0;
  private const int InjectTarget = 1;
  private const int InjectAttribute = 2;
  private const int InjectDelegate = 3;
  private const int InjectReturnValue = 4;
  private const int InjectMember = 5;
  private const int InjectParameter = 6;
  private static readonly ConditionalWeakTable<INamedTypeSymbol, Dictionary<string, string>>
    TargetDefinitionCache = new();

  public void Initialize(IncrementalGeneratorInitializationContext context) {
    // An inherited [EnableMixins] cannot be found by an attribute syntax provider. Limit the
    // broader syntax walk to classes which either carry attributes, have a base list, or are
    // partial (the relevant attribute/base list may be on another part). Discard duplicate
    // partial declarations before doing any mixin analysis.
    var targets = context.SyntaxProvider.CreateSyntaxProvider(
        static (node, _) =>
          node is TypeDeclarationSyntax {
            RawKind: (int)SyntaxKind.ClassDeclaration or (int)SyntaxKind.RecordDeclaration
          } declaration && (declaration.AttributeLists.Count != 0 || declaration.BaseList is not null ||
            declaration.Modifiers.Any(SyntaxKind.PartialKeyword)),
        static (ctx, _) => GetTarget(ctx)
      )
      .Where(static target => target is not null);
    var preparedExpressions =
      context.CompilationProvider.Select(static (compilation, _) => CollectPreparedExpressions(compilation)
      );

    context.RegisterSourceOutput(
      preparedExpressions,
      static (spc, prepared) => ReportPreparedExpressionDiagnostics(spc, prepared)
    );

    context.RegisterSourceOutput(
      targets.Combine(preparedExpressions),
      static (spc, input) => Generate(
        spc,
        input.Left,
        input.Right.State
      )
    );
  }

  private static PreparedMixinExpressions CollectPreparedExpressions(
    Compilation compilation
  ) {
    var result = new List<PreparedMixinExpression>();
    var assemblies = compilation.SourceModule.ReferencedAssemblySymbols
      .OrderBy(item => item.Identity.Name, StringComparer.Ordinal)
      .Concat(new[] { compilation.Assembly });
    var interpreter = new MixinExpressionInterpreter();
    foreach (var assembly in assemblies) {
      foreach (var attribute in assembly.GetAttributes().Where(item =>
        IsAttribute(item, Attributes.MixinPrepareGlobal)
      )) {
        var expression = attribute.ConstructorArguments.Length == 1
          ? attribute.ConstructorArguments[0].Value as string
          : null;
        var validation = interpreter.ValidateSyntax(expression);
        result.Add(
          new PreparedMixinExpression(
            assembly.Identity.Name,
            expression ?? "",
            attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? Location.None,
            validation
          )
        );
      }
    }
    var state = interpreter.PrepareGlobals(
      result.Where(item => item.Validation.Success).Select(item => item.Expression)
    );
    return new PreparedMixinExpressions(result, state);
  }

  private static void ReportPreparedExpressionDiagnostics(
    SourceProductionContext context,
    PreparedMixinExpressions preparedExpressions
  ) {
    foreach (var prepared in preparedExpressions.Items.Where(item => !item.Validation.Success)) {
      context.ReportDiagnostic(
        Diagnostic.Create(
          InvalidPreparedExpression,
          prepared.Location,
          prepared.Provider,
          prepared.Validation.ErrorLine.ToString(CultureInfo.InvariantCulture),
          prepared.Validation.Error
        )
      );
    }
    var valid = preparedExpressions.Items.Where(item => item.Validation.Success).ToArray();
    foreach (var log in preparedExpressions.State.Logs) {
      if (log.ProgramIndex < 0 || log.ProgramIndex >= valid.Length) continue;
      context.ReportDiagnostic(
        Diagnostic.Create(
          ExpressionLog,
          valid[log.ProgramIndex].Location,
          log.Text
        )
      );
    }
  }

  private static MixinTarget GetTarget(GeneratorSyntaxContext context) {
    var declaration = (TypeDeclarationSyntax)context.Node;
    if (context.SemanticModel.GetDeclaredSymbol(declaration) is not INamedTypeSymbol { TypeKind: TypeKind.Class } type)
      return null;

    var first = type.DeclaringSyntaxReferences.FirstOrDefault();
    if (first is null || first.SyntaxTree != declaration.SyntaxTree ||
      first.Span.Start != declaration.SpanStart) return null;
    return HasMixinsEnabled(type) && context.SemanticModel.Compilation is CSharpCompilation compilation
      ? new MixinTarget(type, compilation)
      : null;
  }

  private static bool HasMixinsEnabled(INamedTypeSymbol type) {
    for (var current = type; current is not null; current = current.BaseType) {
      if (Attribute(current, Attributes.EnableMixins) is not null ||
        current.GetAttributes().Any(attribute => IsBuiltinMixinStereotype(
            attribute.AttributeClass
          )
        )) return true;
    }
    return type.AllInterfaces.Any(item => Attribute(item, Attributes.EnableMixins) is not null);
  }

  private static bool IsBuiltinMixinStereotype(INamedTypeSymbol attributeType) {
    for (var current = attributeType; current is not null; current = current.BaseType)
      if (BuiltinMixinStereotypes.Contains(current.ToDisplayString(), StringComparer.Ordinal))
        return true;
    return false;
  }

  private static bool HasComponentStereotype(INamedTypeSymbol type) {
    for (var current = type; current is not null; current = current.BaseType) {
      foreach (var attribute in current.GetAttributes()) {
        for (var attributeType = attribute.AttributeClass;
          attributeType is not null;
          attributeType = attributeType.BaseType)
          if (attributeType.ToDisplayString() == Attributes.Component)
            return true;
      }
    }
    return false;
  }

  private static IReadOnlyDictionary<string, string> TargetDefinitions(INamedTypeSymbol type) {
    return TargetDefinitionCache.GetValue(type, CreateTargetDefinitions);
  }

  private static Dictionary<string, string> CreateTargetDefinitions(INamedTypeSymbol type) {
    var definitions = new Dictionary<string, string>(StringComparer.Ordinal) {
      ["$Init"] = HasComponentStereotype(type) ? "^LoadComponent" : "Awake",
      ["$Dispose"] = HasComponentStereotype(type) ? "^UnloadComponent" : "OnDestroy"
    };
    var hierarchy = new Stack<INamedTypeSymbol>();
    for (var current = type; current is not null; current = current.BaseType) hierarchy.Push(current);
    while (hierarchy.Count != 0) {
      foreach (var applied in OrderedAttributes(hierarchy.Pop())) {
        if (IsAttribute(applied, Attributes.MixinDefineTarget)) ApplyTargetDefinition(applied, definitions);
        if (applied.AttributeClass is null) continue;
        var attributeHierarchy = new Stack<INamedTypeSymbol>();
        for (var current = applied.AttributeClass;
          current is not null;
          current = current.BaseType) attributeHierarchy.Push(current);
        while (attributeHierarchy.Count != 0) {
          foreach (var definition in OrderedAttributes(attributeHierarchy.Pop())
            .Where(item => IsAttribute(item, Attributes.MixinDefineTarget)))
            ApplyTargetDefinition(definition, definitions);
        }
      }
    }
    return definitions;
  }

  private static void ApplyTargetDefinition(
    AttributeData attribute,
    IDictionary<string, string> definitions
  ) {
    if (attribute.ConstructorArguments.Length < 2 ||
      attribute.ConstructorArguments[0].Value is not string key ||
      attribute.ConstructorArguments[1].Value is not string target ||
      string.IsNullOrEmpty(key)) return;
    definitions["$" + key.TrimStart('$')] = target;
  }

  private static void Generate(
    SourceProductionContext context,
    MixinTarget candidate,
    MixinExpressionPreparedState preparedExpressions
  ) {
    var target = candidate.Type;
    var location = LocationOf(target);
    if (!IsPartial(target)) {
      context.ReportDiagnostic(Diagnostic.Create(MustBePartial, location, target.Name));
      return;
    }

    var containing = FirstNonPartialContainingType(target);
    if (containing is not null) {
      context.ReportDiagnostic(
        Diagnostic.Create(
          ContainingTypeMustBePartial, location, target.Name, containing.Name
        )
      );
      return;
    }

    var interfaces = CollectMixinInterfaces(target);
    var implicitAttributes = new List<ImplicitMixinAttribute>();
    var implicitInterfaces = new List<INamedTypeSymbol>();
    ResolveMixinRequirements(
      context, target, interfaces, implicitAttributes, implicitInterfaces
    );
    var variables = CollectVariables(context, target, interfaces);
    var properties = CollectProperties(context, target, interfaces);
    var resources = CollectResources(target, variables, properties);
    var contributions = new List<MixinContribution>();
    var attributeExpressionOutputs = new List<MixinExpressionOutput>();
    foreach (var implicitInterface in implicitInterfaces) {
      attributeExpressionOutputs.Add(
        new MixinExpressionOutput(
          MixinExpressionOutputTarget.Implements,
          implicitInterface.ToDisplayString(TypeDisplayFormat)
        )
      );
    }
    var expressionVariables = new Dictionary<string, object>(StringComparer.Ordinal);
    CollectLocalContributions(context, target, contributions);
    CollectInterfaceContributions(
      context, target, interfaces, candidate.Compilation, preparedExpressions, expressionVariables,
      attributeExpressionOutputs, contributions
    );
    SpecializeContributions(
      context, target, resources, candidate.Compilation, preparedExpressions,
      expressionVariables, contributions
    );
    CollectAttributeContributions(
      context, target, resources, candidate.Compilation, preparedExpressions, expressionVariables,
      attributeExpressionOutputs, implicitAttributes, contributions
    );
    var methods = BuildMethods(
      context, target, contributions, resources, candidate.Compilation
    );
    var outputs = new ExpressionOutputs();
    foreach (var attribute in implicitAttributes) {
      outputs.AddAnnotation(attribute.Type.ToDisplayString(TypeDisplayFormat));
    }
    foreach (var contribution in contributions) {
      if (contribution.ExpressionResult is null) continue;
      foreach (var output in contribution.ExpressionResult.Outputs) outputs.Add(output);
    }
    foreach (var output in attributeExpressionOutputs) outputs.Add(output);

    if (variables.Count == 0 && properties.Count == 0 && methods.Count == 0 &&
      !outputs.Any) return;
    var wrapper = WrapType(
      target, "mixins", outputs.Usings,
      baseType: outputs.Implements.Count == 0 ? null : string.Join(", ", outputs.Implements),
      typeAttributes: outputs.Annotations
    );
    var source = wrapper.Build(
      builder => {
        if (variables.Count != 0) builder.AppendLine("#pragma warning disable CS0169");
        foreach (var variable in variables) {
          builder.Field(
            "private", variable.Type.ToDisplayString(TypeDisplayFormat), EscapeIdentifier(variable.Name)
          );
        }
        if (variables.Count != 0) builder.AppendLine("#pragma warning restore CS0169");
        if (variables.Count != 0 && (properties.Count != 0 || methods.Count != 0)) builder.BlankLine();

        for (var index = 0; index < properties.Count; index++) {
          AppendProperty(builder, properties[index]);
          if (index != properties.Count - 1 || methods.Count != 0) builder.BlankLine();
        }

        for (var index = 0; index < methods.Count; index++) {
          AppendMethod(builder, methods[index]);
          if (index != methods.Count - 1) builder.BlankLine();
        }
        foreach (var output in outputs.Class) {
          if (variables.Count != 0 || properties.Count != 0 || methods.Count != 0) builder.BlankLine();
          builder.AppendCode(output);
        }
      }, builder => {
        foreach (var output in outputs.File) builder.AppendCode(output);
      }, true
    );
    context.AddSource(wrapper.HintName, source);
  }
}
