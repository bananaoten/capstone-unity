using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToMainPortrait : MonoBehaviour
{
    [Header("Model Name to Pass to Feedback")]
    public string modelName = "lancriscorner";

    public void ShowFeedbackThenExit()
    {
        Screen.orientation = ScreenOrientation.Portrait;

        PlayerPrefs.SetString("ShowUI", "PropertyDetails");
        PlayerPrefs.SetString("TargetCanvas", "Property Details");
        PlayerPrefs.SetInt("ShowFeedback", 1);
        PlayerPrefs.SetString("FeedbackModelName", modelName); // Pass the selected model name
        PlayerPrefs.Save();

        SceneManager.LoadScene("MainPortraitScene");
    }

    public void LoadMainPortraitWithoutFeedback()
    {
        Screen.orientation = ScreenOrientation.Portrait;

        PlayerPrefs.SetString("ShowUI", "PropertyDetails");
        PlayerPrefs.SetString("TargetCanvas", "Property Details");
        PlayerPrefs.Save();

        SceneManager.LoadScene("MainPortraitScene");
    }
}