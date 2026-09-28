using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using RoadRage.Features.Rage;
using RoadRage.Features.Vehicles;
using RoadRage.Shared.Domain;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.4 : comportements IA pilotes par la rage. Couvre le profil de conduite pur des six
    /// dispositions, la projection host-owned de la rage vers NetworkedAIVehicleState.Behavior
    /// (permissions reseau, repli Calm, independance entre vehicules) et le caractere lecture seule
    /// du label debug. Deterministe et sans Netcode : les NetworkVariables sont lues/ecrites sans
    /// spawn, comme dans Story51NpcRageFearFoundationTests.
    /// </summary>
    [Category("Core")]
    public sealed class Story54RageDrivenAiBehaviorStatesTests
    {
        private const string DriverControllerSourcePath = "Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs";
        private const string DebugViewSourcePath = "Assets/RoadRage/Features/Vehicles/AIVehicleBehaviorDebugView.cs";
        private const string AiVehiclePrefabPath = "Assets/RoadRage/Prefabs/Greybox_AIVehicle.prefab";

        private static readonly RageDisposition[] AllDispositions =
        {
            RageDisposition.Calm,
            RageDisposition.Irritated,
            RageDisposition.Flee,
            RageDisposition.Block,
            RageDisposition.Ram,
            RageDisposition.ConfrontationCapable
        };

        /// <summary>
        /// Profil authore de reference (valeurs de DriverProfileDef_Default.asset) : la Story 5.9 a
        /// remplace le multiplicateur de vitesse par une modulation du profil, donc les proprietes de
        /// la Story 5.4 se verifient desormais sur le profil effectif.
        /// </summary>
        private static readonly DriverProfile BaselineProfile = new DriverProfile(
            desiredSpeed: 8f,
            timeHeadway: 1.5f,
            minimumGap: 2f,
            maxAcceleration: 1.5f,
            comfortableDeceleration: 2f,
            politeness: 0.25f,
            laneChangeThreshold: 0.2f,
            safeBrakingLimit: 4f,
            reactionTime: 0.3f,
            laneChangeEvaluationInterval: 1f,
            consistency: 0.8f);

        private readonly List<Object> spawned = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var instance in spawned)
            {
                if (instance != null)
                {
                    Object.DestroyImmediate(instance);
                }
            }

            spawned.Clear();
        }

        // ---------------------------------------------------------------- Profils de conduite

        [Test]
        public void EveryDispositionResolvesAFiniteDriveProfile()
        {
            foreach (var disposition in AllDispositions)
            {
                var effective = DriverModel.ResolveEffectiveProfile(BaselineProfile, disposition);

                Assert.That(float.IsFinite(effective.DesiredSpeed), Is.True, disposition.ToString());
                Assert.That(effective.DesiredSpeed, Is.GreaterThanOrEqualTo(0f), disposition.ToString());
                Assert.That(float.IsFinite(effective.TimeHeadway), Is.True, disposition.ToString());
                Assert.That(float.IsFinite(effective.MinimumGap), Is.True, disposition.ToString());
                Assert.That(effective.MaxAcceleration, Is.GreaterThan(0f), disposition.ToString());
                Assert.That(effective.ComfortableDeceleration, Is.GreaterThan(0f), disposition.ToString());
                Assert.That(float.IsFinite(effective.Politeness), Is.True, disposition.ToString());
                Assert.That(float.IsFinite(effective.LaneChangeThreshold), Is.True, disposition.ToString());
                Assert.That(float.IsFinite(effective.SafeBrakingLimit), Is.True, disposition.ToString());
            }
        }

        [Test]
        public void RouteFollowingProfilesEscalateWithRage()
        {
            var calm = DriverModel.ResolveEffectiveProfile(BaselineProfile, RageDisposition.Calm);
            var irritated = DriverModel.ResolveEffectiveProfile(BaselineProfile, RageDisposition.Irritated);
            var flee = DriverModel.ResolveEffectiveProfile(BaselineProfile, RageDisposition.Flee);
            var ram = DriverModel.ResolveEffectiveProfile(BaselineProfile, RageDisposition.Ram);

            Assert.That(calm.DesiredSpeed, Is.EqualTo(BaselineProfile.DesiredSpeed).Within(0.0001f),
                "Calm est le profil de reference : le profil authore n'est pas modifie.");
            Assert.That(irritated.DesiredSpeed, Is.GreaterThan(calm.DesiredSpeed));
            Assert.That(flee.DesiredSpeed, Is.GreaterThan(irritated.DesiredSpeed));
            Assert.That(ram.DesiredSpeed, Is.GreaterThan(flee.DesiredSpeed), "L'escalade doit rester lisible au mouvement seul.");

            // Story 5.9 : l'escalade ne se resume plus a la vitesse -- la rage change aussi la
            // maniere de conduire (carte des leviers de la recherche).
            Assert.That(ram.TimeHeadway, Is.LessThan(calm.TimeHeadway), "Rage : colle au pare-chocs.");
            Assert.That(ram.MinimumGap, Is.LessThan(calm.MinimumGap));
            Assert.That(ram.MaxAcceleration, Is.GreaterThan(calm.MaxAcceleration));
            Assert.That(ram.ComfortableDeceleration, Is.GreaterThan(calm.ComfortableDeceleration));
            Assert.That(ram.Politeness, Is.LessThan(calm.Politeness));
            Assert.That(ram.LaneChangeThreshold, Is.LessThan(calm.LaneChangeThreshold));
            Assert.That(ram.SafeBrakingLimit, Is.GreaterThan(calm.SafeBrakingLimit));
        }

        [Test]
        public void BlockAndConfrontationCapableStopPursuingTheRoute()
        {
            // Matrice I/O : ces deux etats cessent de poursuivre la route. Leur profil de mouvement est
            // donc volontairement identique ; c'est le label (Behavior synchronise) qui les distingue,
            // pas la vitesse -- aucune cible ni confrontation n'est introduite par cette story.
            foreach (var disposition in AllDispositions)
            {
                var stops = DriverModel.ResolveEffectiveProfile(BaselineProfile, disposition).DesiredSpeed <= 0f;
                var expected = disposition == RageDisposition.Block || disposition == RageDisposition.ConfrontationCapable;
                Assert.That(stops, Is.EqualTo(expected), disposition + " : seuls Block et ConfrontationCapable immobilisent.");
            }
        }

        // ---------------------------------------------------------------- Etat reseau

        [Test]
        public void BehaviorIsServerWriteEveryoneReadAndDefaultsToCalm()
        {
            var state = NewAiVehicleState();

            Assert.That(state.Behavior.Value, Is.EqualTo(RageDisposition.Calm), "Repli Calm avant toute derivation.");
            Assert.That(state.Behavior.WritePerm, Is.EqualTo(NetworkVariableWritePermission.Server),
                "Aucun client ne doit pouvoir forcer un changement de comportement.");
            Assert.That(state.Behavior.ReadPerm, Is.EqualTo(NetworkVariableReadPermission.Everyone),
                "Les clients (y compris ceux qui rejoignent tard) lisent l'etat courant.");
        }

        [Test]
        public void RageStateExposesItsDispositionThroughTheNarrowReadOnlySource()
        {
            var rage = NewRageState();
            var tuning = NewStandardTuning();

            Assert.That(((IRageDispositionSource)rage).CurrentDisposition, Is.EqualTo(RageDisposition.Calm));

            rage.ApplyRageDelta(65f, tuning);

            Assert.That(rage.Disposition.Value, Is.EqualTo(RageDisposition.Block));
            Assert.That(((IRageDispositionSource)rage).CurrentDisposition, Is.EqualTo(RageDisposition.Block),
                "La source etroite ne duplique rien : elle projette la NetworkVariable de rage.");
        }

        [Test]
        public void EachVehicleDerivesItsOwnBehaviorWithoutAffectingTheOthers()
        {
            var tuning = NewStandardTuning();
            var first = NewAiVehicleWithRage();
            var second = NewAiVehicleWithRage();

            first.Rage.ApplyRageDelta(85f, tuning);

            // Derivation telle que l'hote l'applique : chaque vehicule ne lit que sa propre rage.
            first.State.Behavior.Value = first.Rage.CurrentDisposition;
            second.State.Behavior.Value = second.Rage.CurrentDisposition;

            Assert.That(first.State.Behavior.Value, Is.EqualTo(RageDisposition.Ram));
            Assert.That(second.State.Behavior.Value, Is.EqualTo(RageDisposition.Calm),
                "Pas de jauge globale : la rage d'un vehicule ne deplace pas celle d'un autre.");
            Assert.That(second.Rage.RageValue.Value, Is.EqualTo(0f));
            Assert.That(
                DriverModel.ResolveEffectiveProfile(BaselineProfile, first.State.Behavior.Value).DesiredSpeed,
                Is.Not.EqualTo(DriverModel.ResolveEffectiveProfile(BaselineProfile, second.State.Behavior.Value).DesiredSpeed),
                "Deux comportements distincts doivent produire deux profils de conduite distincts.");
        }

        [Test]
        public void MissingRageComponentLeavesTheVehicleOnTheCalmFallback()
        {
            var vehicle = new GameObject("Story54AiVehicleWithoutRage");
            vehicle.AddComponent<NetworkObject>();
            var state = vehicle.AddComponent<NetworkedAIVehicleState>();
            spawned.Add(vehicle);

            Assert.That(vehicle.GetComponent<IRageDispositionSource>(), Is.Null,
                "Cas de la matrice I/O : etat de rage absent.");
            Assert.That(state.Behavior.Value, Is.EqualTo(RageDisposition.Calm));

            var source = File.ReadAllText(DriverControllerSourcePath);
            Assert.That(source, Does.Contain("rageSource == null ? RageDisposition.Calm : rageSource.CurrentDisposition"),
                "Composant de rage absent : repli Calm, sans exception.");
            Assert.That(
                DriverModel.ResolveEffectiveProfile(BaselineProfile, RageDisposition.Calm).DesiredSpeed,
                Is.EqualTo(BaselineProfile.DesiredSpeed).Within(0.0001f),
                "Le repli laisse le vehicule suivre sa route normalement.");
        }

        // ---------------------------------------------------------------- Autorite et presentation

        [Test]
        public void BehaviorIsDerivedHostOnlyFromTheVehiclesOwnRage()
        {
            var source = File.ReadAllText(DriverControllerSourcePath);

            Assert.That(source, Does.Contain("if (!IsServer"),
                "La derivation vit dans la boucle host-only existante.");
            Assert.That(source, Does.Contain("GetComponent<IRageDispositionSource>()"),
                "Sa propre rage uniquement : jamais une recherche de scene ni une autre cible.");
            Assert.That(source, Does.Not.Contain("FindAnyObjectByType"));
            Assert.That(source, Does.Not.Contain("FindObjectsByType"));
            Assert.That(source, Does.Not.Contain("[Rpc"), "Aucune RPC : le comportement passe par la NetworkVariable server-write.");
            Assert.That(source, Does.Not.Contain("RageValue"), "Features/Vehicles consomme la rage et ne la mute jamais.");
            Assert.That(source, Does.Not.Contain("ApplyRageDelta"));
        }

        [Test]
        public void BehaviorDebugViewRendersTextOnlyAndNeverWritesSharedState()
        {
            var source = File.ReadAllText(DebugViewSourcePath);

            Assert.That(source, Does.Contain("Behavior.Value"), "La vue lit l'etat synchronise.");
            Assert.That(source, Does.Not.Contain(".Value ="), "Une vue n'ecrit jamais dans une NetworkVariable.");
            Assert.That(source, Does.Not.Contain("Rpc"));
            Assert.That(source, Does.Not.Contain(".color"), "L'etat ne doit pas reposer sur la couleur seule.");
            Assert.That(source, Does.Contain("rage absente"), "L'absence d'etat de rage reste explicite dans le label.");
        }

        [Test]
        public void AiVehiclePrefabCarriesRageBehaviorAndItsDebugLabelTogether()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AiVehiclePrefabPath);
            Assert.That(prefab != null, Is.True, "prefab introuvable : " + AiVehiclePrefabPath);

            Assert.That(prefab.GetComponent<NetworkedAIVehicleState>() != null, Is.True);
            Assert.That(prefab.GetComponent<NetworkedAIVehicleDriverController>() != null, Is.True);
            Assert.That(prefab.GetComponent<NetworkedRageState>() != null, Is.True,
                "Chaque vehicule IA de MVP_Run porte sa propre rage, sinon son comportement ne peut pas changer.");
            Assert.That(prefab.GetComponent<IRageDispositionSource>(), Is.Not.Null,
                "Le controleur resout la rage par cette interface.");
            Assert.That(prefab.GetComponent<AIVehicleBehaviorDebugView>() != null, Is.True,
                "Le label debug vient du prefab : les trois instances de MVP_Run l'obtiennent sans cablage par instance.");
        }

        // ---------------------------------------------------------------- Helpers

        private sealed class AiVehicleFixture
        {
            public NetworkedAIVehicleState State;
            public NetworkedRageState Rage;
        }

        private AiVehicleFixture NewAiVehicleWithRage()
        {
            var vehicle = new GameObject("Story54AiVehicle");
            vehicle.AddComponent<NetworkObject>();
            var fixture = new AiVehicleFixture
            {
                State = vehicle.AddComponent<NetworkedAIVehicleState>(),
                Rage = vehicle.AddComponent<NetworkedRageState>()
            };

            spawned.Add(vehicle);
            return fixture;
        }

        private NetworkedAIVehicleState NewAiVehicleState()
        {
            var root = new GameObject("Story54AiVehicleState");
            root.AddComponent<NetworkObject>();
            var state = root.AddComponent<NetworkedAIVehicleState>();
            spawned.Add(root);
            return state;
        }

        private NetworkedRageState NewRageState()
        {
            var root = new GameObject("Story54RageTarget");
            root.AddComponent<NetworkObject>();
            var state = root.AddComponent<NetworkedRageState>();
            spawned.Add(root);
            return state;
        }

        /// <summary>Gabarit de paliers repris de RageTuningDef_Default.asset (meme convention que Story 5.1).</summary>
        private RageTuningDef NewStandardTuning()
        {
            var tuning = ScriptableObject.CreateInstance<RageTuningDef>();
            SetPrivateField(tuning, "id", "rage_default");
            SetPrivateField(tuning, "maxRageValue", 100f);
            SetPrivateField(tuning, "maxFearValue", 100f);
            SetPrivateField(tuning, "rageSensitivity", 1f);
            SetPrivateField(tuning, "fearSensitivity", 1f);
            SetPrivateField(tuning, "thresholds", new[]
            {
                new RageTuningDef.RageThreshold(RageDisposition.Irritated, 20f),
                new RageTuningDef.RageThreshold(RageDisposition.Flee, 40f),
                new RageTuningDef.RageThreshold(RageDisposition.Block, 60f),
                new RageTuningDef.RageThreshold(RageDisposition.Ram, 80f),
                new RageTuningDef.RageThreshold(RageDisposition.ConfrontationCapable, 95f)
            });

            spawned.Add(tuning);
            return tuning;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "field not found: " + fieldName);
            field.SetValue(target, value);
        }
    }
}
