---
title: 'Sprint Change Proposal: Steamworks Networking Replaces Unity Multiplayer Services/Relay'
created: '2026-09-02'
status: 'approved'
scope_classification: 'Moderate'
triggered_by: 'Story 0.3 (Epic 0) - Unity Cloud, Services, Lobby, and Relay Readiness'
---

# Sprint Change Proposal - Steamworks Networking Replaces Unity Multiplayer Services/Relay

## 1. Issue Summary

While reviewing Story 0.3 (Unity Cloud, Services, Lobby, and Relay Readiness), the developer (Kenan) identified a business-constraint gap that the architecture had not accounted for: RoadRage_Simulator has no recurring revenue model. The architecture spine had already adopted (`AD-4`, status `ADOPTED`) **Unity Multiplayer Services Sessions + Relay** as the sole networking path for the game, both in development and in the eventual shipped product, with no documented cost ceiling, free-tier confirmation, usage-monitoring plan, or fallback for a scenario where concurrent-player load scales without matching revenue.

Investigation confirmed:

- `AD-4` (`ADOPTED`) commits to Unity Multiplayer Services Sessions + Relay for both dev and production use.
- `AD-15` ("Transient Session, No Game Backend") only scopes out persistent backend concerns; it does not address Relay's own usage-based billing.
- `NFR9` only requires a non-production Unity services environment for the Epic 0 setup phase - no production cost policy exists.
- Story 0.3's tutorial has a reactive-only guardrail ("ask before any paid service") rather than a designed cost strategy.
- `addon-adoption-register.md` documents cost/license per candidate but does not enforce a free/libre/open-source default - a paid candidate can still be `Adopt`ed if reviewed.

Research into comparable shipped indie co-op titles in the same genre/scale (Lethal Company, How to Fish, Meccha Chameleon) confirmed a viable, well-precedented, zero-marginal-cost alternative already used by peers: **Steamworks Networking Sockets (Steam Datagram Relay)**, with one connected player acting as host (matching the project's existing host-authoritative model, `AD-3`) and Steam absorbing relay/NAT-traversal cost for free, regardless of concurrent player count - the only cost is a one-time ~$100 Steamworks fee at publishing time, not a recurring or usage-scaled cost. A free official Valve test AppID (480, "Spacewar") additionally allows development and testing of the Steamworks integration before that one-time fee is ever paid.

The developer confirmed the decision to adopt this path: Steam-exclusive distribution, one connected player hosts the session, joining happens via native Steam friend invite or a shared Steam Lobby ID, no self-hosted infrastructure.

## 2. Impact Analysis

### Epic Impact

- **Epic 0** (in-progress) is not invalidated, but two of its stories require revision:
  - **Story 0.2** (`done`) - package pin revision: `com.unity.services.multiplayer` is removed from the locked stack and replaced with a community Steamworks transport for Netcode for GameObjects. Revised under the story's own `frozen-after-approval` renegotiation clause - no code depended on the removed package yet.
  - **Story 0.3** (`in-review`, zero real evidence gathered - VAL-012 to VAL-015 all `Not Started`) - clean replacement of its entire premise, from a Unity Cloud/Unity Gaming Services tutorial to a Steamworks/AppID-480/Lobby tutorial. No rollback needed since nothing was implemented against the old premise yet.
  - **Story 0.7** (`backlog`) - gains a hard default policy (free/libre/open-source preferred; paid requires explicit approval) and a new registered candidate for the Steamworks transport itself.
- **Epic 2** (`backlog`) - Stories 2.1, 2.2, and 2.3 need their acceptance criteria's vocabulary updated (Unity Multiplayer Services Session / Relay / anonymous dev sign-in -> Steam lobby / Steamworks Networking Sockets / Steam login). The functional contract (private room, 4-player cap, no port forwarding, visible errors) is unchanged.
- **Epics 1 and 3-7** are unaffected. They consume transport-agnostic Netcode for GameObjects concepts (`NetworkedPlayerState`, `NetworkedRunState`, etc.); the transport swap is invisible to gameplay code.
- No epic becomes obsolete; no resequencing needed.

### Story Impact

See Section 4 for the full list of before/after edits across Story 0.2, Story 0.3, Story 0.7, and Epic 2 Stories 2.1-2.3.

### Artifact Conflicts

- **Architecture spine** (`ARCHITECTURE-SPINE.md`, status `final`): `AD-1`, `AD-4` (renamed), `AD-15`, `AD-26`, and the Stack table require updates. This reopens a `final`-status document via explicit human renegotiation - acceptable and expected for a course-correction of this kind.
- **SPEC / epics.md**: `FR4` and `NFR9` wording updates; Epic 2 Stories 2.1-2.3 acceptance criteria updates.
- **Setup docs**: `docs/setup/story-0-2-unity-install-tutorial.md`, `docs/setup/story-0-3-unity-cloud-services-tutorial.md` (renamed/rewritten), `docs/setup/tooling-validation-log.md` (VAL-007 and VAL-012 to VAL-015 reset/retargeted), `docs/setup/epic-0-readiness-checklist.md` (vocabulary sync only), `docs/setup/addon-adoption-register.md` (new policy line + new candidate row).
- **UI/UX**: no impact - lobby/invite UX contract (create, join, visible errors) is unchanged in shape, only the underlying identifiers (Steam Lobby ID / Steam invite vs. Unity join code).
- **CI/CD, deployment, monitoring**: no impact - no such infrastructure exists yet for this project.

### Technical Impact

- `Packages/manifest.json` currently has `com.unity.services.multiplayer` (`2.3.1`) installed and locked (`VAL-007 = Pass`), but no code uses it (Epic 2 is `backlog`). Removing it is a clean package swap, not a code rollback.
- The replacement transport (`com.community.netcode.transport.facepunch` or `.steamnetworkingsockets`, from Unity's own `multiplayer-community-contributions` repository) is free and open-source, but its exact license file has not been verified in this session - it is registered in the adoption register as `In Progress`, not `Adopt`, until license and exact commit/tag are confirmed at actual installation time.
- Ships the project into Steam-exclusive distribution for online play; no non-Steam online path exists unless a future architecture decision adds another transport.

## 3. Recommended Approach

**Option 1 - Direct Adjustment**, selected over the alternatives evaluated:

- **Option 2 (Rollback)**: not applicable - nothing has been implemented in code against the old Unity Relay premise; Story 0.3 has zero gathered evidence to roll back.
- **Option 3 (MVP Review)**: not needed - the MVP's private, low-concurrency co-op testing scope is safely within any free tier regardless of which service is used; no scope reduction required.
- **Option 1 (Direct Adjustment)**: modify the architecture spine and four Epic 0/Epic 2 stories in place. Effort: **Low-Medium**. Risk: **Low-Medium** (new dependency on a community-maintained, not officially Unity-supported transport - mitigated by the fact that this exact combination, Netcode for GameObjects + Steamworks transport, is proven in shipped titles of the same genre and scale as this project).

This keeps the MVP timeline and scope intact, resolves the cost-scaling risk before Epic 2 writes any Relay-dependent code, and matches solo-developer feasibility (`NFR1`) better than either building a self-hosted relay (recurring ops cost and networking complexity) or leaving the risk undocumented.

## 4. Detailed Change Proposals

### 4.1 Architecture Spine (`ARCHITECTURE-SPINE.md`)

**AD-1 - Unity URP MVP Stack**
- OLD: "...Netcode for GameObjects, Unity Multiplayer Services Sessions with Relay, Unity Transport, Cinemachine..."
- NEW: "...Netcode for GameObjects, Steamworks Networking Sockets (community Netcode transport - Facepunch or SteamNetworkingSockets) as the Netcode transport, Cinemachine..."

**AD-4 - renamed "Steam Networking Sockets Are The Internet Path" [ADOPTED]**
- OLD: private host-created Unity Multiplayer Services sessions, `MaxPlayers = 4`, short join code, Relay networking.
- NEW: private host-created Steam lobbies (`ISteamMatchmaking`), `MaxPlayers = 4`, Steamworks Networking Sockets (Steam Datagram Relay) for NAT traversal - free regardless of concurrent player count. Joining via native Steam friend invite or a shared Steam Lobby ID (UI wrapper, never a native OS deep link). Public matchmaking, lobby browsing, dedicated servers, and host migration remain out of the first MVP path. New explicit constraint: **Steam is the sole distribution/runtime platform for online play** for the MVP; non-Steam builds do not support online co-op unless a later AD adds another transport.

**AD-15 - Transient Session, No Game Backend**
- "Unity Gaming Services are used for session connection only" -> "Steamworks (Lobby + Networking Sockets) is used for session connection only, at no cost regardless of concurrent players."

**AD-26 - Session Lifecycle And Run Composition**
- "Bootstrap owns persistent services, UGS initialization/auth, and NetworkManager lifetime" -> "Bootstrap owns persistent services, Steamworks SDK initialization (`SteamClient.Init`) and Steam login state, and NetworkManager lifetime."

**Stack table**
- Remove: `Unity Multiplayer Services (com.unity.services.multiplayer) | 2.3.1`.
- Add: `Steamworks transport (com.community.netcode.transport.facepunch or .steamnetworkingsockets) | commit/tag pinned at installation time (Story 0.2 revision)`.

**Rationale:** Aligns the architecture with the zero-usage-cost constraint while leaving host-authoritative rules, the 4-player cap, and the no-dedicated-server/no-matchmaking/no-host-migration constraints untouched.

### 4.2 Story 0.2 (`spec-0-2-unity-editor-project-creation-and-package-pinning.md`, status `done`)

Renegotiated under the story's own `frozen-after-approval` clause ("ne pas modifier sauf renegociation explicite"):

- Locked-versions list: remove `Multiplayer Services 2.3.1`; add the Steamworks transport package (exact commit/tag pinned when actually installed).
- New Spec Change Log entry documenting this course-correction and the explicit human approval.
- Verification commands: remove the `'2.3.1'` required-string check; add a check for the Steamworks package identifier once named.
- `docs/setup/story-0-2-unity-install-tutorial.md`: replace the "install Multiplayer Services 2.3.1" step with (a) remove Multiplayer Services via Package Manager, (b) add the Steamworks transport via "Add package from git URL".
- `docs/setup/tooling-validation-log.md`: **VAL-007** reset from `Pass` to `Not Started`, retargeted at the Steamworks package's presence.

**Rationale:** The removed package is installed but unused by any code yet (Epic 2 is `backlog`) - a clean swap, not a rollback of working functionality.

### 4.3 Story 0.3 (`spec-0-3-unity-cloud-services-lobby-and-relay-readiness.md`, status `in-review`)

Clean replacement (no real evidence gathered yet - VAL-012 to VAL-015 all `Not Started`):

- New title: "Steamworks, Lobby, and Networking Sockets Readiness."
- Drops all Unity Cloud / Unity Dashboard / Multiplayer Services Sessions / anonymous dev sign-in content.
- `docs/setup/story-0-3-unity-cloud-services-tutorial.md` (renamed) rewritten to cover: (1) development/testing via the free official Valve test AppID 480 ("Spacewar") - no paid Steamworks account needed yet; (2) installing the Steamworks transport and `SteamClient.Init`; (3) creating a private Steam lobby (`ISteamMatchmaking`, `MaxPlayers = 4`); (4) invite flow via native Steam overlay invite and Lobby ID as a fallback "code"; (5) an explicit note that a real Steamworks account and the one-time ~$100 fee are required before any public release, but are not a blocker for development.
- `docs/setup/tooling-validation-log.md`: VAL-012 to VAL-015 retargeted at AppID-480 test config, Steam lobby creation, invite/Lobby-ID flow, and the 4-player cap - instead of Unity Cloud/Dashboard linkage.
- The story's core intent is unchanged: readiness/evidence only, no lobby gameplay UI yet (still Epic 2), secrets redacted, explicit stop before Story 0.4.

**Rationale:** Nothing to roll back; this is a same-story in-place premise swap.

### 4.4 Epics document (`epics.md`)

**FR4**
- "...connect through Relay rather than direct-connect-only networking." -> "...connect through Steamworks Networking Sockets (Steam Datagram Relay) rather than direct-connect-only networking."

**NFR9**
- "Unity services used for the MVP must be configured in a non-production Unity services project/environment." -> "Steamworks services used for the MVP must be configured using the Steamworks test AppID (480/Spacewar) or another non-production Steamworks configuration until public-release readiness is confirmed."

**Story 2.1 (Online Services Bootstrap and Status Feedback)**
- "Unity services initialization and anonymous development sign-in" -> "Steamworks SDK initialization (`SteamClient.Init`) and Steam login state."

**Story 2.2 (Host-Created Private Room with Join Code)**
- "a private Multiplayer Services Session is created with Relay transport" -> "a private Steam lobby (`ISteamMatchmaking`) is created with Steamworks Networking Sockets transport."
- "displays a join code" -> "displays a Steam invite option and/or Lobby ID that can be copied or shared."

**Story 2.3 (Join By Code and Invite-Link Wrapper)**
- "enters the join code or opens an invite-link wrapper... through Relay-backed session flow" -> "enters the Lobby ID or accepts a Steam friend invite... through Steamworks Networking Sockets."

Stories 2.4-2.8 are unchanged (generic gameplay behavior, no Unity/Relay-specific wording).

**Story 0.7 (Unity Add-On, UI Library, and Asset Adoption Register)**
- New acceptance criterion: "By default, a candidate must be free, royalty-free, and open-source (or a built-in Unity/Steamworks/official package); a paid or closed-source candidate requires explicit human approval with a documented cost/license justification before `Adopt`."

### 4.5 Add-on adoption register (`docs/setup/addon-adoption-register.md`)

- New policy line under "Regle d'adoption" mirroring the Story 0.7 criterion above (in French, matching the document's language).
- New candidate row **ADDON-006**: Steamworks transport for Netcode for GameObjects. Status `In Progress`, Decision `In Progress` (not `Adopt`) - license and exact commit/tag not yet verified against a real installation, per the register's own gate rule ("never `Adopt` with an unverifiable license"). Cost: free (open-source). Notes: replaces `com.unity.services.multiplayer`; approved in principle via this Sprint Change Proposal, finalized at actual installation.

### 4.6 Readiness checklist sync (`docs/setup/epic-0-readiness-checklist.md`)

Vocabulary propagation only (no new decision): rows "0.2 Packages verrouilles," "0.3 Unity Cloud et Gaming Services," "0.3 Unity services" validation row, and the "0.8" row's "Relay distant deux joueurs" wording are updated to match the Steamworks terminology adopted above.

## 5. Implementation Handoff

**Scope classification: Moderate** - touches a `final`-status architecture document and four Epic 0/Epic 2 stories, but stays within documentation/acceptance-criteria/package-pin changes; no PM/Architect-level MVP replan is required.

**Roles (solo-developer project - Kenan holds every role):**
- **Architecture spine, epics.md, story specs, setup docs, adoption register**: implemented directly in this session immediately following this proposal's approval (equivalent to Developer-agent direct implementation, since Kenan is also the acting PO/Architect who already approved each edit incrementally).
- **Manual user actions still required afterward** (unchanged ownership model - agents never perform these): installing the Steamworks transport package via Unity Package Manager, removing the Multiplayer Services package, testing against AppID 480, and later (before any public release) creating a real Steamworks account and paying the one-time fee.

**Success criteria:**
- All edits in Section 4 applied to their target files.
- `docs/setup/tooling-validation-log.md` accurately reflects the reset/retargeted VAL rows (no fabricated `Pass`).
- `Packages/manifest.json` still reflects `com.unity.services.multiplayer` as installed until the user manually removes it and adds the Steamworks transport - the log and register must not claim this is done until real evidence exists.
- Epic 1 remains blocked on the Epic 0 gate exactly as before; nothing in this change relaxes that gate.

---
🤖 Generated with [Claude Code](https://claude.com/claude-code)
