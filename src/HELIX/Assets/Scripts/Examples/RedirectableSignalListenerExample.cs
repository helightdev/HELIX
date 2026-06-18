using HELIX.Widgets;
using HELIX.Widgets.Signals;
using HELIX.Widgets.Universal;
using UnityEngine;

namespace Examples {
  public class RedirectableSignalListenerExample : StatefulWidget<RedirectableSignalListenerExample> {
    public override State<RedirectableSignalListenerExample> CreateState() {
      return new State();
    }

    private class State : State<RedirectableSignalListenerExample> {
      public ValueSignal<int> signalA;
      public ValueSignal<int> signalB;
      public bool flip = false;

      public override void InitState() {
        signalA = AddDisposable(new ValueSignal<int>());
        signalB = AddDisposable(new ValueSignal<int>());
      }

      public override Widget Build(BuildContext context) {
        return new HColumn() {
          new HText($"Using Signal: {(flip ? "A" : "B")}"),
          new HButton(
            child: new HText("Flip"),
            onClick: SetState(() => {
                flip = !flip;
              }
            )
          ),
          new HButton(
            child: new HText("Update Signal A"),
            onClick: () => signalA.Value++
          ),
          new HButton(
            child: new HText("Update Signal B"),
            onClick: () => signalB.Value++
          ),
          new ChildSignalConsumer(flip ? signalA : signalB),
        };
      }
    }
  }

  public class ChildSignalConsumer : StatefulWidget<ChildSignalConsumer> {
    public ValueSignal<int> signal;

    public ChildSignalConsumer(ValueSignal<int> signal, Key key = default) : base(key) {
      this.signal = signal;
    }

    public override State<ChildSignalConsumer> CreateState() {
      return new State();
    }

    private class State : State<ChildSignalConsumer> {
      public FunctionSignalObserver function;

      public override void InitState() {
        function = AddDisposable(FunctionSignalObserver.Typed<int>(OnChanged));
        function.Observe(widget.signal);
      }

      public override void DidUpdateWidget(ChildSignalConsumer oldWidget) {
        function.Observe(widget.signal);
      }

      public void OnChanged(int value) {
        Debug.Log($"Child Signal Changed: {value}");
      }

      public override Widget Build(BuildContext context) {
        return new HText($"This is a child widget");
      }
    }
  }
}