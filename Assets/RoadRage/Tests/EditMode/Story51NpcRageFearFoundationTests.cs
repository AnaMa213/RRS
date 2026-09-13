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
    /// Story 5.1 : fondation configurable Rage/Peur. Couvre le contrat de canal
    /// (ReactionChannel / NpcReactionEffect), son application host-authoritative (canal rage, canal
    /// peur, les deux, independance entre deux cibles, entrees inertes, sensibilites nulles ou non
    /// finies) et les nouvelles branches de refus de RageTuningDef.TryValidate et de
    /// RageTuningCatalog.TryValidate. Deterministe et sans Netcode : les NetworkVariables sont
    /// ecrites directement, sans spawn, comme dans Story41RageStateModuleAndDefinitionsTests.
    /// </summary>
    public sealed class Story51NpcRageFearFoundationTests
    {
        private const string RageTuningDefAssetPath = "Assets/RoadRage/ScriptableObjects/Rage/RageTuningDef_Default.asset";

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

        // ---------------------------------------------------------------- ReactionChannel / NpcReactionEffect

        [Test]
        public void ReactionChannelFlagsCoverNoneRageFearAndBoth()
        {
            Assert.That((int)ReactionChannel.None, Is.EqualTo(0));
            Assert.That(ReactionChannel.Both, Is.EqualTo(ReactionChannel.Rage | ReactionChannel.Fear));

            var both = new NpcReactionEffect(ReactionChannel.Both, 25f);
            Assert.That(both.Channel, Is.EqualTo(ReactionChannel.Both));
            Assert.That(both.Magnitude, Is.EqualTo(25f));
            Assert.That(both.AffectsRage, Is.True);
            Assert.That(both.AffectsFear, Is.True);

            var rageOnly = new NpcReactionEffect(ReactionChannel.Rage, 25f);
            Assert.That(rageOnly.AffectsRage, Is.True);
            Assert.That(rageOnly.AffectsFear, Is.False);

            var fearOnly = new NpcReactionEffect(ReactionChannel.Fear, 25f);
            Assert.That(fearOnly.AffectsRage, Is.False);
            Assert.That(fearOnly.AffectsFear, Is.True);

            var none = new NpcReactionEffect(ReactionChannel.None, 25f);
            Assert.That(none.AffectsRage, Is.False);
            Assert.That(none.AffectsFear, Is.False);
        }

        /// <summary>Un canal None ou une magnitude nulle restent authores valides : inertes a l'application, pas refuses ici.</summary>
        [Test]
        public void NpcReactionEffectValidationAcceptsInertButWellFormedEffects()
        {
            var none = new NpcReactionEffect(ReactionChannel.None, 25f);
            var zero = new NpcReactionEffect(ReactionChannel.Both, 0f);

            Assert.That(none.TryValidate(out var noneError), Is.True, noneError);
            Assert.That(zero.TryValidate(out var zeroError), Is.True, zeroError);
            Assert.That(zeroError, Is.Empty);
        }

        [TestCase(4)]
        [TestCase(7)]
        [TestCase(-1)]
        public void NpcReactionEffectValidationRejectsUndefinedChannelBits(int rawChannel)
        {
            var effect = new NpcReactionEffect((ReactionChannel)rawChannel, 25f);

            Assert.That(effect.TryValidate(out var error), Is.False, "un canal hors Rage/Fear/Both doit etre refuse : " + rawChannel);
            Assert.That(error, Does.Contain("channel"));
        }

        [Test]
        public void NpcReactionEffectValidationRejectsNonFiniteMagnitude()
        {
            var effect = new NpcReactionEffect(ReactionChannel.Rage, float.NaN);

            Assert.That(effect.TryValidate(out var error), Is.False);
            Assert.That(error, Does.Contain("magnitude"));
        }

        // ---------------------------------------------------------------- NetworkedRageState.ApplyReactionEffect

        /// <summary>Matrice I/O, canal rage : RageValue 90 + effet (Rage, 25) -> 100, Disposition recalculee, peur intacte.</summary>
        [Test]
        public void ApplyReactionEffectOnRageChannelClampsRageAndRecomputesDisposition()
        {
            var tuning = NewStandardTuning();
            var state = NewRageState();
            state.ApplyRageDelta(90f, tuning);
            state.ApplyFearDelta(40f, tuning);
            Assert.That(state.Disposition.Value, Is.EqualTo(RageDisposition.Ram), "precondition : 90 doit resoudre Ram avant l'effet");

            state.ApplyReactionEffect(new NpcReactionEffect(ReactionChannel.Rage, 25f), tuning);

            Assert.That(state.RageValue.Value, Is.EqualTo(100f));
            Assert.That(state.Disposition.Value, Is.EqualTo(RageDisposition.ConfrontationCapable));
            Assert.That(state.FearValue.Value, Is.EqualTo(40f), "un effet de rage ne doit pas bouger la peur");
        }

        /// <summary>
        /// Matrice I/O, canal peur : FearValue 90 + effet (Fear, 25) -> 100, rage intacte. La Disposition
        /// est volontairement laissee incoherente avec la rage : si l'effet de peur la recalculait, ce
        /// marqueur disparaitrait.
        /// </summary>
        [Test]
        public void ApplyReactionEffectOnFearChannelMovesFearOnly()
        {
            var tuning = NewStandardTuning();
            var state = NewRageState();
            state.ApplyRageDelta(25f, tuning);
            state.ApplyFearDelta(90f, tuning);
            state.Disposition.Value = RageDisposition.ConfrontationCapable;

            state.ApplyReactionEffect(new NpcReactionEffect(ReactionChannel.Fear, 25f), tuning);

            Assert.That(state.FearValue.Value, Is.EqualTo(100f));
            Assert.That(state.RageValue.Value, Is.EqualTo(25f), "un effet de peur ne doit pas bouger la rage");
            Assert.That(state.Disposition.Value, Is.EqualTo(RageDisposition.ConfrontationCapable), "Disposition ne doit pas etre recalculee quand la rage ne bouge pas");
        }

        /// <summary>Matrice I/O, canaux les deux : les deux valeurs bougent et la Disposition reste le reflet de la rage.</summary>
        [Test]
        public void ApplyReactionEffectOnBothChannelsMovesBothAndKeepsDispositionConsistent()
        {
            var tuning = NewStandardTuning();
            var state = NewRageState();
            state.ApplyRageDelta(90f, tuning);
            state.ApplyFearDelta(90f, tuning);

            state.ApplyReactionEffect(new NpcReactionEffect(ReactionChannel.Both, 25f), tuning);

            Assert.That(state.RageValue.Value, Is.EqualTo(100f));
            Assert.That(state.FearValue.Value, Is.EqualTo(100f));
            Assert.That(state.Disposition.Value, Is.EqualTo(RageDisposition.ConfrontationCapable));
            Assert.That(state.Disposition.Value, Is.EqualTo(tuning.ResolveDisposition(state.RageValue.Value)));
        }

        /// <summary>Borne basse du clamp : un effet negatif ne descend pas sous 0 et remet la disposition a Calm.</summary>
        [Test]
        public void ApplyReactionEffectOnRageChannelClampsAtZero()
        {
            var tuning = NewStandardTuning();
            var state = NewRageState();
            state.ApplyRageDelta(10f, tuning);

            state.ApplyReactionEffect(new NpcReactionEffect(ReactionChannel.Rage, -50f), tuning);

            Assert.That(state.RageValue.Value, Is.EqualTo(0f));
            Assert.That(state.Disposition.Value, Is.EqualTo(RageDisposition.Calm));
        }

        /// <summary>Matrice I/O, entrees inertes : canal None, magnitude nulle et tuning absent ne changent rien et ne levent rien.</summary>
        [Test]
        public void ApplyReactionEffectIsASilentNoOpForNoneChannelZeroMagnitudeAndNullTuning()
        {
            var tuning = NewStandardTuning();
            var state = NewRageState();
            state.ApplyRageDelta(30f, tuning);
            state.ApplyFearDelta(30f, tuning);
            var rageBefore = state.RageValue.Value;
            var fearBefore = state.FearValue.Value;
            var dispositionBefore = state.Disposition.Value;

            Assert.DoesNotThrow(() => state.ApplyReactionEffect(new NpcReactionEffect(ReactionChannel.None, 25f), tuning));
            Assert.DoesNotThrow(() => state.ApplyReactionEffect(new NpcReactionEffect(ReactionChannel.Both, 0f), tuning));
            Assert.DoesNotThrow(() => state.ApplyReactionEffect(new NpcReactionEffect(ReactionChannel.Both, 25f), null));

            Assert.That(state.RageValue.Value, Is.EqualTo(rageBefore));
            Assert.That(state.FearValue.Value, Is.EqualTo(fearBefore));
            Assert.That(state.Disposition.Value, Is.EqualTo(dispositionBefore));
        }

        /// <summary>Design Notes : les tendances sont des multiplicateurs de sensibilite, pas des deltas absolus.</summary>
        [Test]
        public void ApplyReactionEffectScalesMagnitudeByTuningSensitivity()
        {
            var tuning = NewStandardTuning(rageSensitivity: 2f, fearSensitivity: 0.5f);
            var state = NewRageState();

            state.ApplyReactionEffect(new NpcReactionEffect(ReactionChannel.Both, 10f), tuning);

            Assert.That(state.RageValue.Value, Is.EqualTo(20f));
            Assert.That(state.FearValue.Value, Is.EqualTo(5f));
        }

        /// <summary>Matrice I/O : une sensibilite de rage nulle rend le canal rage inerte, sans recalcul de Disposition.</summary>
        [Test]
        public void ApplyReactionEffectWithZeroRageSensitivityLeavesRageAndDispositionInert()
        {
            var tuning = NewStandardTuning(rageSensitivity: 0f);
            var state = NewRageState();
            state.ApplyRageDelta(25f, tuning);
            state.Disposition.Value = RageDisposition.Ram;

            state.ApplyReactionEffect(new NpcReactionEffect(ReactionChannel.Both, 25f), tuning);

            Assert.That(state.RageValue.Value, Is.EqualTo(25f));
            Assert.That(state.Disposition.Value, Is.EqualTo(RageDisposition.Ram), "un canal rage inerte ne doit pas recalculer la disposition");
            Assert.That(state.FearValue.Value, Is.EqualTo(25f), "le canal peur reste actif quand seule la sensibilite de rage est nulle");
        }

        /// <summary>Symetrique du cas precedent : une sensibilite de peur nulle rend le canal peur inerte.</summary>
        [Test]
        public void ApplyReactionEffectWithZeroFearSensitivityLeavesFearInert()
        {
            var tuning = NewStandardTuning(fearSensitivity: 0f);
            var state = NewRageState();
            state.ApplyReactionEffect(new NpcReactionEffect(ReactionChannel.Both, 25f), tuning);

            Assert.That(state.FearValue.Value, Is.EqualTo(0f));
            Assert.That(state.RageValue.Value, Is.EqualTo(25f), "le canal rage reste actif quand seule la sensibilite de peur est nulle");
            Assert.That(state.Disposition.Value, Is.EqualTo(RageDisposition.Irritated));
        }

        /// <summary>
        /// Une sensibilite non finie (tuning refuse par TryValidate) rend le canal inerte : un NaN ne
        /// doit jamais entrer dans une NetworkVariable synchronisee.
        /// </summary>
        [Test]
        public void ApplyReactionEffectWithNonFiniteSensitivityKeepsTheChannelInert()
        {
            var tuning = NewStandardTuning(rageSensitivity: float.NaN);
            var state = NewRageState();

            state.ApplyReactionEffect(new NpcReactionEffect(ReactionChannel.Rage, 25f), tuning);

            Assert.That(state.RageValue.Value, Is.EqualTo(0f));
            Assert.That(state.Disposition.Value, Is.EqualTo(RageDisposition.Calm));
        }

        /// <summary>AC : chaque cible reste independante ; un effet sur la premiere ne touche pas la seconde.</summary>
        [Test]
        public void ApplyReactionEffectOnOneTargetLeavesTheOtherUntouched()
        {
            var tuning = NewStandardTuning();
            var first = NewRageState();
            var second = NewRageState();
            second.ApplyRageDelta(30f, tuning);
            second.ApplyFearDelta(30f, tuning);

            first.ApplyReactionEffect(new NpcReactionEffect(ReactionChannel.Both, 25f), tuning);

            Assert.That(first.RageValue.Value, Is.EqualTo(25f));
            Assert.That(first.FearValue.Value, Is.EqualTo(25f));
            Assert.That(second.RageValue.Value, Is.EqualTo(30f));
            Assert.That(second.FearValue.Value, Is.EqualTo(30f));
            Assert.That(second.Disposition.Value, Is.EqualTo(RageDisposition.Irritated));
        }

        // ---------------------------------------------------------------- NetworkedRageState.ApplyFearDelta

        [Test]
        public void ApplyFearDeltaClampsAtMaxFearValueAndAtZero()
        {
            var tuning = NewStandardTuning();
            var state = NewRageState();

            state.ApplyFearDelta(999f, tuning);
            Assert.That(state.FearValue.Value, Is.EqualTo(tuning.MaxFearValue));

            state.ApplyFearDelta(-999f, tuning);
            Assert.That(state.FearValue.Value, Is.EqualTo(0f));
        }

        [Test]
        public void ApplyFearDeltaWithNullTuningIsASilentNoOp()
        {
            var tuning = NewStandardTuning();
            var state = NewRageState();
            state.ApplyFearDelta(30f, tuning);
            var fearBefore = state.FearValue.Value;

            Assert.DoesNotThrow(() => state.ApplyFearDelta(999f, null));

            Assert.That(state.FearValue.Value, Is.EqualTo(fearBefore));
        }

        // ---------------------------------------------------------------- RageTuningDef.TryValidate (tendances)

        /// <summary>Matrice I/O : sensibilite 0 = canal inerte (valide) et maxFearValue 0 reste valide.</summary>
        [Test]
        public void RageTuningDefValidationAcceptsZeroSensitivitiesAndZeroMaxFearValue()
        {
            var tuning = NewTuning("rage_default", 100f, 0f, 0f, 0f, StandardThresholds());

            Assert.That(tuning.MaxFearValue, Is.EqualTo(0f), "precondition : la borne de peur doit bien etre posee a 0");
            Assert.That(tuning.TryValidate(out var error), Is.True, error);
        }

        [Test]
        public void RageTuningDefValidationRejectsNegativeRageSensitivity()
        {
            var tuning = NewTuning("rage_default", 100f, 100f, -1f, 1f, StandardThresholds());

            Assert.That(tuning.TryValidate(out var error), Is.False, "une sensibilite de rage negative doit etre refusee");
            Assert.That(error, Does.Contain("RageSensitivity"));
            Assert.That(error, Does.Contain("rageSensitivity"));
        }

        [Test]
        public void RageTuningDefValidationRejectsNonFiniteRageSensitivity()
        {
            var tuning = NewTuning("rage_default", 100f, 100f, float.NaN, 1f, StandardThresholds());

            Assert.That(tuning.TryValidate(out var error), Is.False, "une sensibilite de rage non finie doit etre refusee");
            Assert.That(error, Does.Contain("RageSensitivity"));
        }

        [Test]
        public void RageTuningDefValidationRejectsNegativeFearSensitivity()
        {
            var tuning = NewTuning("rage_default", 100f, 100f, 1f, -1f, StandardThresholds());

            Assert.That(tuning.TryValidate(out var error), Is.False, "une sensibilite de peur negative doit etre refusee");
            Assert.That(error, Does.Contain("FearSensitivity"));
            Assert.That(error, Does.Contain("fearSensitivity"));
        }

        [Test]
        public void RageTuningDefValidationRejectsNonFiniteFearSensitivity()
        {
            var tuning = NewTuning("rage_default", 100f, 100f, 1f, float.NaN, StandardThresholds());

            Assert.That(tuning.TryValidate(out var error), Is.False, "une sensibilite de peur non finie doit etre refusee");
            Assert.That(error, Does.Contain("FearSensitivity"));
        }

        [Test]
        public void RageTuningDefValidationRejectsNonFiniteMaxFearValue()
        {
            var tuning = NewTuning("rage_default", 100f, float.NaN, 1f, 1f, StandardThresholds());

            Assert.That(tuning.TryValidate(out var error), Is.False, "une borne de peur non finie doit etre refusee");
            Assert.That(error, Does.Contain("MaxFearValue"));
            Assert.That(error, Does.Contain("maxFearValue"));
        }

        /// <summary>Les nouvelles tendances ont des defauts neutres : borne de peur alignee sur la rage, sensibilites a 1.</summary>
        [Test]
        public void RageTuningDefDeclaresNeutralFearFoundationDefaults()
        {
            var tuning = ScriptableObject.CreateInstance<RageTuningDef>();
            spawned.Add(tuning);

            Assert.That(tuning.MaxFearValue, Is.EqualTo(100f));
            Assert.That(tuning.RageSensitivity, Is.EqualTo(1f));
            Assert.That(tuning.FearSensitivity, Is.EqualTo(1f));
        }

        // ---------------------------------------------------------------- RageTuningCatalog.TryValidate (tendances)

        [Test]
        public void RageTuningCatalogValidationRejectsTuningWithNegativeRageSensitivity()
        {
            var catalog = NewCatalog(NewTuning("rage_default", 100f, 100f, -1f, 1f, StandardThresholds()));

            string error;
            Assert.That(catalog.TryValidate(out error), Is.False, "un catalogue contenant une sensibilite de rage invalide doit etre refuse");
            Assert.That(error, Does.Contain("RageSensitivity"));
            Assert.That(error, Does.Contain("rage_default"));
        }

        [Test]
        public void RageTuningCatalogValidationRejectsTuningWithNonFiniteFearSensitivity()
        {
            var catalog = NewCatalog(NewTuning("rage_default", 100f, 100f, 1f, float.NaN, StandardThresholds()));

            string error;
            Assert.That(catalog.TryValidate(out error), Is.False, "un catalogue contenant une sensibilite de peur invalide doit etre refuse");
            Assert.That(error, Does.Contain("FearSensitivity"));
        }

        [Test]
        public void RageTuningCatalogValidationRejectsTuningWithNonFiniteMaxFearValue()
        {
            var catalog = NewCatalog(NewTuning("rage_default", 100f, float.NaN, 1f, 1f, StandardThresholds()));

            string error;
            Assert.That(catalog.TryValidate(out error), Is.False, "un catalogue contenant une borne de peur invalide doit etre refuse");
            Assert.That(error, Does.Contain("MaxFearValue"));
        }

        // ---------------------------------------------------------------- Asset livre

        [Test]
        public void RageTuningDefAssetCarriesTheDeliveredFearTuning()
        {
            var tuning = AssetDatabase.LoadAssetAtPath<RageTuningDef>(RageTuningDefAssetPath);
            Assert.That(tuning != null, Is.True, "tuning introuvable : " + RageTuningDefAssetPath);

            Assert.That(tuning.MaxFearValue, Is.EqualTo(100f));
            Assert.That(tuning.RageSensitivity, Is.EqualTo(1f));
            Assert.That(tuning.FearSensitivity, Is.EqualTo(1f));

            Assert.That(tuning.TryValidate(out var error), Is.True, error);
        }

        // ---------------------------------------------------------------- Helpers

        private RageTuningDef NewTuning(
            string id,
            float maxRageValue,
            float maxFearValue,
            float rageSensitivity,
            float fearSensitivity,
            params RageTuningDef.RageThreshold[] thresholds)
        {
            var tuning = ScriptableObject.CreateInstance<RageTuningDef>();
            SetPrivateField(tuning, "id", id);
            SetPrivateField(tuning, "maxRageValue", maxRageValue);
            SetPrivateField(tuning, "maxFearValue", maxFearValue);
            SetPrivateField(tuning, "rageSensitivity", rageSensitivity);
            SetPrivateField(tuning, "fearSensitivity", fearSensitivity);
            SetPrivateField(tuning, "thresholds", thresholds ?? new RageTuningDef.RageThreshold[0]);
            spawned.Add(tuning);
            return tuning;
        }

        private RageTuningDef NewStandardTuning(
            string id = "rage_default",
            float maxFearValue = 100f,
            float rageSensitivity = 1f,
            float fearSensitivity = 1f)
        {
            return NewTuning(id, 100f, maxFearValue, rageSensitivity, fearSensitivity, StandardThresholds());
        }

        /// <summary>Gabarit de paliers repris de RageTuningDef_Default.asset (seuils placeholder de la Story 4.1).</summary>
        private static RageTuningDef.RageThreshold[] StandardThresholds()
        {
            return new[]
            {
                new RageTuningDef.RageThreshold(RageDisposition.Irritated, 20f),
                new RageTuningDef.RageThreshold(RageDisposition.Flee, 40f),
                new RageTuningDef.RageThreshold(RageDisposition.Block, 60f),
                new RageTuningDef.RageThreshold(RageDisposition.Ram, 80f),
                new RageTuningDef.RageThreshold(RageDisposition.ConfrontationCapable, 95f)
            };
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
            var root = new GameObject("Story51RageStateTarget");
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
