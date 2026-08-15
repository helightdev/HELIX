using HELIX.Context;

namespace HELIX {
  [MixinExpression(
    new string[0],
    new int[0],
    @"
@CODE<IMPLEMENTS> global::UnityEngine.ISerializationCallbackReceiver
@CODE<CLASS> [global::UnityEngine.SerializeField, global::UnityEngine.HideInInspector]
@CODE<CLASS> private global::Sirenix.Serialization.SerializationData serializationData;
@CODE<CLASS> void global::UnityEngine.ISerializationCallbackReceiver.OnAfterDeserialize(){ global::Sirenix.Serialization.UnitySerializationUtility.DeserializeUnityObject(this, ref this.serializationData); }
@CODE<CLASS> void global::UnityEngine.ISerializationCallbackReceiver.OnBeforeSerialize() { global::Sirenix.Serialization.UnitySerializationUtility.SerializeUnityObject(this, ref this.serializationData); }
@CODE<FILE> [global::Sirenix.OdinInspector.ShowOdinSerializedPropertiesInInspector] public partial class @this:name {}
"
  )]
  [Mixin] public interface IOdinSerializableMixin : IMixin { }

  [MixinExpression(
    new string[0],
    new int[0],
    @"
@CODE<IMPLEMENTS> global::UnityEngine.ISerializationCallbackReceiver
@CODE<IMPLEMENTS> global::Sirenix.Serialization.ISupportsPrefabSerialization
@CODE<CLASS> [global::UnityEngine.SerializeField, global::UnityEngine.HideInInspector]
@CODE<CLASS> private global::Sirenix.Serialization.SerializationData serializationData;
@CODE<CLASS> global::Sirenix.Serialization.SerializationData global::Sirenix.Serialization.ISupportsPrefabSerialization.SerializationData { get { return this.serializationData; } set { this.serializationData = value; } }
@CODE<CLASS> void global::UnityEngine.ISerializationCallbackReceiver.OnAfterDeserialize(){ global::Sirenix.Serialization.UnitySerializationUtility.DeserializeUnityObject(this, ref this.serializationData); }
@CODE<CLASS> void global::UnityEngine.ISerializationCallbackReceiver.OnBeforeSerialize() { global::Sirenix.Serialization.UnitySerializationUtility.SerializeUnityObject(this, ref this.serializationData); }
@CODE<FILE> [global::Sirenix.OdinInspector.ShowOdinSerializedPropertiesInInspector] public partial class @this:name {}
"
  )]
  [Mixin] public interface IOdinSerializableScriptMixin : IMixin { }
}