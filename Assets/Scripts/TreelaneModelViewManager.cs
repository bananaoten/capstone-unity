using UnityEngine;
using TMPro;
using Firebase.Database;
using Firebase.Auth;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

public class TreelaneModelViewManager : MonoBehaviour
{
    public GameObject userHomeCanvas;

    public GameObject propertyDetailsLeftCanvas;
    public GameObject propertyDetailsMiddleCanvas;
    public GameObject propertyDetailsRightCanvas;

    public TMP_Text viewTextLeft;
    public TMP_Text viewTextMiddle;
    public TMP_Text viewTextRight;

    private DatabaseReference dbRef;
    private FirebaseAuth auth;

    private void Start()
    {
        if (FirebaseInitializer.IsFirebaseReady)
        {
            Initialize();
        }
        else
        {
            FirebaseInitializer.OnFirebaseReady += Initialize;
        }
    }

    private void Initialize()
    {
        auth = FirebaseInitializer.Auth;
        dbRef = FirebaseInitializer.Database.RootReference;

        if (auth.CurrentUser != null)
        {
            Debug.Log("Firebase and user ready.");
            // load counts
            _ = LoadViewCount("treelanecornerleft", viewTextLeft);
            _ = LoadViewCount("treelanemiddle", viewTextMiddle);
            _ = LoadViewCount("treelanecornerright", viewTextRight);
        }
        else
        {
            Debug.LogWarning("No user signed in.");
        }
    }

    public async void OnClickLeftModel()
    {
        await IncrementViewCount("treelanecornerleft", viewTextLeft);
        userHomeCanvas.SetActive(false);
        propertyDetailsLeftCanvas.SetActive(true);
    }

    public async void OnClickMiddleModel()
    {
        await IncrementViewCount("treelanemiddle", viewTextMiddle);
        userHomeCanvas.SetActive(false);
        propertyDetailsMiddleCanvas.SetActive(true);
    }

    public async void OnClickRightModel()
    {
        await IncrementViewCount("treelanecornerright", viewTextRight);
        userHomeCanvas.SetActive(false);
        propertyDetailsRightCanvas.SetActive(true);
    }

    private async Task IncrementViewCount(string modelId, TMP_Text viewText)
    {
        var modelRef = dbRef.Child("models").Child(modelId).Child("views");

        DataSnapshot snapshot = await modelRef.GetValueAsync();

        int currentViews = 0;
        if (snapshot.Exists && int.TryParse(snapshot.Value.ToString(), out int parsedViews))
        {
            currentViews = parsedViews;
        }

        currentViews++;
        await modelRef.SetValueAsync(currentViews);

        if (viewText != null)
            viewText.text = $"Views: {currentViews}";

        // push analytics event so React dashboard can read per-event data
        try
        {
            var analyticsRef = dbRef.Child("analytics").Child(modelId).Child("views");
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
        var modelRef = dbRef.Child("models").Child(modelId).Child("views");

        DataSnapshot snapshot = await modelRef.GetValueAsync();

        if (snapshot.Exists && int.TryParse(snapshot.Value.ToString(), out int views))
        {
            if (viewText != null)
                viewText.text = $"Views: {views}";
        }
    }
}