using UnityEngine;

namespace RoadRage.Features.Vehicles
{
    /// <summary>
    /// Retour visuel client des degats vehicule : lit les flags synchronises du NetworkedVehicleState
    /// et pilote uniquement des effets cosmetiques.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NetworkedVehicleState))]
    public sealed class NetworkedVehicleDamageVfxController : MonoBehaviour
    {
        [SerializeField]
        private ParticleSystem engineSmokeEffect;

        [SerializeField]
        private ParticleSystem brakeSparkEffect;

        [SerializeField]
        private Transform wheelWobbleRoot;

        [SerializeField]
        private Vector3 engineSmokeLocalPosition = new Vector3(0f, 1.15f, 1.7f);

        [SerializeField]
        private Vector3 brakeSparkLocalPosition = new Vector3(0f, 0.35f, -1.55f);

        [SerializeField]
        [Min(0f)]
        private float wheelWobbleDegrees = 2.5f;

        [SerializeField]
        [Min(0f)]
        private float wheelWobbleFrequency = 9f;

        private NetworkedVehicleState state;
        private Quaternion wheelWobbleBaseLocalRotation;
        private bool wheelWobbleBaseCaptured;

        private void Awake()
        {
            CacheComponents();
            EnsureDefaultEffectInstances();
            CaptureWheelWobbleBaseRotation();
            RefreshDamageEffects();
        }

        private void OnEnable()
        {
            CacheComponents();
            EnsureDefaultEffectInstances();
            CaptureWheelWobbleBaseRotation();
            SubscribeToDamageFlags();
            RefreshDamageEffects();
        }

        private void OnDisable()
        {
            UnsubscribeFromDamageFlags();
            SetParticleEffectActive(engineSmokeEffect, false);
            SetParticleEffectActive(brakeSparkEffect, false);
            RestoreWheelWobbleRotation();
        }

        private void Update()
        {
            if (state == null || wheelWobbleRoot == null || !state.WheelDamaged.Value)
            {
                return;
            }

            var wobble = Mathf.Sin(Time.time * wheelWobbleFrequency) * wheelWobbleDegrees;
            wheelWobbleRoot.localRotation = wheelWobbleBaseLocalRotation * Quaternion.Euler(0f, 0f, wobble);
        }

        public void RefreshDamageEffects()
        {
            if (state == null)
            {
                return;
            }

            SetParticleEffectActive(engineSmokeEffect, state.EngineDamaged.Value);
            SetParticleEffectActive(brakeSparkEffect, state.BrakeDamaged.Value);

            if (!state.WheelDamaged.Value)
            {
                RestoreWheelWobbleRotation();
            }
        }

        private void SubscribeToDamageFlags()
        {
            if (state == null)
            {
                return;
            }

            state.WheelDamaged.OnValueChanged += HandleWheelDamagedChanged;
            state.EngineDamaged.OnValueChanged += HandleEngineDamagedChanged;
            state.BrakeDamaged.OnValueChanged += HandleBrakeDamagedChanged;
        }

        private void UnsubscribeFromDamageFlags()
        {
            if (state == null)
            {
                return;
            }

            state.WheelDamaged.OnValueChanged -= HandleWheelDamagedChanged;
            state.EngineDamaged.OnValueChanged -= HandleEngineDamagedChanged;
            state.BrakeDamaged.OnValueChanged -= HandleBrakeDamagedChanged;
        }

        private void HandleWheelDamagedChanged(bool previousValue, bool newValue)
        {
            if (!newValue)
            {
                RestoreWheelWobbleRotation();
            }
        }

        private void HandleEngineDamagedChanged(bool previousValue, bool newValue)
        {
            SetParticleEffectActive(engineSmokeEffect, newValue);
        }

        private void HandleBrakeDamagedChanged(bool previousValue, bool newValue)
        {
            SetParticleEffectActive(brakeSparkEffect, newValue);
        }

        private void CacheComponents()
        {
            if (state == null)
            {
                state = GetComponent<NetworkedVehicleState>();
            }

            if (wheelWobbleRoot == null)
            {
                wheelWobbleRoot = ResolveDefaultVisualRoot();
            }
        }

        private Transform ResolveDefaultVisualRoot()
        {
            var directVisual = transform.Find("Visual_Greybox_PlayerCar");
            if (directVisual != null)
            {
                return directVisual;
            }

            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (child != null && child.GetComponentInChildren<Renderer>(true) != null)
                {
                    return child;
                }
            }

            return null;
        }

        private void EnsureDefaultEffectInstances()
        {
            if (engineSmokeEffect == null)
            {
                engineSmokeEffect = CreateDefaultEffect("Damage_EngineSmoke", engineSmokeLocalPosition, new Color(0.35f, 0.35f, 0.35f, 0.85f), 14f, 1.4f, 0.55f);
            }

            if (brakeSparkEffect == null)
            {
                brakeSparkEffect = CreateDefaultEffect("Damage_BrakeSparks", brakeSparkLocalPosition, new Color(1f, 0.64f, 0.18f, 1f), 24f, 0.28f, 0.12f);
            }
        }

        private ParticleSystem CreateDefaultEffect(string effectName, Vector3 localPosition, Color startColor, float rateOverTime, float startLifetime, float startSize)
        {
            var effectObject = new GameObject(effectName);
            effectObject.transform.SetParent(transform, false);
            effectObject.transform.localPosition = localPosition;
            effectObject.transform.localRotation = Quaternion.identity;

            var particles = effectObject.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.loop = true;
            main.startLifetime = startLifetime;
            main.startSpeed = 0.65f;
            main.startSize = startSize;
            main.startColor = startColor;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = particles.emission;
            emission.enabled = false;
            emission.rateOverTime = rateOverTime;

            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 22f;
            shape.radius = 0.12f;

            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            return particles;
        }

        private void CaptureWheelWobbleBaseRotation()
        {
            if (wheelWobbleRoot == null || wheelWobbleBaseCaptured)
            {
                return;
            }

            wheelWobbleBaseLocalRotation = wheelWobbleRoot.localRotation;
            wheelWobbleBaseCaptured = true;
        }

        private void RestoreWheelWobbleRotation()
        {
            if (wheelWobbleRoot != null && wheelWobbleBaseCaptured)
            {
                wheelWobbleRoot.localRotation = wheelWobbleBaseLocalRotation;
            }
        }

        private static void SetParticleEffectActive(ParticleSystem particles, bool active)
        {
            if (particles == null)
            {
                return;
            }

            var emission = particles.emission;
            emission.enabled = active;

            if (active)
            {
                if (!particles.isPlaying)
                {
                    particles.Play(true);
                }

                return;
            }

            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}
