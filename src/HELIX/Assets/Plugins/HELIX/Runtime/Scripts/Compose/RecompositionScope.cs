using System;

namespace HELIX.Compose {
  public readonly struct RecompositionScope : IDisposable {
    private readonly bool _hasClaimed;

    internal RecompositionScope(bool hasClaimed) {
      _hasClaimed = hasClaimed;
    }

    public void Dispose() {
      if (_hasClaimed) HXComposer.ProcessDirty();
    }
  }
}