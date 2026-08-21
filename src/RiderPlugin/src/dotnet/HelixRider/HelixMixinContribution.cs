using System;

namespace HelixRider;

public sealed class HelixMixinContribution : IEquatable<HelixMixinContribution>
{
    public HelixMixinContribution(int offset, string target, string method, string mixin, int priority,
        string sourceType, string sourceMember, string sourceKind, int sourceParameterCount)
    {
        Offset = offset;
        Target = target;
        Method = method;
        Mixin = mixin;
        Priority = priority;
        SourceType = sourceType;
        SourceMember = sourceMember;
        SourceKind = sourceKind;
        SourceParameterCount = sourceParameterCount;
    }

    public int Offset { get; }
    public string Target { get; }
    public string Method { get; }
    public string Mixin { get; }
    public int Priority { get; }
    public string SourceType { get; }
    public string SourceMember { get; }
    public string SourceKind { get; }
    public int SourceParameterCount { get; }

    public bool Equals(HelixMixinContribution other) => other != null && Offset == other.Offset &&
        Target == other.Target && Method == other.Method && Mixin == other.Mixin && Priority == other.Priority &&
        SourceType == other.SourceType && SourceMember == other.SourceMember && SourceKind == other.SourceKind &&
        SourceParameterCount == other.SourceParameterCount;

    public override bool Equals(object obj) => Equals(obj as HelixMixinContribution);
    public override int GetHashCode()
    {
        unchecked
        {
            var hash = Offset;
            hash = hash * 397 ^ Target.GetHashCode();
            hash = hash * 397 ^ Method.GetHashCode();
            hash = hash * 397 ^ Mixin.GetHashCode();
            hash = hash * 397 ^ Priority;
            hash = hash * 397 ^ SourceType.GetHashCode();
            hash = hash * 397 ^ SourceMember.GetHashCode();
            hash = hash * 397 ^ SourceKind.GetHashCode();
            return hash * 397 ^ SourceParameterCount;
        }
    }
}
