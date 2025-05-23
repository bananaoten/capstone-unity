using UnityEngine;
using Google.XR.Cardboard;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Management;
using System.Collections;

public class CardboardExitHandlerTreelaneMiddle : MonoBehaviour
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
            Debug.Log("Exit (X) button pressed. Returning to MainPortraitScene (Treelane Middle)...");

            // Set orientation to portrait before scene switch
            Screen.orientation = ScreenOrientation.Portrait;

            // Save UI state to show Treelane Middle canvas on main scene load
            PlayerPrefs.SetString("ShowUI", "PropertyDetailsTreelaneMiddle");

            PlayerPrefs.Save();

            // Load MainPortraitScene
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
