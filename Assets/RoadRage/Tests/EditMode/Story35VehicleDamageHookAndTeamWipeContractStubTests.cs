using System;
using System.IO;
using NUnit.Framework;
using RoadRage.App.Run;
using RoadRage.Features.OnFoot;
using RoadRage.Features.Vehicles;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 3.5 : verification ciblee du contrat de degats vehicule/joueur -- predicats purs
    /// (seuils de degat vehicule cumules, interpolation de degat vehicule selon la vitesse d'impact,
    /// tirage sans repetition des types de degat), contrat host-authoritatif de
    /// NetworkedPlayerLifecycleService (Downed/resurrection/respawn consommable), ejection totale a
    /// HP voiture 0, pont RunFlowController, prefab joueur, et reaffirmation de la frontiere asmdef
    /// du module Vehicules -- meme motif que Story 3.3/3.4.
    /// </summary>
    public sealed class Story35VehicleDamageHookAndTeamWipeContractStubTests
    {
        private const string VehicleAsmdefPath = "Assets/RoadRage/Features/Vehicles/RoadRage.Features.Vehicles.asmdef";
        private const string VehiclePrefabPath = "Assets/RoadRage/Prefabs/Greybox_PlayerCar.prefab";
        private const string PlayerRootPrefabPath = "Assets/RoadRage/Resources/NetworkedPlayerRoot.prefab";
        private const string LifecycleServiceSourcePath = "Assets/RoadRage/App/Run/NetworkedPlayerLifecycleService.cs";
        private const string ReviveIntentSourcePath = "Assets/RoadRage/App/Run/NetworkedPlayerReviveIntent.cs";
        private const string SeatServiceSourcePath = "Assets/RoadRage/App/Run/NetworkedVehicleSeatService.cs";
        private const string RunFlowSourcePath = "Assets/RoadRage/App/Run/RunFlowController.cs";
        private const string HudSourcePath = "Assets/RoadRage/Features/UI/RunCheckpointHudScreen.cs";
        private const string DriverControllerSourcePath = "Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs";
        private const string DamageVfxSourcePath = "Assets/RoadRage/Features/Vehicles/NetworkedVehicleDamageVfxController.cs";
        private const string OnFootSourcePath = "Assets/RoadRage/Features/OnFoot/LocalOnFootController.cs";
        private const string LocalVoidRespawnSourcePath = "Assets/RoadRage/App/Run/LocalVoidRespawnController.cs";

        [Test]
        public void CollisionDamageConstantsMatchIoMatrix()
        {
            Assert.That(NetworkedPlayerLifecycleService.PlayerCollisionDamage, Is.EqualTo(5),
                "matrice I/O : chaque collision reduit l'HP du joueur assis de 5.");
            Assert.That(NetworkedPlayerLifecycleService.ReviveHp, Is.EqualTo(30),
                "matrice I/O : resurrection a temps -> retour Alive a 30 HP.");
        }

        [Test]
        public void ComputeThresholdsCrossedMatchesDesignNoteBreakpoints()
        {
            Assert.That(NetworkedVehicleState.ComputeThresholdsCrossed(100, 100), Is.EqualTo(0));
            Assert.That(NetworkedVehicleState.ComputeThresholdsCrossed(67, 100), Is.EqualTo(1),
                "premier seuil cumule (33 HP perdus) -> 67 HP restants.");
            Assert.That(NetworkedVehicleState.ComputeThresholdsCrossed(34, 100), Is.EqualTo(2),
                "deuxieme seuil cumule (66 HP perdus) -> 34 HP restants.");
            Assert.That(NetworkedVehicleState.ComputeThresholdsCrossed(1, 100), Is.EqualTo(3),
                "troisieme seuil cumule (99 HP perdus) -> 1 HP restant.");
            Assert.That(NetworkedVehicleState.ComputeThresholdsCrossed(0, 100), Is.EqualTo(3),
                "plafonne a MaxDamageTypeCount meme a 0 HP.");
        }

        [Test]
        public void ShuffleDamageTypeOrderProducesFullPermutationWithoutRepetition()
        {
            var order = NetworkedVehicleState.ShuffleDamageTypeOrder((min, max) => min);
            Assert.That(order, Is.EquivalentTo(new[] { 0, 1, 2 }),
                "le tirage doit toujours produire les 3 types une fois chacun, jamais de repetition.");

            var reversedOrder = NetworkedVehicleState.ShuffleDamageTypeOrder((min, max) => max - 1);
            Assert.That(reversedOrder, Is.EquivalentTo(new[] { 0, 1, 2 }));
        }

        [Test]
        public void ComputeCollisionDamageRespectsMinimumThresholdAndInterpolatesToMax()
        {
            Assert.That(NetworkedVehicleDriverController.ComputeCollisionDamage(0f), Is.EqualTo(0),
                "sous le seuil minimal : aucun degat voiture (matrice I/O).");
            Assert.That(NetworkedVehicleDriverController.ComputeCollisionDamage(NetworkedVehicleDriverController.MinCollisionDamageSpeed - 0.01f), Is.EqualTo(0));
            Assert.That(NetworkedVehicleDriverController.ComputeCollisionDamage(NetworkedVehicleDriverController.MinCollisionDamageSpeed), Is.EqualTo(NetworkedVehicleDriverController.MinCollisionDamage));
            Assert.That(NetworkedVehicleDriverController.ComputeCollisionDamage(1000f), Is.EqualTo(NetworkedVehicleDriverController.MaxCollisionDamage),
                "au-dela de la vitesse de reference : plafonne a MaxCollisionDamage.");

            var midDamage = NetworkedVehicleDriverController.ComputeCollisionDamage(
                (NetworkedVehicleDriverController.MinCollisionDamageSpeed + NetworkedVehicleDriverController.ReferenceCollisionDamageSpeed) / 2f);
            Assert.That(midDamage, Is.InRange(NetworkedVehicleDriverController.MinCollisionDamage, NetworkedVehicleDriverController.MaxCollisionDamage));
        }

        [Test]
        public void VehicleStateApplyDamageAssignsOneDamageTypePerThresholdWithoutRepetition()
        {
            var root = new GameObject("VehicleDamageHarness");
            try
            {
                root.AddComponent<NetworkObject>();
                var state = root.AddComponent<NetworkedVehicleState>();

                Assert.That(state.IsInoperable(), Is.False);

                state.ApplyDamage(40);
                Assert.That(state.CurrentHp, Is.EqualTo(60));
                Assert.That(state.IsWheelDamaged, Is.True);
                Assert.That(state.IsEngineDamaged, Is.False);
                Assert.That(state.IsBrakeDamaged, Is.False);

                state.ApplyDamage(40);
                Assert.That(state.CurrentHp, Is.EqualTo(20));
                Assert.That(state.IsEngineDamaged, Is.True, "deuxieme seuil franchi -> deuxieme type de l'ordre, sans repeter le premier.");
                Assert.That(state.IsBrakeDamaged, Is.False);

                state.ApplyDamage(50);
                Assert.That(state.CurrentHp, Is.EqualTo(0), "Hp clampe a 0, jamais negatif.");
                Assert.That(state.IsBrakeDamaged, Is.True);
                Assert.That(state.IsInoperable(), Is.True);

                state.ApplyDamage(10);
                Assert.That(state.CurrentHp, Is.EqualTo(0), "aucun degat supplementaire une fois inoperable.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void LifecycleServiceExposesCollisionDamageReviveAndConsumableRespawnHostOnlyApi()
        {
            var source = File.ReadAllText(LifecycleServiceSourcePath);

            Assert.That(source, Does.Contain("public void ApplyCollisionDamage(ulong clientId, int amount)"));
            Assert.That(source, Does.Contain("public void TryReviveNearestDowned(ulong reviverClientId)"));
            Assert.That(source, Does.Contain("current != PlayerLifecycle.Dead && (current != PlayerLifecycle.Downed || !consumeHeart)"),
                "TryRespawn doit rester accessible depuis Dead (touche manuelle) ou Downed (expiration automatique).");
            Assert.That(source, Does.Contain("ExpireReviveWindowIfNeeded"));
            Assert.That(source, Does.Contain("bestState.Hp.Value = ReviveHp"),
                "la resurrection reussie doit fixer l'HP a ReviveHp, jamais depenser de heart.");
        }

        [Test]
        public void ReviveIntentMirrorsRecoveryIntentClientToHostRpcPatternAndNeverWritesLifecycleDirectly()
        {
            var source = File.ReadAllText(ReviveIntentSourcePath);

            Assert.That(source, Does.Contain("[RequireComponent(typeof(NetworkedPlayerState))]"));
            Assert.That(source, Does.Contain("[Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]"));
            Assert.That(source, Does.Contain("state.ClientId.Value != rpcParams.Receive.SenderClientId"),
                "la RPC doit rester validee contre l'emetteur, comme NetworkedVehicleRecoveryIntent.");
            Assert.That(source, Does.Contain("TryReviveNearestDowned(reviverClientId)"));
            Assert.That(source, Does.Not.Contain("Lifecycle.Value = PlayerLifecycle"),
                "l'intention ne doit jamais ecrire Lifecycle elle-meme (seul le service en a le droit) -- " +
                "distinct de la lecture 'Lifecycle.Value == PlayerLifecycle...' utilisee pour le garde local.");
        }

        [Test]
        public void SeatServiceReleasesDownedOccupantsAndAllOccupantsWhenVehicleInoperable()
        {
            var source = File.ReadAllText(SeatServiceSourcePath);

            Assert.That(source, Does.Contain("lifecycle == PlayerLifecycle.Downed"),
                "ShouldReleaseOccupant doit desormais ejecter aussi un joueur Downed, comme Dead/Disconnected.");
            Assert.That(source, Does.Contain("vehicleState.IsInoperable()"),
                "ReleaseInvalidOccupants doit ejecter tous les occupants quand la voiture atteint 0 HP.");
        }

        [Test]
        public void DriverControllerCollisionEventCarriesImpactSpeedAndReadsDamageFlagsForHandling()
        {
            var source = File.ReadAllText(DriverControllerSourcePath);

            Assert.That(source, Does.Contain("collision.relativeVelocity.magnitude"));
            Assert.That(source, Does.Contain("public event Action<float> VehicleCollided;"));
            Assert.That(source, Does.Contain("public static int ComputeCollisionDamage(float impactSpeed)"));
            Assert.That(source, Does.Contain("ResolveEffectiveMaxForwardSpeed()"));
            Assert.That(source, Does.Contain("ResolveEffectiveSteerDegreesPerSecond()"));
            Assert.That(source, Does.Contain("ResolveEffectiveBrakeDeceleration()"));
            Assert.That(source, Does.Contain("state.IsEngineDamaged"));
            Assert.That(source, Does.Contain("state.IsWheelDamaged"));
            Assert.That(source, Does.Contain("state.IsBrakeDamaged"));
        }

        [Test]
        public void RunFlowBridgesCollisionToPlayerAndVehicleDamageHostAuthoritativeOnly()
        {
            var source = File.ReadAllText(RunFlowSourcePath);

            Assert.That(source, Does.Contain("private void HandleVehicleCollided(float impactSpeed)"));
            Assert.That(source, Does.Contain("ApplyCollisionConsequencesIfAuthoritative"));
            Assert.That(source, Does.Contain("IsAuthoritativeForDamage"));
            Assert.That(source, Does.Contain("NetworkedPlayerLifecycleService.PlayerCollisionDamage"));
            Assert.That(source, Does.Contain("subscribedVehicleState.ApplyDamage(vehicleDamage)"));
            Assert.That(source, Does.Not.Contain("localSoloVehicleState"),
                "Story 5.3 : le solo est toujours host-authoritative, les degats vehicule passent uniquement par ApplyNetworkedCollisionDamage/subscribedVehicleState.");
            Assert.That(source, Does.Contain("localOnFootController.IsDowned = true;"));
            Assert.That(source, Does.Contain("checkpointHud.ShowPlayerDownedMessage();"));
            Assert.That(source, Does.Contain("checkpointHud.ShowVehicleInoperableMessage();"));
            Assert.That(source, Does.Contain("RefreshVehicleDamageHud();"));
        }

        [Test]
        public void HudExposesMinimalDownedAndVehicleStatusFeedbackReusingSharedLabelWithoutNewDurableUi()
        {
            var source = File.ReadAllText(HudSourcePath);

            Assert.That(source, Does.Contain("public void SetHp(int hp, int maxHp)"),
                "la vie joueur visible doit etre un nombre de HP, pas des glyphes de coeurs.");
            Assert.That(source, Does.Not.Contain("public void SetHearts"));
            Assert.That(source, Does.Contain("public void ShowPlayerDownedMessage()"));
            Assert.That(source, Does.Contain("public void ShowPlayerRevivedMessage()"));
            Assert.That(source, Does.Contain("public void ShowVehicleInoperableMessage()"));
            Assert.That(source, Does.Contain("public void SetVehicleDamageStatus"));
            Assert.That(source, Does.Contain("FormatVehicleDamageTypes"));
            Assert.That(source, Does.Contain("ShowVehicleSeatMessage(PlayerDownedMessage)"),
                "reutilise le meme label partage que le siege/collision/recuperation -- pas de nouvelle UI durable.");
        }

        [Test]
        public void VehicleDamageVfxControllerReadsDamageFlagsAndNeverWritesGameplayState()
        {
            var source = File.ReadAllText(DamageVfxSourcePath);

            Assert.That(source, Does.Contain("[RequireComponent(typeof(NetworkedVehicleState))]"));
            Assert.That(source, Does.Contain("ParticleSystem"));
            Assert.That(source, Does.Contain("state.WheelDamaged.OnValueChanged += HandleWheelDamagedChanged"));
            Assert.That(source, Does.Contain("state.EngineDamaged.OnValueChanged += HandleEngineDamagedChanged"));
            Assert.That(source, Does.Contain("state.BrakeDamaged.OnValueChanged += HandleBrakeDamagedChanged"));
            Assert.That(source, Does.Contain("state.LocalDamageStateChanged += RefreshDamageEffects"));
            Assert.That(source, Does.Contain("CreateDefaultEffect(\"Damage_EngineSmoke\""));
            Assert.That(source, Does.Contain("CreateDefaultEffect(\"Damage_BrakeSparks\""));
            Assert.That(source, Does.Not.Contain("WheelDamaged.Value ="));
            Assert.That(source, Does.Not.Contain("EngineDamaged.Value ="));
            Assert.That(source, Does.Not.Contain("BrakeDamaged.Value ="));
            Assert.That(source, Does.Not.Contain("Hp.Value ="));
        }

        [Test]
        public void GreyboxPlayerCarCarriesDamageVfxController()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(VehiclePrefabPath);
            Assert.That(prefab, Is.Not.Null, "Greybox_PlayerCar doit rester chargeable.");

            var vfx = prefab.GetComponent<NetworkedVehicleDamageVfxController>();
            Assert.That(vfx, Is.Not.Null, "Greybox_PlayerCar doit porter le controller VFX de degats visibles.");
        }

        [Test]
        public void LocalOnFootControllerExposesDownedSpeedCapDistinctFromMovementEnabled()
        {
            var root = new GameObject("DownedSpeedCapTest");
            try
            {
                root.AddComponent<CharacterController>();
                var controller = root.AddComponent<LocalOnFootController>();

                Assert.That(controller.IsDowned, Is.False, "par defaut, aucun plafond de vitesse.");
                controller.IsDowned = true;
                Assert.That(controller.IsDowned, Is.True);

                var source = File.ReadAllText(OnFootSourcePath);
                Assert.That(source, Does.Contain("downedSpeedMultiplier"));
                Assert.That(source, Does.Contain("if (IsDowned)"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void LocalVoidRespawnControllerAppliesVehicleCollisionDamageAndDiesAtZeroHp()
        {
            var root = new GameObject("SoloVehicleDamageTest");
            try
            {
                root.AddComponent<CharacterController>();
                root.AddComponent<LocalOnFootController>();
                var controller = root.AddComponent<LocalVoidRespawnController>();

                Assert.That(controller.IsDead, Is.False);

                controller.ApplyVehicleCollisionDamage(60);
                Assert.That(controller.IsDead, Is.False, "60 degats sur 100 HP par defaut : pas encore mort.");

                controller.ApplyVehicleCollisionDamage(60);
                Assert.That(controller.IsDead, Is.True, "cumul au-dela de 100 : le joueur solo doit mourir.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void VehiclesAssemblyStillReferencesNoOtherFeatureSlice()
        {
            var definition = JsonUtility.FromJson<AssemblyDefinition>(File.ReadAllText(VehicleAsmdefPath));
            Assert.That(definition.name, Is.EqualTo("RoadRage.Features.Vehicles"));
            Assert.That(definition.references, Is.Not.Null);

            foreach (var reference in definition.references)
            {
                Assert.That(reference.StartsWith("RoadRage.Features.", StringComparison.Ordinal), Is.False,
                    "RoadRage.Features.Vehicles ne doit toujours referencer aucune autre feature slice : " + reference);
            }
        }

        [Test]
        public void NetworkedPlayerRootCarriesReviveIntentNetworkBehaviour()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerRootPrefabPath);
            Assert.That(prefab, Is.Not.Null, "NetworkedPlayerRoot doit rester chargeable.");

            var intent = prefab.GetComponent<NetworkedPlayerReviveIntent>();
            Assert.That(intent, Is.Not.Null, "NetworkedPlayerRoot doit porter l'intent de resurrection.");
            Assert.That(typeof(NetworkBehaviour).IsAssignableFrom(intent.GetType()), Is.True,
                "L'intent doit etre un NetworkBehaviour pour porter la Rpc client -> host.");
        }

        [Serializable]
        private sealed class AssemblyDefinition
        {
            public string name;
            public string[] references;
        }
    }
}
