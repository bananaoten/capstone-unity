using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToPropertyDetailsTreelaneLeft : MonoBehaviour
{
    [Header("Assign the FeedbackPanel prefab")]
    public GameObject feedbackPanelPrefab;

    private GameObject feedbackPanelInstance;

    public void ShowFeedbackThenExit()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        PlayerPrefs.SetString("ShowUI", "PropertyDetailsTreelaneLeft");
        PlayerPrefs.SetString("TargetCanvas", "Property Details Treelane Left");
        PlayerPrefs.SetInt("ShowFeedback", 1);
        PlayerPrefs.SetString("FeedbackModelName", "treelanecornerleft"); // Set correct model name
        PlayerPrefs.Save();

        SceneManager.LoadScene("MainPortraitScene");
    }

    public void LoadMainPortraitWithTreelaneLeft()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        PlayerPrefs.SetString("ShowUI", "PropertyDetailsTreelaneLeft");
        PlayerPrefs.SetString("TargetCanvas", "Property Details Treelane Left");
        PlayerPrefs.Save();

        SceneManager.LoadScene("MainPortraitScene");
    }
}