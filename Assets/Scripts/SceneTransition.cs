using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    // Method to go to Scene 2 (3D Mode)
    public void GoToScene2()
    {
        // Change orientation to Landscape (for 3D Mode)
        Screen.orientation = ScreenOrientation.LandscapeLeft;

        // Load Scene 2
        SceneManager.LoadScene("3D Mode");
    }

    // Method to go to Scene 3 (VR Mode with Google Cardboard)
    public void GoToScene3()
    {
        // Change orientation to Landscape (for VR Mode)
        Screen.orientation = ScreenOrientation.LandscapeLeft;

        // Load Scene 3 (VR Mode)
        SceneManager.LoadScene("VR Mode");
    }

    // Method to go to Scene 1 (Main Front End) from any scene
    public void GoToScene1()
    {
        // Change orientation back to Portrait (for main front end)
        Screen.orientation = ScreenOrientation.Portrait;

        // Load Scene 1
        SceneManager.LoadScene("MainPortraitScene");
    }
}
