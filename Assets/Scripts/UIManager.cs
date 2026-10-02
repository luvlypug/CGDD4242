using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [HideInInspector] public bool isPaused = false;
    [HideInInspector] public float currentTimeScale = 1;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void OnPause()
    {
        if (!isPaused)
        {
            Pause();
        }
        else
        {
            Unpause();
        }
    }

    void Pause()
    {
        isPaused = true;
        Time.timeScale = 0;
    }

    void Unpause()
    {
        isPaused = false;
        Time.timeScale = currentTimeScale;
    }
}