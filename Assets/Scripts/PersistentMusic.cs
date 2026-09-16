using UnityEngine;

public class PersistentMusic : MonoBehaviour
{
    // This static variable holds the single instance of our music player
    private static PersistentMusic instance;

    void Awake()
    {
        // If an instance already exists and it is NOT this one, destroy this duplicate
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Otherwise, this is the original instance. Keep it, and don't destroy it when loading scenes.
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}