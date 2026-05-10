using System.Collections.Generic;
using HELIX.Extensions;
using HELIX.Widgets.Elements;
using HELIX.Widgets.Modifiers;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Scrolling {

  /// <summary>
  /// A widget that can be used to scroll through a list of widgets horizontally and vertically.
  /// </summary>
  public class HPanView : MultiChildWidget {
    public readonly ScrollController horizontalController;
    public readonly ScrollController verticalController;

    /// <summary>
    /// Creates a scrollable view that can be used to scroll through a list of widgets horizontally and vertically.
    /// </summary>
    /// <param name="horizontalController">
    /// The horizontal <see cref="ScrollController"/> to use for this scroll view.
    /// If not specified, a controller will be created and managed by the widget's state.
    /// </param>
    /// <param name="verticalController">
    /// The vertical <see cref="ScrollController"/> to use for this scroll view.
    /// If not specified, a controller will be created and managed by the widget's state.
    /// </param>
    /// <param name="children">The children of this scroll view.</param>
    /// <param name="key">Passed on to <see cref="Widget.key"/>.</param>
    /// <param name="constants">Passed on to <see cref="Widget.constants"/>.</param>
    /// <param name="modifiers">Passed on to <see cref="Widget.modifiers"/>.</param>
    /// <seealso cref="IPreferExplicitFlex"/>
    /// <seealso cref="ModifierFallbacks.ImplicitFlexFill"/>
    /// <inheritdoc/>
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

    public override IWidgetElement CreateElement() {
      return ReconcileInto(new HPanViewElement());
    }
  }

  public class HPanViewElement : MultiChildWidgetBaseElement<HPanView>, IHierarchyDisposable, IPreferExplicitFlex {
    private readonly ScrollView _scrollView;
    private ScrollController _horizontalController;
    private ScrollPosition _horizontalPosition;
    private ScrollController _verticalController;

    private ScrollPosition _verticalPosition;

    public HPanViewElement() {
      _scrollView = new ScrollView().AddTo(hierarchy);
      _scrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
      _scrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;
      _scrollView.contentContainer.userData = new ElementTreeAncestorTraversalHint(this);
    }

    public override VisualElement contentContainer => _scrollView.contentContainer;

    public void Dispose() {
      _verticalPosition?.Dispose();
      _horizontalPosition?.Dispose();
    }

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
  }
}