using Hix.Mixins;
using Hix.Compiler;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Hix.Roslyn;
using Hix.Runtime;
using static Hix.Runtime.HixThread;
using K = Hix.HixValueKind;
namespace Hix;
public class HixMixinBackend : HixRoslynBackend {
  public override ErrorHixValue ValidateDerivationRecord(HixThread thread, HixTableValue record) =>
    record.Select(thread, HixString.Dynamic("symbol")).Kind == K.Symbol
      ? null : thread.Error("record must contain a semantic symbol");

  public override HixContext CreateContext() => new MixinOutputContext(this);
  public override HixRoslynContext CreateContext(CSharpCompilation compilation, INamedTypeSymbol currentType = null,
    ISymbol target = null, AttributeData attribute = null) =>
    new HixMixinContext(this, currentType, target, attribute, compilation, null, null, null);
  protected override void RegisterEmissionFunctions(FunctionSignatureRegistryBuilder functions) {
    functions.Add(new SimpleFunction("emit", [new(K.Null, [K.Any]), new(K.Null, [K.String, K.Any])],
      (thread, arguments) => {
        var target = arguments.Length == 1 ? "TARGET" : thread.ResolveText(arguments[0]);
        var rendered = thread.RenderText(arguments[arguments.Length - 1]);
        if (rendered is ErrorHixValue) return rendered;
        var text = thread.Text(rendered);
        Emissions(thread).Add(Enum.TryParse<MixinEmissionTarget>(target, true, out var output)
          ? new MixinOutput(output, text, strings: thread.Strings)
          : new MixinOutput(MixinEmissionTarget.Mixin, text, HixString.Dynamic(ResolveInjectionTarget(thread, target)), strings: thread.Strings));
        return NullHixValue.Instance;
      }, effects: true, documentation: "Renders and emits generated source text, optionally to a named generator destination."));
  }

  private static MixinOutputBuffer Emissions(HixThread thread) => ((IMixinOutputContext)thread.Context).Emissions;

  protected override void RegisterFunctions(FunctionSignatureRegistryBuilder functions) {
    base.RegisterFunctions(functions);
    functions.Add(
      new SimpleFunction(
        "using", [new FunctionSignature(K.Null, [K.String])], (e, a) => {
          Emissions(e).Add(new MixinOutput(MixinEmissionTarget.Using, e.Text(a[0]), strings: e.Strings));
          return NullHixValue.Instance;
        }, effects: true,
        documentation: "Adds a namespace import to the generated source."),
      new SimpleFunction(
        "inject",
        [new FunctionSignature(K.Null, [K.String, K.Any]), new FunctionSignature(K.Null, [K.String, K.Number, K.Any])],
        (e, a) => {
          var rendered = e.RenderText(a[a.Length - 1]);
          if (rendered is ErrorHixValue) return rendered;
          Emissions(e).Add(
            new MixinOutput(
              MixinEmissionTarget.Mixin, e.Text(rendered),
              HixString.Dynamic(ResolveInjectionTarget(e, e.ResolveText(a[0]))),
              a.Length == 3 ? checked((int)((NumberHixValue)a[1]).Value) : 0, e.Strings
            )
          );
          return NullHixValue.Instance;
        }, effects: true,
        documentation: "Emits generated text into a named injection target, with an optional priority."),
      new SimpleFunction(
        "resolveMixin", [new FunctionSignature(K.Any, [K.String, K.Any])],
        (e, a) =>
          e.IsPrelude
            ? ResolveMixin(e, a[0].Render(e), a[1])
            : e.Error("resolveMixin requires the prelude pass"), effects: true, requiresPrelude: true,
        documentation: "Resolves a named mixin for the supplied operand during the prelude."),
      new SimpleFunction(
        "defineTarget", [new FunctionSignature(K.Null, [K.String, K.String])],
        (e, a) =>
          e.IsPrelude
            ? DefineTarget(e, e.ResolveText(a[0]), e.ResolveText(a[1]))
            : e.Error("defineTarget requires the prelude pass"), effects: true, requiresPrelude: true,
        documentation: "Defines a named injection-target alias during the prelude.")
    );
  }
  public IHixValue DefineTarget(HixThread thread, string name, string descriptor) => thread.Context is HixMixinContext mixin ? mixin.DefineTargetService(thread, name, descriptor) : thread.Error("target aliases require a mixin context");
  public string ResolveInjectionTarget(HixThread thread, string target) => thread.Context is HixMixinContext mixin ? mixin.ResolveInjectionTargetService(target) : target;
  public IHixValue ResolveMixin(HixThread thread, HixString local, IHixValue operand) => thread.Context is HixMixinContext mixin ? mixin.ResolveMixinService(thread, local, operand) : thread.Error("mixin resolution requires a mixin context");
  public static HixMixinBackend Instance { get; } = new();
  internal HixMixinContext CreateContext(INamedTypeSymbol currentType, ISymbol target, AttributeData attribute,
    CSharpCompilation compilation, IReadOnlyDictionary<string,string> targetDefinitions,
    HixCompilerCatalog prepared, RoslynHostExpressionCache cache) => new(this, currentType, target, attribute, compilation, targetDefinitions, prepared, cache);
}
internal sealed class HixMixinContext : HixRoslynContext, IMixinOutputContext {
  public MixinOutputBuffer Emissions { get; } = new();
  public override void BeginExecution() => Emissions.Clear();
  public override int CaptureEffects() => Emissions.Count;
  public override void RollbackEffects(int checkpoint) => Emissions.Rollback(checkpoint);
  private readonly Dictionary<string,string> _targetDefinitions;
  internal HixMixinContext(HixBackend backend, INamedTypeSymbol currentType, ISymbol target, AttributeData attribute,
    CSharpCompilation compilation, IReadOnlyDictionary<string,string> targets, HixCompilerCatalog prepared,
    RoslynHostExpressionCache cache) : base(currentType, target, attribute, compilation, preparedExpressions: prepared, hostValues: cache, backend: backend) {
    _targetDefinitions = targets == null ? new(StringComparer.Ordinal) : targets.ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
  }
  internal IHixValue DefineTargetService(HixThread thread, string name, string descriptor) {
    if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(descriptor)) return thread.Error("target alias and descriptor cannot be empty");
    _targetDefinitions["$" + name.TrimStart('$')] = descriptor;
    return NullHixValue.Instance;
  }

  internal string ResolveInjectionTargetService(string target) {
    var descriptor = ParseMixinTarget(target, _targetDefinitions);
    return (descriptor.IsPublic ? "^" : "") + (descriptor.IsStatic ? "*" : "") + descriptor.Name +
      (string.IsNullOrEmpty(descriptor.DelegateType) ? "" : ":" + descriptor.DelegateType);
  }

  internal IHixValue ResolveMixinService(HixThread thread, HixString localName, IHixValue operand) {
    var descriptor = ParseMixinTarget(operand.Render(thread).Resolve(thread.Strings), _targetDefinitions);
    string callable = null;
    if (!string.IsNullOrEmpty(descriptor.DelegateType)) {
      if (ResolveType(descriptor.DelegateType) is { TypeKind: TypeKind.Delegate } delegateType)
        callable = delegateType.ToDisplayString(GeneratorAnalysis.TypeDisplayFormat);
      else callable = descriptor.DelegateType;
    } else if (ResolveType(descriptor.Name) is { TypeKind: TypeKind.Delegate } namedDelegate)
      callable = namedDelegate.ToDisplayString(GeneratorAnalysis.TypeDisplayFormat);
    else {
      var methods = MethodsInHierarchy(CurrentType, descriptor.Name).ToArray();
      if (methods.Length == 1) callable = CallableReference(methods[0]);
    }
    IHixValue result = callable is null ? NullHixValue.Instance : new LiteralHixValue(thread.ResolveString(callable));
    return result;
  }

  internal static MixinTargetDescriptor ParseMixinTarget(
    string target,
    IReadOnlyDictionary<string, string> targetDefinitions = null
  ) {
    var value = target?.Trim() ?? "";
    if (targetDefinitions is not null && targetDefinitions.TryGetValue(value, out var defined))
      return ParseMixinTarget(defined, targetDefinitions);
    var isStatic = false;
    var isPublic = false;
    while (value.Length != 0 && (value[0] == '*' || value[0] == '^')) {
      if (value[0] == '*') isStatic = true;
      else isPublic = true;
      value = value.Substring(1);
    }
    string name;
    string delegateType = null;
    if (value.StartsWith("~", StringComparison.Ordinal)) {
      delegateType = value.Substring(1);
      var normalized = delegateType.Replace("global::", "");
      name = normalized.Split('.', '+').LastOrDefault() ?? normalized;
    } else {
      var separator = value.IndexOf(':');
      name = separator < 0 ? value : value.Substring(0, separator);
      if (separator >= 0) delegateType = value.Substring(separator + 1);
    }
    name = name switch {
      "$Init" => "Awake",
      "$Dispose" => "OnDestroy",
      _ when name.StartsWith("$", StringComparison.Ordinal) => name.Substring(1),
      _ => name
    };
    return new MixinTargetDescriptor(name, isStatic, isPublic, delegateType);
  }

}
