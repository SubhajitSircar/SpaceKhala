using UnityEngine;
using UnityEngine.SceneManagement;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }

    [Header("Persistent Progression")]
    public int currentDay = 1;
    public int maxDays = 7;
    public int totalScore = 0;
    public int totalStrikes = 0;
    public int visitorsPerDay = 10;

    [Header("Scene Names")]
    public string terminalSceneName = "ComputerInterface";
    public string overworldSceneName = "SampleScene";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveShiftResults(int finalScore, int finalStrikes)
    {
        totalScore = finalScore;
        totalStrikes = finalStrikes;
        currentDay++;
    }

    public bool IsCampaignComplete()
    {
        return currentDay > maxDays;
    }

    public void LoadOverworldScene()
    {
        SceneManager.LoadScene(overworldSceneName);
    }

    public void LoadTerminalScene()
    {
        SceneManager.LoadScene(terminalSceneName);
    }

    public void ResetCampaign()
    {
        currentDay = 1;
        totalScore = 0;
        totalStrikes = 0;
    }
}