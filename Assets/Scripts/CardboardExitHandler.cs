using UnityEngine;
using Google.XR.Cardboard;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Management;
using System.Collections;

public class CardboardExitHandler : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(WaitForXRAndUpdateScreenParams());
    }

    IEnumerator WaitForXRAndUpdateScreenParams()
    {
        // Wait until XR initialization is complete
        yield return new WaitUntil(() => XRGeneralSettings.Instance.Manager.isInitializationComplete);

        Api.UpdateScreenParams();
    }

    void Update()
    {
        if (Api.IsCloseButtonPressed)
        {
            Debug.Log("Exit (X) button pressed. Returning to MainPortraitScene...");

            // Ensure portrait orientation is set before loading the scene
            Screen.orientation = ScreenOrientation.Portrait;

            // Save the UI state in PlayerPrefs
            PlayerPrefs.SetString("ShowUI", "PropertyDetails");
            PlayerPrefs.Save();

            // Load the MainPortraitScene
            SceneManager.LoadScene("MainPortraitScene");
        }

        if (Api.IsGearButtonPressed)
        {
            Debug.Log("Settings (gear) button pressed.");
            Api.ScanDeviceParams();
        }
    }

    void OnApplicationPause(bool pause)
    {
        if (!pause && XRGeneralSettings.Instance.Manager.isInitializationComplete)
        {
            Api.UpdateScreenParams();
        }
    }
}
