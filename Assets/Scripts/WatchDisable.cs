using UnityEngine;

public class WatchDisable : MonoBehaviour
{
    void OnDisable()
    {
        Debug.LogWarning("⚠️ submissionStatusText has been DISABLED by something!");
    }

    void OnEnable()
    {
        Debug.Log("✅ submissionStatusText ENABLED");
    }
}
