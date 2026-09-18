using UnityEngine;

namespace RoadRage.Features.Vehicles
{
    /// <summary>
    /// Story 5.12 : le modele de pneu, en fonctions **pures**. C'est lui qui remplace le frottement de
    /// contact provisoire de la Story 5.11 (`VehicleSuspensionModel.ResolveGroundFrictionForce`), qui
    /// n'etait qu'un contact borne par la charge : sans glissement, il n'y avait aucune perte
    /// d'adherence progressive, donc ni derive controlee ni glissement.
    ///
    /// Pourquoi des fonctions pures : une roue ne se prouve pas hors d'un pas de physique, un pneu si.
    /// Toute la partie qui decide -- glissement, courbe de force, borne d'adherence -- se verifie en
    /// EditMode ; <see cref="VehiclePhysicsBody"/> ne fait que brancher ces sorties sur des raycasts
    /// et des <c>AddForceAtPosition</c> (AD-35 : decision de testabilite, pas de fidelite).
    ///
    /// Conventions :
    /// - une adherence est un coefficient sans unite ; la force maximale qu'une roue peut transmettre
    ///   vaut exactement <c>adherence x charge portee par CETTE roue</c> ;
    /// - une force de pneu est un scalaire dans l'axe considere (avant de la roue, ou droite de la
    ///   roue), signe comme l'axe ;
    /// - la perte d'adherence est une **courbe continue** : la force monte jusqu'a un pic authore,
    ///   puis decroit jusqu'a une asymptote authoree, jamais a zero et jamais a l'infini. Aucun seuil
    ///   binaire nulle part : c'est ce qui rend la derive atteignable ET recuperable.
    ///
    /// Le modele porte aussi le **train roulant** (couple moteur, couple de frein, integration de la
    /// vitesse de rotation de la roue) : c'est ce qui produit le glissement longitudinal que le pneu
    /// consomme, et le separement serait artificiel -- un pneu sans rotation de roue n'a pas de
    /// glissement longitudinal du tout.
    /// </summary>
    public static class VehicleTireModel
    {
        /// <summary>Vitesse de reference du glissement longitudinal : sert de denominateur plancher sous cette vitesse, et d'echelle a l'attenuation basse vitesse.</summary>
        public const float SlipReferenceSpeed = 0.5f;

        /// <summary>
        /// Bande de vitesse, en fraction de la pointe authoree, sur laquelle l'effort moteur s'eteint
        /// progressivement. La pointe est ainsi APPROCHEE par une pente de force, jamais atteinte par
        /// une ecriture de vitesse : c'est ce qui permet de garder
        /// <c>ResolveEffectiveMaxForwardSpeed()</c> (contrat de source de la Story 3.5) sans
        /// reintroduire d'ecriture.
        /// </summary>
        private const float DriveTorqueFalloffBand = 0.06f;

        /// <summary>Borne de rotation d'une roue (rad/s) : garde-fou de valeur finie pour une roue en l'air a plein gaz.</summary>
        private const float MaxWheelAngularVelocity = 400f;

        private const float InputEpsilon = 0.0001f;

        /// <summary>
        /// Glissement longitudinal d'une roue : ecart relatif entre la vitesse de sa bande de roulement
        /// et la vitesse du sol, rapporte a la plus grande des deux echelles (vitesse du sol, ou
        /// <see cref="SlipReferenceSpeed"/>). Le plancher evite le rapport sur une vitesse nulle, qui
        /// rendrait un glissement arbitraire a l'arret.
        /// </summary>
        public static float ResolveSlipRatio(float wheelSurfaceSpeed, float groundSpeed)
        {
            if (!float.IsFinite(wheelSurfaceSpeed) || !float.IsFinite(groundSpeed))
            {
                return 0f;
            }

            return (wheelSurfaceSpeed - groundSpeed) / Mathf.Max(Mathf.Abs(groundSpeed), SlipReferenceSpeed);
        }

        /// <summary>
        /// Angle de glissement d'une roue (degres) : angle entre le plan de la roue et sa trajectoire
        /// reelle, signe comme l'axe droit de la roue. Le denominateur prend la composante
        /// longitudinale en valeur absolue, donc la convention ne s'inverse pas en marche arriere.
        ///
        /// Cet angle se calcule des que la roue a une vitesse, aussi faible soit-elle : contrairement
        /// au glissement longitudinal, il n'a pas besoin de plancher. C'est l'ATTENUATION de la force
        /// (<see cref="ResolveLowSpeedRamp"/>) qui evite le broutement a l'arret, et non un angle
        /// fausse -- c'est la meme raison que la rampe de 0,5 m/s de la Story 5.11.
        /// </summary>
        public static float ResolveSlipAngleDegrees(float longitudinalSpeed, float lateralSpeed)
        {
            if (!float.IsFinite(longitudinalSpeed) || !float.IsFinite(lateralSpeed))
            {
                return 0f;
            }

            return Mathf.Atan2(-lateralSpeed, Mathf.Abs(longitudinalSpeed)) * Mathf.Rad2Deg;
        }

        /// <summary>
        /// Part de l'adherence disponible effectivement transmise pour un glissement donne, dans
        /// <c>[0, 1]</c> : montee lineaire jusqu'au pic (<paramref name="peakSlip"/>), puis decroissance
        /// monotone jusqu'a <paramref name="falloffFraction"/>.
        ///
        /// Les deux proprietes qui comptent sont tenues par construction :
        /// - la valeur vaut exactement 1 au pic, donc la force maximale transmise est exactement
        ///   <c>adherence x charge</c> -- jamais plus ;
        /// - au-dela du pic la courbe tend vers <paramref name="falloffFraction"/> (fraction authoree,
        ///   strictement positive), donc la force ne tombe **jamais a zero** : c'est ce qui rend la
        ///   derive atteignable ET recuperable, et non un interrupteur.
        ///
        /// Aucun seuil, aucune discontinuite : la pente change au pic, la valeur non.
        /// </summary>
        public static float ResolveGripFraction(float slipMagnitude, float peakSlip, float falloffFraction)
        {
            if (!float.IsFinite(slipMagnitude) || !float.IsFinite(peakSlip) || peakSlip <= 0f || !float.IsFinite(falloffFraction))
            {
                return 0f;
            }

            var magnitude = Mathf.Abs(slipMagnitude);
            if (magnitude <= 0f)
            {
                return 0f;
            }

            var normalized = magnitude / peakSlip;
            if (normalized <= 1f)
            {
                return normalized;
            }

            var asymptote = Mathf.Clamp(falloffFraction, 0f, 0.999f);
            return asymptote + ((1f - asymptote) / normalized);
        }

        /// <summary>
        /// Force de pneu dans le repere de la roue : <c>x</c> le long de l'axe avant de la roue,
        /// <c>y</c> le long de son axe droit. C'est LE point ou le budget d'adherence est reparti.
        ///
        /// Un pneu n'a qu'UN seul budget : <c>adherence x charge portee</c>. Les deux glissements sont
        /// donc normalises par leur propre pic authore, puis reunis en une seule magnitude -- celle de
        /// leur somme vectorielle -- dont la courbe de force donne la part d'adherence utilisee, et
        /// dont la DIRECTION donne le sens de la force. Consequences, toutes voulues :
        ///
        /// - le pic est atteint exactement quand la magnitude normalisee vaut 1, et la force vaut alors
        ///   exactement <c>adherence x charge</c> : elle ne le depasse jamais ;
        /// - freiner ET tourner partage le meme budget, sans qu'aucun des deux axes soit privilegie :
        ///   c'est le sous-virage au freinage, et c'est aussi ce qui fait qu'une roue BLOQUEE perd
        ///   l'essentiel de son adherence laterale -- le glissement longitudinal sature prend toute la
        ///   magnitude, donc la force s'aligne sur l'axe de la roue. C'est le mecanisme de la derive au
        ///   frein a main : un pneu bloque ne tient plus la caisse en travers ;
        /// - la force ne tombe jamais a zero (asymptote authoree) : la derive reste atteignable ET
        ///   recuperable, sans seuil binaire nulle part.
        /// </summary>
        public static Vector2 ResolveTireForces(
            float slipRatio,
            float peakSlipRatio,
            float slipAngleDegrees,
            float peakSlipAngleDegrees,
            float falloffFraction,
            float adherence,
            float normalLoad)
        {
            if (!float.IsFinite(adherence) || adherence <= 0f || !float.IsFinite(normalLoad) || normalLoad <= 0f)
            {
                return Vector2.zero;
            }

            if (!float.IsFinite(slipRatio) || !float.IsFinite(slipAngleDegrees)
                || !float.IsFinite(peakSlipRatio) || peakSlipRatio <= 0f
                || !float.IsFinite(peakSlipAngleDegrees) || peakSlipAngleDegrees <= 0f)
            {
                return Vector2.zero;
            }

            // Glissements normalises par leur pic respectif : les deux echelles deviennent comparables,
            // ce qui est la condition pour qu'ils partagent un budget commun.
            var normalized = new Vector2(slipRatio / peakSlipRatio, slipAngleDegrees / peakSlipAngleDegrees);
            var magnitude = normalized.magnitude;
            if (magnitude <= 0.000001f)
            {
                return Vector2.zero;
            }

            var grip = ResolveGripFraction(magnitude, 1f, falloffFraction);
            return (normalized / magnitude) * (adherence * normalLoad * grip);
        }

        /// <summary>
        /// Attenuation de la force de contact sous <see cref="SlipReferenceSpeed"/> : la force croit
        /// avec la vitesse au lieu de rester pleine a l'arret. Sans elle, un vehicule pose sur ses
        /// roues broute et glisse au lieu de s'immobiliser -- mesure de la Story 5.11, conservee ici
        /// sous une forme equivalente parce que la cause est la meme.
        /// </summary>
        public static float ResolveLowSpeedRamp(float speed)
        {
            if (!float.IsFinite(speed))
            {
                return 0f;
            }

            return Mathf.Clamp01(Mathf.Abs(speed) / SlipReferenceSpeed);
        }

        /// <summary>
        /// Part de l'effort moteur disponible a une vitesse donnee : pleine jusqu'a
        /// <c>maxForwardSpeed x (1 - bande)</c>, puis decroissante jusqu'a zero exactement a la pointe
        /// authoree. C'est la pente qui approche la pointe, jamais une ecriture de vitesse.
        /// </summary>
        public static float ResolveDriveTorqueFactor(float speed, float maxForwardSpeed)
        {
            if (!float.IsFinite(speed) || !float.IsFinite(maxForwardSpeed) || maxForwardSpeed <= 0f)
            {
                return 0f;
            }

            var band = Mathf.Max(maxForwardSpeed * DriveTorqueFalloffBand, 0.0001f);
            return Mathf.Clamp01((maxForwardSpeed - speed) / band);
        }

        /// <summary>
        /// Couple moteur a appliquer a une roue MOTRICE : plein gaz positif, marche arriere negative,
        /// et rien du tout quand l'entree de frein sert a freiner (vitesse au-dessus du seuil de
        /// changement de sens). L'effort s'eteint progressivement a l'approche de la pointe authoree.
        ///
        /// Aucun couple n'est applique a une roue non motrice : c'est l'appelant qui filtre sur
        /// <see cref="VehicleWheel.IsDriven"/>, et cette fonction ne connait pas le vehicule.
        /// </summary>
        public static float ResolveWheelDriveTorque(
            float throttle,
            float brakeReverse,
            float longitudinalSpeed,
            float minimumDirectionSpeed,
            float engineTorque,
            float reverseTorque,
            float maxForwardSpeed,
            float maxReverseSpeed)
        {
            if (!float.IsFinite(longitudinalSpeed) || !float.IsFinite(minimumDirectionSpeed))
            {
                return 0f;
            }

            if (brakeReverse > InputEpsilon)
            {
                // La marche arriere ne s'engage que sous le seuil de vitesse : au-dessus, l'entree de
                // frein freine et ne fait rien d'autre. Son effort s'eteint lui aussi a l'approche de
                // sa propre pointe authoree, en prenant la vitesse en valeur absolue (elle est
                // negative quand le vehicule recule).
                return longitudinalSpeed > minimumDirectionSpeed
                    ? 0f
                    : -Mathf.Clamp01(brakeReverse)
                        * Mathf.Max(0f, reverseTorque)
                        * ResolveDriveTorqueFactor(-longitudinalSpeed, maxReverseSpeed);
            }

            if (throttle <= InputEpsilon)
            {
                return 0f;
            }

            return Mathf.Clamp01(throttle) * Mathf.Max(0f, engineTorque) * ResolveDriveTorqueFactor(longitudinalSpeed, maxForwardSpeed);
        }

        /// <summary>
        /// Couple de frein a appliquer a une roue : frein de service quand on freine au-dessus du
        /// seuil de changement de sens, frein moteur (authore, volontairement faible) quand aucune
        /// entree n'est donnee, rien quand on accelere.
        ///
        /// Le frein moteur permanent quand rien n'est demande est ce qui retient un vehicule gare sans
        /// conducteur : la couche physique simule des roues, et une roue libre n'oppose rien au
        /// roulement.
        /// </summary>
        public static float ResolveWheelBrakeTorque(
            float throttle,
            float brakeReverse,
            float longitudinalSpeed,
            float minimumDirectionSpeed,
            float brakeTorque,
            float coastTorque)
        {
            if (!float.IsFinite(longitudinalSpeed) || !float.IsFinite(minimumDirectionSpeed))
            {
                return 0f;
            }

            if (brakeReverse > InputEpsilon)
            {
                return longitudinalSpeed > minimumDirectionSpeed
                    ? Mathf.Clamp01(brakeReverse) * Mathf.Max(0f, brakeTorque)
                    : 0f;
            }

            return throttle > InputEpsilon ? 0f : Mathf.Max(0f, coastTorque);
        }

        /// <summary>
        /// Integre la vitesse de rotation d'une roue sur un pas : couple moteur, reaction du pneu
        /// (opposee a la force qu'il transmet au sol) et couple de frein, qui ne peut que RAMENER la
        /// rotation a zero -- un frein ne fait pas tourner une roue a l'envers.
        ///
        /// La rotation est bornee : une roue en l'air a plein gaz accelere sans fin autrement, et le
        /// glissement qu'elle produirait a l'atterrissage deviendrait une valeur non finie.
        /// </summary>
        public static float IntegrateWheelAngularVelocity(
            float angularVelocity,
            float driveTorque,
            float brakeTorque,
            float tireForce,
            float wheelRadius,
            float wheelInertia,
            float deltaTime)
        {
            if (!float.IsFinite(angularVelocity) || !float.IsFinite(deltaTime)
                || !float.IsFinite(wheelInertia) || wheelInertia <= 0f
                || !float.IsFinite(wheelRadius) || wheelRadius <= 0f
                || deltaTime <= 0f)
            {
                return float.IsFinite(angularVelocity) ? angularVelocity : 0f;
            }

            var drive = float.IsFinite(driveTorque) ? driveTorque : 0f;
            var reaction = float.IsFinite(tireForce) ? tireForce * wheelRadius : 0f;
            var next = angularVelocity + ((drive - reaction) / wheelInertia * deltaTime);

            var brake = float.IsFinite(brakeTorque) ? Mathf.Max(0f, brakeTorque) : 0f;
            if (brake > 0f)
            {
                var brakeDelta = brake / wheelInertia * deltaTime;
                next = Mathf.Abs(next) <= brakeDelta ? 0f : next - (Mathf.Sign(next) * brakeDelta);
            }

            return Mathf.Clamp(next, -MaxWheelAngularVelocity, MaxWheelAngularVelocity);
        }

        /// <summary>
        /// Echantillon de pneu d'une roue pour une frame donnee, publie en lecture seule par
        /// <see cref="VehiclePhysicsBody"/>. C'est l'unite de lecture de la telemetrie et des
        /// controles humains de verification : glissement, force transmise et adherence disponible.
        /// </summary>
        public static TireSample SampleTire(
            bool grounded,
            float normalLoad,
            float slipRatio,
            float slipAngleDegrees,
            float forceMagnitude,
            float adherence)
        {
            var load = float.IsFinite(normalLoad) ? Mathf.Max(0f, normalLoad) : 0f;
            var grip = float.IsFinite(adherence) ? Mathf.Max(0f, adherence) : 0f;

            return new TireSample(
                grounded,
                load,
                slipRatio,
                slipAngleDegrees,
                grounded ? load * grip : 0f,
                forceMagnitude,
                grip);
        }

        /// <summary>Lecture de pneu publiee en lecture seule par <see cref="VehiclePhysicsBody"/>.</summary>
        public readonly struct TireSample
        {
            public TireSample(
                bool grounded,
                float normalLoad,
                float slipRatio,
                float slipAngleDegrees,
                float maximumForce,
                float forceMagnitude,
                float adherence)
            {
                Grounded = grounded;
                NormalLoad = normalLoad;
                SlipRatio = slipRatio;
                SlipAngleDegrees = slipAngleDegrees;
                MaximumForce = maximumForce;
                ForceMagnitude = forceMagnitude;
                Adherence = adherence;
            }

            /// <summary>Vrai quand la roue portait sur le sol pour cette frame.</summary>
            public bool Grounded { get; }

            /// <summary>Charge normale effectivement portee par cette roue (N). Zero en l'air.</summary>
            public float NormalLoad { get; }

            /// <summary>Glissement longitudinal signe (sans unite).</summary>
            public float SlipRatio { get; }

            /// <summary>Angle de glissement (degres), signe.</summary>
            public float SlipAngleDegrees { get; }

            /// <summary>Adherence disponible pour cette roue (N) : <c>adherence x charge</c>. Aucune force de pneu ne la depasse.</summary>
            public float MaximumForce { get; }

            /// <summary>Force de pneu effectivement transmise (N), les deux axes confondus.</summary>
            public float ForceMagnitude { get; }

            /// <summary>Coefficient d'adherence applique a cette roue, lu dans le profil authore.</summary>
            public float Adherence { get; }

            /// <summary>Part de l'adherence effectivement utilisee, dans <c>[0, 1]</c>.</summary>
            public float GripUsage
            {
                get { return MaximumForce > 0.0001f ? Mathf.Clamp01(ForceMagnitude / MaximumForce) : 0f; }
            }
        }
    }
}
