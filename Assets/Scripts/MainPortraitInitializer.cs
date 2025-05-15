using UnityEngine;

public class MainPortraitInitializer : MonoBehaviour
{
    public GameObject propertyDetailsCanvas;

    void Start()
    {
        // Initially show the "Property Details" canvas when the game starts
        Debug.Log("Loading Property Details UI...");
        propertyDetailsCanvas.SetActive(true);

        // Optionally, you can store the "PropertyDetails" state in PlayerPrefs
        PlayerPrefs.SetString("ShowUI", "PropertyDetails");

        // Optionally, reset the flag so it's not shown again after the first time
        PlayerPrefs.DeleteKey("ShowUI");
    }
}
