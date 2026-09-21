using UnityEngine;

namespace RoadRage.Features.Vehicles
{
    /// <summary>
    /// Story 5.12 : le modele de direction, en fonctions **pures**. La direction n'est plus un lacet
    /// impose a la caisse (`Rigidbody.MoveRotation`) : c'est un **angle de roue** que la couche
    /// physique donne aux roues directrices, et ce sont les efforts lateraux des pneus qui font
    /// tourner le vehicule.
    ///
    /// Deux consequences de ce changement, portees ici :
    /// - l'angle de roue **diminue avec la vitesse** : a vitesse elevee, un plein braquage n'est plus
    ///   lisible (ni tenable) -- la consigne est reduite progressivement, sans seuil ;
    /// - le retour au centre est **progressif** : relacher la direction ne remet pas la roue droite
    ///   dans la frame, elle y revient a un taux authore.
    ///
    /// Pourquoi des fonctions pures : la consigne d'angle et le taux de rappel se prouvent sans scene
    /// ni <c>Rigidbody</c>. L'application (qui roue braque, et l'integration de la consigne) vit dans
    /// <see cref="VehiclePhysicsBody"/>.
    /// </summary>
    public static class VehicleSteeringModel
    {
        private const float InputEpsilon = 0.0001f;

        /// <summary>
        /// Consigne d'angle de roue (degres) pour une entree de direction donnee : l'angle maximal
        /// authore a l'arret et a basse vitesse, reduit lineairement jusqu'a
        /// <paramref name="highSpeedSteerAngleDegrees"/> atteint a
        /// <paramref name="fullReductionSpeed"/>.
        ///
        /// Sous <paramref name="minimumDirectionSpeed"/>, la consigne est exactement zero : un vehicule
        /// quasi immobile ne braque pas ses roues. Sans ce plancher, la consigne existerait sans que le
        /// pneu transmette quoi que ce soit (l'attenuation basse vitesse annule la force laterale), donc
        /// l'angle resterait en memoire et se libererait d'un coup au premier metre parcouru.
        ///
        /// Le signe de <paramref name="steerInput"/> est porte tel quel : c'est l'appelant qui applique
        /// l'inversion marche arriere.
        /// </summary>
        public static float ResolveSteerAngleDegrees(
            float steerInput,
            float speed,
            float minimumDirectionSpeed,
            float maxSteerAngleDegrees,
            float highSpeedSteerAngleDegrees,
            float fullReductionSpeed)
        {
            if (!float.IsFinite(steerInput) || !float.IsFinite(speed)
                || !float.IsFinite(minimumDirectionSpeed) || !float.IsFinite(fullReductionSpeed)
                || !float.IsFinite(maxSteerAngleDegrees) || !float.IsFinite(highSpeedSteerAngleDegrees))
            {
                return 0f;
            }

            if (Mathf.Abs(steerInput) <= InputEpsilon || Mathf.Abs(speed) < minimumDirectionSpeed)
            {
                return 0f;
            }

            var low = Mathf.Clamp(maxSteerAngleDegrees, 0f, 90f);
            var high = Mathf.Clamp(highSpeedSteerAngleDegrees, 0f, low);
            var t = fullReductionSpeed <= 0f ? 1f : Mathf.Clamp01(Mathf.Abs(speed) / fullReductionSpeed);

            return Mathf.Clamp(steerInput, -1f, 1f) * Mathf.Lerp(low, high, t);
        }

        /// <summary>
        /// Vitesse maximale a laquelle ce vehicule peut TENIR un rayon donne -- la reciproque exacte de
        /// <see cref="ResolveSteerAngleDegrees"/>, rien de plus.
        ///
        /// Le braquage disponible decroit avec la vitesse. Un rayon exige un braquage
        /// <c>atan(empattement / rayon)</c> (modele bicyclette). La vitesse tenable est donc celle ou
        /// le braquage disponible cesse de couvrir le braquage exige. Aucune constante nouvelle :
        /// empattement, angle maximal, angle a haute vitesse et vitesse de reduction sont tous
        /// authores sur le vehicule, donc un carrefour plus large ou un vehicule plus maniable donnent
        /// d'eux-memes une autre reponse.
        ///
        /// Mesure ANO-5.18-04 : le connecteur de virage serre du district demande 4,25 m ; a 8,0 m/s le
        /// braquage disponible ne permet que 4,85 m. Le vehicule ne pouvait donc pas suivre sa propre
        /// trajectoire, et sortait 1,84 m a cote -- 0,6 m au-dela de l'axe de la voie opposee.
        /// </summary>
        public static float ResolveCurveSpeedLimit(
            float radius,
            float wheelbase,
            float maxSteerAngleDegrees,
            float highSpeedSteerAngleDegrees,
            float fullReductionSpeed)
        {
            if (!float.IsFinite(radius) || radius <= 0f || !float.IsFinite(wheelbase) || wheelbase <= 0f)
            {
                return float.PositiveInfinity;
            }

            var low = Mathf.Clamp(maxSteerAngleDegrees, 0f, 90f);
            var high = Mathf.Clamp(highSpeedSteerAngleDegrees, 0f, low);
            var required = Mathf.Atan(wheelbase / radius) * Mathf.Rad2Deg;

            // Le braquage tient a toute vitesse : la courbe n'impose rien.
            if (required <= high) return float.PositiveInfinity;

            // Le braquage ne suffit a aucune vitesse : c'est une geometrie infaisable, pas un reglage.
            // Rendre zero immobiliserait le vehicule ; la reponse honnete est "au plus lentement".
            if (required >= low || !float.IsFinite(fullReductionSpeed) || fullReductionSpeed <= 0f) return 0f;

            return fullReductionSpeed * (low - required) / Mathf.Max(0.0001f, low - high);
        }

        /// <summary>
        /// Taux de deplacement de la consigne, en degres par seconde : le taux de braquage authore
        /// quand l'angle s'eloigne du centre, le taux de rappel authore quand il y revient.
        ///
        /// Le rappel a son propre taux parce qu'il ne se conduit pas comme un braquage : un retour au
        /// centre trop lent se lit comme une roue bloquee en virage, un retour trop brutal comme une
        /// remise sur rails. Aucun des deux n'est instantane.
        /// </summary>
        public static float ResolveSteerRateDegreesPerSecond(
            float currentAngleDegrees,
            float targetAngleDegrees,
            float steerRateDegreesPerSecond,
            float returnRateDegreesPerSecond)
        {
            if (!float.IsFinite(currentAngleDegrees) || !float.IsFinite(targetAngleDegrees))
            {
                return 0f;
            }

            var returning = Mathf.Abs(targetAngleDegrees) < Mathf.Abs(currentAngleDegrees);
            return Mathf.Max(0f, returning ? returnRateDegreesPerSecond : steerRateDegreesPerSecond);
        }

        /// <summary>
        /// Angle effectif d'une roue : c'est ici que <see cref="VehicleWheel.IsSteering"/> est enfin
        /// consomme. Les roues directrices prennent la consigne, les autres restent droites -- il n'y a
        /// pas de braquage arriere sur ce vehicule, et en inventer un serait un reglage sans porteur.
        /// </summary>
        public static float ResolveWheelSteerAngleDegrees(bool isSteering, float axleAngleDegrees)
        {
            if (!float.IsFinite(axleAngleDegrees))
            {
                return 0f;
            }

            return isSteering ? axleAngleDegrees : 0f;
        }

        /// <summary>
        /// Avance une consigne d'angle vers sa cible a taux borne. C'est le seul endroit ou l'angle se
        /// deplace : ni saut, ni valeur posee. La cible peut etre de l'autre cote du zero (relachement
        /// en virage) -- <c>MoveTowards</c> traverse alors le centre a la meme vitesse, ce qui est bien
        /// le comportement attendu d'une roue qui revient.
        /// </summary>
        public static float MoveSteerAngleDegrees(
            float currentAngleDegrees,
            float targetAngleDegrees,
            float rateDegreesPerSecond,
            float deltaTime)
        {
            if (!float.IsFinite(currentAngleDegrees) || !float.IsFinite(targetAngleDegrees)
                || !float.IsFinite(deltaTime) || deltaTime <= 0f)
            {
                return float.IsFinite(currentAngleDegrees) ? currentAngleDegrees : 0f;
            }

            if (!float.IsFinite(rateDegreesPerSecond) || rateDegreesPerSecond <= 0f)
            {
                return currentAngleDegrees;
            }

            return Mathf.MoveTowards(currentAngleDegrees, targetAngleDegrees, rateDegreesPerSecond * deltaTime);
        }
    }
}
