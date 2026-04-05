using System.Collections.Generic;
using HELIX;
using HELIX.Coloring;
using HELIX.Extensions;
using HELIX.Types;
using HELIX.Widgets;
using HELIX.Widgets.Elements;
using HELIX.Widgets.Modifiers;
using HELIX.Widgets.Universal;
using HELIX.Widgets.Universal.Styles;
using UnityEngine;
using UnityEngine.UIElements;

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
        Debug.Log($"Disposing element {id}");
    }
}

public class InteractiveExampleState : WidgetState<InteractiveExample> {
    public bool isOn = false;
    public int counter = 0;

    static SimpleButtonStyle buttonStyle = new SimpleButtonStyle {
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

    public override Widget Build(BuildContext context) {
        var boxes = new List<Widget>();
        for (var i = 0; i < counter; i++) {
            var closedIndex = i;
            var factoryWidget = new FactoryWidget<DummyCallbackElement> {
                creator = () => {
                    var element = new DummyCallbackElement().Sized(20, 20);
                    return element;
                },
                updater = element => {
                    element.id = closedIndex;
                    element.BackgroundColor(Colors.All[closedIndex % Colors.All.Length]);
                }
            };
            boxes.Add(factoryWidget);
        }

        return new FlexColumn {
            gap = 10,
            children = new Widget[] {
                new BoxShadow {
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
                new Text($"Hello World! {counter}"), //
                new TestComponent(),
                
                new ButtonBuilder {
                    enabled = isOn,
                    selected = counter % 5 == 0,
                    builder = (_, state) => {
                        if (state.Pressed()) return new FlexRow { children = boxes };
                        return new Text(state.ToStateString());
                    }
                },
                new StyleButton {
                    style = buttonStyle,
                    child = new Text("Toggle"),
                    selected = isOn,
                    onClick = SetState(() => {
                            isOn = !isOn;
                            counter++;
                        }
                    )
                }.Flexible(selfCrossAxisAlign: Align.Stretch),
                new StyleButton {
                    style = buttonStyle,
                    child = new Text("Remove"),
                    onClick = SetState(() => { counter--; })
                },
                new FlexRow { children = boxes }
            }
        }.Stretch();
    }
}

public class TestComponent : StatelessWidget<TestComponent> {
    public override Widget Build(BuildContext context) {
        return new Text("Hello from TestComponent!");
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