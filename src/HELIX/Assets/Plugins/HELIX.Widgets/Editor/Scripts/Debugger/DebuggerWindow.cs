using System.Collections.Generic;
using System.Linq;
using System.Text;
using HELIX.Coloring;
using HELIX.Coloring.Material;
using HELIX.Extensions;
using HELIX.Types;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Modifiers;
using HELIX.Widgets.Scrolling;
using HELIX.Widgets.Signals;
using HELIX.Widgets.Universal;
using HELIX.Widgets.Universal.Controllers;
using HELIX.Widgets.Universal.Styles;
using HELIX.Widgets.Universal.Substances;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Editor.Debugger {
  public class DebuggerWindow : EditorWindow {
    private void CreateGUI() {
      rootVisualElement.Add(
        new WidgetHostElement {
          Buildable = new DebuggerWidget().Stretch().ToBuildable()
        }.Stretched()
      );
    }

    [MenuItem("HELIX/Debugger")]
    private static void ShowWindow() {
      var window = GetWindow<DebuggerWindow>();
      window.titleContent = new GUIContent("Widget Debugger");
      window.Show();
    }
  }

  public class DebuggerWidget : StatefulWidget<DebuggerWidget> {
    public override State<DebuggerWidget> CreateState() {
      return new DebuggerWidgetState();
    }
  }

  public class DebuggerWidgetState : State<DebuggerWidget> {
    private readonly HButtonStyle _buttonStyle = new() {
      textStyle = new WidgetStatePropertyMap<TextStyle> {
        [WidgetState.Hovered] = new TextStyle { color = Colors.White },
        [WidgetState.None] = new TextStyle { color = Colors.White80 }
      },
      padding = EdgeInsets.Symmetric(8, 4),
      layers = new BoxSubstance {
        background = new WidgetStatePropertyMap<BackgroundStyle> {
          [WidgetState.Pressed] = new BackgroundStyle { color = Colors.White50 },
          [WidgetState.Hovered] = new BackgroundStyle { color = Colors.White30 },
          [WidgetState.None] = new BackgroundStyle { color = Colors.White10 }
        }
      }
    };

    private readonly List<DiagnosticsNode> _hosts = new();
    private readonly ScrollController _scrollControllerHorizontal = new();
    private readonly ScrollController _scrollControllerVertical = new();

    private readonly HSliderStyle _sliderStyle = new() {
      constraints = new WidgetStatePropertyMap<BoxConstraints> {
        [WidgetState.Special1] = BoxConstraints.Tight(StyleKeyword.Auto, 8),
        [WidgetState.Special2] = BoxConstraints.Tight(8, StyleKeyword.Auto)
      },
      track = new BoxSubstance { background = new BackgroundStyle { color = Colors.White30 } },
      progress = new SubstanceLayers(),
      thumb = new BoxSubstance { background = new BackgroundStyle { color = Colors.White50 } }
    };

    public Signal<DiagnosticsNode> inspectedSignal = Signal.Value<DiagnosticsNode>();

    public override Widget Build(BuildContext context) {
      return new HRow(crossAxisAlign: Align.Stretch) {
        new HColumn(crossAxisAlign: Align.Stretch) {
          new HRow {
            new HText("Hierarchy").Body(context), //
            new HBox().Expand(), //
            new HButton(
              style: _buttonStyle,
              onClick: () => {
                _hosts.Clear();
                _hosts.AddRange(
                  WidgetHostElement.Instances
                    .Where(x => !x.Element.Contains(context.Element))
                    .Select(x => x.ToDiagnosticsNodeSafe())
                );
                SetState();
              }
            ) { new HText("Refresh") }
          }.Padding(8),
          new HPanView(
            verticalController: _scrollControllerVertical,
            horizontalController: _scrollControllerHorizontal,
            children: _hosts.Select(x => new NodeTreeWidget {
                node = x,
                inspectedSignal = inspectedSignal
              }.Tight()
            ).ToArray()
          ).Fill().WithModifier(PaddingModifier.Only(8)),
          new HSlider(_scrollControllerHorizontal, style: _sliderStyle, axis: Axis.Horizontal).Const()
        }.Tight().Size(300, StyleKeyword.Auto),
        new HSlider(_scrollControllerVertical, style: _sliderStyle).Const(),
        new NodeViewerWidget { inspectedSignal = inspectedSignal }.Fill()
          .WithModifier(MarginModifier.Only(16)).Const()
      }.Stretch();
    }
  }

  public class NodeTreeWidget : StatefulWidget<NodeTreeWidget> {
    public Signal<DiagnosticsNode> inspectedSignal;
    public bool isScaffolding;

    public DiagnosticsNode node;

    public override State<NodeTreeWidget> CreateState() {
      return new NodeTreeWidgetState();
    }
  }

  public class NodeTreeWidgetState : State<NodeTreeWidget> {
    private static readonly WidgetStateProperty<TextStyle> _textStyle = new WidgetStatePropertyMap<TextStyle> {
      [WidgetState.Selected] = new TextStyle { color = MaterialColors.Amber },
      [WidgetState.Hovered] = new TextStyle { color = Colors.White },
      [WidgetState.Special1] = new TextStyle { color = Colors.White30 },
      [WidgetState.Special2] = new TextStyle { color = Colors.White60 },
      [WidgetState.None] = new TextStyle { color = Colors.White90 }
    };

    private NodeTreeWidget[] _children;
    private ButtonController _controller;
    private bool _toggled = true;
    private WidgetStateController _widgetStateController;

    public override void InitState() {
      base.InitState();
      _widgetStateController = new WidgetStateController();
      _controller = new ButtonController(_widgetStateController);
      _controller.onClick = () => {
        if (widget.node == null) return;
        if (_widgetStateController.PeekValue().Selected()) {
          _toggled = !_toggled;
          _widgetStateController.Toggle(WidgetState.Special1, !_toggled);
          SetState();
        }

        widget.inspectedSignal.Value = widget.node;
      };

      DidUpdateWidget(null);
    }

    public override void DidUpdateWidget(NodeTreeWidget oldWidget) {
      base.DidUpdateWidget(oldWidget);
      if (oldWidget != null && oldWidget.node == widget.node) return;

      var nodes = widget.node.GetChildren();
      var isSingleChild = nodes.Count == 1;
      _children = nodes
        .Where(x => x != widget.node)
        .Select(x => new NodeTreeWidget {
            node = x,
            inspectedSignal = widget.inspectedSignal,
            isScaffolding = isSingleChild
          }
        )
        .ToArray();
      _widgetStateController.Toggle(WidgetState.Special2, widget.isScaffolding);
    }

    public override Widget Build(BuildContext context) {
      _widgetStateController.Toggle(
        WidgetState.Selected,
        Equals(widget.inspectedSignal.Value?.Value, widget.node?.Value)
      );

      var text = new StringBuilder();
      if (widget.isScaffolding) text.Append("-> ");
      text.Append(widget.node?.ToDescription());

      var self = new HStatefulBuilder((_, _) =>
        new HText(
            text.ToString(),
            style: _textStyle.ResolveOrDefault(_widgetStateController.Value)
          ).WithModifier(new ButtonControllerModifier(_controller))
          .WithModifier(new WidgetStateModifier(_widgetStateController))
      );

      if (_children.Length == 0) return self;

      var isSingleChild = _children.Length == 1;
      var childPadding = 16f;
      if (!widget.isScaffolding && isSingleChild) childPadding = 0f;
      else if (widget.isScaffolding && isSingleChild) childPadding = 0f;

      return new HColumn(crossAxisAlign: Align.FlexStart) {
        self,
        new HColumn(gap: 8f, crossAxisAlign: Align.FlexStart, children: _children)
          .Margin(EdgeInsets.Only(childPadding))
          .Display(_toggled)
      };
    }
  }

  public class NodeViewerWidget : StatefulWidget<NodeViewerWidget> {
    public Signal<DiagnosticsNode> inspectedSignal;

    public override State<NodeViewerWidget> CreateState() {
      return new NodeViewerWidgetState();
    }
  }

  public class NodeViewerWidgetState : State<NodeViewerWidget> {
    public override Widget Build(BuildContext context) {
      var node = widget.inspectedSignal.Value;
      if (node == null) return new HText("No node selected").Heading(context);

      var properties = node.GetProperties();
      return new HColumn(crossAxisAlign: Align.Stretch, gap: 16) {
        new HText(node.ToDescription()).Heading(context),
        new HColumn(
          gap: 4,
          crossAxisAlign: Align.FlexStart,
          children: properties.Select(x => new HText(
              $"{x.Name}: {x.Value}",
              style: new TextStyle { wrap = WhiteSpace.Normal }
            )
          ).ToArray()
        ).Margin(EdgeInsets.Only(16))
      }.TightStretch();
    }
  }
}