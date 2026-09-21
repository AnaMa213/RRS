using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RoadRage.Features.Vehicles;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// ANO-5.18-04, probleme 2 : un interblocage de giratoire ne doit jamais etre PERMANENT, et il ne
    /// doit jamais etre rompu par un minuteur.
    ///
    /// Ce que ces tests prouvent est la propriete dont depend toute la garantie : sur un cycle
    /// d'attente, EXACTEMENT UN membre se libere, et tous les membres s'accordent sur lequel sans se
    /// parler. C'est la meme forme d'argument que l'antisymetrie de l'arbitrage de paire, etendue a
    /// n participants -- et c'est ce qui permet de supprimer le relachement sur delai sans rien
    /// perdre.
    ///
    /// Le corollaire compte autant : une attente qui n'est PAS un cycle ne se rompt pas. Une file
    /// lente et une saturation reelle ont un debouche, et les forcer fabriquerait des collisions la
    /// ou le trafic fonctionnait.
    /// </summary>
    public sealed class Story518WaitCycleTests
    {
        /// <summary>Qui, dans ce cycle, se declare libere ? Chaque membre calcule pour lui-meme.</summary>
        private static List<ulong> Released(params ulong[] cycle)
        {
            var released = new List<ulong>();
            foreach (var self in cycle)
            {
                // Chaque membre parcourt le cycle DEPUIS LUI : c'est ce que fait le controleur en
                // suivant sa chaine WaitsFor. L'ensemble observe est donc le meme, l'ordre non.
                var index = System.Array.IndexOf(cycle, self);
                var others = new ulong[cycle.Length - 1];
                for (var i = 0; i < others.Length; i++) others[i] = cycle[(index + 1 + i) % cycle.Length];

                if (JunctionRules.ReleasesWaitCycle(self, others, others.Length)) released.Add(self);
            }

            return released;
        }

        [Test]
        public void ATwoVehicleCycleReleasesExactlyOne()
        {
            Assert.That(Released(7UL, 3UL), Is.EqualTo(new[] { 3UL }));
        }

        [Test]
        public void AThreeVehicleCycleReleasesExactlyOne()
        {
            // Le cas nomme dans la demande : A attend B, B attend C, C attend A.
            Assert.That(Released(11UL, 4UL, 9UL), Is.EqualTo(new[] { 4UL }));
        }

        [Test]
        public void EveryCycleSizeReleasesExactlyOneWhoeverDoesTheArithmetic()
        {
            for (var size = 2; size <= 12; size++)
            {
                var cycle = Enumerable.Range(0, size).Select(i => (ulong)(1 + i * 7 % 23)).Distinct().ToArray();
                if (cycle.Length < 2) continue;

                var released = Released(cycle);
                Assert.That(released.Count, Is.EqualTo(1),
                    "Cycle de " + cycle.Length + " : " + released.Count + " liberations au lieu d'une.");
                Assert.That(released[0], Is.EqualTo(cycle.Min()),
                    "Le libere doit etre le plus petit identifiant, sur lequel tous les membres s'accordent.");
            }
        }

        [Test]
        public void AWaitThatIsNotACycleReleasesNobody()
        {
            // Aucun membre observe : la chaine d'attente n'est jamais revenue sur l'observateur. Elle
            // a donc un debouche -- file legitime, sortie saturee, prioritaire qui progresse -- et la
            // forcer serait exactement le minuteur que la revue interdit.
            Assert.That(JunctionRules.ReleasesWaitCycle(5UL, new ulong[0], 0), Is.False);
            Assert.That(JunctionRules.ReleasesWaitCycle(5UL, null, 0), Is.False);
            Assert.That(JunctionRules.ReleasesWaitCycle(5UL, new ulong[] { 1UL, 2UL }, 0), Is.False,
                "Un compte nul decrit une absence de cycle, quel que soit le contenu du tampon.");
        }

        [Test]
        public void ASaturatedRoundaboutIsNotACycleAndKeepsWaiting()
        {
            // Sept vehicules qui attendent tous le MEME vehicule de tete, lequel attend autre chose
            // hors du groupe : ce n'est pas un cycle, c'est une file. Personne ne force le passage.
            for (var follower = 2UL; follower <= 8UL; follower++)
            {
                Assert.That(JunctionRules.ReleasesWaitCycle(follower, new ulong[0], 0), Is.False,
                    "Le vehicule " + follower + " attend une file, pas un cycle : il ne force rien.");
            }
        }

        [Test]
        public void TheSmallestIdentifierNeverYieldsItsTurnToAHigherOne()
        {
            // L'antisymetrie, dite dans l'autre sens : si nous sommes le minimum nous passons, et si
            // un plus petit existe nous ne passons pas. Aucune position intermediaire n'existe, donc
            // deux membres ne peuvent pas se liberer ensemble.
            Assert.That(JunctionRules.ReleasesWaitCycle(2UL, new ulong[] { 9UL, 40UL }, 2), Is.True);
            Assert.That(JunctionRules.ReleasesWaitCycle(9UL, new ulong[] { 2UL, 40UL }, 2), Is.False);
            Assert.That(JunctionRules.ReleasesWaitCycle(40UL, new ulong[] { 2UL, 9UL }, 2), Is.False);
        }
    }
}
