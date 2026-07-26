using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HELIX.SourceGen {
  // For every method tagged [HELIX.NW.Composition], generates in the same partial class:
  //
  //   private static readonly ushort _myCompositionId = HELIX.NW.CompositionId.GetCompositionId();
  //   public static void MyComposition(this ref HELIX.NW.Composition cx) {
  //     cx.Metadata(_myCompositionId);
  //     _MyComposition(ref cx);
  //   }
  //
  // Requirements on the source method:
  //   - static
  //   - name starts with '_' (the '_' is stripped to make the public name)
  //   - first parameter: ref HELIX.NW.Composition
  //   - optional second parameter: T or in T
  // Anything else is a hard error (HLX001-HLX004) reported on the method.
  [Generator(LanguageNames.CSharp)]
  public sealed class CompositionGenerator : IIncrementalGenerator {
    private const string AttributeMetadataName = "HELIX.NW.CompositionAttribute";
    private const string CompositionTypeName = "HELIX.NW.Composition";
    private const string CompositionIdTypeName = "HELIX.NW.CompositionId";
    private const string CompositionInternalsTypeName = "HELIX.NW.CompositionInternals";
    private const string CompositionTransferTypeName = "HELIX.NW.CompositionInternals.TransferData";
    private const string ComposableTypeName = "HELIX.NW.Composable";

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
      "Method '{0}' is marked [Composition] but must take 'ref HELIX.NW.Composition' and at most one additional value or 'in' parameter",
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
        spc.ReportDiagnostic(Diagnostic.Create(MustHaveSupportedParameters, loc, method.Name));
        return;
      }

      var publicName = method.Name.Substring(1);
      var fieldName = $"_{char.ToLowerInvariant(publicName[0])}{publicName.Substring(1)}Id";
      var argument = method.Parameters.Length == 2 ? method.Parameters[1] : null;
      var composableType = argument is null
        ? ComposableTypeName
        : $"{ComposableTypeName}<{GetTypeDisplayName(argument.Type)}>";
      var lambdaArgument = argument is null ? "" : $", {GetTypeDisplayName(argument.Type)} value";
      var invocationArgument = argument is null
        ? ""
        : argument.RefKind == RefKind.In ? ", in value" : ", value";

      var containing = BuildContainingTypeWrapper(method.ContainingType, method.Name, out var hintName);

      var body = $@"
    private static readonly ushort {fieldName} = {CompositionIdTypeName}.GetCompositionId();

    public static readonly {composableType} {publicName} = static (ref {CompositionTypeName} cx{lambdaArgument}) => {{
      var transfer = new {CompositionTransferTypeName}();
      {CompositionInternalsTypeName}.EnterComposition(ref cx, {fieldName}, ref transfer);
      try {{{method.Name}(ref cx{invocationArgument});}}
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

    private static bool HasSupportedParameters(System.Collections.Immutable.ImmutableArray<IParameterSymbol> parameters) {
      if (parameters.Length < 1 || parameters.Length > 2 || !IsRefComposition(parameters[0])) return false;
      return parameters.Length == 1 ||
             parameters[1].RefKind == RefKind.None ||
             parameters[1].RefKind == RefKind.In;
    }

    private static string GetTypeDisplayName(ITypeSymbol type) {
      return type.ToDisplayString(
        SymbolDisplayFormat.FullyQualifiedFormat
          .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)
      );
    }

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
