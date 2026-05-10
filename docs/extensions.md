# BreakoutNet Extension Events

BreakoutNet includes a local in-process event layer for mod authors who need extension points before a feature belongs in BreakoutNet core.

Events are not networked. Use RPCs for client/server traffic.

## Scoped Typed Events

Typed events are scoped to the mod GUID. They are ideal for communication between a mod's shared, client, and server modules.

```csharp
public sealed class PolicyCheckedEvent : IBreakoutEvent
{
    public bool Allowed;
    public string Reason;
}

Context.Events.Subscribe<PolicyCheckedEvent>(OnPolicyChecked);
Context.Events.Publish(new PolicyCheckedEvent { Allowed = true });
```

## Named Events

Named events are for documented public extension points other mods may subscribe to.

```csharp
Context.Events.Subscribe<PolicyCheckedEvent>(
    "joinguard.policy.checked",
    OnExternalPolicyChecked);

Context.Events.Publish(
    "joinguard.policy.checked",
    new PolicyCheckedEvent { Allowed = true });
```

Use globally readable names such as `joinguard.policy.checked` or `voip.voice.relayed`.

## Subscription Cleanup

Every subscription returns a token:

```csharp
BreakoutSubscription subscription =
    Context.Events.Subscribe<MyEvent>(Handle);

subscription.Dispose();
```

Subscriptions created through a `BreakoutModApp` context are disposed automatically when the app or owning plugin is destroyed.

## Core Hooks

Core hooks are read-only observations. They cannot cancel BreakoutNet behavior.

```csharp
Context.Hooks.OnNetworkReady(OnNetworkReady);
Context.Hooks.OnWorldLeft(OnWorldLeft);
Context.Hooks.OnPeerJoined(OnPeerJoined);
Context.Hooks.OnPeerLeft(OnPeerLeft);
Context.Hooks.OnRpcReceived(OnRpcReceived);
Context.Hooks.OnRpcRejected(OnRpcRejected);
```

Use hooks for diagnostics, prototype integrations, HUD state, and compatibility experiments.
