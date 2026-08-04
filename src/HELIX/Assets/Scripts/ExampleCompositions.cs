using HELIX.Coloring;
using HELIX.Compose;
using HELIX.Signals;
using HELIX.Theming;
using HELIX.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace TestNamespace {
  public static partial class ExampleCompositions {
    public static ulong counter = 0;
    public static readonly Signal<int> CounterSignal = Signal.Value(0);

    public static readonly SpecConfig MyFactory = new SpecConfig(SpecConfig.Default)
      .AddFactory<ButtonSpecs>(ButtonSpecDrawer);

    [Composition]
    private static void _MyComposition(ref Composition cx) {
      var theme = ThemeData.Key[cx];
      counter++;

      using (cx.WriteContext(out var context)) {
        SpecConfig.Key[in context] = MyFactory;
        TextStyle.Merge(in context, in theme[TextRole.BodyMedium].style);
      }

      cx.CURSOR.Name("MainBoundary");

      using (cx.ScrollView()) {
        cx.CURSOR.Size(BoxConstraints.Tight(200, 200));

        cx.Text("Scroll Item 1\n\n\n\n\n\n");
        cx.Text("Scroll Item 2\n\n\n\n\n\n");
        cx.Text("Scroll Item 3\n\n\n\n\n\n");
        cx.Text("Scroll Item 4\n\n\n\n\n\n");
        cx.Text("Scroll Item 5\n\n\n\n\n\n");
      }

      using (cx.ScrollView(axis: Axis.Horizontal)) {
        cx.CURSOR.Size(BoxConstraints.Tight(200, 200));

        cx.Text("Scroll Item 1\n\n\n\n\n\n");
        cx.Text("Scroll Item 2\n\n\n\n\n\n");
        cx.Text("Scroll Item 3\n\n\n\n\n\n");
        cx.Text("Scroll Item 4\n\n\n\n\n\n");
        cx.Text("Scroll Item 5\n\n\n\n\n\n");
      }


      // using var exampleContext = cx.WriteContext<ExampleContext>();
      // exampleContext.value.counter = counter;
      using (cx.Flex(Axis.Vertical, main: Justify.Center, cross: Align.Center)) {
        cx.CURSOR.Padding(10).Padding(20);

        //cx.APPLY.BackgroundColor(theme.GetColor(ColorRoles.Surface));

        cx.Text($"Title");
        using (cx.Flex(Axis.Horizontal)) {
          cx.Text($"Hello");
          cx.Text($"World");
        }
        cx.Text($"AfterGroup1");

        // cx.Button(ButtonContent, static x => {
        //   clickCounter++;
        // }).Padding(20);

        cx.DrawSolidBox(
          color: theme[ColorRoles.SurfaceContainerLow],
          constraints: BoxConstraints.Preferred(64, 64)
        );
        cx.DrawSolidBox(
          color: theme[ColorRoles.SurfaceContainer],
          constraints: BoxConstraints.Preferred(64, 64)
        );
        cx.DrawSolidBox(
          color: theme[ColorRoles.SurfaceContainerHigh],
          constraints: BoxConstraints.Preferred(64, 64)
        );
        cx.DrawSolidBox(
          color: theme[ColorRoles.SurfaceContainerHighest],
          constraints: BoxConstraints.Preferred(64, 64)
        );

        //if (cx.Conditional(counter / 100 % 2 == 0))
        // cx.Button(
        //   $"Click me Switch",
        //   static x => {
        //     clickCounter++;
        //     Debug.Log($"Click {clickCounter} Switch!");
        //   }
        // ).BackgroundColor(Colors.BlueGrey).Display(counter / 100 % 2 == 0);

        cx.Text($"AfterSwitch");
        cx.HXButton(
          static (ref Composition cx) => {
            cx.Text($"Click me {CounterSignal.Value}");
          },
          static ctx => {
            CounterSignal.Value++;
          },
          selected: true
        );
        cx.Text($"AfterButton");

        cx.Text($"Title2");
        using (cx.Flex(Axis.Horizontal)) {
          cx.Text($"Hello").TextColor(new Color(counter % 20 / 20f, 0f, 0f));
          //if (cx.Conditional(counter % 200 == 0)) cx.Text($"World").Padding(100);
          cx.Text($"Another");
        }
        cx.Text($"Title2After");

        //if (cx.Conditional(counter / 100 % 2 == 0))
        cx.Boundary(InnerComposition);
        cx.CURSOR.Name("InnerBoundary");

        cx.TextField();

        cx.Text($"Title3");
        using (cx.Flex(Axis.Horizontal)) {
          cx.Text($"Hello");
          cx.Text($"World");
        }
      }
    }

    [Composition]
    public static void _InnerComposition(ref Composition cx) {
      new ButtonSpecs {
        Label = "Click me from InnerComposition", OnClick = static x => {
          CounterSignal.Value++;
          Debug.Log($"Click {CounterSignal.Value} from InnerComposition!");
        }
      }.Compose(ref cx);
    }

    public static void ButtonSpecDrawer(ref Composition cx, in ButtonSpecs specs) {
      cx.HXButton(
        static (ref Composition cx) => {
          cx.Text("Click me Spec");
        },
        specs.OnClick
      );
    }

    public struct ButtonSpecs : ISpec {
      public string Label { get; set; }
      public CompositionAction OnClick { get; set; }
    }

    // [CompositionBoundary]
    // public static partial void ClickRegion(
    //   this ref Composition cx
    // );
    //
    // public partial class ClickRegionState {
    //   protected override void OnAttach() {
    //     base.OnAttach();
    //   }
    // }

    private static readonly Composable<State> ButtonContent =
      static (ref Composition cx, State state) => {
        var color = Colors.Black;
        if (state.Active()) color = Colors.Red;
        else if (state.Hovered()) color = Colors.Blue;

        cx.Text("Button").TextColor(color);
      };
  }


  [UxmlElement]
  public partial class ExampleVisualElement : VisualElement {
    public ExampleVisualElement() {
      var boundaryNode = new CompositionBoundaryNode { composable = ExampleCompositions.MyComposition };
      Add(boundaryNode);
      //RecompositionScope.MarkDirty(boundaryNode);
      // schedule.Execute(() => {
      //     RecompositionScope.MarkDirty(boundaryNode);
      //   }
      // ).Every(0).Resume();
    }
  }
}
