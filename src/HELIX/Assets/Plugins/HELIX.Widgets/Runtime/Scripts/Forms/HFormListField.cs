using System;
using System.Collections.Generic;
using HELIX.Widgets.Modifiers;

namespace HELIX.Widgets.Forms {
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
      var formContext = FormContext.Require(context, nameof(HFormListField));
      return new HFormContext(formContext.Controller, formContext.ResolvePath(widget.path), widget.child);
    }

    private void ResolveAndRegister() {
      if (_disposed) return;
      var formContext = FormContext.Require(mount, nameof(HFormListField));
      var fullPath = formContext.ResolvePath(widget.path);
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
}
