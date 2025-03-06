using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Weather
{
    public class UpdateForeground : MonoBehaviour
    {
        State weatherState;
        private EventBinding<WeatherChanged> weatherChangedEventBinding;

        [SerializeField] GameObject foregroundObject;
        [SerializeField] Sprite foregroundDefault;
        [SerializeField] Sprite foregroundSnowy;
        [SerializeField] Sprite foregroundRainy;

        private void OnEnable()
        {
            weatherChangedEventBinding = new EventBinding<WeatherChanged>((weatherChanged) =>
                {
                    // Lambda Function: Updates the foreground depending on what the current weather is. 
                    weatherState = weatherChanged.WeatherParameters.weatherState;
                    if (weatherState == State.SnowStorm || weatherState == State.Snowy) foregroundObject.GetComponent<SpriteRenderer>().sprite = foregroundSnowy;
                    else if (weatherState == State.RainStorm || weatherState == State.Rainy) foregroundObject.GetComponent<SpriteRenderer>().sprite = foregroundRainy;
                    else foregroundObject.GetComponent<SpriteRenderer>().sprite = foregroundDefault;
                });
            EventBus<WeatherChanged>.Register(weatherChangedEventBinding);
        }
        private void OnDisable()
        {
            EventBus<WeatherChanged>.Deregister(weatherChangedEventBinding);
        }
    }
}
