using System.Text;
using HELIX.Boot;
using HELIX.UI.Console;
using UnityEngine;

namespace HELIX.Context {
  /// <summary>Example console commands registered into the application command service.</summary>
  [Managed(typeof(ApplicationScope))]
  [MixinUsing("static HELIX.Datatypes")]
  public partial class ExampleCommands {
    [RegisterCommand] public readonly Command echo = new EchoCommand();
    [RegisterCommand] public readonly Command math = new MathCommand();
    [RegisterCommand] public readonly Command quality = new QualityCommand();
    [RegisterCommand] public readonly Command timeScale = new TimeScaleCommand();

    [Command("mycommand", "An example command demonstrating a custom datatype.")]
    public CommandResult MyCommand(
      string key,
      [Prop(1f, Datatype = "PercentNormalized")] float value,
      [Prop(false)] bool verbose
    ) {
      return CommandResult.Successful($"MyCommand called with key: {key} and value: {value}, verbose: {verbose}");
    }

    [Command("action", parent: "mycommand")]
    public CommandResult MyCommandSubcommand() {
      return CommandResult.Successful("MyCommand subcommand executed.");
    }
  }

  public sealed class EchoCommand : Command {
    private readonly CommandProperty<string> _message =
      CommandProperties.String("message", "Text to echo. Use quotes for spaces.", required: true);
    private readonly CommandProperty<int> _repeat =
      CommandProperties.Named("repeat", new IntDatatype(min: 1, max: 10), "Number of repetitions.", 1);
    private readonly CommandProperty<bool> _upper = CommandProperties.Flag("upper", "Convert the result to uppercase.");

    public EchoCommand() => AddProperties(_message, _repeat, _upper);
    public override string Name => "echo";
    public override string Description => "Echoes text and demonstrates positional, named, and flag properties.";

    public override CommandResult Execute(CommandContext context) {
      var message = _message.Get(context);
      if (_upper.Get(context)) message = message.ToUpperInvariant();
      var builder = new StringBuilder();
      for (var i = 0; i < _repeat.Get(context); i++) {
        if (i != 0) builder.AppendLine();
        builder.Append(message);
      }
      return CommandResult.Successful(builder.ToString());
    }
  }

  public sealed class MathCommand : Command {
    public MathCommand() => AddSubcommands(new AddCommand(), new ClampCommand());
    public override string Name => "math";
    public override string Description => "Contains example numeric subcommands.";

    public override CommandResult Execute(CommandContext context) =>
      CommandResult.Successful("Choose a subcommand: math add or math clamp");

    private sealed class AddCommand : Command {
      private readonly CommandProperty<float> _a = CommandProperties.Float("a", "First value.", required: true);
      private readonly CommandProperty<float> _b = CommandProperties.Float("b", "Second value.", required: true);
      public AddCommand() => AddProperties(_a, _b);
      public override string Name => "add";
      public override string Description => "Adds two floating-point values.";

      public override CommandResult Execute(CommandContext context) => CommandResult.Successful(
        $"{_a.Get(context)} + {_b.Get(context)} = {_a.Get(context) + _b.Get(context)}"
      );
    }

    private sealed class ClampCommand : Command {
      private readonly CommandProperty<float> _value = CommandProperties.Float(
        "value",
        "Value to clamp.",
        required: true
      );
      private readonly CommandProperty<float> _min = CommandProperties.Named(
        "min",
        Datatypes.Float,
        "Minimum value.",
        0f
      );
      private readonly CommandProperty<float> _max = CommandProperties.Named(
        "max",
        Datatypes.Float,
        "Maximum value.",
        1f
      );
      public ClampCommand() => AddProperties(_value, _min, _max);
      public override string Name => "clamp";
      public override string Description => "Clamps a value using optional --min and --max properties.";

      public override CommandResult Execute(CommandContext context) {
        var min = _min.Get(context);
        var max = _max.Get(context);
        if (min > max) return CommandResult.Failed("--min cannot be greater than --max.");
        return CommandResult.Successful(Mathf.Clamp(_value.Get(context), min, max));
      }
    }
  }

  public sealed class QualityCommand : Command {
    private enum Preset { Low, Medium, High, Ultra }

    private readonly CommandProperty<Preset> _preset =
      CommandProperties.Enum<Preset>("preset", "Example enum value with datatype-driven completion.", required: true);
    public QualityCommand() => AddProperties(_preset);
    public override string Name => "quality";
    public override string Description =>
      "Demonstrates enum parsing and Tab completion without changing project settings.";

    public override CommandResult Execute(CommandContext context) =>
      CommandResult.Successful($"Selected quality preset: {_preset.Get(context)}");
  }

  public sealed class TimeScaleCommand : Command {
    private readonly CommandProperty<float> _scale =
      CommandProperties.Float("scale", "Unity time scale between 0 and 4.", 1f, required: true, min: 0f, max: 4f);
    public TimeScaleCommand() => AddProperties(_scale);
    public override string Name => "timescale";
    public override string Description => "Sets Time.timeScale and demonstrates a ranged datatype.";

    public override CommandResult Execute(CommandContext context) {
      Time.timeScale = _scale.Get(context);
      return CommandResult.Successful($"Time.timeScale = {Time.timeScale}");
    }
  }
}