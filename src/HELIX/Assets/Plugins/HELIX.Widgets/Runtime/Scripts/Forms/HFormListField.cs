using System;
using System.Collections.Generic;
using HELIX.Widgets.Modifiers;

namespace HELIX.Widgets.Forms {
  public sealed class HFormListScope : SingleChildStatefulWidget<HFormListScope> {
    public readonly string path;
    public readonly IEnumerable<IFormValidator> validators;
    public readonly ValidationMode validationMode;
    public readonly IEqualityComparer<object> comparer;
    public readonly bool enabled;

    public HFormListScope(
      string path,
      IEnumerable<IFormValidator> validators = null,
      ValidationMode validationMode = ValidationMode.OnSubmit,
      IEqualityComparer<object> comparer = null,
      bool enabled = true,
      Widget child = null,
      Key key = default,
      object[] constants = null,
      IReadOnlyCollection<Modifier> modifiers = null
    ) : base(child, key, constants, modifiers) {
      this.path = path;
      this.validators = validators;
      this.validationMode = validationMode;
      this.comparer = comparer;
      this.enabled = enabled;
    }

    public override State<HFormListScope> CreateState() {
      return new HFormListScopeState();
    }
  }

  public sealed class HFormListScopeState : State<HFormListScope>, IFormField {
    private FormController _form;
    private string _path;
    private bool _disposed;

    public override void InitState() {
      base.InitState();
      ResolveAndRegister();
    }

    public override void DidUpdateWidget(HFormListScope oldWidget) {
      ResolveAndRegister();
    }

    public override bool CanReconcile(HFormListScope oldWidget) {
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
      var formContext = FormContext.Require(context, nameof(HFormListScope));
      return new HFormContext(formContext.Controller, formContext.ResolvePath(widget.path), widget.child);
    }

    private void ResolveAndRegister() {
      if (_disposed) return;
      var formContext = FormContext.Require(mount, nameof(HFormListScope));
      var fullPath = formContext.ResolvePath(widget.path);
      if (ReferenceEquals(_form, formContext.Controller) && _path == fullPath) {
        _form.RegisterListField(_path, this, widget.validators, widget.validationMode, widget.comparer, widget.enabled);
        return;
      }

      _form?.UnregisterField(_path, this);
      _form = formContext.Controller;
      _path = fullPath;
      _form.RegisterListField(_path, this, widget.validators, widget.validationMode, widget.comparer, widget.enabled);
    }
  }

  public sealed class HFormListField : StatefulWidget<HFormListField> {
    public readonly string path;
    public readonly BuildFunction<int> itemBuilder;
    public readonly BuildFunction<WidgetList> containerBuilder;
    public readonly BuildFunction<FormListItemContext, Widget> itemWrapperBuilder;
    public readonly IEnumerable<IFormValidator> validators;
    public readonly ValidationMode validationMode;
    public readonly IEqualityComparer<object> comparer;
    public readonly bool enabled;

    public HFormListField(
      string path,
      BuildFunction<int> itemBuilder,
      BuildFunction<WidgetList> containerBuilder,
      BuildFunction<FormListItemContext, Widget> itemWrapperBuilder = null,
      IEnumerable<IFormValidator> validators = null,
      ValidationMode validationMode = ValidationMode.OnSubmit,
      IEqualityComparer<object> comparer = null,
      bool enabled = true,
      Key key = default,
      object[] constants = null,
      IReadOnlyCollection<Modifier> modifiers = null
    ) : base(key, constants, modifiers) {
      this.path = path;
      this.itemBuilder = itemBuilder ?? throw new ArgumentNullException(nameof(itemBuilder));
      this.containerBuilder = containerBuilder ?? throw new ArgumentNullException(nameof(containerBuilder));
      this.itemWrapperBuilder = itemWrapperBuilder;
      this.validators = validators;
      this.validationMode = validationMode;
      this.comparer = comparer;
      this.enabled = enabled;
    }

    public override State<HFormListField> CreateState() {
      return new HFormListFieldState();
    }
  }

  public sealed class HFormListFieldState : State<HFormListField> {
    public override Widget Build(BuildContext context) {
      var formContext = FormContext.Require(context, nameof(HFormListField));
      var listPath = formContext.ResolvePath(widget.path);
      var count = formContext.Controller.GetListCount(listPath);
      var items = new WidgetList(count);

      for (var index = 0; index < count; index++) {
        var itemContext = new FormListItemContext(formContext.Controller, listPath, index, count);
        var item = new HFormContext(
          formContext.Controller,
          itemContext.Path,
          widget.itemBuilder(context, index)
        );

        items.Add(widget.itemWrapperBuilder != null
          ? widget.itemWrapperBuilder(context, itemContext, item)
          : item);
      }

      var container = widget.containerBuilder(context, items)
                      ?? throw new InvalidOperationException("HFormListField containerBuilder cannot return null.");

      return new HFormListScope(
        widget.path,
        validators: widget.validators,
        validationMode: widget.validationMode,
        comparer: widget.comparer,
        enabled: widget.enabled,
        child: container
      );
    }
  }

  public sealed class FormListItemContext {
    public readonly FormController Controller;
    public readonly string ListPath;
    public readonly int Index;
    public readonly int Count;

    public FormListItemContext(FormController controller, string listPath, int index, int count) {
      Controller = controller ?? throw new ArgumentNullException(nameof(controller));
      ListPath = FormPath.Require(listPath);
      Index = index;
      Count = count;
    }

    public string Path => FormPath.Item(ListPath, Index);
    public bool IsFirst => Index == 0;
    public bool IsLast => Index >= Count - 1;

    public string ResolvePath(string path) {
      return FormPath.Compose(Path, path);
    }

    public void InsertBefore() {
      Controller.InsertListItem(ListPath, Index);
    }

    public void InsertAfter() {
      Controller.InsertListItem(ListPath, Index + 1);
    }

    public void Remove() {
      Controller.RemoveListItem(ListPath, Index);
    }

    public void MoveUp() {
      if (!IsFirst) Controller.MoveListItem(ListPath, Index, Index - 1);
    }

    public void MoveDown() {
      if (!IsLast) Controller.MoveListItem(ListPath, Index, Index + 1);
    }

    public void MoveTo(int index) {
      Controller.MoveListItem(ListPath, Index, index);
    }
  }
}
