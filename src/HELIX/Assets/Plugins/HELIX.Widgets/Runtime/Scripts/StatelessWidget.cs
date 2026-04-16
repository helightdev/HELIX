using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Elements;
using HELIX.Widgets.Modifiers;
using HELIX.Widgets.Theming;

namespace HELIX.Widgets {

    public interface IStatelessWidget { }

    public abstract class StatelessWidget<T> : Widget, IStatelessWidget, IBuildable where T : StatelessWidget<T> {
        protected StatelessWidget() {
            AddModifier(ModifierFallbacks.ImplicitFlexFill);
        }

        public abstract Widget Build(BuildContext context);

        public override IWidgetElement CreateElement() {
            return ReconcileInto(new StatelessWidgetElement<T>());
        }
    }

    public class StatelessWidgetElement<T> : BuildingWidgetBaseElement<T>, IStatelessWidget, IHierarchyDisposable
        where T : StatelessWidget<T> {

        private bool _isDisposed;

        protected override void OnWatchedThemeUpdated(ThemeProperty property, object value) {
            base.OnWatchedThemeUpdated(property, value);
            OnDependencyUpdated();
        }

        public override S GetThemed<S>(BaseThemeProperty<S> property) {
            return ThemeValue(property).Value;
        }
        
        public override bool TryGetThemed<S>(BaseThemeProperty<S> property, out S value) {
            ThemeValue(property);
            return ThemeProviderElement.TryResolve(property, out value);
        }
        

        private void OnDependencyUpdated() {
            if (_isDisposed) return;
            if (Descriptor == null) return;
            ModificationBarrier.Rebuild(this);
        }

        protected override IBuildable GetBuildableForWidget(T previous, T widget) {
            return widget;
        }

        public override string ToStringShort() {
            return $"{typeof(T).Name}:Element#{this.ShortHash()}";
        }

        public void Dispose() {
            _isDisposed = true;
        }
    }
}