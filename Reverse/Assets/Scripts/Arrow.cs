using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Arrow : MonoBehaviour
{
    [SerializeField] float arrowSpeed = 20f;
    int damageAmt = 1;
    GameObject rob;

    private void Awake()
    {
        rob = GameObject.FindGameObjectWithTag("Rob");
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 dirToRob = rob.transform.position - this.transform.position;
        dirToRob = new Vector3(dirToRob.x, 0f, 0f);
        transform.position += dirToRob.normalized * arrowSpeed * Time.deltaTime;
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
