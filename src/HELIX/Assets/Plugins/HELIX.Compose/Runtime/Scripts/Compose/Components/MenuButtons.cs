using System;
using System.Collections;
using System.Collections.Generic;
using HELIX.Signals;
using HELIX.Theming;
using HELIX.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  [Structure] public readonly partial struct PopupMenuStyle {
    public readonly HXControlBoxStyle button;
    public readonly HXControlBoxStyle panel;
    public readonly HXControlBoxStyle item;
    public readonly TextStyle headingTextStyle;
    public readonly StyleLength4 headingPadding;
    public readonly Color separatorColor;
    public readonly Color iconColor;
    [Prop(1f)] public readonly float separatorThickness;
    [Prop(2f)] public readonly float gap;
    [Prop("default", PropInit.Constant)] public readonly Vector2 offset;
    [Prop("default", PropInit.Constant)] public readonly Vector2 submenuOffset;
    [Prop(false)] public readonly bool matchAnchorWidth;
  }

  public interface IDropdownOption {
    public string Label { get; }
    public bool Enabled { get; }
    public object RawValue { get; }
  }

  public readonly struct DropdownOption<T> : IDropdownOption {
    public readonly T value;
    public readonly string label;
    public readonly bool enabled;

    public DropdownOption(T value, string label, bool enabled = true) {
      this.value = value;
      this.label = label;
      this.enabled = enabled;
    }

    public string Label => label;
    public bool Enabled => enabled;
    public object RawValue => value;
  }

  public abstract class DropdownController : Signal {
    public bool enabled = true;
    public bool error;
    public string placeholder;

    protected DropdownController(string name, Type registeredType) : base(name, registeredType) { }

    public State State {
      get {
        var state = State.None;
        state |= enabled ? State.None : State.Disabled;
        state |= error ? State.Error : State.None;
        return state;
      }
    }

    public abstract int Count { get; }
    public abstract string SelectedLabel { get; }
    public abstract string Label(int index);
    public abstract bool IsEnabled(int index);
    public abstract bool IsSelected(int index);
    internal abstract void SynchronizeAutomatic(
      object value,
      object options,
      Delegate onChanged,
      string placeholder,
      bool enabled,
      bool error
    );
    internal abstract void SetUserIndex(IBoundary boundary, int index);

    protected void NotifyListeners() {
      NotifyDirty();
      NotifyObservers();
    }
  }

  public sealed class DropdownController<T> : DropdownController {
    private static readonly EqualityComparer<T> _equality = EqualityComparer<T>.Default;

    public T value;
    public IReadOnlyList<DropdownOption<T>> options;
    public CompositionAction<T> onChanged;

    public DropdownController(
      T initialValue = default,
      IReadOnlyList<DropdownOption<T>> options = null
    ) : base("DropdownController", typeof(DropdownController<T>)) {
      value = initialValue;
      this.options = options;
    }

    public override int Count => options?.Count ?? 0;

    public override string SelectedLabel {
      get {
        for (var i = 0; i < Count; i++) {
          if (IsSelected(i)) return Label(i);
        }
        return placeholder ?? string.Empty;
      }
    }

    public override string Label(int index) => options[index].label ?? string.Empty;
    public override bool IsEnabled(int index) => options[index].enabled;
    public override bool IsSelected(int index) => _equality.Equals(options[index].value, value);

    public void SetValue(T updated) {
      if (_equality.Equals(value, updated)) return;
      value = updated;
      NotifyListeners();
    }

    public void SetWithoutNotify(T updated) {
      value = updated;
      NotifyDirty();
    }

    internal void Synchronize(
      T updated,
      IReadOnlyList<DropdownOption<T>> updatedOptions,
      CompositionAction<T> updatedOnChanged,
      string updatedPlaceholder,
      bool updatedEnabled,
      bool updatedError
    ) {
      value = updated;
      options = updatedOptions;
      onChanged = updatedOnChanged;
      placeholder = updatedPlaceholder;
      enabled = updatedEnabled;
      error = updatedError;
    }

    internal override void SynchronizeAutomatic(
      object updated,
      object updatedOptions,
      Delegate updatedOnChanged,
      string updatedPlaceholder,
      bool updatedEnabled,
      bool updatedError
    ) => Synchronize(
      updated == null ? default : (T)updated,
      (IReadOnlyList<DropdownOption<T>>)updatedOptions,
      (CompositionAction<T>)updatedOnChanged,
      updatedPlaceholder,
      updatedEnabled,
      updatedError
    );

    internal override void SetUserIndex(IBoundary boundary, int index) {
      if (index < 0 || index >= Count || !IsEnabled(index)) return;
      var updated = options[index].value;
      if (_equality.Equals(value, updated)) return;
      value = updated;
      onChanged?.Call(boundary, updated);
      NotifyListeners();
    }
  }

  public enum MenuItemKind : byte { Action, Separator, Heading }

  public class MenuItem : IEnumerable<MenuItem> {
    public readonly MenuItemKind kind;
    public readonly string label;
    public readonly CompositionAction action;
    public readonly bool enabled;
    public readonly bool selected;
    public IReadOnlyList<MenuItem> children;

    public MenuItem(
      string label,
      CompositionAction action = null,
      bool enabled = true,
      bool selected = false,
      IReadOnlyList<MenuItem> children = null
    ) {
      kind = MenuItemKind.Action;
      this.label = label;
      this.action = action;
      this.enabled = enabled;
      this.selected = selected;
      this.children = children;
    }

    public void Add(MenuItem spec) {
      children ??= new List<MenuItem>();
      if (children is not List<MenuItem> list)
        throw new InvalidOperationException("Cannot add child items to a non-list children collection.");
      list.Add(spec);
    }

    public MenuItem(MenuItemKind kind, string label = null) {
      this.kind = kind;
      this.label = label;
      action = null;
      enabled = false;
      selected = false;
      children = null;
    }

    public IEnumerator<MenuItem> GetEnumerator() {
      throw new NotImplementedException(
        "MenuItemSpec does not support enumeration. Use the children property to access submenu items."
      );
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
  }

  internal interface IPopupMenuItemOwner {
    void ActivateItem(int index, IBoundary item);
    void PreviewItem(int index, IBoundary item);
  }

  [EnableMixins]
  [BoundaryElementMixin]
  [InputStateListener]
  internal partial class PopupMenuItemBoundary {
    public partial struct Props {
      public IPopupMenuItemOwner owner;
      public int index;
      public string label;
      public bool enabled;
      public bool selected;
      public bool hasChildren;
      public HXControlBoxStyle style;
    }

    [Hook]
    private void OnCompose(ref Composition cx) {
      this.Toggle(State.Disabled, !props.enabled);
      this.Toggle(State.Selected, props.selected);
      SetEnabled(props.enabled);
      cx.CURSOR.Focusable(props.enabled).AlignSelf(Align.Stretch);
      props.style.RenderBoundary(ref cx, InputState);

      using (cx.Group(Axis.Horizontal, main: Justify.SpaceBetween, cross: Align.Center)) {
        cx.CURSOR.Flexible().AlignSelf(Align.Stretch);
        cx.Text(props.label ?? string.Empty);
        if (props.hasChildren) {
          cx.Spacing(1);
          new ChevronSpec(
            ArrowPosition.Right, ThemeProperties.ChevronSize[in cx], TextStyle.Key[in cx].color
          ).Compose(ref cx);
        }
      }
    }

    [Hook]
    private void OnInit() => RegisterCallback<PointerEnterEvent>(OnPointerEnter);

    [Hook]
    private void OnDispose() => UnregisterCallback<PointerEnterEvent>(OnPointerEnter);

    private void OnPointerEnter(PointerEnterEvent evt) {
      if (props.enabled) props.owner?.PreviewItem(props.index, this);
    }

    [ClickHandler]
    private void OnClick(EventBase evt) {
      if (props.enabled) props.owner?.ActivateItem(props.index, this);
    }
  }

  [EnableMixins]
  [BoundaryElementMixin(name: "DropdownButton")]
  [InputStateListener]
  public partial class HXDropdownButton : IPopupMenuItemOwner {
    public partial struct Props {
      [Prop(null)] public DropdownController controller;
      [Prop(null)] public PopupMenuStyle? style;

      // Implicit controller definition
      [Prop(null)] public object value;
      [Prop(null)] public object options;
      [Prop(null)] public Delegate onChanged;
      [Prop(null)] public Func<DropdownController> controllerFactory;
      [Prop(null)] public string placeholder;
      [Prop(true)] public bool enabled;
      [Prop(false)] public bool error;
    }

    private Composable<OverlayContextData> _menuContent;
    private CompositionAction<OverlayDismissReason> _dismissed;
    private OverlayController _overlays;
    private OverlayHandle _menu;
    private PopupMenuStyle _resolvedStyle;
    private Func<DropdownController> _automaticControllerFactory;
    public DropdownController controller;
    public bool isAutomaticController = true;

    [Hook]
    private void OnInit() {
      _menuContent ??= ComposeMenu;
      _dismissed ??= HandleDismissed;
    }

    [Hook]
    private void OnCompose(ref Composition cx) {
      EnsureController(props.controller);
      cx.SubscribeTo(controller);
      _overlays = cx.Overlays();
      _resolvedStyle = props.style ?? ThemeProperties.DropdownButton[in cx];
      if (_menu?.IsOpen == true) _menu.Replace(_menuContent);
      this.Toggle(State.Selected, _menu?.IsOpen == true);
      this.Toggle(State.Disabled, !controller.enabled);
      this.Toggle(State.Error, controller.error);
      SetEnabled(controller.enabled);
      cx.CURSOR.Focusable(controller.enabled);
      _resolvedStyle.button.RenderBoundary(ref cx, InputState);

      using (cx.Group(Axis.Horizontal, main: Justify.SpaceBetween, cross: Align.Center)) {
        cx.CURSOR.Flexible().AlignSelf(Align.Stretch);
        cx.Text(controller.SelectedLabel);
        cx.Spacing(1);

        new ChevronSpec(
          _menu?.IsOpen == true ? ArrowPosition.Up : ArrowPosition.Down, ThemeProperties.ChevronSize[in cx],
          _resolvedStyle.iconColor
        ).Compose(ref cx);
      }
    }

    [ClickHandler]
    private void OnClick(EventBase evt) {
      if (controller?.enabled != true || _overlays == null) return;
      if (_menu?.IsOpen == true) {
        _menu.Dismiss();
        return;
      }

      _menu = Overlay.Build(_menuContent)
        .AnchorTo(this, OverlayPlacement.BelowStart, _resolvedStyle.offset)
        .MatchAnchorWidth(_resolvedStyle.matchAnchorWidth)
        .DismissOnOutsidePointer()
        .DismissOnCancel()
        .CaptureFocus()
        .OnDismissed(_dismissed)
        .Show(_overlays, this);
      MarkDirty();
    }

    [Hook]
    private void OnDispose() {
      _menu?.Dismiss(OverlayDismissReason.AnchorDetached);
      _menu = null;
      _overlays = null;
      DisposeAutomaticController();
    }

    public void ActivateItem(int index, IBoundary item) {
      if (controller == null || index < 0 || index >= controller.Count || !controller.IsEnabled(index)) return;
      _menu?.Dismiss(OverlayDismissReason.Action);
      controller.SetUserIndex(this, index);
    }

    public void PreviewItem(int index, IBoundary item) { }

    private void ComposeMenu(ref Composition cx, OverlayContextData overlay) {
      var style = props.style ?? ThemeProperties.DropdownButton[in cx];
      style.panel.RenderBoundary(ref cx, State.None);
      using (cx.Group(Axis.Vertical, cross: Align.Stretch)) {
        cx.CURSOR.AlignSelf(Align.Stretch);
        if (controller == null) return;
        for (var i = 0; i < controller.Count; i++) {
          if (i > 0 && style.gap > 0f) cx.Gap(style.gap);
          PopupMenuItemBoundary.ComposeBoundary(
            ref cx, this, i,
            controller.Label(i), controller.IsEnabled(i), controller.IsSelected(i), false, style.item
          );
        }
      }
    }

    private void EnsureController(DropdownController given) {
      if (given == null) {
        var factory = props.controllerFactory;
        if (factory == null) {
          throw new InvalidOperationException("An implicit dropdown requires a controller factory.");
        }
        if (!isAutomaticController || controller == null ||
            !ReferenceEquals(_automaticControllerFactory, factory)) {
          DisposeAutomaticController();
          controller = factory();
          _automaticControllerFactory = factory;
          isAutomaticController = true;
        }
        controller.SynchronizeAutomatic(
          props.value,
          props.options,
          props.onChanged,
          props.placeholder,
          props.enabled,
          props.error
        );
        return;
      }
      if (ReferenceEquals(controller, given)) return;
      DisposeAutomaticController();
      controller = given;
      _automaticControllerFactory = null;
      isAutomaticController = false;
    }

    private void DisposeAutomaticController() {
      if (!isAutomaticController) return;
      controller?.Dispose();
      controller = null;
      _automaticControllerFactory = null;
      isAutomaticController = false;
    }

    private void HandleDismissed(CompositionContext context, OverlayDismissReason reason) {
      _menu = null;
      if (panel != null) MarkDirty();
    }
  }

  [EnableMixins]
  [BoundaryElementMixin(name: "MenuButton", extension: true)]
  [InputStateListener]
  public partial class HXMenuButton {
    public partial struct Props {
      public Composable content;
      public IReadOnlyList<MenuItem> items;
      [Prop(true)] public bool enabled;
      [Prop(false)] public bool selected;
      [Prop(null)] public PopupMenuStyle? style;
    }

    private OverlayController _overlays;
    private MenuPresenter _presenter;
    private PopupMenuStyle _resolvedStyle;

    [Hook]
    private void OnCompose(ref Composition cx) {
      _overlays = cx.Overlays();
      _resolvedStyle = props.style ?? ThemeProperties.MenuButton[in cx];
      if (_presenter != null) {
        if (!props.enabled || props.items == null || props.items.Count == 0) {
          _presenter.Dismiss(OverlayDismissReason.Replaced);
        } else _presenter.Update(props.items, _resolvedStyle);
      }
      this.Toggle(State.Selected, props.selected || _presenter?.IsOpen == true);
      this.Toggle(State.Disabled, !props.enabled);
      SetEnabled(props.enabled);
      cx.CURSOR.Focusable(props.enabled);
      _resolvedStyle.button.RenderBoundary(ref cx, InputState);

      using (cx.Group(Axis.Horizontal, main: Justify.SpaceBetween, cross: Align.Center)) {
        cx.CURSOR.Flexible().AlignSelf(Align.Stretch);
        props.content?.Invoke(ref cx);
        cx.Spacing(1);
        new ChevronSpec(
          _presenter?.IsOpen == true ? ArrowPosition.Up : ArrowPosition.Down, ThemeProperties.ChevronSize[in cx],
          _resolvedStyle.iconColor
        ).Compose(ref cx);
      }
    }

    [ClickHandler]
    private void OnClick(EventBase evt) {
      if (!props.enabled || _overlays == null || props.items == null || props.items.Count == 0) return;
      if (_presenter?.IsOpen == true) {
        _presenter.Dismiss();
        return;
      }

      _presenter = new MenuPresenter(_overlays, props.items, _resolvedStyle, HandleDismissed);
      _presenter.Show(this);
      MarkDirty();
    }

    [Hook]
    private void OnDispose() {
      _presenter?.Dismiss(OverlayDismissReason.AnchorDetached);
      _presenter = null;
      _overlays = null;
    }

    private void HandleDismissed(CompositionContext context, OverlayDismissReason reason) {
      _presenter = null;
      if (panel != null) MarkDirty();
    }
  }

  internal sealed class MenuPresenter : IPopupMenuItemOwner {
    private readonly OverlayController _controller;
    private readonly MenuPresenter _parent;
    private readonly CompositionAction<OverlayDismissReason> _externalDismissed;
    private readonly Composable<OverlayContextData> _content;
    private readonly CompositionAction<OverlayDismissReason> _dismissed;
    private IReadOnlyList<MenuItem> _items;
    private PopupMenuStyle _style;
    private OverlayHandle _handle;
    private MenuPresenter _child;
    private int _childIndex = -1;

    public MenuPresenter(
      OverlayController controller,
      IReadOnlyList<MenuItem> items,
      PopupMenuStyle style,
      CompositionAction<OverlayDismissReason> dismissed,
      MenuPresenter parent = null
    ) {
      _controller = controller;
      _items = items;
      _style = style;
      _parent = parent;
      _externalDismissed = dismissed;
      _content = Compose;
      _dismissed = HandleDismissed;
    }

    public bool IsOpen => _handle?.IsOpen == true;

    public void Update(IReadOnlyList<MenuItem> items, PopupMenuStyle style) {
      if (ReferenceEquals(_items, items)) return;
      _items = items;
      _style = style;
      _child?.Dismiss(OverlayDismissReason.Replaced);
      _handle?.Replace(_content);
    }

    public void Show(VisualElement anchor) {
      var builder = Overlay.Build(_content)
        .AnchorTo(
          anchor,
          _parent == null ? OverlayPlacement.BelowStart : OverlayPlacement.After,
          _parent == null ? _style.offset : _style.submenuOffset
        )
        .MatchAnchorWidth(_parent == null && _style.matchAnchorWidth)
        .DismissOnCancel()
        .CaptureFocus()
        .OnDismissed(_dismissed);

      if (_parent == null) builder.DismissOnOutsidePointer();
      else builder.Parent(_parent._handle);
      _handle = builder.Show(_controller, anchor as IBoundary);
    }

    public void Dismiss(OverlayDismissReason reason = OverlayDismissReason.Manual) => _handle?.Dismiss(reason);

    public void ActivateItem(int index, IBoundary item) {
      if (!TryGetAction(index, out var spec)) return;
      if (spec.children != null && spec.children.Count > 0) {
        ShowChild(index, item.Element);
        return;
      }

      spec.action?.Call(item);
      DismissChain();
    }

    public void PreviewItem(int index, IBoundary item) {
      if (!TryGetAction(index, out var spec)) return;
      if (spec.children == null || spec.children.Count == 0) {
        _child?.Dismiss(OverlayDismissReason.Replaced);
        return;
      }
      ShowChild(index, item.Element);
    }

    private bool TryGetAction(int index, out MenuItem spec) {
      spec = default;
      if (_items == null || index < 0 || index >= _items.Count) return false;
      spec = _items[index];
      return spec is { kind: MenuItemKind.Action, enabled: true };
    }

    private void ShowChild(int index, VisualElement anchor) {
      if (_childIndex == index && _child?.IsOpen == true) return;
      _child?.Dismiss(OverlayDismissReason.Replaced);
      var children = _items[index].children;
      _childIndex = index;
      _child = new MenuPresenter(_controller, children, _style, null, this);
      _child.Show(anchor);
    }

    private void Compose(ref Composition cx, OverlayContextData overlay) {
      _style.panel.RenderBoundary(ref cx, State.None);
      using (cx.Group(Axis.Vertical, cross: Align.Stretch)) {
        cx.CURSOR.AlignSelf(Align.Stretch);

        if (_items == null) return;
        for (var i = 0; i < _items.Count; i++) {
          if (i > 0 && _style.gap > 0f) cx.Gap(_style.gap);
          var item = _items[i];
          switch (item.kind) {
            case MenuItemKind.Separator:
              cx.DrawSolidBox(
                color: _style.separatorColor,
                constraints: BoxConstraints.Preferred(
                  StyleKeyword.Auto, Mathf.Max(0f, _style.separatorThickness)
                )
              );
              break;
            case MenuItemKind.Heading:
              ref var heading = ref cx.Text(item.label ?? string.Empty);
              heading.Padding(_style.headingPadding);
              _style.headingTextStyle.Apply(heading.composable);
              break;
            default:
              PopupMenuItemBoundary.ComposeBoundary(
                ref cx, this, i,
                item.label ?? string.Empty, item.enabled, item.selected, item.children is { Count: > 0 },
                _style.item
              );
              break;
          }
        }
      }
    }

    private void DismissChain() {
      var current = this;
      while (current != null) {
        var parent = current._parent;
        current.Dismiss(OverlayDismissReason.Action);
        current = parent;
      }
    }

    private void HandleDismissed(CompositionContext context, OverlayDismissReason reason) {
      _handle = null;
      var child = _child;
      _child = null;
      _childIndex = -1;
      child?.Dismiss(OverlayDismissReason.Replaced);
      if (_parent != null && ReferenceEquals(_parent._child, this)) {
        _parent._child = null;
        _parent._childIndex = -1;
      }
      _externalDismissed?.Invoke(context, reason);
    }
  }

  public static class ManualDropdownButtonExtensions {
    private static class AutomaticController<T> {
      public static readonly Func<DropdownController> Factory = Create;

      private static DropdownController Create() => new DropdownController<T>();
    }

    public static ref ElementRef DropdownButton(
      this ref Composition cx,
      DropdownController controller,
      PopupMenuStyle? style = null
    ) {
      if (controller == null) throw new ArgumentNullException(nameof(controller));
      return ref HXDropdownButton.ComposeBoundary(ref cx, controller, style);
    }

    public static ref ElementRef DropdownButton<T>(
      this ref Composition cx,
      T value,
      IReadOnlyList<DropdownOption<T>> options,
      CompositionAction<T> onChanged = null,
      string placeholder = null,
      bool enabled = true,
      bool error = false,
      PopupMenuStyle? style = null
    ) => ref HXDropdownButton.ComposeBoundary(
      ref cx,
      controller: null,
      style: style,
      value: value,
      options: options,
      onChanged: onChanged,
      controllerFactory: AutomaticController<T>.Factory,
      placeholder: placeholder,
      enabled: enabled,
      error: error
    );
  }
}
