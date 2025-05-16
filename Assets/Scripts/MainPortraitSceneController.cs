using UnityEngine;

public class MainPortraitSceneController : MonoBehaviour
{
    public GameObject landingPageCanvas;           // Assign in Inspector
    public GameObject propertyDetailsCanvasParent; // Assign in Inspector

    void Start()
    {
        string showUI = PlayerPrefs.GetString("ShowUI", "");

        if (showUI == "PropertyDetails")
        {
            if (propertyDetailsCanvasParent != null)
                propertyDetailsCanvasParent.SetActive(true);

            if (landingPageCanvas != null)
                landingPageCanvas.SetActive(false);  // Make sure landing is hidden

            PlayerPrefs.DeleteKey("ShowUI");
        }
        else
        {
            // Default behavior: show landing page
            if (landingPageCanvas != null)
                landingPageCanvas.SetActive(true);

            if (propertyDetailsCanvasParent != null)
                propertyDetailsCanvasParent.SetActive(false);
        }
    }
}
