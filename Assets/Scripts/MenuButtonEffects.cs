
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

[RequireComponent(typeof(Button))]
public class MenuButtonEffects : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [Header("Animation")]
    public float hoverScale = 1.08f;
    public float pressScale = 0.94f;
    public float animationSpeed = 12f;

    [Header("Audio")]
    public AudioClip hoverSound;
    public AudioClip clickSound;

    [Range(0f, 1f)]
    public float volume = 0.7f;

    private Vector3 originalScale;
    private Vector3 targetScale;

    private Button button;
    private AudioSource audioSource;

    void Awake()
    {
        button = GetComponent<Button>();

        originalScale = transform.localScale;
        targetScale = originalScale;

        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.unscaledDeltaTime * animationSpeed
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!button.interactable) return;

        targetScale = originalScale * hoverScale;

        if (hoverSound != null)
            audioSource.PlayOneShot(hoverSound, volume);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!button.interactable) return;

        targetScale = originalScale * pressScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!button.interactable) return;

        targetScale = originalScale * hoverScale;
    }

    public void PlayClickSound()
    {
        if (clickSound != null)
            audioSource.PlayOneShot(clickSound, volume);
    }
}
