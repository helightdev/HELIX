using System;

namespace HELIX.Widgets.Forms {
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
}
