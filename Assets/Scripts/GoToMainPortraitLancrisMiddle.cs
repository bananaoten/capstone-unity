using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToMainPortraitLancrisMiddle : MonoBehaviour
{
    [Header("Assign the FeedbackPanel prefab")]
    public GameObject feedbackPanelPrefab;

    private GameObject feedbackPanelInstance;

    public void ShowFeedbackThenExit()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        PlayerPrefs.SetString("ShowUI", "PropertyDetailsLancrisMiddle");
        PlayerPrefs.SetString("TargetCanvas", "Property Details Lancris Middle");
        PlayerPrefs.SetInt("ShowFeedback", 1);
        PlayerPrefs.SetString("FeedbackModelName", "lancrismiddle"); // Set correct model name
        PlayerPrefs.Save();

        SceneManager.LoadScene("MainPortraitScene");
    }

    public void LoadMainPortraitWithLancrisMiddle()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        PlayerPrefs.SetString("ShowUI", "PropertyDetailsLancrisMiddle");
        PlayerPrefs.SetString("TargetCanvas", "Property Details Lancris Middle");
        PlayerPrefs.Save();

        SceneManager.LoadScene("MainPortraitScene");
    }
}