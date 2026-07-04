using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.UIElements;

namespace HELIX.NW {

  public readonly struct ContextKey<T> {

    public readonly int id;
    public readonly T defaultValue;

    private ContextKey(int id) {
      this.id = id;
      defaultValue = default;
    }

    public ContextKey(string name) {
      if (ContextKeyData.ByName.TryGetValue(name, out var target)) {
        var data = ContextKeyData.Registry.GetValueOrDefault(target);
        throw new ArgumentException($"ContextKey with name '{name}' already exists for type {data.type} with id {target}.");
      }

      id = ContextKeyData.NextId++;
      defaultValue = default;
      ContextKeyData.Registry[id] = new ContextKeyData(typeof(T), name);
    }

    public ContextKey(string name, T defaultValue) : this(name) {
      this.defaultValue = defaultValue;
    }

    public bool TryReadScope(out T value) {
      value = defaultValue;
      if (!RecompositionScope.TryGetContext(this, out var read)) return false;
      value = read.value;
      return true;
    }

    public T ReadScopeOrDefault() {
      return !RecompositionScope.TryGetContext(this, out var read) ? defaultValue : read.value;
    }

    public static implicit operator ContextKey<T>(int id) => new(id);
    public static implicit operator int(ContextKey<T> key) => key.id;

    public ContextReference<T> CreateReference() => new(this, null);

    public static ContextKeyData GetData(int id) {
      if (ContextKeyData.Registry.TryGetValue(id, out var data)) {
        return data;
      }
      throw new KeyNotFoundException($"ContextKey with id {id} not found.");
    }

    public static bool TryGetData(int id, out ContextKeyData data) {
      return ContextKeyData.Registry.TryGetValue(id, out data);
    }
  }


  public readonly struct ContextKeyData {
    internal static readonly Dictionary<int, ContextKeyData> Registry = new();
    internal static readonly Dictionary<string, int> ByName = new();
    internal static int NextId = 1;

    public readonly Type type;
    public readonly string name;

    public ContextKeyData(Type type, string name) {
      this.type = type;
      this.name = name;
    }
  }

  public abstract class ContextData : IDisposable {
    internal ContextVersion version = ContextVersion.Initial;

    public void Clean() {
      version.flags = ContextFlags.None;
    }

    public abstract void Delete();
    public abstract void Dispose();

    public void Increment(ContextFlags flags) {
      unchecked { version.counter++; }
      version.flags = flags;
    }
  }

  public class ContextData<T> : ContextData {

    public T value;
    public ContextVersion Version => version;

    public void Update(T updated) {
      value = updated;
      unchecked { version.counter++; }
      version.flags = ContextFlags.None;
    }

    public override void Delete() {
      unchecked { version.counter++; }
      version.flags = ContextFlags.Empty;
      value = default;
    }

    public override void Dispose() {
      unchecked { version.counter++; }
      version.flags = ContextFlags.Disposed;
      value = default;
    }

    public ref T GetValueRef() => ref value;
  }

  [StructLayout(LayoutKind.Sequential, Size = 8)]
  public struct ContextVersion {
    public static readonly ContextVersion Initial = new(0, ContextFlags.None);
    public static readonly ContextVersion Disposed = new(0, ContextFlags.Disposed);

    public uint counter;
    public ContextFlags flags;

    public ContextVersion(uint counter, ContextFlags flags) {
      this.counter = counter;
      this.flags = flags;
    }
  }

  [Flags]
  public enum ContextFlags { None = 0, Dirty = 1 << 1, Empty = 1 << 6, Disposed = 1 << 7, }

  public struct ContextReference<T> {
    public readonly ContextKey<T> key;
    public ContextData<T> value;
    public ContextVersion version;

    public ContextReference(ContextKey<T> key, ContextData<T> value) : this() {
      this.value = value;
      this.key = key;
      version = value?.Version ?? ContextVersion.Disposed;
    }

    public static ContextReference<T> Create(ContextKey<T> key) => new(key, null);
    public static ContextReference<T> Create(ContextKey<T> key, ContextData<T> value) => new(key, value);

    public static implicit operator ContextReference<T>(ContextKey<T> key) => new(key, null);
    public static implicit operator ContextKey<T>(ContextReference<T> reference) => reference.key;

    public bool IsDirty => version.flags.HasFlag(ContextFlags.Dirty);
    public bool IsEmpty => version.flags.HasFlag(ContextFlags.Empty);
    public bool IsDisposed => version.flags.HasFlag(ContextFlags.Disposed);
    public bool HasValue => version.flags < ContextFlags.Empty;

    public bool Refresh(ContextData<T> read) {
      if (read != value) {
        value = read;
        version = read.Version;
        version.flags |= ContextFlags.Dirty;
        return true;
      }

      if (value.version.counter == version.counter) {
        version.flags &= ~ContextFlags.Dirty;
        return false;
      }

      version = value.version;
      version.flags |= ContextFlags.Dirty;
      return true;
    }

    public void Pull() {
      if (RecompositionScope.TryGetContext(key, out var current)) {
        Refresh(current);
      } else {
        if (version.flags < ContextFlags.Empty) { // We previously had data, this is technically dirty
          version.flags = ContextFlags.Dirty | ContextFlags.Disposed;
          value = null;
        } else { // No data, so this doesn't affect consumers
          Reset();
        }
      }
    }

    public void Reset() {
      value = null;
      version = ContextVersion.Disposed;
    }

    public bool TryRead(out T output) {
      if (version.flags >= ContextFlags.Empty) {
        output = default;
        return false;
      }

      output = value.value;
      return true;
    }

    public T GetOrDefault(T defaultValue) {
      return TryRead(out var result) ? result : defaultValue;
    }
  }

  public interface IEmitEvent {
    public object BoxedValue { get; }

    public static void Send<T>(IComposable composable, T value) {
      using var evt = EmitEvent<T>.GetPooled(value);
      composable.Element.SendEvent(evt);
    }
  }

  public class EmitEvent<T> : EventBase<EmitEvent<T>>, IEmitEvent {
    public T Value;

    static EmitEvent() {
      SetCreateFunction(() => new EmitEvent<T>());
    }

    public object BoxedValue => Value;

    public static EmitEvent<T> GetPooled(T data) {
      var evt = GetPooled();
      evt.Value = data;
      return evt;
    }

    protected override void Init() {
      base.Init();
      bubbles = true;
      tricklesDown = false;
      Value = default;
    }
  }
}