using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class BobDieAnimController : MonoBehaviour
{
    private EventBinding<BobDieEvent> bobDieEvent;
    private EventBinding<BobRespawnEvent> bobRespawnEvent;
    private Animator robDieAnimator;

    private void Start()
    {
        robDieAnimator = GetComponent<Animator>();
    }


    private void OnEnable()
    {
        bobDieEvent = new EventBinding<BobDieEvent>(TriggerPlayAnimation);
        EventBus<BobDieEvent>.Register(bobDieEvent);
        bobRespawnEvent = new EventBinding<BobRespawnEvent>(TriggerHideAnimation);
        EventBus<BobRespawnEvent>.Register(bobRespawnEvent);
    }

    private void OnDisable()
    {
        EventBus<BobDieEvent>.Deregister(bobDieEvent);
        EventBus<BobRespawnEvent>.Deregister(bobRespawnEvent);
    }

    private void TriggerPlayAnimation()
    {
        robDieAnimator.SetTrigger("BobDied");
    }

    private void TriggerHideAnimation()
    {
        robDieAnimator.SetTrigger("BobRespawned");
    }
}
