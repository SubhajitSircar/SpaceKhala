using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RadioDecoderManager : MonoBehaviour
{
    [Header("Dial Inputs")]
    [SerializeField] private UIRotatableKnob frequencyKnob;
    [SerializeField] private UIRotatableKnob phaseKnob;

    [Header("UI Feedback Elements")]
    [SerializeField] private UISineWave targetWaveGraphic;
    [SerializeField] private UISineWave playerWaveGraphic;
    [SerializeField] private TextMeshProUGUI decodedOutputText;
    [SerializeField] private Slider lockProgressBar;
    [SerializeField] private Image lockStatusLamp;

    [Header("Wave Visual Settings")]
    [SerializeField] private float minVisualFreq = 1f;
    [SerializeField] private float maxVisualFreq = 10f;
    [SerializeField] private float minVisualPhase = 0f;
    [SerializeField] private float maxVisualPhase = 10f;

    [Header("Audio Feedback")]
    [SerializeField] private AudioSource radioStaticAudio;
    [SerializeField] private float minPitch = 0.4f;
    [SerializeField] private float maxPitch = 1.8f;

    [Header("Decoder Settings")]
    [SerializeField] private float matchTolerance = 0.15f;
    [SerializeField] private float lockHoldTime = 1.2f;

    private float targetFrequency;
    private float targetPhase;
    private string originalMessage = "";
    private float currentLockTimer = 0f;

    public bool IsSignalLocked { get; private set; } = false;
    public string DecodedMessage => IsSignalLocked ? originalMessage : "";

    private const string ScramblePool = "XYZ#$&@%?!=+*0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public void SetIncomingTransmission(string message, float reqFreq, float reqPhase)
    {
        originalMessage = message;
        targetFrequency = reqFreq;
        targetPhase = reqPhase;
        IsSignalLocked = false;
        currentLockTimer = 0f;

        if (lockProgressBar) lockProgressBar.value = 0f;
        if (lockStatusLamp) lockStatusLamp.color = new Color(0.3f, 0.1f, 0.1f);

        if (targetWaveGraphic)
        {
            targetWaveGraphic.frequency = Mathf.Lerp(minVisualFreq, maxVisualFreq, targetFrequency);
            targetWaveGraphic.phaseOffset = Mathf.Lerp(minVisualPhase, maxVisualPhase, targetPhase);
        }

        if (radioStaticAudio && !radioStaticAudio.isPlaying)
            radioStaticAudio.Play();
    }

    private void Update()
    {
        if (IsSignalLocked) return;

        float currentAccuracy = CalculateAccuracy();

        UpdateWaveformVisual();
        UpdateAudioPitch();
        UpdateTextScramble(currentAccuracy);

        if (currentAccuracy >= (1f - matchTolerance))
        {
            currentLockTimer += Time.deltaTime;
            if (lockProgressBar) lockProgressBar.value = currentLockTimer / lockHoldTime;

            if (currentLockTimer >= lockHoldTime)
            {
                LockSignal();
            }
        }
        else
        {
            currentLockTimer = Mathf.Max(0f, currentLockTimer - (Time.deltaTime * 1.5f));
            if (lockProgressBar) lockProgressBar.value = currentLockTimer / lockHoldTime;
        }
    }

    private float CalculateAccuracy()
    {
        if (frequencyKnob == null || phaseKnob == null) return 0f;
        float freqError = Mathf.Abs(frequencyKnob.CurrentValue - targetFrequency);
        float phaseError = Mathf.Abs(phaseKnob.CurrentValue - targetPhase);
        return Mathf.Clamp01(1f - ((freqError + phaseError) / 2f));
    }

    private void UpdateWaveformVisual()
    {
        if (playerWaveGraphic && frequencyKnob && phaseKnob)
        {
            playerWaveGraphic.frequency = Mathf.Lerp(minVisualFreq, maxVisualFreq, frequencyKnob.CurrentValue);
            playerWaveGraphic.phaseOffset = Mathf.Lerp(minVisualPhase, maxVisualPhase, phaseKnob.CurrentValue);
        }
    }

    private void UpdateAudioPitch()
    {
        if (!radioStaticAudio || !frequencyKnob) return;
        float freqError = Mathf.Abs(frequencyKnob.CurrentValue - targetFrequency);
        radioStaticAudio.pitch = Mathf.Lerp(1.0f, maxPitch, freqError);
        radioStaticAudio.volume = Mathf.Lerp(0.3f, 1.0f, freqError);
    }

    private void UpdateTextScramble(float accuracy)
    {
        if (decodedOutputText == null || string.IsNullOrEmpty(originalMessage)) return;

        char[] chars = originalMessage.ToCharArray();
        int total = chars.Length;
        int revealedCount = Mathf.FloorToInt(total * accuracy);

        for (int i = 0; i < total; i++)
        {
            if (chars[i] == ' ' || chars[i] == '.' || chars[i] == ':') continue;

            if (i >= revealedCount)
            {
                chars[i] = ScramblePool[Random.Range(0, ScramblePool.Length)];
            }
        }

        decodedOutputText.text = new string(chars);
    }

    private void LockSignal()
    {
        IsSignalLocked = true;
        if (lockProgressBar) lockProgressBar.value = 1f;
        if (lockStatusLamp) lockStatusLamp.color = new Color(0.0f, 1.0f, 0.4f);
        if (decodedOutputText) decodedOutputText.text = originalMessage;

        if (radioStaticAudio) radioStaticAudio.Stop();
    }
}