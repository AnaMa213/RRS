# Handoff: TextMeshPro text never renders on screen in MainMenuLobby

**Date:** 2026-09-08
**Reported by:** Kenan (user), while manually testing Story 2.2 in the Unity Editor.
**Status:** Unresolved. Root cause NOT found. Do not re-guess blindly — read "Already ruled out" below first, it will save you a lot of time.

## Symptom

In `MainMenuLobby.unity`, **no TextMeshPro text ever renders on screen**, in either the **Game** tab or the **Scene** tab, during Play mode. This is confirmed by the user directly (not inferred):

> "je teste sur les 2 (Game et Scene), sur scène je vois le squelette mais c'est vide"

"Le squelette" = the panels/buttons (`Image` components) render fine — backgrounds, button shapes, borders all visible. Only the **text** is missing. This affects every `TMP_Text` in the scene, not just the ones added by Story 2.2:

- `SettingsSummaryLabel` ("Difficulte : Normal") — pre-existing since Story 1.2, still broken.
- `RoomCodeLabel` — added by Story 2.2, same symptom.
- Presumably every other TMP label in the scene (title, button labels, etc.) — not individually confirmed, but the user reports "aucun texte ne s'affiche."

The user confirms this **has never worked**, in this project, on this machine — it's not a regression introduced today.

## Environment

- Unity **6000.6.0f1** (`ProjectSettings/ProjectVersion.txt`)
- Render pipeline: **URP**, `com.unity.render-pipelines.universal: 17.6.0`, active SRP asset `PC_RPAsset`
- `com.unity.ugui: 2.6.0`
- TMP font in use: `LiberationSans SDF` (Unity's bundled default TMP font/material, `TextMeshPro/Mobile/Distance Field` shader)
- Repo root: `d:/Projets/RRS`
- Unity MCP (CoplayDev-style `unity-mcp` server) is connected to the live Editor instance and was used for all the checks below — same live process the user was interacting with.

## How to reproduce

1. Open `Assets/RoadRage/App/Scenes/Bootstrap.unity` (or `MainMenuLobby.unity` directly).
2. Enter Play mode.
3. Click **Play** on the main menu -> lands on the lobby setup panel.
4. Look at "Difficulte : Normal" (`Canvas/SetupPanel/SettingsSummaryLabel`) in the Game tab. It should be readable text. It is not visible.
5. Switch to the Scene tab while still in Play mode. The panel/button shapes are visible; no text glyphs anywhere.

## Already ruled out (do not re-check these — verified with hard data, not guesses)

1. **Panel `localScale`.** A Story 1.3 deferred-work entry (`deferred-work.md` line ~135) documented `MenuPanel`/`SetupPanel`/`NoticePanel` inheriting a `localScale` of `2.9179332`, pushing children off-screen. Two commits already fixed this, before this session started:
   - `30ddc843dae50bba9c0d38520d79087743246fc6` — reset MenuPanel/SetupPanel localScale to 1
   - `5ee34d6da25d1af2183adbfcbeae03b99115e470` — reset NoticePanel localScale to 1

   Confirmed **currently correct** in the committed scene (`Assets/RoadRage/App/Scenes/MainMenuLobby.unity`):
   - MenuPanel RectTransform `&1895343229`: `m_LocalScale: {x: 1, y: 1, z: 1}` (line ~4835)
   - SetupPanel RectTransform `&2097947412`: `m_LocalScale: {x: 1, y: 1, z: 1}` (line ~5547)
   - NoticePanel RectTransform `&2062058726`: `m_LocalScale: {x: 1, y: 1, z: 1}` (line ~5349)

   **This is not the current cause.** The symptom persists even with these at 1.

2. **Canvas's own `localScale`.** Measured live during Play mode: `Canvas.localScale = (0.78, 0.78, 0.78)`, `Screen.width/height = 1490/838`. `1490/1920 ≈ 0.776` — this matches `CanvasScaler` (`ScaleWithScreenSize`, `matchWidthOrHeight = 0`, reference `1920x1080`) doing exactly what it's supposed to do. **Normal, not a bug.**

3. **RoomCodeLabel-specific layout bug** (Story 2.2's own contribution): it originally overlapped `SettingsSummaryLabel` by ~30px (40px offset for two 70px-tall labels). This was real and has been fixed (commit `fa9c27325e25d77c2ff7b84988f8c1e04bd7456f`, anchoredPosition now `(0, -285)`, confirmed zero overlap). **But this cannot be the root cause of the broader symptom**, since `SettingsSummaryLabel` alone (pre-existing, no overlap, was never touched by Story 2.2) is equally invisible.

4. **Font/material/shader assignment**, checked live on `SettingsSummaryLabel`'s `TMP_Text` component:
   - `font` = `LiberationSans SDF` (not null)
   - `font.atlasTexture` = `LiberationSans SDF Atlas` (not null)
   - `fontSharedMaterial` = `LiberationSans SDF Material` (not null)
   - `fontSharedMaterial.shader` = `TextMeshPro/Mobile/Distance Field`, `shader.isSupported = true`
   - `TMP_Settings.defaultFontAsset` = `LiberationSans SDF` (not null)
   - 7 `LiberationSans SDF`-related assets found via `AssetDatabase.FindAssets`, including the essentials under `Assets/TextMesh Pro/Resources/Fonts & Materials/` — TMP Essential Resources **are** imported.

5. **Mesh/geometry generation.** After a real Create Lobby click resolved to `Open` (real Steam lobby, real join code), forced `TMP_Text.ForceMeshUpdate()` and read the `CanvasRenderer`'s mesh directly:
   - `RoomCodeLabel`: `characterCount=25`, `meshVertexCount=128`, `meshBounds` = `Center: (0.00, 0.26, 0.00), Extents: (211.44, 12.19, 0.00)` — a real, correctly-sized mesh.
   - `SettingsSummaryLabel`: `characterCount=19`, `meshVertexCount=72`, correct bounds too.

   **The text mesh geometry is being generated correctly, with correct bounds, on the CPU side.** Whatever is wrong happens at actual draw/rasterization time, not in TMP's layout engine.

6. **GameObject/component sanity**: `activeSelf`/`activeInHierarchy` = true, `layer` = 0 (Default, same as everything else in the panel), `TMP_Text.enabled` = true, `color` = opaque white `(1,1,1,1)`, `CanvasRenderer.cull` = false, `CanvasRenderer.GetAlpha()` = 1, no `Mask`/`RectMask2D` on `SetupPanel` clipping children, `RoomCodeLabel`'s world corners are well within `SetupPanel`'s world corners (no off-screen clipping).

7. **Console is clean.** No errors, no warnings — not from TMP, not from shaders, not from URP — at any point, including right after clicking Create Lobby. Only the new `Debug.Log("[Lobby] Create Lobby demande...")` line added by Story 2.2 appears.

## Not yet tried / actual next steps for whoever picks this up

Everything above rules out the "obvious" causes (layout, assets, mesh generation). The remaining suspects are lower-level rendering pipeline issues that couldn't be checked through C# reflection/`RunCommand` scripting alone — need actual visual/GPU-level inspection:

1. **Frame Debugger** (`Window > Analysis > Frame Debugger`) during Play mode: step through the draw calls and check whether a draw call for the TMP text mesh is even issued, and if so, what it actually renders (this is the single most likely next step to actually pinpoint the failure).
2. **URP Renderer Data asset** (find it via the active `PC_RPAsset` SRP asset's Renderer List): check for anything unusual in Renderer Features, opaque/transparent layer masks, or a post-process/full-screen effect that could be swallowing alpha-blended geometry. Screen Space Overlay canvases bypass camera culling for rendering, but URP could still interact with them at composite time in ways worth checking.
3. **Try a different TMP shader variant** on one label as an isolated test — e.g. swap `SettingsSummaryLabel`'s material to `LiberationSans SDF - Overlay` (`Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/LiberationSans SDF - Overlay.mat`, found during the asset search above) and see if that one renders. If it does, it's a `TextMeshPro/Mobile/Distance Field` shader-specific problem (e.g. an SRP Batcher / Distance Field shader keyword issue under this specific URP/Unity 6.6 combo). If it doesn't either, the problem is upstream of the shader (e.g. render queue, camera, compositing).
4. **Graphics API**: check `Edit > Project Settings > Player > Other Settings > Graphics APIs`. If running with DirectX12 or Vulkan on Windows, try forcing DirectX11 (a very common source of obscure, silent UI/shader rendering bugs, especially on newer/beta Unity versions) and see if text appears.
5. Check whether **non-UI text renders correctly** (e.g. a `TextMeshPro` — not `TextMeshProUGUI` — component in world space, or a `UnityEngine.UI.Text` legacy component) to narrow down whether this is TMP-specific, UI-Canvas-specific, or a fully general text/font rendering problem.
6. Ask the user directly what GPU/drivers they're running, and whether `Editor.log` (`Help > ... > Open Editor Log` or `%LOCALAPPDATA%\Unity\Editor\Editor.log`) has anything relevant around shader compilation at startup (shader compile errors sometimes only show there, not in the in-Editor Console).

## Scope note

This is **not** a Story 2.2 bug and blocks manual QA of any UI in this project, not just this story's. Story 2.2 itself is implemented, tested (107 EditMode + 37 PlayMode tests passing, including live verification against a real Steam session via scripted `RunCommand` interaction, which doesn't depend on the text actually being visible on screen), and committed (`5f3ed7f`, `fa9c273`). This rendering issue should probably become its own tracked story/bug once diagnosed, separate from Epic 2 feature work.
