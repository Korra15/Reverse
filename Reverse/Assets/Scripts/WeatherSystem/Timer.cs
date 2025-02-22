using System;
using UnityEngine;

public class Timer : MonoBehaviour
{
   [SerializeField] private float timeBetweenWeather;
   [SerializeField] private float timer;

   private void Start() => timer = timeBetweenWeather;


   private void Update()
   {
      timer -= Time.deltaTime;

      if (timer <= 0)
      {
         timer = timeBetweenWeather;
         EventBus<CycleWeather>.Raise(new CycleWeather(){});
      }
   }

   public void SetTimer(float time) => timer = time;

   public float GetCurrentTime => timer;
}
