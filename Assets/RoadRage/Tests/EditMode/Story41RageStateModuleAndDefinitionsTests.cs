using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using RoadRage.Features.Rage;
using RoadRage.Shared.Domain;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 4.1 : bornes de seuil de RageTuningDef.ResolveDisposition, clamp haut/bas et no-op de
    /// NetworkedRageState.ApplyRageDelta, independance entre deux cibles, et branches de refus de
    /// RageTuningCatalog.TryValidate (id duplique/vide/non minuscule/paliers non ascendants).
    /// </summary>
    public sealed class Story41RageStateModuleAndDefinitionsTests
    {
        private const string RageTuningDefAssetPath = "Assets/RoadRage/ScriptableObjects/Rage/RageTuningDef_Default.asset";
        private const string RageTuningCatalogAssetPath = "Assets/RoadRage/ScriptableObjects/Rage/RageTuningCatalog.asset";

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

        // ---------------------------------------------------------------- RageTuningDef.ResolveDisposition

        [Test]
        public void ResolveDispositionReturnsCalmBelowFirstThreshold()
        {
            var tuning = NewStandardTuning();

            Assert.That(tuning.ResolveDisposition(5f), Is.EqualTo(RageDisposition.Calm));
        }

        /// <summary>Reprend l'exemple de la matrice I/O : RageValue 15 -> 25, seuil Irritated a 20 -> Irritated.</summary>
        [Test]
        public void ResolveDispositionMatchesIoMatrixThresholdCrossedUpward()
        {
            var tuning = NewStandardTuning();

            Assert.That(tuning.ResolveDisposition(15f), Is.EqualTo(RageDisposition.Calm));
            Assert.That(tuning.ResolveDisposition(25f), Is.EqualTo(RageDisposition.Irritated));
        }

        /// <summary>Borne exacte : le seuil doit etre inclusif ("&gt;="), pas exclusif.</summary>
        [Test]
        public void ResolveDispositionCrossesThresholdAtExactBoundaryValue()
        {
            var tuning = NewStandardTuning();

            Assert.That(tuning.ResolveDisposition(20f), Is.EqualTo(RageDisposition.Irritated));
        }

        /// <summary>Borne exacte - epsilon : sans elle, un "&gt;" remplacant "&gt;=" resterait indetectable.</summary>
        [Test]
        public void ResolveDispositionStaysBelowPalierJustUnderBoundaryValue()
        {
            var tuning = NewStandardTuning();

            Assert.That(tuning.ResolveDisposition(19.99f), Is.EqualTo(RageDisposition.Calm));
        }

        [Test]
        public void ResolveDispositionResolvesToTheHighestPalierReached()
        {
            var tuning = NewStandardTuning();

            Assert.That(tuning.ResolveDisposition(85f), Is.EqualTo(RageDisposition.Ram));
            Assert.That(tuning.ResolveDisposition(100f), Is.EqualTo(RageDisposition.ConfrontationCapable));
        }

        // ---------------------------------------------------------------- NetworkedRageState.ApplyRageDelta

        [Test]
        public void ApplyRageDeltaClampsAtMaxRageValueAndResolvesLastPalier()
        {
            var tuning = NewStandardTuning();
            var state = NewRageState();

            state.ApplyRageDelta(999f, tuning);

            Assert.That(state.RageValue.Value, Is.EqualTo(tuning.MaxRageValue));
            Assert.That(state.Disposition.Value, Is.EqualTo(RageDisposition.ConfrontationCapable));
        }

        [Test]
        public void ApplyRageDeltaClampsAtZeroAndResolvesCalm()
        {
            var tuning = NewStandardTuning();
            var state = NewRageState();
            state.ApplyRageDelta(60f, tuning);
            Assert.That(state.Disposition.Value, Is.Not.EqualTo(RageDisposition.Calm), "precondition : la cible doit avoir quitte Calm avant le delta negatif");

            state.ApplyRageDelta(-999f, tuning);

            Assert.That(state.RageValue.Value, Is.EqualTo(0f));
            Assert.That(state.Disposition.Value, Is.EqualTo(RageDisposition.Calm));
        }

        [Test]
        public void ApplyRageDeltaWithNullTuningIsASilentNoOp()
        {
            var tuning = NewStandardTuning();
            var state = NewRageState();
            state.ApplyRageDelta(30f, tuning);
            var rageValueBefore = state.RageValue.Value;
            var dispositionBefore = state.Disposition.Value;

            Assert.DoesNotThrow(() => state.ApplyRageDelta(999f, null));

            Assert.That(state.RageValue.Value, Is.EqualTo(rageValueBefore));
            Assert.That(state.Disposition.Value, Is.EqualTo(dispositionBefore));
        }

        /// <summary>
        /// AC : un NetworkedRageState par cible ; deux cibles recevant des deltas differents evoluent
        /// independamment (pas de collection interne partagee).
        /// </summary>
        [Test]
        public void TwoNetworkedRageStatesEvolveIndependently()
        {
            var tuning = NewStandardTuning();
            var first = NewRageState();
            var second = NewRageState();

            first.ApplyRageDelta(25f, tuning);
            second.ApplyRageDelta(65f, tuning);

            Assert.That(first.RageValue.Value, Is.EqualTo(25f));
            Assert.That(first.Disposition.Value, Is.EqualTo(RageDisposition.Irritated));
            Assert.That(second.RageValue.Value, Is.EqualTo(65f));
            Assert.That(second.Disposition.Value, Is.EqualTo(RageDisposition.Block));
        }

        // ---------------------------------------------------------------- RageTuningCatalog.TryValidate

        [Test]
        public void RageTuningCatalogValidationRejectsEmptyList()
        {
            var catalog = NewCatalog();

            string error;
            Assert.That(catalog.TryValidate(out error), Is.False);
            Assert.That(error, Does.Contain("vide"));
        }

        [Test]
        public void RageTuningCatalogValidationRejectsNullEntry()
        {
            var catalog = NewCatalog(NewStandardTuning("rage_default"), null);

            string error;
            Assert.That(catalog.TryValidate(out error), Is.False, "une entree nulle doit etre refusee");
            Assert.That(error, Does.Contain("nulle"));
        }

        [TestCase("")]
        [TestCase("   ")]
        public void RageTuningCatalogValidationRejectsEmptyId(string id)
        {
            var catalog = NewCatalog(NewTuning(id));

            string error;
            Assert.That(catalog.TryValidate(out error), Is.False, "un id vide doit etre refuse");
            Assert.That(error, Does.Contain("vide"));
        }

        [TestCase(" rage_default")]
        [TestCase("rage_default ")]
        public void RageTuningCatalogValidationRejectsPaddedId(string id)
        {
            var catalog = NewCatalog(NewTuning(id));

            string error;
            Assert.That(catalog.TryValidate(out error), Is.False, "un id borde d'espaces doit etre refuse : '" + id + "'");
            Assert.That(error, Does.Contain("espaces"));
        }

        [TestCase("Rage_Default")]
        [TestCase("RAGE_DEFAULT")]
        public void RageTuningCatalogValidationRejectsNonLowercaseId(string id)
        {
            var catalog = NewCatalog(NewTuning(id));

            string error;
            Assert.That(catalog.TryValidate(out error), Is.False, "un id non minuscule doit etre refuse : " + id);
            Assert.That(error, Does.Contain("minuscule"));
        }

        [Test]
        public void RageTuningCatalogValidationRejectsDuplicateIds()
        {
            var catalog = NewCatalog(NewTuning("rage_default"), NewTuning("rage_default"));

            string error;
            Assert.That(catalog.TryValidate(out error), Is.False, "deux ids identiques doivent etre refuses");
            Assert.That(error, Does.Contain("duplique"));
            Assert.That(error, Does.Contain("rage_default"));
        }

        /// <summary>Design Notes : l'ordre ascendant des paliers est verifie par le catalogue, pas recalcule a l'execution.</summary>
        [Test]
        public void RageTuningCatalogValidationRejectsNonAscendingThresholds()
        {
            var tuning = NewTuning("rage_default", 100f,
                new RageTuningDef.RageThreshold(RageDisposition.Irritated, 40f),
                new RageTuningDef.RageThreshold(RageDisposition.Flee, 20f));
            var catalog = NewCatalog(tuning);

            string error;
            Assert.That(catalog.TryValidate(out error), Is.False, "des paliers non ascendants doivent etre refuses");
            Assert.That(error, Does.Contain("ascendants"));
        }

        /// <summary>ApplyRageDelta clampe RageValue a MaxRageValue : un palier au-dessus ne resoudrait jamais.</summary>
        [Test]
        public void RageTuningCatalogValidationRejectsThresholdAboveMaxRageValue()
        {
            var tuning = NewTuning("rage_default", 100f,
                new RageTuningDef.RageThreshold(RageDisposition.Irritated, 40f),
                new RageTuningDef.RageThreshold(RageDisposition.ConfrontationCapable, 120f));
            var catalog = NewCatalog(tuning);

            string error;
            Assert.That(catalog.TryValidate(out error), Is.False, "un palier au-dessus de MaxRageValue doit etre refuse");
            Assert.That(error, Does.Contain("MaxRageValue"));
        }

        [Test]
        public void RageTuningCatalogValidationAcceptsAWellFormedInMemoryCatalog()
        {
            var catalog = NewCatalog(NewStandardTuning("rage_default"), NewStandardTuning("rage_boss"));

            string error;
            Assert.That(catalog.TryValidate(out error), Is.True, error);
            Assert.That(error, Is.Empty);
        }

        [Test]
        public void RageTuningDefValidationRejectsNonFiniteMaxValue()
        {
            var tuning = NewTuning("rage_default", float.NaN);

            Assert.That(tuning.TryValidate(out var error), Is.False);
            Assert.That(error, Does.Contain("MaxRageValue"));
        }

        // ---------------------------------------------------------------- Assets livres

        [Test]
        public void RageTuningCatalogAssetExistsAndPassesItsOwnValidationGuard()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<RageTuningCatalog>(RageTuningCatalogAssetPath);
            Assert.That(catalog != null, Is.True, "catalogue introuvable : " + RageTuningCatalogAssetPath);

            string error;
            Assert.That(catalog.TryValidate(out error), Is.True, error);
        }

        [Test]
        public void RageTuningDefAssetExistsWithStableLowercaseId()
        {
            var tuning = AssetDatabase.LoadAssetAtPath<RageTuningDef>(RageTuningDefAssetPath);
            Assert.That(tuning != null, Is.True, "tuning introuvable : " + RageTuningDefAssetPath);

            Assert.That(tuning.RawId, Is.EqualTo(tuning.RawId.ToLowerInvariant()));
            Assert.That(tuning.RawId, Is.Not.Empty);
            Assert.That(tuning.TryValidate(out var error), Is.True, error);
            Assert.That(tuning.HasAscendingThresholds(), Is.True);
        }

        // ---------------------------------------------------------------- Helpers

        private RageTuningDef NewTuning(string id, float maxRageValue = 100f, params RageTuningDef.RageThreshold[] thresholds)
        {
            var tuning = ScriptableObject.CreateInstance<RageTuningDef>();
            SetPrivateField(tuning, "id", id);
            SetPrivateField(tuning, "maxRageValue", maxRageValue);
            SetPrivateField(tuning, "thresholds", thresholds ?? new RageTuningDef.RageThreshold[0]);
            spawned.Add(tuning);
            return tuning;
        }

        /// <summary>Gabarit de paliers repris de RageTuningDef_Default.asset (seuils placeholder de la Story 4.1).</summary>
        private RageTuningDef NewStandardTuning(string id = "rage_default")
        {
            return NewTuning(id, 100f,
                new RageTuningDef.RageThreshold(RageDisposition.Irritated, 20f),
                new RageTuningDef.RageThreshold(RageDisposition.Flee, 40f),
                new RageTuningDef.RageThreshold(RageDisposition.Block, 60f),
                new RageTuningDef.RageThreshold(RageDisposition.Ram, 80f),
                new RageTuningDef.RageThreshold(RageDisposition.ConfrontationCapable, 95f));
        }

        private RageTuningCatalog NewCatalog(params RageTuningDef[] tunings)
        {
            var catalog = ScriptableObject.CreateInstance<RageTuningCatalog>();
            SetPrivateField(catalog, "tunings", new List<RageTuningDef>(tunings));
            spawned.Add(catalog);
            return catalog;
        }

        private NetworkedRageState NewRageState()
        {
            var root = new GameObject("NetworkedRageStateTestTarget");
            root.AddComponent<NetworkObject>();
            var state = root.AddComponent<NetworkedRageState>();
            spawned.Add(root);
            return state;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "field not found: " + fieldName);
            field.SetValue(target, value);
        }
    }
}
