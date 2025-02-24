using UnityEngine;

public class AttackEvents : MonoBehaviour
{
    //Event to change the weather to the next weather in the WeatherState
    public struct BobDesiredPositionUpdateAttackEvent : IEvent
    {
        public int attackId;
        public int attackTimes;
    }

    //Event that updates the weatherEffects to use new parameters
    public struct RobAttackEvent : IEvent
    {
        public float[] attackBoundaries;
        public float occurTimes;
        public float duration;
    }

}
