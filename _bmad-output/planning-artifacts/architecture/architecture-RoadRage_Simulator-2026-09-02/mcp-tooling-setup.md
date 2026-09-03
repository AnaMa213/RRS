# MCP Tooling Setup Draft

This draft will be folded into the beginner-facing architecture guide. Goal: make AI-assisted Unity and Blender work practical for one solo developer, while keeping the project reviewable.

## Principle

Use MCPs as controlled assistants, not as autopilot. Every MCP action that changes scenes, prefabs, assets, packages, or scripts must be reviewed in Unity/Blender and committed in small steps.

## Client Targets

- Codex: include where the chosen MCP supports generic MCP clients or explicitly lists Codex. The exact Codex-side registration path must be confirmed in the active Codex environment before setup.
- Claude: include Claude Desktop and Claude Code when the MCP project documents them.
- Cursor/Windsurf/VS Code: optional, useful if they become the main code editor.

## Unity MCP

### Preferred Option: Unity Official MCP Server

Use this first if the account has access to Unity AI tools beta/trial/subscription.

Verified source: https://unity.com/blog/unity-ai-mcp-how-to-get-started

What it is for:
- Inspect Unity scene hierarchy, GameObjects, component values, build settings, and console messages.
- Let an AI agent edit scripts and trigger Editor actions with live project context.
- Help build and debug prefabs, scenes, ScriptableObjects, lobby flow, Netcode setup, and play-mode issues.

Install:
1. Install Unity Hub and Unity `6000.6.0f1`.
2. Create the Road Rage Simulator project from a Universal 3D/URP template.
3. Connect the Unity project to Unity Cloud.
4. Install Unity's in-editor AI Assistant package.
5. In Unity, open `Edit > Project Settings > AI > Unity MCP`.
6. Confirm `Unity Bridge` is running; click `Start` if it is stopped.
7. In the `Integrations` section, configure the AI client that will control Unity.
8. Test with a harmless prompt: inspect the current scene, create an empty GameObject named `MCP_SmokeTest`, then delete it manually after verification.

Notes:
- Unity's official setup documents MCP-compatible clients such as Claude Code, Cursor, Windsurf, VS Code Copilot, and Claude Desktop.
- Codex support should be treated as MCP-compatible but must be confirmed in the active Codex client before relying on it.
- If account access or client support blocks this path, use the community fallback below.

### Fallback Option: CoplayDev MCP for Unity

Use this if the official Unity MCP is unavailable, blocked by subscription, or does not configure the desired client.

Verified source: https://github.com/CoplayDev/unity-mcp

What it is for:
- Unity Editor control from MCP-compatible AI clients, including Codex and Claude.
- Manage scenes and GameObjects, edit C# scripts, manage assets, run tests, profile, and build through exposed MCP tools.

Install:
1. Install Python 3.10+ and `uv`.
2. In Unity, open `Window > Package Manager`.
3. Choose `Add package from git URL`.
4. Add the package:

```text
https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#v10.0.0
```

5. In Unity, open `Window > MCP for Unity > Configure All Detected Clients`.
6. Restart the MCP client and Unity if the client does not detect tools immediately.
7. Test with a harmless prompt: create a cube at the origin and add a Rigidbody, then inspect the result before keeping it.

Notes:
- Pin a release tag instead of `#main` so the AI toolchain does not change unexpectedly.
- Do not run multiple Unity MCP bridges against the same Unity Editor session unless the MCP explicitly supports multi-instance routing.

### Optional Later: IvanMurzak Unity MCP

Use only if the project later needs its specialized Unity packages for areas such as Cinemachine, Input System, Navigation, Particle System, or ProBuilder.

Verified source: https://openupm.com/packages/com.ivanmurzak.unity.mcp/

Install later with OpenUPM if needed:

```powershell
openupm add com.ivanmurzak.unity.mcp
```

Do not install this in the first setup unless the basic Unity MCP path fails or a specific missing tool blocks progress.

## Blender MCP

### Preferred Option: Blender Lab MCP Server

Use this first if the official Blender Lab MCP extension is available and stable in the installed Blender version.

Verified source: https://www.blender.org/lab/mcp-server/

What it is for:
- Let an AI agent inspect and manipulate Blender scenes.
- Build or adjust simple stylized props, placeholder vehicles, rough environment pieces, and material variants.
- Clean AI-generated assets before Unity import: names, origins, transforms, scale, material count, and export.

Install:
1. Install Blender 5.2 LTS.
2. Install `uv`.
3. Open https://www.blender.org/lab/mcp-server/.
4. Install the Blender MCP extension from Blender Lab, either by drag-and-drop into Blender or by downloading and installing from disk.
5. In Blender, open `Edit > Preferences > Add-ons`, search `mcp`, and enable/configure the MCP extension.
6. Keep the default host/port unless needed: `localhost:9876`.
7. Start the MCP server from the add-on settings.
8. Configure the AI client as a local stdio MCP server pointing to the Blender MCP server command documented by Blender Lab.
9. Test by asking the AI to inspect the empty scene, create one simple cube prop, assign one material, and export a `.glb` test file.

### Fallback Option: ahujasid Blender MCP

Use this if Blender Lab setup is too rough, unavailable, or incompatible with the chosen AI client.

Verified source: https://github.com/ahujasid/blender-mcp

Install:
1. Install Blender 3.0+ and Python 3.10+; Blender 5.2 LTS remains the project target.
2. Install `uv` on Windows:

```powershell
powershell -c "irm https://astral.sh/uv/install.ps1 | iex"
```

3. Ensure `uvx` is on PATH. If a GUI client cannot find it, use the full path or wrap through `cmd /c`.
4. Add the MCP server to Claude Desktop:

```json
{
  "mcpServers": {
    "blender": {
      "command": "uvx",
      "args": ["blender-mcp"]
    }
  }
}
```

5. Add the MCP server to Claude Code:

```powershell
claude mcp add blender uvx blender-mcp
```

6. Install the Blender add-on:

```powershell
uvx blender-mcp install-addon
```

7. In Blender, open `Edit > Preferences > Add-ons` and enable `Interface: MCP for Blender`.
8. In the 3D viewport, press `N`, open the MCP tab, and start the MCP server.

Notes:
- Run only one Blender MCP server instance at a time.
- Treat generated mesh work as draft until it passes the asset intake checklist.

## Netcode, Lobby, and Networking Sockets

There is no separate Netcode-specific MCP verified for this project. Netcode work should be automated through Unity MCP and checked against Unity's official packages and docs. (This section replaces the earlier Unity Multiplayer Services/Relay approach - see `_bmad-output/planning-artifacts/sprint-change-proposal-2026-09-02.md`.)

Verified sources:
- Netcode for GameObjects: https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/
- Install Netcode: https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/manual/install.html
- Netcode transport: https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/manual/advanced-topics/transports.html
- Facepunch Transport for Netcode: https://github.com/Unity-Technologies/multiplayer-community-contributions/tree/main/Transports/com.community.netcode.transport.facepunch
- Steam Networking Sockets Transport for Netcode: https://github.com/Unity-Technologies/multiplayer-community-contributions/tree/main/Transports/com.community.netcode.transport.steamnetworkingsockets
- Steam Networking overview: https://partner.steamgames.com/doc/features/multiplayer/networking
- Steam Datagram Relay: https://partner.steamgames.com/doc/features/multiplayer/steamdatagramrelay
- Steamworks API example app (AppID 480/Spacewar): https://partner.steamgames.com/doc/sdk/api/example

Install in Unity:
1. Open `Window > Package Manager`.
2. Add Netcode by name:

```text
com.unity.netcode.gameobjects
```

3. Add the Steamworks transport via **Add package from git URL**:

```text
https://github.com/Unity-Technologies/multiplayer-community-contributions.git?path=/Transports/com.community.netcode.transport.facepunch
```

(or the `steamnetworkingsockets` path instead - pin the exact commit/tag used.)

4. Make Unity Transport visible in the lockfile and confirm Netcode uses it:

```text
com.unity.transport
```

5. Intended package pins for the first setup:
   - `com.unity.netcode.gameobjects` `2.13.2`
   - Steamworks transport (commit/tag pinned at installation)
   - `com.unity.transport` `6.6.0`
6. Do not install `com.unity.services.multiplayer` (removed by course-correction) unless a future architecture decision reintroduces it.
7. Create a `steam_appid.txt` with `480` for development/testing (no paid Steamworks account needed yet) and confirm `SteamClient.Init` succeeds.
8. Build the first network slice as host-authoritative:
   - host creates a private Steam lobby (`ISteamMatchmaking`);
   - joining players connect via native Steam invite, with a Lobby ID as a UI-wrapper fallback;
   - Steamworks Networking Sockets (Steam Datagram Relay) carries the connection;
   - session cap is four players including the host;
   - Netcode synchronizes the player car, players, rage state, and event triggers.

What Unity MCP should do here:
- Create the lobby UI stub and scripts.
- Add `NetworkManager`, NetworkObjects, and NetworkBehaviours.
- Generate small test scenes for host/join smoke tests.
- Inspect scene objects for missing network components.
- Read console errors and propose minimal fixes.

What Unity MCP must not silently do:
- Add paid services or cloud products without approval.
- Change package versions without a note.
- Convert the project to dedicated servers before the host-authoritative MVP works.
- Store API keys, tokens, or secrets in prompts, scenes, scripts, or committed files.

## Asset Generation and Intake

Runway is available in this environment for AI image/video generation, but it is not a Unity or Blender control MCP and is not a verified 3D production pipeline by itself.

Use asset-generation tools for:
- mood images and visual references;
- rough character/vehicle/prop concept passes;
- texture ideas;
- short clips for inspiration.

Every 3D asset, whether made by AI or manually, must pass through Blender before Unity import:
1. Rename objects and collections clearly.
2. Apply transforms.
3. Set real scale.
4. Reduce material slots.
5. Check normals.
6. Remove invisible or excessive geometry.
7. Export as `.fbx` or `.glb`.
8. Import into Unity under a controlled asset folder.
9. Convert to prefab only after scene-scale testing.

## Setup Order

1. Unity project and packages.
2. Unity MCP, preferably official, fallback CoplayDev.
3. Netcode/Lobby/Networking Sockets smoke test.
4. Blender LTS.
5. Blender MCP.
6. Asset intake checklist.
7. Only then start building gameplay scenes and generated asset variants.
