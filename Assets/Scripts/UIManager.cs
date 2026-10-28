using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private GameObject pauseScreen;

    public GameObject winScreen;
    public GameObject loseScreen;

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
            pauseScreen.SetActive(true);
            Pause();
        }
        else
        {
            pauseScreen.SetActive(false);
            Unpause();
        }
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0;
    }

    public void Unpause()
    {
        isPaused = false;
        Time.timeScale = currentTimeScale;
    }

    public void ReplayButton()
    {
        SceneManager.LoadScene(1);
    }
}