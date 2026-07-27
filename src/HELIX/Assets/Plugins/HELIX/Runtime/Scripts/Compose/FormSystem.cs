using System;
using System.Collections.Generic;
using System.Globalization;
using HELIX.Signals;
using HELIX.Widgets.Signals;

namespace HELIX.Compose {
  [Flags]
  public enum FieldFlags : byte {
    None = 0,
    Touched = 1 << 0,
    Dirty = 1 << 1,
    Error = 1 << 2,
    Stale = 1 << 3,
    Disabled = 1 << 4
  }

  [Flags]
  public enum ValidationMode : byte {
    None = 0,
    OnChange = 1 << 0,
    OnDirty = 1 << 1,
    OnFinishEditing = 1 << 2,
    OnSubmit = 1 << 3
  }

  public enum FormChangeReason : byte {
    Programmatic,
    User
  }

  public interface IFormField {
    void OnFormFieldChanged(FormController form, string path);
  }

  public interface IFormValidator {
    string Validate(FormController form, string path, object value);
  }

  public sealed class FieldData {
    internal IReadOnlyList<IFormValidator> validatorsSource;
    internal List<IFormField> observers;

    public IFormField field;
    public FieldFlags flags;
    public readonly List<string> errors = new(2);
    public readonly List<IFormValidator> validators = new(2);
    public ValidationMode validationMode;
    public object initialValue;
    public IEqualityComparer<object> comparer = ObjectFormValueComparer.Default;
    public bool hasInitialValue;
    public bool enabled = true;
    public bool isListField;
    public int listCount = -1;
    public int initialListCount = -1;
    public int listRevision;

    public bool HasFlag(FieldFlags flag) => (flags & flag) != 0;
  }

  public sealed class FormValidationResult {
    public bool valid;
    public IReadOnlyDictionary<string, IReadOnlyList<string>> errors;
  }

  public sealed class FormSubmitResult {
    public bool valid;
    public Dictionary<string, object> data;
    public Dictionary<string, object> tree;
    public IReadOnlyDictionary<string, IReadOnlyList<string>> errors;
  }

  public sealed class ObjectFormValueComparer : IEqualityComparer<object> {
    public static readonly ObjectFormValueComparer Default = new();

    private ObjectFormValueComparer() { }
    public new bool Equals(object x, object y) => object.Equals(x, y);
    public int GetHashCode(object obj) => obj?.GetHashCode() ?? 0;
  }

  public static class FormValidators {
    public static IFormValidator Required(string message = "Required") {
      return new DelegateFormValidator((_, _, value) => {
        if (value == null) return message;
        return value is string text && string.IsNullOrWhiteSpace(text) ? message : null;
      });
    }

    public static IFormValidator MinLength(int min, string message = null) {
      return new DelegateFormValidator((_, _, value) => {
        var text = value as string ?? string.Empty;
        return text.Length < min ? message ?? $"Must be at least {min} characters" : null;
      });
    }

    public static IFormValidator MaxLength(int max, string message = null) {
      return new DelegateFormValidator((_, _, value) => {
        var text = value as string ?? string.Empty;
        return text.Length > max ? message ?? $"Must be at most {max} characters" : null;
      });
    }

    public static IFormValidator Range(float min, float max, string message = null) {
      return new DelegateFormValidator((_, _, value) => {
        if (value == null) return null;
        float numeric;
        try {
          numeric = Convert.ToSingle(value, CultureInfo.InvariantCulture);
        } catch {
          return message ?? $"Must be between {min} and {max}";
        }
        return numeric < min || numeric > max ? message ?? $"Must be between {min} and {max}" : null;
      });
    }

    public static IFormValidator Func(Func<FormController, string, object, string> validator) {
      return new DelegateFormValidator(validator);
    }
  }

  public sealed class DelegateFormValidator : IFormValidator {
    private readonly Func<FormController, string, object, string> _validator;

    public DelegateFormValidator(Func<FormController, string, object, string> validator) {
      _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    public string Validate(FormController form, string path, object value) {
      return _validator(form, path, value);
    }
  }

  public static class FormPath {
    public static string Normalize(string path) {
      if (string.IsNullOrEmpty(path)) return string.Empty;
      var start = 0;
      var end = path.Length;
      while (start < end && path[start] == '.') start++;
      while (end > start && path[end - 1] == '.') end--;
      return start == 0 && end == path.Length ? path : path.Substring(start, end - start);
    }

    public static string Require(string path) {
      var normalized = Normalize(path);
      if (string.IsNullOrWhiteSpace(normalized)) {
        throw new ArgumentException("Form path cannot be empty.", nameof(path));
      }
      return normalized;
    }

    public static string Compose(string prefix, string path) {
      prefix = Normalize(prefix);
      path = Normalize(path);
      if (prefix.Length == 0) return path;
      if (path.Length == 0) return prefix;
      return prefix + "." + path;
    }

    public static string Item(string listPath, int index) {
      if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
      return Compose(Require(listPath), index.ToString(CultureInfo.InvariantCulture));
    }

    public static bool IsInSubtree(string path, string prefix) {
      path = Normalize(path);
      prefix = Normalize(prefix);
      if (prefix.Length == 0) return true;
      if (string.Equals(path, prefix, StringComparison.Ordinal)) return true;
      return path.Length > prefix.Length &&
             path[prefix.Length] == '.' &&
             path.StartsWith(prefix, StringComparison.Ordinal);
    }

    internal static bool TryParse(string path, List<FormPathSegment> output, out string error) {
      output.Clear();
      error = null;
      path = Normalize(path);
      if (path.Length == 0) {
        error = "Form path cannot be empty.";
        return false;
      }

      var segmentStart = 0;
      for (var i = 0; i <= path.Length; i++) {
        if (i != path.Length && path[i] != '.') continue;
        if (i == segmentStart) {
          error = $"Form path '{path}' contains an empty segment.";
          return false;
        }

        var token = path.Substring(segmentStart, i - segmentStart);
        if (int.TryParse(token, NumberStyles.None, CultureInfo.InvariantCulture, out var index)) {
          output.Add(FormPathSegment.Indexed(index));
        } else {
          output.Add(FormPathSegment.Named(token));
        }
        segmentStart = i + 1;
      }
      return true;
    }

    public static bool TryGetListIndex(string listPath, string path, out int index) {
      index = -1;
      listPath = Normalize(listPath);
      path = Normalize(path);
      if (listPath.Length == 0 ||
          path.Length <= listPath.Length + 1 ||
          path[listPath.Length] != '.' ||
          !path.StartsWith(listPath, StringComparison.Ordinal)) {
        return false;
      }

      var start = listPath.Length + 1;
      var end = path.IndexOf('.', start);
      if (end < 0) end = path.Length;
      return int.TryParse(
        path.Substring(start, end - start),
        NumberStyles.None,
        CultureInfo.InvariantCulture,
        out index
      );
    }

    public static string ReplaceListIndex(string listPath, string path, int index) {
      listPath = Require(listPath);
      path = Require(path);
      if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
      var start = listPath.Length + 1;
      var end = path.IndexOf('.', start);
      if (end < 0) end = path.Length;
      return path.Substring(0, start) +
             index.ToString(CultureInfo.InvariantCulture) +
             path.Substring(end);
    }
  }

  internal readonly struct FormPathSegment {
    public readonly string name;
    public readonly int index;
    public readonly bool isIndex;

    private FormPathSegment(string name, int index, bool isIndex) {
      this.name = name;
      this.index = index;
      this.isIndex = isIndex;
    }

    public static FormPathSegment Named(string name) => new(name, -1, false);
    public static FormPathSegment Indexed(int index) => new(null, index, true);
  }

  public sealed class FormController : Signal {
    private readonly Dictionary<string, object> _data = new(StringComparer.Ordinal);
    private readonly Dictionary<string, FieldData> _fields = new(StringComparer.Ordinal);
    private readonly List<(IFormField Field, string Path)> _notificationBuffer = new(8);
    private readonly List<(string Path, int Count)> _listResetBuffer = new(2);
    private readonly List<string> _pathBuffer = new(8);
    private readonly List<FormPathSegment> _segmentBuffer = new(8);

    private bool _submitAttempted;
    private bool _pendingNotify;
    private int _batchDepth;

    public IReadOnlyDictionary<string, object> Data => _data;
    public IReadOnlyDictionary<string, FieldData> Fields => _fields;
    public bool SubmitAttempted => _submitAttempted;

    public bool IsDirty {
      get {
        foreach (var field in _fields.Values) {
          if (field.HasFlag(FieldFlags.Dirty)) return true;
        }
        return false;
      }
    }

    public bool IsTouched {
      get {
        foreach (var field in _fields.Values) {
          if (field.HasFlag(FieldFlags.Touched)) return true;
        }
        return false;
      }
    }

    public bool HasErrors {
      get {
        foreach (var field in _fields.Values) {
          if (field.errors.Count > 0) return true;
        }
        return false;
      }
    }

    public FormUpdateScope BeginUpdate() {
      _batchDepth++;
      return new FormUpdateScope(this);
    }

    public bool HasValue(string path) => _data.ContainsKey(FormPath.Normalize(path));

    public bool TryGetValue<T>(string path, out T value) {
      if (_data.TryGetValue(FormPath.Normalize(path), out var boxed) && boxed is T typed) {
        value = typed;
        return true;
      }
      value = default;
      return false;
    }

    public T GetValue<T>(string path, T fallback = default) {
      if (!_data.TryGetValue(FormPath.Normalize(path), out var boxed) || boxed == null) return fallback;
      if (boxed is T typed) return typed;
      try {
        return (T)Convert.ChangeType(boxed, typeof(T), CultureInfo.InvariantCulture);
      } catch {
        return fallback;
      }
    }

    public object GetValue(string path, object fallback = null) {
      return _data.TryGetValue(FormPath.Normalize(path), out var value) ? value : fallback;
    }

    public FieldData GetFieldData(string path) {
      return _fields.TryGetValue(FormPath.Normalize(path), out var field) ? field : null;
    }

    public IReadOnlyList<string> GetErrors(string path) {
      return GetFieldData(path)?.errors ?? (IReadOnlyList<string>)Array.Empty<string>();
    }

    public IReadOnlyDictionary<string, IReadOnlyList<string>> GetErrorMap() {
      return SnapshotErrors(Array.Empty<string>());
    }

    public IReadOnlyList<string> GetFieldPaths(string prefix = null) {
      var normalized = FormPath.Normalize(prefix);
      var paths = new List<string>();
      foreach (var path in _fields.Keys) {
        if (FormPath.IsInSubtree(path, normalized)) paths.Add(path);
      }
      paths.Sort(StringComparer.Ordinal);
      return paths;
    }

    public void SetValue<T>(
      string path,
      T value,
      FormChangeReason reason = FormChangeReason.Programmatic
    ) {
      var normalized = FormPath.Require(path);
      if (_data.TryGetValue(normalized, out var previous) && object.Equals(previous, value)) return;
      _data[normalized] = value;
      if (_fields.TryGetValue(normalized, out var field)) {
        if (field.field != null) field.flags &= ~FieldFlags.Stale;
        if (reason == FormChangeReason.User) UpdateDirty(field, value);
        if (ShouldValidateOnChange(field)) ValidateFieldCore(normalized, field);
      }
      NotifyFormChanged();
    }

    public void SetValues(
      IReadOnlyDictionary<string, object> values,
      FormChangeReason reason = FormChangeReason.Programmatic
    ) {
      if (values == null) throw new ArgumentNullException(nameof(values));
      using (BeginUpdate()) {
        foreach (var entry in values) SetValue(entry.Key, entry.Value, reason);
      }
    }

    public void RegisterField(
      string path,
      IFormField field,
      IReadOnlyList<IFormValidator> validators = null,
      ValidationMode validationMode = ValidationMode.OnSubmit,
      object initialValue = null,
      IEqualityComparer<object> comparer = null,
      bool enabled = true
    ) {
      RegisterFieldCore(
        path,
        field,
        validators,
        validationMode,
        initialValue,
        comparer,
        enabled
      );
    }

    public void RegisterField<T>(
      string path,
      IFormField field,
      IReadOnlyList<IFormValidator> validators,
      ValidationMode validationMode,
      T initialValue,
      IEqualityComparer<object> comparer = null,
      bool enabled = true
    ) {
      RegisterFieldCore(
        path,
        field,
        validators,
        validationMode,
        initialValue,
        comparer,
        enabled
      );
    }

    private void RegisterFieldCore<T>(
      string path,
      IFormField field,
      IReadOnlyList<IFormValidator> validators,
      ValidationMode validationMode,
      T initialValue,
      IEqualityComparer<object> comparer,
      bool enabled
    ) {
      var normalized = FormPath.Require(path);
      if (field == null) throw new ArgumentNullException(nameof(field));

      if (!_fields.TryGetValue(normalized, out var data)) {
        data = new FieldData();
        _fields.Add(normalized, data);
      } else if (data.field != null && !ReferenceEquals(data.field, field) && !data.HasFlag(FieldFlags.Stale)) {
        throw new InvalidOperationException($"A form field is already registered for path '{normalized}'.");
      }

      var notifyField = !ReferenceEquals(data.field, field);
      data.field = field;
      data.flags &= ~FieldFlags.Stale;
      data.validationMode = validationMode;
      data.comparer = comparer ?? ObjectFormValueComparer.Default;
      data.enabled = enabled;
      SetFlag(data, FieldFlags.Disabled, !enabled);

      if (!ReferenceEquals(data.validatorsSource, validators)) {
        data.validatorsSource = validators;
        data.validators.Clear();
        if (validators != null) {
          for (var i = 0; i < validators.Count; i++) {
            if (validators[i] != null) data.validators.Add(validators[i]);
          }
        }
      }

      if (!data.hasInitialValue) {
        if (_data.TryGetValue(normalized, out var existing)) data.initialValue = existing;
        else {
          object boxedInitialValue = initialValue;
          data.initialValue = boxedInitialValue;
          _data[normalized] = boxedInitialValue;
        }
        data.hasInitialValue = true;
      }

      UpdateDirty(data, GetValue(normalized));
      if (notifyField) field.OnFormFieldChanged(this, normalized);
    }

    public void RegisterListField(
      string path,
      IFormField field,
      IReadOnlyList<IFormValidator> validators = null,
      ValidationMode validationMode = ValidationMode.OnSubmit,
      bool enabled = true,
      int initialCount = 0
    ) {
      if (initialCount < 0) throw new ArgumentOutOfRangeException(nameof(initialCount));
      var normalized = FormPath.Require(path);
      RegisterField(normalized, field, validators, validationMode, null, null, enabled);
      var data = _fields[normalized];
      _data.Remove(normalized);
      data.isListField = true;
      if (data.listCount < 0) {
        var discoveredCount = FindListCount(normalized);
        data.listCount = discoveredCount > 0 ? discoveredCount : initialCount;
      }
      if (data.initialListCount < 0) data.initialListCount = data.listCount;
      UpdateListDirty(data);
    }

    public void ObserveField(string path, IFormField observer) {
      var normalized = FormPath.Require(path);
      if (observer == null) throw new ArgumentNullException(nameof(observer));
      if (!_fields.TryGetValue(normalized, out var data)) {
        data = new FieldData { flags = FieldFlags.Stale };
        _fields.Add(normalized, data);
      }
      data.observers ??= new List<IFormField>(1);
      if (!data.observers.Contains(observer)) data.observers.Add(observer);
    }

    public void UnobserveField(string path, IFormField observer) {
      var normalized = FormPath.Normalize(path);
      if (!_fields.TryGetValue(normalized, out var data) || data.observers == null) return;
      data.observers.Remove(observer);
      if (data.observers.Count == 0) data.observers = null;
    }

    public void UnregisterField(string path, IFormField field, bool notify = true) {
      var normalized = FormPath.Normalize(path);
      if (!_fields.TryGetValue(normalized, out var data) || !ReferenceEquals(data.field, field)) return;
      data.field = null;
      data.flags |= FieldFlags.Stale;
      data.errors.Clear();
      data.flags &= ~FieldFlags.Error;
      if (notify) NotifyFormChanged();
    }

    public void SetFieldEnabled(string path, bool enabled, bool includeChildren = false) {
      var normalized = FormPath.Require(path);
      foreach (var entry in _fields) {
        if (includeChildren
              ? !FormPath.IsInSubtree(entry.Key, normalized)
              : !string.Equals(entry.Key, normalized, StringComparison.Ordinal)) {
          continue;
        }
        entry.Value.enabled = enabled;
        SetFlag(entry.Value, FieldFlags.Disabled, !enabled);
        if (!enabled) {
          entry.Value.errors.Clear();
          entry.Value.flags &= ~FieldFlags.Error;
        }
      }
      NotifyFormChanged();
    }

    public void MarkTouched(string path, bool includeChildren = false) {
      var normalized = FormPath.Require(path);
      foreach (var entry in _fields) {
        if (includeChildren
              ? FormPath.IsInSubtree(entry.Key, normalized)
              : string.Equals(entry.Key, normalized, StringComparison.Ordinal)) {
          entry.Value.flags |= FieldFlags.Touched;
        }
      }
      NotifyFormChanged();
    }

    public void MarkFinishedEditing(string path) {
      var normalized = FormPath.Require(path);
      if (!_fields.TryGetValue(normalized, out var data)) return;
      data.flags |= FieldFlags.Touched;
      if ((data.validationMode & ValidationMode.OnFinishEditing) != 0) {
        ValidateFieldCore(normalized, data);
      }
      NotifyFormChanged();
    }

    public bool ValidateField(string path) {
      var normalized = FormPath.Require(path);
      var valid = !_fields.TryGetValue(normalized, out var data) || ValidateFieldCore(normalized, data);
      NotifyFormChanged();
      return valid;
    }

    public bool ValidateAll() {
      var valid = true;
      foreach (var entry in _fields) {
        if (!ValidateFieldCore(entry.Key, entry.Value)) valid = false;
      }
      NotifyFormChanged();
      return valid;
    }

    public FormValidationResult ValidateAllSilently() {
      return ValidateSilently(null, false);
    }

    public FormValidationResult ValidateFieldSilently(string path) {
      return ValidateSilently(FormPath.Require(path), true);
    }

    public FormValidationResult ValidatePathSilently(string prefix) {
      return ValidateSilently(FormPath.Require(prefix), false);
    }

    private FormValidationResult ValidateSilently(string path, bool exact) {
      var errors = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
      var valid = true;
      foreach (var entry in _fields) {
        if (path != null &&
            (exact
              ? !string.Equals(entry.Key, path, StringComparison.Ordinal)
              : !FormPath.IsInSubtree(entry.Key, path))) {
          continue;
        }
        if (ShouldSkip(entry.Value)) continue;
        List<string> collected = null;
        for (var i = 0; i < entry.Value.validators.Count; i++) {
          var error = entry.Value.validators[i].Validate(this, entry.Key, GetValidationValue(entry.Key, entry.Value));
          if (string.IsNullOrEmpty(error)) continue;
          collected ??= new List<string>(2);
          collected.Add(error);
        }
        if (collected == null) continue;
        valid = false;
        errors.Add(entry.Key, collected);
      }
      return new FormValidationResult { valid = valid, errors = errors };
    }

    public FormSubmitResult Submit() {
      _submitAttempted = true;
      var valid = true;
      foreach (var entry in _fields) {
        var mode = entry.Value.validationMode;
        if (mode != ValidationMode.None && (mode & ValidationMode.OnSubmit) == 0) continue;
        if (!ValidateFieldCore(entry.Key, entry.Value)) valid = false;
      }
      if (HasErrors) valid = false;

      TryDumpTree(out var tree, out var dumpErrors);
      if (dumpErrors.Count > 0) valid = false;
      var data = SnapshotData(false, false);
      var errors = SnapshotErrors(dumpErrors);
      NotifyFormChanged();
      return new FormSubmitResult {
        valid = valid,
        data = data,
        tree = valid ? tree : null,
        errors = errors
      };
    }

    public void Reset() {
      using (BeginUpdate()) {
        _listResetBuffer.Clear();
        foreach (var entry in _fields) {
          if (entry.Value.isListField) {
            _listResetBuffer.Add((
              entry.Key,
              MathfMax(0, entry.Value.initialListCount)
            ));
            entry.Value.errors.Clear();
            entry.Value.flags &= FieldFlags.Stale | FieldFlags.Disabled;
            continue;
          }
          _data[entry.Key] = entry.Value.initialValue;
          entry.Value.errors.Clear();
          entry.Value.flags &= FieldFlags.Stale | FieldFlags.Disabled;
        }
        for (var i = 0; i < _listResetBuffer.Count; i++) {
          var reset = _listResetBuffer[i];
          var current = GetListCount(reset.Path);
          if (reset.Count < current) {
            for (var index = current - 1; index >= reset.Count; index--) {
              DiscardListIndex(reset.Path, index);
            }
          }
          SetListCountCore(reset.Path, reset.Count);
          if (_fields.TryGetValue(reset.Path, out var listData)) {
            listData.listRevision = 0;
            UpdateListDirty(listData);
          }
        }
        _submitAttempted = false;
        NotifyFormChanged();
      }
    }

    public void Reset(IReadOnlyDictionary<string, object> values) {
      using (BeginUpdate()) {
        _data.Clear();
        if (values != null) {
          foreach (var entry in values) _data[FormPath.Require(entry.Key)] = entry.Value;
        }
        foreach (var entry in _fields) {
          entry.Value.initialValue = GetValue(entry.Key);
          entry.Value.hasInitialValue = true;
          entry.Value.errors.Clear();
          entry.Value.flags &= FieldFlags.Stale | FieldFlags.Disabled;
          if (entry.Value.isListField) {
            entry.Value.listCount = FindListCount(entry.Key);
            entry.Value.initialListCount = entry.Value.listCount;
            entry.Value.listRevision = 0;
          }
        }
        _submitAttempted = false;
        NotifyFormChanged();
      }
    }

    public void AcceptCurrentValuesAsInitial(string prefix = null) {
      var normalized = FormPath.Normalize(prefix);
      foreach (var entry in _fields) {
        if (!FormPath.IsInSubtree(entry.Key, normalized)) continue;
        entry.Value.initialValue = GetValidationValue(entry.Key, entry.Value);
        if (entry.Value.isListField) {
          entry.Value.initialListCount = entry.Value.listCount;
          entry.Value.listRevision = 0;
        }
        entry.Value.flags &= ~FieldFlags.Dirty;
      }
      NotifyFormChanged();
    }

    public void DiscardField(string path) {
      var normalized = FormPath.Require(path);
      _fields.Remove(normalized);
      _data.Remove(normalized);
      NotifyFormChanged();
    }

    public void DiscardSubtree(string prefix) {
      var normalized = FormPath.Require(prefix);
      _pathBuffer.Clear();
      foreach (var path in _data.Keys) {
        if (FormPath.IsInSubtree(path, normalized)) _pathBuffer.Add(path);
      }
      for (var i = 0; i < _pathBuffer.Count; i++) _data.Remove(_pathBuffer[i]);
      _pathBuffer.Clear();
      foreach (var path in _fields.Keys) {
        if (FormPath.IsInSubtree(path, normalized)) _pathBuffer.Add(path);
      }
      for (var i = 0; i < _pathBuffer.Count; i++) _fields.Remove(_pathBuffer[i]);
      NotifyFormChanged();
    }

    public int GetListCount(string path) {
      var normalized = FormPath.Require(path);
      if (_fields.TryGetValue(normalized, out var data) && data.isListField && data.listCount >= 0) {
        return data.listCount;
      }
      return FindListCount(normalized);
    }

    public int AppendListItem(string path) {
      var count = GetListCount(path);
      SetListCount(path, count + 1);
      return count;
    }

    public void InsertListItem(string path, int index) {
      var normalized = FormPath.Require(path);
      var count = GetListCount(normalized);
      if (index < 0 || index > count) throw new ArgumentOutOfRangeException(nameof(index));
      RewriteListIndices(normalized, index, 1, false);
      InvalidateListFieldOwners(normalized);
      SetListCountCore(normalized, count + 1);
      TouchList(normalized);
      NotifyFormChanged();
    }

    public void RemoveListItem(string path, int index) {
      var normalized = FormPath.Require(path);
      var count = GetListCount(normalized);
      if (index < 0 || index >= count) throw new ArgumentOutOfRangeException(nameof(index));
      DiscardListIndex(normalized, index);
      RewriteListIndices(normalized, index + 1, -1, false);
      InvalidateListFieldOwners(normalized);
      SetListCountCore(normalized, count - 1);
      TouchList(normalized);
      NotifyFormChanged();
    }

    public void MoveListItem(string path, int fromIndex, int toIndex) {
      var normalized = FormPath.Require(path);
      var count = GetListCount(normalized);
      if (fromIndex < 0 || fromIndex >= count) throw new ArgumentOutOfRangeException(nameof(fromIndex));
      if (toIndex < 0 || toIndex >= count) throw new ArgumentOutOfRangeException(nameof(toIndex));
      if (fromIndex == toIndex) return;
      RewriteListMove(_data, normalized, fromIndex, toIndex);
      RewriteListMove(_fields, normalized, fromIndex, toIndex);
      InvalidateListFieldOwners(normalized);
      TouchList(normalized);
      NotifyFormChanged();
    }

    public void SetListCount(string path, int count) {
      var normalized = FormPath.Require(path);
      if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
      var current = GetListCount(normalized);
      if (count == current) return;
      if (count < current) {
        for (var i = current - 1; i >= count; i--) DiscardListIndex(normalized, i);
        InvalidateListFieldOwners(normalized);
      }
      SetListCountCore(normalized, count);
      TouchList(normalized);
      NotifyFormChanged();
    }

    public Dictionary<string, object> DumpTree(
      bool includeStaleData = false,
      bool includeDisabledData = false
    ) {
      if (!TryDumpTree(out var tree, out var errors, includeStaleData, includeDisabledData)) {
        throw new InvalidOperationException(string.Join(Environment.NewLine, errors));
      }
      return tree;
    }

    public bool TryDumpTree(
      out Dictionary<string, object> tree,
      out List<string> errors,
      bool includeStaleData = false,
      bool includeDisabledData = false
    ) {
      tree = new Dictionary<string, object>();
      errors = new List<string>();
      var entries = new List<KeyValuePair<string, object>>(_data);
      entries.Sort(FormDataEntryComparer.Instance);
      for (var i = 0; i < entries.Count; i++) {
        var entry = entries[i];
        if (ShouldExclude(entry.Key, includeStaleData, includeDisabledData)) continue;
        if (!FormPath.TryParse(entry.Key, _segmentBuffer, out var error)) {
          errors.Add(error);
          continue;
        }
        FormTreeWriter.Insert(tree, entry.Key, _segmentBuffer, entry.Value, errors);
      }
      return errors.Count == 0;
    }

    public readonly struct FormUpdateScope : IDisposable {
      private readonly FormController _controller;

      internal FormUpdateScope(FormController controller) {
        _controller = controller;
      }

      public void Dispose() => _controller?.EndUpdate();
    }

    private void EndUpdate() {
      if (_batchDepth <= 0) return;
      _batchDepth--;
      if (_batchDepth != 0 || !_pendingNotify) return;
      _pendingNotify = false;
      NotifyFormChanged();
    }

    private bool ValidateFieldCore(string path, FieldData data) {
      if (ShouldSkip(data)) return true;
      data.errors.Clear();
      var value = GetValidationValue(path, data);
      for (var i = 0; i < data.validators.Count; i++) {
        var error = data.validators[i].Validate(this, path, value);
        if (!string.IsNullOrEmpty(error)) data.errors.Add(error);
      }
      SetFlag(data, FieldFlags.Error, data.errors.Count > 0);
      return data.errors.Count == 0;
    }

    private object GetValidationValue(string path, FieldData data) {
      return data.isListField ? data.listCount : GetValue(path);
    }

    private static bool ShouldSkip(FieldData data) {
      return data.HasFlag(FieldFlags.Stale | FieldFlags.Disabled);
    }

    private bool ShouldValidateOnChange(FieldData data) {
      if (ShouldSkip(data)) return false;
      if ((data.validationMode & ValidationMode.OnChange) != 0) return true;
      return (data.validationMode & ValidationMode.OnDirty) != 0 && data.HasFlag(FieldFlags.Dirty);
    }

    private static void SetFlag(FieldData data, FieldFlags flag, bool value) {
      if (value) data.flags |= flag;
      else data.flags &= ~flag;
    }

    private static void UpdateDirty(FieldData data, object value) {
      SetFlag(data, FieldFlags.Dirty, !data.comparer.Equals(data.initialValue, value));
    }

    private static void UpdateListDirty(FieldData data) {
      SetFlag(
        data,
        FieldFlags.Dirty,
        data.listCount != data.initialListCount || data.listRevision != 0
      );
    }

    private void NotifyFormChanged() {
      if (_batchDepth > 0) {
        _pendingNotify = true;
        return;
      }

      _notificationBuffer.Clear();
      foreach (var entry in _fields) {
        if (entry.Value.field != null) _notificationBuffer.Add((entry.Value.field, entry.Key));
        if (entry.Value.observers == null) continue;
        for (var i = 0; i < entry.Value.observers.Count; i++) {
          _notificationBuffer.Add((entry.Value.observers[i], entry.Key));
        }
      }

      using (HX.BatchScope()) {
        for (var i = 0; i < _notificationBuffer.Count; i++) {
          var target = _notificationBuffer[i];
          target.Field.OnFormFieldChanged(this, target.Path);
        }
        NotifyDirty();
        NotifyObservers();
      }
    }

    private Dictionary<string, object> SnapshotData(bool includeStale, bool includeDisabled) {
      var result = new Dictionary<string, object>(StringComparer.Ordinal);
      foreach (var entry in _data) {
        if (!ShouldExclude(entry.Key, includeStale, includeDisabled)) result.Add(entry.Key, entry.Value);
      }
      return result;
    }

    private IReadOnlyDictionary<string, IReadOnlyList<string>> SnapshotErrors(IReadOnlyList<string> dumpErrors) {
      var result = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
      foreach (var entry in _fields) {
        if (entry.Value.errors.Count > 0) result.Add(entry.Key, entry.Value.errors.ToArray());
      }
      if (dumpErrors.Count > 0) {
        var copy = new string[dumpErrors.Count];
        for (var i = 0; i < copy.Length; i++) copy[i] = dumpErrors[i];
        result[string.Empty] = copy;
      }
      return result;
    }

    private bool ShouldExclude(string path, bool includeStale, bool includeDisabled) {
      foreach (var entry in _fields) {
        if (!string.Equals(path, entry.Key, StringComparison.Ordinal) &&
            !(entry.Value.isListField && FormPath.IsInSubtree(path, entry.Key))) {
          continue;
        }
        if (!includeStale && entry.Value.HasFlag(FieldFlags.Stale)) return true;
        if (!includeDisabled && entry.Value.HasFlag(FieldFlags.Disabled)) return true;
      }
      return false;
    }

    private int FindListCount(string path) {
      var max = -1;
      foreach (var key in _data.Keys) {
        if (FormPath.TryGetListIndex(path, key, out var index) && index > max) max = index;
      }
      foreach (var key in _fields.Keys) {
        if (FormPath.TryGetListIndex(path, key, out var index) && index > max) max = index;
      }
      return max + 1;
    }

    private void SetListCountCore(string path, int count) {
      if (_fields.TryGetValue(path, out var data) && data.isListField) {
        data.listCount = count;
        UpdateListDirty(data);
      }
    }

    private void TouchList(string path) {
      if (!_fields.TryGetValue(path, out var data) || !data.isListField) return;
      data.listRevision++;
      UpdateListDirty(data);
    }

    private void InvalidateListFieldOwners(string listPath) {
      foreach (var entry in _fields) {
        if (string.Equals(entry.Key, listPath, StringComparison.Ordinal) ||
            !FormPath.IsInSubtree(entry.Key, listPath)) {
          continue;
        }
        entry.Value.field = null;
        entry.Value.observers = null;
        entry.Value.flags |= FieldFlags.Stale;
      }
    }

    private void DiscardListIndex(string listPath, int index) {
      _pathBuffer.Clear();
      foreach (var path in _data.Keys) {
        if (FormPath.TryGetListIndex(listPath, path, out var found) && found == index) {
          _pathBuffer.Add(path);
        }
      }
      for (var i = 0; i < _pathBuffer.Count; i++) _data.Remove(_pathBuffer[i]);
      _pathBuffer.Clear();
      foreach (var path in _fields.Keys) {
        if (FormPath.TryGetListIndex(listPath, path, out var found) && found == index) {
          _pathBuffer.Add(path);
        }
      }
      for (var i = 0; i < _pathBuffer.Count; i++) _fields.Remove(_pathBuffer[i]);
    }

    private void RewriteListIndices(string listPath, int start, int delta, bool includeBefore) {
      RewriteListIndices(_data, listPath, start, delta, includeBefore);
      RewriteListIndices(_fields, listPath, start, delta, includeBefore);
    }

    private static void RewriteListIndices<T>(
      Dictionary<string, T> target,
      string listPath,
      int start,
      int delta,
      bool includeBefore
    ) {
      var changes = new List<(string OldPath, string NewPath, T Value)>();
      foreach (var entry in target) {
        if (!FormPath.TryGetListIndex(listPath, entry.Key, out var index)) continue;
        if (includeBefore ? index > start : index < start) continue;
        changes.Add((
          entry.Key,
          FormPath.ReplaceListIndex(listPath, entry.Key, index + delta),
          entry.Value
        ));
      }
      ApplyChanges(target, changes);
    }

    private static void RewriteListMove<T>(
      Dictionary<string, T> target,
      string listPath,
      int fromIndex,
      int toIndex
    ) {
      var changes = new List<(string OldPath, string NewPath, T Value)>();
      foreach (var entry in target) {
        if (!FormPath.TryGetListIndex(listPath, entry.Key, out var index)) continue;
        var next = index;
        if (index == fromIndex) next = toIndex;
        else if (fromIndex < toIndex && index > fromIndex && index <= toIndex) next--;
        else if (fromIndex > toIndex && index >= toIndex && index < fromIndex) next++;
        if (next != index) {
          changes.Add((entry.Key, FormPath.ReplaceListIndex(listPath, entry.Key, next), entry.Value));
        }
      }
      ApplyChanges(target, changes);
    }

    private static void ApplyChanges<T>(
      Dictionary<string, T> target,
      List<(string OldPath, string NewPath, T Value)> changes
    ) {
      for (var i = 0; i < changes.Count; i++) target.Remove(changes[i].OldPath);
      for (var i = 0; i < changes.Count; i++) target[changes[i].NewPath] = changes[i].Value;
    }

    private static int MathfMax(int left, int right) => left > right ? left : right;
  }

  internal sealed class FormDataEntryComparer : IComparer<KeyValuePair<string, object>> {
    public static readonly FormDataEntryComparer Instance = new();

    public int Compare(KeyValuePair<string, object> x, KeyValuePair<string, object> y) {
      var length = x.Key.Length.CompareTo(y.Key.Length);
      return length != 0 ? length : string.CompareOrdinal(x.Key, y.Key);
    }
  }

  internal static class FormTreeWriter {
    public static void Insert(
      Dictionary<string, object> root,
      string path,
      IReadOnlyList<FormPathSegment> segments,
      object value,
      List<string> errors
    ) {
      object current = root;
      for (var i = 0; i < segments.Count; i++) {
        var segment = segments[i];
        var leaf = i == segments.Count - 1;
        if (segment.isIndex) {
          errors.Add($"Path '{path}' cannot begin with a list index.");
          return;
        }

        if (current is not Dictionary<string, object> dictionary) {
          errors.Add($"Path '{path}' conflicts with an existing list.");
          return;
        }

        if (leaf) {
          if (dictionary.TryGetValue(segment.name, out var existing) && IsContainer(existing)) {
            errors.Add($"Path '{path}' conflicts with an existing container.");
            return;
          }
          dictionary[segment.name] = value;
          return;
        }

        var nextIsIndex = segments[i + 1].isIndex;
        if (!dictionary.TryGetValue(segment.name, out var child)) {
          child = nextIsIndex ? new List<object>() : new Dictionary<string, object>();
          dictionary.Add(segment.name, child);
        } else if (nextIsIndex ? child is not List<object> : child is not Dictionary<string, object>) {
          errors.Add($"Path '{path}' conflicts with an existing scalar.");
          return;
        }

        current = child;
        while (i + 1 < segments.Count && segments[i + 1].isIndex) {
          i++;
          var indexed = segments[i];
          var list = current as List<object>;
          while (list.Count <= indexed.index) list.Add(null);
          if (i == segments.Count - 1) {
            if (IsContainer(list[indexed.index])) {
              errors.Add($"Path '{path}' conflicts with an existing container.");
              return;
            }
            list[indexed.index] = value;
            return;
          }

          nextIsIndex = segments[i + 1].isIndex;
          child = list[indexed.index];
          if (child == null) {
            child = nextIsIndex ? new List<object>() : new Dictionary<string, object>();
            list[indexed.index] = child;
          } else if (nextIsIndex ? child is not List<object> : child is not Dictionary<string, object>) {
            errors.Add($"Path '{path}' conflicts with an existing scalar.");
            return;
          }
          current = child;
        }
      }
    }

    private static bool IsContainer(object value) {
      return value is Dictionary<string, object> or List<object>;
    }
  }
}
