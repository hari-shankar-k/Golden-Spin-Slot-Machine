
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PaytableController : MonoBehaviour
{
    [Header("References")]
    public GameObject paytablePanel;
    public Button openButton;
    public Button closeButton;

    [Header("Animation Settings")]
    public float animationDuration = 0.25f;
    public float openScale = 1f;
    public float closedScale = 0.7f;

    private CanvasGroup canvasGroup;
    private RectTransform panelRect;
    private Coroutine animationCoroutine;
    private bool isOpen = false;

    void Start()
    {
        panelRect = paytablePanel.GetComponent<RectTransform>();

        canvasGroup = paytablePanel.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = paytablePanel.AddComponent<CanvasGroup>();

        paytablePanel.SetActive(false);

        openButton.onClick.AddListener(OpenPaytable);
        closeButton.onClick.AddListener(ClosePaytable);
    }

    public void OpenPaytable()
    {
        if (isOpen) return;

        isOpen = true;

        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        paytablePanel.SetActive(true);

        animationCoroutine = StartCoroutine(
            AnimatePopup(0f, 1f, closedScale, openScale)
        );
    }

    public void ClosePaytable()
    {
        if (!isOpen) return;

        isOpen = false;

        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        animationCoroutine = StartCoroutine(
            AnimatePopup(1f, 0f, openScale, closedScale)
        );
    }

    IEnumerator AnimatePopup(
        float startAlpha,
        float targetAlpha,
        float startScale,
        float targetScale)
    {
        float elapsed = 0f;

        canvasGroup.alpha = startAlpha;

        panelRect.localScale = Vector3.one * startScale;

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;

        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / animationDuration);

            // Smooth easing
            float smoothT = t * t * (3f - 2f * t);

            // Fade animation
            canvasGroup.alpha = Mathf.Lerp(
                startAlpha,
                targetAlpha,
                smoothT
            );

            // Zoom animation
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

        if (targetAlpha == 0f)
        {
            paytablePanel.SetActive(false);
        }
        else
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        animationCoroutine = null;
    }
}
