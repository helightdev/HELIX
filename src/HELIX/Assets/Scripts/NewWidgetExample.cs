using System;
using System.Collections.Generic;
using HELIX;
using HELIX.Coloring;
using HELIX.Extensions;
using HELIX.Types;
using HELIX.Widgets;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Error;
using HELIX.Widgets.Diagnostics.Formatting;
using HELIX.Widgets.Diagnostics.Properties;
using HELIX.Widgets.Elements;
using HELIX.Widgets.Modifiers;
using HELIX.Widgets.Signals;
using HELIX.Widgets.Universal;
using HELIX.Widgets.Universal.Styles;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

[UxmlElement]
public partial class NewTestWidget : HostWidgetElement {
    public NewTestWidget() {
        Buildable = new InteractiveExample().Stretch().ToConstantBuildable();
    }
}

public class InteractiveExample : StatefulWidget<InteractiveExample> {
    public Color onColor = Colors.Green;
    public Color offColor = Colors.Red;

    public override WidgetState<InteractiveExample> CreateState() {
        return new InteractiveExampleState();
    }

    public override void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
        base.DebugFillProperties(properties);
        properties.Add(new DiagnosticsProperty<Color>("onColor", onColor));
        properties.Add(new DiagnosticsProperty<Color>("offColor", offColor));
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
    public static readonly Signal<int> Combined = Signal.Computed(() => Counter + Computed );

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

    public override void DidUpdateWidget(InteractiveExample oldWidget) {
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
                        child = new FlexAlign() {
                            alignment = isOn ? new Alignment(0.66f, 0.66f) : new Alignment(-0.66f, -0.66f),
                            child = new Text($"Counter: {Counter.Value}")
                        },
                        Modifiers = new Modifier[] {
                            TransitionModifier.Of(new Transition(StyleProperties.BackgroundColor) { duration = 1f })
                        }
                    }
                },
                new Text($"Hello World! {Combined.Value}"), //
                new TestComponent { key = testComponentKey}.Const().If(Counter % 5 == 0),
                new ButtonBuilder {
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
                        Modifiers = new[] { TransitionModifier.Of(new Transition(StyleProperties.All)) }
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
        if (Random.value < 0.5f) return new Text("TestComponent is working fine!");
            
        
        // Debug.Log("Building TestComponent\nSecond Line");
        // DiagnosticsNode root =
        //     new DiagnosticsBlock(
        //         name: "RootSystem",
        //         style: DiagnosticsTreeStyle.Error,
        //         description: "General diagnostics test",
        //         properties: new List<DiagnosticsNode> {
        //             new StringProperty("mode", "playtest"),
        //             new FlagProperty("connected", true, ifTrue: "connected"),
        //             new PercentProperty("load", 0.742, showName: true),
        //         },
        //         children: new List<DiagnosticsNode> {
        //             new DiagnosticsBlock(
        //                 name: "Renderer",
        //                 style: DiagnosticsTreeStyle.Sparse,
        //                 description: "Primary renderer",
        //                 properties: new List<DiagnosticsNode> {
        //                     new StringProperty("backend", "URP"),
        //                     new IntProperty("drawCalls", 184),
        //                     new DoubleProperty("frameTime", 16.67, unit: "ms"),
        //                 },
        //                 children: new List<DiagnosticsNode> {
        //                     new DiagnosticsBlock(
        //                         name: "ShadowPass",
        //                         style: DiagnosticsTreeStyle.Dense,
        //                         description: "Shadow map stage",
        //                         properties: new List<DiagnosticsNode> {
        //                             new FlagProperty("enabled", true, ifTrue: "enabled", ifFalse: "disabled"),
        //                             new IntProperty("cascadeCount", 4),
        //                             new IterableProperty<string>(
        //                                 "maps",
        //                                 new[] { "dirLight_0", "dirLight_1", "dirLight_2", "dirLight_3" },
        //                                 style: DiagnosticsTreeStyle.SingleLine
        //                             ),
        //                         },
        //                         children: new List<DiagnosticsNode> {
        //                             new DiagnosticsBlock(
        //                                 name: "Atlas",
        //                                 style: DiagnosticsTreeStyle.SingleLine,
        //                                 description: "packed 4096x4096",
        //                                 properties: new List<DiagnosticsNode> {
        //                                     new PercentProperty("occupancy", 0.813),
        //                                     new StringProperty("format", "D32"),
        //                                 }
        //                             ),
        //                             new DiagnosticsBlock(
        //                                 name: "Shadow Atlas",
        //                                 style: DiagnosticsTreeStyle.SingleLine,
        //                                 description: "packed 4096x4096",
        //                                 properties: new List<DiagnosticsNode> {
        //                                     new PercentProperty("occupancy", 0.813),
        //                                     new StringProperty("format", "D32"),
        //                                 }
        //                             )
        //                         }
        //                     ),
        //                     new DiagnosticsBlock(
        //                         name: "PostFX",
        //                         style: DiagnosticsTreeStyle.Offstage,
        //                         description: "Post-processing stack",
        //                         properties: new List<DiagnosticsNode> {
        //                             new FlagProperty("bloom", true, ifTrue: "bloom on"),
        //                             new FlagProperty("taa", false, ifFalse: "taa off"),
        //                         }
        //                     )
        //                 }
        //             ),
        //             new DiagnosticsBlock(
        //                 name: "Gameplay",
        //                 style: DiagnosticsTreeStyle.Sparse,
        //                 description: "Simulation state",
        //                 properties: new List<DiagnosticsNode> {
        //                     new IntProperty("agents", 37),
        //                     new StringProperty("state", "combat"),
        //                 },
        //                 children: new List<DiagnosticsNode> {
        //                     new DiagnosticsBlock(
        //                         name: "AI",
        //                         style: DiagnosticsTreeStyle.Transition,
        //                         description: "Decision layer",
        //                         properties: new List<DiagnosticsNode> {
        //                             new PercentProperty("confidence", 0.91),
        //                             new IterableProperty<string>(
        //                                 "activeBehaviors",
        //                                 new[] { "seek", "aim", "cover" },
        //                                 style: DiagnosticsTreeStyle.SingleLine
        //                             ),
        //                         },
        //                         children: new List<DiagnosticsNode> {
        //                             new DiagnosticsBlock(
        //                                 name: "Targeting",
        //                                 style: DiagnosticsTreeStyle.ErrorProperty,
        //                                 description: "fallback target selected",
        //                                 properties: new List<DiagnosticsNode> {
        //                                     new StringProperty("reason", "primary target lost"),
        //                                     new DoubleProperty("distance", 23.4, unit: "m"),
        //                                 }
        //                             )
        //                         }
        //                     )
        //                 }
        //             ),
        //             new DiagnosticsBlock(
        //                 name: "Messages",
        //                 style: DiagnosticsTreeStyle.Whitespace,
        //                 description: "Recent events",
        //                 properties: new List<DiagnosticsNode> {
        //                     DiagnosticsNode.Message("Loaded checkpoint A17"),
        //                     DiagnosticsNode.Message("Spawned reinforcement wave"),
        //                     DiagnosticsNode.Message("Audio device hot-swapped"),
        //                 }
        //             )
        //         }
        //     );
        //         var deepTree = new DiagnosticsBlock(
        //     name: "RootSystem",
        //     style: DiagnosticsTreeStyle.Sparse,
        //     description: "Full diagnostics tree test",
        //     properties: new List<DiagnosticsNode>
        //     {
        //         new StringProperty("environment", "development"),
        //         new StringProperty("scene", "CombatArena_03"),
        //         new PercentProperty("bootProgress", 0.873),
        //         new FlagProperty("networkReady", true, ifTrue: "network ready"),
        //         new ObjectFlagProperty<object>("saveGame", new object(), ifPresent: "save present", ifNull: "save missing"),
        //         new IterableProperty<string>(
        //             "loadedBundles",
        //             new[] { "core", "characters", "weapons", "audio_ambience", "ui_common" },
        //             style: DiagnosticsTreeStyle.SingleLine),
        //     },
        //     children: new List<DiagnosticsNode>
        //     {
        //         new DiagnosticsBlock(
        //             name: "Renderer",
        //             style: DiagnosticsTreeStyle.Sparse,
        //             description: "Primary rendering pipeline",
        //             properties: new List<DiagnosticsNode>
        //             {
        //                 new StringProperty("pipeline", "URP"),
        //                 new IntProperty("drawCalls", 1842),
        //                 new DoubleProperty("frameTime", 18.73, unit: "ms"),
        //                 new PercentProperty("gpuLoad", 0.917),
        //             },
        //             children: new List<DiagnosticsNode>
        //             {
        //                 new DiagnosticsBlock(
        //                     name: "ShadowSystem",
        //                     style: DiagnosticsTreeStyle.Dense,
        //                     description: "Cascaded directional shadows",
        //                     properties: new List<DiagnosticsNode>
        //                     {
        //                         new FlagProperty("enabled", true, ifTrue: "enabled", ifFalse: "disabled"),
        //                         new IntProperty("cascadeCount", 4),
        //                         new StringProperty("atlasSize", "4096x4096"),
        //                         new PercentProperty("atlasUsage", 0.812),
        //                     },
        //                     children: new List<DiagnosticsNode>
        //                     {
        //                         new DiagnosticsBlock(
        //                             name: "Cascade0",
        //                             style: DiagnosticsTreeStyle.SingleLine,
        //                             description: "near field",
        //                             properties: new List<DiagnosticsNode>
        //                             {
        //                                 new DoubleProperty("split", 0.10),
        //                                 new DoubleProperty("resolutionScale", 1.0),
        //                             }),
        //                         new DiagnosticsBlock(
        //                             name: "Cascade1",
        //                             style: DiagnosticsTreeStyle.SingleLine,
        //                             description: "mid field",
        //                             properties: new List<DiagnosticsNode>
        //                             {
        //                                 new DoubleProperty("split", 0.25),
        //                                 new DoubleProperty("resolutionScale", 0.75),
        //                             }),
        //                         new DiagnosticsBlock(
        //                             name: "Cascade2",
        //                             style: DiagnosticsTreeStyle.SingleLine,
        //                             description: "far field",
        //                             properties: new List<DiagnosticsNode>
        //                             {
        //                                 new DoubleProperty("split", 0.55),
        //                                 new DoubleProperty("resolutionScale", 0.5),
        //                             }),
        //                         new DiagnosticsBlock(
        //                             name: "Cascade3",
        //                             style: DiagnosticsTreeStyle.SingleLine,
        //                             description: "distance field",
        //                             properties: new List<DiagnosticsNode>
        //                             {
        //                                 new DoubleProperty("split", 1.0),
        //                                 new DoubleProperty("resolutionScale", 0.25),
        //                             }),
        //                     }),
        //                 new DiagnosticsBlock(
        //                     name: "PostFX",
        //                     style: DiagnosticsTreeStyle.Offstage,
        //                     description: "Post stack",
        //                     properties: new List<DiagnosticsNode>
        //                     {
        //                         new FlagProperty("bloom", true, ifTrue: "bloom on"),
        //                         new FlagProperty("motionBlur", false, ifFalse: "motion blur off"),
        //                         new FlagProperty("taa", true, ifTrue: "taa on"),
        //                         new DoubleProperty("exposure", 1.23),
        //                     },
        //                     children: new List<DiagnosticsNode>
        //                     {
        //                         new DiagnosticsBlock(
        //                             name: "ColorGrading",
        //                             style: DiagnosticsTreeStyle.SingleLine,
        //                             description: "filmic response",
        //                             properties: new List<DiagnosticsNode>
        //                             {
        //                                 new StringProperty("lut", "ArenaWarmGrade"),
        //                                 new DoubleProperty("contrast", 1.15),
        //                                 new DoubleProperty("saturation", 0.92),
        //                             }),
        //                     }),
        //             }),
        //
        //         new DiagnosticsBlock(
        //             name: "Simulation",
        //             style: DiagnosticsTreeStyle.Sparse,
        //             description: "Gameplay and AI state",
        //             properties: new List<DiagnosticsNode>
        //             {
        //                 new IntProperty("agents", 37),
        //                 new IntProperty("projectiles", 142),
        //                 new StringProperty("matchState", "sudden_death"),
        //                 new PercentProperty("timescale", 1.0),
        //             },
        //             children: new List<DiagnosticsNode>
        //             {
        //                 new DiagnosticsBlock(
        //                     name: "AI",
        //                     style: DiagnosticsTreeStyle.Transition,
        //                     description: "Decision framework",
        //                     properties: new List<DiagnosticsNode>
        //                     {
        //                         new PercentProperty("confidence", 0.91),
        //                         new IterableProperty<string>(
        //                             "activeBehaviors",
        //                             new[] { "seek", "cover", "suppress", "flank" },
        //                             style: DiagnosticsTreeStyle.SingleLine),
        //                     },
        //                     children: new List<DiagnosticsNode>
        //                     {
        //                         new DiagnosticsBlock(
        //                             name: "SquadAlpha",
        //                             style: DiagnosticsTreeStyle.Sparse,
        //                             description: "engagement team",
        //                             properties: new List<DiagnosticsNode>
        //                             {
        //                                 new IntProperty("members", 5),
        //                                 new StringProperty("target", "Player"),
        //                                 new PercentProperty("morale", 0.62),
        //                             },
        //                             children: new List<DiagnosticsNode>
        //                             {
        //                                 new DiagnosticsBlock(
        //                                     name: "Unit_01",
        //                                     style: DiagnosticsTreeStyle.SingleLine,
        //                                     description: "rifleman",
        //                                     properties: new List<DiagnosticsNode>
        //                                     {
        //                                         new StringProperty("state", "advancing"),
        //                                         new DoubleProperty("distanceToTarget", 14.2, unit: "m"),
        //                                     }),
        //                                 new DiagnosticsBlock(
        //                                     name: "Unit_02",
        //                                     style: DiagnosticsTreeStyle.SingleLine,
        //                                     description: "support gunner",
        //                                     properties: new List<DiagnosticsNode>
        //                                     {
        //                                         new StringProperty("state", "suppression_fire"),
        //                                         new DoubleProperty("distanceToTarget", 22.8, unit: "m"),
        //                                     }),
        //                             }),
        //                         new DiagnosticsBlock(
        //                             name: "Targeting",
        //                             style: DiagnosticsTreeStyle.ErrorProperty,
        //                             description: "fallback target selected",
        //                             properties: new List<DiagnosticsNode>
        //                             {
        //                                 new StringProperty("reason", "primary target occluded"),
        //                                 new StringProperty("selectedTarget", "LastKnownPlayerPosition"),
        //                                 new DoubleProperty("staleness", 2.4, unit: "s"),
        //                             }),
        //                     }),
        //             }),
        //
        //         new DiagnosticsBlock(
        //             name: "Streaming",
        //             style: DiagnosticsTreeStyle.Whitespace,
        //             description: "Asset and IO state",
        //             properties: new List<DiagnosticsNode>
        //             {
        //                 new StringProperty("region", "eu-central"),
        //                 new PercentProperty("diskUsage", 0.68),
        //                 new PercentProperty("memoryPressure", 0.79),
        //                 new IterableProperty<string>(
        //                     "pendingRequests",
        //                     new[]
        //                     {
        //                         "char_enemy_heavy.bundle",
        //                         "music_phase3.bank",
        //                         "voice_boss_intro.bank"
        //                     },
        //                     style: DiagnosticsTreeStyle.Flat),
        //             },
        //             children: new List<DiagnosticsNode>
        //             {
        //                 new DiagnosticsBlock(
        //                     name: "HotCache",
        //                     style: DiagnosticsTreeStyle.Shallow,
        //                     description: "recently used assets",
        //                     properties: new List<DiagnosticsNode>
        //                     {
        //                         DiagnosticsNode.Message("vfx/explosion_large"),
        //                         DiagnosticsNode.Message("anim/boss_phase2_transition"),
        //                         DiagnosticsNode.Message("audio/ui_warning_ping"),
        //                     }),
        //             }),
        //
        //         new DiagnosticsBlock(
        //             name: "RecentEvents",
        //             style: DiagnosticsTreeStyle.Flat,
        //             description: "Event log",
        //             properties: new List<DiagnosticsNode>
        //             {
        //                 DiagnosticsNode.Message("Match entered sudden death."),
        //                 DiagnosticsNode.Message("Boss shield broken by player ultimate."),
        //                 DiagnosticsNode.Message("Streaming request stalled for music_phase3.bank."),
        //                 DiagnosticsNode.Message("AI target fallback engaged for SquadAlpha."),
        //             }),
        //     });
        //
        // Console.WriteLine("=== DEEP TREE ===");
        // Console.WriteLine(deepTree.ToStringDeep(wrapWidth: 100));
        //
        // try {
        //     throw new Exception("Test");
        // } catch (Exception ex) {
        //    var exception = HelixDiagnostics.Build(summary: "Failed to initialize combat session.",
        //     description: "A fatal error occurred while bringing the combat loop online." + "A fatal error occurred while bringing the combat loop online." + "A fatal error occurred while bringing the combat loop online." + "A fatal error occurred while bringing the combat loop online.",
        //
        //     details: new DiagnosticsNode[] {
        //         OwnershipChainErrorProperty.FromBuildContext(context), 
        //         
        //         new ErrorProperty("sessionId", "arena-03-run-000184"), new ErrorProperty("region", "eu-central"),
        //         new ErrorProperty("buildVersion", "1.4.17-dev"), new ErrorProperty("netMode", "client-hosted"),
        //         new DiagnosticsBlock(
        //             name: "RuntimeState",
        //             style: DiagnosticsTreeStyle.Sparse,
        //             description: "Snapshot at failure",
        //             properties: new List<DiagnosticsNode> {
        //                 new StringProperty("scene", "CombatArena_03"),
        //                 new StringProperty("phase", "BossIntro"),
        //                 new PercentProperty("prewarmProgress", 0.58),
        //                 new FlagProperty("authorityReady", false, ifFalse: "authority not ready"),
        //                 new FlagProperty("saveLoaded", true, ifTrue: "save loaded"),
        //             },
        //             children: new List<DiagnosticsNode> {
        //                 new DiagnosticsBlock(
        //                     name: "Networking",
        //                     style: DiagnosticsTreeStyle.Sparse,
        //                     description: "Transport bootstrap",
        //                     properties: new List<DiagnosticsNode> {
        //                         new StringProperty("transport", "Relay"),
        //                         new IntProperty("ping", 148, unit: "ms"),
        //                         new PercentProperty("packetLoss", 0.12),
        //                         new FlagProperty("authenticated", true, ifTrue: "authenticated"),
        //                     },
        //                     children: new List<DiagnosticsNode> {
        //                         new DiagnosticsBlock(
        //                             name: "Handshake",
        //                             style: DiagnosticsTreeStyle.ErrorProperty,
        //                             description: "timed out while waiting for host ack",
        //                             properties: new List<DiagnosticsNode> {
        //                                 new DoubleProperty("elapsed", 8.5, unit: "s"),
        //                                 new IntProperty("retryCount", 3),
        //                                 new StringProperty("lastStage", "SendJoinRequest"),
        //                             }
        //                         ),
        //                     }
        //                 ),
        //                 new DiagnosticsBlock(
        //                     name: "ContentValidation",
        //                     style: DiagnosticsTreeStyle.Transition,
        //                     description: "Required bundles audit",
        //                     properties: new List<DiagnosticsNode> {
        //                         new IntProperty("required", 6),
        //                         new IntProperty("resolved", 4),
        //                         new PercentProperty("completion", 4.0 / 6.0),
        //                     },
        //                     children: new List<DiagnosticsNode> {
        //                         new DiagnosticsBlock(
        //                             name: "MissingBundles",
        //                             style: DiagnosticsTreeStyle.Flat,
        //                             description: "Missing bundle list",
        //                             properties: new List<DiagnosticsNode> {
        //                                 DiagnosticsNode.Message("music_phase3.bank"),
        //                                 DiagnosticsNode.Message("voice_boss_intro.bank"),
        //                             }
        //                         ),
        //                     }
        //                 ),
        //             }
        //         )
        //     },
        //     hints: new DiagnosticsNode[] {
        //         new ErrorHint("Check whether the host completed content prewarm before advertising the session."),
        //         new ErrorHint("Verify the missing bundles are present in the current content catalog."),
        //         new ErrorHint("If this is a relay issue, inspect handshake timeout thresholds and retry backoff.")
        //     },
        //     stackTrace: ex.StackTrace);
        //
        // Debug.LogError(exception.ToStringDeep(wrapWidth: 110));
        // }
        //
        //
        // Debug.Log(root.ToStringDeep());
        
        return new Text("Hello from TestComponent!");
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