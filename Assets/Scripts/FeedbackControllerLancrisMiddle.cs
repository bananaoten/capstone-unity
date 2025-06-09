using UnityEngine;

public class FeedbackControllerLancrisMiddle : MonoBehaviour
{
    public GameObject feedbackPanelPrefab;

    private void Start()
    {
        if (PlayerPrefs.GetInt("ShowFeedback", 0) == 1)
        {
            PlayerPrefs.SetInt("ShowFeedback", 0); // Reset the flag
            PlayerPrefs.Save();

            GameObject canvas = GameObject.Find("Property Details Lancris Middle");

            if (canvas != null)
            {
                Instantiate(feedbackPanelPrefab, canvas.transform);
                Debug.Log("Feedback panel shown on 'Property Details Lancris Middle' canvas.");
            }
            else
            {
                Debug.LogError("Canvas 'Property Details Lancris Middle' not found!");
            }
        }
    }
}
