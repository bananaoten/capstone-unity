using UnityEngine;
using Google.XR.Cardboard;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Management;
using System.Collections;

public class CardboardExitHandlerTreelaneMiddle : MonoBehaviour
{
    [Header("Model Name to Pass")]
    public string modelName = "treelanemiddle";

    private bool exitInitiated = false;

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
        if (Api.IsCloseButtonPressed && !exitInitiated)
        {
            exitInitiated = true;
            Debug.Log("Exit (X) button pressed. Returning to MainPortraitScene (Treelane Middle)...");

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
        // Stop VR
        XRGeneralSettings.Instance.Manager.StopSubsystems();
        XRGeneralSettings.Instance.Manager.DeinitializeLoader();

        // Set PlayerPrefs for feedback
        PlayerPrefs.SetString("ShowUI", "PropertyDetailsTreelaneMiddle");
        PlayerPrefs.SetInt("ShowFeedback", 1);
        PlayerPrefs.SetString("FeedbackModelName", modelName);
        PlayerPrefs.Save();

        // Set screen orientation
        Screen.orientation = ScreenOrientation.Portrait;
        yield return null;

        // Load scene
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
