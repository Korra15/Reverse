using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ControlsMenu : MonoBehaviour
{
    [Header("ControlsMenu Buttons")]
    [SerializeField] private Button playButton;

    private void Start()
    {
        playButton.onClick.AddListener(Play);
    }

    private void Play()
    {
        SceneManager.LoadScene(2);
    }
}
