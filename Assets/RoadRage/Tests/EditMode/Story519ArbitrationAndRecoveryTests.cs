using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using RoadRage.Features.Vehicles;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Revue post-5.18. Deux defauts STRUCTURELS ont ete mesures, et ce banc les tient fermes.
    ///
    /// 1. L'arbitrage de jonction notait un TOURNOI sur un ensemble filtre par observateur, donc
    ///    deux vehicules pouvaient se croire prioritaires en meme temps.
    /// 2. La recuperation ne pouvait pas se terminer quand une manoeuvre laissait le vehicule a
    ///    contresens : rien ne detectait le contresens en conduite nominale, aucun palier ne
    ///    reorientait, et la sortie d'episode etait inatteignable.
    /// </summary>
    [Category("Story519")]
    public sealed class Story519ArbitrationAndRecoveryTests
    {
        private const string TrafficSource =
            "Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.Traffic.cs";

        // ----------------------------------------------------------------- arbitrage multi-agents

        /// <summary>
        /// Le cas mesure : A et C ne se croisent pas, B croise les deux. Avec un ensemble filtre sur
        /// soi, A et B -- qui SE CROISENT -- entraient ensemble.
        /// </summary>
        [Test]
        public void TwoConflictingMovementsAreNeverAdmittedTogether()
        {
            var claims = ThreeWayWithOneNonCrossingPair();

            var admitted = Admitted(claims);

            Assert.That(admitted, Is.Not.Empty, "Un ensemble en conflit doit toujours laisser passer quelqu'un.");
            for (var i = 0; i < admitted.Count; i++)
            {
                for (var j = i + 1; j < admitted.Count; j++)
                {
                    Assert.That(JunctionRules.Conflicts(claims[admitted[i]], claims[admitted[j]]), Is.False,
                        "Deux mouvements admis ensemble ne doivent jamais se croiser.");
                }
            }
        }

        /// <summary>
        /// Non-regression du defaut lui-meme : l'ancien arbitrage, rejoue ici sur l'ensemble FILTRE
        /// tel que le controleur le construisait, autorisait bien deux mouvements concurrents. Si ce
        /// test cesse d'echouer a l'ancienne facon, c'est que la demonstration a perdu son sens.
        /// </summary>
        [Test]
        public void TheFilteredSetWasTheDefectAndTheFullSetIsTheFix()
        {
            var claims = ThreeWayWithOneNonCrossingPair();

            var grantedByOldModel = new List<int>();
            for (var self = 0; self < claims.Count; self++)
            {
                // Reproduction de GatherJunctionClaimants d'avant la correction : soi, puis
                // uniquement les revendications en conflit avec soi.
                var set = new List<JunctionClaim> { claims[self] };
                for (var other = 0; other < claims.Count; other++)
                {
                    if (other == self) continue;
                    if (!JunctionRules.Conflicts(claims[self], claims[other])) continue;
                    set.Add(claims[other]);
                }

                var winner = JunctionRules.ResolveWinnerIndex(set, set.Count, new int[set.Count]);
                if (winner == 0 || winner < 0) grantedByOldModel.Add(self);
            }

            var oldModelLetConflictingPairThrough = false;
            for (var i = 0; i < grantedByOldModel.Count; i++)
            {
                for (var j = i + 1; j < grantedByOldModel.Count; j++)
                {
                    if (JunctionRules.Conflicts(claims[grantedByOldModel[i]], claims[grantedByOldModel[j]]))
                    {
                        oldModelLetConflictingPairThrough = true;
                    }
                }
            }

            Assert.That(oldModelLetConflictingPairThrough, Is.True,
                "Le cas de demonstration doit bien exhiber l'ancien defaut, sinon il ne prouve rien.");

            var admitted = Admitted(claims);
            Assert.That(admitted.Count, Is.EqualTo(1), "Sur cette configuration, un seul mouvement peut entrer.");
        }

        /// <summary>
        /// L'admission ne doit pas serialiser ce qui n'a pas besoin de l'etre : deux mouvements qui
        /// ne se croisent pas passent ensemble. Un vainqueur unique par jonction l'interdisait.
        /// </summary>
        [Test]
        public void MovementsThatDoNotCrossAreAdmittedTogether()
        {
            var claims = new List<JunctionClaim>
            {
                // Deux tout-droit paralleles, voies distinctes, qui ne se coupent pas.
                Claim(new Vector3(0, 0, -1), 5f, 10, 100, new Vector3(-2, 0, 4), new Vector3(-2, 0, -4)),
                Claim(new Vector3(0, 0, -1), 6f, 11, 101, new Vector3(-6, 0, 4), new Vector3(-6, 0, -4)),
            };

            Assert.That(JunctionRules.Conflicts(claims[0], claims[1]), Is.False,
                "Preuve du montage : ces deux mouvements ne se croisent pas.");
            Assert.That(Admitted(claims).Count, Is.EqualTo(2),
                "Deux mouvements sans conflit doivent entrer ensemble.");
        }

        /// <summary>
        /// VIVACITE, et c'est l'invariant que la recette demandait explicitement : quel que soit
        /// l'ensemble, fini et arbitraire, il en sort toujours au moins un admis. Un ensemble ou
        /// TOUT LE MONDE attend est l'interblocage artificiel que l'arbitrage doit rendre
        /// impossible -- y compris sur les cycles de priorite a droite a 3 et 4 revendiquants.
        /// </summary>
        [Test]
        public void AnyFiniteClaimSetAlwaysAdmitsAtLeastOneMovement()
        {
            var random = new System.Random(5190);
            var headings = new[]
            {
                new Vector3(0, 0, -1), new Vector3(0, 0, 1), new Vector3(1, 0, 0), new Vector3(-1, 0, 0),
            };

            for (var trial = 0; trial < 400; trial++)
            {
                var count = 2 + random.Next(5);
                var claims = new List<JunctionClaim>();
                for (var i = 0; i < count; i++)
                {
                    var forward = headings[random.Next(headings.Length)];
                    var right = Vector3.Cross(Vector3.up, forward);
                    var entry = -forward * 4f + right * (random.Next(2) == 0 ? 2f : -2f);
                    var exitHeading = headings[random.Next(headings.Length)];
                    var exit = exitHeading * 4f;
                    claims.Add(Claim(forward, 1f + random.Next(80) / 10f, 10 + i, (ulong)(100 + i), entry, exit));
                }

                var admitted = Admitted(claims);
                Assert.That(admitted, Is.Not.Empty,
                    "Essai " + trial + " : aucun revendiquant admis -- interblocage artificiel.");

                for (var i = 0; i < admitted.Count; i++)
                {
                    for (var j = i + 1; j < admitted.Count; j++)
                    {
                        Assert.That(JunctionRules.Conflicts(claims[admitted[i]], claims[admitted[j]]), Is.False,
                            "Essai " + trial + " : deux mouvements admis se croisent.");
                    }
                }
            }
        }

        /// <summary>
        /// Un occupant deja engage garde son mouvement : le lui reprendre l'immobiliserait au milieu
        /// de l'aire de conflit. L'admission doit conserver cette propriete de l'arbitrage par paire.
        /// </summary>
        [Test]
        public void ACommittedOccupantIsAlwaysAdmitted()
        {
            var entrant = Claim(new Vector3(0, 0, -1), 1f, 10, 100, new Vector3(-2, 0, 4), new Vector3(-2, 0, -4));
            var occupant = new JunctionClaim(1, JunctionApproachRule.PriorityToRight, new Vector3(-1, 0, 0),
                8f, 20, 200, true, true, true, false, new Vector3(4, 0, 2), new Vector3(-4, 0, 2), true);

            var claims = new List<JunctionClaim> { entrant, occupant };
            Assert.That(JunctionRules.Conflicts(entrant, occupant), Is.True, "Preuve du montage.");

            var admitted = Admitted(claims);
            Assert.That(admitted, Contains.Item(1), "L'occupant engage doit passer.");
            Assert.That(admitted, Is.Not.Contains(0), "L'entrant doit ceder a l'occupant.");
        }

        // ----------------------------------------------------------------- progres le long du chemin

        /// <summary>
        /// En courbe, la distance a vol d'oiseau n'est pas une mesure de progres. Sur un quart de
        /// cercle, l'arc restant decroit strictement a chaque pas, la ou la corde peut stagner.
        /// </summary>
        [Test]
        public void RemainingDistanceFollowsTheArcAndDecreasesAllAlongACurve()
        {
            const int count = 41;
            var path = new Vector3[count];
            for (var i = 0; i < count; i++)
            {
                var angle = Mathf.PI * 0.5f * i / (count - 1);
                path[i] = new Vector3(Mathf.Sin(angle) * 10f, 0f, 10f - Mathf.Cos(angle) * 10f);
            }

            var target = path[count - 1];
            var previous = float.PositiveInfinity;
            for (var i = 0; i < count - 1; i++)
            {
                var remaining = LaneGraphRouting.ResolvePathRemainingDistance(path, count, path[i], target);
                Assert.That(remaining, Is.GreaterThan(0f), "L'arc restant doit etre mesurable en tout point.");
                Assert.That(remaining, Is.LessThan(previous), "L'arc restant doit decroitre strictement.");
                previous = remaining;
            }

            var chord = Vector3.ProjectOnPlane(target - path[0], Vector3.up).magnitude;
            var arc = LaneGraphRouting.ResolvePathRemainingDistance(path, count, path[0], target);
            Assert.That(arc, Is.GreaterThan(chord),
                "Sur une courbe, l'arc est plus long que la corde : c'est tout l'objet de la mesure.");
        }

        [Test]
        public void ADegeneratePathNeverPretendsToMeasureProgress()
        {
            Assert.That(LaneGraphRouting.ResolvePathRemainingDistance(null, 0, Vector3.zero, Vector3.one),
                Is.LessThan(0f));
            Assert.That(LaneGraphRouting.ResolvePathRemainingDistance(new[] { Vector3.zero }, 1, Vector3.zero, Vector3.one),
                Is.LessThan(0f));

            // Cible DERRIERE la projection : ce n'est pas un progres de 0, c'est une mesure
            // inexploitable, et l'appelant doit pouvoir la distinguer pour retomber sur la distance
            // planaire au lieu de conclure "aucun progres".
            var straight = new[] { Vector3.zero, new Vector3(10f, 0f, 0f) };
            Assert.That(LaneGraphRouting.ResolvePathRemainingDistance(straight, 2, new Vector3(8f, 0f, 0f), new Vector3(2f, 0f, 0f)),
                Is.LessThan(0f));
        }

        // ----------------------------------------------------------------- recuperation

        /// <summary>
        /// Un vehicule a contresens qui ROULE ne declenche aucune horloge de blocage : il progresse.
        /// Il lui faut donc son propre compteur, sinon l'echelle ne le voit jamais -- c'est la cause
        /// mesuree des 53,1 s de circulation a l'envers du banc de soak.
        /// </summary>
        [Test]
        public void WrongWayDrivingHasItsOwnClockBecauseAMovingVehicleNeverStalls()
        {
            var tick = Body("private void TickRouteProgress(DriverProfile profile, float dt)");
            Assert.That(tick, Does.Contain("wrongWayElapsedSeconds"),
                "L'escalade doit repondre au contresens comme a l'immobilite.");
            Assert.That(tick, Does.Contain("Mathf.Max(stalledStage, wrongWayStage)"),
                "Les deux motifs alimentent le MEME escalier.");

            // Le contresens n'ouvre jamais le dernier recours : le palier 4 tolere le contact avec
            // un vehicule immobile, ce qui ne se justifie que devant une immobilite reelle. Sans
            // ce plafond, une detection de contresens trop sensible fait manoeuvrer des vehicules
            // qui roulent les uns DANS les autres -- mesure au banc : contresens passe de 53,1 s a
            // 113,6 s et debit tombe de 15 sorties a 9.
            Assert.That(tick, Does.Contain("Mathf.Min(3,"),
                "Le contresens seul doit plafonner sous le dernier recours.");

            var conformance = Body("private void TickLaneConformance(DriverProfile profile, float dt)");
            Assert.That(conformance, Does.Contain("maneuverActive"),
                "Une manoeuvre de deblocage sort legitimement de la voie : son temps ne compte pas.");
            Assert.That(conformance, Does.Contain("wrongWayElapsedSeconds = 0f"),
                "Revenir dans le bon sens doit remettre le compteur a zero.");

            // La reference de sens est la tangente de NOTRE trajectoire, pas l'arete la plus
            // proche : au milieu d'un carrefour celle-ci est souvent transversale ou opposee.
            Assert.That(conformance, Does.Contain("ResolvePathTangent"),
                "Le sens se juge sur la trajectoire suivie, seule reference fiable en continu.");
            Assert.That(conformance, Does.Contain("-SameLaneAlignmentDot"),
                "Il faut un cap franchement oppose, pas un simple defaut d'alignement.");

            var local = Body("private bool TickLocalTraffic(DriverProfile profile, float dt, float speed, float longitudinalSpeed)");
            Assert.That(local.IndexOf("TickLaneConformance(", StringComparison.Ordinal), Is.GreaterThanOrEqualTo(0),
                "La conformite de voie doit etre mesuree a chaque pas, pas seulement en face-a-face.");
        }

        /// <summary>
        /// La sortie d'episode etait inatteignable : elle ne se testait que sous le palier 0, or le
        /// palier vient du temps sans progres, qui ne retombe pas tant que le vehicule est coince.
        /// </summary>
        [Test]
        public void TheRecoveryEpisodeCanCloseAtAnyStage()
        {
            var body = Body("private bool TryEscalateRecovery(DriverProfile profile, float longitudinalSpeed)");
            var recovered = body.IndexOf("HasRecoveredRoute(profile)", StringComparison.Ordinal);
            var stageGate = body.IndexOf("if (recoveryStage <= 0) return false;", StringComparison.Ordinal);

            Assert.That(recovered, Is.GreaterThanOrEqualTo(0), "La sortie d'episode doit etre testee.");
            Assert.That(stageGate, Is.GreaterThanOrEqualTo(0), "Le garde de palier doit exister.");
            Assert.That(recovered, Is.LessThan(stageGate),
                "La reacquisition de la route doit se constater AVANT le garde de palier, sinon un"
                + " episode ouvert ne peut plus jamais se refermer.");
        }

        /// <summary>
        /// Se remettre dans le bon sens passe AVANT les autres paliers : un contournement se
        /// construit sur le cap courant et un recul le suit a l'envers, donc tous deux conservent
        /// l'erreur qu'il faut corriger.
        /// </summary>
        [Test]
        public void ReorientationIsTriedBeforeEveryOtherManeuver()
        {
            var body = Body("private bool TryEscalateRecovery(DriverProfile profile, float longitudinalSpeed)");
            var reorient = body.IndexOf("TryStartReorientation(", StringComparison.Ordinal);
            var detour = body.IndexOf("TryStartDetour(profile, false", StringComparison.Ordinal);
            var reverse = body.IndexOf("TrafficUnblockingAction.Reverse", StringComparison.Ordinal);
            var sidewalk = body.IndexOf("TryStartDetour(profile, true", StringComparison.Ordinal);

            Assert.That(reorient, Is.GreaterThanOrEqualTo(0), "Le palier de remise dans le sens doit exister.");
            Assert.That(reorient, Is.LessThan(detour), "La reorientation precede le contournement.");
            Assert.That(detour, Is.LessThan(reverse), "Le contournement sur la chaussee precede le recul.");
            Assert.That(reverse, Is.LessThan(sidewalk), "Le recul precede le trottoir.");
        }

        /// <summary>
        /// La recuperation s'execute AU-DESSUS des regles de jonction : une manoeuvre engagee ne
        /// repasse plus par TickJunctionRules, donc elle franchirait un feu rouge sans le voir.
        /// </summary>
        [Test]
        public void ARedLightIsNeverManoeuvredAroundBelowLastResort()
        {
            var body = Body("private bool TryEscalateRecovery(DriverProfile profile, float longitudinalSpeed)");
            Assert.That(body, Does.Contain("junctionHeldBySignal && !lastResort"),
                "Tant que le feu est la cause de l'attente, seul le dernier recours manoeuvre.");

            var gate = body.IndexOf("junctionHeldBySignal && !lastResort", StringComparison.Ordinal);
            Assert.That(gate, Is.LessThan(body.IndexOf("TryStartReorientation(", StringComparison.Ordinal)),
                "Le garde du feu doit preceder toute manoeuvre.");
        }

        /// <summary>
        /// La reorientation ne doit inventer aucune place : elle passe par la meme validation de pose
        /// que les autres paliers, et elle essaie les deux cotes avant d'abandonner.
        /// </summary>
        [Test]
        public void ReorientationInventsNoSpaceAndTriesBothSides()
        {
            var body = Body("private bool TryStartReorientation(DriverProfile profile, bool lastResort)");
            Assert.That(body, Does.Contain("ValidatePath("), "La manoeuvre doit etre validee sur le gabarit reel.");
            Assert.That(body, Does.Contain("side >= -1"), "Les deux cotes doivent etre essayes.");
            Assert.That(body, Does.Contain("TryGetRoadPosition"),
                "La cible vient du SENS AUTORISE de l'axe, c'est-a-dire de la semantique LaneGraph.");
        }

        /// <summary>
        /// L'ensemble rassemble ne doit plus etre filtre sur "en conflit avec nous" : c'est ce filtre
        /// qui donnait a chaque observateur un tournoi different.
        /// </summary>
        [Test]
        public void TheGatheredClaimSetIsNoLongerFilteredByOurOwnConflicts()
        {
            var body = Body("private void GatherJunctionClaimants(DriverProfile profile, in JunctionClaim ownProvisional, Vector3 junctionPosition)");
            Assert.That(body, Does.Not.Contain("if (!JunctionRules.Conflicts(ownProvisional, claim)) continue;"),
                "Le filtre par observateur est precisement le defaut corrige.");

            // La revendication provisoire sert encore, mais SEULEMENT a savoir si un revendiquant
            // immobile nous dispute reellement la jonction -- jamais a retrancher de l'ensemble.
            Assert.That(body, Does.Contain("if (JunctionRules.Conflicts(ownProvisional, claim)) junctionClaimantsHeld |= stationary;"),
                "Un immobile qui ne nous dispute rien ne doit pas declencher le palier de deblocage.");
            Assert.That(body, Does.Contain("ResolveJunctionApproachRadius(profile)"),
                "Le rayon d'approche doit venir de la donnee de monde quand elle existe.");
        }

        // ----------------------------------------------------------------- outillage

        private static List<int> Admitted(IReadOnlyList<JunctionClaim> claims)
        {
            var admitted = new List<int>();
            for (var self = 0; self < claims.Count; self++)
            {
                if (JunctionRules.IsAdmitted(claims, claims.Count, self, new int[claims.Count], new int[claims.Count]))
                {
                    admitted.Add(self);
                }
            }

            return admitted;
        }

        /// <summary>
        /// A : nord -> sud (voie x=-2). B : est -> ouest (voie z=+2), croise A et C.
        /// C : sud -> nord (voie x=+2), ne croise PAS A.
        /// </summary>
        private static List<JunctionClaim> ThreeWayWithOneNonCrossingPair()
        {
            var claims = new List<JunctionClaim>
            {
                Claim(new Vector3(0, 0, -1), 8f, 10, 100, new Vector3(-2, 0, 4), new Vector3(-2, 0, -4)),
                Claim(new Vector3(-1, 0, 0), 3f, 20, 200, new Vector3(4, 0, 2), new Vector3(-4, 0, 2)),
                Claim(new Vector3(0, 0, 1), 5f, 30, 300, new Vector3(2, 0, -4), new Vector3(2, 0, 4)),
            };

            Assert.That(JunctionRules.Conflicts(claims[0], claims[1]), Is.True, "Montage : A croise B.");
            Assert.That(JunctionRules.Conflicts(claims[0], claims[2]), Is.False, "Montage : A ne croise pas C.");
            Assert.That(JunctionRules.Conflicts(claims[1], claims[2]), Is.True, "Montage : B croise C.");
            return claims;
        }

        private static JunctionClaim Claim(Vector3 forward, float distance, int lane, ulong id,
            Vector3 entry, Vector3 exit)
        {
            return new JunctionClaim(1, JunctionApproachRule.PriorityToRight, forward, distance, lane, id,
                true, true, true, false, entry, exit);
        }

        private static string Body(string signature)
        {
            var source = File.ReadAllText(TrafficSource);
            var start = source.IndexOf(signature, StringComparison.Ordinal);
            Assert.That(start, Is.GreaterThanOrEqualTo(0), "Signature introuvable : " + signature);

            var brace = source.IndexOf('{', start);
            Assert.That(brace, Is.GreaterThanOrEqualTo(0));

            var depth = 0;
            for (var i = brace; i < source.Length; i++)
            {
                if (source[i] == '{') depth++;
                else if (source[i] == '}')
                {
                    depth--;
                    if (depth == 0) return source.Substring(brace, i - brace + 1);
                }
            }

            Assert.Fail("Corps non equilibre : " + signature);
            return string.Empty;
        }
    }
}
