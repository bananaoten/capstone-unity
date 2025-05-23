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

    public void GoToScene5()
    {
        // Change orientation to Landscape (for 3D Mode)
        Screen.orientation = ScreenOrientation.LandscapeLeft;

        // Load Scene 2
        SceneManager.LoadScene("3D Mode Lancris Middle");
    }

    // Method to go to Scene 3 (VR Mode with Google Cardboard)
    public void GoToScene3()
    {
        // Change orientation to Landscape (for VR Mode)
        Screen.orientation = ScreenOrientation.LandscapeLeft;

        // Load Scene 3 (VR Mode)
        SceneManager.LoadScene("VR Mode");
    }



public void GoToScene4()
    {
        // Change orientation to Landscape (for VR Mode)
        Screen.orientation = ScreenOrientation.LandscapeLeft;

        // Load Scene 3 (VR Mode)
        SceneManager.LoadScene("VR Mode Lancris Middle");
    }

    // Method to go to Scene 1 (Main Front End) from any scene
    public void GoToScene1()
    {
        // Change orientation back to Portrait (for main front end)
        Screen.orientation = ScreenOrientation.Portrait;

        // Load Scene 1
        SceneManager.LoadScene("MainPortraitScene");
    }

    public void GoToScene6()
    {
        // Change orientation to Landscape (for 3D Mode)
        Screen.orientation = ScreenOrientation.LandscapeLeft;

        // Load Scene 2
        SceneManager.LoadScene("3D Mode Lancris Treelane Left");
    }

public void GoToScene7()
    {
        // Change orientation to Landscape (for VR Mode)
        Screen.orientation = ScreenOrientation.LandscapeLeft;

        // Load Scene 3 (VR Mode)
        SceneManager.LoadScene("VR Mode Treelane Left");
    }

     public void GoToScene8()
    {
        // Change orientation to Landscape (for 3D Mode)
        Screen.orientation = ScreenOrientation.LandscapeLeft;

        // Load Scene 2
        SceneManager.LoadScene("3D Mode Lancris Treelane Middle");
    }

    public void GoToScene9()
    {
        // Change orientation to Landscape (for VR Mode)
        Screen.orientation = ScreenOrientation.LandscapeLeft;

        // Load Scene 3 (VR Mode)
        SceneManager.LoadScene("VR Mode Treelane Middle");
    }

     public void GoToScene10()
    {
        // Change orientation to Landscape (for 3D Mode)
        Screen.orientation = ScreenOrientation.LandscapeLeft;

        // Load Scene 2
        SceneManager.LoadScene("3D Mode Lancris Treelane Right");
    }

    public void GoToScene11()
    {
        // Change orientation to Landscape (for VR Mode)
        Screen.orientation = ScreenOrientation.LandscapeLeft;

        // Load Scene 3 (VR Mode)
        SceneManager.LoadScene("VR Mode Treelane Right");
    }
}
