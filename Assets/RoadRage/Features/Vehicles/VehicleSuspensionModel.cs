using UnityEngine;

namespace RoadRage.Features.Vehicles
{
    /// <summary>
    /// Story 5.11 : le modele de suspension, en fonctions **pures**. C'est la seule partie de la
    /// couche physique qui se prouve sans scene, sans <c>Rigidbody</c> et sans <c>Time</c> -- et
    /// c'est deliberé : <c>WheelCollider</c> ne se prouverait que pendant un pas de physique, alors
    /// qu'une fonction pure se prouve en EditMode (AD-35 : decision de testabilite, pas de fidelite).
    ///
    /// Conventions, valables partout dans la couche :
    /// - une force de suspension est un **scalaire positif vers le haut**, applique par l'appelant au
    ///   point de contact le long de l'axe haut du vehicule ;
    /// - une force anti-roulis est un **scalaire signe**, applique en opposition sur les deux roues
    ///   d'un meme essieu (positif = la roue gauche est la plus comprimee, donc on charge la droite) ;
    /// - une roue decollee ne produit rien : compression 0, force 0.
    /// </summary>
    public static class VehicleSuspensionModel
    {
        /// <summary>Vitesse planaire sous laquelle le glissement relatif n'est pas calcule : diviser par une vitesse quasi nulle donnerait un rapport arbitraire.</summary>
        private const float SlipSpeedEpsilon = 0.01f;

        /// <summary>
        /// Plafond d'une force de suspension, exprime en multiples de la charge statique d'une roue.
        ///
        /// Ce n'est pas un reglage de confort, c'est un garde-fou de modele : la force d'amortisseur
        /// vient d'une difference finie, donc elle vaut `damper x (derivee de compression)`, une
        /// quantite que rien ne borne pendant un choc violent. Une suspension reelle ne rend jamais
        /// plus de trois a quatre fois la charge qu'elle porte ; au-dela, elle se comporte comme un
        /// propulseur -- c'est exactement ce qui faisait decoller un vehicule encaisse violemment et
        /// continuer de monter. La borne est DERIVEE du vehicule (sa propre charge statique), jamais un
        /// nombre absolu : elle s'adapte a la masse sans reglage.
        /// </summary>
        private const float MaxSuspensionForceMultiple = 4f;

        /// <summary>Vitesse sous laquelle le frottement de contact s'attenue, pour ne pas faire broutter un vehicule a l'arret.</summary>
        private const float FrictionRampSpeed = 0.5f;

        /// <summary>Charge statique portee par une roue (N) : le poids du vehicule reparti sur ses roues.</summary>
        public static float ResolveStaticLoad(float mass, float gravity, int wheelCount)
        {
            if (wheelCount <= 0 || !float.IsFinite(mass) || !float.IsFinite(gravity))
            {
                return 0f;
            }

            return mass * Mathf.Abs(gravity) / wheelCount;
        }

        /// <summary>
        /// Compression statique d'une roue au repos : la part de poids qu'elle porte divisee par sa
        /// raideur. C'est elle qui fixe la hauteur de caisse au repos, donc la garde au sol du
        /// chassis -- d'ou l'interet de la connaitre avant de choisir une hauteur de bordure.
        ///
        /// La gravite est prise en valeur absolue : <c>Physics.gravity.y</c> se passe tel quel.
        /// </summary>
        public static float ResolveStaticCompression(float mass, float gravity, int wheelCount, float springRate)
        {
            if (wheelCount <= 0
                || !float.IsFinite(mass)
                || !float.IsFinite(gravity)
                || !float.IsFinite(springRate)
                || springRate <= 0f)
            {
                return 0f;
            }

            return Mathf.Max(0f, mass * Mathf.Abs(gravity) / (wheelCount * springRate));
        }

        /// <summary>
        /// Une roue est posee des lors que son rayon a touche ET que la longueur courante reste sous
        /// la longueur au repos. Un rayon plus long que <paramref name="restLength"/> signifie que la
        /// roue pend dans le vide : elle est en l'air, sans exception et sans valeur de repli.
        /// </summary>
        public static bool IsWheelGrounded(bool hasContact, float rayDistance, float restLength)
        {
            return hasContact && float.IsFinite(rayDistance) && rayDistance < restLength;
        }

        /// <summary>
        /// Compression d'une roue : <c>restLength - distance</c>, bornee a <c>[0, travel]</c>. Une
        /// roue decollee (ou dont le rayon n'a rien touche) vaut exactement 0 : elle ne contribue
        /// jamais a l'anti-roulis par une valeur inventee.
        /// </summary>
        public static float ResolveCompression(bool grounded, float rayDistance, float restLength, float travel)
        {
            if (!grounded)
            {
                return 0f;
            }

            return Mathf.Clamp(restLength - rayDistance, 0f, Mathf.Max(0f, travel));
        }

        /// <summary>
        /// Force ressort + amortisseur, en newtons, positive vers le haut, **bornee par la charge
        /// statique de la roue** (voir <see cref="MaxSuspensionForceMultiple"/>).
        ///
        /// <paramref name="compressionVelocity"/> est la derivee de la compression : positive quand la
        /// roue se comprime, et l'amortisseur s'y oppose alors dans le meme sens que le ressort.
        /// Le total est borne a zero : un ressort de suspension ne tire pas le chassis vers le sol,
        /// donc la detente ne peut pas aspirer la caisse sous sa roue.
        /// </summary>
        public static float ResolveSuspensionForce(
            float compression,
            float compressionVelocity,
            float springRate,
            float damper,
            float staticLoad)
        {
            var spring = Mathf.Max(0f, springRate) * Mathf.Max(0f, compression);
            var damping = Mathf.Max(0f, damper) * compressionVelocity;
            var force = Mathf.Max(0f, spring + damping);

            if (!float.IsFinite(staticLoad) || staticLoad <= 0f)
            {
                return force;
            }

            return Mathf.Min(force, staticLoad * MaxSuspensionForceMultiple);
        }

        /// <summary>
        /// Force de contact au sol : frottement LATERAL (la bande de roulement resiste au glissement de
        /// travers) et resistance au roulement (faible, dans l'axe de la roue), appliquees le long des
        /// axes donnes. Les deux sont bornees par la charge que la roue porte -- un pneu ne transmet pas
        /// plus que ce que le sol lui rend -- avec une attenuation sous <see cref="FrictionRampSpeed"/>,
        /// sinon le vehicule brouterait a l'arret au lieu de s'immobiliser.
        ///
        /// Sans ce terme, un vehicule pose sur quatre rayons n'a AUCUN frottement : il glisse comme sur
        /// de la glace, et un choc lateral l'emporte sur plusieurs metres. C'est un contact, pas un
        /// modele de pneu : la Story 5.12 le remplace par un glissement progressif pilote par le slip.
        /// </summary>
        public static Vector3 ResolveGroundFrictionForce(
            Vector3 velocity,
            Vector3 forward,
            Vector3 right,
            float normalLoad,
            float lateralCoefficient,
            float rollingCoefficient)
        {
            if (!float.IsFinite(normalLoad) || normalLoad <= 0f)
            {
                return Vector3.zero;
            }

            return (right * ResolveFrictionAlong(velocity, right, normalLoad, lateralCoefficient))
                + (forward * ResolveFrictionAlong(velocity, forward, normalLoad, rollingCoefficient));
        }

        /// <summary>Frottement le long d'un axe, borne par la charge portee et attenue pres de l'arret.</summary>
        private static float ResolveFrictionAlong(Vector3 velocity, Vector3 axis, float normalLoad, float coefficient)
        {
            if (!float.IsFinite(coefficient) || coefficient <= 0f)
            {
                return 0f;
            }

            var speed = Vector3.Dot(velocity, axis);
            if (Mathf.Abs(speed) <= 0.0001f)
            {
                return 0f;
            }

            var ramp = Mathf.Min(1f, Mathf.Abs(speed) / FrictionRampSpeed);
            return -Mathf.Sign(speed) * coefficient * normalLoad * ramp;
        }

        /// <summary>
        /// Forces anti-roulis d'un essieu, rendues en PAIRE pour que le sens ne puisse pas etre inverse
        /// par l'appelant. Le cote le plus COMPRIME est le cote ou la caisse s'affaisse : c'est lui
        /// qu'il faut CHARGER (pousser vers le haut), et l'autre qu'il faut soulager (pousser vers le
        /// bas). Le couple s'annule exactement quand l'essieu est a plat.
        ///
        /// Le terme est proportionnel a l'ECART de compression, sans plafond et sans seuil : c'est ce
        /// qui fait qu'il REDUIT le roulis sans l'annuler. A l'equilibre en virage, le couple de roulis
        /// reste contrebalance par un ecart non nul, donc la caisse s'incline toujours.
        ///
        /// Rendue en paire parce que le sens a deja ete inverse une fois : un scalaire signe se lit
        /// aussi bien dans les deux sens, une paire non.
        /// </summary>
        public static void ResolveAntiRollForces(
            float leftCompression,
            float rightCompression,
            float antiRollRate,
            out float leftForce,
            out float rightForce)
        {
            var magnitude = Mathf.Max(0f, antiRollRate) * (leftCompression - rightCompression);
            leftForce = magnitude;
            rightForce = -magnitude;
        }

        /// <summary>
        /// Echantillon de telemetrie de caisse : vitesse, composantes longitudinale et laterale,
        /// glissement et angle de derive. Aucune dependance a une scene ni a un <c>Rigidbody</c> :
        /// seule la vitesse et les axes du vehicule entrent.
        ///
        /// Le glissement est celui de la CAISSE (part de la vitesse qui part de travers), pas celui
        /// d'un pneu : les roues n'ont pas encore de vitesse de rotation propre. La Story 5.12, qui
        /// remplace les ecritures de vitesse par des efforts aux roues, apportera le glissement de
        /// pneu et c'est SON echantillon qui le portera.
        /// </summary>
        public static TelemetrySample SampleTelemetry(Vector3 velocity, Vector3 forward, Vector3 right)
        {
            var planarVelocity = Vector3.ProjectOnPlane(velocity, Vector3.up);
            var longitudinalSpeed = Vector3.Dot(planarVelocity, forward);
            var lateralSpeed = Vector3.Dot(planarVelocity, right);
            var speed = planarVelocity.magnitude;

            return new TelemetrySample(
                speed,
                longitudinalSpeed,
                lateralSpeed,
                speed > SlipSpeedEpsilon ? Mathf.Abs(lateralSpeed) / speed : 0f,
                Mathf.Atan2(lateralSpeed, longitudinalSpeed) * Mathf.Rad2Deg);
        }

        /// <summary>
        /// Couple de rappel d'assiette : ramene l'axe haut de la caisse vers la verticale monde.
        /// <c>cross(up, worldUp)</c> donne a la fois l'axe ET la magnitude de l'inclinaison, donc le
        /// couple disparait exactement quand la caisse est d'aplomb -- aucun seuil, aucune valeur de
        /// repli.
        ///
        /// Pourquoi ce terme existe alors que la suspension et l'anti-roulis suffisent tant que les
        /// roues touchent : des qu'une caisse s'incline d'une vingtaine de degres, les ancrages de roue
        /// montent au-dessus de la longueur au repos, les rayons ne touchent plus le sol, et la
        /// suspension cesse d'exister -- donc plus rien ne redresse, et le vehicule reste couche sur le
        /// flanc indefiniment. Un vehicule arcade (AD-7) revient sur ses roues.
        /// </summary>
        public static Vector3 ResolveLevellingTorque(Vector3 bodyUp, Vector3 worldUp, float rate)
        {
            if (!float.IsFinite(rate) || rate <= 0f)
            {
                return Vector3.zero;
            }

            if (bodyUp.sqrMagnitude < 0.000001f || worldUp.sqrMagnitude < 0.000001f)
            {
                return Vector3.zero;
            }

            return Vector3.Cross(bodyUp.normalized, worldUp.normalized) * rate;
        }

        /// <summary>
        /// Amortissement du TANGAGE et du ROULIS seulement : la vitesse angulaire est projetee sur les
        /// deux axes de la caisse et amortie separement. Le LACET n'est jamais touche -- il appartient a
        /// la direction (Story 5.12), et l'amortir retirerait au conducteur son autorite sur le cap.
        /// </summary>
        public static Vector3 ResolveAttitudeDampingTorque(Vector3 angularVelocity, Vector3 forward, Vector3 right, float damping)
        {
            if (!float.IsFinite(damping) || damping <= 0f)
            {
                return Vector3.zero;
            }

            var rollRate = Vector3.Dot(angularVelocity, forward);
            var pitchRate = Vector3.Dot(angularVelocity, right);
            return ((forward * rollRate) + (right * pitchRate)) * -damping;
        }

        /// <summary>
        /// Vrai quand le contact touche le vehicule A HAUTEUR DE SON DESSOUS : c'est un relief franchi
        /// -- trottoir, levre de dalle, bordure authoree -- et non un obstacle.
        ///
        /// Ce qui separe une dalle d'un mur n'est pas la vitesse de fermeture, comparable a vitesse de
        /// conduite, mais la hauteur a laquelle le contact touche la caisse : monter un relief touche le
        /// dessous, un mur touche la face, et une voiture qui en percute une autre touche son pare-chocs.
        /// Un contact purement vertical qui ne touche que le dessous ne doit donc produire aucun degat.
        /// </summary>
        public static bool IsSurfaceContact(float contactHeight, float vehicleUndersideHeight, float tolerance)
        {
            return contactHeight <= vehicleUndersideHeight + Mathf.Max(0f, tolerance);
        }

        /// <summary>Lecture de caisse publiee en lecture seule par <see cref="VehiclePhysicsBody"/>.</summary>
        public readonly struct TelemetrySample
        {
            public TelemetrySample(float speed, float longitudinalSpeed, float lateralSpeed, float slip, float slipAngleDegrees)
            {
                Speed = speed;
                LongitudinalSpeed = longitudinalSpeed;
                LateralSpeed = lateralSpeed;
                Slip = slip;
                SlipAngleDegrees = slipAngleDegrees;
            }

            /// <summary>Vitesse planaire (m/s).</summary>
            public float Speed { get; }

            /// <summary>Composante de vitesse dans l'axe avant (m/s), signee.</summary>
            public float LongitudinalSpeed { get; }

            /// <summary>Composante de vitesse dans l'axe droit (m/s), signee.</summary>
            public float LateralSpeed { get; }

            /// <summary>Part de la vitesse qui part de travers, dans [0, 1].</summary>
            public float Slip { get; }

            /// <summary>Angle entre le cap et la trajectoire (degres), signe.</summary>
            public float SlipAngleDegrees { get; }
        }

        /// <summary>Etat d'une roue pour une frame donnee, publie en lecture seule par <see cref="VehiclePhysicsBody"/>.</summary>
        public readonly struct WheelState
        {
            public WheelState(bool grounded, float compression, Vector3 contactPoint, Vector3 hubPosition)
            {
                Grounded = grounded;
                Compression = compression;
                ContactPoint = contactPoint;
                HubPosition = hubPosition;
            }

            /// <summary>Vrai quand la roue porte sur le sol pour cette frame.</summary>
            public bool Grounded { get; }

            /// <summary>Compression courante (m), dans <c>[0, travel]</c>.</summary>
            public float Compression { get; }

            /// <summary>Point de contact monde. Egal a l'ancrage quand la roue est en l'air.</summary>
            public Vector3 ContactPoint { get; }

            /// <summary>Position monde du moyeu, deduite du contact et du rayon authore.</summary>
            public Vector3 HubPosition { get; }
        }
    }
}
