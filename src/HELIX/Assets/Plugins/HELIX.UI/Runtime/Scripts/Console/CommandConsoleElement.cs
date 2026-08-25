using System.Collections.Generic;
using System.Collections;
using HELIX.Compose;
using HELIX.Theming;
using HELIX.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.UI.Console {
  /// <summary>A Compose console whose command-line behavior is entirely implemented by a TextEditProcessor.</summary>
  public sealed class CommandConsoleElement : BoundaryVisualElement {
    private readonly List<HistoryEntry> _output = new();
    private readonly List<string> _history = new();
    private TextEditingValue _input = TextEditingValue.Empty;
    private ICommandSystem _system;
    private int _historyIndex;
    private VisualElement _previousFocus;
    private ConsoleHistoryListElement _historyView;
    private IVisualElementScheduledItem _recomposeRequest;

    public CommandConsoleElement() {
      style.position = Position.Absolute; style.left = 0; style.right = 0; style.top = 0;
      style.bottom = 0; style.backgroundColor = new Color(.018f, .022f, .03f, .96f);
      style.paddingLeft = 12; style.paddingRight = 12; style.paddingTop = 10; style.paddingBottom = 10;
      style.display = DisplayStyle.None;
    }

    public void Bind(ICommandSystem system) {
      if (ReferenceEquals(_system, system)) return;
      _system = system;
      RequestRecompose();
    }
    public void Toggle() => SetVisible(resolvedStyle.display == DisplayStyle.None);
    public void SetVisible(bool visible) {
      if (visible) {
        _previousFocus = panel?.focusController.focusedElement as VisualElement;
        style.display = DisplayStyle.Flex;
        RequestInputFocus();
      } else {
        style.display = DisplayStyle.None;
        if (_previousFocus?.panel != null) _previousFocus.Focus();
        _previousFocus = null;
      }
    }

    public override void PerformCompose(ref Composition cx) => Compose(ref cx);

    public override void Compose(ref Composition cx) {
      using (cx.Column(cross: Align.Stretch).With(Flex.FillFlexible())) {
        _historyView = ConsoleHistoryComposition.Compose(ref cx, _output);
        var completion = _system?.Complete(_input.text);
        var help = completion?.help;
        if (string.IsNullOrEmpty(help) && completion.HasValue && completion.Value.items.Count > 0)
          help = string.Join("  ", completion.Value.items);
        if (cx.Conditional(!string.IsNullOrEmpty(help))) {
          var theme = cx.ReadContextOrDefault(ThemeData.Key, HXThemes.DefaultDark);
          using (cx.Column(cross: Align.Stretch)) {
            cx.CURSOR.FlexShrink(0);
            if (cx.CursorDirty)
              cx.CURSOR.Padding(8f)
                .BackgroundColor(theme.GetColor(ColorRoles.SurfaceContainerHighest))
                .TextColor(theme.GetColor(ColorRoles.OnSurface));
            cx.Text(help);
          }
        }
        cx.TextField(value: _input, onChanged: InputChanged, processor: ProcessInput)
          .FlexShrink(0);
      }
    }

    private static void InputChanged(CompositionContext context, TextEditingValue value) {
      var self = context.Lookup<CommandConsoleElement>();
      if (self == null) return;
      self.schedule.Execute(() => {
        if (self.panel == null) return;
        self._input = value;
        HXComposer.MarkDirty(self, false);
      }).ExecuteLater(1);
    }

    private void ProcessInput(ref TextEditProcessorContext context) {
      if (context.trigger.type != TextEditTriggerType.RawKeyDownEvent) return;
      switch (context.trigger.keyCode) {
        case KeyCode.Return:
        case KeyCode.KeypadEnter:
          var command = context.next.text.Trim();
          if (command.Length != 0) {
            schedule.Execute(() => {
              if (panel != null) Execute(command).Forget();
            }).ExecuteLater(1);
          }
          context.next = TextEditingValue.Empty;
          context.result = TextEditResult.Continue(true);
          break;
        case KeyCode.Tab:
          Complete(ref context);
          context.result = TextEditResult.Continue(true);
          break;
        case KeyCode.UpArrow:
          NavigateHistory(-1, ref context); context.result = TextEditResult.Continue(true); break;
        case KeyCode.DownArrow:
          NavigateHistory(1, ref context); context.result = TextEditResult.Continue(true); break;
      }
    }

    private void Complete(ref TextEditProcessorContext context) {
      var completion = _system?.Complete(context.next.text);
      if (!completion.HasValue || completion.Value.items.Count != 1) return;
      var text = context.next.text; var cut = text.LastIndexOf(' ');
      context.next = new TextEditingValue((cut < 0 ? "" : text.Substring(0, cut + 1)) + completion.Value.items[0] + " ");
      context.next = context.next.SetSelection(context.next.text.Length, context.next.text.Length);
    }

    private void NavigateHistory(int delta, ref TextEditProcessorContext context) {
      if (_history.Count == 0) return;
      _historyIndex = Mathf.Clamp(_historyIndex + delta, 0, _history.Count);
      var text = _historyIndex == _history.Count ? "" : _history[_historyIndex];
      context.next = new TextEditingValue(text, text.Length, text.Length);
    }

    private async Cysharp.Threading.Tasks.UniTaskVoid Execute(string input) {
      if (_system == null) return;
      _history.Add(input); _historyIndex = _history.Count; AppendOutput("› " + input);
      var result = await _system.Execute(input);
      if (result.HasMessage) AppendOutput(result.message.ToString());
      _input = TextEditingValue.Empty;
      RequestRecompose();
      RequestInputFocus();
    }

    private void AppendOutput(string value) {
      if (value == null) return;
      var lines = value.Replace("\r\n", "\n").Split('\n');
      for (var i = 0; i < lines.Length; i++) _output.Add(new HistoryEntry(lines[i]));
      _historyView?.Refresh();
    }

    private void RequestRecompose() {
      if (panel == null) return;
      _recomposeRequest?.Pause();
      _recomposeRequest = schedule.Execute(() => {
        _recomposeRequest = null;
        if (panel != null) HXComposer.MarkDirty(this, false);
      });
      _recomposeRequest.ExecuteLater(1);
    }
    private void RequestInputFocus() => schedule.Execute(() => {
      if (panel == null || resolvedStyle.display == DisplayStyle.None) return;
      this.Q<TextFieldElement>()?.TextEdition.Focus();
    }).ExecuteLater(1);
    internal readonly struct HistoryEntry { public readonly string text; public HistoryEntry(string text) => this.text = text; }
  }

  internal sealed class ConsoleHistoryListElement : ListView {
    private IList _items;

    public ConsoleHistoryListElement() {
      selectionType = SelectionType.None;
      virtualizationMethod = CollectionVirtualizationMethod.FixedHeight;
      fixedItemHeight = 28f;
      reorderable = false;
      showAlternatingRowBackgrounds = AlternatingRowBackground.None;
      makeItem = MakeItem;
      bindItem = BindItem;
      style.flexGrow = 1f;
      style.flexShrink = 1f;
      style.minHeight = 0;
    }

    public void Bind(IList items) {
      if (ReferenceEquals(_items, items)) return;
      _items = items;
      itemsSource = items;
      RefreshItems();
    }

    public void Refresh() {
      RefreshItems();
      if (_items?.Count > 0) ScrollToItem(_items.Count - 1);
    }

    private static VisualElement MakeItem() {
      var label = new Label { pickingMode = PickingMode.Ignore };
      label.style.paddingLeft = 6f;
      label.style.paddingRight = 6f;
      label.style.unityTextAlign = TextAnchor.MiddleLeft;
      return label;
    }

    private void BindItem(VisualElement element, int index) {
      if (element is Label label && _items?[index] is CommandConsoleElement.HistoryEntry entry)
        label.text = entry.text;
    }
  }

  internal static class ConsoleHistoryComposition {
    private static readonly ushort HistoryId = CompositionId.GetTypeId(nameof(ConsoleHistoryListElement));

    public static ConsoleHistoryListElement Compose(ref Composition cx, IList items) {
      if (!cx.AUTHORING.RequireTracked<ConsoleHistoryListElement>(HistoryId, out var element, out _))
        element = new ConsoleHistoryListElement();
      element.Bind(items);
      cx.AUTHORING.YieldElement(ref cx, element);
      return element;
    }
  }
}
