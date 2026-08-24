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
        option.Apply();
      }

      Debug.Log("OptionsService loaded and applied all options.");
    }

    [EventHandler]
    private void OnDefaultOptionsRender(OptionsRenderEvent evt) {
      foreach (var option in _options) {
        using (evt.writer.Path(option.groupPath)) {
          option.ToProse(evt.writer);
        }
      }
    }

    [EventHandler]
    private void OnOptionsLoadPageState(OptionsLoadPageStateEvent evt) {
      foreach (var option in _options) {
        option.LoadInto(evt.pages.Form);
      }
    }

    public OptionPages BuildOptionPages(OptionPagesOptions? options = null) {
      var fieldReducer = new ComposeProseFieldReducer().Add(OptionPageFieldFactory.Create);
      var handlers = new ProseReducerChain<Composable>().Add(new ComposeProseFieldHandler(fieldReducer));
      var writer = new NavTreeProseWriter(delegates: handlers);
      new OptionsRenderEvent(writer).Raise();
      var prose = writer.BuildSections();

      // var debugWriter = new ProseTextWriter();
      // debugWriter.Write(prose);
      // Debug.Log(debugWriter.Build());

      var state = new OptionModificationState(_options);
      var pages = new OptionPages(prose, options ?? OptionPagesOptions.Default, state);
      new OptionsLoadPageStateEvent(pages).Raise();
      state.Bind(pages.Form);
      return pages;
    }
  }

  public sealed class OptionModificationState : IOptionPagesState, ISignalObserver {
    private readonly IReadOnlyList<Option> _options;
    private readonly Dictionary<Option, object> _confirmationValues = new();
    private FormController _controller;
    private bool _updating;

    public OptionModificationState(IReadOnlyList<Option> options) =>
      _options = options ?? throw new ArgumentNullException(nameof(options));

    public bool IsDisposed { get; private set; }
    public bool IsDirty {
      get {
        if (_controller == null) return false;
        for (var i = 0; i < _options.Count; i++) {
          var option = _options[i];
          if (option.eagerness != OptionEagerness.Immediate && IsOptionDirty(option)) return true;
        }
        return false;
      }
    }

    public bool IsChanged(string path) => TryGetOption(path, out var option) &&
      option.eagerness >= OptionEagerness.Delayed &&
      IsOptionDirty(option);

    public bool IsNonDefault(string path) => _controller != null && TryGetOption(path, out var option) &&
      !option.IsDefaultValue(
        _controller.GetValue(
          _controller.Path(option.path)
        )
      );

    public void ResetChange(string path) {
      if (!TryGetOption(path, out var option) || option.eagerness < OptionEagerness.Delayed) return;
      _controller.ResetPath(_controller.Path(option.path));
    }

    public void ResetToDefault(string path) {
      if (!TryGetOption(path, out var option)) return;
      option.LoadDefaultInto(_controller);
    }

    public void Bind(FormController controller) {
      if (IsDisposed) throw new ObjectDisposedException(nameof(OptionModificationState));
      if (controller == null) throw new ArgumentNullException(nameof(controller));
      if (ReferenceEquals(_controller, controller)) return;
      _controller?.RemoveObserver(this);
      _controller = controller;
      _controller.AddObserver(this);
      ProcessChanges();
    }

    public void OnSignalChanged(Signal signal) {
      if (ReferenceEquals(signal, _controller)) ProcessChanges();
    }

    public OptionPagesApplyResult Apply() {
      if (_controller == null || !IsDirty) return OptionPagesApplyResult.NothingToApply;
      if (!_controller.Submit().valid) return OptionPagesApplyResult.Invalid;

      var changed = false;
      var needsConfirmation = false;
      _updating = true;
      try {
        for (var i = 0; i < _options.Count; i++) {
          var option = _options[i];
          if (option.eagerness == OptionEagerness.Immediate || !IsOptionDirty(option)) continue;
          if (option.eagerness == OptionEagerness.Confirmed) {
            _confirmationValues[option] = option.CaptureValue();
            needsConfirmation = true;
          }
          option.AcceptFrom(_controller);
          option.Apply();
          if (option.eagerness == OptionEagerness.Delayed) {
            option.SaveData();
            AcceptAsInitial(option);
          }
          changed = true;
        }
        if (changed) new OptionsModifiedEvent().Raise();
        if (changed) PlayerPrefs.Save();
      } finally {
        _updating = false;
      }
      return needsConfirmation ? OptionPagesApplyResult.ConfirmationRequired :
        changed ? OptionPagesApplyResult.Applied : OptionPagesApplyResult.NothingToApply;
    }

    public void Revert() {
      if (_controller == null || _confirmationValues.Count != 0) return;
      _updating = true;
      try {
        for (var i = 0; i < _options.Count; i++) {
          var option = _options[i];
          if (option.eagerness != OptionEagerness.Immediate && IsOptionDirty(option))
            _controller.ResetPath(_controller.Path(option.path));
        }
      } finally { _updating = false; }
    }

    public void Confirm() {
      if (_controller == null || _confirmationValues.Count == 0) return;
      _updating = true;
      try {
        foreach (var pair in _confirmationValues) {
          pair.Key.SaveData();
          AcceptAsInitial(pair.Key);
        }
        _confirmationValues.Clear();
        PlayerPrefs.Save();
      } finally {
        _updating = false;
      }
    }

    public void Reject() {
      if (_controller == null || _confirmationValues.Count == 0) return;
      _updating = true;
      try {
        foreach (var pair in _confirmationValues) {
          pair.Key.RestoreValue(pair.Value);
          pair.Key.LoadInto(_controller);
          pair.Key.Apply();
          AcceptAsInitial(pair.Key);
        }
        _confirmationValues.Clear();
        new OptionsModifiedEvent().Raise();
      } finally {
        _updating = false;
      }
    }

    public void Dispose() {
      if (IsDisposed) return;
      Reject();
      _controller?.RemoveObserver(this);
      _controller = null;
      _confirmationValues.Clear();
      IsDisposed = true;
    }

    private void ProcessChanges() {
      if (_updating || _controller == null) return;
      var changed = false;
      _updating = true;
      try {
        for (var i = 0; i < _options.Count; i++) {
          var option = _options[i];
          if (option.eagerness != OptionEagerness.Immediate || !IsOptionDirty(option)) continue;
          option.AcceptFrom(_controller);
          option.Apply();
          option.SaveData();
          AcceptAsInitial(option);
          changed = true;
        }
        if (changed) {
          PlayerPrefs.Save();
          new OptionsModifiedEvent().Raise();
        }
      } finally {
        _updating = false;
      }
    }

    private bool IsOptionDirty(Option option) => _controller.IsFieldDirty(_controller.Path(option.path));

    private bool TryGetOption(string path, out Option option) {
      for (var i = 0; i < _options.Count; i++) {
        if (_options[i].path != path) continue;
        option = _options[i];
        return true;
      }
      option = null;
      return false;
    }

    private void AcceptAsInitial(Option option) =>
      _controller.AcceptCurrentValuesAsInitial(_controller.Path(option.path));
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
    public readonly NavTreeProseWriter writer;

    public OptionsRenderEvent(NavTreeProseWriter writer) {
      this.writer = writer;
    }

    public void DeclareCategory(string path, string title, string description = null, IconRef icon = default) {
      using (writer.Path(path)) {
        writer.Push(PathSectionModifiers.Title(title));
        if (!string.IsNullOrEmpty(description)) {
          writer.Push(PathSectionModifiers.Description(description));
        }
        if (icon.icon != null) {
          writer.Push(PathSectionModifiers.Icon(icon));
        }
      }
    }
  }

  public struct OptionsLoadPageStateEvent : Evt<OptionsLoadPageStateEvent> {
    public readonly OptionPages pages;

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
    public virtual void Apply() { }
    public virtual object CaptureValue() => null;
    public virtual void RestoreValue(object captured) { }
    public virtual bool IsDefaultValue(object candidate) => true;
    public virtual void LoadDefaultInto(FormController controller) { }
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
    public bool defaultResettable;

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

    public override void Apply() => new OptionApplyEvent<T> { option = this }.Raise();
    public override object CaptureValue() => value;
    public override void RestoreValue(object captured) => value = (T)captured;

    public override bool IsDefaultValue(object candidate) => candidate == null
      ? defaultValue is null
      : candidate is T typed && EqualityComparer<T>.Default.Equals(typed, defaultValue);

    public override void LoadDefaultInto(FormController controller) => controller.SetValue(
      controller.Path(path),
      defaultValue,
      FormChangeReason.User
    );

    public override void ToProse(IProseWriter writer) {
      using (writer.Field(path, name, datatype)) {
        writer.Push(OptionPageFieldModifiers.DefaultReset(defaultResettable));
        modifiers?.Invoke(writer);
      }
    }

    public override string ToString() {
      return
        $"{nameof(path)}: {path}, {nameof(name)}: {name}, {nameof(datatype)}: {datatype}, {nameof(defaultValue)}: {defaultValue}, {nameof(value)}: {value}, {nameof(modifiers)}: {modifiers}";
    }
  }
}