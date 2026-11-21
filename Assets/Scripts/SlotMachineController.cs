using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SlotMachineController : MonoBehaviour
{
    [Header("Reels")]
    public RectTransform[] reels;

    [Header("Spin Settings")]
    public float spinDuration = 2f;
    public float stopDelay = 0.65f;
    public int minimumRotations = 4;

    [Header("Symbol Settings")]
    public float symbolSpacing = 100f;
    public float wrapDistance = 400f;
    public float wrapThreshold = -153f;

    [Header("Winning Symbol Highlight")]
    public float highlightDuration = 2.5f;
    public float highlightScale = 1.12f;
    public float highlightPulseSpeed = 8f;
    public Color normalWinColor = Color.yellow;
    public Color jackpotWinColor = new Color(1f, 0.65f, 0f);

    [Header("Stop Alignment")]
    public float targetY = 18f;

    [Header("Betting")]
    public int balance = 1000;
    public int selectedBet = 10;
    public int minimumBet = 10;

    [Header("UI References")]
    public TMP_Text balanceText;
    public TMP_Text resultText;
    public Button bet10Button;
    public Button bet50Button;
    public Button bet100Button;

    [Header("Balance Animation")]
    public float balanceAnimationDuration = 0.6f;
    public Color winBalanceColor = new Color(1f, 0.84f, 0f);
    public float winPulseScale = 1.2f;

    [Header("Lever Animation")]
    public GameObject leverUp;
    public GameObject leverDown;
    public float leverPullDuration = 0.3f;

    [Header("Coin Rain Animation")]
    public CoinRainAnimation coinRainAnimation;

    [Header("TESTING - Must be OFF for submission")]
    public bool forceJackpotForTesting = false;

    [Header("Jackpot Animation")]
    public TMP_Text jackpotText;
    public float jackpotScale = 1.5f;
    public float jackpotAnimationDuration = 0.5f;
    public float jackpotDisplayDuration = 3f;
    public float colorChangeSpeed = 0.35f;

    [Header("Winning Payout Animation")]
    public TMP_Text payoutPopupText;
    public float payoutAnimationDuration = 1.2f;
    public float payoutPopupScale = 1.35f;
    public float payoutRiseDistance = 70f;

    [Header("Near Miss Effect")]
    public float nearMissDuration = 1.1f;
    public float nearMissPulseSpeed = 8f;
    public float nearMissScale = 1.2f;
    public Color nearMissColor = new Color(1f, 0.84f, 0f);

    [Header("Reel Jackpot Blink")]
    public CanvasGroup reelsFlashGroup;
    public int reelFlashCount = 8;
    public float reelFlashSpeed = 0.15f;
    public float reelDimAlpha = 0.2f;

    [Header("Audio Effects")]
    public AudioSource leverAudio;
    public AudioSource reelStartAudio;
    public AudioSource reelStopAudio;
    public AudioSource jackpotAudio;
    public AudioSource pairWinAudio;
    public AudioSource threeMatchAudio;

    [Header("Popup System")]
    public PopupAnimationController popupAnimationController;

    private bool isSpinning;
    private bool isGameOver;

    private int[] randomResults;

    private int displayedBalance;
    private Color normalBalanceColor;
    private Vector3 originalBalanceScale;

    private Coroutine balanceCoroutine;
    private Coroutine highlightCoroutine;
    private Coroutine payoutAnimationCoroutine;
    private Coroutine nearMissCoroutine;

    private Color originalResultColor;
    private Vector3 originalResultScale;

    // PlayerPrefs key used to store the player balance between sessions.
    private const string BALANCE_KEY = "SlotMachine_Balance";


    void Start()
    {
        // Load a saved balance, or use the Inspector default on the first run.
        balance = Mathf.Max(0, PlayerPrefs.GetInt(BALANCE_KEY, balance));
        minimumBet = Mathf.Max(1, minimumBet);
        selectedBet = Mathf.Max(minimumBet, selectedBet);

        if (!ValidateReelSetup())
        {
            Debug.LogError("SlotMachineController: Setup is invalid. Check that exactly three reels are assigned and each reel has at least one symbol.", this);
            SetBetButtonsInteractable(false);
            enabled = false;
            return;
        }

        SetupReels();

        if (leverUp != null) leverUp.SetActive(true);
        if (leverDown != null) leverDown.SetActive(false);
        if (jackpotText != null) jackpotText.gameObject.SetActive(false);

        // Keep the normal-win payout popup hidden until a win occurs.
        if (payoutPopupText != null)
        {
            CanvasGroup payoutCanvasGroup = payoutPopupText.GetComponent<CanvasGroup>();
            if (payoutCanvasGroup == null)
                payoutCanvasGroup = payoutPopupText.gameObject.AddComponent<CanvasGroup>();

            payoutCanvasGroup.alpha = 0f;
            payoutCanvasGroup.interactable = false;
            payoutCanvasGroup.blocksRaycasts = false;
            payoutPopupText.gameObject.SetActive(false);
        }

        if (reelsFlashGroup != null)
            reelsFlashGroup.alpha = 1f;

        // Use configured audio lengths only when valid clips are assigned.
        if (reelStartAudio != null && reelStartAudio.clip != null && reelStartAudio.clip.length > 0f)
            spinDuration = reelStartAudio.clip.length;

        if (reelStopAudio != null && reelStopAudio.clip != null && reels.Length > 1)
            stopDelay = reelStopAudio.clip.length / (reels.Length - 1);

        spinDuration = Mathf.Max(0.1f, spinDuration);
        stopDelay = Mathf.Max(0f, stopDelay);
        minimumRotations = Mathf.Max(1, minimumRotations);
        wrapDistance = Mathf.Max(1f, wrapDistance);

        if (balanceText != null)
        {
            displayedBalance = balance;
            normalBalanceColor = balanceText.color;
            originalBalanceScale = balanceText.transform.localScale;
            UpdateBalanceUI();
        }

        // Add each listener exactly once.
        if (bet10Button != null)
            bet10Button.onClick.AddListener(() => PlaceBetAndSpin(10));
        if (bet50Button != null)
            bet50Button.onClick.AddListener(() => PlaceBetAndSpin(50));
        if (bet100Button != null)
            bet100Button.onClick.AddListener(() => PlaceBetAndSpin(100));

        if (resultText != null)
        {
            originalResultColor = resultText.color;
            originalResultScale = resultText.transform.localScale;
            resultText.text = "SELECT YOUR BET!";
        }

        SetBetButtonsInteractable(balance >= minimumBet);

        // Wait one frame so PopupAnimationController has completed its Start().
        StartCoroutine(CheckLoadedBalance());
    }

    private bool ValidateReelSetup()
    {
        if (reels == null || reels.Length != 3)
            return false;

        for (int i = 0; i < reels.Length; i++)
        {
            if (reels[i] == null || reels[i].childCount < 1)
                return false;

            for (int j = 0; j < reels[i].childCount; j++)
            {
                if (!(reels[i].GetChild(j) is RectTransform))
                    return false;
            }
        }

        return true;
    }

    private IEnumerator CheckLoadedBalance()
    {
        yield return null;

        if (balance < minimumBet)
            HandleGameOver();
    }


    // REEL SETUP

    void SetupReels()
    {
        foreach (RectTransform reel in reels)
        {
            if (reel == null) continue;

            for (int i = 0; i < reel.childCount; i++)
            {
                RectTransform symbol =
                    reel.GetChild(i) as RectTransform;

                if (symbol == null) continue;

                symbol.anchoredPosition =
                    new Vector2(0, 150 - (i * symbolSpacing));
            }
        }
    }


    // BETTING BUTTON CONTROL

    void SetBetButtonsInteractable(bool interactable)
    {
        bool canBet = interactable && !isSpinning && !isGameOver;

        if (bet10Button != null)
            bet10Button.interactable = canBet && balance >= 10;
        if (bet50Button != null)
            bet50Button.interactable = canBet && balance >= 50;
        if (bet100Button != null)
            bet100Button.interactable = canBet && balance >= 100;
    }


    // BETTING SYSTEM

    public void PlaceBetAndSpin(int amount)
    {
        if (isSpinning || isGameOver)
            return;

        if (amount < minimumBet || (amount != 10 && amount != 50 && amount != 100))
        {
            Debug.LogWarning("SlotMachineController: Invalid bet amount: " + amount, this);
            return;
        }

        if (!ValidateReelSetup())
        {
            Debug.LogError("Cannot spin: reel references or symbols are missing.", this);
            SetBetButtonsInteractable(false);
            return;
        }

        if (balance < amount)
        {
            if (popupAnimationController != null)
                popupAnimationController.ShowInsufficientBalance();
            else
                Debug.LogWarning("Insufficient balance and no popup controller is assigned.", this);
            return;
        }

        selectedBet = amount;
        ResetNearMissEffect();
        ResetSymbolHighlights();

        // Deduct and persist the wager before starting the spin.
        balance -= selectedBet;
        SaveBalance();
        SetBetButtonsInteractable(false);
        AnimateBalanceTo(balance, false);
        GenerateRandomResults();

        if (leverAudio != null) leverAudio.Play();
        if (reelStartAudio != null) reelStartAudio.Play();

        StartCoroutine(PullLever());
        StartCoroutine(SpinAllReels());
    }


    IEnumerator PullLever()
    {
        if (leverUp == null || leverDown == null)
            yield break;

        leverUp.SetActive(false);
        leverDown.SetActive(true);
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, leverPullDuration));

        if (leverUp != null) leverUp.SetActive(true);
        if (leverDown != null) leverDown.SetActive(false);
    }


    // BALANCE UI

    void UpdateBalanceUI()
    {
        if (balanceText != null)
            balanceText.text = "GOLD: " + balance;
    }


    // BALANCE ANIMATION

    void AnimateBalanceTo(int targetBalance, bool isWin)
    {
        if (balanceText == null)
            return;

        if (balanceCoroutine != null)
            StopCoroutine(balanceCoroutine);

        balanceCoroutine = StartCoroutine(
            BalanceAnimation(targetBalance, isWin)
        );
    }


    IEnumerator BalanceAnimation(int targetBalance, bool isWin)
    {
        int startBalance = displayedBalance;

        float elapsed = 0f;

        float duration = Mathf.Max(0.01f, balanceAnimationDuration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            float smoothT = t * t * (3f - 2f * t);

            displayedBalance = Mathf.RoundToInt(
                Mathf.Lerp(startBalance, targetBalance, smoothT)
            );

            balanceText.text = "GOLD: " + displayedBalance;

            if (isWin)
            {
                balanceText.color = Color.Lerp(
                    normalBalanceColor,
                    winBalanceColor,
                    smoothT
                );

                float pulse = 1f +
                    Mathf.Sin(t * Mathf.PI) * (winPulseScale - 1f);

                balanceText.transform.localScale =
                    originalBalanceScale * pulse;
            }
            else
            {
                balanceText.color = normalBalanceColor;

                balanceText.transform.localScale =
                    originalBalanceScale;
            }

            yield return null;
        }

        displayedBalance = targetBalance;

        balanceText.text = "GOLD: " + displayedBalance;

        balanceText.color = normalBalanceColor;

        balanceText.transform.localScale = originalBalanceScale;

        balanceCoroutine = null;
    }


    // RANDOM SYMBOL GENERATION

    void GenerateRandomResults()
    {
        randomResults = new int[reels.Length];

        // This option is for local testing only. Keep it OFF for submission.
        if (forceJackpotForTesting)
        {
            for (int i = 0; i < reels.Length; i++)
                randomResults[i] = 0;

            Debug.LogWarning("TEST MODE: Forced 777 jackpot is enabled.", this);
            return;
        }

        for (int i = 0; i < reels.Length; i++)
        {
            randomResults[i] = Random.Range(0, reels[i].childCount);
            Debug.Log("Reel " + (i + 1) + " Result: " + GetSymbolName(randomResults[i]));
        }
    }


    string GetSymbolName(int index)
    {
        switch (index)
        {
            case 0: return "7";
            case 1: return "Cherries";
            case 2: return "Bell";
            case 3: return "BAR";
            default: return "Unknown";
        }
    }


    // REEL SPINNING

    IEnumerator SpinAllReels()
    {
        isSpinning = true;
        SetBetButtonsInteractable(false);

        if (resultText != null)
            resultText.text = "GOOD LUCK!";

        float totalDuration = spinDuration + ((reels.Length - 1) * stopDelay);

        for (int i = 0; i < reels.Length; i++)
        {
            float duration = spinDuration + (i * stopDelay);
            StartCoroutine(SpinReel(reels[i], duration, randomResults[i], i));
        }

        yield return new WaitForSecondsRealtime(spinDuration);

        if (reelStopAudio != null)
            reelStopAudio.Play();

        yield return new WaitForSecondsRealtime(Mathf.Max(0f, totalDuration - spinDuration));

        // Allow the final reel-stop sound to finish before presenting the result.
        if (reelStopAudio != null && reelStopAudio.isPlaying)
        {
            // A looping or misconfigured stop AudioSource must not freeze the game flow.
            float waited = 0f;
            float maxWait = reelStopAudio.clip != null
                ? Mathf.Max(0.5f, reelStopAudio.clip.length + 0.25f)
                : 5f;

            while (reelStopAudio.isPlaying && waited < maxWait)
            {
                waited += Time.unscaledDeltaTime;
                yield return null;
            }

            if (reelStopAudio.isPlaying)
                reelStopAudio.Stop();
        }

        CheckWinningResult();
        isSpinning = false;
        SetBetButtonsInteractable(!isGameOver);
        Debug.Log("All reels stopped!", this);
    }


    IEnumerator SpinReel(
        RectTransform reel,
        float duration,
        int targetSymbolIndex,
        int reelIndex)
    {
        if (reel == null || targetSymbolIndex < 0 || targetSymbolIndex >= reel.childCount)
            yield break;

        RectTransform selectedSymbol = reel.GetChild(targetSymbolIndex) as RectTransform;
        if (selectedSymbol == null)
            yield break;

        duration = Mathf.Max(0.01f, duration);
        float startY = selectedSymbol.anchoredPosition.y;

        float remainingDistance =
            Mathf.Repeat(startY - targetY, wrapDistance);

        float totalDistance =
            (minimumRotations * wrapDistance) + remainingDistance;

        float elapsed = 0f;
        float previousDistance = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            float easedT =
                1f - Mathf.Pow(1f - t, 3f);

            float currentDistance =
                totalDistance * easedT;

            float movement =
                currentDistance - previousDistance;

            previousDistance = currentDistance;

            MoveReel(reel, movement);

            yield return null;
        }

        MoveReel(reel, totalDistance - previousDistance);

        AlignSelectedSymbol(reel, targetSymbolIndex);

        Debug.Log(
            "Reel " + (reelIndex + 1) +
            " stopped on " + GetSymbolName(targetSymbolIndex)
        );
    }


    void MoveReel(RectTransform reel, float movement)
    {
        for (int i = 0; i < reel.childCount; i++)
        {
            RectTransform symbol = reel.GetChild(i) as RectTransform;
            if (symbol == null) continue;

            Vector2 position = symbol.anchoredPosition;

            position.y -= movement;

            if (position.y < wrapThreshold)
                position.y += wrapDistance;

            symbol.anchoredPosition = position;
        }
    }


    void AlignSelectedSymbol(
        RectTransform reel,
        int targetSymbolIndex)
    {
        RectTransform selectedSymbol =
            reel.GetChild(targetSymbolIndex) as RectTransform;

        float difference =
            targetY - selectedSymbol.anchoredPosition.y;

        for (int i = 0; i < reel.childCount; i++)
        {
            RectTransform symbol = reel.GetChild(i) as RectTransform;
            if (symbol == null) continue;

            Vector2 position = symbol.anchoredPosition;

            position.y += difference;

            symbol.anchoredPosition = position;
        }
    }


    // WINNING COMBINATIONS AND PAYOUTS

    void CheckWinningResult()
    {
        if (randomResults == null || randomResults.Length != 3)
        {
            Debug.LogError("Cannot calculate payout: result data is invalid.", this);
            return;
        }

        int first = randomResults[0];
        int second = randomResults[1];
        int third = randomResults[2];
        int winnings = 0;

        // Jackpot: all three reels show 7.
        if (first == 0 && second == 0 && third == 0)
        {
            winnings = selectedBet * 10;
            AwardWinnings(winnings);

            if (resultText != null) resultText.text = "";
            Debug.Log("777 JACKPOT!", this);

            if (coinRainAnimation != null) coinRainAnimation.PlayCoinRain();
            if (jackpotAudio != null) jackpotAudio.Play();
            if (jackpotText != null) StartCoroutine(PlayJackpotAnimation(winnings));
            if (reelsFlashGroup != null) StartCoroutine(BlinkReels());

            highlightCoroutine = StartCoroutine(
                HighlightWinningSymbols(new bool[] { true, true, true }, true));
            return;
        }

        bool allMatch = first == second && second == third;
        bool firstSecond = first == second;
        bool secondThird = second == third;
        bool firstThird = first == third;

        if (allMatch)
        {
            winnings = selectedBet * 5;
            if (resultText != null)
                resultText.text = "THREE OF A KIND! +" + winnings + " GOLD";
            if (threeMatchAudio != null) threeMatchAudio.Play();
        }
        else if (firstSecond || secondThird || firstThird)
        {
            winnings = selectedBet * 2;
            bool twoSevens =
                (first == 0 && second == 0) ||
                (second == 0 && third == 0) ||
                (first == 0 && third == 0);

            if (resultText != null)
                resultText.text = twoSevens ? "SO CLOSE TO JACKPOT!" : "PAIR! +" + winnings + " GOLD";

            if (twoSevens && resultText != null)
            {
                if (nearMissCoroutine != null) StopCoroutine(nearMissCoroutine);
                nearMissCoroutine = StartCoroutine(PlayNearMissEffect());
            }
            if (pairWinAudio != null) pairWinAudio.Play();
        }
        else
        {
            if (resultText != null) resultText.text = "NO WIN! TRY AGAIN!";
        }

        if (winnings > 0)
        {
            bool[] matchingReels;
            if (allMatch)
                matchingReels = new bool[] { true, true, true };
            else if (firstSecond)
                matchingReels = new bool[] { true, true, false };
            else if (secondThird)
                matchingReels = new bool[] { false, true, true };
            else
                matchingReels = new bool[] { true, false, true };

            highlightCoroutine = StartCoroutine(HighlightWinningSymbols(matchingReels, false));
            AwardWinnings(winnings);

            if (payoutPopupText != null)
            {
                if (payoutAnimationCoroutine != null)
                    StopCoroutine(payoutAnimationCoroutine);
                payoutAnimationCoroutine = StartCoroutine(PlayPayoutAnimation(winnings));
            }
        }

        Debug.Log("Winnings: " + winnings, this);

        if (balance < minimumBet)
            HandleGameOver();
    }

    private void AwardWinnings(int winnings)
    {
        if (winnings <= 0) return;

        // Prevent integer overflow from corrupting the saved balance.
        long updatedBalance = (long)balance + winnings;
        balance = updatedBalance > int.MaxValue ? int.MaxValue : (int)updatedBalance;
        SaveBalance();
        AnimateBalanceTo(balance, true);
    }

    private void HandleGameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        SetBetButtonsInteractable(false);

        if (popupAnimationController != null)
            popupAnimationController.ShowGameOver();
        else
            Debug.LogWarning("Game over, but PopupAnimationController is not assigned.", this);
    }


    // PERSISTENT BALANCE

    void SaveBalance()
    {
        balance = Mathf.Max(0, balance);
        PlayerPrefs.SetInt(BALANCE_KEY, balance);
        PlayerPrefs.Save();
    }

    // Save when the application closes or is sent to the background.
    void OnApplicationQuit()
    {
        SaveBalance();
    }

    void OnApplicationPause(bool paused)
    {
        if (paused)
            SaveBalance();
    }


    // RESTART GAME

    public void RestartGame()
    {
        if (isSpinning) return;

        isGameOver = false;
        balance = 1000;
        selectedBet = minimumBet;
        SaveBalance();

        if (balanceCoroutine != null)
        {
            StopCoroutine(balanceCoroutine);
            balanceCoroutine = null;
        }
        displayedBalance = balance;
        UpdateBalanceUI();

        ResetNearMissEffect();
        ResetSymbolHighlights();
        if (resultText != null) resultText.text = "SELECT YOUR BET!";
        SetBetButtonsInteractable(true);
        Debug.Log("Game restarted!", this);
    }


    // NEAR-MISS EFFECT: TWO 7s, BUT NOT A 777 JACKPOT

    IEnumerator PlayNearMissEffect()
    {
        if (resultText == null)
            yield break;

        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, nearMissDuration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Pulse the existing Result text in gold without creating another UI object.
            float pulse = (Mathf.Sin(elapsed * nearMissPulseSpeed) + 1f) * 0.5f;
            resultText.color = Color.Lerp(originalResultColor, nearMissColor, pulse);
            resultText.transform.localScale = Vector3.Lerp(
                originalResultScale,
                originalResultScale * nearMissScale,
                pulse
            );

            yield return null;
        }

        resultText.color = originalResultColor;
        resultText.transform.localScale = originalResultScale;
        nearMissCoroutine = null;
    }

    void ResetNearMissEffect()
    {
        if (nearMissCoroutine != null)
        {
            StopCoroutine(nearMissCoroutine);
            nearMissCoroutine = null;
        }

        if (resultText != null)
        {
            resultText.color = originalResultColor;
            resultText.transform.localScale = originalResultScale;
        }
    }


    // WINNING SYMBOL HIGHLIGHT

    void ResetSymbolHighlights()
    {
        if (highlightCoroutine != null)
        {
            StopCoroutine(highlightCoroutine);
            highlightCoroutine = null;
        }

        if (reels == null)
            return;

        foreach (RectTransform reel in reels)
        {
            if (reel == null)
                continue;

            for (int i = 0; i < reel.childCount; i++)
            {
                Transform symbol = reel.GetChild(i);

                if (symbol == null)
                    continue;

                Image image = symbol.GetComponent<Image>();

                if (image != null)
                    image.color = Color.white;

                symbol.localScale = Vector3.one;
            }
        }
    }


    IEnumerator HighlightWinningSymbols(bool[] matchingReels, bool jackpot)
    {
        if (reels == null || randomResults == null ||
            matchingReels == null || reels.Length < 3 ||
            randomResults.Length < 3 || matchingReels.Length < 3)
        {
            yield break;
        }

        Transform[] winningSymbols = new Transform[3];
        Image[] winningImages = new Image[3];
        Vector3[] originalScales = new Vector3[3];
        Color[] originalColors = new Color[3];

        Color targetColor = jackpot ? jackpotWinColor : normalWinColor;

        // Cache the selected symbol and its original appearance.
        for (int i = 0; i < 3; i++)
        {
            if (!matchingReels[i] || reels[i] == null ||
                randomResults[i] < 0 ||
                randomResults[i] >= reels[i].childCount)
            {
                continue;
            }

            winningSymbols[i] = reels[i].GetChild(randomResults[i]);
            winningImages[i] = winningSymbols[i].GetComponent<Image>();
            originalScales[i] = winningSymbols[i].localScale;

            if (winningImages[i] != null)
                originalColors[i] = winningImages[i].color;
        }

        float elapsed = 0f;

        while (elapsed < highlightDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float pulse = (Mathf.Sin(elapsed * highlightPulseSpeed) + 1f) / 2f;

            for (int i = 0; i < 3; i++)
            {
                if (winningSymbols[i] == null)
                    continue;

                winningSymbols[i].localScale = Vector3.Lerp(
                    originalScales[i],
                    originalScales[i] * highlightScale,
                    pulse
                );

                if (winningImages[i] != null)
                {
                    winningImages[i].color = Color.Lerp(
                        originalColors[i],
                        targetColor,
                        pulse
                    );
                }
            }

            yield return null;
        }

        // Restore the original appearance after the highlight finishes.
        for (int i = 0; i < 3; i++)
        {
            if (winningSymbols[i] == null)
                continue;

            winningSymbols[i].localScale = originalScales[i];

            if (winningImages[i] != null)
                winningImages[i].color = originalColors[i];
        }

        highlightCoroutine = null;
    }


    // NORMAL WIN PAYOUT POPUP ANIMATION

    IEnumerator PlayPayoutAnimation(int winnings)
    {
        if (payoutPopupText == null)
            yield break;

        RectTransform rect =
            payoutPopupText.GetComponent<RectTransform>();

        CanvasGroup canvasGroup =
            payoutPopupText.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup =
                payoutPopupText.gameObject.AddComponent<CanvasGroup>();

        // Cache the original UI position and scale.
        Vector2 originalPosition = rect.anchoredPosition;
        Vector3 originalScale = rect.localScale;

        payoutPopupText.text = "+" + winnings + " GOLD";
        payoutPopupText.color = new Color(1f, 0.84f, 0f);

        payoutPopupText.gameObject.SetActive(true);

        canvasGroup.alpha = 0f;
        rect.localScale = originalScale * 0.5f;
        rect.anchoredPosition = originalPosition;

        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, payoutAnimationDuration);

        // Pop in, bounce slightly, rise and fade out.
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            float scaleProgress;
            if (t < 0.25f)
            {
                float popT = t / 0.25f;
                scaleProgress = Mathf.Lerp(0.5f, payoutPopupScale,
                    1f - Mathf.Pow(1f - popT, 3f));
            }
            else
            {
                float settleT = (t - 0.25f) / 0.75f;
                scaleProgress = Mathf.Lerp(payoutPopupScale, 1f,
                    settleT * settleT * (3f - 2f * settleT));
            }

            rect.localScale = originalScale * scaleProgress;

            float riseT = Mathf.SmoothStep(0f, 1f, t);
            rect.anchoredPosition = originalPosition +
                Vector2.up * (payoutRiseDistance * riseT);

            // Fade in quickly, then fade out during the final part.
            if (t < 0.15f)
                canvasGroup.alpha = t / 0.15f;
            else if (t > 0.65f)
                canvasGroup.alpha = 1f - ((t - 0.65f) / 0.35f);
            else
                canvasGroup.alpha = 1f;

            yield return null;
        }

        canvasGroup.alpha = 0f;
        rect.anchoredPosition = originalPosition;
        rect.localScale = originalScale;
        payoutPopupText.gameObject.SetActive(false);

        payoutAnimationCoroutine = null;
    }


    // JACKPOT ANIMATION

    IEnumerator PlayJackpotAnimation(int winnings)
    {
        if (jackpotText == null) yield break;

        jackpotText.gameObject.SetActive(true);

        jackpotText.enabled = true;

        jackpotText.text =
            "JACKPOT!\n+" + winnings + " GOLD";

        RectTransform rect =
            jackpotText.GetComponent<RectTransform>();

        rect.localScale = Vector3.zero;

        Color[] jackpotColors =
        {
            new Color(1f, 0.84f, 0f),
            Color.red,
            Color.cyan,
            Color.green,
            new Color(1f, 0.55f, 0f),
            Color.yellow
        };

        float elapsed = 0f;

        // Zoom-in animation
        float introDuration = Mathf.Max(0.01f, jackpotAnimationDuration);
        while (elapsed < introDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(
                elapsed / introDuration
            );

            float scale = Mathf.Lerp(
                0f,
                jackpotScale,
                1f - Mathf.Pow(1f - t, 3f)
            );

            rect.localScale = Vector3.one * scale;

            yield return null;
        }

        rect.localScale = Vector3.one * jackpotScale;

        // Colour-changing animation
        float displayTime = 0f;
        int colorIndex = 0;

        while (displayTime < jackpotDisplayDuration)
        {
            jackpotText.color =
                jackpotColors[colorIndex];

            colorIndex =
                (colorIndex + 1) % jackpotColors.Length;

            float colorDelay = Mathf.Max(0.05f, colorChangeSpeed);
            yield return new WaitForSecondsRealtime(colorDelay);

            displayTime += colorDelay;
        }

        jackpotText.color = Color.yellow;

        jackpotText.gameObject.SetActive(false);

        rect.localScale = Vector3.zero;
    }


    // REEL BLINK ANIMATION

    IEnumerator BlinkReels()
    {
        if (reelsFlashGroup == null)
            yield break;

        reelsFlashGroup.alpha = 1f;

        for (int i = 0; i < reelFlashCount; i++)
        {
            reelsFlashGroup.alpha = reelDimAlpha;

            yield return new WaitForSecondsRealtime(Mathf.Max(0.01f, reelFlashSpeed));

            reelsFlashGroup.alpha = 1f;

            yield return new WaitForSecondsRealtime(Mathf.Max(0.01f, reelFlashSpeed));
        }

        reelsFlashGroup.alpha = 1f;
    }



}