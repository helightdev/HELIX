// Port of Context.hix: dependency injection, resources, tickers, event handlers and modules.
using System;
using System.Linq;
using Helix.Weave;

namespace HELIX.Weave;

[Mixin(Unit.Method, Attribute = "HELIX.Context.EventHandlerAttribute")]
public sealed class EventHandler : Attribute {
  public EventHandler(int priority = 0) => Priority = priority;
  public int Priority { get; init; }

  [Weave]
  static void Apply(MethodTarget t, EventHandler a) {
    Shared.RequireHandler(t);

    if (t.Params.Length != 1) {
      t.Report("HX0200", "an event handler takes exactly one event parameter");
      return;
    }

    var p = t.Param(0);
    if (Shared.IsManagedComponent(t))
      t.Inject(Targets.ConfigureManaged, -90000,
        $"registration.RegisterHandlerBinding<{p.Type}>({Lit.Of(a.Priority)})");

    // Async chain events register through a different method; everything else is a plain Evt,
    // by value or by ref. Hix fell off the end of this cascade and emitted nothing.
    var register =
      p.ByValue && p.Type.Implements(Types.AsyncChainEvt) ? "RegisterAsync" :
      (p.ByValue || p.ByRef) && p.Type.Implements(Types.Evt) ? "Register" : null;

    if (register is null) {
      t.Report("HX0201", $"'{p.Name}' must be an {Types.Evt} (by value or by ref) " +
                         $"or an {Types.AsyncChainEvt} by value");
      return;
    }

    t.Inject(Targets.Init, 1,
      $"eventHandlerList.{register}<{p.Type}>({t.Name}, {Lit.Of(a.Priority)});");
  }
}

[Mixin(Unit.Member, Attribute = "HELIX.Context.InjectAttribute")]
public sealed class Inject : Attribute {
  public Inject(string qualifier = null, bool required = true) {
    Qualifier = qualifier;
    Required = required;
  }

  public string Qualifier { get; init; }
  public bool Required { get; init; } = true;

  [Weave]
  static void Apply(MemberTarget t, Inject a) {
    t.Using("HELIX.Context");
    var qualifier = Lit.Of(a.Qualifier);

    // A collection of components resolves as a list rather than a single binding.
    var element = t.Type.Argument(0);
    if (t.Type.Implements(Types.Enumerable) && !t.Type.Implements(Types.String) && element.Exists) {
      if (element.Symbol is { IsValueType: true }) {
        t.Report("HX0210", "an injected collection must hold a reference type");
        return;
      }
      t.Using("System.Collections.Generic");
      t.Inject(Targets.ConfigureManaged, 0,
        $"registration.Dependency(new ComponentDependency(new TypeKey(typeof({element}), {qualifier}), false, true));");
      t.Inject(Targets.Init, -1,
        $"{t.Name} = managed.ResolveAll<{element}>({qualifier}) as {t.Type};");
      return;
    }

    t.Inject(Targets.ConfigureManaged, 0,
      $"registration.Dependency(new ComponentDependency(new TypeKey(typeof({t.Type}), {qualifier}), {Lit.Of(a.Required)}));");
    t.Inject(Targets.Init, -1, a.Required
      ? $"{t.Name} = managed.Resolve<{t.Type}>({qualifier});"
      : $"{t.Name} = managed.ResolveOptional<{t.Type}>({qualifier});");
  }
}

[Mixin(Unit.Member, Attribute = "HELIX.Context.BindAttribute")]
public sealed class Bind : Attribute {
  public Bind(PureTypeRef type = default, string qualifier = null, bool required = true,
              bool proxied = false) {
    Type = type;
    Qualifier = qualifier;
    Required = required;
    Proxied = proxied;
  }

  public PureTypeRef Type { get; init; }
  public string Qualifier { get; init; }
  public bool Required { get; init; } = true;
  public bool Proxied { get; init; }

  [Weave]
  static void Apply(MemberTarget t, Bind a) {
    t.Using("HELIX.Context");
    var qualifier = Lit.Of(a.Qualifier);
    var type = a.Type.Exists ? $"typeof({a.Type.Qualified})" : $"typeof({t.Type})";
    var value = a.Proxied ? $"() => {t.Name}" : t.Name;
    var method = a.Proxied ? "PublishProxyBind" : "PublishBind";
    var guard = a.Required ? "" : $"if ({t.Name} != null) ";

    t.Inject(Targets.ConfigureManaged, 0,
      $"registration.Publication({type}, {qualifier}, {Lit.Of(a.Required)});");
    t.Inject(Targets.LoadManagedLate, 0,
      $"{guard}context.{method}({type}, {qualifier}, {value});");
  }
}

/// <summary>Where a resource is loaded from. Mirrors the numeric source in Context.hix.</summary>
public enum ResourceSource {
  None = 0,
  Inline = 1,
  Addressables = 2,
  Resources = 3,
}

[Mixin(Unit.Member, Attribute = "HELIX.Context.ResourceAttribute")]
public sealed class Resource : Attribute {
  public Resource(ResourceSource source = ResourceSource.None, string qualifier = null,
                  bool required = true) {
    Source = source;
    Qualifier = qualifier;
    Required = required;
  }

  public ResourceSource Source { get; init; }
  public string Qualifier { get; init; }
  public bool Required { get; init; } = true;

  [Weave]
  static void Apply(MemberTarget t, Resource a) {
    var element = t.Type.Argument(0);
    if (t.Type.Implements(Types.Enumerable) && !t.Type.Implements(Types.String) && element.Exists) {
      ApplyList(t, a, element);
      return;
    }
    ApplySingle(t, a);
  }

  private static void ApplySingle(MemberTarget t, Resource a) {
    if (t.Type.Symbol is { IsValueType: true } || !t.Type.Implements(Types.UnityObject)) {
      t.Report("HX0220", $"a resource must be a {Types.UnityObject} subclass");
      return;
    }

    var qualifier = Lit.Of(a.Qualifier);
    var key = Lit.Of($"{t.Type.Qualified}|{(int)a.Source}|{a.Qualifier}");
    var dependency = a.Source switch {
      ResourceSource.Addressables => $"new AddressableDependency<{t.Type}>({qualifier}, {key}, {Lit.Of(a.Required)})",
      ResourceSource.Resources => $"new ResourceDependency<{t.Type}>({qualifier}, {key}, {Lit.Of(a.Required)})",
      _ => null,
    };

    if (dependency is null) {
      t.Report("HX0221", "no valid injection source found for the target type");
      return;
    }

    t.Inject(Targets.ConfigureManaged, 0,
      $"registration.Dependency(new ComponentDependency({dependency}, {Lit.Of(a.Required)}));");
    t.Inject(Targets.Init, -1, $"{t.Name} = managed.ResolveOptional<{t.Type}>({key});");
  }

  private static void ApplyList(MemberTarget t, Resource a, TypeRef element) {
    if (element.Symbol is { IsValueType: true } || !element.Implements(Types.UnityObject)) {
      t.Report("HX0222", $"a collected resource must hold a {Types.UnityObject} subclass");
      return;
    }

    t.Using("System.Collections.Generic", "System.Collections");
    var qualifier = Lit.Of(a.Qualifier);
    var key = Lit.Of($"list|{element.Qualified}|{(int)a.Source}|{a.Qualifier}");
    var dependency = a.Source switch {
      ResourceSource.Addressables => $"new AddressableListDependency<{element}>({qualifier}, {key})",
      ResourceSource.Resources => $"new ResourceListDependency<{element}>({qualifier}, {key})",
      _ => null,
    };

    if (dependency is null) {
      t.Report("HX0223", "no valid injection source found for the collected target type");
      return;
    }

    t.Inject(Targets.ConfigureManaged, 0,
      $"registration.Dependency(new ComponentDependency({dependency}, true));");
    t.Inject(Targets.Init, -1, $"{t.Name} = managed.Resolve<List<{element}>>({key}) as {t.Type};");
  }
}

[Mixin(Unit.Member, Attribute = "HELIX.Context.AutoDisposeAttribute")]
public sealed class AutoDispose : Attribute {
  [Weave]
  static void Apply(MemberTarget t, AutoDispose a) {
    t.Using("HELIX.Context");
    t.Inject(Targets.Dispose, 1, $"AutoDisposeMethods.DisposeDisposable(ref {t.Name});");
  }
}

[Mixin(Unit.Type, Attribute = "HELIX.Context.Tests.Fixtures.AlwaysEnabledAttribute")]
public sealed class AlwaysEnabled : Attribute {
  [Weave]
  static void Apply(TypeTarget t, AlwaysEnabled a) =>
    t.Inject(Targets.ConfigureManaged, 0, "registration.Condition(context => true);");
}

/// <summary>
/// Port of <c>mixin HELIX.Context.ManagedAttribute</c>. Runs before the member mixins, which is
/// what lets them see <c>IsManagedComponent</c>.
/// </summary>
[Mixin(Unit.Type, Order = -1000, Attribute = "HELIX.Context.ManagedAttribute")]
public sealed class Managed : Attribute {
  public Managed(PureTypeRef scope = default, bool optional = false, int order = 0, int phase = 0) {
    Scope = scope;
    Optional = optional;
    Order = order;
    Phase = phase;
  }

  public PureTypeRef Scope { get; init; }
  public bool Optional { get; init; }
  public int Order { get; init; }
  public int Phase { get; init; }

  /// <summary>Hix's defineTarget block: what hooks and other mixins weave into.</summary>
  [Aliases]
  static void Aliases(TypeTarget t) {
    t.DefineTarget("$ConfigureManaged", Targets.ConfigureManaged);
    t.DefineTarget("$LoadManaged", Targets.LoadManaged);
    t.DefineTarget("$LoadManagedLate", Targets.LoadManagedLate);
    t.DefineTarget("$UnloadManaged", Targets.UnloadManaged);
    t.DefineTarget("$Init", Targets.LoadManaged);
    t.DefineTarget("$Dispose", Targets.UnloadManaged);
  }

  [Weave]
  static void Apply(TypeTarget t, Managed a) {
    t.Using("HELIX.Context", "UnityEngine");

    t.Inject(Targets.ConfigureManaged, -100000, $"registration.name = {Lit.Of(t.SelfName)};");
    t.Inject(Targets.ConfigureManaged, -100000, $"registration.optional = {Lit.Of(a.Optional)};");
    t.Inject(Targets.ConfigureManaged, -100000, $"registration.phase = {Lit.Of(a.Phase)};");
    t.Inject(Targets.ConfigureManaged, -100000, $"registration.order = {Lit.Of(a.Order)};");

    t.Implements("IManaged");
    t.Class("public RuntimeManagedData managed { get; } = new();");

    if (a.Scope.Exists)
      t.Inject(Targets.ConfigureManaged, -100000, $"registration.scope = typeof({a.Scope.Qualified});");

    var activator = t.IsA(Types.MonoBehaviour) ? "MonoBehaviour" : "PlainObject";
    t.Inject(Targets.ConfigureManaged, -100000,
      $"registration.activator = DefaultComponentActivators.{activator}<{t.TypeRef}>();");
  }
}

[Mixin(Unit.Method, Attribute = "HELIX.Context.TickerAttribute")]
public sealed class Ticker : Attribute {
  public Ticker(string time = "tick", string target = "Update", string condition = null, int order = 10) {
    Time = time;
    Target = target;
    Condition = condition;
    Order = order;
  }

  public string Time { get; init; } = "tick";
  public string Target { get; init; } = "Update";
  public string Condition { get; init; }
  public int Order { get; init; } = 10;

  [Weave]
  static void Apply(MethodTarget t, Ticker a) {
    t.Using("HELIX.Context");

    if (!TimeSpec.TryParse(a.Time, out var frames, out var seconds)) {
      t.Report("HX0230", $"'{a.Time}' is not a tick count or a duration");
      return;
    }

    var field = $"_{t.Name}Ticker";
    t.Class(frames is not null
      ? $"private FrameCountTicker {field} = new({Lit.Of(frames.Value)});"
      : $"private FrameTimeTicker {field} = new({Lit.Of(seconds)});");

    var condition = $"{field}.Tick()";
    if (!string.IsNullOrWhiteSpace(a.Condition)) condition = $"{a.Condition} && {condition}";
    if (Shared.IsManagedComponent(t)) condition = $"managed.IsActive && {condition}";

    var invoke = $"{t.Name}();";
    if (t.IsAsync) {
      if (!t.ReturnType.Implements(Types.UniTask)) {
        t.Report("HX0231", $"an async ticker must return {Types.UniTask}");
        return;
      }
      t.Using("Cysharp.Threading.Tasks");
      var proxy = $"_{t.Name}AsyncProxy";
      t.Class($$"""
        private async UniTaskVoid {{proxy}}() {
          try {
            {{field}}.running = true;
            await {{t.Name}}();
          } finally { {{field}}.running = false; }
        }
        """);
      invoke = $"{proxy}().Forget();";
    }

    t.Inject(new InjectionTarget(a.Target), a.Order, $"if ({condition}) {invoke}");
  }
}

/// <summary>
/// Hix's <c>:floatTime</c>: a bare number is a frame count, a suffixed one a duration.
/// </summary>
public static class TimeSpec {
  public static bool TryParse(string text, out int? frames, out float seconds) {
    frames = null;
    seconds = 0f;
    if (string.IsNullOrWhiteSpace(text)) return false;

    var value = text.Trim();
    if (value == "tick") { frames = 1; return true; }

    var multiplier = 1f;
    if (value.EndsWith("ms", StringComparison.OrdinalIgnoreCase)) {
      multiplier = 0.001f;
      value = value.Substring(0, value.Length - 2);
    } else if (value.EndsWith("s", StringComparison.OrdinalIgnoreCase)) {
      value = value.Substring(0, value.Length - 1);
    } else if (value.EndsWith("f", StringComparison.OrdinalIgnoreCase)) {
      value = value.Substring(0, value.Length - 1);
    } else {
      // No suffix: a frame count.
      if (!int.TryParse(value, System.Globalization.NumberStyles.Integer,
                        System.Globalization.CultureInfo.InvariantCulture, out var count)) return false;
      frames = count;
      return true;
    }

    if (!float.TryParse(value, System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out var parsed)) return false;
    seconds = parsed * multiplier;
    return true;
  }
}

/// <summary>
/// Module discovery, generated from compilation symbols without runtime reflection.
/// Port of <c>ConfigureModule</c> plus the two module mixins.
/// </summary>
public static class Modules {
  public static void Configure(TypeTarget t, string filter, PureTypeRef[] imports) {
    if (t.IsGeneric) return;

    // Sorted by qualified name: registration order is a tie-breaker during loading, so it has
    // to be stable rather than whatever order the symbol walk happened to take.
    var registrations = Code.Join(
      t.AnnotatedTypes("HELIX.Context.ManagedAttribute")
        .Select(type => Registration(t, type, filter))
        .Where(code => code.Length > 0)
        .OrderBy(code => code, StringComparer.Ordinal),
      "\n");

    var importCode = Code.Join(
      (imports ?? Array.Empty<PureTypeRef>()).Where(import => import.Exists),
      import => $"{import.Qualified}.Instance.Discover(registrations);", "\n");

    t.Implements("global::HELIX.Context.IHelixModule");
    t.Class($$"""
      public static readonly {{t.TypeRef}} Instance = new {{t.TypeRef}}();

      public void Discover(global::HELIX.Context.ManagedRegistrations registrations) {
        {{importCode}}
        {{registrations}}
      }
      """);
  }

  private static string Registration(TypeTarget t, Microsoft.CodeAnalysis.INamedTypeSymbol type,
                                     string filter) {
    if (type.TypeKind != Microsoft.CodeAnalysis.TypeKind.Class || type.IsGenericType) return "";
    if (type.DeclaredAccessibility is not (Microsoft.CodeAnalysis.Accessibility.Public
                                           or Microsoft.CodeAnalysis.Accessibility.Internal)) return "";

    if (!string.IsNullOrEmpty(filter)) {
      var ns = type.ContainingNamespace is { IsGlobalNamespace: false } containing
        ? containing.ToDisplayString()
        : "";
      if (ns != filter && !ns.StartsWith(filter + ".", StringComparison.Ordinal)) return "";
    }

    var qualified = t.Facts.Qualified(type);
    return $"registrations.Register(typeof({qualified}), {qualified}.RegistrationConfigurator);";
  }
}

[Mixin(Unit.Type, WholeCompilation = true, Attribute = "HELIX.Context.HelixModuleAttribute")]
public sealed class HelixModule : Attribute {
  public HelixModule(string name = null, string filter = null, PureTypeRef[] import = null) {
    Name = name;
    Filter = filter;
    Import = import;
  }

  public string Name { get; init; }
  public string Filter { get; init; }
  public PureTypeRef[] Import { get; init; }

  [Weave]
  static void Apply(TypeTarget t, HelixModule a) => Modules.Configure(t, a.Filter, a.Import);
}

[Mixin(Unit.Type, WholeCompilation = true, Attribute = "HELIX.Context.HelixApplicationAttribute")]
public sealed class HelixApplication : Attribute {
  public HelixApplication(string name = null, string filter = null, PureTypeRef[] import = null) {
    Name = name;
    Filter = filter;
    Import = import;
  }

  public string Name { get; init; }
  public string Filter { get; init; }
  public PureTypeRef[] Import { get; init; }

  [Weave]
  static void Apply(TypeTarget t, HelixApplication a) {
    Modules.Configure(t, a.Filter, a.Import);
    if (t.IsGeneric) return;

    t.Class("""
      public static global::HELIX.Context.ManagedRegistrations Discover() {
        var registrations = new global::HELIX.Context.ManagedRegistrations();
        Instance.Discover(registrations);
        return registrations;
      }
      """);
  }
}
