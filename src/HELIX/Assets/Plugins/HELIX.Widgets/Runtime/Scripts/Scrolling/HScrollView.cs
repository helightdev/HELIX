using System.Collections.Generic;
using HELIX.Extensions;
using HELIX.Types;
using HELIX.Widgets.Elements;
using HELIX.Widgets.Modifiers;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Scrolling {

  /// <summary>
  /// A widget that can be used to scroll through a list of widgets.
  /// </summary>
  public class HScrollView : MultiChildWidget {
    public readonly Axis axis;
    public readonly ScrollController controller;

    /// <summary>
    /// Creates a scrollable view that can be used to scroll through a list of widgets.
    /// </summary>
    /// <param name="axis">The axis along which the scroll view will scroll.</param>
    /// <param name="controller">
    /// The <see cref="ScrollController"/> to use for this scroll view.
    /// If not specified, a controller will be created and managed by the widget's state.
    /// </param>
    /// <param name="children">The children of this scroll view.</param>
    /// <param name="key">Passed on to <see cref="Widget.key"/>.</param>
    /// <param name="constants">Passed on to <see cref="Widget.constants"/>.</param>
    /// <param name="modifiers">Passed on to <see cref="Widget.modifiers"/>.</param>
    /// <seealso cref="IPreferExplicitFlex"/>
    /// <seealso cref="ModifierFallbacks.ImplicitFlexFill"/>
    /// <inheritdoc/>
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

    public override IWidgetElement CreateElement() {
      return ReconcileInto(new HScrollViewElement());
    }
  }

  public class HScrollViewElement : MultiChildWidgetBaseElement<HScrollView>, IHierarchyDisposable,
    IPreferExplicitFlex {
    private readonly ScrollView _scrollView;
    private ScrollController _scrollController;
    private ScrollPosition _scrollPosition;

    public HScrollViewElement() {
      _scrollView = new ScrollView().AddTo(hierarchy);
      _scrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
      _scrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;
      _scrollView.contentContainer.userData = new ElementTreeAncestorTraversalHint(this);
    }

    public override VisualElement contentContainer => _scrollView.contentContainer;

    public void Dispose() {
      _scrollPosition?.Dispose();
    }

    public override void Apply(HScrollView previous, HScrollView widget) {
      _scrollView.mode = widget.axis == Axis.Vertical ? ScrollViewMode.Vertical : ScrollViewMode.Horizontal;

      var previousPosition = _scrollPosition;
      if (previous == null || (_scrollPosition == null) | (previous.axis != widget.axis)) {
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
  }
}