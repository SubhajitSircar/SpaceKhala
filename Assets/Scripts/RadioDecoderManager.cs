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
    [SerializeField] private float matchTolerance = 0.15f; // Increased so it is easier to lock on
    [SerializeField] private float lockHoldTime = 1.2f;

    private float targetFrequency;
    private float targetPhase;
    private string originalMessage = "";
    private float currentLockTimer = 0f;

    public bool IsSignalLocked { get; private set; } = false;
    public string DecodedMessage => IsSignalLocked ? originalMessage : "";

    private const string ScramblePool = "XYZ#$&@%?!=+*0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    private void Start()
    {
        // Automatically give a target signal so you can test in Play Mode.
        // Once you build your Alien script, you can delete this Start method entirely!
        SetIncomingTransmission("ACCESS GRANTED", 0.7f, 0.3f);
    }

    public void SetIncomingTransmission(string message, float reqFreq, float reqPhase)
    {
        originalMessage = message;
        targetFrequency = reqFreq;
        targetPhase = reqPhase;
        IsSignalLocked = false;
        currentLockTimer = 0f;

        if (lockProgressBar) lockProgressBar.value = 0f;
        if (lockStatusLamp) lockStatusLamp.color = new Color(0.3f, 0.1f, 0.1f); // Dark Red

        // Map the 0-1 target values to the visual min/max ranges
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

        // Check if player is close enough to target
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
        float freqError = Mathf.Abs(frequencyKnob.CurrentValue - targetFrequency);
        float phaseError = Mathf.Abs(phaseKnob.CurrentValue - targetPhase);
        return Mathf.Clamp01(1f - ((freqError + phaseError) / 2f));
    }

    private void UpdateWaveformVisual()
    {
        // Feed the player's knob inputs into the wave graphic, mapped to visual ranges
        if (playerWaveGraphic)
        {
            playerWaveGraphic.frequency = Mathf.Lerp(minVisualFreq, maxVisualFreq, frequencyKnob.CurrentValue);
            playerWaveGraphic.phaseOffset = Mathf.Lerp(minVisualPhase, maxVisualPhase, phaseKnob.CurrentValue);
        }
    }

    private void UpdateAudioPitch()
    {
        if (!radioStaticAudio) return;
        float freqError = Mathf.Abs(frequencyKnob.CurrentValue - targetFrequency);
        radioStaticAudio.pitch = Mathf.Lerp(1.0f, maxPitch, freqError);
        radioStaticAudio.volume = Mathf.Lerp(0.3f, 1.0f, freqError);
    }

    private void UpdateTextScramble(float accuracy)
    {
        char[] chars = originalMessage.ToCharArray();
        int total = chars.Length;
        int revealedCount = Mathf.FloorToInt(total * accuracy);

        System.Random prng = new System.Random(1337);

        for (int i = 0; i < total; i++)
        {
            if (chars[i] == ' ' || chars[i] == '.' || chars[i] == ':') continue;

            if (i >= revealedCount)
            {
                chars[i] = ScramblePool[prng.Next(ScramblePool.Length)];
            }
        }

        decodedOutputText.text = new string(chars);
    }

    private void LockSignal()
    {
        IsSignalLocked = true;
        if (lockProgressBar) lockProgressBar.value = 1f;
        if (lockStatusLamp) lockStatusLamp.color = new Color(0.0f, 1.0f, 0.4f); // Neon Green
        decodedOutputText.text = originalMessage;

        if (radioStaticAudio) radioStaticAudio.Stop();
    }
}