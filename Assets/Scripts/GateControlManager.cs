using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GateControlManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Image statusBackground;
    [SerializeField] private Button approveButton;
    [SerializeField] private Button denyButton;

    [Header("Color Presets")]
    [SerializeField] private Color awaitingColor = new Color(1f, 0.8f, 0f); // Amber
    [SerializeField] private Color approvedColor = new Color(0f, 1f, 0.4f); // Neon Green
    [SerializeField] private Color deniedColor = new Color(1f, 0.2f, 0.2f);   // Bright Red

    private System.Action<bool> onDecisionMade;

    private void Awake()
    {
        if (approveButton != null) approveButton.onClick.AddListener(() => MakeDecision(true));
        if (denyButton != null) denyButton.onClick.AddListener(() => MakeDecision(false));
    }

    public void InitializeGateControl(System.Action<bool> decisionCallback)
    {
        onDecisionMade = decisionCallback;
        SetStatus("STATUS: AWAITING DECISION", awaitingColor);
        SetButtonsInteractable(true);
    }

    public void SetStatus(string message, Color textColor)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = textColor;
        }
    }

    public void SetButtonsInteractable(bool interactable)
    {
        if (approveButton != null) approveButton.interactable = interactable;
        if (denyButton != null) denyButton.interactable = interactable;
    }

    private void MakeDecision(bool isApproved)
    {
        SetButtonsInteractable(false);

        if (isApproved)
        {
            SetStatus("DECISION: APPROVED - GATE OPENING", approvedColor);
        }
        else
        {
            SetStatus("DECISION: DENIED - ACCESS BLOCKED", deniedColor);
        }

        onDecisionMade?.Invoke(isApproved);
    }
}