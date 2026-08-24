using HELIX;
using HELIX.Boot;
using HELIX.Compose;
using HELIX.Context;
using HELIX.UI;
using HELIX.Widgets.Universal;
using UnityEngine.UIElements;

namespace DefaultNamespace {
  [Managed(typeof(ApplicationScope))]
  public partial class ExampleServiceGui {
    [Inject] public OptionsService optionsService;
    [Inject] public GuiService panel;

    [EventHandler]
    public void OnBuildNavigation(BuildNavigationEvent evt) {
      evt.builder.Route(
        "options",
        NavigationPage.Build((ref Composition cx, NavigationContextData value) => {
            var pages = optionsService.BuildOptionPages();
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
    public readonly Option<string> userName = new(Datatypes.String, "User");

    [RegisterOption]
    public readonly Option<int> userAge = new(Datatypes.Int, 18);

    [RegisterOption("user.anonymous")]
    public readonly Option<bool> anonymous = new(Datatypes.Bool, false);

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