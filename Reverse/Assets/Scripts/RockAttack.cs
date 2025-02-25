using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockAttack : MonoBehaviour
{
    Collider2D collider;

    private void Awake()
    {
        collider = GetComponent<BoxCollider2D>();
        collider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Bob"))
        {
            // TODO: Damage Bob
        }
    }

    // This event gets called on the hit impact frame to enable the hitbox.
    public void HitAnimEvent()
    {
        collider.enabled = true;
    }
    
    // This event gets called on the last frame of the animation to destroy this instance of the game object. 
    public void DestroyItself()
    {
        Destroy(this.gameObject);
    }
}
