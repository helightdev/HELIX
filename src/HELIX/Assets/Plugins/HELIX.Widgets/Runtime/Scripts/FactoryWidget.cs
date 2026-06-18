using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace HELIX.Widgets {
  public class FactoryWidget<T> : WrappingBaseWidget<FactoryWidget<T>, T> where T : VisualElement {
    public Func<T> creator;
    public Action<T> updater;

    public FactoryWidget(
      Func<T> creator = null,
      Action<T> updater = null,
      Key key = default,
      object[] constants = null,
      IReadOnlyCollection<Modifier> modifiers = null
    ) : base(key, constants, modifiers) {
      this.creator = creator;
      this.updater = updater;
    }

    public override T Create() {
      if (creator == null) {
        throw new InvalidOperationException(
          "FactoryDescriptor requires a builder function to create an element."
        );
      }

      return creator();
    }

    public override void Apply(FactoryWidget<T> previous, T element) {
      if (updater != null) updater(element);
    }
  }
}