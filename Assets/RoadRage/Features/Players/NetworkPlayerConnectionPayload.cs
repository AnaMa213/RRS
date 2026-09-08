using System;
using System.IO;
using System.Text;

namespace RoadRage.Features.Players
{
    /// <summary>
    /// Encode/decode le profil joueur local (nom affiche + id de personnage) transporte dans
    /// NetworkConfig.ConnectionData avant StartHost/StartClient (Story 2.5). Objet C# pur, sans
    /// dependance Unity ni Netcode : c'est ce que le host lit dans son callback d'approbation de
    /// connexion pour savoir quel personnage instancier pour chaque client, hote inclus.
    /// </summary>
    public static class NetworkPlayerConnectionPayload
    {
        public static byte[] Encode(string displayName, string characterId)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream, Encoding.UTF8))
            {
                writer.Write(displayName ?? string.Empty);
                writer.Write(characterId ?? string.Empty);
                return stream.ToArray();
            }
        }

        /// <summary>Ne leve jamais : un payload absent, tronque ou corrompu rend juste le decodage impossible.</summary>
        public static bool TryDecode(byte[] data, out string displayName, out string characterId)
        {
            displayName = string.Empty;
            characterId = string.Empty;

            if (data == null || data.Length == 0)
            {
                return false;
            }

            try
            {
                using (var stream = new MemoryStream(data))
                using (var reader = new BinaryReader(stream, Encoding.UTF8))
                {
                    displayName = reader.ReadString();
                    characterId = reader.ReadString();
                    return true;
                }
            }
            catch (Exception)
            {
                displayName = string.Empty;
                characterId = string.Empty;
                return false;
            }
        }
    }
}
