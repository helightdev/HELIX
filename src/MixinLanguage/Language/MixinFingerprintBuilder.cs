using System;

namespace HelixSourceGenerator.Language;

public sealed class MixinFingerprintBuilder {
  private const ulong Offset = 14695981039346656037UL;
  private const ulong Prime = 1099511628211UL;
  private ulong _hash;

  public ulong Hash => _hash == 0 ? Offset : _hash;
  public long Length { get; private set; }

  public void Append(string value) {
    Append(value?.Length ?? -1);
    if (value is null) return;
    for (var index = 0; index < value.Length; index++) Mix(value[index]);
    Length += value.Length;
  }

  public void Append(bool value) {
    Mix(value ? 1UL : 0UL);
  }

  public void Append(int value) {
    Mix(unchecked((ulong)value));
  }

  public void Append(long value) {
    Mix(unchecked((ulong)value));
  }

  public void Append(ulong value) {
    Mix(value);
  }

  public void Append(double value) {
    Append(BitConverter.DoubleToInt64Bits(value));
  }

  private void Mix(ulong value) {
    if (_hash == 0) _hash = Offset;
    unchecked {
      _hash ^= value;
      _hash *= Prime;
    }
    Length++;
  }
}