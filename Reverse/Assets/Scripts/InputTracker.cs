using System;
using System.Collections.Generic;
using UnityEngine;

public class InputTracker : MonoBehaviour
{

    [SerializeField] private float comboTimeBetweenInputs = 1.0f;
    private float timeSinceLastInput;
    Dictionary<string, float> comboTracker = new Dictionary<string, float>();

    private List<char> activeComboHolder = new List<char>();
    private bool activeCombo = false;

    private void Start() => timeSinceLastInput = 0;

    private void Update()
    {
        timeSinceLastInput += Time.deltaTime;
        
        if(timeSinceLastInput > comboTimeBetweenInputs && activeComboHolder.Count > 0)
            StoreCombo();
    }

    /// <summary>
    /// Stores an input, will store a combo if the time has expired
    /// </summary>
    /// <param name="input"></param>
    public void AddInput(Char input)
    {
        if (timeSinceLastInput > comboTimeBetweenInputs && activeComboHolder.Count > 0) StoreCombo(); 
        
        activeComboHolder.Add(input);
        timeSinceLastInput = 0;
    }
    
    /// <summary>
    /// Stores the current combo in the dictionary and resets the combo list.
    /// </summary>
    private void StoreCombo()
    {
        string combo = string.Join("", activeComboHolder);

        if (comboTracker.ContainsKey(combo)) comboTracker[combo]++;
        else comboTracker.Add(combo, 1);

        activeComboHolder.Clear();
        timeSinceLastInput = 0;
    }
}
