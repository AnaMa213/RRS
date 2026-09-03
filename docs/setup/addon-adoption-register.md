# Registre d'adoption add-on, librairie et asset

Utilise ce registre avant d'importer un add-on Unity, une librairie, un framework UI, un scaffold controller, un starter asset, un art genere, un modele telecharge ou un asset payant. Le but est de reutiliser intelligemment sans laisser un package externe devenir proprietaire cache de l'etat gameplay central.

## Vocabulaire de statut partage

| Statut | Signification |
| --- | --- |
| `Not Started` | Le candidat n'a pas encore ete evalue. |
| `In Progress` | L'evaluation a commence, mais la preuve ou la revue est incomplete. |
| `Pass` | Le candidat a passe la revue pour l'usage et la decision indiques. |
| `Blocked` | Le candidat ne peut pas avancer tant qu'un risque ou une preuve manquante n'est pas resolu. |
| `Not Applicable` | Le candidat est hors scope ou n'est plus pertinent ; noter pourquoi. |

## Regle d'adoption

1. Evaluer d'abord.
2. Importer ensuite, seulement quand la ligne est assez complete pour le justifier.
3. Wrapper ou adapter ensuite pour que le code projet garde la limite d'integration.
4. Customiser en dernier, apres que l'asset fonctionne dans une petite scene de test.

N'importe quel asset ou package tiers doit passer par cette revue avant d'entrer dans le flux MVP principal. Meme les packages Unity built-in et primitives meritent une ligne legere quand ils structurent menu/UI, controller, mouvement, reseau, art ou strategie placeholder.

**Politique gratuit/libre/open-source par defaut (ajoutee par course-correction, `sprint-change-proposal-2026-09-02.md`) :** Sauf approbation humaine explicite documentee avec cout et justification, tout candidat doit etre gratuit, libre de droits et idealement open-source ou un package Unity/Steamworks built-in/officiel. Un candidat payant ou a licence fermee ne peut recevoir `Adopt` sans note explicite de justification cout/revenu dans la colonne Notes.

## Options de decision

Les valeurs de decision `Pending`, `Adopt`, `Reject`, `Defer`, `Needs Spike` et `Not Applicable` sont des tokens controles. Ils restent volontairement non traduits pour faciliter les filtres, les recherches et les futures automatisations.

Avant toute decision `Adopt`, la ligne doit renseigner : responsable, validateur, licence, URL source, version/provenance, droits de redistribution si pertinents, cout, compatibilite Unity, statut de maintenance, impact dependances, securite multiplayer, source/editabilite, alignement architecture, cout de rollback, preuve et notes de decision.

## Registre

| ID | Statut | Responsable | Validateur | Candidat | Categorie | Source / URL | Version / provenance | Licence | Droits redistribution | Cout | Compatibilite Unity | Maintenance | Impact dependances | Securite multiplayer | Source / editabilite | Alignement architecture | Cout rollback | Decision | Preuve | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| ADDON-001 | `Not Started` | A designer | Agent | Fondation UI built-in Unity ou choix package UI | Fondation menu/UI | A renseigner | A renseigner | A renseigner | A renseigner si redistribution de contenu | A renseigner | Doit supporter Unity `6000.6.0f1` et URP `17.6.0` | A renseigner | A renseigner | L'UI doit lire l'etat partage et envoyer des intentions, sans muter directement l'etat gameplay | A renseigner | Doit preserver `RoadRage.Features.UI`, les erreurs Lobby/UI visibles et l'ownership host-authoritative | A renseigner | `Pending` | Aucune | Evaluer avant le travail menu/lobby Epic 1. |
| ADDON-002 | `Not Started` | A designer | Agent | Scaffold mouvement/controller ou starter controller | Mouvement joueur/controller | A renseigner | A renseigner | A renseigner | A renseigner si redistribution de contenu | A renseigner | Doit supporter Unity `6000.6.0f1`, Input System `1.20.0` et Cinemachine `6.6.0` | A renseigner | A renseigner | Camera/input doivent rester local-only et transformer les actions joueur en intentions validees par l'host | A renseigner | Ne doit pas introduire de mutation gameplay owner-authoritative ou pile input/network parallele | A renseigner | `Pending` | Aucune | Evaluer avant adoption on-foot ou vehicle control. |
| ADDON-003 | `Not Started` | A designer | Agent | Asset vehicle controller ou helper arcade driving | Fondation vehicule | A renseigner | A renseigner | A renseigner | A renseigner si redistribution de contenu | A renseigner | Doit supporter Unity `6000.6.0f1`, URP `17.6.0` et le stack Unity Transport/Netcode | A renseigner | A renseigner | L'host doit simuler le mouvement de la voiture joueur pour le MVP ; prediction client presentation-only sauf approbation ulterieure | A renseigner | Doit convenir a une direction arcade Rigidbody et eviter le scope creep simulation realiste | A renseigner | `Pending` | Aucune | Utiliser greybox primitives tant que la boucle online n'est pas prouvee. |
| ADDON-004 | `Not Started` | A designer | Agent | Sample ou helper multiplayer/lobby | Fondation lobby/network | A renseigner | A renseigner | A renseigner | A renseigner si redistribution de contenu | A renseigner | Doit supporter Netcode for GameObjects `2.13.2`, le transport Steamworks (voir ADDON-006) et Unity Transport `6.6.0` | A renseigner | A renseigner | Doit preserver lobbies prives host-created, invite Steam/Lobby ID, `MaxPlayers = 4` et aucun port forwarding routeur | A renseigner | Ne doit pas introduire dedicated servers, matchmaking public, host migration ou modele service separe sans approbation | A renseigner | `Pending` | Aucune | Preferer les samples officiels Unity/Steamworks apres revue de version. |
| ADDON-005 | `Not Started` | A designer | Agent | Placeholder art, modele genere, modele telecharge ou kit environnement | Asset 3D | A renseigner | A renseigner | A renseigner | A renseigner si redistribution de contenu | A renseigner | Doit importer proprement dans Unity `6000.6.0f1` via workflow FBX ou GLB | A renseigner | A renseigner | Le remplacement mesh ne doit pas changer identite prefab gameplay, registration `NetworkObject`, composants gameplay, colliders ou definition ids | Doit fournir une source editable ou un fichier source nettoye Blender | Doit passer l'intake Blender avant creation prefab | A renseigner | `Pending` | Aucune | Greybox first, art second reste un principe verrouille. |
| ADDON-006 | `In Progress` | Kenan | Agent | Transport Steamworks pour Netcode for GameObjects (`com.community.netcode.transport.facepunch` ou `.steamnetworkingsockets`) | Fondation lobby/network | https://github.com/Unity-Technologies/multiplayer-community-contributions (dossier Transports) | Commit/tag exact a epingler lors de l'installation reelle (Story 0.2 revisee) | A verifier lors de l'installation reelle -- pas de `Adopt` avant verification | Aucun (bibliotheque de code, pas de contenu redistribue) | Gratuit (package communautaire open-source) | Doit supporter Netcode for GameObjects `2.13.2` -- a verifier a l'installation | Depot communautaire officiel sous `Unity-Technologies/multiplayer-community-contributions`, sans garantie de support officiel Unity | Ajoute le SDK Steamworks natif (Facepunch.Steamworks ou Steamworks.NET) ; necessite Steam client installe cote joueur | Doit preserver sessions privees host-created, cap `MaxPlayers = 4`, absence de port forwarding routeur | Open-source, editable | Doit respecter AD-1/AD-4 revisees (`sprint-change-proposal-2026-09-02.md`) | Retrait du package et retour a Unity Transport si besoin ; Netcode for GameObjects reste inchange (transport swappable) | `In Progress` | `sprint-change-proposal-2026-09-02.md` | Remplace `com.unity.services.multiplayer` ; approuve en principe via ce Sprint Change Proposal, finalisation (licence + commit/tag) lors de l'installation reelle. |

## Template de revue candidat

Copie cette ligne pour ajouter un candidat :

| ID | Statut | Responsable | Validateur | Candidat | Categorie | Source / URL | Version / provenance | Licence | Droits redistribution | Cout | Compatibilite Unity | Maintenance | Impact dependances | Securite multiplayer | Source / editabilite | Alignement architecture | Cout rollback | Decision | Preuve | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| ADDON-### | `Not Started` |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  | `Pending` |  |  |

## Gate avant `Adopt`

Ne passe jamais une ligne a `Adopt` si son Statut n'est pas `Pass`, ou si l'un de ces champs est vide, invraisemblable ou non verifiable : licence, source URL, version/provenance, droits de redistribution si pertinents, cout, compatibilite Unity `6000.6.0f1`, statut de maintenance, impact dependances, securite multiplayer, source/editabilite, alignement avec l'architecture spine, cout de rollback, responsable, validateur, preuve et notes de decision.

Si un champ critique reste inconnu, utiliser `Needs Spike`, `Defer`, `Reject` ou `Blocked` selon le risque. Un asset importe doit pouvoir etre retire sans casser la verite runtime host-authoritative, les IDs de definitions, les colliders gameplay, les scenes seed ou les packages verrouilles.

## Declencheurs de rejet ou blocage

Rejette ou bloque un candidat s'il :

- Exige une autre version Unity, render pipeline, input stack, networking stack ou modele de service.
- Possede la verite gameplay centrale pour l'issue de run, le cycle de vie joueur, la rage, l'economie, l'etat boss ou l'autorite reseau.
- Cache son source ou empeche l'edition pratique d'un comportement gameplay critique.
- Ajoute des services payants, comptes, telemetrie, backend economy ou secrets sans approbation explicite.
- Ne peut pas etre teste dans une petite scene avant le flux MVP principal.
- Contourne le nettoyage Blender pour les assets 3D generes ou telecharges.
