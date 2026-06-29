using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HELIX.SourceGen {
  // For every partial method tagged [HELIX.NW.CompositionBoundary], generates in the same
  // partial class:
  //
  //   private static readonly ushort _buttonId     = HELIX.NW.CompositionId.GetCompositionId();
  //   private static readonly ushort _buttonTypeId = HELIX.NW.CompositionId.GetTypeId();
  //
  //   public static partial void Button(ref this HELIX.NW.Composition cx, string label, Action<IBoundary> action) {
  //     cx.AUTHORING.PropsBoundaryStateNode<ButtonState, ButtonProps>(_buttonTypeId, out var node, out _, out var attachment);
  //     var props = new ButtonProps { Label = label, Action = action };
  //     attachment.ReceiveProps(props);
  //     node.composable = null;
  //     cx.AUTHORING.YieldBoundary(ref cx, node);
  //   }
  //
  //   public struct ButtonProps { /* one prop per extra parameter */ }
  //
  //   partial class ButtonState {
  //     public override void OnRecompose(ref Composition cx, NodeState state, IBoundary boundary) {
  //       var transfer = new CompositionInternals.TransferData();
  //       CompositionInternals.EnterComposition(ref cx, _buttonId, ref transfer);
  //       try { base.OnRecompose(ref cx, state, boundary); }
  //       finally { CompositionInternals.ExitComposition(ref cx, ref transfer); }
  //     }
  //   }
  //
  // Requirements on the source method:
  //   - static, partial
  //   - name does NOT start with '_' (it is the public name, e.g. 'Button')
  //   - first parameter: 'ref this HELIX.NW.Composition' (extension receiver)
  //   - not generic
  //   - the containing type must declare a nested partial type named '{Name}State'
  // Anything else is a hard error (HLX010-HLX015) reported on the method.
  [Generator(LanguageNames.CSharp)]
  public sealed class CompositionBoundaryGenerator : IIncrementalGenerator {
    private const string AttributeMetadataName = "HELIX.NW.CompositionBoundaryAttribute";
    private const string CompositionTypeName = "HELIX.NW.Composition";
    private const string CompositionIdTypeName = "HELIX.NW.CompositionId";
    private const string NodeStateTypeName = "HELIX.NW.NodeState";
    private const string IBoundaryTypeName = "HELIX.NW.IBoundary";
    private const string CompositionInternalsTypeName = "HELIX.NW.CompositionInternals";
    private const string CompositionTransferTypeName = "HELIX.NW.CompositionInternals.TransferData";
    private const string PropsBaseTypeName = "HELIX.NW.PropsNodeStateAttachmentBase";
    private const string ContextAttributeName = "HELIX.NW.ContextAttribute";

    private static readonly DiagnosticDescriptor MustBeStatic = new DiagnosticDescriptor(
      "HLX010",
      "CompositionBoundary method must be static",
      "Method '{0}' is marked [CompositionBoundary] but is not static",
      "HELIX",
      DiagnosticSeverity.Error,
      true
    );

    private static readonly DiagnosticDescriptor MustBePartial = new DiagnosticDescriptor(
      "HLX011",
      "CompositionBoundary method must be partial",
      "Method '{0}' is marked [CompositionBoundary] but is not declared 'partial'",
      "HELIX",
      DiagnosticSeverity.Error,
      true
    );

    private static readonly DiagnosticDescriptor MustNotStartWithUnderscore = new DiagnosticDescriptor(
      "HLX012",
      "CompositionBoundary method must not start with '_'",
      "Method '{0}' is marked [CompositionBoundary] but starts with '_'; the name is used directly as the public boundary name",
      "HELIX",
      DiagnosticSeverity.Error,
      true
    );

    private static readonly DiagnosticDescriptor MustTakeRefThisComposition = new DiagnosticDescriptor(
      "HLX013",
      "CompositionBoundary method must take 'ref this Composition' as its first parameter",
      "Method '{0}' is marked [CompositionBoundary] but its first parameter must be 'ref this HELIX.NW.Composition'",
      "HELIX",
      DiagnosticSeverity.Error,
      true
    );

    private static readonly DiagnosticDescriptor MustNotBeGeneric = new DiagnosticDescriptor(
      "HLX014",
      "CompositionBoundary method must not be generic",
      "Method '{0}' is marked [CompositionBoundary] but is generic, which is not supported",
      "HELIX",
      DiagnosticSeverity.Error,
      true
    );

    private static readonly DiagnosticDescriptor MissingStateType = new DiagnosticDescriptor(
      "HLX015",
      "CompositionBoundary requires a nested '{Name}State' partial type",
      "Method '{0}' is marked [CompositionBoundary] but its containing type does not declare a nested partial type named '{1}'",
      "HELIX",
      DiagnosticSeverity.Error,
      true
    );

    private static readonly DiagnosticDescriptor ContainingTypeMustBeStaticPartialClass = new DiagnosticDescriptor(
      "HLX016",
      "CompositionBoundary must be declared in a static partial class",
      "Method '{0}' is marked [CompositionBoundary] but its containing type '{1}' is not a static partial class",
      "HELIX",
      DiagnosticSeverity.Error,
      true
    );

    private static readonly DiagnosticDescriptor StateMustBeClass = new DiagnosticDescriptor(
      "HLX017",
      "CompositionBoundary state type must be a class",
      "Method '{0}' is marked [CompositionBoundary] but '{1}' must be a class inheriting PropsNodeStateAttachmentBase; it is not a class",
      "HELIX",
      DiagnosticSeverity.Error,
      true
    );

    private static readonly DiagnosticDescriptor InvalidBaseType = new DiagnosticDescriptor(
      "HLX018",
      "CompositionBoundary Base must be an unbound generic type of arity 1",
      "Method '{0}' specifies Base = '{1}', but Base must be an unbound generic type with exactly one type parameter (e.g. typeof(InputStateBase<>))",
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

      if (!IsPartial(method)) {
        spc.ReportDiagnostic(Diagnostic.Create(MustBePartial, loc, method.Name));
        return;
      }

      if (method.Name.StartsWith("_", StringComparison.Ordinal)) {
        spc.ReportDiagnostic(Diagnostic.Create(MustNotStartWithUnderscore, loc, method.Name));
        return;
      }

      if (!method.IsExtensionMethod ||
          method.Parameters.Length < 1 ||
          !IsRefComposition(method.Parameters[0])) {
        spc.ReportDiagnostic(Diagnostic.Create(MustTakeRefThisComposition, loc, method.Name));
        return;
      }

      var containingType = method.ContainingType;
      if (containingType.TypeKind != TypeKind.Class ||
          !containingType.IsStatic ||
          !IsPartialType(containingType)) {
        spc.ReportDiagnostic(Diagnostic.Create(
          ContainingTypeMustBeStaticPartialClass, loc, method.Name, containingType.Name));
        return;
      }

      var publicName = method.Name;            // e.g. "Button"
      var stateName = publicName + "State";    // e.g. "ButtonState"
      var propsName = publicName + "Props";    // e.g. "ButtonProps"

      var stateType = containingType
        .GetTypeMembers(stateName)
        .FirstOrDefault(t => t.Arity == 0);

      if (stateType is null) {
        spc.ReportDiagnostic(Diagnostic.Create(MissingStateType, loc, method.Name, stateName));
        return;
      }

      if (stateType.TypeKind != TypeKind.Class) {
        spc.ReportDiagnostic(Diagnostic.Create(StateMustBeClass, loc, method.Name, stateName));
        return;
      }

      // Resolve the optional Base = typeof(Foo<>) named argument. When present it must be
      // an unbound generic of arity 1; we close it over the generated props struct.
      // When absent, fall back to the default PropsNodeStateAttachmentBase<{Props}>.
      string baseOpen; // fully-qualified, generic name without the type argument
      var baseArg = GetBaseTypeArgument(method);
      if (baseArg is null) {
        baseOpen = PropsBaseTypeName;
      } else {
        if (baseArg is not INamedTypeSymbol nb ||
            !nb.IsUnboundGenericType ||
            nb.TypeParameters.Length != 1) {
          spc.ReportDiagnostic(Diagnostic.Create(
            InvalidBaseType, loc, method.Name, baseArg.ToDisplayString()));
          return;
        }
        // ConstructedFrom drops the unbound '<>' marker so we can re-close it ourselves.
        baseOpen = nb.ConstructedFrom.ToDisplayString(
          SymbolDisplayFormat.FullyQualifiedFormat
            .WithGenericsOptions(SymbolDisplayGenericsOptions.None));
      }

      var lower = char.ToLowerInvariant(publicName[0]) + publicName.Substring(1);
      var idField = $"_{lower}Id";
      var typeIdField = $"_{lower}TypeId";

      // Gather [Context]-marked fields on the state type; each gets a Pull() call injected
      // into OnRecompose before base.OnRecompose runs. Initialization/reset is the user's
      // responsibility via the field initializer.
      var pullLines = new StringBuilder();
      foreach (var member in stateType.GetMembers()) {
        if (member is not IFieldSymbol field) continue;
        if (!field.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == ContextAttributeName)) continue;
        pullLines.Append($"          {field.Name}.Pull();\n");
      }

      var receiverName = method.Parameters[0].Name; // the 'cx' name
      var extraParams = method.Parameters.Skip(1).ToArray();

      // Build the props struct members + the props initializer, mapping each extra
      // parameter to a property whose name is the PascalCased parameter name.
      var propsMembers = new StringBuilder();
      var propsInit = new StringBuilder();
      foreach (var p in extraParams) {
        var propName = char.ToUpperInvariant(p.Name[0]) + p.Name.Substring(1);
        var typeStr = p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        propsMembers.Append($"      public {typeStr} {propName} {{ get; set; }}\n");
        propsInit.Append($"        {propName} = {p.Name},\n");
      }

      // Reconstruct the partial method signature so the generated implementation
      // matches the declaration (including 'ref this', modifiers).
      var signatureParams = BuildSignatureParameters(method.Parameters, method.IsExtensionMethod);

      // The declaration may return void or 'ref ElementRef' (or any by-ref type). Mirror it,
      // and in the ref case return the YieldBoundary result by reference.
      string returnTypeText;
      string yieldStatement;
      if (method.ReturnsVoid) {
        returnTypeText = "void";
        yieldStatement = $"{receiverName}.AUTHORING.YieldBoundary(ref {receiverName}, node);";
      } else {
        var refPrefix = method.ReturnsByRef ? "ref " :
                        method.ReturnsByRefReadonly ? "ref readonly " : "";
        var retType = method.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        returnTypeText = refPrefix + retType;
        var returnRef = method.ReturnsByRef || method.ReturnsByRefReadonly ? "ref " : "";
        yieldStatement = $"return {returnRef}{receiverName}.AUTHORING.YieldBoundary(ref {receiverName}, node);";
      }

      var containing = BuildContainingTypeWrapper(method.ContainingType, method.Name, out var hintName);

      var body = $@"
    private static readonly ushort {idField} = {CompositionIdTypeName}.GetCompositionId();
    private static readonly ushort {typeIdField} = {CompositionIdTypeName}.GetTypeId();

    public static partial {returnTypeText} {publicName}({signatureParams}) {{
      {receiverName}.AUTHORING.PropsBoundaryStateNode<{stateName}, {propsName}>({typeIdField}, out var node, out _, out var attachment);
      var props = new {propsName} {{
{TrimTrailingComma(propsInit.ToString())}      }};
      attachment.ReceiveProps(props);
      node.composable = null;
      {yieldStatement}
    }}

    public struct {propsName} {{
{propsMembers}    }}

    partial class {stateName} : {baseOpen}<{propsName}> {{
      public override void OnRecompose(ref global::{CompositionTypeName} cx, global::{NodeStateTypeName} state, global::{IBoundaryTypeName} boundary) {{
        var transfer = new {CompositionTransferTypeName}();
        {CompositionInternalsTypeName}.EnterComposition(ref cx, {idField}, ref transfer);
        try {{
{pullLines}          base.OnRecompose(ref cx, state, boundary);
        }}
        finally {{ {CompositionInternalsTypeName}.ExitComposition(ref cx, ref transfer); }}
      }}
    }}
";

      var source = containing.Header + body + containing.Footer;
      spc.AddSource(hintName, source);
    }

    private static string TrimTrailingComma(string init) {
      // init lines end in ",\n"; that's valid C# for object initializers, so leave as-is.
      return init;
    }

    private static bool IsPartial(IMethodSymbol method) {
      foreach (var r in method.DeclaringSyntaxReferences) {
        if (r.GetSyntax() is MethodDeclarationSyntax m &&
            m.Modifiers.Any(SyntaxKind.PartialKeyword)) {
          return true;
        }
      }
      return false;
    }

    private static bool IsRefComposition(IParameterSymbol p) {
      if (p.RefKind != RefKind.Ref) return false;
      var displayName = p.Type.ToDisplayString(
        SymbolDisplayFormat.FullyQualifiedFormat
          .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)
      );
      return displayName == CompositionTypeName;
    }

    // Rebuilds the parameter list text for the partial method implementation, preserving
    // 'ref this Composition', ref/out/in modifiers, fully-qualified types and default values.
    private static string BuildSignatureParameters(
      IEnumerable<IParameterSymbol> parameters,
      bool isExtension
    ) {
      var parts = new List<string>();
      var first = true;
      foreach (var p in parameters) {
        var sb = new StringBuilder();

        if (first && isExtension) {
          // extension receiver: 'ref this Composition cx'
          sb.Append(RefKindPrefix(p.RefKind));
          sb.Append("this ");
        } else {
          sb.Append(RefKindPrefix(p.RefKind));
        }

        sb.Append(p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
        sb.Append(' ');
        sb.Append(p.Name);

        parts.Add(sb.ToString());
        first = false;
      }
      return string.Join(", ", parts);
    }

    private static string RefKindPrefix(RefKind kind) => kind switch {
      RefKind.Ref => "ref ",
      RefKind.Out => "out ",
      RefKind.In => "in ",
      _ => ""
    };

    private static ITypeSymbol GetBaseTypeArgument(IMethodSymbol method) {
      foreach (var attr in method.GetAttributes()) {
        if (attr.AttributeClass?.ToDisplayString() != AttributeMetadataName) continue;
        foreach (var named in attr.NamedArguments) {
          if (named.Key == "Base" && named.Value.Value is ITypeSymbol t) {
            return t;
          }
        }
      }
      return null;
    }

    private static bool IsPartialType(INamedTypeSymbol type) {
      foreach (var r in type.DeclaringSyntaxReferences) {
        if (r.GetSyntax() is TypeDeclarationSyntax t &&
            t.Modifiers.Any(SyntaxKind.PartialKeyword)) {
          return true;
        }
      }
      return false;
    }

    // Builds the minimal "namespace { partial class Outer { partial class Inner { ... } } }"
    // wrapper needed to reopen the containing type(s), one file per [CompositionBoundary] method.
    // The containing type is guaranteed to be a top-level static partial class
    // (enforced by HLX016 — an extension method could not compile otherwise), so we
    // just reopen that single class, optionally inside its namespace.
    private static (string Header, string Footer) BuildContainingTypeWrapper(
      INamedTypeSymbol type,
      string methodName,
      out string hintName
    ) {
      var ns = type.ContainingNamespace is { IsGlobalNamespace: false } n ? n.ToDisplayString() : null;

      var header = "// <auto-generated/>\n";
      var footer = "";
      var depth = 0;

      if (ns is not null) {
        header += "namespace " + ns + " {\n";
        depth = 1;
      }

      header += new string(' ', depth * 2) + "static partial class " + type.Name + " {\n";
      depth++;

      for (var i = depth - 1; i >= 0; i--) footer += new string(' ', i * 2) + "}\n";

      var hintBase = (ns ?? "global") + "." + type.Name + "." + methodName + ".boundary";
      hintName = Sanitize(hintBase) + ".g.cs";
      return (header, footer);
    }


    private static string Sanitize(string s) {
      var arr = s.ToCharArray();
      for (var i = 0; i < arr.Length; i++) {
        if (!char.IsLetterOrDigit(arr[i])) arr[i] = '_';
      }
      return new string(arr);
    }
  }
}