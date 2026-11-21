#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class QuitConfirmationController : MonoBehaviour
{
    [Header("Popup References")]
    public GameObject popupPanel;
    public Button yesButton;
    public Button noButton;

    [Header("Animation")]
    public float animationDuration = 0.25f;
    public float closedScale = 0.7f;

    private RectTransform popupRect;
    private CanvasGroup canvasGroup;

    private bool isAnimating = false;

    void Start()
    {
        popupRect = popupPanel.GetComponent<RectTransform>();

        canvasGroup = popupPanel.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = popupPanel.AddComponent<CanvasGroup>();

        popupPanel.SetActive(false);

        yesButton.onClick.AddListener(QuitGame);
        noButton.onClick.AddListener(ClosePopup);
    }

    public void ShowPopup()
    {
        if (isAnimating || popupPanel.activeSelf)
            return;

        popupPanel.SetActive(true);

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;

        popupRect.localScale = Vector3.one * closedScale;

        StartCoroutine(OpenAnimation());
    }

    IEnumerator OpenAnimation()
    {
        isAnimating = true;

        float timer = 0f;

        while (timer < animationDuration)
        {
            timer += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(timer / animationDuration);

            canvasGroup.alpha = progress;

            popupRect.localScale = Vector3.Lerp(
                Vector3.one * closedScale,
                Vector3.one,
                progress
            );

            yield return null;
        }

        canvasGroup.alpha = 1f;
        popupRect.localScale = Vector3.one;

        canvasGroup.interactable = true;
        isAnimating = false;
    }

    public void ClosePopup()
    {
        if (isAnimating || !popupPanel.activeSelf)
            return;

        StartCoroutine(CloseAnimation());
    }

    IEnumerator CloseAnimation()
    {
        isAnimating = true;

        canvasGroup.interactable = false;

        float timer = 0f;

        while (timer < animationDuration)
        {
            timer += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(timer / animationDuration);

            canvasGroup.alpha = 1f - progress;

            popupRect.localScale = Vector3.Lerp(
                Vector3.one,
                Vector3.one * closedScale,
                progress
            );

            yield return null;
        }

        popupPanel.SetActive(false);

        canvasGroup.alpha = 1f;
        popupRect.localScale = Vector3.one;

        isAnimating = false;
    }

    public void QuitGame()
    {
        Debug.Log("Player confirmed exit.");

#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
