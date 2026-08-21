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

    private static readonly HXFormFieldStyle ProfileNameFieldStyle = new(
      decorators: new FormFieldDecorators(
        label: ComposeProfileNameLabel,
        description: ComposeProfileNameDescription,
        prefix: ComposeProfileNamePrefix
      )
    );

    private static readonly HXFormFieldStyle UpdatesFieldStyle = new(
      arrangement: DecoratorArrangement.Inline,
      decorators: new FormFieldDecorators(
        label: ComposeUpdatesLabel,
        description: ComposeUpdatesDescription
      )
    );

    private static readonly HXFormFieldStyle VolumeFieldStyle = new(
      decorators: new FormFieldDecorators(
        label: ComposeVolumeLabel,
        suffix: ComposeVolumeSuffix
      )
    );

    private void ComposeFormExample(ref Composition cx) {
      cx.Text("Compose form context", TextRole.TitleMedium);
      cx.Spacing(1);

      using (cx.ProvideForm(_exampleForm)) {
        cx.SubscribeTo(_exampleForm);
        using (cx.Group(Axis.Vertical, cross: Align.Stretch)) {
          if (cx.CursorDirty) cx.CURSOR.Size(BoxConstraints.Only(min: new StyleLength2(320f, 0f)));
          cx.FormField(
            "profile.name",
            ComposeProfileNameField,
            style: ProfileNameFieldStyle,
            validators: ProfileNameValidators,
            validationMode: ValidationMode.OnDirty | ValidationMode.OnSubmit,
            initialValue: "Ada"
          );
          cx.Spacing(1);
          cx.FormField(
            "receiveUpdates",
            ComposeUpdatesField,
            style: UpdatesFieldStyle,
            initialValue: true
          );
          cx.Spacing(1);
          cx.FormField(
            "volume",
            ComposeVolumeField,
            style: VolumeFieldStyle,
            initialValue: 0.65f
          );
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
    }

    private static void ComposeProfileNameField(ref Composition cx) {
      var field = cx.Lookup<HXFormField>();
      cx.TextField(
        value: new TextEditingValue(field.GetValue("")),
        onChanged: static (context, value) => context.Lookup<HXFormField>()?.SetUserValue(value.text),
        onEditingEnded: static (context, _, _) => context.Lookup<HXFormField>()?.MarkFinishedEditing()
      );
    }

    private static void ComposeUpdatesField(ref Composition cx) {
      var field = cx.Lookup<HXFormField>();
      cx.Checkbox(
        field.GetValue(false),
        onChanged: static (context, value) => context.Lookup<HXFormField>()?.SetUserValue(value)
      );
    }

    private static void ComposeVolumeField(ref Composition cx) {
      var field = cx.Lookup<HXFormField>();
      cx.Slider(
        field.GetValue(0.65f),
        options: VolumeOptions,
        onChanged: static (context, value) => context.Lookup<HXFormField>()?.SetUserValue(value)
      );
    }

    private static void ComposeProfileNameLabel(ref Composition cx) =>
      HXDecorator.Label(ref cx, new LabelSpec("Profile name"));

    private static void ComposeProfileNamePrefix(ref Composition cx) =>
      HXDecorator.Label(ref cx, new LabelSpec("User"));

    private static void ComposeProfileNameDescription(ref Composition cx) {
      var errors = cx.Lookup<HXFormField>()?.Data?.errors;
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

    private static void ComposeVolumeSuffix(ref Composition cx) {
      var value = cx.Lookup<HXFormField>()?.GetValue(0.65f) ?? 0.65f;
      HXDecorator.Label(ref cx, new LabelSpec($"{value:P0}"));
    }
  }
}
