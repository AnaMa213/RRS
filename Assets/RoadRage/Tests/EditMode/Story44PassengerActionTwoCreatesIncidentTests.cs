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
    public sealed class Story44PassengerActionTwoCreatesIncidentTests
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
        public void AcceptedSlotOneActivatesIncident()
        {
            var fixture = NewFixture();

            fixture.Intent.RequestSlot(1);

            Assert.That(fixture.Incident.ActivationCount.Value, Is.EqualTo(1));
            Assert.That(fixture.Incident.IsActive, Is.True);
        }

        [Test]
        public void RejectedOrCooldownSlotOneDoesNotRefreshIncident()
        {
            var rejected = NewFixture(() => new Vector3(100f, 0f, 0f));
            rejected.Intent.RequestSlot(1);
            Assert.That(rejected.Incident.ActivationCount.Value, Is.Zero);

            var accepted = NewFixture();
            accepted.Intent.RequestSlot(1);
            accepted.Intent.RequestSlot(1);
            Assert.That(accepted.Incident.ActivationCount.Value, Is.EqualTo(1));
        }

        private Fixture NewFixture(Func<Vector3> actorPosition = null)
        {
            var action = NewAction(1);
            var catalog = ScriptableObject.CreateInstance<PassengerActionCatalog>();
            spawned.Add(catalog);
            Set(catalog, "version", 1);
            Set(catalog, "actions", new List<PassengerActionDef> { NewAction(0), action, NewAction(2) });

            var actor = NewObject("Actor");
            actor.AddComponent<NetworkObject>();
            var intent = actor.AddComponent<NetworkedPassengerActionIntent>();
            var targetObject = NewObject("Target");
            targetObject.AddComponent<NetworkObject>();
            targetObject.AddComponent<NetworkedAIVehicleState>();
            var target = targetObject.AddComponent<NetworkedRageState>();
            var incidentObject = NewObject("Incident");
            incidentObject.AddComponent<NetworkObject>();
            var incident = incidentObject.AddComponent<NetworkedPassengerActionIncidentState>();
            intent.Configure(catalog, null, null, target, null, null, () => true, actorPosition ?? (() => actor.transform.position), incident);
            return new Fixture(intent, incident);
        }

        private PassengerActionDef NewAction(int slot)
        {
            var action = ScriptableObject.CreateInstance<PassengerActionDef>();
            spawned.Add(action);
            Set(action, "id", "test_action_" + slot);
            Set(action, "displayName", "Test");
            Set(action, "slot", slot);
            Set(action, "version", 1);
            Set(action, "cooldownSeconds", 10f);
            Set(action, "maxRange", 5f);
            Set(action, "allowedPhases", new[] { RunPhase.NotStarted });
            return action;
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

        private readonly struct Fixture
        {
            public Fixture(NetworkedPassengerActionIntent intent, NetworkedPassengerActionIncidentState incident)
            {
                Intent = intent;
                Incident = incident;
            }

            public NetworkedPassengerActionIntent Intent { get; }
            public NetworkedPassengerActionIncidentState Incident { get; }
        }
    }
}
