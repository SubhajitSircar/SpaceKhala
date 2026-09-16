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
    [SerializeField] private AudioSource lockBeepAudioSource;
    [SerializeField] private AudioClip signalLockSFX;
    [SerializeField] private float minPitch = 0.5f;
    [SerializeField] private float maxPitch = 1.6f;

    [Header("Decoder Tuning Settings")]
    [Tooltip("Tolerance threshold (0.05 = tight precision required, 0.15 = easy tuning)")]
    [SerializeField] private float matchTolerance = 0.08f;
    [SerializeField] private float lockHoldTime = 1.0f;

    [Header("Color States for Status Lamp")]
    [SerializeField] private Color searchingColor = new Color(0.8f, 0.1f, 0.1f);
    [SerializeField] private Color tuningColor = new Color(0.9f, 0.7f, 0.1f);
    [SerializeField] private Color lockedColor = new Color(0.0f, 1.0f, 0.4f);

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

        // Reset UI Feedback
        if (lockProgressBar != null) lockProgressBar.value = 0f;
        if (lockStatusLamp != null) lockStatusLamp.color = searchingColor;
        if (decodedOutputText != null) decodedOutputText.text = "";

        // Update Target Waveform display
        if (targetWaveGraphic != null)
        {
            targetWaveGraphic.frequency = Mathf.Lerp(minVisualFreq, maxVisualFreq, targetFrequency);
            targetWaveGraphic.phaseOffset = Mathf.Lerp(minVisualPhase, maxVisualPhase, targetPhase);
        }

        // Play or restart static audio
        if (radioStaticAudio != null)
        {
            radioStaticAudio.volume = 1.0f;
            if (!radioStaticAudio.isPlaying) radioStaticAudio.Play();
        }

        // Force initial display to be completely garbled
        UpdateTextScramble(0f);
    }

    private void Update()
    {
        if (IsSignalLocked) return;

        float currentAccuracy = CalculateAccuracy();

        UpdateWaveformVisual();
        UpdateAudioFeedback(currentAccuracy);
        UpdateTextScramble(currentAccuracy);

        // Required accuracy threshold
        float requiredAccuracy = 1f - matchTolerance;

        if (currentAccuracy >= requiredAccuracy)
        {
            // Update hold timer
            currentLockTimer += Time.deltaTime;
            if (lockProgressBar != null) lockProgressBar.value = currentLockTimer / lockHoldTime;
            if (lockStatusLamp != null) lockStatusLamp.color = tuningColor;

            if (currentLockTimer >= lockHoldTime)
            {
                LockSignal();
            }
        }
        else
        {
            // Decays lock timer when off frequency
            currentLockTimer = Mathf.Max(0f, currentLockTimer - (Time.deltaTime * 2.0f));
            if (lockProgressBar != null) lockProgressBar.value = currentLockTimer / lockHoldTime;
            if (lockStatusLamp != null) lockStatusLamp.color = searchingColor;
        }
    }

    private float CalculateAccuracy()
    {
        if (frequencyKnob == null || phaseKnob == null) return 0f;

        float freqError = Mathf.Abs(frequencyKnob.CurrentValue - targetFrequency);
        float phaseError = Mathf.Abs(phaseKnob.CurrentValue - targetPhase);

        // Linear alignment value (0 to 1)
        float rawAccuracy = Mathf.Clamp01(1f - ((freqError + phaseError) / 2f));

        // Exponential curve: Keeps text garbled until player is very close to target frequency
        return Mathf.Pow(rawAccuracy, 3.5f);
    }

    private void UpdateWaveformVisual()
    {
        if (playerWaveGraphic != null && frequencyKnob != null && phaseKnob != null)
        {
            playerWaveGraphic.frequency = Mathf.Lerp(minVisualFreq, maxVisualFreq, frequencyKnob.CurrentValue);
            playerWaveGraphic.phaseOffset = Mathf.Lerp(minVisualPhase, maxVisualPhase, phaseKnob.CurrentValue);
        }
    }

    private void UpdateAudioFeedback(float accuracy)
    {
        if (radioStaticAudio == null || frequencyKnob == null) return;

        float freqError = Mathf.Abs(frequencyKnob.CurrentValue - targetFrequency);

        // Pitch shifts with tuning error
        radioStaticAudio.pitch = Mathf.Lerp(minPitch, maxPitch, freqError);

        // Static quiets down as accuracy increases (clearer signal)
        radioStaticAudio.volume = Mathf.Lerp(1.0f, 0.15f, accuracy);
    }

    private void UpdateTextScramble(float accuracy)
    {
        if (decodedOutputText == null || string.IsNullOrEmpty(originalMessage)) return;

        char[] chars = originalMessage.ToCharArray();
        int total = chars.Length;
        int revealedCount = Mathf.FloorToInt(total * accuracy);

        for (int i = 0; i < total; i++)
        {
            // Preserve spacing and punctuation
            if (chars[i] == ' ' || chars[i] == '.' || chars[i] == ':' || chars[i] == '-') continue;

            if (i >= revealedCount)
            {
                // Assign pseudo-random character from pool
                int randomIndex = (i + (int)(Time.time * 20f)) % ScramblePool.Length;
                chars[i] = ScramblePool[randomIndex];
            }
        }

        decodedOutputText.text = new string(chars);
    }

    private void LockSignal()
    {
        IsSignalLocked = true;

        if (lockProgressBar != null) lockProgressBar.value = 1f;
        if (lockStatusLamp != null) lockStatusLamp.color = lockedColor;
        if (decodedOutputText != null) decodedOutputText.text = originalMessage;

        // Quiet or stop static upon lock
        if (radioStaticAudio != null) radioStaticAudio.Stop();

        // Optional lock chirp sound
        if (lockBeepAudioSource != null && signalLockSFX != null)
        {
            lockBeepAudioSource.PlayOneShot(signalLockSFX);
        }
    }
}