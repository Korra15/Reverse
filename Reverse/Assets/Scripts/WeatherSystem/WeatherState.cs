using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Weather
{
    public enum State
    {
        Sunny,
        LightRain,
        Rainy,
        Stormy
    }
    public class WeatherState : MonoBehaviour
    {
        private int currentStateIndex;
        private EventBinding<CycleWeather> weatherCycleEvent;

        [SerializeField] private WeatherParameters[] weatherStateOrder;
        
        private void OnEnable()
        {
            weatherCycleEvent = new EventBinding<CycleWeather>(CycleWeatherParameters);
            EventBus<CycleWeather>.Register(weatherCycleEvent);
        }

        private void OnDisable() => EventBus<CycleWeather>.Deregister(weatherCycleEvent);

        private void Start()
        {
            currentStateIndex = -1;
            CycleWeatherParameters();
        }

        public void CycleWeatherParameters()
        {
            Debug.Log("CYCLE");
            currentStateIndex = (currentStateIndex + 1) % weatherStateOrder.Length;

            print("The Weather is now " + weatherStateOrder[currentStateIndex]);
            
            EventBus<WeatherChanged>.Raise(new WeatherChanged()
            {
                WeatherParameters = weatherStateOrder[currentStateIndex]
            });
        }
        
        public WeatherParameters GetCurrentWeatherParameters => weatherStateOrder[currentStateIndex];
        public int GetNextWeatherParametersIndex => (currentStateIndex + 1) % weatherStateOrder.Length;
    }
}

