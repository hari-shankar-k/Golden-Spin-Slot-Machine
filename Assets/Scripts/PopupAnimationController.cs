using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class PopupAnimationController : MonoBehaviour
{
    [Header("Popup References")]
    public GameObject popupPanel;
    public TMP_Text messageText;
    public TMP_Text gameOverText;
    public Button actionButton;
    public TMP_Text actionButtonText;

    [Header("Animation Settings")]
    public float animationDuration = 0.3f;
    public float closedScale = 0.7f;
    public float openScale = 1f;

    [Header("Blink Settings")]
    public float blinkSpeed = 0.5f;
    public Color blinkColor = Color.red;

    [Header("Restart Event")]
    public UnityEvent onRestart;

    private CanvasGroup canvasGroup;
    private RectTransform panelRect;

    private Coroutine popupCoroutine;
    private Coroutine blinkCoroutine;
    private Coroutine disableCoroutine;

    private bool isGameOver;


    void Start()
    {
        panelRect = popupPanel.GetComponent<RectTransform>();

        canvasGroup = popupPanel.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = popupPanel.AddComponent<CanvasGroup>();

        popupPanel.SetActive(false);

        actionButton.onClick.AddListener(OnActionButtonClicked);
    }


    // INSUFFICIENT BALANCE POPUP

    public void ShowInsufficientBalance()
    {
        isGameOver = false;

        messageText.gameObject.SetActive(true);
        gameOverText.gameObject.SetActive(false);

        messageText.text = "NOT ENOUGH GOLD!";

        actionButtonText.text = "OK";

        ShowPopup(messageText);
    }


    // GAME OVER POPUP

    public void ShowGameOver()
    {
        isGameOver = true;

        messageText.gameObject.SetActive(true);
        gameOverText.gameObject.SetActive(true);

        messageText.text = "YOU RAN OUT OF GOLD!";
        gameOverText.text = "GAME OVER";

        actionButtonText.text = "RESTART";

        ShowPopup(gameOverText);
    }


    // OPEN POPUP

    void ShowPopup(TMP_Text blinkingText)
    {
        if (popupCoroutine != null)
            StopCoroutine(popupCoroutine);

        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);

        if (disableCoroutine != null)
            StopCoroutine(disableCoroutine);

        popupPanel.SetActive(true);

        canvasGroup.alpha = 0f;
        panelRect.localScale = Vector3.one * closedScale;

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;

        blinkCoroutine = StartCoroutine(
            BlinkText(blinkingText)
        );

        popupCoroutine = StartCoroutine(
            AnimatePopup(0f, 1f, closedScale, openScale)
        );
    }


    // POPUP SCALE AND FADE ANIMATION

    IEnumerator AnimatePopup(
        float startAlpha,
        float targetAlpha,
        float startScale,
        float targetScale)
    {
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(
                elapsed / animationDuration
            );

            float smoothT = t * t * (3f - 2f * t);

            canvasGroup.alpha = Mathf.Lerp(
                startAlpha,
                targetAlpha,
                smoothT
            );

            float scale = Mathf.Lerp(
                startScale,
                targetScale,
                smoothT
            );

            panelRect.localScale = Vector3.one * scale;

            yield return null;
        }

        canvasGroup.alpha = targetAlpha;

        panelRect.localScale = Vector3.one * targetScale;

        canvasGroup.interactable = targetAlpha > 0f;

        popupCoroutine = null;
    }


    // BLINK TEXT ANIMATION

    IEnumerator BlinkText(TMP_Text text)
    {
        Color originalColor = text.color;

        while (true)
        {
            float t = Mathf.PingPong(
                Time.unscaledTime / blinkSpeed,
                1f
            );

            text.color = Color.Lerp(
                originalColor,
                blinkColor,
                t
            );

            yield return null;
        }
    }


    // BUTTON CLICK

    void OnActionButtonClicked()
    {
        if (isGameOver)
        {
            onRestart.Invoke();
        }

        HidePopup();
    }


    // CLOSE POPUP

    public void HidePopup()
    {
        if (!popupPanel.activeSelf)
            return;

        if (popupCoroutine != null)
            StopCoroutine(popupCoroutine);

        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);

        popupCoroutine = StartCoroutine(
            AnimatePopup(1f, 0f, openScale, closedScale)
        );

        disableCoroutine = StartCoroutine(
            DisableAfterAnimation()
        );
    }


    IEnumerator DisableAfterAnimation()
    {
        yield return new WaitForSecondsRealtime(
            animationDuration
        );

        popupPanel.SetActive(false);

        messageText.color = Color.white;
        gameOverText.color = Color.white;

        disableCoroutine = null;
    }
}