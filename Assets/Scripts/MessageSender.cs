using UnityEngine;
using Firebase.Auth;
using Firebase.Database;
using TMPro;
using System.Collections.Generic;

public class MessageSender : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField inputField;

    [Header("References (optional)")]
    public MessageListener messageListener;

    public void SendMessageToAdmin()
    {
        string text = inputField.text.Trim();
        if (string.IsNullOrEmpty(text)) return;

        var user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null)
        {
            Debug.LogError("Cannot send message: no authenticated user.");
            return;
        }

        string userId = user.UserId;

        var messageRef = FirebaseDatabase.DefaultInstance
            .GetReference("messages")
            .Child("treelane")
            .Child(userId);

        string msgKey = messageRef.Push().Key;

        var data = new Dictionary<string, object>
        {
            { "from", "user" },
            { "text", text },
            { "timestamp", ServerValue.Timestamp },
            { "isRead", true }       // user messages are already read by the user
        };

        messageRef.Child(msgKey).SetValueAsync(data).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Failed to send message: " + task.Exception);
            }
            else
            {
                Debug.Log("Message sent.");
            }
        });

        inputField.text = "";
    }
}
