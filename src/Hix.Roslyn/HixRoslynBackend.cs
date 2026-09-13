using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Hix.Runtime;
using Hix.Functions;
namespace Hix.Roslyn;

/// <summary>Roslyn semantic services. Each invocation owns its semantic value caches and VM state.</summary>
public class HixRoslynBackend : HixBackend {
  public static readonly HixOpaqueTypeDefinition SemanticType = new("hix.roslyn.semantic", "roslynValue", Documentation:
    "A Roslyn semantic value, symbol, or attribute supplied by the host.");
  protected override void RegisterRoots(IDictionary<string, HixBackendRoot> roots) {
    base.RegisterRoots(roots);
    foreach (var name in new[] {"this", "target", "attr"}) roots.Add(name, new(name, HixValueKind.Opaque, true) {
      Pattern = SemanticType.Construct()
    });
  }
  protected override void RegisterFunctions(FunctionSignatureRegistryBuilder functions) {

    functions.DefineOpaqueType(SemanticType.Identity, SemanticType.Name, documentation: SemanticType.Documentation);
    base.RegisterFunctions(functions);
    functions.AddLibrary("""
      %description("Returns the type symbol associated with a semantic value, or null if unavailable.")
      pure func <type>(%roslynValue() any arg0) -> %roslynValue() any external;

      %description("Renders the associated type name with generic type parameters.")
      pure func <fullName>(%roslynValue() any arg0) -> string external;

      %description("Returns the members of a named type as a tuple of semantic values.")
      pure func <members>(%roslynValue() any arg0) -> tuple external;

      %description("Returns the parameters of a method or delegate as a tuple.")
      pure func <parameters>(%roslynValue() any arg0) -> tuple external;

      %description("Renders a nullable form of the type represented by the value.")
      pure func <nullableType>(%roslynValue() any arg0) -> %roslynValue() any external;

      %description("Renders a constant or scalar value as a valid C# literal.")
      pure func <csharpLiteral>(string? arg0) -> string external;
      pure func <csharpLiteral>(null arg0) -> string external;
      pure func <csharpLiteral>(number arg0) -> string external;
      pure func <csharpLiteral>(bool arg0) -> string external;
      pure func <csharpLiteral>(opaque arg0) -> string external;

      %description("Renders a generic type name using the supplied type arguments.")
      pure func <makeGeneric>(%roslynValue() any arg0, %roslynValue() any arg1) -> string external;

      %description("Returns the declared accessibility of a symbol as lowercase text.")
      pure func <visibility>(%roslynValue() any arg0) -> string external;

      %description("Escapes a name so it can be used as a C# identifier.")
      pure func <identifier>(string arg0) -> string external;

      %description("Parses a time value and renders its numeric representation for generated code.")
      pure func <floatTime>(string arg0) -> string external;
      pure func <floatTime>(number arg0) -> string external;
      pure func <floatTime>(null arg0) -> string external;
      pure func <floatTime>(opaque arg0) -> string external;

      %description("Returns attributes attached to a symbol.")
      pure func <attributes>(%roslynValue() any arg0) -> tuple external;

      %description("Returns attributes assignable to the specified attribute type.")
      pure func <attributesOf>(%roslynValue() any arg0, %reference("CSharpType") string arg1) -> tuple external;

      %description("Returns attributes with exactly the specified attribute type.")
      pure func <attributesOfExact>(%roslynValue() any arg0, %reference("CSharpType") string arg1) -> tuple external;

      %description("Returns the first matching attribute, or null when absent.")
      pure func <attributeOf>(%roslynValue() any arg0, %reference("CSharpType") string arg1) -> any external;

      %requiresPrelude()
      %description("Returns types in the current compilation carrying the named attribute, including nested types. Results are deduplicated and sorted.")
      func <collectAnnotatedTypes>(%reference("CSharpType") string arg0) -> tuple external;

      %description("Returns the namespace of a type symbol.")
      pure func <namespace>(%roslynValue() any arg0) -> string external;

      %description("Tests whether the symbol is accessible from the current target.")
      pure func <accessible>(any arg0) -> bool external;

      %description("Renders a semantic value for wiring into generated source. Methods cannot be wired directly.")
      pure func <wire>(any arg0, any arg1) -> string external;

      %description("Renders the backend signature of a semantic value.")
      pure func <signature>(any arg0, any arg1) -> bool external;

      %description("Tests whether a semantic value can be wired into generated code.")
      pure func <wireable>(any arg0, any arg1) -> bool external;

      %description("Tests whether the semantic value matches the specified C# type name.")
      pure func <is>(any arg0, %reference("CSharpType") string arg1) -> bool external roslyn_is;

      %description("Tests whether a semantic value exposes the specified member.")
      pure func <has>(any arg0, any arg1) -> bool external roslyn_has;

      %description("Tests whether the value refers to the current target type.")
      pure func <isSelf>(any arg0) -> bool external;

      %description("Tests whether a parameter is passed by ref.")
      pure func <ref>(any arg0) -> bool external;

      %description("Tests whether a parameter is passed by in.")
      pure func <in>(any arg0) -> bool external;

      %description("Tests whether a parameter is passed by out.")
      pure func <out>(any arg0) -> bool external;

      %description("Tests whether a parameter is passed by in or out.")
      pure func <inout>(any arg0) -> bool external;

      %description("Tests whether a parameter is passed by value.")
      pure func <argument>(any arg0) -> bool external;

      %description("Tests whether a symbol is static.")
      pure func <static>(any arg0) -> bool external;

      %description("Tests whether a method is async.")
      pure func <async>(any arg0) -> bool external;

      %description("Tests whether a symbol is public.")
      pure func <public>(any arg0) -> bool external;

      %description("Tests whether a symbol is public, internal, or protected internal.")
      pure func <exposed>(any arg0) -> bool external;

      %description("Tests whether the associated type is not nested in another type.")
      pure func <top>(any arg0) -> bool external;

      %description("Tests whether the associated type is neither an interface nor abstract.")
      pure func <concrete>(any arg0) -> bool external;

      %description("Tests whether the associated declaration is partial.")
      pure func <partial>(any arg0) -> bool external;

      %description("Tests whether the associated type contains type parameters.")
      pure func <generic>(any arg0) -> bool external;

      %description("Tests whether a method has generic type parameters.")
      pure func <genericMethod>(any arg0) -> bool external;

      %description("Tests whether the associated type is a struct.")
      pure func <struct>(any arg0) -> bool external;

      %description("Tests whether the associated type is a reference type.")
      pure func <class>(any arg0) -> bool external;

      %description("Tests whether the symbol is a field.")
      pure func <field>(any arg0) -> bool external;

      %description("Tests whether the symbol is a property.")
      pure func <property>(any arg0) -> bool external;

      %description("Tests whether the symbol is a method.")
      pure func <method>(any arg0) -> bool external;

      %description("Tests whether the symbol is an event.")
      pure func <event>(any arg0) -> bool external;

      %description("Tests whether the symbol is a parameter.")
      pure func <parameter>(any arg0) -> bool external;

      %description("Tests whether the value represents a type symbol.")
      pure func <typeSymbol>(any arg0) -> bool external;

      %description("Tests whether the associated type is a reference type.")
      pure func <referenceType>(any arg0) -> bool external;

      %description("Tests whether the associated type is a value type.")
      pure func <valueType>(any arg0) -> bool external;

      %description("Tests whether the associated type is nullable.")
      pure func <nullable>(any arg0) -> bool external;

      %description("Tests whether the associated type is a pointer.")
      pure func <pointer>(any arg0) -> bool external;

      %description("Tests whether the type contains pointer data.")
      pure func <containsPointer>(any arg0) -> bool external;

      %description("Tests whether the associated type is an enum.")
      pure func <enum>(any arg0) -> bool external;

      %description("Tests whether the associated type is a supported primitive type.")
      pure func <primitive>(any arg0) -> bool external;

      %description("Tests whether a parameter has an explicit default value; null also satisfies this predicate.")
      pure func <parameterDefault>(any arg0) -> bool external;

      %description("Tests whether the value is a nonempty string constant.")
      pure func <nonEmptyStringConstant>(any arg0) -> bool external;

      %description("Tests whether the type implements IEquatable of itself.")
      pure func <equatableSelf>(any arg0) -> bool external;

      %description("Tests whether the type exposes a typed Equals method for itself.")
      pure func <typedEqualsSelf>(any arg0) -> bool external;

      %description("Tests whether the type declares an ordinary typed Equals method for itself.")
      pure func <ordinaryTypedEqualsSelf>(any arg0) -> bool external;

      %description("Tests whether the type provides an object-based Equals implementation.")
      pure func <objectEquals>(any arg0) -> bool external;

      %description("Tests whether the type provides a GetHashCode implementation.")
      pure func <hashCode>(any arg0) -> bool external;
      """);
    functions.Add(
      new TypeFunction(), new FullNameFunction(), new MembersFunction(),
      new ParametersFunction(), new NullableTypeFunction(), new CSharpLiteralFunction(),
      new MakeGenericFunction(), new VisibilityFunction(),
      new IdentifierFunction(), new FloatTimeFunction(), new AttributesFunction(),
      new AttributesOfFunction(), new AttributesOfExactFunction(), new AttributeOfFunction(),
      new CollectAnnotatedTypesFunction(), new NamespaceFunction(), new TraitFunction("accessible"), new WireFunction(), new SignatureFunction(), new WireableFunction(), new IsTypeFunction(),
      new HasMemberFunction(), new TraitFunction("isSelf"), new TraitFunction("ref"), new TraitFunction("in"),
      new TraitFunction("out"), new TraitFunction("inout"), new TraitFunction("argument"),
      new TraitFunction("static"), new TraitFunction("async"), new TraitFunction("public"),
      new TraitFunction("exposed"), new TraitFunction("top"), new TraitFunction("concrete"),
      new TraitFunction("partial"), new TraitFunction("generic"), new TraitFunction("genericMethod"),
      new TraitFunction("struct"), new TraitFunction("class"), new TraitFunction("field"),
      new TraitFunction("property"), new TraitFunction("method"), new TraitFunction("event"),
      new TraitFunction("parameter"), new TraitFunction("typeSymbol"), new TraitFunction("referenceType"),
      new TraitFunction("valueType"), new TraitFunction("nullable"), new TraitFunction("pointer"),
      new TraitFunction("containsPointer"), new TraitFunction("enum"), new TraitFunction("primitive"),
      new TraitFunction("parameterDefault"), new TraitFunction("nonEmptyStringConstant"),
      new TraitFunction("equatableSelf"), new TraitFunction("typedEqualsSelf"),
      new TraitFunction("ordinaryTypedEqualsSelf"), new TraitFunction("objectEquals"),
      new TraitFunction("hashCode")
    );
  }
  public virtual HixRoslynContext CreateContext(CSharpCompilation compilation, INamedTypeSymbol currentType = null,
    ISymbol target = null, AttributeData attribute = null) => new HixRoslynContext(currentType, target, attribute, compilation, backend: this);
  public override IHixValue ResolveRoot(HixCoroutine thread, string name) => thread.Context is HixRoslynContext roslyn
    ? roslyn.ResolveHostService(thread, name switch { "this" => HixExpressionRoot.This, "target" => HixExpressionRoot.Target, _ => HixExpressionRoot.Attribute }, HixString.Dynamic(""))
    : thread.Error("Roslyn semantic context is required");
  public override object UnlinkSnapshot(HixCoroutine thread, IHixValue value) => thread.Context is HixRoslynContext roslyn ? roslyn.UnlinkSemanticSnapshot(thread, value) : base.UnlinkSnapshot(thread, value);
  public override IHixValue DetachValue(HixCoroutine thread, IHixValue value) => thread.Context is HixRoslynContext roslyn ? roslyn.DetachSemanticValue(thread, value) : base.DetachValue(thread, value);
  public override IHixValue Import(HixCoroutine thread, object value) => value is DetachedSemanticData data ? DetachedSemanticHixValue.Materialize(data) : base.Import(thread, value);
  public override HixString NameOf(HixCoroutine thread, IHixValue value) => thread.Context is HixRoslynContext roslyn ? roslyn.NameOfService(thread, value) : base.NameOf(thread, value);
  public override IHixValue Unwrap(HixCoroutine thread, IHixValue value) => thread.Context is HixRoslynContext roslyn ? roslyn.UnwrapService(thread, value) : value;
  public override bool IsType(HixCoroutine thread, IHixValue value, HixString type) => thread.Context is HixRoslynContext roslyn && roslyn.IsTypeService(thread, value, type);
  public override bool HasTrait(HixCoroutine thread, IHixValue value, HixString trait) => thread.Context is HixRoslynContext roslyn && roslyn.HasTraitService(thread, value, trait);
  public override IHixValue Attributes(HixCoroutine thread, IHixValue value, HixString type, bool exact, bool first) => thread.Context is HixRoslynContext roslyn ? roslyn.AttributesService(thread, value, type, exact, first) : base.Attributes(thread, value, type, exact, first);
}
