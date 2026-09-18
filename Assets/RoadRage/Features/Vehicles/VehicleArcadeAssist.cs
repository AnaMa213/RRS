using UnityEngine;

namespace RoadRage.Features.Vehicles
{
    /// <summary>
    /// Story 5.13 : les AIDES ARCADE de la couche physique, en fonctions **pures**. Trois termes
    /// authores -- stabilite en lacet, controle de traction, recuperation de tete-a-queue -- et un
    /// facteur d'autorite derive du nombre de roues au sol.
    ///
    /// Pourquoi des fonctions pures : une aide ne se prouve pas pendant un pas de physique, son
    /// ARITHMETIQUE si. Tout ce qui decide -- quand le terme s'engage, de combien, et ou il s'arrete --
    /// se verifie en EditMode ; <see cref="VehiclePhysicsBody"/> ne fait que brancher ces sorties sur
    /// des <c>AddTorque</c> et des couples de roue (AD-35 : decision de testabilite, pas de fidelite).
    ///
    /// Trois regles valables pour TOUS les termes de ce fichier, et tenues par construction :
    ///
    /// 1. **Chaque aide est inerte a sa valeur authoree nulle**, sur le patron deja en place dans la
    ///    couche (<c>rate &lt;= 0 -&gt; Vector3.zero</c>). Aucune aide n'a de valeur de repli : une
    ///    valeur absente ou non finie rend le terme inerte, jamais un reglage invente.
    /// 2. **Aucun terme ne depasse le budget de charge porte.** Un terme non borne est un defaut, pas
    ///    un reglage : c'est cette regle qui manquait a l'anti-roulis de la Story 5.11, dont la
    ///    magnitude maximale (5 000 N) depassait la charge statique d'une roue (2 943 N).
    /// 3. **Aucune aide ne remet l'entree conducteur a zero.** Ces fonctions ne touchent jamais
    ///    l'intent : elles produisent un couple ou un facteur, et la neutralisation reste le fait de
    ///    l'entree (<see cref="VehicleDriveIntent"/>). C'est verifiable dans le controleur joueur :
    ///    seules ses deux sorties d'arret poussent l'intent neutre.
    ///
    /// Aucune dependance a <c>Time</c>, a un <c>Rigidbody</c> ou a une scene : le pas de temps et les
    /// grandeurs du vehicule sont des arguments.
    ///
    /// Une aide ne lit QUE <see cref="VehicleProfileDef"/> -- ni rage, ni peur, ni disposition
    /// (AD-33) : une conduite qui dependrait de l'humeur du conducteur serait un second chemin de
    /// verite pour la meme conduite, et la modulation appartient a la Story 5.19.
    /// </summary>
    public static class VehicleArcadeAssist
    {
        /// <summary>Bande morte des entrees : sous cette valeur, une entree est un bruit numerique, pas une demande.</summary>
        private const float InputEpsilon = 0.0001f;

        /// <summary>
        /// Plafond de la force du controle de traction, en fraction. Il est STRICTEMENT inferieur a 1
        /// par construction : a 1, une roue en glissement total perdrait tout son couple moteur, ce qui
        /// serait un seuil binaire deguise -- exactement ce que la couche interdit. La validation du
        /// profil refuse deja une valeur authoree superieure ou egale a 1 ; ce plafond est le garde-fou
        /// de la fonction pure, pour qu'un appelant ne puisse pas la contourner.
        /// </summary>
        private const float MaxTractionControlStrength = 0.99f;

        /// <summary>
        /// Nombre de roues au sol, ramene a un facteur d'autorite de conduite et de direction : quatre
        /// roues sur quatre valent 1, une seule vaut 0,25, aucune vaut 0. C'est une PROPORTION, pas un
        /// reglage -- l'AC dit « reduite dans la proportion du nombre de roues au sol », donc il n'y a
        /// pas de valeur authoree a chercher ici : un facteur authore aurait rendu la proportion fausse.
        ///
        /// <paramref name="wheelCount"/> nul ou negatif rend le facteur NEUTRE (1) plutot que NaN : un
        /// vehicule sans roue authoree n'a aucune autorite a perdre, et le profil refuse deja ce cas.
        ///
        /// Un vehicule en vol (facteur 0) ne recoit donc plus de couple moteur et ne bouge plus ses
        /// roues -- mais AUCUNE vitesse et AUCUNE rotation ne sont ecrites pour autant : la couche
        /// physique ne fait que reduire ce qu'elle produit.
        /// </summary>
        public static float ResolveGroundedAuthorityFactor(int groundedWheels, int wheelCount)
        {
            if (wheelCount <= 0)
            {
                return 1f;
            }

            if (groundedWheels <= 0)
            {
                return 0f;
            }

            if (groundedWheels >= wheelCount)
            {
                return 1f;
            }

            return (float)groundedWheels / wheelCount;
        }

        /// <summary>
        /// Budget de couple en LACET de la couche (N.m) : <c>adherence x charge statique d'une roue x
        /// demi-voie</c>, c'est-a-dire le couple qu'UN SEUL pneu peut produire autour de l'axe vertical
        /// a sa pleine adherence.
        ///
        /// C'est la borne de tous les termes de lacet de ce fichier, et elle est DERIVEE du vehicule
        /// (son adherence author ee, sa charge, sa voie) -- jamais un nombre absolu. Une aide qui
        /// depasserait ce couple ferait tourner la caisse plus fort qu'un pneu ne peut la retenir : a
        /// ce point ce n'est plus une aide, c'est un mouvement impose.
        ///
        /// Un budget nul ou non fini rend les termes qu'il borne INERTES plutot que non bornes : le
        /// sens sur du garde-fou est celui-la, jamais l'absence de borne.
        /// </summary>
        public static float ResolveYawTorqueBudget(float adherence, float staticLoad, float halfTrack)
        {
            if (!float.IsFinite(adherence) || adherence <= 0f
                || !float.IsFinite(staticLoad) || staticLoad <= 0f
                || !float.IsFinite(halfTrack) || halfTrack <= 0f)
            {
                return 0f;
            }

            return adherence * staticLoad * halfTrack;
        }

        /// <summary>
        /// Stabilite en lacet : amortit la composante de LACET de la vitesse angulaire, en s'opposant
        /// a elle, proportionnellement a son taux authore, et bornee par
        /// <see cref="ResolveYawTorqueBudget"/>.
        ///
        /// **Pourquoi ce terme manquait, et pourquoi l'amortissement d'assiette ne le remplace pas.**
        /// <see cref="VehicleSuspensionModel.ResolveAttitudeDampingTorque"/> projette la vitesse
        /// angulaire sur les axes TANGAGE et ROULIS seulement : le lacet n'y est jamais touche,
        /// puisqu'il porte la direction. Rien, donc, n'amortissait la rotation de la caisse autour de
        /// la verticale, et une caisse mise en lacet par un choc ou par la geometrie du pneu le gardait
        /// jusqu'a ce que les pneus le reprennent.
        ///
        /// **Pourquoi il ne neutralise jamais le cap demande.** Le terme est une fonction du seul taux
        /// de lacet : il est EXACTEMENT nul quand la caisse ne tourne pas, il s'oppose toujours au sens
        /// de la rotation en cours, et il n'a AUCUNE memoire d'un cap vise -- il ne peut donc pas tenir
        /// un cap contre le conducteur, seulement ralentir une rotation. Une demande conducteur qui
        /// produit un couple superieur au budget garde le dessus, et le regime permanent du virage
        /// s'etablit la ou le couple des pneus egale le couple d'amortissement : la caisse tourne
        /// toujours, moins vite.
        ///
        /// Un taux authore nul, une valeur non finie ou un taux de lacet non fini rendent le terme
        /// inerte.
        /// </summary>
        public static float ResolveYawStabilityTorque(
            float yawRateDegreesPerSecond,
            float yawStabilityRate,
            float yawTorqueBudget)
        {
            if (!float.IsFinite(yawStabilityRate) || yawStabilityRate <= 0f)
            {
                return 0f;
            }

            if (!float.IsFinite(yawRateDegreesPerSecond) || Mathf.Abs(yawRateDegreesPerSecond) <= InputEpsilon)
            {
                return 0f;
            }

            return ClampToBudget(-yawStabilityRate * yawRateDegreesPerSecond, yawTorqueBudget);
        }

        /// <summary>
        /// Controle de traction : facteur multiplicatif applique au couple moteur d'UNE roue, dans
        /// <c>]0, 1]</c>.
        ///
        /// L'attenuation est EXACTEMENT de 1 (aucune attenuation) tant que le glissement longitudinal
        /// de la roue reste sous le pic authore : sous le pic, le pneu transmet ce qu'on lui demande,
        /// et l'aide n'a rien a corriger. Au-dela, elle retire une part authoree du couple moteur, qui
        /// croit avec l'exces de glissement mais **ne peut jamais l'annuler** : la part retiree est
        /// bornee par la force authoree, strictement inferieure a 1, et le facteur tend donc vers
        /// <c>1 - force</c> sans jamais l'atteindre.
        ///
        /// Pourquoi un facteur et pas un couple : c'est le couple MOTEUR qui entretient un glissement
        /// excessif. Atténuer les forces de pneu serait traiter le symptome ; c'est la cause qu'on
        /// limite, et la roue continue de tourner, donc le glissement reste atteignable et recuperable.
        ///
        /// Une force authoree nulle, un glissement non fini ou un pic non positif rendent le facteur
        /// neutre (1) : aucune attenuation, comportement d'avant cette story.
        /// </summary>
        public static float ResolveTractionControlFactor(
            float slipRatio,
            float peakSlipRatio,
            float tractionControlStrength)
        {
            if (!float.IsFinite(tractionControlStrength) || tractionControlStrength <= 0f)
            {
                return 1f;
            }

            if (!float.IsFinite(slipRatio) || !float.IsFinite(peakSlipRatio) || peakSlipRatio <= 0f)
            {
                return 1f;
            }

            var normalized = Mathf.Abs(slipRatio) / peakSlipRatio;
            if (!float.IsFinite(normalized) || normalized <= 1f)
            {
                return 1f;
            }

            var strength = Mathf.Clamp(tractionControlStrength, 0f, MaxTractionControlStrength);
            var attenuation = strength * (normalized - 1f) / normalized;
            return Mathf.Clamp01(1f - attenuation);
        }

        /// <summary>
        /// Recuperation de tete-a-queue : couple de rappel, borne par
        /// <see cref="ResolveYawTorqueBudget"/>, dirige du cote demande par le conducteur et
        /// proportionnel a l'exces de derive, au-dela du seuil de derive authore.
        ///
        /// **Il ne remplace jamais l'entree** : le couple est proportionnel a l'entree de direction,
        /// donc il est exactement nul quand elle est nulle. Un conducteur qui lache le volant pendant
        /// une rotation ne se fait pas redresser par cette aide -- c'est la seule lecture compatible
        /// avec l'AC, et c'est aussi la seule qui ne cree pas un pilote automatique dans la couche
        /// physique.
        ///
        /// **Il ne se declenche pas sous le seuil authore** : une caisse qui tourne normalement dans un
        /// virage ne doit pas sentir de couple de recuperation. L'exces (derive moins seuil) est ce qui
        /// alimente le terme, donc une rotation sous le seuil n'y touche pas du tout.
        ///
        /// Un taux authore nul, un seuil non fini ou des entrees non finies rendent le terme inerte.
        /// </summary>
        public static float ResolveSpinRecoveryTorque(
            float yawRateDegreesPerSecond,
            float steerInput,
            float spinDriftThresholdDegreesPerSecond,
            float spinRecoveryRate,
            float yawTorqueBudget)
        {
            if (!float.IsFinite(spinRecoveryRate) || spinRecoveryRate <= 0f)
            {
                return 0f;
            }

            if (!float.IsFinite(yawRateDegreesPerSecond)
                || !float.IsFinite(steerInput)
                || !float.IsFinite(spinDriftThresholdDegreesPerSecond))
            {
                return 0f;
            }

            var drift = Mathf.Abs(yawRateDegreesPerSecond) - Mathf.Max(0f, spinDriftThresholdDegreesPerSecond);
            if (drift <= 0f)
            {
                return 0f;
            }

            var demand = Mathf.Clamp(steerInput, -1f, 1f);
            if (Mathf.Abs(demand) <= InputEpsilon)
            {
                return 0f;
            }

            return ClampToBudget(spinRecoveryRate * drift * demand, yawTorqueBudget);
        }

        /// <summary>
        /// Borne un couple au budget de lacet. Un budget nul ou non fini rend le terme INERTE : c'est
        /// le sens sur du garde-fou, puisqu'un terme non borne est precisement ce que cette story
        /// corrige ailleurs dans la couche.
        /// </summary>
        private static float ClampToBudget(float torque, float yawTorqueBudget)
        {
            if (!float.IsFinite(torque))
            {
                return 0f;
            }

            if (!float.IsFinite(yawTorqueBudget) || yawTorqueBudget <= 0f)
            {
                return 0f;
            }

            return Mathf.Clamp(torque, -yawTorqueBudget, yawTorqueBudget);
        }
    }
}
