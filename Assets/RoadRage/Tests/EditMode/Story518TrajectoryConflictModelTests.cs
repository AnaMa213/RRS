using System.Collections.Generic;
using NUnit.Framework;
using RoadRage.Features.Vehicles;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.18, correctif du retour terrain du 2026-09-21 : le trafic s'arretait sur rien.
    ///
    /// Ce qui est teste ici n'est pas un reglage mais le MODELE de decision. Le defaut corrige tenait
    /// en une phrase : la prediction extrapolait la vitesse instantanee en LIGNE DROITE pendant tout
    /// l'horizon, alors que chaque vehicule suit une VOIE. Trois symptomes en decoulaient, et les
    /// trois disparaissent ensemble quand la prediction suit la trajectoire reelle :
    ///
    /// - deux vehicules en sens inverse s'arretaient a 20 m, parce que la moindre derive laterale de
    ///   l'asservissement de braquage faisait converger les deux demi-droites dans l'horizon ;
    /// - un giratoire se figeait, parce que la tangente d'un vehicule en train de tourner sort de
    ///   l'anneau et percute le decor exterieur ;
    /// - un joueur ARRETE a 15 m provoquait un freinage, parce que la tangente du vehicule le
    ///   traversait avant d'avoir tourne.
    ///
    /// Les geometries employees sont MESUREES dans MVP_Run (sonde du 2026-09-21) et non choisies :
    /// entraxe de deux voies opposees 4,00 m, anneau de giratoire de rayon 6,00 m a 11 noeuds,
    /// gabarit du vehicule 2,06 x 4,44 m. Un test qui passerait sur des nombres inventes ne dirait
    /// rien du terrain -- c'est exactement pourquoi la version precedente passait ses tests.
    /// </summary>
    public sealed class Story518TrajectoryConflictModelTests
    {
        /// <summary>Demi-largeur mesuree du Greybox_AIVehicle (collider 2,06 m).</summary>
        private const float HalfWidth = 1.03f;

        /// <summary>Demi-longueur mesuree du Greybox_AIVehicle (collider 4,44 m).</summary>
        private const float HalfLength = 2.22f;

        /// <summary>Marge de securite authoree dans DriverProfileDef_Default.</summary>
        private const float Margin = 0.3f;

        /// <summary>Horizon de prediction authore dans DriverProfileDef_Default.</summary>
        private const float Horizon = 3f;

        /// <summary>Vitesse desiree authoree dans DriverProfileDef_Default.</summary>
        private const float Cruise = 8f;

        /// <summary>Entraxe mesure de deux voies opposees dans MVP_Run.</summary>
        private const float LaneSeparation = 4f;

        /// <summary>Rayon mesure de l'anneau des quatre giratoires du district.</summary>
        private const float RingRadius = 6f;

        // ------------------------------------------------------------------ Scenario 1

        [Test]
        public void Scenario1_OppositeLanesPassEachOtherEvenWhileDriftingLaterally()
        {
            // La derive laterale est le point du test. Sans elle, l'ancien modele passait deja : c'est
            // ce qui rendait le defaut invisible en test et systematique en jeu. Un asservissement de
            // braquage produit en permanence quelques dizaines de cm/s de derive ; a 0,6 m/s relative,
            // l'ancienne extrapolation rectiligne comblait les 1,64 m d'ecart restants en 2,7 s, donc
            // DANS l'horizon de 3 s -- les deux vehicules s'arretaient a 20 m l'un de l'autre.
            var own = Lane(Vector3.zero, Vector3.forward, 40f, lateralDrift: 0.3f);
            var other = Lane(new Vector3(LaneSeparation, 0f, 20f), Vector3.back, 40f, lateralDrift: -0.3f);

            var conflict = Conflict(own, Cruise, other, Cruise);

            Assert.That(conflict.Exists, Is.False,
                "Deux voies separees de 4 m ne partagent aucun point : se croiser n'est pas un conflit, "
                + "quelle que soit la derive de braquage.");
        }

        [Test]
        public void Scenario1_OppositeLaneVehicleIsNeverSelectedAsALeader()
        {
            // Une derive laterale fait osciller l'angle vu depuis le nez ; c'est la PROJECTION SUR LA
            // VOIE qui doit trancher, pas ce cone. L'ecart lateral mesure vaut 4 m contre une bande de
            // 1,03 + 0,3 + 1,03 = 2,36 m.
            var oncoming = new TrafficPerceptionCandidate(12f, 3f, -Cruise, true, false, true, false,
                lateralOffset: LaneSeparation, halfWidth: HalfWidth, isOnOwnPath: false);

            Assert.That(TrafficPerception.TrySelectLeader(new[] { oncoming }, 1, 20f, 100f,
                out _, out _, HalfWidth + Margin), Is.False);
        }

        // ------------------------------------------------------------------ Scenario 2

        [Test]
        public void Scenario2_SameLaneFollowerKeepsItsLeaderAndAdaptsItsSpeed()
        {
            var leader = new TrafficPerceptionCandidate(9f, 0f, 6f, true, false, true, false,
                lateralOffset: 0.1f, halfWidth: HalfWidth, isOnOwnPath: true);

            Assert.That(TrafficPerception.TrySelectLeader(new[] { leader }, 1, 20f, 100f,
                out var gap, out var speed, HalfWidth + Margin), Is.True);
            Assert.That(gap, Is.EqualTo(9f).Within(0.001f));
            Assert.That(speed, Is.EqualTo(6f).Within(0.001f));

            // Une file n'est pas un arret : l'IDM rend une reponse CONTINUE a l'ecart, pas un verdict
            // binaire. On verifie la forme (monotone, finie, et nulle des que l'ecart est confortable)
            // plutot qu'une valeur, qui ne serait qu'un reglage recopie.
            var profile = new DriverProfile(Cruise, 1.5f, 0.75f, 1.5f, 2f, 0.25f, 0.2f, 4f, 0.3f, 1f, 0.8f);
            var close = DriverModel.ComputeAcceleration(profile, Cruise, 6f, gap);
            var roomy = DriverModel.ComputeAcceleration(profile, Cruise, 6f, 40f);
            Assert.That(close, Is.LessThan(0f), "Le suiveur ralentit a 9 m.");
            Assert.That(close, Is.LessThan(roomy), "...et d'autant plus que l'ecart est court.");
            Assert.That(roomy, Is.GreaterThan(-profile.ComfortableDeceleration * 0.25f),
                "A 40 m, le freinage est residuel : la file n'est pas un arret.");
            Assert.That(float.IsFinite(close), Is.True);
        }

        [Test]
        public void Scenario2_AStationaryObstacleInOurLaneStillStopsUsIncludingAParkedPlayer()
        {
            // Contrepartie indispensable du correctif : desserrer la peur ne doit pas rendre l'IA
            // aveugle. Un joueur ARRETE SUR la voie reste un leader -- la variante precedente, qui
            // ecartait tout joueur sans collision predite, l'aurait traverse.
            var parkedPlayer = new TrafficPerceptionCandidate(8f, 0f, 0f, true, false, true, false,
                lateralOffset: 0.2f, halfWidth: HalfWidth, isPlayer: true,
                isCollisionThreat: false, isOnOwnPath: true);

            Assert.That(TrafficPerception.TrySelectLeader(new[] { parkedPlayer }, 1, 20f, 100f,
                out _, out _, HalfWidth + Margin), Is.True);

            var wall = Point(new Vector3(0f, 0f, 10f));
            Assert.That(Conflict(Lane(Vector3.zero, Vector3.forward, 40f), Cruise, wall, 0f).Exists, Is.True,
                "Un obstacle immobile pose SUR notre trajectoire reste un conflit.");
        }

        // ------------------------------------------------------------------ Scenario 3 / 4

        [Test]
        public void Scenario3_CrossingCorridorsUsedAtDifferentTimesAreNotAConflict()
        {
            // Les deux trajectoires se coupent bel et bien : c'est le TEMPS qui les separe. Un modele
            // qui ne regarde que la geometrie ("il va au meme carrefour") arreterait les deux.
            var own = Lane(new Vector3(0f, 0f, -20f), Vector3.forward, 40f);
            var crossing = Lane(new Vector3(-40f, 0f, 0f), Vector3.right, 40f);

            // L'autre est a 40 m du point de croisement a 8 m/s : il y sera dans 5 s, nous dans 2,5 s.
            var conflict = Conflict(own, Cruise, crossing, Cruise);

            Assert.That(conflict.Exists, Is.False,
                "Passer au meme endroit a 2,5 s d'ecart n'est pas se disputer la place.");
        }

        [Test]
        public void Scenario4_GenuinelyCrossingTrajectoriesConflictAndExactlyOneVehicleYields()
        {
            var own = Lane(new Vector3(0f, 0f, -20f), Vector3.forward, 40f);
            var crossing = Lane(new Vector3(-20f, 0f, 0f), Vector3.right, 40f);

            var mine = Conflict(own, Cruise, crossing, Cruise);
            var theirs = Conflict(crossing, Cruise, own, Cruise);

            Assert.That(mine.Exists, Is.True, "Meme point, meme instant : c'est un conflit.");
            Assert.That(theirs.Exists, Is.True, "Et il est vu des deux cotes.");

            var iYield = TrafficPerception.YieldsAtConflict(mine.OwnArrival, 7UL, mine.OtherArrival, 9UL);
            var theyYield = TrafficPerception.YieldsAtConflict(theirs.OwnArrival, 9UL, theirs.OtherArrival, 7UL);
            Assert.That(iYield, Is.Not.EqualTo(theyYield),
                "Exactement un des deux cede : c'est ce qui rend l'interblocage impossible pour une paire.");
        }

        // ------------------------------------------------------------------ Scenario 5 / 6

        [Test]
        public void Scenario5_ACirculatingVehicleIsNotStoppedByTheSceneryOnItsOwnTangent()
        {
            // LE test de la cause racine du giratoire fige. Sur un anneau de 6 m, un vehicule a 8 m/s
            // parcourt 24 m en 3 s : sa TANGENTE sort de l'anneau et heurte le decor exterieur, mesure
            // entre 10,3 m et 23,6 m sur les noeuds reels du district. Sa TRAJECTOIRE, elle, reste sur
            // l'anneau et ne le touche jamais.
            var ring = Ring(Vector3.zero, RingRadius, 11);
            var wallOnTheTangent = Point(ring[0] + RingTangent(ring, 0) * 10.3f);

            var tangent = Lane(ring[0], RingTangent(ring, 0), 24f);
            Assert.That(Conflict(tangent, Cruise, wallOnTheTangent, 0f).Exists, Is.True,
                "Repere : l'extrapolation rectiligne -- l'ancien modele -- percute bien ce mur.");

            Assert.That(Conflict(ring, Cruise, wallOnTheTangent, 0f).Exists, Is.False,
                "La trajectoire reelle suit l'anneau : le mur exterieur n'est jamais sur son chemin.");
        }

        [Test]
        public void Scenario5_AVehicleApproachingAnEntranceNeverStopsWhatIsAlreadyCirculating()
        {
            // La garantie demandee par le retour terrain est "A CONTINUE", et non "il n'y a aucun
            // conflit" : les deux formulations different, et confondre les deux est precisement ce qui
            // figeait les giratoires. Quand les emprises se rencontrent vraiment, la bonne reponse
            // n'est pas que les deux s'arretent, c'est que le plus avance passe.
            // A circule et passe la bouche d'entree (sud) dans 0,2 s ; B est encore a 24 m de cette
            // bouche, soit 3 s. Les deux occupent bien le MEME point, mais pas au meme moment.
            var ring = RingFrom(Vector3.zero, RingRadius, 11, startAngleDegrees: 160f);
            var approaching = Lane(new Vector3(0f, 0f, -30f), Vector3.forward, 26f);

            var fromRing = Conflict(ring, Cruise, approaching, Cruise);
            Assert.That(fromRing.Exists, Is.False,
                "Un vehicule qui approche une entree ne fige pas ce qui circule deja : c'est le TEMPS "
                + "qui les separe, pas la distance brute.");

            // Et si le conflit devenait reel, la reponse resterait "A continue", jamais "les deux
            // s'arretent" : la garantie demandee porte sur A, pas sur l'absence de conflit.
            var atTheMouth = Lane(new Vector3(0f, 0f, -RingRadius - 4f), Vector3.forward, 14f);
            var contested = Conflict(ring, Cruise, atTheMouth, Cruise);
            if (contested.Exists)
            {
                Assert.That(TrafficPerception.YieldsAtConflict(contested.OwnArrival, 1UL, contested.OtherArrival, 2UL),
                    Is.False, "Deja engage sur l'anneau : il passe.");
            }
        }

        [Test]
        public void Scenario6_OnARealEntryConflictTheCirculatingVehicleGoesFirst()
        {
            // A circule et arrive sur la bouche d'entree (sud) dans ~0,5 s ; B y arrive au meme
            // instant. C'est le conflit d'insertion reel, par opposition au scenario 5.
            var ring = RingFrom(Vector3.zero, RingRadius, 11, startAngleDegrees: 160f);
            var entering = Lane(new Vector3(0f, 0f, -RingRadius - 4f), Vector3.forward, 14f);

            var fromRing = Conflict(ring, Cruise, entering, Cruise);
            var fromEntry = Conflict(entering, Cruise, ring, Cruise);

            Assert.That(fromRing.Exists && fromEntry.Exists, Is.True, "Conflit reel, vu des deux cotes.");

            var ringYields = TrafficPerception.YieldsAtConflict(fromRing.OwnArrival, 1UL, fromRing.OtherArrival, 2UL);
            Assert.That(ringYields, Is.False,
                "Deja engage sur l'anneau, donc plus proche du point de conflit : il passe. C'est la "
                + "priorite a l'anneau, obtenue sans aucune regle de giratoire.");

            var entryYields = TrafficPerception.YieldsAtConflict(fromEntry.OwnArrival, 2UL, fromEntry.OtherArrival, 1UL);
            Assert.That(entryYields, Is.True, "Et l'entrant cede, au lieu que les deux s'arretent.");
        }

        // ------------------------------------------------------------------ Scenario 7 / 8

        [Test]
        public void Scenario7_AHarmlessPlayerNeverBrakesTheTraffic()
        {
            var own = Lane(Vector3.zero, Vector3.forward, 40f);

            // a) arrete a 15 m, hors de la voie.
            Assert.That(Conflict(own, Cruise, Point(new Vector3(LaneSeparation, 0f, 15f)), 0f).Exists, Is.False,
                "Proche n'est pas dangereux : immobile a cote de la voie, il n'occupe pas notre place.");

            // b) arrete a 18 m dans l'axe mais decale d'une voie.
            Assert.That(Conflict(own, Cruise, Point(new Vector3(LaneSeparation, 0f, 18f)), 0f).Exists, Is.False);

            // c) roulant vite sur une trajectoire parallele qui ne coupe jamais la notre.
            var parallel = Lane(new Vector3(LaneSeparation, 0f, -10f), Vector3.forward, 50f);
            Assert.That(Conflict(own, Cruise, parallel, 14f).Exists, Is.False,
                "Une trajectoire parallele ne se croise nulle part, quelle que soit la vitesse.");
        }

        [Test]
        public void Scenario8_APlayerConvergingIntoOurPathIsAnticipated()
        {
            var own = Lane(Vector3.zero, Vector3.forward, 40f);

            // Meme distance de depart que le joueur inoffensif du scenario 7, mais il vient sur nous :
            // 18 m a 12 m/s le posent sur notre voie a 1,5 s, quand nous y serons aussi.
            var converging = Lane(new Vector3(18f, 0f, 12f), new Vector3(-1f, 0f, 0f), 24f);
            var conflict = Conflict(own, Cruise, converging, 12f);

            Assert.That(conflict.Exists, Is.True,
                "Meme proximite que le cas inoffensif : c'est la TRAJECTOIRE qui fait la difference.");
            Assert.That(conflict.Time, Is.InRange(0f, Horizon));
        }

        // ------------------------------------------------------------------ Scenario 9

        [Test]
        public void Scenario9_NoSetOfVehiclesCanReachAStableCircularWait()
        {
            // L'arbitrage est un ordre strict : on le verifie exhaustivement sur des delais egaux,
            // quasi egaux et distincts. Une seule paire symetrique suffirait a figer un carrefour.
            var arrivals = new[] { 0f, 0.1f, 0.2f, 0.9f, 1f, 1.4f, 3f, float.PositiveInfinity };
            for (var i = 0; i < arrivals.Length; i++)
            {
                for (var j = 0; j < arrivals.Length; j++)
                {
                    for (var idA = 1UL; idA <= 3UL; idA++)
                    {
                        for (var idB = 1UL; idB <= 3UL; idB++)
                        {
                            if (idA == idB) continue;
                            var a = TrafficPerception.YieldsAtConflict(arrivals[i], idA, arrivals[j], idB);
                            var b = TrafficPerception.YieldsAtConflict(arrivals[j], idB, arrivals[i], idA);
                            Assert.That(a, Is.Not.EqualTo(b),
                                "Arrivees " + arrivals[i] + "/" + arrivals[j] + ", ids " + idA + "/" + idB
                                + " : les deux ne peuvent jamais attendre l'autre.");
                        }
                    }
                }
            }
        }

        // ------------------------------------------------------------------ Proprietes du modele

        [Test]
        public void TheTwoSidesOfAConflictMeasureTheSameMeetingPoint()
        {
            // Regression precise. Le delai d'arrivee doit se mesurer au point de RENCONTRE des deux
            // voies, pas au premier recouvrement des emprises : le recouvrement commence une
            // demi-longueur plus tot, donc chacun se croyait en avance sur l'autre et AUCUN des deux
            // ne cedait -- un arbitrage qui produit une collision au lieu d'un interblocage.
            var north = Lane(new Vector3(0f, 0f, -20f), Vector3.forward, 40f);
            var west = Lane(new Vector3(-20f, 0f, 0f), Vector3.right, 40f);

            var mine = Conflict(north, Cruise, west, Cruise);
            var theirs = Conflict(west, Cruise, north, Cruise);

            Assert.That(mine.Exists && theirs.Exists, Is.True);
            Assert.That(mine.OwnArrival, Is.EqualTo(theirs.OtherArrival).Within(0.2f),
                "Les deux cotes lisent le meme delai pour le meme vehicule.");
            Assert.That(mine.OtherArrival, Is.EqualTo(theirs.OwnArrival).Within(0.2f));
        }


        [Test]
        public void TheConflictTestNeverReturnsANonFiniteOrNegativeTime()
        {
            var own = Lane(Vector3.zero, Vector3.forward, 40f);
            var other = Lane(new Vector3(0f, 0f, 10f), Vector3.back, 20f);
            foreach (var speed in new[] { 0f, 0.001f, 8f, 60f, float.NaN, float.PositiveInfinity })
            {
                var conflict = Conflict(own, speed, other, speed);
                if (!conflict.Exists) continue;
                Assert.That(float.IsFinite(conflict.Time), Is.True, "vitesse " + speed);
                Assert.That(conflict.Time, Is.GreaterThanOrEqualTo(0f), "vitesse " + speed);
            }
        }

        [Test]
        public void DegenerateInputsAreRefusedRatherThanGuessed()
        {
            var own = Lane(Vector3.zero, Vector3.forward, 40f);
            var extents = new Vector3(HalfWidth, 0.71f, HalfLength);
            Assert.That(TrafficPerception.FindPathConflict(own, own.Count, Cruise, extents,
                null, 0, 0f, extents, Quaternion.identity, Margin, Horizon).Exists, Is.False);
            Assert.That(TrafficPerception.FindPathConflict(own, 1, Cruise, extents,
                own, own.Count, Cruise, extents, Quaternion.identity, Margin, Horizon).Exists, Is.False,
                "Un seul point ne decrit pas NOTRE trajectoire : rien a arbitrer.");
            Assert.That(TrafficPerception.FindPathConflict(own, own.Count, Cruise, extents,
                own, own.Count, Cruise, extents, Quaternion.identity, Margin, 0f).Exists, Is.False,
                "Un horizon nul n'anticipe rien.");
        }

        [Test]
        public void ProjectionOnAPathReadsTheLaneAndNotTheNoseTangent()
        {
            // Sur un anneau, un point situe 3 m plus loin SUR l'anneau est a 3 m "devant" au sens de la
            // voie, alors que la tangente du nez le voit a plusieurs metres sur le cote. C'est
            // exactement la confusion qui transformait un vehicule de l'anneau en obstacle lateral.
            var ring = Ring(Vector3.zero, RingRadius, 11);
            // 10 m plus loin SUR l'anneau : la voie a deja tourne de plus de 90 degres.
            var ahead = TrafficPerception.SamplePath(ring, ring.Count, 10f);

            Assert.That(TrafficPerception.TryProjectOnPath(ring, ring.Count, ahead,
                out var along, out var lateral, out _), Is.True);
            Assert.That(along, Is.EqualTo(10f).Within(0.05f));
            Assert.That(Mathf.Abs(lateral), Is.LessThan(0.05f), "Il est DANS la voie, pas a cote.");

            var tangentLateral = Vector3.Dot(ahead - ring[0], Vector3.Cross(Vector3.up, RingTangent(ring, 0)));
            Assert.That(Mathf.Abs(tangentLateral), Is.GreaterThan(2f),
                "Repere : la tangente du nez le lit a plus de 2 m sur le cote -- c'est cette lecture "
                + "qui transformait un vehicule de l'anneau en obstacle lateral.");
        }

        [Test]
        public void SamplingAPathNeverOvershootsItsEnd()
        {
            var path = Lane(Vector3.zero, Vector3.forward, 10f);
            var end = TrafficPerception.SamplePath(path, path.Count, 1000f);
            Assert.That(Vector3.Distance(end, path[path.Count - 1]), Is.LessThan(0.001f),
                "Une trajectoire epuisee decrit un acteur qui s'arrete la, pas qui fonce tout droit.");
        }

        // ------------------------------------------------------------------ Outils

        private static PathConflict Conflict(IReadOnlyList<Vector3> own, float ownSpeed,
            IReadOnlyList<Vector3> other, float otherSpeed)
        {
            var extents = new Vector3(HalfWidth, 0.71f, HalfLength);
            return TrafficPerception.FindPathConflict(own, own.Count, ownSpeed, extents,
                other, other.Count, otherSpeed, extents, Quaternion.identity, Margin, Horizon);
        }

        /// <summary>Voie droite, echantillonnee tous les 4 m, avec une derive laterale optionnelle (m/s a 8 m/s).</summary>
        private static List<Vector3> Lane(Vector3 origin, Vector3 direction, float length, float lateralDrift = 0f)
        {
            var forward = Vector3.ProjectOnPlane(direction, Vector3.up).normalized;
            var right = Vector3.Cross(Vector3.up, forward);
            var points = new List<Vector3>();
            for (var travelled = 0f; travelled <= length + 0.001f; travelled += 4f)
            {
                points.Add(origin + forward * travelled + right * (lateralDrift * travelled / Cruise));
            }

            return points;
        }

        private static List<Vector3> Point(Vector3 position)
        {
            return new List<Vector3> { position };
        }

        /// <summary>Anneau de giratoire : la geometrie mesuree du district, 11 noeuds sur 6 m de rayon.</summary>
        private static List<Vector3> Ring(Vector3 center, float radius, int nodes)
        {
            var points = new List<Vector3>();
            for (var i = 0; i < nodes; i++)
            {
                var angle = i * 2f * Mathf.PI / nodes;
                points.Add(center + new Vector3(Mathf.Sin(angle) * radius, 0f, Mathf.Cos(angle) * radius));
            }

            return points;
        }

        /// <summary>Anneau demarrant a un angle donne : place le vehicule ou le scenario l'exige.</summary>
        private static List<Vector3> RingFrom(Vector3 center, float radius, int nodes, float startAngleDegrees)
        {
            var points = new List<Vector3>();
            for (var i = 0; i < nodes; i++)
            {
                var angle = (startAngleDegrees * Mathf.Deg2Rad) + (i * 2f * Mathf.PI / nodes);
                points.Add(center + new Vector3(Mathf.Sin(angle) * radius, 0f, Mathf.Cos(angle) * radius));
            }

            return points;
        }

        private static Vector3 RingTangent(IReadOnlyList<Vector3> ring, int index)
        {
            return Vector3.ProjectOnPlane(ring[index + 1] - ring[index], Vector3.up).normalized;
        }
    }
}
