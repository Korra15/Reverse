using System;
using System.Collections.Generic;
using UnityEngine;

public class InputTracker : MonoBehaviour
{

    [SerializeField] private float comboTimeBetweenInputs = 1.0f;
    
    private float timeSinceLastInput;
    Dictionary<string, float> comboTracker = new Dictionary<string, float>();
    private List<char> activeComboHolder = new List<char>();

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
            AddInput('1');
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            AddInput('2');
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            AddInput('3');
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
    public void AddInput(Char input)
    {
        if(activeComboHolder.Count >= 3)  activeComboHolder.Clear();
        
        activeComboHolder.Add(input);
        timeSinceLastInput = 0;


        string combo = String.Join("", activeComboHolder);

        //if the dict has a combo matching current, get it
        if (comboTracker.ContainsKey(combo))
        {
            //give bob the number of times combo has been used
            Debug.Log("Had Combo tracked");
        }
        else
        {
            //give zero
            Debug.Log("Combo  was Untrackked");
        }
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
