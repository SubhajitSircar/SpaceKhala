using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShiftSummaryUI : MonoBehaviour
{
    [Header("UI Panel Reference")]
    [SerializeField] private GameObject summaryPanel;

    [Header("Text Components")]
    [SerializeField] private TextMeshProUGUI headerTitleText;
    [SerializeField] private TextMeshProUGUI gradeText;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI correctDecisionsText;
    [SerializeField] private TextMeshProUGUI smugglersCaughtText;
    [SerializeField] private TextMeshProUGUI strikesIncurredText;

    [Header("Buttons")]
    [SerializeField] private Button nextShiftButton;

    private void OnEnable()
    {
        GameManager.OnShiftCompleted += ShowShiftCompletedSummary;
        GameManager.OnGameOverTriggered += ShowGameOverSummary;
    }

    private void OnDisable()
    {
        GameManager.OnShiftCompleted -= ShowShiftCompletedSummary;
        GameManager.OnGameOverTriggered -= ShowGameOverSummary;
    }

    private void Start()
    {
        if (summaryPanel != null) summaryPanel.SetActive(false);

        if (nextShiftButton != null)
        {
            nextShiftButton.onClick.AddListener(OnNextShiftButtonPressed);
        }
    }

    private void ShowShiftCompletedSummary()
    {
        if (summaryPanel == null) return;

        if (headerTitleText != null)
        {
            headerTitleText.text = "SHIFT COMPLETED";
            headerTitleText.color = new Color(0f, 1f, 0.4f); // Green
        }

        PopulateMetrics(isGameOver: false);
        summaryPanel.SetActive(true);
    }

    private void ShowGameOverSummary()
    {
        if (summaryPanel == null) return;

        if (headerTitleText != null)
        {
            headerTitleText.text = "STATION LOCKDOWN";
            headerTitleText.color = new Color(1f, 0.2f, 0.2f); // Red
        }

        PopulateMetrics(isGameOver: true);
        summaryPanel.SetActive(true);
    }

    private void PopulateMetrics(bool isGameOver)
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        float accuracyRatio = gm.ProcessedVisitorsCount > 0
            ? (float)gm.CorrectDecisionsCount / gm.ProcessedVisitorsCount
            : 0f;

        if (finalScoreText != null)
            finalScoreText.text = $"FINAL SCORE: {gm.Score:D6}";

        if (correctDecisionsText != null)
            correctDecisionsText.text = $"ACCURACY: {gm.CorrectDecisionsCount} / {gm.ProcessedVisitorsCount} ({accuracyRatio * 100f:F0}%)";

        if (smugglersCaughtText != null)
            smugglersCaughtText.text = $"SMUGGLERS INTERCEPTED: {gm.CaughtSmugglersCount}";

        if (strikesIncurredText != null)
            strikesIncurredText.text = $"STRIKES INCURRED: {gm.CurrentStrikes} / 3";

        if (gradeText != null)
        {
            string grade = EvaluateGrade(accuracyRatio, gm.CurrentStrikes, isGameOver, out Color gradeColor);
            gradeText.text = $"GRADE: {grade}";
            gradeText.color = gradeColor;
        }
    }

    private string EvaluateGrade(float accuracy, int strikes, bool isGameOver, out Color color)
    {
        if (isGameOver || strikes >= 3)
        {
            color = new Color(1f, 0.2f, 0.2f); // Red
            return "F";
        }

        if (accuracy >= 1.0f && strikes == 0)
        {
            color = new Color(1f, 0.84f, 0f); // Gold
            return "S";
        }
        if (accuracy >= 0.9f && strikes <= 1)
        {
            color = new Color(0.2f, 0.9f, 0.3f); // Green
            return "A";
        }
        if (accuracy >= 0.8f && strikes <= 1)
        {
            color = new Color(0.3f, 0.7f, 1f); // Blue
            return "B";
        }
        if (accuracy >= 0.7f && strikes <= 2)
        {
            color = new Color(1f, 0.6f, 0f); // Orange
            return "C";
        }

        color = new Color(1f, 0.2f, 0.2f); // Red
        return "F";
    }

    private void OnNextShiftButtonPressed()
    {
        if (summaryPanel != null) summaryPanel.SetActive(false);

        var gm = GameManager.Instance;
        var dataManager = GameDataManager.Instance;

        if (gm != null && dataManager != null)
        {
            // 1. Save the day's points and strikes permanently
            dataManager.SaveShiftResults(gm.Score, gm.CurrentStrikes);

            // 2. Check if the player lost or won the campaign
            if (gm.IsGameOver)
            {
                Debug.Log("Game Over! Restarting Campaign...");
                dataManager.ResetCampaign();
                dataManager.LoadOverworldScene();
            }
            else if (dataManager.IsCampaignComplete())
            {
                Debug.Log("7 Days Survived! You Win!");
                // You can load a Victory Scene here later. For now, reset and go back.
                dataManager.ResetCampaign();
                dataManager.LoadOverworldScene();
            }
            else
            {
                // 3. Just a normal shift finished, return to the Overworld for the next day
                dataManager.LoadOverworldScene();
            }
        }
        else
        {
            Debug.LogError("Missing GameManager or GameDataManager!");
        }
    }
}