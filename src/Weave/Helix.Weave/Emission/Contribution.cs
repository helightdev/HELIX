using System;
using System.Collections.Immutable;
using System.Linq;

namespace Helix.Weave;

/// <summary>
/// One piece of generated text. Pure value data: this is what the incremental pipeline caches,
/// so it holds strings rather than handles into a pool that only exists for one run.
/// </summary>
public readonly struct Emission : IEquatable<Emission> {
  public Emission(EmissionTarget target, string text, string injectionTarget = null,
                  int priority = 0, int sequence = 0, string sharedKey = null) {
    Target = target;
    Text = text ?? "";
    InjectionTarget = injectionTarget;
    Priority = priority;
    Sequence = sequence;
    SharedKey = sharedKey;
  }

  public EmissionTarget Target { get; }
  public string Text { get; }
  /// <summary>Name of the method this is woven into, for <see cref="EmissionTarget.Injection"/>.</summary>
  public string InjectionTarget { get; }
  public int Priority { get; }
  /// <summary>Insertion order, the tie-break within equal priority.</summary>
  public int Sequence { get; }
  /// <summary>
  /// Set when emitted inside a shared scope: every symbol that needs the block emits it, and the
  /// merge keeps one. This replaces cross-symbol state, which a cached prelude cannot have.
  /// </summary>
  public string SharedKey { get; }

  public bool Equals(Emission other) =>
    Target == other.Target && Priority == other.Priority && Sequence == other.Sequence &&
    string.Equals(Text, other.Text, StringComparison.Ordinal) &&
    string.Equals(InjectionTarget, other.InjectionTarget, StringComparison.Ordinal) &&
    string.Equals(SharedKey, other.SharedKey, StringComparison.Ordinal);

  public override bool Equals(object obj) => obj is Emission other && Equals(other);

  public override int GetHashCode() {
    var hash = (int)Target * 397 ^ Priority;
    hash = hash * 397 ^ Sequence;
    hash = hash * 397 ^ (Text?.GetHashCode() ?? 0);
    hash = hash * 397 ^ (InjectionTarget?.GetHashCode() ?? 0);
    return hash * 397 ^ (SharedKey?.GetHashCode() ?? 0);
  }
}

/// <summary>
/// An injection target, resolved against the type while symbols were still in hand so that
/// rendering needs none. Carries the signature the generated method must have.
/// </summary>
public readonly struct ResolvedTarget : IEquatable<ResolvedTarget> {
  public ResolvedTarget(string name, string declaration, string baseCall) {
    Name = name;
    Declaration = declaration;
    BaseCall = baseCall;
  }

  public string Name { get; }
  /// <summary>The full method declaration, minus the body.</summary>
  public string Declaration { get; }
  /// <summary>A call to the base implementation, when overriding one.</summary>
  public string BaseCall { get; }

  public bool Equals(ResolvedTarget other) =>
    string.Equals(Name, other.Name, StringComparison.Ordinal) &&
    string.Equals(Declaration, other.Declaration, StringComparison.Ordinal) &&
    string.Equals(BaseCall, other.BaseCall, StringComparison.Ordinal);

  public override bool Equals(object obj) => obj is ResolvedTarget other && Equals(other);
  public override int GetHashCode() =>
    (Name?.GetHashCode() ?? 0) * 397 ^ (Declaration?.GetHashCode() ?? 0);
}

/// <summary>A diagnostic, kept as data so it survives caching.</summary>
public readonly struct ReportedDiagnostic : IEquatable<ReportedDiagnostic> {
  public ReportedDiagnostic(string id, string message, string severity, string filePath,
                            int start, int length) {
    Id = id;
    Message = message;
    Severity = severity;
    FilePath = filePath;
    Start = start;
    Length = length;
  }

  public string Id { get; }
  public string Message { get; }
  public string Severity { get; }
  public string FilePath { get; }
  public int Start { get; }
  public int Length { get; }

  public bool Equals(ReportedDiagnostic other) =>
    string.Equals(Id, other.Id, StringComparison.Ordinal) &&
    string.Equals(Message, other.Message, StringComparison.Ordinal) &&
    string.Equals(Severity, other.Severity, StringComparison.Ordinal) &&
    string.Equals(FilePath, other.FilePath, StringComparison.Ordinal) &&
    Start == other.Start && Length == other.Length;

  public override bool Equals(object obj) => obj is ReportedDiagnostic other && Equals(other);
  public override int GetHashCode() => (Id?.GetHashCode() ?? 0) * 397 ^ Start;
}

/// <summary>
/// Everything rendering needs to know about the type a partial is generated for, as value data.
/// </summary>
public readonly struct TypeShape : IEquatable<TypeShape> {
  public TypeShape(string key, string namespaceName, string name, string keyword,
                   string typeParameters, bool isPartial, string fileName) {
    Key = key;
    Namespace = namespaceName;
    Name = name;
    Keyword = keyword;
    TypeParameters = typeParameters;
    IsPartial = isPartial;
    FileName = fileName;
  }

  /// <summary>Fully qualified name; identifies the type across contributions.</summary>
  public string Key { get; }
  public string Namespace { get; }
  public string Name { get; }
  /// <summary>e.g. <c>partial class</c>, <c>partial record struct</c>.</summary>
  public string Keyword { get; }
  public string TypeParameters { get; }
  /// <summary>False when the type (or an enclosing one) is not declared partial.</summary>
  public bool IsPartial { get; }
  public string FileName { get; }

  public bool Equals(TypeShape other) =>
    string.Equals(Key, other.Key, StringComparison.Ordinal) &&
    string.Equals(Keyword, other.Keyword, StringComparison.Ordinal) &&
    string.Equals(TypeParameters, other.TypeParameters, StringComparison.Ordinal) &&
    IsPartial == other.IsPartial;

  public override bool Equals(object obj) => obj is TypeShape other && Equals(other);
  public override int GetHashCode() => (Key?.GetHashCode() ?? 0) * 397 ^ (IsPartial ? 1 : 0);
}

/// <summary>
/// What one mixin produced for one annotated symbol. This is the unit the incremental pipeline
/// caches: if a symbol is untouched by an edit, none of this is recomputed.
/// </summary>
public sealed class SymbolContribution : IEquatable<SymbolContribution> {
  public SymbolContribution(TypeShape type, int order, string mixinName,
                            ImmutableArray<Emission> emissions,
                            ImmutableArray<ResolvedTarget> targets,
                            ImmutableArray<KeyValuePairModel> aliases,
                            ImmutableArray<ReportedDiagnostic> diagnostics,
                            ImmutableArray<UniqueClaim> uniques,
                            object carry, int lateIndex) {
    Type = type;
    Order = order;
    MixinName = mixinName;
    Emissions = emissions;
    Targets = targets;
    Aliases = aliases;
    Diagnostics = diagnostics;
    Uniques = uniques;
    Carry = carry;
    LateIndex = lateIndex;
  }

  public TypeShape Type { get; }
  /// <summary>Source position of the annotated symbol, so output order is deterministic.</summary>
  public int Order { get; }
  public string MixinName { get; }
  public ImmutableArray<Emission> Emissions { get; }
  public ImmutableArray<ResolvedTarget> Targets { get; }
  public ImmutableArray<KeyValuePairModel> Aliases { get; }
  public ImmutableArray<ReportedDiagnostic> Diagnostics { get; }
  public ImmutableArray<UniqueClaim> Uniques { get; }

  /// <summary>
  /// The carry the late pass receives. A record, so it compares by value — which is what lets
  /// this whole contribution be a cache key.
  /// </summary>
  public object Carry { get; }

  /// <summary>Index of the mixin in the registry, for running its late pass at merge time.</summary>
  public int LateIndex { get; }

  public bool Equals(SymbolContribution other) =>
    other is not null && Type.Equals(other.Type) && Order == other.Order &&
    string.Equals(MixinName, other.MixinName, StringComparison.Ordinal) &&
    LateIndex == other.LateIndex &&
    Equals(Carry, other.Carry) &&
    Emissions.SequenceEqual(other.Emissions) &&
    Targets.SequenceEqual(other.Targets) &&
    Aliases.SequenceEqual(other.Aliases) &&
    Diagnostics.SequenceEqual(other.Diagnostics) &&
    Uniques.SequenceEqual(other.Uniques);

  public override bool Equals(object obj) => Equals(obj as SymbolContribution);

  public override int GetHashCode() {
    var hash = Type.GetHashCode() * 397 ^ Order;
    hash = hash * 397 ^ (MixinName?.GetHashCode() ?? 0);
    hash = hash * 397 ^ Emissions.Length;
    return hash * 397 ^ (Carry?.GetHashCode() ?? 0);
  }
}

/// <summary>A per-type injection-target alias, as value data.</summary>
public readonly struct KeyValuePairModel : IEquatable<KeyValuePairModel> {
  public KeyValuePairModel(string key, string name, string delegateType, bool isStatic, bool isPublic) {
    Key = key;
    Name = name;
    DelegateType = delegateType;
    IsStatic = isStatic;
    IsPublic = isPublic;
  }

  public string Key { get; }
  public string Name { get; }
  public string DelegateType { get; }
  public bool IsStatic { get; }
  public bool IsPublic { get; }

  public InjectionTarget ToTarget() => new(Name, DelegateType, IsStatic, IsPublic);

  public bool Equals(KeyValuePairModel other) =>
    string.Equals(Key, other.Key, StringComparison.Ordinal) &&
    string.Equals(Name, other.Name, StringComparison.Ordinal) &&
    string.Equals(DelegateType, other.DelegateType, StringComparison.Ordinal) &&
    IsStatic == other.IsStatic && IsPublic == other.IsPublic;

  public override bool Equals(object obj) => obj is KeyValuePairModel other && Equals(other);
  public override int GetHashCode() => (Key?.GetHashCode() ?? 0) * 397 ^ (Name?.GetHashCode() ?? 0);
}

/// <summary>A claim that only one symbol per type may make, checked when contributions merge.</summary>
public readonly struct UniqueClaim : IEquatable<UniqueClaim> {
  public UniqueClaim(string key, string id, string message) {
    Key = key;
    Id = id;
    Message = message;
  }

  public string Key { get; }
  public string Id { get; }
  public string Message { get; }

  public bool Equals(UniqueClaim other) =>
    string.Equals(Key, other.Key, StringComparison.Ordinal) &&
    string.Equals(Id, other.Id, StringComparison.Ordinal);

  public override bool Equals(object obj) => obj is UniqueClaim other && Equals(other);
  public override int GetHashCode() => Key?.GetHashCode() ?? 0;
}

/// <summary>Every contribution for one type, which is the unit a source file is rendered from.</summary>
public sealed class TypeContribution : IEquatable<TypeContribution> {
  public TypeContribution(TypeShape type, ImmutableArray<SymbolContribution> contributions) {
    Type = type;
    Contributions = contributions;
  }

  public TypeShape Type { get; }
  public ImmutableArray<SymbolContribution> Contributions { get; }

  public bool Equals(TypeContribution other) =>
    other is not null && Type.Equals(other.Type) && Contributions.SequenceEqual(other.Contributions);

  public override bool Equals(object obj) => Equals(obj as TypeContribution);
  public override int GetHashCode() => Type.GetHashCode() * 397 ^ Contributions.Length;
}
