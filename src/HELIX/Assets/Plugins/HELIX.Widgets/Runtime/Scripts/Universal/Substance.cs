using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Theming;

namespace HELIX.Widgets.Universal {
  public abstract class Substance : DiagnosticableBase {
    public static SubstanceFactory Factory => SubstanceFactory.Instance;
    public abstract IWidgetListCandidate Build(BuildContext context, WidgetState state);
  }

  public class ConditionalSubstance : Substance {
    public WidgetStateProperty<SubstanceLayers> candidates;

    public ConditionalSubstance(WidgetStateProperty<SubstanceLayers> candidates) {
      this.candidates = candidates;
    }

    public override IWidgetListCandidate Build(BuildContext context, WidgetState state) {
      return candidates.ResolveOrDefault(state).Select(s => s.Build(context, state)).Spread();
    }
  }

  public interface ISubstanceBuilder<TBuilder> where TBuilder : ISubstanceBuilder<TBuilder> {
    bool Listening { get; }
    TBuilder Self { get; }
    BuilderAndSubstance<TBuilder, T> Append<T>(Func<IThemeProvider, T> builder) where T : Substance;
  }

  public readonly struct SubstanceLayers : IReadOnlyList<Substance> {
    private readonly IReadOnlyList<Substance> _substances;

    public SubstanceLayers(IReadOnlyList<Substance> substances) : this() {
      _substances = substances;
    }

    public IEnumerator<Substance> GetEnumerator() {
      if (_substances == null) return Enumerable.Empty<Substance>().GetEnumerator();
      return _substances.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }

    public int Count => _substances?.Count ?? 0;

    public Substance this[int index] => _substances[index];

    public static implicit operator SubstanceLayers(SubstanceBuilder builder) {
      return new SubstanceLayers(builder);
    }

    public static implicit operator SubstanceLayers(List<Substance> substances) {
      return new SubstanceLayers(substances);
    }

    public static implicit operator SubstanceLayers(Substance[] substances) {
      return new SubstanceLayers(substances);
    }

    public static implicit operator SubstanceLayers(Substance substance) {
      return new SubstanceLayers(new[] { substance });
    }
  }

  public readonly struct BuilderAndSubstance<TBuilder, T> : ISubstanceBuilder<TBuilder>
    where TBuilder : ISubstanceBuilder<TBuilder> where T : Substance {
    public readonly TBuilder valueBuilder;
    public readonly T value;

    public BuilderAndSubstance(TBuilder valueBuilder, T value) {
      this.valueBuilder = valueBuilder;
      this.value = value;
    }

    public bool Listening => valueBuilder.Listening;
    public TBuilder Self => valueBuilder;

    public BuilderAndSubstance<TBuilder, TNext> Append<TNext>(Func<IThemeProvider, TNext> builder)
      where TNext : Substance {
      return valueBuilder.Append(builder);
    }

    public static implicit operator T(BuilderAndSubstance<TBuilder, T> wrapper) {
      return wrapper.value;
    }

    public static implicit operator TBuilder(BuilderAndSubstance<TBuilder, T> wrapper) {
      return wrapper.valueBuilder;
    }
  }

  public sealed class SubstanceFactory : ISubstanceBuilder<SubstanceFactory> {
    public static readonly SubstanceFactory Instance = new();
    private SubstanceFactory() { }

    public bool Listening => false;
    public SubstanceFactory Self => this;

    public BuilderAndSubstance<SubstanceFactory, T> Append<T>(Func<IThemeProvider, T> builder)
      where T : Substance {
      var substance = builder.Invoke(null);
      return new BuilderAndSubstance<SubstanceFactory, T>(this, substance);
    }
  }

  public sealed class SubstanceBuilder : WidgetStateProperty<IReadOnlyList<Substance>>, IReadOnlyList<Substance>,
    ISubstanceBuilder<SubstanceBuilder> {
    private readonly IThemeProvider _context;
    private readonly List<Substance> _substances = new();

    public SubstanceBuilder(IThemeProvider context, bool listening = false) {
      _context = context ?? FallbackThemeProvider.Instance;
      Listening = listening;
    }

    public IEnumerator<Substance> GetEnumerator() {
      return _substances.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }

    public int Count => _substances.Count;

    public Substance this[int index] => _substances[index];

    public bool Listening { get; }
    public SubstanceBuilder Self => this;

    public BuilderAndSubstance<SubstanceBuilder, T> Append<T>(Func<IThemeProvider, T> builder)
      where T : Substance {
      var substance = builder.Invoke(_context);
      _substances.Add(substance);
      return new BuilderAndSubstance<SubstanceBuilder, T>(this, substance);
    }

    public void Clear() {
      _substances.Clear();
    }

    public override bool TryResolve(WidgetState state, out IReadOnlyList<Substance> value) {
      value = _substances;
      return true;
    }
  }

  public static class SubstanceBuilderExtensions {
    public static SubstanceLayers Build(this ISubstanceBuilder<SubstanceBuilder> builder) {
      return new SubstanceLayers(builder.Self);
    }
  }
}