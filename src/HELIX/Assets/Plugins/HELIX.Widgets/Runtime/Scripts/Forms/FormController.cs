using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HELIX.Widgets.Signals;

namespace HELIX.Widgets.Forms {
  public sealed class FormController : Signal {
    public static readonly object NoInitialValue = new FormNoInitialValue();

    private readonly Dictionary<string, object> _data = new();
    private readonly Dictionary<string, FieldData> _fields = new();
    private readonly Dictionary<string, bool> _enabledOverrides = new();
    private bool _submitAttempted;
    private int _batchDepth;
    private bool _pendingNotify;

    public IReadOnlyDictionary<string, object> Data => _data;
    public IReadOnlyDictionary<string, FieldData> Fields => _fields;
    public bool SubmitAttempted => _submitAttempted;
    public bool IsDirty => _fields.Values.Any(x => (x.flags & FieldFlags.Dirty) != 0);
    public bool IsTouched => _fields.Values.Any(x => (x.flags & FieldFlags.Touched) != 0);
    public bool HasErrors => _fields.Values.Any(x => x.errors.Count > 0);

    public IDisposable BeginUpdate() {
      _batchDepth++;
      return new FormUpdateScope(this);
    }

    public void Batch(Action action) {
      if (action == null) throw new ArgumentNullException(nameof(action));
      using (BeginUpdate()) action();
    }

    public bool HasValue(string path) {
      return _data.ContainsKey(FormPath.Normalize(path));
    }

    public bool TryGetValue<T>(string path, out T value) {
      if (_data.TryGetValue(FormPath.Normalize(path), out var obj) && obj is T typed) {
        value = typed;
        return true;
      }

      value = default;
      return false;
    }

    public T GetValue<T>(string path, T fallback = default) {
      if (!_data.TryGetValue(FormPath.Normalize(path), out var value) || value == null) return fallback;
      if (value is T typed) return typed;

      try {
        return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
      } catch {
        return fallback;
      }
    }

    public object GetValue(string path, object fallback = null) {
      return _data.TryGetValue(FormPath.Normalize(path), out var value) ? value : fallback;
    }

    public IReadOnlyDictionary<string, object> SnapshotData() {
      return new Dictionary<string, object>(_data);
    }

    public IReadOnlyList<string> GetFieldPaths(string prefix = null) {
      var normalized = FormPath.Normalize(prefix);
      return _fields.Keys
        .Where(path => string.IsNullOrEmpty(normalized) || FormPath.IsInSubtree(path, normalized))
        .OrderBy(path => path, StringComparer.Ordinal)
        .ToArray();
    }

    public FieldData GetFieldData(string path) {
      return _fields.TryGetValue(FormPath.Normalize(path), out var fieldData) ? fieldData : null;
    }

    public bool IsFieldDirty(string path) {
      return GetFieldData(path)?.HasFlag(FieldFlags.Dirty) == true;
    }

    public bool IsFieldTouched(string path) {
      return GetFieldData(path)?.HasFlag(FieldFlags.Touched) == true;
    }

    public bool IsFieldValid(string path) {
      return GetFieldData(path)?.HasFlag(FieldFlags.Error) != true;
    }

    public IReadOnlyList<string> GetErrors(string path, bool includeChildren = false) {
      var normalized = FormPath.Normalize(path);
      if (!includeChildren) return _fields.TryGetValue(normalized, out var fieldData) ? fieldData.errors : Array.Empty<string>();

      return _fields
        .Where(entry => FormPath.IsInSubtree(entry.Key, normalized))
        .SelectMany(entry => entry.Value.errors)
        .ToArray();
    }

    public IReadOnlyDictionary<string, IReadOnlyList<string>> GetErrorMap(string prefix = null) {
      var normalized = FormPath.Normalize(prefix);
      return _fields
        .Where(entry => entry.Value.errors.Count > 0)
        .Where(entry => string.IsNullOrEmpty(normalized) || FormPath.IsInSubtree(entry.Key, normalized))
        .ToDictionary(entry => entry.Key, entry => (IReadOnlyList<string>)entry.Value.errors.ToArray());
    }

    public void SetValue<T>(string path, T value, FormChangeReason reason = FormChangeReason.Programmatic) {
      SetValueCore(FormPath.Require(path), value, reason);
      NotifyFormChanged();
    }

    public void SetValues(IDictionary<string, object> values, FormChangeReason reason = FormChangeReason.Programmatic) {
      if (values == null) throw new ArgumentNullException(nameof(values));
      using (BeginUpdate()) {
        foreach (var entry in values) SetValueCore(FormPath.Require(entry.Key), entry.Value, reason);
      }

      NotifyFormChanged();
    }

    public bool RemoveValue(string path) {
      var normalized = FormPath.Require(path);
      var removed = _data.Remove(normalized);
      var changed = removed;
      if (_fields.TryGetValue(normalized, out var fieldData)) {
        changed = true;
        MarkDirty(normalized, null);
        if (ShouldValidateOnChange(fieldData)) ValidateField(normalized, false);
      }

      if (changed) NotifyFormChanged();
      return removed;
    }

    public void ClearValues() {
      _data.Clear();
      foreach (var entry in _fields.ToList()) {
        if (entry.Value.isListField) {
          entry.Value.listCount = 0;
          TouchListStructure(entry.Key);
          MarkListDirty(entry.Key);
          continue;
        }

        MarkDirty(entry.Key, null);
      }

      NotifyFormChanged();
    }

    public void RegisterField(
      string path,
      IFormField field,
      IEnumerable<IFormValidator> validators = null,
      ValidationMode validationMode = ValidationMode.OnSubmit,
      object initialValue = null,
      IEqualityComparer<object> comparer = null,
      bool enabled = true
    ) {
      var normalized = FormPath.Require(path);
      if (field == null) throw new ArgumentNullException(nameof(field));

      if (!_fields.TryGetValue(normalized, out var fieldData)) {
        fieldData = new FieldData();
        _fields[normalized] = fieldData;
      } else if (fieldData.field != null
                 && !ReferenceEquals(fieldData.field, field)
                 && (fieldData.flags & FieldFlags.Stale) == 0) {
        fieldData.field = null;
        fieldData.errors.Clear();
        fieldData.flags |= FieldFlags.Stale;
        fieldData.flags = SetFlag(fieldData.flags, FieldFlags.Error, false);
      }

      fieldData.field = field;
      fieldData.validators.Clear();
      if (validators != null) fieldData.validators.AddRange(validators.Where(x => x != null));
      fieldData.validationMode = validationMode;
      fieldData.comparer = comparer ?? ObjectEqualityComparer.Default;
      fieldData.enabled = enabled;
      var effectiveEnabled = GetEffectiveEnabled(normalized, enabled);

      var hasValue = _data.TryGetValue(normalized, out var currentValue);
      if (!fieldData.hasInitialValue) {
        fieldData.initialValue = hasValue && !ReferenceEquals(initialValue, NoInitialValue) ? currentValue : initialValue;
        fieldData.hasInitialValue = true;
      }

      fieldData.flags = SetFlag(fieldData.flags, FieldFlags.Stale, false);
      fieldData.flags = SetFlag(fieldData.flags, FieldFlags.Disabled, !effectiveEnabled);
      if (!effectiveEnabled) {
        fieldData.errors.Clear();
        fieldData.flags = SetFlag(fieldData.flags, FieldFlags.Error, false);
      }

      if (!ReferenceEquals(initialValue, NoInitialValue) && !hasValue) {
        _data[normalized] = initialValue;
        currentValue = initialValue;
        hasValue = true;
      }

      MarkDirty(normalized, hasValue ? currentValue : initialValue);
      field.OnFormFieldChanged(this, normalized);
    }

    public void RegisterListField(
      string path,
      IFormField field,
      IEnumerable<IFormValidator> validators = null,
      ValidationMode validationMode = ValidationMode.OnSubmit,
      IEqualityComparer<object> comparer = null,
      bool enabled = true
    ) {
      var normalized = FormPath.Require(path);
      RegisterField(normalized, field, validators, validationMode, NoInitialValue, comparer, enabled);
      var fieldData = _fields[normalized];
      fieldData.isListField = true;
      if (fieldData.listCount < 0) fieldData.listCount = GetListPathCount(normalized);
      if (ReferenceEquals(fieldData.initialValue, NoInitialValue)) fieldData.initialValue = fieldData.listCount;
      MarkListDirty(normalized);
    }

    public void UnregisterField(string path, IFormField field, bool notify = true) {
      var normalized = FormPath.Normalize(path);
      if (!_fields.TryGetValue(normalized, out var fieldData)) return;
      if (!ReferenceEquals(fieldData.field, field)) return;

      fieldData.field = null;
      fieldData.errors.Clear();
      fieldData.flags = SetFlag(fieldData.flags, FieldFlags.Error, false);
      fieldData.flags |= FieldFlags.Stale;
      if (notify) NotifyFormChanged();
    }

    public void SetFieldEnabled(string path, bool enabled, bool includeChildren = false, bool notify = true) {
      var normalized = FormPath.Require(path);
      _enabledOverrides[normalized] = enabled;
      foreach (var entry in SelectFields(normalized, includeChildren)) {
        SetFieldEnabledCore(entry.Value, enabled);
      }

      if (notify) NotifyFormChanged();
    }

    public void ClearFieldEnabledOverride(string path, bool includeChildren = false, bool notify = true) {
      var normalized = FormPath.Require(path);
      foreach (var key in _enabledOverrides.Keys.Where(key => includeChildren
                 ? FormPath.IsInSubtree(key, normalized)
                 : string.Equals(key, normalized, StringComparison.Ordinal)).ToList()) {
        _enabledOverrides.Remove(key);
      }

      foreach (var entry in SelectFields(normalized, includeChildren)) {
        SetFieldEnabledCore(entry.Value, GetEffectiveEnabled(entry.Key, entry.Value.enabled));
      }

      if (notify) NotifyFormChanged();
    }

    public void MarkTouched(string path, bool includeChildren = false) {
      foreach (var entry in SelectFields(path, includeChildren)) entry.Value.flags |= FieldFlags.Touched;
      NotifyFormChanged();
    }

    public void MarkFinishedEditing(string path) {
      var normalized = FormPath.Require(path);
      using (BeginUpdate()) {
        MarkTouched(normalized);
        if (!_fields.TryGetValue(normalized, out var fieldData)) return;
        if ((fieldData.validationMode & ValidationMode.OnFinishEditing) != 0) ValidateField(normalized);
      }
    }

    public void DiscardField(string path) {
      var normalized = FormPath.Require(path);
      _fields.Remove(normalized);
      _data.Remove(normalized);
      NotifyFormChanged();
    }

    public void DiscardSubtree(string prefix) {
      DiscardSubtreeCore(FormPath.Normalize(prefix), true);
    }

    public bool ValidateField(string path) {
      return ValidateField(FormPath.Require(path), true);
    }

    public FormValidationResult CheckField(string path) {
      var normalized = FormPath.Require(path);
      var errors = new Dictionary<string, IReadOnlyList<string>>();
      var valid = CheckFieldCore(normalized, errors);
      return new FormValidationResult {
        valid = valid,
        errors = errors
      };
    }

    public FormValidationResult ValidateFieldSilently(string path) {
      return CheckField(path);
    }

    public bool ValidatePath(string path, bool includeChildren = true) {
      var valid = true;
      foreach (var entry in SelectFields(path, includeChildren)) valid &= ValidateField(entry.Key, false);
      NotifyFormChanged();
      return valid;
    }

    public FormValidationResult CheckPath(string path, bool includeChildren = true) {
      var errors = new Dictionary<string, IReadOnlyList<string>>();
      var valid = true;
      foreach (var entry in SelectFields(path, includeChildren)) valid &= CheckFieldCore(entry.Key, errors);
      return new FormValidationResult {
        valid = valid,
        errors = errors
      };
    }

    public FormValidationResult ValidatePathSilently(string path, bool includeChildren = true) {
      return CheckPath(path, includeChildren);
    }

    public bool ValidateAll() {
      return ValidatePath(string.Empty, true);
    }

    public FormValidationResult CheckAll() {
      return CheckPath(string.Empty, true);
    }

    public FormValidationResult ValidateAllSilently() {
      return CheckAll();
    }

    public FormSubmitResult Submit() {
      _submitAttempted = true;
      var valid = true;

      foreach (var entry in _fields.ToList()) {
        if (ShouldSkipField(entry.Value)) continue;
        var mode = entry.Value.validationMode;
        if ((mode & ValidationMode.OnSubmit) == 0 && mode != ValidationMode.None) continue;
        valid &= ValidateField(entry.Key, false);
      }

      TryDumpTree(out var tree, out var dumpErrors);
      if (dumpErrors.Count > 0) valid = false;

      NotifyFormChanged();
      return new FormSubmitResult {
        valid = valid,
        data = SnapshotSubmittableData(),
        tree = valid ? tree : null,
        errors = SnapshotErrors(dumpErrors)
      };
    }

    public void Reset(bool notify = true) {
      using (BeginUpdate()) {
        ResetSelectedFields(_fields.ToList());
        ResetMetadata(_fields.Keys);
        _submitAttempted = false;
      }

      if (notify) NotifyFormChanged();
    }

    public void Reset(Dictionary<string, object> values, bool notify = true) {
      using (BeginUpdate()) {
        _data.Clear();
        if (values != null) foreach (var entry in values) _data[FormPath.Require(entry.Key)] = entry.Value;

        foreach (var entry in _fields.ToList()) {
          if (!_fields.ContainsKey(entry.Key)) continue;
          if (entry.Value.isListField) {
            _data.Remove(entry.Key);
            entry.Value.listCount = GetListPathCount(entry.Key);
            entry.Value.initialValue = entry.Value.listCount;
            entry.Value.listRevision = 0;
            entry.Value.initialListRevision = 0;
            entry.Value.hasInitialValue = true;
            continue;
          }

          entry.Value.initialValue = _data.TryGetValue(entry.Key, out var value) ? value : null;
          entry.Value.hasInitialValue = true;
          if (!_data.ContainsKey(entry.Key)) _data[entry.Key] = entry.Value.initialValue;
        }

        ResetMetadata(_fields.Keys);
        _submitAttempted = false;
      }

      if (notify) NotifyFormChanged();
    }

    public void ResetField(string path) {
      ResetPath(path, false);
    }

    public void ResetPath(string path, bool includeChildren = true) {
      using (BeginUpdate()) {
        ResetSelectedFields(SelectFields(path, includeChildren));
        ResetMetadata(SelectFieldKeys(path, includeChildren));
      }

      NotifyFormChanged();
    }

    public void AcceptCurrentValuesAsInitial(string prefix = null) {
      foreach (var entry in SelectFields(prefix, true)) {
        if (entry.Value.isListField) {
          entry.Value.initialValue = GetListCount(entry.Key);
          entry.Value.initialListRevision = entry.Value.listRevision;
          entry.Value.hasInitialValue = true;
          MarkListDirty(entry.Key);
          continue;
        }

        _data.TryGetValue(entry.Key, out var value);
        entry.Value.initialValue = value;
        entry.Value.hasInitialValue = true;
        MarkDirty(entry.Key, value);
      }

      NotifyFormChanged();
    }

    public int GetListCount(string path) {
      var normalized = FormPath.Require(path);
      if (_fields.TryGetValue(normalized, out var fieldData) && fieldData.isListField) {
        return Math.Max(fieldData.listCount, GetListPathCount(normalized));
      }

      return GetListPathCount(normalized);
    }

    public int AppendListItem(string path) {
      var index = GetListCount(path);
      InsertListItem(path, index);
      return index;
    }

    public int PrependListItem(string path) {
      InsertListItem(path, 0);
      return 0;
    }

    public void InsertListItem(string path, int index) {
      var normalized = FormPath.Require(path);
      using (BeginUpdate()) {
        var count = GetListCount(normalized);
        if (index < 0 || index > count) throw new ArgumentOutOfRangeException(nameof(index));

        ShiftListPaths(_data, normalized, index, 1);
        ShiftListFields(normalized, index, 1);
        SetListCountCore(normalized, count + 1);
        RecalculateListFieldDirty(normalized, index, count);
        TouchListStructure(normalized);
        MarkListDirty(normalized);
      }

      NotifyFormChanged();
    }

    public void RemoveListItem(string path, int index) {
      var normalized = FormPath.Require(path);
      using (BeginUpdate()) {
        var count = GetListCount(normalized);
        if (index < 0 || index >= count) throw new ArgumentOutOfRangeException(nameof(index));

        DiscardSubtreeCore(FormPath.Compose(normalized, index.ToString(CultureInfo.InvariantCulture)), false);
        ShiftListPaths(_data, normalized, index + 1, -1);
        ShiftListFields(normalized, index + 1, -1);
        SetListCountCore(normalized, Math.Max(0, count - 1));
        RecalculateListFieldDirty(normalized, index, Math.Max(0, count - 2));
        TouchListStructure(normalized);
        MarkListDirty(normalized);
      }

      NotifyFormChanged();
    }

    public void MoveListItem(string path, int fromIndex, int toIndex) {
      var normalized = FormPath.Require(path);
      using (BeginUpdate()) {
        var count = GetListCount(normalized);
        if (fromIndex < 0 || fromIndex >= count) throw new ArgumentOutOfRangeException(nameof(fromIndex));
        if (toIndex < 0 || toIndex >= count) throw new ArgumentOutOfRangeException(nameof(toIndex));
        if (fromIndex == toIndex) return;

        MoveListPaths(_data, normalized, fromIndex, toIndex);
        MoveListFields(normalized, fromIndex, toIndex);
        RecalculateListFieldDirty(normalized, Math.Min(fromIndex, toIndex), Math.Max(fromIndex, toIndex));
        TouchListStructure(normalized);
        MarkListDirty(normalized);
      }

      NotifyFormChanged();
    }

    public void SwapListItems(string path, int leftIndex, int rightIndex) {
      var normalized = FormPath.Require(path);
      using (BeginUpdate()) {
        var count = GetListCount(normalized);
        if (leftIndex < 0 || leftIndex >= count) throw new ArgumentOutOfRangeException(nameof(leftIndex));
        if (rightIndex < 0 || rightIndex >= count) throw new ArgumentOutOfRangeException(nameof(rightIndex));
        if (leftIndex == rightIndex) return;

        SwapListPaths(_data, normalized, leftIndex, rightIndex);
        SwapListFields(normalized, leftIndex, rightIndex);
        RecalculateListFieldDirty(normalized, Math.Min(leftIndex, rightIndex), Math.Max(leftIndex, rightIndex));
        TouchListStructure(normalized);
        MarkListDirty(normalized);
      }

      NotifyFormChanged();
    }

    public void SetListCount(string path, int count) {
      var normalized = FormPath.Require(path);
      if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));

      using (BeginUpdate()) {
        var current = GetListCount(normalized);
        if (count < current) RemoveListPathsAtOrAfter(normalized, count);
        SetListCountCore(normalized, count);
        if (count != current) TouchListStructure(normalized);
        MarkListDirty(normalized);
      }

      NotifyFormChanged();
    }

    public void ClearList(string path) {
      SetListCount(path, 0);
    }

    public Dictionary<string, object> DumpTree(bool includeStaleData = false, bool includeDisabledData = false) {
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

      foreach (var entry in _data.OrderBy(x => x.Key.Length).ThenBy(x => x.Key, StringComparer.Ordinal)) {
        if (ShouldExcludeDataPath(entry.Key, includeStaleData, includeDisabledData)) continue;
        if (!FormPath.TryParse(entry.Key, out var segments, out var parseError)) {
          errors.Add(parseError);
          continue;
        }

        FormTreeWriter.InsertValue(tree, entry.Key, segments, entry.Value, errors);
      }

      return errors.Count == 0;
    }

    private bool ShouldExcludeDataPath(string path, bool includeStaleData, bool includeDisabledData) {
      if (!includeDisabledData) {
        foreach (var entry in _enabledOverrides) {
          if (entry.Value) continue;
          if (string.Equals(path, entry.Key, StringComparison.Ordinal)) return true;
          if (FormPath.IsInSubtree(path, entry.Key)) return true;
        }
      }

      foreach (var entry in _fields) {
        var stale = (entry.Value.flags & FieldFlags.Stale) != 0;
        var disabled = (entry.Value.flags & FieldFlags.Disabled) != 0;
        if ((!stale || includeStaleData) && (!disabled || includeDisabledData)) continue;
        if (string.Equals(path, entry.Key, StringComparison.Ordinal)) return true;
        if ((entry.Value.isListField || IsContainerPath(entry.Key)) && FormPath.IsInSubtree(path, entry.Key)) return true;
      }

      return false;
    }

    private bool IsContainerPath(string path) {
      var prefix = FormPath.Normalize(path);
      return _fields.Keys.Any(fieldPath => !string.Equals(fieldPath, prefix, StringComparison.Ordinal)
                                           && FormPath.IsInSubtree(fieldPath, prefix));
    }

    private void EndUpdate() {
      if (_batchDepth == 0) return;
      _batchDepth--;
      if (_batchDepth == 0 && _pendingNotify) {
        _pendingNotify = false;
        NotifyFormChanged();
      }
    }

    private void SetValueCore(string path, object value, FormChangeReason reason) {
      _data[path] = value;

      if (_fields.TryGetValue(path, out var fieldData)) {
        fieldData.flags = SetFlag(fieldData.flags, FieldFlags.Stale, false);
        if (reason == FormChangeReason.User) MarkDirty(path, value);
        if (ShouldValidateOnChange(fieldData)) ValidateField(path, false);
      }
    }

    private void ResetFieldCore(string path, FieldData fieldData) {
      if (fieldData.isListField) {
        _data.Remove(path);
        var count = fieldData.initialValue is int initialCount ? initialCount : 0;
        fieldData.listCount = count;
        fieldData.listRevision = fieldData.initialListRevision;
        RemoveListPathsAtOrAfter(path, count);
        MarkListDirty(path);
        return;
      }

      _data[path] = fieldData.initialValue;
      MarkDirty(path, fieldData.initialValue);
    }

    private void ResetSelectedFields(IEnumerable<KeyValuePair<string, FieldData>> selected) {
      var resetListPrefixes = new List<string>();
      foreach (var entry in selected.OrderBy(entry => entry.Key.Length).ThenBy(entry => entry.Key, StringComparer.Ordinal).ToList()) {
        if (resetListPrefixes.Any(prefix => !string.Equals(entry.Key, prefix, StringComparison.Ordinal)
                                            && FormPath.IsInSubtree(entry.Key, prefix))) {
          continue;
        }

        if (!_fields.TryGetValue(entry.Key, out var current)) continue;
        ResetFieldCore(entry.Key, current);
        if (current.isListField) resetListPrefixes.Add(entry.Key);
      }
    }

    private bool ValidateField(string path, bool notify) {
      if (!_fields.TryGetValue(path, out var fieldData)) return true;
      if (ShouldSkipField(fieldData)) return true;

      fieldData.errors.Clear();
      fieldData.errors.AddRange(CollectFieldErrors(path, fieldData));

      fieldData.flags = SetFlag(fieldData.flags, FieldFlags.Error, fieldData.errors.Count > 0);
      if (notify) NotifyFormChanged();
      return fieldData.errors.Count == 0;
    }

    private bool CheckFieldCore(string path, Dictionary<string, IReadOnlyList<string>> errors) {
      if (!_fields.TryGetValue(path, out var fieldData)) return true;
      if (ShouldSkipField(fieldData)) return true;

      var fieldErrors = CollectFieldErrors(path, fieldData);
      if (fieldErrors.Count == 0) return true;

      errors[path] = fieldErrors;
      return false;
    }

    private IReadOnlyList<string> CollectFieldErrors(string path, FieldData fieldData) {
      var value = fieldData.isListField ? GetListCount(path) : GetValue(path);
      var errors = new List<string>();
      foreach (var validator in fieldData.validators) {
        var error = validator.Validate(this, path, value);
        if (!string.IsNullOrEmpty(error)) errors.Add(error);
      }

      return errors;
    }

    private bool ShouldValidateOnChange(FieldData fieldData) {
      if (ShouldSkipField(fieldData)) return false;
      var mode = fieldData.validationMode;
      if ((mode & ValidationMode.OnChange) != 0) return true;
      return (mode & ValidationMode.OnDirty) != 0 && (fieldData.flags & FieldFlags.Dirty) != 0;
    }

    private void MarkDirty(string path, object value) {
      if (!_fields.TryGetValue(path, out var fieldData)) return;
      var dirty = !(fieldData.comparer ?? ObjectEqualityComparer.Default).Equals(fieldData.initialValue, value);
      fieldData.flags = SetFlag(fieldData.flags, FieldFlags.Dirty, dirty);
    }

    private void SetFieldEnabledCore(FieldData fieldData, bool enabled) {
      fieldData.flags = SetFlag(fieldData.flags, FieldFlags.Disabled, !enabled);
      if (enabled) return;

      fieldData.errors.Clear();
      fieldData.flags = SetFlag(fieldData.flags, FieldFlags.Error, false);
    }

    private bool GetEffectiveEnabled(string path, bool fallback) {
      var enabled = fallback;
      var bestMatchLength = -1;
      foreach (var entry in _enabledOverrides) {
        if (!FormPath.IsInSubtree(path, entry.Key)) continue;
        if (entry.Key.Length < bestMatchLength) continue;

        enabled = fallback && entry.Value;
        bestMatchLength = entry.Key.Length;
      }

      return enabled;
    }

    private static bool ShouldSkipField(FieldData fieldData) {
      return (fieldData.flags & (FieldFlags.Stale | FieldFlags.Disabled)) != 0;
    }

    private void MarkListDirty(string path) {
      if (!_fields.TryGetValue(path, out var fieldData) || !fieldData.isListField) return;
      var initialCount = fieldData.initialValue is int count ? count : 0;
      var dirty = fieldData.listCount != initialCount || fieldData.listRevision != fieldData.initialListRevision;
      fieldData.flags = SetFlag(fieldData.flags, FieldFlags.Dirty, dirty);
    }

    private void ResetMetadata(IEnumerable<string> paths) {
      foreach (var path in paths.ToList()) {
        if (!_fields.TryGetValue(path, out var fieldData)) continue;
        fieldData.errors.Clear();
        fieldData.flags &= FieldFlags.Stale | FieldFlags.Disabled;
      }
    }

    private IReadOnlyDictionary<string, IReadOnlyList<string>> SnapshotErrors(IReadOnlyList<string> dumpErrors) {
      var result = new Dictionary<string, IReadOnlyList<string>>();
      foreach (var entry in _fields) {
        if (entry.Value.errors.Count == 0) continue;
        result[entry.Key] = entry.Value.errors.ToArray();
      }

      if (dumpErrors.Count > 0) result[string.Empty] = dumpErrors.ToArray();
      return result;
    }

    private Dictionary<string, object> SnapshotSubmittableData() {
      return _data
        .Where(entry => !ShouldExcludeDataPath(entry.Key, false, false))
        .ToDictionary(entry => entry.Key, entry => entry.Value);
    }

    private void NotifyFormChanged() {
      if (_batchDepth > 0) {
        _pendingNotify = true;
        return;
      }

      foreach (var entry in _fields.ToList()) entry.Value.field?.OnFormFieldChanged(this, entry.Key);
      NotifyDirty();
      NotifyObservers();
    }

    private IEnumerable<KeyValuePair<string, FieldData>> SelectFields(string path, bool includeChildren) {
      var normalized = FormPath.Normalize(path);
      return _fields
        .Where(entry => includeChildren
          ? FormPath.IsInSubtree(entry.Key, normalized)
          : string.Equals(entry.Key, normalized, StringComparison.Ordinal))
        .ToList();
    }

    private IEnumerable<string> SelectFieldKeys(string path, bool includeChildren) {
      return SelectFields(path, includeChildren).Select(entry => entry.Key).ToArray();
    }

    private void DiscardSubtreeCore(string prefix, bool notify) {
      foreach (var path in _data.Keys.Where(path => FormPath.IsInSubtree(path, prefix)).ToList()) _data.Remove(path);
      foreach (var path in _fields.Keys.Where(path => FormPath.IsInSubtree(path, prefix)).ToList()) _fields.Remove(path);
      if (notify) NotifyFormChanged();
    }

    private int GetListPathCount(string path) {
      if (string.IsNullOrWhiteSpace(path)) return 0;
      var max = -1;
      foreach (var key in _data.Keys.Concat(_fields.Keys)) {
        if (FormPath.TryGetListIndex(path, key, out var index) && index > max) max = index;
      }

      return max + 1;
    }

    private void SetListCountCore(string path, int count) {
      if (_fields.TryGetValue(path, out var fieldData) && fieldData.isListField) fieldData.listCount = count;
    }

    private void TouchListStructure(string path) {
      if (_fields.TryGetValue(path, out var fieldData) && fieldData.isListField) fieldData.listRevision++;
    }

    private void RecalculateListFieldDirty(string listPath, int startIndex, int endIndex) {
      foreach (var entry in _fields.ToList()) {
        if (!FormPath.TryGetListIndex(listPath, entry.Key, out var index)) continue;
        if (index < startIndex || index > endIndex) continue;
        if (entry.Value.isListField) {
          MarkListDirty(entry.Key);
          continue;
        }

        _data.TryGetValue(entry.Key, out var value);
        MarkDirty(entry.Key, value);
      }
    }

    private static void ShiftListPaths(Dictionary<string, object> target, string listPath, int startIndex, int delta) {
      var changes = new List<(string OldPath, string NewPath, object Value)>();
      foreach (var entry in target.ToList()) {
        if (!FormPath.TryGetListIndex(listPath, entry.Key, out var index) || index < startIndex) continue;
        changes.Add((entry.Key, FormPath.ReplaceListIndex(listPath, entry.Key, index + delta), entry.Value));
      }

      ApplyPathChanges(target, changes);
    }

    private static void MoveListPaths(Dictionary<string, object> target, string listPath, int fromIndex, int toIndex) {
      var changes = new List<(string OldPath, string NewPath, object Value)>();
      foreach (var entry in target.ToList()) {
        if (!FormPath.TryGetListIndex(listPath, entry.Key, out var index)) continue;
        if (!FormPath.TryMoveListIndex(index, fromIndex, toIndex, out var movedIndex)) continue;
        changes.Add((entry.Key, FormPath.ReplaceListIndex(listPath, entry.Key, movedIndex), entry.Value));
      }

      ApplyPathChanges(target, changes);
    }

    private static void SwapListPaths(Dictionary<string, object> target, string listPath, int leftIndex, int rightIndex) {
      var changes = new List<(string OldPath, string NewPath, object Value)>();
      foreach (var entry in target.ToList()) {
        if (!FormPath.TryGetListIndex(listPath, entry.Key, out var index)) continue;
        if (index != leftIndex && index != rightIndex) continue;
        var swappedIndex = index == leftIndex ? rightIndex : leftIndex;
        changes.Add((entry.Key, FormPath.ReplaceListIndex(listPath, entry.Key, swappedIndex), entry.Value));
      }

      ApplyPathChanges(target, changes);
    }

    private void RemoveListPathsAtOrAfter(string listPath, int startIndex) {
      foreach (var entry in _data.Keys.ToList()) {
        if (FormPath.TryGetListIndex(listPath, entry, out var index) && index >= startIndex) _data.Remove(entry);
      }

      foreach (var entry in _fields.Keys.ToList()) {
        if (FormPath.TryGetListIndex(listPath, entry, out var index) && index >= startIndex) _fields.Remove(entry);
      }
    }

    private void ShiftListFields(string listPath, int startIndex, int delta) {
      var changes = new List<(string OldPath, string NewPath, FieldData Value)>();
      foreach (var entry in _fields.ToList()) {
        if (!FormPath.TryGetListIndex(listPath, entry.Key, out var index) || index < startIndex) continue;
        changes.Add((entry.Key, FormPath.ReplaceListIndex(listPath, entry.Key, index + delta), entry.Value.Detached(false)));
      }

      ApplyPathChanges(_fields, changes);
    }

    private void MoveListFields(string listPath, int fromIndex, int toIndex) {
      var changes = new List<(string OldPath, string NewPath, FieldData Value)>();
      foreach (var entry in _fields.ToList()) {
        if (!FormPath.TryGetListIndex(listPath, entry.Key, out var index)) continue;
        if (!FormPath.TryMoveListIndex(index, fromIndex, toIndex, out var movedIndex)) continue;
        changes.Add((entry.Key, FormPath.ReplaceListIndex(listPath, entry.Key, movedIndex), entry.Value.Detached(false)));
      }

      ApplyPathChanges(_fields, changes);
    }

    private void SwapListFields(string listPath, int leftIndex, int rightIndex) {
      var changes = new List<(string OldPath, string NewPath, FieldData Value)>();
      foreach (var entry in _fields.ToList()) {
        if (!FormPath.TryGetListIndex(listPath, entry.Key, out var index)) continue;
        if (index != leftIndex && index != rightIndex) continue;
        var swappedIndex = index == leftIndex ? rightIndex : leftIndex;
        changes.Add((entry.Key, FormPath.ReplaceListIndex(listPath, entry.Key, swappedIndex), entry.Value.Detached(false)));
      }

      ApplyPathChanges(_fields, changes);
    }

    private static void ApplyPathChanges<T>(Dictionary<string, T> target, IEnumerable<(string OldPath, string NewPath, T Value)> changes) {
      var snapshot = changes.ToList();
      foreach (var change in snapshot) target.Remove(change.OldPath);
      foreach (var change in snapshot) target[change.NewPath] = change.Value;
    }

    private static FieldFlags SetFlag(FieldFlags flags, FieldFlags flag, bool enabled) {
      return enabled ? flags | flag : flags & ~flag;
    }

    private sealed class FormUpdateScope : IDisposable {
      private FormController _controller;

      public FormUpdateScope(FormController controller) {
        _controller = controller;
      }

      public void Dispose() {
        _controller?.EndUpdate();
        _controller = null;
      }
    }
  }
}
