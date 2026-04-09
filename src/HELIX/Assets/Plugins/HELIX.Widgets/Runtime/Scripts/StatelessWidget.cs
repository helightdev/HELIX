using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Elements;
using HELIX.Widgets.Modifiers;

namespace HELIX.Widgets {
    public interface IBuildable {
        Widget Build(BuildContext context);
    }

    public delegate Widget BuildFunction(BuildContext context);

    public delegate Widget BuildFunction<in T>(BuildContext context, T parameter);

    public readonly struct FunctionBuildable : IBuildable {
        private readonly BuildFunction _func;

        public FunctionBuildable(BuildFunction func) {
            _func = func;
        }

        public Widget Build(BuildContext context) {
            return _func(context);
        }
    }

    public readonly struct ParameterizedFunctionBuildable<T> : IBuildable {
        private readonly BuildFunction<T> _func;
        private readonly T _param;

        public ParameterizedFunctionBuildable(BuildFunction<T> func, T param) {
            _func = func;
            _param = param;
        }

        public Widget Build(BuildContext context) {
            return _func(context, _param);
        }
    }

    public interface IStatelessWidget { }

    public abstract class StatelessWidget<T> : Widget, IStatelessWidget, IBuildable where T : StatelessWidget<T> {
        protected StatelessWidget() {
            AddModifier(ModifierFallbacks.FlexFill);
        }

        public abstract Widget Build(BuildContext context);

        public override IWidgetElement CreateElement() {
            return ReconcileInto(new StatelessWidgetElement<T>());
        }
    }

    public class StatelessWidgetElement<T> : BuildingWidgetBaseElement<T>, IStatelessWidget
        where T : StatelessWidget<T> {
        protected override void OnThemeUpdated() {
            if (Descriptor != null) ModificationBarrier.RunRebuild(this);
        }

        protected override IBuildable GetBuildableForWidget(T previous, T widget) {
            return widget;
        }

        public override string ToStringShort() {
            return $"{typeof(T).Name}:Element#{this.ShortHash()}";
        }
    }
}