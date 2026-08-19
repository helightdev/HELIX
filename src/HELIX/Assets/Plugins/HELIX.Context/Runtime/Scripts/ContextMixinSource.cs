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


// EventHandler method implementation
[assembly: MixinPrepareGlobal(
  @"
@FUNC<EventHandlerImpl>
  @CALL<RequireEventHandler>

  @SCOPE
    @MATCH @var#IsComponent:?eq<true>
    @CODE<$ConfigureComponent> registration.RegisterHandlerBinding<@arg#0:type>(@attr#priority)
  @END

  @SCOPE
    @MATCH @arg#0:?is<IAsyncChainEvt>
    @ASSERT @arg#0:?argument
    @CODE<$Init> eventHandlerList.RegisterAsync<@arg#0:type>(@target, @attr#priority);
    @RETURN
  @SCOPE
    @MATCH @arg#0:?is<Evt>
    @MATCH @arg#0:?ref
    @CODE<$Init> eventHandlerList.Register<@arg#0:type>(@target, @attr#priority);
    @RETURN
  @SCOPE
    @MATCH @arg#0:?is<Evt>
    @MATCH @arg#0:?argument
    @CODE<$Init> eventHandlerList.Register<@arg#0:type>(@target, @attr#priority);
    @RETURN
  @END
@END
"
)]

// Component implementation
[assembly: MixinPrepareGlobal(
  @"
@FUNC<ComponentImpl>
  @USING HELIX.Context;
  @USING UnityEngine;
  @CODE<$ConfigureComponent> registration.name = ""@this:name"";
  @CODE<$ConfigureComponent> registration.optional = @attr#optional;
  @CODE<$ConfigureComponent> registration.phase = @attr#phase;
  @CODE<$ConfigureComponent> registration.order = @attr#order;
  @CODE<IMPLEMENTS> IComponent
  @CODE<CLASS> public RuntimeComponentData ComponentBinding { get; } = new();
  @VAR<IsComponent> true

  @SCOPE
    @MATCH@attr#scope:?eq<null>
    @GOTO<Activator>
  @SCOPE
    @CODE<$ConfigureComponent> registration.scope = @attr#scope;
  @END

  @SCOPE<Activator>
  @SCOPE
    @MATCH @this:?is<MonoBehaviour>
    @CODE<$ConfigureComponent> registration.activator = DefaultComponentActivators.MonoBehaviour<@this:type>();
    @GOTO<End>
  @SCOPE
    @CODE<$ConfigureComponent> registration.activator = DefaultComponentActivators.PlainObject<@this:type>();
  @END

  @SCOPE<End>
  @END
@END
"
)]

// Bind method impl
[assembly: MixinPrepareGlobal(
  @"
@FUNC<BindImpl>
  @USING HELIX.Context;
  @LOCAL<Value> @target:name
  @LOCAL<Method> PublishKey
  @LOCAL<Guard> 
  @LOCAL<Type> typeof(@target:type:unwrap)
  @SCOPE
    @MATCH @attr#proxied:?eq<true>
    @LOCAL<Value> () => @target:name
    @LOCAL<Method> PublishProxyKey
  @SCOPE
    @MATCH @attr#required:?eq<false>
    @LOCAL<Guard> if (@target:name != null)
  @SCOPE
    @MATCH @attr#type:?exists
    @MATCH @attr#type:!?eq<null>
    @LOCAL<TYPE> @attr#type
  @END

  @LOCAL<TypeKey> new TypeKey(@local#Type, @attr#qualifier)
  @LOCAL<Dependency> new ComponentDependency(@local#TypeKey, @attr#required)
  @CODE<$ConfigureComponent> registration.Publication(@local#Dependency);
  @CODE<$LoadComponentLate> @(local#Guard)context.@local#Method(@local#TypeKey, @local#Value);
@END
"
)]
// Base implementation for the [Resource] attribute
[assembly: MixinPrepareGlobal(
  @"
@FUNC<ResourceImpl>
  @CALL<RequireEventHandler>

  @SCOPE
    @MATCH @target:type:?is<System.Collections.IEnumerable>
    @MATCH @target:type:!?is<string>
    @MATCH @target:type#0:?exists
    @CALL<ResourceImplList>
    @RETURN
  @END

  @CALL<ResourceImplSingle>
  @RETURN
@END

@FUNC<ResourceImplSingle>
  @LOCAL<WireKey> @@""@target:type:unwrap|@attr#source|@attr#qualifier:unwrap""
  @ASSERT @target:type:?class
  @ASSERT @target:type:?is<UnityEngine.Object>

  @SCOPE<Addressables>
    @MATCH @attr#source:?eq<2>
    @CODE<$ConfigureComponent> registration.Dependency(new ComponentDependency(new AddressableDependency<@target:type>(@attr#qualifier, @local#WireKey), true));
    @CODE<$Init> @target:name = ComponentBinding.Resolve<@target:type>(@local#WireKey);
    @RETURN
  @SCOPE<Resources>
    @MATCH @attr#source:?eq<3>
    @CODE<$ConfigureComponent> registration.Dependency(new ComponentDependency(new ResourceDependency<@target:type>(@attr#qualifier, @local#WireKey), true));
    @CODE<$Init> @target:name = ComponentBinding.Resolve<@target:type>(@local#WireKey);
    @RETURN
  @END
  @FAIL No valid injection source found for the target type.
@END

@FUNC<ResourceImplList>
  @USING System.Collections.Generic
  @USING System.Collections
  @LOCAL<WireKey> @@""list|@target:type#0:unwrap|@attr#source|@attr#qualifier:unwrap""
  
  @ASSERT @target:type#0:?class
  @ASSERT @target:type#0:?is<UnityEngine.Object>

  @SCOPE<AddressableList>
    @MATCH @attr#source:?eq<2>
    @CODE<$ConfigureComponent> registration.Dependency(new ComponentDependency(new AddressableListDependency<@target:type#0>(@attr#qualifier, @local#WireKey), true));
    @CODE<$Init> @target:name = ComponentBinding.Resolve<List<@target:type#0>>(@local#WireKey) as @target:type;
    @RETURN
  @SCOPE<ResourceList>
    @MATCH @attr#source:?eq<3>
    @CODE<$ConfigureComponent> registration.Dependency(new ComponentDependency(new ResourceListDependency<@target:type#0>(@attr#qualifier, @local#WireKey), true));
    @CODE<$Init> @target:name = ComponentBinding.Resolve<List<@target:type#0>>(@local#WireKey) as @target:type;
    @RETURN
  @END
  @FAIL No valid injection source found for the collected target type.
@END
"
)]


// Base DI implementation for the [Inject] attribute
[assembly: MixinPrepareGlobal(
  @"
@FUNC<InjectDiImpl>
  @USING HELIX.Context;
  @CALL<RequireEventHandler>

  @SCOPE
    @MATCH @target:type:?is<System.Collections.IEnumerable>
    @MATCH @target:type:!?is<string>
    @MATCH @target:type#0:?exists
    @CALL<InjectDiImplList>
    @RETURN
  @END

  @CODE<$ConfigureComponent> registration.Dependency(new ComponentDependency(new TypeKey(typeof(@target:type), @attr#qualifier), @attr#required));
  @SCOPE
    @MATCH @attr#required:?eq<false>
    @CODE<$Init> @target:name = ComponentBinding.ResolveOptional<@target:type>(@attr#qualifier);
    @RETURN
  @END
  @CODE<$Init> @target:name = ComponentBinding.Resolve<@target:type>(@attr#qualifier);
@END

@FUNC<InjectDiImplList>
  @USING System.Collections.Generic;
  @ASSERT @target:type#0:?class
  @CODE<$ConfigureComponent> registration.Dependency(new ComponentDependency(new TypeKey(typeof(@target:type#0), @attr#qualifier), false, true));
  @CODE<$Init> @target:name = ComponentBinding.ResolveAll<@target:type#0>(@attr#qualifier) as @target:type;
@END
"
)]

// Ticker method implementation
[assembly: MixinPrepareGlobal(
  @"
@FUNC<TickerImpl>
  @USING HELIX.Context;

  @LOCAL<Time> -0f
  @SCOPE
    @MATCH @attr#time:?exists
    @LOCAL<Time> @attr#time:floatTime
  @END
  @LOCAL<TickerField> _@(target:name)Ticker

  @SCOPE
    @MATCH @local#Time:matches<^-.*>
    @LOCAL<Time> @local#Time:replace<-|f><>
    @CODE<CLASS> private FrameCountTicker @local#TickerField = new(@local#Time);
    @GOTO<HookCaller>
  @SCOPE
    @CODE<CLASS> private FrameTimeTicker @local#TickerField = new(@local#Time);
  @END

  @SCOPE<HookCaller>
    @LOCAL<TickerCondition> @local#TickerField.Tick()
  @SCOPE
    @MATCH @attr#condition:!?eq<null>
    @LOCAL<TickerCondition> @attr#condition:unwrap && @local#TickerCondition
  @SCOPE
    @MATCH @var#IsComponent:?eq<true>
    @LOCAL<TickerCondition> ComponentBinding.IsActive && @local#TickerCondition
  @END

  @LOCAL<InvokeTicker> @target:name();
  @SCOPE
    @MATCH @target:?async
    @ASSERT @target:?is<Cysharp.Threading.Tasks.UniTask>
    @USING Cysharp.Threading.Tasks;
    @LOCAL<AsyncProxyName> _@(target:name)AsyncProxy
    @CODE<CLASS> private async UniTaskVoid @local#AsyncProxyName() {
    @CODE<CLASS>  try {  @local#TickerField.running = true; await @local#InvokeTicker }
    @CODE<CLASS>  finally { @local#TickerField.running = false; }
    @CODE<CLASS> }
    @LOCAL<InvokeTicker> @local#AsyncProxyName().Forget();
  @END

  @MIXIN<(@attr#target:unwrap)><(@attr#order)> if (@local#TickerCondition) @local#InvokeTicker
@END
"
)]