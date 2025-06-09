using UnityEngine;
using System.Collections.Generic;

public class FeedbackController : MonoBehaviour
{
    [Header("Assign all canvas GameObjects here")]
    public List<GameObject> canvases; // List of canvases you’ll assign in the Inspector

    public GameObject feedbackPanelPrefab;

    private void Start()
    {
        if (PlayerPrefs.GetInt("ShowFeedback", 0) == 1)
        {
            PlayerPrefs.SetInt("ShowFeedback", 0); // Reset flag
            PlayerPrefs.Save();

            string targetCanvasName = PlayerPrefs.GetString("TargetCanvas", "Property Details");
            Debug.Log("Looking for canvas: " + targetCanvasName);

            GameObject targetCanvas = canvases.Find(c => c.name == targetCanvasName);

            if (targetCanvas != null)
            {
                Instantiate(feedbackPanelPrefab, targetCanvas.transform);
                Debug.Log("Feedback panel shown on '" + targetCanvasName + "' canvas.");
            }
            else
            {
                Debug.LogError("Canvas '" + targetCanvasName + "' not found in assigned list!");
            }
        }
    }
}
