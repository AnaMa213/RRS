# Proposition de correction de trajectoire — ciblage IA Rage/Fear — 2026-09-14

## Résumé du problème

Le ciblage actuel des actions rage/peur utilise une cible implicite et peut verrouiller la caméra sur un point proche du spawn qui n'est pas un véhicule IA. Les actions affectent alors une cible arbitraire; le défaut concerne Host et Clients.

## Impact et approche retenue

Le travail différé issu de la Story 5.4 devient une story autonome placée après les états de comportement : il modifie le ciblage local, la caméra, les intentions d'action et leur validation réseau, mais pas la machine de comportement IA. C'est une correction de backlog modérée : aucune exigence produit ou décision d'architecture n'est retirée.

## Modifications approuvées

| Ancien identifiant | Nouvel identifiant |
| --- | --- |
| Travail différé : ciblage IA Rage/Fear | **Story 5.5: Networked AI Rage Targeting** |
| Story 5.5: Rage Road Event Trigger | **Story 5.6** |
| Story 5.6: AI Traffic Networking and Client Presentation | **Story 5.7** |
| Story 5.7: Epic 5 AI Traffic Playable Checkpoint | **Story 5.8** |

La Story 5.5 exige un lock individuel T/Y, la sélection déterministe d'IA éligibles, une cible temporaire dans la portée quand aucun lock n'existe, l'absence de repli quand un lock est hors portée, et une validation host de la cible/range avant synchronisation rage/peur.

## Artefacts modifiés

- `epics.md` : nouvelle Story 5.5, critères d'acceptation et renumérotation 5.6–5.8.
- `sprint-status.yaml` : clés backlog renumérotées.
- `epic-5-context.md` : ordre, dépendances, contraintes et UX de ciblage mis à jour.
- Specs historiques 5.2 et 5.3 : références prospectives renumérotées.

## Handoff

Portée : modérée — réorganisation du backlog terminée. La Story 5.4 reste prête à approuver; la Story 5.5 sera spécifiée et implémentée après 5.4. Le test intégré final reste `Assets/RoadRage/App/Scenes/MVP_Run.unity`.
