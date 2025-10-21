using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Auth;
using Firebase.Database;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

public class HeartManager : MonoBehaviour
{
    [System.Serializable]
    public class LancrisModel
    {
        public string modelId;
        public Button heartButton;
        public TMP_Text heartCountText;
        public GameObject propertyDetailsCanvas;
        [HideInInspector] public bool hasHearted;
        [HideInInspector] public int heartCount;
        [HideInInspector] public string heartAnalyticsKey; // store analytics push key when user hearts
    }

    [Header("Lancris Models")]
    public GameObject userHomeCanvas;
    public LancrisModel lancrisCorner;
    public LancrisModel lancrisMiddle;

    [Header("Heart Sprites")]
    public Sprite heartOutlineSprite;
    public Sprite heartFilledSprite;

    private FirebaseAuth auth;
    private DatabaseReference dbRef;
    private string userId;

    async void Start()
    {
        if (FirebaseInitializer.IsFirebaseReady)
        {
            InitializeFirebase();
            await WaitForUserLoginAndLoad();
        }
        else
        {
            FirebaseInitializer.OnFirebaseReady += OnFirebaseReady;
        }
    }

    private async void OnFirebaseReady()
    {
        FirebaseInitializer.OnFirebaseReady -= OnFirebaseReady;
        InitializeFirebase();
        await WaitForUserLoginAndLoad();
    }

    private void InitializeFirebase()
    {
        auth = FirebaseInitializer.Auth;
        dbRef = FirebaseInitializer.Database.RootReference;
        Debug.Log("HeartManager Firebase Initialized");
    }

    private async Task WaitForUserLoginAndLoad()
    {
        int retries = 0;
        while (auth.CurrentUser == null && retries < 50)
        {
            await Task.Delay(100);
            retries++;
        }

        if (auth.CurrentUser != null)
        {
            userId = auth.CurrentUser.UserId;
            Debug.Log("HeartManager: User logged in: " + userId);

            SetupModel(lancrisCorner);
            SetupModel(lancrisMiddle);

            await LoadAllHeartData();
        }
        else
        {
            Debug.LogWarning("HeartManager: Firebase user not found.");
        }
    }

    private void SetupModel(LancrisModel model)
    {
        if (model.heartButton != null)
        {
            model.heartButton.interactable = false;
            model.heartButton.onClick.AddListener(() => OnHeartButtonClicked(model));
        }
    }

    private async Task LoadAllHeartData()
    {
        await LoadHeartData(lancrisCorner);
        await LoadHeartData(lancrisMiddle);
    }

    private async Task LoadHeartData(LancrisModel model)
    {
        if (model.heartCountText == null || string.IsNullOrEmpty(model.modelId)) return;

        var modelRef = dbRef.Child("models").Child(model.modelId);

        // Load heart count
        var heartCountSnap = await modelRef.Child("heartCount").GetValueAsync();
        model.heartCount = 0;
        if (heartCountSnap.Exists) int.TryParse(heartCountSnap.Value.ToString(), out model.heartCount);
        model.heartCountText.text = model.heartCount.ToString();

        // Load user heart state
        var userHeartSnap = await modelRef.Child("heartedUsers").Child(userId).GetValueAsync();
        model.hasHearted = false;
        model.heartAnalyticsKey = null;

        if (userHeartSnap.Exists)
        {
            string val = userHeartSnap.Value.ToString();
            // backwards compatible: previously used "true" boolean; now we store analytics push key
            if (val == "true")
            {
                model.hasHearted = true;
                model.heartAnalyticsKey = null;
            }
            else
            {
                model.hasHearted = true;
                model.heartAnalyticsKey = val; // stored push key
            }
        }

        UpdateHeartUI(model);

        if (model.heartButton != null)
        {
            model.heartButton.interactable = true;
            Debug.Log($"Heart button for {model.modelId} now interactable");
        }
    }

    private async void OnHeartButtonClicked(LancrisModel model)
    {
        if (string.IsNullOrEmpty(userId) || model.heartButton == null) return;

        var modelRef = dbRef.Child("models").Child(model.modelId);
        var heartedUserRef = modelRef.Child("heartedUsers").Child(userId);

        var heartCountSnap = await modelRef.Child("heartCount").GetValueAsync();
        model.heartCount = 0;
        if (heartCountSnap.Exists) int.TryParse(heartCountSnap.Value.ToString(), out model.heartCount);

        if (model.hasHearted)
        {
            // remove heart: decrement count, remove heartedUsers mapping and analytics event if exists
            model.heartCount = Mathf.Max(0, model.heartCount - 1);
            model.hasHearted = false;

            // remove analytics like event if we stored its key
            if (!string.IsNullOrEmpty(model.heartAnalyticsKey))
            {
                await dbRef.Child("analytics").Child(model.modelId).Child("likes").Child(model.heartAnalyticsKey).RemoveValueAsync();
            }
            // remove mapping
            await heartedUserRef.RemoveValueAsync();
        }
        else
        {
            // add heart: increment, create analytics like event and save its key under heartedUsers/{userId}
            model.heartCount += 1;
            model.hasHearted = true;

            // push analytics event
            try
            {
                var analyticsRef = dbRef.Child("analytics").Child(model.modelId).Child("likes");
                string pushKey = analyticsRef.Push().Key;
                if (!string.IsNullOrEmpty(pushKey))
                {
                    var eventData = new Dictionary<string, object>
                    {
                        { "timestamp", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() },
                        { "userId", userId }
                    };
                    await analyticsRef.Child(pushKey).SetValueAsync(eventData);
                    // store the pushKey so we can remove the analytics event when user unhearts
                    await heartedUserRef.SetValueAsync(pushKey);
                    model.heartAnalyticsKey = pushKey;
                }
                else
                {
                    // fallback: mark as true if push failed
                    await heartedUserRef.SetValueAsync(true);
                    model.heartAnalyticsKey = null;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to push analytics like event for {model.modelId}: {ex.Message}");
                await heartedUserRef.SetValueAsync(true);
                model.heartAnalyticsKey = null;
            }
        }

        await modelRef.Child("heartCount").SetValueAsync(model.heartCount);

        model.heartCountText.text = model.heartCount.ToString();
        UpdateHeartUI(model);

        // Toggle canvases
        if (userHomeCanvas != null) userHomeCanvas.SetActive(false);
        if (model.propertyDetailsCanvas != null) model.propertyDetailsCanvas.SetActive(true);
    }

    private void UpdateHeartUI(LancrisModel model)
    {
        if (model.heartButton == null) return;

        var heartImage = model.heartButton.GetComponent<Image>();
        if (heartImage != null)
        {
            heartImage.sprite = model.hasHearted ? heartFilledSprite : heartOutlineSprite;
        }
    }
}