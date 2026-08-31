using Microsoft.CodeAnalysis;
using static Mixins.Roslyn.GeneratorStrings;

namespace Mixins.Roslyn;

internal static class GeneratorDiagnostics {
  private static DiagnosticDescriptor Error(string id, string title, string message) {
    return new DiagnosticDescriptor(
      id, title, message, DiagnosticCategory, DiagnosticSeverity.Error, true
    );
  }

  private static DiagnosticDescriptor Warning(string id, string title, string message) {
    return new DiagnosticDescriptor(
      id, title, message, DiagnosticCategory, DiagnosticSeverity.Warning, true
    );
  }

  private static DiagnosticDescriptor Info(string id, string title, string message) {
    return new DiagnosticDescriptor(
      id, title, message, DiagnosticCategory, DiagnosticSeverity.Info, true
    );
  }

  private static DiagnosticDescriptor Hidden(string id, string title, string message) {
    return new DiagnosticDescriptor(
      id, title, message, DiagnosticCategory, DiagnosticSeverity.Hidden, true,
      customTags: [WellKnownDiagnosticTags.NotConfigurable]
    );
  }

  internal static class Structure {
    internal static readonly DiagnosticDescriptor
      MustBePartial = Error(
        "HLXP00", "Prop struct must be partial",
        "Struct '{0}' is marked [Structure] but is not declared partial"
      ),
      ContainingTypeMustBePartial = Error(
        "HLXP01", "Containing type must be partial",
        "Struct '{0}' is marked [Structure], but containing type '{1}' is not declared partial"
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
      InvalidTarget = Error(
        "HLXM03", "Mixin target is invalid",
        "Mixin target '{0}' on class '{1}' is invalid: {2}"
      ),
      ExistingTarget = Error(
        "HLXM04", "Mixin target already exists",
        "Mixin target '{0}' on class '{1}' already has an implementation"
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
      ),
      ExpressionHint = Info(
        "HLXM13", "Mixin expression performance hint", "{0}"
      ),
      ContributionData = Hidden(
        "HLXM14", "Mixin contribution data", "{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}"
      ),
      InvalidLibraryImport = Error(
        "HLXM15", "Mixin library import is invalid",
        "Mixin library import on '{0}' is invalid: {1}"
      );
  }
}