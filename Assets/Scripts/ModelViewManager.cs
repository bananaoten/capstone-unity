using UnityEngine;
using TMPro;
using Firebase.Database;
using Firebase.Auth;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

public class ModelViewManager : MonoBehaviour
{
    public GameObject userHomeLancrisCanvas;

    public GameObject propertyDetailsLancrisCornerCanvas;
    public GameObject propertyDetailsLancrisMiddleCanvas;

    public TMP_Text viewTextCorner;
    public TMP_Text viewTextMiddle;

    private DatabaseReference dbReference;
    private FirebaseAuth auth;

    async void Start()
    {
        if (FirebaseInitializer.IsFirebaseReady)
        {
            InitializeFirebase();
            await LoadInitialViewCounts();
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
        await LoadInitialViewCounts();
    }

    private void InitializeFirebase()
    {
        auth = FirebaseInitializer.Auth;
        dbReference = FirebaseInitializer.Database.RootReference;

        if (auth.CurrentUser == null)
        {
            Debug.LogWarning("No user signed in.");
        }
        else
        {
            Debug.Log("Firebase and user ready.");
        }
    }

    public async void OnClickLancrisCornerModel()
    {
        await IncrementViewCount("lancrisdeluxe", viewTextCorner);
        userHomeLancrisCanvas.SetActive(false);
        propertyDetailsLancrisCornerCanvas.SetActive(true);
    }

    public async void OnClickLancrisMiddleModel()
    {
        await IncrementViewCount("lancrisstandard", viewTextMiddle);
        userHomeLancrisCanvas.SetActive(false);
        propertyDetailsLancrisMiddleCanvas.SetActive(true);
    }

    private async Task IncrementViewCount(string modelId, TMP_Text viewText)
    {
        var modelRef = dbReference.Child("models").Child(modelId).Child("views");

        var snapshot = await modelRef.GetValueAsync();

        int currentViews = 0;
        if (snapshot.Exists && int.TryParse(snapshot.Value.ToString(), out int parsedViews))
        {
            currentViews = parsedViews;
        }

        currentViews++;
        await modelRef.SetValueAsync(currentViews);

        if (viewText != null)
            viewText.text = $"Views: {currentViews}";

        // Push analytics event so React dashboard can read per-event data
        try
        {
            var analyticsRef = dbReference.Child("analytics").Child(modelId).Child("views");
            string pushKey = analyticsRef.Push().Key;
            if (!string.IsNullOrEmpty(pushKey))
            {
                var eventData = new Dictionary<string, object>
                {
                    { "timestamp", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() },
                    { "userId", auth?.CurrentUser?.UserId ?? "anonymous" }
                };
                await analyticsRef.Child(pushKey).SetValueAsync(eventData);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Failed to push analytics view event for {modelId}: {ex.Message}");
        }
    }

    private async Task LoadViewCount(string modelId, TMP_Text viewText)
    {
        var modelRef = dbReference.Child("models").Child(modelId).Child("views");

        var snapshot = await modelRef.GetValueAsync();

        if (snapshot.Exists && int.TryParse(snapshot.Value.ToString(), out int views))
        {
            if (viewText != null)
                viewText.text = $"Views: {views}";
        }
    }

    private async Task LoadInitialViewCounts()
    {
        await LoadViewCount("lancrisdeluxe", viewTextCorner);
        await LoadViewCount("lancrisstandard", viewTextMiddle);
    }
}