using UnityEngine;
using Google.XR.Cardboard;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Management;
using System.Collections;

public class CardboardExitHandlerLancrisMiddle : MonoBehaviour
{
    [Header("Model Name to Pass")]
    public string modelName = "lancrismiddle";

    private bool exitInitiated = false;

    private void Start()
    {
        StartCoroutine(WaitForXRAndUpdateScreenParams());
    }

    IEnumerator WaitForXRAndUpdateScreenParams()
    {
        // Wait until XR is fully initialized
        yield return new WaitUntil(() => XRGeneralSettings.Instance.Manager.isInitializationComplete);
        Api.UpdateScreenParams();
    }

    void Update()
    {
        if (Api.IsCloseButtonPressed && !exitInitiated)
        {
            exitInitiated = true;
            Debug.Log("Exit (X) button pressed. Exiting VR and returning to MainPortraitScene (Lancris Middle)...");

            StartCoroutine(ExitVRAndReturnToMainPortrait());
        }

        if (Api.IsGearButtonPressed)
        {
            Debug.Log("Settings (gear) button pressed.");
            Api.ScanDeviceParams();
        }
    }

    IEnumerator ExitVRAndReturnToMainPortrait()
    {
        // Stop XR safely
        XRGeneralSettings.Instance.Manager.StopSubsystems();
        XRGeneralSettings.Instance.Manager.DeinitializeLoader();

        // Set PlayerPrefs for UI and feedback
        PlayerPrefs.SetString("ShowUI", "PropertyDetailsLancrisMiddle"); // UI canvas to show
        PlayerPrefs.SetInt("ShowFeedback", 1); // Trigger feedback display
        PlayerPrefs.SetString("FeedbackModelName", modelName); // Model name for feedback
        PlayerPrefs.Save();

        // Set to portrait and wait a frame
        Screen.orientation = ScreenOrientation.Portrait;
        yield return null;

        // Load the MainPortraitScene where feedback will show
        SceneManager.LoadScene("MainPortraitScene");
    }

    void OnApplicationPause(bool pause)
    {
        if (!pause && XRGeneralSettings.Instance.Manager.isInitializationComplete)
        {
            Api.UpdateScreenParams();
        }
    }
}
