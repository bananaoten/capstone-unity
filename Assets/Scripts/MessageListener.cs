using UnityEngine;
using Firebase.Auth;
using Firebase.Database;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MessageListener : MonoBehaviour
{
    public Transform contentPanel; // ScrollView/Viewport/Content
    public GameObject messageBubblePrefab;
    public Color userColor = new Color(0.7f, 0.9f, 1f);   // Light blue
    public Color adminColor = new Color(0.9f, 0.9f, 0.9f); // Light gray

    private string userId;

    private ScrollRect scrollRect;

    void Start()
    {
        userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        var msgRef = FirebaseDatabase.DefaultInstance.GetReference("messages").Child(userId);

        msgRef.ChildAdded += HandleNewMessage;

        // Cache ScrollRect for efficiency
        scrollRect = contentPanel.parent.parent.GetComponent<ScrollRect>();
    }

    private void HandleNewMessage(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null || args.Snapshot?.Value == null)
            return;

        var data = args.Snapshot.Value as Dictionary<string, object>;
        if (data == null) return;

        string from = data["from"].ToString();
        string text = data["text"].ToString();

        DisplayMessageLocally(from, text);
    }

    public void DisplayMessageLocally(string from, string text)
    {
        GameObject bubble = Instantiate(messageBubblePrefab, contentPanel);
        TMP_Text messageText = bubble.GetComponentInChildren<TMP_Text>();
        messageText.text = text;

        Image bg = bubble.GetComponent<Image>();
        if (from == "admin")
            bg.color = adminColor;
        else
            bg.color = userColor;

        StartCoroutine(ScrollToBottomCoroutine());
    }

    private IEnumerator ScrollToBottomCoroutine()
    {
        // Wait till end of frame to let layout rebuild happen
        yield return new WaitForEndOfFrame();

        // Force rebuild layout to get accurate sizes
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentPanel.GetComponent<RectTransform>());

        // Scroll to bottom
        scrollRect.verticalNormalizedPosition = 0f;

        // Sometimes one frame isn't enough, so try again next frame
        yield return null;
        scrollRect.verticalNormalizedPosition = 0f;
    }
}
