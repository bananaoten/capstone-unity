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

    private string currentUserId;
    private DatabaseReference msgRef;
    private ScrollRect scrollRect;
    private bool isListening = false;

    void Start()
    {
        scrollRect = contentPanel.parent.parent.GetComponent<ScrollRect>();

        // Automatically subscribe if already logged in
        if (FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            SubscribeToCurrentUser();
        }

        // Listen for login state changes
        FirebaseAuth.DefaultInstance.StateChanged += HandleAuthStateChanged;
    }

    void OnDestroy()
    {
        // Clean up
        FirebaseAuth.DefaultInstance.StateChanged -= HandleAuthStateChanged;
        if (msgRef != null)
        {
            msgRef.ChildAdded -= HandleNewMessage;
        }
    }

    private void HandleAuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user != null && user.UserId != currentUserId)
        {
            SubscribeToCurrentUser();
        }
    }

    private void SubscribeToCurrentUser()
    {
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null)
            return;

        string newUserId = user.UserId;

        if (isListening && msgRef != null)
        {
            msgRef.ChildAdded -= HandleNewMessage;
            Debug.Log($"Unsubscribed from old user: {currentUserId}");
            ClearMessageUI();
        }

        currentUserId = newUserId;
        msgRef = FirebaseDatabase.DefaultInstance.GetReference("messages").Child(currentUserId);

        // Load previous messages
        msgRef.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Failed to get messages: " + task.Exception);
                return;
            }

            DataSnapshot snapshot = task.Result;

            foreach (var child in snapshot.Children)
            {
                var data = child.Value as Dictionary<string, object>;
                if (data == null || !data.ContainsKey("from") || !data.ContainsKey("text"))
                    continue;

                string from = data["from"].ToString();
                string text = data["text"].ToString();

                DisplayMessageLocally(from, text);
            }

            StartCoroutine(ScrollToBottomCoroutine());
        });

        // Listen for new messages
        msgRef.ChildAdded += HandleNewMessage;
        isListening = true;

        Debug.Log($"Subscribed to user: {currentUserId}");
    }

    private void HandleNewMessage(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null || args.Snapshot?.Value == null)
            return;

        var data = args.Snapshot.Value as Dictionary<string, object>;
        if (data == null || !data.ContainsKey("from") || !data.ContainsKey("text"))
        {
            Debug.LogWarning("Message data missing 'from' or 'text'");
            return;
        }

        string from = data["from"].ToString();
        string text = data["text"].ToString();

        DisplayMessageLocally(from, text);
        StartCoroutine(ScrollToBottomCoroutine());
    }

    public void DisplayMessageLocally(string from, string text)
    {
        GameObject bubble = Instantiate(messageBubblePrefab, contentPanel);
        TMP_Text messageText = bubble.GetComponentInChildren<TMP_Text>();
        messageText.text = text;

        Image bg = bubble.GetComponent<Image>();
        bg.color = from == "admin" ? adminColor : userColor;
    }

    private IEnumerator ScrollToBottomCoroutine()
    {
        yield return new WaitForEndOfFrame();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentPanel.GetComponent<RectTransform>());
        scrollRect.verticalNormalizedPosition = 0f;
        yield return null;
        scrollRect.verticalNormalizedPosition = 0f;
    }

    private void ClearMessageUI()
    {
        for (int i = contentPanel.childCount - 1; i >= 0; i--)
        {
            Destroy(contentPanel.GetChild(i).gameObject);
        }
    }
}
