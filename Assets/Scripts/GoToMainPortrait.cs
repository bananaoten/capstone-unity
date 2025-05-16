using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToMainPortrait : MonoBehaviour
{
    public void LoadMainPortraitWithPropertyDetails()
    {
        // Set orientation to portrait BEFORE scene loads
        Screen.orientation = ScreenOrientation.Portrait;

        // Tell the MainPortraitScene to open Property Details
        PlayerPrefs.SetString("ShowUI", "PropertyDetails");
        PlayerPrefs.Save();

        // Load the MainPortraitScene
        SceneManager.LoadScene("MainPortraitScene");
    }
}
