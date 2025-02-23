using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUIManager : MonoBehaviour
{
    private bool isPaused = false;

    [Header("PauseMenu UI")]
    [SerializeField] private GameObject PauseMenu;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("paused pressed");
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (!isPaused) PauseGame();
        else ResumeGame();
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        isPaused = true;
        PauseMenu.SetActive(true);
    }

    public void ResumeGame()
    {
        PauseMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }
}
