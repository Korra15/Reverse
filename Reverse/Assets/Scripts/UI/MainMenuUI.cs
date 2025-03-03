using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Menu Panel")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private Image mainMenuBackground;
    [SerializeField] private Image mainMenuButtons;

    [Header("Fade Panel")]
    [SerializeField] private GameObject fadeScenePanel; 

    [Header("UI Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button exitButton;

    void Start()
    {
        playButton.onClick.AddListener(Play);
        exitButton.onClick.AddListener(Exit);
    }

    private async void Play()
    {
        await AnimateMenu();
        SceneManager.LoadScene(1);
    }

    private void Exit()
    {
        Application.Quit();
    }

    //private async Task AnimateMenu()
    //{
    //    TaskCompletionSource<bool> backgroundFadeTask = new TaskCompletionSource<bool>();
    //    TaskCompletionSource<bool> moveTask = new TaskCompletionSource<bool>();
    //    TaskCompletionSource<bool> scaleTask = new TaskCompletionSource<bool>();

    //    mainMenuBackground.DOFade(0, 1.5f).OnComplete(() => backgroundFadeTask.SetResult(true)); 
    //    mainMenuPanel.transform.DOMoveZ(Camera.main.transform.position.z + 2, 1.5f).SetEase(Ease.InOutQuad).OnComplete(() => backgroundFadeTask.SetResult(true)); 
    //    mainMenuPanel.transform.DOScale(1.2f, 1.5f).OnComplete(() => backgroundFadeTask.SetResult(true));

    //    await Task.WhenAll(backgroundFadeTask.Task, moveTask.Task, scaleTask.Task);
    //}

    private async Task AnimateMenu()
    {
        // Backgound animaitions
        Task fadeBgTask = mainMenuBackground.DOFade(0, 1.5f).AsyncWaitForCompletion();
        Task moveBgTask = mainMenuBackground.transform.DOMoveZ(Camera.main.transform.position.z - 2, 1.5f).SetEase(Ease.InOutQuad).AsyncWaitForCompletion();
        Task scaleBgTask = mainMenuBackground.transform.DOScale(0.8f, 1.5f).AsyncWaitForCompletion();

        // Button animations
        Task fadeButtonsTask = mainMenuButtons.GetComponent<CanvasGroup>().DOFade(0, 1.5f).AsyncWaitForCompletion();
        Task moveButtonsTask = mainMenuButtons.transform.DOMoveZ(Camera.main.transform.position.z + 2, 1.5f).SetEase(Ease.InOutQuad).AsyncWaitForCompletion();
        Task scaleButtonsTask = mainMenuButtons.transform.DOScale(1.2f, 1.5f).AsyncWaitForCompletion();
        await Task.WhenAll(fadeBgTask, moveBgTask, scaleBgTask, fadeButtonsTask, moveButtonsTask, scaleButtonsTask);
    }


}
