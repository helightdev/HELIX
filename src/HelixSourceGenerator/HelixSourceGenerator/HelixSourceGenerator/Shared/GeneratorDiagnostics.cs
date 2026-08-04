using Microsoft.CodeAnalysis;
using static HELIX.SourceGen.GeneratorStrings;

namespace HELIX.SourceGen {
  internal static class GeneratorDiagnostics {
    private static DiagnosticDescriptor Error(string id, string title, string message) => new(
      id, title, message, DiagnosticCategory, DiagnosticSeverity.Error, true
    );

    internal static class PropStruct {
      internal static readonly DiagnosticDescriptor
        MustBePartial = Error(
          "HLXP00", "Prop struct must be partial",
          "Struct '{0}' is marked [PropStruct] but is not declared partial"
        ),
        ContainingTypeMustBePartial = Error(
          "HLXP01", "Containing type must be partial",
          "Struct '{0}' is marked [PropStruct], but containing type '{1}' is not declared partial"
        ),
        RequiredFieldAfterOptionalField = Error(
          "HLXP02", "Required prop must precede optional props",
          "Field '{0}' has no [PropDefault] but follows a field with a default value"
        ),
        InvalidDefault = Error(
          "HLXP03", "Invalid prop default",
          "The [PropDefault] on field '{0}' is invalid: {1}"
        );
    }

    internal static class BoundaryComposable {
      internal static readonly DiagnosticDescriptor
        MustBePartial = Error(
          "HLXC00", "Boundary composable must be partial",
          "Class '{0}' is marked [BoundaryComposable] but is not declared partial"
        ),
        InvalidProps = Error(
          "HLXC02", "Boundary composable Props must be a partial struct",
          "Nested type '{0}.Props' must be a partial struct"
        ),
        InvalidBaseType = Error(
          "HLXC03", "Boundary composable Base is invalid",
          "Class '{0}' specifies Base = '{1}', but {2}"
        ),
        GenericNotSupported = Error(
          "HLXC04", "Generic boundary composables are not supported",
          "Class '{0}' is marked [BoundaryComposable] but is generic"
        ),
        ContainingTypeMustBePartial = Error(
          "HLXC05", "Containing type must be partial",
          "Class '{0}' is marked [BoundaryComposable], but containing type '{1}' is not declared partial"
        );
    }

    internal static class ComposableProxy {
      internal static readonly DiagnosticDescriptor
        MissingTarget = Error(
          "HLXCP00", "Composable proxy target is required",
          "Struct '{0}' is marked [ComposableProxy] but does not specify a Target type"
        ),
        GenericNotSupported = Error(
          "HLXCP01", "Generic composable proxies are not supported",
          "Struct '{0}' is marked [ComposableProxy], but it or its containing type is generic"
        ),
        InvalidName = Error(
          "HLXCP02", "Composable proxy name is invalid",
          "Struct '{0}' specifies '{1}' as its composable name, but it is not a valid C# identifier"
        ),
        InvalidKind = Error(
          "HLXCP03", "Composable proxy kind is invalid",
          "Struct '{0}' specifies an unrecognized ComposableKind value"
        ),
        InvalidPropProxy = Error(
          "HLXCP04", "Proxy prop configuration is invalid",
          "Field '{0}' has an invalid [PropProxy]: {1}"
        ),
        MustBePartial = Error(
          "HLXCP05", "Composable proxy must be partial",
          "Struct '{0}' is marked [ComposableProxy] but is not declared partial"
        ),
        ContainingTypeMustBePartial = Error(
          "HLXCP06", "Containing type must be partial",
          "Struct '{0}' is marked [ComposableProxy], but containing type '{1}' is not declared partial"
        );
    }

    internal static class Composition {
      internal static readonly DiagnosticDescriptor
        MustBeStatic = Error(
          "HLX001", "Composition method must be static",
          "Method '{0}' is marked [Composition] but is not static"
        ),
        MustStartWithUnderscore = Error(
          "HLX002", "Composition method must start with '_'",
          "Method '{0}' is marked [Composition] but does not start with '_'"
        ),
        MustHaveSupportedParameters = Error(
          "HLX003", "Composition method has unsupported parameters",
          "Method '{0}' is marked [Composition] but must take 'ref HELIX.NW.Composition' and at most {1} additional value or 'in' parameters"
        ),
        MustNotBeGeneric = Error(
          "HLX004", "Composition method must not be generic",
          "Method '{0}' is marked [Composition] but is generic, which is not supported"
        );
    }
  }
}