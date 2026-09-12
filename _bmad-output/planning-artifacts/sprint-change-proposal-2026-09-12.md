---
title: 'Sprint Change Proposal: Epic 4 Test-Tooling Expansion and Epic 6 Melee/NPC-Test Greybox Stories'
created: '2026-09-12'
status: 'accepted'
scope_classification: 'Moderate'
triggered_by: 'Kenan reviewing Epic 4 scope ahead of Story 4.3 implementation'
---

# Sprint Change Proposal - Epic 4 Test-Tooling Expansion and Epic 6 Melee/NPC-Test Greybox Stories

## 1. Issue Summary

While Epic 4 is `in-progress` (4.1 and 4.2 are `review`, 4.3 is still `backlog` with no spec written yet), Kenan identified two gaps before letting Story 4.3 proceed as originally scoped:

1. **Story 4.3's acceptance criteria under-specify the test surface needed to actually validate rage on `MVP_Run`.** The current AC only says "the HUD or dev UI shows the rage change clearly" and assumes "a rage target exists," without saying where the HUD lives, how many rage targets are available for multi-target testing, whether new ones can be spawned on demand, or how a passenger picks which target to watch. Building 4.3 as currently written would leave no repeatable way to test rage against more than one target, or to add vehicles to the scene without hand-placing them in the editor between test runs.
2. **Two new capabilities are wanted that don't exist in any epic today**: an inventory bar with a default fist melee action (with concrete damage numbers), and a dev-only way to eject an NPC "driver" from a rage-target vehicle and spawn a damageable test mannequin, with a blood VFX cue on any HP loss.

Investigation confirmed:
- `NetworkedPlayerState` already owns player HP (100 HP, 3 hearts) and `NetworkedVehicleState` already owns vehicle HP (100 HP) with an existing damage/VFX pattern (`NetworkedVehicleDamageVfxController`), both landed in Story 3.5 (`done`). Fist melee damage should apply through these existing HP owners, not a parallel health system.
- `NFR21` ("exact health values, revive windows, damage sources... remain open until the first combat/Rage Road greybox exists") was written anticipating Epic 6 as the epic that finally pins down melee/combat damage numbers. No FR currently covers an inventory system or a melee action at all - this is new scope, not a re-interpretation of an existing FR.
- Epic 6's existing Story 6.1 ("On-Foot Transition for Confrontation and Sandbox Stops") already plans "a targeted AI vehicle whose rage reaches the confrontation-triggering state can dismount its occupant as a host-owned on-foot AI NPC combatant." The requested dev-only PNJ-eject-and-mannequin story is a smaller, testable precursor to that exact mechanic, not an unrelated feature - it belongs in Epic 6, staged before that story, so the real AI-driven dismount can reuse it instead of it being duplicated or half-built inside Epic 4 (whose stated goal is passenger chaos actions, not on-foot combat). **Kenan confirmed this placement** when asked directly.
- The architecture spine's feature-folder naming list (`Lobby, Run, Players, Vehicles, Rage, PassengerActions, OnFoot, SandboxStops, Economy, Boss, UI`) has no entry for inventory or melee/combat - a small, additive naming-table update is needed, not a redesign.

## 2. Impact Analysis

### Epic Impact

- **Epic 4** (`in-progress`): only **Story 4.3**'s acceptance criteria are expanded. No new stories, no resequencing, no change to Story 4.3's core intent (a passenger action that raises rage). Stories 4.1, 4.2 (`review`), 4.4, 4.5, 4.6 (`backlog`) are unaffected.
- **Epic 6** (`backlog`, not started, no compiled context file yet): gains **two new stories inserted before the existing Story 6.1**, which pushes every existing Epic 6 story number up by two (old 6.1 -> 6.3, ..., old 6.8 -> 6.10). The checkpoint story stays last (now 6.10), preserving the project's convention that only an epic's own checkpoint story bulk-promotes sprint status to `done`. Epic 6's intro paragraph and requirements-covered list gain one new FR and one new UX-DR (see below).
- No epic becomes obsolete. No other epic (0, 1, 2, 3, 5, 7) requires changes - nothing here touches lobby, driving, or AI-traffic FRs.

### Story Impact

Full before/after text is in Section 4. Summary:
- Story 4.3 (`backlog`, no spec yet): AC gains four new bullets (HUD placement, extra drivable rage-target vehicles, on-demand safe vehicle spawning, camera focus/switch between rage targets). Story intent, FR/NFR mapping, and status are unchanged.
- Epic 6 renumbering (`backlog`, no specs, no code, nothing to roll back):
  - New Story 6.1: Inventory Bar and Fist Melee Action.
  - New Story 6.2: Dev NPC Dismount and Test Mannequin.
  - Old 6.1 -> 6.3, old 6.2 -> 6.4, old 6.3 -> 6.5, old 6.4 -> 6.6, old 6.5 -> 6.7, old 6.6 -> 6.8, old 6.7 -> 6.9, old 6.8 -> 6.10.
  - No spec files exist yet for any Epic 6 story, so renumbering is a pure rename with zero rollback risk.

### Artifact Conflicts

- **`epics.md`** (planning artifact): Story 4.3 AC block; Epic 6 intro paragraph, "Requirements covered" line, and full story block (renumbered + 2 new stories inserted); Requirements Inventory gains `FR29` and `UX-DR8`; `FR Coverage Map` gains an `FR29` line; the "Epic List" overview's Epic 6 `FRs covered` line gains `FR29`; the "Module slices to preserve" line gains `Inventory` and `Combat`.
- **`ARCHITECTURE-SPINE.md`** (status `final`, reopened by explicit human renegotiation, same as the 2026-09-02 precedent change): the naming table's feature-folder list gains `Inventory` and `Combat`. No other architecture rule (host authority, intent pattern, networked-state ownership, camera/input locality) needs to change - the new stories are explicitly designed to reuse `NetworkedPlayerState`/`NetworkedVehicleState` HP and the existing intent-validation pattern, not to introduce new authority rules.
- **`sprint-status.yaml`**: `epic-4` unaffected (4-3 stays `backlog`, no ID change). `epic-6` block gets two new `backlog` entries and six renumbered entries (old IDs replaced with new IDs; no status values change since everything in Epic 6 is still `backlog`).
- **`epic-4-context.md`** (compiled, implementation artifact): stale after the Story 4.3 AC change; regenerated via `compile-epic-context` as part of this change (Stories list and Goal are unaffected; UX & Interaction Patterns section gains the HUD-placement/camera-switch notes).
- **Epic 6 compiled context**: none exists yet (`epic-6-context.md` is not created until Epic 6 starts), so nothing to regenerate there.
- **UI/UX**: no dedicated UX spec document exists (UX requirements live in `epics.md`'s UX-DR list, already covered above).
- **CI/CD, deployment, monitoring, docs/setup**: no impact.

### Technical Impact

- No code exists yet for Story 4.3, or for any Epic 6 story - this is a pre-implementation scope correction, not a rework of shipped code.
- Fist-melee damage against vehicles/players reuses the existing `NetworkedVehicleState` and `NetworkedPlayerState` HP fields and the existing host-validated-intent pattern from Epic 3/4 - no new authority model needed.
- The dev NPC-dismount/mannequin story needs one new lightweight host-owned HP holder for a non-player NPC (the mannequin), plus reuse of the existing vehicle damage VFX pattern (`NetworkedVehicleDamageVfxController`) as the template for the new blood-VFX trigger, applied to both players and NPCs.
- The "spawn a vehicle without it being stuck in geometry" requirement needs a validated-spawn-point check (e.g., an overlap/clearance test before placing the vehicle) - this is new but small, self-contained logic inside the Vehicles feature slice, not an architecture change.

## 3. Recommended Approach

**Option 1 - Direct Adjustment**, selected over the alternatives:

- **Option 2 (Rollback)**: not applicable - nothing has been implemented against either the old Story 4.3 AC or the old Epic 6 story numbering.
- **Option 3 (MVP Review)**: not needed - both changes stay inside the already-approved MVP scope (Epic 4's rage testing, Epic 6's on-foot combat/dismount); no goal or platform change.
- **Option 1 (Direct Adjustment)**: edit `epics.md`, `sprint-status.yaml`, and the architecture spine's naming table in place; regenerate `epic-4-context.md`. Effort: **Low**. Risk: **Low** (pre-implementation, no code or specs to reconcile).

## 4. Detailed Change Proposals

### 4.1 `epics.md` - Story 4.3 (Passenger Action One Changes Rage)

**Section:** Acceptance Criteria (story intent, "As a passenger..." text, and `Implements:` line unchanged)

OLD (last two AC bullets stay; nothing is removed):
```
**Given** a passenger is seated and a rage target exists
**When** the passenger triggers the first MVP action
**Then** the host validates the action and increases or changes the target's rage state
**And** the HUD or dev UI shows the rage change clearly
**And** the action has placeholder animation, sound, or visual feedback suitable for greybox testing
**And** the final action name and tone remain replaceable until content-rating boundaries are finalized
```

NEW (four bullets appended):
```
**Given** a passenger is seated and a rage target exists
**When** the passenger triggers the first MVP action
**Then** the host validates the action and increases or changes the target's rage state
**And** the HUD or dev UI shows the rage change clearly
**And** the action has placeholder animation, sound, or visual feedback suitable for greybox testing
**And** the final action name and tone remain replaceable until content-rating boundaries are finalized
**And** on `MVP_Run`, a Rage HUD element is anchored to the top-right of the screen and shows the currently focused rage target's state
**And** `MVP_Run` contains at least two additional drivable vehicles configured as rage targets, so rage can be tested against more than one target at once
**And** a dev-facing control lets a player spawn a normal vehicle or a rage-target vehicle into `MVP_Run` at any time, and every spawned vehicle is checked for clearance and placed so it is immediately drivable, never embedded in terrain or geometry
**And** the passenger camera can focus on a chosen rage target and cycle between all currently available rage targets, so the passenger can see which target's rage is changing
```

**Rationale:** Kenan's ask. Turns the vague "a rage target exists" / "HUD shows the change" AC into a concretely testable multi-target setup, without changing what the first passenger action does. No FR/NFR/UX-DR change needed - `UX-DR4` (gameplay UI shows rage) and `UX-DR6` (camera stays local-only presentation) already cover this.

### 4.2 `epics.md` - Requirements Inventory additions

**New Functional Requirement** (appended after `FR28`):
```
FR29: The player has an inventory bar (starting at 3 slots, expandable later) with a default melee weapon (fists) in the first slot, usable to strike whatever is directly in front of the player, dealing type-specific damage (vehicle vs. character).
```

**New UX Design Requirement** (appended after `UX-DR7`):
```
UX-DR8: Gameplay UI must show the player's inventory bar and currently equipped item.
```

**`FR Coverage Map`** (appended after the `FR28` line):
```
FR29: Epic 6 - inventory bar and default fist melee action.
```

**Rationale:** No existing FR covers an inventory or melee action - this is genuinely new player-facing capability, not a reinterpretation of `FR9`/`FR17` (which are about passenger chaos actions from inside the car, not on-foot melee). Keeping the project's FR-traceability convention intact (every story maps to at least one FR) requires adding one FR and one UX-DR rather than forcing the new stories onto unrelated existing requirements.

### 4.3 `epics.md` - Epic 6 intro, requirements-covered lines, and "Module slices to preserve"

**Epic List overview entry** (the short-form list before the detailed sections):
- OLD: `**FRs covered:** FR12, FR13, FR14, FR15, FR16, FR17, FR18, FR19, FR20, FR24, FR25, FR26, FR27`
- NEW: `**FRs covered:** FR12, FR13, FR14, FR15, FR16, FR17, FR18, FR19, FR20, FR24, FR25, FR26, FR27, FR29`

**Detailed Epic 6 section header:**
- OLD intro: "Players can leave the car for a compact confrontation or sandbox stop, resolve one Rage Road event, earn a shared money reward, buy one upgrade, and return that value to the next driving loop. A high-rage AI vehicle can dismount its occupant as an on-foot NPC combatant, using the same on-foot module as the player confrontation."
- NEW intro (one sentence appended): "...using the same on-foot module as the player confrontation. A dev-only harness lets the fist-melee action and a test mannequin validate combat damage before the real AI-driven dismount exists."
- OLD: `**Requirements covered:** FR12, FR13, FR14, FR15, FR16, FR17, FR18, FR19, FR20, FR24, FR25, FR26, FR27, NFR1, NFR2, NFR4, NFR5, NFR6, NFR13, NFR14, NFR15, NFR20, NFR21, UX-DR4`
- NEW: `**Requirements covered:** FR12, FR13, FR14, FR15, FR16, FR17, FR18, FR19, FR20, FR24, FR25, FR26, FR27, FR29, NFR1, NFR2, NFR4, NFR5, NFR6, NFR13, NFR14, NFR15, NFR20, NFR21, UX-DR4, UX-DR8`

**"Module slices to preserve in epic/story design"** (Architecture & Technical Constraints list):
- OLD: `Vehicle, OnFoot, PassengerActions, Rage, Economy, Lobby/Network, Run, Boss, SandboxStops, and UI.`
- NEW: `Vehicle, OnFoot, PassengerActions, Rage, Inventory, Combat, Economy, Lobby/Network, Run, Boss, SandboxStops, and UI.`

**Rationale:** Keeps the FR-coverage tables and epic summaries consistent with the new story content; names the two new feature areas the same way the architecture spine's naming table will.

### 4.4 `epics.md` - Epic 6 story block (renumber + 2 new stories)

Insert two new stories immediately before the current Story 6.1, then renumber every existing Epic 6 story by +2 (checkpoint stays last):

**New Story 6.1: Inventory Bar and Fist Melee Action**

```
**Implements:** FR29, FR24, FR25, FR26, FR27, NFR4, NFR5, NFR13, NFR14, NFR15, NFR21, UX-DR8

As a player,
I want a small inventory bar with my fists as a default melee option,
So that I can strike something in front of me even before other items exist.

**Acceptance Criteria:**

**Given** a connected player exists
**When** the player's inventory is initialized
**Then** the player has an inventory bar with 3 slots (expandable later) and fists occupy slot 1 by default
**And** the player can trigger a melee strike against whatever is directly in front of them within a short reach
**And** a strike against a vehicle's HP applies -0.5 HP or 0 HP at random
**And** a strike against another player's or an NPC's HP applies -3 HP, or -5 HP on a critical hit with a 2% critical chance
**And** melee damage is applied host-side through a validated intent, consistent with how other player-initiated effects mutate shared state
**And** strike stats are authored as a ScriptableObject definition with a stable id, not hardcoded per caller
**And** the inventory bar and currently equipped item are visible in the gameplay UI
```

**New Story 6.2: Dev NPC Dismount and Test Mannequin**

```
**Implements:** FR24, NFR2, NFR21

As a solo developer,
I want a dev-only control that ejects the "PNJ" driver from the currently focused rage-target vehicle and spawns a test mannequin,
So that I can validate melee damage against a real target before the real AI-driven dismount behavior exists.

**Acceptance Criteria:**

**Given** a rage target vehicle is currently focused (per Story 4.3's camera target switching)
**When** a developer triggers the dev dismount control on `MVP_Run`
**Then** a test mannequin NPC spawns beside that rage target vehicle, positioned so it is immediately reachable and not embedded in geometry
**And** the mannequin has 100 HP tracked in host-owned state
**And** the mannequin can be damaged by the player's fist melee action from Story 6.1
**And** a small blood VFX plays whenever a player or the mannequin loses HP, using one shared effect for both cases
**And** this control is explicitly dev-only tooling, kept separate from the real AI-vehicle-dismount behavior planned for Story 6.3
```

**Then renumber, title text unchanged:**

| Old | New |
|---|---|
| Story 6.1: On-Foot Transition for Confrontation and Sandbox Stops | Story 6.3 |
| Story 6.2: Compact Rage Road Confrontation Resolution | Story 6.4 |
| Story 6.3: Shared Money Reward for Road-Rage Victory | Story 6.5 |
| Story 6.4: Compact Sandbox Stop with Happenings | Story 6.6 |
| Story 6.5: One Upgrade Purchase | Story 6.7 |
| Story 6.6: Upgrade Effect on the Next Driving Loop | Story 6.8 |
| Story 6.7: Economy, Confrontation, and Stop UI Feedback | Story 6.9 |
| Story 6.8: Epic 6 Economy Loop Playable Checkpoint | Story 6.10 |

Story 6.10's checkpoint AC also gains one bullet noting the inventory/melee/mannequin greybox as part of what's verified, since the checkpoint is meant to summarize everything the epic shipped:
```
**And** the fist melee action and dev NPC-dismount/mannequin harness from Stories 6.1-6.2 are confirmed working against the real on-foot AI dismount from Story 6.3
```

**Rationale:** Matches Kenan's chosen placement (new stories belong to Epic 6, ahead of the on-foot transition story they feed into). No spec files exist for any Epic 6 story yet, so this is a pure rename with no rollback risk.

### 4.5 `ARCHITECTURE-SPINE.md` - naming table

**Section:** the "Naming" row in the conventions table (line ~253).

- OLD: `Feature folders use PascalCase nouns: Lobby, Run, Players, Vehicles, Rage, PassengerActions, OnFoot, SandboxStops, Economy, Boss, UI.`
- NEW: `Feature folders use PascalCase nouns: Lobby, Run, Players, Vehicles, Rage, PassengerActions, OnFoot, SandboxStops, Economy, Boss, Inventory, Combat, UI.`

**Rationale:** Same reasoning as the 2026-09-02 precedent (Steamworks) change: a `final`-status architecture document can be reopened via explicit human renegotiation for a small, additive, non-breaking update. No other architecture rule changes - host authority, the intent pattern, and networked-state ownership rules already cover the new stories' needs.

### 4.6 `sprint-status.yaml` - epic-6 block

OLD:
```yaml
  epic-6: backlog
  6-1-on-foot-transition-for-confrontation-and-sandbox-stops: backlog
  6-2-compact-rage-road-confrontation-resolution: backlog
  6-3-shared-money-reward-for-road-rage-victory: backlog
  6-4-compact-sandbox-stop-with-happenings: backlog
  6-5-one-upgrade-purchase: backlog
  6-6-upgrade-effect-on-the-next-driving-loop: backlog
  6-7-economy-confrontation-and-stop-ui-feedback: backlog
  6-8-epic-6-economy-loop-playable-checkpoint: backlog
  epic-6-retrospective: optional
```

NEW:
```yaml
  epic-6: backlog
  6-1-inventory-bar-and-fist-melee-action: backlog
  6-2-dev-npc-dismount-and-test-mannequin: backlog
  6-3-on-foot-transition-for-confrontation-and-sandbox-stops: backlog
  6-4-compact-rage-road-confrontation-resolution: backlog
  6-5-shared-money-reward-for-road-rage-victory: backlog
  6-6-compact-sandbox-stop-with-happenings: backlog
  6-7-one-upgrade-purchase: backlog
  6-8-upgrade-effect-on-the-next-driving-loop: backlog
  6-9-economy-confrontation-and-stop-ui-feedback: backlog
  6-10-epic-6-economy-loop-playable-checkpoint: backlog
  epic-6-retrospective: optional
```

`epic-4` block is untouched (4-3 stays `backlog`, ID unchanged - only its AC content changes, tracked in `epics.md`, not in the status file).

### 4.7 `epic-4-context.md` - regeneration

Regenerate via `compile-epic-context` after the Story 4.3 edit lands, so the `UX & Interaction Patterns` section reflects the top-right HUD placement and rage-target camera switching. `Goal`, `Stories`, and `Cross-Story Dependencies` sections are unaffected. No `epic-6-context.md` exists yet, so nothing to regenerate there.

## 5. Implementation Handoff

**Scope classification: Moderate** - touches a `final`-status architecture document and adds two new backlog stories with a renumbering ripple across `epics.md` and `sprint-status.yaml`, but stays entirely pre-implementation (no code, no specs, no shipped work to reconcile) and inside the already-approved MVP scope.

**Roles (solo-developer project - Kenan holds every role):**
- `epics.md`, `ARCHITECTURE-SPINE.md`, `sprint-status.yaml`, `epic-4-context.md`: implemented directly in this session immediately following approval (Developer-agent-equivalent direct implementation, since Kenan is also the acting PO/Architect approving each edit).
- **No manual user action required** - unlike the Steamworks change, nothing here touches installed packages, external accounts, or paid services.

**Success criteria:**
- All edits in Section 4 applied to their target files.
- `sprint-status.yaml` epic-6 IDs match the renumbered story titles exactly (no orphaned old IDs).
- Story 4.3 keeps its original core AC (host validates the action, rage changes, greybox feedback) - the new bullets are additive, nothing is deleted.
- Epic 4's status stays `in-progress` with 4-3 still `backlog`; nothing here marks any story `done` prematurely.

---
🤖 Generated with [Claude Code](https://claude.com/claude-code)
