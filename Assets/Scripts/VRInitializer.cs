using UnityEngine;
using UnityEngine.XR.Management;
using System.Collections;

public class VRInitializer : MonoBehaviour
{
    private XRManagerSettings xrManager;

    private IEnumerator Start()
    {
        Debug.Log("🌀 Forcing Landscape Orientation...");
        Screen.orientation = ScreenOrientation.LandscapeLeft;

        // Wait until orientation is confirmed
        float timeout = 3f;
        while (Screen.orientation != ScreenOrientation.LandscapeLeft && timeout > 0f)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.5f); // Ensure stability

        Debug.Log("✅ Orientation confirmed. Waiting for XR initialization...");

        // Wait for XRGeneralSettings to be available
        yield return new WaitUntil(() => XRGeneralSettings.Instance != null);

        xrManager = XRGeneralSettings.Instance.Manager;

        if (xrManager == null)
        {
            Debug.LogError("❌ XR Manager is null. Cannot initialize XR.");
            yield break;
        }

        if (!xrManager.isInitializationComplete)
        {
            xrManager.InitializeLoaderSync();
        }

        if (xrManager.activeLoader == null)
        {
            Debug.LogError("❌ XR Loader is not active.");
            yield break;
        }

        xrManager.StartSubsystems();
        Debug.Log("✅ XR Subsystems started.");
    }

    private void OnDestroy()
    {
        if (xrManager == null)
        {
            Debug.LogWarning("❌ XR Manager is null in OnDestroy. Skipping cleanup.");
            return;
        }

        Debug.Log("OnDestroy called. XR Manager is initialized.");

        if (xrManager.isInitializationComplete)
        {
            Debug.Log("🛑 Stopping XR...");
            try
            {
                xrManager.StopSubsystems();
                xrManager.DeinitializeLoader();
                Debug.Log("✅ XR stopped.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError("❌ Error stopping XR subsystems: " + ex.Message);
            }
        }
        else
        {
            Debug.LogWarning("❌ XR Manager is not initialized. Skipping stopping XR subsystems.");
        }
    }
}
