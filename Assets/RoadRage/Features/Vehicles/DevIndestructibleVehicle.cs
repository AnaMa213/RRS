using UnityEngine;

namespace RoadRage.Features.Vehicles
{
    /// <summary>
    /// Marqueur de DEVELOPPEMENT : ce vehicule ne subit aucun degat. Il existe pour pouvoir eprouver la
    /// conduite et la physique en percutant ce qu'on veut, sans perdre le vehicule au premier choc.
    ///
    /// Ce n'est PAS une fonctionnalite de jeu : le marqueur ne porte aucun etat, ne se replique pas, et
    /// le prefab qui le porte n'est monte ni par le lobby ni par la composition de run. Il ne vaut que
    /// pose a la main, ou par le chemin de spawn de developpement.
    ///
    /// Comment l'immunite est obtenue, et pourquoi la : c'est
    /// <see cref="NetworkedVehicleDriverController.OnCollisionEnter"/> qui leve l'evenement de
    /// collision, et c'est cet evenement qui alimente TOUT le chemin de degats (vehicule, puis
    /// occupants via <c>RunFlowController.ApplyNetworkedCollisionDamage</c>). Ne pas le lever pour ce
    /// vehicule rend donc l'immunite totale en un seul point, sans qu'aucune couche de degats ait a
    /// connaitre l'existence du developpement. Poser la garde plus loin aurait au contraire demande de
    /// modifier le code de jeu a plusieurs endroits.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DevIndestructibleVehicle : MonoBehaviour
    {
    }
}
