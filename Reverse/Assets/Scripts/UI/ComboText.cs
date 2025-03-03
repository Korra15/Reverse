using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Xml.Serialization;

public class ComboText : MonoBehaviour
{
    private string[] comboIDStringMapping = { "Melee", "Ranged", "AOE" };
    private TextMeshProUGUI comboText;

    //Events
    private EventBinding<AddingToCombo> addingToComboEvent;    
    private EventBinding<ClearCombo> clearComboEvent;

    private void OnEnable()
    {
        addingToComboEvent = new EventBinding<AddingToCombo>(AddToComboString);
        EventBus<AddingToCombo>.Register(addingToComboEvent);

        clearComboEvent = new EventBinding<ClearCombo>(ClearComboString);
        EventBus<ClearCombo>.Register(clearComboEvent);
    }

    private void OnDisable()
    {
        EventBus<AddingToCombo>.Deregister(addingToComboEvent);
        EventBus<ClearCombo>.Deregister(clearComboEvent);
    }

    private void Start()
    {
        comboText = GetComponent<TextMeshProUGUI>();
        comboText.text = null;
    }

    private void AddToComboString(AddingToCombo eventData)
    {
        string comboStr = " ";
        switch(eventData.comboToAdd)
        {
            case "1":
                comboStr = comboIDStringMapping[0];
                break;
            case "2":
                 comboStr = comboIDStringMapping[1];
                break;
            case "3":
                 comboStr = comboIDStringMapping[2];
                break;
        }

        if (comboText.text == null) comboText.text = comboStr;
        else comboText.text += " + " + comboStr;
    }

    private void ClearComboString()
    {
        comboText.text = null;
    }
}
