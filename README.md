# BreakoutMods BreakoutNet

Part of the BreakoutMods Valheim modding suite.

BreakoutNet is a small client/server helper library for Valheim mods. It wraps the repetitive BepInEx and `ZRoutedRpc` glue that most server-aware mods need: side detection, peer lookup, typed RPC messages, server-authoritative settings sync, proximity broadcasts, logging, and basic abuse protection.

GitHub: [BreakoutMods/BreakoutNet](https://github.com/BreakoutMods/BreakoutNet)

## Why BreakoutNet Exists

Valheim mods often need the same networking building blocks:

- The client asks the server something.
- The server validates the request and replies.
- The server broadcasts an event to all players or nearby players.
- Clients apply server-owned settings and reject spoofed settings.

BreakoutNet keeps those patterns boring and reusable so mods can share the same network layer.

## Terminology

- **Dedicated server:** headless server process. It should relay and validate, but not create client-only UI/audio/input systems.
- **Client:** a normal game client connected to a server.
- **Listen server:** a player hosting a world locally. This side is both server-authoritative and a playable client.
- **In world:** `ZNet` and `ZRoutedRpc` are available.

## Typed RPC Quickstart

```csharp
public sealed class JoinCheckRequest : IBreakoutSerializable
{
    public string ModListHash;

    public void Write(ZPackage package) => package.Write(ModListHash ?? string.Empty);
    public void Read(ZPackage package) => ModListHash = package.ReadString();
}

BreakoutRpc.Server.Register<JoinCheckRequest>(
    "joinguard.check",
    (context, request) =>
    {
        if (string.IsNullOrWhiteSpace(request.ModListHash))
        {
            context.Reject("Client did not send a mod list hash.");
            return;
        }

        BreakoutRpc.Server.SendToClient(
            context.SenderPeerId,
            "joinguard.ok",
            new JoinCheckResult(),
            "com.breakoutmods.valheim.joinguard");
    });
```

Clients send to the server with:

```csharp
BreakoutRpc.Client.SendToServer(
    "joinguard.check",
    new JoinCheckRequest { ModListHash = localHash },
    "com.breakoutmods.valheim.joinguard");
```

## Settings Sync

```csharp
BreakoutSettingsSync.RegisterServerSettings(
    "joinguard.settings",
    () => new JoinGuardSettings { RequireMatchingMods = true });

BreakoutSettingsSync.Client.Register<JoinGuardSettings>(
    "joinguard.settings",
    settings => ApplyServerSettings(settings));
```

The server broadcasts registered settings periodically and when a session becomes available. Clients only apply settings packets that arrive from the server peer.

## Side And Peer Helpers

```csharp
if (BreakoutSide.IsDedicatedServer)
{
    // Server relay only.
}

foreach (ZNetPeer peer in BreakoutPeers.ConnectedPeers)
{
    // Server-visible peers.
}
```

## Safe Defaults

Every BreakoutNet package includes:

- protocol version
- RPC name
- sender mod GUID
- message type name
- sequence number

BreakoutNet rejects unknown protocol versions, unregistered RPC names, client-side messages from non-server peers, mismatched DTO types, and excessive inbound client RPCs.

## Build

```powershell
.\build.ps1 -Configuration Release
```

Deploy into this Valheim server install only when you explicitly ask for it:

```powershell
.\build.ps1 -Configuration Release -Deploy
```

## Dependencies

- BepInEx 5.x
- Valheim assemblies
