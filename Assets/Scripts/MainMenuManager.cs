using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip clickSound;

    public GameObject howToPlayPanel;
    public GameObject menuPlayer; // NEW: Reference to the astronaut

    public void OpenHowToPlay()
    {
        PlayClickSound();
        howToPlayPanel.SetActive(true);

        // NEW: Hide the astronaut so they don't block the text
        if (menuPlayer != null) menuPlayer.SetActive(false);
    }

    public void CloseHowToPlay()
    {
        PlayClickSound();
        howToPlayPanel.SetActive(false);

        // NEW: Bring the astronaut back
        if (menuPlayer != null) menuPlayer.SetActive(true);
    }

    public void PlayGame() { StartCoroutine(PlaySoundAndLoad()); }

    IEnumerator PlaySoundAndLoad()
    {
        PlayClickSound();
        yield return new WaitForSeconds(0.3f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame()
    {
        PlayClickSound();

        // This prints a message to your console so you know the button works
        Debug.Log("Quit Game Button Pressed!");

        // This closes the actual built game
        Application.Quit();

        // This stops the play mode inside the Unity Editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
            audioSource.PlayOneShot(clickSound);
    }
}