using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("System References")]
    [SerializeField] private RadioDecoderManager decoderManager;
    [SerializeField] private SurveillanceManager surveillanceManager;
    [SerializeField] private DocumentManager documentManager;
    [SerializeField] private GateControlManager gateControlManager;

    [Header("Gameplay State")]
    [SerializeField] private float nextVisitorDelay = 3.0f;
    private AlienProfile currentVisitor;
    private int score = 0;
    private bool lockHandled = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        StartCoroutine(StartFirstRoundRoutine());
    }

    private IEnumerator StartFirstRoundRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        SpawnNextVisitor();
    }

    public void SpawnNextVisitor()
    {
        lockHandled = false;

        // 1. Generate new Alien Profile
        currentVisitor = AlienDataGenerator.Instance.GenerateNewProfile();

        // 2. Send transmission to Radio Decoder
        if (decoderManager != null)
        {
            string transmissionHeader = $"TRANSMISSION: {currentVisitor.cargoItem.ToUpper()}";
            decoderManager.SetIncomingTransmission(
                transmissionHeader,
                currentVisitor.targetFrequency,
                currentVisitor.targetPhase
            );
        }

        // 3. Update Surveillance & Document Windows
        if (surveillanceManager != null) surveillanceManager.DisplayAlienTransmission(currentVisitor, false);
        if (documentManager != null) documentManager.PopulateDocument(currentVisitor);

        // 4. Initialize Gate Controls
        if (gateControlManager != null) gateControlManager.InitializeGateControl(OnPlayerDecisionMade);

        Debug.Log($"[GameManager] New Visitor: {currentVisitor.alienName} | Threat: {currentVisitor.cargoThreat}");
    }

    private void Update()
    {
        // When signal locks on Decoder, unmask true cargo dialogue in Surveillance
        if (decoderManager != null && decoderManager.IsSignalLocked)
        {
            OnSignalDecoderLocked();
        }
    }

    private void OnSignalDecoderLocked()
    {
        if (lockHandled) return;
        lockHandled = true;

        // Display full unmasked dialogue in surveillance
        if (surveillanceManager != null && currentVisitor != null)
        {
            surveillanceManager.DisplayAlienTransmission(currentVisitor, true);
        }
    }

    private void OnPlayerDecisionMade(bool approved)
    {
        bool wasCorrect = false;

        // Evaluation Logic: Safe aliens approved; Hazards & Unknowns denied
        if (currentVisitor.cargoThreat == ThreatCategory.Safe)
        {
            wasCorrect = approved;
        }
        else
        {
            wasCorrect = !approved;
        }

        if (wasCorrect)
        {
            score += 100;
            Debug.Log($"<color=green>CORRECT DECISION!</color> Score: {score}");
        }
        else
        {
            score -= 50;
            Debug.Log($"<color=red>INCORRECT DECISION!</color> Penalty applied. Score: {score}");
        }

        StartCoroutine(NextVisitorRoutine());
    }

    private IEnumerator NextVisitorRoutine()
    {
        yield return new WaitForSeconds(nextVisitorDelay);
        SpawnNextVisitor();
    }
}