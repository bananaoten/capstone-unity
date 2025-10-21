using UnityEngine;

public class AudioTriggerManager : MonoBehaviour
{
    public static AudioTriggerManager Instance;

    private AudioSource audioSource;

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            audioSource = GetComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Call this to play any clip, stopping the previous one
    public void PlayAudio(AudioClip clip)
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        if (clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    // Optional: Stop the sound manually if needed
    public void StopAudio()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}
