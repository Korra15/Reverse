using System.Collections;
using UnityEngine;

namespace Weather
{
    public class WeatherEffects : MonoBehaviour
    {
        [SerializeField] private ParticleSystem weatherParticles;
        [SerializeField] private float weatherTransitionTime = 3.0f;

        //particle system components
        private ParticleSystemRenderer weatherParticleRenderer;

        private EventBinding<WeatherChanged> weatherChangedEventBinding;
        private WeatherParameters currentEffectParameters;
        private WeatherParameters targetEffectParameters;

        //event binding
        private void OnEnable()
        {
            weatherChangedEventBinding = new EventBinding<WeatherChanged>((weatherChanged) =>
            {
                //If the swap was in the middle of a transition, skip coroutine and set initial values
                StopAllCoroutines();
                currentEffectParameters = targetEffectParameters;
                UpdateParticleSystem(currentEffectParameters);

                StartCoroutine(SetWeatherEffect(weatherChanged.WeatherParameters));
            });

            EventBus<WeatherChanged>.Register(weatherChangedEventBinding);
        }

        private void OnDisable() => EventBus<WeatherChanged>.Deregister(weatherChangedEventBinding);

        private void Awake()
        {
            weatherParticleRenderer = weatherParticles.GetComponent<ParticleSystemRenderer>();
            targetEffectParameters = ScriptableObject.CreateInstance<WeatherParameters>();
        }

        /// <summary>
        /// Sets a weather effect with new parameters, will transition to new effect
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private IEnumerator SetWeatherEffect(WeatherParameters parameters)
        {
            targetEffectParameters = parameters;
            float elapsedTime = 0f;

            WeatherParameters startParameters = currentEffectParameters;

            while (elapsedTime < weatherTransitionTime)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / weatherTransitionTime);

                currentEffectParameters = LerpWeatherEffectParameters(startParameters, targetEffectParameters, t);
                UpdateParticleSystem(currentEffectParameters);
                yield return null;
            }

            currentEffectParameters = targetEffectParameters;
            UpdateParticleSystem(currentEffectParameters);
        }

        /// <summary>
        /// Lerps 2 parameter sets together to enable a gradual transition between effects
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        private WeatherParameters LerpWeatherEffectParameters(WeatherParameters from, WeatherParameters to, float t)
        {
            WeatherParameters result = ScriptableObject.CreateInstance<WeatherParameters>();
            result.maxParticles = Mathf.Lerp(from.maxParticles, to.maxParticles, t);
            result.emissionRate = Mathf.Lerp(from.emissionRate, to.emissionRate, t);
            result.gravityModifier = Mathf.Lerp(from.gravityModifier, to.gravityModifier, t);
            result.startLifetime = Mathf.Lerp(from.startLifetime, to.startLifetime, t);
            result.endLifetime = Mathf.Lerp(from.endLifetime, to.endLifetime, t);
            result.startVelocityLifetime = Vector3.Lerp(from.startVelocityLifetime, to.startVelocityLifetime, t);
            result.endVelocityLifetime = Vector3.Lerp(from.endVelocityLifetime, to.endVelocityLifetime, t);
            result.lengthScale = Mathf.Lerp(from.lengthScale, to.lengthScale, t);
            result.speedScale = Mathf.Lerp(from.speedScale, to.speedScale, t);
            result.dampen = Mathf.Lerp(from.dampen, to.dampen, t);
            result.startColor = Color.Lerp(from.startColor, to.startColor, t);
            result.endColor = Color.Lerp(from.endColor, to.endColor, t);
            return result;
        }

        /// <summary>
        /// Updates the values of the particle system to the parameters given
        /// </summary>
        /// <param name="parameters"></param>
        private void UpdateParticleSystem(WeatherParameters parameters)
        {
            ParticleSystem.MainModule main = weatherParticles.main;
            ParticleSystem.EmissionModule emission = weatherParticles.emission;
            ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = weatherParticles.velocityOverLifetime;
            ParticleSystem.CollisionModule collisionModule = weatherParticles.collision;


            main.maxParticles = (int)parameters.maxParticles;
            main.gravityModifier = parameters.gravityModifier;
            main.startLifetime = new ParticleSystem.MinMaxCurve(parameters.startLifetime, parameters.endLifetime);
            main.startColor = new ParticleSystem.MinMaxGradient(parameters.startColor, parameters.endColor);

            velocityOverLifetime.enabled = true;
            velocityOverLifetime.x =
                new ParticleSystem.MinMaxCurve(parameters.startVelocityLifetime.x, parameters.endVelocityLifetime.x);
            velocityOverLifetime.y =
                new ParticleSystem.MinMaxCurve(parameters.startVelocityLifetime.y, parameters.endVelocityLifetime.y);
            velocityOverLifetime.z =
                new ParticleSystem.MinMaxCurve(parameters.startVelocityLifetime.z, parameters.endVelocityLifetime.z);

            emission.rateOverTime = parameters.emissionRate;

            weatherParticleRenderer.lengthScale = parameters.lengthScale;
            weatherParticleRenderer.velocityScale = parameters.speedScale;

            collisionModule.dampen = parameters.dampen;
        }
    }
}
