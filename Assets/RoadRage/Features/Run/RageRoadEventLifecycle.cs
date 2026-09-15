using System.Collections.Generic;
using RoadRage.Shared.Domain;

namespace RoadRage.Features.Run
{
    /// <summary>
    /// Story 5.6 : cycle de vie AD-22 de l'unique evenement Rage Road. Logique pure, sans Netcode et
    /// sans dependance a une autre feature : <see cref="RoadRage.Features.Run"/> ne connait ni la rage
    /// ni l'eligibilite IA (les asmdefs de features ne se referencent jamais entre eux), donc
    /// l'appelant App/Run projette les dispositions des candidats avant d'appeler
    /// <see cref="ResolveFirstTriggerIndex"/>.
    ///
    /// Ce fichier est le point unique de l'arbitrage premier-arrive-gagne (AD-16) et de l'anti-doublon :
    /// l'etat lui-meme refuse toute nouvelle demande, aucun drapeau "deja declenche" par vehicule n'est
    /// necessaire (ce serait une seconde source de verite a resynchroniser en fin de run).
    /// </summary>
    public static class RageRoadEventLifecycle
    {
        /// <summary>
        /// Etats pendant lesquels l'evenement occupe l'unique creneau Rage Road (AD-16) : aucun second
        /// evenement ne peut naitre tant qu'un de ces deux etats est publie.
        /// </summary>
        public static bool IsEventActive(RageRoadEventState state)
        {
            return state == RageRoadEventState.Triggered || state == RageRoadEventState.Confrontation;
        }

        /// <summary>
        /// Table AD-22 : seules les avancees d'un seul cran
        /// <c>Idle -> Triggered -> Confrontation -> Resolved -> RewardGranted</c> sont acceptees.
        /// Tout saut, tout retour arriere et toute transition depuis l'etat terminal sont refuses.
        /// </summary>
        public static bool CanAdvance(RageRoadEventState current, RageRoadEventState next)
        {
            if (current == RageRoadEventState.RewardGranted)
            {
                return false;
            }

            return next == (RageRoadEventState)((int)current + 1);
        }

        /// <summary>
        /// Condition de declenchement : aucune valeur authorée propre au Rage Road, seulement la
        /// disposition <see cref="RageDisposition.ConfrontationCapable"/> deja derivee des paliers de
        /// <c>RageTuningDef</c> (95 dans <c>RageTuningDef_Default</c>) via <c>IRageDispositionSource</c>.
        /// Un seuil dedie dupliquerait l'autorite du tuning de rage et creerait une seconde machine.
        /// </summary>
        public static bool IsTriggerConditionMet(RageDisposition disposition)
        {
            return disposition == RageDisposition.ConfrontationCapable;
        }

        /// <summary>
        /// Index du premier candidat qualifie d'une liste deja ordonnee par <c>NetworkObjectId</c>
        /// (<c>AiRageTargetResolution.FindEligibleCandidates</c>, donc identique sur l'hote et sur un
        /// client), ou -1 si la demande est refusee.
        ///
        /// Rien n'est accepte tant que l'etat n'est pas <see cref="RageRoadEventState.Idle"/> ce qui rend
        /// l'anti-doublon (la meme IA qui reste au-dessus de la condition) et l'arbitrage du meme tick
        /// (une seule cible, la premiere de l'ordre) triviaux a garantir.
        /// </summary>
        public static int ResolveFirstTriggerIndex(RageRoadEventState currentState, IReadOnlyList<RageDisposition> orderedDispositions)
        {
            if (currentState != RageRoadEventState.Idle || orderedDispositions == null)
            {
                return -1;
            }

            for (var i = 0; i < orderedDispositions.Count; i++)
            {
                if (IsTriggerConditionMet(orderedDispositions[i]))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
