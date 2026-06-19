using System.Collections.Generic;
using HELIX.Widgets.Modifiers;

namespace HELIX.Widgets.Forms {
  public sealed class HForm : SingleChildStatefulWidget<HForm> {
    public readonly FormController controller;

    public HForm(
      FormController controller = null,
      Widget child = null,
      Key key = default,
      object[] constants = null,
      IReadOnlyCollection<Modifier> modifiers = null
    ) : base(child, key, constants, modifiers) {
      this.controller = controller;
    }

    public override State<HForm> CreateState() {
      return new HFormState();
    }
  }

  public sealed class HFormState : State<HForm> {
    private FormController _ownedController;

    public FormController Controller => widget.controller ?? _ownedController;

    public override void InitState() {
      base.InitState();
      if (widget.controller == null) _ownedController = AddDisposable(new FormController());
    }

    public override void DidUpdateWidget(HForm oldWidget) {
      if (widget.controller == null && _ownedController == null) _ownedController = AddDisposable(new FormController());
    }

    public override Widget Build(BuildContext context) {
      return new HFormContext(Controller, string.Empty, widget.child);
    }
  }
}
