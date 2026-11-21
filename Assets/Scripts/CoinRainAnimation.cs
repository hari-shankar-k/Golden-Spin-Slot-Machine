using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CoinRainAnimation : MonoBehaviour
{
    [Header("References")]
    public RectTransform coinContainer;
    public GameObject coinPrefab;

    [Header("Coin Settings")]
    [Min(1)] public int coinCount = 45;
    [Min(0.1f)] public float fallDuration = 3f;
    [Min(0.1f)] public float spawnDuration = 1.5f;

    [Header("Animation")]
    [Tooltip("Degrees per second. Coins flip around their vertical Y axis.")]
    public float rotationSpeed = 720f;
    public float fallDistance = 1200f;
    public float horizontalDrift = 150f;

    [Header("Coin Rain Sound")]
    public AudioSource coinRainAudioSource;
    public AudioClip coinRainSound;
    [Range(0f, 1f)] public float coinRainVolume = 0.8f;

    private readonly List<GameObject> activeCoins = new List<GameObject>();
    private Coroutine rainCoroutine;

    private void Awake()
    {
        SetupAudioSource();
    }

    private void OnEnable()
    {
        // Prevent any sound from carrying over when this object is enabled.
        StopCoinRainSound();
    }

    private void SetupAudioSource()
    {
        if (coinRainAudioSource == null)
            coinRainAudioSource = GetComponent<AudioSource>();

        if (coinRainAudioSource == null)
            coinRainAudioSource = gameObject.AddComponent<AudioSource>();

        coinRainAudioSource.playOnAwake = false;
        coinRainAudioSource.loop = true;
        coinRainAudioSource.spatialBlend = 0f;
        coinRainAudioSource.volume = coinRainVolume;
        coinRainAudioSource.Stop();
    }

    public void PlayCoinRain()
    {
        if (coinContainer == null || coinPrefab == null)
        {
            Debug.LogWarning(
                "CoinRainAnimation: Assign Coin Container and Coin Prefab."
            );
            return;
        }

        if (rainCoroutine != null)
            StopCoroutine(rainCoroutine);

        ClearCoins();
        StopCoinRainSound();

        if (coinRainAudioSource != null && coinRainSound != null)
        {
            coinRainAudioSource.clip = coinRainSound;
            coinRainAudioSource.volume = coinRainVolume;
            coinRainAudioSource.loop = true;
            coinRainAudioSource.Play();
        }

        rainCoroutine = StartCoroutine(SpawnCoins());
    }

    private IEnumerator SpawnCoins()
    {
        int safeCoinCount = Mathf.Max(1, coinCount);
        float safeSpawnDuration = Mathf.Max(0.1f, spawnDuration);
        float delay = safeSpawnDuration / safeCoinCount;

        for (int i = 0; i < safeCoinCount; i++)
        {
            GameObject coin = Instantiate(coinPrefab, coinContainer);
            RectTransform rect = coin.GetComponent<RectTransform>();

            if (rect != null)
            {
                float halfWidth = coinContainer.rect.width * 0.5f;
                float randomX = Random.Range(-halfWidth, halfWidth);

                rect.anchoredPosition = new Vector2(
                    randomX,
                    coinContainer.rect.height * 0.5f + rect.rect.height
                );

                rect.localRotation = Quaternion.identity;
                activeCoins.Add(coin);
                StartCoroutine(AnimateCoin(coin));
            }
            else
            {
                Debug.LogWarning("GoldCoin prefab needs a RectTransform.");
                Destroy(coin);
            }

            yield return new WaitForSecondsRealtime(delay);
        }

        // Wait long enough for the final coin to finish falling.
        yield return new WaitForSecondsRealtime(Mathf.Max(0.1f, fallDuration));

        ClearCoins();
        StopCoinRainSound();
        rainCoroutine = null;
    }

    private IEnumerator AnimateCoin(GameObject coin)
    {
        RectTransform rect = coin.GetComponent<RectTransform>();
        Image image = coin.GetComponent<Image>();

        if (rect == null)
        {
            activeCoins.Remove(coin);
            yield break;
        }

        float timer = 0f;
        float duration = Mathf.Max(0.1f, fallDuration);

        Vector2 startPosition = rect.anchoredPosition;
        Vector2 endPosition = new Vector2(
            startPosition.x + Random.Range(-horizontalDrift, horizontalDrift),
            startPosition.y - fallDistance
        );

        float flipDirection = Random.value < 0.5f ? -1f : 1f;
        float flipSpeed = Random.Range(
            rotationSpeed * 0.7f,
            rotationSpeed * 1.3f
        );

        Color originalColor = image != null ? image.color : Color.white;

        while (timer < duration && coin != null)
        {
            timer += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(timer / duration);
            float eased = progress * progress * (3f - 2f * progress);

            rect.anchoredPosition = Vector2.Lerp(
                startPosition,
                endPosition,
                eased
            );

            // Flip around the vertical Y axis: the coin turns edge-on,
            // then reveals its opposite face, like a real falling coin.
            rect.Rotate(
                Vector3.up,
                flipDirection * flipSpeed * Time.unscaledDeltaTime,
                Space.Self
            );

            if (image != null)
            {
                Color color = originalColor;
                color.a = 1f - progress;
                image.color = color;
            }

            yield return null;
        }

        if (coin != null)
            Destroy(coin);

        activeCoins.Remove(coin);
    }

    private void StopCoinRainSound()
    {
        if (coinRainAudioSource != null)
        {
            coinRainAudioSource.Stop();
            coinRainAudioSource.loop = false;
        }
    }

    private void ClearCoins()
    {
        foreach (GameObject coin in activeCoins)
        {
            if (coin != null)
                Destroy(coin);
        }

        activeCoins.Clear();
    }

    private void OnDisable()
    {
        if (rainCoroutine != null)
        {
            StopCoroutine(rainCoroutine);
            rainCoroutine = null;
        }

        ClearCoins();
        StopCoinRainSound();
    }
}
