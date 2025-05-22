using UnityEngine;
using Firebase.Auth;
using Firebase.Database;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;

public class MessagePreviewLoader : MonoBehaviour
{
    public Transform contentPanel;
    public GameObject messagePreviewPrefab;

    void Start()
    {
        LoadLatestMessage();
    }

    void LoadLatestMessage()
    {
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null) return;

        string userId = user.UserId;
        var msgRef = FirebaseDatabase.DefaultInstance.GetReference("messages").Child(userId);

        msgRef.OrderByChild("timestamp").LimitToLast(1).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted || task.IsCanceled) return;
            DataSnapshot snapshot = task.Result;

            foreach (var child in snapshot.Children)
            {
                var data = child.Value as Dictionary<string, object>;
                if (data == null || !data.ContainsKey("text") || !data.ContainsKey("timestamp"))
                    continue;

                string lastText = data["text"].ToString();
                long timestamp = Convert.ToInt64(data["timestamp"]);
                string formattedTime = DateTimeOffset.FromUnixTimeSeconds(timestamp).ToLocalTime().ToString("t");

                StartCoroutine(CreatePreview(lastText, formattedTime));
            }
        });
    }

    IEnumerator CreatePreview(string text, string time)
    {
        yield return new WaitForEndOfFrame();

        GameObject preview = Instantiate(messagePreviewPrefab, contentPanel);
        preview.transform.Find("NameText").GetComponent<TMP_Text>().text = "Admin";
        preview.transform.Find("MessagePreviewText").GetComponent<TMP_Text>().text = text;
        preview.transform.Find("TimestampText").GetComponent<TMP_Text>().text = time;
    }
}
