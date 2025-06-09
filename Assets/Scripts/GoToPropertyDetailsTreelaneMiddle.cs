using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToPropertyDetailsTreelaneMiddle : MonoBehaviour
{
    [Header("Assign the FeedbackPanel prefab")]
    public GameObject feedbackPanelPrefab;

    private GameObject feedbackPanelInstance;

    public void ShowFeedbackThenExit()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        PlayerPrefs.SetString("ShowUI", "PropertyDetailsTreelaneMiddle");
        PlayerPrefs.SetString("TargetCanvas", "Property Details Treelane Middle");
        PlayerPrefs.SetInt("ShowFeedback", 1);
        PlayerPrefs.SetString("FeedbackModelName", "treelanemiddle"); // Set correct model name
        PlayerPrefs.Save();

        SceneManager.LoadScene("MainPortraitScene");
    }

    public void LoadMainPortraitWithTreelaneMiddle()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        PlayerPrefs.SetString("ShowUI", "PropertyDetailsTreelaneMiddle");
        PlayerPrefs.SetString("TargetCanvas", "Property Details Treelane Middle");
        PlayerPrefs.Save();

        SceneManager.LoadScene("MainPortraitScene");
    }
}