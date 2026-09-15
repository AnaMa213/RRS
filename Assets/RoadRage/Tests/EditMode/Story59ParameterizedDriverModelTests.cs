using System.IO;
using System.Reflection;
using NUnit.Framework;
using RoadRage.Features.Vehicles;
using RoadRage.Shared.Domain;
using UnityEditor;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.9 : modele de conduite parametre (IDM/MOBIL). Couvre la matrice d'E/S de la spec sur
    /// les fonctions pures de <see cref="DriverModel"/>, l'authoring du profil
    /// (<see cref="DriverProfileDef"/> et son asset par defaut), et les gardes de source qui
    /// garantissent qu'aucune valeur de conduite ne survit dans le controleur. Deterministe, sans
    /// scene ni Netcode.
    /// </summary>
    public sealed class Story59ParameterizedDriverModelTests
    {
        private const string DriverControllerSourcePath = "Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs";
        private const string DriverProfileDefAssetPath = "Assets/RoadRage/ScriptableObjects/Vehicles/DriverProfileDef_Default.asset";
        private const string AiVehiclePrefabPath = "Assets/RoadRage/Prefabs/Greybox_AIVehicle.prefab";
        private const string FeaturesRootPath = "Assets/RoadRage";

        /// <summary>Profil authore de reference, aligne sur DriverProfileDef_Default.asset.</summary>
        private static DriverProfile Baseline()
        {
            return new DriverProfile(
                desiredSpeed: 8f,
                timeHeadway: 1.5f,
                minimumGap: 2f,
                maxAcceleration: 1.5f,
                comfortableDeceleration: 2f,
                politeness: 0.25f,
                laneChangeThreshold: 0.2f,
                safeBrakingLimit: 4f,
                reactionTime: 0.3f,
                laneChangeEvaluationInterval: 1f,
                consistency: 0.8f);
        }

        // ---------------------------------------------------------------- IDM

        [Test]
        public void FreeRoadAcceleratesAndFadesAsSpeedApproachesDesiredSpeed()
        {
            var profile = Baseline();

            var atRest = DriverModel.ComputeAcceleration(profile, 0f, 0f, DriverModel.NoLeaderGap);
            var nearlyThere = DriverModel.ComputeAcceleration(profile, profile.DesiredSpeed * 0.95f, 0f, DriverModel.NoLeaderGap);

            Assert.That(atRest, Is.GreaterThan(0f), "Route libre sous v0 : acceleration positive.");
            Assert.That(nearlyThere, Is.GreaterThan(0f));
            Assert.That(nearlyThere, Is.LessThan(atRest), "L'acceleration tend vers 0 quand la vitesse approche v0.");
        }

        [Test]
        public void AtDesiredSpeedAccelerationIsZeroAndNeverPositive()
        {
            var profile = Baseline();

            Assert.That(DriverModel.ComputeAcceleration(profile, profile.DesiredSpeed, 0f, DriverModel.NoLeaderGap),
                Is.EqualTo(0f).Within(0.0001f));
            Assert.That(DriverModel.ComputeAcceleration(profile, profile.DesiredSpeed * 1.1f, 0f, DriverModel.NoLeaderGap),
                Is.LessThan(0f), "Au-dela de v0 le vehicule ne doit jamais accelerer.");
        }

        [Test]
        public void StoppedLeaderProducesFirmFiniteBraking()
        {
            var acceleration = DriverModel.ComputeAcceleration(Baseline(), speed: 8f, leaderSpeed: 0f, gap: 5f);

            Assert.That(float.IsFinite(acceleration), Is.True);
            Assert.That(acceleration, Is.LessThan(-Baseline().ComfortableDeceleration),
                "Leader a l'arret a faible distance : deceleration franche.");
        }

        [Test]
        public void ZeroGapProducesBoundedFiniteDeceleration()
        {
            var profile = Baseline();

            foreach (var gap in new[] { 0f, -1f, float.NaN })
            {
                var acceleration = DriverModel.ComputeAcceleration(profile, speed: 8f, leaderSpeed: 0f, gap: gap);

                Assert.That(float.IsNaN(acceleration), Is.False, "gap=" + gap + " : jamais NaN, sinon le linearVelocity est contamine.");
                Assert.That(float.IsInfinity(acceleration), Is.False, "gap=" + gap + " : jamais Inf.");
                Assert.That(acceleration, Is.LessThan(0f).Or.EqualTo(0f));
                Assert.That(acceleration, Is.GreaterThan(-1000f), "gap=" + gap + " : deceleration bornee.");
            }
        }

        // ---------------------------------------------------------------- MOBIL

        [Test]
        public void LaneChangeIsAcceptedWhenGainExceedsThresholdAndSafetyHolds()
        {
            var accepted = DriverModel.TryEvaluateLaneChange(
                Baseline(),
                selfAccelerationBefore: -2f,
                selfAccelerationAfter: 1f,
                newFollowerAccelerationBefore: 0f,
                newFollowerAccelerationAfter: -1f,
                oldFollowerAccelerationBefore: 0f,
                oldFollowerAccelerationAfter: 0f,
                out var gain);

            Assert.That(gain, Is.GreaterThan(Baseline().LaneChangeThreshold));
            Assert.That(accepted, Is.True);
        }

        [Test]
        public void SafetyVetoRefusesTheLaneChangeWhateverTheGain()
        {
            var profile = Baseline();

            // Le nouveau suiveur devrait freiner plus fort que b_safe : veto, malgre un gain enorme.
            var accepted = DriverModel.TryEvaluateLaneChange(
                profile,
                selfAccelerationBefore: -9f,
                selfAccelerationAfter: 1.5f,
                newFollowerAccelerationBefore: 0f,
                newFollowerAccelerationAfter: -(profile.SafeBrakingLimit + 0.5f),
                oldFollowerAccelerationBefore: 0f,
                oldFollowerAccelerationAfter: 0f,
                out var gain);

            Assert.That(accepted, Is.False, "Le critere de securite s'oppose independamment du gain calcule.");
            Assert.That(gain, Is.GreaterThan(profile.LaneChangeThreshold), "Le gain reste calcule et expose meme quand le veto tombe.");
        }

        [Test]
        public void NegativePolitenessRaisesTheGainWhenTheChangeHurtsOthers()
        {
            var polite = Baseline();
            var malicious = new DriverProfile(
                polite.DesiredSpeed, polite.TimeHeadway, polite.MinimumGap, polite.MaxAcceleration,
                polite.ComfortableDeceleration, politeness: -0.5f, laneChangeThreshold: polite.LaneChangeThreshold,
                safeBrakingLimit: polite.SafeBrakingLimit, reactionTime: polite.ReactionTime,
                laneChangeEvaluationInterval: polite.LaneChangeEvaluationInterval, consistency: polite.Consistency);

            DriverModel.TryEvaluateLaneChange(polite, 0f, 0.5f, 0f, -1f, 0f, -1f, out var politeGain);
            DriverModel.TryEvaluateLaneChange(malicious, 0f, 0.5f, 0f, -1f, 0f, -1f, out var maliciousGain);

            Assert.That(maliciousGain, Is.GreaterThan(politeGain),
                "p < 0 : penaliser autrui augmente le gain percu (cadran de malveillance de MOBIL).");
        }

        [Test]
        public void LaneChangeGatingFollowsTheAuthoredIntervalOfTheProfile()
        {
            var profile = Baseline();

            Assert.That(DriverModel.ShouldEvaluateLaneChange(profile.LaneChangeEvaluationInterval - 0.01f, profile.LaneChangeEvaluationInterval), Is.False);
            Assert.That(DriverModel.ShouldEvaluateLaneChange(profile.LaneChangeEvaluationInterval, profile.LaneChangeEvaluationInterval), Is.True);

            // Appelee a chaque FixedUpdate : au plus une evaluation par intervalle authore.
            const float FixedDeltaTime = 0.02f;
            var elapsed = 0f;
            var evaluations = 0;
            for (var step = 0; step < 500; step++)
            {
                elapsed += FixedDeltaTime;
                if (DriverModel.ShouldEvaluateLaneChange(elapsed, profile.LaneChangeEvaluationInterval))
                {
                    elapsed = 0f;
                    evaluations++;
                }
            }

            // 500 pas de 0.02 s = 10 s, soit au plus 10 evaluations a 1 s d'intervalle (l'accumulation
            // en flottants peut reporter la derniere d'une frame).
            Assert.That(evaluations, Is.InRange(9, 10));
        }

        [Test]
        public void TwoVehiclesCreatedAtTheSameInstantDoNotEvaluateOnTheSameFrame()
        {
            const float Interval = 1f;
            const float FixedDeltaTime = 0.02f;

            // Phase par instance : le controleur seed son compteur a phase * intervalle au spawn.
            var firstElapsed = 0.1f * Interval;
            var secondElapsed = 0.7f * Interval;
            var firstFrame = -1;
            var secondFrame = -1;

            for (var step = 0; step < 100; step++)
            {
                firstElapsed += FixedDeltaTime;
                secondElapsed += FixedDeltaTime;

                if (firstFrame < 0 && DriverModel.ShouldEvaluateLaneChange(firstElapsed, Interval))
                {
                    firstFrame = step;
                }

                if (secondFrame < 0 && DriverModel.ShouldEvaluateLaneChange(secondElapsed, Interval))
                {
                    secondFrame = step;
                }
            }

            Assert.That(firstFrame, Is.GreaterThanOrEqualTo(0));
            Assert.That(secondFrame, Is.GreaterThanOrEqualTo(0));
            Assert.That(firstFrame, Is.Not.EqualTo(secondFrame), "Les phases distinctes desynchronisent les evaluations.");
        }

        // ---------------------------------------------------------------- Dispositions

        [Test]
        public void ImmobilizingDispositionsZeroTheEffectiveDesiredSpeed()
        {
            foreach (var disposition in new[] { RageDisposition.Block, RageDisposition.ConfrontationCapable })
            {
                var effective = DriverModel.ResolveEffectiveProfile(Baseline(), disposition);
                Assert.That(effective.DesiredSpeed, Is.EqualTo(0f), disposition + " : v0 effectif nul.");

                // Un vehicule deja lance freine ; a l'arret il ne repart pas.
                Assert.That(DriverModel.ComputeAcceleration(effective, speed: 5f, leaderSpeed: 0f, gap: DriverModel.NoLeaderGap), Is.LessThan(0f));
                Assert.That(DriverModel.ComputeAcceleration(effective, speed: 0f, leaderSpeed: 0f, gap: DriverModel.NoLeaderGap), Is.EqualTo(0f));
            }
        }

        [Test]
        public void DispositionModulationLeavesThePersonalityParametersUntouched()
        {
            var profile = Baseline();

            foreach (var disposition in new[] { RageDisposition.Calm, RageDisposition.Irritated, RageDisposition.Flee, RageDisposition.Block, RageDisposition.Ram, RageDisposition.ConfrontationCapable })
            {
                var effective = DriverModel.ResolveEffectiveProfile(profile, disposition);

                Assert.That(effective.ReactionTime, Is.EqualTo(profile.ReactionTime), disposition.ToString());
                Assert.That(effective.LaneChangeEvaluationInterval, Is.EqualTo(profile.LaneChangeEvaluationInterval), disposition.ToString());
                Assert.That(effective.Consistency, Is.EqualTo(profile.Consistency), disposition.ToString());
            }
        }

        // ---------------------------------------------------------------- Reaction

        [Test]
        public void SmoothedAccelerationConvergesTowardTheTargetWithoutOvershoot()
        {
            const float Target = 1.5f;
            const float FixedDeltaTime = 0.02f;

            var applied = 0f;
            var previous = float.NegativeInfinity;

            for (var step = 0; step < 100; step++)
            {
                applied = DriverModel.SmoothAcceleration(applied, Target, reactionTime: 0.3f, deltaTime: FixedDeltaTime);

                Assert.That(applied, Is.LessThanOrEqualTo(Target + 0.0001f), "Loi de premier ordre : jamais de depassement.");
                Assert.That(applied, Is.GreaterThan(previous), "Convergence monotone vers la cible.");
                previous = applied;
            }

            Assert.That(applied, Is.EqualTo(Target).Within(0.01f));
        }

        [Test]
        public void ANearInstantReactionTimeTracksTheTargetFromTheFirstStep()
        {
            const float Target = 2f;
            var fast = DriverModel.SmoothAcceleration(0f, Target, reactionTime: DriverModel.MinReactionTime, deltaTime: 0.02f);
            var slow = DriverModel.SmoothAcceleration(0f, Target, reactionTime: 1f, deltaTime: 0.02f);

            Assert.That(fast, Is.GreaterThan(0.8f * Target), "Reaction au plancher : suit la cible de tres pres des le premier pas.");
            Assert.That(fast, Is.LessThanOrEqualTo(Target), "Jamais de depassement, meme au plancher.");
            Assert.That(slow, Is.LessThan(0.1f * Target), "Reaction lente : la cible n'est approchee qu'apres plusieurs pas.");
        }

        [Test]
        public void AZeroOrNegativeReactionTimeIsClampedToAPositiveFloorAndStaysFinite()
        {
            foreach (var reactionTime in new[] { 0f, -1f, float.NaN })
            {
                var applied = DriverModel.SmoothAcceleration(0f, 2f, reactionTime, 0.02f);
                Assert.That(float.IsFinite(applied), Is.True, "reactionTime=" + reactionTime);
                Assert.That(applied, Is.GreaterThan(0f).And.LessThanOrEqualTo(2f));
            }
        }

        // ---------------------------------------------------------------- Bruit de personnalite

        [Test]
        public void APerfectlyConsistentDriverHasNoDesiredSpeedNoise()
        {
            for (var step = 0; step < 50; step++)
            {
                var time = step * 0.37f;
                Assert.That(DriverModel.ResolveNoisyDesiredSpeed(8f, consistency: 1f, time: time, phase: 0.42f),
                    Is.EqualTo(8f), "consistency = 1 : bruit nul a tout instant.");
            }
        }

        [Test]
        public void AnErraticDriverStaysWithinTheSharedEnvelope()
        {
            const float DesiredSpeed = 8f;
            const float MaxEnvelope = 0.06f;
            var sawDeviation = false;

            for (var step = 0; step < 2000; step++)
            {
                var noisy = DriverModel.ResolveNoisyDesiredSpeed(DesiredSpeed, consistency: 0f, time: step * 0.05f, phase: 0.13f);

                Assert.That(float.IsFinite(noisy), Is.True);
                Assert.That(Mathf.Abs(noisy - DesiredSpeed), Is.LessThanOrEqualTo((MaxEnvelope * DesiredSpeed) + 0.0001f),
                    "consistency = 0 : jamais au-dela de l'enveloppe maximale partagee.");

                if (Mathf.Abs(noisy - DesiredSpeed) > 0.01f)
                {
                    sawDeviation = true;
                }
            }

            Assert.That(sawDeviation, Is.True, "Un conducteur erratique doit effectivement flotter.");
        }

        [Test]
        public void TheNoiseIsDeterministicForTheSameConsistencyPhaseAndInstant()
        {
            var first = DriverModel.ResolveNoisyDesiredSpeed(8f, 0.2f, 12.5f, 0.77f);
            var second = DriverModel.ResolveNoisyDesiredSpeed(8f, 0.2f, 12.5f, 0.77f);

            Assert.That(second, Is.EqualTo(first), "Aucun Random : meme entree, meme sortie.");
            Assert.That(DriverModel.ResolveNoisyDesiredSpeed(8f, 0.2f, 12.5f, 0.11f), Is.Not.EqualTo(first),
                "Deux phases distinctes ne produisent pas le meme bruit au meme instant.");
        }

        // ---------------------------------------------------------------- Personnalite mesurable

        [Test]
        public void TwoProfilesSharingTheSameDesiredSpeedStillDivergeMeasurably()
        {
            var calmDriver = new DriverProfile(8f, 1.5f, 2f, 1.5f, 2f, 0.25f, 0.2f, 4f, reactionTime: 0.15f, laneChangeEvaluationInterval: 0.5f, consistency: 1f);
            var twitchyDriver = new DriverProfile(8f, 1.5f, 2f, 1.5f, 2f, 0.25f, 0.2f, 4f, reactionTime: 1.2f, laneChangeEvaluationInterval: 2.5f, consistency: 0f);

            Assert.That(calmDriver.DesiredSpeed, Is.EqualTo(twitchyDriver.DesiredSpeed), "Meme v0 : c'est le point du test.");

            var calmSpeed = IntegrateSpeed(calmDriver, steps: 100, fixedDeltaTime: 0.02f);
            var twitchySpeed = IntegrateSpeed(twitchyDriver, steps: 100, fixedDeltaTime: 0.02f);

            Assert.That(Mathf.Abs(calmSpeed - twitchySpeed), Is.GreaterThan(0.1f),
                "Des reactionTime differents produisent des vitesses instantanees mesurablement differentes.");
            Assert.That(CountLaneChangeEvaluations(calmDriver, steps: 500, fixedDeltaTime: 0.02f),
                Is.GreaterThan(CountLaneChangeEvaluations(twitchyDriver, steps: 500, fixedDeltaTime: 0.02f)),
                "Des intervalles differents produisent des cadences de recherche differentes.");
        }

        // ---------------------------------------------------------------- Authoring

        [Test]
        public void TheDefaultDriverProfileAssetIsAuthoredAndValid()
        {
            var def = AssetDatabase.LoadAssetAtPath<DriverProfileDef>(DriverProfileDefAssetPath);
            Assert.That(def != null, Is.True, "asset introuvable : " + DriverProfileDefAssetPath);
            Assert.That(def.TryValidate(out var error), Is.True, error);
            Assert.That(def.RawId, Is.EqualTo(def.RawId.ToLowerInvariant()), "Id minuscule stable.");
            Assert.That(def.Id.Value, Is.EqualTo(def.RawId));

            var profile = def.Profile;
            Assert.That(profile.DesiredSpeed, Is.GreaterThan(0f));
            Assert.That(profile.TimeHeadway, Is.GreaterThan(0f));
            Assert.That(profile.MaxAcceleration, Is.GreaterThan(0f));
            Assert.That(profile.ComfortableDeceleration, Is.GreaterThan(0f));
            Assert.That(profile.SafeBrakingLimit, Is.GreaterThan(0f));
            Assert.That(profile.ReactionTime, Is.GreaterThanOrEqualTo(DriverModel.MinReactionTime));
            Assert.That(profile.LaneChangeEvaluationInterval, Is.GreaterThanOrEqualTo(DriverModel.MinLaneChangeEvaluationInterval));
            Assert.That(profile.Consistency, Is.InRange(0f, 1f));
        }

        [Test]
        public void TheDefRejectsValuesThatWouldBreakTheModel()
        {
            var def = ScriptableObject.CreateInstance<DriverProfileDef>();
            try
            {
                SetPrivateField(def, "id", "driver_broken");
                SetPrivateField(def, "profile", new DriverProfile(8f, 1.5f, 2f, 0f, 2f, 0.25f, 0.2f, 4f, 0.3f, 1f, 0.8f));
                Assert.That(def.TryValidate(out var error), Is.False, "a = 0 divise dans l'IDM : doit etre refuse.");
                Assert.That(error, Does.Contain("MaxAcceleration"));

                SetPrivateField(def, "id", "Driver_Broken");
                SetPrivateField(def, "profile", new DriverProfile(8f, 1.5f, 2f, 1.5f, 2f, 0.25f, 0.2f, 4f, 0.3f, 1f, 0.8f));
                Assert.That(def.TryValidate(out _), Is.False, "Id non minuscule : doit etre refuse.");
            }
            finally
            {
                Object.DestroyImmediate(def);
            }
        }

        [Test]
        public void TheAiVehiclePrefabCarriesTheDefaultDriverProfile()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AiVehiclePrefabPath);
            Assert.That(prefab != null, Is.True, "prefab introuvable : " + AiVehiclePrefabPath);

            var controller = prefab.GetComponent<NetworkedAIVehicleDriverController>();
            Assert.That(controller != null, Is.True);

            var assigned = GetPrivateField(controller, "driverProfile") as DriverProfileDef;
            Assert.That(assigned != null, Is.True,
                "Le profil vient du prefab : les trois instances de MVP_Run en heritent sans edition de scene.");
            Assert.That(AssetDatabase.GetAssetPath(assigned), Is.EqualTo(DriverProfileDefAssetPath));
        }

        // ---------------------------------------------------------------- Gardes de source

        [Test]
        public void TheSpeedOnlyDriveProfileResolverNoLongerExistsAnywhereInTheCodebase()
        {
            // Nom assemble : ce fichier ne doit pas etre lui-meme une occurrence du symbole disparu.
            var removedSymbol = "ResolveCruiseSpeed" + "Multiplier";

            foreach (var path in Directory.GetFiles(FeaturesRootPath, "*.cs", SearchOption.AllDirectories))
            {
                Assert.That(File.ReadAllText(path), Does.Not.Contain(removedSymbol),
                    path + " : le reglage de conduite exprime uniquement en vitesse a disparu (Story 5.9).");
            }
        }

        [Test]
        public void TheControllerHoldsNoDriveConstantsAndReadsEveryParameterFromTheDef()
        {
            var source = File.ReadAllText(DriverControllerSourcePath);

            Assert.That(source, Does.Not.Contain("cruiseSpeed"), "Le champ de vitesse cable a ete retire.");
            Assert.That(source, Does.Contain("private DriverProfileDef driverProfile"), "Le profil est serialise sur le controleur.");
            Assert.That(source, Does.Contain("DriverModel.ResolveEffectiveProfile(driverProfile.Profile, behavior)"),
                "Les onze parametres viennent du Def, modules par la disposition publiee.");
            Assert.That(source, Does.Contain("DriverModel.ComputeAcceleration("), "L'acceleration est calculee par le modele, pas par le controleur.");
            Assert.That(source, Does.Contain("DriverModel.SmoothAcceleration("));
            Assert.That(source, Does.Contain("DriverModel.ShouldEvaluateLaneChange("));
            Assert.That(source, Does.Contain("DriverModel.ResolveNoisyDesiredSpeed("));
        }

        [Test]
        public void AMissingProfileLeavesTheVehicleInertWithASingleWarningAndNoHardCodedFallback()
        {
            var source = File.ReadAllText(DriverControllerSourcePath);

            Assert.That(source, Does.Contain("if (driverProfile == null)"), "Profil absent : sortie avant tout mouvement.");
            Assert.That(source, Does.Contain("warnedMissingDriverProfile"), "Avertissement emis une seule fois, pas a chaque tick.");
            Assert.That(source, Does.Contain("Debug.LogWarning"));
            Assert.That(source, Does.Not.Contain("new DriverProfile("),
                "Aucun repli code en dur : le vehicule reste inerte, comme quand la route est absente.");
        }

        // ------------------------------------------- Arret voulu vs blocage (correctif Play Mode)

        [Test]
        public void StoppingBehindALeaderIsADeliberateStopNotABlockage()
        {
            var profile = Baseline();

            Assert.That(DriverModel.IsDeliberateStop(hasLeader: true, gap: profile.MinimumGap, profile.MinimumGap), Is.True,
                "Arret a l'ecart minimal authore : voulu, donc jamais alimente la detection de blocage.");
            Assert.That(DriverModel.IsDeliberateStop(hasLeader: true, gap: profile.MinimumGap * 4f, profile.MinimumGap), Is.True,
                "Arret bien en amont du leader : voulu aussi.");
        }

        [Test]
        public void NoLeaderMeansNoDeliberateStopSoARealWedgeStillCounts()
        {
            var profile = Baseline();

            Assert.That(DriverModel.IsDeliberateStop(hasLeader: false, gap: DriverModel.NoLeaderGap, profile.MinimumGap), Is.False,
                "Rien devant soi : une immobilisation est un encastrement, pas un arret voulu.");
            Assert.That(DriverModel.IsDeliberateStop(hasLeader: false, gap: 0.2f, profile.MinimumGap), Is.False);
        }

        [Test]
        public void ACollapsedGapIsAWedgeNotADeliberateStop()
        {
            var profile = Baseline();

            // Nez contre pare-chocs : le vehicule n'est pas arrete proprement derriere un leader, il
            // est encastre. Le compteur de blocage doit reprendre son cours.
            Assert.That(DriverModel.IsDeliberateStop(hasLeader: true, gap: 0f, profile.MinimumGap), Is.False);
            Assert.That(DriverModel.IsDeliberateStop(hasLeader: true, gap: profile.MinimumGap * 0.1f, profile.MinimumGap), Is.False);
        }

        [Test]
        public void TheControllerNeverTeleportsAVehicleThatIsDeliberatelyStoppedOrWatchedByAPlayer()
        {
            var source = File.ReadAllText(DriverControllerSourcePath);

            Assert.That(source, Does.Contain("DriverModel.IsDeliberateStop("),
                "L'arret voulu derriere un leader court-circuite la detection de blocage.");
            Assert.That(source, Does.Contain("!IsAnyPlayerWithinClearanceRadius()"),
                "La teleportation de recuperation est interdite tant qu'un joueur est assez proche pour la voir.");
            Assert.That(source, Does.Not.Contain("FindObjectsOfType"),
                "La proximite joueur est une requete de physique bornee, pas un balayage de scene.");
        }

        [Test]
        public void TheIdmSettlesAtTheAuthoredMinimumGapBehindAStoppedLeader()
        {
            var profile = Baseline();

            // Equilibre de l'IDM a l'arret : a vitesse nulle, s* vaut exactement s0, donc
            // l'acceleration s'annule quand l'ecart vaut l'ecart minimal authore. C'est cette
            // propriete qui garantit qu'un embouteillage s'espace au lieu de se tasser -- a
            // condition que l'ecart mesure soit bien pare-chocs a pare-chocs.
            var atGap = DriverModel.ComputeAcceleration(profile, speed: 0f, leaderSpeed: 0f, gap: profile.MinimumGap);
            Assert.That(atGap, Is.EqualTo(0f).Within(0.001f),
                "A l'arret et a l'ecart minimal, l'IDM est a l'equilibre : ni avance ni recul.");

            var tooClose = DriverModel.ComputeAcceleration(profile, speed: 0f, leaderSpeed: 0f, gap: profile.MinimumGap * 0.5f);
            Assert.That(tooClose, Is.LessThan(0f),
                "Plus pres que l'ecart minimal : l'IDM repousse, il ne laisse jamais avancer.");

            var farther = DriverModel.ComputeAcceleration(profile, speed: 0f, leaderSpeed: 0f, gap: profile.MinimumGap * 3f);
            Assert.That(farther, Is.GreaterThan(0f), "Plus loin que l'ecart minimal : il peut repartir.");
        }

        [Test]
        public void LeaderDetectionMeasuresTheGapFromTheBumperNotTheCenterOfMass()
        {
            var source = File.ReadAllText(DriverControllerSourcePath);

            // Cause racine de l'embouteillage qui se tasse : mesurer depuis worldCenterOfMass ajoute
            // la propre demi-longueur du vehicule (2,22 m sur le greybox) a l'ecart, donc
            // l'equilibre de l'IDM a s0 = 2 m tombait A L'INTERIEUR du leader.
            Assert.That(source, Does.Contain("frontOffset"),
                "L'ecart part du pare-chocs : la demi-longueur du vehicule est retiree de la mesure.");
            Assert.That(source, Does.Not.Contain("var origin = body.worldCenterOfMass;"),
                "Le depart au centre de masse a ete retire.");
            Assert.That(source, Does.Contain("frontOffset - scanRadius"),
                "Le centre de la sphere recule d'un rayon pour que son bord avant parte du pare-chocs.");
        }

        [Test]
        public void LeaderDetectionSweepsAVolumeAlongTheSteeredHeadingNotAThinNoseRay()
        {
            var source = File.ReadAllText(DriverControllerSourcePath);

            Assert.That(source, Does.Contain("Physics.SphereCastNonAlloc("),
                "Un rayon d'epaisseur nulle ne voit pas un obstacle decale d'un demi-vehicule.");
            Assert.That(source, Does.Contain("ResolveScanDirection()"),
                "En virage, viser le seul nez fait manquer l'obstacle en sortie de courbe.");
        }

        [Test]
        public void LeaderDetectionSeesWalkingPlayersNotJustRigidbodies()
        {
            var source = File.ReadAllText(DriverControllerSourcePath);

            // Le joueur a pied est un CharacterController (LocalOnFootController), sans Rigidbody :
            // ne filtrer que sur Rigidbody rendait l'IA aveugle aux pietons -- elle les encastrait
            // au lieu de freiner. C'est la cause racine du symptome de teleportation observe.
            Assert.That(source, Does.Contain("GetComponentInParent<CharacterController>()"),
                "Un pieton est un obstacle mobile : il doit compter comme leader.");
            Assert.That(source, Does.Not.Contain("if (hit.rigidbody == null || hit.rigidbody == body)"),
                "Le filtre qui excluait les pietons a ete retire.");
        }

        [Test]
        public void PhysicsQueryBuffersAreMarginedWellBeyondCurrentSceneDensityAndWarnOnSaturation()
        {
            var source = File.ReadAllText(DriverControllerSourcePath);

            // MVP_Run ne porte que 9 colliders au total : un tampon sature aujourd'hui trahirait un
            // depassement pur (pas un pic realiste de densite), donc la marge doit rester tres large.
            Assert.That(source, Does.Contain("new RaycastHit[32]"),
                "Le tampon de detection de leader est marge tres au-dela des 9 colliders actuels de MVP_Run.");
            Assert.That(source, Does.Contain("new Collider[48]"),
                "Le tampon de proximite joueur est marge tres au-dela des 9 colliders actuels de MVP_Run.");

            // Un depassement de tampon NonAlloc ne plante jamais : il tronque silencieusement. Sans
            // avertissement, un leader ou un joueur proche manque sans aucun signal en Console.
            Assert.That(source, Does.Contain("warnedLeaderBufferSaturated"),
                "Saturation du tampon de leader : avertissement une seule fois, pas un plantage silencieux.");
            Assert.That(source, Does.Contain("warnedClearanceBufferSaturated"),
                "Saturation du tampon de proximite : avertissement une seule fois, pas un plantage silencieux.");
        }

        [Test]
        public void TheModelKeepsNoStateBetweenCallsAndOwnsTheNoiseEnvelope()
        {
            var source = File.ReadAllText("Assets/RoadRage/Features/Vehicles/DriverModel.cs");

            // Cible l'APPEL, pas le mot : le commentaire de ResolveNoisyDesiredSpeed dit lui-meme
            // "aucun Random", et une garde sur le mot nu se declencherait sur sa propre prose.
            Assert.That(source, Does.Not.Contain("Random."),
                "Le bruit est deterministe : aucun appel a Random.Range / Random.value.");
            Assert.That(source, Does.Not.Contain("Random("),
                "Le bruit est deterministe : aucun generateur instancie (System.Random comme UnityEngine.Random).");
            Assert.That(source, Does.Not.Contain("Guid"), "Aucune source d'entropie detournee.");
            Assert.That(source, Does.Not.Contain("List<"), "Aucune collection statique partagee entre vehicules.");
            Assert.That(source, Does.Not.Contain("Queue<"), "Aucun tampon d'historique de reaction.");
            Assert.That(source, Does.Contain("NoiseAmplitudeFraction"),
                "L'enveloppe de bruit est une constante partagee du modele, pas un champ du Def pour cette story.");
        }

        // ---------------------------------------------------------------- Helpers

        /// <summary>Reproduit la boucle d'integration du controleur avec les seules fonctions pures.</summary>
        private static float IntegrateSpeed(DriverProfile profile, int steps, float fixedDeltaTime)
        {
            var speed = 0f;
            var acceleration = 0f;

            for (var step = 0; step < steps; step++)
            {
                var time = step * fixedDeltaTime;
                var noisy = DriverModel.ResolveNoisyDesiredSpeed(profile.DesiredSpeed, profile.Consistency, time, phase: 0.31f);
                var target = DriverModel.ComputeAcceleration(profile.WithDesiredSpeed(noisy), speed, 0f, DriverModel.NoLeaderGap);
                acceleration = DriverModel.SmoothAcceleration(acceleration, target, profile.ReactionTime, fixedDeltaTime);
                speed = Mathf.Max(0f, speed + (acceleration * fixedDeltaTime));
            }

            return speed;
        }

        private static int CountLaneChangeEvaluations(DriverProfile profile, int steps, float fixedDeltaTime)
        {
            var elapsed = 0f;
            var evaluations = 0;

            for (var step = 0; step < steps; step++)
            {
                elapsed += fixedDeltaTime;
                if (DriverModel.ShouldEvaluateLaneChange(elapsed, profile.LaneChangeEvaluationInterval))
                {
                    elapsed = 0f;
                    evaluations++;
                }
            }

            return evaluations;
        }

        private static object GetPrivateField(object target, string fieldName)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "field not found: " + fieldName);
            return field.GetValue(target);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "field not found: " + fieldName);
            field.SetValue(target, value);
        }
    }
}
