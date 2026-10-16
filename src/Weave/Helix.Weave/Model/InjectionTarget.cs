namespace Helix.Weave;

/// <summary>
/// A method that mixins weave statements into. The descriptor carries what Hix encoded in
/// its <c>^*~Name:Delegate</c> target strings, constructed rather than parsed.
/// </summary>
public sealed class InjectionTarget {
  public InjectionTarget(string name, string delegateType = null, bool isStatic = false, bool isPublic = false) {
    Name = name;
    DelegateType = delegateType;
    IsStatic = isStatic;
    IsPublic = isPublic;
  }

  /// <summary>Method name on the generated partial.</summary>
  public string Name { get; }
  /// <summary>Qualified delegate type whose Invoke signature the generated method matches, if any.</summary>
  public string DelegateType { get; }
  public bool IsStatic { get; }
  public bool IsPublic { get; }

  /// <summary>Target named after the delegate type's own name, as Hix's <c>~Type</c> form did.</summary>
  public static InjectionTarget FromDelegate(string delegateType, bool isStatic = false, bool isPublic = false) {
    var normalized = delegateType.Replace("global::", "");
    var separator = normalized.LastIndexOfAny(new[] { '.', '+' });
    var name = separator < 0 ? normalized : normalized.Substring(separator + 1);
    return new InjectionTarget(name, delegateType, isStatic, isPublic);
  }

  public override string ToString() => Name;
}
