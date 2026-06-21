using NUnit.Framework;
using System.Collections.Generic;
using HELIX.Widgets.Elements;
using HELIX.Widgets.Forms;
using HELIX.Widgets.Universal;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Tests {
  public class FormSystemTest : WidgetTestFixture {
    [Test]
    public void FormFieldRegistersWithContextPathAndUpdatesEnabledMetadata() {
      var probeKey = new GlobalKey("form-probe");
      var fieldKey = new GlobalKey("name-field");
      var probe = new FormProbe(fieldKey, key: probeKey);

      PumpWidget(probe);

      var state = StateOf<FormProbe, FormProbeState>(probeKey);
      Assert.That(fieldKey.Target, Is.Not.Null);
      Assert.That(state.BuildCount, Is.GreaterThanOrEqualTo(1));
      Assert.That(state.Form.HasValue("profile.name"), Is.True);
      Assert.That(state.Form.GetValue<string>("profile.name"), Is.EqualTo("Ada"));
      Assert.That(state.Form.GetFieldData("profile.name")?.HasFlag(FieldFlags.Disabled), Is.False);

      state.SetNameFieldEnabled(false);
      Pump();

      Assert.That(state.Form.GetFieldData("profile.name")?.HasFlag(FieldFlags.Disabled), Is.True);
      Assert.That(state.LastFieldStatus, Is.EqualTo("Disabled / OnSubmit"));

      state.SetNameFieldEnabled(true);
      Pump();

      Assert.That(state.Form.GetFieldData("profile.name")?.HasFlag(FieldFlags.Disabled), Is.False);
      Assert.That(state.LastFieldStatus, Is.EqualTo("clean / OnSubmit"));
    }

    [Test]
    public void SmokeWidgetRegistersNestedConditionalAndListFields() {
      var keys = new FormSmokeKeys();
      var smokeKey = new GlobalKey("form-smoke");

      PumpWidget(new FormSmokeWidget(keys, key: smokeKey));

      var state = StateOf<FormSmokeWidget, FormSmokeWidgetState>(smokeKey);
      Assert.That(keys.ProfileName.Target, Is.Not.Null);
      Assert.That(keys.ShippingCity.Target, Is.Not.Null);
      Assert.That(keys.Contact0Name.Target, Is.Not.Null);
      Assert.That(keys.Contact1Email.Target, Is.Not.Null);
      Assert.That(state.Form.GetFieldPaths(), Is.EquivalentTo(new[] {
        "contacts",
        "contacts.0.email",
        "contacts.0.name",
        "contacts.1.email",
        "contacts.1.name",
        "newsletter",
        "newsletterTopic",
        "profile.email",
        "profile.name",
        "shipping.address.city",
        "shipping.address.postalCode"
      }));
      Assert.That(state.Form.GetListCount("contacts"), Is.EqualTo(2));
      AssertFieldDisabled(state.Form, "newsletterTopic", true);
      AssertFieldDisabled(state.Form, "shipping.address.city", true);
      AssertFieldDisabled(state.Form, "shipping.address.postalCode", true);

      var submit = state.Form.Submit();
      Assert.That(submit.valid, Is.True);
      Assert.That(submit.data.ContainsKey("profile.name"), Is.True);
      Assert.That(submit.data.ContainsKey("contacts.1.email"), Is.True);
      Assert.That(submit.data.ContainsKey("newsletterTopic"), Is.False);
      Assert.That(submit.data.ContainsKey("shipping.address.city"), Is.False);

      state.SetNewsletterEnabled(true);
      Pump();

      AssertFieldDisabled(state.Form, "newsletterTopic", false);
      AssertFieldDisabled(state.Form, "shipping.address.city", false);
      Assert.That(state.Form.ValidateAll(), Is.False);
      Assert.That(state.Form.GetErrorMap().Keys, Is.EquivalentTo(new[] {
        "newsletterTopic",
        "shipping.address.city",
        "shipping.address.postalCode"
      }));
    }

    [Test]
    public void FormTextFieldEnabledStateUpdatesWithoutParentFormObserver() {
      var fieldKey = new GlobalKey("standalone-form-field");
      var probeKey = new GlobalKey("standalone-form");

      PumpWidget(new StandaloneFormFieldProbe(fieldKey, key: probeKey));

      var state = StateOf<StandaloneFormFieldProbe, StandaloneFormFieldProbeState>(probeKey);
      var input = TextInputOf(fieldKey);
      Assert.That(input.enabledSelf, Is.True);

      state.Form.SetFieldEnabled("name", false);
      Pump();

      Assert.That(state.BuildCount, Is.EqualTo(1));
      Assert.That(TextInputOf(fieldKey).enabledSelf, Is.False);

      state.Form.SetFieldEnabled("name", true);
      Pump();

      Assert.That(state.BuildCount, Is.EqualTo(1));
      Assert.That(TextInputOf(fieldKey).enabledSelf, Is.True);
    }

    [Test]
    public void SilentValidationDoesNotPersistErrorsOrNotifyObservers() {
      var controller = new FormController();
      var notifications = 0;
      using var observer = controller.AddObserver(() => notifications++);
      var field = new RecordingFormField();

      controller.RegisterField(
        "name",
        field,
        new[] { FormValidators.Required("Name is required") },
        ValidationMode.OnSubmit,
        string.Empty
      );

      var notificationsAfterRegister = notifications;
      var result = controller.ValidateFieldSilently("name");

      Assert.That(result.valid, Is.False);
      Assert.That(result.errors, Contains.Key("name"));
      Assert.That(result.errors["name"], Is.EqualTo(new[] { "Name is required" }));
      Assert.That(controller.GetErrors("name"), Is.Empty);
      Assert.That(controller.GetFieldData("name")?.HasFlag(FieldFlags.Error), Is.False);
      Assert.That(notifications, Is.EqualTo(notificationsAfterRegister));
      Assert.That(field.ChangeCount, Is.EqualTo(1));
    }

    [Test]
    public void SilentPathAndAllValidationDoNotPersistErrorsOrNotifyObservers() {
      var controller = new FormController();
      var notifications = 0;
      using var observer = controller.AddObserver(() => notifications++);

      controller.RegisterField(
        "profile.name",
        new RecordingFormField(),
        new[] { FormValidators.Required("Name is required") },
        ValidationMode.OnSubmit,
        string.Empty
      );
      controller.RegisterField(
        "profile.email",
        new RecordingFormField(),
        new[] { FormValidators.Required("Email is required") },
        ValidationMode.OnSubmit,
        string.Empty
      );

      var pathResult = controller.ValidatePathSilently("profile");
      var allResult = controller.ValidateAllSilently();

      Assert.That(pathResult.valid, Is.False);
      Assert.That(pathResult.errors.Keys, Is.EquivalentTo(new[] { "profile.email", "profile.name" }));
      Assert.That(allResult.valid, Is.False);
      Assert.That(allResult.errors.Keys, Is.EquivalentTo(new[] { "profile.email", "profile.name" }));
      Assert.That(controller.GetErrorMap(), Is.Empty);
      Assert.That(controller.GetFieldData("profile.name")?.HasFlag(FieldFlags.Error), Is.False);
      Assert.That(controller.GetFieldData("profile.email")?.HasFlag(FieldFlags.Error), Is.False);
      Assert.That(notifications, Is.EqualTo(0));
    }

    [Test]
    public void DisabledFieldsClearErrorsAndAreExcludedFromValidationSubmitAndDumpTree() {
      var controller = new FormController();
      controller.RegisterField(
        "profile.name",
        new RecordingFormField(),
        initialValue: "Ada"
      );
      controller.RegisterField(
        "profile.email",
        new RecordingFormField(),
        new[] { FormValidators.Required("Email is required") },
        ValidationMode.OnSubmit,
        string.Empty
      );

      Assert.That(controller.ValidateField("profile.email"), Is.False);
      Assert.That(controller.GetFieldData("profile.email")?.HasFlag(FieldFlags.Error), Is.True);

      controller.SetFieldEnabled("profile.email", false);

      Assert.That(controller.GetErrors("profile.email"), Is.Empty);
      Assert.That(controller.GetFieldData("profile.email")?.HasFlag(FieldFlags.Disabled), Is.True);
      Assert.That(controller.GetFieldData("profile.email")?.HasFlag(FieldFlags.Error), Is.False);
      Assert.That(controller.ValidateAll(), Is.True);

      var submit = controller.Submit();
      Assert.That(submit.valid, Is.True);
      Assert.That(submit.data.ContainsKey("profile.name"), Is.True);
      Assert.That(submit.data.ContainsKey("profile.email"), Is.False);

      var tree = controller.DumpTree();
      var profile = (Dictionary<string, object>)tree["profile"];
      Assert.That(profile.ContainsKey("name"), Is.True);
      Assert.That(profile.ContainsKey("email"), Is.False);

      var debugTree = controller.DumpTree(includeDisabledData: true);
      var debugProfile = (Dictionary<string, object>)debugTree["profile"];
      Assert.That(debugProfile.ContainsKey("email"), Is.True);
    }

    [Test]
    public void StaleFieldsAreExcludedFromSubmitAndDumpTreeUnlessIncludedForDebugging() {
      var controller = new FormController();
      var field = new RecordingFormField();

      controller.RegisterField("profile.legacy", field, initialValue: "old");
      controller.UnregisterField("profile.legacy", field);

      Assert.That(controller.GetFieldData("profile.legacy")?.HasFlag(FieldFlags.Stale), Is.True);

      var submit = controller.Submit();
      Assert.That(submit.valid, Is.True);
      Assert.That(submit.data.ContainsKey("profile.legacy"), Is.False);
      Assert.That(controller.DumpTree().ContainsKey("profile"), Is.False);

      var debugTree = controller.DumpTree(includeStaleData: true);
      var debugProfile = (Dictionary<string, object>)debugTree["profile"];
      Assert.That(debugProfile["legacy"], Is.EqualTo("old"));
    }

    [Test]
    public void MovingListItemsRewritesValuesAndFieldMetadataWithoutMarkingChildrenStale() {
      var controller = new FormController();
      controller.Reset(new Dictionary<string, object> {
        ["contacts.0.name"] = "Ada",
        ["contacts.1.name"] = "Grace",
        ["contacts.2.name"] = "Linus"
      }, false);

      controller.RegisterListField("contacts", new RecordingFormField());
      controller.RegisterField("contacts.0.name", new RecordingFormField());
      controller.RegisterField("contacts.1.name", new RecordingFormField());
      controller.RegisterField("contacts.2.name", new RecordingFormField());

      controller.MoveListItem("contacts", 0, 2);

      Assert.That(controller.GetValue<string>("contacts.0.name"), Is.EqualTo("Grace"));
      Assert.That(controller.GetValue<string>("contacts.1.name"), Is.EqualTo("Linus"));
      Assert.That(controller.GetValue<string>("contacts.2.name"), Is.EqualTo("Ada"));
      Assert.That(controller.GetFieldPaths("contacts"), Is.EquivalentTo(new[] {
        "contacts",
        "contacts.0.name",
        "contacts.1.name",
        "contacts.2.name"
      }));
      Assert.That(controller.GetFieldData("contacts.0.name")?.HasFlag(FieldFlags.Stale), Is.False);
      Assert.That(controller.GetFieldData("contacts.1.name")?.HasFlag(FieldFlags.Stale), Is.False);
      Assert.That(controller.GetFieldData("contacts.2.name")?.HasFlag(FieldFlags.Stale), Is.False);
      Assert.That(controller.GetFieldData("contacts")?.HasFlag(FieldFlags.Dirty), Is.True);
      Assert.That(controller.GetFieldData("contacts.0.name")?.HasFlag(FieldFlags.Dirty), Is.False);
      Assert.That(controller.GetFieldData("contacts.1.name")?.HasFlag(FieldFlags.Dirty), Is.False);
      Assert.That(controller.GetFieldData("contacts.2.name")?.HasFlag(FieldFlags.Dirty), Is.False);
    }

    private static void AssertFieldDisabled(FormController form, string path, bool disabled) {
      Assert.That(form.GetFieldData(path), Is.Not.Null, path);
      Assert.That(form.GetFieldData(path).HasFlag(FieldFlags.Disabled), Is.EqualTo(disabled), path);
    }

    private static GenericTextInput TextInputOf(GlobalKey key) {
      Assert.That(key.Target, Is.Not.Null);
      var input = key.Target.Element.Q<GenericTextInput>();
      Assert.That(input, Is.Not.Null);
      return input;
    }
  }

  public sealed class FormProbe : StatefulWidget<FormProbe> {
    public readonly GlobalKey fieldKey;

    public FormProbe(GlobalKey fieldKey, Key key = default) : base(key) {
      this.fieldKey = fieldKey;
    }

    public override State<FormProbe> CreateState() {
      return new FormProbeState();
    }
  }

  public sealed class FormProbeState : State<FormProbe> {
    public FormController Form { get; private set; }
    public int BuildCount { get; private set; }
    public string LastFieldStatus { get; private set; }

    public override void InitState() {
      Form = AddDisposable(new FormController());
      Form.Reset(new System.Collections.Generic.Dictionary<string, object> {
        ["profile.name"] = "Ada"
      }, false);
      AddDisposable(Form.AddObserver(SetState));
    }

    public override Widget Build(BuildContext context) {
      BuildCount++;
      LastFieldStatus = FieldStatus("profile.name");

      return new HForm(Form) {
        new HFormScope("profile") {
          new HColumn {
            new HFormTextField(
              "name",
              validators: new[] { FormValidators.Required() },
              validationMode: ValidationMode.OnSubmit,
              initialValue: "",
              key: widget.fieldKey
            )
          }
        }
      };
    }

    public void SetNameFieldEnabled(bool enabled) {
      Form.SetFieldEnabled("profile.name", enabled);
    }

    private string FieldStatus(string path) {
      var data = Form.GetFieldData(path);
      if (data == null) return "unregistered";

      var flags = data.flags == FieldFlags.None
        ? "clean"
        : data.flags.ToString();
      var mode = data.validationMode == ValidationMode.None
        ? "no validation"
        : data.validationMode.ToString();
      return $"{flags} / {mode}";
    }
  }

  public sealed class RecordingFormField : IFormField {
    public int ChangeCount { get; private set; }
    public string LastPath { get; private set; }

    public void OnFormFieldChanged(FormController form, string path) {
      ChangeCount++;
      LastPath = path;
    }
  }

  public sealed class StandaloneFormFieldProbe : StatefulWidget<StandaloneFormFieldProbe> {
    public readonly GlobalKey fieldKey;

    public StandaloneFormFieldProbe(GlobalKey fieldKey, Key key = default) : base(key) {
      this.fieldKey = fieldKey;
    }

    public override State<StandaloneFormFieldProbe> CreateState() {
      return new StandaloneFormFieldProbeState();
    }
  }

  public sealed class StandaloneFormFieldProbeState : State<StandaloneFormFieldProbe> {
    public FormController Form { get; private set; }
    public int BuildCount { get; private set; }

    public override void InitState() {
      Form = AddDisposable(new FormController());
      Form.Reset(new Dictionary<string, object> {
        ["name"] = "Ada"
      }, false);
    }

    public override Widget Build(BuildContext context) {
      BuildCount++;
      return new HForm(Form) {
        new HFormTextField(
          "name",
          validators: new[] { FormValidators.Required() },
          validationMode: ValidationMode.OnSubmit,
          key: widget.fieldKey
        )
      };
    }
  }

  public sealed class FormSmokeKeys {
    public readonly GlobalKey ProfileName = new GlobalKey("smoke-profile-name");
    public readonly GlobalKey ShippingCity = new GlobalKey("smoke-shipping-city");
    public readonly GlobalKey Contact0Name = new GlobalKey("smoke-contact-0-name");
    public readonly GlobalKey Contact1Email = new GlobalKey("smoke-contact-1-email");
  }

  public sealed class FormSmokeWidget : StatefulWidget<FormSmokeWidget> {
    public readonly FormSmokeKeys keys;

    public FormSmokeWidget(FormSmokeKeys keys = null, Key key = default) : base(key) {
      this.keys = keys ?? new FormSmokeKeys();
    }

    public override State<FormSmokeWidget> CreateState() {
      return new FormSmokeWidgetState();
    }
  }

  public sealed class FormSmokeWidgetState : State<FormSmokeWidget> {
    public FormController Form { get; private set; }

    public override void InitState() {
      Form = AddDisposable(new FormController());
      Form.Reset(new Dictionary<string, object> {
        ["profile.name"] = "Ada",
        ["profile.email"] = "ada@example.test",
        ["newsletter"] = "no",
        ["newsletterTopic"] = "",
        ["shipping.address.city"] = "",
        ["shipping.address.postalCode"] = "",
        ["contacts.0.name"] = "Grace",
        ["contacts.0.email"] = "grace@example.test",
        ["contacts.1.name"] = "Linus",
        ["contacts.1.email"] = "linus@example.test"
      }, false);
      SetNewsletterEnabled(false, false);
      //AddDisposable(Form.AddObserver(SetState));
    }

    public override Widget Build(BuildContext context) {
      return new HForm(Form) {
        new HColumn(gap: 8, crossAxisAlign: Align.Stretch) {
          new HText("Form system smoke test"),
          ProfileFields(),
          NewsletterFields(),
          ShippingFields(),
          ContactFields()
        }
      };
    }

    public void SetNewsletterEnabled(bool enabled, bool notify = true) {
      using (Form.BeginUpdate()) {
        Form.SetValue(
          "newsletter",
          enabled ? "yes" : "no",
          notify ? FormChangeReason.User : FormChangeReason.Programmatic
        );
        SetNewsletterDependentFieldsEnabled(enabled, notify);
      }
    }

    private Widget ProfileFields() {
      return new HFormScope("profile") {
        new HColumn(gap: 4, crossAxisAlign: Align.Stretch) {
          new HText("Profile"),
          RequiredTextField("name", widget.keys.ProfileName),
          RequiredTextField("email")
        }
      };
    }

    private Widget NewsletterFields() {
      return new HColumn(gap: 4, crossAxisAlign: Align.Stretch) {
        new HText("Newsletter"),
        new HRow(gap: 4, crossAxisAlign: Align.Stretch) {
          new HButton(
            child: new HText("Use yes"),
            onClick: () => SetNewsletterEnabled(true)
          ),
          new HButton(
            child: new HText("Use no"),
            onClick: () => SetNewsletterEnabled(false)
          )
        },
        NewsletterTextField(),
        RequiredTextField("newsletterTopic"),
        FieldStatus("newsletterTopic"),
        FieldStatus("shipping.address.city"),
        FieldStatus("shipping.address.postalCode")
      };
    }

    private Widget ShippingFields() {
      return new HFormScope("shipping.address") {
        new HColumn(gap: 4, crossAxisAlign: Align.Stretch) {
          new HText("Shipping address"),
          RequiredTextField("city", widget.keys.ShippingCity),
          RequiredTextField("postalCode")
        }
      };
    }

    private Widget ContactFields() {
      return new HFormListField(
        "contacts",
        itemBuilder: ContactItemFields,
        containerBuilder: (_, items) => new HColumn(gap: 4, crossAxisAlign: Align.Stretch, children: items),
        validators: new[] {
          FormValidators.Func((form, path, _) => form.GetListCount(path) > 0 ? null : "Add at least one contact")
        },
        validationMode: ValidationMode.OnSubmit
      );
    }

    private Widget ContactItemFields(BuildContext context, int index) {
      return new HColumn(gap: 4, crossAxisAlign: Align.Stretch) {
        new HText($"Contact {index + 1}"),
        RequiredTextField("name", index == 0 ? widget.keys.Contact0Name : Key.None),
        RequiredTextField("email", index == 1 ? widget.keys.Contact1Email : Key.None)
      };
    }

    private Widget NewsletterTextField() {
      return new HFormTextField(
        "newsletter",
        validators: new[] { FormValidators.Required() },
        validationMode: ValidationMode.OnSubmit,
        initialValue: "no",
        onChanged: value => SetNewsletterDependentFieldsEnabled(IsNewsletterEnabled(value))
      );
    }

    private static Widget RequiredTextField(string path, Key key = default) {
      return new HFormTextField(
        path,
        validators: new[] { FormValidators.Required() },
        validationMode: ValidationMode.OnSubmit,
        initialValue: "",
        key: key
      );
    }

    private void SetNewsletterDependentFieldsEnabled(bool enabled, bool notify = true) {
      Form.SetFieldEnabled("newsletterTopic", enabled, notify: false);
      Form.SetFieldEnabled("shipping.address", enabled, true, notify);
    }

    private static bool IsNewsletterEnabled(string value) {
      return string.Equals(value, "yes", System.StringComparison.OrdinalIgnoreCase)
             || string.Equals(value, "true", System.StringComparison.OrdinalIgnoreCase);
    }

    private Widget FieldStatus(string path) {
      var data = Form.GetFieldData(path);
      if (data == null) return new HText($"{path}: unregistered");

      var flags = data.flags == FieldFlags.None ? "clean" : data.flags.ToString();
      var errors = Form.GetErrors(path);
      var suffix = errors.Count > 0 ? $" errors: {string.Join(", ", errors)}" : string.Empty;
      return new HText($"{path}: {flags}{suffix}");
    }
  }
}
