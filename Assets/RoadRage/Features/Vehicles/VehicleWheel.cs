using System;
using UnityEngine;

namespace RoadRage.Features.Vehicles
{
    /// <summary>
    /// Story 5.11 : implantation authoree d'une roue, telle qu'elle vit dans un
    /// <see cref="VehicleProfile"/>. Une roue n'est **pas** un GameObject : le prefab greybox n'en
    /// porte aucun et ne doit pas en gagner (roues visuelles = art, AD-14 ; ajouter des enfants
    /// changerait la composition du prefab et la garde de parite joueur/IA). Sa position locale est
    /// donc de la donnee, posee une fois pour les deux prefabs.
    ///
    /// Geometrie du modele (AD-35 : raycasts par roue, jamais <c>WheelCollider</c>) :
    /// <see cref="LocalPosition"/> est l'**origine du raycast** -- le haut de la course de
    /// suspension. Le rayon descend de <see cref="VehicleProfile.RestLength"/> au plus, et la
    /// distance effectivement parcourue jusqu'au sol est la longueur courante de la suspension ;
    /// <see cref="VehicleSuspensionModel.ResolveCompression"/> en deduit la compression. La roue
    /// elle-meme est ponctuelle pour la physique : <see cref="Radius"/> sert a placer son moyeu
    /// (contact + rayon, vers le haut) pour la presentation et la telemetrie.
    /// </summary>
    [Serializable]
    public struct VehicleWheel
    {
        [SerializeField]
        [Tooltip("Origine du raycast de suspension, en coordonnees locales du Rigidbody (le haut de la course).")]
        private Vector3 localPosition;

        [SerializeField]
        [Min(0.01f)]
        [Tooltip("Rayon de la roue (m) : place le moyeu a 'contact + rayon' pour la presentation. Ne participe pas a la compression.")]
        private float radius;

        [SerializeField]
        [Min(0)]
        [Tooltip("Essieu : deux roues de meme index forment une paire anti-roulis. Chaque essieu doit porter exactement deux roues.")]
        private int axleIndex;

        [SerializeField]
        [Tooltip("Roue directrice. Lue par la Story 5.12 (efforts de pneu et direction) : inerte en 5.11.")]
        private bool isSteering;

        [SerializeField]
        [Tooltip("Roue motrice. Lue par la Story 5.12 (efforts de pneu et direction) : inerte en 5.11.")]
        private bool isDriven;

        public VehicleWheel(Vector3 localPosition, float radius, int axleIndex, bool isSteering, bool isDriven)
        {
            this.localPosition = localPosition;
            this.radius = radius;
            this.axleIndex = axleIndex;
            this.isSteering = isSteering;
            this.isDriven = isDriven;
        }

        /// <summary>Origine du raycast de suspension, en local.</summary>
        public Vector3 LocalPosition
        {
            get { return localPosition; }
        }

        /// <summary>Rayon de la roue (m).</summary>
        public float Radius
        {
            get { return radius; }
        }

        /// <summary>Essieu d'appartenance : c'est lui qui apparie deux roues pour l'anti-roulis.</summary>
        public int AxleIndex
        {
            get { return axleIndex; }
        }

        /// <summary>Roue directrice (Story 5.12).</summary>
        public bool IsSteering
        {
            get { return isSteering; }
        }

        /// <summary>Roue motrice (Story 5.12).</summary>
        public bool IsDriven
        {
            get { return isDriven; }
        }
    }
}
