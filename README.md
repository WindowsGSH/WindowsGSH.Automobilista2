# Automobilista 2 Dedicated Server

[![WindowsGSH](.github/assets/windowsgsh-badge.svg)](https://windowsgsh.com)
[![Status](https://img.shields.io/badge/status-needs_live_test-f59e0b)](#status)
[![Module version](https://img.shields.io/badge/dynamic/json?url=https%3A%2F%2Fraw.githubusercontent.com%2FWindowsGSH%2FWindowsGSH.Automobilista2%2Fmain%2FAutomobilista2.mod%2Fmodule.json&query=%24.version&prefix=v&label=module&color=1E8449)](Automobilista2.mod/module.json)
[![Requires WindowsGSH](https://img.shields.io/badge/dynamic/json?url=https%3A%2F%2Fraw.githubusercontent.com%2FWindowsGSH%2FWindowsGSH.Automobilista2%2Fmain%2FAutomobilista2.mod%2Fmodule.json%3Fbadge%3Dminimum&query=%24.minimumWindowsGshVersion&prefix=v&label=requires%20WindowsGSH&color=2563EB)](Automobilista2.mod/module.json)
[![Licence](https://img.shields.io/badge/licence-MIT-64748B)](LICENSE.md)

WindowsGSH module for installing, configuring, starting, stopping, updating, and backing up the Automobilista 2 Dedicated Server tool.

## Status

This module is ready for live beta validation. Its native WindowsGSH implementation:

- installs Steam app `1338040` anonymously;
- starts `DedicatedServerCmd.exe`;
- creates `server.cfg` from the vendor-provided `config_sample\server_with_lists.cfg` when needed;
- applies the configured server name, host port, and query port before every start;
- preserves the other settings and lists in the vendor configuration;
- requests a graceful window close before using the normal bounded forced-stop fallback;
- provides a graceful-only stop path during Windows sign-out or shutdown; and
- backs up `server.cfg`.

Automobilista 2's dedicated-server tool manages and advertises a multiplayer lobby. The game simulation can still use a player as its peer-to-peer host; do not describe this module as hosting the complete race simulation independently of players.

## Installation

1. Import the `Automobilista2.mod` folder into WindowsGSH, or import the repository root and allow WindowsGSH to discover it.
2. Add an Automobilista 2 server.
3. Run Install or Update. WindowsGSH downloads Steam app `1338040` using SteamCMD.
4. Open the server configuration and choose the name and ports.
5. Start the server.

The Steam installation must contain:

- `DedicatedServerCmd.exe`
- `config_sample\server_with_lists.cfg`

If the sample configuration is missing, WindowsGSH stops before launch and asks you to run Verify Files rather than generating an incomplete configuration.

### Import an existing server

WindowsGSH can import either a normal server installation folder or a WindowsGSM server folder containing `serverfiles`. The preview verifies the server executable, reads supported settings when present, and lets you copy the installation into WindowsGSH or adopt it in place. Review every previewed/defaulted value before completing the import; the source installation is not modified during preview.

## Configuration

| WindowsGSH setting | Default | Applied to |
|---|---:|---|
| `server.name` | `Automobilista 2 Dedicated Server` | `name` in `server.cfg` |
| `network.port` | `27015` | `hostPort` in `server.cfg` |
| `network.queryPort` | `27016` | `queryPort` in `server.cfg` |
| `server.additionalArguments` | empty | Appended to the server command line |

WindowsGSH writes only those three named values. Existing session, vehicle, track, security, HTTP API, rotation, and list settings in `server.cfg` are preserved.

## Networking

The module currently declares:

- UDP `27015` by default as the host/game port;
- UDP `27016` by default as the Steam query port.

Both are configurable. The exact query protocol and router/firewall requirements still require validation against a current live server. Until that test is recorded, treat the Networking page as guidance rather than proof that the lobby is externally reachable.

Declaring these ports does not automatically forward them. UPnP remains a per-server, opt-in WindowsGSH policy.

### Configuration ownership

The generated file is `<server install>\server.cfg`. On first start WindowsGSH copies the vendor sample, then changes only `name`, `hostPort`, and `queryPort`. Subsequent saves update those keys in the existing file atomically.

Use a distinct server installation directory for each instance. Sharing one `server.cfg` between multiple instances will cause their settings to overwrite one another.

## Query, console, and administration

- WindowsGSH reports process status only. It does not implement A2S or player querying for Automobilista 2; the configured vendor query port remains available to the game and external compatible clients.
- Process output is available for diagnostics; interactive console commands are not certified.
- The optional HTTP administration interface is not managed by this module. Do not expose it unless it has been configured and secured manually.
- WindowsGSH does not claim AMS2 RCON support.

## Files and backups

| Purpose | Path |
| --- | --- |
| Executable | `DedicatedServerCmd.exe` |
| Vendor configuration template | `config_sample\server_with_lists.cfg` |
| Managed configuration and backup target | `server.cfg` |

The first configuration write copies the vendor template when needed. Subsequent writes preserve unowned settings and lists.

## Known limitations

- The query-port protocol and player-count behavior have not been captured; WindowsGSH intentionally makes no A2S capability claim.
- Additional AMS2 lobby/session options must currently be edited in `server.cfg`.
- The module does not manage the HTTP API, access control lists, or result processing.
- A clean SteamCMD install and live graceful-shutdown test are still required before this module is marked beta-tested.

## Beta verification checklist

- [ ] Fresh install produces `DedicatedServerCmd.exe` and the sample config.
- [ ] First start creates `server.cfg` and preserves vendor sample entries.
- [ ] The configured name appears in the multiplayer lobby.
- [ ] Changing host/query ports changes the listening endpoints.
- [ ] WindowsGSH reports the process as running and reattaches after an app restart.
- [ ] Capture the query endpoint/protocol; add A2S/player support only if a current repeatable response is proven.
- [ ] Normal Stop closes the tool cleanly.
- [ ] Windows sign-out/shutdown uses the graceful-only path without corrupting configuration/results.
- [ ] Backup and restore preserve `server.cfg`.

## Support

Report module issues at <https://github.com/WindowsGSH/WindowsGSH.Automobilista2>. Include a redacted WindowsGSH support bundle and state whether this was a fresh installation or an imported server. Never post passwords, tokens, public administration URLs, or an unredacted `server.cfg`.

## Support development

If you like the work I do and would like to support continued WindowsGSH and module development, you can contribute here:

- [Ko-fi](https://ko-fi.com/shenniko)
- [PayPal](https://paypal.me/shenniko)

## Trust and source

C# modules run with the same Windows user permissions as WindowsGSH. Review the module source, provenance, and [SECURITY.md](SECURITY.md) before importing it. WindowsGSH cannot sandbox unrestricted compiled module code.
