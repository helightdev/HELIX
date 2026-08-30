using System;

namespace HELIX {
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
  public sealed class OdinSerializableAttribute : Attribute, IMixin { }

  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
  public sealed class OdinSerializableScriptAttribute : Attribute, IMixin { }
}
