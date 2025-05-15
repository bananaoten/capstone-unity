using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneOrientationHandler : MonoBehaviour
{
    void Start()
    {
        string scene = SceneManager.GetActiveScene().name;

        if (scene == "MainPortraitScene")
        {
            Screen.orientation = ScreenOrientation.Portrait;
        }
        else if (scene == "3D Mode")
        {
            Screen.orientation = ScreenOrientation.LandscapeLeft;
        }
        // VR Mode is handled by VRInitializer.cs
    }
}
