using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Formatting;
using HELIX.Widgets.Theming;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Elements {
  [SuppressMessage("ReSharper", "ParameterHidesMember")]
  public abstract class WidgetBaseElement : BaseElement, IWidgetElement, IReconcileScheduler,
    IScheduledReconcileRunner {
    public Widget Descriptor { get; set; }
    public BuildContext ParentContext { get; set; }
    public abstract bool CanReconcile(Widget updated);
    public abstract bool Reconcile(Widget updated);

    private ReconcileMode _reconcileMode;
    private Widget _reconcileTarget;

    protected override void OnAttached(AttachToPanelEvent evt) {
      base.OnAttached(evt);
      RunScheduledReconcile();
    }

    public void ScheduleReconcile(Widget descriptor, ReconcileMode mode = ReconcileMode.AfterParent) {
      if (mode == ReconcileMode.Eager) {
        Reconciler.Reconcile(this, descriptor);
      } else {
        _reconcileMode = mode;
        _reconcileTarget = descriptor;
      }
    }

    public bool TryRunScheduledReconcile(ReconcileMode mode) {
      if (_reconcileTarget == null || _reconcileMode != mode) return false;
      RunScheduledReconcile();
      return true;
    }

    private void RunScheduledReconcile() {
      if (_reconcileTarget == null) return;
      var target = _reconcileTarget;
      try {
        Reconciler.Reconcile(this, target);
      } finally {
        if (ReferenceEquals(_reconcileTarget, target)) _reconcileTarget = null;
      }
    }

    public virtual List<DiagnosticsNode> DebugDescribeChildren() {
      return new List<DiagnosticsNode>();
    }

    public virtual T GetThemed<T>(BaseThemeProperty<T> property, bool listen = true) {
      return ThemeProviderElement.Resolve(ThemeProviderElement, property);
    }

    public virtual bool TryGetThemed<S>(BaseThemeProperty<S> property, out S value, bool listen = true) {
      return ThemeProviderElement.TryResolve(property, out value);
    }

    public virtual string ToStringDeep(
      string prefixLineOne = "",
      string prefixOtherLines = null,
      DiagnosticLevel minLevel = DiagnosticLevel.Debug,
      int wrapWidth = 65
    ) {
      return this.ToDiagnosticsNodeSafe().ToStringDeep(
        prefixLineOne,
        prefixOtherLines,
        null,
        minLevel,
        wrapWidth
      );
    }

    public virtual string ToStringShallow(string joiner = ", ", DiagnosticLevel minLevel = DiagnosticLevel.Debug) {
      return IDiagnosticableTree.DefaultToStringShallow(this, joiner, minLevel);
    }

    public virtual DiagnosticsNode ToDiagnosticsNode(string name = null, DiagnosticsTreeStyle? style = null) {
      return new DiagnosticableTreeNode(name, this, style);
    }

    public virtual string ToStringShort() {
      return this.DescribeIdentity();
    }

    public virtual void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
      properties.Add(new DiagnosticsProperty<Widget>("descriptor", Descriptor, showName: false));
    }

    public override string ToString() {
      return this.ToDiagnosticsNodeSafe(style: DiagnosticsTreeStyle.SingleLine).ToString();
    }
  }

  public abstract class WidgetBaseElement<T> : WidgetBaseElement, IWidgetElement where T : Widget {
    public T TypedDescriptor {
      get => Descriptor as T;
      set => Descriptor = value;
    }

    public override bool CanReconcile(Widget updated) {
      return updated is T;
    }

    public override bool Reconcile(Widget updated) {
      if (updated is not T widget) return false;
      var previous = TypedDescriptor;
      Apply(previous, widget);
      Modifier.ApplyDelta(previous, updated, this);
      Descriptor = updated;
      return true;
    }

    public virtual void Apply(T previous, T widget) { }
  }
}
