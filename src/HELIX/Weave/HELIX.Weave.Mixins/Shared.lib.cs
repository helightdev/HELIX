// The RequireX family and the structure emitters shared across mixins. Ported from the helper
// funcs at the top of Core.hix, Context.hix and Compose.hix.
using System.Collections.Immutable;
using System.Linq;
using Helix.Weave;

namespace HELIX.Weave;

public static class Shared {
  private static readonly Slot<bool> HasCompanion = new(Scope.Type);
  private static readonly Slot<string> CompanionName = new(Scope.Type);
  private static readonly Slot<bool> HasHandler = new(Scope.Type);
  private static readonly Slot<bool> HasTypeId = new(Scope.Type);
  private static readonly Slot<bool> HasCompositionId = new(Scope.Type);
  private static readonly Slot<bool> HasInputState = new(Scope.Type);
  private static readonly Slot<bool> HasWriteContext = new(Scope.Type);
  private static readonly Slot<bool> IsComponent = new(Scope.Type);

  /// <summary>The companion static class every Compose type accumulates members into.</summary>
  public static string Companion(SymbolTarget t) {
    var existing = t.Get(CompanionName);
    if (existing is not null) return existing;
    var name = t.SelfName + "_Companion";
    t.Set(CompanionName, name);
    return name;
  }

  /// <summary>Hix's <c>RequireCompanion</c>: declare the empty companion once.</summary>
  public static void RequireCompanion(SymbolTarget t) {
    var name = Companion(t);
    if (t.Once(HasCompanion)) t.File($"public static partial class {name} {{ }}");
  }

  /// <summary>Hix's <c>DeclareCompanion</c>: add members to it. Partial, so repeats are fine.</summary>
  public static void DeclareCompanion(SymbolTarget t, string body) {
    var name = Companion(t);
    t.Once(HasCompanion);
    t.File($$"""
      public static partial class {{name}} {
        {{body}}
      }
      """);
  }

  public static void RequireCompositionTypeId(SymbolTarget t) {
    if (!t.Once(HasTypeId)) return;
    DeclareCompanion(t,
      $"public static readonly ushort typeId = CompositionId.GetTypeId({Lit.Of(t.SelfName)});");
  }

  public static void RequireCompositionId(SymbolTarget t) {
    if (!t.Once(HasCompositionId)) return;
    DeclareCompanion(t,
      $"public static readonly ushort compositionId = CompositionId.GetCompositionId({Lit.Of(t.SelfName)});");
  }

  public static void RequireInputState(SymbolTarget t) {
    if (!t.Once(HasInputState)) return;
    t.Using("HELIX.Compose");
    t.Implements("IStateHolder");
    t.Class("""
      private State inputState;
      public ref State InputState => ref inputState;
      """);
  }

  /// <summary>Only one write-context handler per type; Hix asserted this.</summary>
  public static bool ClaimWriteContext(SymbolTarget t) => t.Once(HasWriteContext);

  /// <summary>Hix's <c>RequireHandler</c>.</summary>
  public static void RequireHandler(SymbolTarget t) {
    if (!t.Once(HasHandler)) return;
    t.Using("System");
    t.Implements("IEventListener");
    t.Class("""
      [NonSerializedAttribute]
      protected readonly EventHandlerList eventHandlerList = EventHandlerList.Create();
      EventHandlerList IEventListener.HandlerList => eventHandlerList;
      """);
    t.Inject(Targets.Dispose, 1, "eventHandlerList.UnregisterAll();");
  }

  /// <summary>Set by ManagedAttribute; read by mixins that register against the container.</summary>
  public static void MarkComponent(SymbolTarget t) => t.Set(IsComponent, true);
  public static bool IsManagedComponent(SymbolTarget t) => t.Get(IsComponent);

  /// <summary>Hix's <c>BuildDatatype</c>.</summary>
  public static string BuildDatatype(string type, string name, ImmutableArray<DerivedProperty> props) {
    var properties = Code.Join(props, p => p.Datatype, ",\n      ");
    var configurations = Code.Join(props.Where(p => p.Configuration.Length > 0), p => p.Configuration, "\n  ");
    return $$"""
      public static readonly global::HELIX.StructureDatatype<{{type}}> Datatype =
        new global::HELIX.ConfigurableStructureDatatype<{{type}}>(
          new global::HELIX.StructureDatatype<{{type}}>(
            {{Lit.Of(name)}},
            new global::HELIX.StructurePropertyDatatype<{{type}}>[] {
              {{properties}}
            }),
          datatype => ConfigureDatatype(datatype)).Datatype;

      static void ConfigureDatatype(global::HELIX.StructureDatatype<{{type}}> datatype) {
        {{configurations}}
      }
      """;
  }

  /// <summary>The per-property datatype entry. Hix's <c>SelectDatatypeProperty</c>.</summary>
  public static string DatatypeProperty(string structureType, DerivedProperty p) =>
    $"new global::HELIX.StructurePropertyDatatype<{structureType}, {p.Type.Qualified}>(" +
    $"{Lit.Of(p.Name)}, {DatatypeOf(p)}, " +
    $"(ref {structureType} value) => value.{p.Name}, " +
    $"(ref {structureType} value, {p.Type.Qualified} propertyValue) => value.{p.Name} = propertyValue, " +
    $"required: {Lit.Of(p.Required)}, defaultValue: {p.Default})";

  /// <summary>Hix's <c>EmitGeneratedStructure</c>: a struct built out of derived properties.</summary>
  public static void EmitGeneratedStructure(SymbolTarget t, string name,
                                            ImmutableArray<DerivedProperty> props, bool datatype) {
    var fields = Code.Join(props, p => $"public {p.Type.Qualified} {p.Name};", "\n  ");
    var parameters = Code.Join(props, p => p.Parameter);
    var assignments = Code.Join(props, p => p.Assignment, "\n    ");
    var unsafeness = props.Any(p => p.Unsafe) ? "unsafe " : "";

    var constructor = props.Length == 0
      ? ""
      : $$"""
        public {{name}}({{parameters}}) {
          {{assignments}}
        }
        """;

    var datatypeCode = datatype ? BuildDatatype(name, name, DatatypeEntries(name, props)) : "";

    t.Class($$"""
      public {{unsafeness}}struct {{name}} {
        {{fields}}
        {{constructor}}
        {{datatypeCode}}
      }
      """);
  }

  /// <summary>
  /// The datatype a property maps to: whatever a derivation set, else the default for its type.
  /// Hix seeded this inside SeedProp; keeping it here means the weave core stays free of any
  /// opinion about what a datatype is.
  /// </summary>
  public static string DatatypeOf(DerivedProperty p) =>
    p.Datatype.Length > 0 ? p.Datatype : Datatypes.For(p.Type, p.Type.Qualified);

  /// <summary>Fills each property's Datatype fragment against the owning structure type.</summary>
  public static ImmutableArray<DerivedProperty> DatatypeEntries(
    string structureType, ImmutableArray<DerivedProperty> props) =>
    props.Select(p => p with { Datatype = DatatypeProperty(structureType, p) }).ToImmutableArray();
}
