using UnityEngine;

public class AudioTrigger : MonoBehaviour
{
    public AudioClip sound;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (AudioTriggerManager.Instance != null)
        {
            AudioTriggerManager.Instance.PlayAudio(sound);
        }
    }
}
