using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Arrow : MonoBehaviour
{
    [SerializeField] float arrowSpeed = 20f;
    int damageAmt = 1;

    // Update is called once per frame
    void Update()
    {
        transform.position += transform.right * arrowSpeed * Time.deltaTime;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Rob"))
        {
            FindObjectOfType<RobBasics>().TakeHealth(damageAmt);
            Destroy(this.gameObject);
        }
    }
    public void SetDamageAmount(int damage) { damageAmt = damage; }
}
