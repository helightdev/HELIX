// Port of Odin.hix.
using System;
using Helix.Weave;

namespace HELIX.Weave;

[Mixin(Unit.Type, Attribute = "HELIX.OdinSerializableAttribute")]
public sealed class OdinSerializable : Attribute {
  [Weave]
  static void Apply(TypeTarget t, OdinSerializable a) {
    t.Using("UnityEngine", "Sirenix.Serialization", "Sirenix.OdinInspector");
    t.Implements("ISerializationCallbackReceiver", "ISupportsPrefabSerialization");
    t.Annotation("ShowOdinSerializedPropertiesInInspector");
    t.Class("""
      [SerializeField, HideInInspector]
      private SerializationData serializationData;

      SerializationData ISupportsPrefabSerialization.SerializationData {
        get { return this.serializationData; }
        set { this.serializationData = value; }
      }

      void ISerializationCallbackReceiver.OnAfterDeserialize()
        { UnitySerializationUtility.DeserializeUnityObject(this, ref this.serializationData); }

      void ISerializationCallbackReceiver.OnBeforeSerialize()
        { UnitySerializationUtility.SerializeUnityObject(this, ref this.serializationData); }
      """);
  }
}

[Mixin(Unit.Type, Attribute = "HELIX.OdinSerializableScriptAttribute")]
public sealed class OdinSerializableScript : Attribute {
  [Weave]
  static void Apply(TypeTarget t, OdinSerializableScript a) {
    t.Using("UnityEngine", "Sirenix.Serialization", "Sirenix.OdinInspector");
    t.Implements("ISerializationCallbackReceiver");
    t.Annotation("ShowOdinSerializedPropertiesInInspector");
    t.Class("""
      [SerializeField, HideInInspector]
      private SerializationData serializationData;

      void ISerializationCallbackReceiver.OnAfterDeserialize()
        { UnitySerializationUtility.DeserializeUnityObject(this, ref this.serializationData); }

      void ISerializationCallbackReceiver.OnBeforeSerialize()
        { UnitySerializationUtility.SerializeUnityObject(this, ref this.serializationData); }
      """);
  }
}
