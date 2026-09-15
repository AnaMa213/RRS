using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using RoadRage.App.Run;
using RoadRage.Features.PassengerActions;
using RoadRage.Features.Rage;
using RoadRage.Features.Vehicles;
using RoadRage.Shared.Domain;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.5 : lock de cible IA rage/peur per-player, resolution partagee entre provocations
    /// passager et klaxon (AiRageTargetResolution), validation hote (type/portee/acteur) et
    /// isolation du lock entre joueurs. Deterministe et sans Netcode : les fixtures ne sont jamais
    /// spawnees (meme convention que Story54RageDrivenAiBehaviorStatesTests) -- les fonctions
    /// testees directement (ResolveNearestEligible/ResolveActionTarget/ResolveTarget/
    /// ResolveNextInCycle) gardent requireSpawned=false par defaut ; le test de rejet hote l'active
    /// explicitement pour couvrir la contrainte "spawne" sans demarrer de session Netcode.
    /// </summary>
    public sealed class Story55NetworkedAiRageTargetingTests
    {
        private const string DriverControllerSourcePath = "Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs";
        private const string RageTuningPath = "Assets/RoadRage/ScriptableObjects/Rage/RageTuningDef_Default.asset";

        private readonly List<UnityEngine.Object> spawned = new List<UnityEngine.Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var instance in spawned)
            {
                if (instance != null)
                {
                    UnityEngine.Object.DestroyImmediate(instance);
                }
            }

            spawned.Clear();
        }

        // ---------------------------------------------------------------- Resolution partagee (pure)

        [Test]
        public void ResolveNearestEligiblePicksTheClosestCandidate()
        {
            var near = NewEligibleVehicle("Near", new Vector3(3f, 0f, 0f));
            var far = NewEligibleVehicle("Far", new Vector3(10f, 0f, 0f));

            var resolved = AiRageTargetResolution.ResolveNearestEligible(Vector3.zero, new[] { far, near }, 50f);

            Assert.That(resolved, Is.EqualTo(near));
        }

        [Test]
        public void ResolveNearestEligibleIgnoresIneligibleCandidates()
        {
            var aiWithoutRage = NewAiVehicleWithoutRageSource("AiWithoutRage", new Vector3(1f, 0f, 0f));
            var eligible = NewEligibleVehicle("Eligible", new Vector3(10f, 0f, 0f));

            var resolved = AiRageTargetResolution.ResolveNearestEligible(Vector3.zero, new[] { aiWithoutRage, eligible }, 50f);

            Assert.That(resolved, Is.EqualTo(eligible), "Un vehicule sans source rage/peur n'est jamais eligible, meme plus proche.");
        }

        [Test]
        public void NoLockResolutionIsBoundedToTheActionsOwnRange()
        {
            var inRange = NewEligibleVehicle("InRange", new Vector3(4f, 0f, 0f));
            var outOfRange = NewEligibleVehicle("OutOfRange", new Vector3(100f, 0f, 0f));

            var resolved = AiRageTargetResolution.ResolveTarget(Vector3.zero, false, null, new[] { inRange, outOfRange }, 5f);

            Assert.That(resolved, Is.EqualTo(inRange));
        }

        [Test]
        public void NoLockResolutionYieldsNothingWhenNoEligibleCandidateIsInRange()
        {
            var tooFar = NewEligibleVehicle("TooFar", new Vector3(100f, 0f, 0f));

            var resolved = AiRageTargetResolution.ResolveTarget(Vector3.zero, false, null, new[] { tooFar }, 5f);

            Assert.That(resolved, Is.Null, "Aucune IA eligible dans la portee : l'action n'affecte personne.");
        }

        [Test]
        public void LockedTargetOutOfRangeNeverFallsBackToAnotherCandidate()
        {
            var lockedFarAway = NewEligibleVehicle("LockedFarAway", new Vector3(100f, 0f, 0f));
            var closerAlternative = NewEligibleVehicle("CloserAlternative", new Vector3(2f, 0f, 0f));

            var resolved = AiRageTargetResolution.ResolveTarget(Vector3.zero, true, lockedFarAway, new[] { lockedFarAway, closerAlternative }, 5f);

            Assert.That(resolved, Is.Null, "Lock hors de portee : aucune mutation, jamais de repli sur une autre cible.");
        }

        [Test]
        public void LockedTargetInRangeIsUsedEvenWithACloserAlternative()
        {
            var locked = NewEligibleVehicle("Locked", new Vector3(4f, 0f, 0f));
            var closerAlternative = NewEligibleVehicle("CloserAlternative", new Vector3(1f, 0f, 0f));

            var resolved = AiRageTargetResolution.ResolveTarget(Vector3.zero, true, locked, new[] { locked, closerAlternative }, 5f);

            Assert.That(resolved, Is.EqualTo(locked), "Un lock en portee reste la cible, meme si une autre IA eligible est plus proche.");
        }

        [Test]
        public void SpecifiedButIneligibleLockIsRejectedWithoutFallingBackToNearest()
        {
            var nearbyEligible = NewEligibleVehicle("NearbyEligible", new Vector3(1f, 0f, 0f));

            // lockedCandidate == null modelise une reference client invalide/usurpee (type non-IA ou
            // objet non resolu) : doit refuser, jamais retomber sur la plus proche eligible.
            var resolved = AiRageTargetResolution.ResolveTarget(Vector3.zero, true, null, new[] { nearbyEligible }, 50f);

            Assert.That(resolved, Is.Null);
        }

        [Test]
        public void NetworkResolutionRejectsAnUnspawnedEligibleLock()
        {
            var unspawnedLock = NewEligibleVehicle("UnspawnedLock", new Vector3(1f, 0f, 0f));

            var resolved = AiRageTargetResolution.ResolveTarget(
                Vector3.zero,
                true,
                unspawnedLock,
                new[] { unspawnedLock },
                5f,
                true);

            Assert.That(resolved, Is.Null, "L'hote refuse une cible non spawnee, meme structurellement eligible et en portee.");
        }

        [Test]
        public void IntentCarriesLockPresenceSeparatelyFromItsNetworkReference()
        {
            var intent = new PassengerActionIntent(
                0,
                "test_action",
                1,
                1,
                1UL,
                new NetworkObjectReference((NetworkObject)null),
                true);

            Assert.That(intent.HasTarget, Is.True,
                "Une reference devenue irresolvable doit rester un lock specifie et etre refusee, jamais devenir un fallback sans lock.");
        }

        [Test]
        public void CycleMovesToTheNextEligibleCandidateDeterministically()
        {
            var first = NewEligibleVehicle("First", Vector3.zero);
            var second = NewEligibleVehicle("Second", Vector3.zero);
            var third = NewEligibleVehicle("Third", Vector3.zero);
            var ordered = new[] { first, second, third };

            Assert.That(AiRageTargetResolution.ResolveNextInCycle(null, ordered), Is.EqualTo(first));
            Assert.That(AiRageTargetResolution.ResolveNextInCycle(first, ordered), Is.EqualTo(second));
            Assert.That(AiRageTargetResolution.ResolveNextInCycle(second, ordered), Is.EqualTo(third));
            Assert.That(AiRageTargetResolution.ResolveNextInCycle(third, ordered), Is.EqualTo(first), "Le cycle boucle sur la premiere.");
        }

        [Test]
        public void CycleStaysStableWithOnlyOneEligibleCandidate()
        {
            var onlyOne = NewEligibleVehicle("OnlyOne", Vector3.zero);

            var next = AiRageTargetResolution.ResolveNextInCycle(onlyOne, new[] { onlyOne });

            Assert.That(next, Is.EqualTo(onlyOne), "Une seule IA eligible : le lock reste stable.");
        }

        // ---------------------------------------------------------------- Validation hote (type)

        [Test]
        public void HostRejectsATargetMissingNetworkedAiVehicleState()
        {
            var fixture = NewIntentFixture(NewRageOnlyTarget("NonAiTarget"));

            fixture.Intent.RequestSlot(0);

            Assert.That(fixture.Target.RageValue.Value, Is.Zero, "Cible non-IA : verdict refuse, aucune mutation.");
        }

        [Test]
        public void HostAcceptsAnEligibleAiTarget()
        {
            var fixture = NewIntentFixture(NewEligibleTarget("AiTarget"));

            fixture.Intent.RequestSlot(0);

            Assert.That(fixture.Target.RageValue.Value, Is.EqualTo(25f));
        }

        // ---------------------------------------------------------------- Locks independants

        [Test]
        public void LocksAreIndependentPerPlayerInstance()
        {
            var playerOneVehicle = NewEligibleVehicle("PlayerOneLock", Vector3.zero);
            var playerTwoVehicle = NewEligibleVehicle("PlayerTwoLock", Vector3.zero);
            var playerOneFlow = NewObject("PlayerOneFlow").AddComponent<RunFlowController>();
            var playerTwoFlow = NewObject("PlayerTwoFlow").AddComponent<RunFlowController>();

            SetPrivateField(playerOneFlow, "rageTargetLock", playerOneVehicle);
            SetPrivateField(playerOneFlow, "hasRageTargetLock", true);
            SetPrivateField(playerTwoFlow, "rageTargetLock", playerTwoVehicle);
            SetPrivateField(playerTwoFlow, "hasRageTargetLock", true);

            Assert.That(GetPrivateField<NetworkedAIVehicleState>(playerOneFlow, "rageTargetLock"), Is.EqualTo(playerOneVehicle));
            Assert.That(GetPrivateField<bool>(playerOneFlow, "hasRageTargetLock"), Is.True);
            Assert.That(GetPrivateField<NetworkedAIVehicleState>(playerTwoFlow, "rageTargetLock"), Is.EqualTo(playerTwoVehicle),
                "Changer le lock d'un joueur ne doit jamais affecter celui d'un autre : champ d'instance, jamais partage.");
            Assert.That(GetPrivateField<bool>(playerTwoFlow, "hasRageTargetLock"), Is.True);
        }

        // ---------------------------------------------------------------- Auto-clear du lock stale (code review 2026-09-15)

        /// <summary>
        /// Bug fix (hors Story 5.5, code review du 2026-09-15) : sans revalidation, hasRageTargetLock
        /// restait vrai apres destruction/despawn de l'IA verrouillee (rageTargetLock devient un
        /// fake-null Unity), donc targetSpecified restait vrai avec Target null -- provocations et
        /// klaxon refuses silencieusement, sans jamais s'auto-corriger.
        /// </summary>
        [Test]
        public void RevalidateClearsTheLockWhenTheLockedVehicleIsDestroyed()
        {
            var flow = NewObject("Flow").AddComponent<RunFlowController>();
            var locked = NewEligibleVehicle("LockedThenDestroyed", Vector3.zero);
            SetPrivateField(flow, "rageTargetLock", locked);
            SetPrivateField(flow, "hasRageTargetLock", true);

            spawned.Remove(locked);
            UnityEngine.Object.DestroyImmediate(locked.gameObject);

            InvokeRevalidateRageTargetLock(flow);

            Assert.That(GetPrivateField<bool>(flow, "hasRageTargetLock"), Is.False,
                "Le lock doit s'auto-effacer quand sa cible est detruite, sinon les actions restent refusees indefiniment.");
        }

        [Test]
        public void RevalidateLeavesAStillAliveLockUntouched()
        {
            var flow = NewObject("Flow").AddComponent<RunFlowController>();
            var locked = NewEligibleVehicle("StillAlive", Vector3.zero);
            SetPrivateField(flow, "rageTargetLock", locked);
            SetPrivateField(flow, "hasRageTargetLock", true);

            InvokeRevalidateRageTargetLock(flow);

            Assert.That(GetPrivateField<bool>(flow, "hasRageTargetLock"), Is.True,
                "Un lock toujours valide ne doit jamais etre efface.");
            Assert.That(GetPrivateField<NetworkedAIVehicleState>(flow, "rageTargetLock"), Is.EqualTo(locked));
        }

        private static void InvokeRevalidateRageTargetLock(RunFlowController flow)
        {
            var method = typeof(RunFlowController).GetMethod("RevalidateRageTargetLock", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            method.Invoke(flow, null);
        }

        // ---------------------------------------------------------------- Klaxon (Story 5.5)

        [Test]
        public void HonkStillFiresPresentationEventWithoutAnyNetworkContext()
        {
            var vehicle = NewObject("HonkVehicle");
            vehicle.AddComponent<NetworkObject>();
            var driver = vehicle.AddComponent<NetworkedVehicleDriverController>();
            var honked = 0;
            var resolvedCount = 0;
            driver.VehicleHonked += () => honked++;
            driver.HonkTargetResolved += (_, _) => resolvedCount++;

            driver.RequestHonk();

            Assert.That(honked, Is.EqualTo(1), "Le klaxon reste presentation-only meme sans cible/reseau.");
            Assert.That(resolvedCount, Is.Zero, "Sans spawn reseau, aucune resolution host-authoritative ne peut avoir lieu.");
        }

        [Test]
        public void ConfigureHonkReactionStoresTheAuthoredValues()
        {
            var vehicle = NewObject("HonkVehicleConfigured");
            vehicle.AddComponent<NetworkObject>();
            var driver = vehicle.AddComponent<NetworkedVehicleDriverController>();

            driver.ConfigureHonkReaction(12f, 18f, ReactionChannel.Both);

            Assert.That(GetPrivateField<float>(driver, "honkRange"), Is.EqualTo(12f));
            Assert.That(GetPrivateField<float>(driver, "honkMagnitude"), Is.EqualTo(18f));
            Assert.That(GetPrivateField<ReactionChannel>(driver, "honkChannel"), Is.EqualTo(ReactionChannel.Both));
        }

        [Test]
        public void HonkEffectMutatesOnlyTheResolvedTarget()
        {
            var flow = NewObject("Flow").AddComponent<RunFlowController>();
            SetPrivateField(flow, "passengerActionRageTuning", NewTuning());
            var resolvedTarget = NewEligibleTarget("ResolvedTarget");
            var otherTarget = NewEligibleTarget("OtherTarget");
            var handler = typeof(RunFlowController).GetMethod(
                "HandleHonkTargetResolved",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(handler, Is.Not.Null);
            handler.Invoke(flow, new object[]
            {
                resolvedTarget.GetComponent<NetworkedAIVehicleState>(),
                new NpcReactionEffect(ReactionChannel.Rage, 15f)
            });

            Assert.That(resolvedTarget.RageValue.Value, Is.EqualTo(15f));
            Assert.That(otherTarget.RageValue.Value, Is.Zero, "Le klaxon n'affecte que la cible unique resolue.");
        }

        [Test]
        public void RageTuningDefValidatesAuthoredHonkFields()
        {
            var tuning = ScriptableObject.CreateInstance<RageTuningDef>();
            spawned.Add(tuning);
            SetPrivateField(tuning, "id", "rage_test");
            SetPrivateField(tuning, "maxRageValue", 100f);
            SetPrivateField(tuning, "maxFearValue", 100f);
            SetPrivateField(tuning, "rageSensitivity", 1f);
            SetPrivateField(tuning, "fearSensitivity", 1f);
            SetPrivateField(tuning, "honkRange", 15f);
            SetPrivateField(tuning, "honkMagnitude", 15f);
            SetPrivateField(tuning, "honkChannel", ReactionChannel.Rage);

            Assert.That(tuning.TryValidate(out var error), Is.True, error);
            Assert.That(tuning.HonkRange, Is.EqualTo(15f));
            Assert.That(tuning.HonkMagnitude, Is.EqualTo(15f));
            Assert.That(tuning.HonkChannel, Is.EqualTo(ReactionChannel.Rage));

            SetPrivateField(tuning, "honkRange", 0f);
            Assert.That(tuning.TryValidate(out _), Is.False, "HonkRange doit rester strictement positif.");

            SetPrivateField(tuning, "honkRange", 15f);
            SetPrivateField(tuning, "honkChannel", ReactionChannel.None);
            Assert.That(tuning.TryValidate(out _), Is.False, "Le klaxon doit authorer Rage, Fear ou Both.");
        }

        [Test]
        public void DefaultRageTuningAuthorsTheHonkEffect()
        {
            var tuning = AssetDatabase.LoadAssetAtPath<RageTuningDef>(RageTuningPath);

            Assert.That(tuning, Is.Not.Null);
            Assert.That(tuning.HonkRange, Is.EqualTo(15f));
            Assert.That(tuning.HonkMagnitude, Is.EqualTo(15f));
            Assert.That(tuning.HonkChannel, Is.EqualTo(ReactionChannel.Rage));
        }

        /// <summary>
        /// Meme philosophie que Story54 (BehaviorIsDerivedHostOnlyFromTheVehiclesOwnRage) : le klaxon
        /// host-authoritative ne peut pas etre exerce en EditMode sans session Netcode reelle (IsServer
        /// exige un NetworkManager), donc son cablage est verifie par assertions sur le source --
        /// meme point de resolution partage que les provocations passager, revalidation acteur, et
        /// jamais de mutation directe de la rage (Features/Vehicles ne mute jamais Features/Rage).
        /// </summary>
        [Test]
        public void HonkResolutionReusesTheSharedResolutionPointAndNeverMutatesRageDirectly()
        {
            var source = File.ReadAllText(DriverControllerSourcePath);

            Assert.That(source, Does.Contain("AiRageTargetResolution.ResolveTarget("),
                "Le klaxon doit reutiliser le meme point de resolution que les provocations passager.");
            Assert.That(source, Does.Contain("state.DriverClientId.Value != senderClientId"),
                "Revalidation acteur : le klaxon reste conducteur-only, meme cote hote.");
            Assert.That(source, Does.Contain("lockSpecified"),
                "La presence du lock doit survivre a une reference non resolue pour interdire tout repli implicite.");
            Assert.That(source, Does.Contain("HonkTargetResolved?.Invoke"));
            Assert.That(source, Does.Not.Contain("ApplyReactionEffect"),
                "Features/Vehicles ne mute jamais la rage directement (Epic 5, Technical Decisions) -- App/Run applique l'effet.");
            Assert.That(source, Does.Not.Contain("RageTuningDef"),
                "Le klaxon consomme des valeurs primitives poussees par ConfigureHonkReaction, jamais le type RageTuningDef (Features/Rage).");
        }

        // ---------------------------------------------------------------- Helpers

        private readonly struct IntentFixture
        {
            public IntentFixture(NetworkedPassengerActionIntent intent, NetworkedRageState target)
            {
                Intent = intent;
                Target = target;
            }

            public NetworkedPassengerActionIntent Intent { get; }
            public NetworkedRageState Target { get; }
        }

        private IntentFixture NewIntentFixture(NetworkedRageState target)
        {
            var flow = NewObject("Flow").AddComponent<RunFlowController>();
            SetPrivateField(flow, "passengerActionRageTuning", NewTuning());

            var action = NewAction();
            var catalog = NewCatalog(action);
            var actor = NewObject("Actor");
            actor.AddComponent<NetworkObject>();
            var intent = actor.AddComponent<NetworkedPassengerActionIntent>();

            var handler = (Action<PassengerActionDef, Transform, NetworkedRageState>)Delegate.CreateDelegate(
                typeof(Action<PassengerActionDef, Transform, NetworkedRageState>),
                flow,
                typeof(RunFlowController).GetMethod("HandlePassengerActionValidated", BindingFlags.Instance | BindingFlags.NonPublic));
            intent.ActionValidated += handler;
            intent.Configure(catalog, null, null, target, null, null, () => true, () => actor.transform.position);
            return new IntentFixture(intent, target);
        }

        private PassengerActionDef NewAction()
        {
            var action = ScriptableObject.CreateInstance<PassengerActionDef>();
            spawned.Add(action);
            SetPrivateField(action, "id", "test_action");
            SetPrivateField(action, "displayName", "Test");
            SetPrivateField(action, "slot", 0);
            SetPrivateField(action, "version", 1);
            SetPrivateField(action, "cooldownSeconds", 10f);
            SetPrivateField(action, "maxRange", 5f);
            SetPrivateField(action, "allowedPhases", new[] { RunPhase.NotStarted });
            return action;
        }

        private PassengerActionCatalog NewCatalog(PassengerActionDef action)
        {
            var catalog = ScriptableObject.CreateInstance<PassengerActionCatalog>();
            spawned.Add(catalog);
            SetPrivateField(catalog, "version", 1);
            SetPrivateField(catalog, "actions", new List<PassengerActionDef> { action, NewSlottedAction(1), NewSlottedAction(2) });
            return catalog;
        }

        private PassengerActionDef NewSlottedAction(int slot)
        {
            var action = NewAction();
            SetPrivateField(action, "id", "test_action_" + slot);
            SetPrivateField(action, "slot", slot);
            return action;
        }

        private RageTuningDef NewTuning()
        {
            var tuning = ScriptableObject.CreateInstance<RageTuningDef>();
            spawned.Add(tuning);
            SetPrivateField(tuning, "maxRageValue", 100f);
            SetPrivateField(tuning, "thresholds", new[] { new RageTuningDef.RageThreshold(RageDisposition.Irritated, 25f) });
            return tuning;
        }

        /// <summary>Cible eligible (NetworkedAIVehicleState + NetworkedRageState), au meme endroit que l'acteur -- toujours en portee (maxRange 5).</summary>
        private NetworkedRageState NewEligibleTarget(string name)
        {
            var target = NewObject(name);
            target.AddComponent<NetworkObject>();
            target.AddComponent<NetworkedAIVehicleState>();
            return target.AddComponent<NetworkedRageState>();
        }

        /// <summary>Cible non-IA (NetworkedRageState seul) : structurellement inelligible sous la nouvelle regle Story 5.5.</summary>
        private NetworkedRageState NewRageOnlyTarget(string name)
        {
            var target = NewObject(name);
            target.AddComponent<NetworkObject>();
            return target.AddComponent<NetworkedRageState>();
        }

        private NetworkedAIVehicleState NewEligibleVehicle(string name, Vector3 position)
        {
            var vehicle = NewObject(name);
            vehicle.transform.position = position;
            vehicle.AddComponent<NetworkObject>();
            var state = vehicle.AddComponent<NetworkedAIVehicleState>();
            vehicle.AddComponent<NetworkedRageState>();
            return state;
        }

        /// <summary>NetworkedAIVehicleState sans source rage/peur : structurellement inelligible (Boundaries "jamais un seul des deux").</summary>
        private NetworkedAIVehicleState NewAiVehicleWithoutRageSource(string name, Vector3 position)
        {
            var vehicle = NewObject(name);
            vehicle.transform.position = position;
            vehicle.AddComponent<NetworkObject>();
            return vehicle.AddComponent<NetworkedAIVehicleState>();
        }

        private GameObject NewObject(string name)
        {
            var value = new GameObject(name);
            spawned.Add(value);
            return value;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "field not found: " + fieldName);
            field.SetValue(target, value);
        }

        private static T GetPrivateField<T>(object target, string fieldName)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "field not found: " + fieldName);
            return (T)field.GetValue(target);
        }
    }
}
