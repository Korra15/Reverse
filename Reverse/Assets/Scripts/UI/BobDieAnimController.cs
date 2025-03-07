using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using TMPro;

public class BobDieAnimController : MonoBehaviour
{
    [Header("Kill Counter")]
    [SerializeField] private TextMeshProUGUI killCounter;

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

    private void TriggerHideAnimation(BobRespawnEvent eventData)
    {
        robDieAnimator.SetTrigger("BobRespawned");
        killCounter.text = eventData.killCtr.ToString();
    }
}
