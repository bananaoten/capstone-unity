using UnityEngine;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Extensions;
using System;
using TMPro;

public class MessagePageManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI usernameText;     // "Admin"
    public TextMeshProUGUI lastMessageText;  // Last message preview
    public TextMeshProUGUI timestampText;    // Time of last message
    public MessagePageItem badgeItem;        // Your badge script

    private DatabaseReference rootRef;
    private string userId;

    void Start()
    {
        var user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null) return;

        userId = user.UserId;
        rootRef = FirebaseDatabase.DefaultInstance.GetReference("messages").Child("treelane").Child(userId);

        // Listen for changes
        rootRef.ValueChanged += HandleMessagesChanged;
    }

    private void HandleMessagesChanged(object sender, ValueChangedEventArgs e)
    {
        if (e.DatabaseError != null) return;
        if (!e.Snapshot.Exists) return;

        int unreadCount = 0;
        string lastMsg = "";
        string lastTime = "";

        foreach (var msg in e.Snapshot.Children)
        {
            var dict = msg.Value as System.Collections.Generic.Dictionary<string, object>;
            if (dict == null) continue;

            string text = dict.ContainsKey("text") ? dict["text"].ToString() : "";
            string from = dict.ContainsKey("from") ? dict["from"].ToString() : "";
            bool isRead = dict.ContainsKey("isRead") && Convert.ToBoolean(dict["isRead"]);
            string timestamp = dict.ContainsKey("timestamp") ? dict["timestamp"].ToString() : "";

            lastMsg = text;
            lastTime = timestamp;

            if (from == "admin" && !isRead)
                unreadCount++;
        }

        // Update UI
        usernameText.text = "Admin";
        lastMessageText.text = lastMsg;
        timestampText.text = lastTime;
        badgeItem.SetBadge(unreadCount);
    }
}
