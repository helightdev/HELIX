using HELIX.Widgets.Elements;
using NUnit.Framework;
using UnityEngine.UIElements;
using UnityEngine.UIElements.TestFramework;

namespace HELIX.Widgets.Tests {
  public abstract class WidgetTestFixture : UITestFixture {
    private WidgetHostElement _host;

    protected WidgetHostElement Host => _host;

    protected void PumpWidget(Widget widget) {
      PumpWidget(new FunctionBuildable(_ => widget));
    }

    protected void PumpWidget(IBuildable buildable) {
      _host ??= new WidgetHostElement();
      if (_host.parent == null) rootVisualElement.Add(_host);

      _host.Buildable = buildable;
      Pump();
    }

    protected void Pump(int frames = 1) {
      for (var i = 0; i < frames; i++) simulate.FrameUpdate();
    }

    protected IWidgetElement ElementOf(GlobalKey key) {
      Assert.That(key.Target, Is.Not.Null);
      return key.Target;
    }


    protected TElement ElementOf<TElement>(Key key) where TElement : class, IWidgetElement {
      foreach (var element in _host.Query<WidgetBaseElement>().ToList()) {
        if (element is not TElement typed) continue;
        if (element.Descriptor.key == key) return typed;
      }
      return null;
    }

    protected TElement ElementOf<TElement>(GlobalKey key) where TElement : class, IWidgetElement {
      var element = ElementOf(key);
      Assert.That(element, Is.InstanceOf<TElement>());
      return (TElement)element;
    }

    protected TState StateOf<TWidget, TState>(GlobalKey key)
      where TWidget : StatefulWidget<TWidget>
      where TState : State<TWidget> {
      var element = ElementOf<StatefulWidgetElement<TWidget>>(key);
      Assert.That(element.State, Is.InstanceOf<TState>());
      return (TState)element.State;
    }

    protected TState StateOf<TWidget, TState>(Key key)
      where TWidget : StatefulWidget<TWidget>
      where TState : State<TWidget> {
      var element = ElementOf<StatefulWidgetElement<TWidget>>(key);
      Assert.That(element.State, Is.InstanceOf<TState>());
      return (TState)element.State;
    }
  }
}
