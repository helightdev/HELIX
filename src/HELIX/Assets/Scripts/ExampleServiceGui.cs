using System;
using HELIX;
using HELIX.Boot;
using HELIX.Compose;
using HELIX.Context;
using HELIX.UI;
using HELIX.UI.Console;
using HELIX.UI.Options;
using UnityEngine.UIElements;
using UnityEngine;

namespace DefaultNamespace {
  [Managed(typeof(ApplicationScope))]
  [Mixable] public partial class ExampleServiceGui {
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


  [Mixable, Managed(typeof(ApplicationScope), phase: LoadPhase.Configuration)]
  public partial class UserOptionsConfiguration {

    private static readonly CompositeDatatype<UserProfile> _profileDatatype = new(
      new ICompositeDatatypeComponent<UserProfile>[] {
        new CompositeDatatypeComponent<UserProfile, string>(
          "Display name", Datatypes.String, value => value.displayName,
          (value, component) => new UserProfile(component, value.notifications)
        ),
        new CompositeDatatypeComponent<UserProfile, bool>(
          "Notifications", Datatypes.Bool, value => value.notifications,
          (value, component) => new UserProfile(value.displayName, component)
        )
      },
      JsonUtility.ToJson,
      JsonUtility.FromJson<UserProfile>
    );

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

    [RegisterOption("user.spawn-position")]
    public readonly Option<Vector3> spawnPosition = new(Datatypes.Vector3, new Vector3(1f, 2f, 3f)) {
      eagerness = OptionEagerness.Delayed,
      defaultResettable = true
    };

    [RegisterOption("user.profile")]
    public readonly Option<UserProfile> profile = new(
      _profileDatatype, new UserProfile("User", true)
    ) {
      eagerness = OptionEagerness.Delayed,
      defaultResettable = true
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

  [Serializable]
  public sealed class UserProfile : IEquatable<UserProfile> {
    public string displayName;
    public bool notifications;

    public UserProfile(string displayName, bool notifications) {
      this.displayName = displayName;
      this.notifications = notifications;
    }

    public bool Equals(UserProfile other) => other != null &&
      displayName == other.displayName && notifications == other.notifications;
    public override bool Equals(object obj) => obj is UserProfile other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(displayName, notifications);
  }
}
