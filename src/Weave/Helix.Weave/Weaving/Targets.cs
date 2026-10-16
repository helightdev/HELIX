using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Helix.Weave;

/// <summary>
/// The emission surface. This is all the late pass gets: text in, nothing analyzed out.
/// </summary>
public class LateTarget {
  internal LateTarget(WeaveSession session, ISymbol symbol) {
    Session = session;
    Symbol = symbol;
  }

  internal WeaveSession Session { get; }
  /// <summary>Used for diagnostic locations and target-scoped state; never exposed to the late pass.</summary>
  internal ISymbol Symbol { get; }

  /// <summary>A member of the generated partial type.</summary>
  public void Class(string text) => Emit(EmissionTarget.Class, text);
  /// <summary>A top-level declaration in the generated file.</summary>
  public void File(string text) => Emit(EmissionTarget.File, text);
  /// <summary>An interface on the generated partial.</summary>
  public void Implements(string text) => Emit(EmissionTarget.Implements, text);
  public void Implements(params string[] interfaces) { foreach (var item in interfaces) Implements(item); }
  /// <summary>The base type of the generated partial.</summary>
  public void Extends(string text) => Emit(EmissionTarget.Extends, text);
  /// <summary>An attribute on the generated partial.</summary>
  public void Annotation(string text) => Emit(EmissionTarget.Annotation, text);
  public void Using(string text) => Emit(EmissionTarget.Using, text);
  public void Using(params string[] namespaces) { foreach (var item in namespaces) Using(item); }

  /// <summary>Weaves a statement into a named method, ordered by <paramref name="priority"/>.</summary>
  public void Inject(InjectionTarget target, int priority, string statement) {
    if (string.IsNullOrWhiteSpace(statement)) return;
    Session.Outputs.Add(EmissionTarget.Injection, Session.Pool.Intern(Code.Dedent(statement)),
                        Session.Pool.Intern(target.Name), priority);
    Session.DeclareTarget(target);
  }

  /// <summary>Defines a per-type injection target alias. Hix's <c>defineTarget</c>.</summary>
  public void DefineTarget(string alias, InjectionTarget target) => Session.DefineAlias(alias, target);

  /// <summary>Weaves into a target named by alias on this type.</summary>
  public void Inject(string alias, int priority, string statement) {
    var target = Session.ResolveAlias(alias);
    if (target is null) {
      Report("WEAVE002", $"injection target '{alias}' is not defined on this type");
      return;
    }
    Inject(target, priority, statement);
  }

  public bool HasTarget(string alias) => Session.ResolveAlias(alias) is not null;

  public void Report(string id, string message, DiagnosticSeverity severity = DiagnosticSeverity.Error) =>
    Session.Report(new WeaveDiagnostic(id, message, Symbol?.Locations.Length > 0 ? Symbol.Locations[0] : null, severity));

  /// <summary>
  /// True the first time it is called for this slot, false afterwards — the whole of Hix's
  /// <c>RequireX()</c> idempotency pattern.
  /// </summary>
  public bool Once(Slot<bool> slot) {
    if (Session.GetState(slot, Symbol)) return false;
    Session.SetState(slot, Symbol, true);
    return true;
  }

  public T Get<T>(Slot<T> slot) => Session.GetState(slot, Symbol);
  public void Set<T>(Slot<T> slot, T value) => Session.SetState(slot, Symbol, value);

  private void Emit(EmissionTarget target, string text) {
    if (string.IsNullOrWhiteSpace(text)) return;
    Session.Outputs.Add(target, Session.Pool.Intern(Code.Dedent(text)));
  }
}

/// <summary>Emission plus symbol access. Prelude only.</summary>
public abstract class SymbolTarget : LateTarget {
  internal SymbolTarget(WeaveSession session, ISymbol symbol) : base(session, symbol) { }

  /// <summary>The annotated symbol. The escape hatch to raw Roslyn, always available.</summary>
  public new ISymbol Symbol => base.Symbol;
  public SymbolFacts Facts => Session.Facts;
  public string Name => Symbol.Name;
  public bool IsStatic => Symbol.IsStatic;
  public Accessibility Accessibility => Symbol.DeclaredAccessibility;
  public string Visibility => Accessibility switch {
    Accessibility.Public => "public",
    Accessibility.Internal => "internal",
    Accessibility.Protected => "protected",
    Accessibility.ProtectedOrInternal => "protected internal",
    Accessibility.ProtectedAndInternal => "private protected",
    _ => "private",
  };

  /// <summary>The type the generated partial is for.</summary>
  public TypeRef Self => Facts.Ref(Session.Type);
  public string SelfName => Session.Type.Name;

  /// <summary>The owning type as a target, for queries about where a member lives.</summary>
  public TypeTarget Owner => new(Session, Session.Type);

  public AttributeData Attribute(string metadataName) {
    foreach (var attribute in Facts.Attributes(Symbol))
      if (attribute.AttributeClass is not null && Facts.MetadataName(attribute.AttributeClass) == metadataName)
        return attribute;
    return null;
  }

  public bool HasAttribute(string metadataName) => Attribute(metadataName) is not null;

  /// <summary>
  /// The argument list that calls <paramref name="method"/> from inside the generated method for
  /// <paramref name="target"/>, or null when the signatures do not line up. Hix's <c>wire</c>.
  /// </summary>
  public string Wire(InjectionTarget target, IMethodSymbol method) {
    if (method is null) return null;
    if (target?.DelegateType is null) return method.Parameters.Length == 0 ? "" : null;

    var resolved = Facts.Lookup(Session.Compilation, target.DelegateType);
    if (resolved is not { TypeKind: TypeKind.Delegate } || resolved.DelegateInvokeMethod is null) return null;

    var available = resolved.DelegateInvokeMethod.Parameters;
    if (method.Parameters.Length > available.Length) return null;

    var arguments = new List<string>(method.Parameters.Length);
    for (var i = 0; i < method.Parameters.Length; i++) {
      if (!SymbolEqualityComparer.Default.Equals(available[i].Type, method.Parameters[i].Type) ||
          available[i].RefKind != method.Parameters[i].RefKind) return null;
      var prefix = available[i].RefKind switch {
        RefKind.Ref => "ref ",
        RefKind.Out => "out ",
        RefKind.In => "in ",
        _ => "",
      };
      arguments.Add(prefix + available[i].Name);
    }
    return string.Join(", ", arguments);
  }

  /// <summary>
  /// The injection target an alias resolves to on this type. An alias no mixin defined falls back
  /// to a method of that name, with Hix's two special cases — the same rule its ParseMixinTarget
  /// applied, so a hook into an undeclared target still lands somewhere predictable.
  /// </summary>
  public InjectionTarget TargetFor(string alias) {
    if (string.IsNullOrEmpty(alias)) return null;
    var defined = Session.ResolveAlias(alias);
    if (defined is not null) return defined;
    if (!alias.StartsWith("$", System.StringComparison.Ordinal)) return new InjectionTarget(alias);

    return alias switch {
      "$Init" => new InjectionTarget("Awake"),
      "$Dispose" => new InjectionTarget("OnDestroy"),
      _ => new InjectionTarget(alias.Substring(1)),
    };
  }

  /// <summary>Seeds and derives one property per symbol, in order.</summary>
  public ImmutableArray<DerivedProperty> Derive(IEnumerable<ISymbol> symbols) =>
    Session.Derivations.Run(Session, symbols);
}

public sealed class TypeTarget : SymbolTarget {
  internal TypeTarget(WeaveSession session, INamedTypeSymbol symbol) : base(session, symbol) => Type = symbol;
  public new INamedTypeSymbol Symbol => Type;
  public INamedTypeSymbol Type { get; }
  public TypeRef TypeRef => Facts.Ref(Type);
  public bool IsGeneric => Type.IsGenericType;

  /// <summary>The instance fields of this type, derived. Hix's <c>DeriveFields</c>.</summary>
  public ImmutableArray<DerivedProperty> DeriveFields() => Derive(InstanceFields.Cast<ISymbol>());

  /// <summary>A nested type by name, or a non-existent target. Hix's <c>this#Name</c>.</summary>
  public TypeTarget Nested(string name) {
    foreach (var member in Type.GetTypeMembers())
      if (member.Name == name) return new TypeTarget(Session, member);
    return null;
  }

  /// <summary>Whether this type (or a base) declares a member of that name.</summary>
  public bool HasMember(string name) {
    for (var current = Type; current is not null; current = current.BaseType)
      if (current.GetMembers(name).Length > 0) return true;
    return false;
  }

  /// <summary>Whether this type declares a member of that name itself.</summary>
  public bool DeclaresMember(string name) => Type.GetMembers(name).Length > 0;

  /// <summary>
  /// Whether this type's own declaration lists that interface, as opposed to inheriting it.
  /// Hix's <c>this:equatableSelf</c> and friends ask this, not the full hierarchy.
  /// </summary>
  public bool DeclaresInterface(string metadataName) {
    foreach (var iface in Type.Interfaces)
      if (Facts.MetadataName(iface) == metadataName) return true;
    return false;
  }

  /// <summary>Whether this type is, implements or inherits that type. Hix's <c>this:is&lt;T&gt;</c>.</summary>
  public bool IsA(string metadataName) => Facts.Implements(Type, metadataName);
  public bool InheritsFrom(string metadataName) => Facts.InheritsFrom(Type, metadataName);

  /// <summary>
  /// Resolves a name to something callable: a delegate type, or a uniquely named method in the
  /// hierarchy. Hix's <c>resolveMixin</c> / <c>wire</c>.
  /// </summary>
  public string ResolveCallable(string nameOrDelegateType) {
    var delegateType = Facts.Lookup(Session.Compilation, nameOrDelegateType);
    if (delegateType is { TypeKind: TypeKind.Delegate }) return Facts.Qualified(delegateType);

    IMethodSymbol found = null;
    for (var current = Type; current is not null; current = current.BaseType)
      foreach (var member in current.GetMembers(nameOrDelegateType))
        if (member is IMethodSymbol method) {
          if (found is not null) return null; // ambiguous
          found = method;
        }
    return found is null ? null : found.Name;
  }

  /// <summary>
  /// Every type carrying this attribute **declared in this compilation**. Hix's
  /// <c>collectAnnotatedTypes</c>: referenced assemblies are deliberately excluded, since each
  /// assembly's own module registers its own components — including them would register them
  /// twice. It is still a whole-assembly query, so use it sparingly.
  /// </summary>
  public IReadOnlyList<INamedTypeSymbol> AnnotatedTypes(string attributeMetadataName) =>
    Facts.AnnotatedTypes(Session.Compilation, attributeMetadataName);

  public IEnumerable<IFieldSymbol> InstanceFields {
    get {
      foreach (var member in Type.GetMembers())
        if (member is IFieldSymbol { IsStatic: false, IsImplicitlyDeclared: false } field) yield return field;
    }
  }
}

/// <summary>A parameter, detached-friendly: its type is a backed <see cref="TypeRef"/>.</summary>
public readonly struct ParamRef {
  internal ParamRef(IParameterSymbol symbol, SymbolFacts facts) {
    Symbol = symbol;
    Type = facts.Ref(symbol.Type);
  }

  public IParameterSymbol Symbol { get; }
  public TypeRef Type { get; }
  public string Name => Symbol.Name;
  public RefKind RefKind => Symbol.RefKind;
  public bool ByValue => Symbol.RefKind == RefKind.None;
  public bool ByRef => Symbol.RefKind == RefKind.Ref;
  public bool In => Symbol.RefKind == RefKind.In;
  public bool Out => Symbol.RefKind == RefKind.Out;
  public bool HasDefault => Symbol.HasExplicitDefaultValue;
}

public sealed class MethodTarget : SymbolTarget {
  private ImmutableArray<ParamRef> _params;

  internal MethodTarget(WeaveSession session, IMethodSymbol symbol) : base(session, symbol) => Method = symbol;
  public new IMethodSymbol Symbol => Method;
  public IMethodSymbol Method { get; }
  public TypeRef ReturnType => Facts.Ref(Method.ReturnType);
  public bool IsAsync => Method.IsAsync;
  public bool IsGenericMethod => Method.IsGenericMethod;

  public ImmutableArray<ParamRef> Params {
    get {
      if (_params.IsDefault) {
        var builder = ImmutableArray.CreateBuilder<ParamRef>(Method.Parameters.Length);
        foreach (var parameter in Method.Parameters) builder.Add(new ParamRef(parameter, Facts));
        _params = builder.MoveToImmutable();
      }
      return _params;
    }
  }

  public ParamRef Param(int index) => Params[index];

  /// <summary>This method's parameters, derived. Hix's <c>DeriveParameters</c>.</summary>
  public ImmutableArray<DerivedProperty> DeriveParameters() => Derive(Method.Parameters.Cast<ISymbol>());
}

/// <summary>A field or property.</summary>
public sealed class MemberTarget : SymbolTarget {
  internal MemberTarget(WeaveSession session, ISymbol symbol) : base(session, symbol) =>
    Type = Facts.Ref(symbol switch {
      IFieldSymbol field => field.Type,
      IPropertySymbol property => property.Type,
      _ => null,
    });

  public TypeRef Type { get; }
  public bool IsField => Symbol is IFieldSymbol;
  public bool IsProperty => Symbol is IPropertySymbol;
}
