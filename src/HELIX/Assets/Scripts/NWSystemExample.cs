using System;
using System.Collections.Generic;
using System.Globalization;
using HELIX.Compose;
using HELIX.Compose.Forms;
using HELIX.Extensions;
using HELIX.Prose;
using HELIX.Theming;
using HELIX.Types;
using HELIX.Widgets.Universal;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

namespace HELIX.Examples {
  [BoundaryComposable(Extension = true, UseLookupCache = true)]
  public partial class MyBetterWidget {
    public partial struct Props {
      [Prop("HELIX.Coloring.Colors.Red", PropInit.Deferred)]
      public Color color;
    }

    protected override void OnRecompose(ref Composition cx) { }
  }

  [UxmlElement]
  public partial class NwSystemExampleElement : BoundaryVisualElement {
    public override void Compose(ref Composition cx) {
      this.Fill();
      cx.NWSystemExample();
      cx.CURSOR.Fill();
    }
  }

  [BoundaryComposable(Extension = true)]
  public partial class NWSystemExample {
    protected override void OnRecompose(ref Composition cx) {
      var theme = cx.ReadContextOrDefault(ThemeData.Key, HXThemes.DefaultDark).Copy();
      //var kennyBg = Resources.Load<Texture2D>("kenney/PNG/Double/button_grey");
      // var kennyBg = Resources.Load<Texture2D>("kenney/PNG/Double/pattern_diagonal_red_large");
      // ThemeProperties.ButtonFilled[theme] = new HXControlBoxStyle(
      //   background: new HXImageStyle(
      //     image: BackgroundImage.Texture2D(kennyBg, scaling: ImageScaling.RepeatX(32), anchor: ImageAnchor.Left),
      //     tint: new StatePropertyMap<Color> {
      //       [State.Active] = Colors.White,
      //       [State.Hovered] = Colors.White90,
      //       [State.None] = Colors.White80
      //     }
      //   ).Bake(),
      //   padding: ThemeProperties.ButtonPadding[theme],
      //   textStyle: HXStyles
      //     .TextColor(StateProperties.Const(Colors.White))
      //     .Derive(States.Common)
      // );

      using (cx.WriteContext(out var context)) {
        ThemeData.Key[context] = theme;
      }


      cx.CURSOR
        .BackgroundColor(theme.GetColor(ColorRoles.Surface))
        .TextColor(theme.GetColor(ColorRoles.OnSurface));


      HomeComposable.ComposeBoundary(ref cx);
      cx.CURSOR.Fill();
    }
  }

  [BoundaryComposable]
  public partial class HomeComposable {
    public enum ExampleMode : byte { Balanced, Performance, Quality }

    public enum NavigationTransitionKind : byte { Slide, Fade, Instant }

    private const string TabNavigation = "examples-navigation";
    private const string TabOverlays = "examples-overlays";
    private const string TabInputs = "examples-inputs";
    private const string TabProse = "examples-prose";

    public static readonly SliderOptions VolumeOptions = new(0f, 1f, step: 0f, thumbRange: 0.1f);
    public static readonly SliderOptions VolumeOptionsScroll = new(
      0f, 1f, step: 0f, thumbRange: 0.1f, axis: Axis.Vertical
    );

    public static readonly TextInputValueAdapter<float> DecimalValueAdapter = new(
      value => value.ToString("0.00", CultureInfo.InvariantCulture),
      (string text, out float value) => float.TryParse(
        text, NumberStyles.Float, CultureInfo.InvariantCulture, out value
      )
    );

    public static readonly IReadOnlyList<DropdownOption<ExampleMode>> ModeOptions =
      new DropdownOption<ExampleMode>[] {
        new(ExampleMode.Balanced, "Balanced"),
        new(ExampleMode.Performance, "Performance"),
        new(ExampleMode.Quality, "Quality")
      };

    public static readonly IReadOnlyList<DropdownOption<NavigationTransitionKind>> NavigationTransitionOptions =
      new DropdownOption<NavigationTransitionKind>[] {
        new(NavigationTransitionKind.Slide, "Slide"),
        new(NavigationTransitionKind.Fade, "Fade"),
        new(NavigationTransitionKind.Instant, "Instant")
      };

    public static readonly IReadOnlyList<MenuItem> ExampleMenuItems = new[] {
      new MenuItem(MenuItemKind.Heading, "Actions"),
      new MenuItem(
        "Toggle controls", static ctx => {
          using (ctx.Modify<HomeComposable>(out var state)) { state.enabled = !state.enabled; }
        }
      ),
      new MenuItem("Selected action", static ctx => Debug.Log("Selected menu action"), selected: true),
      new MenuItem("Unavailable action", enabled: false),
      new MenuItem(MenuItemKind.Separator),
      new MenuItem("Mode") {
        new MenuItem(
          "Balanced", static ctx => {
            using (ctx.Modify<HomeComposable>(out var state)) { state.mode = ExampleMode.Balanced; }
          }
        ),
        new MenuItem(
          "Performance", static ctx => {
            using (ctx.Modify<HomeComposable>(out var state)) { state.mode = ExampleMode.Performance; }
          }
        ),
        new MenuItem("Quality") {
          new MenuItem(
            "High", static ctx => {
              using (ctx.Modify<HomeComposable>(out var state)) { state.mode = ExampleMode.Quality; }
            }
          ),
          new MenuItem("Ultra", static ctx => Debug.Log("Ultra quality selected"))
        }
      }
    };

    public string text = "Editable text";
    public int integer = 12;
    public float floatingPoint = 1.25f;
    public float volume = 0.65f;
    public bool enabled = true;
    public ExampleMode mode = ExampleMode.Balanced;
    public NavigationTransitionKind navigationTransition = NavigationTransitionKind.Slide;
    public string navigationResult = "No result returned yet";
    public string navigationLifecycle = "Initial route created";
    public int dynamicPageSequence;

    private FontAsset _iconFont;
    private NavigationGraph _tabNavigationGraph;
    private NavigationController _tabNavigationController;
    private NavigationGraph _navigationGraph;
    private NavigationController _navigationController;
    private NavigationGraph _dialogNavigationGraph;
    private NavigationController _dialogNavigationController;
    private OverlayController _overlayController;
    private FormController _exampleForm;
    private FormFieldRegistration<string> _exampleNameField;
    private FormFieldRegistration<bool> _exampleUpdatesField;

    public override void OnAttach(BoundaryData data, IBoundary boundary) {
      base.OnAttach(data, boundary);
      _iconFont = Resources.Load<FontAsset>("helix/fa/FontAwesome7FreeSolid");
      _tabNavigationGraph = NavigationGraph.Builder(TabNavigation)
        .Route(
          TabNavigation,
          NavigationPage.Build(ComposeNavigationTab)
            .Name("Navigation")
            .Transition(NavigationTransitions.SlideHorizontal)
        )
        .Route(
          TabOverlays,
          NavigationPage.Build(ComposeOverlaysTab)
            .Name("Overlays and menus")
            .Transition(NavigationTransitions.SlideHorizontal)
        )
        .Route(
          TabInputs,
          NavigationPage.Build(ComposeInputsTab)
            .Name("Inputs")
            .Transition(NavigationTransitions.SlideHorizontal)
        )
        .Route(
          TabProse,
          NavigationPage.Build(ComposeProseTab)
            .Name("Prose")
            .Transition(NavigationTransitions.SlideHorizontal)
        )
        .Build();
      _tabNavigationController = new NavigationController(_tabNavigationGraph);
      for (var i = 0; i < _tabNavigationGraph.Routes.Count; i++) {
        var route = _tabNavigationGraph.Routes[i];
        if (route.Name != _tabNavigationGraph.InitialRoute) _tabNavigationController.Preload(route);
      }
      _navigationGraph = NavigationGraph.Builder("navigation-home")
        .Route("navigation-home", ComposeNavigationHome)
        .Route("navigation-details", ComposeNavigationDetails)
        .Route("navigation-settings", ComposeNavigationSettings)
        .Build();
      _navigationController = new NavigationController(_navigationGraph);
      _navigationController.onLifecycle = RecordNavigationLifecycle;
      _dialogNavigationGraph = NavigationGraph.Builder("dialog-intro")
        .Route("dialog-intro", ComposeDialogIntro)
        .Route("dialog-details", ComposeDialogDetails)
        .Route("dialog-confirm", ComposeDialogConfirm)
        .Build();
      _dialogNavigationController = new NavigationController(_dialogNavigationGraph);
      _overlayController = new OverlayController();
      _exampleForm = new FormController();
      _exampleNameField = new FormFieldRegistration<string>(static (_, _) => { });
      _exampleUpdatesField = new FormFieldRegistration<bool>(static (_, _) => { });
    }

    public override void OnDetach(BoundaryData data, IBoundary boundary) {
      _tabNavigationController?.Dispose();
      if (_navigationController != null) _navigationController.onLifecycle = null;
      _navigationController?.Dispose();
      _dialogNavigationController?.Dispose();
      _overlayController?.Dispose();
      _exampleNameField?.Dispose();
      _exampleUpdatesField?.Dispose();
      _exampleForm?.Dispose();
      _tabNavigationController = null;
      _navigationController = null;
      _dialogNavigationController = null;
      _overlayController = null;
      _exampleNameField = null;
      _exampleUpdatesField = null;
      _exampleForm = null;
      _tabNavigationGraph = null;
      _navigationGraph = null;
      _dialogNavigationGraph = null;
      base.OnDetach(data, boundary);
    }

    protected override void OnRecompose(ref Composition cx) {
      using (cx.OverlayHost(_overlayController))
      using (cx.Group(Axis.Vertical, cross: Align.Stretch)) {
        if (cx.CursorDirty) cx.CURSOR.Fill().Padding(16f);

        ComposeTabHeader(ref cx);
        cx.Spacing(2);
        cx.NavigationHost(
            _tabNavigationGraph,
            _tabNavigationController,
            NavigationTransitions.Instant,
            NavigationHostBehavior.None
          )
          .Flexible()
          .Overflow(Overflow.Hidden);
      }
    }

    private static void ComposeNavigationTab(ref Composition cx, NavigationContextData navigation) =>
      cx.Lookup<HomeComposable>()?.ComposeNavigationTab(ref cx);

    private static void ComposeOverlaysTab(ref Composition cx, NavigationContextData navigation) =>
      cx.Lookup<HomeComposable>()?.ComposeOverlaysTab(ref cx);

    private static void ComposeInputsTab(ref Composition cx, NavigationContextData navigation) =>
      cx.Lookup<HomeComposable>()?.ComposeInputsTab(ref cx);

    private static void ComposeProseTab(ref Composition cx, NavigationContextData navigation) =>
      cx.Lookup<HomeComposable>()?.ComposeProseTab(ref cx);

    private void ComposeInputsTab(ref Composition cx) {
      using (cx.Group(Axis.Vertical, cross: Align.Stretch)) {
        if (cx.CursorDirty) cx.CURSOR.Fill();
        ComposeInputsShowcase(ref cx);
      }
    }

    private void ComposeNavigationTab(ref Composition cx) {
      using (cx.Group(Axis.Vertical, cross: Align.Stretch)) {
        if (cx.CursorDirty) cx.CURSOR.Fill();
        ComposeNavigationShowcase(ref cx);
      }
    }

    private void ComposeOverlaysTab(ref Composition cx) {
      using (cx.Group(Axis.Vertical, cross: Align.Stretch)) {
        if (cx.CursorDirty) cx.CURSOR.Fill();
        ComposeOverlayShowcase(ref cx);
      }
    }

    private void ComposeProseTab(ref Composition cx) {
      using (cx.Group(Axis.Vertical, cross: Align.Stretch)) {
        if (cx.CursorDirty) cx.CURSOR.Fill();
        cx.Text("Semantic Prose writers", TextRole.TitleMedium);
        cx.Spacing(1);
        cx.Text(
          "The same immediate-mode station report can be projected through several allocation-conscious " +
          "plain-text configurations, Markdown, Unity rich text, or the data-only dictionary writer. " +
          "Open the Unity console to compare them.",
          TextRole.BodySmall
        );
        cx.Spacing(2);

        using (cx.Group(Axis.Horizontal, cross: Align.Stretch)) {
          cx.Button(
            static (ref Composition child) => child.Text("Sparse tree"),
            action: static _ => DetailedProseExample.PrintPlainText(
              "Sparse tree", ProsePlainTextConfigurations.Sparse
            )
          );
          cx.Spacing(1);
          cx.Button(
            static (ref Composition child) => child.Text("Error tree"),
            style: ThemeProperties.ButtonOutlined[in cx],
            action: static _ => DetailedProseExample.PrintPlainText(
              "Error tree", ProsePlainTextConfigurations.Error
            )
          );
        }
        cx.Spacing(1);
        using (cx.Group(Axis.Horizontal, cross: Align.Stretch)) {
          cx.Button(
            static (ref Composition child) => child.Text("Plain"),
            action: static _ => DetailedProseExample.PrintPlainText(
              "Plain", ProsePlainTextConfigurations.Plain
            )
          );
          cx.Spacing(1);
          cx.Button(
            static (ref Composition child) => child.Text("Markdown"),
            style: ThemeProperties.ButtonOutlined[in cx],
            action: static _ => DetailedProseExample.PrintPlainText(
              "Markdown", ProsePlainTextConfigurations.Markdown
            )
          );
        }
        cx.Spacing(1);
        using (cx.Group(Axis.Horizontal, cross: Align.Stretch)) {
          cx.Button(
            static (ref Composition child) => child.Text("Unity rich text"),
            action: static _ => DetailedProseExample.PrintUnityRichText()
          );
          cx.Spacing(1);
          cx.Button(
            static (ref Composition child) => child.Text("Dictionary tree"),
            style: ThemeProperties.ButtonOutlined[in cx],
            action: static _ => DetailedProseExample.PrintDictionary()
          );
        }
        cx.Spacing(1);
        using (cx.Group(Axis.Horizontal, cross: Align.Stretch)) {
          cx.Button(
            static (ref Composition child) => child.Text("Whitespace tree"),
            action: static _ => DetailedProseExample.PrintPlainText(
              "Whitespace tree", ProsePlainTextConfigurations.Whitespace
            )
          );
          cx.Spacing(1);
          cx.Button(
            static (ref Composition child) => child.Text("Shallow"),
            style: ThemeProperties.ButtonOutlined[in cx],
            action: static _ => DetailedProseExample.PrintPlainText(
              "Shallow", ProsePlainTextConfigurations.Shallow
            )
          );
        }
        using (cx.Group(Axis.Horizontal, cross: Align.Stretch)) {
          cx.Button(
            static (ref Composition child) => child.Text("Test: wide decorated"),
            style: ThemeProperties.ButtonOutlined[in cx],
            action: static _ => DetailedProseExample.PrintPlainText(
              "test wide decorated", DetailedProseExample.TestWideDecorated
            )
          );
        }
        cx.Spacing(1);
        using (cx.Group(Axis.Horizontal, cross: Align.Stretch)) {
          cx.Button(
            static (ref Composition child) => child.Text("Test: compact sections"),
            action: static _ => DetailedProseExample.PrintPlainText(
              "test compact sections", DetailedProseExample.TestCompactSections
            )
          );
        }

        cx.Spacing(2);
        cx.Text("Example contents", TextRole.LabelLarge);
        cx.Spacing(1);
        cx.Text(
          "Mission metadata, typed and constrained properties, hidden data, severity markers, power and " +
          "communications subsystems, nested reactor and antenna trees, cargo, crew, and active alerts.",
          TextRole.BodySmall
        );
      }
    }

    private void ComposeTabHeader(ref Composition cx) {
      cx.Text("HELIX NW system example", TextRole.TitleLarge);
      cx.Spacing(1);
      using (cx.Group(Axis.Horizontal, cross: Align.Stretch)) {
        var routes = _tabNavigationGraph.Routes;
        for (var i = 0; i < routes.Count; i++) {
          cx.NavigationLink(
            routes[i],
            controller: _tabNavigationController,
            style: ThemeProperties.ButtonToggle[in cx]
          );
          if (i + 1 < routes.Count) cx.Spacing(1);
        }
      }
    }

    private void ComposeNavigationShowcase(ref Composition cx) {
      cx.SubscribeTo(_navigationController);
      cx.Text("Navigation stack and operation queue", TextRole.TitleMedium);
      cx.Spacing(1);
      cx.Text(
        $"Current: {_navigationController.Current?.Name ?? "<empty>"}  •  " +
        $"Stack: {_navigationController.BackStack.Count}  •  " +
        $"Queued: {_navigationController.PendingOperationCount}  •  " +
        $"Transitioning: {_navigationController.IsTransitioning}", TextRole.BodySmall
      );
      cx.Text($"Lifecycle: {navigationLifecycle}", TextRole.BodySmall);
      cx.Text($"Dynamic result: {navigationResult}", TextRole.BodySmall);
      cx.Spacing(2);


      using (cx.Group(Axis.Horizontal, cross: Align.Stretch)) {
        using (cx.Group(Axis.Vertical, cross: Align.Stretch)) {
          cx.CURSOR.AlignSelf(Align.Stretch);

          cx.Text("Presented pages", TextRole.LabelLarge);
          cx.Spacing(1);
          cx.NavigationHost(_navigationGraph, _navigationController, ResolveNavigationTransition())
            .With(BoxConstraints.Preferred(640, 380))
            .With(BorderRadius.All(12))
            .Overflow(Overflow.Hidden);
        }

        cx.Spacing(2);
        using (cx.Group(Axis.Vertical, cross: Align.Stretch)) {
          if (cx.CursorDirty) cx.CURSOR.Width(300f);
          cx.Text("Operations", TextRole.LabelLarge);
          cx.Spacing(1);
          cx.DropdownButton(
            navigationTransition,
            NavigationTransitionOptions,
            onChanged: static (ctx, value) => {
              using (ctx.Modify<HomeComposable>(out var state)) { state.navigationTransition = value; }
            },
            placeholder: "Transition"
          );
          cx.Spacing(1);
          cx.Button(
            static (ref Composition child) => child.Text("Push registered details"),
            action: static ctx => {
              var owner = ctx.Lookup<HomeComposable>();
              owner?._navigationController?.Push(
                "navigation-details",
                NavigationArguments.Empty.With("message", "Pushed from the operation panel."),
                owner.CurrentNavigationOptions()
              );
            }
          );
          cx.Spacing(1);
          cx.Button(
            static (ref Composition child) => child.Text("Push dynamic result page"),
            style: ThemeProperties.ButtonOutlined[in cx],
            action: PushDynamicResultPage
          );
          cx.Spacing(1);
          cx.Button(
            static (ref Composition child) => child.Text("Replace with settings"),
            style: ThemeProperties.ButtonOutlined[in cx],
            action: static ctx => {
              var owner = ctx.Lookup<HomeComposable>();
              owner?._navigationController?.Replace(
                "navigation-settings", options: owner.CurrentNavigationOptions()
              );
            }
          );
          cx.Spacing(1);
          cx.Button(
            static (ref Composition child) => child.Text("Queue reset → push → push → pop"),
            style: ThemeProperties.ButtonOutlined[in cx],
            action: RunQueuedNavigationDemo
          );
          cx.Spacing(1);
          cx.Button(
            static (ref Composition child) => child.Text("Pop to home"),
            style: ThemeProperties.ButtonGhost[in cx],
            action: static ctx => ctx.Lookup<HomeComposable>()?._navigationController?.PopTo("navigation-home")
          );
          cx.Spacing(1);
          cx.Button(
            static (ref Composition child) => child.Text("Reset"),
            style: ThemeProperties.ButtonGhost[in cx],
            action: static ctx => ctx.Lookup<HomeComposable>()?._navigationController?.Reset()
          );
          cx.Spacing(1);
          cx.Button(
            static (ref Composition child) => child.Text("Open navigation dialog"),
            action: OpenNavigationDialog
          );
        }
      }
    }

    private void ComposeOverlayShowcase(ref Composition cx) {
      cx.Text("Overlay builders and themed popup controls", TextRole.TitleMedium);
      cx.Spacing(2);
      using (cx.Group(Axis.Horizontal, cross: Align.Stretch)) {
        using (cx.Group(Axis.Vertical, cross: Align.Stretch)) {
          cx.Text("Overlay builders", TextRole.LabelLarge);
          cx.Spacing(1);
          cx.Button(
            static (ref Composition child) => child.Text("Open modal"),
            action: static ctx => Overlay.Build(ComposeExampleModal)
              .Modal(dismissOnOutsidePointer: true)
              .Constraints(
                BoxConstraints.Only(
                  min: new StyleLength2(360f, 0f),
                  max: new StyleLength2(520f, StyleKeyword.None)
                )
              )
              .DismissOnCancel()
              .Show(ctx)
          );
          cx.Spacing(1);
          cx.Button(
            static (ref Composition child) => child.Text("Open navigation dialog"),
            style: ThemeProperties.ButtonOutlined[in cx],
            action: OpenNavigationDialog
          );
          cx.Spacing(1);
          cx.Button(
            static (ref Composition child) => child.Text("Open anchored menu"),
            style: ThemeProperties.ButtonOutlined[in cx],
            action: static ctx => Overlay.Build(ComposeExampleMenu)
              .AnchorTo(ctx.element, OverlayPlacement.BelowStart, new Vector2(0f, 6f))
              .MatchAnchorWidth()
              .DismissOnOutsidePointer()
              .CaptureFocus()
              .Show(ctx)
          );
          cx.Spacing(1);
          cx.Button(
            static (ref Composition child) => child.Text("Show notification"),
            style: ThemeProperties.ButtonGhost[in cx],
            action: static ctx => Overlay.Build(ComposeExampleNotification)
              .At(OverlayPlacement.TopEnd)
              .Stacked(spacing: 8f)
              .Timeout(3500)
              .Show(ctx)
          );
        }

        cx.Spacing(2);
        using (cx.Group(Axis.Vertical, cross: Align.Stretch)) {
          if (cx.CursorDirty) cx.CURSOR.Width(240f);
          cx.Text("Themed popup controls", TextRole.LabelLarge);
          cx.Spacing(1);
          cx.DropdownButton(
            mode,
            ModeOptions,
            onChanged: static (ctx, value) => {
              using (ctx.Modify<HomeComposable>(out var state)) { state.mode = value; }
            },
            placeholder: "Choose a mode"
          );
          cx.Spacing(1);
          cx.MenuButton(
            static (ref Composition child) => child.Text("Open menu with submenus"),
            ExampleMenuItems
          );
        }
      }
    }

    private void ComposeInputsShowcase(ref Composition cx) {
      cx.Text("Controlled inputs");
      cx.Spacing(2);

      using (cx.Decorator(out var slots)) {
        using (slots.Prefix()) {
          HXDecorator.Label(
            ref cx,
            new LabelSpec("Prefix", new IconRef(FaSolidIcons.User.ToString(), _iconFont).Composable())
          );
        }
        using (slots.Element()) cx.Text("Element Data", TextRole.BodyMedium);
        using (slots.Suffix()) HXDecorator.Label(ref cx, new LabelSpec("Suffix"));
        using (slots.Label()) HXDecorator.Label(ref cx, new LabelSpec("Label"));
        using (slots.Before()) HXDecorator.Label(ref cx, new LabelSpec("Before"));
        using (slots.After()) HXDecorator.Label(ref cx, new LabelSpec("After"));
        using (slots.Description()) HXDecorator.Label(ref cx, new LabelSpec("Description"));
      }

      using (cx.Group(Axis.Horizontal, cross: Align.Stretch)) {
        using (cx.Group(Axis.Vertical, cross: Align.Stretch)) {
          if (cx.CursorDirty) cx.CURSOR.Size(BoxConstraints.Only(min: new StyleLength2(260f, 0f)));
          cx.Text("Text");
          cx.Spacing(1);
          cx.TextField(
            initialValue: new TextEditingValue("Hello World!"),
            onChanged: static (ctx, value) => {
              Debug.Log($"Text changed: {value}");
              using (ctx.Modify<HomeComposable>(out var composable)) { composable.text = value.text; }
            },
            processor: (ref TextEditProcessorContext context) => {
              // Debug.Log(
              //   $"Processor: {context.trigger}\n{context.next.ToFormattedString()}\n/\\ Becomes /\\\n{context.previous.ToFormattedString()}\n;{context.physical}”"
              // );
            }
          );
          cx.TextField(
            initialValue: new TextEditingValue("Hello World!"),
            value: text,
            onChanged: static (ctx, value) => {
              Debug.Log($"Text changed: {value}");
              using (ctx.Modify<HomeComposable>(out var composable)) { composable.text = value.text; }
            },
            processor: (ref TextEditProcessorContext context) => {
              // Debug.Log(
              //   $"Processor: {context.trigger}\n{context.next.ToFormattedString()}\n/\\ Becomes /\\\n{context.previous.ToFormattedString()}\n;{context.physical}”"
              // );
            }
          );
          // cx.TextInput(
          //   text,
          //   onChanged: (ctx, value) => {
          //     Debug.Log($"Text changed: {value}");
          //     using (ctx.Modify<HomeComposable>(out var composable)) { composable.text = value; }
          //   },
          //   onSubmitted: (ctx, value) => {
          //     Debug.Log($"Text submitted: {value}");
          //   },
          //   onEditingStarted: (ctx) => {
          //     Debug.Log($"Text editing started");
          //   },
          //   onEditingEnded: (ctx, value, reason) => {
          //     Debug.Log($"Text editing ended");
          //   }
          // );
          cx.Spacing(2);
          // cx.TextInput(
          //   text,
          //   options: new TextInputOptions(multiline: true),
          //   processor: (ref TextEditProcessorContext context) => {
          //     if (context.AbsoluteLengthDelta > 3) {
          //       context.next = context.previous;
          //       context.result = TextEditResult.Break();
          //       return;
          //     }
          //
          //     Debug.Log($"Processor: {context.trigger}\n{context.next.ToFormattedString()}\n/\\ Becomes /\\\n{context.previous.ToFormattedString()}\n;{context.physical}”");
          //
          //     //context.result = TextEditResult.Break(true);
          //   },
          //   onChanged: (ctx, value) => {
          //     Debug.Log($"Text changed: {value}");
          //     using (ctx.Modify<HomeComposable>(out var composable)) { composable.text = value; }
          //   },
          //   onSubmitted: (ctx, value) => {
          //     Debug.Log($"Text submitted: {value}");
          //   },
          //   onEditingStarted: (ctx) => {
          //     Debug.Log($"Text editing started");
          //   },
          //   onEditingEnded: (ctx, value, reason) => {
          //     Debug.Log($"Text editing ended: {value.text} {reason}");
          //   }
          // );
          cx.Spacing(2);

          // cx.Text("Integer");
          // cx.Spacing(1);
          // cx.TextInput(
          //   integer,
          //   TextInputAdapters.Int32,
          //   onChanged: static (ctx, value) => {
          //     using (ctx.Modify<HomeComposable>(out var composable)) { composable.integer = value; }
          //   }
          // );
          // cx.Spacing(2);
          //
          // cx.Text("Float");
          // cx.Spacing(1);
          // cx.TextInput(
          //   floatingPoint,
          //   DecimalAdapter,
          //   onChanged: static (ctx, value) => {
          //     using (ctx.Modify<HomeComposable>(out var state)) {
          //       state.floatingPoint = value;
          //       Debug.Log($"Float changed: {value}");
          //     }
          //   }
          // );
        }
        cx.Spacing(2);

        using (cx.Group(Axis.Vertical, cross: Align.Stretch)) {
          cx.CURSOR.Size(BoxConstraints.Only(min: new StyleLength2(260f, 0f)));
          cx.Text("Volume");
          cx.Spacing(1);
          cx.Slider(
            volume,
            options: VolumeOptions,
            onChanged: static (ctx, value) => {
              using (ctx.Modify<HomeComposable>(out var state)) { state.volume = value; }
            }
          );
          cx.Spacing(2);

          cx.Checkbox(
            enabled,
            onChanged: static (ctx, value) => {
              using (ctx.Modify<HomeComposable>(out var state)) { state.enabled = value; }
            }
          );
          cx.Spacing(2);
          cx.Button(
            static (ref Composition cx) => cx.Text("Filled"),
            selected: enabled,
            style: ThemeProperties.ButtonFilled[in cx], action: static (ctx) => {
              using (ctx.Modify<HomeComposable>(out var state)) { state.enabled = !state.enabled; }
            }
          );
          cx.Spacing(2);
          cx.Button(
            static (ref Composition cx) => cx.Text("Outlined"),
            selected: enabled,
            style: ThemeProperties.ButtonOutlined[in cx], action: static (ctx) => {
              using (ctx.Modify<HomeComposable>(out var state)) { state.enabled = !state.enabled; }
            }
          );
          cx.Spacing(2);
          cx.Button(
            static (ref Composition cx) => cx.Text("Toggle"),
            selected: enabled,
            style: ThemeProperties.ButtonToggle[in cx], action: static (ctx) => {
              using (ctx.Modify<HomeComposable>(out var state)) { state.enabled = !state.enabled; }
            }
          );
          cx.Spacing(2);
          cx.Button(
            static (ref Composition cx) => cx.Text("Ghost"),
            selected: enabled,
            style: ThemeProperties.ButtonGhost[in cx], action: static (ctx) => {
              using (ctx.Modify<HomeComposable>(out var state)) { state.enabled = !state.enabled; }
            }
          );
        }

        cx.Slider(
          volume,
          options: VolumeOptionsScroll,
          onChanged: static (ctx, value) => {
            using (ctx.Modify<HomeComposable>(out var state)) { state.volume = value; }
          },
          style: ThemeProperties.Scroller[in cx]
        );
      }
      cx.Spacing(2);

      // var colors = new Color[] {
      //   MaterialColors.White,
      //   MaterialColors.Red,
      //   MaterialColors.Pink,
      //   MaterialColors.Purple,
      //   MaterialColors.DeepPurple,
      //   MaterialColors.Indigo,
      //   MaterialColors.Blue,
      //   MaterialColors.LightBlue,
      //   MaterialColors.Cyan,
      //   MaterialColors.Teal,
      //   MaterialColors.Green,
      //   MaterialColors.LightGreen,
      //   MaterialColors.Lime,
      //   MaterialColors.Yellow,
      //   MaterialColors.Amber,
      //   MaterialColors.Orange,
      //   MaterialColors.DeepOrange
      // };

      // for (var i = 0; i < colors.Length; i++) {
      //   using (cx.Flex(Axis.Horizontal, cross: Align.Stretch)) {
      //     cx.APPLY.Size(BoxConstraints.Tight(400, 32));
      //
      //     cx.DrawSolidBox(color: MaterialColors.Black).Flexible();
      //     cx.DrawSolidBox(color: Colors.ContrastBlend(MaterialColors.Black, colors[i], 0.08f)).Flexible();
      //     cx.DrawSolidBox(color: Colors.ContrastBlend(MaterialColors.Black, colors[i], 0.12f)).Flexible();
      //     cx.DrawSolidBox(color: Colors.ContrastBlend(MaterialColors.Black, colors[i], 0.38f)).Flexible();
      //     cx.DrawSolidBox(color: colors[i]).Flexible();
      //     cx.Spacing(2);
      //     cx.DrawSolidBox(color: colors[i]).Flexible();
      //     cx.DrawSolidBox(color: Colors.ContrastBlend(colors[i], MaterialColors.White, 0.08f)).Flexible();
      //     cx.DrawSolidBox(color: Colors.ContrastBlend(colors[i], MaterialColors.White, 0.12f)).Flexible();
      //     cx.DrawSolidBox(color: Colors.ContrastBlend(colors[i], MaterialColors.White, 0.38f)).Flexible();
      //     cx.DrawSolidBox(color: MaterialColors.White).Flexible();
      //     cx.Spacing(2);
      //     cx.DrawSolidBox(color: colors[i]).Flexible();
      //     cx.DrawSolidBox(color: Colors.ContrastBlend(colors[i], MaterialColors.Black, 0.08f)).Flexible();
      //     cx.DrawSolidBox(color: Colors.ContrastBlend(colors[i], MaterialColors.Black, 0.12f)).Flexible();
      //     cx.DrawSolidBox(color: Colors.ContrastBlend(colors[i], MaterialColors.Black, 0.38f)).Flexible();
      //     cx.DrawSolidBox(color: MaterialColors.Black).Flexible();
      //
      //   }
      // }

      cx.Spacing(3);
      ComposeFormExample(ref cx);
    }

    private static void ComposeNavigationHome(ref Composition cx, NavigationContextData navigation) {
      var theme = cx.ReadContextOrDefault(ThemeData.Key, HXThemes.DefaultDark);
      cx.CURSOR
        .BackgroundColor(theme.GetColor(ColorRoles.SurfaceContainerLow))
        .TextColor(theme.GetColor(ColorRoles.OnSurface));
      using (cx.Group(Axis.Vertical, main: Justify.Center, cross: Align.Center)) {
        if (cx.CursorDirty) cx.CURSOR.Fill().Padding(16f);
        cx.Text("Navigation home", TextRole.TitleMedium);
        cx.Spacing(1);
        cx.Text(
          "Registered routes, dynamic pages, typed results, and queued operations share one stack.", TextRole.BodySmall
        );
        cx.Spacing(2);
        cx.Button(
          static (ref Composition child) => child.Text("Push details"),
          action: static ctx => {
            var owner = ctx.Lookup<HomeComposable>();
            owner?._navigationController?.Push(
              "navigation-details",
              NavigationArguments.Empty.With("message", "Arguments are retained with the back-stack entry."),
              owner.CurrentNavigationOptions()
            );
          }
        );
        cx.Spacing(1);
        cx.Button(
          static (ref Composition child) => child.Text("Go to settings (replace stack)"),
          style: ThemeProperties.ButtonOutlined[in cx],
          action: static ctx => {
            var owner = ctx.Lookup<HomeComposable>();
            owner?._navigationController?.Go(
              "navigation-settings", options: owner.CurrentNavigationOptions()
            );
          }
        );
      }
    }

    private static void ComposeNavigationDetails(ref Composition cx, NavigationContextData navigation) {
      var theme = cx.ReadContextOrDefault(ThemeData.Key, HXThemes.DefaultDark);
      cx.CURSOR
        .BackgroundColor(theme.GetColor(ColorRoles.SecondaryContainer))
        .TextColor(theme.GetColor(ColorRoles.OnSecondaryContainer));
      using (cx.Group(Axis.Vertical, main: Justify.Center, cross: Align.Center)) {
        if (cx.CursorDirty) cx.CURSOR.Fill().Padding(16f);
        cx.Text("Details route", TextRole.TitleMedium);
        cx.Spacing(1);
        cx.Text(navigation.Arguments.Get("message", "No route argument was supplied."));
        cx.Text($"Entry #{navigation.entry.Id}", TextRole.BodySmall);
        cx.Spacing(2);
        cx.Button(
          static (ref Composition child) => child.Text("Push single-top update"),
          action: static ctx => {
            var owner = ctx.Lookup<HomeComposable>();
            if (owner == null) return;
            int sequence;
            using (ctx.Modify<HomeComposable>(out var state)) sequence = ++state.dynamicPageSequence;
            owner._navigationController?.Push(
              "navigation-details",
              NavigationArguments.Empty.With("message", $"Single-top update #{sequence}; entry id is preserved."),
              owner.CurrentNavigationOptions().SingleTop()
            );
          }
        );
        cx.Spacing(1);
        cx.Button(
          static (ref Composition child) => child.Text("Pop route"),
          style: ThemeProperties.ButtonOutlined[in cx],
          action: static ctx => ctx.NavigationController()?.Pop()
        );
      }
    }

    private static void ComposeNavigationSettings(ref Composition cx, NavigationContextData navigation) {
      var theme = cx.ReadContextOrDefault(ThemeData.Key, HXThemes.DefaultDark);
      cx.CURSOR
        .BackgroundColor(theme.GetColor(ColorRoles.PrimaryContainer))
        .TextColor(theme.GetColor(ColorRoles.OnPrimaryContainer));
      using (cx.Group(Axis.Vertical, main: Justify.Center, cross: Align.Center)) {
        if (cx.CursorDirty) cx.CURSOR.Fill().Padding(16f);
        cx.Text("Settings route", TextRole.TitleMedium);
        cx.Spacing(1);
        cx.Text("Use this page to test replacement, go, pop-to, and queued transitions.", TextRole.BodySmall);
        cx.Spacing(2);
        cx.Button(
          static (ref Composition child) => child.Text("Replace with details"),
          action: static ctx => {
            var owner = ctx.Lookup<HomeComposable>();
            owner?._navigationController?.Replace(
              "navigation-details",
              NavigationArguments.Empty.With("message", "Settings was replaced by this page."),
              owner.CurrentNavigationOptions()
            );
          }
        );
        cx.Spacing(1);
        cx.Button(
          static (ref Composition child) => child.Text("Back to home"),
          style: ThemeProperties.ButtonOutlined[in cx],
          action: static ctx => {
            var controller = ctx.NavigationController();
            if (controller == null) return;
            if (controller.CanPop) controller.Pop();
            else controller.Reset("navigation-home");
          }
        );
      }
    }

    private static void ComposeDynamicResultPage(ref Composition cx, NavigationContextData navigation) {
      var theme = cx.ReadContextOrDefault(ThemeData.Key, HXThemes.DefaultDark);
      var sequence = navigation.Arguments.Get("sequence", 0);
      cx.CURSOR
        .BackgroundColor(theme.GetColor(ColorRoles.TertiaryContainer))
        .TextColor(theme.GetColor(ColorRoles.OnTertiaryContainer));
      using (cx.Group(Axis.Vertical, main: Justify.Center, cross: Align.Center)) {
        if (cx.CursorDirty) cx.CURSOR.Fill().Padding(16f);
        cx.Text($"Dynamic page #{sequence}", TextRole.TitleMedium);
        cx.Spacing(1);
        cx.Text("This page was not registered in the graph. Pop it with a typed result.", TextRole.BodySmall);
        cx.Spacing(2);
        cx.Button(
          static (ref Composition child) => child.Text("Return accepted result"),
          action: static ctx => {
            var entry = ctx.NavigationEntry();
            ctx.NavigationController()?.Pop($"Accepted {entry?.Name ?? "dynamic page"}");
          }
        );
        cx.Spacing(1);
        cx.Button(
          static (ref Composition child) => child.Text("Return null"),
          style: ThemeProperties.ButtonOutlined[in cx],
          action: static ctx => ctx.NavigationController()?.Pop()
        );
      }
    }

    private INavigationTransition ResolveNavigationTransition() => navigationTransition switch {
      NavigationTransitionKind.Fade => NavigationTransitions.Fade,
      NavigationTransitionKind.Instant => NavigationTransitions.Instant,
      _ => NavigationTransitions.SlideHorizontal
    };

    private NavigationOptions CurrentNavigationOptions() =>
      NavigationOptions.Default.WithTransition(ResolveNavigationTransition());

    private static void PushDynamicResultPage(CompositionContext context) {
      var owner = context.Lookup<HomeComposable>();
      var controller = owner?._navigationController;
      if (owner == null || controller == null) return;

      int sequence;
      using (context.Modify<HomeComposable>(out var sequenceState)) {
        sequence = ++sequenceState.dynamicPageSequence;
      }
      controller.Push<string>(
        NavigationPage.Build(ComposeDynamicResultPage)
          .Name($"dynamic-result-{sequence}")
          .Transition(owner.ResolveNavigationTransition()),
        onResult: OnDynamicNavigationResult,
        arguments: NavigationArguments.Empty.With("sequence", sequence),
        options: owner.CurrentNavigationOptions(),
        onError: OnDynamicNavigationError
      );
    }

    private static void OnDynamicNavigationResult(CompositionContext context, string result) {
      using (context.Modify<HomeComposable>(out var state)) {
        state.navigationResult = result ?? "<null>";
      }
    }

    private static void OnDynamicNavigationError(CompositionContext context, Exception exception) {
      using (context.Modify<HomeComposable>(out var state)) {
        state.navigationResult = $"Failed: {exception.Message}";
      }
    }

    private static void RunQueuedNavigationDemo(CompositionContext context) {
      var owner = context.Lookup<HomeComposable>();
      var controller = owner?._navigationController;
      if (owner == null || controller == null) return;
      var options = owner.CurrentNavigationOptions();
      controller.Reset();
      controller.Push(
        "navigation-details",
        NavigationArguments.Empty.With("message", "First queued destination."),
        options
      );
      controller.Push("navigation-settings", options: options);
      controller.Pop();
    }

    private static void RecordNavigationLifecycle(CompositionContext context, NavigationEvent navigation) {
      using (context.Modify<HomeComposable>(out var state)) {
        state.navigationLifecycle = $"{navigation.Entry.Name} {navigation.Phase} ({navigation.Operation})";
      }
    }

    private static void OpenNavigationDialog(CompositionContext context) {
      var owner = context.Lookup<HomeComposable>();
      if (owner?._dialogNavigationController == null) return;
      owner._dialogNavigationController.Reset();
      Overlay.Build(ComposeNavigationDialog)
        .Modal(dismissOnOutsidePointer: true)
        .Constraints(
          BoxConstraints.Only(
            min: new StyleLength2(560f, 420f),
            max: new StyleLength2(720f, 520f)
          )
        )
        .DismissOnCancel()
        .Show(context);
    }

    private static void ComposeNavigationDialog(ref Composition cx, OverlayContextData overlay) {
      var owner = cx.Lookup<HomeComposable>();
      var theme = cx.ReadContextOrDefault(ThemeData.Key, HXThemes.DefaultDark);
      cx.CURSOR
        .BackgroundColor(theme.GetColor(ColorRoles.SurfaceContainer))
        .TextColor(theme.GetColor(ColorRoles.OnSurfaceContainer))
        .BorderRadius(16f);
      using (cx.Group(Axis.Vertical, cross: Align.Stretch)) {
        if (cx.CursorDirty) cx.CURSOR.Fill().Padding(20f);
        cx.Text("Navigation-driven dialog", TextRole.TitleLarge);
        cx.Spacing(1);
        cx.Text("The dialog owns an independent graph, controller, queue, and back stack.", TextRole.BodySmall);
        cx.Spacing(2);
        if (owner?._dialogNavigationController != null) {
          cx.NavigationHost(
              owner._dialogNavigationGraph,
              owner._dialogNavigationController,
              NavigationTransitions.Fade
            )
            .Height(280f)
            .BorderRadius(10f)
            .Overflow(Overflow.Hidden);
        }
        cx.Spacing(2);
        cx.Button(
          static (ref Composition child) => child.Text("Cancel dialog"),
          style: ThemeProperties.ButtonGhost[in cx],
          action: static ctx => ctx.OverlayEntry()?.Dismiss()
        );
      }
    }

    private static void ComposeDialogIntro(ref Composition cx, NavigationContextData navigation) {
      ComposeDialogPage(
        ref cx,
        "Step 1 · Intro",
        "Registered dialog routes use the same navigation host as full pages.",
        static (ref Composition child) => child.Text("Continue"),
        navigation.CanPop,
        static ctx => ctx.NavigationController()?.Push("dialog-details")
      );
    }

    private static void ComposeDialogDetails(ref Composition cx, NavigationContextData navigation) {
      ComposeDialogPage(
        ref cx,
        "Step 2 · Details",
        "Back and forward operations are serialized while the fade transition is active.",
        static (ref Composition child) => child.Text("Review"),
        navigation.CanPop,
        static ctx => ctx.NavigationController()?.Push("dialog-confirm")
      );
    }

    private static void ComposeDialogConfirm(ref Composition cx, NavigationContextData navigation) {
      var theme = cx.ReadContextOrDefault(ThemeData.Key, HXThemes.DefaultDark);
      cx.CURSOR
        .BackgroundColor(theme.GetColor(ColorRoles.PrimaryContainer))
        .TextColor(theme.GetColor(ColorRoles.OnPrimaryContainer));
      using (cx.Group(Axis.Vertical, main: Justify.Center, cross: Align.Center)) {
        if (cx.CursorDirty) cx.CURSOR.Fill().Padding(16f);
        cx.Text("Step 3 · Confirm", TextRole.TitleMedium);
        cx.Spacing(1);
        cx.Text("Finish dismisses the overlay; Back pops only the dialog navigator.", TextRole.BodySmall);
        cx.Spacing(2);
        using (cx.Group(Axis.Horizontal, cross: Align.Center)) {
          cx.Button(
            static (ref Composition child) => child.Text("Back"),
            style: ThemeProperties.ButtonOutlined[in cx],
            action: static ctx => ctx.NavigationController()?.Pop()
          );
          cx.Spacing(1);
          cx.Button(
            static (ref Composition child) => child.Text("Finish"),
            action: static ctx => ctx.OverlayEntry()?.Dismiss()
          );
        }
      }
    }

    private void ComposeFormExample(ref Composition cx) {
      cx.Text("Compose form context", TextRole.TitleMedium);
      cx.Spacing(1);

      // Form context is published through Compose's retained context contributor.
      using (cx.ProvideForm(_exampleForm)) {
        cx.SubscribeTo(_exampleForm);
        var form = cx.RequireForm();
        var profile = form.WithPrefix("profile");
        _exampleNameField.Attach(
          profile,
          profile.Resolve("name"),
          validators: new[] { FormValidators.Required("A display name is required.") },
          validationMode: ValidationMode.OnDirty | ValidationMode.OnSubmit,
          initialValue: "Ada"
        );
        _exampleUpdatesField.Attach(form, form.Resolve("receiveUpdates"), initialValue: true);

        using (cx.Group(Axis.Vertical, cross: Align.Stretch)) {
          if (cx.CursorDirty) cx.CURSOR.Size(BoxConstraints.Only(min: new StyleLength2(320f, 0f)));
          cx.Text("Profile name", TextRole.LabelLarge);
          cx.TextField(
            value: new TextEditingValue(_exampleForm.GetValue(profile.Resolve("name"), "")),
            onChanged: static (context, value) =>
              context.Lookup<HomeComposable>()?._exampleNameField.SetUserValue(value.text)
          );
          cx.Spacing(1);
          cx.Checkbox(
            _exampleForm.GetValue(form.Resolve("receiveUpdates"), false),
            onChanged: static (context, value) =>
              context.Lookup<HomeComposable>()?._exampleUpdatesField.SetUserValue(value)
          );
          cx.Text("Receive product updates", TextRole.BodySmall);
          cx.Spacing(1);
          cx.Text(
            $"Dirty: {_exampleForm.IsDirty}  •  Errors: {_exampleForm.HasErrors}  •  " +
            $"Submitted: {_exampleForm.SubmitAttempted}",
            TextRole.BodySmall
          );
          cx.Spacing(1);
          using (cx.Group(Axis.Horizontal)) {
            cx.Button(
              static (ref Composition child) => child.Text("Submit"),
              action: static context => {
                var form = context.Lookup<HomeComposable>()?._exampleForm;
                if (form == null) return;
                var result = form.Submit();
                Debug.Log($"Compose form submit ({(result.valid ? "valid" : "invalid")}): {form.FormatData(result.data)}");
              }
            );
            cx.Spacing(1);
            cx.Button(
              static (ref Composition child) => child.Text("Reset"),
              style: ThemeProperties.ButtonOutlined[in cx],
              action: static context => context.Lookup<HomeComposable>()?._exampleForm.Reset()
            );
          }
        }
      }
    }

    private static void ComposeDialogPage(
      ref Composition cx,
      string title,
      string body,
      Composable nextContent,
      bool canPop,
      CompositionAction next
    ) {
      var theme = cx.ReadContextOrDefault(ThemeData.Key, HXThemes.DefaultDark);
      cx.CURSOR
        .BackgroundColor(theme.GetColor(ColorRoles.SurfaceContainerLow))
        .TextColor(theme.GetColor(ColorRoles.OnSurface));
      using (cx.Group(Axis.Vertical, main: Justify.Center, cross: Align.Center)) {
        if (cx.CursorDirty) cx.CURSOR.Fill().Padding(16f);
        cx.Text(title, TextRole.TitleMedium);
        cx.Spacing(1);
        cx.Text(body, TextRole.BodySmall);
        cx.Spacing(2);
        using (cx.Group(Axis.Horizontal, cross: Align.Center)) {
          cx.Button(
            static (ref Composition child) => child.Text("Back"),
            enabled: canPop,
            style: ThemeProperties.ButtonOutlined[in cx],
            action: static ctx => ctx.NavigationController()?.Pop()
          );
          cx.Spacing(1);
          cx.Button(nextContent, action: next);
        }
      }
    }

    private static void ComposeExampleModal(ref Composition cx, OverlayContextData overlay) {
      var theme = cx.ReadContextOrDefault(ThemeData.Key, HXThemes.DefaultDark);
      cx.CURSOR
        .BackgroundColor(theme.GetColor(ColorRoles.SurfaceContainer))
        .TextColor(theme.GetColor(ColorRoles.OnSurfaceContainer))
        .BorderRadius(16f);
      using (cx.Group(Axis.Vertical, cross: Align.Stretch)) {
        if (cx.CursorDirty) cx.CURSOR.Padding(20f);
        cx.Text("Composable modal", TextRole.TitleLarge);
        cx.Spacing(1);
        cx.Text(
          "The modal, its barrier, focus policy, and dismissal rules are all an overlay entry.", TextRole.BodySmall
        );
        cx.Spacing(2);
        cx.Button(
          static (ref Composition child) => child.Text("Close"),
          action: static ctx => ctx.OverlayEntry()?.Dismiss()
        );
      }
    }

    private static void ComposeExampleMenu(ref Composition cx, OverlayContextData overlay) {
      var theme = cx.ReadContextOrDefault(ThemeData.Key, HXThemes.DefaultDark);
      cx.CURSOR
        .BackgroundColor(theme.GetColor(ColorRoles.SurfaceContainerHighest))
        .TextColor(theme.GetColor(ColorRoles.OnSurface))
        .BorderRadius(10f);
      using (cx.Group(Axis.Vertical, cross: Align.Stretch)) {
        if (cx.CursorDirty) cx.CURSOR.Padding(8f);
        cx.Button(
          static (ref Composition child) => child.Text("First menu action"),
          style: ThemeProperties.ButtonGhost[in cx],
          action: static ctx => ctx.OverlayEntry()?.Dismiss()
        );
        cx.Button(
          static (ref Composition child) => child.Text("Second menu action"),
          style: ThemeProperties.ButtonGhost[in cx],
          action: static ctx => ctx.OverlayEntry()?.Dismiss()
        );
      }
    }

    private static void ComposeExampleNotification(ref Composition cx, OverlayContextData overlay) {
      var theme = cx.ReadContextOrDefault(ThemeData.Key, HXThemes.DefaultDark);
      cx.CURSOR
        .BackgroundColor(theme.GetColor(ColorRoles.PrimaryContainer))
        .TextColor(theme.GetColor(ColorRoles.OnPrimaryContainer))
        .BorderRadius(12f);
      using (cx.Group(Axis.Horizontal, cross: Align.Center)) {
        if (cx.CursorDirty) cx.CURSOR.Padding(12f);
        cx.Text("A stacked notification that dismisses after 3.5 seconds.").Flexible();
        cx.Spacing(1);
        cx.Button(
          static (ref Composition child) => child.Text("Dismiss"),
          style: ThemeProperties.ButtonGhost[in cx],
          action: static ctx => ctx.OverlayEntry()?.Dismiss()
        );
      }
    }
  }
}
