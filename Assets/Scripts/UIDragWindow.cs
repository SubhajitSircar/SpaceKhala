using UnityEngine;
using UnityEngine.EventSystems;

public class UIDragWindow : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    [Tooltip("The parent window frame to move. If unassigned, automatically grabs parent transform.")]
    [SerializeField] private RectTransform targetWindow;
    [SerializeField] private Canvas parentCanvas;

    private void Awake()
    {
        if (targetWindow == null)
            targetWindow = transform.parent as RectTransform ?? GetComponent<RectTransform>();

        if (parentCanvas == null)
            parentCanvas = GetComponentInParent<Canvas>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (targetWindow != null)
            targetWindow.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (targetWindow == null) return;
        float scale = (parentCanvas != null) ? parentCanvas.scaleFactor : 1f;
        targetWindow.anchoredPosition += eventData.delta / scale;
    }
}