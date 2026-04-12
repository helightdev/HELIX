using HELIX.Extensions;
using HELIX.Widgets.Elements;
using HELIX.Widgets.Modifiers;
using HELIX.Widgets.Utilities;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Scrolling {
    public class HListView : Widget {
        public int itemCount;
        public BuildFunction<int> itemBuilder;
        public float fixedItemHeight = -1;
        public ScrollController scrollController;

        public HListView() {
            AddModifier(ModifierFallbacks.ImplicitFlexFill);
        }

        public override IWidgetElement CreateElement() => ReconcileInto(new HListViewElement());
    }

    public class HListViewElement : WidgetBaseElement<HListView>, IHierarchyDisposable {
        private readonly ListView _listView;
        private readonly ScrollView _scrollView;
        private readonly DummyCounterList _dummyList = new();
        private readonly ScrollPosition _scrollPosition;
        private ScrollController _scrollController;

        public HListViewElement() {
            _listView = new ListView().AddTo(this);
            _scrollView = _listView.Q<ScrollView>();
            _scrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            _scrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;
            _listView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            _listView.makeItem = () => new WidgetHostElement().TightStretch().Sized(width: 100.Percent());
            _listView.destroyItem = element => element.Clear();
            _listView.bindItem = BindItem;
            _listView.unbindItem = UnbindItem;
            _listView.allowAdd = false;
            _listView.allowRemove = false;
            _listView.selectionType = SelectionType.None;
            _scrollPosition = new ScrollerScrollPosition(_scrollView.verticalScroller, _scrollView);
        }

        protected override void OnAttached(AttachToPanelEvent evt) {
            base.OnAttached(evt);
            _scrollController?.Attach(_scrollPosition);
        }

        protected override void OnDetached(DetachFromPanelEvent evt) {
            base.OnDetached(evt);
            _scrollController?.Detach(_scrollPosition);
        }

        private void UnbindItem(VisualElement elem, int index) {
            var host = elem as WidgetHostElement;
            if (host == null) return;
            host.Buildable = null;
            ModificationBarrier.Rebuild(host);
        }

        private void BindItem(VisualElement elem, int index) {
            var host = elem as WidgetHostElement;
            var typed = TypedDescriptor;
            if (host == null || typed == null) return;
            if (index < 0 || index >= typed.itemCount) {
                host.Buildable = null;
                ModificationBarrier.Rebuild(host);
                return;
            }

            var buildable = new ParameterizedFunctionBuildable<int>(typed.itemBuilder, index);
            host.Buildable = buildable;
            ModificationBarrier.Rebuild(host);
        }

        public override void Apply(HListView previous, HListView widget) {
            base.Apply(previous, widget);

            if (_dummyList.Count != widget.itemCount) {
                _dummyList.Count = widget.itemCount;
                _listView.itemsSource = _dummyList;
            }

            if (widget.fixedItemHeight >= 0) {
                _listView.fixedItemHeight = widget.fixedItemHeight;
                _listView.virtualizationMethod = CollectionVirtualizationMethod.FixedHeight;
            } else { _listView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight; }

            _listView.RefreshItems();

            if (widget.scrollController != _scrollController) {
                _scrollController?.Detach(_scrollPosition);
                widget.scrollController?.Attach(_scrollPosition);
                _scrollController = widget.scrollController;
            }
        }

        public void Dispose() {
            _scrollPosition.Dispose();
        }
    }
}