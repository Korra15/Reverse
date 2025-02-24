using System;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public class InputTracker : MonoBehaviour
{

    [SerializeField] private float comboTimeBetweenInputs = 1.0f;
    
    private float timeSinceLastInput;
    Dictionary<string, float> comboTracker = new Dictionary<string, float>();
    private Dictionary<int, int> individualAttackTracker = new Dictionary<int, int>();
    private List<string> activeComboHolder = new List<string>();

    private void Start() => timeSinceLastInput = 0;

    private void Update()
    {
        timeSinceLastInput += Time.deltaTime;

        if (timeSinceLastInput > comboTimeBetweenInputs && activeComboHolder.Count > 0)  activeComboHolder.Clear();
    }

    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            AddInput("1");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            AddInput("2");
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            AddInput("3");
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("Killed Bob");
            StoreCombo();
        }
    }

    /// <summary>
    /// Stores an input, will store a combo if the time has expired
    /// </summary>
    /// <param name="input"></param>
    public void AddInput(string input)
    {
        if(activeComboHolder.Count >= 3)  activeComboHolder.Clear();
        
        activeComboHolder.Add(input);
        timeSinceLastInput = 0;
        
        //update the tracker for individual attacks
        int attackNum = int.Parse(input);
        
        individualAttackTracker.TryAdd(attackNum, 0);
        individualAttackTracker[attackNum] += 1;
        Debug.Log(individualAttackTracker[attackNum]);
        
        //raise event with combo id and num times
        EventBus<AttackEvents.BobDesiredPositionUpdateAttackEvent>.Raise(new AttackEvents.BobDesiredPositionUpdateAttackEvent()
        {
            attackId = attackNum,
            attackTimes = individualAttackTracker[attackNum]
        });
        
        
        string combo = String.Join("", activeComboHolder);

        //if the dict has a combo matching current, get it
        float comboNum;
        if (comboTracker.TryGetValue(combo, out comboNum))
        {
            Debug.Log("Had Combo in brain");
        }
        else
        {
            comboNum = 0;
            Debug.Log("Combo  was not in brain");
        }
        
        //give bob the number of times combo has been used
        EventBus<AttackEvents.RobAttackEvent>.Raise(new AttackEvents.RobAttackEvent()
        {
            attackBoundaries = new []{-2f,2f},
            duration = 1.0f,
            occurTimes = comboNum
        });
    }
    
    /// <summary>
    /// Stores the current combo in the dictionary and resets the combo list.
    /// </summary>
    private void StoreCombo()
    {
        string combo = string.Join("", activeComboHolder);
        Debug.Log(combo);

        if (comboTracker.ContainsKey(combo)) comboTracker[combo]++;
        else comboTracker.Add(combo, 1);

        activeComboHolder.Clear();
        timeSinceLastInput = 0;
    }
    
}
