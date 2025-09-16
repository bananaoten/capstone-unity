using UnityEngine;
using TMPro;
using Firebase.Database;
using Firebase.Auth;
using System;
using System.Collections;
using System.Collections.Generic;

public class RecentMessageDisplay : MonoBehaviour
{
    [Tooltip("Set the village or subdivision name, e.g. 'treelane' or 'lancris'")]
    public string villageName = "treelane";

    [Header("UI References")]
    public TMP_Text recentMessageText;
    public TMP_Text recentTimestampText;
    public GameObject badgeObject;           // 🔴 red circle background
    public TMP_Text badgeText;               // number inside badge

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

        messageRef = FirebaseInitializer.Database
            .GetReference("messages")
            .Child(villageName)
            .Child(currentUserId);

        messageRef.ValueChanged += OnRecentMessageChanged;
        isListening = true;

        Debug.Log($"[RecentMessage] Listening to messages/{villageName}/{currentUserId}");
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
    if (currentUser == null || currentUser.UserId != expectedUserId) return;

    string lastMsg = "(No Message)";
    string lastTime = "";
    int unreadCount = 0;
    string lastFrom = "";

    foreach (var child in snapshot.Children)
    {
        var data = child.Value as Dictionary<string, object>;
        if (data == null) continue;

        string from = data.ContainsKey("from") ? data["from"].ToString() : "";
        string text = data.ContainsKey("text") ? data["text"].ToString() : "(No Message)";
        bool isRead = data.ContainsKey("isRead") && Convert.ToBoolean(data["isRead"]);
        string time = "";

        if (data.TryGetValue("timestamp", out object timestampObj) &&
            long.TryParse(timestampObj.ToString(), out long ts))
        {
            if (ts > 9999999999) ts /= 1000;
            try
            {
                time = DateTimeOffset.FromUnixTimeSeconds(ts).ToLocalTime().ToString("h:mm tt");
            }
            catch { time = "(Invalid Time)"; }
        }

        lastMsg = text;
        lastTime = time;
        lastFrom = from;

        if ((from == "admin" || from == "agent") && !isRead)
            unreadCount++;
    }

    // Prefix the last message
    string prefix = "";
    if (lastFrom == "admin" || lastFrom == "agent")
        prefix = "Agent: ";
    else if (lastFrom == "user")
        prefix = "You: ";

    if (recentMessageText != null)
        recentMessageText.text = prefix + lastMsg;

    if (recentTimestampText != null)
        recentTimestampText.text = lastTime;

    // 🔴 Badge
    if (badgeObject != null && badgeText != null)
    {
        if (unreadCount > 0)
        {
            badgeObject.SetActive(true);
            badgeText.text = unreadCount.ToString();
        }
        else
        {
            badgeObject.SetActive(false);
        }
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

        if (badgeObject != null)
            badgeObject.SetActive(false);
    }

    private void OnDestroy()
    {
        DetachListener();
        FirebaseAuth.DefaultInstance.StateChanged -= OnAuthStateChanged;
    }
}
