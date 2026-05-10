# Changelog

## 0.2.1

- Added per-server-RPC inbound client rate limit policies.
- Added `BreakoutRpcRateLimit` helpers for high-frequency streams such as VOIP frames.
- Kept the conservative default rate limit for normal RPCs.

## 0.2.0

- Added scoped local event bus for typed and named extension events.
- Added read-only BreakoutNet core hooks for network, peer, and RPC observations.
- Added `BreakoutNet.ForMod(...)` and `BreakoutNet.ForPlugin(...).AddShared/AddServer/AddClient().Build()` developer entrypoints.
- Added disposable subscriptions and automatic subscription cleanup for plugin-owned apps.
- Updated the example plugin with shared/client/server modules and extension events.

## 0.1.0

- Added BreakoutNet BepInEx plugin and persistent Valheim RPC runner.
- Added typed `IBreakoutSerializable` RPC API for client-to-server, server-to-client, broadcast, except-broadcast, and proximity broadcast flows.
- Added side and peer helper APIs.
- Added server-authoritative settings sync.
- Added protocol envelope validation, message type validation, basic per-peer rate limiting, and rate-limited malformed packet logs.
- Added example plugin and developer documentation.
