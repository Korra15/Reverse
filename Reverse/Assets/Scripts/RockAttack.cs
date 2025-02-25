using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockAttack : MonoBehaviour
{
    Animator animator;
    Collider2D collider;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        collider = GetComponent<BoxCollider2D>();
        collider.enabled = false;
    }

    private void Update()
    {
        // If the animation is finished, destroy itself.
        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.95f)
        {
            GameObject.Destroy(this.gameObject);
        }
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
