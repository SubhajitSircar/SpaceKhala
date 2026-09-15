using UnityEngine;

public class DesktopWindowManager : MonoBehaviour
{
    public void OpenWindow(GameObject window)
    {
        if (window == null) return;
        window.SetActive(true);
        window.transform.SetAsLastSibling();
    }

    public void CloseWindow(GameObject window)
    {
        if (window == null) return;
        window.SetActive(false);
    }

    public void ToggleWindow(GameObject window)
    {
        if (window == null) return;
        if (window.activeSelf)
        {
            CloseWindow(window);
        }
        else
        {
            OpenWindow(window);
        }
    }
}