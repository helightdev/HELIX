using HELIX.Widgets.Signals;

namespace HELIX.Widgets.Prompts {
  public class HelixInputController : ValueSignal<InputConfiguration> {
    public static readonly HelixInputController Instance = new(InputConfiguration.Default);
    private HelixInputController(InputConfiguration value) : base(value) { }
  }
}