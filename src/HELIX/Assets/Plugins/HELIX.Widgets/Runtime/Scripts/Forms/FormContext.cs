using System;
using System.Collections.Generic;
using HELIX.Widgets.Elements;
using HELIX.Widgets.Modifiers;

namespace HELIX.Widgets.Forms {
  public sealed class FormContext {
    public readonly FormController Controller;
    public readonly string PathPrefix;

    private FormContext(FormController controller, string pathPrefix) {
      Controller = controller ?? throw new ArgumentNullException(nameof(controller));
      PathPrefix = FormPath.Normalize(pathPrefix);
    }

    public string ResolvePath(string path) {
      return FormPath.Compose(PathPrefix, path);
    }

    public FormContext WithPrefix(string prefix) {
      return new FormContext(Controller, ResolvePath(prefix));
    }

    public static FormContext Resolve(BuildContext context) {
      return BuildContext.TryFindParent<FormContextElement>(context, out var element)
        ? new FormContext(element.Controller, element.PathPrefix)
        : null;
    }

    public static FormContext Require(BuildContext context, string owner) {
      return Resolve(context) ?? throw new InvalidOperationException($"{owner} must be built below an HForm.");
    }
  }

  public sealed class HFormContext : SingleChildWidget {
    public readonly FormController controller;
    public readonly string pathPrefix;

    public HFormContext(
      FormController controller,
      string pathPrefix,
      Widget child = null,
      Key key = default,
      object[] constants = null,
      IReadOnlyCollection<Modifier> modifiers = null
    ) : base(child, key, constants, modifiers) {
      this.controller = controller ?? throw new ArgumentNullException(nameof(controller));
      this.pathPrefix = FormPath.Normalize(pathPrefix);
    }

    public override IWidgetElement CreateElement() {
      return ReconcileInto(new FormContextElement());
    }
  }

  public sealed class FormContextElement : SingleChildWidgetBaseElement<HFormContext> {
    public FormController Controller { get; private set; }
    public string PathPrefix { get; private set; } = string.Empty;

    public override void Apply(HFormContext previous, HFormContext widget) {
      Controller = widget.controller;
      PathPrefix = FormPath.Normalize(widget.pathPrefix);
    }
  }
}
