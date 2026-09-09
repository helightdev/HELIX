using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Hix.Runtime;
using Hix.Functions;
namespace Hix.Roslyn;

/// <summary>Roslyn semantic services. Each invocation owns its semantic value caches and VM state.</summary>
public class HixRoslynBackend : HixBackend {
  protected override void RegisterRoots(IDictionary<string, HixBackendRoot> roots) {
    base.RegisterRoots(roots);
    foreach (var name in new[] {"this", "target", "attr"}) roots.Add(name, new(name, HixValueKind.Symbol, true));
  }
  protected override void RegisterFunctions(FunctionSignatureRegistryBuilder functions) {
    base.RegisterFunctions(functions);
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
  public override IHixValue ResolveRoot(HixThread thread, string name) => thread.Context is HixRoslynContext roslyn
    ? roslyn.ResolveHostService(thread, name switch { "this" => HixExpressionRoot.This, "target" => HixExpressionRoot.Target, _ => HixExpressionRoot.Attribute }, HixString.Dynamic(""))
    : thread.Error("Roslyn semantic context is required");
  public override object UnlinkSnapshot(HixThread thread, IHixValue value) => thread.Context is HixRoslynContext roslyn ? roslyn.UnlinkSemanticSnapshot(thread, value) : base.UnlinkSnapshot(thread, value);
  public override IHixValue DetachValue(HixThread thread, IHixValue value) => thread.Context is HixRoslynContext roslyn ? roslyn.DetachSemanticValue(thread, value) : base.DetachValue(thread, value);
  public override IHixValue Import(HixThread thread, object value) => value is DetachedSemanticData data ? DetachedSemanticHixValue.Materialize(data) : base.Import(thread, value);
  public override HixString NameOf(HixThread thread, IHixValue value) => thread.Context is HixRoslynContext roslyn ? roslyn.NameOfService(thread, value) : base.NameOf(thread, value);
  public override IHixValue Unwrap(HixThread thread, IHixValue value) => thread.Context is HixRoslynContext roslyn ? roslyn.UnwrapService(thread, value) : value;
  public override bool IsType(HixThread thread, IHixValue value, HixString type) => thread.Context is HixRoslynContext roslyn && roslyn.IsTypeService(thread, value, type);
  public override bool HasTrait(HixThread thread, IHixValue value, HixString trait) => thread.Context is HixRoslynContext roslyn && roslyn.HasTraitService(thread, value, trait);
  public override IHixValue Attributes(HixThread thread, IHixValue value, HixString type, bool exact, bool first) => thread.Context is HixRoslynContext roslyn ? roslyn.AttributesService(thread, value, type, exact, first) : base.Attributes(thread, value, type, exact, first);
}
