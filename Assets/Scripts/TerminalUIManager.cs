using UnityEngine;
using TMPro;
using System.Collections;

public class TerminalUIManager : MonoBehaviour
{
    [Header("UI Text References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI multiplierText;
    [SerializeField] private TextMeshProUGUI dayQuotaText;
    [SerializeField] private TextMeshProUGUI strikesText;
    [SerializeField] private CanvasGroup screenFadeGroup;

    [Header("Screen Shake Settings")]
    [SerializeField] private Transform mainCameraTransform;
    [SerializeField] private RectTransform uiPanelToShake;
    [SerializeField] private float errorShakeDuration = 0.35f;
    [SerializeField] private float errorShakeMagnitude = 12f;

    [Header("Score Style (Gold)")]
    [SerializeField] private Color scoreGoldColor = new Color(1f, 0.83f, 0f); // Gold Base
    [SerializeField] private Color scoreGlowColor = new Color(1f, 0.98f, 0.6f); // Bright Gold/White Pulse

    [Header("Multiplier Fiery Gradient")]
    [SerializeField] private float maxMultiplier = 5.0f;
    [SerializeField] private Color multBaseColor = Color.white;
    [SerializeField] private Color multMidColor = new Color(1f, 0.65f, 0f); // Bright Amber
    [SerializeField] private Color multMaxColor = new Color(1f, 0.15f, 0.15f); // Fiery Red

    [Header("Strikes & Warning Style")]
    [SerializeField] private Color strikeBaseColor = Color.white;
    [SerializeField] private Color strikeFlashColor = new Color(1f, 0.2f, 0.2f); // Red Flash

    [Header("Audio Feedback")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip successSound;
    [SerializeField] private AudioClip errorBuzzerSound;
    [SerializeField] private AudioClip strikeAlarmSound;
    [SerializeField] private AudioClip buttonClickSound;

    // Original Local Scale Caches
    private Vector3 scoreOriginalScale;
    private Vector3 multOriginalScale;
    private Vector3 strikeOriginalScale;

    private Vector3 originalCameraPos;
    private Vector3 originalPanelPos;

    private int previousScore = -1;
    private int previousStrikes = -1;
    private float previousMultiplier = -1f;

    // Independent Coroutine Trackers
    private Coroutine scoreRoutine;
    private Coroutine multRoutine;
    private Coroutine strikeRoutine;
    private Coroutine shakeRoutine;

    private void OnEnable()
    {
        GameManager.OnScoreUpdated += HandleScoreUpdated;
        GameManager.OnStrikesUpdated += HandleStrikesUpdated;
        GameManager.OnQuotaUpdated += HandleQuotaUpdated;
    }

    private void OnDisable()
    {
        GameManager.OnScoreUpdated -= HandleScoreUpdated;
        GameManager.OnStrikesUpdated -= HandleStrikesUpdated;
        GameManager.OnQuotaUpdated -= HandleQuotaUpdated;
    }

    private void Start()
    {
        if (mainCameraTransform != null) originalCameraPos = mainCameraTransform.localPosition;
        if (uiPanelToShake != null) originalPanelPos = uiPanelToShake.anchoredPosition;

        // Cache initial scales & disable word wrapping so text never breaks onto two lines
        InitTextElement(scoreText, ref scoreOriginalScale, scoreGoldColor);
        InitTextElement(multiplierText, ref multOriginalScale, multBaseColor);
        InitTextElement(strikesText, ref strikeOriginalScale, strikeBaseColor);
        InitTextElement(dayQuotaText, ref multOriginalScale, Color.white);

        // Force initial state update
        if (GameManager.Instance != null)
        {
            HandleScoreUpdated(GameManager.Instance.Score, GameManager.Instance.CurrentMultiplier);
            HandleStrikesUpdated(GameManager.Instance.CurrentStrikes, 3);
            HandleQuotaUpdated(GameManager.Instance.ProcessedVisitorsCount, 10);
        }
    }

    private void InitTextElement(TextMeshProUGUI text, ref Vector3 scaleCache, Color initialColor)
    {
        if (text == null) return;
        scaleCache = text.transform.localScale;
        text.color = initialColor;
        text.enableWordWrapping = false; // Prevents TMP text from dropping into 2 lines on scale
        text.overflowMode = TextOverflowModes.Overflow;
    }

    private void HandleScoreUpdated(int newScore, float multiplier)
    {
        // --- SCORE PULSE ---
        if (scoreText != null)
        {
            scoreText.text = $"SCORE: {newScore:D6}";

            if (previousScore >= 0 && newScore > previousScore)
            {
                TriggerPulse(ref scoreRoutine, scoreText, scoreOriginalScale, scoreGlowColor, scoreGoldColor, 1.25f);
                PlaySound(successSound);
            }
            else if (previousScore < 0)
            {
                scoreText.color = scoreGoldColor;
            }
            previousScore = newScore;
        }

        // --- MULTIPLIER HEAT GRADIENT & PULSE ---
        if (multiplierText != null)
        {
            multiplierText.text = $"MULT: {multiplier:F1}x";
            Color targetHeatColor = GetHeatColor(multiplier);

            if (previousMultiplier >= 0 && multiplier > previousMultiplier)
            {
                TriggerPulse(ref multRoutine, multiplierText, multOriginalScale, Color.white, targetHeatColor, 1.3f);
            }
            else
            {
                multiplierText.color = targetHeatColor;
            }
            previousMultiplier = multiplier;
        }
    }

    private void HandleStrikesUpdated(int currentStrikes, int maxStrikes)
    {
        if (strikesText != null)
        {
            strikesText.text = $"STRIKES: {currentStrikes}/{maxStrikes}";

            if (previousStrikes >= 0 && currentStrikes > previousStrikes)
            {
                TriggerPulse(ref strikeRoutine, strikesText, strikeOriginalScale, strikeFlashColor, strikeBaseColor, 1.3f);
                TriggerScreenShake();
                PlaySound(strikeAlarmSound);
            }
            else if (previousStrikes < 0)
            {
                strikesText.color = strikeBaseColor;
            }
            previousStrikes = currentStrikes;
        }
    }

    private void HandleQuotaUpdated(int processed, int totalQuota)
    {
        if (dayQuotaText != null)
        {
            dayQuotaText.text = $"QUOTA: {processed}/{totalQuota}";
        }
    }

    // --- PULSE ENGINE ---

    private void TriggerPulse(ref Coroutine tracker, TextMeshProUGUI text, Vector3 originalScale, Color flashColor, Color baseColor, float scaleMultiplier)
    {
        if (text == null) return;

        // If a pulse is already running, stop it and immediately reset properties before starting new pulse
        if (tracker != null)
        {
            StopCoroutine(tracker);
            text.transform.localScale = originalScale;
            text.color = baseColor;
        }

        tracker = StartCoroutine(SmoothPulseRoutine(text, originalScale, flashColor, baseColor, scaleMultiplier));
    }

    private IEnumerator SmoothPulseRoutine(TextMeshProUGUI targetText, Vector3 baseScale, Color flashColor, Color baseColor, float scaleMultiplier)
    {
        float duration = 0.25f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Sine wave calculation for pulse (0 -> 1 -> 0)
            float pulseFactor = Mathf.Sin(t * Mathf.PI);

            // Scale up and return smoothly to original base scale
            targetText.transform.localScale = Vector3.Lerp(baseScale, baseScale * scaleMultiplier, pulseFactor);

            // Flash glow color and smoothstep back to baseline
            targetText.color = Color.Lerp(flashColor, baseColor, Mathf.SmoothStep(0f, 1f, t));

            yield return null;
        }

        // Hard reset to ensure exact scale and color parity
        targetText.transform.localScale = baseScale;
        targetText.color = baseColor;
    }

    private Color GetHeatColor(float mult)
    {
        float t = Mathf.Clamp01((mult - 1f) / (maxMultiplier - 1f));
        if (t < 0.5f)
        {
            return Color.Lerp(multBaseColor, multMidColor, t * 2f);
        }
        return Color.Lerp(multMidColor, multMaxColor, (t - 0.5f) * 2f);
    }

    // --- SCREEN SHAKE & AUDIO ---

    public void TriggerScreenShake()
    {
        if (shakeRoutine != null) StopCoroutine(shakeRoutine);
        shakeRoutine = StartCoroutine(ShakeRoutine());
    }

    private IEnumerator ShakeRoutine()
    {
        float elapsed = 0f;

        while (elapsed < errorShakeDuration)
        {
            Vector3 randomOffset = Random.insideUnitSphere * errorShakeMagnitude;

            if (uiPanelToShake != null)
                uiPanelToShake.anchoredPosition = originalPanelPos + (Vector3)randomOffset;

            if (mainCameraTransform != null)
                mainCameraTransform.localPosition = originalCameraPos + new Vector3(randomOffset.x * 0.008f, randomOffset.y * 0.008f, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (uiPanelToShake != null) uiPanelToShake.anchoredPosition = originalPanelPos;
        if (mainCameraTransform != null) mainCameraTransform.localPosition = originalCameraPos;
    }

    public void PlayButtonClickSound() => PlaySound(buttonClickSound);
    public void PlayErrorSound() => PlaySound(errorBuzzerSound);

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}