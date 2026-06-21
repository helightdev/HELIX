using System;
using System.Collections.Generic;

namespace HELIX.Widgets.Forms {
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
    public int listRevision;
    public int initialListRevision;
    public bool hasInitialValue;
    public bool enabled = true;

    public bool HasFlag(FieldFlags flag) {
      return (flags & flag) != 0;
    }

    public FieldData Detached(bool markStale = true) {
      var detached = new FieldData {
        flags = markStale ? flags | FieldFlags.Stale : flags & ~FieldFlags.Stale,
        validationMode = validationMode,
        initialValue = initialValue,
        comparer = comparer,
        isListField = isListField,
        listCount = listCount,
        listRevision = listRevision,
        initialListRevision = initialListRevision,
        hasInitialValue = hasInitialValue,
        enabled = enabled
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
    Stale = 1 << 3,
    Disabled = 1 << 4
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

  public sealed class FormValidationResult {
    public bool valid;
    public IReadOnlyDictionary<string, IReadOnlyList<string>> errors;
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
