using UnityEngine;
using TMPro;
using Firebase.Database;
using Firebase.Auth;
using System;
using System.Collections;
using System.Collections.Generic;

public class RecentMessageDisplay : MonoBehaviour
{
    public TMP_Text recentMessageText;
    public TMP_Text recentTimestampText;

    private DatabaseReference messageRef;
    private FirebaseUser currentUser;
    private string currentUserId;
    private bool isListening = false;

    void Start()
    {
        ClearDisplay();
        currentUserId = null;
        currentUser = null;

        if (FirebaseInitializer.IsFirebaseReady)
        {
            FirebaseAuth.DefaultInstance.StateChanged -= OnAuthStateChanged;
            FirebaseAuth.DefaultInstance.StateChanged += OnAuthStateChanged;
            StartCoroutine(WaitForUserAndInit());
        }
        else
        {
            FirebaseInitializer.OnFirebaseReady += () =>
            {
                if (this == null || gameObject == null) return;

                FirebaseAuth.DefaultInstance.StateChanged -= OnAuthStateChanged;
                FirebaseAuth.DefaultInstance.StateChanged += OnAuthStateChanged;
                StartCoroutine(WaitForUserAndInit());
            };
        }
    }

    private IEnumerator WaitForUserAndInit()
    {
        yield return new WaitUntil(() =>
            FirebaseInitializer.Auth.CurrentUser != null &&
            !string.IsNullOrEmpty(FirebaseInitializer.Auth.CurrentUser.UserId)
        );

        HandleUserChange(FirebaseInitializer.Auth.CurrentUser);
    }

    private void OnAuthStateChanged(object sender, EventArgs eventArgs)
    {
        FirebaseUser newUser = FirebaseInitializer.Auth.CurrentUser;

        if (newUser == null || newUser.UserId == currentUserId)
            return;

        HandleUserChange(newUser);
    }

    private void HandleUserChange(FirebaseUser newUser)
    {
        DetachListener();
        ClearDisplay();

        currentUser = newUser;
        currentUserId = currentUser?.UserId;

        if (!string.IsNullOrEmpty(currentUserId))
        {
            SetupListener();
        }
    }

    private void SetupListener()
    {
        if (string.IsNullOrEmpty(currentUserId))
        {
            Debug.LogWarning("[RecentMessage] Cannot set up listener - user ID is null.");
            return;
        }

        DetachListener();
        messageRef = FirebaseInitializer.Database.GetReference("messages").Child(currentUserId);

        // Listen for new message
        messageRef.OrderByChild("timestamp").LimitToLast(1).ValueChanged += OnRecentMessageChanged;
        isListening = true;

        // Get most recent message once (initial load)
        messageRef.OrderByChild("timestamp").LimitToLast(1).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted && task.Result != null)
            {
                ProcessSnapshot(task.Result, currentUserId);
            }
        });

        Debug.Log($"[RecentMessage] Listening to messages/{currentUserId}");
    }

    private void OnRecentMessageChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError("DB Error: " + args.DatabaseError.Message);
            ClearDisplay();
            return;
        }

        if (!args.Snapshot.HasChildren)
        {
            ClearDisplay();
            return;
        }

        ProcessSnapshot(args.Snapshot, currentUserId);
    }

  private void ProcessSnapshot(DataSnapshot snapshot, string expectedUserId)
{
    // Avoid race condition from previous listener
    if (currentUser == null || currentUser.UserId != expectedUserId) return;

    foreach (var child in snapshot.Children)
    {
        var data = child.Value as Dictionary<string, object>;
        if (data == null) continue;

        string from = data.ContainsKey("from") ? data["from"].ToString() : "";
        string text = data.ContainsKey("text") ? data["text"].ToString() : "(No Message)";
        string time = "(Unknown Time)";

        if (data.TryGetValue("timestamp", out object timestampObj) &&
            long.TryParse(timestampObj.ToString(), out long ts))
        {
            try
            {
                // Check if in milliseconds and convert to seconds
                if (ts > 9999999999)
                    ts /= 1000;

                time = DateTimeOffset.FromUnixTimeSeconds(ts)
                    .ToLocalTime()
                    .ToString("h:mm tt"); // 👈 Example: 3:11 PM
            }
            catch
            {
                time = "(Invalid Timestamp)";
            }
        }
        else
        {
            time = "(Missing Timestamp)";
        }

        string prefix = from == "admin" ? "Agent: " : "You: ";

        if (recentMessageText != null)
            recentMessageText.text = prefix + text;

        if (recentTimestampText != null)
            recentTimestampText.text = time;
    }
}



    private void DetachListener()
    {
        if (messageRef != null && isListening)
        {
            messageRef.ValueChanged -= OnRecentMessageChanged;
        }

        messageRef = null;
        isListening = false;
    }

    private void ClearDisplay()
    {
        if (recentMessageText != null)
            recentMessageText.text = "No recent message";

        if (recentTimestampText != null)
            recentTimestampText.text = "";
    }

    private void OnDestroy()
    {
        DetachListener();
        FirebaseAuth.DefaultInstance.StateChanged -= OnAuthStateChanged;
    }
}
