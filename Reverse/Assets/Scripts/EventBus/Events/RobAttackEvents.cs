using UnityEngine;

    //Event to change the weather to the next weather in the WeatherState
    public struct BobDesiredPositionUpdateAttackEvent : IEvent
    {
        public int attackId;
        public int attackTimes;
    }

    //Event that updates the weatherEffects to use new parameters
    public struct RobAttackEvent : IEvent
    {
        public Collider2D attackBoundaries;
        public float occurTimes;
        public float duration;
    }

