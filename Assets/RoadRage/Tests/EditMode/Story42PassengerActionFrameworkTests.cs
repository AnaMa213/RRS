using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using RoadRage.App.Run;
using RoadRage.Features.PassengerActions;
using RoadRage.Features.Rage;
using RoadRage.Shared.Domain;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    public sealed class Story42PassengerActionFrameworkTests
    {
        private const string CatalogPath = "Assets/RoadRage/ScriptableObjects/PassengerActions/PassengerActionCatalog.asset";
        private const string PlayerPrefabPath = "Assets/RoadRage/Resources/NetworkedPlayerRoot.prefab";
        private readonly List<UnityEngine.Object> spawned = new List<UnityEngine.Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var item in spawned)
            {
                if (item != null) UnityEngine.Object.DestroyImmediate(item);
            }
            spawned.Clear();
        }

        [Test]
        public void AuthoredCatalogHasExactlyThreeStableVersionedSlots()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<PassengerActionCatalog>(CatalogPath);
            Assert.That(catalog, Is.Not.Null);
            Assert.That(catalog.TryValidate(out var error), Is.True, error);
            Assert.That(catalog.Count, Is.EqualTo(PassengerActionCatalog.SlotCount));
            for (var slot = 0; slot < PassengerActionCatalog.SlotCount; slot++)
            {
                var action = catalog.GetAtSlot(slot);
                Assert.That(action, Is.Not.Null);
                Assert.That(action.Slot, Is.EqualTo(slot));
                Assert.That(action.RawId, Is.EqualTo(action.RawId.ToLowerInvariant()));
                Assert.That(action.Version, Is.GreaterThan(0));
            }
        }

        [Test]
        public void ValidIntentIsAcceptedAndEveryInvalidMatrixBranchIsRejected()
        {
            var action = NewAction();
            var catalog = NewCatalog(action);
            var intent = NewIntent(action, catalog);

            AssertCode(catalog, action, intent, Context(), PassengerActionVerdictCode.Accepted);
            AssertCode(catalog, action, intent, Context(senderConnected: false), PassengerActionVerdictCode.SenderDisconnected);
            AssertCode(catalog, action, intent, Context(actorMatches: false), PassengerActionVerdictCode.ActorMismatch);
            AssertCode(catalog, action, intent, Context(lifecycle: PlayerLifecycle.Dead), PassengerActionVerdictCode.ActorNotAlive);

            Set(action, "allowedPhases", Array.Empty<RunPhase>());
            AssertCode(catalog, action, intent, Context(), PassengerActionVerdictCode.InvalidPhase);
            Set(action, "allowedPhases", new[] { RunPhase.NotStarted });

            AssertCode(catalog, action, intent, Context(mode: PlayerMode.Driver), PassengerActionVerdictCode.ActorNotPassenger);
            AssertCode(catalog, action, intent, Context(passengerSeat: false), PassengerActionVerdictCode.SeatMismatch);
            AssertCode(catalog, action, intent, Context(seatMatches: false), PassengerActionVerdictCode.SeatMismatch);
            AssertCode(catalog, action, intent, Context(catalogValid: false), PassengerActionVerdictCode.InvalidCatalog);

            var unknown = intent;
            unknown.ActionId = "unknown";
            AssertCode(catalog, action, unknown, Context(), PassengerActionVerdictCode.InvalidAction);
            var wrongVersion = intent;
            wrongVersion.ActionVersion++;
            AssertCode(catalog, action, wrongVersion, Context(), PassengerActionVerdictCode.InvalidVersion);
            AssertCode(catalog, action, intent, Context(lastSequence: intent.Sequence), PassengerActionVerdictCode.ReplayedSequence);
            AssertCode(catalog, action, intent, Context(cooldownEndsAt: 2d), PassengerActionVerdictCode.CooldownActive);
            AssertCode(catalog, action, intent, Context(targetValid: false), PassengerActionVerdictCode.InvalidTarget);
            AssertCode(catalog, action, intent, Context(distance: action.MaxRange + 0.01f), PassengerActionVerdictCode.TargetOutOfRange);
        }

        [Test]
        public void OfflineEntryPointPublishesOnceThenStartsAuthoritativeCooldown()
        {
            var action = NewAction();
            var catalog = NewCatalog(action);
            var actor = NewObject("Actor");
            actor.AddComponent<NetworkObject>();
            var component = actor.AddComponent<NetworkedPassengerActionIntent>();
            var targetObject = NewObject("Target");
            targetObject.AddComponent<NetworkObject>();
            var target = targetObject.AddComponent<NetworkedRageState>();
            var accepted = 0;
            var lastCode = PassengerActionVerdictCode.Accepted;
            component.ActionValidated += (_, _, _) => accepted++;
            component.VerdictReceived += verdict => lastCode = verdict.Code;
            component.Configure(catalog, null, null, target, null, null, () => true, () => actor.transform.position);

            component.RequestSlot(0);
            component.RequestSlot(0);

            Assert.That(accepted, Is.EqualTo(1));
            Assert.That(lastCode, Is.EqualTo(PassengerActionVerdictCode.CooldownActive));
            Assert.That(target.RageValue.Value, Is.Zero, "Le framework ne doit appliquer aucun effet gameplay.");
        }

        [Test]
        public void PlayerPrefabCarriesHostValidatedIntent()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            Assert.That(prefab, Is.Not.Null);
            Assert.That(prefab.GetComponent<NetworkedPassengerActionIntent>(), Is.Not.Null);
        }

        private PassengerActionDef NewAction()
        {
            var action = ScriptableObject.CreateInstance<PassengerActionDef>();
            spawned.Add(action);
            Set(action, "id", "test_action");
            Set(action, "displayName", "Test");
            Set(action, "slot", 0);
            Set(action, "version", 1);
            Set(action, "cooldownSeconds", 10f);
            Set(action, "maxRange", 5f);
            Set(action, "allowedPhases", new[] { RunPhase.NotStarted });
            return action;
        }

        private PassengerActionCatalog NewCatalog(PassengerActionDef action)
        {
            var catalog = ScriptableObject.CreateInstance<PassengerActionCatalog>();
            spawned.Add(catalog);
            var actions = new List<PassengerActionDef> { action, NewSlottedAction(1), NewSlottedAction(2) };
            Set(catalog, "version", 1);
            Set(catalog, "actions", actions);
            return catalog;
        }

        private PassengerActionDef NewSlottedAction(int slot)
        {
            var action = NewAction();
            Set(action, "id", "test_action_" + slot);
            Set(action, "slot", slot);
            return action;
        }

        private static PassengerActionIntent NewIntent(PassengerActionDef action, PassengerActionCatalog catalog)
        {
            return new PassengerActionIntent(action.Slot, action.RawId, catalog.Version, action.Version, 1UL, default);
        }

        private static PassengerActionValidationContext Context(
            bool senderConnected = true, bool actorMatches = true, PlayerLifecycle lifecycle = PlayerLifecycle.Alive,
            PlayerMode mode = PlayerMode.Passenger, bool passengerSeat = true, bool seatMatches = true,
            bool catalogValid = true, bool targetValid = true, float distance = 1f,
            ulong lastSequence = 0UL, double cooldownEndsAt = 0d)
        {
            return new PassengerActionValidationContext(senderConnected, actorMatches, lifecycle, mode, passengerSeat,
                seatMatches, RunPhase.NotStarted, catalogValid, targetValid, distance, lastSequence, cooldownEndsAt, 1d);
        }

        private static void AssertCode(PassengerActionCatalog catalog, PassengerActionDef action,
            PassengerActionIntent intent, PassengerActionValidationContext context, PassengerActionVerdictCode expected)
        {
            Assert.That(PassengerActionValidation.Validate(catalog, action, intent, context).Code, Is.EqualTo(expected));
        }

        private GameObject NewObject(string name)
        {
            var value = new GameObject(name);
            spawned.Add(value);
            return value;
        }

        private static void Set(object target, string field, object value)
        {
            target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
        }
    }
}
