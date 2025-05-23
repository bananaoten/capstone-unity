using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToPropertyDetailsTreelaneMiddle : MonoBehaviour
{
    public void LoadMainPortraitWithTreelaneMiddle()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        PlayerPrefs.SetString("ShowUI", "PropertyDetailsTreelaneMiddle");
        PlayerPrefs.Save();
        SceneManager.LoadScene("MainPortraitScene");
    }
}
