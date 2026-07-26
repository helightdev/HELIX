using System;
using HELIX.Coloring;
using HELIX.Extensions;
using HELIX.NW;
using HELIX.NW.Forms;
using HELIX.NW.Navigation;
using HELIX.NW.Overlays;
using HELIX.Types;
using HELIX.Widgets.Universal;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

namespace HELIX.Examples {
  // public static partial class NWSystemsExampleCompositions {
  //   [CompositionBoundary]
  //   public static partial void NWSystemExample(ref this Composition cx);
  //
  //   public partial class NWSystemExampleState {
  //     public FormController FormController { get; private set; }
  //     public NavigationController NavigationController { get; private set; }
  //     public OverlayController OverlayController { get; private set; }
  //
  //     protected override void OnAttach() {
  //       base.OnAttach();
  //       FormController = new FormController();
  //       NavigationController = new NavigationController();
  //       OverlayController = new OverlayController();
  //     }
  //
  //     protected override void OnDetach() {
  //       base.OnDetach();
  //       NavigationController.Dispose();
  //       OverlayController.Dispose();
  //     }
  //
  //     protected override void OnRecompose(ref Composition cx) {
  //       var theme = cx.ReadContextOrDefault(ThemeData.Context, BuiltinThemes.DefaultDark);
  //       cx.APPLY
  //         .BackgroundColor(theme.GetColor(ColorRoles.Surface))
  //         .TextColor(theme.GetColor(ColorRoles.OnSurface));
  //       cx.OverlayHost(ComposeApplication, OverlayController);
  //     }
  //   }
  //
  //   [Composition] static void _ComposeApplication(ref Composition cx, OverlayController overlays) {
  //     cx.NavigationHost(_navigation);
  //   }
  //
  //   private void ComposeRoot(ref Composition cx) {
  //     var theme = cx.ReadContextOrDefault(ThemeData.Context, BuiltinThemes.DefaultDark);
  //     cx.APPLY
  //       .BackgroundColor(theme.GetColor(ColorRoles.Surface))
  //       .TextColor(theme.GetColor(ColorRoles.OnSurface));
  //     cx.OverlayHost(_view.composeApplication, _overlays);
  //   }
  //
  //   private void ComposeHome(ref Composition cx, NavigationEntry entry) {
  //     using (cx.Flex(Axis.Vertical, cross: Align.Stretch)) {
  //       cx.APPLY.Padding(16f);
  //       cx.Text("HELIX NW system example").TextRole(TextRole.TitleLarge);
  //       cx.Spacing(2);
  //       cx.Text("Controlled inputs");
  //       cx.Spacing(2);
  //
  //       var font = Resources.Load<FontAsset>("helix/fa/FontAwesome7FreeSolid");
  //
  //       using (cx.Decorator(out var slots)) {
  //         using (slots.Prefix())
  //           CommonDecoratorElement.ComposeScopedLabel(
  //             ref cx,
  //             new LabelSpec("Prefix", new IconRef(FaSolidIcons.User.ToString(), font).Composable())
  //           );
  //         using (slots.Element()) {
  //           ref var reference = ref cx.Text("Element Data");
  //           Debug.Log($"Element Ref: {reference.element}; {reference.composable} -> {reference.composable.Element}");
  //           reference.TextRole(TextRole.BodyMedium);
  //           Debug.Log($"Element Ref: {reference.element}; {reference.composable} -> {reference.composable.Element}");
  //         }
  //         using (slots.Suffix()) CommonDecoratorElement.ComposeScopedLabel(ref cx, new LabelSpec("Suffix"));
  //         using (slots.Label()) CommonDecoratorElement.ComposeScopedLabel(ref cx, new LabelSpec("Label"));
  //         using (slots.Before()) CommonDecoratorElement.ComposeScopedLabel(ref cx, new LabelSpec("Before"));
  //         using (slots.After()) CommonDecoratorElement.ComposeScopedLabel(ref cx, new LabelSpec("After"));
  //         using (slots.Description()) CommonDecoratorElement.ComposeScopedLabel(ref cx, new LabelSpec("Description"));
  //       }
  //
  //       using (cx.Flex(Axis.Horizontal, cross: Align.FlexStart)) {
  //         using (cx.Flex(Axis.Vertical, cross: Align.Stretch)) {
  //           cx.APPLY.Size(BoxConstraints.Only(min: new StyleLength2(260f, 0f)));
  //           cx.Text("Text");
  //           cx.Spacing(1);
  //           cx.TextInput(_text, _view.textChanged);
  //           cx.Spacing(2);
  //
  //           cx.Text("Integer");
  //           cx.Spacing(1);
  //           cx.IntInput(_integer, _view.integerChanged);
  //           cx.Spacing(2);
  //
  //           cx.Text("Float");
  //           cx.Spacing(1);
  //           cx.FloatInput(
  //             _floatingPoint,
  //             _view.floatChanged,
  //             options: _decimalOptions
  //           );
  //         }
  //         cx.Spacing(2);
  //
  //         using (cx.Flex(Axis.Vertical, cross: Align.Stretch)) {
  //           cx.APPLY.Size(BoxConstraints.Only(min: new StyleLength2(260f, 0f)));
  //           cx.Text("Volume");
  //           cx.Spacing(1);
  //           cx.Slider(_volume, _view.volumeChanged, options: _volumeOptions);
  //           cx.Spacing(2);
  //
  //           cx.Checkbox(
  //             _enabled,
  //             new PrefixLabelSuffixSpec("Enabled"),
  //             _view.enabledChanged
  //           );
  //           cx.Spacing(2);
  //
  //           cx.Text("Rendering mode");
  //           cx.Spacing(1);
  //           cx.Dropdown(_mode, _modeOptions, _view.modeChanged);
  //         }
  //       }
  //       cx.Spacing(2);
  //
  //       using (cx.Flex(Axis.Horizontal, cross: Align.Center)) {
  //         cx.Button(new PrefixLabelSuffixSpec("Open details"), _view.openDetails);
  //         cx.Spacing(2);
  //         cx.Button(new PrefixLabelSuffixSpec("Open menu"), _view.openMenu);
  //         cx.Spacing(2);
  //         cx.Button(new PrefixLabelSuffixSpec("Open modal"), _view.openModal);
  //         cx.Spacing(2);
  //         cx.Button(new PrefixLabelSuffixSpec("Notify"), _view.showNotification);
  //       }
  //       cx.Spacing(2);
  //
  //       cx.Text("Form");
  //       cx.Spacing(1);
  //       cx.Form(_view.composeForm, _form);
  //     }
  //   }
  //
  //   private void ComposeForm(ref Composition cx, FormController form) {
  //     using (cx.Flex(Axis.Vertical, cross: Align.Stretch)) {
  //       cx.FormTextInput(
  //         "profile.name",
  //         validators: _requiredName,
  //         validationMode: ValidationMode.OnFinishEditing | ValidationMode.OnSubmit,
  //         decorator: new FormFieldOptions(label: new LabelSpec("Display name"))
  //       );
  //       cx.FormErrorText("profile.name");
  //       cx.Spacing(2);
  //
  //       cx.FormIntInput(
  //         "profile.age",
  //         18,
  //         validators: _adultAge,
  //         validationMode: ValidationMode.OnChange | ValidationMode.OnSubmit,
  //         decorator: new FormFieldOptions(label: new LabelSpec("Age"))
  //       );
  //       cx.Spacing(2);
  //
  //       cx.FormDropdown(
  //         "preferences.mode",
  //         ExampleMode.Balanced,
  //         _modeOptions,
  //         decorator: new FormFieldOptions(label: new LabelSpec("Mode"))
  //       );
  //       cx.Spacing(2);
  //
  //       cx.FormSlider(
  //         "preferences.volume",
  //         0.65f,
  //         options: _volumeOptions,
  //         decorator: new FormFieldOptions(label: new LabelSpec("Volume"))
  //       );
  //       cx.Spacing(2);
  //
  //       cx.FormCheckbox(
  //         "preferences.enabled",
  //         true,
  //         decorator: new FormFieldOptions(
  //           label: new LabelSpec("Enable feature"),
  //           anchor: FormFieldAnchor.After
  //         )
  //       );
  //       cx.Spacing(2);
  //
  //       cx.Text("Dynamic form items");
  //       cx.Spacing(1);
  //       cx.FormList("items", 2, _view.composeFormListItem);
  //       cx.Spacing(2);
  //       using (cx.Flex(Axis.Horizontal, cross: Align.Center)) {
  //         cx.Button(new PrefixLabelSuffixSpec("Add item"), _view.appendItem);
  //         cx.Spacing(2);
  //         cx.Button(new PrefixLabelSuffixSpec("Remove item"), _view.removeItem);
  //         cx.Spacing(2);
  //         cx.Button(new PrefixLabelSuffixSpec("Submit form"), _view.submitForm);
  //         cx.Spacing(2);
  //         cx.Button(new PrefixLabelSuffixSpec("Reset form"), _view.resetForm);
  //       }
  //     }
  //   }
  //
  //   private void ComposeFormListItem(ref Composition cx, int index) {
  //     using (cx.Flex(Axis.Horizontal, cross: Align.Center)) {
  //       cx.FormTextInput(
  //         "label",
  //         "Item",
  //         decorator: new FormFieldOptions(label: new LabelSpec("Label"))
  //       );
  //       cx.Spacing(2);
  //       cx.FormIntInput(
  //         "amount",
  //         1,
  //         decorator: new FormFieldOptions(label: new LabelSpec("Amount"))
  //       );
  //     }
  //   }
  //
  //   private void ComposeDetails(ref Composition cx, string message) {
  //     using (cx.Flex(Axis.Vertical, main: Justify.Center, cross: Align.Center)) {
  //       cx.APPLY.Padding(24f);
  //       cx.Text("Typed navigation route").TextRole(TextRole.TitleLarge);
  //       cx.Text(message);
  //       cx.Button(new PrefixLabelSuffixSpec("Back"), _view.goBack);
  //     }
  //   }
  //
  //   private void ComposeModal(ref Composition cx, OverlayHandle handle) {
  //     cx.OverlayPanel(_view.composeModalBody);
  //   }
  //
  //   private void ComposeModalBody(ref Composition cx) {
  //     using (cx.Flex(Axis.Vertical, cross: Align.Stretch)) {
  //       cx.Text("Modal overlay").TextRole(TextRole.TitleLarge);
  //       cx.Text("This modal is composed entirely from code.");
  //       cx.Button(new PrefixLabelSuffixSpec("Close"), _view.closeModal);
  //     }
  //   }
  // }

  /// <summary>
  /// Code-only example for the NW controls, form, navigation, and overlay systems.
  /// Add this element to a UIDocument root from code:
  ///
  /// rootVisualElement.Add(new NWSystemExample());
  /// </summary>
  ///
  [UxmlElement]
  public partial class NWSystemExample : VisualElement {
    private enum ExampleMode : byte { Balanced, Performance, Quality }

    private static readonly DropdownOption<ExampleMode>[] _modeOptions = {
      new(ExampleMode.Balanced, "Balanced"),
      new(ExampleMode.Performance, "Performance"),
      new(ExampleMode.Quality, "Quality")
    };

    private static readonly IFormValidator[] _requiredName = {
      FormValidators.Required("Enter a display name.")
    };

    private static readonly IFormValidator[] _adultAge = {
      FormValidators.Range(18, 120, "Age must be between 18 and 120.")
    };

    private static readonly SliderOptions _volumeOptions = new(0f, 1f, step: 0f, thumbRange: 0.1f);
    private static readonly NumericInputOptions _decimalOptions = new(format: "0.00");

    private readonly struct ViewBindings {
      public readonly Composable<OverlayController> composeApplication;
      public readonly Composable<FormController> composeForm;
      public readonly Composable<int> composeFormListItem;
      public readonly Composable composeModalBody;
      public readonly OverlayComposable composeModal;
      public readonly Action<string, IBoundary> textChanged;
      public readonly Action<int, IBoundary> integerChanged;
      public readonly Action<float, IBoundary> floatChanged;
      public readonly Action<float, IBoundary> volumeChanged;
      public readonly Action<bool, IBoundary> enabledChanged;
      public readonly Action<ExampleMode, IBoundary> modeChanged;
      public readonly Action<IBoundary> openDetails;
      public readonly Action<IBoundary> openMenu;
      public readonly Action<IBoundary> openModal;
      public readonly Action<IBoundary> showNotification;
      public readonly Action<IBoundary> submitForm;
      public readonly Action<IBoundary> resetForm;
      public readonly Action<IBoundary> appendItem;
      public readonly Action<IBoundary> removeItem;
      public readonly Action<IBoundary> goBack;
      public readonly Action<IBoundary> closeModal;
      public readonly Action<OverlayHandle, OverlayDismissReason> modalDismissed;

      public ViewBindings(NWSystemExample owner) {
        composeApplication = owner.ComposeApplication;
        composeForm = owner.ComposeForm;
        composeFormListItem = owner.ComposeFormListItem;
        composeModalBody = owner.ComposeModalBody;
        composeModal = owner.ComposeModal;
        textChanged = owner.OnTextChanged;
        integerChanged = owner.OnIntegerChanged;
        floatChanged = owner.OnFloatChanged;
        volumeChanged = owner.OnVolumeChanged;
        enabledChanged = owner.OnEnabledChanged;
        modeChanged = owner.OnModeChanged;
        openDetails = owner.OpenDetails;
        openMenu = owner.OpenMenu;
        openModal = owner.OpenModal;
        showNotification = owner.ShowNotification;
        submitForm = owner.SubmitForm;
        resetForm = owner.ResetForm;
        appendItem = owner.AppendItem;
        removeItem = owner.RemoveItem;
        goBack = owner.GoBack;
        closeModal = owner.CloseModal;
        modalDismissed = owner.ModalDismissed;
      }
    }

    private readonly CompositionBoundaryNode _root;
    private readonly ViewBindings _view;
    private readonly FormController _form = new();
    private readonly NavigationController _navigation = new();
    private readonly OverlayController _overlays = new();
    private readonly NavigationRoute _homeRoute;
    private readonly NavigationRoute<string> _detailsRoute;
    private readonly MenuItemSpec[] _menuItems;

    private string _text = "Editable text";
    private int _integer = 12;
    private float _floatingPoint = 1.25f;
    private float _volume = 0.65f;
    private bool _enabled = true;
    private ExampleMode _mode = ExampleMode.Balanced;
    private OverlayHandle _modal;

    public NWSystemExample() {
      name = "NWSystemExample";
      _view = new ViewBindings(this);

      _homeRoute = new NavigationRoute("home", ComposeHome);
      _detailsRoute = new NavigationRoute<string>("details", ComposeDetails);
      _navigation.Reset(_homeRoute);

      var nestedItems = new[] {
        new MenuItemSpec("Balanced", SelectBalanced),
        new MenuItemSpec("Performance", SelectPerformance),
        new MenuItemSpec("Quality", SelectQuality)
      };
      _menuItems = new[] {
        new MenuItemSpec("Show notification", ShowMenuNotification),
        new MenuItemSpec("Mode", children: nestedItems),
        MenuItemSpec.Separator(),
        new MenuItemSpec("Open details", OpenDetailsFromMenu)
      };

      _root = new CompositionBoundaryNode {
        name = "NWSystemExampleComposition",
        composable = ComposeRoot
      }.Fill();

      hierarchy.Add(_root);
      RecompositionScope.MarkDirty(_root);

      // schedule.Execute(state => {
      //   RecompositionScope.MarkDirty(_root);
      // }).Every(1);
    }

    private void ComposeRoot(ref Composition cx) {
      var theme = cx.ReadContextOrDefault(ThemeData.Context, BuiltinThemes.DefaultDark);
      cx.APPLY
        .BackgroundColor(theme.GetColor(ColorRoles.Surface))
        .TextColor(theme.GetColor(ColorRoles.OnSurface));
      cx.OverlayHost(_view.composeApplication, _overlays);
    }

    private void ComposeApplication(ref Composition cx, OverlayController overlays) {
      cx.NavigationHost(_navigation);
    }

    private void ComposeHome(ref Composition cx, NavigationEntry entry) {
      using (cx.Flex(Axis.Vertical, cross: Align.Stretch)) {
        cx.APPLY.Padding(16f);
        cx.Text("HELIX NW system example").TextRole(TextRole.TitleLarge);
        cx.Spacing(2);
        cx.Text("Controlled inputs");
        cx.Spacing(2);

        var font = Resources.Load<FontAsset>("helix/fa/FontAwesome7FreeSolid");

        using (cx.Decorator(out var slots)) {
          using (slots.Prefix())
            CommonDecoratorElement.ComposeScopedLabel(
              ref cx,
              new LabelSpec("Prefix", new IconRef(FaSolidIcons.User.ToString(), font).Composable())
            );
          using (slots.Element()) {
            ref var reference = ref cx.Text("Element Data");
            Debug.Log($"Element Ref: {reference.element}; {reference.composable} -> {reference.composable.Element}");
            reference.TextRole(TextRole.BodyMedium);
            Debug.Log($"Element Ref: {reference.element}; {reference.composable} -> {reference.composable.Element}");
          }
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
            cx.TextInput(_text, _view.textChanged);
            cx.Spacing(2);

            cx.Text("Integer");
            cx.Spacing(1);
            cx.IntInput(_integer, _view.integerChanged);
            cx.Spacing(2);

            cx.Text("Float");
            cx.Spacing(1);
            cx.FloatInput(
              _floatingPoint,
              _view.floatChanged,
              options: _decimalOptions
            );
          }
          cx.Spacing(2);

          using (cx.Flex(Axis.Vertical, cross: Align.Stretch)) {
            cx.APPLY.Size(BoxConstraints.Only(min: new StyleLength2(260f, 0f)));
            cx.Text("Volume");
            cx.Spacing(1);
            cx.Slider(_volume, _view.volumeChanged, options: _volumeOptions);
            cx.Spacing(2);

            cx.Checkbox(
              _enabled,
              new PrefixLabelSuffixSpec("Enabled"),
              _view.enabledChanged
            );
            cx.Spacing(2);

            cx.Text("Rendering mode");
            cx.Spacing(1);
            cx.Dropdown(_mode, _modeOptions, _view.modeChanged);
          }
        }
        cx.Spacing(2);

        using (cx.Flex(Axis.Horizontal, cross: Align.Center)) {
          cx.Button(new PrefixLabelSuffixSpec("Open details"), _view.openDetails);
          cx.Spacing(2);
          cx.Button(new PrefixLabelSuffixSpec("Open menu"), _view.openMenu);
          cx.Spacing(2);
          cx.Button(new PrefixLabelSuffixSpec("Open modal"), _view.openModal);
          cx.Spacing(2);
          cx.Button(new PrefixLabelSuffixSpec("Notify"), _view.showNotification);
        }
        cx.Spacing(2);

        cx.Text("Form");
        cx.Spacing(1);
        cx.Form(_view.composeForm, _form);
      }
    }

    private void ComposeForm(ref Composition cx, FormController form) {
      using (cx.Flex(Axis.Vertical, cross: Align.Stretch)) {
        cx.FormTextInput(
          "profile.name",
          validators: _requiredName,
          validationMode: ValidationMode.OnFinishEditing | ValidationMode.OnSubmit,
          decorator: new FormFieldOptions(label: new LabelSpec("Display name"))
        );
        cx.FormErrorText("profile.name");
        cx.Spacing(2);

        cx.FormIntInput(
          "profile.age",
          18,
          validators: _adultAge,
          validationMode: ValidationMode.OnChange | ValidationMode.OnSubmit,
          decorator: new FormFieldOptions(label: new LabelSpec("Age"))
        );
        cx.Spacing(2);

        cx.FormDropdown(
          "preferences.mode",
          ExampleMode.Balanced,
          _modeOptions,
          decorator: new FormFieldOptions(label: new LabelSpec("Mode"))
        );
        cx.Spacing(2);

        cx.FormSlider(
          "preferences.volume",
          0.65f,
          options: _volumeOptions,
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
        cx.FormList("items", 2, _view.composeFormListItem);
        cx.Spacing(2);
        using (cx.Flex(Axis.Horizontal, cross: Align.Center)) {
          cx.Button(new PrefixLabelSuffixSpec("Add item"), _view.appendItem);
          cx.Spacing(2);
          cx.Button(new PrefixLabelSuffixSpec("Remove item"), _view.removeItem);
          cx.Spacing(2);
          cx.Button(new PrefixLabelSuffixSpec("Submit form"), _view.submitForm);
          cx.Spacing(2);
          cx.Button(new PrefixLabelSuffixSpec("Reset form"), _view.resetForm);
        }
      }
    }

    private void ComposeFormListItem(ref Composition cx, int index) {
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

    private void ComposeDetails(ref Composition cx, string message) {
      using (cx.Flex(Axis.Vertical, main: Justify.Center, cross: Align.Center)) {
        cx.APPLY.Padding(24f);
        cx.Text("Typed navigation route").TextRole(TextRole.TitleLarge);
        cx.Text(message);
        cx.Button(new PrefixLabelSuffixSpec("Back"), _view.goBack);
      }
    }

    private void ComposeModal(ref Composition cx, OverlayHandle handle) {
      cx.OverlayPanel(_view.composeModalBody);
    }

    private void ComposeModalBody(ref Composition cx) {
      using (cx.Flex(Axis.Vertical, cross: Align.Stretch)) {
        cx.Text("Modal overlay").TextRole(TextRole.TitleLarge);
        cx.Text("This modal is composed entirely from code.");
        cx.Button(new PrefixLabelSuffixSpec("Close"), _view.closeModal);
      }
    }

    private void OnTextChanged(string value, IBoundary boundary) {
      _text = value;
      _root.MarkDirty();
    }

    private void OnIntegerChanged(int value, IBoundary boundary) {
      _integer = value;
      _root.MarkDirty();
    }

    private void OnFloatChanged(float value, IBoundary boundary) {
      _floatingPoint = value;
      _root.MarkDirty();
    }

    private void OnVolumeChanged(float value, IBoundary boundary) {
      _volume = value;
      _root.MarkDirty();
    }

    private void OnEnabledChanged(bool value, IBoundary boundary) {
      _enabled = value;
      _root.MarkDirty();
    }

    private void OnModeChanged(ExampleMode value, IBoundary boundary) {
      _mode = value;
      _root.MarkDirty();
    }

    private void OpenDetails(IBoundary boundary) {
      _navigation.Push(_detailsRoute, "This value was passed through NavigationRoute<string>.");
    }

    private void OpenMenu(IBoundary boundary) {
      _overlays.ShowMenu(boundary.Element, _menuItems);
    }

    private void OpenModal(IBoundary boundary) {
      _modal?.Dismiss(OverlayDismissReason.Replaced);
      _modal = _overlays.ShowModal(_view.composeModal, dismissed: _view.modalDismissed);
    }

    private void ShowNotification(IBoundary boundary) {
      var notification = new NotificationSpec(
        "Notifications share the same overlay host.",
        "HELIX NW"
      );
      _overlays.ShowNotification(in notification);
    }

    private void SubmitForm(IBoundary boundary) {
      var result = _form.Submit();
      var notification = new NotificationSpec(
        result.valid ? "Form submitted successfully." : "Fix the validation errors first.",
        result.valid ? "Valid form" : "Invalid form"
      );
      _overlays.ShowNotification(in notification);
    }

    private void ResetForm(IBoundary boundary) {
      _form.Reset();
    }

    private void AppendItem(IBoundary boundary) {
      _form.AppendListItem("items");
    }

    private void RemoveItem(IBoundary boundary) {
      var count = _form.GetListCount("items");
      if (count > 0) _form.RemoveListItem("items", count - 1);
    }

    private void GoBack(IBoundary boundary) {
      _navigation.Pop();
    }

    private void CloseModal(IBoundary boundary) {
      _modal?.Dismiss(OverlayDismissReason.Action);
    }

    private void ModalDismissed(OverlayHandle handle, OverlayDismissReason reason) {
      if (ReferenceEquals(_modal, handle)) _modal = null;
    }

    private void ShowMenuNotification() {
      var notification = new NotificationSpec("Opened from a menu action.", "Menu");
      _overlays.ShowNotification(in notification);
    }

    private void SelectBalanced() {
      OnModeChanged(ExampleMode.Balanced, null);
    }

    private void SelectPerformance() {
      OnModeChanged(ExampleMode.Performance, null);
    }

    private void SelectQuality() {
      OnModeChanged(ExampleMode.Quality, null);
    }

    private void OpenDetailsFromMenu() {
      _navigation.Push(_detailsRoute, "Navigation can also be triggered by a menu item.");
    }
  }
}