using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using HELIX.Compose;
using HELIX.Compose.Forms;
using HELIX.Context;
using HELIX.Prose;
using HELIX.Signals;
using HELIX.UI;
using HELIX.UI.Options;
using HELIX.Widgets.Signals;
using UnityEngine;

namespace HELIX.Boot {
  [Managed(typeof(ApplicationScope), phase: LoadPhase.AfterConfiguration)]
  public partial class OptionsService {
    [Inject] private List<Option> _options;

    public IReadOnlyList<Option> Options => _options;

    [Hook]
    private void OnLoadManaged(ManagedLoadContext context) {
      Debug.Log($"Discovered options: {string.Join(";", _options)}");

      foreach (var option in _options) {
        option.LoadData();
      }
    }

    [EventHandler]
    private void OnDefaultOptionsRender(OptionsRenderEvent evt) {
      foreach (var option in _options) {
        using (evt.writer.Path(option.groupPath)) {
          option.ToProse(evt.writer);
          Debug.Log($"Writing option {option.path} to prose with group path {option.groupPath}");
        }
      }
    }

    [EventHandler]
    private void OnOptionsLoadPageState(OptionsLoadPageStateEvent evt) {
      foreach (var option in _options) {
        option.LoadInto(evt.pages.Form);
      }
    }

    public OptionPages BuildOptionPages() {
      var fieldReducer = new ComposeProseFieldReducer().Add(OptionPageFieldFactory.Create);
      var handlers = new ProseScopeDelegates<Composable>().Add(new ComposeProseFieldHandler(fieldReducer));
      var writer = new NavigableProseWriter(delegates: handlers);
      new OptionsRenderEvent(writer).Raise();
      var prose = writer.BuildSections();

      var debugWriter = new ProseTextWriter();
      debugWriter.Write(prose);
      Debug.Log(debugWriter.Build());

      var pages = new OptionPages(prose, OptionPagesOptions.Default);
      new OptionsLoadPageStateEvent(pages).Raise();
      pages.Form.AcceptCurrentValuesAsInitial();
      return pages;
    }
  }

  // public class OptionModificationState : ISignalObserver {
  //   public FormController controller;
  //   public OptionEagerness eagerness;
  //
  //   public void Activate() {
  //     controller.AddObserver(this);
  //   }
  //
  //   public void OnSignalChanged(Signal signal) {
  //
  //   }
  //
  //   public bool IsDisposed { get; } = false;
  // }

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
  @CODE<$ConfigureManaged> registration.Publication(typeof(HELIX.Boot.Option), null, true);
  @CODE<$LoadManagedLate> context.PublishBind(typeof(HELIX.Boot.Option), null, @target:name);
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

  public struct OptionsRenderEvent : Evt<OptionsRenderEvent> {
    public NavigableProseWriter writer;

    public OptionsRenderEvent(NavigableProseWriter writer) {
      this.writer = writer;
    }

    public void DeclareCategory(string path, string title, string description = null, IconRef icon = default) {
      using (writer.Path(path)) {
        writer.PushModifier(PathSectionModifiers.Title(title));
        if (!string.IsNullOrEmpty(description)) {
          writer.PushModifier(PathSectionModifiers.Description(description));
        }
        if (icon.icon != null) {
          writer.PushModifier(PathSectionModifiers.Icon(icon));
        }
      }
    }
  }

  public struct OptionsLoadPageStateEvent : Evt<OptionsLoadPageStateEvent> {
    public OptionPages pages;

    public OptionsLoadPageStateEvent(OptionPages pages) {
      this.pages = pages;
    }
  }


  public abstract class Option : IProse {
    public string path;
    public string name;
    public string groupPath;
    public OptionEagerness eagerness = OptionEagerness.Immediate;
    public abstract void LoadInto(FormController controller);
    public abstract void AcceptFrom(FormController controller);
    public abstract void LoadData();

    public abstract void SaveData();
    public abstract void ToProse(IProseWriter writer);
  }

  public enum OptionEagerness : int {
    /// <summary>
    /// Option is saved and applied immediately on change.
    /// </summary>
    Immediate = 0,

    /// <summary>
    /// Option is saved and applied when the user presses the "Apply" button.
    /// </summary>
    Delayed = 1,

    /// <summary>
    /// Option is applied on "Apply" but not saved until the user confirms again in a modal after application.
    /// </summary>
    Confirmed = 2
  }

  public class Option<T> : Option {
    public Action<IProseWriter> modifiers;
    public IDatatype<T> datatype;
    public T defaultValue;
    public T value;

    public Option(IDatatype<T> datatype, T defaultValue) {
      this.datatype = datatype;
      this.defaultValue = defaultValue;
      this.value = defaultValue;
    }

    public Option(IDatatype<T> datatype, T defaultValue, Action<IProseWriter> modifiers) {
      this.datatype = datatype;
      this.defaultValue = defaultValue;
      this.value = defaultValue;
      this.modifiers = modifiers;
    }

    public void SetInferredName(string inferred) {
      path ??= inferred;
      name = Casing.Convert(inferred, Casing.CaseStyle.TitleCase);
      path = Casing.Convert(inferred, Casing.CaseStyle.DomainCase);
      SetGroupPath();
    }

    public void SetInferredPath(string inferred) {
      path ??= inferred;
      if (string.IsNullOrEmpty(name)) {
        name = Casing.Convert(inferred, Casing.CaseStyle.TitleCase);
      }
      SetGroupPath();
    }

    private void SetGroupPath() {
      var parts = path.Split(".");
      groupPath = string.Join(".", parts.Take(parts.Length - 1));
    }

    public override void LoadInto(FormController controller) {
      var formPath = controller.Path(path);
      controller.SetValue(formPath, value);
    }

    public override void AcceptFrom(FormController controller) {
      var formPath = controller.Path(path);
      var formValue = controller.GetValue(formPath);
      try {
        value = (T)formValue;
      } catch (Exception e) {
        Debug.LogError($"Failed to accept value '{value}' for option {path} from form: {e}");
      }
    }

    public override void LoadData() {
      if (datatype is IStringConvertible<T> convertible) {
        var exists = PlayerPrefs.HasKey(path);
        if (!exists) {
          value = defaultValue;
        } else {
          var str = PlayerPrefs.GetString(path);
          value = convertible.FromString(str);
        }
        return;
      }

      Debug.LogError($"No string converter for {datatype}");
    }

    public override void SaveData() {
      if (datatype is IStringConvertible<T> convertible) {
        PlayerPrefs.SetString(path, convertible.ToString(value));
        return;
      }

      Debug.LogError($"No string converter for {datatype}");
    }

    public override void ToProse(IProseWriter writer) {
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