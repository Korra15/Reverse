using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Arrow : MonoBehaviour
{
    [SerializeField] float arrowSpeed = 20f;

    // Update is called once per frame
    void Update()
    {
        transform.position += transform.right * arrowSpeed * Time.deltaTime;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Rob")) Destroy(this.gameObject);
    }
}
