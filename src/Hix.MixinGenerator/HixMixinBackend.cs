using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Hix.Roslyn;
using Hix.Runtime;
using static Hix.Runtime.LanguageExecution;
using K = Hix.HixValueKind;
namespace Hix;
public class HixMixinBackend : HixRoslynBackend {
  protected override void RegisterFunctions(FunctionSignatureRegistryBuilder functions) {
    base.RegisterFunctions(functions);
    functions.Add(
      new SimpleFunction(
        "using", [new FunctionSignature(K.Null, [K.String])], (e, a) => {
          e.Outputs.Add(new HixExpressionOutput(HixEmissionTarget.Using, e.Text(a[0])));
          return NullHixValue.Instance;
        }, effects: true
      ),
      new SimpleFunction(
        "inject",
        [new FunctionSignature(K.Null, [K.String, K.Any]), new FunctionSignature(K.Null, [K.String, K.Number, K.Any])],
        (e, a) => {
          var rendered = e.RenderText(a[a.Length - 1]);
          if (rendered is ErrorHixValue) return rendered;
          e.Outputs.Add(
            new HixExpressionOutput(
              HixEmissionTarget.Mixin, e.Text(rendered),
              e.Context.ResolveInjectionTarget(e.Text(a[0])),
              a.Length == 3 ? checked((int)((NumberHixValue)a[1]).Value) : 0
            )
          );
          return NullHixValue.Instance;
        }, effects: true
      ),
      new SimpleFunction(
        "resolveMixin", [new FunctionSignature(K.Any, [K.String, K.Any])],
        (e, a) =>
          e.IsPrelude
            ? e.Context.ResolveMixin(a[0].Render(e.Context), a[1])
            : e.Context.Error("resolveMixin requires the prelude pass"), effects: true, requiresPrelude: true
      ),
      new SimpleFunction(
        "defineTarget", [new FunctionSignature(K.Null, [K.String, K.String])],
        (e, a) =>
          e.IsPrelude
            ? e.Context.DefineTarget(e.Text(a[0]), e.Text(a[1]))
            : e.Context.Error("defineTarget requires the prelude pass"), effects: true, requiresPrelude: true
      ),
      new SimpleFunction(
        "config", [new FunctionSignature(K.Null, [K.String, K.Any])],
        (e, a) =>
          e.IsPrelude ? e.Context.Configure(e.Text(a[0]), a[1]) : e.Context.Error("config requires the prelude pass"),
        effects: true, requiresPrelude: true
      )
    );
  }
  public override IHixValue DefineTarget(HixExecutionContext context, string name, string descriptor) => context is HixMixinContext mixin ? mixin.DefineTargetService(name, descriptor) : base.DefineTarget(context, name, descriptor);
  public override string ResolveInjectionTarget(HixExecutionContext context, string target) => context is HixMixinContext mixin ? mixin.ResolveInjectionTargetService(target) : target;
  public override IHixValue Configure(HixExecutionContext context, string name, IHixValue value) => context is HixMixinContext mixin ? mixin.ConfigureService(name, value) : base.Configure(context, name, value);
  public override IHixValue ResolveMixin(HixExecutionContext context, HixString local, IHixValue operand) => context is HixMixinContext mixin ? mixin.ResolveMixinService(local, operand) : base.ResolveMixin(context, local, operand);
  public static HixMixinBackend Instance { get; } = new();
  internal HixMixinContext CreateContext(INamedTypeSymbol currentType, ISymbol target, AttributeData attribute,
    CSharpCompilation compilation, IReadOnlyDictionary<string,string> targetDefinitions,
    HixExpressionPreparedState prepared, RoslynHostExpressionCache cache) => new(this, currentType, target, attribute, compilation, targetDefinitions, prepared, cache);
}
internal sealed class HixMixinContext : HixRoslynContext {
  private readonly Dictionary<string,string> _targetDefinitions;
  internal HixMixinContext(HixBackend backend, INamedTypeSymbol currentType, ISymbol target, AttributeData attribute,
    CSharpCompilation compilation, IReadOnlyDictionary<string,string> targets, HixExpressionPreparedState prepared,
    RoslynHostExpressionCache cache) : base(currentType, target, attribute, compilation, preparedExpressions: prepared, hostValues: cache, backend: backend) {
    _targetDefinitions = targets == null ? new(StringComparer.Ordinal) : targets.ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
  }
  internal IHixValue DefineTargetService(string name, string descriptor) {
    if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(descriptor)) return Error("target alias and descriptor cannot be empty");
    _targetDefinitions["$" + name.TrimStart('$')] = descriptor;
    return NullHixValue.Instance;
  }

  internal string ResolveInjectionTargetService(string target) {
    var descriptor = ParseMixinTarget(target, _targetDefinitions);
    return (descriptor.IsPublic ? "^" : "") + (descriptor.IsStatic ? "*" : "") + descriptor.Name +
      (string.IsNullOrEmpty(descriptor.DelegateType) ? "" : ":" + descriptor.DelegateType);
  }

  internal IHixValue ConfigureService(string name, IHixValue value) => name.ToUpperInvariant() is "DEBUG" or "PROFILE"
    ? NullHixValue.Instance : Error("unknown host configuration '" + name + "'");

  internal IHixValue ResolveMixinService(HixString localName, IHixValue operand) {
    var descriptor = ParseMixinTarget(operand.Render(this).Resolve(Strings), _targetDefinitions);
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
    IHixValue result = callable is null ? NullHixValue.Instance : new LiteralHixValue(ResolveString(callable));
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
