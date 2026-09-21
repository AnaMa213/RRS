using System.Collections.Generic;
using System.Linq;
using System.Text;
using RoadRage.Features.Vehicles;
using UnityEngine;

/// <summary>
/// Etat REEL du trafic dans la session de jeu en cours. Lecture seule : aucune ecriture, aucune
/// ouverture de scene, rien qui puisse perturber la partie observee.
///
/// Repond nommement aux questions de la recette : qui cede a qui, ou est le point de conflit, qui
/// est prioritaire, et si un leader ARRETE influence indument une decision de cession.
/// </summary>
public static class Story518LiveTrafficProbe
{
    public static string Run()
    {
        var report = new StringBuilder();
        var vehicles = Object.FindObjectsByType<NetworkedAIVehicleDriverController>(
            FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        var graph = Object.FindFirstObjectByType<LaneGraph>();

        report.AppendLine("vehicules IA : " + vehicles.Length);
        report.AppendLine();

        var byReason = vehicles.GroupBy(v => v.DecisionReason).OrderByDescending(g => g.Count());
        foreach (var group in byReason)
        {
            report.AppendLine("  " + group.Key + " : " + group.Count());
        }

        report.AppendLine();
        report.AppendLine("=== detail par vehicule ===");
        foreach (var vehicle in vehicles.OrderBy(v => v.name))
        {
            var speed = vehicle.GetComponent<Rigidbody>() != null
                ? Vector3.ProjectOnPlane(vehicle.GetComponent<Rigidbody>().linearVelocity, Vector3.up).magnitude
                : -1f;
            var partner = vehicle.ConflictPartner;
            var waits = vehicle.WaitsFor;
            report.AppendLine(vehicle.name
                + " | " + vehicle.DecisionReason
                + " | v=" + speed.ToString("0.00")
                + " | voie=" + vehicle.CurrentLaneNode
                + " | anneau=" + vehicle.RingId
                + " | conflit=" + (partner != null ? partner.name : "-")
                + " | attend=" + (waits != null ? waits.name : "-")
                + " | jonction=" + (vehicle.CommittedJunctionKey > 0 ? "ENGAGE " + vehicle.CommittedJunctionKey
                    : vehicle.JunctionHoldsWaypoint ? "retenu" : "-")
                + " | arret_ligne=" + (float.IsFinite(vehicle.JunctionStopGap)
                    ? vehicle.JunctionStopGap.ToString("0.00") + "m" : "-"));
        }

        // Chaines d'attente : la question "cette attente a-t-elle un debouche ?" se lit ici.
        report.AppendLine();
        report.AppendLine("=== chaines d'attente ===");
        foreach (var vehicle in vehicles.OrderBy(v => v.name))
        {
            if (vehicle.WaitsFor == null) continue;
            var chain = new List<string> { vehicle.name };
            var seen = new HashSet<NetworkedAIVehicleDriverController> { vehicle };
            var current = vehicle.WaitsFor;
            var closed = false;
            for (var depth = 0; current != null && depth < 16; depth++)
            {
                chain.Add(current.name + "(" + current.DecisionReason + ")");
                if (!seen.Add(current)) { closed = true; break; }
                current = current.WaitsFor;
            }

            report.AppendLine((closed ? "[CYCLE] " : "[ouverte] ") + string.Join(" -> ", chain)
                + (current == null ? " -> personne" : string.Empty));
        }

        // Immobilite : combien de vehicules sont a l'arret, et depuis ou.
        report.AppendLine();
        report.AppendLine("=== immobiles ===");
        foreach (var vehicle in vehicles)
        {
            var rb = vehicle.GetComponent<Rigidbody>();
            if (rb == null || Vector3.ProjectOnPlane(rb.linearVelocity, Vector3.up).magnitude > 0.2f) continue;
            var position = vehicle.transform.position;
            var lane = "hors graphe";
            if (graph != null && graph.TryGetRoadPosition(position, out _, out var direction, out var distance))
            {
                var aligned = Vector3.Dot(Vector3.ProjectOnPlane(vehicle.transform.forward, Vector3.up).normalized, direction);
                lane = "axe a " + distance.ToString("0.00") + " m, alignement " + aligned.ToString("0.00")
                    + (aligned < 0f ? "  [A CONTRESENS]" : string.Empty);
            }

            report.AppendLine(vehicle.name + " @(" + position.x.ToString("0.0") + ";" + position.z.ToString("0.0")
                + ") " + vehicle.DecisionReason + " -- " + lane);
        }

        return report.ToString();
    }
}
