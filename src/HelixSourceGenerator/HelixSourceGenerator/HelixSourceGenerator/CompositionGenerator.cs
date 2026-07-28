using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HELIX.SourceGen {


  [Generator(LanguageNames.CSharp)]
  public sealed class CompositionGenerator : IIncrementalGenerator {
    private const int MaxArgumentCount = 4;
    private const string AttributeMetadataName = "HELIX.Compose.CompositionAttribute";
    private const string CompositionTypeName = "HELIX.Compose.Composition";
    private const string CompositionIdTypeName = "HELIX.Compose.CompositionId";
    private const string CompositionInternalsTypeName = "HELIX.Compose.CompositionInternals";
    private const string CompositionTransferTypeName = "HELIX.Compose.CompositionInternals.TransferData";
    private const string ComposableTypeName = "HELIX.Compose.Composable";
    private const string ReadComposableTypeName = "HELIX.Compose.ReadComposable";

    private static readonly DiagnosticDescriptor MustBeStatic = new DiagnosticDescriptor(
      "HLX001",
      "Composition method must be static",
      "Method '{0}' is marked [Composition] but is not static",
      "HELIX",
      DiagnosticSeverity.Error,
      true
    );

    private static readonly DiagnosticDescriptor MustStartWithUnderscore = new DiagnosticDescriptor(
      "HLX002",
      "Composition method must start with '_'",
      "Method '{0}' is marked [Composition] but does not start with '_'",
      "HELIX",
      DiagnosticSeverity.Error,
      true
    );

    private static readonly DiagnosticDescriptor MustHaveSupportedParameters = new DiagnosticDescriptor(
      "HLX003",
      "Composition method has unsupported parameters",
      "Method '{0}' is marked [Composition] but must take 'ref HELIX.NW.Composition' and at most {1} additional value or 'in' parameters",
      "HELIX",
      DiagnosticSeverity.Error,
      true
    );

    private static readonly DiagnosticDescriptor MustNotBeGeneric = new DiagnosticDescriptor(
      "HLX004",
      "Composition method must not be generic",
      "Method '{0}' is marked [Composition] but is generic, which is not supported",
      "HELIX",
      DiagnosticSeverity.Error,
      true
    );

    public void Initialize(IncrementalGeneratorInitializationContext context) {
      var methods = context.SyntaxProvider.ForAttributeWithMetadataName(
        AttributeMetadataName,
        predicate: static (node, _) => node is MethodDeclarationSyntax,
        transform: static (ctx, _) => (IMethodSymbol)ctx.TargetSymbol
      );


      context.RegisterSourceOutput(methods, static (spc, method) => Generate(spc, method));
    }

    private static void Generate(SourceProductionContext spc, IMethodSymbol method) {
      var loc = method.Locations.FirstOrDefault() ?? Location.None;

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
        spc.ReportDiagnostic(
          Diagnostic.Create(
            MustHaveSupportedParameters,
            loc,
            method.Name,
            MaxArgumentCount
          )
        );
        return;
      }

      var publicName = method.Name.Substring(1);
      var fieldName = $"_{char.ToLowerInvariant(publicName[0])}{publicName.Substring(1)}Id";
      var arguments = method.Parameters.Skip(1).ToArray();
      var composableType = BuildComposableType(arguments);
      var lambdaArguments = BuildLambdaArguments(arguments);
      var invocationArguments = BuildInvocationArguments(arguments);

      var containing = BuildContainingTypeWrapper(method.ContainingType, method.Name, out var hintName);

      var body = $@"
    private static readonly ushort {fieldName} = {CompositionIdTypeName}.GetCompositionId();

    public static readonly {composableType} {publicName} = static (ref {CompositionTypeName} cx{lambdaArguments}) => {{
      var transfer = new {CompositionTransferTypeName}();
      {CompositionInternalsTypeName}.EnterComposition(ref cx, {fieldName}, ref transfer);
      try {{{method.Name}(ref cx{invocationArguments});}}
      finally {{{CompositionInternalsTypeName}.ExitComposition(ref cx, ref transfer);}}
    }};
";

      var source = containing.Header + body + containing.Footer;
      spc.AddSource(hintName, source);
    }

    private static bool IsRefComposition(IParameterSymbol p) {
      if (p.RefKind != RefKind.Ref) return false;
      return GetTypeDisplayName(p.Type) == CompositionTypeName;
    }

    private static bool HasSupportedParameters(
      System.Collections.Immutable.ImmutableArray<IParameterSymbol> parameters
    ) {
      if (parameters.Length < 1 || parameters.Length > MaxArgumentCount + 1 ||
          !IsRefComposition(parameters[0])) return false;

      return parameters
        .Skip(1)
        .All(parameter => parameter.RefKind == RefKind.None || parameter.RefKind == RefKind.In);
    }

    private static string BuildComposableType(IReadOnlyCollection<IParameterSymbol> arguments) => arguments.Count == 0
      ? ComposableTypeName
      : $"{(IsReadComposable(arguments) ? ReadComposableTypeName : ComposableTypeName)}<{string.Join(", ", arguments.Select(argument => GetTypeDisplayName(argument.Type)))}>";

    private static string BuildLambdaArguments(IReadOnlyCollection<IParameterSymbol> arguments) => string.Concat(
      arguments.Select((argument, index) => $", {(IsReadComposable(arguments) ? "in " : "")}{GetTypeDisplayName(argument.Type)} arg{index}"
      )
    );

    private static bool IsReadComposable(IReadOnlyCollection<IParameterSymbol> arguments) =>
      arguments.Count == 1 && arguments.First().RefKind == RefKind.In;

    private static string BuildInvocationArguments(IEnumerable<IParameterSymbol> arguments) => string.Concat(
      arguments.Select((argument, index) => argument.RefKind == RefKind.In ? $", in arg{index}" : $", arg{index}"
      )
    );

    private static string GetTypeDisplayName(ITypeSymbol type) => type.ToDisplayString(
      SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)
    );

    // Builds the minimal "namespace { partial class Outer { partial class Inner { ... } } }"
    // wrapper needed to reopen the containing type(s), one file per [Composition] method.
    private static (string Header, string Footer) BuildContainingTypeWrapper(
      INamedTypeSymbol type,
      string methodName,
      out string hintName
    ) {
      var chain = new List<INamedTypeSymbol>();
      for (var t = type; t is not null; t = t.ContainingType) chain.Add(t);
      chain.Reverse(); // outermost first

      var ns = type.ContainingNamespace is { IsGlobalNamespace: false } n ? n.ToDisplayString() : null;

      var header = "// <auto-generated/>\n";
      var footer = "";
      var depth = 0;

      if (ns is not null) {
        header += "namespace " + ns + " {\n";
        depth = 1;
      }

      foreach (var t in chain) {
        header += new string(' ', depth * 2) + "partial " + KeywordFor(t) + " " + t.Name + " {\n";
        depth++;
      }

      for (var i = depth - 1; i >= 0; i--) footer += new string(' ', i * 2) + "}\n";

      var hintBase = (ns ?? "global") + "." + string.Join(".", chain.Select(t => t.Name)) + "." + methodName;
      hintName = Sanitize(hintBase) + ".g.cs";
      return (header, footer);
    }

    private static string KeywordFor(INamedTypeSymbol t) => t.TypeKind switch {
      TypeKind.Struct => "struct",
      TypeKind.Interface => "interface",
      _ => "class"
    };

    private static string Sanitize(string s) {
      var arr = s.ToCharArray();
      for (var i = 0; i < arr.Length; i++) {
        if (!char.IsLetterOrDigit(arr[i])) arr[i] = '_';
      }
      return new string(arr);
    }
  }
}
