using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToMainPortraitLancrisMiddle : MonoBehaviour
{
    public void LoadMainPortraitWithLancrisMiddle()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        PlayerPrefs.SetString("ShowUI", "PropertyDetailsLancrisMiddle");
        PlayerPrefs.Save();
        SceneManager.LoadScene("MainPortraitScene");
    }
}
