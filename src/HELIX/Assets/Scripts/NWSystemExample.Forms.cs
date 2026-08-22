using System.Collections.Generic;
using HELIX.Compose;
using HELIX.Compose.Forms;
using HELIX.Theming;
using HELIX.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Examples {
  public partial class HomeComposable {
    private static readonly IFormValidator[] ProfileNameValidators = {
      FormValidators.Required("A display name is required.")
    };

    private static readonly FormFieldDecorators ProfileNameDecorators = new(
      label: ComposeProfileNameLabel,
      description: ComposeProfileNameDescription,
      prefix: ComposeProfileNamePrefix
    );

    private static readonly FormFieldDecorators UpdatesDecorators = new(
      label: ComposeUpdatesLabel,
      description: ComposeUpdatesDescription
    );

    private static readonly FormFieldDecorators VolumeDecorators = new(
      label: ComposeVolumeLabel
    );

    private static readonly FormFieldDecorators AgeDecorators = new(label: ComposeAgeLabel);
    private static readonly FormFieldDecorators ModeDecorators = new(label: ComposeModeLabel);

    private static readonly IReadOnlyList<DropdownOption<object>> FormModeValues = new DropdownOption<object>[] {
      new DropdownOption<object>(ExampleMode.Balanced, "Balanced"),
      new DropdownOption<object>(ExampleMode.Performance, "Performance"),
      new DropdownOption<object>(ExampleMode.Quality, "Quality")
    };

    private void ComposeFormExample(ref Composition cx) {
      cx.Text("Compose form context", TextRole.TitleMedium);
      cx.Spacing(1);

      cx.SubscribeTo(_exampleForm);
      using (cx.Group(Axis.Vertical, cross: Align.Stretch)) {
          if (cx.CursorDirty) cx.CURSOR.Size(BoxConstraints.Only(min: new StyleLength2(320f, 0f)));
          cx.Spec(new StringFormField(new FormField<string>(
            "profile.name",
            initialValue: "Ada",
            decorators: ProfileNameDecorators,
            validators: ProfileNameValidators,
            validationMode: ValidationMode.OnDirty | ValidationMode.OnSubmit
          ), placeholder: "Display name"));
          cx.Spacing(1);
          cx.Spec(new IntFormField(
            new FormField<int>("profile.age", initialValue: 32, decorators: AgeDecorators),
            min: 0,
            max: 130,
            step: 1
          ));
          cx.Spacing(1);
          cx.Spec(new FloatFormField(
            new FormField<float>("volume", initialValue: 0.65f, decorators: VolumeDecorators),
            min: 0f,
            max: 1f,
            step: 0.05f,
            formatting: new NumericFormatSettings("0", scale: 100f),
            prefix: ComposeApproximatePrefix,
            suffix: ComposePercentSuffix
          ));
          cx.Spacing(1);
          cx.Spec(new BoolFormField(new FormField<bool>(
            "receiveUpdates", initialValue: true, decorators: UpdatesDecorators
          )));
          cx.Spacing(1);
          cx.Spec(new EnumFormField(
            new FormField<object>(
              "mode", initialValue: new HXOptional<object>(ExampleMode.Balanced), decorators: ModeDecorators
            ),
            FormModeValues
          ));
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
                Debug.Log(
                  $"Compose form submit ({(result.valid ? "valid" : "invalid")}): {form.FormatData(result.data)}"
                );
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

    private static void ComposeProfileNameLabel(ref Composition cx) =>
      HXDecorator.Label(ref cx, new LabelSpec("Profile name"));

    private static void ComposeProfileNamePrefix(ref Composition cx) =>
      HXDecorator.Label(ref cx, new LabelSpec("User"));

    private static void ComposeProfileNameDescription(ref Composition cx) {
      var errors = cx.Lookup<HXFormField>()?.FieldData?.errors;
      HXDecorator.Label(
        ref cx,
        new LabelSpec(errors is { Count: > 0 } ? errors[0] : "Required; validated after editing or submit.")
      );
    }

    private static void ComposeUpdatesLabel(ref Composition cx) =>
      HXDecorator.Label(ref cx, new LabelSpec("Receive product updates"));

    private static void ComposeUpdatesDescription(ref Composition cx) =>
      HXDecorator.Label(ref cx, new LabelSpec("A boolean field rendered with Checkbox."));

    private static void ComposeVolumeLabel(ref Composition cx) =>
      HXDecorator.Label(ref cx, new LabelSpec("Notification volume"));

    private static void ComposePercentSuffix(ref Composition cx) => cx.Text("%");
    private static void ComposeApproximatePrefix(ref Composition cx) => cx.Text("≈");

    private static void ComposeAgeLabel(ref Composition cx) =>
      HXDecorator.Label(ref cx, new LabelSpec("Age"));

    private static void ComposeModeLabel(ref Composition cx) =>
      HXDecorator.Label(ref cx, new LabelSpec("Preferred mode"));

  }
}
