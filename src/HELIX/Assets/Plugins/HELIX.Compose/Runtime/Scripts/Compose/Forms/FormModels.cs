using System;
using System.Collections.Generic;

namespace HELIX.Compose.Forms {
  [Flags] public enum FieldFlags : byte { None = 0, Touched = 1, Dirty = 2, Error = 4, Stale = 8, Disabled = 16 }

  [Flags] public enum ValidationMode { None = 0, OnChange = 1, OnDirty = 2, OnFinishEditing = 4, OnSubmit = 8 }

  public enum FormChangeReason : byte { Programmatic, User }

  public interface IFormField {
    void OnFormFieldChanged(FormController form, FormPath path);
  }

  public interface IFormValidator {
    string Validate(FormController form, FormPath path, object value);
  }

  public sealed class FieldData {
    public IFormField field;
    public FieldFlags flags;
    public readonly List<string> errors = new();
    public readonly List<IFormValidator> validators = new();
    public ValidationMode validationMode;
    public object initialValue;
    public IEqualityComparer<object> comparer = EqualityComparer<object>.Default;
    public bool hasInitialValue, enabled = true, isListField;
    public int listCount = -1, listRevision, initialListRevision;
    public bool HasFlag(FieldFlags flag) => (flags & flag) != 0;
  }

  public sealed class FormSubmitResult {
    public bool valid;
    public IReadOnlyDictionary<FormPath, object> data;
    public IReadOnlyDictionary<FormPath, IReadOnlyList<string>> errors;
  }
}