using System.IO;
using NUnit.Framework;
using RoadRage.Features.Vehicles;

namespace RoadRage.Tests.EditMode
{
    [Category("Core")]
    public sealed class Story515CredibleCollisionsAndDamageIntegrationTests
    {
        private const string PhysicsBodySourcePath = "Assets/RoadRage/Features/Vehicles/VehiclePhysicsBody.cs";
        private const string DriverControllerSourcePath = "Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs";

        [Test]
        public void MeasuredProfileKeepsTheLeastCostNoTunnelModeAndDamageBreakpoints()
        {
            var physicsSource = File.ReadAllText(PhysicsBodySourcePath);
            Assert.That(physicsSource, Does.Contain("body.collisionDetectionMode = CollisionDetectionMode.Discrete;"));
            Assert.That(NetworkedVehicleDriverController.MinCollisionDamageSpeed, Is.EqualTo(4f));
            Assert.That(NetworkedVehicleDriverController.ReferenceCollisionDamageSpeed, Is.EqualTo(12f));
            Assert.That(NetworkedVehicleDriverController.ComputeCollisionDamage(3.99f), Is.EqualTo(0));
            Assert.That(NetworkedVehicleDriverController.ComputeCollisionDamage(4f), Is.EqualTo(5));
            Assert.That(NetworkedVehicleDriverController.ComputeCollisionDamage(12f), Is.EqualTo(15));
        }

        [Test]
        public void CollisionDamageRemainsHostOnlyAndPreservesExistingFilters()
        {
            var source = File.ReadAllText(DriverControllerSourcePath);
            var collisionStart = source.IndexOf("private void OnCollisionEnter", System.StringComparison.Ordinal);
            var collisionBody = source.Substring(collisionStart, source.IndexOf("private bool IsSurfaceOnlyCollision", collisionStart, System.StringComparison.Ordinal) - collisionStart);
            Assert.That(collisionBody, Does.Contain("if (!IsServer)"));
            Assert.That(collisionBody, Does.Contain("devIndestructible != null && devIndestructible.isActiveAndEnabled"));
            Assert.That(collisionBody, Does.Contain("if (IsSurfaceOnlyCollision(collision))"));
            Assert.That(collisionBody, Does.Contain("collision.relativeVelocity.magnitude"));
        }
    }
}
