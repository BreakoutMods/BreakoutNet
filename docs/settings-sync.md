# BreakoutNet Settings Sync

Settings sync is for server-authoritative configuration. Clients can register an apply callback, but the server owns the value.

## Server

```csharp
BreakoutSettingsSync.RegisterServerSettings(
    "mymod.settings",
    () => new MySettings
    {
        Radius = 32f,
        Enabled = true
    });
```

The server broadcasts all registered settings when a world/session is available and then periodically.

## Client

```csharp
BreakoutSettingsSync.Client.Register<MySettings>(
    "mymod.settings",
    settings => ApplySettings(settings));
```

Client settings handlers are normal BreakoutNet client RPCs. They only apply packets from the server peer.

## DTOs

Settings DTOs use the same explicit serializer as normal RPCs:

```csharp
public sealed class MySettings : IBreakoutSerializable
{
    public float Radius;
    public bool Enabled;

    public void Write(ZPackage package)
    {
        package.Write(Radius);
        package.Write(Enabled);
    }

    public void Read(ZPackage package)
    {
        Radius = package.ReadSingle();
        Enabled = package.ReadBool();
    }
}
```

## Pattern

Keep config authority on the server:

- load server config on server startup
- broadcast the current server settings
- let clients cache the last accepted server settings
- ignore local client config for gameplay rules once connected
