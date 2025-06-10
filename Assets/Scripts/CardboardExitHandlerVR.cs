using UnityEngine;
using Google.XR.Cardboard;
using UnityEngine.XR.Management;
using UnityEngine.SceneManagement;
using System.Collections;

public class CardboardExitHandlerVR : MonoBehaviour
{
    [Header("Model Name to Pass (must match the canvas name logic)")]
    public string modelName = "lancrismiddle"; // change per scene

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
            Debug.Log("❎ Exit VR button pressed. Processing exit...");

            StartCoroutine(ExitVRAndReturn());
        }

        if (Api.IsGearButtonPressed)
        {
            Debug.Log("⚙️ Gear button pressed.");
            Api.ScanDeviceParams();
        }
    }

    IEnumerator ExitVRAndReturn()
    {
        // Stop XR system safely
        XRGeneralSettings.Instance.Manager.StopSubsystems();
        XRGeneralSettings.Instance.Manager.DeinitializeLoader();

        // Set required PlayerPrefs
        PlayerPrefs.SetInt("ShowFeedback", 1); // tell UI to show feedback
        PlayerPrefs.SetString("FeedbackModelName", modelName); // pass model name
        PlayerPrefs.SetString("ShowUI", GetCanvasName(modelName)); // match UI Canvas

        PlayerPrefs.Save();

        // Reset orientation and load
        Screen.orientation = ScreenOrientation.Portrait;
        yield return null; // wait one frame
        SceneManager.LoadScene("MainPortraitScene");
    }

    void OnApplicationPause(bool pause)
    {
        if (!pause && XRGeneralSettings.Instance.Manager.isInitializationComplete)
        {
            Api.UpdateScreenParams();
        }
    }

    /// <summary>
    /// Converts model name to expected Canvas name in MainPortraitScene.
    /// Adjust this if your naming changes.
    /// </summary>
    private string GetCanvasName(string model)
    {
        switch (model.ToLower())
        {
            case "lancrismiddle": return "PropertyDetailsLancrisMiddle";
            case "lancriscorner": return "PropertyDetailsLancrisCorner";
            case "treelaneleft": return "PropertyDetailsTreelaneLeft";
            case "treelanemiddle": return "PropertyDetailsTreelaneMiddle";
            case "treelaneright": return "PropertyDetailsTreelaneRight";
            default: return "Property Details"; // fallback
        }
    }
}
