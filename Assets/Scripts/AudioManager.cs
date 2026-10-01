using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public AudioSource clickButtonSound;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StopAllAudio()
    {
        foreach (Transform child in transform)
        {
            if (child.TryGetComponent<AudioSource>(out AudioSource audioSource))
            {
                if (child.gameObject.name != "clickButtonSound") // Add sounds here that you want to continue playing even when all audios are stopped
                {
                    audioSource.Stop();
                }
            }
        }
    }
}