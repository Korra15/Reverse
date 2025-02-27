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
        private EventBinding<BobDieEvent> bobDieEvent;
        private EventBinding<CycleWeather> weatherCycleEvent;

        //array of weather objects to call from in order
        [SerializeField] private WeatherParameters[] weatherStateOrder;
        
        private void OnEnable()
        {
            bobDieEvent = new EventBinding<BobDieEvent>(RandomWeatherParameters);
            EventBus<BobDieEvent>.Register(bobDieEvent);
            weatherCycleEvent = new EventBinding<CycleWeather>(RandomWeatherParameters);
            EventBus<CycleWeather>.Register(weatherCycleEvent);
        }

        private void OnDisable()
        {
            EventBus<BobDieEvent>.Deregister(bobDieEvent);
            EventBus<CycleWeather>.Deregister(weatherCycleEvent);
        }

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
        
        /// <summary>
        /// Random Weather
        /// </summary>
        public void RandomWeatherParameters()
        {
            Debug.Log("CYCLE");
            currentStateIndex = Random.Range(0, weatherStateOrder.Length);

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

