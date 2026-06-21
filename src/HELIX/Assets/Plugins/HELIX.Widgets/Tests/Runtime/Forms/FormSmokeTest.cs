using System.Collections.Generic;
using HELIX.Widgets.Elements;
using HELIX.Widgets.Forms;
using HELIX.Widgets.Universal;
using HELIX.Widgets.Universal.Forms;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Tests.Forms {
  public sealed class FormSmokeTest : WidgetTestFixture {
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

      simulate.Click(ElementOf(keys.NewsletterYes).Element);
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
    public void ListWidgetUpdatesPhysicalItemFieldsFromUiWithoutParentFormObserver() {
      var probeKey = new GlobalKey("list-widget-form");

      PumpWidget(new FormListWidgetProbe(key: probeKey));

      var state = StateOf<FormListWidgetProbe, FormListWidgetProbeState>(probeKey);
      Assert.That(TextInputOf("list-widget-name-0").Value, Is.EqualTo("Ada"));
      Assert.That(TextInputOf("list-widget-name-1").Value, Is.EqualTo("Grace"));
      Assert.That(TextInputOf("list-widget-name-2").Value, Is.EqualTo("Linus"));
      AssertListState(state.Form, 3, false);
      AssertHealthyListItemFields(state.Form, "Ada", "Grace", "Linus");

      Click("list-widget-move-down-0");
      Pump(2);

      Assert.That(state.BuildCount, Is.EqualTo(1));
      Assert.That(TextInputOf("list-widget-name-0").Value, Is.EqualTo("Grace"));
      Assert.That(TextInputOf("list-widget-name-1").Value, Is.EqualTo("Ada"));
      Assert.That(TextInputOf("list-widget-name-2").Value, Is.EqualTo("Linus"));
      AssertListState(state.Form, 3, true);
      AssertHealthyListItemFields(state.Form, "Grace", "Ada", "Linus");

      Click("list-widget-insert-before-1");
      Pump(2);

      Assert.That(state.BuildCount, Is.EqualTo(1));
      Assert.That(TextInputOf("list-widget-name-0").Value, Is.EqualTo("Grace"));
      Assert.That(TextInputOf("list-widget-name-1").Value, Is.EqualTo(""));
      Assert.That(TextInputOf("list-widget-name-2").Value, Is.EqualTo("Ada"));
      Assert.That(TextInputOf("list-widget-name-3").Value, Is.EqualTo("Linus"));
      AssertListState(state.Form, 4, true);
      AssertHealthyListItemFields(state.Form, "Grace", "", "Ada", "Linus");

      Click("list-widget-remove-1");
      Pump(2);

      Assert.That(state.BuildCount, Is.EqualTo(1));
      Assert.That(TextInputOf("list-widget-name-0").Value, Is.EqualTo("Grace"));
      Assert.That(TextInputOf("list-widget-name-1").Value, Is.EqualTo("Ada"));
      Assert.That(TextInputOf("list-widget-name-2").Value, Is.EqualTo("Linus"));
      AssertListState(state.Form, 3, true);
      AssertHealthyListItemFields(state.Form, "Grace", "Ada", "Linus");

      Click("list-widget-append");
      Pump(2);

      Assert.That(state.BuildCount, Is.EqualTo(1));
      Assert.That(TextInputOf("list-widget-name-0").Value, Is.EqualTo("Grace"));
      Assert.That(TextInputOf("list-widget-name-1").Value, Is.EqualTo("Ada"));
      Assert.That(TextInputOf("list-widget-name-2").Value, Is.EqualTo("Linus"));
      Assert.That(TextInputOf("list-widget-name-3").Value, Is.EqualTo("New 4"));
      AssertListState(state.Form, 4, true);
      AssertHealthyListItemFields(state.Form, "Grace", "Ada", "Linus", "New 4");
    }

    private static void AssertFieldDisabled(FormController form, string path, bool disabled) {
      Assert.That(form.GetFieldData(path), Is.Not.Null, path);
      Assert.That(form.GetFieldData(path).HasFlag(FieldFlags.Disabled), Is.EqualTo(disabled), path);
    }

    private void Click(Key key) {
      var element = ElementOf<IWidgetElement>(key);
      Assert.That(element, Is.Not.Null);
      simulate.Click(element.Element);
    }

    private GenericTextInput TextInputOf(Key key) {
      var element = ElementOf<StatefulWidgetElement<HFormTextField>>(key);
      Assert.That(element, Is.Not.Null);
      var input = element.Element.Q<GenericTextInput>();
      Assert.That(input, Is.Not.Null);
      return input;
    }

    private static void AssertListState(FormController form, int count, bool dirty) {
      var list = form.GetFieldData("contacts");
      Assert.That(list, Is.Not.Null);
      Assert.That(list.isListField, Is.True);
      Assert.That(list.listCount, Is.EqualTo(count));
      Assert.That(list.HasFlag(FieldFlags.Stale), Is.False);
      Assert.That(list.HasFlag(FieldFlags.Disabled), Is.False);
      Assert.That(list.HasFlag(FieldFlags.Error), Is.False);
      Assert.That(list.HasFlag(FieldFlags.Dirty), Is.EqualTo(dirty));
    }

    private static void AssertHealthyListItemFields(FormController form, params string[] expectedNames) {
      for (var i = 0; i < expectedNames.Length; i++) {
        var path = $"contacts.{i}.name";
        var field = form.GetFieldData(path);

        Assert.That(field, Is.Not.Null, path);
        Assert.That(field.field, Is.Not.Null, path);
        Assert.That(field.HasFlag(FieldFlags.Stale), Is.False, path);
        Assert.That(field.HasFlag(FieldFlags.Disabled), Is.False, path);
        Assert.That(field.HasFlag(FieldFlags.Error), Is.False, path);
        Assert.That(field.HasFlag(FieldFlags.Dirty), Is.False, path);
        Assert.That(form.GetErrors(path), Is.Empty, path);
        Assert.That(form.GetValue<string>(path), Is.EqualTo(expectedNames[i]), path);
      }
    }
  }

  public sealed class FormListWidgetProbe : StatefulWidget<FormListWidgetProbe> {
    public FormListWidgetProbe(Key key = default) : base(key) { }

    public override State<FormListWidgetProbe> CreateState() {
      return new FormListWidgetProbeState();
    }
  }

  public sealed class FormListWidgetProbeState : State<FormListWidgetProbe> {
    public FormController Form { get; private set; }
    public int BuildCount { get; private set; }

    public override void InitState() {
      Form = AddDisposable(new FormController());
      Form.Reset(new Dictionary<string, object> {
        ["contacts.0.name"] = "Ada",
        ["contacts.1.name"] = "Grace",
        ["contacts.2.name"] = "Linus"
      }, false);
    }

    public override Widget Build(BuildContext context) {
      BuildCount++;
      return new HForm(Form) {
        new HColumn(gap: 4, crossAxisAlign: Align.Stretch) {
          new HButton(
            focusKey: "list-widget-append",
            child: new HText("Add contact"),
            onClick: AppendContact
          ),
          new HFormListField(
            "contacts",
            itemBuilder: ContactFields,
            itemWrapperBuilder: ContactChrome,
            containerBuilder: (_, items) => new HColumn(gap: 4, crossAxisAlign: Align.Stretch, children: items)
          )
        }
      };
    }

    private void AppendContact() {
      var index = Form.AppendListItem("contacts");
      Form.SetValue($"contacts.{index}.name", $"New {index + 1}");
    }

    private Widget ContactFields(BuildContext context, int index) {
      return new HFormTextField(
        "name",
        validators: new[] { FormValidators.Required() },
        validationMode: ValidationMode.OnSubmit,
        key: $"list-widget-name-{index}"
      );
    }

    private Widget ContactChrome(BuildContext context, FormListItemContext item, Widget child) {
      return new HColumn(gap: 4, crossAxisAlign: Align.Stretch) {
        new HRow(gap: 4, crossAxisAlign: Align.Stretch) {
          new HButton(
            focusKey: $"list-widget-insert-before-{item.Index}",
            child: new HText("Insert before"),
            onClick: item.InsertBefore
          ),
          new HButton(
            focusKey: $"list-widget-move-down-{item.Index}",
            enabled: !item.IsLast,
            child: new HText("Move down"),
            onClick: item.MoveDown
          ),
          new HButton(
            focusKey: $"list-widget-remove-{item.Index}",
            child: new HText("Remove"),
            onClick: item.Remove
          )
        },
        child
      };
    }
  }

  public sealed class FormSmokeKeys {
    public readonly GlobalKey ProfileName = new GlobalKey("smoke-profile-name");
    public readonly GlobalKey ShippingCity = new GlobalKey("smoke-shipping-city");
    public readonly GlobalKey Contact0Name = new GlobalKey("smoke-contact-0-name");
    public readonly GlobalKey Contact1Email = new GlobalKey("smoke-contact-1-email");
    public readonly GlobalKey NewsletterYes = new GlobalKey("smoke-newsletter-yes");
    public readonly GlobalKey NewsletterNo = new GlobalKey("smoke-newsletter-no");
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
            focusKey: widget.keys.NewsletterYes,
            child: new HText("Use yes"),
            onClick: () => SetNewsletterEnabled(true)
          ),
          new HButton(
            focusKey: widget.keys.NewsletterNo,
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
