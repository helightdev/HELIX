using System.Collections.Generic;
using HELIX.Widgets.Elements;
using HELIX.Widgets.Forms;
using HELIX.Widgets.Universal;
using HELIX.Widgets.Universal.Forms;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Tests.Forms {
  public sealed class FormProbeTest : WidgetTestFixture {
    [Test]
    public void FormFieldRegistersWithContextPathAndUpdatesEnabledMetadata() {
      var probeKey = new GlobalKey("form-probe");
      var fieldKey = new GlobalKey("name-field");
      var disableKey = new GlobalKey("name-disable");
      var enableKey = new GlobalKey("name-enable");

      PumpWidget(new ContextPathProbe(fieldKey, disableKey, enableKey, key: probeKey));

      var state = StateOf<ContextPathProbe, ContextPathProbeState>(probeKey);
      Assert.That(fieldKey.Target, Is.Not.Null);
      Assert.That(state.BuildCount, Is.GreaterThanOrEqualTo(1));
      Assert.That(state.Form.HasValue("profile.name"), Is.True);
      Assert.That(state.Form.GetValue<string>("profile.name"), Is.EqualTo("Ada"));
      Assert.That(state.Form.GetFieldData("profile.name")?.HasFlag(FieldFlags.Disabled), Is.False);

      simulate.Click(ElementOf(disableKey).Element);
      Pump();

      Assert.That(state.Form.GetFieldData("profile.name")?.HasFlag(FieldFlags.Disabled), Is.True);
      Assert.That(state.LastFieldStatus, Is.EqualTo("Disabled / OnSubmit"));

      simulate.Click(ElementOf(enableKey).Element);
      Pump();

      Assert.That(state.Form.GetFieldData("profile.name")?.HasFlag(FieldFlags.Disabled), Is.False);
      Assert.That(state.LastFieldStatus, Is.EqualTo("clean / OnSubmit"));
    }

    [Test]
    public void FormTextFieldEnabledStateUpdatesFromUiWithoutParentFormObserver() {
      var fieldKey = new GlobalKey("standalone-form-field");
      var disableKey = new GlobalKey("standalone-form-disable");
      var enableKey = new GlobalKey("standalone-form-enable");
      var probeKey = new GlobalKey("standalone-form");

      PumpWidget(new StandaloneFieldProbe(fieldKey, disableKey, enableKey, key: probeKey));

      var state = StateOf<StandaloneFieldProbe, StandaloneFieldProbeState>(probeKey);
      Assert.That(TextInputOf(fieldKey).enabledSelf, Is.True);

      simulate.Click(ElementOf(disableKey).Element);
      Pump();

      Assert.That(state.BuildCount, Is.EqualTo(1));
      Assert.That(TextInputOf(fieldKey).enabledSelf, Is.False);

      simulate.Click(ElementOf(enableKey).Element);
      Pump();

      Assert.That(state.BuildCount, Is.EqualTo(1));
      Assert.That(TextInputOf(fieldKey).enabledSelf, Is.True);
    }

    private static GenericTextInput TextInputOf(GlobalKey key) {
      Assert.That(key.Target, Is.Not.Null);
      var input = key.Target.Element.Q<GenericTextInput>();
      Assert.That(input, Is.Not.Null);
      return input;
    }
  }

  public sealed class ContextPathProbe : StatefulWidget<ContextPathProbe> {
    public readonly GlobalKey fieldKey;
    public readonly GlobalKey disableKey;
    public readonly GlobalKey enableKey;

    public ContextPathProbe(
      GlobalKey fieldKey,
      GlobalKey disableKey,
      GlobalKey enableKey,
      Key key = default
    ) : base(key) {
      this.fieldKey = fieldKey;
      this.disableKey = disableKey;
      this.enableKey = enableKey;
    }

    public override State<ContextPathProbe> CreateState() {
      return new ContextPathProbeState();
    }
  }

  public sealed class ContextPathProbeState : State<ContextPathProbe> {
    public FormController Form { get; private set; }
    public int BuildCount { get; private set; }
    public string LastFieldStatus { get; private set; }

    public override void InitState() {
      Form = AddDisposable(new FormController());
      Form.Reset(new Dictionary<string, object> {
        ["profile.name"] = "Ada"
      }, false);
      AddDisposable(Form.AddObserver(SetState));
    }

    public override Widget Build(BuildContext context) {
      BuildCount++;
      LastFieldStatus = FieldStatus("profile.name");

      return new HForm(Form) {
        new HFormScope("profile") {
          new HColumn(gap: 4, crossAxisAlign: Align.Stretch) {
            new HRow(gap: 4, crossAxisAlign: Align.Stretch) {
              new HButton(
                focusKey: widget.disableKey,
                child: new HText("Disable name"),
                onClick: () => Form.SetFieldEnabled("profile.name", false)
              ),
              new HButton(
                focusKey: widget.enableKey,
                child: new HText("Enable name"),
                onClick: () => Form.SetFieldEnabled("profile.name", true)
              )
            },
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

    private string FieldStatus(string path) {
      var data = Form.GetFieldData(path);
      if (data == null) return "unregistered";

      var flags = data.flags == FieldFlags.None ? "clean" : data.flags.ToString();
      var mode = data.validationMode == ValidationMode.None ? "no validation" : data.validationMode.ToString();
      return $"{flags} / {mode}";
    }
  }

  public sealed class StandaloneFieldProbe : StatefulWidget<StandaloneFieldProbe> {
    public readonly GlobalKey fieldKey;
    public readonly GlobalKey disableKey;
    public readonly GlobalKey enableKey;

    public StandaloneFieldProbe(
      GlobalKey fieldKey,
      GlobalKey disableKey,
      GlobalKey enableKey,
      Key key = default
    ) : base(key) {
      this.fieldKey = fieldKey;
      this.disableKey = disableKey;
      this.enableKey = enableKey;
    }

    public override State<StandaloneFieldProbe> CreateState() {
      return new StandaloneFieldProbeState();
    }
  }

  public sealed class StandaloneFieldProbeState : State<StandaloneFieldProbe> {
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
        new HColumn(gap: 4, crossAxisAlign: Align.Stretch) {
          new HRow(gap: 4, crossAxisAlign: Align.Stretch) {
            new HButton(
              focusKey: widget.disableKey,
              child: new HText("Disable"),
              onClick: () => Form.SetFieldEnabled("name", false)
            ),
            new HButton(
              focusKey: widget.enableKey,
              child: new HText("Enable"),
              onClick: () => Form.SetFieldEnabled("name", true)
            )
          },
          new HFormTextField(
            "name",
            validators: new[] { FormValidators.Required() },
            validationMode: ValidationMode.OnSubmit,
            key: widget.fieldKey
          )
        }
      };
    }
  }
}
