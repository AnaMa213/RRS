using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using RoadRage.App.Run;
using RoadRage.Features.PassengerActions;
using RoadRage.Features.Rage;
using RoadRage.Features.Vehicles;
using RoadRage.Shared.Domain;
using Unity.Netcode;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    public sealed class Story43PassengerActionOneChangesRageTests
    {
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
        public void AcceptedSlotZeroAppliesRageOnce()
        {
            var fixture = NewFixture();
            fixture.Intent.RequestSlot(0);

            Assert.That(fixture.Target.RageValue.Value, Is.EqualTo(25f));
            Assert.That(fixture.Target.Disposition.Value, Is.EqualTo(RageDisposition.Irritated));
        }

        [Test]
        public void RejectedSlotZeroDoesNotApplyRage()
        {
            var fixture = NewFixture(() => new Vector3(100f, 0f, 0f));
            fixture.Intent.RequestSlot(0);

            Assert.That(fixture.Target.RageValue.Value, Is.Zero);
        }

        [Test]
        public void CooldownReplayDoesNotApplyRageTwice()
        {
            var fixture = NewFixture();
            fixture.Intent.RequestSlot(0);
            fixture.Intent.RequestSlot(0);

            Assert.That(fixture.Target.RageValue.Value, Is.EqualTo(25f));
        }

        [Test]
        public void CycleFocusedRageTargetChangesNextIntentTarget()
        {
            var flow = NewObject("Flow").AddComponent<RunFlowController>();
            var first = NewTarget("First");
            var second = NewTarget("Second");
            Set(flow, "passengerActionTarget", first);

            flow.CycleFocusedRageTarget();

            Assert.That(Read<NetworkedRageState>(flow, "passengerActionTarget"), Is.EqualTo(second));
        }

        private Fixture NewFixture(Func<Vector3> actorPosition = null)
        {
            var flow = NewObject("Flow").AddComponent<RunFlowController>();
            Set(flow, "passengerActionRageTuning", NewTuning());

            var action = NewAction();
            var catalog = NewCatalog(action);
            var actor = NewObject("Actor");
            actor.AddComponent<NetworkObject>();
            var intent = actor.AddComponent<NetworkedPassengerActionIntent>();
            var target = NewTarget("Target");

            var handler = (Action<PassengerActionDef, Transform, NetworkedRageState>)Delegate.CreateDelegate(
                typeof(Action<PassengerActionDef, Transform, NetworkedRageState>),
                flow,
                typeof(RunFlowController).GetMethod("HandlePassengerActionValidated", BindingFlags.Instance | BindingFlags.NonPublic));
            intent.ActionValidated += handler;
            intent.Configure(catalog, null, null, target, null, null, () => true, actorPosition ?? (() => actor.transform.position));
            return new Fixture(intent, target);
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
            Set(catalog, "version", 1);
            Set(catalog, "actions", new List<PassengerActionDef> { action, NewSlottedAction(1), NewSlottedAction(2) });
            return catalog;
        }

        private PassengerActionDef NewSlottedAction(int slot)
        {
            var action = NewAction();
            Set(action, "id", "test_action_" + slot);
            Set(action, "slot", slot);
            return action;
        }

        private RageTuningDef NewTuning()
        {
            var tuning = ScriptableObject.CreateInstance<RageTuningDef>();
            spawned.Add(tuning);
            Set(tuning, "maxRageValue", 100f);
            Set(tuning, "thresholds", new[] { new RageTuningDef.RageThreshold(RageDisposition.Irritated, 25f) });
            return tuning;
        }

        private NetworkedRageState NewTarget(string name)
        {
            var targetObject = NewObject(name);
            targetObject.AddComponent<NetworkObject>();
            return targetObject.AddComponent<NetworkedRageState>();
        }

        private GameObject NewObject(string name)
        {
            var value = new GameObject(name);
            spawned.Add(value);
            return value;
        }

        private static T Read<T>(object target, string field) where T : class
        {
            return target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target) as T;
        }

        private static void Set(object target, string field, object value)
        {
            target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
        }

        private readonly struct Fixture
        {
            public Fixture(NetworkedPassengerActionIntent intent, NetworkedRageState target)
            {
                Intent = intent;
                Target = target;
            }

            public NetworkedPassengerActionIntent Intent { get; }
            public NetworkedRageState Target { get; }
        }
    }
}
