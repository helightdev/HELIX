using HELIX.Extensions;
using HELIX.NW;
using HELIX.NW.Forms;
using HELIX.NW.Navigation;
using HELIX.NW.Overlays;
using HELIX.Types;
using HELIX.Widgets.Universal;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

namespace HELIX.Examples {
  [UxmlElement]
  public partial class NwSystemExampleElement : BoundaryVisualElement {
    public override void Compose(ref Composition cx) {
      this.Fill();
      cx.NWSystemExample();
      cx.APPLY.Fill();
    }
  }

  public static partial class NwSystemsExample {
    [CompositionBoundary] public static partial void NWSystemExample(ref this Composition cx);

    public partial class NWSystemExampleComposable {
      public FormController FormController { get; private set; }
      public NavigationController NavigationController { get; private set; }
      public OverlayController OverlayController { get; private set; }
      public NavigationRoute HomeRoute { get; private set; }

      public OverlayHandle modal;

      protected override void OnAttach() {
        base.OnAttach();
        FormController = new FormController();
        NavigationController = new NavigationController();
        OverlayController = new OverlayController();
        HomeRoute = new NavigationRoute("home", ComposeHome);

        NavigationController.Reset(HomeRoute);
      }

      protected override void OnDetach() {
        base.OnDetach();
        NavigationController.Dispose();
        OverlayController.Dispose();
      }

      protected override void OnRecompose(ref Composition cx) {
        var theme = cx.ReadContextOrDefault(ThemeData.Context, BuiltinThemes.DefaultDark);
        cx.APPLY
          .BackgroundColor(theme.GetColor(ColorRoles.Surface))
          .TextColor(theme.GetColor(ColorRoles.OnSurface));

        cx.OverlayHost(ComposeApplication, OverlayController);
      }
    }

    [Composition] static void _ComposeApplication(ref Composition cx, OverlayController overlays) {
      var controller = cx.LookupComposable<NWSystemExampleComposable>().NavigationController;
      cx.NavigationHost(controller);
    }

    [Composition] static void _ComposeHome(ref Composition cx, NavigationEntry entry) => Home(ref cx);

    [CompositionBoundary] public static partial void Home(ref Composition cx);

    public partial class HomeComposable {
      public enum ExampleMode : byte { Balanced, Performance, Quality }

      public static readonly DropdownOption<ExampleMode>[] ModeOptions = {
        new(ExampleMode.Balanced, "Balanced"), new(ExampleMode.Performance, "Performance"),
        new(ExampleMode.Quality, "Quality")
      };

      public static readonly IFormValidator[] RequiredName = { FormValidators.Required("Enter a display name.") };

      public static readonly IFormValidator[] AdultAge = {
        FormValidators.Range(18, 120, "Age must be between 18 and 120.")
      };

      public static readonly SliderOptions VolumeOptions = new(0f, 1f, step: 0f, thumbRange: 0.1f);
      public static readonly NumericInputOptions DecimalOptions = new(format: "0.00");

      public string text = "Editable text";
      public int integer = 12;
      public float floatingPoint = 1.25f;
      public float volume = 0.65f;
      public bool enabled = true;
      public ExampleMode mode = ExampleMode.Balanced;
      public OverlayHandle modal;


      protected override void OnRecompose(ref Composition cx) {
        var systemState = cx.LookupComposable<NWSystemExampleComposable>();

        using (cx.Flex(Axis.Vertical, cross: Align.Stretch)) {
          cx.APPLY.Padding(16f);
          cx.Text("HELIX NW system example").TextRole(TextRole.TitleLarge);
          cx.Spacing(2);
          cx.Text("Controlled inputs");
          cx.Spacing(2);

          var font = Resources.Load<FontAsset>("helix/fa/FontAwesome7FreeSolid");

          using (cx.Decorator(out var slots)) {
            using (slots.Prefix()) {
              CommonDecoratorElement.ComposeScopedLabel(
                ref cx,
                new LabelSpec("Prefix", new IconRef(FaSolidIcons.User.ToString(), font).Composable())
              );
            }
            using (slots.Element()) cx.Text("Element Data").TextRole(TextRole.BodyMedium);
            using (slots.Suffix()) CommonDecoratorElement.ComposeScopedLabel(ref cx, new LabelSpec("Suffix"));
            using (slots.Label()) CommonDecoratorElement.ComposeScopedLabel(ref cx, new LabelSpec("Label"));
            using (slots.Before()) CommonDecoratorElement.ComposeScopedLabel(ref cx, new LabelSpec("Before"));
            using (slots.After()) CommonDecoratorElement.ComposeScopedLabel(ref cx, new LabelSpec("After"));
            using (slots.Description()) CommonDecoratorElement.ComposeScopedLabel(ref cx, new LabelSpec("Description"));
          }

          using (cx.Flex(Axis.Horizontal, cross: Align.FlexStart)) {
            using (cx.Flex(Axis.Vertical, cross: Align.Stretch)) {
              cx.APPLY.Size(BoxConstraints.Only(min: new StyleLength2(260f, 0f)));
              cx.Text("Text");
              cx.Spacing(1);
              cx.TextInput(
                text, (ctx, value) => {
                  using (ctx.ModifyComposable<HomeComposable>(out var composable)) {
                    composable.text = value;
                  }
                }
              );
              cx.Spacing(2);

              cx.Text("Integer");
              cx.Spacing(1);
              cx.IntInput(
                integer, (ctx, value) => {
                  using (ctx.ModifyComposable<HomeComposable>(out var composable)) {
                    composable.integer = value;
                  }
                }
              );
              cx.Spacing(2);

              cx.Text("Float");
              cx.Spacing(1);
              cx.FloatInput(
                floatingPoint,
                options: DecimalOptions,
                onChanged: static (ctx, value) => {
                  using (ctx.ModifyComposable<HomeComposable>(out var state)) {
                    state.volume = value;
                  }
                }
              );
            }
            cx.Spacing(2);

            using (cx.Flex(Axis.Vertical, cross: Align.Stretch)) {
              cx.APPLY.Size(BoxConstraints.Only(min: new StyleLength2(260f, 0f)));
              cx.Text("Volume");
              cx.Spacing(1);
              cx.Slider(
                volume,
                options: VolumeOptions,
                onChanged: static (ctx, value) => {
                  using (ctx.ModifyComposable<HomeComposable>(out var state)) {
                    state.volume = value;
                  }
                }
              );
              cx.Spacing(2);

              cx.Checkbox(
                enabled,
                new PrefixLabelSuffixSpec("Enabled"),
                static (ctx, value) => {
                  using (ctx.ModifyComposable<HomeComposable>(out var state)) {
                    state.enabled = value;
                  }
                }
              );
              cx.Spacing(2);

              cx.Text("Rendering mode");
              cx.Spacing(1);
              cx.Dropdown(
                mode,
                ModeOptions,
                onChanged: static (ctx, mode) => {
                  using (ctx.ModifyComposable<HomeComposable>(out var state)) {
                    state.mode = mode;
                  }
                }
              );
            }
          }
          cx.Spacing(2);

          using (cx.Flex(Axis.Horizontal, cross: Align.Center)) {
            cx.Button(
              new PrefixLabelSuffixSpec("Open details"),
              static ctx => {
                ctx.LookupComposable<NWSystemExampleComposable>().NavigationController.Push(
                  _detailsRoute, "This value was passed through NavigationRoute<string>."
                );
              }
            );
            cx.Spacing(2);
            cx.Button(
              new PrefixLabelSuffixSpec("Open menu"),
              static ctx => {
                var home = ctx.LookupComposable<HomeComposable>();
                var system = ctx.LookupComposable<NWSystemExampleComposable>();

                var nestedItems = new[] {
                  new MenuItemSpec(
                    "Balanced", () => {
                      home.mode = ExampleMode.Balanced;
                      home.MarkDirty();
                    }
                  ),
                  new MenuItemSpec(
                    "Performance", () => {
                      home.mode = ExampleMode.Performance;
                      home.MarkDirty();
                    }
                  ),
                  new MenuItemSpec(
                    "Quality", () => {
                      home.mode = ExampleMode.Quality;
                      home.MarkDirty();
                    }
                  )
                };
                var menuItems = new[] {
                  new MenuItemSpec(
                    "Show notification", () => {
                      var notification = new NotificationSpec("Notification from Menu item");
                      system.OverlayController.ShowNotification(in notification);
                    }
                  ),
                  new MenuItemSpec("Mode", children: nestedItems), MenuItemSpec.Separator(), new MenuItemSpec(
                    "Open details", () => {
                      system.NavigationController.Push(
                        _detailsRoute,
                        "This value was passed through NavigationRoute<string>."
                      );
                    }
                  )
                };
                system.OverlayController.ShowMenu(ctx.boundary.Element, menuItems);
              }
            );
            cx.Spacing(2);
            cx.Button(
              new PrefixLabelSuffixSpec("Open modal"),
              static ctx => {
                var system = ctx.LookupComposable<NWSystemExampleComposable>();
                system.modal = system.OverlayController.ShowModal(
                  _ComposeModal,
                  dismissed: (_, _) => system.modal = null
                );
              }
            );
            cx.Spacing(2);
            cx.Button(
              new PrefixLabelSuffixSpec("Notify"),
              static ctx => {
                var notification = new NotificationSpec(
                  "Notifications share the same overlay host.",
                  "HELIX NW"
                );
                ctx.LookupComposable<NWSystemExampleComposable>().OverlayController.ShowNotification(in notification);
              }
            );
          }
          cx.Spacing(2);

          cx.Text("Form");
          cx.Spacing(1);
          cx.Form(ComposeForm, systemState.FormController);
        }
      }

      [Composition] static void _ComposeForm(ref Composition cx, FormController form) {
        using (cx.Flex(Axis.Vertical, cross: Align.Stretch)) {
          cx.FormTextInput(
            "profile.name",
            validators: HomeComposable.RequiredName,
            validationMode: ValidationMode.OnFinishEditing | ValidationMode.OnSubmit,
            decorator: new FormFieldOptions(label: new LabelSpec("Display name"))
          );
          cx.FormErrorText("profile.name");
          cx.Spacing(2);

          cx.FormIntInput(
            "profile.age",
            18,
            validators: HomeComposable.AdultAge,
            validationMode: ValidationMode.OnChange | ValidationMode.OnSubmit,
            decorator: new FormFieldOptions(label: new LabelSpec("Age"))
          );
          cx.Spacing(2);

          cx.FormDropdown(
            "preferences.mode",
            HomeComposable.ExampleMode.Balanced,
            HomeComposable.ModeOptions,
            decorator: new FormFieldOptions(label: new LabelSpec("Mode"))
          );
          cx.Spacing(2);

          cx.FormSlider(
            "preferences.volume",
            0.65f,
            options: HomeComposable.VolumeOptions,
            decorator: new FormFieldOptions(label: new LabelSpec("Volume"))
          );
          cx.Spacing(2);

          cx.FormCheckbox(
            "preferences.enabled",
            true,
            decorator: new FormFieldOptions(
              label: new LabelSpec("Enable feature"),
              anchor: FormFieldAnchor.After
            )
          );
          cx.Spacing(2);

          cx.Text("Dynamic form items");
          cx.Spacing(1);
          cx.FormList("items", 2, ComposeFormListItem);
          cx.Spacing(2);
          using (cx.Flex(Axis.Horizontal, cross: Align.Center)) {
            cx.Button(
              new PrefixLabelSuffixSpec("Add item"),
              static ctx => {
                var form = ctx.LookupComposable<NWSystemExampleComposable>().FormController;
                form.AppendListItem("items");
              }
            );
            cx.Spacing(2);
            cx.Button(
              new PrefixLabelSuffixSpec("Remove item"),
              static ctx => {
                var form = ctx.LookupComposable<NWSystemExampleComposable>().FormController;
                var count = form.GetListCount("items");
                if (count > 0) form.RemoveListItem("items", count - 1);
              }
            );
            cx.Spacing(2);
            cx.Button(
              new PrefixLabelSuffixSpec("Submit form"),
              static ctx => {
                var system = ctx.LookupComposable<NWSystemExampleComposable>();
                var result = system.FormController.Submit();
                var notification = new NotificationSpec(
                  result.valid ? "Form submitted successfully." : "Fix the validation errors first.",
                  result.valid ? "Valid form" : "Invalid form"
                );
                system.OverlayController.ShowNotification(in notification);
              }
            );
            cx.Spacing(2);
            cx.Button(
              new PrefixLabelSuffixSpec("Reset form"),
              static ctx => {
                var form = ctx.LookupComposable<NWSystemExampleComposable>().FormController;
                form.Reset();
              }
            );
          }
        }
      }

      [Composition] static void _ComposeFormListItem(ref Composition cx, int index) {
        using (cx.Flex(Axis.Horizontal, cross: Align.Center)) {
          cx.FormTextInput(
            "label",
            "Item",
            decorator: new FormFieldOptions(label: new LabelSpec("Label"))
          );
          cx.Spacing(2);
          cx.FormIntInput(
            "amount",
            1,
            decorator: new FormFieldOptions(label: new LabelSpec("Amount"))
          );
        }
      }

      private static readonly NavigationRoute<string> _detailsRoute = new("details", _ComposeDetails);

      [Composition] static void _ComposeDetails(ref Composition cx, string message) {
        using (cx.Flex(Axis.Vertical, main: Justify.Center, cross: Align.Center)) {
          cx.APPLY.Padding(24f);
          cx.Text("Typed navigation route").TextRole(TextRole.TitleLarge);
          cx.Text(message);
          cx.Button(
            new PrefixLabelSuffixSpec("Back"),
            static ctx => {
              ctx.LookupComposable<NWSystemExampleComposable>().NavigationController.Pop();
            }
          );
        }
      }

      [Composition] static void _ComposeModal(ref Composition cx, OverlayHandle handle) {
        cx.OverlayPanel(ComposeModalBody);
      }

      [Composition] static void _ComposeModalBody(ref Composition cx) {
        using (cx.Flex(Axis.Vertical, cross: Align.Stretch)) {
          cx.Text("Modal overlay").TextRole(TextRole.TitleLarge);
          cx.Text("This modal is composed entirely from code.");
          cx.Button(
            new PrefixLabelSuffixSpec("Close"),
            static (ctx) => {
              var modal = ctx.LookupComposable<HomeComposable>().modal;
              modal.Dismiss();
            }
          );
        }
      }
    }
  }
}