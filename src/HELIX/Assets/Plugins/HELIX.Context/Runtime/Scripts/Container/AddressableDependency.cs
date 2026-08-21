using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace HELIX.Context {
  public sealed class AddressableDependency<T> : ManagedDependency where T : UnityEngine.Object {
    public string Key { get; }

    public AddressableDependency(string address, string wireKey) : base(
      wireKey,
      flags: DependencyFlags.Wirable | DependencyFlags.ImplicitLoadable | DependencyFlags.Async
    ) {
      Key = ValidateAddress(address);
    }

    public override async UniTask<ManagedLoadResult> LoadAsync(ManagedLoadContext context) {
      var handle = Addressables.LoadAssetAsync<T>(Key);
      try {
        var value = await handle.ToUniTask(
          cancellationToken: context.CancellationToken,
          autoReleaseWhenCanceled: true
        );
        if (value == null)
          throw new ComponentInitializationException($"Addressable '{Key}' returned null for {typeof(T).FullName}.");
        context.Own(new AddressableHandleLease(handle));
        context.PublishKey(new TypeKey(typeof(T), WireKey), value);
        return new ManagedLoadResult(true);
      } catch {
        Release(handle);
        throw;
      }
    }

    public override ManagedLoadResult Load(ManagedLoadContext context) => AsyncRequired(Key);

    internal static string ValidateAddress(string address) => string.IsNullOrWhiteSpace(address)
      ? throw new ArgumentException("An Addressables address or key is required.", nameof(address))
      : address;

    private static ManagedLoadResult AsyncRequired(string address) => throw new AsyncScopeInitializationException(
      $"Addressable '{address}' requires asynchronous scope initialization."
    );

    internal static void Release(AsyncOperationHandle handle) {
      if (handle.IsValid()) Addressables.Release(handle);
    }

    internal sealed class AddressableHandleLease : IDisposable {
      private AsyncOperationHandle _handle;
      public AddressableHandleLease(AsyncOperationHandle handle) => _handle = handle;

      public void Dispose() {
        Release(_handle);
        _handle = default;
      }
    }
  }

  public sealed class AddressableListDependency<T> : ManagedDependency where T : UnityEngine.Object {
    public string Key { get; }

    public AddressableListDependency(string key, string wireKey) : base(
      wireKey,
      flags: DependencyFlags.Wirable | DependencyFlags.ImplicitLoadable | DependencyFlags.Async
    ) {
      Key = AddressableDependency<T>.ValidateAddress(key);
    }

    public override async UniTask<ManagedLoadResult> LoadAsync(ManagedLoadContext context) {
      var handle = Addressables.LoadAssetsAsync<T>(Key, null);
      try {
        var values = await handle.ToUniTask(
          cancellationToken: context.CancellationToken,
          autoReleaseWhenCanceled: true
        );
        var list = values.ToList();
        context.Own(new AddressableDependency<T>.AddressableHandleLease(handle));
        context.PublishKey(new TypeKey(typeof(List<T>), WireKey), list);
        return new ManagedLoadResult(true);
      } catch {
        AddressableDependency<T>.Release(handle);
        throw;
      }
    }

    public override ManagedLoadResult Load(ManagedLoadContext context) {
      throw new AsyncScopeInitializationException(
        $"Addressables key '{Key}' requires asynchronous scope initialization."
      );
    }
  }
}