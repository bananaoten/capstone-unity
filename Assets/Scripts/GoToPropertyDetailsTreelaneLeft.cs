using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToPropertyDetailsTreelaneLeft : MonoBehaviour
{
    public void LoadMainPortraitWithTreelaneLeft()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        PlayerPrefs.SetString("ShowUI", "PropertyDetailsTreelaneLeft");
        PlayerPrefs.Save();
        SceneManager.LoadScene("MainPortraitScene");
    }
}
