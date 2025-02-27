using UnityEngine;

public class LightningAttack : MonoBehaviour
{
    Animator animator;

    private void Awake() => animator = GetComponent<Animator>();
    

    private void Update()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.95f) Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.CompareTag("Bob"))
        {
            print("LIGHTING HIT BOB");
        }

        if (other.gameObject.CompareTag("Rob"))
        {
            print("LIGHTING HIT ROB");
        }
    }
}
