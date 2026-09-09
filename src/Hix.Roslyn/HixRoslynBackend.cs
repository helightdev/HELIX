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
  public HixExecutionContext CreateContext(CSharpCompilation compilation, INamedTypeSymbol currentType = null,
    ISymbol target = null, AttributeData attribute = null) => new HixRoslynContext(currentType, target, attribute, compilation, backend: this);
  public override IHixValue ResolveRoot(HixExecutionContext context, string name) => context is HixRoslynContext roslyn
    ? roslyn.ResolveHostService(name switch { "this" => HixExpressionRoot.This, "target" => HixExpressionRoot.Target, _ => HixExpressionRoot.Attribute }, HixExecutionContext.Dynamic(""))
    : context.Error("Roslyn semantic context is required");
  public override object UnlinkSnapshot(HixExecutionContext context, IHixValue value) => context is HixRoslynContext roslyn ? roslyn.UnlinkSemanticSnapshot(value) : base.UnlinkSnapshot(context, value);
  public override IHixValue DetachValue(HixExecutionContext context, IHixValue value) => context is HixRoslynContext roslyn ? roslyn.DetachSemanticValue(value) : base.DetachValue(context, value);
  public override IHixValue Import(HixExecutionContext context, object value) => value is DetachedSemanticData data ? DetachedSemanticHixValue.Materialize(data) : base.Import(context, value);
  public override HixString NameOf(HixExecutionContext context, IHixValue value) => context is HixRoslynContext roslyn ? roslyn.NameOfService(value) : base.NameOf(context, value);
  public override IHixValue Unwrap(HixExecutionContext context, IHixValue value) => context is HixRoslynContext roslyn ? roslyn.UnwrapService(value) : value;
  public override bool IsType(HixExecutionContext context, IHixValue value, HixString type) => context is HixRoslynContext roslyn && roslyn.IsTypeService(value, type);
  public override bool HasTrait(HixExecutionContext context, IHixValue value, HixString trait) => context is HixRoslynContext roslyn && roslyn.HasTraitService(value, trait);
  public override IHixValue Attributes(HixExecutionContext context, IHixValue value, HixString type, bool exact, bool first) => context is HixRoslynContext roslyn ? roslyn.AttributesService(value, type, exact, first) : base.Attributes(context, value, type, exact, first);
}
