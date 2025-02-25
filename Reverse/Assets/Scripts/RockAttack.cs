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

    public void HitAnimEvent()
    {
        collider.enabled = true;
    }
}
