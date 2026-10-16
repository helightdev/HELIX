using System;
using System.Collections.Generic;

namespace Helix.Weave;

/// <summary>Handle into a <see cref="StringPool"/>. Equality is an integer compare.</summary>
public readonly struct StringHandle : IEquatable<StringHandle> {
  internal StringHandle(int id) => Id = id;
  internal int Id { get; }
  public bool IsNull => Id == 0;
  public static StringHandle Null => default;
  public bool Equals(StringHandle other) => Id == other.Id;
  public override bool Equals(object obj) => obj is StringHandle other && Equals(other);
  public override int GetHashCode() => Id;
  public static bool operator ==(StringHandle a, StringHandle b) => a.Id == b.Id;
  public static bool operator !=(StringHandle a, StringHandle b) => a.Id != b.Id;
}

/// <summary>
/// Interns generated text so outputs carry handles rather than strings; text is resolved
/// once at render. Ported in spirit from Hix's string pool.
/// </summary>
public sealed class StringPool {
  private readonly List<string> _values = new() { null };
  private readonly Dictionary<string, int> _index = new(StringComparer.Ordinal);

  public StringHandle Intern(string value) {
    if (value is null) return StringHandle.Null;
    if (_index.TryGetValue(value, out var id)) return new StringHandle(id);
    id = _values.Count;
    _values.Add(value);
    _index.Add(value, id);
    return new StringHandle(id);
  }

  public string Resolve(StringHandle handle) => handle.IsNull ? null : _values[handle.Id];
  public int Count => _values.Count - 1;
}
