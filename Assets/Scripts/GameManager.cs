using UnityEngine;
using System.Collections;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("System References")]
    [SerializeField] private RadioDecoderManager decoderManager;
    [SerializeField] private SurveillanceManager surveillanceManager;
    [SerializeField] private DocumentManager documentManager;
    [SerializeField] private GateControlManager gateControlManager;

    [Header("Shift & Quota Settings")]
    [SerializeField] private int visitorsPerShift = 10;
    [SerializeField] private float nextVisitorDelay = 2.5f;

    [Header("Scoring & Multiplier Scaling")]
    [SerializeField] private int baseCorrectDecisionPoints = 100;
    [SerializeField] private float multiplierStepPerStreak = 0.2f;
    [SerializeField] private float maxMultiplier = 5.0f;

    [Header("Penalties")]
    [SerializeField] private int safeDenialPenalty = 50;
    [SerializeField] private int hazardApprovalPenalty = 150;
    [SerializeField] private int unknownApprovalPenalty = 100;

    [Header("Security & Game Over State")]
    [SerializeField] private int maxStrikes = 3;

    // Gameplay State Properties
    public int Score { get; private set; } = 0;
    public int CurrentStrikes { get; private set; } = 0;
    public int ProcessedVisitorsCount { get; private set; } = 0;
    public int StreakCount { get; private set; } = 0;
    public float CurrentMultiplier { get; private set; } = 1.0f;
    public bool IsGameOver { get; private set; } = false;
    public bool IsShiftComplete { get; private set; } = false;
    public AlienProfile CurrentVisitor => currentVisitor;

    // Shift Summary Tracking Properties
    public int CaughtSmugglersCount { get; private set; } = 0;
    public int CorrectDecisionsCount { get; private set; } = 0;

    // Events for UI & Audio integration
    public static event Action<int, float> OnScoreUpdated;       // (score, multiplier)
    public static event Action<int, int> OnStrikesUpdated;      // (currentStrikes, maxStrikes)
    public static event Action<int, int> OnQuotaUpdated;        // (processed, targetQuota)
    public static event Action OnGameOverTriggered;
    public static event Action OnShiftCompleted;

    private AlienProfile currentVisitor;
    private bool lockHandled = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        StartCoroutine(StartFirstRoundRoutine());
    }

    private IEnumerator StartFirstRoundRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        ResetGameSession();
        SpawnNextVisitor();
    }

    public void ResetGameSession()
    {
        // Pull persistent data from GameDataManager if it exists
        if (GameDataManager.Instance != null)
        {
            Score = GameDataManager.Instance.totalScore;
            CurrentStrikes = GameDataManager.Instance.totalStrikes;
        }
        else
        {
            Score = 0;
            CurrentStrikes = 0;
        }

        // These ALWAYS reset every shift
        ProcessedVisitorsCount = 0;
        StreakCount = 0;
        CurrentMultiplier = 1.0f;
        CaughtSmugglersCount = 0;
        CorrectDecisionsCount = 0;
        IsGameOver = false;
        IsShiftComplete = false;

        OnScoreUpdated?.Invoke(Score, CurrentMultiplier);
        OnStrikesUpdated?.Invoke(CurrentStrikes, maxStrikes);
        OnQuotaUpdated?.Invoke(ProcessedVisitorsCount, visitorsPerShift);
    }

    public void SpawnNextVisitor()
    {
        if (IsGameOver || IsShiftComplete) return;

        lockHandled = false;

        if (AlienDataGenerator.Instance != null)
        {
            currentVisitor = AlienDataGenerator.Instance.GenerateNewProfile();
        }
        else
        {
            Debug.LogError("[GameManager] AlienDataGenerator instance is missing in scene!");
            return;
        }

        if (decoderManager != null)
        {
            string transmissionHeader = $"TRANSMISSION: {currentVisitor.cargoItem.ToUpper()}";
            decoderManager.SetIncomingTransmission(
                transmissionHeader,
                currentVisitor.targetFrequency,
                currentVisitor.targetPhase
            );
        }

        if (surveillanceManager != null)
        {
            surveillanceManager.DisplayAlienTransmission(currentVisitor, false);
        }

        if (documentManager != null)
        {
            documentManager.PopulateDocument(currentVisitor);
        }

        if (gateControlManager != null)
        {
            gateControlManager.InitializeGateControl(OnPlayerDecisionMade);
        }

        Debug.Log($"[GameManager] Visitor #{ProcessedVisitorsCount + 1}: {currentVisitor.alienName} | Threat: {currentVisitor.cargoThreat}");
    }

    private void Update()
    {
        if (IsGameOver || IsShiftComplete) return;

        if (decoderManager != null && decoderManager.IsSignalLocked)
        {
            OnSignalDecoderLocked();
        }
    }

    private void OnSignalDecoderLocked()
    {
        if (lockHandled) return;
        lockHandled = true;

        if (surveillanceManager != null && currentVisitor != null)
        {
            surveillanceManager.DisplayAlienTransmission(currentVisitor, true);
        }
    }

    private void OnPlayerDecisionMade(bool approved)
    {
        if (IsGameOver || IsShiftComplete) return;

        bool shouldApprove = EvaluateCorrectDecision();
        bool wasCorrect = (approved == shouldApprove);

        if (wasCorrect)
        {
            StreakCount++;
            CorrectDecisionsCount++;

            // Track intercepted smugglers/hazards (player correctly DENIED a hazard)
            if (!approved && currentVisitor.cargoThreat == ThreatCategory.Hazard)
            {
                CaughtSmugglersCount++;
            }

            CalculateMultiplier();

            int pointsEarned = Mathf.RoundToInt(baseCorrectDecisionPoints * CurrentMultiplier);
            Score += pointsEarned;

            string actionString = approved ? "APPROVED" : "DENIED";
            RegisterSuccess($"Correctly {actionString} vessel payload! +{pointsEarned} PTS");
        }
        else
        {
            ResetStreak();

            if (currentVisitor.cargoThreat == ThreatCategory.Hazard && approved)
            {
                DeductScore(hazardApprovalPenalty);
                AddStrike("SECURITY BREACH: ALLOWED CONTRABAND ONTO STATION!");
            }
            else if (currentVisitor.cargoThreat == ThreatCategory.Safe && !approved)
            {
                DeductScore(safeDenialPenalty);
                RegisterMistake("Mistakenly denied a fully legitimate civilian ship.");
            }
            else
            {
                DeductScore(unknownApprovalPenalty);
                RegisterMistake("Violated Class-B cargo signal verification protocols.");
            }
        }

        ProcessedVisitorsCount++;
        OnQuotaUpdated?.Invoke(ProcessedVisitorsCount, visitorsPerShift);

        if (ProcessedVisitorsCount >= visitorsPerShift && CurrentStrikes < maxStrikes)
        {
            CompleteShift();
            return;
        }

        if (!IsGameOver)
        {
            StartCoroutine(NextVisitorRoutine());
        }
    }

    private bool EvaluateCorrectDecision()
    {
        if (currentVisitor == null) return true;

        bool isSignalLocked = (decoderManager != null && decoderManager.IsSignalLocked);

        switch (currentVisitor.cargoThreat)
        {
            case ThreatCategory.Safe:
                // Clean Civilian Vessel (Class-A) -> Approve
                return true;

            case ThreatCategory.Hazard:
                // Contraband / Smuggler (Class-A forged or Class-C declared) -> Always Deny
                return false;

            case ThreatCategory.Unknown:
                // Class-B Restricted Cargo: Must decode radio signal to inspect
                if (isSignalLocked)
                {
                    // Signal lock reveals if hidden contents are safe
                    bool isDecodedSafe = ((currentVisitor.alienName.GetHashCode() & 1) == 0);
                    return isDecodedSafe;
                }
                else
                {
                    // Approving unverified Class-B cargo without radio scan is dangerous -> Deny
                    return false;
                }

            default:
                return true;
        }
    }

    private void CalculateMultiplier()
    {
        CurrentMultiplier = Mathf.Min(maxMultiplier, 1.0f + ((StreakCount - 1) * multiplierStepPerStreak));
    }

    private void RegisterSuccess(string reason)
    {
        OnScoreUpdated?.Invoke(Score, CurrentMultiplier);
        Debug.Log($"<color=green>SUCCESS:</color> {reason} | Multiplier: {CurrentMultiplier:F1}x | Score: {Score}");
    }

    private void RegisterMistake(string reason)
    {
        OnScoreUpdated?.Invoke(Score, CurrentMultiplier);
        Debug.Log($"<color=yellow>WARNING:</color> {reason} | Streak Reset | Score: {Score}");
    }

    private void AddStrike(string reason)
    {
        CurrentStrikes++;
        OnStrikesUpdated?.Invoke(CurrentStrikes, maxStrikes);
        OnScoreUpdated?.Invoke(Score, CurrentMultiplier);

        Debug.Log($"<color=red>SECURITY BREACH ({CurrentStrikes}/{maxStrikes}):</color> {reason}");

        if (CurrentStrikes >= maxStrikes)
        {
            TriggerGameOver();
        }
    }

    private void ResetStreak()
    {
        StreakCount = 0;
        CurrentMultiplier = 1.0f;
    }

    private void DeductScore(int amount)
    {
        Score = Mathf.Max(0, Score - amount);
    }

    private void TriggerGameOver()
    {
        IsGameOver = true;
        Debug.Log("<color=red>=== STATION LOCKDOWN TRIGGERED: GAME OVER ===</color>");
        OnGameOverTriggered?.Invoke();
    }

    private void CompleteShift()
    {
        IsShiftComplete = true;
        Debug.Log($"<color=cyan>=== SHIFT COMPLETED! Final Score: {Score} ===</color>");
        OnShiftCompleted?.Invoke();
    }

    private IEnumerator NextVisitorRoutine()
    {
        yield return new WaitForSeconds(nextVisitorDelay);
        SpawnNextVisitor();
    }
}