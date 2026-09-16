using UnityEngine;

public class StationInteract : MonoBehaviour
{
    public GameObject promptUI; // The 'E' button prompt above the computer

    private bool isPlayerNear = false;

    void Start()
    {
        if (promptUI != null) promptUI.SetActive(false);
    }

    void Update()
    {
        // When player is near and presses E, load the Terminal
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            if (GameDataManager.Instance != null)
            {
                GameDataManager.Instance.LoadTerminalScene();
            }
            else
            {
                Debug.LogError("GameDataManager is missing! Make sure it's in the scene.");
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            if (promptUI != null) promptUI.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            if (promptUI != null) promptUI.SetActive(false);
        }
    }
}