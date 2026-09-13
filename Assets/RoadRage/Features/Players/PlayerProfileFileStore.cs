using System;
using System.IO;
using RoadRage.Shared.Definitions;
using UnityEngine;

namespace RoadRage.Features.Players
{
    /// <summary>
    /// Port de persistance du profil joueur (Story 4.5) : un fichier JSON unique, a chemin injecte,
    /// qui ne contient que l'identite joueur et le choix cosmetique.
    /// Objet C# pur, sans MonoBehaviour : il ne touche ni le depot de session, ni le catalogue, ni
    /// l'identite Steam. Aucune exception ne remonte pour un fichier absent, tronque ou illisible :
    /// un profil illisible doit rendre le flux recreatif, pas casser le menu.
    /// </summary>
    public sealed class PlayerProfileFileStore
    {
        public const string DefaultFileName = "player-profile.json";

        private static string defaultFilePath;

        public PlayerProfileFileStore(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("Chemin de fichier de profil vide.", nameof(filePath));
            }

            FilePath = filePath;
        }

        /// <summary>
        /// Chemin par defaut du fichier de profil. Lu paresseusement, et remplacable par les tests
        /// PlayMode qui doivent rediriger l'ecriture hors du dossier persistant reel de la machine.
        /// </summary>
        public static string DefaultFilePath
        {
            get
            {
                if (defaultFilePath == null)
                {
                    defaultFilePath = Path.Combine(Application.persistentDataPath, DefaultFileName);
                }

                return defaultFilePath;
            }

            set { defaultFilePath = value; }
        }

        public string FilePath { get; }

        /// <summary>
        /// Lit le profil persistant de ce compte Steam. Retourne faux si le fichier manque, est vide,
        /// illisible ou incomplet, ou s'il appartient a un autre compte Steam : dans tous ces cas
        /// l'appelant recree un profil depuis l'identite courante, et aucun profil partiel ni celui
        /// d'un autre joueur n'est publie.
        /// </summary>
        public bool TryLoad(string steamId, out PlayerProfile profile)
        {
            profile = null;

            if (string.IsNullOrWhiteSpace(steamId))
            {
                return false;
            }

            try
            {
                if (!File.Exists(FilePath))
                {
                    return false;
                }

                var json = File.ReadAllText(FilePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return false;
                }

                var record = JsonUtility.FromJson<PersistentPlayerProfileRecord>(json);
                if (record == null || string.IsNullOrWhiteSpace(record.displayName) || string.IsNullOrWhiteSpace(record.characterId))
                {
                    return false;
                }

                if (!string.Equals(record.steamId, steamId, StringComparison.Ordinal))
                {
                    Debug.Log("[Players] Profil persistant appartenant a un autre compte Steam : recree pour le compte courant.");
                    return false;
                }

                profile = new PlayerProfile(record.displayName, new DefinitionId(record.characterId));
                return true;
            }
            catch (Exception exception)
            {
                if (!IsRecoverable(exception))
                {
                    throw;
                }

                Debug.LogWarning("[Players] Fichier de profil illisible (" + FilePath + ") : " + exception.GetType().Name + " ; profil ignore.");
                return false;
            }
        }

        /// <summary>
        /// Ecrit le profil persistant de ce compte Steam. Un profil sans proprietaire n'est jamais
        /// ecrit : un fichier sans id Steam serait adopte par le compte suivant. Retourne faux si
        /// l'ecriture echoue : l'echec est journalise et laisse la session utilisable.
        /// </summary>
        public bool TrySave(PlayerProfile profile, string steamId)
        {
            if (profile == null || string.IsNullOrWhiteSpace(steamId))
            {
                return false;
            }

            var record = new PersistentPlayerProfileRecord
            {
                displayName = profile.DisplayName,
                characterId = profile.CharacterId.Value,
                steamId = steamId,
            };

            try
            {
                var directory = Path.GetDirectoryName(FilePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(FilePath, JsonUtility.ToJson(record, true));
                return true;
            }
            catch (Exception exception)
            {
                if (!IsRecoverable(exception))
                {
                    throw;
                }

                Debug.LogWarning("[Players] Ecriture du profil impossible (" + FilePath + ") : " + exception.GetType().Name + ".");
                return false;
            }
        }

        private static bool IsRecoverable(Exception exception)
        {
            return exception is IOException
                || exception is UnauthorizedAccessException
                || exception is ArgumentException
                || exception is NotSupportedException
                || exception is System.Security.SecurityException;
        }
    }
}
