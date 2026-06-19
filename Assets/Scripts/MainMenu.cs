using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private AudioSource buttonAudioSource;
    [SerializeField] private AudioClip buttonClickSound;
    
    public void LoadLevel1()
    {
        PlayClickSound();
        SceneManager.LoadScene("Erstes Level");
    }

    public void LoadLevel2()
    {
        PlayClickSound();
        SceneManager.LoadScene("Zweites Level");
    }

    /*public void LoadLevel3()
    {
        PlayClickSound();
        SceneManager.LoadScene("Drittes Level");
    }*/

    public void QuitGame()
    {
        PlayClickSound();
        Application.Quit();
    }

    private void PlayClickSound()
    {
        if (buttonAudioSource != null && buttonClickSound != null)
        {
            buttonAudioSource.PlayOneShot(buttonClickSound);
        }
    }
}