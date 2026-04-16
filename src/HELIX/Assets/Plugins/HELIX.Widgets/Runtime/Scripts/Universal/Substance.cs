using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HELIX.Widgets.Diagnostics;

namespace HELIX.Widgets.Universal {
    public abstract class Substance : DiagnosticableBase {
        public static SubstanceFactory Factory => SubstanceFactory.Instance;
        public abstract Widget Build(BuildContext context, WidgetState state);
    }

    public interface ISubstanceBuilder<TBuilder> where TBuilder : ISubstanceBuilder<TBuilder> {
        bool Listening { get; }
        TBuilder Self { get; }
        BuilderAndSubstance<TBuilder, T> AppendAndReturn<T>(Func<BuildContext, T> builder) where T : Substance;
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

        public static implicit operator SubstanceLayers(SubstanceBuilder builder) => new(builder);
        public static implicit operator SubstanceLayers(List<Substance> substances) => new(substances);
        public static implicit operator SubstanceLayers(Substance[] substances) => new(substances);
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

        public BuilderAndSubstance<TBuilder, TNext> AppendAndReturn<TNext>(Func<BuildContext, TNext> builder)
            where TNext : Substance {
            return valueBuilder.AppendAndReturn(builder);
        }

        public static implicit operator T(BuilderAndSubstance<TBuilder, T> wrapper) => wrapper.value;
        public static implicit operator TBuilder(BuilderAndSubstance<TBuilder, T> wrapper) => wrapper.valueBuilder;
    }

    public sealed class SubstanceFactory : ISubstanceBuilder<SubstanceFactory> {
        public static readonly SubstanceFactory Instance = new();
        private SubstanceFactory() { }

        public bool Listening => false;
        public SubstanceFactory Self => this;

        public BuilderAndSubstance<SubstanceFactory, T> AppendAndReturn<T>(Func<BuildContext, T> builder)
            where T : Substance {
            var substance = builder.Invoke(null);
            return new BuilderAndSubstance<SubstanceFactory, T>(this, substance);
        }
    }

    public sealed class SubstanceBuilder : WidgetStateProperty<IReadOnlyList<Substance>>, IReadOnlyList<Substance>,
        ISubstanceBuilder<SubstanceBuilder> {
        private BuildContext _context;
        private readonly List<Substance> _substances = new();

        public SubstanceBuilder(BuildContext context, bool listening = false) {
            _context = context;
            Listening = listening;
        }

        public bool Listening { get; }
        public SubstanceBuilder Self => this;

        public BuilderAndSubstance<SubstanceBuilder, T> AppendAndReturn<T>(Func<BuildContext, T> builder)
            where T : Substance {
            var substance = builder.Invoke(_context);
            _substances.Add(substance);
            return new BuilderAndSubstance<SubstanceBuilder, T>(this, substance);
        }

        public IEnumerator<Substance> GetEnumerator() {
            return _substances.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => _substances.Count;

        public Substance this[int index] => _substances[index];

        public void Clear() => _substances.Clear();

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