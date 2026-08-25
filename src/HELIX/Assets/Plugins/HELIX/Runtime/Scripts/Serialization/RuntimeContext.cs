using System;
using System.Collections.Generic;
using System.IO;
using HELIX.Prose;
using Newtonsoft.Json;
using Unity.Collections;

namespace HELIX.Serialization {
  public interface IStringDatatypeFactory {
    IUniversalReader Create(TextReader reader);
    IUniversalWriter Create(TextWriter writer);
  }

  public interface IBinaryDatatypeFactory {
    IUniversalReader Create(DataStreamReader reader);
    IUniversalWriter Create(DataStreamWriter writer);
  }

  public sealed class NewtonsoftJsonDatatypeFactory : IStringDatatypeFactory {
    private readonly JsonSerializer _serializer;

    public NewtonsoftJsonDatatypeFactory(JsonSerializer serializer = null) =>
      _serializer = serializer ?? JsonSerializer.CreateDefault();

    public IUniversalReader Create(TextReader reader) => new NewtonsoftJsonUniversalReader(
      new JsonTextReader(reader ?? throw new ArgumentNullException(nameof(reader))), _serializer
    );

    public IUniversalWriter Create(TextWriter writer) => new NewtonsoftJsonUniversalWriter(
      new JsonTextWriter(writer ?? throw new ArgumentNullException(nameof(writer))), _serializer
    );
  }

  public sealed class DataStreamDatatypeFactory : IBinaryDatatypeFactory {
    public IUniversalReader Create(DataStreamReader reader) => new DataStreamUniversalReader(reader);
    public IUniversalWriter Create(DataStreamWriter writer) => new DataStreamUniversalWriter(writer);
  }

  /// <summary>
  /// Instance-scoped datatype configuration and reusable marshalling state.
  /// Contexts are not thread-safe; use one context per concurrent serialization flow.
  /// </summary>
  public sealed class HXRuntimeContext : IDisposable {
    private bool _disposed;

    public HXRuntimeContext(
      IStringDatatypeFactory stringFactory = null,
      IBinaryDatatypeFactory binaryFactory = null,
      ProseTextWriter proseWriter = null
    ) {
      StringFactory = stringFactory ?? new NewtonsoftJsonDatatypeFactory();
      BinaryFactory = binaryFactory ?? new DataStreamDatatypeFactory();
      ProseWriter = proseWriter ?? new ProseTextWriter();
      Extra = new Dictionary<object, object>();
    }

    public IStringDatatypeFactory StringFactory { get; set; }
    public IBinaryDatatypeFactory BinaryFactory { get; set; }
    public ProseTextWriter ProseWriter { get; }
    public IDictionary<object, object> Extra { get; }

    public ProseTextWriter ResetProseWriter() {
      ThrowIfDisposed();
      ProseWriter.Reset();
      return ProseWriter;
    }

    public IUniversalReader CreateStringReader(TextReader reader) {
      ThrowIfDisposed();
      return (StringFactory ?? throw MissingFactory(nameof(StringFactory))).Create(reader);
    }

    public IUniversalWriter CreateStringWriter(TextWriter writer) {
      ThrowIfDisposed();
      return (StringFactory ?? throw MissingFactory(nameof(StringFactory))).Create(writer);
    }

    public IUniversalReader CreateBinaryReader(DataStreamReader reader) {
      ThrowIfDisposed();
      return (BinaryFactory ?? throw MissingFactory(nameof(BinaryFactory))).Create(reader);
    }

    public IUniversalWriter CreateBinaryWriter(DataStreamWriter writer) {
      ThrowIfDisposed();
      return (BinaryFactory ?? throw MissingFactory(nameof(BinaryFactory))).Create(writer);
    }

    public void Dispose() {
      if (_disposed) return;
      _disposed = true;
      Extra.Clear();
      ProseWriter.Reset();
    }

    private void ThrowIfDisposed() {
      if (_disposed) throw new ObjectDisposedException(nameof(HXRuntimeContext));
    }

    private static InvalidOperationException MissingFactory(string name) =>
      new($"Datatype context has no {name} configured.");
  }
}
