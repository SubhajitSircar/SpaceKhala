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
            targetWindow = transform.parent as RectTransform;

        if (parentCanvas == null)
            parentCanvas = GetComponentInParent<Canvas>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Bring clicked window to the front layer of the desktop background
        targetWindow.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Divide delta by scaleFactor so dragging remains pixel-accurate at any screen resolution
        targetWindow.anchoredPosition += eventData.delta / parentCanvas.scaleFactor;
    }
}