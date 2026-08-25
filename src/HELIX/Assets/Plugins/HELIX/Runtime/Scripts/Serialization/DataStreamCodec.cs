using System;
using System.Buffers;
using System.Text;
using Unity.Collections;

namespace HELIX.Serialization {
  /// <summary>A positional datatype reader over Unity's native data stream format.</summary>
  public sealed class DataStreamUniversalReader : IUniversalReader {
    private const int StackStringBytes = 1024;
    private DataStreamReader _reader;

    public DataStreamUniversalReader(DataStreamReader reader) => _reader = reader;
    public DataStreamReader Reader => _reader;
    public bool SupportsCustomType(Type type) => false;
    public bool TryEnterObject(string name = null) => true;
    public bool TryExitObject() => true;

    public bool TryEnterArray(string name, out int length) {
      var snapshot = _reader;
      length = 0;
      if (TryReadInt32(null, out length) && length >= 0) return true;
      _reader = snapshot;
      length = 0;
      return false;
    }

    public bool TryExitArray() => true;
    public bool TryReadArrayEnd(out bool isEnd) { isEnd = false; return false; }

    public bool TryReadNull(string name, out bool isNull) {
      var snapshot = _reader;
      isNull = false;
      if (!TryReadByte(out var marker) || marker > 1) {
        _reader = snapshot;
        return false;
      }
      isNull = marker == 0;
      return true;
    }

    public bool TryReadBoolean(string name, out bool value) {
      var snapshot = _reader;
      value = false;
      if (TryReadByte(out var encoded) && encoded <= 1) {
        value = encoded != 0;
        return true;
      }
      _reader = snapshot;
      return false;
    }

    public bool TryReadInt32(string name, out int value) {
      var snapshot = _reader;
      value = _reader.ReadInt();
      if (!_reader.HasFailedReads) return true;
      _reader = snapshot;
      value = 0;
      return false;
    }

    public bool TryReadInt64(string name, out long value) {
      var snapshot = _reader;
      value = _reader.ReadLong();
      if (!_reader.HasFailedReads) return true;
      _reader = snapshot;
      value = 0;
      return false;
    }

    public bool TryReadSingle(string name, out float value) {
      var snapshot = _reader;
      value = _reader.ReadFloat();
      if (!_reader.HasFailedReads) return true;
      _reader = snapshot;
      value = 0f;
      return false;
    }

    public bool TryReadDouble(string name, out double value) {
      var snapshot = _reader;
      value = _reader.ReadDouble();
      if (!_reader.HasFailedReads) return true;
      _reader = snapshot;
      value = 0d;
      return false;
    }

    public bool TryReadString(string name, out string value) {
      var snapshot = _reader;
      value = null;
      if (!TryReadInt32(null, out var length) || length < 0 ||
          length > _reader.Length - _reader.GetBytesRead()) {
        _reader = snapshot;
        return false;
      }
      if (length <= StackStringBytes) {
        Span<byte> bytes = stackalloc byte[length];
        if (TryReadRawBytes(bytes)) {
          value = Encoding.UTF8.GetString(bytes);
          return true;
        }
      } else {
        var bytes = ArrayPool<byte>.Shared.Rent(length);
        try {
          var span = bytes.AsSpan(0, length);
          if (TryReadRawBytes(span)) {
            value = Encoding.UTF8.GetString(span);
            return true;
          }
        } finally {
          ArrayPool<byte>.Shared.Return(bytes);
        }
      }
      _reader = snapshot;
      return false;
    }

    public bool TryReadUtf8(string name, Span<byte> destination, out int bytesWritten) {
      var snapshot = _reader;
      bytesWritten = 0;
      if (!TryReadBufferLength(out var length) || length > destination.Length) {
        _reader = snapshot;
        return false;
      }
      if (!TryReadRawBytes(destination[..length])) {
        _reader = snapshot;
        return false;
      }
      bytesWritten = length;
      return true;
    }

    public bool TryReadBytes(string name, out byte[] value) {
      var snapshot = _reader;
      value = null;
      if (!TryReadBufferLength(out var length)) {
        _reader = snapshot;
        return false;
      }
      value = new byte[length];
      if (TryReadRawBytes(value.AsSpan())) return true;
      _reader = snapshot;
      value = null;
      return false;
    }

    public bool TryReadCustom<T>(string name, out T value) { value = default; return false; }

    private bool TryReadBufferLength(out int length) {
      length = 0;
      return TryReadInt32(null, out length) && length >= 0 &&
             length <= _reader.Length - _reader.GetBytesRead();
    }

    private bool TryReadRawBytes(Span<byte> destination) {
      _reader.ReadBytes(destination);
      return !_reader.HasFailedReads;
    }

    private bool TryReadByte(out byte value) {
      var snapshot = _reader;
      value = _reader.ReadByte();
      if (!_reader.HasFailedReads) return true;
      _reader = snapshot;
      value = 0;
      return false;
    }
  }

  /// <summary>A positional datatype writer over Unity's native data stream format.</summary>
  public sealed class DataStreamUniversalWriter : IUniversalWriter {
    private const int StackStringBytes = 1024;
    private DataStreamWriter _writer;

    public DataStreamUniversalWriter(DataStreamWriter writer) => _writer = writer;
    public DataStreamWriter Writer => _writer;
    public bool SupportsCustomType(Type type) => false;
    public bool TryBeginObject(string name = null) => true;
    public bool TryEndObject() => true;

    public bool TryBeginArray(string name, int length) {
      if (length < 0) return false;
      var snapshot = _writer;
      if (_writer.WriteInt(length) && !_writer.HasFailedWrites) return true;
      _writer = snapshot;
      return false;
    }

    public bool TryEndArray() => true;
    public bool TryWriteNull(string name, bool isNull) => TryWriteByte(isNull ? (byte)0 : (byte)1);
    public bool TryWriteBoolean(string name, bool value) => TryWriteByte(value ? (byte)1 : (byte)0);

    public bool TryWriteInt32(string name, int value) {
      var snapshot = _writer;
      if (_writer.WriteInt(value) && !_writer.HasFailedWrites) return true;
      _writer = snapshot;
      return false;
    }

    public bool TryWriteInt64(string name, long value) {
      var snapshot = _writer;
      if (_writer.WriteLong(value) && !_writer.HasFailedWrites) return true;
      _writer = snapshot;
      return false;
    }

    public bool TryWriteSingle(string name, float value) {
      var snapshot = _writer;
      if (_writer.WriteFloat(value) && !_writer.HasFailedWrites) return true;
      _writer = snapshot;
      return false;
    }

    public bool TryWriteDouble(string name, double value) {
      var snapshot = _writer;
      if (_writer.WriteDouble(value) && !_writer.HasFailedWrites) return true;
      _writer = snapshot;
      return false;
    }

    public bool TryWriteString(string name, string value) {
      if (value == null) return false;
      var length = Encoding.UTF8.GetByteCount(value);
      if (length <= StackStringBytes) {
        Span<byte> bytes = stackalloc byte[length];
        Encoding.UTF8.GetBytes(value.AsSpan(), bytes);
        return TryWriteUtf8(name, bytes);
      }
      var bytesArray = ArrayPool<byte>.Shared.Rent(length);
      try {
        var bytes = bytesArray.AsSpan(0, length);
        Encoding.UTF8.GetBytes(value.AsSpan(), bytes);
        return TryWriteUtf8(name, bytes);
      } finally {
        ArrayPool<byte>.Shared.Return(bytesArray);
      }
    }

    public bool TryWriteUtf8(string name, Span<byte> value) => TryWriteBuffer(value);
    public bool TryWriteBytes(string name, byte[] value) => value != null && TryWriteBuffer(value.AsSpan());
    public bool TryWriteCustom<T>(string name, T value) => false;

    private bool TryWriteByte(byte value) {
      var snapshot = _writer;
      if (_writer.WriteByte(value) && !_writer.HasFailedWrites) return true;
      _writer = snapshot;
      return false;
    }

    private bool TryWriteBuffer(Span<byte> value) {
      var snapshot = _writer;
      if (_writer.WriteInt(value.Length) && _writer.WriteBytes(value) &&
          !_writer.HasFailedWrites) return true;
      _writer = snapshot;
      return false;
    }
  }
}
