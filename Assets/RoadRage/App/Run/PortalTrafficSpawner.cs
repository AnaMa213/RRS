using System.Collections;
using System.Collections.Generic;
using RoadRage.Features.Online;
using RoadRage.Features.Run;
using RoadRage.Features.Vehicles;
using RoadRage.Shared.Domain;
using Unity.Netcode;
using UnityEngine;

namespace RoadRage.App.Run
{
    /// <summary>
    /// Story 5.10 : service hote qui fait entrer et sortir le trafic EXCLUSIVEMENT par les portails
    /// authores du <see cref="LaneGraph"/> (AD-34). Il tend vers un effectif cible resolu a
    /// l'execution : aucun effectif litteral ne vit ici, dans un prefab, ni dans la scene.
    ///
    /// La seule porte de sortie du trafic est l'arrivee a un noeud <c>PortalExit</c>, signalee par
    /// <see cref="NetworkedAIVehicleDriverController.HasReachedExitPortal"/>. Aucun autre mecanisme --
    /// blocage, embouteillage, distance au joueur, echec de trajet, capot retourne, hors-zone -- ne
    /// retire un vehicule : le palier de recuperation sur place de la Story 5.2 reste, et il remet le
    /// vehicule en jeu au lieu de le supprimer.
    ///
    /// La "file d'attente" d'insertion est le deficit lui-meme (cible moins effectif vivant) : un
    /// portail encombre n'est pas servi ce pas-ci, le deficit reste, et le pas suivant retente. Rien
    /// n'est donc jamais abandonne, et la file est bornee par l'effectif cible par construction.
    ///
    /// Vit dans l'assembly App aux cotes de <see cref="DevVehicleSpawner"/> et de
    /// <see cref="NetworkedPlayerSpawnService"/> : <c>RunFlowController</c> et
    /// <c>RunCompositionRoot</c> restent exempts d'appel ".Spawn(" / "Instantiate(" (garde Epic 1).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PortalTrafficSpawner : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Graphe de voies du district. Seule surface consommee : portails d'entree, portails de sortie, positions de noeuds, reglages de trafic.")]
        private LaneGraph laneGraph;

        [SerializeField]
        [Tooltip("Prefab reseau du vehicule de trafic (enregistre dans DefaultNetworkPrefabs). Non assigne : aucune insertion, avertissement une fois.")]
        private GameObject vehiclePrefab;

        /// <summary>Vehicules inseres par ce service, host-owned. Jamais partagee, jamais statique.</summary>
        private readonly List<NetworkObject> liveVehicles = new List<NetworkObject>();

        private readonly Collider[] clearanceHits = new Collider[32];

        private bool isActiveHost;
        private bool sceneProcessed;
        private bool warnedMissingGraph;
        private bool warnedMissingSettings;
        private bool warnedMissingPrefab;
        private bool warnedMissingEntryPortal;
        private bool warnedPrefabWithoutNetworkObject;
        private bool warnedPrefabWithoutDriverController;
        private int insertionCounter;

        /// <summary>Etat de run hote-owned, resolu paresseusement : seule surface de lecture de la valeur de session resolue.</summary>
        private NetworkedRunState runState;

        /// <summary>Vrai des que la valeur de session a ete depossee dans l'etat de run, ou qu'il a ete etabli qu'aucune session n'en portait.</summary>
        private bool sessionSettingsPublished;

        /// <summary>Effectif actuellement en jeu : lu par les tests d'integration.</summary>
        public int LivePopulation
        {
            get { return liveVehicles.Count; }
        }

        private void Awake()
        {
            var manager = NetworkManager.Singleton;
            isActiveHost = manager != null && manager.IsListening && manager.IsServer;

            if (isActiveHost)
            {
                StartCoroutine(WaitForNetworkSceneProcessing());
            }
        }

        /// <summary>
        /// Meme rythme que <see cref="NetworkedPlayerSpawnService"/> : une frame d'attente pour que le
        /// SceneManager reseau ait fini de traiter MVP_Run avant toute insertion.
        /// </summary>
        private IEnumerator WaitForNetworkSceneProcessing()
        {
            yield return null;
            sceneProcessed = true;
        }

        private void FixedUpdate()
        {
            if (!isActiveHost || !sceneProcessed)
            {
                return;
            }

            var manager = NetworkManager.Singleton;
            if (manager == null || !manager.IsListening || !manager.IsServer)
            {
                return;
            }

            ReleaseVehiclesAtExitPortals();

            if (laneGraph == null)
            {
                WarnOnce(ref warnedMissingGraph, "aucun LaneGraph assigne : aucun vehicule n'est insere.");
                return;
            }

            var settings = laneGraph.TrafficSettings;
            if (settings == null)
            {
                // Aucun repli code en dur : sans Def de trafic il n'existe aucun effectif cible, donc
                // aucune insertion. Inventer une valeur ici serait exactement le reglage cable que
                // AD-32 supprime.
                WarnOnce(ref warnedMissingSettings, "aucun TrafficSettingsDef assigne sur le LaneGraph : aucun vehicule n'est insere.");
                return;
            }

            if (vehiclePrefab == null)
            {
                WarnOnce(ref warnedMissingPrefab, "aucun prefab de vehicule de trafic assigne : aucun vehicule n'est insere.");
                return;
            }

            if (!PublishSessionTrafficSettingsOnce(settings))
            {
                return;
            }

            var deficit = ResolveSessionTargetPopulation(settings) - liveVehicles.Count;
            if (deficit <= 0)
            {
                return;
            }

            var entryPortals = laneGraph.EntryPortals;
            if (entryPortals.Count == 0)
            {
                WarnOnce(ref warnedMissingEntryPortal, "le graphe de voies ne porte aucun portail d'entree : aucun vehicule n'est insere.");
                return;
            }

            for (var i = 0; i < entryPortals.Count && deficit > 0; i++)
            {
                var portalIndex = entryPortals[i];

                // Portail encombre : ce portail n'est pas servi ce pas-ci. Le deficit reste, donc
                // l'insertion est simplement reportee au pas suivant -- jamais abandonnee.
                if (!IsPortalClear(portalIndex, settings.PortalClearanceRadius))
                {
                    continue;
                }

                if (TryInsertAtPortal(portalIndex))
                {
                    deficit--;
                }
            }
        }

        /// <summary>
        /// Point de branchement UNIQUE de l'effectif cible (Story 5.16). La valeur de session resolue
        /// vit dans <see cref="NetworkedRunState"/> : elle y a ete deposee par l'hote depuis la valeur
        /// publiee au lobby, qui est le seul chemin de livraison. Tant qu'aucune session ne l'a
        /// resolue (<see cref="SessionTrafficValue.Unresolved"/>), la valeur par defaut authoree du Def
        /// est la seule source. Aucun autre endroit du code, du prefab ou de la scene ne porte
        /// d'effectif (AD-32).
        /// </summary>
        private int ResolveSessionTargetPopulation(TrafficSettingsDef settings)
        {
            var sessionValue = runState != null ? runState.AiVehicleTargetCount.Value : SessionTrafficValue.Unresolved;
            if (sessionValue < 0)
            {
                return settings.ClampTargetPopulation(settings.DefaultTargetPopulation);
            }

            return settings.ClampTargetPopulation(sessionValue);
        }

        /// <summary>
        /// Story 5.16 : depose une seule fois dans l'etat de run la valeur de session resolue par le
        /// lobby. La source est l'instantane du service de roster, qui vit sur le bootstrap persistant
        /// et survit donc au chargement de MVP_Run : aucun troisieme porteur n'est introduit, et le
        /// chemin de la Story 2.4 reste le seul par lequel une valeur franchit le chargement.
        ///
        /// Sans room ouverte, aucune valeur n'a ete publiee : l'etat de run garde
        /// <see cref="SessionTrafficValue.Unresolved"/> et le repli sur le defaut authore s'applique.
        /// Si l'etat de run n'est pas encore apparu, la tentative est simplement reportee.
        /// </summary>
        private bool PublishSessionTrafficSettingsOnce(TrafficSettingsDef settings)
        {
            if (sessionSettingsPublished)
            {
                return true;
            }

            if (runState == null)
            {
                runState = FindAnyObjectByType<NetworkedRunState>();
                if (runState == null)
                {
                    return false;
                }
            }

            var bootstrap = RoadRageBootstrap.Instance;
            if (bootstrap == null)
            {
                sessionSettingsPublished = true;
                return true;
            }

            var roster = bootstrap.LobbyRoster;
            if (roster == null)
            {
                var room = bootstrap.LobbyRoom;
                if (room == null || room.Status != LobbyRoomStatus.Open)
                {
                    sessionSettingsPublished = true;
                }

                return sessionSettingsPublished;
            }

            var snapshot = roster.Current;
            if (snapshot.HasLobby && snapshot.AiVehicleTargetCount >= 0 && snapshot.LitterThrowerCount >= 0)
            {
                var targetPopulation = settings.ClampTargetPopulation(snapshot.AiVehicleTargetCount);
                var litterThrowers = Mathf.Min(settings.ClampLitterThrowers(snapshot.LitterThrowerCount), targetPopulation);
                runState.AiVehicleTargetCount.Value = targetPopulation;
                runState.LitterThrowerCount.Value = litterThrowers;
                sessionSettingsPublished = true;
                return true;
            }

            // L'instantane du roster est lu par polling (LobbyRosterService.Tick) : tant qu'une room hote
            // est ouverte, un instantane encore non resolu peut simplement vouloir dire qu'aucun Tick n'a
            // eu lieu depuis le chargement de la scene. Renoncer ici figerait le trafic sur le defaut
            // authore pour TOUTE la run, alors que la valeur publiee arrive une frame plus tard. Seule
            // l'absence certaine de session autorise a renoncer.
            var lobbyRoom = bootstrap.LobbyRoom;
            if (lobbyRoom == null || lobbyRoom.Status != LobbyRoomStatus.Open)
            {
                sessionSettingsPublished = true;
            }

            return sessionSettingsPublished;
        }

        /// <summary>
        /// LA seule porte de sortie du trafic : l'arrivee a un noeud de portail de sortie, signalee
        /// par le driver. Aucun compteur, aucune distance, aucun echec de trajet n'entre ici.
        /// </summary>
        private void ReleaseVehiclesAtExitPortals()
        {
            for (var i = liveVehicles.Count - 1; i >= 0; i--)
            {
                var networkObject = liveVehicles[i];
                if (networkObject == null)
                {
                    // Objet deja detruit par ailleurs (arret de session, dechargement de scene) :
                    // simple menage de la liste, ce n'est pas un retrait de trafic.
                    liveVehicles.RemoveAt(i);
                    continue;
                }

                var controller = networkObject.GetComponent<NetworkedAIVehicleDriverController>();
                if (controller == null || !controller.HasReachedExitPortal)
                {
                    continue;
                }

                liveVehicles.RemoveAt(i);

                if (networkObject.IsSpawned)
                {
                    networkObject.Despawn();
                }
            }
        }

        /// <summary>
        /// Zone d'insertion libre : aucun vehicule ni pieton dans le rayon d'encombrement authore.
        /// Le decor statique ne porte ni Rigidbody ni CharacterController et n'encombre donc rien --
        /// sans ce filtre, les parois du tunnel bloqueraient l'insertion en permanence.
        /// </summary>
        private bool IsPortalClear(int portalIndex, float clearanceRadius)
        {
            var origin = laneGraph.GetNodePosition(portalIndex);
            var found = Physics.OverlapSphereNonAlloc(
                origin, clearanceRadius, clearanceHits, ~0, QueryTriggerInteraction.Ignore);

            // NonAlloc ne dit pas quels colliders ont ete tronques. Un tampon plein signifie donc
            // environnement inconnu, jamais portail libre.
            if (found >= clearanceHits.Length)
            {
                return false;
            }

            for (var i = 0; i < found; i++)
            {
                var candidate = clearanceHits[i];
                if (candidate == null)
                {
                    continue;
                }

                if (candidate.attachedRigidbody != null || candidate.GetComponentInParent<CharacterController>() != null)
                {
                    return false;
                }
            }

            return true;
        }

        private bool TryInsertAtPortal(int portalIndex)
        {
            var instance = Instantiate(
                vehiclePrefab, laneGraph.GetNodePosition(portalIndex), laneGraph.GetNodeRotation(portalIndex));

            insertionCounter++;
            instance.name = "AI_Vehicle_Portal_" + insertionCounter.ToString("D3");

            var networkObject = instance.GetComponent<NetworkObject>();
            if (networkObject == null)
            {
                Destroy(instance);
                WarnOnce(ref warnedPrefabWithoutNetworkObject,
                    "le prefab de vehicule de trafic ne porte pas de NetworkObject : aucune insertion possible.");
                return false;
            }

            // Le composant qui fait CONDUIRE le vehicule est aussi indispensable que le NetworkObject :
            // sans lui le vehicule ne consomme aucune arete et n'atteint donc jamais un portail de
            // sortie -- or rien ne retire un vehicule ailleurs qu'a un portail (AD-34) : il resterait
            // en jeu toute la session en encombrant sa zone d'insertion. Un echec a la fois silencieux
            // et permanent merite le meme traitement que le prefab sans NetworkObject : refus,
            // avertissement une fois, aucune place consommee.
            var controller = instance.GetComponent<NetworkedAIVehicleDriverController>();
            if (controller == null)
            {
                Destroy(instance);
                WarnOnce(ref warnedPrefabWithoutDriverController,
                    "le prefab de vehicule de trafic ne porte pas de NetworkedAIVehicleDriverController : aucune insertion possible.");
                return false;
            }

            // Reference de scene posee AVANT le spawn : OnNetworkSpawn lit le graphe pour choisir son
            // noeud de depart, et un prefab ne peut pas porter cette reference.
            controller.BindLaneGraph(laneGraph);

            // Spawn host-owned (aucun clientId passe) : gameplay-authoritative NetworkObject, jamais
            // client-owned, conformement au contrat d'autorite reseau (NFR4/NFR6).
            if (!networkObject.IsSpawned)
            {
                networkObject.Spawn();
            }

            liveVehicles.Add(networkObject);
            return true;
        }

        private void WarnOnce(ref bool alreadyWarned, string message)
        {
            if (alreadyWarned)
            {
                return;
            }

            alreadyWarned = true;
            Debug.LogWarning("[Run] Trafic portail-a-portail : " + message, this);
        }
    }
}
