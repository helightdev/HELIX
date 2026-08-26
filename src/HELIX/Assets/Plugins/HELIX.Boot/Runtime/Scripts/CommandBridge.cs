using System;
using System.Collections.Generic;
using HELIX.Prose;
using HELIX.UI.Console;

namespace HELIX.Boot {
  /// <summary>Marks a structure property as a named command argument.</summary>
  public sealed class NamedArgModifier : IProseModifier {
    public static readonly NamedArgModifier Instance = new();
    private NamedArgModifier() { }
  }

  /// <summary>Datatype configuration helpers used by command-related mixins.</summary>
  public static class CommandBridge {
    public static void Named<T>(StructureDatatype<T> datatype, string fieldName) {
      if (datatype == null) throw new ArgumentNullException(nameof(datatype));
      if (string.IsNullOrWhiteSpace(fieldName))
        throw new ArgumentException("A field name is required.", nameof(fieldName));
      for (var i = 0; i < datatype.Properties.Count; i++) {
        var property = datatype.Properties[i];
        if (!string.Equals(property.FieldName, fieldName, StringComparison.Ordinal)) continue;
        for (var modifierIndex = 0; modifierIndex < property.Modifiers.Count; modifierIndex++)
          if (property.Modifiers[modifierIndex] is NamedArgModifier)
            return;
        property.Modifiers.Add(NamedArgModifier.Instance);
        return;
      }
      throw new ArgumentException(
        $"Structure datatype '{datatype.Name}' has no property named '{fieldName}'.",
        nameof(fieldName)
      );
    }
  }

  /// <summary>Creates console commands from generated structure datatypes.</summary>
  public static class CommandBridge<T> {
    public static Command Create(
      StructureDatatype<T> datatype,
      Func<T, CommandResult> callable,
      string name, string description, string parentPath
    ) {
      if (datatype == null) throw new ArgumentNullException(nameof(datatype));
      if (callable == null) throw new ArgumentNullException(nameof(callable));

      var collector = new PropertyCollector();
      var value = default(T);
      for (var i = 0; i < datatype.Properties.Count; i++)
        datatype.Properties[i].Visit(ref value, ref collector);

      var command = Command.Action(name ?? datatype.Name, context => {
        var arguments = default(T);
        for (var i = 0; i < collector.Bindings.Count; i++)
          collector.Bindings[i].Assign(context, ref arguments);
        return callable(arguments);
      }, description ?? "", parentPath);
      for (var i = 0; i < collector.Bindings.Count; i++)
        command.Properties.Add(collector.Bindings[i].CommandProperty);
      return command;
    }

    private interface IPropertyBinding {
      CommandProperty CommandProperty { get; }
      void Assign(CommandContext context, ref T value);
    }

    private sealed class PropertyBinding<TValue> : IPropertyBinding {
      private readonly StructurePropertyDatatype<T, TValue> _property;
      private readonly CommandProperty<TValue> _commandProperty;

      internal PropertyBinding(StructurePropertyDatatype<T, TValue> property) {
        _property = property;
        var defaultValue = property.DefaultValue is TValue typed ? typed : default;
        if (property.Datatype is IDatatype<bool>) {
          _commandProperty = new CommandProperty<TValue>(
            property.FieldName,
            property.Datatype,
            defaultValue: defaultValue,
            required: false,
            flag: true
          );
        } else {
          _commandProperty = new CommandProperty<TValue>(
            property.FieldName,
            property.Datatype,
            defaultValue: defaultValue,
            required: property.Required,
            named: IsNamed(property.Modifiers)
          );
        }
      }

      public CommandProperty CommandProperty => _commandProperty;
      public void Assign(CommandContext context, ref T value) =>
        _property.SetValue(ref value, _commandProperty.Get(context));
    }

    private sealed class PropertyCollector : IStructurePropertyVisitor<T> {
      internal readonly List<IPropertyBinding> Bindings = new();

      public void Visit<TValue>(
        ref T structure,
        StructurePropertyDatatype<T, TValue> property
      ) => Bindings.Add(new PropertyBinding<TValue>(property));
    }

    private static bool IsNamed(IList<IProseModifier> modifiers) {
      for (var i = 0; i < modifiers.Count; i++)
        if (modifiers[i] is NamedArgModifier)
          return true;
      return false;
    }
  }

  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter)]
  [MixinExpression(
    @"
@CALL<AddStructurePropertyModifier> NamedArgModifier.Instance
"
  )]
  public sealed class NamedArgAttribute : Attribute { }
}
