using System.Collections.Generic;
using RoadRage.Features.Run;
using RoadRage.Features.UI;
using RoadRage.Features.Vehicles;
using RoadRage.Shared.Domain;
using Unity.Netcode;
using UnityEngine;

namespace RoadRage.App.Run
{
    /// <summary>
    /// Story 5.6 : proprietaire unique du declenchement de l'evenement Rage Road. App/Run est le seul
    /// assemblage qui voit a la fois Features/Run (l'etat AD-17/AD-22), Features/Rage (la disposition
    /// deja derivee des paliers de RageTuningDef) et Features/Vehicles (l'eligibilite et l'ordre
    /// deterministe des candidats) : la logique pure vit dans
    /// <see cref="RageRoadEventLifecycle"/>, l'orchestration host-side et la presentation ici.
    ///
    /// Cote hote : evaluer les candidats eligibles et, si l'etat est <c>Idle</c>, ecrire
    /// <c>Triggered</c> + la cible dans <see cref="NetworkedRunState"/>. Cote tous les pairs : rafraichir
    /// le libelle HUD, le marqueur <c>[RAGE ROAD]</c> de la cible et le signalement de cible perdue
    /// depuis les NetworkVariables -- un client ne fait que lire. Aucun nouveau composant HUD :
    /// RunFlowController reste le hub existant, ce composant ne fait qu'ajouter sa propre ligne.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RageRoadEventFlowController : MonoBehaviour
    {
        private NetworkedRunState runState;

        private RunCheckpointHudScreen checkpointHud;

        private AIVehicleBehaviorDebugView markedEventTargetView;

        private ulong lastRefusedTargetId;

        private bool hasRefusedRequestLog;

        /// <summary>
        /// Cadence de l'evaluation host-side. Un balayage de scene par frame
        /// (<c>AiRageTargetResolution.FindEligibleCandidates</c> : <c>FindObjectsByType</c> + tri +
        /// allocation) serait un cout permanent sur le pair qui simule deja, pour une escalade de rage
        /// pilotee par le joueur -- donc jamais frame-critique, et l'Epic 5 ne quitte jamais
        /// <c>Triggered</c> une fois declenche. Assez reactif pour le ressenti, assez espace pour que le
        /// balayage ne domine pas le tick.
        /// </summary>
        private const float HostEvaluationIntervalSeconds = 0.25f;

        private float nextHostEvaluationSeconds;

        private void Update()
        {
            var state = ResolveRunState();
            if (state == null)
            {
                return;
            }

            // Meme ordre que les autres chemins d'App/Run : la decision host-side d'abord, puis la
            // presentation -- l'hote publie sa propre modification dans la meme frame puis l'affiche.
            if (IsAuthoritative() && Time.time >= nextHostEvaluationSeconds)
            {
                nextHostEvaluationSeconds = Time.time + HostEvaluationIntervalSeconds;
                EvaluateHostTrigger(state);
            }

            RefreshEventPresentation(state);
        }

        private void OnDestroy()
        {
            // Le marqueur de cible est purement local (jamais replique) : le retirer evite de laisser un
            // vehicule marque si ce composant disparait avant que l'evenement ne se termine.
            if (markedEventTargetView != null)
            {
                markedEventTargetView.SetRageRoadEventTarget(false);
                markedEventTargetView = null;
            }
        }

        /// <summary>
        /// Arbitrage host-side, pure et sans Netcode (donc testable en EditMode) : premier candidat
        /// qualifie d'une liste deja ordonnee par <c>NetworkObjectId</c>
        /// (<c>AiRageTargetResolution.FindEligibleCandidates</c>, donc identique host/client), ou null.
        /// Seule la projection candidat -> disposition est faite ici : la decision reste dans
        /// <see cref="RageRoadEventLifecycle.ResolveFirstTriggerIndex"/>.
        /// </summary>
        public static NetworkedAIVehicleState ResolveFirstTriggerCandidate(
            RageRoadEventState currentState,
            IReadOnlyList<NetworkedAIVehicleState> orderedCandidates)
        {
            if (orderedCandidates == null || orderedCandidates.Count == 0)
            {
                return null;
            }

            var dispositions = new RageDisposition[orderedCandidates.Count];
            for (var i = 0; i < orderedCandidates.Count; i++)
            {
                var candidate = orderedCandidates[i];
                var source = candidate == null ? null : candidate.GetComponent<IRageDispositionSource>();
                dispositions[i] = source == null ? RageDisposition.Calm : source.CurrentDisposition;
            }

            var index = RageRoadEventLifecycle.ResolveFirstTriggerIndex(currentState, dispositions);
            return index < 0 ? null : orderedCandidates[index];
        }

        /// <summary>
        /// Cible perdue (matrice I/O) : l'evenement est actif mais sa reference ne se resout plus
        /// (cible detruite ou despawnee). L'etat reste celui publie par l'hote : ni retargeting ni reset
        /// automatique, seul le retour textuel change (le reset appartient a une story ulterieure).
        /// </summary>
        public static bool IsEventTargetLost(RageRoadEventState state, bool hasResolvedTarget)
        {
            return RageRoadEventLifecycle.IsEventActive(state) && !hasResolvedTarget;
        }

        /// <summary>
        /// Meme garde d'autorite que les autres chemins host-only d'App/Run
        /// (RunFlowController.IsAuthoritativeForDamage) : hote, ou aucune session en ecoute (solo).
        /// </summary>
        private static bool IsAuthoritative()
        {
            var manager = NetworkManager.Singleton;
            return manager == null || !manager.IsListening || manager.IsServer;
        }

        private void EvaluateHostTrigger(NetworkedRunState state)
        {
            // Ecrire dans une NetworkVariable server-write exige l'objet spawn : hors session (scenes de
            // dev sans NetworkedRunState spawn) l'evaluation reste un no-op silencieux.
            if (!state.IsSpawned)
            {
                return;
            }

            var current = state.RageRoadEvent.Value;
            var candidates = AiRageTargetResolution.FindEligibleCandidates();

            if (current == RageRoadEventState.Idle)
            {
                var target = ResolveFirstTriggerCandidate(current, candidates);
                if (target != null)
                {
                    WriteTrigger(state, target);
                }

                return;
            }

            // Un evenement occupe deja l'unique creneau (AD-16) : la demande concurrente est refusee,
            // la cible d'origine est conservee. On journalise la premiere demande refusee par cible, pas
            // une ligne par frame tant que la meme IA reste au-dessus de la condition.
            LogRefusedRequestIfAny(current, ResolveFirstTriggerCandidate(RageRoadEventState.Idle, candidates));
        }

        /// <summary>
        /// Ecriture unique de l'evenement (Epic 5 ne cable que <c>Idle -> Triggered</c>) : cible d'abord,
        /// etat ensuite, pour qu'un pair qui observe <c>Triggered</c> ait deja la cible. Aucune autre
        /// ecriture d'etat ni de cible n'existe dans ce composant -- c'est ce qui rend impossible tout
        /// retargeting silencieux comme tout reset automatique.
        /// </summary>
        private static void WriteTrigger(NetworkedRunState state, NetworkedAIVehicleState target)
        {
            state.RageRoadEventTarget.Value = new NetworkObjectReference(target.NetworkObject);
            state.RageRoadEvent.Value = RageRoadEventState.Triggered;

            Debug.Log("[Run] Rage Road declenche : cible " + target.gameObject.name + ".");
        }

        private void LogRefusedRequestIfAny(RageRoadEventState current, NetworkedAIVehicleState refusedTarget)
        {
            if (refusedTarget == null || (hasRefusedRequestLog && lastRefusedTargetId == refusedTarget.NetworkObjectId))
            {
                return;
            }

            hasRefusedRequestLog = true;
            lastRefusedTargetId = refusedTarget.NetworkObjectId;

            Debug.Log("[Run] Rage Road : demande refusee pour " + refusedTarget.gameObject.name
                + " -- premier-arrive-gagne, un seul evenement a la fois (etat : " + current + ").");
        }

        private void RefreshEventPresentation(NetworkedRunState state)
        {
            var current = state.RageRoadEvent.Value;
            var target = ResolveEventTarget(state.RageRoadEventTarget.Value);

            UpdateEventTargetMarker(target);

            var hud = ResolveCheckpointHud();
            if (hud == null)
            {
                return;
            }

            hud.ShowRageRoadEventStatus(
                current,
                target == null ? null : target.name,
                IsEventTargetLost(current, target != null));
        }

        /// <summary>
        /// Marque ou demarque le libelle monde du vehicule cible, sur chaque pair et pour son propre
        /// affichage uniquement. Comme le lock de Story 5.5 (ApplyRageTargetLockMarker), le marquage
        /// n'est jamais replique : il est re-derive depuis l'etat synchronise.
        /// </summary>
        private void UpdateEventTargetMarker(GameObject target)
        {
            var view = target == null ? null : target.GetComponent<AIVehicleBehaviorDebugView>();
            if (view == markedEventTargetView)
            {
                return;
            }

            if (markedEventTargetView != null)
            {
                markedEventTargetView.SetRageRoadEventTarget(false);
            }

            markedEventTargetView = view;
            if (markedEventTargetView != null)
            {
                markedEventTargetView.SetRageRoadEventTarget(true);
            }
        }

        /// <summary>
        /// Resolution de la reference de cible. La garde ne peut pas se limiter a l'existence du
        /// <see cref="NetworkManager"/> : <c>NetworkManager.Shutdown()</c> met <c>SpawnManager</c> a null
        /// tout en laissant <c>Singleton</c> non nul, et <c>NetworkObjectReference.TryGet</c> deroule
        /// jusqu'a ce <c>SpawnManager</c>. La ou un <c>TryGet</c> d'App/Run vivait jusqu'ici dans une RPC
        /// (donc avec une session servante vivante), celui-ci est lu a chaque frame : une perte d'hote
        /// pendant un evenement declenche (le moniteur de session arrete la connexion un frame avant de
        /// changer de scene) leverait une NullReferenceException par frame. D'ou la garde sur une session
        /// reellement en ecoute.
        /// </summary>
        private static GameObject ResolveEventTarget(NetworkObjectReference reference)
        {
            var manager = NetworkManager.Singleton;
            if (manager == null || !manager.IsListening)
            {
                return null;
            }

            if (!reference.TryGet(out var networkObject, manager) || networkObject == null)
            {
                return null;
            }

            return networkObject.gameObject;
        }

        private NetworkedRunState ResolveRunState()
        {
            if (runState == null)
            {
                // Patron "resoudre l'etat de run par recherche de scene" (RunFlowController) : aucun
                // cablage serialise a maintenir dans la scene, donc rien a oublier au chargement.
                runState = FindAnyObjectByType<NetworkedRunState>();
            }

            return runState;
        }

        private RunCheckpointHudScreen ResolveCheckpointHud()
        {
            if (checkpointHud == null)
            {
                checkpointHud = FindAnyObjectByType<RunCheckpointHudScreen>();
            }

            return checkpointHud;
        }
    }
}
