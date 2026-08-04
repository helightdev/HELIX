using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static HELIX.SourceGen.GeneratorDiagnostics.Composition;
using static HELIX.SourceGen.GeneratorSource;
using static HELIX.SourceGen.GeneratorStrings;

namespace HELIX.SourceGen {
  [Generator(LanguageNames.CSharp)]
  public sealed class CompositionGenerator : IIncrementalGenerator {
    private const int MaxArgumentCount = 4;

    public void Initialize(IncrementalGeneratorInitializationContext context) {
      var methods = context.SyntaxProvider.ForAttributeWithMetadataName(
        Attributes.Composition,
        predicate: static (node, _) => node is MethodDeclarationSyntax,
        transform: static (ctx, _) => (IMethodSymbol)ctx.TargetSymbol
      );


      context.RegisterSourceOutput(methods, static (spc, method) => Generate(spc, method));
    }

    private static void Generate(SourceProductionContext spc, IMethodSymbol method) {
      var loc = GeneratorAnalysis.LocationOf(method);

      if (!method.IsStatic) {
        spc.ReportDiagnostic(Diagnostic.Create(MustBeStatic, loc, method.Name));
        return;
      }

      if (method.TypeParameters.Length > 0) {
        spc.ReportDiagnostic(Diagnostic.Create(MustNotBeGeneric, loc, method.Name));
        return;
      }

      if (!method.Name.StartsWith("_", StringComparison.Ordinal) || method.Name.Length < 2) {
        spc.ReportDiagnostic(Diagnostic.Create(MustStartWithUnderscore, loc, method.Name));
        return;
      }

      if (!HasSupportedParameters(method.Parameters)) {
        spc.ReportDiagnostic(Diagnostic.Create(MustHaveSupportedParameters, loc, method.Name, MaxArgumentCount));
        return;
      }

      var publicName = method.Name.Substring(1);
      var fieldName = $"_{char.ToLowerInvariant(publicName[0])}{publicName.Substring(1)}Id";
      var arguments = method.Parameters.Skip(1).ToArray();
      var composableType = BuildComposableType(arguments);
      var lambdaParameters = BuildLambdaParameters(arguments).Prepend($"ref {Types.Composition} cx");
      var invocationArguments = BuildInvocationArguments(arguments).Prepend("ref cx");

      var containing = WrapType(
        method.ContainingType,
        "composition",
        hintDiscriminator: method.Name
      );

      var source = containing.Build(builder => {
        builder.BlankLine()
          .Field("private static readonly", "ushort", fieldName, GetCompositionId(publicName))
          .BlankLine()
          .Append($"public static readonly {composableType} {publicName} = static ")
          .Parameters(lambdaParameters, multiline: false)
          .Append(" =>");
        using (builder.Block(suffix: ";")) {
          builder.Statement($"var transfer = new {Types.CompositionTransfer}()")
            .Statement($"{Types.CompositionInternals}.EnterComposition(ref cx, {fieldName}, ref transfer)");
          using (builder.Try()) {
            builder.Append(method.Name).Arguments(invocationArguments).AppendLine(";");
          }
          using (builder.Finally()) {
            builder.Statement($"{Types.CompositionInternals}.ExitComposition(ref cx, ref transfer)");
          }
        }
      });

      spc.AddSource(containing.HintName, source);
    }

    private static bool IsRefComposition(IParameterSymbol p) {
      if (p.RefKind != RefKind.Ref) return false;
      return GetTypeDisplayName(p.Type) == Types.Composition;
    }

    private static bool HasSupportedParameters(
      System.Collections.Immutable.ImmutableArray<IParameterSymbol> parameters
    ) {
      if (parameters.Length < 1 || parameters.Length > MaxArgumentCount + 1 ||
          !IsRefComposition(parameters[0])) return false;

      return parameters
        .Skip(1)
        .All(parameter => parameter.RefKind is RefKind.None or RefKind.In);
    }

    private static string BuildComposableType(IReadOnlyCollection<IParameterSymbol> arguments) => arguments.Count == 0
      ? Types.Composable
      : $"{(IsReadComposable(arguments) ? Types.ReadComposable : Types.Composable)}<{string.Join(", ", arguments.Select(argument => GetTypeDisplayName(argument.Type)))}>";

    private static IEnumerable<string> BuildLambdaParameters(IReadOnlyCollection<IParameterSymbol> arguments) =>
      arguments.Select((argument, index) =>
        $"{(IsReadComposable(arguments) ? "in " : "")}{GetTypeDisplayName(argument.Type)} arg{index}"
      );

    private static bool IsReadComposable(IReadOnlyCollection<IParameterSymbol> arguments) =>
      arguments.Count == 1 && arguments.First().RefKind == RefKind.In;

    private static IEnumerable<string> BuildInvocationArguments(IEnumerable<IParameterSymbol> arguments) =>
      arguments.Select((argument, index) => argument.RefKind == RefKind.In ? $"in arg{index}" : $"arg{index}");

    private static string GetTypeDisplayName(ITypeSymbol type) => type.ToDisplayString(
      SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)
    );
  }
}
