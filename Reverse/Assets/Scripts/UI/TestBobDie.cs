using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBobDie : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) // Simulate Bob's death
        {
            Debug.Log("Bob has died!");
            EventBus<BobDieEvent>.Raise(new BobDieEvent());
        }

        if(Input.GetKeyDown(KeyCode.Tab))
        {
            Debug.Log("Bob respawn!");
            EventBus<BobRespawnEvent>.Raise(new BobRespawnEvent());
        }
    }
}
