---
title: "Cloture de revue Epic 4 — preuve multi-joueur et reconciliation"
date: 2026-09-13
status: done
author: Codex
related:
  - spec-4-1-rage-system-tuning-and-threshold-events.md
  - spec-4-2-passenger-action-catalog-slot-input-and-hud-stubs.md
  - spec-4-3-driver-vehicle-impacts-and-rage-integration.md
  - spec-4-4-passenger-action-effects-and-rage-event-bridge.md
  - spec-4-5-camera-and-feedback-playable-polish.md
  - spec-4-6-networked-playable-loop-integration.md
  - spec-fix-ui-assembly-and-camera-input-tests.md
---

# Cloture de revue Epic 4 — preuve multi-joueur et reconciliation

## Intent

<frozen-after-approval>
Terminer la revue de l'Epic 4 sans declarer vert un flux qui n'est prouve qu'en local : conserver les correctifs de regressions existants, ajouter la preuve deterministe du parcours `Joined`, rendre le sandbox de rage exploitable en Multiplayer Play Mode pour observer le trajet client distant vers l'hote des slots 0 et 1, puis fermer les findings et statuts BMAD seulement avec les rapports manuels verts.
</frozen-after-approval>

## Problemes a fermer

| Story | Evidence manquante | Definition de done |
| --- | --- | --- |
| 4.2 | Un client distant envoie slot 0, l'hote valide le sender, siege, cible et cooldown, puis le verdict et l'effet se repliquent. | Scenario MPPM reproductible et resultat observe sur les deux roles, en plus des tests existants. |
| 4.4 | Un client distant envoie slot 1 et seul l'hote publie l'incident replique. | Meme scenario, avec `ActivationCount == 1` et absence d'ecriture client directe. |
| 4.6 | Le chemin `Joined` est execute, fige le choix avant publication et le profil choisi est porte jusqu'au spawn. | Fixture EditMode avec faux lobby pour le payload/freeze; le spawn reste couvert par le chemin hote existant et un smoke manuel deux joueurs si Steam est disponible. |
| Regressions | Les corrections Camera/UI/Story11/16/44 ne possedent pas encore de rapport post-diff. | Rapports manuels cibles puis suites EditMode et PlayMode vertes. |

## Travail minimal

- Ajouter un test EditMode deterministe du chemin `Joined`, en reutilisant les faux services de lobby existants : publication du personnage selectionne, freeze avant publication et payload reseau decode par l'hote.
- Adapter seulement le harness de sandbox existant pour choisir host/client via le mecanisme Multiplayer Play Mode deja utilise par le smoke netcode. Le host cree et assied l'acteur passager de chaque client connecte; le client declenche les deux slots; les logs/etats exposes permettent la verification manuelle des valeurs repliquees.
- Ne pas tenter un faux test mono-processus pour pretendre couvrir un RPC distant, ni ajouter de dependance ou de transport de test.
- Conserver les corrections existantes des tests Story11, Story16, Story44 et de la camera; ne les ajuster qu'en cas d'echec confirme par les rapports manuels.
- Apres les preuves vertes : cocher les findings de revue, terminer `spec-fix-ui-assembly-and-camera-input-tests.md`, aligner `spec-4-1` a `spec-4-6` et `sprint-status.yaml` sur `done`, puis marquer `epic-4: done`.

## Validation manuelle requise

1. Tests cibles EditMode : `ThirdPersonCameraTests`, Stories 41, 42, 43 et le nouveau test `Joined`.
2. Tests cibles PlayMode : Stories 11, 16, 42, 43, 44 et 46.
3. Multiplayer Play Mode avec deux editeurs : le client demande slots 0 et 1; l'hote constate rage + incident une seule fois; le client constate le verdict et les etats repliques.
4. Recompilation sans erreur, puis suites EditMode et PlayMode completes vertes.

## Execution

- 2026-09-13 — Approuvee. Le sandbox `Dev_RageSandbox` adopte les roles MPPM existants, cree un acteur passager par client connecte et lie l'UI du client a son intention locale. Une fixture EditMode couvre la publication roster d'une selection gelee dans un lobby `Joined`. En attente des rapports de validation manuelle.
- 2026-09-13 — Validation manuelle recue : tests EditMode et PlayMode, y compris `DevRageSandboxAcceptsAllThreePassengerSlots`, verts. La preuve MPPM slots 0/1 est egalement confirmee. Findings Epic 4 fermes.

## Hors perimetre

- Transport Steam multi-processus automatise en CI : il necessite deux identites Steam et ne serait pas deterministe dans le runner actuel.
- Nouvelle fonctionnalite de gameplay ou refonte du catalogue, de la validation, du HUD ou des lobbies.

## Risques et garde-fous

- Le sandbox ne doit pas devenir un deuxieme flux de production : il reste un outil de verification, reutilise les composants de run existants et ne modifie pas les regles d'autorite.
- Aucun statut BMAD `done` avant reception des resultats de validation manuelle demandee ci-dessus.
