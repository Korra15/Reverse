using System;
using UnityEngine;
using Weather;
using Random = UnityEngine.Random;


[RequireComponent(typeof(Collider2D))]
public class LightningSpawner : MonoBehaviour
{
    [SerializeField] private float timeBetweenSpawns;
    [SerializeField] private GameObject lightingPrefab;

    private Collider2D collider;
    private EventBinding<WeatherChanged> weatherChangedEventBinding;
    private bool isStorming = false;
    private float timer;


    private void Awake()
    {
        collider = GetComponent<Collider2D>();
    }

    //event binding
    private void OnEnable()
    {
        weatherChangedEventBinding = new EventBinding<WeatherChanged>((weatherChanged) =>
        {
            if (weatherChanged.WeatherParameters.weatherState != State.RainStorm)
            {
                isStorming = false;
                return;
            }

            isStorming = true;
        });

        EventBus<WeatherChanged>.Register(weatherChangedEventBinding);
    }

    private void OnDisable() => EventBus<WeatherChanged>.Deregister(weatherChangedEventBinding);

    private void Update()
    {
        timer += Time.deltaTime;
        if (!isStorming || timer < timeBetweenSpawns) return;

        timer = 0;
        SpawnLighting();
    }

    private void SpawnLighting()
    {
        Vector3 randomSpawnPos = RandomPointInBounds(collider.bounds);
        GameObject lighting = Instantiate(lightingPrefab, transform);
        lighting.transform.position =
            new Vector3(randomSpawnPos.x, transform.position.y, lighting.transform.position.z);
    }
    
    /// <summary>
    /// Helper to get a random point in bounds (collider)
    /// </summary>
    /// <param name="bounds"></param>
    /// <returns></returns>
    private static Vector3 RandomPointInBounds(Bounds bounds) {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }
}
