using HELIX;
using HELIX.Boot;
using HELIX.Compose;
using HELIX.Context;
using HELIX.UI;
using HELIX.UI.Console;
using HELIX.UI.Options;
using UnityEngine.UIElements;

namespace DefaultNamespace {
  [Managed(typeof(ApplicationScope))]
  public partial class ExampleServiceGui {
    [Inject] public OptionsService optionsService;
    [Inject] public GuiService panel;

    [Hook]
    private void OnInit() {

    }

    [EventHandler]
    public void OnBuildNavigation(BuildNavigationEvent evt) {
      evt.builder.Route(
        "options",
        NavigationPage.Build((ref Composition cx, NavigationContextData value) => {
            var pages = optionsService.BuildOptionPages(new OptionPagesOptions(
              showChangedOptionReset: true,
              showDefaultOptionReset: true
            ));
            pages.Compose(ref cx);
          }
        )
      );

      evt.builder.Route(
        "/",
        NavigationPage.Build((ref Composition cx, NavigationContextData value) => { OnMain(ref cx); })
      );
    }

    private void OnMain(ref Composition cx) {
      cx.Text("Welcome to the Example Service GUI!");
      cx.Button(
        static (ref Composition cx) => { cx.Text("Options"); },
        action: _ => { panel.navigation.Activate("options", options: NavigationOptions.Default); }
      );
    }
  }


  [Managed(typeof(ApplicationScope), phase: LoadPhase.Configuration)]
  public partial class UserOptionsConfiguration {

    [RegisterOption]
    public readonly Option<float> audioVolume = new(Datatypes.PercentNormalized, 0.9f);


    [RegisterOption]
    public readonly Option<string> userName = new(Datatypes.String, "User") {
      eagerness = OptionEagerness.Delayed
    };

    [RegisterOption]
    public readonly Option<int> userAge = new(Datatypes.Int, 18) {
      eagerness = OptionEagerness.Delayed
    };

    [RegisterOption("user.anonymous")]
    public readonly Option<bool> anonymous = new(Datatypes.Bool, false) {
      eagerness = OptionEagerness.Confirmed,
      defaultResettable = true
    };

    [EventHandler]
    private void OnRender(OptionsRenderEvent evt) {
      evt.DeclareCategory(
        path: "user",
        title: "User",
        description: "Configure your user settings below.",
        icon: FaSolidIcons.Ref(FaSolidIcons.Person)
      );
    }
  }
}
