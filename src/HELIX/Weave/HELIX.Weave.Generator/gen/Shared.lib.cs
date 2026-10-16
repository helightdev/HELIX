// The RequireX family and the structure emitters shared across mixins. Ported from the helper
// funcs at the top of Core.hix, Context.hix and Compose.hix.
using System.Collections.Immutable;
using System.Linq;
using Helix.Weave;

namespace HELIX.Weave;

public static class Shared {
  // Shared keys, not slots: a prelude is cached per symbol, so it cannot know whether another
  // symbol already emitted a block. Every symbol that needs one emits it, tagged with its key,
  // and the merge keeps a single copy per type.
  private const string CompanionKey = "HELIX.Companion";
  private const string HandlerKey = "HELIX.EventHandlerList";
  private const string TypeIdKey = "HELIX.CompositionTypeId";
  private const string CompositionIdKey = "HELIX.CompositionId";
  private const string InputStateKey = "HELIX.InputState";
  private const string WriteContextKey = "HELIX.WriteContext";

  /// <summary>The companion static class every Compose type accumulates members into.</summary>
  public static string Companion(SymbolTarget t) => t.SelfName + "_Companion";

  /// <summary>Hix's <c>RequireCompanion</c>: declare the empty companion once.</summary>
  public static void RequireCompanion(SymbolTarget t) =>
    t.Shared(CompanionKey, target => target.File($"public static partial class {Companion(t)} {{ }}"));

  /// <summary>
  /// Hix's <c>DeclareCompanion</c>: add members to it. The class is partial, so each block is a
  /// declaration of its own and no deduplication is wanted here.
  /// </summary>
  public static void DeclareCompanion(SymbolTarget t, string body) =>
    t.File($$"""
      public static partial class {{Companion(t)}} {
        {{body}}
      }
      """);

  public static void RequireCompositionTypeId(SymbolTarget t) =>
    t.Shared(TypeIdKey, target => DeclareCompanion(t,
      $"public static readonly ushort typeId = CompositionId.GetTypeId({Lit.Of(t.SelfName)});"));

  public static void RequireCompositionId(SymbolTarget t) =>
    t.Shared(CompositionIdKey, target => DeclareCompanion(t,
      $"public static readonly ushort compositionId = CompositionId.GetCompositionId({Lit.Of(t.SelfName)});"));

  public static void RequireInputState(SymbolTarget t) => t.Shared(InputStateKey, target => {
    target.Using("HELIX.Compose");
    target.Implements("IStateHolder");
    target.Class("""
      private State inputState;
      public ref State InputState => ref inputState;
      """);
  });

  /// <summary>Only one write-context handler per type; the merge reports a second one.</summary>
  public static void ClaimWriteContext(SymbolTarget t) =>
    t.Unique(WriteContextKey, "HX0400", "a type can declare only one write-context handler");

  /// <summary>Hix's <c>RequireHandler</c>.</summary>
  public static void RequireHandler(SymbolTarget t) => t.Shared(HandlerKey, target => {
    target.Using("System");
    target.Implements("IEventListener");
    target.Class("""
      [NonSerializedAttribute]
      protected readonly EventHandlerList eventHandlerList = EventHandlerList.Create();
      EventHandlerList IEventListener.HandlerList => eventHandlerList;
      """);
    target.Inject(Targets.Dispose, 1, "eventHandlerList.UnregisterAll();");
  });

  /// <summary>
  /// Whether the owning type is a managed component. A question about the type, not state another
  /// mixin left behind — which also removes Hix's dependence on attribute evaluation order.
  /// </summary>
  public static bool IsManagedComponent(SymbolTarget t) =>
    t.Owner.HasAttribute("HELIX.Context.ManagedAttribute");

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
