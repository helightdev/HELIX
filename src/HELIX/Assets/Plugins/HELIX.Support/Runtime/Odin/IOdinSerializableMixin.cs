using HELIX;

namespace HELIX {
  [MixinExpression(
    new string[0],
    new int[0],
    @"
@USING UnityEngine;
@USING Sirenix.Serialization;
@USING Sirenix.OdinInspector;

@CODE<IMPLEMENTS> ISerializationCallbackReceiver
@CODE<ANNOTATION> ShowOdinSerializedPropertiesInInspector
@CODE<CLASS> [SerializeField, HideInInspector]
@CODE<CLASS> private SerializationData serializationData;
@CODE<CLASS> void ISerializationCallbackReceiver.OnAfterDeserialize() { UnitySerializationUtility.DeserializeUnityObject(this, ref this.serializationData); }
@CODE<CLASS> void ISerializationCallbackReceiver.OnBeforeSerialize() { UnitySerializationUtility.SerializeUnityObject(this, ref this.serializationData); }
"
  )]
  [Mixin] public interface IOdinSerializableMixin : IMixin { }

  [MixinExpression(
    new string[0],
    new int[0],
    @"
@USING UnityEngine;
@USING Sirenix.Serialization;
@USING Sirenix.OdinInspector;

@CODE<IMPLEMENTS> ISerializationCallbackReceiver
@CODE<IMPLEMENTS> ISupportsPrefabSerialization
@CODE<ANNOTATION> ShowOdinSerializedPropertiesInInspector
@CODE<CLASS> [SerializeField, HideInInspector]
@CODE<CLASS> private SerializationData serializationData;
@CODE<CLASS> SerializationData ISupportsPrefabSerialization.SerializationData { get { return this.serializationData; } set { this.serializationData = value; } }
@CODE<CLASS> void ISerializationCallbackReceiver.OnAfterDeserialize() { UnitySerializationUtility.DeserializeUnityObject(this, ref this.serializationData); }
@CODE<CLASS> void ISerializationCallbackReceiver.OnBeforeSerialize() { UnitySerializationUtility.SerializeUnityObject(this, ref this.serializationData); }
"
  )]
  [Mixin] public interface IOdinSerializableScriptMixin : IMixin { }
}
