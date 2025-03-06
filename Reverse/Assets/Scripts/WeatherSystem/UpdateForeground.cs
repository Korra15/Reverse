using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Weather
{
    public class UpdateForeground : MonoBehaviour
    {
        private EventBinding<WeatherChanged> weatherChangedEventBinding;

        [SerializeField] SpriteRenderer foregroundObject;
        [SerializeField] SpriteRenderer foregroundObjectSnowy;
        [SerializeField] SpriteRenderer foregroundObjectRainy;

        private SpriteRenderer currentForegroundObject;
        
        [SerializeField] private float transitionDuration = 10;

        private void Awake()
        {
            currentForegroundObject = foregroundObject;
            foregroundObject.color = new Color(1,1,1, 1.0f);
            foregroundObjectSnowy.color = new Color(1,1,1, 0.0f);
            foregroundObjectRainy.color = new Color(1,1,1, 0.0f);
        }

        private void OnEnable()
        {
            weatherChangedEventBinding = new EventBinding<WeatherChanged>((weatherChanged) =>
                {
                    // Lambda Function: Updates the foreground depending on what the current weather is. 
                    StartCoroutine(SetBackground(weatherChanged.WeatherParameters.weatherState));

                });
            EventBus<WeatherChanged>.Register(weatherChangedEventBinding);
        }
        private void OnDisable()
        {
            EventBus<WeatherChanged>.Deregister(weatherChangedEventBinding);
        }


        private void LerpBackgrounds(Weather.State state, float t)
        {
            currentForegroundObject.color = new Color(1,1,1, Mathf.Lerp(currentForegroundObject.color.a, 0.0f, t));
            
            //lerp to snow
            if (state == State.SnowStorm || state == State.Snowy)
            {
                foregroundObjectSnowy.color = new Color(1,1,1, Mathf.Lerp( foregroundObjectSnowy.color.a, 1, t));
            }
            else if (state == State.RainStorm || state == State.Rainy) //lerp to rain
            {
                foregroundObjectRainy.color = new Color(1,1,1, Mathf.Lerp(foregroundObjectRainy.color.a, 1, t));
            }
            else //lerp to normal
            {
                foregroundObject.color = new Color(1,1,1, Mathf.Lerp(foregroundObject.color.a, 1, t));
            }
        }
        
        /// <summary>
        /// Sets a weather effect with new parameters, will transition to new effect
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private IEnumerator SetBackground(Weather.State state)
        {
            float elapsedTime = 0f;
            
            while (elapsedTime <= transitionDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / transitionDuration);
                
                LerpBackgrounds(state, t);
                yield return null;
            }
            
            //update currentbackground
            if (state == State.SnowStorm || state == State.Snowy)
            {
                print("snow");
                currentForegroundObject = foregroundObjectSnowy;
            }
            else if (state == State.RainStorm || state == State.Rainy) //lerp to rain
            {
                print("rain");
                currentForegroundObject = foregroundObjectRainy;
            }
            else //lerp to normal
            {
                print("normal");
                currentForegroundObject = foregroundObject;
            }
        }
    }
}
