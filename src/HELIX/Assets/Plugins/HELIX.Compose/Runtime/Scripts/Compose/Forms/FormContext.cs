using System;
using System.Collections.Generic;

namespace HELIX.Compose.Forms {
  /// <summary>Compose context value. Copying it is allocation-free and paths remain controller-local.</summary>
  public readonly struct FormContext {
    public static readonly ContextKey<FormContext> Key = new("FormContext");
    public readonly FormController controller;
    public readonly FormPath prefix;

    public FormContext(FormController controller, FormPath prefix = default) {
      this.controller = controller ?? throw new ArgumentNullException(nameof(controller));
      this.prefix = prefix;
    }

    public FormPath Resolve(string path) => controller.Paths.ParseRelative(prefix, path);
    public FormPath Resolve(int index) => controller.Paths.Append(prefix, index);
    public FormContext WithPrefix(string segment) => new(controller, Resolve(segment));
    public FormContext WithPrefix(int index) => new(controller, Resolve(index));
  }

  public static class FormComposition {
    /// <summary>
    /// Creates the core context contributor required by Compose, then publishes the form for its child scope.
    /// The contributor is a retained framework element, not an authoring composition utility.
    /// </summary>
    public static ScopeHandle ProvideForm(
      this ref Composition cx, FormController controller, FormPath prefix = default
    ) {
      var scope = cx.ContextContributor();
      using (cx.WriteContext(out var context)) FormContext.Key[context] = new FormContext(controller, prefix);
      return scope;
    }

    public static bool TryGetForm(this in Composition cx, out FormContext form) =>
      cx.TryReadContext(FormContext.Key, out form);

    public static FormContext RequireForm(this in Composition cx) {
      if (cx.TryReadContext(FormContext.Key, out var form)) return form;
      throw new InvalidOperationException("This form field must be composed below ProvideForm.");
    }
  }

  /// <summary>Reusable registration endpoint for Compose inputs. It has no subscriptions or string paths.</summary>
  public sealed class FormFieldRegistration<T> : IFormField, IDisposable {
    private readonly Action<T, bool> _apply;
    private FormController _controller;
    private FormPath _path;
    private bool _attached;

    public FormFieldRegistration(Action<T, bool> apply) =>
      _apply = apply ?? throw new ArgumentNullException(nameof(apply));

    public FormPath Path => _path;

    public void Attach(
      FormContext context, FormPath path, IEnumerable<IFormValidator> validators = null,
      ValidationMode validationMode = ValidationMode.OnChange, T initialValue = default,
      IEqualityComparer<object> comparer = null, bool enabled = true
    ) {
      if (_attached && (!ReferenceEquals(_controller, context.controller) || _path != path))
        _controller.UnregisterField(_path, this, false);
      _controller = context.controller;
      _path = path;
      _attached = true;
      _controller.RegisterField(path, this, validators, validationMode, initialValue, comparer, enabled);
    }

    public void SetUserValue(T value) {
      if (!_attached) throw new InvalidOperationException("Field is not attached.");
      _controller.SetValue(_path, value, FormChangeReason.User);
    }

    public void OnFormFieldChanged(FormController form, FormPath path) {
      if (!_attached || !ReferenceEquals(form, _controller) || path != _path) return;
      _apply(_controller.GetValue(_path, default(T)), _controller.HasValue(_path));
    }

    public void Dispose() {
      if (!_attached) return;
      _controller.UnregisterField(_path, this);
      _attached = false;
      _controller = null;
    }
  }
}
