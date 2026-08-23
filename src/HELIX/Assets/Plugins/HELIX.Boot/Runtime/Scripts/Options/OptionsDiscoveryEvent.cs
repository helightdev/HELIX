using System;
using System.Collections.Generic;
using HELIX.Compose;
using HELIX.Compose.Forms;
using HELIX.Context;
using HELIX.Prose;
using HELIX.UI;
using UnityEngine;

namespace HELIX.Boot {
  [Managed(typeof(ApplicationScope), phase: LoadPhase.AfterConfiguration)]
  public partial class OptionsService {
    [Inject] private List<IOption> _options;

    public IReadOnlyList<IOption> Options => _options;

    [Hook]
    private void OnLoadManaged(ManagedLoadContext context) {
      Debug.Log($"Discovered options: {string.Join(";", _options)}");
    }

    public void Discover() {
      var fieldReducer = new ComposeProseFieldReducer().Add(OptionPageFieldFactory.Create);
      var handlers = new ProseScopeDelegates<Composable>().Add(new ComposeProseFieldHandler(fieldReducer));
      var writer = new PathSectionedComposeProseWriter(delegates: handlers);
      foreach (var option in _options) {
        writer.Write(option);
      }

      writer.BuildSections();
    }
  }

  [Managed(typeof(ApplicationScope), phase: LoadPhase.Configuration)]
  public partial class OptionsUser {
    [RegisterOption]
    public readonly Option<string> userName = new(Datatypes.String, "User");

    [RegisterOption]
    public readonly Option<int> userAge = new(Datatypes.Int, 18);

    [RegisterOption("user.anonymous")]
    public readonly Option<bool> anonymous = new(Datatypes.Bool, false);
  }

  [AttributeUsage(AttributeTargets.Field)]
  [MixinExpression(
    new[] { MixinOn.Init, MixinOn.ConfigureManaged, MixinOn.LoadManagedLate },
    new[] { -1, -1, 1 },
    @"
@SCOPE
  @MATCH @attr#path:?exists
  @CODE<$Init> @target:name.SetInferredPath(@attr#path);
  @CODE<$ConfigureManaged> registration.Publication(typeof(@target:type:unwrap), @attr#path, true);
  @CODE<$LoadManagedLate> context.PublishBind(typeof(@target:type:unwrap), @attr#path, @target:name);
  @GOTO<Next>
@SCOPE
  @CODE<$Init> @target:name.SetInferredName(""@target:name"");
@END

@SCOPE<Next>
  @CODE<$ConfigureManaged> registration.Publication(typeof(HELIX.Boot.IOption), null, true);
  @CODE<$LoadManagedLate> context.PublishBind(typeof(HELIX.Boot.IOption), null, @target:name);
@END
"
  )]
  public class RegisterOptionAttribute : Attribute {
    public RegisterOptionAttribute() { }

    public RegisterOptionAttribute(string path) { }
  }

  public struct OptionsModifiedEvent : Evt<OptionsModifiedEvent> { }

  public struct OptionApplyEvent<T> : Evt<OptionApplyEvent<T>> {
    public Option<T> option;
  }

  public interface IOption : IProse {
    void LoadInto(FormController controller);
    void AcceptFrom(FormController controller);
  }

  public class Option<T> : IOption {
    public string path;
    public string name;
    public IDatatype<T> datatype;
    public T defaultValue;
    public T value;
    public Action<IProseWriter> modifiers;

    public Option(IDatatype<T> datatype, T defaultValue) {
      this.datatype = datatype;
      this.defaultValue = defaultValue;
    }

    public Option(IDatatype<T> datatype, T defaultValue, Action<IProseWriter> modifiers) {
      this.datatype = datatype;
      this.defaultValue = defaultValue;
      this.modifiers = modifiers;
    }

    public void SetInferredName(string inferred) {
      path ??= inferred;
      name = Casing.Convert(inferred, Casing.CaseStyle.TitleCase);
      path = Casing.Convert(inferred, Casing.CaseStyle.DomainCase);
    }

    public void SetInferredPath(string inferred) {
      path ??= inferred;
      if (string.IsNullOrEmpty(name)) {
        name = Casing.Convert(inferred, Casing.CaseStyle.TitleCase);
      }
    }

    public void LoadInto(FormController controller) {
      var formPath = controller.Path(path);
      controller.SetValue(formPath, value);
      controller.AcceptCurrentValuesAsInitial();
    }

    public void AcceptFrom(FormController controller) {
      var formPath = controller.Path(path);
      // value = controller.GetValue(formPath);
    }

    public void ToProse(IProseWriter writer) {
      using (writer.Field(path, name, datatype)) {
        modifiers?.Invoke(writer);
      }
    }

    public override string ToString() {
      return
        $"{nameof(path)}: {path}, {nameof(name)}: {name}, {nameof(datatype)}: {datatype}, {nameof(defaultValue)}: {defaultValue}, {nameof(value)}: {value}, {nameof(modifiers)}: {modifiers}";
    }
  }
}