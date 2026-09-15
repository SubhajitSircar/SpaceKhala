using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SurveillanceManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI alienInfoText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Transform waveformBarsContainer;

    [Header("Typewriter Settings")]
    [SerializeField] private float charactersPerSecond = 30f;

    [Header("Audio Feedback (Optional)")]
    [SerializeField] private AudioSource speechBeepAudio;

    private Image[] waveBars;
    private bool isSpeaking = false;
    private const string ScramblePool = "@#$&*!?!=+*$%";

    private void Awake()
    {
        if (waveformBarsContainer != null)
        {
            waveBars = waveformBarsContainer.GetComponentsInChildren<Image>();
        }
    }

    public void DisplayAlienTransmission(AlienProfile profile)
    {
        StopAllCoroutines();

        if (alienInfoText != null)
        {
            alienInfoText.text = $"ID: {profile.alienName} | ORIGIN: {profile.originPlanet} | RACE: {profile.race}";
        }

        string maskedDialogue = MaskCargoString(profile.dialogue, profile.cargoItem);
        StartCoroutine(TypewriterRoutine(maskedDialogue));
    }

    private string MaskCargoString(string fullDialogue, string rawCargo)
    {
        string garbled = "";
        for (int i = 0; i < rawCargo.Length; i++)
        {
            garbled += ScramblePool[Random.Range(0, ScramblePool.Length)];
        }
        return fullDialogue.Replace($"[{rawCargo}]", $"[{garbled}]");
    }

    private IEnumerator TypewriterRoutine(string textToType)
    {
        isSpeaking = true;
        dialogueText.text = "";
        float delay = 1f / charactersPerSecond;

        for (int i = 0; i <= textToType.Length; i++)
        {
            dialogueText.text = textToType.Substring(0, i);

            if (speechBeepAudio != null && i % 2 == 0)
            {
                speechBeepAudio.pitch = Random.Range(0.8f, 1.2f);
                speechBeepAudio.PlayOneShot(speechBeepAudio.clip);
            }

            yield return new WaitForSeconds(delay);
        }

        isSpeaking = false;
        ResetWaveformBars();
    }

    private void Update()
    {
        if (isSpeaking && waveBars != null && waveBars.Length > 0)
        {
            for (int i = 0; i < waveBars.Length; i++)
            {
                float noise = Mathf.PerlinNoise(Time.time * 12f, i * 0.4f);
                float targetScaleY = Mathf.Lerp(0.1f, 1.0f, noise);
                waveBars[i].rectTransform.localScale = new Vector3(1f, targetScaleY, 1f);
            }
        }
    }

    private void ResetWaveformBars()
    {
        if (waveBars == null) return;
        for (int i = 0; i < waveBars.Length; i++)
        {
            waveBars[i].rectTransform.localScale = new Vector3(1f, 0.1f, 1f);
        }
    }
}