using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Weather;

public class WeatherEffects : MonoBehaviour
{
    [SerializeField] private ParticleSystem weatherParticles;
    [SerializeField] private float weatherTransitionTime = 3.0f;
    
    //particle system components
    private ParticleSystemRenderer weatherParticleRenderer;

    private EventBinding<WeatherChanged> weatherChangedEventBinding;
    private WeatherParameters currentEffectParameters;
    private WeatherParameters targetEffectParameters;

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
    
    private void Awake() => targetEffectParameters = ScriptableObject.CreateInstance<WeatherParameters>();

    private void Start() => weatherParticleRenderer = weatherParticles.GetComponent<ParticleSystemRenderer>();

    private IEnumerator SetWeatherEffect(WeatherParameters parameters)
    {
        targetEffectParameters = parameters;
        float elapsedTime = 0f;

        WeatherParameters startParameters = currentEffectParameters;

        while (elapsedTime < weatherTransitionTime)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime/weatherTransitionTime);

            currentEffectParameters = LerpWeatherEffectParameters(startParameters, targetEffectParameters, t);
            UpdateParticleSystem(currentEffectParameters);
            yield return null;
        }

        currentEffectParameters = targetEffectParameters;
        UpdateParticleSystem(currentEffectParameters);
    }

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
        return result;
    }

    private void UpdateParticleSystem(WeatherParameters parameters)
    {
        ParticleSystem.MainModule main = weatherParticles.main;
        ParticleSystem.EmissionModule emission = weatherParticles.emission;
        ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = weatherParticles.velocityOverLifetime;
        ParticleSystem.CollisionModule collisionModule = weatherParticles.collision;
        weatherParticleRenderer = weatherParticles.GetComponent<ParticleSystemRenderer>();
        
        main.maxParticles = (int)parameters.maxParticles;
        main.gravityModifier = parameters.gravityModifier;
        main.startLifetime = new ParticleSystem.MinMaxCurve(parameters.startLifetime, parameters.endLifetime);
        
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(parameters.startVelocityLifetime.x, parameters.endVelocityLifetime.x);
        velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(parameters.startVelocityLifetime.y, parameters.endVelocityLifetime.y);
        velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(parameters.startVelocityLifetime.z, parameters.endVelocityLifetime.z);
        
        emission.rateOverTime = parameters.emissionRate;

        weatherParticleRenderer.lengthScale = parameters.lengthScale;
        weatherParticleRenderer.velocityScale = parameters.speedScale;

        collisionModule.dampen = parameters.dampen;
    }
}
