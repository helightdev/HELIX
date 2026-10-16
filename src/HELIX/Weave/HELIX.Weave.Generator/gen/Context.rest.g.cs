// Port of Context.hix: dependency injection, resources, tickers, event handlers and modules.
using System;
using System.Linq;
using Helix.Weave;

namespace HELIX.Weave;

/// <summary>Where a resource is loaded from. Mirrors the numeric source in Context.hix.</summary>
public enum ResourceSource {
  None = 0,
  Inline = 1,
  Addressables = 2,
  Resources = 3,
}

/// <summary>
/// Hix's <c>:floatTime</c>: a bare number is a frame count, a suffixed one a duration.
/// </summary>
public static class TimeSpec {
  public static bool TryParse(string text, out int? frames, out float seconds) {
    frames = null;
    seconds = 0f;
    if (string.IsNullOrWhiteSpace(text)) return false;

    var value = text.Trim();
    // Rate 0 is FrameCountTicker's "every frame" short-circuit, which is what tick means.
    if (value == "tick") { frames = 0; return true; }

    var multiplier = 1f;
    if (value.EndsWith("ms", StringComparison.OrdinalIgnoreCase)) {
      multiplier = 0.001f;
      value = value.Substring(0, value.Length - 2);
    } else if (value.EndsWith("s", StringComparison.OrdinalIgnoreCase)) {
      value = value.Substring(0, value.Length - 1);
    } else if (value.EndsWith("f", StringComparison.OrdinalIgnoreCase)) {
      value = value.Substring(0, value.Length - 1);
    } else {
      // No suffix: a frame count.
      if (!int.TryParse(value, System.Globalization.NumberStyles.Integer,
                        System.Globalization.CultureInfo.InvariantCulture, out var count)) return false;
      frames = count;
      return true;
    }

    if (!float.TryParse(value, System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out var parsed)) return false;
    seconds = parsed * multiplier;
    return true;
  }
}

/// <summary>
/// Module discovery, generated from compilation symbols without runtime reflection.
/// Port of <c>ConfigureModule</c> plus the two module mixins.
/// </summary>
public static class Modules {
  public static void Configure(TypeTarget t, string filter, PureTypeRef[] imports) {
    if (t.IsGeneric) return;

    // Sorted by qualified name: registration order is a tie-breaker during loading, so it has
    // to be stable rather than whatever order the symbol walk happened to take.
    var registrations = Code.Join(
      t.AnnotatedTypes("HELIX.Context.ManagedAttribute")
        .Select(type => Registration(t, type, filter))
        .Where(code => code.Length > 0)
        .OrderBy(code => code, StringComparer.Ordinal),
      "\n");

    var importCode = Code.Join(
      (imports ?? Array.Empty<PureTypeRef>()).Where(import => import.Exists),
      import => $"{import.Qualified}.Instance.Discover(registrations);", "\n");

    t.Implements("global::HELIX.Context.IHelixModule");
    t.Class($$"""
      public static readonly {{t.TypeRef}} Instance = new {{t.TypeRef}}();

      public void Discover(global::HELIX.Context.ManagedRegistrations registrations) {
        {{importCode}}
        {{registrations}}
      }
      """);
  }

  private static string Registration(TypeTarget t, Microsoft.CodeAnalysis.INamedTypeSymbol type,
                                     string filter) {
    if (type.TypeKind != Microsoft.CodeAnalysis.TypeKind.Class || type.IsGenericType) return "";
    if (type.DeclaredAccessibility is not (Microsoft.CodeAnalysis.Accessibility.Public
                                           or Microsoft.CodeAnalysis.Accessibility.Internal)) return "";

    if (!string.IsNullOrEmpty(filter)) {
      var ns = type.ContainingNamespace is { IsGlobalNamespace: false } containing
        ? containing.ToDisplayString()
        : "";
      if (ns != filter && !ns.StartsWith(filter + ".", StringComparison.Ordinal)) return "";
    }

    var qualified = t.Facts.Qualified(type);
    return $"registrations.Register(typeof({qualified}), {qualified}.RegistrationConfigurator);";
  }
}
