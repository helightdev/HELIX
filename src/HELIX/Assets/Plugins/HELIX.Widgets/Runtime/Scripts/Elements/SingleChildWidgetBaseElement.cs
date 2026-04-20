namespace HELIX.Widgets.Elements {
    public abstract class SingleChildWidgetBaseElement<T> : SuperSingleChildWidgetBaseElement<T>
        where T : SingleChildWidget {

        protected override Widget GetChildFromWidget(T previous, T widget) {
            return widget.child;
        }

    }
}