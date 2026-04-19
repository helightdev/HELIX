using System.Collections.Generic;
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

        public HScrollView(
            Axis axis = Axis.Vertical,
            ScrollController controller = null,
            IReadOnlyList<Widget> children = null,
            Key key = default,
            object[] constants = null,
            IReadOnlyCollection<Modifier> modifiers = null
        ) : base(children, key, constants) {
            this.axis = axis;
            this.controller = controller;
            
            DefaultModifiers(ModifierSet.DefaultFlexFill, modifiers);
        }

        public override IWidgetElement CreateElement() => ReconcileInto(new HScrollViewElement());
    }

    public class HScrollViewElement : MultiChildWidgetBaseElement<HScrollView>, IHierarchyDisposable,
        IPreferExplicitFlex {
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

    public class HPanView : MultiChildWidget {
        public ScrollController horizontalController;
        public ScrollController verticalController;

        public HPanView() {
            AddModifier(ModifierFallbacks.ImplicitFlexFill);
        }

        public HPanView(
            ScrollController horizontalController = null,
            ScrollController verticalController = null,
            IReadOnlyList<Widget> children = null,
            Key key = default,
            object[] constants = null,
            IReadOnlyCollection<Modifier> modifiers = null
        ) : base(children, key, constants) {
            this.horizontalController = horizontalController;
            this.verticalController = verticalController;
            
            DefaultModifiers(ModifierSet.DefaultFlexFill, modifiers);
        }

        public override IWidgetElement CreateElement() => ReconcileInto(new HPanViewElement());
    }

    public class HPanViewElement : MultiChildWidgetBaseElement<HPanView>, IHierarchyDisposable, IPreferExplicitFlex {
        private readonly ScrollView _scrollView;

        private ScrollPosition _verticalPosition;
        private ScrollPosition _horizontalPosition;
        private ScrollController _verticalController;
        private ScrollController _horizontalController;

        public HPanViewElement() {
            _scrollView = new ScrollView().AddTo(hierarchy);
            _scrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            _scrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;
        }

        public override VisualElement contentContainer => _scrollView.contentContainer;

        public override void Apply(HPanView previous, HPanView widget) {
            _scrollView.mode = ScrollViewMode.VerticalAndHorizontal;

            var previousPosition = _verticalPosition;
            if (previous == null || _verticalPosition == null) {
                _verticalPosition?.Dispose();
                _verticalPosition = new ScrollerScrollPosition(_scrollView.verticalScroller, _scrollView);
            }

            if (previous == null || _horizontalPosition == null) {
                _horizontalPosition?.Dispose();
                _horizontalPosition = new ScrollerScrollPosition(_scrollView.horizontalScroller, _scrollView);
            }

            if (_verticalController != widget.verticalController) {
                if (_verticalController != null && previousPosition != null)
                    _verticalController.Detach(previousPosition);
                _verticalController = widget.verticalController;
                _verticalController?.Attach(_verticalPosition);
            }

            if (_horizontalController != widget.horizontalController) {
                if (_horizontalController != null && previousPosition != null)
                    _horizontalController.Detach(previousPosition);
                _horizontalController = widget.horizontalController;
                _horizontalController?.Attach(_horizontalPosition);
            }
        }

        public void Dispose() {
            _verticalPosition?.Dispose();
            _horizontalPosition?.Dispose();
        }
    }
}