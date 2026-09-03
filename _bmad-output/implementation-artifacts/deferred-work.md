# Deferred Work

- source_spec: `_bmad-output/implementation-artifacts/spec-0-1-setup-readiness-checklist-and-local-workspace-baseline.md`
  summary: No local workspace baseline guidance exists yet for version control (git init, Unity-appropriate .gitignore, Git LFS for binary assets).
  evidence: Repo currently has no VCS at all; a story titled "local workspace baseline" never mentions git, and none of Epic 0's stories 0.1-0.8 cover it either. Real gap, but adding it changes frozen scope, so it needs a human decision on where it belongs rather than a silent patch.

- source_spec: `_bmad-output/implementation-artifacts/spec-0-1-setup-readiness-checklist-and-local-workspace-baseline.md`
  summary: No cross-references tie checklist story rows to their supporting `VAL-###` entries in the tooling log or `ADDON-###` entries in the adoption register.
  evidence: Makes it harder to verify a checklist row's completeness by cross-checking the detailed log/register entries that back it; a nice-to-have traceability improvement, not required by the story's acceptance criteria.

- source_spec: `_bmad-output/implementation-artifacts/spec-0-1-setup-readiness-checklist-and-local-workspace-baseline.md`
  summary: VAL-011 (Cinemachine) has an ambiguous pass criterion when the package ships pre-embedded rather than needing explicit install.
  evidence: "Unity resout `com.unity.cinemachine` `6.6.0` si non deja embarque ou active" does not state what status to record if it is already embedded, leaving the validator without a concrete rule for that branch.

- source_spec: `_bmad-output/implementation-artifacts/spec-0-1-setup-readiness-checklist-and-local-workspace-baseline.md`
  summary: `docs/setup/addon-adoption-register.md` has no mechanism to revisit an `Adopt` decision if the underlying package/version changes later.
  evidence: A pinned Unity package version bump could invalidate a prior compatibility assessment; nothing in the register or gate rules triggers re-evaluation, so decisions can go stale silently.

- source_spec: `_bmad-output/implementation-artifacts/spec-0-1-setup-readiness-checklist-and-local-workspace-baseline.md`
  summary: `tooling-validation-log.md` VAL-017/VAL-018 reference the `Assets/RoadRage` feature folder structure only in prose elsewhere, without enumerating the concrete `RoadRage.Features.<Feature>` names to check against.
  evidence: The validator has no concrete checklist of expected feature names (Vehicle, OnFoot, PassengerActions, Rage, Economy, Lobby/Network, Run, Boss, SandboxStops, UI) to verify Story 0.4 output against.

- source_spec: `_bmad-output/implementation-artifacts/spec-0-1-setup-readiness-checklist-and-local-workspace-baseline.md`
  summary: The setup docs prohibit committing secrets/tokens but never state where they should actually be stored locally (env file, OS keychain, dashboard-only, etc.).
  evidence: A solo developer following the checklist has no guidance on the correct place to keep Unity Cloud/Relay credentials, only what not to do with them.

- source_spec: `_bmad-output/implementation-artifacts/spec-0-1-setup-readiness-checklist-and-local-workspace-baseline.md`
  summary: No index/README in `docs/setup/` explains the relationship between the three files (checklist, tooling log, adoption register) or their reading order.
  evidence: A new reader has to already know the structure; discoverability relies on tribal knowledge rather than a pointer file.

- source_spec: `_bmad-output/implementation-artifacts/spec-0-2-unity-editor-project-creation-and-package-pinning.md`
  summary: `docs/setup/story-0-2-unity-install-tutorial.md` never tells the user to add a Unity-appropriate `.gitignore` (`Library/`, `Temp/`, `obj/`, `Logs/`, etc.) once the project exists, before any commit happens.
  evidence: Same underlying gap as the earlier deferred VCS-baseline item, now concrete once a real Unity project folder exists to gitignore; still out of this story's scope since no Epic 0 story (0.1-0.8) owns VCS setup.

- source_spec: `_bmad-output/implementation-artifacts/spec-0-2-unity-editor-project-creation-and-package-pinning.md`
  summary: No defined convention for where to store screenshot evidence; the tutorial and tooling log ask for "une capture" but a Markdown table cell cannot hold an image.
  evidence: `docs/setup/tooling-validation-log.md`'s "Chemin/resume preuve" column has no naming/path convention (e.g. `docs/setup/evidence/VAL-004.png`) for binary proof, only text.

- source_spec: `_bmad-output/implementation-artifacts/spec-0-2-unity-editor-project-creation-and-package-pinning.md`
  summary: No fallback guidance for restricted-network/corporate-proxy scenarios blocking Unity Hub sign-in or package downloads.
  evidence: The tutorial assumes unrestricted home internet access, consistent with the project's solo-beginner target, but offers no troubleshooting note if that assumption fails.
