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
    public Sprite heartOutlineSprite;
    public Sprite heartFilledSprite;

    private Image heartImage;
    private FirebaseAuth auth;
    private DatabaseReference dbRef;

    private string userId;
    private string modelId = "model1";

    private bool hasHearted = false;
    private int heartCount = 0;

    async void Start()
    {
        // Setup references
        auth = FirebaseAuth.DefaultInstance;
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        heartImage = heartButton.GetComponent<Image>();

        heartButton.interactable = false; // Disable until Firebase is ready
        heartButton.onClick.AddListener(OnHeartButtonClicked);

        // Wait for Firebase Auth to be ready
        await WaitForUserLogin();

        if (!string.IsNullOrEmpty(userId))
        {
            await LoadHeartDataAsync();  // Load heart info
            UpdateHeartUI();            // Update UI visuals
            heartButton.interactable = true; // Enable button
        }
        else
        {
            Debug.LogError("HeartManager: Firebase user is null.");
        }
    }

    async Task WaitForUserLogin()
    {
        int retries = 0;
        while (auth.CurrentUser == null && retries < 50) // Wait up to 5 seconds
        {
            await Task.Delay(100);
            retries++;
        }

        if (auth.CurrentUser != null)
        {
            userId = auth.CurrentUser.UserId;
            Debug.Log("HeartManager: Firebase user ready.");
        }
        else
        {
            Debug.LogWarning("HeartManager: Firebase user still null after waiting.");
        }
    }

    private async Task LoadHeartDataAsync()
    {
        try
        {
            // Get total heart count
            var heartCountSnap = await dbRef.Child("models").Child(modelId).Child("heartCount").GetValueAsync();
            if (heartCountSnap.Exists)
            {
                int.TryParse(heartCountSnap.Value.ToString(), out heartCount);
            }

            // Check if this user has already hearted
            var userHeartSnap = await dbRef.Child("models").Child(modelId).Child("heartedUsers").Child(userId).GetValueAsync();
            hasHearted = userHeartSnap.Exists && userHeartSnap.Value.ToString() == "true";

            heartCountText.text = heartCount.ToString();
        }
        catch (System.Exception e)
        {
            Debug.LogError("HeartManager: Error loading data - " + e.Message);
        }
    }

    private async void OnHeartButtonClicked()
    {
        if (string.IsNullOrEmpty(userId)) return;

        try
        {
            if (hasHearted)
            {
                // Unlike
                heartCount = Mathf.Max(heartCount - 1, 0);
                await dbRef.Child("models").Child(modelId).Child("heartedUsers").Child(userId).RemoveValueAsync();
            }
            else
            {
                // Like
                heartCount += 1;
                await dbRef.Child("models").Child(modelId).Child("heartedUsers").Child(userId).SetValueAsync(true);
            }

            hasHearted = !hasHearted;
            await dbRef.Child("models").Child(modelId).Child("heartCount").SetValueAsync(heartCount);
            heartCountText.text = heartCount.ToString();

            UpdateHeartUI();
        }
        catch (System.Exception ex)
        {
            Debug.LogError("HeartManager: Error updating heart - " + ex.Message);
        }
    }

    private void UpdateHeartUI()
    {
        if (heartImage != null)
        {
            heartImage.sprite = hasHearted ? heartFilledSprite : heartOutlineSprite;
        }
    }
}
