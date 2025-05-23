using UnityEngine;

public class MainPortraitSceneController : MonoBehaviour
{
    public GameObject landingPageCanvas;                      // Assign in Inspector
    public GameObject propertyDetailsCanvasParent;            // Lancris Corner
    public GameObject propertyDetailsLancrisMiddleCanvas;     // Lancris Middle
    public GameObject propertyDetailsTreelaneLeftCanvas;      // Treelane Left
    public GameObject propertyDetailsTreelaneMiddleCanvas;    // Treelane Middle
    public GameObject propertyDetailsTreelaneRightCanvas;     // Treelane Right (new)

    void Start()
    {
        string showUI = PlayerPrefs.GetString("ShowUI", "");

        if (showUI == "PropertyDetails")
        {
            propertyDetailsCanvasParent?.SetActive(true);
            propertyDetailsLancrisMiddleCanvas?.SetActive(false);
            propertyDetailsTreelaneLeftCanvas?.SetActive(false);
            propertyDetailsTreelaneMiddleCanvas?.SetActive(false);
            propertyDetailsTreelaneRightCanvas?.SetActive(false);
            landingPageCanvas?.SetActive(false);
        }
        else if (showUI == "PropertyDetailsLancrisMiddle")
        {
            propertyDetailsCanvasParent?.SetActive(false);
            propertyDetailsLancrisMiddleCanvas?.SetActive(true);
            propertyDetailsTreelaneLeftCanvas?.SetActive(false);
            propertyDetailsTreelaneMiddleCanvas?.SetActive(false);
            propertyDetailsTreelaneRightCanvas?.SetActive(false);
            landingPageCanvas?.SetActive(false);
        }
        else if (showUI == "PropertyDetailsTreelaneLeft")
        {
            propertyDetailsCanvasParent?.SetActive(false);
            propertyDetailsLancrisMiddleCanvas?.SetActive(false);
            propertyDetailsTreelaneLeftCanvas?.SetActive(true);
            propertyDetailsTreelaneMiddleCanvas?.SetActive(false);
            propertyDetailsTreelaneRightCanvas?.SetActive(false);
            landingPageCanvas?.SetActive(false);
        }
        else if (showUI == "PropertyDetailsTreelaneMiddle")
        {
            propertyDetailsCanvasParent?.SetActive(false);
            propertyDetailsLancrisMiddleCanvas?.SetActive(false);
            propertyDetailsTreelaneLeftCanvas?.SetActive(false);
            propertyDetailsTreelaneMiddleCanvas?.SetActive(true);
            propertyDetailsTreelaneRightCanvas?.SetActive(false);
            landingPageCanvas?.SetActive(false);
        }
        else if (showUI == "PropertyDetailsTreelaneRight")
        {
            propertyDetailsCanvasParent?.SetActive(false);
            propertyDetailsLancrisMiddleCanvas?.SetActive(false);
            propertyDetailsTreelaneLeftCanvas?.SetActive(false);
            propertyDetailsTreelaneMiddleCanvas?.SetActive(false);
            propertyDetailsTreelaneRightCanvas?.SetActive(true);
            landingPageCanvas?.SetActive(false);
        }
        else
        {
            // Default: show landing, hide all property detail canvases
            landingPageCanvas?.SetActive(true);
            propertyDetailsCanvasParent?.SetActive(false);
            propertyDetailsLancrisMiddleCanvas?.SetActive(false);
            propertyDetailsTreelaneLeftCanvas?.SetActive(false);
            propertyDetailsTreelaneMiddleCanvas?.SetActive(false);
            propertyDetailsTreelaneRightCanvas?.SetActive(false);
        }

        PlayerPrefs.DeleteKey("ShowUI");
    }
}
