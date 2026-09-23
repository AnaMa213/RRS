#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RoadRage.Features.Vehicles.Traffic.Migration
{
    /// <summary>
    /// Lignee persistee de la migration (Story 5.27, AD-44) : cle de lignee -> identite stable.
    /// Seul etat que l'importeur conserve d'un import a l'autre ; le modele candidat est regenere
    /// depuis la scene. Une cle vivante garde son identite ; une cle disparue est tombstonee avec
    /// son identite, qui n'est jamais recyclee, meme si la meme cle reapparait plus tard.
    /// </summary>
    public sealed class RoadLineage
    {
        [Serializable]
        private sealed class FileRecord
        {
            public string Key;
            public string Kind;
            public string Id;
        }

        [Serializable]
        private sealed class FileLayout
        {
            /// <summary>Sans initialiseur : un champ absent se lit 0 et est rejete.</summary>
            public int Format;
            public string ModelId;
            public FileRecord[] Records;
            public FileRecord[] Tombstones;
        }

        public struct Entry
        {
            public string Key;
            public RoadRecordKind Kind;
            public RoadId Id;
        }

        private readonly SortedDictionary<string, Entry> _live = new SortedDictionary<string, Entry>(StringComparer.Ordinal);
        private readonly List<Entry> _tombstones = new List<Entry>();

        public RoadId ModelId { get; private set; }

        public IEnumerable<Entry> Live
        {
            get { return _live.Values; }
        }

        public IReadOnlyList<Entry> Tombstones
        {
            get { return _tombstones; }
        }

        public bool TryGet(string key, out Entry entry)
        {
            return _live.TryGetValue(key, out entry);
        }

        public static RoadLineage Empty()
        {
            return new RoadLineage();
        }

        /// <summary>
        /// Relit une lignee serialisee. <c>null</c> (aucun fichier) = premier import. Un texte vide
        /// ou blanc est un echec dur : un fichier tronque ne doit jamais refrapper toutes les
        /// identites. Tout autre defaut (identifiant illisible, genre inconnu, cle ou identifiant
        /// en double) est aussi un echec dur.
        /// </summary>
        /// <exception cref="FormatException">Lignee vide, illisible ou incoherente.</exception>
        public static RoadLineage Parse(string json)
        {
            var lineage = new RoadLineage();
            if (json == null)
            {
                return lineage;
            }

            if (json.Trim().Length == 0)
            {
                throw new FormatException("Lignee vide : fichier present mais sans contenu, aucune identite n'est refrappee.");
            }

            FileLayout layout;
            try
            {
                layout = JsonUtility.FromJson<FileLayout>(json);
            }
            catch (ArgumentException exception)
            {
                throw new FormatException("Lignee illisible : " + exception.Message);
            }

            if (layout == null || layout.Format != 1)
            {
                throw new FormatException("Lignee : format absent ou inconnu.");
            }

            lineage.ModelId = ParseId(layout.ModelId, "ModelId");
            var seenIds = new HashSet<RoadId> { lineage.ModelId };
            foreach (var record in layout.Records ?? new FileRecord[0])
            {
                var entry = ParseEntry(record, seenIds);
                if (lineage._live.ContainsKey(entry.Key))
                {
                    throw new FormatException("Lignee : cle en double " + entry.Key + ".");
                }

                lineage._live.Add(entry.Key, entry);
            }

            var tombstoneKeys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var record in layout.Tombstones ?? new FileRecord[0])
            {
                var tombstone = ParseEntry(record, seenIds);
                if (lineage._live.ContainsKey(tombstone.Key) || !tombstoneKeys.Add(tombstone.Key))
                {
                    throw new FormatException("Lignee : cle " + tombstone.Key
                        + " a la fois vivante et tombstonee, ou tombstonee deux fois : une cle porte une seule identite.");
                }

                lineage._tombstones.Add(tombstone);
            }

            lineage.SortTombstones();
            return lineage;
        }

        /// <summary>
        /// Serialisation deterministe : cles en ordre ordinal, tombstones par identifiant, LF,
        /// terminee par un saut de ligne. Deux lignees egales donnent les memes octets.
        /// </summary>
        public string Serialize()
        {
            var layout = new FileLayout();
            layout.Format = 1;
            layout.ModelId = ModelId.ToString();
            layout.Records = new FileRecord[_live.Count];
            int i = 0;
            foreach (var entry in _live.Values)
            {
                layout.Records[i++] = ToRecord(entry);
            }

            layout.Tombstones = new FileRecord[_tombstones.Count];
            for (int t = 0; t < _tombstones.Count; t++)
            {
                layout.Tombstones[t] = ToRecord(_tombstones[t]);
            }

            return JsonUtility.ToJson(layout, true).Replace("\r\n", "\n") + "\n";
        }

        /// <summary>
        /// Resout les cles de l'import courant contre cette lignee et rend la lignee suivante.
        /// Cle connue : identite preservee. Cle nouvelle : identite frappee, jamais egale a une
        /// identite vivante ou tombstonee. Cle disparue : tombstonee. Ne modifie pas cette instance.
        /// </summary>
        /// <exception cref="InvalidOperationException">Une cle connue change de genre.</exception>
        public LineageResolution Resolve(IReadOnlyList<KeyValuePair<string, RoadRecordKind>> keys)
        {
            var next = new RoadLineage();
            next.ModelId = ModelId.IsEmpty ? MintUnused(null) : ModelId;
            next._tombstones.AddRange(_tombstones);

            var resolution = new LineageResolution();
            resolution.Next = next;
            resolution.ModelIdMinted = ModelId.IsEmpty;

            var current = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < keys.Count; i++)
            {
                current.Add(keys[i].Key);
                Entry known;
                Entry entry;
                if (_live.TryGetValue(keys[i].Key, out known))
                {
                    if (known.Kind != keys[i].Value)
                    {
                        throw new InvalidOperationException("La cle " + keys[i].Key + " change de genre : " + known.Kind + " -> " + keys[i].Value + ".");
                    }

                    entry = known;
                    resolution.Preserved.Add(entry.Key);
                }
                else
                {
                    entry.Key = keys[i].Key;
                    entry.Kind = keys[i].Value;
                    entry.Id = MintUnused(next);
                    resolution.Minted.Add(entry.Key);
                }

                next._live.Add(entry.Key, entry);
            }

            foreach (var entry in _live.Values)
            {
                if (!current.Contains(entry.Key))
                {
                    next._tombstones.Add(entry);
                    resolution.Retired.Add(entry);
                }
            }

            next.SortTombstones();
            return resolution;
        }

        private RoadId MintUnused(RoadLineage next)
        {
            while (true)
            {
                var id = RoadId.New();
                if (!id.IsEmpty && !Uses(id) && (next == null || !next.Uses(id)))
                {
                    return id;
                }
            }
        }

        private bool Uses(RoadId id)
        {
            if (id == ModelId)
            {
                return true;
            }

            foreach (var entry in _live.Values)
            {
                if (entry.Id == id)
                {
                    return true;
                }
            }

            for (int i = 0; i < _tombstones.Count; i++)
            {
                if (_tombstones[i].Id == id)
                {
                    return true;
                }
            }

            return false;
        }

        private void SortTombstones()
        {
            _tombstones.Sort(delegate(Entry a, Entry b) { return a.Id.CompareTo(b.Id); });
        }

        private static FileRecord ToRecord(Entry entry)
        {
            var record = new FileRecord();
            record.Key = entry.Key;
            record.Kind = entry.Kind.ToString();
            record.Id = entry.Id.ToString();
            return record;
        }

        private static Entry ParseEntry(FileRecord record, HashSet<RoadId> seenIds)
        {
            if (record == null || string.IsNullOrEmpty(record.Key))
            {
                throw new FormatException("Lignee : enregistrement sans cle.");
            }

            RoadRecordKind kind;
            if (!Enum.TryParse(record.Kind, false, out kind) || kind == RoadRecordKind.Unknown)
            {
                throw new FormatException("Lignee : genre inconnu '" + record.Kind + "' pour " + record.Key + ".");
            }

            var entry = new Entry();
            entry.Key = record.Key;
            entry.Kind = kind;
            entry.Id = ParseId(record.Id, record.Key);
            if (!seenIds.Add(entry.Id))
            {
                throw new FormatException("Lignee : identifiant " + entry.Id + " porte deux fois.");
            }

            return entry;
        }

        private static RoadId ParseId(string hex, string what)
        {
            RoadId id;
            if (!RoadId.TryParse(hex, out id) || id.IsEmpty)
            {
                throw new FormatException("Lignee : identifiant illisible pour " + what + ".");
            }

            return id;
        }
    }

    /// <summary>Resultat d'une resolution de lignee : la lignee suivante et ce qui a change.</summary>
    public sealed class LineageResolution
    {
        public RoadLineage Next;
        public bool ModelIdMinted;
        public readonly List<string> Preserved = new List<string>();
        public readonly List<string> Minted = new List<string>();
        public readonly List<RoadLineage.Entry> Retired = new List<RoadLineage.Entry>();
    }
}
#endif
