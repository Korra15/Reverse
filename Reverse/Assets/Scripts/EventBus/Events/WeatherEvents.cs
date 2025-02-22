
//Event to change the weather to the next weather in the WeatherState
public struct CycleWeather : IEvent
{
}

//Event that updates the weatherEffects to use new parameters
public struct WeatherChanged : IEvent
{
    public Weather.WeatherParameters WeatherParameters;
}
