// Port of Boot.hix: console commands and option registration.
using System;
using System.Collections.Immutable;
using Helix.Weave;

namespace HELIX.Weave;

/// <summary>Port of <c>derivation mixin HELIX.Boot.NamedArgAttribute</c>.</summary>
[Mixin(Unit.Derivation, Attribute = "HELIX.Boot.NamedArgAttribute")]
public sealed class NamedArg : Attribute {
  [Weave]
  static DerivedProperty Derive(DerivationTarget t, NamedArg a, DerivedProperty p) =>
    p with {
      Configuration = $"datatype.GetProperty({Lit.Of(p.Name)})" +
                      ".Modifiers.Add(NamedArgModifier.Instance);",
    };
}

/// <summary>
/// Port of <c>mixin HELIX.Boot.CommandAttribute</c>. Two passes: the prelude derives the
/// argument struct and emits it, the late pass registers the bridge. Hix re-derived the
/// parameter model in its late block because it had no way to hand data across; the carry
/// record is that way.
/// </summary>
[Mixin(Unit.Method, Attribute = "HELIX.Boot.CommandAttribute")]
public sealed class Command : Attribute {
  public Command(string name = null, string description = null, string parent = null) {
    Name = name;
    Description = description;
    Parent = parent;
  }

  public string Name { get; init; }
  public string Description { get; init; }
  public string Parent { get; init; }

  public sealed record Carry(string StructName, string Invoke, string CommandName,
                             string Description, string Parent);

  [Prelude]
  static Carry Prelude(MethodTarget t, Command a) {
    if (t.IsGenericMethod) {
      t.Report("HX0300", "a command cannot be generic");
      return null;
    }

    var structName = $"{t.Name}_Args";
    var props = t.DeriveParameters();
    Shared.EmitGeneratedStructure(t, structName, props, datatype: true);

    var arguments = Code.Join(props, p => $"{p.Modifier}args.{p.Name}");
    return new Carry(
      structName,
      $"args => {t.Name}({arguments})",
      Lit.Of(a.Name ?? t.Name),
      Lit.Of(a.Description),
      Lit.Of(a.Parent));
  }

  [Late]
  static void Late(LateTarget t, Carry c) {
    if (c is null) return;
    t.Using("HELIX.Boot");
    t.Inject(Targets.LoadManagedLate, 0,
      $"context.PublishBind(typeof(HELIX.UI.Console.Command), null, " +
      $"CommandBridge<{c.StructName}>.Create({c.StructName}.Datatype, {c.Invoke}, " +
      $"{c.CommandName}, {c.Description}, {c.Parent}));");
  }
}

[Mixin(Unit.Member, Attribute = "HELIX.Boot.RegisterCommandAttribute")]
public sealed class RegisterCommand : Attribute {
  [Weave]
  static void Apply(MemberTarget t, RegisterCommand a) {
    t.Inject(Targets.ConfigureManaged, -1,
      "registration.Publication(typeof(HELIX.UI.Console.Command), null, true);");
    t.Inject(Targets.LoadManagedLate, 1,
      $"context.PublishBind(typeof(HELIX.UI.Console.Command), null, {t.Name});");
  }
}

[Mixin(Unit.Member, Attribute = "HELIX.Boot.RegisterOptionAttribute")]
public sealed class RegisterOption : Attribute {
  public RegisterOption(string path = null) => Path = path;
  public string Path { get; init; }

  [Weave]
  static void Apply(MemberTarget t, RegisterOption a) {
    if (a.Path is not null) {
      var path = Lit.Of(a.Path);
      t.Inject(Targets.BootInit, -1, $"{t.Name}.SetInferredPath({path});");
      t.Inject(Targets.ConfigureManaged, -1,
        $"registration.Publication(typeof({t.Type}), {path}, true);");
      t.Inject(Targets.LoadManagedLate, 1,
        $"context.PublishBind(typeof({t.Type}), {path}, {t.Name});");
      return;
    }

    t.Inject(Targets.BootInit, -1, $"{t.Name}.SetInferredName({Lit.Of(t.Name)});");
    t.Inject(Targets.ConfigureManaged, -1,
      "registration.Publication(typeof(HELIX.Boot.Option), null, true);");
    t.Inject(Targets.LoadManagedLate, 1,
      $"context.PublishBind(typeof(HELIX.Boot.Option), null, {t.Name});");
  }
}
