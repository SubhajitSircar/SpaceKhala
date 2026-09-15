using UnityEngine;
using UnityEngine.EventSystems;

public class UIRotatableKnob : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    [Header("Rotation Bounds")]
    [SerializeField] private float minAngle = -140f; // Far left position
    [SerializeField] private float maxAngle = 140f;  // Far right position

    [Header("Output Value (0.0 to 1.0)")]
    [Range(0f, 1f)] public float CurrentValue = 0.5f;

    private RectTransform rectTransform;
    private Canvas parentCanvas;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
        UpdateKnobRotation();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        ProcessRotation(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        ProcessRotation(eventData);
    }

    private void ProcessRotation(PointerEventData eventData)
    {
        if (rectTransform == null) return;

        Camera cam = (parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            ? parentCanvas.worldCamera
            : null;

        Vector2 knobCenter = RectTransformUtility.WorldToScreenPoint(cam, rectTransform.position);
        Vector2 dir = eventData.position - knobCenter;

        if (dir.sqrMagnitude < 0.001f) return;

        // Calculate signed angle relative to UP (12 o'clock = 0 deg)
        float angle = Vector2.SignedAngle(Vector2.up, dir);

        // Clamp rotation between min and max angles
        float clampedAngle = Mathf.Clamp(angle, minAngle, maxAngle);

        // Map clamped angle to normalized 0.0 - 1.0 value
        CurrentValue = Mathf.InverseLerp(minAngle, maxAngle, clampedAngle);

        UpdateKnobRotation();
    }

    private void UpdateKnobRotation()
    {
        if (rectTransform == null) return;
        float currentAngle = Mathf.Lerp(minAngle, maxAngle, CurrentValue);
        rectTransform.localEulerAngles = new Vector3(0, 0, currentAngle);
    }

    // Call from code to programmatically set dial position
    public void SetValue(float value)
    {
        CurrentValue = Mathf.Clamp01(value);
        UpdateKnobRotation();
    }
}