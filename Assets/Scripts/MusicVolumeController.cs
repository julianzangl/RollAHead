using UnityEngine;
using UnityEngine.UI;

public class MusicVolumeController : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource musicAudioSource;

    [Header("UI")]
    [SerializeField] private Slider volumeSlider;

    private const string MusicVolumeKey = "MusicVolume";

    private void Start()
    {
        float savedVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 0.35f);

        volumeSlider.value = savedVolume;
        musicAudioSource.volume = savedVolume;

        volumeSlider.onValueChanged.AddListener(ChangeMusicVolume);
    }

    private void ChangeMusicVolume(float volume)
    {
        musicAudioSource.volume = volume;

        PlayerPrefs.SetFloat(MusicVolumeKey, volume);
        PlayerPrefs.Save();
    }
}