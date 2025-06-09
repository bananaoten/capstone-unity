using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToPropertyDetailsTreelaneRight : MonoBehaviour
{
    [Header("Assign the FeedbackPanel prefab")]
    public GameObject feedbackPanelPrefab;

    private GameObject feedbackPanelInstance;

    public void ShowFeedbackThenExit()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        PlayerPrefs.SetString("ShowUI", "PropertyDetailsTreelaneRight");
        PlayerPrefs.SetString("TargetCanvas", "Property Details Treelane Right");
        PlayerPrefs.SetInt("ShowFeedback", 1);
        PlayerPrefs.SetString("FeedbackModelName", "treelanecornerright"); // Set correct model name
        PlayerPrefs.Save();

        SceneManager.LoadScene("MainPortraitScene");
    }

    public void LoadMainPortraitWithTreelaneRight()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        PlayerPrefs.SetString("ShowUI", "PropertyDetailsTreelaneRight");
        PlayerPrefs.SetString("TargetCanvas", "Property Details Treelane Right");
        PlayerPrefs.Save();

        SceneManager.LoadScene("MainPortraitScene");
    }
}