using UnityEngine;

public class DesktopWindowManager : MonoBehaviour
{
    // Opens a window and brings it to the top rendering layer
    public void OpenWindow(GameObject window)
    {
        window.SetActive(true);
        window.transform.SetAsLastSibling();
    }

    // Closes a window
    public void CloseWindow(GameObject window)
    {
        window.SetActive(false);
    }

    // Toggles window state
    public void ToggleWindow(GameObject window)
    {
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