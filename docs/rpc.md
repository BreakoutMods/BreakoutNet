# BreakoutNet RPC

## Packet Lifecycle

1. A mod registers a typed handler with `BreakoutRpc.Server.Register<T>()` or `BreakoutRpc.Client.Register<T>()`.
2. The sender calls `SendToServer`, `SendToClient`, `Broadcast`, `BroadcastExcept`, or `BroadcastNear`.
3. BreakoutNet writes an envelope before the DTO payload.
4. Valheim delivers the package through one shared routed RPC endpoint: `BreakoutNet.RPC`.
5. BreakoutNet validates the envelope, side rules, DTO type, and rate limits.
6. The registered handler receives a `BreakoutRpcContext` and a deserialized DTO.

## Envelope

Every package starts with:

- protocol version
- RPC name
- sender mod GUID
- message type name
- sequence number

The DTO writes its own fields after that header using `IBreakoutSerializable`.

## Naming

Use globally unique RPC names:

```text
joinguard.check
joinguard.check.result
voip.settings
discordadmin.command
```

Avoid generic names such as `sync`, `request`, or `message`.

## Validation

BreakoutNet rejects:

- unknown protocol versions
- malformed packages
- empty RPC names
- unregistered RPC names
- messages sent to client handlers by non-server peers
- DTO type mismatches
- excessive inbound client RPCs per peer and RPC name

Use `context.Reject("reason")` when your own server-side policy denies a request.

## Broadcast Helpers

```csharp
BreakoutRpc.Server.Broadcast("mymod.event", message);
BreakoutRpc.Server.BroadcastExcept(senderPeerId, "mymod.event", message);
BreakoutRpc.Server.BroadcastNear(position, radius, "mymod.event", message);
```

`BroadcastNear` uses the server-known peer reference position. This is intended for proximity systems such as VOIP, local admin tools, and RP actions.
