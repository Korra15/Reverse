using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public class GameUIManager : MonoBehaviour
{
    private bool isPaused = false;

    [Header("PauseMenu Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button exitButton;
    
    [Header("PauseMenu UI")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] RectTransform pauseMenuRect;
    [SerializeField] float topPosY, middlePosY;
    [SerializeField] float tweenDuration;   

    private void Start()
    {
        resumeButton.onClick.AddListener(ResumeGame);
        exitButton.onClick.AddListener(Exit);
        restartButton.onClick.AddListener(RestartGame);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
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
        PauseMenuIntro();
        Time.timeScale = 0f;
        isPaused = true;
        pauseMenu.SetActive(true);
    }

    public async void ResumeGame()
    {
        await PauseMenuOutro();
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void PauseMenuIntro()
    {
        pauseMenuRect.DOAnchorPosY(middlePosY, tweenDuration).SetUpdate(true);
        //pauseMenu.GetComponent<RectTransform>().DOAnchorPosY(middlePosY, tweenDuration).SetUpdate(true);
    }

    async Task PauseMenuOutro()
    {
        await pauseMenuRect.DOAnchorPosY(topPosY, tweenDuration).SetUpdate(true).AsyncWaitForCompletion();
    }

    public void Exit()
    {
        Application.Quit();
    }
    
    public void RestartGame()
    {
        Time.timeScale = 1;
        DOTween.Clear(true);
        DOTween.KillAll();
        SceneManager.LoadScene(2);
    }

}
