using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RobMissedAnimController : MonoBehaviour
{
    private EventBinding<RobMissedEvent> robMissedEvent;
    private Animator robMissedAnimator;

    private void Start()
    {
        robMissedAnimator = GetComponent<Animator>();
    }


    private void OnEnable()
    {
        robMissedEvent = new EventBinding<RobMissedEvent>(TriggerPlayAnimation);
        EventBus<RobMissedEvent>.Register(robMissedEvent);
    }

    private void OnDisable()
    {
        EventBus<RobMissedEvent>.Deregister(robMissedEvent);
    }

    private void TriggerPlayAnimation()
    {
        //robMissedAnimator.Play();
        //robMissedAnimator.SetTrigger("RobMissed");
    }

}
