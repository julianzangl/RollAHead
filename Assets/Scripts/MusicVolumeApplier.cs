using UnityEngine;

public class MusicVolumeApplier : MonoBehaviour
{
    [SerializeField] private AudioSource musicAudioSource;

    private const string MusicVolumeKey = "MusicVolume";
    private const float DefaultVolume = 0.35f;

    private void Start()
    {
        float savedVolume = PlayerPrefs.GetFloat(MusicVolumeKey, DefaultVolume);

        if (musicAudioSource != null)
        {
            musicAudioSource.volume = savedVolume;
        }
    }
}