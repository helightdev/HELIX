using System;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;

namespace HELIX.UI.Console {
  public readonly struct CommandResult {
    public readonly bool success; public readonly object message;
    public bool HasMessage => message != null;
    public CommandResult(bool success, object message = null) { this.success = success; this.message = message; }
    public static CommandResult Successful(object message = null) => new(true, message);
    public static CommandResult Failed(object message) => new(false, message);
    public static implicit operator bool(CommandResult value) => value.success;
    public static implicit operator CommandResult(bool value) => new(value);
    public static implicit operator CommandResult(string value) => Successful(value);
  }

  public sealed class CommandContext {
    private readonly Dictionary<CommandProperty, object> _values = new();
    public CommandContext(string rawInput, Command command) { RawInput = rawInput; Command = command; }
    public string RawInput { get; }
    public Command Command { get; }
    public bool HasValue(CommandProperty property) => _values.ContainsKey(property);
    public object GetValue(CommandProperty property) => _values.TryGetValue(property, out var value) ? value : property.GetDefaultValue();
    public T GetValue<T>(CommandProperty<T> property) => (T)GetValue((CommandProperty)property);
    public void SetValue(CommandProperty property, object value) => _values[property] = value;
  }

  public abstract class CommandProperty {
    protected CommandProperty(string name, string description, bool required, bool named, bool flag) {
      Name = name ?? throw new ArgumentNullException(nameof(name)); Description = description;
      Required = required; Named = named; Flag = flag;
    }
    public string Name { get; }
    public string Description { get; }
    public bool Required { get; }
    public bool Named { get; }
    public bool Flag { get; }
    public string Token => Flag ? "-" + Name : Named ? "--" + Name : Name;
    public abstract object Parse(string text);
    internal abstract object GetDefaultValue();
    public abstract void Complete(string text, List<string> result);
    protected static void CompleteChoices(IDatatype datatype, string text, List<string> result) {
      if (datatype is not IDatatypeChoice choices) return;
      for (var i = 0; i < choices.ChoiceCount; i++) {
        var label = choices.GetChoiceLabel(i);
        if (choices.IsChoiceEnabled(i) && label.StartsWith(text, StringComparison.OrdinalIgnoreCase)) result.Add(label);
      }
    }
    public string ShortHelp(CommandCompletionStyle style, string value = null, bool active = false, bool malformed = false) {
      var token = Named || Flag ? Token : Name;
      var display = Flag || !Required ? $"[{token}]" : $"<noparse><{token}></noparse>";
      if (value != null) {
        var color = malformed ? style.error : style.valid;
        display = Flag ? $"<color={color}>{token}</color>" :
          $"<color={style.weak}>{Name}:</color><color={color}>{value}</color>";
      }
      return active ? $"<b><color={style.active}>{display}</color></b>" : display;
    }
  }

  public sealed class CommandProperty<T> : CommandProperty {
    public readonly IDatatype<T> datatype;
    public readonly T defaultValue;
    public CommandProperty(string name, IDatatype<T> datatype, string description = null, T defaultValue = default,
      bool required = false, bool named = false, bool flag = false) : base(name, description, required, named, flag) {
      this.datatype = datatype ?? throw new ArgumentNullException(nameof(datatype));
      this.defaultValue = defaultValue;
      if (datatype is not IStringConvertible<T> && !flag)
        throw new ArgumentException($"Command datatype {datatype.GetType().Name} must support string conversion.");
    }
    public override object Parse(string text) => Flag ? true : ((IStringConvertible<T>)datatype).FromString(text);
    internal override object GetDefaultValue() => defaultValue;
    public override void Complete(string text, List<string> result) => CompleteChoices(datatype, text, result);
    public T Get(CommandContext context) => context.GetValue(this);
  }

  public static class CommandProperties {
    public static CommandProperty<string> String(string name, string description = null, string defaultValue = "", bool required = false) =>
      new(name, Datatypes.String, description, defaultValue, required);
    public static CommandProperty<int> Int(string name, string description = null, int defaultValue = 0, bool required = false,
      int? min = null, int? max = null) => new(name, new IntDatatype(min: min, max: max), description, defaultValue, required);
    public static CommandProperty<float> Float(string name, string description = null, float defaultValue = 0, bool required = false,
      float? min = null, float? max = null) => new(name, new FloatDatatype(min: min, max: max), description, defaultValue, required);
    public static CommandProperty<T> Enum<T>(string name, string description = null, T defaultValue = default, bool required = false)
      where T : struct, Enum => new(name, Datatypes.Enum<T>(), description, defaultValue, required);
    public static CommandProperty<T> Named<T>(string name, IDatatype<T> datatype, string description = null,
      T defaultValue = default, bool required = false) => new(name, datatype, description, defaultValue, required, named: true);
    public static CommandProperty<bool> Flag(string name, string description = null, bool defaultValue = false) =>
      new(name, Datatypes.Bool, description, defaultValue, flag: true);
  }

  public abstract class Command {
    public abstract string Name { get; }
    public virtual string Description => "";
    public string ParentPath { get; set; }
    public readonly List<CommandProperty> Properties = new();
    public readonly List<Command> Subcommands = new();
    protected void AddProperties(params CommandProperty[] properties) => Properties.AddRange(properties);
    protected void AddSubcommands(params Command[] commands) => Subcommands.AddRange(commands);
    public virtual UniTask<CommandResult> ExecuteAsync(CommandContext context) => UniTask.FromResult(Execute(context));
    public virtual CommandResult Execute(CommandContext context) => CommandResult.Failed("Command not implemented");
    public string GetHelp() {
      var text = new StringBuilder(Name).Append(": ").AppendLine(Description);
      if (Properties.Count > 0) {
        text.AppendLine("Properties:");
        for (var i = 0; i < Properties.Count; i++) {
          var property = Properties[i];
          text.Append("  ").Append(property.Required ? '<' : '[').Append(property.Token)
            .Append(property.Required ? '>' : ']').Append(": ").AppendLine(property.Description);
        }
      }
      if (Subcommands.Count > 0) {
        text.AppendLine("Subcommands:");
        for (var i = 0; i < Subcommands.Count; i++)
          text.Append("  ").Append(Subcommands[i].Name).Append(": ").AppendLine(Subcommands[i].Description);
      }
      return text.ToString().TrimEnd();
    }
    public string GetShortHelp(
      CommandCompletionStyle style, IReadOnlyDictionary<CommandProperty, string> values,
      CommandProperty active, ISet<CommandProperty> malformed, bool atSubcommand,
      string parentPath = "", string error = null
    ) {
      var text = new StringBuilder(string.IsNullOrEmpty(parentPath) ? Name : parentPath + " " + Name);
      if (Subcommands.Count > 0 && atSubcommand) {
        text.Append(" [");
        for (var i = 0; i < Subcommands.Count; i++) { if (i != 0) text.Append('|'); text.Append(Subcommands[i].Name); }
        text.Append(']');
      }
      for (var i = 0; i < Properties.Count; i++) {
        var property = Properties[i];
        string value = null;
        values?.TryGetValue(property, out value);
        text.Append(' ').Append(property.ShortHelp(style, value, property == active, malformed?.Contains(property) == true));
      }
      if (!string.IsNullOrEmpty(error)) text.Append(" <color=").Append(style.error).Append('>').Append(error).Append("</color>");
      return text.ToString();
    }
    public static Command Action(
      string name,
      Func<CommandContext, CommandResult> action,
      string description = "",
      string parentPath = null
    ) => new ActionCommand(name, description, context => UniTask.FromResult(action(context))) {
      ParentPath = parentPath
    };
    public static Command Action(
      string name,
      Func<CommandContext, UniTask<CommandResult>> action,
      string description = "",
      string parentPath = null
    ) => new ActionCommand(name, description, action) { ParentPath = parentPath };
  }

  public sealed class ActionCommand : Command {
    private readonly string _name, _description;
    private readonly Func<CommandContext, UniTask<CommandResult>> _action;
    public ActionCommand(string name, string description, Func<CommandContext, UniTask<CommandResult>> action) {
      _name = name; _description = description; _action = action;
    }
    public override string Name => _name;
    public override string Description => _description;
    public override UniTask<CommandResult> ExecuteAsync(CommandContext context) => _action(context);
  }

  public readonly struct CommandCompletion {
    public readonly IReadOnlyList<string> items; public readonly string help;
    public CommandCompletion(IReadOnlyList<string> items, string help = null) { this.items = items; this.help = help; }
  }
  public sealed class CommandCompletionStyle {
    public static readonly CommandCompletionStyle Default = new();
    public string active = "#FFD54F";
    public string error = "#FF6B6B";
    public string valid = "#69DB7C";
    public string weak = "#8C98A8";
  }
  public interface ICommandSystem {
    IReadOnlyList<Command> Commands { get; }
    UniTask<CommandResult> Execute(string input);
    CommandCompletion Complete(string input);
    string GetHelp(string input = "");
  }
}
