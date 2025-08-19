using UnityEngine;
using Firebase.Auth;
using Firebase.Database;
using TMPro;
using System.Collections.Generic;

public class MessageSender : MonoBehaviour
{
    public TMP_InputField inputField;
    public MessageListener messageListener;

    public void SendMessageToAdmin()
    {
        string text = inputField.text.Trim();
        if (string.IsNullOrEmpty(text)) return;

        string userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;

        // ✅ Updated to use "treelane" path
        var messageRef = FirebaseDatabase.DefaultInstance
            .GetReference("messages")
            .Child("treelane")
            .Child(userId);

        string msgKey = messageRef.Push().Key;

        var data = new Dictionary<string, object>
        {
            { "from", "user" },
            { "text", text },
            { "timestamp", ServerValue.Timestamp }
        };

        messageRef.Child(msgKey).SetValueAsync(data);

        inputField.text = "";

        // Do NOT display locally here, MessageListener will handle it from Firebase
    }
}
