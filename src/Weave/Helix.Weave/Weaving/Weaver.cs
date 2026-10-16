using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Helix.Weave;

/// <summary>
/// Builds the methods that injection contributions are woven into. Ordering is by priority,
/// then by insertion sequence — the tie-break is part of the contract, not an accident.
/// </summary>
public static class Weaver {
  public static IReadOnlyList<string> BuildMethods(WeaveSession session) {
    var pool = session.Pool;
    var grouped = new Dictionary<string, List<MixinOutput>>(System.StringComparer.Ordinal);

    foreach (var output in session.Outputs.Outputs) {
      if (output.Target != EmissionTarget.Injection) continue;
      var name = pool.Resolve(output.InjectionTarget);
      if (name is null) continue;
      if (!grouped.TryGetValue(name, out var list)) grouped.Add(name, list = new List<MixinOutput>());
      list.Add(output);
    }

    var methods = new List<string>();
    foreach (var name in grouped.Keys.OrderBy(static key => key, System.StringComparer.Ordinal)) {
      var target = session.Targets.TryGetValue(name, out var declared) ? declared : new InjectionTarget(name);
      methods.Add(BuildMethod(session, target, grouped[name]));
    }
    return methods;
  }

  private static string BuildMethod(WeaveSession session, InjectionTarget target, List<MixinOutput> contributions) {
    contributions.Sort(static (left, right) => {
      var compared = left.Priority.CompareTo(right.Priority);
      return compared != 0 ? compared : left.Sequence.CompareTo(right.Sequence);
    });

    var existing = FindMethod(session.Type, target.Name);
    var signature = Signature(session, target, existing);
    var builder = new SourceBuilder();

    using (builder.Braces(signature.Declaration)) {
      if (signature.BaseCall is not null) builder.Line(signature.BaseCall);
      foreach (var contribution in contributions) {
        var text = session.Pool.Resolve(contribution.Text);
        builder.Block(EnsureTerminated(text));
      }
    }
    return builder.ToString().TrimEnd('\n');
  }

  /// <summary>
  /// Adds the semicolon a bare expression needs. Contributions are often deliberate fragments —
  /// a lone `try {`, a comment, a label — so only terminate what actually looks unterminated.
  /// </summary>
  private static string EnsureTerminated(string statement) {
    var trimmed = statement.TrimEnd();
    if (trimmed.Length == 0) return trimmed;

    var lastLine = trimmed.Substring(trimmed.LastIndexOf('\n') + 1).Trim();
    if (lastLine.StartsWith("//", System.StringComparison.Ordinal)) return trimmed;

    var last = trimmed[trimmed.Length - 1];
    return last is ';' or '}' or '{' or ':' ? trimmed : trimmed + ";";
  }

  private static IMethodSymbol FindMethod(INamedTypeSymbol type, string name) {
    for (var current = type; current is not null; current = current.BaseType)
      foreach (var member in current.GetMembers(name))
        if (member is IMethodSymbol method) return method;
    return null;
  }

  private readonly struct MethodSignature {
    public MethodSignature(string declaration, string baseCall) {
      Declaration = declaration;
      BaseCall = baseCall;
    }

    public string Declaration { get; }
    public string BaseCall { get; }
  }

  private static MethodSignature Signature(WeaveSession session, InjectionTarget target, IMethodSymbol existing) {
    var facts = session.Facts;
    var modifiers = (target.IsPublic ? "public" : "private") + (target.IsStatic ? " static" : "");

    // A partial declaration in the user's own type wins: we implement it.
    if (existing is { IsPartialDefinition: true })
      return new MethodSignature(DeclarationFrom(facts, existing, "partial"), null);

    // An inherited virtual or abstract method: override and chain.
    if (existing is not null && !SymbolEqualityComparer.Default.Equals(existing.ContainingType, session.Type) &&
        (existing.IsVirtual || existing.IsAbstract || existing.IsOverride)) {
      var call = existing.IsAbstract
        ? null
        : "base." + existing.Name + "(" + string.Join(", ", existing.Parameters.Select(ArgumentOf)) + ");";
      return new MethodSignature(DeclarationFrom(facts, existing, "override"), call);
    }

    var parameters = DelegateParameters(session, target);
    return new MethodSignature(modifiers + " void " + target.Name + "(" + parameters + ")", null);
  }

  private static string ArgumentOf(IParameterSymbol parameter) =>
    RefPrefix(parameter.RefKind) + parameter.Name;

  private static string DeclarationFrom(SymbolFacts facts, IMethodSymbol method, string modifier) {
    var accessibility = method.DeclaredAccessibility switch {
      Accessibility.Public => "public ",
      Accessibility.Internal => "internal ",
      Accessibility.Protected => "protected ",
      _ => modifier == "partial" ? "" : "private ",
    };
    var statics = method.IsStatic ? "static " : "";
    var keyword = modifier == "partial" ? "partial " : "override ";
    var parameters = string.Join(", ", method.Parameters.Select(p =>
      RefPrefix(p.RefKind) + facts.Qualified(p.Type) + " " + p.Name));
    return accessibility + statics + keyword + facts.Qualified(method.ReturnType) + " " + method.Name +
           "(" + parameters + ")";
  }

  /// <summary>
  /// Parameters taken from the target's delegate type, so a generated method matches the
  /// signature the host expects to call it through.
  /// </summary>
  private static string DelegateParameters(WeaveSession session, InjectionTarget target) {
    if (target.DelegateType is null) return "";
    var resolved = session.Facts.Lookup(session.Compilation, target.DelegateType);
    if (resolved is not { TypeKind: TypeKind.Delegate }) return "";
    var invoke = resolved.DelegateInvokeMethod;
    if (invoke is null) return "";
    return string.Join(", ", invoke.Parameters.Select(p =>
      RefPrefix(p.RefKind) + session.Facts.Qualified(p.Type) + " " + p.Name));
  }

  private static string RefPrefix(RefKind kind) => kind switch {
    RefKind.Ref => "ref ",
    RefKind.Out => "out ",
    RefKind.In => "in ",
    _ => "",
  };
}
