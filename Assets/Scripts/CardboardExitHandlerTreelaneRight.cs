using UnityEngine;
using Google.XR.Cardboard;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Management;
using System.Collections;

public class CardboardExitHandlerTreelaneRight : MonoBehaviour
{
    [Header("Model Name to Pass")]
    public string modelName = "treleaneright";

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
            Debug.Log("Exit (X) button pressed. Returning to MainPortraitScene (Treelane Right)...");

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
        XRGeneralSettings.Instance.Manager.StopSubsystems();
        XRGeneralSettings.Instance.Manager.DeinitializeLoader();

        PlayerPrefs.SetString("ShowUI", "PropertyDetailsTreelaneRight");
        PlayerPrefs.SetInt("ShowFeedback", 1);
        PlayerPrefs.SetString("FeedbackModelName", modelName);
        PlayerPrefs.Save();

        Screen.orientation = ScreenOrientation.Portrait;
        yield return null;

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
