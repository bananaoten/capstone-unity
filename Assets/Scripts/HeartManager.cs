using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Auth;
using Firebase.Database;
using System.Threading.Tasks;

public class HeartManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Button heartButton;
    public TMP_Text heartCountText;

    [Header("Heart Sprites")]
    public Sprite heartOutlineSprite;  // default empty heart
    public Sprite heartFilledSprite;   // filled heart when liked

    private Image heartImage;           // will be assigned automatically

    private FirebaseAuth auth;
    private string userId;
    private int heartCount = 0;
    private bool hasHearted = false;

    // Fixed model ID to track in Firebase
    private string modelId = "model1";

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;

        // Auto get the Image component from the heart button GameObject or its child
        heartImage = heartButton.GetComponent<Image>();
        if (heartImage == null)
        {
            heartImage = heartButton.GetComponentInChildren<Image>();
        }

        if (auth.CurrentUser != null)
        {
            userId = auth.CurrentUser.UserId;
            heartButton.onClick.AddListener(OnHeartButtonClicked);
            LoadHeartDataAsync();
        }
        else
        {
            Debug.LogError("User not logged in.");
        }
    }

    private async void LoadHeartDataAsync()
    {
        var reference = FirebaseDatabase.DefaultInstance.RootReference;

        // Load heart count
        var heartCountSnapshot = await reference.Child("models").Child(modelId).Child("heartCount").GetValueAsync();
        if (heartCountSnapshot.Exists)
        {
            heartCount = int.Parse(heartCountSnapshot.Value.ToString());
        }
        else
        {
            heartCount = 0;
        }
        heartCountText.text = heartCount.ToString();

        // Check if user has hearted this model
        var userHeartSnapshot = await reference.Child("models").Child(modelId).Child("heartedUsers").Child(userId).GetValueAsync();
        hasHearted = userHeartSnapshot.Exists && userHeartSnapshot.Value.ToString() == "true";

        UpdateHeartUI();
    }

    private async void OnHeartButtonClicked()
    {
        var reference = FirebaseDatabase.DefaultInstance.RootReference;

        if (!hasHearted)
        {
            // User likes the model
            heartCount++;
            hasHearted = true;
            heartCountText.text = heartCount.ToString();

            // Update Firebase
            await reference.Child("models").Child(modelId).Child("heartCount").SetValueAsync(heartCount);
            await reference.Child("models").Child(modelId).Child("heartedUsers").Child(userId).SetValueAsync(true);
        }
        else
        {
            // User unlikes the model
            heartCount = Mathf.Max(heartCount - 1, 0);
            hasHearted = false;
            heartCountText.text = heartCount.ToString();

            // Update Firebase
            await reference.Child("models").Child(modelId).Child("heartCount").SetValueAsync(heartCount);
            await reference.Child("models").Child(modelId).Child("heartedUsers").Child(userId).RemoveValueAsync();
        }

        UpdateHeartUI();
    }

    private void UpdateHeartUI()
    {
        if (heartImage == null)
        {
            Debug.LogWarning("Heart image component not found!");
            return;
        }

        if (hasHearted)
        {
            heartImage.sprite = heartFilledSprite;
        }
        else
        {
            heartImage.sprite = heartOutlineSprite;
        }
    }
}
