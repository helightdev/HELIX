using System;
using HELIX.Context;
using UnityEngine;

namespace HELIX {
  [EnableMixins]
  public partial class TestExample : MonoBehaviour, IOdinSerializableScriptMixin {
    public int b;
  }
}