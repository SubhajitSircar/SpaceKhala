using UnityEngine;

public class StationInteract : MonoBehaviour
{
    public GameObject terminalUI;       
    public PlayerMovement playerScript; 
    public GameObject promptUI;         

    private bool isPlayerNear = false;
    private bool isTerminalOpen = false;

    void Start()
    {
        
        if (promptUI != null)
        {
            promptUI.SetActive(false);
        }
    }

    void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E) && !isTerminalOpen)
        {
            ToggleTerminal(true);
        }
        else if (isTerminalOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleTerminal(false);
        }
    }

    void ToggleTerminal(bool state)
    {
        isTerminalOpen = state;
        terminalUI.SetActive(state);
        playerScript.canMove = !state;


        promptUI.SetActive(!state);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            promptUI.SetActive(true); 
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            promptUI.SetActive(false); 
        }
    }
}