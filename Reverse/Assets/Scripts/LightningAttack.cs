using UnityEngine;

public class LightningAttack : MonoBehaviour
{
    Animator animator;
    private Collider2D collider;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        collider = GetComponent<Collider2D>();
    }


    private void Update()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.95f) Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.CompareTag("Bob"))
        {
            EventBus<RobAttackEvent>.Raise(new RobAttackEvent()
            {
                attackBoundaries = collider,
                occurTimes =  0,
                duration = .1f
            });
        }

        if (other.gameObject.CompareTag("Rob"))
        {
            other.GetComponent<RobBasics>().TakeHealth(1);
        }
        
    }
}
