using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace HELIX.Compose {
  /// <summary>
  ///   Reactive form state keyed by interned path chains. The only allocations on normal reads/writes are user values
  ///   and validator errors; paths themselves are value types and are reused by the controller pool.
  /// </summary>
  public sealed class FormController : Signal {
    public static readonly object NoInitialValue = new();

    public readonly FormPathPool Paths = new();
    private readonly Dictionary<FormPath, object> _data = new();
    private readonly Dictionary<FormPath, bool> _enabledOverrides = new();
    private readonly Dictionary<FormPath, FieldData> _fields = new();
    private int _batchDepth;
    private bool _pendingNotify;

    public FormController() : base("FormController", typeof(FormController)) { }
    public IReadOnlyDictionary<FormPath, object> Data => _data;
    public IReadOnlyDictionary<FormPath, FieldData> Fields => _fields;
    public bool SubmitAttempted { get; private set; }
    public bool IsDirty {
      get {
        foreach (var pair in _fields) {
          if (pair.Value.HasFlag(FieldFlags.Dirty))
            return true;
        }
        return false;
      }
    }
    public bool IsTouched {
      get {
        foreach (var pair in _fields) {
          if (pair.Value.HasFlag(FieldFlags.Touched))
            return true;
        }
        return false;
      }
    }
    public bool HasErrors {
      get {
        foreach (var pair in _fields) {
          if (pair.Value.errors.Count != 0)
            return true;
        }
        return false;
      }
    }

    public FormPath Path(string path) {
      return Paths.Parse(path);
    }

    public string Format(FormPath path) {
      return Paths.Format(path);
    }

    public IDisposable BeginUpdate() {
      _batchDepth++;
      return new UpdateScope(this);
    }

    public void Batch(Action action) {
      if (action == null) throw new ArgumentNullException(nameof(action));
      using (BeginUpdate()) action();
    }

    public bool FieldHasValue(FormPath path) {
      return _data.ContainsKey(path);
    }

    public bool FieldHasValue(string path) {
      return FieldHasValue(Path(path));
    }

    public bool TryGetValue<T>(FormPath path, out T value) {
      if (_data.TryGetValue(path, out var raw) && raw is T typed) {
        value = typed;
        return true;
      }
      value = default;
      return false;
    }

    public T GetValue<T>(FormPath path, T fallback = default) {
      if (!_data.TryGetValue(path, out var raw) || raw == null) return fallback;
      if (raw is T typed) return typed;
      try { return (T)Convert.ChangeType(raw, typeof(T), CultureInfo.InvariantCulture); } catch { return fallback; }
    }

    public object GetValue(FormPath path, object fallback = null) {
      return _data.TryGetValue(path, out var value) ? value : fallback;
    }

    public T GetValue<T>(string path, T fallback = default) {
      return GetValue(Path(path), fallback);
    }

    public FieldData GetFieldData(FormPath path) {
      return _fields.TryGetValue(path, out var data) ? data : null;
    }

    public FieldData GetFieldData(string path) {
      return GetFieldData(Path(path));
    }

    public bool IsFieldDirty(FormPath path) {
      return GetFieldData(path)?.HasFlag(FieldFlags.Dirty) == true;
    }

    public bool IsFieldTouched(FormPath path) {
      return GetFieldData(path)?.HasFlag(FieldFlags.Touched) == true;
    }

    public bool IsFieldValid(FormPath path) {
      return GetFieldData(path)?.HasFlag(FieldFlags.Error) != true;
    }

    public IReadOnlyList<string> GetErrors(FormPath path, bool includeChildren = false) {
      if (!includeChildren) return _fields.TryGetValue(path, out var field) ? field.errors : Array.Empty<string>();
      var result = new List<string>();
      foreach (var pair in Selected(path, true)) result.AddRange(pair.Value.errors);
      return result;
    }

    public void RegisterField(
      FormPath path,
      IFormField field,
      IEnumerable<IFormValidator> validators = null,
      ValidationMode validationMode = ValidationMode.OnChange,
      object initialValue = null,
      IEqualityComparer<object> comparer = null,
      bool enabled = true
    ) {
      if (!path.IsValid || path.IsRoot) throw new ArgumentException("Form path cannot be empty.", nameof(path));
      if (field == null) throw new ArgumentNullException(nameof(field));
      if (!_fields.TryGetValue(path, out var data)) _fields.Add(path, data = new FieldData());
      data.field = field;
      data.validators.Clear();
      if (validators != null) {
        foreach (var validator in validators) {
          if (validator != null)
            data.validators.Add(validator);
        }
      }
      data.validationMode = validationMode;
      data.comparer = comparer ?? EqualityComparer<object>.Default;
      data.enabled = enabled;
      if (!data.hasInitialValue) {
        data.initialValue = _data.TryGetValue(path, out var existing) ? existing : initialValue;
        data.hasInitialValue = true;
      }
      if (!ReferenceEquals(initialValue, NoInitialValue) && !_data.ContainsKey(path)) _data[path] = initialValue;
      data.flags &= ~FieldFlags.Stale;
      SetEnabled(data, EffectiveEnabled(path, enabled));
      MarkDirty(path);
      field.OnFormFieldChanged(this, path);
    }

    public void RegisterField(
      string path,
      IFormField field,
      IEnumerable<IFormValidator> validators = null,
      ValidationMode validationMode = ValidationMode.OnChange,
      object initialValue = null,
      IEqualityComparer<object> comparer = null,
      bool enabled = true
    ) {
      RegisterField(Path(path), field, validators, validationMode, initialValue, comparer, enabled);
    }

    public void RegisterListField(
      FormPath path,
      IFormField field,
      IEnumerable<IFormValidator> validators = null,
      ValidationMode validationMode = ValidationMode.OnSubmit,
      IEqualityComparer<object> comparer = null,
      bool enabled = true
    ) {
      RegisterField(path, field, validators, validationMode, NoInitialValue, comparer, enabled);
      var data = _fields[path];
      data.isListField = true;
      if (data.listCount < 0) data.listCount = ListPathCount(path);
      if (ReferenceEquals(data.initialValue, NoInitialValue)) data.initialValue = data.listCount;
      MarkListDirty(path);
    }

    public void UnregisterField(FormPath path, IFormField field, bool notify = true) {
      if (!_fields.TryGetValue(path, out var data) || !ReferenceEquals(data.field, field)) return;
      data.field = null;
      data.errors.Clear();
      data.flags = (data.flags | FieldFlags.Stale) & ~FieldFlags.Error;
      if (notify) NotifyChanged();
    }

    public void SetValue<T>(FormPath path, T value, FormChangeReason reason = FormChangeReason.Programmatic) {
      SetValueCore(path, value, reason);
      NotifyChanged();
    }

    public void SetValue<T>(string path, T value, FormChangeReason reason = FormChangeReason.Programmatic) {
      SetValue(Path(path), value, reason);
    }

    public bool RemoveValue(FormPath path) {
      var removed = _data.Remove(path);
      if (removed) {
        if (_fields.TryGetValue(path, out var field)) {
          MarkDirty(path);
          if (ShouldValidate(field)) ValidateField(path, false);
        }
        NotifyChanged();
      }
      return removed;
    }

    public void SetValues(
      IEnumerable<KeyValuePair<FormPath, object>> values,
      FormChangeReason reason = FormChangeReason.Programmatic
    ) {
      if (values == null) throw new ArgumentNullException(nameof(values));
      using (BeginUpdate()) {
        foreach (var pair in values)
          SetValueCore(pair.Key, pair.Value, reason);
      }
      NotifyChanged();
    }

    public void ClearValues() {
      _data.Clear();
      foreach (var pair in _fields) {
        if (pair.Value.isListField) {
          pair.Value.listCount = 0;
          pair.Value.listRevision++;
          MarkListDirty(pair.Key);
        } else MarkDirty(pair.Key);
      }
      NotifyChanged();
    }

    public void MarkTouched(FormPath path, bool includeChildren = false) {
      foreach (var pair in Selected(path, includeChildren)) pair.Value.flags |= FieldFlags.Touched;
      NotifyChanged();
    }

    public void MarkFinishedEditing(FormPath path) {
      using (BeginUpdate()) {
        MarkTouched(path);
        if (_fields.TryGetValue(path, out var data) && (data.validationMode & ValidationMode.OnFinishEditing) != 0)
          ValidateField(path, false);
      }
    }

    public void SetFieldEnabled(FormPath path, bool enabled, bool includeChildren = false) {
      _enabledOverrides[path] = enabled;
      foreach (var pair in Selected(path, includeChildren)) SetEnabled(pair.Value, enabled);
      NotifyChanged();
    }

    public void ClearFieldEnabledOverride(FormPath path, bool includeChildren = false) {
      var remove = new List<FormPath>();
      foreach (var pair in _enabledOverrides) {
        if (pair.Key == path || (includeChildren && Paths.IsInSubtree(pair.Key, path)))
          remove.Add(pair.Key);
      }
      foreach (var key in remove) _enabledOverrides.Remove(key);
      foreach (var pair in Selected(path, includeChildren))
        SetEnabled(pair.Value, EffectiveEnabled(pair.Key, pair.Value.enabled));
      NotifyChanged();
    }

    public bool ValidateField(FormPath path) {
      return ValidateField(path, true);
    }

    public bool ValidatePath(FormPath path, bool includeChildren = true) {
      var valid = true;
      using (BeginUpdate()) {
        foreach (var pair in Selected(path, includeChildren))
          valid &= ValidateField(pair.Key, false);
      }
      NotifyChanged();
      return valid;
    }

    public bool ValidateAll() {
      return ValidatePath(Paths.Root);
    }

    public FormSubmitResult Submit() {
      SubmitAttempted = true;
      var valid = true;
      using (BeginUpdate()) {
        foreach (var pair in _fields) {
          if (!Skip(pair.Value) && ((pair.Value.validationMode & ValidationMode.OnSubmit) != 0 ||
            pair.Value.validationMode == ValidationMode.None))
            valid &= ValidateField(pair.Key, false);
        }
      }
      NotifyChanged();
      return new FormSubmitResult { valid = valid, data = SnapshotData(), errors = SnapshotErrors() };
    }

    public IReadOnlyDictionary<FormPath, object> SnapshotData() {
      return new Dictionary<FormPath, object>(_data);
    }

    /// <summary>Formats the current flat path/value data for diagnostics and submit logging.</summary>
    public string FormatData() {
      return FormatData(_data);
    }

    public string FormatData(IReadOnlyDictionary<FormPath, object> data) {
      if (data == null || data.Count == 0) return "{}";
      var entries = new List<KeyValuePair<FormPath, object>>(data);
      entries.Sort((left, right) => string.CompareOrdinal(Paths.Format(left.Key), Paths.Format(right.Key)));
      var result = new StringBuilder("{ ");
      for (var i = 0; i < entries.Count; i++) {
        if (i != 0) result.Append(", ");
        result.Append(Paths.Format(entries[i].Key)).Append(": ");
        AppendFormattedValue(result, entries[i].Value);
      }
      return result.Append(" }").ToString();
    }

    public IReadOnlyDictionary<FormPath, IReadOnlyList<string>> SnapshotErrors() {
      var result = new Dictionary<FormPath, IReadOnlyList<string>>();
      foreach (var pair in _fields) {
        if (pair.Value.errors.Count > 0)
          result[pair.Key] = pair.Value.errors.ToArray();
      }
      return result;
    }

    public int GetListCount(FormPath path) {
      return _fields.TryGetValue(path, out var field) && field.isListField
        ? Math.Max(field.listCount, ListPathCount(path))
        : ListPathCount(path);
    }

    public int AppendListItem(FormPath path) {
      var index = GetListCount(path);
      InsertListItem(path, index);
      return index;
    }

    public void InsertListItem(FormPath path, int index) {
      var count = GetListCount(path);
      if (index < 0 || index > count) throw new ArgumentOutOfRangeException(nameof(index));
      ReindexList(path, index, 1);
      SetListCount(path, count + 1);
    }

    public void MoveListItem(FormPath path, int fromIndex, int toIndex) {
      var count = GetListCount(path);
      if (fromIndex < 0 || fromIndex >= count) throw new ArgumentOutOfRangeException(nameof(fromIndex));
      if (toIndex < 0 || toIndex >= count) throw new ArgumentOutOfRangeException(nameof(toIndex));
      if (fromIndex == toIndex) return;
      RemapList(
        path,
        index => {
          if (index == fromIndex) return toIndex;
          if (fromIndex < toIndex && index > fromIndex && index <= toIndex) return index - 1;
          if (fromIndex > toIndex && index >= toIndex && index < fromIndex) return index + 1;
          return index;
        }
      );
      TouchList(path);
    }

    public void SwapListItems(FormPath path, int leftIndex, int rightIndex) {
      var count = GetListCount(path);
      if (leftIndex < 0 || leftIndex >= count) throw new ArgumentOutOfRangeException(nameof(leftIndex));
      if (rightIndex < 0 || rightIndex >= count) throw new ArgumentOutOfRangeException(nameof(rightIndex));
      if (leftIndex == rightIndex) return;
      RemapList(path, index => index == leftIndex ? rightIndex : index == rightIndex ? leftIndex : index);
      TouchList(path);
    }

    public void RemoveListItem(FormPath path, int index) {
      var count = GetListCount(path);
      if (index < 0 || index >= count) throw new ArgumentOutOfRangeException(nameof(index));
      RemoveSubtree(Paths.Append(path, index));
      ReindexList(path, index + 1, -1);
      SetListCount(path, count - 1);
    }

    public void SetListCount(FormPath path, int count) {
      if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
      var old = GetListCount(path);
      if (count < old) {
        for (var i = count; i < old; i++)
          RemoveSubtree(Paths.Append(path, i));
      }
      if (_fields.TryGetValue(path, out var field)) {
        field.isListField = true;
        field.listCount = count;
        field.listRevision++;
        MarkListDirty(path);
      }
      NotifyChanged();
    }

    public void Reset() {
      using (BeginUpdate()) {
        foreach (var pair in _fields) {
          if (pair.Value.isListField) {
            SetListCount(pair.Key, pair.Value.initialValue is int count ? count : 0);
            pair.Value.listRevision = pair.Value.initialListRevision;
          } else if (ReferenceEquals(pair.Value.initialValue, NoInitialValue)) _data.Remove(pair.Key);
          else _data[pair.Key] = pair.Value.initialValue;
          pair.Value.flags &= ~(FieldFlags.Dirty | FieldFlags.Touched | FieldFlags.Error);
          pair.Value.errors.Clear();
          NotifyField(pair.Key, pair.Value);
        }
        SubmitAttempted = false;
      }
      NotifyChanged();
    }

    public void ResetPath(FormPath path, bool includeChildren = true) {
      using (BeginUpdate()) {
        foreach (var pair in Selected(path, includeChildren)) {
          var field = pair.Value;
          if (field.isListField) {
            SetListCount(pair.Key, field.initialValue is int count ? count : 0);
            field.listRevision = field.initialListRevision;
          } else if (ReferenceEquals(field.initialValue, NoInitialValue)) _data.Remove(pair.Key);
          else _data[pair.Key] = field.initialValue;
          field.flags &= ~(FieldFlags.Dirty | FieldFlags.Touched | FieldFlags.Error);
          field.errors.Clear();
          NotifyField(pair.Key, field);
        }
      }
      NotifyChanged();
    }

    public void AcceptCurrentValuesAsInitial(FormPath prefix = default) {
      foreach (var pair in Selected(prefix, true)) {
        var field = pair.Value;
        if (field.isListField) {
          field.initialValue = GetListCount(pair.Key);
          field.initialListRevision = field.listRevision;
          MarkListDirty(pair.Key);
        } else {
          _data.TryGetValue(pair.Key, out field.initialValue);
          field.hasInitialValue = true;
          MarkDirty(pair.Key);
        }
      }
      NotifyChanged();
    }

    private void SetValueCore(FormPath path, object value, FormChangeReason reason) {
      _data[path] = value;
      if (!_fields.TryGetValue(path, out var field)) return;
      field.flags &= ~FieldFlags.Stale;
      if (reason == FormChangeReason.User) MarkDirty(path);
      if (ShouldValidate(field)) ValidateField(path, false);
      NotifyField(path, field);
    }

    private bool ValidateField(FormPath path, bool notify) {
      if (!_fields.TryGetValue(path, out var data) || Skip(data)) return true;
      data.errors.Clear();
      var value = GetValue(path);
      foreach (var validator in data.validators) {
        var error = validator.Validate(this, path, value);
        if (!string.IsNullOrEmpty(error)) data.errors.Add(error);
      }
      data.flags = data.errors.Count == 0 ? data.flags & ~FieldFlags.Error : data.flags | FieldFlags.Error;
      NotifyField(path, data);
      if (notify) NotifyChanged();
      return data.errors.Count == 0;
    }

    private bool ShouldValidate(FieldData data) {
      return !Skip(data) &&
        ((data.validationMode & ValidationMode.OnChange) != 0 ||
          (data.validationMode & ValidationMode.OnDirty) != 0 &&
          data.HasFlag(FieldFlags.Dirty));
    }

    private static bool Skip(FieldData data) {
      return data.HasFlag(FieldFlags.Stale) || data.HasFlag(FieldFlags.Disabled);
    }

    private void MarkDirty(FormPath path) {
      if (!_fields.TryGetValue(path, out var data) || data.isListField) return;
      var hasCurrent = _data.TryGetValue(path, out var current);
      var unchanged = ReferenceEquals(data.initialValue, NoInitialValue)
        ? !hasCurrent
        : data.comparer.Equals(current, data.initialValue);
      data.flags = unchanged
        ? data.flags & ~FieldFlags.Dirty
        : data.flags | FieldFlags.Dirty;
    }

    private void MarkListDirty(FormPath path) {
      if (!_fields.TryGetValue(path, out var data)) return;
      var count = data.initialValue is int initial ? initial : 0;
      data.flags = data.listCount == count && data.listRevision == data.initialListRevision
        ? data.flags & ~FieldFlags.Dirty
        : data.flags | FieldFlags.Dirty;
    }

    private void SetEnabled(FieldData data, bool enabled) {
      data.flags = enabled ? data.flags & ~FieldFlags.Disabled : data.flags | FieldFlags.Disabled;
      if (!enabled) {
        data.errors.Clear();
        data.flags &= ~FieldFlags.Error;
      }
    }

    private bool EffectiveEnabled(FormPath path, bool fallback) {
      for (var current = path;;) {
        if (_enabledOverrides.TryGetValue(current, out var enabled)) return enabled;
        if (current.IsRoot) return fallback;
        current = Parent(current);
      }
    }

    private FormPath Parent(FormPath path) {
      return Paths.Parent(path);
    }

    private IEnumerable<KeyValuePair<FormPath, FieldData>> Selected(FormPath path, bool descendants) {
      foreach (var pair in _fields) {
        if (pair.Key == path || (descendants && Paths.IsInSubtree(pair.Key, path)))
          yield return pair;
      }
    }

    private int ListPathCount(FormPath path) {
      var max = -1;
      foreach (var pair in _data) {
        if (Paths.TryGetDirectIndex(path, pair.Key, out var index) && index > max)
          max = index;
      }
      foreach (var pair in _fields) {
        if (Paths.TryGetDirectIndex(path, pair.Key, out var index) && index > max)
          max = index;
      }
      return max + 1;
    }

    private void ReindexList(FormPath path, int start, int delta) {
      var values = new List<KeyValuePair<FormPath, object>>();
      foreach (var pair in _data) {
        if (Paths.TryGetDirectIndex(path, pair.Key, out var index) && index >= start)
          values.Add(pair);
      }
      foreach (var pair in values) {
        _data.Remove(pair.Key);
        var target = ReplaceIndex(path, pair.Key, delta + DirectIndex(path, pair.Key));
        _data[target] = pair.Value;
      }
      var fields = new List<KeyValuePair<FormPath, FieldData>>();
      foreach (var pair in _fields) {
        if (Paths.TryGetDirectIndex(path, pair.Key, out var index) && index >= start)
          fields.Add(pair);
      }
      foreach (var pair in fields) {
        _fields.Remove(pair.Key);
        _fields[ReplaceIndex(path, pair.Key, delta + DirectIndex(path, pair.Key))] = pair.Value;
      }
    }

    private void RemapList(FormPath path, Func<int, int> remap) {
      var values = new List<KeyValuePair<FormPath, object>>();
      foreach (var pair in _data)
        if (Paths.TryGetDirectIndex(path, pair.Key, out var index))
          values.Add(pair);
      foreach (var pair in values) _data.Remove(pair.Key);
      foreach (var pair in values) _data[ReplaceIndex(path, pair.Key, remap(DirectIndex(path, pair.Key)))] = pair.Value;

      var fields = new List<KeyValuePair<FormPath, FieldData>>();
      foreach (var pair in _fields)
        if (Paths.TryGetDirectIndex(path, pair.Key, out var index))
          fields.Add(pair);
      foreach (var pair in fields) _fields.Remove(pair.Key);
      foreach (var pair in fields)
        _fields[ReplaceIndex(path, pair.Key, remap(DirectIndex(path, pair.Key)))] = pair.Value;
    }

    private void TouchList(FormPath path) {
      if (_fields.TryGetValue(path, out var field)) {
        field.listRevision++;
        MarkListDirty(path);
      }
      NotifyChanged();
    }

    private int DirectIndex(FormPath list, FormPath path) {
      Paths.TryGetDirectIndex(list, path, out var index);
      return index;
    }

    private FormPath ReplaceIndex(FormPath list, FormPath path, int index) {
      return Paths.ReplaceDirectIndex(list, path, index);
    }

    private void RemoveSubtree(FormPath prefix) {
      var remove = new List<FormPath>();
      foreach (var pair in _data) {
        if (Paths.IsInSubtree(pair.Key, prefix))
          remove.Add(pair.Key);
      }
      foreach (var key in remove) _data.Remove(key);
      remove.Clear();
      foreach (var pair in _fields) {
        if (Paths.IsInSubtree(pair.Key, prefix))
          remove.Add(pair.Key);
      }
      foreach (var key in remove) _fields.Remove(key);
    }

    private void NotifyField(FormPath path, FieldData data) {
      data.field?.OnFormFieldChanged(this, path);
    }

    private static void AppendFormattedValue(StringBuilder result, object value) {
      if (value == null) {
        result.Append("null");
        return;
      }
      if (value is string text) {
        result.Append('\"').Append(text.Replace("\\", "\\\\").Replace("\"", "\\\"")).Append('\"');
        return;
      }
      if (value is IFormattable formattable) result.Append(formattable.ToString(null, CultureInfo.InvariantCulture));
      else result.Append(value);
    }

    private void NotifyChanged() {
      if (_batchDepth > 0) {
        _pendingNotify = true;
        return;
      }
      NotifyDirty();
      NotifyObservers();
    }

    private void EndUpdate() {
      if (--_batchDepth == 0 && _pendingNotify) {
        _pendingNotify = false;
        NotifyChanged();
      }
    }

    private readonly struct UpdateScope : IDisposable {
      private readonly FormController _owner;

      public UpdateScope(FormController owner) {
        _owner = owner;
      }

      public void Dispose() {
        _owner.EndUpdate();
      }
    }
  }
}