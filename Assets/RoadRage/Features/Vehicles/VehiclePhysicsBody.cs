using UnityEngine;

namespace RoadRage.Features.Vehicles
{
    /// <summary>
    /// Story 5.11 : LE composant physique du vehicule, porte par le prefab joueur **et** par le
    /// prefab IA (AD-35). Un seul composant de ce genre dans tout le projet : masse, centre de masse
    /// et tenseur d'inertie sont appliques au <c>Rigidbody</c> depuis un <see cref="VehicleProfileDef"/>
    /// authore, puis le contact au sol, la suspension et l'anti-roulis sont calcules par **raycasts
    /// par roue** dans <see cref="VehicleSuspensionModel"/>. Jamais de <c>WheelCollider</c>, jamais un
    /// reglage de chassis en <c>[SerializeField]</c> sur un controleur.
    ///
    /// ETAT INTERMEDIAIRE, NOMME ET DATTE -- 2026-09-18, Story 5.11. La couche physique propriete
    /// l'axe VERTICAL : gravite, ressort, amortisseur et anti-roulis. Les controleurs, eux, ecrivent
    /// encore la vitesse longitudinale et laterale en bloc et n'ecrivent plus la composante
    /// verticale. Ce partage est volontairement provisoire : la Story 5.12 (Tire Forces and Steering)
    /// remplace les ecritures de <c>linearVelocity</c> par des efforts aux roues et leve cet etat.
    /// Il est ecrit ici pour etre lu, pas subi.
    ///
    /// AUCUNE VERIFICATION D'AUTORITE, VOLONTAIREMENT. Les deux controleurs posent deja
    /// <c>body.isKinematic = !IsServer</c> : cote client un <c>AddForce</c> est sans effet. Ajouter ici
    /// un second test d'autorite creerait un second chemin de verite pour la meme regle (AD-21).
    ///
    /// Aucun etat de gameplay n'est tenu ni lu : ni rage, ni peur, ni disposition (AD-33).
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class VehiclePhysicsBody : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Profil physique authore. Non assigne : le composant ne s'active pas et le Rigidbody reste tel quel -- aucun repli numerique en dur.")]
        private VehicleProfileDef vehicleProfile;

        /// <summary>Une paire anti-roulis : les deux roues d'un meme essieu, la gauche d'abord.</summary>
        private struct WheelPair
        {
            public int LeftIndex;
            public int RightIndex;
        }

        private Rigidbody body;
        private VehicleProfile profile;
        private VehicleSuspensionModel.WheelState[] wheelStates;
        private float[] previousCompression;
        private bool[] wheelWasGrounded;
        private WheelPair[] antiRollPairs;
        private bool hasPreviousCompression;
        private bool warnedMissingProfile;
        private bool warnedInvalidProfile;

        /// <summary>Vrai quand un profil valide a effectivement ete applique. Faux : ce composant ne simule rien.</summary>
        public bool HasProfile { get; private set; }

        /// <summary>Nombre de roues simulees. Zero tant qu'aucun profil valide n'est applique.</summary>
        public int WheelCount
        {
            get { return wheelStates == null ? 0 : wheelStates.Length; }
        }

        /// <summary>Profil effectivement applique. Lu par la telemetrie de developpement (lecture seule).</summary>
        public VehicleProfile Profile
        {
            get { return profile; }
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            ApplyProfile();
        }

        /// <summary>
        /// Cablage du profil a l'execution : un appelant qui compose un vehicule hors prefab (harnais
        /// de developpement, test) n'a pas de reference serialisee a poser. Un vehicule pose en scene
        /// ou instancie depuis un prefab garde la reference authoree dans l'Inspector et n'a jamais
        /// besoin de cet appel -- meme motif que <c>BindLaneGraph</c> cote IA.
        /// </summary>
        public void BindProfile(VehicleProfileDef def)
        {
            vehicleProfile = def;
            ApplyProfile();
        }

        /// <summary>
        /// Applique le profil au <c>Rigidbody</c> : masse, centre de masse et tenseur d'inertie
        /// **explicites** (<c>automaticCenterOfMass</c> et <c>automaticInertiaTensor</c> desactives,
        /// donc plus aucun centre de masse implicite), interpolation et detection de collision
        /// continues, puis dimensionne les tampons de roue et les paires anti-roulis.
        /// </summary>
        public void ApplyProfile()
        {
            if (body == null)
            {
                body = GetComponent<Rigidbody>();
            }

            if (vehicleProfile == null)
            {
                if (!warnedMissingProfile)
                {
                    warnedMissingProfile = true;
                    Debug.LogWarning("[Vehicles] " + name + " : aucun VehicleProfileDef assigne, la couche physique reste inerte.", this);
                }

                ClearProfileState();
                return;
            }

            if (!vehicleProfile.TryValidate(out var error))
            {
                if (!warnedInvalidProfile)
                {
                    warnedInvalidProfile = true;
                    Debug.LogWarning("[Vehicles] " + name + " : VehicleProfileDef '" + vehicleProfile.RawId + "' invalide (" + error
                        + "), la couche physique reste inerte -- aucune valeur de repli.", this);
                }

                ClearProfileState();
                return;
            }

            profile = vehicleProfile.Profile;

            body.mass = profile.Mass;
            body.automaticCenterOfMass = false;
            body.centerOfMass = profile.CenterOfMass;
            body.automaticInertiaTensor = false;
            body.inertiaTensor = profile.InertiaTensor;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            wheelStates = new VehicleSuspensionModel.WheelState[profile.WheelCount];
            previousCompression = new float[profile.WheelCount];
            wheelWasGrounded = new bool[profile.WheelCount];
            hasPreviousCompression = false;
            antiRollPairs = BuildAntiRollPairs(profile);
            HasProfile = true;
        }

        /// <summary>
        /// Oublie tout etat applique. Appelee sur chaque chemin d'echec du cablage, pour qu'un
        /// <c>BindProfile</c> refuse ne laisse JAMAIS derriere lui l'ancien profil : <see cref="HasProfile"/>,
        /// <see cref="Profile"/> et les tampons de roue decrivent l'etat present, pas un souvenir.
        /// </summary>
        private void ClearProfileState()
        {
            profile = default;
            wheelStates = null;
            previousCompression = null;
            wheelWasGrounded = null;
            antiRollPairs = null;
            hasPreviousCompression = false;
            HasProfile = false;
        }

        /// <summary>
        /// Remet a zero la memoire de suspension. A appeler par TOUT chemin qui deplace le vehicule
        /// d'un coup -- recuperation, teleportation reseau -- : la vitesse de compression vient d'une
        /// difference finie entre deux frames, et un deplacement discontinu lui ferait produire un pic
        /// d'amortisseur que rien, physiquement, ne justifie.
        /// </summary>
        public void ResetSuspensionState()
        {
            hasPreviousCompression = false;

            if (previousCompression == null || wheelWasGrounded == null)
            {
                return;
            }

            for (var i = 0; i < previousCompression.Length; i++)
            {
                previousCompression[i] = 0f;
                wheelWasGrounded[i] = false;
            }
        }

        private void FixedUpdate()
        {
            if (!HasProfile || body == null || wheelStates == null)
            {
                return;
            }

            var fixedDeltaTime = Time.fixedDeltaTime;
            if (fixedDeltaTime <= 0f)
            {
                return;
            }

            var up = transform.up;
            var current = profile;
            var groundedWheels = 0;

            // Charge statique d'une roue : elle borne la force de suspension. Derivee du vehicule, donc
            // elle suit la masse sans reglage supplementaire.
            var staticLoad = VehicleSuspensionModel.ResolveStaticLoad(current.Mass, Physics.gravity.y, current.WheelCount);

            var planarVelocity = Vector3.ProjectOnPlane(body.linearVelocity, Vector3.up);
            var forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            if (forward.sqrMagnitude < 0.0001f)
            {
                forward = Vector3.forward;
            }

            forward.Normalize();
            var right = Vector3.Cross(Vector3.up, forward).normalized;

            for (var i = 0; i < wheelStates.Length; i++)
            {
                var wheel = current.GetWheel(i);
                var anchor = transform.TransformPoint(wheel.LocalPosition);
                var hasContact = Physics.Raycast(
                    anchor,
                    -up,
                    out var hit,
                    current.RestLength,
                    current.GroundMask,
                    QueryTriggerInteraction.Ignore);

                var rayDistance = hasContact ? hit.distance : 0f;
                var grounded = hasContact && !IsDynamicBody(hit) && VehicleSuspensionModel.IsWheelGrounded(hasContact, rayDistance, current.RestLength);
                var compression = VehicleSuspensionModel.ResolveCompression(grounded, rayDistance, current.RestLength, current.Travel);
                // La vitesse de compression vient d'une difference finie ENTRE DEUX FRAMES OU LA ROUE
                // TOUCHAIT. La calculer a travers une perte de contact produit un pic artificiel -- la
                // compression passe de 0 a sa valeur en un pas, soit plusieurs metres par seconde -- et
                // ce pic injecte a chaque reprise de contact une force d'amortisseur qui fait trembler
                // la caisse sans fin, donc un vehicule qui « bouge tout seul ». Un amortisseur agit sur
                // la vitesse RELATIVE de la suspension, qui n'a aucun sens a travers un decollement.
                var compressionVelocity = hasPreviousCompression && wheelWasGrounded[i]
                    ? (compression - previousCompression[i]) / fixedDeltaTime
                    : 0f;
                previousCompression[i] = compression;
                wheelWasGrounded[i] = grounded;

                var contactPoint = grounded ? hit.point : anchor;
                wheelStates[i] = new VehicleSuspensionModel.WheelState(
                    grounded,
                    compression,
                    contactPoint,
                    contactPoint + (up * wheel.Radius));

                if (!grounded)
                {
                    continue;
                }

                groundedWheels++;

                var force = VehicleSuspensionModel.ResolveSuspensionForce(
                    compression,
                    compressionVelocity,
                    current.SpringRate,
                    current.Damper,
                    staticLoad);

                if (force > 0f)
                {
                    body.AddForceAtPosition(up * force, contactPoint, ForceMode.Force);

                    // Frottement de contact, borne par la charge que CETTE roue porte : c'est un contact,
                    // pas un modele de pneu (la Story 5.12 le remplace par un glissement progressif
                    // pilote par le slip). Sans lui, un vehicule pose sur quatre rayons n'a aucun
                    // frottement -- il glisse comme sur de la glace et un choc de flanc l'emporte.
                    var friction = VehicleSuspensionModel.ResolveGroundFrictionForce(
                        planarVelocity,
                        forward,
                        right,
                        force,
                        current.LateralFrictionCoefficient,
                        current.RollingResistanceCoefficient);

                    if (friction.sqrMagnitude > 0f)
                    {
                        body.AddForceAtPosition(friction, contactPoint, ForceMode.Force);
                    }
                }
            }

            hasPreviousCompression = true;
            ApplyAntiRoll(up);
            ApplyAttitudeAssist(up, groundedWheels);
        }

        /// <summary>
        /// Assiette : rappel vers la verticale et amortissement du TANGAGE et du ROULIS. Applique
        /// seulement quand au moins une roue touche -- en l'air, un vehicule garde son assiette, un saut
        /// ne se redresse pas tout seul. Le LACET n'est jamais touche : il porte la direction.
        ///
        /// Sans ce terme, la suspension ne peut pas redresser une caisse qui s'est couchee : au-dela
        /// d'une vingtaine de degres d'inclinaison, les ancrages de roue montent au-dessus de la
        /// longueur au repos, les rayons ne touchent plus le sol et la suspension cesse d'exister. Le
        /// vehicule restait alors couche indefiniment.
        /// </summary>
        private void ApplyAttitudeAssist(Vector3 up, int groundedWheels)
        {
            if (groundedWheels <= 0)
            {
                return;
            }

            var torque = VehicleSuspensionModel.ResolveLevellingTorque(up, Vector3.up, profile.AttitudeLevellingRate)
                + VehicleSuspensionModel.ResolveAttitudeDampingTorque(
                    body.angularVelocity,
                    transform.forward,
                    transform.right,
                    profile.AttitudeDamping);

            if (torque.sqrMagnitude > 0f)
            {
                body.AddTorque(torque, ForceMode.Force);
            }
        }

        /// <summary>
        /// Anti-roulis : le cote le plus comprime est CHARGE et l'autre SOULAGE, ce qui redresse la
        /// caisse sans faire disparaitre le roulis. La paire de forces est calculee dans
        /// <see cref="VehicleSuspensionModel.ResolveAntiRollForces"/> : le sens est rendu par la
        /// fonction pure, jamais reinterprete ici -- il avait deja ete inverse une fois, ce qui faisait
        /// pencher les vehicules au lieu de les redresser. Une roue en l'air garde une compression
        /// nulle, donc ne participe qu'en recevant la charge transferee.
        /// </summary>
        private void ApplyAntiRoll(Vector3 up)
        {
            var rate = profile.AntiRollRate;
            if (rate <= 0f || antiRollPairs == null)
            {
                return;
            }

            for (var i = 0; i < antiRollPairs.Length; i++)
            {
                var left = wheelStates[antiRollPairs[i].LeftIndex];
                var right = wheelStates[antiRollPairs[i].RightIndex];

                if (!left.Grounded && !right.Grounded)
                {
                    continue;
                }

                VehicleSuspensionModel.ResolveAntiRollForces(
                    left.Compression,
                    right.Compression,
                    rate,
                    out var leftForce,
                    out var rightForce);

                if (left.Grounded && leftForce != 0f)
                {
                    body.AddForceAtPosition(up * leftForce, left.ContactPoint, ForceMode.Force);
                }

                if (right.Grounded && rightForce != 0f)
                {
                    body.AddForceAtPosition(up * rightForce, right.ContactPoint, ForceMode.Force);
                }
            }
        }

        /// <summary>
        /// Etat de la roue d'index donne pour la derniere frame simulee. Lecture seule : c'est la
        /// seule fenetre de la telemetrie sur la couche physique, et elle n'ecrit rien.
        /// </summary>
        public bool TryGetWheelState(int index, out VehicleSuspensionModel.WheelState state)
        {
            if (wheelStates == null || index < 0 || index >= wheelStates.Length)
            {
                state = default;
                return false;
            }

            state = wheelStates[index];
            return true;
        }

        /// <summary>
        /// Echantillon de caisse (vitesse, vitesse laterale, glissement, angle de derive), calcule a la
        /// demande depuis la vitesse du <c>Rigidbody</c> et les axes du vehicule. Aucun etat n'est
        /// stocke : la fonction appelee est pure.
        /// </summary>
        public bool TrySampleTelemetry(out VehicleSuspensionModel.TelemetrySample sample)
        {
            if (body == null)
            {
                sample = default;
                return false;
            }

            sample = VehicleSuspensionModel.SampleTelemetry(body.linearVelocity, transform.forward, transform.right);
            return true;
        }

        /// <summary>
        /// Le sol est une geometrie AUTOR EE, donc statique. Le masque de sol est la couche par defaut, qui
        /// porte aussi les autres vehicules, les personnages et les props : sans cette garde, une roue
        /// posee sur le toit d'une autre voiture prendrait cette voiture pour le sol et pousserait
        /// contre elle. Un corps non cinematique n'est jamais un sol.
        /// </summary>
        private static bool IsDynamicBody(RaycastHit hit)
        {
            return hit.rigidbody != null && !hit.rigidbody.isKinematic;
        }

        /// <summary>
        /// Apparie les roues par essieu : la roue au plus petit x local est la gauche. La validation du
        /// profil garantit exactement deux roues par essieu, donc l'appariement ne peut pas laisser une
        /// roue orpheline.
        /// </summary>
        private static WheelPair[] BuildAntiRollPairs(VehicleProfile current)
        {
            var pairs = new WheelPair[current.WheelCount / 2];
            var filled = 0;

            for (var i = 0; i < current.WheelCount; i++)
            {
                var axleIndex = current.GetWheel(i).AxleIndex;
                if (HasEarlierWheelOnAxle(current, i, axleIndex))
                {
                    continue;
                }

                var partner = -1;
                for (var j = i + 1; j < current.WheelCount; j++)
                {
                    if (current.GetWheel(j).AxleIndex == axleIndex)
                    {
                        partner = j;
                        break;
                    }
                }

                pairs[filled++] = current.GetWheel(i).LocalPosition.x <= current.GetWheel(partner).LocalPosition.x
                    ? new WheelPair { LeftIndex = i, RightIndex = partner }
                    : new WheelPair { LeftIndex = partner, RightIndex = i };
            }

            return pairs;
        }

        private static bool HasEarlierWheelOnAxle(VehicleProfile current, int index, int axleIndex)
        {
            for (var i = 0; i < index; i++)
            {
                if (current.GetWheel(i).AxleIndex == axleIndex)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
