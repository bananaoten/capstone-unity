using UnityEngine;

public class OrientationManager : MonoBehaviour
{
    void Start()
    {
        // Force landscape again in case the phone didn't rotate yet
        Screen.orientation = ScreenOrientation.LandscapeLeft;
    }
}
