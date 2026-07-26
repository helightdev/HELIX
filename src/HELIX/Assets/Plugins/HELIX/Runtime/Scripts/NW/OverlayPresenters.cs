using System;
using System.Collections.Generic;
using HELIX.Coloring;
using HELIX.Types;
using TextMateSharp.Themes;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.NW.Overlays {
  public sealed class OverlayPanelStyle {
    public static readonly OverlayPanelStyle Default = BuildDefault(BuiltinThemes.DefaultDark);
    public static readonly ContextKey<OverlayPanelStyle> Context =
      new("NW.OverlayPanelStyle", Default);

    public readonly InputFieldStyle surface;
    public readonly ControlBoxStyle item;
    public readonly float gap;

    public OverlayPanelStyle(
      InputFieldStyle surface,
      ControlBoxStyle item,
      float gap = 2f
    ) {
      this.surface = surface ?? InputFieldStyle.Default;
      this.item = item.padding == null &&
                  item.margin == null &&
                  item.alignment == null &&
                  item.constraints == null &&
                  item.textStyle == null &&
                  item.background == null
        ? ControlBoxStyle.Default
        : item;
      this.gap = Mathf.Max(0f, gap);
    }

    public static OverlayPanelStyle BuildDefault(ThemeData theme) {
      return new OverlayPanelStyle(
        new InputFieldStyle(
          padding: StyleLength4.All(4f),
          constraints: BoxConstraints.Min(new StyleLength2(120f, 0f)),
          background: new DrawSolidBoxStyle(
            border: Border.All(1f, theme.GetColor(ColorRoles.Outline)),
            radius: BorderRadius.All(6f),
            color: theme.GetColor(ColorRoles.SurfaceContainerHigh)
          ).Bake()
        ),
        CommonShapes.ToggleControlBox(
          theme,
          constraints: BoxConstraints.Min(new StyleLength2(30f)),
          paddingHorizontal: SpacingRole.Spacing2,
          paddingVertical: SpacingRole.Spacing1
        )
      );
    }
  }

  public static partial class OverlayPanelDefinition {
    [CompositionBoundary]
    public static partial ref ElementRef OverlayPanel(
      ref this Composition cx,
      [Prop] Composable content,
      [Prop] OverlayPanelStyle style = null
    );

    public partial class OverlayPanelState {
      protected override void OnRecompose(ref Composition cx) {
        var style = Props.Style ??
                    cx.ReadContextOrDefault(OverlayPanelStyle.Context, OverlayPanelStyle.Default);
        style.surface.RenderBoundary(ref cx, StateFlag.None);
        Props.Content?.Invoke(ref cx);
      }
    }
  }

  public readonly struct DropdownOption<T> {
    public readonly T value;
    public readonly string label;
    public readonly bool enabled;

    public DropdownOption(T value, string label, bool enabled = true) {
      this.value = value;
      this.label = label;
      this.enabled = enabled;
    }
  }

  internal struct DropdownProps<T> {
    public T value;
    public IReadOnlyList<DropdownOption<T>> options;
    public Action<T, IBoundary> onChanged;
    public string placeholder;
    public bool enabled;
    public bool error;
    public ControlBoxStyle? style;
    public OverlayPanelStyle menuStyle;
    public OverlayOptions? overlayOptions;
  }

  internal static class DropdownIdentity<T> {
    public static readonly ushort TypeId = CompositionId.GetTypeId($"Dropdown<{typeof(T).FullName}>");
  }

  public interface IOverlayActionItemOwner {
    void ActivateOverlayItem(int index, VisualElement item);
  }

  internal sealed class DropdownState<T> :
    BuiltIns.InputClickableBase<DropdownProps<T>>,
    IOverlayActionItemOwner {
    private static readonly EqualityComparer<T> _equality = EqualityComparer<T>.Default;

    private readonly OverlayComposable _menuContent;
    private readonly Action<OverlayHandle, OverlayDismissReason> _dismissed;
    private OverlayController _overlays;
    private OverlayHandle _menu;

    public DropdownState() {
      _menuContent = ComposeMenu;
      _dismissed = HandleDismissed;
    }

    protected override void OnRecompose(ref Composition cx) {
      _overlays = cx.ReadContext(OverlayContextData.Key).controller;
      this.Toggle(StateFlag.Disabled, !Props.enabled);
      this.Toggle(StateFlag.Error, Props.error);
      Node.SetEnabled(Props.enabled);
      cx.APPLY.Focusable(Props.enabled);

      var style = Props.style ?? ControlBoxStyle.Default;
      style.RenderBoundary(ref cx, InputState);
      using (cx.Flex(Axis.Horizontal, main: Justify.SpaceBetween, cross: Align.Center)) {
        cx.APPLY.Flexible().AlignSelf(Align.Stretch);

        cx.Text(SelectedLabel());
        cx.Spacing(2);
        var isShown = _menu is { IsShown: true };
        cx.Spec(new ChevronSpec(isShown ? ArrowPosition.Up : ArrowPosition.Down, 12, ThemeData.Context.ReadScope()[ColorRoles.OnSurfaceVariant]));
      }
    }

    protected override void OnClick(EventBase evt) {
      if (!Props.enabled || _overlays == null) return;
      if (_menu != null && !_menu.DismissReason.HasValue) {
        _menu.Dismiss(OverlayDismissReason.Manual);
        return;
      }

      var options = Props.overlayOptions ?? OverlayOptions.Popover();
      var spec = new OverlaySpec(
        OverlayKind.Menu,
        _menuContent,
        Node,
        options,
        _dismissed
      );
      _menu = _overlays.Show(in spec);
    }

    protected override void OnDetach() {
      _menu?.Dismiss(OverlayDismissReason.AnchorDetached);
      _menu = null;
      _overlays = null;
      base.OnDetach();
    }

    public override void ReceiveProps(DropdownProps<T> props) {
      base.ReceiveProps(props);
      if (_menu?.Entry?.boundary != null) _menu.Entry.boundary.MarkDirty();
    }

    public void ActivateOverlayItem(int index, VisualElement item) {
      var options = Props.options;
      if (options == null || index < 0 || index >= options.Count || !options[index].enabled) return;
      var option = options[index];
      var callback = Props.onChanged;
      _menu?.Dismiss(OverlayDismissReason.Action);
      callback?.Invoke(option.value, Node);
    }

    private string SelectedLabel() {
      var options = Props.options;
      if (options != null) {
        for (var i = 0; i < options.Count; i++) {
          if (_equality.Equals(options[i].value, Props.value)) return options[i].label ?? string.Empty;
        }
      }
      return Props.placeholder ?? string.Empty;
    }

    private void ComposeMenu(ref Composition cx, OverlayHandle handle) {
      var style = Props.menuStyle ??
                  cx.ReadContextOrDefault(OverlayPanelStyle.Context, OverlayPanelStyle.Default);
      style.surface.RenderBoundary(ref cx, StateFlag.None);
      if (Node.panel == cx.boundary.Element.panel) {
        var anchorWidth = Node.resolvedStyle.width;
        if (!float.IsNaN(anchorWidth) && anchorWidth > 0f) {
          cx.boundary.Element.style.minWidth = anchorWidth;
          cx.boundary.Flag |= UssFlag.Size;
        }
      }

      using (cx.Flex(Axis.Vertical, cross: Align.Stretch)) {
        var options = Props.options;
        if (options == null) return;
        for (var i = 0; i < options.Count; i++) {
          var option = options[i];
          if (i > 0) cx.Gap(style.gap);
          cx.OverlayActionItem(
            this,
            i,
            option.label ?? string.Empty,
            option.enabled,
            _equality.Equals(option.value, Props.value),
            false,
            style.item
          );
        }
      }
    }

    private void HandleDismissed(OverlayHandle handle, OverlayDismissReason reason) {
      if (ReferenceEquals(_menu, handle)) _menu = null;
    }
  }

  public static partial class DropdownDefinition {
    public static ref ElementRef Dropdown<T>(
      this ref Composition cx,
      T value,
      IReadOnlyList<DropdownOption<T>> options,
      Action<T, IBoundary> onChanged = null,
      string placeholder = null,
      bool enabled = true,
      bool error = false,
      ControlBoxStyle? style = null,
      OverlayPanelStyle menuStyle = null,
      OverlayOptions? overlayOptions = null
    ) {
      cx.AUTHORING.PropsBoundaryStateNode<DropdownState<T>, DropdownProps<T>>(
        DropdownIdentity<T>.TypeId,
        out var node,
        out _,
        out var attachment
      );
      attachment.ReceiveProps(new DropdownProps<T> {
        value = value,
        options = options,
        onChanged = onChanged,
        placeholder = placeholder,
        enabled = enabled,
        error = error,
        style = style,
        menuStyle = menuStyle,
        overlayOptions = overlayOptions
      });
      node.composable = null;
      return ref cx.AUTHORING.YieldBoundary(ref cx, node);
    }
  }

  public enum MenuItemKind : byte {
    Action,
    Separator,
    Heading
  }

  public readonly struct MenuItemSpec {
    public readonly MenuItemKind kind;
    public readonly string label;
    public readonly Action action;
    public readonly bool enabled;
    public readonly bool selected;
    public readonly IReadOnlyList<MenuItemSpec> children;

    public MenuItemSpec(
      string label,
      Action action = null,
      bool enabled = true,
      bool selected = false,
      IReadOnlyList<MenuItemSpec> children = null
    ) {
      kind = MenuItemKind.Action;
      this.label = label;
      this.action = action;
      this.enabled = enabled;
      this.selected = selected;
      this.children = children;
    }

    private MenuItemSpec(MenuItemKind kind, string label) {
      this.kind = kind;
      this.label = label;
      action = null;
      enabled = false;
      selected = false;
      children = null;
    }

    public static MenuItemSpec Separator() => new(MenuItemKind.Separator, null);
    public static MenuItemSpec Heading(string label) => new(MenuItemKind.Heading, label);
  }

  public readonly struct NotificationSpec {
    public readonly string title;
    public readonly string message;
    public readonly string actionLabel;
    public readonly Action action;
    public readonly bool dismissible;

    public NotificationSpec(
      string message,
      string title = null,
      string actionLabel = null,
      Action action = null,
      bool dismissible = true
    ) {
      this.title = title;
      this.message = message;
      this.actionLabel = actionLabel;
      this.action = action;
      this.dismissible = dismissible;
    }
  }

  internal sealed class MenuPresenter : IOverlayActionItemOwner {
    private readonly OverlayController _controller;
    private readonly IReadOnlyList<MenuItemSpec> _items;
    private readonly OverlayPanelStyle _style;
    private readonly OverlayComposable _content;
    private readonly Action<OverlayHandle, OverlayDismissReason> _dismissed;
    private readonly Action<OverlayHandle, OverlayDismissReason> _externalDismissed;
    private readonly MenuPresenter _parent;
    private OverlayHandle _handle;
    private MenuPresenter _child;

    public MenuPresenter(
      OverlayController controller,
      IReadOnlyList<MenuItemSpec> items,
      OverlayPanelStyle style,
      MenuPresenter parent = null,
      Action<OverlayHandle, OverlayDismissReason> dismissed = null
    ) {
      _controller = controller;
      _items = items;
      _style = style ?? OverlayPanelStyle.Default;
      _parent = parent;
      _externalDismissed = dismissed;
      _content = Compose;
      _dismissed = HandleDismissed;
    }

    public OverlayComposable Content => _content;
    public Action<OverlayHandle, OverlayDismissReason> Dismissed => _dismissed;

    public void SetHandle(OverlayHandle handle) {
      _handle = handle;
    }

    public void ActivateOverlayItem(int index, VisualElement item) {
      if (_items == null || index < 0 || index >= _items.Count) return;
      var spec = _items[index];
      if (spec.kind != MenuItemKind.Action || !spec.enabled) return;

      if (spec.children != null && spec.children.Count > 0) {
        _child?._handle?.Dismiss(OverlayDismissReason.Replaced);
        _child = new MenuPresenter(_controller, spec.children, _style, this);
        var options = new OverlayOptions(
          OverlayPlacement.Right,
          dismissOnOutsidePointer: true,
          dismissOnCancel: true,
          flip: true,
          clampToHost: true
        );
        var overlaySpec = new OverlaySpec(
          OverlayKind.Menu,
          _child.Content,
          item,
          options,
          _child.Dismissed
        );
        _child.SetHandle(_controller.Show(in overlaySpec));
        return;
      }

      spec.action?.Invoke();
      DismissChain();
    }

    private void Compose(ref Composition cx, OverlayHandle handle) {
      _style.surface.RenderBoundary(ref cx, StateFlag.None);
      using (cx.Flex(Axis.Vertical, cross: Align.Stretch)) {
        if (_items == null) return;
        for (var i = 0; i < _items.Count; i++) {
          var item = _items[i];
          if (i > 0) cx.Gap(_style.gap);
          switch (item.kind) {
            case MenuItemKind.Separator:
              cx.DrawSolidBox(
                color: Colors.White10,
                constraints: BoxConstraints.Preferred(StyleKeyword.Auto, 1f)
              );
              break;
            case MenuItemKind.Heading:
              cx.Text(item.label ?? string.Empty);
              cx.APPLY.Padding(StyleLength4.Symmetric(horizontal: 8f, vertical: 4f));
              break;
            default:
              cx.OverlayActionItem(
                this,
                i,
                item.label ?? string.Empty,
                item.enabled,
                item.selected,
                item.children is { Count: > 0 },
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
        current._handle?.Dismiss(OverlayDismissReason.Action);
        current = current._parent;
      }
    }

    private void HandleDismissed(OverlayHandle handle, OverlayDismissReason reason) {
      _child?._handle?.Dismiss(OverlayDismissReason.Replaced);
      _child = null;
      _externalDismissed?.Invoke(handle, reason);
      if (_parent != null && ReferenceEquals(_parent._child, this)) _parent._child = null;
    }
  }

  internal sealed class NotificationPresenter : IOverlayActionItemOwner {
    private readonly NotificationSpec _spec;
    private readonly OverlayPanelStyle _style;
    private readonly OverlayComposable _content;
    private OverlayHandle _handle;

    public NotificationPresenter(in NotificationSpec spec, OverlayPanelStyle style) {
      _spec = spec;
      _style = style ?? OverlayPanelStyle.Default;
      _content = Compose;
    }

    public OverlayComposable Content => _content;

    public void SetHandle(OverlayHandle handle) {
      _handle = handle;
    }

    public void ActivateOverlayItem(int index, VisualElement item) {
      if (index == 0) _spec.action?.Invoke();
      _handle?.Dismiss(OverlayDismissReason.Action);
    }

    private void Compose(ref Composition cx, OverlayHandle handle) {
      _style.surface.RenderBoundary(ref cx, StateFlag.None);
      cx.boundary.Element.style.maxWidth = 480f;
      cx.boundary.Flag |= UssFlag.Size;
      using (cx.Flex(Axis.Vertical, cross: Align.Stretch)) {
        if (!string.IsNullOrEmpty(_spec.title)) cx.Text(_spec.title);
        cx.Text(_spec.message ?? string.Empty);
        if (!string.IsNullOrEmpty(_spec.actionLabel)) {
          cx.OverlayActionItem(
            this,
            0,
            _spec.actionLabel,
            _spec.action != null,
            false,
            false,
            _style.item
          );
        }
        if (_spec.dismissible) {
          cx.OverlayActionItem(this, 1, "Dismiss", true, false, false, _style.item);
        }
      }
    }
  }

  public static class OverlayControllerPresenters {
    public static OverlayHandle ShowMenu(
      this OverlayController controller,
      VisualElement anchor,
      IReadOnlyList<MenuItemSpec> items,
      OverlayOptions? options = null,
      OverlayPanelStyle style = null,
      Action<OverlayHandle, OverlayDismissReason> dismissed = null
    ) {
      if (controller == null) throw new ArgumentNullException(nameof(controller));
      var presenter = new MenuPresenter(controller, items, style, dismissed: dismissed);
      var spec = new OverlaySpec(
        OverlayKind.Menu,
        presenter.Content,
        anchor,
        options ?? OverlayOptions.Popover(),
        presenter.Dismissed
      );
      var handle = controller.Show(in spec);
      presenter.SetHandle(handle);
      return handle;
    }

    public static OverlayHandle ShowNotification(
      this OverlayController controller,
      in NotificationSpec notification,
      int durationMs = 4000,
      OverlayPlacement placement = OverlayPlacement.Bottom,
      OverlayPanelStyle style = null,
      Action<OverlayHandle, OverlayDismissReason> dismissed = null
    ) {
      if (controller == null) throw new ArgumentNullException(nameof(controller));
      var presenter = new NotificationPresenter(in notification, style);
      var handle = controller.ShowNotification(
        presenter.Content,
        durationMs,
        placement,
        dismissed
      );
      presenter.SetHandle(handle);
      return handle;
    }
  }

  public static partial class OverlayActionItemDefinition {
    [CompositionBoundary(Base = typeof(BuiltIns.InputClickableBase<>))]
    public static partial ref ElementRef OverlayActionItem(
      ref this Composition cx,
      [Prop] IOverlayActionItemOwner owner,
      [Prop] int index,
      [Prop] string label,
      [Prop] bool enabled,
      [Prop] bool selected,
      [Prop] bool hasChildren,
      [Prop] ControlBoxStyle style
    );

    public partial class OverlayActionItemState {
      protected override void OnRecompose(ref Composition cx) {
        this.Toggle(StateFlag.Disabled, !Props.Enabled);
        this.Toggle(StateFlag.Selected, Props.Selected);
        Node.SetEnabled(Props.Enabled);
        cx.APPLY.Focusable(Props.Enabled);
        Props.Style.RenderBoundary(ref cx, InputState);
        using (cx.Flex(Axis.Horizontal, main: Justify.SpaceBetween, cross: Align.Center)) {
          cx.Text(Props.Label ?? string.Empty);
          if (Props.HasChildren) cx.Text("›");
        }
      }

      protected override void OnClick(EventBase evt) {
        if (Props.Enabled) Props.Owner?.ActivateOverlayItem(Props.Index, Node);
      }
    }
  }
}
