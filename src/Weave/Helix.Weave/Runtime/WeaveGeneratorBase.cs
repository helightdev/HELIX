using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Helix.Weave;

/// <summary>
/// The incremental generator. Lowering generates a subclass that supplies the mixin list;
/// everything else is here.
/// </summary>
public abstract class WeaveGeneratorBase : IIncrementalGenerator {
  protected abstract IReadOnlyList<Mixin> Mixins { get; }

  public void Initialize(IncrementalGeneratorInitializationContext context) {
    var mixins = Mixins;
    IncrementalValueProvider<ImmutableArray<AnnotatedSymbol>> all = default;
    var first = true;

    foreach (var mixin in mixins) {
      // Derivation steps are consulted from inside other mixins' preludes, not triggered here.
      if (mixin.Unit == Unit.Derivation) continue;
      var captured = mixin;
      var matches = context.SyntaxProvider.ForAttributeWithMetadataName(
          captured.AttributeName,
          predicate: static (node, _) => true,
          transform: (ctx, _) => new AnnotatedSymbol(captured.AttributeName, ctx.TargetSymbol, ctx.Attributes))
        .Where(static item => item.Symbol is not null)
        .Collect();

      all = first ? matches : Merge(all, matches);
      first = false;
    }

    if (first) return;

    var source = all.Combine(context.CompilationProvider);
    context.RegisterSourceOutput(source, (spc, pair) => Execute(spc, pair.Right, pair.Left, mixins));
  }

  private static IncrementalValueProvider<ImmutableArray<AnnotatedSymbol>> Merge(
    IncrementalValueProvider<ImmutableArray<AnnotatedSymbol>> left,
    IncrementalValueProvider<ImmutableArray<AnnotatedSymbol>> right) =>
    left.Combine(right).Select(static (pair, _) => pair.Left.AddRange(pair.Right));

  private static void Execute(SourceProductionContext context, Compilation compilation,
                              ImmutableArray<AnnotatedSymbol> symbols, IReadOnlyList<Mixin> mixins) {
    if (symbols.IsDefaultOrEmpty) return;

    var byName = new Dictionary<string, List<Mixin>>(System.StringComparer.Ordinal);
    foreach (var mixin in mixins) {
      if (!byName.TryGetValue(mixin.AttributeName, out var list))
        byName.Add(mixin.AttributeName, list = new List<Mixin>());
      list.Add(mixin);
    }

    var pool = new StringPool();
    var facts = new SymbolFacts(pool);
    var derivations = new DerivationPipeline(mixins);

    // ponytail: one session per containing type, rebuilt per run. Per-type incremental caching
    // is the next step up if generation time ever shows on a profile.
    var sessions = new Dictionary<INamedTypeSymbol, WeaveSession>(SymbolEqualityComparer.Default);
    var late = new List<(Mixin Mixin, WeaveSession Session, ISymbol Symbol, object Carry)>();

    foreach (var annotated in symbols.OrderBy(static item => item.SortKey, System.StringComparer.Ordinal)) {
      var containing = ContainingType(annotated.Symbol);
      if (containing is null) continue;
      if (!sessions.TryGetValue(containing, out var session))
        sessions.Add(containing, session = new WeaveSession(containing, facts, compilation, derivations));

      if (!byName.TryGetValue(annotated.AttributeName, out var candidates)) continue;
      foreach (var mixin in candidates.OrderBy(static item => item.Order)) {
        var target = CreateTarget(session, annotated.Symbol, mixin.Unit);
        if (target is null) {
          session.Report(new WeaveDiagnostic("WEAVE001",
            $"mixin for {mixin.AttributeName} expects a {mixin.Unit} but was applied to {annotated.Symbol.Kind}",
            annotated.Symbol.Locations.FirstOrDefault(), DiagnosticSeverity.Error));
          continue;
        }

        var attribute = annotated.Attributes.FirstOrDefault();
        object carry;
        try {
          carry = mixin.Prelude(target, attribute);
        } catch (System.Exception exception) {
          // A mixin that throws takes down the whole generator run otherwise, and the driver
          // reports only the exception type. Name the mixin and the symbol instead.
          session.Report(Failure(mixin, annotated.Symbol, "prelude", exception));
          continue;
        }
        if (mixin.HasLate) late.Add((mixin, session, annotated.Symbol, carry));
      }
    }

    // The late pass sees only its carry: no symbols, no attribute, nothing analyzed-source-backed.
    foreach (var work in late) {
      try {
        work.Mixin.Late(new LateTarget(work.Session, work.Symbol), work.Carry);
      } catch (System.Exception exception) {
        work.Session.Report(Failure(work.Mixin, work.Symbol, "late pass", exception));
      }
    }

    foreach (var pair in sessions) {
      var session = pair.Value;
      foreach (var diagnostic in session.Diagnostics) context.ReportDiagnostic(diagnostic.ToDiagnostic());
      if (session.Outputs.Count == 0) continue;

      // Generated members land in a partial declaration, so there has to be one to join.
      // A warning rather than an error: it keeps a project that has such a type compiling.
      if (!IsPartialAllTheWayOut(pair.Key)) {
        context.ReportDiagnostic(new WeaveDiagnostic("WEAVE003",
          $"'{pair.Key.Name}' is not declared partial, so nothing was generated for it",
          pair.Key.Locations.FirstOrDefault(), DiagnosticSeverity.Warning).ToDiagnostic());
        continue;
      }

      context.AddSource(FileNameFor(pair.Key), Renderer.Render(session));
    }
  }

  private static WeaveDiagnostic Failure(Mixin mixin, ISymbol symbol, string pass, System.Exception exception) =>
    new("WEAVE000",
        $"the {pass} of {mixin.GetType().FullName} threw on '{symbol?.Name}': " +
        exception.GetType().Name + ": " + exception.Message + "\n" + exception.StackTrace,
        symbol?.Locations.FirstOrDefault(), DiagnosticSeverity.Error);

  /// <summary>The type and every type it is nested in must be partial for the join to work.</summary>
  private static bool IsPartialAllTheWayOut(INamedTypeSymbol type) {
    for (var current = type; current is not null; current = current.ContainingType) {
      var partial = false;
      foreach (var reference in current.DeclaringSyntaxReferences)
        if (reference.GetSyntax() is Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax declaration &&
            declaration.Modifiers.Any(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PartialKeyword)) {
          partial = true;
          break;
        }
      if (!partial) return false;
    }
    return true;
  }

  private static INamedTypeSymbol ContainingType(ISymbol symbol) =>
    symbol as INamedTypeSymbol ?? symbol.ContainingType;

  private static SymbolTarget CreateTarget(WeaveSession session, ISymbol symbol, Unit unit) => unit switch {
    Unit.Type when symbol is INamedTypeSymbol type => new TypeTarget(session, type),
    Unit.Method when symbol is IMethodSymbol method => new MethodTarget(session, method),
    Unit.Field when symbol is IFieldSymbol => new MemberTarget(session, symbol),
    Unit.Property when symbol is IPropertySymbol => new MemberTarget(session, symbol),
    Unit.Member when symbol is IFieldSymbol or IPropertySymbol => new MemberTarget(session, symbol),
    // ponytail: Unit.Parameter and Unit.Derivation are declared but not dispatched yet; they
    // report WEAVE001 rather than silently doing nothing. Wire them when a mixin needs them.
    _ => null,
  };

  private static string FileNameFor(INamedTypeSymbol type) {
    var name = type.ToDisplayString(SymbolFacts.MetadataFormat).Replace('<', '_').Replace('>', '_');
    return name + ".weave.g.cs";
  }

  private readonly struct AnnotatedSymbol {
    public AnnotatedSymbol(string attributeName, ISymbol symbol, ImmutableArray<AttributeData> attributes) {
      AttributeName = attributeName;
      Symbol = symbol;
      Attributes = attributes;
    }

    public string AttributeName { get; }
    public ISymbol Symbol { get; }
    public ImmutableArray<AttributeData> Attributes { get; }

    /// <summary>Deterministic ordering, so generated output doesn't depend on pipeline timing.</summary>
    public string SortKey =>
      (Symbol?.Locations.FirstOrDefault()?.SourceTree?.FilePath ?? "") + ":" +
      (Symbol?.Locations.FirstOrDefault()?.SourceSpan.Start ?? 0).ToString("D8") + ":" + AttributeName;
  }
}
