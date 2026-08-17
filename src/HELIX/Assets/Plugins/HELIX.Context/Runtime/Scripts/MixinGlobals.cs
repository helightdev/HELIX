using HELIX.Context;

// Variable guarded event handler mixin block
[assembly: MixinPrepareGlobal(
  @"
@FUNC<RequireEventHandler>
  @SCOPE
    @MATCH @var#IsEventHandler:?eq<true>
    @RETURN
  @END

  @USING HELIX.Context;
  @USING System;
  @CODE<CLASS> [NonSerializedAttribute]
  @CODE<CLASS> protected readonly EventHandlerList eventHandlerList = EventHandlerList.Create();
  @CODE<CLASS> EventHandlerList IEventListener.HandlerList => eventHandlerList;

  @CODE<$Dispose> eventHandlerList.UnregisterAll();
  @CODE<IMPLEMENTS> IEventListener
  @VAR<IsEventHandler> true
@END
"
)]

// Base implementation for the [Inject] attribute
[assembly: MixinPrepareGlobal(
  @"
@FUNC<InjectImplSingle>
  @LOCAL<WireKey> @@""@target:type:unwrap|@attr#source|@attr#qualifier:unwrap""
  @SCOPE
    @MATCH @target:type:?class
    @MATCH @target:type:?is<UnityEngine.Object>
    @LOCAL<InjectIsUnityObject> true
  @END

  @SCOPE<Container>
    @MATCH @attr#source:?eq<0>
    @CODE<$ConfigureComponent> registration.Dependency(typeof(@target:type), @attr#qualifier);
    @CODE<$Init> @target:name = RuntimeComponentData.Resolve<@target:type>(@attr#qualifier);
    @RETURN
  @SCOPE<Addressables>
    @MATCH @attr#source:?eq<2>
    @MATCH<NoUnityObject> @local#InjectIsUnityObject:?eq<true>
    @CODE<$ConfigureComponent> registration.Dependency(new ComponentDependency(new AddressableDependency<@target:type>(@attr#qualifier, @local#WireKey), true));
    @CODE<$Init> @target:name = RuntimeComponentData.Resolve<@target:type>(@local#WireKey);
    @RETURN
  @SCOPE<Resources>
    @MATCH @attr#source:?eq<3>
    @MATCH<NoUnityObject> @local#InjectIsUnityObject:?eq<true>
    @CODE<$ConfigureComponent> registration.Dependency(new ComponentDependency(new ResourceDependency<@target:type>(@attr#qualifier, @local#WireKey), true));
    @CODE<$Init> @target:name = RuntimeComponentData.Resolve<@target:type>(@local#WireKey);
    @RETURN
  @END
  @FAIL No valid injection source found for the target type.

  @SCOPE<NoUnityObject>
    @FAIL This source requires the injection of a unity object type, but the target is not a subtype of UnityEngine.Object.
  @END
@END

@FUNC<InjectImplList>
  @USING System.Collections.Generic
  @USING System.Collections
  @LOCAL<WireKey> @@""list|@target:type#0:unwrap|@attr#source|@attr#qualifier:unwrap""
  
  @SCOPE
    @MATCH @target:type#0:?class
    @MATCH @target:type#0:?is<UnityEngine.Object>
    @LOCAL<InjectIsUnityObject> true
  @END

  @SCOPE<AddressableList>
    @MATCH @attr#source:?eq<2>
    @MATCH<NoUnityObjectList> @local#InjectIsUnityObject:?eq<true>
    @CODE<$ConfigureComponent> registration.Dependency(new ComponentDependency(new AddressableListDependency<@target:type#0>(@attr#qualifier, @local#WireKey), true));
    @CODE<$Init> @target:name = RuntimeComponentData.Resolve<List<@target:type#0>>(@local#WireKey) as @target:type;
    @RETURN
  @SCOPE<ResourceList>
    @MATCH @attr#source:?eq<3>
    @MATCH<NoUnityObjectList> @local#InjectIsUnityObject:?eq<true>
    @CODE<$ConfigureComponent> registration.Dependency(new ComponentDependency(new ResourceListDependency<@target:type#0>(@attr#qualifier, @local#WireKey), true));
    @CODE<$Init> @target:name = RuntimeComponentData.Resolve<List<@target:type#0>>(@local#WireKey) as @target:type;
    @RETURN
  @END
  @FAIL No valid injection source found for the collected target type.

  @SCOPE<NoUnityObjectList>
    @FAIL This source requires the injection of a unity object type, but the target is not a subtype of UnityEngine.Object.
  @END
@END

@FUNC<InjectImpl>
  @SCOPE
    @MATCH @target:type:?is<System.Collections.IEnumerable>
    @MATCH @target:type:!?is<string>
    @MATCH @target:type#0:?exists
    @CALL<InjectImplList>
    @RETURN
  @END

  @CALL<InjectImplSingle>
  @RETURN
@END
"
)]