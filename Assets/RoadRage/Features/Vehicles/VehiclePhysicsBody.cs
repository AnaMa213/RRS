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
    /// Story 5.12 : la couche propriete desormais le PLAN HORIZONTAL aussi. Le longitudinal et le
    /// lateral ne viennent plus d'une ecriture de <c>linearVelocity</c> ni d'un <c>MoveRotation</c>,
    /// mais d'efforts de pneu appliques au point de contact de chaque roue, calcules dans
    /// <see cref="VehicleTireModel"/> (glissement, courbe de force avec pic puis chute, budget
    /// d'adherence). La direction est un **angle de roue** (<see cref="VehicleSteeringModel"/>) suivi
    /// par les roues marquees <see cref="VehicleWheel.IsSteering"/>, et le couple est envoye aux seules
    /// roues marquees <see cref="VehicleWheel.IsDriven"/>.
    ///
    /// ETAT INTERMEDIAIRE, NOMME ET DATTE -- 2026-09-18, Story 5.12. Le vehicule JOUEUR passe
    /// entierement par <see cref="ApplyDriveIntent"/> : il n'ecrit plus jamais la vitesse. Le vehicule
    /// IA, lui, ecrit encore <c>linearVelocity</c> et <c>MoveRotation</c> dans
    /// <c>NetworkedAIVehicleDriverController.ApplyMovement</c> : c'est ce que la Story 5.14 (AI drives
    /// by intent) supprime. La couche physique est deja la seule et la meme pour les deux -- c'est
    /// elle que la 5.14 consommera.
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
        private VehicleTireModel.TireSample[] tireSamples;
        private float[] previousCompression;
        private bool[] wheelWasGrounded;
        private float[] wheelAngularVelocity;
        private WheelPair[] antiRollPairs;
        private bool hasPreviousCompression;
        private bool warnedMissingProfile;
        private bool warnedInvalidProfile;

        // Etat de conduite du pas courant. Il vit ici, et pas dans un controleur, parce que c'est la
        // seule couche qui en fait quelque chose : un angle de roue, un couple par roue, une rotation
        // par roue. Les valeurs par defaut viennent du profil (vehicule au repos, roues droites).
        private VehicleDriveIntent driveIntent = VehicleDriveIntent.Idle;
        private float driveMaxForwardSpeed;
        private float driveBrakeTorque;
        private float driveSteerRateDegreesPerSecond;
        private float currentSteerAngleDegrees;

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
            tireSamples = new VehicleTireModel.TireSample[profile.WheelCount];
            previousCompression = new float[profile.WheelCount];
            wheelWasGrounded = new bool[profile.WheelCount];
            wheelAngularVelocity = new float[profile.WheelCount];
            hasPreviousCompression = false;
            antiRollPairs = BuildAntiRollPairs(profile);

            // Etat de conduite remis au repos : aucun intent tant qu'un conducteur n'en a pas soumis
            // un, roues droites, et les trois echelles prises dans le profil (une seule source).
            driveIntent = VehicleDriveIntent.Idle;
            driveMaxForwardSpeed = profile.MaxForwardSpeed;
            driveBrakeTorque = profile.BrakeTorque;
            driveSteerRateDegreesPerSecond = profile.SteerRateDegreesPerSecond;
            currentSteerAngleDegrees = 0f;
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
            tireSamples = null;
            previousCompression = null;
            wheelWasGrounded = null;
            wheelAngularVelocity = null;
            antiRollPairs = null;
            hasPreviousCompression = false;
            driveIntent = VehicleDriveIntent.Idle;
            currentSteerAngleDegrees = 0f;
            HasProfile = false;
        }

        /// <summary>
        /// Point d'entree d'intent de conduite (Story 5.12). C'est LE chemin par lequel un vehicule
        /// joueur se deplace : le controleur soumet son intention, la couche physique la transforme en
        /// couples aux roues et en angle de roue. Aucune ecriture de <c>linearVelocity</c> ni de
        /// rotation de caisse n'existe plus dans ce chemin.
        ///
        /// L'intent est STOCKE et consomme par le prochain <c>FixedUpdate</c> de ce composant : l'ordre
        /// d'execution entre le controleur et la couche physique n'a donc aucune importance, ce qui,
        /// sans ce stockage, ferait dependre la conduite de l'ordre des scripts.
        ///
        /// <paramref name="maxForwardSpeed"/> et <paramref name="steerRateDegreesPerSecond"/> et
        /// <paramref name="brakeTorque"/> viennent du profil, eventuellement reduits par les degats de
        /// la Story 3.5 (moteur, roue, freins) : c'est l'autorite de conduite qui est reduite, jamais
        /// une vitesse ecrite.
        /// </summary>
        public void ApplyDriveIntent(
            VehicleDriveIntent intent,
            float maxForwardSpeed,
            float steerRateDegreesPerSecond,
            float brakeTorque)
        {
            driveIntent = intent;
            driveMaxForwardSpeed = maxForwardSpeed;
            driveSteerRateDegreesPerSecond = steerRateDegreesPerSecond;
            driveBrakeTorque = brakeTorque;
        }

        /// <summary>
        /// Remet a zero la memoire de suspension et la rotation des roues. A appeler par TOUT chemin
        /// qui deplace le vehicule d'un coup -- recuperation, teleportation reseau -- : la vitesse de
        /// compression vient d'une difference finie entre deux frames, et un deplacement discontinu
        /// lui ferait produire un pic d'amortisseur que rien, physiquement, ne justifie. La rotation
        /// des roues est remise a zero pour la meme raison : sa valeur n'a plus aucun sens apres un
        /// saut de position, et le glissement qu'elle produirait au pas suivant serait un glissement
        /// invente.
        /// </summary>
        public void ResetSuspensionState()
        {
            hasPreviousCompression = false;

            if (previousCompression != null && wheelWasGrounded != null)
            {
                for (var i = 0; i < previousCompression.Length; i++)
                {
                    previousCompression[i] = 0f;
                    wheelWasGrounded[i] = false;
                }
            }

            if (wheelAngularVelocity == null)
            {
                return;
            }

            for (var i = 0; i < wheelAngularVelocity.Length; i++)
            {
                wheelAngularVelocity[i] = 0f;
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
            var longitudinalSpeed = Vector3.Dot(planarVelocity, forward);

            // Direction : la consigne avance vers son angle cible a taux borne -- braquage ou rappel,
            // jamais un saut. Puis chaque roue prend cet angle si et seulement si elle est authoree
            // directrice (`VehicleWheel.IsSteering`, inerte depuis la Story 5.11).
            UpdateSteeringState(current, fixedDeltaTime, longitudinalSpeed);

            // Train roulant : les couples d'un pas, identiques sur toutes les roues qui y ont droit.
            // La repartition par roue (motrice ; arriere pour le frein a main) se fait dans la boucle.
            var driveTorque = VehicleTireModel.ResolveWheelDriveTorque(
                driveIntent.Throttle,
                driveIntent.BrakeReverse,
                longitudinalSpeed,
                current.MinimumDirectionSpeed,
                current.EngineTorque,
                current.ReverseTorque,
                driveMaxForwardSpeed,
                current.MaxReverseSpeed);

            var serviceBrakeTorque = VehicleTireModel.ResolveWheelBrakeTorque(
                driveIntent.Throttle,
                driveIntent.BrakeReverse,
                longitudinalSpeed,
                current.MinimumDirectionSpeed,
                driveBrakeTorque,
                current.CoastTorque);

            var handbrakeEngaged = driveIntent.Handbrake > 0.0001f;
            var adherence = current.LateralFrictionCoefficient;

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

                // La charge normale portee par CETTE roue est la force que la suspension vient de
                // calculer. C'est l'entree manquante du pneu : elle borne tout ce qu'il peut
                // transmettre, et elle vaut zero des que la roue ne touche plus.
                var normalLoad = grounded
                    ? VehicleSuspensionModel.ResolveSuspensionForce(
                        compression,
                        compressionVelocity,
                        current.SpringRate,
                        current.Damper,
                        staticLoad)
                    : 0f;

                // La roue AU SOL compte, meme si sa charge est nulle pour ce pas : c'est ce compte qui
                // decide si l'assiette peut etre corrigee, et une caisse posee sur ses roues mais
                // momentanement delestee doit continuer d'etre redressee. Le compter sur la charge
                // ferait disparaitre la correction d'assiette exactement quand elle sert.
                if (grounded)
                {
                    groundedWheels++;
                }

                if (normalLoad > 0f)
                {
                    body.AddForceAtPosition(up * normalLoad, contactPoint, ForceMode.Force);
                }

                // Repere de la roue : l'axe avant de la caisse tourne de l'angle de braquage effectif.
                var steerAngleDegrees = VehicleSteeringModel.ResolveWheelSteerAngleDegrees(wheel.IsSteering, currentSteerAngleDegrees);
                var steerRotation = Quaternion.AngleAxis(steerAngleDegrees, up);
                var wheelForward = steerRotation * forward;
                var wheelRight = steerRotation * right;

                // Frein a main : il agit sur les roues ARRIERE, c'est-a-dire celles qui ne braquent pas.
                // C'est le blocage de ces roues -- et lui seul -- qui effondre leur adherence laterale
                // et fait entrer le vehicule en derive.
                var handbrakeOnThisWheel = handbrakeEngaged && !wheel.IsSteering;
                var wheelDriveTorque = wheel.IsDriven && !handbrakeOnThisWheel ? driveTorque : 0f;
                var wheelBrakeTorque = handbrakeOnThisWheel
                    ? Mathf.Max(serviceBrakeTorque, current.HandbrakeTorque)
                    : serviceBrakeTorque;

                var appliedLongitudinal = 0f;
                var appliedLateral = 0f;
                var slipRatio = 0f;
                var slipAngleDegrees = 0f;

                if (normalLoad > 0f)
                {
                    // La vitesse du point de contact -- et non celle du centre de masse : elle seule
                    // contient la contribution du lacet et du roulis, qui est ce qui fait glisser une
                    // roue quand la caisse tourne.
                    var contactVelocity = body.GetPointVelocity(contactPoint);
                    var contactLongitudinal = Vector3.Dot(contactVelocity, wheelForward);
                    var contactLateral = Vector3.Dot(contactVelocity, wheelRight);

                    slipRatio = VehicleTireModel.ResolveSlipRatio(wheelAngularVelocity[i] * wheel.Radius, contactLongitudinal);
                    slipAngleDegrees = VehicleTireModel.ResolveSlipAngleDegrees(contactLongitudinal, contactLateral);

                    // Un pneu n'a qu'UN budget d'adherence, et les deux glissements le partagent : c'est
                    // la fonction pure qui le repartit (chaque glissement normalise par son propre pic,
                    // une seule magnitude, une seule direction). Le sens des deux axes vient donc du
                    // signe des glissements mesures, et rien n'est re-corrige ici : le longitudinal est
                    // positif quand la bande de roulement avance plus vite que le sol (roue motrice),
                    // et l'angle de glissement porte deja l'opposition au glissement lateral.
                    var tireForces = VehicleTireModel.ResolveTireForces(
                        slipRatio,
                        current.TirePeakSlipRatio,
                        slipAngleDegrees,
                        current.TirePeakSlipAngleDegrees,
                        current.TireSlipFalloffFraction,
                        adherence,
                        normalLoad);

                    // Attenuation basse vitesse (equivalente a la rampe de 0,5 m/s de la Story 5.11) :
                    // sans elle, la force laterale pleine a l'arret ferait brouter un vehicule gare au
                    // lieu de l'immobiliser.
                    var ramp = VehicleTireModel.ResolveLowSpeedRamp(Mathf.Sqrt((contactLongitudinal * contactLongitudinal) + (contactLateral * contactLateral)));
                    appliedLongitudinal = tireForces.x * ramp;
                    appliedLateral = tireForces.y * ramp;

                    var tireForce = (wheelForward * appliedLongitudinal) + (wheelRight * appliedLateral);
                    if (tireForce.sqrMagnitude > 0f)
                    {
                        body.AddForceAtPosition(tireForce, contactPoint, ForceMode.Force);
                    }
                }

                // Rotation de la roue : couple moteur, reaction du pneu et frein. Elle est integree meme
                // en l'air -- une roue motrice qui ne touche pas continue de tourner, et c'est
                // exactement ce que le glissement de l'atterrissage doit retrouver.
                wheelAngularVelocity[i] = VehicleTireModel.IntegrateWheelAngularVelocity(
                    wheelAngularVelocity[i],
                    wheelDriveTorque,
                    wheelBrakeTorque,
                    appliedLongitudinal,
                    wheel.Radius,
                    current.WheelInertia,
                    fixedDeltaTime);

                tireSamples[i] = VehicleTireModel.SampleTire(
                    grounded,
                    normalLoad,
                    slipRatio,
                    slipAngleDegrees,
                    Mathf.Sqrt((appliedLongitudinal * appliedLongitudinal) + (appliedLateral * appliedLateral)),
                    adherence);
            }

            hasPreviousCompression = true;
            ApplyAntiRoll(up);
            ApplyAttitudeAssist(up, groundedWheels);
        }

        /// <summary>
        /// Avance la consigne d'angle de roue d'un pas. L'angle cible diminue avec la vitesse (la
        /// direction reste lisible a vitesse elevee) et le retour au centre a son propre taux authore :
        /// relacher la direction ne remet jamais les roues droites d'un coup.
        ///
        /// Sous le seuil de vitesse authore, la consigne est nulle -- un vehicule quasi immobile ne
        /// braque pas ses roues, et l'angle ne s'accumule donc pas pour se liberer d'un coup au premier
        /// metre parcouru.
        /// </summary>
        private void UpdateSteeringState(VehicleProfile current, float fixedDeltaTime, float longitudinalSpeed)
        {
            var target = VehicleSteeringModel.ResolveSteerAngleDegrees(
                driveIntent.Steer,
                longitudinalSpeed,
                current.MinimumDirectionSpeed,
                current.MaxSteerAngleDegrees,
                current.HighSpeedSteerAngleDegrees,
                current.SteerFullReductionSpeed);

            var rate = VehicleSteeringModel.ResolveSteerRateDegreesPerSecond(
                currentSteerAngleDegrees,
                target,
                driveSteerRateDegreesPerSecond,
                current.SteerReturnRateDegreesPerSecond);

            currentSteerAngleDegrees = VehicleSteeringModel.MoveSteerAngleDegrees(
                currentSteerAngleDegrees,
                target,
                rate,
                fixedDeltaTime);
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
        /// Echantillon de pneu de la roue d'index donne pour la derniere frame simulee : glissement
        /// longitudinal et angulaire, charge portee, force transmise et adherence disponible. Lecture
        /// seule, comme <see cref="TryGetWheelState"/> -- c'est la seconde fenetre de la telemetrie sur
        /// la couche physique, et elle n'ecrit rien.
        /// </summary>
        public bool TryGetTireSample(int index, out VehicleTireModel.TireSample sample)
        {
            if (tireSamples == null || index < 0 || index >= tireSamples.Length)
            {
                sample = default;
                return false;
            }

            sample = tireSamples[index];
            return true;
        }

        /// <summary>Angle de roue courant des roues directrices (degres), signe. Lecture seule, pour la telemetrie.</summary>
        public float CurrentSteerAngleDegrees
        {
            get { return currentSteerAngleDegrees; }
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
