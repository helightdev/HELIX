using HELIX.Extensions;
using HELIX.Types;
using HELIX.Widgets.Elements;
using HELIX.Widgets.Modifiers;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Scrolling {
    public class HScrollView : MultiChildWidget {
        public Axis axis = Axis.Vertical;
        public ScrollController controller;
        
        public HScrollView() {
            AddModifier(ModifierFallbacks.ImplicitFlexFill);
        }

        public override IWidgetElement CreateElement() => ReconcileInto(new HScrollViewElement());
    }

    public class HScrollViewElement : MultiChildWidgetBaseElement<HScrollView>, IHierarchyDisposable {
        private readonly ScrollView _scrollView;
        private ScrollPosition _scrollPosition;
        private ScrollController _scrollController;

        public HScrollViewElement() {
            _scrollView = new ScrollView().AddTo(hierarchy);
            _scrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            _scrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;
        }

        public override VisualElement contentContainer => _scrollView.contentContainer;

        public override void Apply(HScrollView previous, HScrollView widget) {
            _scrollView.mode = widget.axis == Axis.Vertical ? ScrollViewMode.Vertical : ScrollViewMode.Horizontal;
            
            var previousPosition = _scrollPosition;
            if (previous == null || _scrollPosition == null | previous.axis != widget.axis) {
                _scrollPosition?.Dispose();
                _scrollPosition = new ScrollerScrollPosition(
                    widget.axis == Axis.Vertical ? _scrollView.verticalScroller : _scrollView.horizontalScroller,
                    _scrollView
                );
            }

            if (_scrollController != widget.controller) {
                if (_scrollController != null && previousPosition != null) _scrollController.Detach(previousPosition);
                _scrollController = widget.controller;
                _scrollController?.Attach(_scrollPosition);
            }
        }

        public void Dispose() {
            _scrollPosition?.Dispose();
        }
    }
}