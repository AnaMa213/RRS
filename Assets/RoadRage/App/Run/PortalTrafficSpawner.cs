using System.Collections;
using System.Collections.Generic;
using RoadRage.Features.Online;
using RoadRage.Features.Run;
using RoadRage.Features.Vehicles;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Lifecycle;
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

        [SerializeField]
        [Tooltip("Story 5.31 : prefab reseau du vehicule Traffic V2 (aucun type V1). Utilise seulement quand la session demande la composition V2Slice.")]
        private GameObject v2VehiclePrefab;

        /// <summary>Vehicules inseres par ce service, host-owned. Jamais partagee, jamais statique.</summary>
        private readonly List<NetworkObject> liveVehicles = new List<NetworkObject>();

        private bool compositionFrozen;
        private TrafficComposition composition = TrafficComposition.V1;
        private MeasurementRun measurement;
        private TrafficV2Scenario scenario;
        private bool warnedCompositionChange;
        private bool warnedV2Refusal;
        private TrafficV2Admission v2Admission;
        private int v2InsertionCounter;
        private int v2NextTriplet;
        private int v2NextScenarioInsertion;
        private int v2EntryCursor;

        /// <summary>Story 5.33 : ordonnanceur hote, une seule frame partagee par pas pour tous les vehicules V2.</summary>
        private readonly TrafficV2StepRunner v2Runner = new TrafficV2StepRunner();
        private readonly List<TrafficV2VehicleDriver> v2Drivers = new List<TrafficV2VehicleDriver>();
        private readonly List<ScenarioInsertionRecord> scenarioInsertions = new List<ScenarioInsertionRecord>();

        /// <summary>Story 5.33 : ordonnanceur V2 de la session (FrameId global, frame, collecteur).</summary>
        public TrafficV2StepRunner V2Runner
        {
            get { return v2Runner; }
        }

        /// <summary>Story 5.33 : insertions effectives du scenario de test, avec leur FrameId reel d'insertion.</summary>
        public IReadOnlyList<ScenarioInsertionRecord> ScenarioInsertions
        {
            get { return scenarioInsertions; }
        }

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

        /// <summary>Story 5.31 : composition figee de la session (V1 tant qu'elle n'a pas ete lue).</summary>
        public TrafficComposition Composition
        {
            get { return composition; }
        }

        /// <summary>Story 5.31 : vrai des que la composition a ete lue et figee.</summary>
        public bool CompositionFrozen
        {
            get { return compositionFrozen; }
        }

        /// <summary>Story 5.31 : dernier code publie par la branche V2 (Allowed tant qu'aucun refus).</summary>
        public TrafficV2Code V2LastCode { get; private set; }

        public int LiveV2Population
        {
            get { return LiveV2Vehicles.Count; }
        }

        public int V2Insertions
        {
            get { return v2InsertionCounter; }
        }

        public int V2Removals { get; private set; }

        /// <summary>Vehicules V2 vivants : ils partagent la liste et l'unique chemin de retrait du trafic (lecture seule).</summary>
        public IReadOnlyList<NetworkObject> LiveV2Vehicles
        {
            get
            {
                var result = new List<NetworkObject>();
                foreach (var networkObject in liveVehicles)
                {
                    if (networkObject != null && networkObject.GetComponent<TrafficV2VehicleDriver>() != null)
                    {
                        result.Add(networkObject);
                    }
                }

                return result;
            }
        }

        /// <summary>Drivers V2 retires a un portail de sortie, conserves pour la trace de mesure.</summary>
        public readonly List<V2DriveRecord> RetiredV2Runs = new List<V2DriveRecord>();

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

            ResolveCompositionOnce();
            if (composition == TrafficComposition.V2Slice)
            {
                // Story 5.33 : retrait, puis frame et pas de chaque vehicule par TrafficId croissant, puis insertion.
                // Un vehicule insere conduit au pas suivant.
                StepV2Vehicles();
                TickV2Slice();
                return;
            }

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
                var v2Driver = controller == null ? networkObject.GetComponent<TrafficV2VehicleDriver>() : null;
                if ((controller == null || !controller.HasReachedExitPortal) && (v2Driver == null || !v2Driver.HasReachedExitPortal))
                {
                    continue;
                }

                liveVehicles.RemoveAt(i);

                // Story 5.31 : un vehicule V2 sort par ce meme et unique chemin, au portail de sortie.
                if (v2Driver != null)
                {
                    RetiredV2Runs.Add(v2Driver.CaptureRecord());
                    V2Removals++;
                }

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

        /// <summary>
        /// Story 5.31 (A4) : la composition de la session est lue une fois, avant la premiere insertion,
        /// puis figee. Un changement ulterieur est ignore avec un diagnostic.
        /// </summary>
        private void ResolveCompositionOnce()
        {
            if (!compositionFrozen)
            {
                composition = TrafficV2Session.Composition;
                measurement = TrafficV2Session.Measurement;
                scenario = TrafficV2Session.Scenario;
                compositionFrozen = true;
                return;
            }

            if (TrafficV2Session.Composition != composition || TrafficV2Session.Measurement != measurement
                || TrafficV2Session.Scenario != scenario)
            {
                WarnOnce(ref warnedCompositionChange, "composition de trafic changee apres la premiere lecture : changement ignore, la session reste en "
                    + composition + ".");
            }
        }

        /// <summary>
        /// Story 5.33 : une seule frame partagee par pas hote, construite avant tout pas de conduite, puis exactement
        /// un pas par vehicule V2 lie, par TrafficId croissant. Le FrameId global avance a chaque pas V2Slice.
        /// </summary>
        private void StepV2Vehicles()
        {
            v2Drivers.Clear();
            foreach (var networkObject in liveVehicles)
            {
                var driver = networkObject != null ? networkObject.GetComponent<TrafficV2VehicleDriver>() : null;
                if (driver != null)
                {
                    v2Drivers.Add(driver);
                }
            }

            v2Runner.Step(v2Admission != null ? v2Admission.Model : null, v2Drivers);
        }

        /// <summary>
        /// Story 5.31 : branche V2Slice. Aucun vehicule V1, au plus un vehicule V2 vivant, insertions
        /// successives a un portail d'entree libre, retrait uniquement au portail de sortie. Hors run de
        /// mesure, un vehicule V2 n'entre que si sa couverture est etablie : sinon un code nomme est publie.
        /// Story 5.33 : seul un scenario de test leve la population, jusqu'a son maximum ; il n'accorde aucune
        /// permission et il est exclusif avec un run de mesure.
        /// </summary>
        private void TickV2Slice()
        {
            if (scenario != null && measurement != null)
            {
                RefuseV2(TrafficV2Code.ScenarioWithMeasurement, "scenario de test et run de mesure demandes ensemble : aucun vehicule V2 insere.");
                return;
            }

            // Le retrait a deja eu lieu dans ReleaseVehiclesAtExitPortals, seul chemin de despawn du trafic.
            if (LiveV2Population >= (scenario != null ? scenario.MaxPopulation : TrafficV2Settings.V2SliceMaxPopulation))
            {
                return;
            }

            if (measurement != null && v2NextTriplet >= measurement.Triplets.Count)
            {
                V2LastCode = TrafficV2Code.CampaignCompleted;
                return;
            }

            if (scenario != null && v2NextScenarioInsertion >= scenario.Insertions.Count)
            {
                V2LastCode = TrafficV2Code.ScenarioCompleted;
                return;
            }

            // Une fois par spawner : relire le modele (~5 Mo) a chaque pas serait trop couteux.
            // L'admission compare la preuve aux entrees de faisabilite du prefab effectivement insere.
            if (v2Admission == null)
            {
                v2Admission = TrafficV2Lifecycle.AdmitCommittedArtifacts(v2VehiclePrefab);
            }

            var verdict = TrafficV2Lifecycle.EvaluateInsertion(v2Admission, measurement, TrafficV2Settings.DeclaredTrackingTolerance);
            if (!verdict.Allowed)
            {
                RefuseV2(verdict.Code);
                return;
            }

            var prefabDriver = v2VehiclePrefab != null ? v2VehiclePrefab.GetComponent<TrafficV2VehicleDriver>() : null;
            if (prefabDriver == null || v2VehiclePrefab.GetComponent<NetworkObject>() == null)
            {
                RefuseV2(TrafficV2Code.RoadModelMissing, "aucun prefab V2 reseau avec TrafficV2VehicleDriver : aucun vehicule V2 n'est insere.");
                return;
            }

            if (prefabDriver.DriverProfileDefinition == null)
            {
                RefuseV2(TrafficV2Code.DriverProfileMissing);
                return;
            }

            RoadId entryId;
            RoadId exitId;
            ulong seed;
            // Objectif intermediaire (contrat §4) : pose seulement ici, sous run de mesure, depuis le triplet.
            RoadId viaMovementId = RoadId.None;
            if (measurement != null)
            {
                var triplet = measurement.Triplets[v2NextTriplet];
                entryId = triplet.EntryPortalId;
                exitId = triplet.ExitPortalId;
                seed = triplet.Seed;
                viaMovementId = triplet.ViaMovementId;
            }
            else if (scenario != null)
            {
                // Calendrier du scenario : l'insertion attend son pas au plus tot, puis un portail libre.
                var next = scenario.Insertions[v2NextScenarioInsertion];
                if (v2Runner.FrameId < next.EarliestStep)
                {
                    return;
                }

                entryId = next.EntryPortalId;
                exitId = next.ExitPortalId;
                seed = next.Seed;
            }
            else
            {
                entryId = NextEntryPortal(v2Admission.Model);
                exitId = RoadId.None;
                if (runState == null)
                {
                    runState = FindAnyObjectByType<NetworkedRunState>();
                }

                seed = runState != null ? unchecked((ulong)runState.SessionSeed.Value) : 0UL;
            }

            var prepared = TrafficV2Lifecycle.PrepareInsertion(v2Admission, entryId, exitId, seed,
                (ulong)(v2InsertionCounter + 1), prefabDriver.DriverProfileDefinition.Profile, Time.fixedDeltaTime,
                viaMovementId);
            if (prepared.Code != TrafficV2Code.Allowed)
            {
                RefuseV2(prepared.Code);
                return;
            }

            var clearance = laneGraph != null && laneGraph.TrafficSettings != null ? laneGraph.TrafficSettings.PortalClearanceRadius : 4f;
            if (!IsPositionClear(prepared.Position, clearance))
            {
                return;
            }

            var instance = Instantiate(v2VehiclePrefab, prepared.Position, prepared.Rotation);

            // Pose d'insertion, avant tout pas physique et avant le spawn : le chassis a la hauteur de caisse
            // statique au-dessus du point de reference (meme hauteur que les poses canoniques de la Gate A),
            // pour ne pas naitre dans la marge de contact de la chaussee. Aucune ecriture apres l'insertion.
            var insertedBody = instance.GetComponent<VehiclePhysicsBody>();
            if (insertedBody == null || !insertedBody.HasProfile)
            {
                RefuseV2(TrafficV2Code.VehicleProfileMissing);
                Destroy(instance);
                return;
            }
            var rideHeight = insertedBody.Profile.ResolveStaticRideHeight(Mathf.Abs(Physics.gravity.y));
            instance.transform.SetPositionAndRotation(prepared.Position + (prepared.Rotation * Vector3.up) * rideHeight, prepared.Rotation);

            v2InsertionCounter++;
            instance.name = "AI_VehicleV2_Portal_" + v2InsertionCounter.ToString("D3");
            instance.GetComponent<TrafficV2VehicleDriver>().Bind(v2Admission, prepared, verdict, measurement != null || scenario != null);
            var spawned = instance.GetComponent<NetworkObject>();
            if (!spawned.IsSpawned)
            {
                spawned.Spawn();
            }

            liveVehicles.Add(spawned);
            V2LastCode = TrafficV2Code.Allowed;
            if (measurement != null)
            {
                v2NextTriplet++;
            }

            if (scenario != null)
            {
                // Pas reel d'insertion publie ; le vehicule conduit au pas hote suivant.
                scenarioInsertions.Add(new ScenarioInsertionRecord(v2NextScenarioInsertion, prepared.TrafficId, v2Runner.FrameId));
                v2NextScenarioInsertion++;
            }
        }

        private int NextEntryIndex(int count)
        {
            return count == 0 ? -1 : v2EntryCursor++ % count;
        }

        private RoadId NextEntryPortal(CompiledRoadModel model)
        {
            var entries = new List<RoadId>();
            for (var i = 0; i < model.Portals.Count; i++)
            {
                if (model.Portals[i].Role == PortalRole.Entry)
                {
                    entries.Add(model.Portals[i].Id);
                }
            }

            entries.Sort((a, b) => a.CompareTo(b));
            var index = NextEntryIndex(entries.Count);
            return index < 0 ? RoadId.None : entries[index];
        }

        private void RefuseV2(TrafficV2Code code, string message = null)
        {
            V2LastCode = code;
            WarnOnce(ref warnedV2Refusal, message ?? ("aucun vehicule V2 insere : " + code + "."));
        }

        private bool IsPositionClear(Vector3 origin, float clearanceRadius)
        {
            var found = Physics.OverlapSphereNonAlloc(
                origin, clearanceRadius, clearanceHits, ~0, QueryTriggerInteraction.Ignore);
            if (found == clearanceHits.Length) return false;

            for (var i = 0; i < found; i++)
            {
                var candidate = clearanceHits[i];
                if (candidate != null
                    && (candidate.attachedRigidbody != null || candidate.GetComponentInParent<CharacterController>() != null))
                {
                    return false;
                }
            }

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
