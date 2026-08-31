using System;
using System.Collections.Generic;
using System.Linq;
using global::MixinLanguage;
using global::MixinLanguage.Compiler;

namespace MixinLanguage.Analysis;

public enum MixinOperandKind { None, Value, Boolean }
public enum MixinArgumentRole {
  Value, Identifier, Label, Function, OutputTarget, Expression, BooleanExpression, CSharpType
}
public enum MixinReceiverKind { Any, Text, Table, Symbol, Type, Boolean }

public sealed record MixinDirectiveDescriptor(
  string Name, int MinimumArguments, int MaximumArguments, MixinOperandKind OperandKind,
  IReadOnlyList<MixinArgumentRole> ArgumentRoles, string Documentation
);

public sealed record MixinFunctionDescriptor(
  string Name, int MinimumArguments, int MaximumArguments, bool IsPredicate,
  MixinReceiverKind ReceiverKind, IReadOnlyList<MixinArgumentRole> ArgumentRoles,
  string Documentation
);

/// <summary>Authoritative static requirements of the executable mixin language.</summary>
public static class MixinLanguageCatalog {
  public static IReadOnlyList<MixinDirectiveDescriptor> Directives { get; } = [
    .. DirectiveLibrary.EnumerateLanguageDefinitions().Select(value => new MixinDirectiveDescriptor(
      value.Name, value.MinimumArguments, value.MaximumArguments, Convert(value.OperandKind),
      value.ArgumentTypes.Select(Convert).ToArray(), value.Documentation
    ))
  ];
  public static IReadOnlyList<MixinFunctionDescriptor> Functions { get; } = [
    .. FunctionLibrary.Enumerate().Select(value => new MixinFunctionDescriptor(
      value.Name, value.MinimumArguments, value.MaximumArguments, value.IsPredicate,
      Receiver(value.ReceiverType), value.ArgumentTypes.Select(Convert).ToArray(), value.Documentation
    ))
  ];
  public static IReadOnlyList<string> Roots { get; } = [
    "target", "this", "attr", "arg", "var", "tar", "local", "true", "false", "null", "table", "param", "carry"
  ];
  public static IReadOnlyList<string> OutputTargets { get; } = [
    "TARGET", "CLASS", "FILE", "EXTENDS", "IMPLEMENTS", "ANNOTATION"
  ];

  public static bool TryGetDirective(string name, out MixinDirectiveDescriptor descriptor) {
    descriptor = Directives.FirstOrDefault(item => string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase));
    return descriptor is not null;
  }
  public static bool TryGetFunction(string name, out MixinFunctionDescriptor descriptor) {
    descriptor = Functions.FirstOrDefault(item => string.Equals(item.Name, name, StringComparison.Ordinal));
    return descriptor is not null;
  }
  private static MixinOperandKind Convert(DirectiveOperandKind kind) => kind switch {
    DirectiveOperandKind.Value => MixinOperandKind.Value, DirectiveOperandKind.Boolean => MixinOperandKind.Boolean,
    _ => MixinOperandKind.None
  };
  private static MixinReceiverKind Receiver(MixinLanguageValueKind kind) => kind switch {
    MixinLanguageValueKind.Text => MixinReceiverKind.Text,
    MixinLanguageValueKind.Table => MixinReceiverKind.Table,
    MixinLanguageValueKind.Symbol => MixinReceiverKind.Symbol,
    MixinLanguageValueKind.Type => MixinReceiverKind.Type,
    MixinLanguageValueKind.Boolean => MixinReceiverKind.Boolean,
    _ => MixinReceiverKind.Any
  };
  private static MixinArgumentRole Convert(MixinLanguageValueKind kind) => kind switch {
    MixinLanguageValueKind.Identifier => MixinArgumentRole.Identifier,
    MixinLanguageValueKind.Label => MixinArgumentRole.Label,
    MixinLanguageValueKind.Function => MixinArgumentRole.Function,
    MixinLanguageValueKind.OutputTarget => MixinArgumentRole.OutputTarget,
    MixinLanguageValueKind.Expression => MixinArgumentRole.Expression,
    MixinLanguageValueKind.Boolean => MixinArgumentRole.BooleanExpression,
    MixinLanguageValueKind.CSharpType => MixinArgumentRole.CSharpType,
    _ => MixinArgumentRole.Value
  };
}
