using Microsoft.CodeAnalysis;
using static HELIX.SourceGen.GeneratorStrings;

namespace HELIX.SourceGen {
  internal static class GeneratorDiagnostics {
    private static DiagnosticDescriptor Error(string id, string title, string message) => new(
      id, title, message, DiagnosticCategory, DiagnosticSeverity.Error, true
    );

    private static DiagnosticDescriptor Warning(string id, string title, string message) => new(
      id, title, message, DiagnosticCategory, DiagnosticSeverity.Warning, true
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
        RequiredPropAfterOptionalProp = Error(
          "HLXP02", "Required prop must precede optional props",
          "Prop '{0}' has no [Prop] default but follows a prop with a default value"
        ),
        InvalidDefault = Error(
          "HLXP03", "Invalid prop default",
          "The [Prop] default on '{0}' is invalid: {1}"
        ),
        InvalidEqualitySyntax = Error(
          "HLXP04", "Invalid prop equality configuration",
          "The [Prop] on field '{0}' has invalid equality or hash-code syntax: {1}"
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
        ),
        InvalidName = Error(
          "HLXC06", "Boundary composable name is invalid",
          "Class '{0}' specifies '{1}' as its composable name, but it is not a valid C# identifier"
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
          "Type '{0}' is marked [ComposableProxy], but it or its containing type is generic"
        ),
        InvalidName = Error(
          "HLXCP02", "Composable proxy name is invalid",
          "Type '{0}' specifies '{1}' as its composable name, but it is not a valid C# identifier"
        ),
        InvalidKind = Error(
          "HLXCP03", "Composable proxy kind is invalid",
          "Type '{0}' specifies an unrecognized ComposableKind value"
        ),
        InvalidProp = Error(
          "HLXCP04", "Proxy prop configuration is invalid",
          "Prop '{0}' has an invalid [Prop]: {1}"
        ),
        MustBePartial = Error(
          "HLXCP05", "Composable proxy must be partial",
          "Type '{0}' is marked [ComposableProxy] but is not declared partial"
        ),
        ContainingTypeMustBePartial = Error(
          "HLXCP06", "Containing type must be partial",
          "Type '{0}' is marked [ComposableProxy], but containing type '{1}' is not declared partial"
        ),
        ClassMustBeVisualElement = Error(
          "HLXCP07", "Composable proxy class must be a VisualElement",
          "Class '{0}' is marked [ComposableProxy] but does not derive from UnityEngine.UIElements.VisualElement"
        ),
        InvalidMethod = Error(
          "HLXCP08", "Composable proxy method is invalid",
          "Method '{0}' defines [Prop] parameters but {1}"
        ),
        DuplicatePropName = Error(
          "HLXCP09", "Composable proxy prop name is duplicated",
          "Prop name '{0}' is defined more than once in composable proxy class '{1}'"
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

    internal static class Mixins {
      internal static readonly DiagnosticDescriptor
        MustBePartial = Error(
          "HLXM00", "Mixin target must be partial",
          "Class '{0}' has mixins enabled but is not declared partial"
        ),
        ContainingTypeMustBePartial = Error(
          "HLXM01", "Containing type must be partial",
          "Class '{0}' has mixins enabled, but containing type '{1}' is not declared partial"
        ),
        InvalidMixinMethod = Error(
          "HLXM02", "Mixin method is invalid",
          "Mixin method '{0}' is invalid: {1}"
        ),
        InvalidTarget = Error(
          "HLXM03", "Mixin target is invalid",
          "Mixin target '{0}' on class '{1}' is invalid: {2}"
        ),
        ExistingTarget = Error(
          "HLXM04", "Mixin target already exists",
          "Mixin target '{0}' on class '{1}' already has an implementation"
        ),
        InvalidVariable = Error(
          "HLXM05", "Mixin variable is invalid",
          "Mixin interface '{0}' declares an invalid variable: {1}"
        ),
        InvalidProperty = Error(
          "HLXM06", "Mixin property is invalid",
          "Mixin property '{0}' is invalid: {1}"
        ),
        NoMatchingVariant = Error(
          "HLXM07", "No mixin proxy variant matches",
          "Attribute '{0}' has no compatible mixin method variant for '{1}': {2}"
        ),
        InvalidAttributeExpression = Error(
          "HLXM08", "Mixin expression is invalid",
          "Mixin expression '{0}' on '{1}' is invalid: {2}"
        ),
        MissingRequiredMixin = Error(
          "HLXM09", "Required mixin is missing",
          "Mixin '{0}' on class '{1}' requires class-level mixin '{2}'"
        ),
        InvalidRequiredMixin = Error(
          "HLXM10", "Required mixin declaration is invalid",
          "Mixin '{0}' on class '{1}' has an invalid requirement: {2}"
        ),
        InvalidPreparedExpression = Error(
          "HLXM11", "Prepared mixin expression is invalid",
          "Prepared mixin expression from '{0}' is invalid at line {1}: {2}"
        ),
        ExpressionLog = Warning(
          "HLXM12", "Mixin expression log", "{0}"
        );
    }
  }
}
