using UnityEngine;

public class ButtonHandler : MonoBehaviour
{
    public void PlayClickButtonSound()
    {
        AudioManager.Instance.clickButtonSound.Play();
    }
}