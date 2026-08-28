using System;

namespace HELIX.Compose {
  public static class FormValidators {
    public static IFormValidator Required(string message = "Required") {
      return new DelegateFormValidator((_, _, value) =>
        value == null || (value is string text && string.IsNullOrWhiteSpace(text)) ? message : null
      );
    }

    public static IFormValidator MinLength(int minimum, string message = null) {
      if (minimum < 0) throw new ArgumentOutOfRangeException(nameof(minimum));
      return new DelegateFormValidator((_, _, value) => value is string text && text.Length < minimum
        ? message ?? $"Must be at least {minimum} characters"
        : null
      );
    }

    public static IFormValidator MaxLength(int maximum, string message = null) {
      if (maximum < 0) throw new ArgumentOutOfRangeException(nameof(maximum));
      return new DelegateFormValidator((_, _, value) => value is string text && text.Length > maximum
        ? message ?? $"Must be at most {maximum} characters"
        : null
      );
    }

    public static IFormValidator Delegate(Func<FormController, FormPath, object, string> validate) {
      return new DelegateFormValidator(validate);
    }

    // Keep the concise legacy spelling while preserving the pointer-based path contract.
    public static IFormValidator Func(Func<FormController, FormPath, object, string> validate) {
      return Delegate(validate);
    }
  }

  public sealed class DelegateFormValidator : IFormValidator {
    private readonly Func<FormController, FormPath, object, string> _validate;

    public DelegateFormValidator(Func<FormController, FormPath, object, string> validate) {
      _validate = validate ?? throw new ArgumentNullException(nameof(validate));
    }

    public string Validate(FormController form, FormPath path, object value) {
      return _validate(form, path, value);
    }
  }
}