using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToPropertyDetailsTreelaneRight : MonoBehaviour
{
    public void LoadMainPortraitWithTreelaneRight()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        PlayerPrefs.SetString("ShowUI", "PropertyDetailsTreelaneRight");
        PlayerPrefs.Save();
        SceneManager.LoadScene("MainPortraitScene");
    }
}
