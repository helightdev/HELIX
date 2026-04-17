using System;
using System.Collections.Generic;
using System.Linq;
using HELIX;
using HELIX.Coloring;
using HELIX.Coloring.Material;
using HELIX.Extensions;
using HELIX.Types;
using HELIX.Widgets;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Properties;
using HELIX.Widgets.Elements;
using HELIX.Widgets.Modifiers;
using HELIX.Widgets.Navigation;
using HELIX.Widgets.Scrolling;
using HELIX.Widgets.Signals;
using HELIX.Widgets.Universal;
using HELIX.Widgets.Universal.Controllers;
using HELIX.Widgets.Universal.Styles;
using HELIX.Widgets.Universal.Substances;
using HELIX.Widgets.Universal.Theme;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

[UxmlElement]
public partial class NewTest : WidgetHostElement {
    public NewTest() {
        Buildable = new NavStack { child = new Scaffold { child = new ScrollExample() } }.Stretch().ToBuildable();
    }

    public override bool Reconcile(Widget updated) {
        base.Reconcile(updated);
        Debug.Log(this.ToStringDeep());
        return true;
    }
}

public class ScrollExample : StatefulWidget<ScrollExample> {
    public override State<ScrollExample> CreateState() {
        return new ScrollExampleState();
    }
}

public class ScrollExampleState : State<ScrollExample> {
    private readonly List<Widget> _children = Enumerable.Range(0, 1000).Select(i => {
            return new HBox { backgroundStyle = new BackgroundStyle { color = Colors.All[i % Colors.All.Length].W500 } }
                .Flexible(selfCrossAxisAlign: Align.Stretch);
        }
    ).ToList<Widget>();

    private ScrollController _controller = new();
    private WidgetStateController _stateController = new();
    private ButtonController _buttonController;
    private bool toggle = true;

    public override void InitState() {
        base.InitState();
        _buttonController = new ButtonController(_stateController);
        _buttonController.onClick = () => { _controller.AnimateTo(0, 5f, EasingMode.EaseOut); };
    }

    public override void Dispose() {
        base.Dispose();
        _controller.Dispose();
        _stateController.Dispose();
        _buttonController.Dispose();
    }

    public override Widget Build(BuildContext context) {
        var style = DefaultButtonStyles.DefaultStyleOf(
            context,
            HButtonVariant.Ghost,
            HButtonSize.Regular,
            HInputRadius.Small,
            false
        );

        return new HBox {
            backgroundStyle = context.GetThemed(PrimitiveTheme.Surface),
            child = new HColumn {
                gap = 8,
                children = new WidgetList {
                    new HRow {
                        gap = 8,
                        children = new Widget[] {
                            new HButton {
                                style = style,
                                child = new HText("Toggle Scroll"),
                                onClick = SetState(() => toggle = !toggle)
                            }.Tight(),
                            new HButton {
                                style = style,
                                child = new HText("Scroll to zero"),
                                onClick = () => { _controller.AnimateTo(0, 5f, EasingMode.EaseOut); }
                            }.Tight(),
                            new HShapeButton {
                                variant = HButtonVariant.Flat,
                                onClick =
                                    () => {
                                        Debug.Log(
                                            $"{_controller.ScrollPosition.Min} {_controller.ScrollPosition.Max} " +
                                            $"{_controller.ScrollPosition.Extent} {_controller.ScrollPosition.ExtentInside}"
                                        );
                                    },
                                child = new HText("Solid Substance")
                            },
                            new HShapeButton {
                                variant = HButtonVariant.FlatTwoState,
                                child = new HText("Solid2 Substance")
                            },
                            new HShapeButton {
                                variant = HButtonVariant.Soft,
                                child = new HText("Soft Substance")
                            },
                            new HShapeButton {
                                variant = HButtonVariant.Outline,
                                child = new HText("Outline Substance")
                            },
                            new HShapeButton {
                                variant = HButtonVariant.Ghost,
                                child = new HText("Ghost Substance")
                            },
                            new HSubstanceBox {
                                controller = _stateController,
                                substances = new Substance[] {
                                    new BoxSubstance {
                                        backgroundStyle = new BackgroundStyle { color = MaterialColors.Red }
                                    },
                                    new BoxSubstance {
                                        opacity = WidgetStateProperties.Func(state => state.Hovered() ? 1f : 0.2f),
                                        backgroundStyle =
                                            WidgetStateProperties.All(
                                                new BackgroundStyle { color = MaterialColors.Green }
                                            )
                                    }
                                },
                                Modifiers = new Modifier[] { new WidgetStateModifier(_stateController) }
                            }.Size(100, 32)
                        }
                    },
                    new HSlider {
                        axis = Axis.Horizontal,
                        style = HSliderStyle.DefaultScrollbarStyleOf(context)
                    }.Flexible(selfCrossAxisAlign: Align.Stretch),
                    new HRow() {
                        crossAxisAlign = Align.Stretch,
                        gap = 8,
                        children = new WidgetList() {
                            new HBox {
                                borderRadius = BorderRadius.All(context.GetThemed(PrimitiveBaseTheme.Radius).Radius4),
                                child = new HScrollView {
                                    controller = _controller,
                                    children = new Widget[] {
                                        new HBox { backgroundStyle = MaterialColors.Red.Value }.Size(
                                            height: 500,
                                            width: StyleKeyword.Auto
                                        ),
                                        new HBox { backgroundStyle = MaterialColors.Blue.Value }.Size(
                                            height: 1000,
                                            width: StyleKeyword.Auto
                                        ),
                                        new HBox { backgroundStyle = MaterialColors.Green.Value }.Size(
                                            height: 500,
                                            width: StyleKeyword.Auto
                                        )
                                    }
                                }
                                // child = new HListView {
                                //     itemCount = 1000,
                                //     fixedItemHeight = 50,
                                //     scrollController = _controller,
                                //     itemBuilder =
                                //         (_, index) =>
                                //             new HBox { backgroundStyle = Colors.All[index % Colors.All.Length].W500 }.Size(
                                //                 height: 100,
                                //                 width: StyleKeyword.Auto
                                //             )
                                // }
                            }.WithModifier(ClipModifier.Clip).Fill().If(toggle),
                            new HSlider {
                                axis = Axis.Vertical,
                                style = HSliderStyle.DefaultScrollbarStyleOf(context)
                            }
                        }
                    }.Fill()
                }
            }.WithModifier(PaddingModifier.Of(8)).Fill()
        }.Stretch();

        return new FactoryWidget<ListView> {
            creator = () => new ListView {
                makeItem = () => new WidgetHostElement(),
                destroyItem = element => element.Clear(),
                bindItem = (element, i) => {
                    var widgetElement = (WidgetHostElement)element;
                    widgetElement.Buildable = _children[i].ToBuildable();
                    ModificationBarrier.Rebuild(widgetElement);
                },
                unbindItem = (element, i) => {
                    var widgetElement = (WidgetHostElement)element;
                    widgetElement.Buildable = null;
                    widgetElement.Clear();
                }
            },
            updater = view => {
                view.itemsSource = _children;
                view.RefreshItems();
            }
        };
    }
}

public class InteractiveExample : StatefulWidget<InteractiveExample> {
    public Color onColor = Colors.Green;
    public Color offColor = Colors.Red;

    public override State<InteractiveExample> CreateState() {
        return new InteractiveExampleState();
    }

    public override void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
        base.DebugFillProperties(properties);
        properties.Add(new ColorProperty("onColor", onColor));
        properties.Add(new ColorProperty("offColor", offColor));
    }
}

public class DummyCallbackElement : BaseElement, IHierarchyDisposable {
    public int id;

    public void Dispose() {
        //Debug.Log($"Disposing element {id}");
    }
}

public class InteractiveExampleState : State<InteractiveExample> {
    public bool isOn = false;
    //public int counter = 0;

    public static readonly Signal<int> Counter = Signal.Value(3);
    public static readonly Signal<int> Computed = Signal.Computed(() => Counter * 100);
    public static readonly Signal<int> Combined = Signal.Computed(() => Counter + Computed);

    public static readonly HButtonStyle ButtonStyle = new() {
        backgroundStyle = new WidgetStatePropertyMap<BackgroundStyle> {
            [WidgetState.Hovered | WidgetState.Selected] = new BackgroundStyle { color = Colors.Blue.W50 },
            [WidgetState.Pressed | WidgetState.Selected] = new BackgroundStyle { color = Colors.Blue.W50 },
            [WidgetState.Hovered] = new BackgroundStyle { color = Colors.Neutral.W100 },
            [WidgetState.Pressed] = new BackgroundStyle { color = Colors.Blue.W100 },
            [WidgetState.Selected] = new BackgroundStyle { color = Colors.Blue.W100 },
            [WidgetState.Disabled] = new BackgroundStyle { color = Colors.Neutral.W400 },
            [WidgetState.None] = new BackgroundStyle { color = Colors.Neutral.W300 },
        },
        padding = WidgetStateProperties.All(new StyleLength4(10)),
        transitions =
            WidgetStateProperties.All(new Transition[] { new(StyleProperties.BackgroundColor) { duration = 0.1f } }),
        boxShadow = new WidgetStatePropertyMap<BoxShadowStyle> {
            [WidgetState.Hovered] = new BoxShadowStyle(),
            [WidgetState.Focused] = new BoxShadowStyle { shadowColor = Colors.Blue.W500.WithOpacity(0.5f) },
            [WidgetState.None] = null
        },
        border = new WidgetStatePropertyMap<Border> {
            [WidgetState.Focused] = Border.All(1, Colors.Blue),
            [WidgetState.None] = Border.All(1, Colors.Transparent),
        }
    };

    private GlobalKey testComponentKey = new GlobalKey();
    private int rebuildCounter = 0;

    public override void InitState() {
        HelixDiagnostics.Build(
            "Init InteractiveExampleState",
            collector => collector
                .OwnerChain(mount)
                .OffendingElement(mount as IWidgetElement)
        ).Report(DiagnosticLevel.Info);
    }

    public override void DidUpdateWidget(InteractiveExample oldWidget) { }

    public override void Dispose() {
        base.Dispose();
        Debug.Log("Disposing InteractiveExample");
    }

    public override Widget Build(BuildContext context) {
        rebuildCounter++;
        var boxes = new List<Widget>();
        for (var i = 0; i < Counter; i++) {
            var closedIndex = i;
            var factoryWidget = new FactoryWidget<DummyCallbackElement> {
                key = i,
                creator = () => {
                    var element = new DummyCallbackElement().Sized(20, 20);
                    element.RegisterCallback<AttachToPanelEvent>(evt => { }
                        //Debug.Log($"Attached element {element.id} to panel {evt.destinationPanel}")
                    );
                    return element;
                },
                updater = element => {
                    element.id = closedIndex;
                    element.BackgroundColor(Colors.All[closedIndex % Colors.All.Length]);
                }
            };
            boxes.Add(factoryWidget);
        }

        // boxes = boxes.OrderBy(_ => Random.value).ToList();
        // Debug.Log($"Boxes: {string.Join(", ", boxes.Select(b => ((FactoryWidget<DummyCallbackElement>)b).key))}");

        return new HColumn {
            gap = 10,
            children = new WidgetList {
                new BoxShadow {
                    key = "display",
                    spreadRadius = 5,
                    child = new HBox {
                        backgroundStyle = isOn ? widget.onColor : widget.offColor,
                        borderRadius = BorderRadius.All(10),
                        child = new HAlign {
                            alignment = isOn ? new Alignment(0.66f, 0.66f) : new Alignment(-0.66f, -0.66f),
                            child = new HText($"Counter: {Counter.Value} Rebuilds: {rebuildCounter}")
                        },
                        Modifiers = new Modifier[] {
                            TransitionsModifier.Of(new Transition(StyleProperties.BackgroundColor) { duration = 1f })
                        }
                    }.Size(200, 200)
                },
                new HText($"Hello World! {Combined.Value}"), //
                new TestComponent { key = testComponentKey }.Tight().Const().If(Counter % 5 == 0),
                new ButtonBuilder {
                    enabled = isOn,
                    selected = Counter % 5 == 0,
                    builder = (_, state) => {
                        if (state.Pressed()) return new HRow { children = boxes };
                        return new HText(state.ToStateString());
                    }
                },
                new HBox {
                    backgroundStyle = new BackgroundStyle { color = Colors.Neutral.W200 },
                    borderRadius = BorderRadius.All(10),
                    alignment = Alignment.CenterLeft,
                    child = new HBox {
                        backgroundStyle = new BackgroundStyle { color = context.GetThemed(MyThemes.PrimaryColor) },
                        borderRadius = BorderRadius.All(10),
                        Modifiers = new Modifier[] {
                            TransitionsModifier.Of(new Transition(StyleProperties.All)),
                            SizeModifier.Of(math.remap(0, 4, 0, 1, Counter % 5).NormalizedPercent(), 100.Percent())
                        }
                    }
                }.Size(200, 20),
                new HButton {
                    key = "Toggle",
                    style = ButtonStyle,
                    child = new HText("Toggle"),
                    selected = isOn,
                    onClick = () => {
                        isOn = !isOn;
                        Counter.Value++;
                        Debug.Log("Updated Counter: " + Counter.Value);
                    }
                }.Tight(),
                new HButton {
                    key = "Remove",
                    style = ButtonStyle,
                    child = new HText("Remove"),
                    onClick = () => { Counter.Value--; }
                },
                new HButton {
                    key = "Popup",
                    style = ButtonStyle,
                    child = new HText("Popup"),
                    onClick = () => { ScaffoldElement.Get(context.Element).AddOverlay(new NewTest().Stretched()); }
                },
                new HButton {
                    key = "Pop",
                    style = ButtonStyle,
                    child = new HText("Pop"),
                    onClick = () => { OverlayEntry.Nearest(context.Element)?.Pop(); }
                }.Fill(),
                new HWrap {
                    key = "Boxes",
                    children = boxes
                }.Display(isOn)
            }
        }.Stretch();
    }
}

public class TestComponent : StatelessWidget<TestComponent> {
    public override Widget Build(BuildContext context) {
        return new HText(
            "Hello from TestComponent!",
            style: new TextStyle() {
                align = TextAnchor.MiddleCenter,
                fontSize = 20.Percent(),
                overflow = TextOverflow.Ellipsis,
                shadow = StyleKeyword.Auto,
                color = Colors.Red
            }
        );
    }
}

public class TestException : Exception {
    public override string ToString() {
        return $"Hello World";
    }
}

[UxmlElement]
public partial class TestVisualElementRebuilding : VisualElement {
    public TestVisualElementRebuilding() {
        var button = new Button();
        button.text = "Test";
        button.clicked += () => { hierarchy.Sort((element, visualElement) => Random.value > 0.5f ? 1 : -1); };
        Add(new Label("Hello World! 1"));
        Add(new Label("Hello World! 2"));
        Add(new Label("Hello World! 3"));
        Add(new Label("Hello World! 4"));
        Insert(0, button);
        style.flexDirection = FlexDirection.Row;
    }
}

// public class ColorSwitcher : StatefulWidget<ColorSwitcher> {
//     public override WidgetState<ColorSwitcher> CreateState() => new State();
//
//     public class State : WidgetState<ColorSwitcher> {
//         
//         
//         public override Widget Build(BuildContext context) {
//             
//         }
//     }
// }