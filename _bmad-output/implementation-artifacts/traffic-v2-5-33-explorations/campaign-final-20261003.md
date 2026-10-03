# Campagne exploratoire finale Story 5.33 (2026-10-03 12:12-12:17)

Code fige : dernier `.cs` modifie a 11:16, avant la campagne et apres D15. Domain Reload actif, aucun redemarrage de
l'Editeur entre les runs.

Commande : `.\scripts\validate.ps1 -TestMode PlayMode -TestFilter Story533Exploration -TestFilterType category -IncludeExplicit`

Sortie : `Tests PlayMode 5/5 passes`, 0 skipped, 0 erreur Console depuis le curseur, compilation saine, aucune scene
modifiee, `OK`. La selection etant ciblee, la suite complete n'est pas executee.

Les constats ci-dessous sont publies, pas juges (spec 5.33, Design Notes « Explorations »). Seul un invariant viole ou une
anomalie de cout D7 aurait fait echouer la campagne : aucun ne l'a ete.

## Resume par scenario

| Run | Pas hote | Sorties | Contacts V2-V2 | Repli 2a | PerceptionUnavailable | Collecteur max / 256 | Cout / vehicule / pas |
|---|---|---|---|---|---|---|---|
| explore-2 (`121210`) | 1 810 | 2/2 | 0 | 0 | aucun | 76, 0 sature | 0,647 ms |
| explore-2-rejeu (`121247`) | 1 810 | 2/2 | 0 | 0 | aucun | 76, 0 sature | 0,646 ms |
| explore-4 (`121400`) | 3 604 | 4/4 | 0 | 0 | aucun | 78, 0 sature | 0,716 ms |
| explore-8 (`121631`) | 7 500 / 7 500 | 5/8 | 1 episode | 0 | aucun | 101, 0 sature | 0,621 ms |
| explore-poussee (`121649`) | 855 (arret diagnostique) | 0/2 | 0 | 1 (vehicule pousse) | aucun | 72, 0 sature | 0,660 ms |

## Invariants et cout D7

- Invariants verts dans les cinq runs : aucun NaN, un intent par vehicule et par pas, une frame par pas
  (frames construites = pas hote - 2, 0 refusee), aucun retrait hors portail, population <= maximum, joueur hote absent de
  tout fait V2.
- D7, seuils declares avant mesure :
  - N = 2 : 0,647 ms par vehicule, pour un seuil de 2 x 4,943 = 9,886 ms ;
  - N = 8 : 0,621 ms, pour un seuil de 2 x 0,647 = 1,294 ms.
  - Le cout par vehicule ne croit pas avec N (0,65 / 0,72 / 0,62 ms). Postes a N = 8 : frame 0,272, perception 0,088,
    spine 0,120, plan + arbitrage 0,131, composition 0,010 ms.
- Collecteur : au plus 101 colliders par requete (capacite D9 256), aucun pas-vehicule sature dans toute la campagne.

## Constats

1. **Suivi et file (explore-2, explore-4).** File de 4 en StopHold, a 2,466-2,499 m du leader. Liberation en cascade :
   SourceGone, puis SourceDeparted a +0,68 / +1,36 / +1,1 s. 0 m de rampement apres arret. 4/4 sorties. Identique au
   run du 2026-10-02 22:01 (memes trames d'entree en StopHold : 703 / 885 / 1014 / 1183).
2. **Reproductibilite physique (explore-2 contre son rejeu).** Ecart maximal de 0,0001 m sur 3 017 pas-vehicule.
3. **Interblocage de carrefour (explore-8), constat pour la 5.34 et la 5.39.**
   - Il s'agit du meme croisement que le run rouge du 2026-10-02 22:02. Au pas hote 1846, le vehicule 1 libere du
     maintien derriere l'obstacle « sortie » traverse le carrefour a 6,9 m/s sur `40ca7f10`. Le vehicule 8, qui
     arrive a 3,8 m/s sur `4e437f94` (zone de conflit `ConflictZones[23]`, controles distincts), est vu des deux cotes
     seulement comme obstacle `TrafficActor` au chevauchement. L'impulsion de contact vaut 1 178 N.s.
   - Difference avec le 2026-10-02 : d max du vehicule 1 = 0,331 m, sous epsilon_t = 0,34 m. Aucun verrou 2a, donc la
     regle D12 n'a pas ete sollicitee.
   - Consequence nouvelle : les deux vehicules entrent en StopHold chacun sur l'autre, au jeu negatif (-3,92 m et
     -3,04 m), et ne sont jamais liberes : de la frame 1866 a la fin de la trace (7499), soit 113 s, la source ne
     disparait pas, ne repart pas et le jeu ne remonte pas. Le vehicule 5 reste en file derriere le vehicule 1 (blocker
     `Obstacle:...01`). D'ou 5 sorties sur 8.
   - A traiter par la 5.34 (grants de carrefour : le croisement ne devrait pas avoir lieu) et par la 5.39
     (interpretation et recuperation d'un StopHold au jeu negatif, c'est-a-dire en contact). Ce n'est pas un defaut 5.33.
4. **Contact de decor (explore-8, vehicule 7).** Contact avec les colliders `Relief_DosDane_AvenueCenterToEast/Rampe_*`,
   pas 950-955, impulsion 0, vitesse 5,03 -> 4,91 m/s, sans consequence de suivi.
5. **Poussee (explore-poussee).** Impulsion laterale de 6 m/s au pas 700, verrou 2a au pas 705 (d 0,383 m), arret
   diagnostique au pas 855. Le suiveur continue sans contact. Identique au 2026-10-02 (contamination 705, arret 855).

## Lecture

Les scenarios nominaux a 2 et 4 vehicules et la poussee reproduisent le comportement d'avant D14/D15. Explore-8
rencontre le meme croisement non coordonne, qui finit cette fois en interblocage mutuel plutot qu'en verrou : c'est le
cas attendu d'un carrefour sans grants. Le cout par vehicule reste plat de N = 2 a N = 8, sous 0,75 ms.

## Rejeu apres les correctifs de revue (2026-10-03 15:52-16:05)

Correctifs : blocker d'un maintien dont la source n'est pas vue (perception indisponible) qui garde ses proprietes, et
assertion d'epoque de decision dans l'observateur du harnais. Toute la section Verification de la spec a ete rejouee,
dans cet ordre :

| Profil | Resultat |
|---|---|
| Story 5.33 EditMode | 66/66 |
| Story 5.33 PlayMode (A `155248`, B `155325`) | 2/2 |
| Story 5.31 EditMode | 50/50 |
| Story 5.31 PlayMode | 13/13 |
| Story 5.52 PlayMode | 4/4 |
| Story533Exploration | 5/5 |

Chaque run : 0 erreur Console, compilation saine, aucune scene modifiee.

Les constats sont identiques a la campagne de 12:12 :
- explore-2 : 2/2 sorties ;
- explore-4 : 4/4 sorties, sans contact ;
- explore-8 : 5/8 sorties, meme interblocage de carrefour, 0 repli 2a ;
- explore-poussee : contamination au pas 705.

Reproductibilite explore-2 contre son rejeu : 0,001 m sur 3 017 pas-vehicule. Cout par vehicule : 0,64 a 0,73 ms de
N = 2 a N = 8.
