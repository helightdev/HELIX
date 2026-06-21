using System;
using System.Collections;
using System.Collections.Generic;
using HELIX.Widgets.Modifiers;

namespace HELIX.Widgets.Forms {
  public sealed class HFormScope : StatelessWidget<HFormScope>, IEnumerable<Widget> {
    public readonly string prefix;
    public Widget child;

    public HFormScope(
      string prefix,
      Widget child = null,
      Key key = default,
      IReadOnlyCollection<Modifier> modifiers = null
    ) : base(key, modifiers) {
      this.prefix = prefix;
      this.child = child;
    }

    public override Widget Build(BuildContext context) {
      var formContext = FormContext.Require(context, nameof(HFormScope));
      return new HFormContext(formContext.controller, formContext.ResolvePath(prefix), child);
    }

    public IEnumerator<Widget> GetEnumerator() {
      if (child != null) yield return child;
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }

    public void Add(Widget widget) {
      if (child != null) throw new InvalidOperationException("HFormScope can only have one child.");
      child = widget;
    }
  }
}
