using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class RobHealthBar : MonoBehaviour
{
    private Slider robHealthBar;
    [SerializeField] private RobBasics rob;
    private EventBinding<RobHealthDecrease> robHealthDecrease;

    private void Start()
    {
        robHealthBar = GetComponent<Slider>();
        if(!rob) rob = GameObject.FindAnyObjectByType<RobBasics>();  
    }

    private void OnEnable()
    {
        robHealthDecrease = new EventBinding<RobHealthDecrease>(DecreaseRobHealth);
        EventBus<RobHealthDecrease>.Register(robHealthDecrease);
    }

    private void OnDisable()
    {
        EventBus<RobHealthDecrease>.Deregister(robHealthDecrease);
    }

    private void DecreaseRobHealth()
    {
        robHealthBar.value = rob.health * 0.01f; 
    }
}
