
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Weather;

public class TorchController : MonoBehaviour
{

    private Animator animator;
    private Light2D light;
    
    private EventBinding<WeatherChanged> weatherChangedEventBinding;


    private void Awake()
    {
        animator = GetComponent<Animator>();
        light = GetComponent<Light2D>();
    }

    //event binding
    private void OnEnable()
    {
        weatherChangedEventBinding = new EventBinding<WeatherChanged>((weatherChanged) =>
        {
            if (weatherChanged.WeatherParameters.weatherState != Weather.State.RainStorm && weatherChanged.WeatherParameters.weatherState != State.Rainy)
            {
                animator.SetBool("On", true);
                light.intensity = 2;
                return;
            }
            animator.SetBool("On", false);
            light.intensity = 0;
        });

        EventBus<WeatherChanged>.Register(weatherChangedEventBinding);
    }

    private void OnDisable() => EventBus<WeatherChanged>.Deregister(weatherChangedEventBinding);
    
}
