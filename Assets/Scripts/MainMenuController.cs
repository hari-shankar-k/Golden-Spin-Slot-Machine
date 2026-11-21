
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [Header("Menu Buttons")]
    public Button newGameButton;
    public Button loadGameButton;
    public Button exitGameButton;

    [Header("Fade Transition")]
    public CanvasGroup fadePanel;
    public float fadeDuration = 0.7f;
    public float blackScreenDelay = 1f;

    private const string BALANCE_KEY = "SlotMachine_Balance";
    private const string GAME_SCENE = "Slot Machine";

    private bool isTransitioning = false;

    void Start()
    {
        newGameButton.onClick.AddListener(NewGame);
        loadGameButton.onClick.AddListener(LoadGame);
        exitGameButton.onClick.AddListener(ExitGame);

        RefreshLoadButton();

        fadePanel.alpha = 1f;
        fadePanel.blocksRaycasts = true;
        fadePanel.interactable = false;

        StartCoroutine(FadeIn());
    }

    // Check whether a valid saved game exists
    void RefreshLoadButton()
    {
        bool hasSave = PlayerPrefs.HasKey(BALANCE_KEY);

        if (hasSave)
        {
            int savedBalance = PlayerPrefs.GetInt(BALANCE_KEY, -1);

            // Negative balance means invalid save data
            if (savedBalance < 0)
            {
                PlayerPrefs.DeleteKey(BALANCE_KEY);
                PlayerPrefs.Save();

                hasSave = false;
            }
        }

        loadGameButton.interactable = hasSave;

        // Change button appearance when disabled
        ColorBlock colors = loadGameButton.colors;
        colors.disabledColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);
        loadGameButton.colors = colors;
    }

    IEnumerator FadeIn()
    {
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;

            fadePanel.alpha = 1f -
                Mathf.Clamp01(timer / fadeDuration);

            yield return null;
        }

        fadePanel.alpha = 0f;
        fadePanel.blocksRaycasts = false;
    }

    IEnumerator FadeOut()
    {
        fadePanel.blocksRaycasts = true;

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;

            fadePanel.alpha =
                Mathf.Clamp01(timer / fadeDuration);

            yield return null;
        }

        fadePanel.alpha = 1f;
    }

    public void NewGame()
    {
        if (isTransitioning) return;

        isTransitioning = true;

        // Start a completely fresh game
        PlayerPrefs.SetInt(BALANCE_KEY, 1000);
        PlayerPrefs.Save();

        StartCoroutine(LoadGameScene());
    }

    public void LoadGame()
    {
        if (isTransitioning) return;

        if (!PlayerPrefs.HasKey(BALANCE_KEY))
        {
            RefreshLoadButton();
            return;
        }

        int savedBalance = PlayerPrefs.GetInt(BALANCE_KEY, -1);

        if (savedBalance < 0)
        {
            PlayerPrefs.DeleteKey(BALANCE_KEY);
            PlayerPrefs.Save();

            RefreshLoadButton();
            return;
        }

        Debug.Log("Loading saved game. Balance: " + savedBalance);

        isTransitioning = true;

        StartCoroutine(LoadGameScene());
    }

    IEnumerator LoadGameScene()
    {
        yield return StartCoroutine(FadeOut());

        yield return new WaitForSecondsRealtime(blackScreenDelay);

        SceneManager.LoadScene(GAME_SCENE);
    }

    public void ExitGame()
    {
        if (isTransitioning) return;

        isTransitioning = true;

        StartCoroutine(QuitApplication());
    }

    IEnumerator QuitApplication()
    {
        yield return StartCoroutine(FadeOut());

        yield return new WaitForSecondsRealtime(blackScreenDelay);

        Application.Quit();

        Debug.Log("Game Exited");
    }
}
