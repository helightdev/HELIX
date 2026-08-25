using System;

namespace HELIX.Serialization {
  /// <summary>
  /// Common capabilities of datatype serialization backends. The scalar and structural operations declared by
  /// <see cref="IUniversalReader"/> and <see cref="IUniversalWriter"/> are always supported; this contract only
  /// reports additional types that a backend can encode as a single value.
  /// </summary>
  public interface IUniversalCodec {
    bool SupportsCustomType(Type type);
  }

  /// <summary>
  /// A schema-driven datatype reader. Named formats should consume the supplied names, while positional formats
  /// may ignore them. A successful read consumes exactly one value or enters one container.
  /// </summary>
  public interface IUniversalReader : IUniversalCodec {
    bool TryEnterObject(string name = null);
    bool TryExitObject();
    bool TryEnterArray(string name, out int length);
    /// <summary>Inspects a streaming array for its end without consuming a pending element.</summary>
    bool TryReadArrayEnd(out bool isEnd);
    bool TryExitArray();
    /// <summary>
    /// Reads the null state for the next value. Non-null values remain available to the following typed read.
    /// Positional backends consume their explicit null-state flag in either case.
    /// </summary>
    bool TryReadNull(string name, out bool isNull);
    bool TryReadBoolean(string name, out bool value);
    bool TryReadInt32(string name, out int value);
    bool TryReadInt64(string name, out long value);
    bool TryReadSingle(string name, out float value);
    bool TryReadDouble(string name, out double value);
    bool TryReadString(string name, out string value);
    /// <summary>Reads the UTF-8 bytes of a string into caller-provided storage.</summary>
    bool TryReadUtf8(string name, Span<byte> destination, out int bytesWritten);
    bool TryReadBytes(string name, out byte[] value);

    /// <summary>Reads a backend-native value. Call only when <see cref="IUniversalCodec.SupportsCustomType"/> is true.</summary>
    bool TryReadCustom<T>(string name, out T value);
  }

  /// <summary>
  /// A schema-driven datatype writer. Names may be ignored by positional formats. False indicates that the
  /// backend rejected the operation, for example because a fixed-capacity stream ran out of space.
  /// </summary>
  public interface IUniversalWriter : IUniversalCodec {
    bool TryBeginObject(string name = null);
    bool TryEndObject();
    bool TryBeginArray(string name, int length);
    bool TryEndArray();
    /// <summary>
    /// Writes the null state for a value. When <paramref name="isNull"/> is false, token backends emit nothing and
    /// positional backends emit their non-null flag. The value itself is written separately only when non-null.
    /// </summary>
    bool TryWriteNull(string name, bool isNull);
    bool TryWriteBoolean(string name, bool value);
    bool TryWriteInt32(string name, int value);
    bool TryWriteInt64(string name, long value);
    bool TryWriteSingle(string name, float value);
    bool TryWriteDouble(string name, double value);
    bool TryWriteString(string name, string value);
    /// <summary>Writes an already UTF-8 encoded string without requiring a managed string.</summary>
    bool TryWriteUtf8(string name, Span<byte> value);
    bool TryWriteBytes(string name, byte[] value);

    /// <summary>Writes a backend-native value. Call only when <see cref="IUniversalCodec.SupportsCustomType"/> is true.</summary>
    bool TryWriteCustom<T>(string name, T value);
  }

  /// <summary>Optional datatype capability for structured serialization independent of string conversion.</summary>
  public interface ISerializableDatatype<T> : IDatatype<T> {
    bool TryRead(IUniversalReader reader, string name, out T value);
    bool TryWrite(IUniversalWriter writer, string name, T value);
  }
}
