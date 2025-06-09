using UnityEngine;
using Google.XR.Cardboard;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Management;
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
            Debug.Log("Exit (X) button pressed. Exiting VR and returning to MainPortraitScene (Treelane Left)...");

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
        // Stop XR
        XRGeneralSettings.Instance.Manager.StopSubsystems();
        XRGeneralSettings.Instance.Manager.DeinitializeLoader();

        // Set UI and feedback flags
        PlayerPrefs.SetString("ShowUI", "PropertyDetailsTreelaneLeft");
        PlayerPrefs.SetInt("ShowFeedback", 1);
        PlayerPrefs.SetString("FeedbackModelName", modelName);
        PlayerPrefs.Save();

        // Ensure portrait orientation
        Screen.orientation = ScreenOrientation.Portrait;
        yield return null;

        // Load main scene
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
