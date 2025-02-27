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
        RainStorm,
        LightSnow,
        Snowy,
        SnowStorm

    }
    public class WeatherState : MonoBehaviour
    {
        private int currentStateIndex;
        private EventBinding<BobDieEvent> weatherCycleEvent;

        //array of weather objects to call from in order
        [SerializeField] private WeatherParameters[] weatherStateOrder;
        
        private void OnEnable()
        {
            weatherCycleEvent = new EventBinding<BobDieEvent>(CycleWeatherParameters);
            EventBus<BobDieEvent>.Register(weatherCycleEvent);
        }

        private void OnDisable() => EventBus<BobDieEvent>.Deregister(weatherCycleEvent);

        private void Start()
        {
            currentStateIndex = -1;
            CycleWeatherParameters();
        }

        /// <summary>
        /// Cycles the weather to the next weather in the list
        /// </summary>
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

