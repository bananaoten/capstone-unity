using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Extensions;
using System;

public class FeedbackPanelManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Button submitButton;
    public Button closeButton;
    public GameObject panelRoot;
    public TMP_InputField feedbackInput;
    public Button[] starButtons;

    [Header("Star Sprites")]
    public Sprite defaultStarSprite;
    public Sprite highlightedStarSprite;

    [HideInInspector] public System.Action onSubmitCallback;

    private int rating = 0;
    private string modelName = "";

    private DatabaseReference dbReference;
    private FirebaseAuth auth;

    private void Start()
    {
        // ✅ Read model name from PlayerPrefs directly
        modelName = PlayerPrefs.GetString("FeedbackModelName", "unknown_model");
        Debug.Log("✅ Loaded model name from PlayerPrefs: " + modelName);

        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        auth = FirebaseAuth.DefaultInstance;

        submitButton.onClick.AddListener(OnSubmitClicked);
        closeButton.onClick.AddListener(OnCloseClicked);

        for (int i = 0; i < starButtons.Length; i++)
        {
            int index = i + 1;
            starButtons[i].onClick.AddListener(() => SetRating(index));
        }

        SetRating(0);

        // Optional: Clear PlayerPrefs after loading
        PlayerPrefs.DeleteKey("FeedbackModelName");
        PlayerPrefs.SetInt("ShowFeedback", 0);
    }

    private void SetRating(int value)
    {
        rating = value;
        for (int i = 0; i < starButtons.Length; i++)
        {
            Image img = starButtons[i].GetComponent<Image>();
            img.sprite = (i < rating) ? highlightedStarSprite : defaultStarSprite;
        }
    }

    private void OnSubmitClicked()
    {
        string feedbackText = feedbackInput.text;

        if (string.IsNullOrEmpty(modelName))
        {
            Debug.LogWarning("⚠️ Model name is not set. Cannot submit feedback.");
            return;
        }

        FirebaseUser user = auth.CurrentUser;
        if (user == null)
        {
            Debug.LogError("No Firebase user logged in.");
            return;
        }

        string email = user.Email;
        string submittedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
        string feedbackId = dbReference.Child("feedbacks").Push().Key;

        FeedbackData data = new FeedbackData(feedbackText, rating, email, submittedAt, modelName);

        dbReference.Child("feedbacks").Child(feedbackId)
            .SetRawJsonValueAsync(JsonUtility.ToJson(data))
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log($"✅ Feedback submitted for {modelName}");
                    onSubmitCallback?.Invoke();
                    panelRoot.SetActive(false);
                }
                else
                {
                    Debug.LogError("❌ Feedback submit failed: " + task.Exception);
                }
            });
    }

    private void OnCloseClicked()
    {
        Debug.Log("Feedback panel closed.");
        panelRoot.SetActive(false);
    }

    [System.Serializable]
    public class FeedbackData
    {
        public string feedbackText;
        public int rating;
        public string email;
        public string submittedAt;
        public string modelName;

        public FeedbackData(string feedbackText, int rating, string email, string submittedAt, string modelName)
        {
            this.feedbackText = feedbackText;
            this.rating = rating;
            this.email = email;
            this.submittedAt = submittedAt;
            this.modelName = modelName;
        }
    }
}
    