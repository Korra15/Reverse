using UnityEngine;

namespace Weather
{
    [CreateAssetMenu(fileName = "WeatherType")]
    public class WeatherParameters : ScriptableObject
    {
        [SerializeField] private State weatherState;
        
        [Header("Standard Particle Effect Modifications")]
        [SerializeField] public float maxParticles;
        [SerializeField] public float emissionRate;
        [SerializeField] public float gravityModifier;

        
        [Header("Lifetime (Random between start and end)")]
        [SerializeField] public float startLifetime;
        [SerializeField] public float endLifetime;
        
        [Header("Velocity (Random between start and end)")]
        [SerializeField] public Vector3 startVelocityLifetime;
        [SerializeField] public Vector3 endVelocityLifetime;

        [Header("Speed based scale and length scale")] 
        [SerializeField] public float lengthScale;
        [SerializeField] public float speedScale;

        [Header("Collision Values")] 
        [SerializeField] public float dampen;
    }   
}
