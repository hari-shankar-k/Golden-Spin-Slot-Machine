using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class BetButtonAnimation : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [Header("Animation")]
    public float hoverScale = 1.08f;
    public float pressScale = 0.94f;
    public float animationSpeed = 12f;

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color hoverColor = new Color(0.8f, 0.8f, 0.8f);
    public Color disabledColor = new Color(0.45f, 0.45f, 0.45f);

    private RectTransform rectTransform;
    private Image buttonImage;
    private Button button;

    private Vector3 originalScale;
    private Vector3 targetScale;

    private bool isHovered;
    private bool isPressed;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        buttonImage = GetComponent<Image>();
        button = GetComponent<Button>();

        originalScale = rectTransform.localScale;
        targetScale = originalScale;
    }

    void Update()
    {
        rectTransform.localScale = Vector3.Lerp(
            rectTransform.localScale,
            targetScale,
            Time.unscaledDeltaTime * animationSpeed
        );

        if (buttonImage == null || button == null)
            return;

        if (!button.interactable)
        {
            buttonImage.color = disabledColor;
        }
        else if (isPressed)
        {
            buttonImage.color = hoverColor;
        }
        else if (isHovered)
        {
            buttonImage.color = hoverColor;
        }
        else
        {
            buttonImage.color = normalColor;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!button.interactable) return;

        isHovered = true;
        targetScale = originalScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        isPressed = false;
        targetScale = originalScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!button.interactable) return;

        isPressed = true;
        targetScale = originalScale * pressScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;

        targetScale = isHovered
            ? originalScale * hoverScale
            : originalScale;
    }

    void OnEnable()
    {
        if (rectTransform != null)
        {
            rectTransform.localScale = originalScale;
            targetScale = originalScale;
        }
    }
}