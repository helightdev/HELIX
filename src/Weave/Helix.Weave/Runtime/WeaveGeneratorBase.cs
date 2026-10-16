using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Helix.Weave;

/// <summary>
/// The incremental generator. Lowering generates a subclass that supplies the mixin list.
///
/// The pipeline is built around the prelude/late split. Each annotated symbol's prelude runs
/// inside its own `ForAttributeWithMetadataName` transform and produces a value-only
/// <see cref="SymbolContribution"/>, so Roslyn caches it and an edit re-runs the preludes of
/// only the symbols that actually changed. Contributions are then grouped per type, and each
/// type's output is registered separately with its own equality — so an unchanged type is not
/// re-rendered either. The late passes are pure functions of their carry and run at merge time,
/// where every alias and shared block is finally visible.
/// </summary>
public abstract class WeaveGeneratorBase : IIncrementalGenerator {
  /// <summary>Step names, so a test can assert the pipeline actually caches.</summary>
  public const string TrackedPrelude = "Weave.Prelude";
  public const string TrackedWholeCompilation = "Weave.Prelude.WholeCompilation";
  public const string TrackedType = "Weave.Type";

  protected abstract IReadOnlyList<Mixin> Mixins { get; }

  public void Initialize(IncrementalGeneratorInitializationContext context) {
    var mixins = Mixins;
    var derivations = new DerivationPipeline(mixins);

    IncrementalValueProvider<ImmutableArray<SymbolContribution>> all = default;
    var any = false;

    for (var index = 0; index < mixins.Count; index++) {
      var mixin = mixins[index];
      // Derivation steps are consulted from inside other mixins' preludes, not triggered here.
      if (mixin.Unit == Unit.Derivation) continue;

      var position = index;
      var registry = mixins;
      var contributions = Contributions(context, mixin, position, derivations, registry);
      all = any ? Concat(all, contributions) : contributions;
      any = true;
    }

    if (!any) return;

    // Per type, with its own equality: an untouched type is neither merged nor rendered again.
    var byType = all.SelectMany(static (items, _) => Merge.ByType(items)).WithTrackingName(TrackedType);
    context.RegisterSourceOutput(byType, (spc, type) => Emit(spc, type, mixins));
  }

  /// <summary>Runs one mixin's prelude for every symbol carrying its attribute.</summary>
  private static IncrementalValueProvider<ImmutableArray<SymbolContribution>> Contributions(
    IncrementalGeneratorInitializationContext context, Mixin mixin, int index,
    DerivationPipeline derivations, IReadOnlyList<Mixin> mixins) {
    var matches = context.SyntaxProvider.ForAttributeWithMetadataName(
      mixin.AttributeName,
      predicate: static (node, _) => true,
      transform: (ctx, token) => Prelude(mixin, index, ctx, derivations, mixins, token));

    // A mixin that queries the whole compilation cannot be cached per symbol: adding an
    // unrelated type elsewhere changes its answer. Those few are tracked separately so the
    // distinction is visible when inspecting the pipeline.
    var name = mixin.WholeCompilation ? TrackedWholeCompilation : TrackedPrelude;
    return matches.Where(static item => item is not null).WithTrackingName(name).Collect();
  }

  private static IncrementalValueProvider<ImmutableArray<SymbolContribution>> Concat(
    IncrementalValueProvider<ImmutableArray<SymbolContribution>> left,
    IncrementalValueProvider<ImmutableArray<SymbolContribution>> right) =>
    left.Combine(right).Select(static (pair, _) => pair.Left.AddRange(pair.Right));

  /// <summary>
  /// One symbol's prelude. Everything expensive — symbol queries, derivation, string building —
  /// happens here, and the result is pure value data that Roslyn can hold onto.
  /// </summary>
  private static SymbolContribution Prelude(Mixin mixin, int index, GeneratorAttributeSyntaxContext ctx,
                                            DerivationPipeline derivations, IReadOnlyList<Mixin> mixins,
                                            CancellationToken token) {
    var symbol = ctx.TargetSymbol;
    var containing = symbol as INamedTypeSymbol ?? symbol.ContainingType;
    if (containing is null) return null;

    token.ThrowIfCancellationRequested();

    var compilation = ctx.SemanticModel.Compilation;
    var facts = new SymbolFacts();
    var shape = Shape(containing, facts);
    var context = new PreludeContext(containing, compilation, facts, derivations);
    var buffer = new ContributionBuffer();
    SeedAliases(containing, context, facts, mixins);

    var target = CreateTarget(buffer, symbol, mixin.Unit, context);
    if (target is null) {
      buffer.Report(new ReportedDiagnostic("WEAVE001",
        $"mixin for {mixin.AttributeName} expects a {mixin.Unit} but was applied to {symbol.Kind}",
        "Error", Location(symbol)?.SourceTree?.FilePath, Location(symbol)?.SourceSpan.Start ?? 0,
        Location(symbol)?.SourceSpan.Length ?? 0));
      return buffer.Build(shape, Order(symbol), mixin.AttributeName, null, -1);
    }

    object carry = null;
    try {
      carry = mixin.Prelude(target, ctx.Attributes.FirstOrDefault());

      // The late pass runs here, against the same target. Purity is enforced by its signature —
      // it only ever sees a LateTarget and its carry — and running it inside the cached
      // transform means its output is cached with everything else the symbol produced.
      if (mixin.HasLate) mixin.Late(target, carry);
    } catch (Exception exception) {
      // A mixin that throws takes down the whole generator run otherwise, and the driver reports
      // only the exception type. Name the mixin and the symbol instead.
      buffer.Report(new ReportedDiagnostic("WEAVE000",
        $"the prelude of {mixin.GetType().FullName} threw on '{symbol.Name}': " +
        exception.GetType().Name + ": " + exception.Message,
        "Error", Location(symbol)?.SourceTree?.FilePath, Location(symbol)?.SourceSpan.Start ?? 0,
        Location(symbol)?.SourceSpan.Length ?? 0));
    }

    return buffer.Build(shape, Order(symbol), mixin.AttributeName, carry, -1);
  }

  /// <summary>
  /// Alias definitions come from the attributes the type carries, not from another mixin having
  /// run first — which is what makes them available to a prelude cached on its own.
  /// </summary>
  private static void SeedAliases(INamedTypeSymbol type, PreludeContext context, SymbolFacts facts,
                                  IReadOnlyList<Mixin> mixins) {
    var attributes = facts.Attributes(type);
    if (attributes.Count == 0) return;

    foreach (var mixin in mixins) {
      if (mixin.Unit != Unit.Type) continue;
      var carried = false;
      foreach (var attribute in attributes)
        if (attribute.AttributeClass is not null &&
            facts.MetadataName(attribute.AttributeClass) == mixin.AttributeName) {
          carried = true;
          break;
        }
      if (!carried) continue;
      mixin.DefineAliases(new TypeTarget(new ContributionBuffer(), type, context));
    }
  }

  private static void Emit(SourceProductionContext context, TypeContribution contribution,
                           IReadOnlyList<Mixin> mixins) {
    var merged = Merge.Run(contribution, mixins);

    foreach (var diagnostic in merged.Diagnostics) context.ReportDiagnostic(ToDiagnostic(diagnostic));
    if (merged.Emissions.Length == 0) return;

    // Generated members land in a partial declaration, so there has to be one to join. A warning
    // rather than an error: it keeps a project that has such a type compiling.
    if (!contribution.Type.IsPartial) {
      context.ReportDiagnostic(ToDiagnostic(new ReportedDiagnostic("WEAVE003",
        $"'{contribution.Type.Name}' is not declared partial, so nothing was generated for it",
        "Warning", null, 0, 0)));
      return;
    }

    context.AddSource(contribution.Type.FileName, Renderer.Render(merged));
  }

  private static Diagnostic ToDiagnostic(ReportedDiagnostic reported) {
    var severity = reported.Severity switch {
      "Warning" => DiagnosticSeverity.Warning,
      "Info" => DiagnosticSeverity.Info,
      "Hidden" => DiagnosticSeverity.Hidden,
      _ => DiagnosticSeverity.Error,
    };
    return Diagnostic.Create(
      new DiagnosticDescriptor(reported.Id, "Weave", "{0}", "Weave", severity, isEnabledByDefault: true),
      Microsoft.CodeAnalysis.Location.None, reported.Message);
  }

  private static Location Location(ISymbol symbol) => symbol?.Locations.FirstOrDefault();

  /// <summary>Source position, so generated output has a deterministic order.</summary>
  private static int Order(ISymbol symbol) => Location(symbol)?.SourceSpan.Start ?? 0;

  private static SymbolTarget CreateTarget(ContributionBuffer buffer, ISymbol symbol, Unit unit,
                                           PreludeContext context) => unit switch {
    Unit.Type when symbol is INamedTypeSymbol type => new TypeTarget(buffer, type, context),
    Unit.Method when symbol is IMethodSymbol method => new MethodTarget(buffer, method, context),
    Unit.Field when symbol is IFieldSymbol => new MemberTarget(buffer, symbol, context),
    Unit.Property when symbol is IPropertySymbol => new MemberTarget(buffer, symbol, context),
    Unit.Member when symbol is IFieldSymbol or IPropertySymbol => new MemberTarget(buffer, symbol, context),
    // ponytail: Unit.Parameter is declared but not dispatched yet; it reports WEAVE001 rather
    // than silently doing nothing. Wire it when a mixin needs it.
    _ => null,
  };

  /// <summary>Everything rendering needs to know about the type, captured as value data.</summary>
  private static TypeShape Shape(INamedTypeSymbol type, SymbolFacts facts) {
    var keyword = type.TypeKind switch {
      TypeKind.Struct => type.IsRecord ? "partial record struct" : "partial struct",
      TypeKind.Interface => "partial interface",
      _ => type.IsRecord ? "partial record" : "partial class",
    };

    var parameters = type.TypeParameters.Length == 0
      ? ""
      : "<" + string.Join(", ", type.TypeParameters.Select(item => item.Name)) + ">";

    var key = facts.MetadataName(type);
    var fileName = key.Replace('<', '_').Replace('>', '_') + ".weave.g.cs";
    var namespaceName = type.ContainingNamespace is { IsGlobalNamespace: false } ns
      ? ns.ToDisplayString()
      : null;

    return new TypeShape(key, namespaceName, type.Name, keyword, parameters,
                         IsPartialAllTheWayOut(type), fileName);
  }

  /// <summary>The type and every type it is nested in must be partial for the join to work.</summary>
  private static bool IsPartialAllTheWayOut(INamedTypeSymbol type) {
    for (var current = type; current is not null; current = current.ContainingType) {
      var partial = false;
      foreach (var reference in current.DeclaringSyntaxReferences)
        if (reference.GetSyntax() is TypeDeclarationSyntax declaration &&
            declaration.Modifiers.Any(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PartialKeyword)) {
          partial = true;
          break;
        }
      if (!partial) return false;
    }
    return true;
  }
}
