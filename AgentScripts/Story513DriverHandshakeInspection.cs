using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RoadRage.Features.Vehicles;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Sonde de LECTURE SEULE a executer EN PLAY MODE, pendant qu'un joueur est assis au volant d'un
/// vehicule. Elle repond a une seule question : ce vehicule accepte-t-il une intention de conduite ?
///
/// Chaine reelle, lue dans le code :
///   - `NetworkedVehicleDriverController:154` ne lit l'entree locale que si
///     `state.DriverClientId.Value == localClientId` ;
///   - `:625` (serveur) rejette ensuite toute intention dont l'expediteur n'est pas ce conducteur,
///     et rejette aussi tout vehicule `IsInoperable()`.
///
/// Aucune mutation, aucune sauvegarde : la sonde ne fait que lire.
/// </summary>
public static class Story513DriverHandshakeInspection
{
    public static object Inspect()
    {
        var manager = NetworkManager.Singleton;
        var report = new Dictionary<string, object>
        {
            ["playMode"] = Application.isPlaying,
            ["networkManagerPresent"] = manager != null,
            ["localClientId"] = manager == null ? "(hors session)" : manager.LocalClientId.ToString(),
            ["isServer"] = manager != null && manager.IsServer,
            ["isClient"] = manager != null && manager.IsClient,
            ["isListening"] = manager != null && manager.IsListening,
            ["inputGateBlocked"] = ReadStaticInputGate(),
            ["connectedClients"] = manager == null || manager.ConnectedClientsIds == null
                ? "(aucun)"
                : string.Join(",", manager.ConnectedClientsIds.Select(id => id.ToString())),
        };

        var states = UnityEngine.Object.FindObjectsByType<NetworkedVehicleState>(FindObjectsSortMode.None);
        var vehicles = new List<object>();

        foreach (var state in states)
        {
            var entry = new Dictionary<string, object>
            {
                ["name"] = state.name,
                ["hierarchyPath"] = HierarchyPath(state.transform),
                ["isSpawned"] = state.IsSpawned,
                ["isServer"] = state.IsServer,
                ["isOwner"] = state.IsOwner,
                ["ownerClientId"] = state.OwnerClientId.ToString(),
                ["driverClientId"] = state.DriverClientId.Value.ToString(),
                ["driverClaimed"] = state.DriverClientId.Value != NetworkedVehicleState.UnclaimedDriverClientId,
                ["isInoperable"] = state.IsInoperable(),
                ["diagnostics"] = DescribePublicMembers(state),
            };

            var body = state.GetComponentInChildren<VehiclePhysicsBody>(true);
            entry["hasPhysicsBody"] = body != null;
            entry["hasProfile"] = body != null && body.HasProfile;
            entry["wheelCount"] = body == null ? -1 : body.WheelCount;
            entry["groundedWheels"] = body == null ? -1 : body.GroundedWheelCount;
            entry["authorityFactor"] = body == null ? -1f : body.GroundedAuthorityFactor;

            var rigidbody = state.GetComponentInChildren<Rigidbody>(true);
            entry["isKinematic"] = rigidbody != null && rigidbody.isKinematic;
            entry["useGravity"] = rigidbody == null || rigidbody.useGravity;

            var driver = state.GetComponentInChildren<NetworkedVehicleDriverController>(true);
            entry["hasDriverController"] = driver != null;
            entry["driverEnabled"] = driver != null && driver.enabled;

            // La question qui reste : l'intention LUE atteint-elle la couche physique ? Les deux champs
            // sont prives, donc lus par reflexion -- c'est la seule facon de distinguer « l'entree n'est
            // pas lue » de « l'entree est lue mais ne produit rien ».
            entry["driverLatestIntent"] = DescribeValue(ReadPrivateField(driver, "latestIntent"));
            entry["physicsDriveIntent"] = DescribeValue(ReadPrivateField(body, "driveIntent"));

            if (rigidbody != null)
            {
                // UNE seule chaine : deux champs separes laissaient un doute sur l'appariement dans le
                // JSON, et on ne peut pas diagnostiquer un mouvement avec une lecture ambigue.
                entry["motion"] = string.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    "v=({0:F3},{1:F3},{2:F3}) |v|={3:F3} w=({4:F3},{5:F3},{6:F3}) pos=({7:F3},{8:F3},{9:F3}) sleeping={10}",
                    rigidbody.linearVelocity.x, rigidbody.linearVelocity.y, rigidbody.linearVelocity.z,
                    rigidbody.linearVelocity.magnitude,
                    rigidbody.angularVelocity.x, rigidbody.angularVelocity.y, rigidbody.angularVelocity.z,
                    rigidbody.position.x, rigidbody.position.y, rigidbody.position.z,
                    rigidbody.IsSleeping());
            }

            // Chaine de force, roue par roue : c'est ici qu'on voit OU la force s'effondre quand
            // l'intention arrive pourtant avec Throttle = 1. La vitesse de rotation est privee, donc
            // lue par reflexion -- sans elle on ne distingue pas « le couple n'est pas integre » de
            // « le pneu ne transmet rien ».
            var wheelLines = new List<string>();
            if (body != null)
            {
                entry["steerDegrees"] = body.CurrentSteerAngleDegrees;

                var angular = ReadPrivateField(body, "wheelAngularVelocity") as float[];
                for (var i = 0; i < body.WheelCount; i++)
                {
                    if (!body.TryGetTireSample(i, out var sample))
                    {
                        wheelLines.Add("#" + i + " : aucun echantillon");
                        continue;
                    }

                    var spin = angular != null && i < angular.Length ? angular[i].ToString("F3") : "?";
                    wheelLines.Add(string.Format(
                        System.Globalization.CultureInfo.InvariantCulture,
                        "#{0} sol={1} spin={2} gliss={3:F4} angle={4:F2} charge={5:F1} adherence={6:F1} Fmax={7:F1} F={8:F1}",
                        i, sample.Grounded, spin, sample.SlipRatio, sample.SlipAngleDegrees,
                        sample.NormalLoad, sample.Adherence, sample.MaximumForce, sample.ForceMagnitude));
                }
            }

            entry["wheels"] = wheelLines;

            vehicles.Add(entry);
        }

        report["vehicleCount"] = states.Length;
        report["vehicles"] = vehicles;

        // Etat JOUEUR : c'est lui qui decide si l'entree en siege aboutit. `CanEnterSeat` exige que le
        // joueur soit a pied, et le siege occupe est ce qui revendique `DriverClientId`. Detecte par NOM
        // DE TYPE plutot que par `using` : la sonde ne doit pas dependre du namespace de la couche joueur.
        var players = new List<object>();
        foreach (var behaviour in UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
        {
            if (behaviour == null || behaviour.GetType().Name != "NetworkedPlayerState")
            {
                continue;
            }

            players.Add(new Dictionary<string, object>
            {
                ["name"] = behaviour.name,
                ["members"] = DescribePublicMembers(behaviour),
            });
        }

        report["playerCount"] = players.Count;
        report["players"] = players;

        return report;
    }

    /// <summary>
    /// Reflete les membres PUBLICS de l'etat reseau : c'est ce qui evite de deviner le nom d'un
    /// accesseur de siege ou d'occupation. Chaque membre est lu sous try/catch -- un accesseur qui
    /// exige le serveur ne doit pas faire echouer la sonde entiere.
    /// </summary>
    private static List<string> DescribePublicMembers(object target)
    {
        var lines = new List<string>();
        var type = target.GetType();

        // Les donnees de gameplay de ce projet sont des NetworkVariable exposees en CHAMPS publics
        // (`Mode`, `SeatIndex`, `Lifecycle`, `WorldPosition`...) : enumerer les seules proprietes les
        // manquait toutes. Les deux families sont donc lues.
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            lines.Add(FormatMember(field.Name, () => field.GetValue(target)));
        }

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (property.GetIndexParameters().Length > 0 || !property.CanRead)
            {
                continue;
            }

            lines.Add(FormatMember(property.Name, () => property.GetValue(target)));
        }

        return lines;
    }

    /// <summary>
    /// Lit `LocalInputGate.IsBlocked` sans dependre de son namespace : le type est cherche par NOM dans
    /// les assemblies chargees. C'est la derniere porte franchie avant la lecture du clavier dans le
    /// controleur de conduite -- et la seule qui pousse une intention NEUTRE sans que le joueur l'ait
    /// demandee.
    /// </summary>
    private static string ReadStaticInputGate()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type type = null;
            try
            {
                type = assembly.GetTypes().FirstOrDefault(candidate => candidate.Name == "LocalInputGate");
            }
            catch (Exception)
            {
                continue;
            }

            if (type == null)
            {
                continue;
            }

            foreach (var member in new[] { "IsBlocked", "BlockedCount" })
            {
                var property = type.GetProperty(member, BindingFlags.Public | BindingFlags.Static);
                if (property != null)
                {
                    return type.FullName + "." + member + " = " + property.GetValue(null);
                }

                var field = type.GetField(member, BindingFlags.Public | BindingFlags.Static);
                if (field != null)
                {
                    return type.FullName + "." + member + " = " + field.GetValue(null);
                }
            }

            return type.FullName + " : aucun membre lisible";
        }

        return "(type LocalInputGate introuvable)";
    }

    /// <summary>Lit un champ prive en remontant la hierarchie de types, ou rend <c>null</c> s'il n'existe pas.</summary>
    private static object ReadPrivateField(object target, string name)
    {
        if (target == null)
        {
            return null;
        }

        var type = target.GetType();
        while (type != null)
        {
            var field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (field != null)
            {
                return field.GetValue(target);
            }

            type = type.BaseType;
        }

        return null;
    }

    /// <summary>Rend lisible une petite structure de valeur : tous ses champs et proprietes publiques.</summary>
    private static string DescribeValue(object value)
    {
        if (value == null)
        {
            return "(absent)";
        }

        var type = value.GetType();
        if (type.IsPrimitive || type == typeof(string))
        {
            return value.ToString();
        }

        var parts = new List<string>();
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            parts.Add(field.Name + "=" + (field.GetValue(value) ?? "null"));
        }

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (property.GetIndexParameters().Length > 0 || !property.CanRead)
            {
                continue;
            }

            parts.Add(property.Name + "=" + (property.GetValue(value) ?? "null"));
        }

        return parts.Count == 0 ? type.Name : type.Name + "{" + string.Join(",", parts) + "}";
    }

    /// <summary>Une ligne par membre, lue sous try/catch : un accesseur qui exige le serveur ne doit pas faire echouer la sonde.</summary>
    private static string FormatMember(string name, Func<object> read)
    {
        try
        {
            var value = read();
            if (value == null)
            {
                return name + " = null";
            }

            var valueType = value.GetType();
            if (valueType == typeof(bool) || valueType == typeof(int) || valueType == typeof(float)
                || valueType == typeof(ulong) || valueType == typeof(string))
            {
                return name + " = " + value;
            }

            // NetworkVariable<T> et consorts : la valeur utile est dans `Value`.
            var inner = valueType.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
            return inner == null
                ? name + " : " + valueType.Name
                : name + ".Value = " + (inner.GetValue(value) ?? "null");
        }
        catch (Exception exception)
        {
            return name + " : lecture impossible (" + exception.GetType().Name + ")";
        }
    }

    private static string HierarchyPath(Transform transform)
    {
        var parts = new List<string>();
        var current = transform;
        while (current != null)
        {
            parts.Insert(0, current.name);
            current = current.parent;
        }

        return string.Join("/", parts);
    }
}
