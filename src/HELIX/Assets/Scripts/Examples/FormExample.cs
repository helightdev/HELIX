using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HELIX.Extensions;
using HELIX.Types;
using HELIX.Widgets;
using HELIX.Widgets.Forms;
using HELIX.Widgets.Modifiers;
using HELIX.Widgets.Scrolling;
using HELIX.Widgets.Universal;
using HELIX.Widgets.Universal.Styles;
using HELIX.Widgets.Universal.Theme;
using UnityEngine;
using UnityEngine.UIElements;

namespace Examples {
  public class FormExample : StatefulWidget<FormExample> {
    public override State<FormExample> CreateState() {
      return new FormExampleState();
    }
  }

  public class FormExampleState : State<FormExample> {
    private FormController _form;
    private ScrollController _formScroll;
    private ScrollController _detailsScroll;
    private FormSubmitResult _lastSubmit;
    private string _lastAction = "No submit yet.";

    public override void InitState() {
      base.InitState();
      _form = AddDisposable(new FormController());
      _formScroll = AddDisposable(new ScrollController());
      _detailsScroll = AddDisposable(new ScrollController());
      SeedInitialValues(false);
      AddDisposable(_form.AddObserver(SetState));
    }

    public override Widget Build(BuildContext context) {
      return new HRow(gap: 16, crossAxisAlign: Align.Stretch) {
        new HBox(
          background: context.GetThemed(PrimitiveTheme.Container),
          borderRadius: BorderRadius.All(12)
        ) {
          new HRow(crossAxisAlign: Align.Stretch) {
            new HForm(
              _form,
              new HColumn(gap: 16, crossAxisAlign: Align.Stretch) {
                new HScrollView(controller: _formScroll) {
                  Header(context),
                  PersonalSection(context),
                  AccountSection(context),
                  PreferencesSection(context),
                  AddressSection(context),
                  ContactsSection(context),
                }.Expand(),
                FormActions(context)
              }.Padding(14)
            ).Expand(),
            new HSlider(_formScroll).Size(width: 12)
          }
        }.Expand(1.35f),
        new HBox(
          background: context.GetThemed(PrimitiveTheme.Container),
          borderRadius: BorderRadius.All(12)
        ) {
          new HRow(crossAxisAlign: Align.Stretch) {
            new HScrollView(controller: _detailsScroll) {
              new HColumn(gap: 14, crossAxisAlign: Align.Stretch) {
                new HText("Live form state").Heading(context, 2),
                DetailsBlock(context, "Last submit", _lastAction),
                DetailsBlock(context, "Flat data", FormatValue(_form.Data)),
                DetailsBlock(context, "DumpTree()", DumpTreePreview()),
                DetailsBlock(context, "DumpTree(include disabled/stale)", DumpTreePreview(true, true)),
                DetailsBlock(context, "Field metadata", FormatFields()),
                DetailsBlock(context, "Submit errors", FormatSubmitErrors())
              }.Padding(14)
            }.Expand(),
            new HSlider(_detailsScroll).Size(width: 12)
          }
        }.Size(33.Percent(), Length.Auto())
      }.Margin(16);
    }

    private void SeedInitialValues(bool notify = true) {
      var values = new Dictionary<string, object> {
        ["profile.firstName"] = "Ada",
        ["profile.lastName"] = "Lovelace",
        ["profile.email"] = "ada@example.com",
        ["profile.bio"] = "Writes notes about analytical engines.",
        ["account.username"] = "ada",
        ["account.password"] = "analytical-engine",
        ["account.confirmPassword"] = "analytical-engine",
        ["newsletter"] = "yes",
        ["newsletterTopic"] = "Computing history",
        ["theme"] = "system",
        ["email"] = "enabled",
        ["sms"] = "disabled",
        ["shipping.address.line1"] = "12 St. James Square",
        ["shipping.address.line2"] = "",
        ["shipping.address.city"] = "London",
        ["shipping.address.region"] = "London",
        ["shipping.address.postalCode"] = "SW1Y",
        ["contacts.0.name"] = "Charles Babbage",
        ["contacts.0.relationship"] = "Collaborator",
        ["contacts.0.email"] = "charles@example.com",
        ["contacts.1.name"] = "Mary Somerville",
        ["contacts.1.relationship"] = "Mentor",
        ["contacts.1.email"] = "mary@example.com"
      };

      using (_form.BeginUpdate()) {
        _form.Reset(values, notify);
        _form.SetFieldEnabled("newsletterTopic", true, notify: notify);
        _form.SetFieldEnabled("shipping.address", true, true, notify);
      }
    }

    private Widget Header(BuildContext context) {
      return new HColumn(gap: 6, crossAxisAlign: Align.Stretch) {
        new HText("Form widgets").Heading(context),
        new HText("Text fields, validation modes, scopes, lists, submit, reset, and tree dumping.")
          .Body(context)
      };
    }

    private Widget PersonalSection(BuildContext context) {
      return Section(context, "Profile", new HColumn(gap: 10, crossAxisAlign: Align.Stretch) {
        Field(
          context,
          "First name",
          new HFormTextField(
            "profile.firstName",
            validators: new[] { FormValidators.Required(), FormValidators.MinLength(2) },
            validationMode: ValidationMode.OnDirty | ValidationMode.OnSubmit,
            initialValue: ""
          ).TightStretch(),
          "profile.firstName"
        ),
        Field(
          context,
          "Last name",
          new HFormTextField(
            "profile.lastName",
            validators: new[] { FormValidators.Required() },
            validationMode: ValidationMode.OnFinishEditing | ValidationMode.OnSubmit,
            initialValue: ""
          ).TightStretch(),
          "profile.lastName"
        ),
        Field(
          context,
          "Email",
          new HFormTextField(
            "profile.email",
            validators: new[] {
              FormValidators.Required(),
              FormValidators.Func((_, _, value) => IsEmail(value) ? null : "Enter a valid email address")
            },
            validationMode: ValidationMode.OnChange | ValidationMode.OnSubmit,
            keyboardType: TouchScreenKeyboardType.EmailAddress,
            initialValue: ""
          ).TightStretch(),
          "profile.email"
        ),
        Field(
          context,
          "Bio",
          new HFormTextField(
            "profile.bio",
            validationMode: ValidationMode.None,
            multiline: true,
            maxLength: 160,
            initialValue: ""
          ).Size(height: 92).TightStretch(),
          "profile.bio"
        )
      });
    }

    private Widget AccountSection(BuildContext context) {
      return Section(context, "Account", new HColumn(gap: 10, crossAxisAlign: Align.Stretch) {
        new HFormScope("account") {
          new HColumn(gap: 10, crossAxisAlign: Align.Stretch) {
            Field(
              context,
              "Username",
              new HFormTextField(
                "username",
                validators: new[] {
                  FormValidators.Required(),
                  FormValidators.MinLength(3),
                  FormValidators.Func((_, _, value) => HasOnlySlugCharacters(value)
                    ? null
                    : "Use lowercase letters, numbers, dots, underscores, or hyphens")
                },
                validationMode: ValidationMode.OnDirty | ValidationMode.OnSubmit,
                initialValue: ""
              ).TightStretch(),
              "account.username"
            ),
            Field(
              context,
              "Password",
              new HFormTextField(
                "password",
                validators: new[] { FormValidators.Required(), FormValidators.MinLength(8) },
                validationMode: ValidationMode.OnSubmit,
                isPasswordField: true,
                maskChar: '*',
                initialValue: ""
              ).TightStretch(),
              "account.password"
            ),
            Field(
              context,
              "Confirm password",
              new HFormTextField(
                "confirmPassword",
                validators: new[] {
                  FormValidators.Func((form, _, value) =>
                    string.Equals(Convert.ToString(value), form.GetValue<string>("account.password", ""))
                      ? null
                      : "Passwords must match")
                },
                validationMode: ValidationMode.OnChange | ValidationMode.OnSubmit,
                isPasswordField: true,
                maskChar: '*',
                initialValue: "analytical-engine"
              ).TightStretch(),
              "account.confirmPassword"
            )
          }
        }
      });
    }

    private Widget PreferencesSection(BuildContext context) {
      return Section(context, "Preferences", new HColumn(gap: 10, crossAxisAlign: Align.Stretch) {
        Field(
          context,
          "Newsletter",
          SegmentedTextField("newsletter", new[] { "yes", "no" }),
          "newsletter"
        ),
        NewsletterTopicField(context),
        Field(
          context,
          "Theme",
          SegmentedTextField("theme", new[] { "system", "light", "dark" }),
          "theme"
        ),
        Field(
          context,
          "Email alerts",
          SegmentedTextField("email", new[] { "enabled", "disabled" }),
          "email"
        ),
        Field(
          context,
          "SMS alerts",
          SegmentedTextField("sms", new[] { "enabled", "disabled" }),
          "sms"
        )
      });
    }

    private Widget NewsletterTopicField(BuildContext context) {
      var enabled = IsNewsletterEnabled();

      return Field(
        context,
        "Newsletter topic",
        new HFormTextField(
          "newsletterTopic",
          validators: new[] { FormValidators.Required("Pick a topic while newsletter is enabled") },
          validationMode: ValidationMode.OnChange | ValidationMode.OnSubmit,
          initialValue: "",
          enabled: enabled
        ).TightStretch(),
        "newsletterTopic"
      );
    }

    private Widget AddressSection(BuildContext context) {
      var enabled = IsNewsletterEnabled();

      return Section(context, "Conditional shipping address", new HFormScope("shipping") {
        new HFormScope("address") {
          new HColumn(gap: 10, crossAxisAlign: Align.Stretch) {
            Field(
              context,
              "Line 1",
              new HFormTextField(
                "line1",
                validators: new[] { FormValidators.Required() },
                validationMode: ValidationMode.OnSubmit,
                initialValue: "",
                enabled: enabled
              ).TightStretch(),
              "shipping.address.line1"
            ),
            Field(
              context,
              "Line 2",
              new HFormTextField(
                "line2",
                validationMode: ValidationMode.None,
                initialValue: "",
                enabled: enabled
              ).TightStretch(),
              "shipping.address.line2"
            ),
            new HRow(gap: 10, crossAxisAlign: Align.FlexStart) {
              Field(
                context,
                "City",
                new HFormTextField(
                  "city",
                  validators: new[] { FormValidators.Required() },
                  validationMode: ValidationMode.OnSubmit,
                  initialValue: "",
                  enabled: enabled
                ).TightStretch(),
                "shipping.address.city"
              ).Expand(),
              Field(
                context,
                "Region",
                new HFormTextField("region", initialValue: "", enabled: enabled).TightStretch(),
                "shipping.address.region"
              ).Expand(),
              Field(
                context,
                "Postal code",
                new HFormTextField(
                  "postalCode",
                  validators: new[] { FormValidators.Required() },
                  validationMode: ValidationMode.OnFinishEditing | ValidationMode.OnSubmit,
                  initialValue: "",
                  enabled: enabled
                ).TightStretch(),
                "shipping.address.postalCode"
              ).Expand()
            }
          }
        }
      }).Display(enabled);
    }

    private bool IsNewsletterEnabled() {
      return string.Equals(_form.GetValue<string>("newsletter", "yes"), "yes", StringComparison.Ordinal);
    }

    private Widget ContactsSection(BuildContext context) {
      var contacts = new HColumn(gap: 10, crossAxisAlign: Align.Stretch) {
        new HFormListField(
          "contacts",
          itemBuilder: ContactFields,
          containerBuilder: ContactListContainer,
          itemWrapperBuilder: ContactItemChrome,
          validators: new[] {
            FormValidators.Func((form, path, value) =>
              form.GetListCount(path) > 0 ? null : "Add at least one contact")
          },
          validationMode: ValidationMode.OnSubmit
        ),
        FieldErrors(context, "contacts"),
        new HRow(gap: 8) {
          new HButton(
            HButtonVariant.Flat,
            child: new HText("Add contact"),
            onClick: () => {
              var index = _form.AppendListItem("contacts");
              _form.SetValue($"contacts.{index}.name", "", FormChangeReason.Programmatic);
              _form.SetValue($"contacts.{index}.relationship", "", FormChangeReason.Programmatic);
              _form.SetValue($"contacts.{index}.email", "", FormChangeReason.Programmatic);
            }
          ),
          new HButton(
            HButtonVariant.Outline,
            child: new HText("Reset list"),
            onClick: () => _form.ResetField("contacts")
          )
        }
      };

      return Section(context, "Dynamic contact list", contacts);
    }

    private Widget ContactListContainer(BuildContext context, WidgetList items) {
      if (items.Count == 0) return new HText("No contacts registered.").Caption(context);
      return new HColumn(gap: 10, crossAxisAlign: Align.Stretch, children: items);
    }

    private Widget ContactItemChrome(BuildContext context, FormListItemContext item, Widget child) {
      return new HBox(
        background: context.GetThemed(PrimitiveTheme.Container),
        borderRadius: BorderRadius.All(8)
      ) {
        new HColumn(gap: 8, crossAxisAlign: Align.Stretch) {
          new HRow(gap: 8) {
            new HText($"Contact {item.Index + 1}").Body(context),
            new HButton(
              HButtonVariant.Ghost,
              size: HButtonSize.Small,
              child: new HText("Insert before"),
              onClick: item.InsertBefore
            ),
            new HButton(
              HButtonVariant.Ghost,
              size: HButtonSize.Small,
              enabled: !item.IsFirst,
              child: new HText("Move up"),
              onClick: item.MoveUp
            ),
            new HButton(
              HButtonVariant.Ghost,
              size: HButtonSize.Small,
              enabled: !item.IsLast,
              child: new HText("Move down"),
              onClick: item.MoveDown
            ),
            new HButton(
              HButtonVariant.Ghost,
              size: HButtonSize.Small,
              child: new HText("Remove"),
              onClick: item.Remove
            )
          },
          child
        }.Padding(10)
      };
    }

    private Widget ContactFields(BuildContext context, int index) {
      var prefix = $"contacts.{index}";
      return new HRow(gap: 10, crossAxisAlign: Align.FlexStart) {
        Field(
          context,
          "Name",
          new HFormTextField(
            "name",
            validators: new[] { FormValidators.Required() },
            validationMode: ValidationMode.OnSubmit,
            initialValue: ""
          ).TightStretch(),
          $"{prefix}.name"
        ).Expand(),
        Field(
          context,
          "Relationship",
          new HFormTextField("relationship", initialValue: "").TightStretch(),
          $"{prefix}.relationship"
        ).Expand(),
        Field(
          context,
          "Email",
          new HFormTextField(
            "email",
            validators: new[] {
              FormValidators.Func((_, _, value) =>
                string.IsNullOrWhiteSpace(Convert.ToString(value)) || IsEmail(value)
                  ? null
                  : "Enter a valid email")
            },
            validationMode: ValidationMode.OnChange | ValidationMode.OnSubmit,
            keyboardType: TouchScreenKeyboardType.EmailAddress,
            initialValue: ""
          ).TightStretch(),
          $"{prefix}.email"
        ).Expand()
      };
    }

    private Widget FormActions(BuildContext context) {
      return new HRow(gap: 8) {
        new HButton(
          HButtonVariant.Flat,
          child: new HText("Submit"),
          onClick: () => {
            _lastSubmit = _form.Submit();
            _lastAction = _lastSubmit.valid
              ? "Submit valid. Tree contains nested dictionaries and contact list values."
              : "Submit invalid. See field errors below.";
            SetState();
          }
        ),
        new HButton(
          HButtonVariant.Outline,
          child: new HText("Validate all"),
          onClick: () => {
            var valid = _form.ValidateAll();
            _lastAction = valid ? "All registered fields are valid." : "Validation found errors.";
            SetState();
          }
        ),
        new HButton(
          HButtonVariant.Soft,
          child: new HText("Set demo values"),
          onClick: () => SeedInitialValues()
        ),
        new HButton(
          HButtonVariant.Soft,
          child: new HText("Accept as initial"),
          onClick: () => {
            _form.AcceptCurrentValuesAsInitial();
            _lastAction = "Current values accepted as initial; dirty flags were cleared.";
          }
        ),
        new HButton(
          HButtonVariant.Ghost,
          child: new HText("Reset"),
          onClick: () => {
            _form.Reset();
            _lastSubmit = null;
            _lastAction = "Form reset to registered initial values.";
          }
        ),
        new HButton(
          HButtonVariant.Ghost,
          child: new HText("Refresh"),
          onClick: () => {
            SetState();
          }
        )
      };
    }

    private Widget Section(BuildContext context, string title, Widget child) {
      return new HBox(
        background: context.GetThemed(PrimitiveTheme.Container),
        borderRadius: BorderRadius.All(8),
        key: $"section.{title}"
      ) {
        new HColumn(gap: 10, crossAxisAlign: Align.Stretch) {
          new HText(title).Heading(context, 2),
          child
        }.Padding(12)
      };
    }

    private Widget Field(BuildContext context, string label, Widget input, string path) {
      return new HColumn(gap: 5, crossAxisAlign: Align.Stretch, key: $"field.{path}") {
        new HRow(gap: 8) {
          new HText(label).Body(context),
          new HText(FieldStatus(path)).Caption(context)
        },
        input,
        FieldErrors(context, path)
      };
    }

    private Widget FieldErrors(BuildContext context, string path) {
      var errors = _form.GetErrors(path);
      return new HText(errors.Count == 0 ? " " : string.Join("\n", errors)).Caption(context);
    }

    private Widget SegmentedTextField(string path, IReadOnlyList<string> options) {
      var selected = _form.GetValue<string>(path, options[0]);
      var row = new HRow(gap: 6);
      foreach (var option in options) {
        row.Add(new HButton(
          HButtonVariant.SoftTwoState,
          size: HButtonSize.Small,
          selected: string.Equals(selected, option, StringComparison.Ordinal),
          child: new HText(option),
          onClick: () => SetSegmentedValue(path, option)
        ));
      }

      return new HColumn(gap: 6, crossAxisAlign: Align.Stretch) {
        row,
        new HFormTextField(
          path,
          validators: new[] { FormValidators.Required() },
          validationMode: ValidationMode.OnChange | ValidationMode.OnSubmit,
          initialValue: options[0],
          isReadOnly: true
        ).TightStretch()
      };
    }

    private void SetSegmentedValue(string path, string value) {
      _form.Batch(() => {
        _form.SetValue(path, value, FormChangeReason.User);
        if (!string.Equals(path, "newsletter", StringComparison.Ordinal)) return;

        var enabled = string.Equals(value, "yes", StringComparison.Ordinal);
        _form.SetFieldEnabled("newsletterTopic", enabled);
        _form.SetFieldEnabled("shipping.address", enabled, true);
      });
    }

    private Widget DetailsBlock(BuildContext context, string title, string value) {
      return new HBox(
        background: context.GetThemed(PrimitiveTheme.Container),
        borderRadius: BorderRadius.All(8)
      ) {
        new HColumn(gap: 6, crossAxisAlign: Align.Stretch) {
          new HText(title).Body(context),
          new HText(value, selectable: true).Caption(context)
        }.Padding(10)
      };
    }

    private string FieldStatus(string path) {
      var data = _form.GetFieldData(path);
      if (data == null) return "unregistered";

      var flags = data.flags == FieldFlags.None
        ? "clean"
        : data.flags.ToString();
      var mode = data.validationMode == ValidationMode.None
        ? "no validation"
        : data.validationMode.ToString();
      return $"{flags} / {mode}";
    }

    private string DumpTreePreview(bool includeStaleData = false, bool includeDisabledData = false) {
      return _form.TryDumpTree(out var tree, out var errors, includeStaleData, includeDisabledData)
        ? FormatValue(tree)
        : string.Join("\n", errors);
    }

    private string FormatFields() {
      if (_form.Fields.Count == 0) return "(no fields)\n";

      var builder = new StringBuilder();
      foreach (var pair in _form.Fields.OrderBy(x => x.Key, StringComparer.Ordinal)) {
        var data = pair.Value;
        builder.Append(pair.Key);
        builder.Append(" = ");
        builder.Append(data.flags == FieldFlags.None ? "clean" : data.flags);
        if (data.isListField) {
          builder.Append(", count ");
          builder.Append(_form.GetListCount(pair.Key));
        }

        if (data.errors.Count > 0) {
          builder.Append(", errors: ");
          builder.Append(string.Join(" | ", data.errors));
        }

        builder.AppendLine();
      }

      return builder.ToString();
    }

    private string FormatSubmitErrors() {
      if (_lastSubmit?.errors == null || _lastSubmit.errors.Count == 0) return "(none)\n";
      var builder = new StringBuilder();
      foreach (var pair in _lastSubmit.errors) {
        builder.Append(pair.Key.Length == 0 ? "<tree>" : pair.Key);
        builder.Append(": ");
        builder.AppendLine(string.Join(" | ", pair.Value));
      }

      return builder.ToString();
    }

    private static bool IsEmail(object value) {
      var text = Convert.ToString(value);
      return !string.IsNullOrWhiteSpace(text)
             && text.IndexOf("@", StringComparison.Ordinal) >= 0
             && text.IndexOf(".", StringComparison.Ordinal) >= 0;
    }

    private static bool HasOnlySlugCharacters(object value) {
      var text = Convert.ToString(value);
      if (string.IsNullOrWhiteSpace(text)) return false;
      return text.All(c => char.IsLower(c) || char.IsDigit(c) || c == '-' || c == '_' || c == '.');
    }

    private static string FormatValue(object value, int indent = 0) {
      var padding = new string(' ', indent);
      switch (value) {
        case Dictionary<string, object> dictionary:
          return FormatDictionary(dictionary, indent);
        case IReadOnlyDictionary<string, object> readOnlyDictionary:
          return FormatDictionary(readOnlyDictionary, indent);
        case IList list:
          var listBuilder = new StringBuilder();
          listBuilder.AppendLine("[");
          for (var i = 0; i < list.Count; i++) {
            listBuilder.Append(new string(' ', indent + 2));
            listBuilder.Append(FormatValue(list[i], indent + 2));
          }

          listBuilder.Append(padding);
          listBuilder.AppendLine("]");
          return listBuilder.ToString();
        case null:
          return "null\n";
        default:
          return $"{value}\n";
      }
    }

    private static string FormatDictionary(IEnumerable<KeyValuePair<string, object>> dictionary, int indent) {
      var padding = new string(' ', indent);
      var builder = new StringBuilder();
      builder.AppendLine("{");
      foreach (var pair in dictionary.OrderBy(x => x.Key, StringComparer.Ordinal)) {
        builder.Append(new string(' ', indent + 2));
        builder.Append(pair.Key);
        builder.Append(": ");
        builder.Append(FormatValue(pair.Value, indent + 2));
      }

      builder.Append(padding);
      builder.AppendLine("}");
      return builder.ToString();
    }
  }
}
