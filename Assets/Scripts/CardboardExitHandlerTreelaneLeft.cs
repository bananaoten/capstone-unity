using UnityEngine;
using Google.XR.Cardboard;
using UnityEngine.XR.Management;
using UnityEngine.SceneManagement;
using System.Collections;

public class CardboardExitHandlerTreelaneLeft : MonoBehaviour
{
    [Header("Model Name to Pass")]
    public string modelName = "treelaneleft";

    private bool exitInitiated = false;

    private void Start()
    {
        StartCoroutine(WaitForXRAndUpdateScreenParams());
    }

    IEnumerator WaitForXRAndUpdateScreenParams()
    {
        yield return new WaitUntil(() => XRGeneralSettings.Instance.Manager.isInitializationComplete);
        Api.UpdateScreenParams();
    }

    void Update()
    {
        if (Api.IsCloseButtonPressed && !exitInitiated)
        {
            exitInitiated = true;
            Debug.Log("Exit (X) button pressed. Stopping XR and reloading scene...");

            StartCoroutine(ExitVRAndReloadScene());
        }

        if (Api.IsGearButtonPressed)
        {
            Debug.Log("Settings (gear) button pressed.");
            Api.ScanDeviceParams();
        }
    }

    IEnumerator ExitVRAndReloadScene()
    {
        // Stop XR
        XRGeneralSettings.Instance.Manager.StopSubsystems();
        XRGeneralSettings.Instance.Manager.DeinitializeLoader();

        // Save feedback data for Treelane Left
        PlayerPrefs.SetInt("ShowFeedback", 1);
        PlayerPrefs.SetString("FeedbackModelName", modelName);
        PlayerPrefs.SetString("ShowUI", "PropertyDetailsTreelaneLeft");
        PlayerPrefs.SetString("TargetCanvas", "Property Details Treelane Left");
        PlayerPrefs.Save();

        yield return null;

        // Reset to portrait orientation
        Screen.orientation = ScreenOrientation.Portrait;

        // Delay one more frame for safety
        yield return null;

        // Load the portrait scene where feedback is shown
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