using UnityEngine;
using Firebase.Auth;
using Firebase.Database;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class MessagePage1 : MonoBehaviour
{
    public GameObject chatListItemPrefab;
    public Transform chatListContainer;
    public GameObject messagingPanelCanvas;
    public GameObject messagePageCanvas;

    void Start()
    {
        string userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        var rootRef = FirebaseDatabase.DefaultInstance.GetReference("messages").Child(userId);

        rootRef.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                foreach (var chat in task.Result.Children)
                {
                    string chatWith = chat.Key;
                    string lastMessage = "";

                    foreach (var msg in chat.Children)
                    {
                        if (msg.HasChild("text"))
                            lastMessage = msg.Child("text").Value.ToString();
                    }

                    CreateChatItem(chatWith, lastMessage);
                }
            }
        });
    }

    void CreateChatItem(string chatWithId, string lastMessage)
    {
        GameObject item = Instantiate(chatListItemPrefab, chatListContainer);
        item.GetComponentInChildren<TMP_Text>().text = $"Admin: {lastMessage}";

        item.GetComponent<Button>().onClick.AddListener(() =>
        {
            PlayerPrefs.SetString("chatWith", chatWithId);
            messagingPanelCanvas.SetActive(true);
            messagePageCanvas.SetActive(false);
        });
    }
}
    