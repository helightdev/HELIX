using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HELIX.Widgets.Elements;
using HELIX.Widgets.Modifiers;
using HELIX.Widgets.Signals;
using HELIX.Widgets.Universal;
using HELIX.Widgets.Universal.Controllers;
using HELIX.Widgets.Universal.Styles;
using UnityEngine;

namespace HELIX.Widgets.Forms {
  public sealed class FormController : Signal {
    public readonly Dictionary<string, object> data = new();
    public readonly Dictionary<string, FieldData> fields = new();

    private bool _submitAttempted;

    public IReadOnlyDictionary<string, object> Data => data;
    public IReadOnlyDictionary<string, FieldData> Fields => fields;
    public bool SubmitAttempted => _submitAttempted;
    public static readonly object NoInitialValue = new FormNoInitialValue();

    public T GetValue<T>(string path, T fallback = default) {
      if (!data.TryGetValue(path, out var value) || value == null) return fallback;
      if (value is T typed) return typed;

      try {
        return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
      } catch {
        return fallback;
      }
    }

    public object GetValue(string path, object fallback = null) {
      return data.TryGetValue(path, out var value) ? value : fallback;
    }

    public void SetValue<T>(
      string path,
      T value,
      FormChangeReason reason = FormChangeReason.Programmatic
    ) {
      data[path] = value;

      if (fields.TryGetValue(path, out var fieldData)) {
        fieldData.flags = SetFlag(fieldData.flags, FieldFlags.Stale, false);
        if (reason == FormChangeReason.User) MarkDirty(path, value);
        if (ShouldValidateOnChange(fieldData)) ValidateField(path, false);
      }

      NotifyFormChanged();
    }

    public FieldData GetFieldData(string path) {
      return fields.TryGetValue(path, out var fieldData) ? fieldData : null;
    }

    public IReadOnlyList<string> GetErrors(string path) {
      return fields.TryGetValue(path, out var fieldData) ? fieldData.errors : Array.Empty<string>();
    }

    public void RegisterField(
      string path,
      IFormField field,
      IEnumerable<IFormValidator> validators = null,
      ValidationMode validationMode = ValidationMode.OnSubmit,
      object initialValue = null,
      IEqualityComparer<object> comparer = null
    ) {
      if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Form field path cannot be empty.", nameof(path));
      if (field == null) throw new ArgumentNullException(nameof(field));

      if (!fields.TryGetValue(path, out var fieldData)) {
        fieldData = new FieldData();
        fields[path] = fieldData;
      } else if (fieldData.field != null
                 && !ReferenceEquals(fieldData.field, field)
                 && (fieldData.flags & FieldFlags.Stale) == 0) {
        throw new InvalidOperationException($"A different form field is already registered for path '{path}'.");
      }

      fieldData.field = field;
      fieldData.validators.Clear();
      if (validators != null) fieldData.validators.AddRange(validators.Where(x => x != null));
      fieldData.validationMode = validationMode;
      fieldData.comparer = comparer ?? ObjectEqualityComparer.Default;
      var hasValue = data.TryGetValue(path, out var currentValue);
      if (!fieldData.hasInitialValue) {
        fieldData.initialValue = hasValue && !ReferenceEquals(initialValue, NoInitialValue) ? currentValue : initialValue;
        fieldData.hasInitialValue = true;
      }

      fieldData.flags = SetFlag(fieldData.flags, FieldFlags.Stale, false);

      if (!ReferenceEquals(initialValue, NoInitialValue) && !hasValue) {
        data[path] = initialValue;
        currentValue = initialValue;
        hasValue = true;
      }

      MarkDirty(path, hasValue ? currentValue : initialValue);
      field.OnFormFieldChanged(this, path);
    }

    public void RegisterListField(
      string path,
      IFormField field,
      IEnumerable<IFormValidator> validators = null,
      ValidationMode validationMode = ValidationMode.OnSubmit,
      IEqualityComparer<object> comparer = null
    ) {
      RegisterField(path, field, validators, validationMode, NoInitialValue, comparer);
      var fieldData = fields[path];
      fieldData.isListField = true;
      if (fieldData.listCount < 0) fieldData.listCount = GetListPathCount(path);
      if (ReferenceEquals(fieldData.initialValue, NoInitialValue)) fieldData.initialValue = fieldData.listCount;
      MarkDirty(path, fieldData.listCount);
    }

    public int GetListCount(string path) {
      if (fields.TryGetValue(path, out var fieldData) && fieldData.isListField) {
        return Math.Max(fieldData.listCount, GetListPathCount(path));
      }

      return GetListPathCount(path);
    }

    private int GetListPathCount(string path) {
      if (string.IsNullOrWhiteSpace(path)) return 0;
      var max = -1;
      foreach (var key in data.Keys.Concat(fields.Keys)) {
        if (TryGetListIndex(path, key, out var index) && index > max) max = index;
      }

      return max + 1;
    }

    public int AppendListItem(string path) {
      var index = GetListCount(path);
      InsertListItem(path, index);
      return index;
    }

    public void InsertListItem(string path, int index) {
      if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
      var count = GetListCount(path);
      if (index > count) throw new ArgumentOutOfRangeException(nameof(index));

      ShiftListPaths(data, path, index, 1);
      ShiftListFields(path, index, 1);
      if (fields.TryGetValue(path, out var fieldData) && fieldData.isListField) fieldData.listCount = count + 1;
      MarkDirty(path, GetListCount(path));
      NotifyFormChanged();
    }

    public void RemoveListItem(string path, int index) {
      if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
      DiscardSubtree(FormPath.Compose(path, index.ToString(CultureInfo.InvariantCulture)));
      ShiftListPaths(data, path, index + 1, -1);
      ShiftListFields(path, index + 1, -1);
      if (fields.TryGetValue(path, out var fieldData) && fieldData.isListField) {
        fieldData.listCount = Math.Max(0, fieldData.listCount - 1);
      }

      MarkDirty(path, GetListCount(path));
      NotifyFormChanged();
    }

    public void MoveListItem(string path, int fromIndex, int toIndex) {
      var count = GetListCount(path);
      if (fromIndex < 0 || fromIndex >= count) throw new ArgumentOutOfRangeException(nameof(fromIndex));
      if (toIndex < 0 || toIndex >= count) throw new ArgumentOutOfRangeException(nameof(toIndex));
      if (fromIndex == toIndex) return;

      MoveListPaths(data, path, fromIndex, toIndex);
      MoveListFields(path, fromIndex, toIndex);
      MarkDirty(path, GetListCount(path));
      NotifyFormChanged();
    }

    public void UnregisterField(string path, IFormField field, bool notify = true) {
      if (!fields.TryGetValue(path, out var fieldData)) return;
      if (!ReferenceEquals(fieldData.field, field)) return;

      fieldData.field = null;
      fieldData.flags |= FieldFlags.Stale;
      if (notify) NotifyFormChanged();
    }

    public void MarkTouched(string path) {
      if (!fields.TryGetValue(path, out var fieldData)) return;
      fieldData.flags |= FieldFlags.Touched;
      NotifyFormChanged();
    }

    public void MarkFinishedEditing(string path) {
      MarkTouched(path);
      if (!fields.TryGetValue(path, out var fieldData)) return;
      if ((fieldData.validationMode & ValidationMode.OnFinishEditing) != 0) ValidateField(path);
    }

    public void DiscardField(string path) {
      fields.Remove(path);
      data.Remove(path);
      NotifyFormChanged();
    }

    public void DiscardSubtree(string prefix) {
      var normalized = NormalizePrefix(prefix);
      foreach (var path in data.Keys.Where(path => IsPathInSubtree(path, normalized)).ToList()) data.Remove(path);
      foreach (var path in fields.Keys.Where(path => IsPathInSubtree(path, normalized)).ToList()) fields.Remove(path);
      NotifyFormChanged();
    }

    public bool ValidateField(string path) {
      return ValidateField(path, true);
    }

    public bool ValidateAll() {
      var valid = true;
      foreach (var path in fields.Keys.ToList()) valid &= ValidateField(path, false);
      NotifyFormChanged();
      return valid;
    }

    public FormSubmitResult Submit() {
      _submitAttempted = true;
      var valid = true;

      foreach (var entry in fields.ToList()) {
        var mode = entry.Value.validationMode;
        if ((mode & ValidationMode.OnSubmit) == 0 && mode != ValidationMode.None) continue;
        valid &= ValidateField(entry.Key, false);
      }

      TryDumpTree(out var tree, out var dumpErrors);
      if (dumpErrors.Count > 0) valid = false;

      NotifyFormChanged();
      return new FormSubmitResult {
        valid = valid,
        data = new Dictionary<string, object>(data),
        tree = valid ? tree : null,
        errors = SnapshotErrors(dumpErrors)
      };
    }

    public void Reset(bool notify = true) {
      foreach (var entry in fields.ToList()) {
        if (!fields.ContainsKey(entry.Key)) continue;
        if (entry.Value.isListField) {
          data.Remove(entry.Key);
          entry.Value.listCount = entry.Value.initialValue is int count ? count : 0;
          RemoveListPathsAtOrAfter(entry.Key, entry.Value.listCount);
          MarkDirty(entry.Key, entry.Value.listCount);
          continue;
        }

        data[entry.Key] = entry.Value.initialValue;
        MarkDirty(entry.Key, entry.Value.initialValue);
      }

      ResetMetadata(fields.Keys);
      _submitAttempted = false;
      if (notify) NotifyFormChanged();
    }

    public void Reset(Dictionary<string, object> values, bool notify = true) {
      data.Clear();
      if (values != null)
        foreach (var entry in values) data[entry.Key] = entry.Value;

      foreach (var entry in fields.ToList()) {
        if (!fields.ContainsKey(entry.Key)) continue;
        if (entry.Value.isListField) {
          data.Remove(entry.Key);
          entry.Value.listCount = GetListPathCount(entry.Key);
          entry.Value.initialValue = entry.Value.listCount;
          entry.Value.hasInitialValue = true;
          continue;
        }

        entry.Value.initialValue = data.TryGetValue(entry.Key, out var value) ? value : null;
        entry.Value.hasInitialValue = true;
        if (!data.ContainsKey(entry.Key)) data[entry.Key] = entry.Value.initialValue;
      }

      ResetMetadata(fields.Keys);
      _submitAttempted = false;
      if (notify) NotifyFormChanged();
    }

    public void ResetField(string path) {
      if (!fields.TryGetValue(path, out var fieldData)) return;
        if (fieldData.isListField) {
          data.Remove(path);
          fieldData.listCount = fieldData.initialValue is int count ? count : 0;
          RemoveListPathsAtOrAfter(path, fieldData.listCount);
          MarkDirty(path, fieldData.listCount);
          ResetMetadata(new[] { path });
          NotifyFormChanged();
          return;
      }

      data[path] = fieldData.initialValue;
      MarkDirty(path, fieldData.initialValue);
      ResetMetadata(new[] { path });
      NotifyFormChanged();
    }

    public void AcceptCurrentValuesAsInitial() {
      foreach (var entry in fields) {
        if (entry.Value.isListField) {
          entry.Value.initialValue = GetListCount(entry.Key);
          entry.Value.hasInitialValue = true;
          MarkDirty(entry.Key, entry.Value.initialValue);
          continue;
        }

        data.TryGetValue(entry.Key, out var value);
        entry.Value.initialValue = value;
        entry.Value.hasInitialValue = true;
        MarkDirty(entry.Key, value);
      }

      NotifyFormChanged();
    }

    public Dictionary<string, object> DumpTree() {
      if (!TryDumpTree(out var tree, out var errors)) {
        throw new InvalidOperationException(string.Join(Environment.NewLine, errors));
      }

      return tree;
    }

    public bool TryDumpTree(out Dictionary<string, object> tree, out List<string> errors) {
      tree = new Dictionary<string, object>();
      errors = new List<string>();

      foreach (var entry in data.OrderBy(x => x.Key.Length).ThenBy(x => x.Key, StringComparer.Ordinal)) {
        if (!FormPath.TryParse(entry.Key, out var segments, out var parseError)) {
          errors.Add(parseError);
          continue;
        }

        InsertTreeValue(tree, entry.Key, segments, entry.Value, errors);
      }

      return errors.Count == 0;
    }

    private bool ValidateField(string path, bool notify) {
      if (!fields.TryGetValue(path, out var fieldData)) return true;

      var value = fieldData.isListField ? GetListCount(path) : GetValue(path);
      fieldData.errors.Clear();
      foreach (var validator in fieldData.validators) {
        var error = validator.Validate(this, path, value);
        if (!string.IsNullOrEmpty(error)) fieldData.errors.Add(error);
      }

      fieldData.flags = SetFlag(fieldData.flags, FieldFlags.Error, fieldData.errors.Count > 0);
      if (notify) NotifyFormChanged();
      return fieldData.errors.Count == 0;
    }

    private bool ShouldValidateOnChange(FieldData fieldData) {
      var mode = fieldData.validationMode;
      if ((mode & ValidationMode.OnChange) != 0) return true;
      return (mode & ValidationMode.OnDirty) != 0 && (fieldData.flags & FieldFlags.Dirty) != 0;
    }

    private void MarkDirty(string path, object value) {
      if (!fields.TryGetValue(path, out var fieldData)) return;
      var dirty = !(fieldData.comparer ?? ObjectEqualityComparer.Default).Equals(fieldData.initialValue, value);
      fieldData.flags = SetFlag(fieldData.flags, FieldFlags.Dirty, dirty);
    }

    private void ResetMetadata(IEnumerable<string> paths) {
      foreach (var path in paths.ToList()) {
        if (!fields.TryGetValue(path, out var fieldData)) continue;
        fieldData.errors.Clear();
        fieldData.flags &= FieldFlags.Stale;
      }
    }

    private IReadOnlyDictionary<string, IReadOnlyList<string>> SnapshotErrors(IReadOnlyList<string> dumpErrors) {
      var result = new Dictionary<string, IReadOnlyList<string>>();
      foreach (var entry in fields) {
        if (entry.Value.errors.Count == 0) continue;
        result[entry.Key] = entry.Value.errors.ToArray();
      }

      if (dumpErrors.Count > 0) result[string.Empty] = dumpErrors.ToArray();
      return result;
    }

    private void NotifyFormChanged() {
      foreach (var entry in fields.ToList()) entry.Value.field?.OnFormFieldChanged(this, entry.Key);
      NotifyDirty();
      NotifyObservers();
    }

    private static FieldFlags SetFlag(FieldFlags flags, FieldFlags flag, bool enabled) {
      return enabled ? flags | flag : flags & ~flag;
    }

    private static string NormalizePrefix(string prefix) {
      return prefix?.Trim('.') ?? string.Empty;
    }

    private static bool IsPathInSubtree(string path, string prefix) {
      if (string.IsNullOrEmpty(prefix)) return true;
      return string.Equals(path, prefix, StringComparison.Ordinal)
             || path.StartsWith(prefix + ".", StringComparison.Ordinal);
    }

    private static void ShiftListPaths(Dictionary<string, object> target, string listPath, int startIndex, int delta) {
      var changes = new List<(string OldPath, string NewPath, object Value)>();
      foreach (var entry in target.ToList()) {
        if (!TryGetListIndex(listPath, entry.Key, out var index) || index < startIndex) continue;
        changes.Add((entry.Key, ReplaceListIndex(listPath, entry.Key, index + delta), entry.Value));
      }

      foreach (var change in changes) target.Remove(change.OldPath);
      foreach (var change in changes) target[change.NewPath] = change.Value;
    }

    private static void MoveListPaths(Dictionary<string, object> target, string listPath, int fromIndex, int toIndex) {
      var changes = new List<(string OldPath, string NewPath, object Value)>();
      foreach (var entry in target.ToList()) {
        if (!TryGetListIndex(listPath, entry.Key, out var index)) continue;
        if (!TryMoveListIndex(index, fromIndex, toIndex, out var movedIndex)) continue;
        changes.Add((entry.Key, ReplaceListIndex(listPath, entry.Key, movedIndex), entry.Value));
      }

      foreach (var change in changes) target.Remove(change.OldPath);
      foreach (var change in changes) target[change.NewPath] = change.Value;
    }

    private void RemoveListPathsAtOrAfter(string listPath, int startIndex) {
      foreach (var entry in data.Keys.ToList()) {
        if (TryGetListIndex(listPath, entry, out var index) && index >= startIndex) data.Remove(entry);
      }

      foreach (var entry in fields.Keys.ToList()) {
        if (TryGetListIndex(listPath, entry, out var index) && index >= startIndex) fields.Remove(entry);
      }
    }

    private void ShiftListFields(string listPath, int startIndex, int delta) {
      var changes = new List<(string OldPath, string NewPath, FieldData Value)>();
      foreach (var entry in fields.ToList()) {
        if (!TryGetListIndex(listPath, entry.Key, out var index) || index < startIndex) continue;
        changes.Add((entry.Key, ReplaceListIndex(listPath, entry.Key, index + delta), entry.Value.Detached()));
      }

      foreach (var change in changes) fields.Remove(change.OldPath);
      foreach (var change in changes) fields[change.NewPath] = change.Value;
    }

    private void MoveListFields(string listPath, int fromIndex, int toIndex) {
      var changes = new List<(string OldPath, string NewPath, FieldData Value)>();
      foreach (var entry in fields.ToList()) {
        if (!TryGetListIndex(listPath, entry.Key, out var index)) continue;
        if (!TryMoveListIndex(index, fromIndex, toIndex, out var movedIndex)) continue;
        changes.Add((entry.Key, ReplaceListIndex(listPath, entry.Key, movedIndex), entry.Value.Detached()));
      }

      foreach (var change in changes) fields.Remove(change.OldPath);
      foreach (var change in changes) fields[change.NewPath] = change.Value;
    }

    private static bool TryMoveListIndex(int index, int fromIndex, int toIndex, out int movedIndex) {
      movedIndex = index;
      if (index == fromIndex) {
        movedIndex = toIndex;
        return true;
      }

      if (fromIndex < toIndex && index > fromIndex && index <= toIndex) {
        movedIndex = index - 1;
        return true;
      }

      if (fromIndex > toIndex && index >= toIndex && index < fromIndex) {
        movedIndex = index + 1;
        return true;
      }

      return false;
    }

    private static bool TryGetListIndex(string listPath, string path, out int index) {
      index = -1;
      if (string.IsNullOrEmpty(listPath) || !path.StartsWith(listPath + ".", StringComparison.Ordinal)) return false;

      var start = listPath.Length + 1;
      var end = path.IndexOf('.', start);
      if (end < 0) end = path.Length;
      return int.TryParse(path.Substring(start, end - start), NumberStyles.None, CultureInfo.InvariantCulture, out index);
    }

    private static string ReplaceListIndex(string listPath, string path, int index) {
      var start = listPath.Length + 1;
      var end = path.IndexOf('.', start);
      if (end < 0) end = path.Length;
      return path.Substring(0, start)
             + index.ToString(CultureInfo.InvariantCulture)
             + path.Substring(end);
    }

    private static void InsertTreeValue(
      Dictionary<string, object> root,
      string path,
      IReadOnlyList<FormPathSegment> segments,
      object value,
      List<string> errors
    ) {
      object current = root;
      for (var i = 0; i < segments.Count; i++) {
        var segment = segments[i];
        var isLeaf = i == segments.Count - 1;
        var nextIsIndex = !isLeaf && segments[i + 1].IsIndex;

        if (segment.IsIndex) {
          errors.Add($"Path '{path}' cannot start with or assign through a list index without a list parent.");
          return;
        }

        var dict = current as Dictionary<string, object>;
        if (dict == null) {
          errors.Add($"Path '{path}' conflicts with an existing list value at '{segment.Name}'.");
          return;
        }

        if (isLeaf) {
          if (dict.TryGetValue(segment.Name, out var existing) && IsContainer(existing)) {
            errors.Add($"Path '{path}' conflicts with an existing container at '{segment.Name}'.");
            return;
          }

          dict[segment.Name] = value;
          return;
        }

        if (!dict.TryGetValue(segment.Name, out var child)) {
          child = nextIsIndex ? new List<object>() : new Dictionary<string, object>();
          dict[segment.Name] = child;
        } else if (!IsExpectedContainer(child, nextIsIndex)) {
          errors.Add($"Path '{path}' conflicts with an existing scalar at '{segment.Name}'.");
          return;
        }

        current = WalkContainer(child, segments, ref i, path, value, errors);
        if (current == null) return;
      }
    }

    private static object WalkContainer(
      object current,
      IReadOnlyList<FormPathSegment> segments,
      ref int index,
      string path,
      object value,
      List<string> errors
    ) {
      while (index + 1 < segments.Count && segments[index + 1].IsIndex) {
        index++;
        var segment = segments[index];
        var list = current as List<object>;
        if (list == null) {
          errors.Add($"Path '{path}' expected a list before index [{segment.Index}].");
          return null;
        }

        while (list.Count <= segment.Index) list.Add(null);
        var isLeaf = index == segments.Count - 1;
        if (isLeaf) {
          if (IsContainer(list[segment.Index])) {
            errors.Add($"Path '{path}' conflicts with an existing container at index [{segment.Index}].");
            return null;
          }

          list[segment.Index] = value;
          return null;
        }

        var nextIsIndex = segments[index + 1].IsIndex;
        var child = list[segment.Index];
        if (child == null) {
          child = nextIsIndex ? new List<object>() : new Dictionary<string, object>();
          list[segment.Index] = child;
        } else if (!IsExpectedContainer(child, nextIsIndex)) {
          errors.Add($"Path '{path}' conflicts with an existing scalar at index [{segment.Index}].");
          return null;
        }

        current = child;
      }

      return current;
    }

    private static bool IsExpectedContainer(object value, bool list) {
      return list ? value is List<object> : value is Dictionary<string, object>;
    }

    private static bool IsContainer(object value) {
      return value is Dictionary<string, object> || value is List<object>;
    }
  }

  public sealed class FieldData {
    public IFormField field;
    public FieldFlags flags;
    public readonly List<string> errors = new();
    public readonly List<IFormValidator> validators = new();
    public ValidationMode validationMode;
    public object initialValue;
    public IEqualityComparer<object> comparer = ObjectEqualityComparer.Default;
    public bool isListField;
    public int listCount = -1;
    public bool hasInitialValue;

    public FieldData Detached() {
      var detached = new FieldData {
        flags = flags | FieldFlags.Stale,
        validationMode = validationMode,
        initialValue = initialValue,
        comparer = comparer,
        isListField = isListField,
        listCount = listCount,
        hasInitialValue = hasInitialValue
      };
      detached.errors.AddRange(errors);
      detached.validators.AddRange(validators);
      return detached;
    }
  }

  public sealed class FormNoInitialValue { }

  [Flags]
  public enum FieldFlags : byte {
    None = 0,
    Touched = 1 << 0,
    Dirty = 1 << 1,
    Error = 1 << 2,
    Stale = 1 << 3
  }

  [Flags]
  public enum ValidationMode {
    None = 0,
    OnChange = 1 << 0,
    OnDirty = 1 << 1,
    OnFinishEditing = 1 << 2,
    OnSubmit = 1 << 3
  }

  public enum FormChangeReason {
    Programmatic,
    User
  }

  public interface IFormField {
    void OnFormFieldChanged(FormController form, string path);
  }

  public interface IFormValidator {
    string Validate(FormController form, string path, object value);
  }

  public sealed class FormSubmitResult {
    public bool valid;
    public Dictionary<string, object> data;
    public Dictionary<string, object> tree;
    public IReadOnlyDictionary<string, IReadOnlyList<string>> errors;
  }

  public static class FormValidators {
    public static IFormValidator Required(string message = "Required") {
      return new FuncFormValidator((_, _, value) => {
        if (value == null) return message;
        return value is string text && string.IsNullOrWhiteSpace(text) ? message : null;
      });
    }

    public static IFormValidator MinLength(int min, string message = null) {
      return new FuncFormValidator((_, _, value) => {
        var text = value as string ?? string.Empty;
        return text.Length < min ? message ?? $"Must be at least {min} characters" : null;
      });
    }

    public static IFormValidator MaxLength(int max, string message = null) {
      return new FuncFormValidator((_, _, value) => {
        var text = value as string ?? string.Empty;
        return text.Length > max ? message ?? $"Must be at most {max} characters" : null;
      });
    }

    public static IFormValidator Func(Func<FormController, string, object, string> validate) {
      return new FuncFormValidator(validate);
    }
  }

  public sealed class FuncFormValidator : IFormValidator {
    private readonly Func<FormController, string, object, string> _validate;

    public FuncFormValidator(Func<FormController, string, object, string> validate) {
      _validate = validate ?? throw new ArgumentNullException(nameof(validate));
    }

    public string Validate(FormController form, string path, object value) {
      return _validate(form, path, value);
    }
  }

  public sealed class HForm : SingleChildStatefulWidget<HForm> {
    public readonly FormController controller;

    public HForm(
      FormController controller = null,
      Widget child = null,
      Key key = default,
      object[] constants = null,
      IReadOnlyCollection<Modifier> modifiers = null
    ) : base(child, key, constants, modifiers) {
      this.controller = controller;
    }

    public override State<HForm> CreateState() {
      return new HFormState();
    }
  }

  public sealed class HFormState : State<HForm> {
    private FormController _ownedController;

    public FormController Controller => widget.controller ?? _ownedController;

    public override void InitState() {
      base.InitState();
      if (widget.controller == null) _ownedController = AddDisposable(new FormController());
    }

    public override void DidUpdateWidget(HForm oldWidget) {
      if (widget.controller == null && _ownedController == null) _ownedController = AddDisposable(new FormController());
    }

    public override Widget Build(BuildContext context) {
      return new HFormContext(Controller, string.Empty, widget.child);
    }
  }

  public sealed class HFormScope : StatelessWidget<HFormScope>, IEnumerable<Widget> {
    public readonly string prefix;
    public Widget child;

    public HFormScope(
      string prefix,
      Widget child = null,
      Key key = default,
      object[] constants = null,
      IReadOnlyCollection<Modifier> modifiers = null
    ) : base(key, constants, modifiers) {
      this.prefix = prefix;
      this.child = child;
    }

    public override Widget Build(BuildContext context) {
      var formContext = FormContext.Resolve(context);
      if (formContext == null) throw new InvalidOperationException("HFormScope must be built below an HForm.");
      return new HFormContext(
        formContext.Controller,
        FormPath.Compose(formContext.PathPrefix, prefix),
        child
      );
    }

    public IEnumerator<Widget> GetEnumerator() {
      if (child != null) yield return child;
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }

    public void Add(Widget widget) {
      if (child != null) throw new InvalidOperationException("HFormScope can only have one child.");
      child = widget;
    }
  }

  public sealed class HFormListField : SingleChildStatefulWidget<HFormListField> {
    public readonly string path;
    public readonly IEnumerable<IFormValidator> validators;
    public readonly ValidationMode validationMode;
    public readonly IEqualityComparer<object> comparer;

    public HFormListField(
      string path,
      IEnumerable<IFormValidator> validators = null,
      ValidationMode validationMode = ValidationMode.OnSubmit,
      IEqualityComparer<object> comparer = null,
      Widget child = null,
      Key key = default,
      object[] constants = null,
      IReadOnlyCollection<Modifier> modifiers = null
    ) : base(child, key, constants, modifiers) {
      this.path = path;
      this.validators = validators;
      this.validationMode = validationMode;
      this.comparer = comparer;
    }

    public override State<HFormListField> CreateState() {
      return new HFormListFieldState();
    }
  }

  public sealed class HFormListFieldState : State<HFormListField>, IFormField {
    private FormController _form;
    private string _path;
    private bool _disposed;

    public override void InitState() {
      base.InitState();
      ResolveAndRegister();
    }

    public override void DidUpdateWidget(HFormListField oldWidget) {
      ResolveAndRegister();
    }

    public override bool CanReconcile(HFormListField oldWidget) {
      return base.CanReconcile(oldWidget) && widget.path == oldWidget.path;
    }

    public override void Dispose() {
      _disposed = true;
      _form?.UnregisterField(_path, this, false);
      _form = null;
      _path = null;
      base.Dispose();
    }

    public void OnFormFieldChanged(FormController form, string path) { }

    public override Widget Build(BuildContext context) {
      ResolveAndRegister();
      var formContext = FormContext.Resolve(context);
      if (formContext == null) throw new InvalidOperationException("HFormListField must be built below an HForm.");
      return new HFormContext(
        formContext.Controller,
        FormPath.Compose(formContext.PathPrefix, widget.path),
        widget.child
      );
    }

    private void ResolveAndRegister() {
      if (_disposed) return;
      var formContext = FormContext.Resolve(mount);
      if (formContext == null) throw new InvalidOperationException("HFormListField must be built below an HForm.");

      var fullPath = FormPath.Compose(formContext.PathPrefix, widget.path);
      if (ReferenceEquals(_form, formContext.Controller) && _path == fullPath) {
        _form.RegisterListField(_path, this, widget.validators, widget.validationMode, widget.comparer);
        return;
      }

      _form?.UnregisterField(_path, this);
      _form = formContext.Controller;
      _path = fullPath;
      _form.RegisterListField(_path, this, widget.validators, widget.validationMode, widget.comparer);
    }
  }

  public sealed class HFormTextField : StatefulWidget<HFormTextField> {
    public readonly string path;
    public readonly IEnumerable<IFormValidator> validators;
    public readonly ValidationMode validationMode;
    public readonly string initialValue;
    public readonly IEqualityComparer<object> comparer;
    public readonly Key focusKey;
    public readonly HTextFieldStyle style;
    public readonly bool multiline;
    public readonly bool autocorrect;
    public readonly bool isReadOnly;
    public readonly bool isPasswordField;
    public readonly bool isDelayed;
    public readonly bool hideMobileInput;
    public readonly TouchScreenKeyboardType keyboardType;
    public readonly char maskChar;
    public readonly int maxLength;
    public readonly bool enabled;
    public readonly Action<string> onChanged;
    public readonly Action<string> onSubmitted;

    public HFormTextField(
      string path,
      IEnumerable<IFormValidator> validators = null,
      ValidationMode validationMode = ValidationMode.OnSubmit,
      string initialValue = "",
      IEqualityComparer<object> comparer = null,
      Key focusKey = default,
      HTextFieldStyle style = null,
      bool multiline = false,
      bool autocorrect = true,
      bool isReadOnly = false,
      bool isPasswordField = false,
      bool isDelayed = false,
      bool hideMobileInput = true,
      TouchScreenKeyboardType keyboardType = TouchScreenKeyboardType.Default,
      char maskChar = '*',
      int maxLength = -1,
      bool enabled = true,
      Action<string> onChanged = null,
      Action<string> onSubmitted = null,
      Key key = default,
      object[] constants = null,
      IReadOnlyCollection<Modifier> modifiers = null
    ) : base(key, constants, modifiers) {
      this.path = path;
      this.validators = validators;
      this.validationMode = validationMode;
      this.initialValue = initialValue;
      this.comparer = comparer;
      this.focusKey = focusKey;
      this.style = style;
      this.multiline = multiline;
      this.autocorrect = autocorrect;
      this.isReadOnly = isReadOnly;
      this.isPasswordField = isPasswordField;
      this.isDelayed = isDelayed;
      this.hideMobileInput = hideMobileInput;
      this.keyboardType = keyboardType;
      this.maskChar = maskChar;
      this.maxLength = maxLength;
      this.enabled = enabled;
      this.onChanged = onChanged;
      this.onSubmitted = onSubmitted;
    }

    public override State<HFormTextField> CreateState() {
      return new HFormTextFieldState();
    }
  }

  public sealed class HFormTextFieldState : State<HFormTextField>, IFormField {
    private TextEditingController _controller;
    private FormController _form;
    private string _path;
    private bool _syncingFromForm;
    private bool _disposed;

    public override void InitState() {
      base.InitState();
      _controller = AddDisposable(new TextEditingController());
      _controller.onChanged += OnChanged;
      _controller.onEndEditing += OnFinishedEditing;
      _controller.onSubmitted += OnSubmitted;
      ResolveAndRegister();
    }

    public override void DidUpdateWidget(HFormTextField oldWidget) {
      ResolveAndRegister();
    }

    public override bool CanReconcile(HFormTextField oldWidget) {
      return base.CanReconcile(oldWidget) && widget.path == oldWidget.path;
    }

    public override void Dispose() {
      _disposed = true;
      if (_controller != null) {
        _controller.onChanged -= OnChanged;
        _controller.onEndEditing -= OnFinishedEditing;
        _controller.onSubmitted -= OnSubmitted;
      }

      _form?.UnregisterField(_path, this, false);
      _form = null;
      _path = null;
      base.Dispose();
    }

    public void OnFormFieldChanged(FormController form, string path) {
      if (_disposed || _controller == null || !ReferenceEquals(form, _form) || path != _path) return;
      var value = form.GetValue<string>(path, widget.initialValue ?? string.Empty) ?? string.Empty;
      if (string.Equals(_controller.PeekValue(), value, StringComparison.InvariantCulture)) return;

      _syncingFromForm = true;
      try {
        _controller.SetValue(value);
      } finally {
        _syncingFromForm = false;
      }
    }

    public override Widget Build(BuildContext context) {
      ResolveAndRegister();
      return new HTextField(
        controller: _controller,
        focusKey: widget.focusKey,
        style: widget.style,
        multiline: widget.multiline,
        autocorrect: widget.autocorrect,
        isReadOnly: widget.isReadOnly,
        isPasswordField: widget.isPasswordField,
        isDelayed: widget.isDelayed,
        hideMobileInput: widget.hideMobileInput,
        keyboardType: widget.keyboardType,
        maskChar: widget.maskChar,
        maxLength: widget.maxLength,
        enabled: widget.enabled
      );
    }

    private void ResolveAndRegister() {
      if (_disposed) return;
      var formContext = FormContext.Resolve(mount);
      if (formContext == null) throw new InvalidOperationException("HFormTextField must be built below an HForm.");

      var fullPath = FormPath.Compose(formContext.PathPrefix, widget.path);
      if (ReferenceEquals(_form, formContext.Controller) && _path == fullPath) {
        _form.RegisterField(_path, this, widget.validators, widget.validationMode, widget.initialValue, widget.comparer);
        return;
      }

      _form?.UnregisterField(_path, this);
      _form = formContext.Controller;
      _path = fullPath;
      _form.RegisterField(_path, this, widget.validators, widget.validationMode, widget.initialValue, widget.comparer);
    }

    private void OnChanged(string value) {
      if (_disposed || _syncingFromForm || _form == null || _path == null) return;
      _form.SetValue(_path, value, FormChangeReason.User);
      widget.onChanged?.Invoke(value);
    }

    private void OnSubmitted(string value) {
      if (_disposed) return;
      _form?.MarkFinishedEditing(_path);
      widget.onSubmitted?.Invoke(value);
    }

    private void OnFinishedEditing() {
      if (_disposed) return;
      _form?.MarkFinishedEditing(_path);
    }
  }

  public sealed class FormContext {
    public readonly FormController Controller;
    public readonly string PathPrefix;

    private FormContext(FormController controller, string pathPrefix) {
      Controller = controller ?? throw new ArgumentNullException(nameof(controller));
      PathPrefix = pathPrefix ?? string.Empty;
    }

    public static FormContext Resolve(BuildContext context) {
      return BuildContext.TryFindParent<FormContextElement>(context, out var element)
        ? new FormContext(element.Controller, element.PathPrefix)
        : null;
    }
  }

  public sealed class HFormContext : SingleChildWidget {
    public readonly FormController controller;
    public readonly string pathPrefix;

    public HFormContext(
      FormController controller,
      string pathPrefix,
      Widget child = null,
      Key key = default,
      object[] constants = null,
      IReadOnlyCollection<Modifier> modifiers = null
    ) : base(child, key, constants, modifiers) {
      this.controller = controller ?? throw new ArgumentNullException(nameof(controller));
      this.pathPrefix = pathPrefix ?? string.Empty;
    }

    public override IWidgetElement CreateElement() {
      return ReconcileInto(new FormContextElement());
    }
  }

  public sealed class FormContextElement : SingleChildWidgetBaseElement<HFormContext> {
    public FormController Controller { get; private set; }
    public string PathPrefix { get; private set; } = string.Empty;

    public override void Apply(HFormContext previous, HFormContext widget) {
      Controller = widget.controller;
      PathPrefix = widget.pathPrefix ?? string.Empty;
    }
  }

  public static class FormPath {
    public static string Compose(string prefix, string path) {
      prefix = prefix?.Trim('.') ?? string.Empty;
      path = path?.Trim('.') ?? string.Empty;

      if (string.IsNullOrEmpty(prefix)) return path;
      if (string.IsNullOrEmpty(path)) return prefix;
      return prefix + "." + path;
    }

    public static bool TryParse(string path, out List<FormPathSegment> segments, out string error) {
      var parsed = new List<FormPathSegment>();
      segments = parsed;
      error = null;

      if (string.IsNullOrWhiteSpace(path)) {
        error = "Form path cannot be empty.";
        return false;
      }

      var token = string.Empty;
      for (var i = 0; i < path.Length; i++) {
        var c = path[i];
        if (c == '.') {
          if (!string.IsNullOrEmpty(token)) {
            parsed.Add(ParseToken(token));
            token = string.Empty;
          }

          continue;
        }

        token += c;
      }

      if (!string.IsNullOrEmpty(token)) parsed.Add(ParseToken(token));
      return parsed.Count > 0;
    }

    private static FormPathSegment ParseToken(string token) {
      return int.TryParse(token, NumberStyles.None, CultureInfo.InvariantCulture, out var index) && index >= 0
        ? FormPathSegment.FromIndex(index)
        : FormPathSegment.FromName(token);
    }
  }

  public readonly struct FormPathSegment {
    public readonly string Name;
    public readonly int Index;
    public readonly bool IsIndex;

    private FormPathSegment(string name, int index, bool isIndex) {
      Name = name;
      Index = index;
      IsIndex = isIndex;
    }

    public static FormPathSegment FromName(string name) {
      return new FormPathSegment(name, -1, false);
    }

    public static FormPathSegment FromIndex(int index) {
      return new FormPathSegment(null, index, true);
    }
  }

  public sealed class ObjectEqualityComparer : IEqualityComparer<object> {
    public static readonly ObjectEqualityComparer Default = new();

    public new bool Equals(object x, object y) {
      return object.Equals(x, y);
    }

    public int GetHashCode(object obj) {
      return obj?.GetHashCode() ?? 0;
    }
  }
}
