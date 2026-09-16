using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject pausePanel;
    private bool isPaused = false;

    void Start()
    {
        // Ensure the menu is hidden when the level starts
        pausePanel.SetActive(false);
    }

    void Update()
    {
        // Press Escape to toggle pause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f; // This freezes all game physics and animations
        isPaused = true;
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f; // This unfreezes the game
        isPaused = false;
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f; // CRITICAL: Unfreeze time before leaving, or the main menu will be frozen!
        SceneManager.LoadScene(0); // 0 is your MainMenu in the Build Settings
    }

 
}