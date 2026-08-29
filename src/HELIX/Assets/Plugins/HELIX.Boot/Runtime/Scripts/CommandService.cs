using System;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using HELIX.Context;
using HELIX.UI;
using HELIX.UI.Console;
using UnityEngine.InputSystem;

namespace HELIX.Boot {
  [Mixable, Managed(typeof(ApplicationScope))]
  public partial class CommandService : ICommandSystem {
    private readonly CommandRegistry _registry = new();
    private readonly List<Command> _commands = new();
    [Inject] private GuiService _gui;
    private readonly List<string> _tokens = new();
    private readonly HashSet<int> _used = new();
    private InputAction _toggleConsoleAction;

    public IReadOnlyList<Command> Commands => _commands;

    [Hook]
    private void OnLoadManaged(ManagedLoadContext context) {
      _registry.roots = _commands;
      context.scope.RegisterBindingObserver(_registry);
      context.Publish<Command>(new HelpCommand(this));
      _gui.commandSystem = this;
      _gui.RebuildNavigation();
      _toggleConsoleAction = new InputAction("Toggle Console", InputActionType.Button, "<Keyboard>/f4");
      _toggleConsoleAction.performed += OnToggleConsole;
      _toggleConsoleAction.Enable();
    }

    private struct CommandRegistryData {
      public Command inferredParent;
    }

    private sealed class CommandRegistry : ManagedRegistry<Command, CommandRegistryData> {
      private readonly List<ManagedId> _ids = new();
      public List<Command> roots;

      protected override void BindingAdded(ManagedScope scope, TypeKey key, ManagedBinding binding) {
        base.BindingAdded(scope, key, binding);
        Rebuild();
      }

      protected override void BindingRemoved(ManagedScope scope, TypeKey key, ManagedBinding binding) {
        if (TryGet(binding.id, out var entry) && entry.data.inferredParent != null)
          entry.data.inferredParent.Subcommands.Remove(entry.value);
        base.BindingRemoved(scope, key, binding);
        Rebuild();
      }

      private void Rebuild() {
        if (roots == null) return;
        roots.Clear();
        _ids.Clear();
        foreach (var pair in items) _ids.Add(pair.Key);
        for (var i = 0; i < _ids.Count; i++) {
          var id = _ids[i];
          var entry = items[id];
          var data = entry.data;
          if (data.inferredParent != null) {
            data.inferredParent.Subcommands.Remove(entry.value);
            data.inferredParent = null;
            SetData(id, data);
          }
        }

        for (var i = 0; i < _ids.Count; i++) {
          var id = _ids[i];
          var command = items[id].value;
          if (string.IsNullOrWhiteSpace(command.ParentPath)) {
            roots.Add(command);
            continue;
          }
          var parent = FindByPath(command.ParentPath);
          if (parent == null) {
            roots.Add(command);
            continue;
          }
          if (parent.Subcommands.Contains(command)) continue;
          parent.Subcommands.Add(command);
          var data = items[id].data;
          data.inferredParent = parent;
          SetData(id, data);
        }
      }

      private Command FindByPath(string path) {
        foreach (var pair in items) {
          var command = pair.Value.value;
          var fullPath = string.IsNullOrWhiteSpace(command.ParentPath)
            ? command.Name
            : command.ParentPath.Trim() + " " + command.Name;
          if (fullPath.Equals(path.Trim(), StringComparison.OrdinalIgnoreCase)) return command;
        }
        return null;
      }
    }

    [Hook]
    private void OnDispose() {
      if (_toggleConsoleAction != null) {
        _toggleConsoleAction.performed -= OnToggleConsole;
        _toggleConsoleAction.Disable();
        _toggleConsoleAction.Dispose();
        _toggleConsoleAction = null;
      }
      if (ReferenceEquals(_gui.commandSystem, this)) _gui.commandSystem = null;
    }

    private void OnToggleConsole(InputAction.CallbackContext context) => _gui.host?.ToggleCommandConsole();

    public async UniTask<CommandResult> Execute(string input) {
      try {
        Tokenize(input, _tokens);
        if (_tokens.Count == 0) return CommandResult.Failed("Empty command");
        var command = Find(_commands, _tokens[0]);
        if (command == null) return CommandResult.Failed($"Unknown command: {_tokens[0]}");
        var index = 1;
        while (index < _tokens.Count) {
          var child = Find(command.Subcommands, _tokens[index]);
          if (child == null) break;
          command = child;
          index++;
        }
        var commandContext = new CommandContext(input, command);
        var result = ParseProperties(command, commandContext, index);
        if (result.HasValue) return result.Value;
        return await command.ExecuteAsync(commandContext);
      } catch (Exception exception) { return CommandResult.Failed(exception.Message); }
    }

    private CommandResult? ParseProperties(Command command, CommandContext context, int first) {
      _used.Clear();
      for (var i = first; i < _tokens.Count; i++) {
        var token = _tokens[i];
        if (token.Length == 0 || token[0] != '-') continue;
        var property = FindProperty(command.Properties, token);
        if (property == null) return CommandResult.Failed($"Unknown property: {token}");
        _used.Add(i);
        if (property.Flag) {
          context.SetValue(property, property.Parse("true"));
          continue;
        }
        if (++i >= _tokens.Count) return CommandResult.Failed($"Missing value for property: {token}");
        context.SetValue(property, property.Parse(_tokens[i]));
        _used.Add(i);
      }
      var positional = 0;
      for (var i = first; i < _tokens.Count; i++) {
        if (_used.Contains(i)) continue;
        while (positional < command.Properties.Count &&
          (command.Properties[positional].Named || command.Properties[positional].Flag)) positional++;
        if (positional >= command.Properties.Count) return CommandResult.Failed($"Unexpected argument: {_tokens[i]}");
        var property = command.Properties[positional++];
        context.SetValue(property, property.Parse(_tokens[i]));
      }
      for (var i = 0; i < command.Properties.Count; i++)
        if (command.Properties[i].Required && !context.HasValue(command.Properties[i]))
          return CommandResult.Failed($"Missing required property: {command.Properties[i].Name}");
      return null;
    }

    public string GetHelp(string input = "") {
      Tokenize(input, _tokens);
      if (_tokens.Count == 0) {
        var builder = new StringBuilder("Available commands:\n");
        for (var i = 0; i < _commands.Count; i++)
          builder.Append("  ").Append(_commands[i].Name).Append(": ").AppendLine(_commands[i].Description);
        return builder.ToString().TrimEnd();
      }
      var command = Find(_commands, _tokens[0]);
      if (command == null) return $"No command matching '{_tokens[0]}'";
      for (var i = 1; i < _tokens.Count; i++) {
        var child = Find(command.Subcommands, _tokens[i]);
        if (child == null) break;
        command = child;
      }
      return command.GetHelp();
    }

    public CommandCompletion Complete(string input) {
      Tokenize(input, _tokens);
      var result = new List<string>();
      var trailing = input != null && input.EndsWith(" ", StringComparison.Ordinal);
      if (_tokens.Count == 0 || _tokens.Count == 1 && !trailing) {
        var search = _tokens.Count == 0 ? "" : _tokens[0];
        CompleteCommands(_commands, search, result);
        return new CommandCompletion(result);
      }
      var command = Find(_commands, _tokens[0]);
      if (command == null) return new CommandCompletion(result);
      var parentPath = "";
      var index = 1;
      while (index < _tokens.Count - (trailing ? 0 : 1)) {
        var child = Find(command.Subcommands, _tokens[index]);
        if (child == null) break;
        parentPath = string.IsNullOrEmpty(parentPath) ? command.Name : parentPath + " " + command.Name;
        command = child;
        index++;
      }
      var current = trailing ? "" : _tokens[_tokens.Count - 1];
      var limit = _tokens.Count - (trailing ? 0 : 1);
      var values = new Dictionary<CommandProperty, string>();
      var malformed = new HashSet<CommandProperty>();
      var used = new HashSet<int>();
      CommandProperty active = null;
      string error = null;

      for (var i = index; i < limit; i++) {
        var token = _tokens[i];
        if (!token.StartsWith("-", StringComparison.Ordinal)) continue;
        var property = FindProperty(command.Properties, token);
        if (property == null) {
          error = $"Unknown property: {token}";
          continue;
        }
        used.Add(i);
        if (property.Flag) {
          values[property] = "true";
          continue;
        }
        if (i + 1 >= limit) {
          active = property;
          continue;
        }
        var value = _tokens[++i];
        used.Add(i);
        values[property] = value;
        try { property.Parse(value); } catch { malformed.Add(property); }
      }

      var positionalIndex = 0;
      for (var i = index; i < limit; i++) {
        if (used.Contains(i)) continue;
        var property = PositionalAt(command.Properties, positionalIndex++);
        if (property == null) {
          error = $"Unexpected argument: {_tokens[i]}";
          continue;
        }
        values[property] = _tokens[i];
        try { property.Parse(_tokens[i]); } catch { malformed.Add(property); }
      }

      if (active == null && limit > index) {
        var previous = _tokens[limit - 1];
        var named = FindProperty(command.Properties, previous);
        if (named is { Named: true, Flag: false }) active = named;
      }

      if (active == null) active = PositionalAt(command.Properties, positionalIndex);
      if (limit == index) CompleteCommands(command.Subcommands, current, result);
      if (active != null) active.Complete(current, result);
      else if (limit != index) CompleteCommands(command.Subcommands, current, result);

      for (var i = 0; i < command.Properties.Count; i++) {
        var property = command.Properties[i];
        if ((!property.Named && !property.Flag) || values.ContainsKey(property)) continue;
        if (property.Token.StartsWith(current, StringComparison.OrdinalIgnoreCase) && !result.Contains(property.Token))
          result.Add(property.Token);
      }

      var help = command.GetShortHelp(
        CommandCompletionStyle.Default,
        values,
        active,
        malformed,
        used.Count == 0 && positionalIndex == 0,
        parentPath,
        error
      );
      if (!string.IsNullOrEmpty(active?.Description)) help = active.Description + "\n" + help;
      return new CommandCompletion(result, help);
    }

    private static CommandProperty PositionalAt(IReadOnlyList<CommandProperty> properties, int index) {
      for (var i = 0; i < properties.Count; i++) {
        if (properties[i].Named || properties[i].Flag) continue;
        if (index-- == 0) return properties[i];
      }
      return null;
    }

    private static Command Find(IReadOnlyList<Command> commands, string name) {
      for (var i = 0; i < commands.Count; i++)
        if (commands[i].Name.Equals(name, StringComparison.OrdinalIgnoreCase))
          return commands[i];
      return null;
    }

    private static CommandProperty FindProperty(IReadOnlyList<CommandProperty> properties, string token) {
      for (var i = 0; i < properties.Count; i++)
        if (properties[i].Token.Equals(token, StringComparison.OrdinalIgnoreCase))
          return properties[i];
      return null;
    }

    private static void CompleteCommands(IReadOnlyList<Command> commands, string search, List<string> result) {
      for (var i = 0; i < commands.Count; i++)
        if (commands[i].Name.StartsWith(search, StringComparison.OrdinalIgnoreCase))
          result.Add(commands[i].Name);
    }

    internal static void Tokenize(string input, List<string> result) {
      result.Clear();
      if (string.IsNullOrWhiteSpace(input)) return;
      var builder = new StringBuilder();
      var quote = false;
      var escape = false;
      for (var i = 0; i < input.Length; i++) {
        var c = input[i];
        if (escape) {
          builder.Append(c);
          escape = false;
          continue;
        }
        if (c == '\\') {
          escape = true;
          continue;
        }
        if (c == '"') {
          quote = !quote;
          continue;
        }
        if (char.IsWhiteSpace(c) && !quote) {
          if (builder.Length > 0) {
            result.Add(builder.ToString());
            builder.Clear();
          }
          continue;
        }
        builder.Append(c);
      }
      if (escape) builder.Append('\\');
      if (builder.Length > 0) result.Add(builder.ToString());
    }
  }

  [AttributeUsage(AttributeTargets.Method)]
  public class CommandAttribute : Attribute {
    public CommandAttribute(
      string name = null,
      string description = null,
      string parent = null
    ) { }
  }

  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public sealed class RegisterCommandAttribute : Attribute { }

  public sealed class HelpCommand : Command {
    private readonly ICommandSystem _system;
    private readonly CommandProperty<string> _command = CommandProperties.String("command", "Command path to inspect.");

    public HelpCommand(ICommandSystem system) {
      _system = system;
      AddProperties(_command);
    }

    public override string Name => "help";
    public override string Description => "Displays information about available commands.";

    public override CommandResult Execute(CommandContext context) {
      return CommandResult.Successful(_system.GetHelp(_command.Get(context)));
    }
  }
}
