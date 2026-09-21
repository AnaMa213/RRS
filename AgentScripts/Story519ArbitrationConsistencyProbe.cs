using System.Collections.Generic;
using System.Text;
using RoadRage.Features.Vehicles;
using UnityEngine;

/// <summary>
/// Deux observateurs d'une MEME jonction designent-ils le MEME vainqueur ?
///
/// GatherJunctionClaimants ne retient que les revendications qui entrent en conflit avec SOI
/// (JunctionRules.Conflicts(ownProvisional, claim)). L'ensemble arbitre est donc different d'un
/// observateur a l'autre, alors que ResolveWinnerIndex est un tournoi (decompte de victoires) dont
/// le resultat depend de l'ensemble entier. Cette sonde construit trois revendications reelles et
/// rejoue l'arbitrage du point de vue de chacune, exactement comme le controleur le fait.
/// </summary>
public static class Story519ArbitrationConsistencyProbe
{
    public static string Run()
    {
        var report = new StringBuilder();

        // Carrefour a l'origine, circulation a droite, voies decalees de 2 m.
        // A : nord -> sud, tout droit, voie x=-2.
        // B : est  -> ouest, tout droit, voie z=+2. Croise A ET C.
        // C : sud  -> nord, tout droit, voie x=+2. Ne croise PAS A (parallele).
        var a = Claim(1, "A", new Vector3(0, 0, -1), 8f, 10, 100, new Vector3(-2, 0, 4), new Vector3(-2, 0, -4));
        var b = Claim(1, "B", new Vector3(-1, 0, 0), 3f, 20, 200, new Vector3(4, 0, 2), new Vector3(-4, 0, 2));
        var c = Claim(1, "C", new Vector3(0, 0, 1), 5f, 30, 300, new Vector3(2, 0, -4), new Vector3(2, 0, 4));

        report.AppendLine("== conflits par paire ==");
        report.AppendLine("A-B : " + JunctionRules.Conflicts(a, b));
        report.AppendLine("A-C : " + JunctionRules.Conflicts(a, c));
        report.AppendLine("B-C : " + JunctionRules.Conflicts(b, c));

        report.AppendLine();
        report.AppendLine("== duels (verdict de own) ==");
        report.AppendLine("A vs B : " + JunctionRules.Resolve(a, b));
        report.AppendLine("B vs A : " + JunctionRules.Resolve(b, a));
        report.AppendLine("B vs C : " + JunctionRules.Resolve(b, c));
        report.AppendLine("C vs B : " + JunctionRules.Resolve(c, b));

        var all = new Dictionary<string, JunctionClaim> { { "A", a }, { "B", b }, { "C", c } };

        report.AppendLine();
        report.AppendLine("== arbitrage tel que le controleur le joue (ensemble filtre sur SOI) ==");
        var granted = new List<string>();
        foreach (var self in all)
        {
            // Reproduction exacte de GatherJunctionClaimants : soi en index 0, puis SEULEMENT les
            // revendications qui entrent en conflit avec soi.
            var set = new List<JunctionClaim> { self.Value };
            var names = new List<string> { self.Key };
            foreach (var other in all)
            {
                if (other.Key == self.Key) continue;
                if (!JunctionRules.Conflicts(self.Value, other.Value)) continue;
                set.Add(other.Value);
                names.Add(other.Key);
            }

            var wins = new int[set.Count];
            var winner = JunctionRules.ResolveWinnerIndex(set, set.Count, wins);
            var ok = winner == 0 || winner < 0;
            if (ok) granted.Add(self.Key);
            report.AppendLine(self.Key + " : ensemble={" + string.Join(",", names) + "} decompte=["
                + string.Join(",", wins) + "] vainqueur=" + (winner < 0 ? "aucun" : names[winner])
                + " -> " + (ok ? "PASSE" : "attend"));
        }

        report.AppendLine();
        report.AppendLine("autorises simultanement : {" + string.Join(",", granted) + "}");
        var collision = false;
        for (var i = 0; i < granted.Count; i++)
            for (var j = i + 1; j < granted.Count; j++)
                if (JunctionRules.Conflicts(all[granted[i]], all[granted[j]])) collision = true;
        report.AppendLine(collision
            ? ">>> DEFAUT : deux mouvements EN CONFLIT sont autorises en meme temps."
            : "ensemble coherent.");

        report.AppendLine();
        report.AppendLine("== APRES CORRECTION : IsAdmitted sur l'ensemble complet ==");
        var admittedNames = new List<string>();
        foreach (var self in all)
        {
            var set = new List<JunctionClaim>();
            var names = new List<string>();
            var selfIndex = -1;
            foreach (var kv in all)
            {
                if (kv.Key == self.Key) selfIndex = set.Count;
                set.Add(kv.Value);
                names.Add(kv.Key);
            }

            var wins = new int[set.Count];
            var order = new int[set.Count];
            var ok = JunctionRules.IsAdmitted(set, set.Count, selfIndex, wins, order);
            if (ok) admittedNames.Add(self.Key);
            report.AppendLine(self.Key + " : " + (ok ? "ADMIS" : "attend"));
        }

        report.AppendLine("admis simultanement : {" + string.Join(",", admittedNames) + "}");
        var stillBad = false;
        for (var i = 0; i < admittedNames.Count; i++)
            for (var j = i + 1; j < admittedNames.Count; j++)
                if (JunctionRules.Conflicts(all[admittedNames[i]], all[admittedNames[j]])) stillBad = true;
        report.AppendLine(stillBad
            ? ">>> ENCORE DEFAILLANT"
            : ">>> CORRIGE : aucun couple admis n'est en conflit.");

        report.AppendLine();
        report.AppendLine("== contre-epreuve : meme arbitrage sur l'ensemble COMPLET ==");
        var full = new List<JunctionClaim>();
        var fullNames = new List<string>();
        foreach (var kv in all) { full.Add(kv.Value); fullNames.Add(kv.Key); }
        foreach (var self in all)
        {
            var set = new List<JunctionClaim> { self.Value };
            var names = new List<string> { self.Key };
            for (var i = 0; i < full.Count; i++)
            {
                if (fullNames[i] == self.Key) continue;
                set.Add(full[i]);
                names.Add(fullNames[i]);
            }
            var wins = new int[set.Count];
            var winner = JunctionRules.ResolveWinnerIndex(set, set.Count, wins);
            report.AppendLine(self.Key + " : ensemble={" + string.Join(",", names) + "} decompte=["
                + string.Join(",", wins) + "] vainqueur=" + (winner < 0 ? "aucun" : names[winner])
                + " -> " + (winner == 0 || winner < 0 ? "PASSE" : "attend"));
        }

        return report.ToString();
    }

    private static JunctionClaim Claim(int key, string _, Vector3 forward, float distance, int lane,
        ulong id, Vector3 entry, Vector3 exit)
    {
        return new JunctionClaim(key, JunctionApproachRule.PriorityToRight, forward, distance, lane, id,
            true, true, true, false, entry, exit);
    }
}
