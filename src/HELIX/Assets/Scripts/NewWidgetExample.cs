using System.Collections.Generic;
using System.Linq;
using HELIX;
using HELIX.Coloring;
using HELIX.Extensions;
using HELIX.Types;
using HELIX.Widgets;
using HELIX.Widgets.Elements;
using HELIX.Widgets.Modifiers;
using HELIX.Widgets.Signals;
using HELIX.Widgets.Universal;
using HELIX.Widgets.Universal.Styles;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

[UxmlElement]
public partial class NewTestWidget : HostWidgetElement {
    public NewTestWidget() {
        Hosted = new InteractiveExample().Stretch();
    }
}

public class InteractiveExample : StatefulWidget<InteractiveExample> {
    public Color onColor = Colors.Green;
    public Color offColor = Colors.Red;

    public override WidgetState<InteractiveExample> CreateState() {
        return new InteractiveExampleState();
    }
}

public class DummyCallbackElement : BaseElement, IHierarchyDisposable {
    public int id;

    public void Dispose() {
        //Debug.Log($"Disposing element {id}");
    }
}

public class InteractiveExampleState : WidgetState<InteractiveExample> {
    public bool isOn = false;
    //public int counter = 0;

    public static readonly Signal<int> Counter = Signal.Value<int>();
    public static readonly Signal<int> Computed = Signal.Computed(() => Counter * 100);
    public static readonly Signal<int> Combined = Signal.Computed(() => Counter + Computed);

    public static readonly SimpleButtonStyle ButtonStyle = new() {
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
            [WidgetState.Focused] = new BoxShadowStyle { shadowColor = Colors.Blue.value.WithOpacity(0.5f) },
            [WidgetState.None] = null
        },
        border = new WidgetStatePropertyMap<Border> {
            [WidgetState.Focused] = Border.All(1, Colors.Blue),
            [WidgetState.None] = Border.All(1, Colors.Transparent),
        }
    };

    private GlobalKey testComponentKey = new GlobalKey();

    public override void InitState() {
    }

    public override Widget Build(BuildContext context) {
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

        return new FlexColumn {
            gap = 10,
            children = new WidgetList {
                new BoxShadow {
                    key = "display",
                    spreadRadius = 5,
                    child = new Container {
                        size = new StyleLength2(200, 200),
                        backgroundStyle = new BackgroundStyle { color = isOn ? widget.onColor : widget.offColor },
                        borderRadius = BorderRadius.All(10),
                        child = isOn ? new Text("Hello :3") : null,
                        Modifiers = new Modifier[] {
                            TransitionModifier.Of(new Transition(StyleProperties.BackgroundColor) { duration = 1f })
                        }
                    }
                },
                new Text($"Hello World! {Combined.Value}"), //
                new TestComponent {
                    key = testComponentKey
                }.If(Counter % 5 == 0), new ButtonBuilder {
                    enabled = isOn,
                    selected = Counter % 5 == 0,
                    builder = (_, state) => {
                        if (state.Pressed()) return new FlexRow { children = boxes };
                        return new Text(state.ToStateString());
                    }
                },
                new Container {
                    backgroundStyle = new BackgroundStyle { color = Colors.Neutral.W200 },
                    borderRadius = BorderRadius.All(10),
                    size = new StyleLength2(200, 20),
                    alignment = Alignment.CenterLeft,
                    child = new Container {
                        backgroundStyle = new BackgroundStyle { color = Colors.Blue },
                        borderRadius = BorderRadius.All(10),
                        size = new StyleLength2(
                            math.remap(0, 4, 0, 1, Counter % 5).NormalizedPercent(),
                            100.Percent()
                        ),
                        Modifiers = new [] {
                            TransitionModifier.Of(new Transition(StyleProperties.All))
                        }
                    }
                },
                new StyleButton {
                    key = "Toggle",
                    style = ButtonStyle,
                    child = new Text("Toggle"),
                    selected = isOn,
                    onClick = () => {
                        isOn = !isOn;
                        Counter.Value++;
                    }
                }.Flexible(selfCrossAxisAlign: Align.Stretch),
                new StyleButton {
                    key = "Remove",
                    style = ButtonStyle,
                    child = new Text("Remove"),
                    onClick = () => { Counter.Value--; }
                },
                new FlexRow {
                    key = "Boxes",
                    children = boxes
                }.Display(isOn)
            }
        }.Stretch();
    }
}

public class TestComponent : StatelessWidget<TestComponent> {
    public override Widget Build(BuildContext context) {
        Debug.Log("Building TestComponent");
        return new Text("Hello from TestComponent!");
    }
}

[UxmlElement]
public partial class TestVisualElementRebuilding : VisualElement {
    public TestVisualElementRebuilding() {
        var button = new Button();
        button.text = "Test";
        button.clicked += () => {
            hierarchy.Sort((element, visualElement) => Random.value > 0.5f ? 1 : -1);
        };
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