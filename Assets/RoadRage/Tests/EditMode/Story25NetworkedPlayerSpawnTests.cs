using NUnit.Framework;
using RoadRage.Features.Players;
using RoadRage.Shared.Domain;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Couvre la Story 2.5 : encodage/decodage du profil transporte par NetworkConfig.ConnectionData
    /// et le depot host-only ClientId -> profil, sans dependance Netcode ni Steam.
    /// </summary>
    public sealed class Story25NetworkedPlayerSpawnTests
    {
        [Test]
        public void ConnectionPayloadRoundTripsDisplayNameAndCharacterId()
        {
            var encoded = NetworkPlayerConnectionPayload.Encode("Hote", "char_rookie");

            var decoded = NetworkPlayerConnectionPayload.TryDecode(encoded, out var displayName, out var characterId);

            Assert.That(decoded, Is.True);
            Assert.That(displayName, Is.EqualTo("Hote"));
            Assert.That(characterId, Is.EqualTo("char_rookie"));
        }

        [Test]
        public void ConnectionPayloadRoundTripsEmptyValues()
        {
            var encoded = NetworkPlayerConnectionPayload.Encode(string.Empty, string.Empty);

            var decoded = NetworkPlayerConnectionPayload.TryDecode(encoded, out var displayName, out var characterId);

            Assert.That(decoded, Is.True);
            Assert.That(displayName, Is.Empty);
            Assert.That(characterId, Is.Empty);
        }

        [Test]
        public void TryDecodeFailsOnNullOrEmptyPayload()
        {
            Assert.That(NetworkPlayerConnectionPayload.TryDecode(null, out _, out _), Is.False);
            Assert.That(NetworkPlayerConnectionPayload.TryDecode(new byte[0], out _, out _), Is.False);
        }

        [Test]
        public void TryDecodeFailsOnCorruptPayloadWithoutThrowing()
        {
            var garbage = new byte[] { 0xFF, 0x01, 0x02 };

            Assert.DoesNotThrow(() => NetworkPlayerConnectionPayload.TryDecode(garbage, out _, out _));
            Assert.That(NetworkPlayerConnectionPayload.TryDecode(garbage, out var displayName, out var characterId), Is.False);
            Assert.That(displayName, Is.Empty);
            Assert.That(characterId, Is.Empty);
        }

        [Test]
        public void RegistryReturnsRegisteredProfileForClientId()
        {
            var registry = new NetworkPlayerRegistry();

            registry.Register(7UL, new NetworkPlayerProfile("Invite", "char_rookie"));

            Assert.That(registry.TryGet(7UL, out var profile), Is.True);
            Assert.That(profile.DisplayName, Is.EqualTo("Invite"));
            Assert.That(profile.CharacterId, Is.EqualTo("char_rookie"));
        }

        [Test]
        public void RegistryTryGetFailsForUnknownClientId()
        {
            var registry = new NetworkPlayerRegistry();

            Assert.That(registry.TryGet(42UL, out _), Is.False);
        }

        [Test]
        public void RegistryUnregisterRemovesProfile()
        {
            var registry = new NetworkPlayerRegistry();
            registry.Register(1UL, new NetworkPlayerProfile("Hote", "char_rookie"));

            registry.Unregister(1UL);

            Assert.That(registry.TryGet(1UL, out _), Is.False);
        }

        [Test]
        public void PlayerModeOnFootIsDistinctFromExistingOnFootValues()
        {
            Assert.That(PlayerMode.OnFoot, Is.Not.EqualTo(PlayerMode.OnFootStop));
            Assert.That(PlayerMode.OnFoot, Is.Not.EqualTo(PlayerMode.OnFootRageRoad));
        }
    }
}
