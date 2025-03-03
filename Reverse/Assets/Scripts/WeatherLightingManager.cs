using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Weather;

public class WeatherLightingManager : MonoBehaviour
{
    [SerializeField] private float transitionDuration = 3.0f;
    private Light2D light;

    private EventBinding<WeatherChanged> weatherChangedEventBinding;

    private void Awake() => light = GetComponent<Light2D>();

    //event binding
    private void OnEnable()
    {
        weatherChangedEventBinding = new EventBinding<WeatherChanged>((weatherChanged) =>
        {
            StartCoroutine(SetLight(weatherChanged.WeatherParameters));
        });

        EventBus<WeatherChanged>.Register(weatherChangedEventBinding);
    }

    private void OnDisable() => EventBus<WeatherChanged>.Deregister(weatherChangedEventBinding);


    /// <summary>
    /// Sets transition to lighting based on weather
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns></returns>
    private IEnumerator SetLight(WeatherParameters parameters)
    {
        SunlightParameters sunlightParameters = new SunlightParameters()
        {
            sunlightColor = light.color,
            sunlightIntensity = light.intensity
        };

        float elapsedTime = 0f;


        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / transitionDuration);

            sunlightParameters = LerpParameters(sunlightParameters, parameters, t);
            light.intensity = sunlightParameters.sunlightIntensity;
            light.color = sunlightParameters.sunlightColor;
            yield return null;
        }
        light.intensity = sunlightParameters.sunlightIntensity;
        light.color = sunlightParameters.sunlightColor;
    }

    /// <summary>
    /// Lerps 2 parameter sets together to enable a gradual transition between effects
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    private SunlightParameters LerpParameters(SunlightParameters sunlightParameters,  WeatherParameters to, float t)
    {
        float fromIntensity = Mathf.Lerp(sunlightParameters.sunlightIntensity, to.sunlightIntensity, t);
        Color fromColor = Color.Lerp(sunlightParameters.sunlightColor, to.sunlightColor, t);
        return new SunlightParameters()
        {
            sunlightIntensity = fromIntensity,
            sunlightColor = fromColor
        };
    }

    struct SunlightParameters
    {
        public float sunlightIntensity;
        public Color sunlightColor;
    }
}
