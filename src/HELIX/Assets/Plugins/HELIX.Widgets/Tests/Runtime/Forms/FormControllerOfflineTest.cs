using System.Collections.Generic;
using HELIX.Widgets.Forms;
using NUnit.Framework;

namespace HELIX.Widgets.Tests.Forms {
  public sealed class FormControllerOfflineTest {
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

    private sealed class RecordingFormField : IFormField {
      public int ChangeCount { get; private set; }

      public void OnFormFieldChanged(FormController form, string path) {
        ChangeCount++;
      }
    }
  }
}
