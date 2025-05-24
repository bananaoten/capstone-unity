using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MessageListener : MonoBehaviour
{
    public Transform contentPanel; // ScrollView/Viewport/Content
    public GameObject leftMessagePrefab;  // For admin (left side)
    public GameObject rightMessagePrefab; // For user (right side)

    private string currentUserId;
    private DatabaseReference msgRef;
    private ScrollRect scrollRect;
    private bool isListening = false;
    private bool isDestroyed = false;

    void Start()
    {
        if (contentPanel != null && contentPanel.parent != null && contentPanel.parent.parent != null)
        {
            scrollRect = contentPanel.parent.parent.GetComponent<ScrollRect>();
        }

        if (FirebaseInitializer.IsFirebaseReady)
        {
            SetupFirebaseListeners();
        }
        else
        {
            FirebaseInitializer.OnFirebaseReady += SetupFirebaseListeners;
        }
    }

    void OnDestroy()
    {
        isDestroyed = true;

        if (FirebaseInitializer.IsFirebaseReady)
        {
            FirebaseInitializer.Auth.StateChanged -= HandleAuthStateChanged;
            if (msgRef != null)
            {
                msgRef.ChildAdded -= HandleNewMessage;
            }
        }
        else
        {
            FirebaseInitializer.OnFirebaseReady -= SetupFirebaseListeners;
        }
    }

    private void SetupFirebaseListeners()
    {
        if (isDestroyed) return;

        FirebaseInitializer.Auth.StateChanged -= HandleAuthStateChanged;
        FirebaseInitializer.Auth.StateChanged += HandleAuthStateChanged;

        if (FirebaseInitializer.Auth.CurrentUser != null)
        {
            SubscribeToCurrentUser();
        }
    }

    private void HandleAuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (isDestroyed) return;

        var user = FirebaseInitializer.Auth.CurrentUser;
        if (user != null && user.UserId != currentUserId)
        {
            SubscribeToCurrentUser();
        }
    }

    private void SubscribeToCurrentUser()
    {
        if (isDestroyed) return;

        var user = FirebaseInitializer.Auth.CurrentUser;
        if (user == null) return;

        string newUserId = user.UserId;

        if (isListening && msgRef != null)
        {
            msgRef.ChildAdded -= HandleNewMessage;
            ClearMessageUI();
        }

        currentUserId = newUserId;
        msgRef = FirebaseInitializer.Database.GetReference("messages").Child(currentUserId);

        // Load previous messages
        msgRef.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted || task.IsCanceled || isDestroyed)
                return;

            var snapshot = task.Result;
            List<(string from, string text)> messageList = new();

            foreach (var child in snapshot.Children)
            {
                var data = child.Value as Dictionary<string, object>;
                if (data == null || !data.ContainsKey("from") || !data.ContainsKey("text"))
                    continue;

                string from = data["from"].ToString();
                string text = data["text"].ToString();
                messageList.Add((from, text));
            }

            // Pass to Unity main thread
            StartCoroutine(ShowMessagesRoutine(messageList));
        });

        msgRef.ChildAdded += HandleNewMessage;
        isListening = true;
    }

    private IEnumerator ShowMessagesRoutine(List<(string from, string text)> messages)
    {
        yield return null;

        if (isDestroyed) yield break;

        foreach (var msg in messages)
        {
            DisplayMessageLocally(msg.from, msg.text);
        }

        yield return new WaitForEndOfFrame();
        if (!isDestroyed)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentPanel.GetComponent<RectTransform>());
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    private void HandleNewMessage(object sender, ChildChangedEventArgs args)
    {
        if (isDestroyed || args.DatabaseError != null || args.Snapshot?.Value == null)
            return;

        var data = args.Snapshot.Value as Dictionary<string, object>;
        if (data == null || !data.ContainsKey("from") || !data.ContainsKey("text"))
            return;

        string from = data["from"].ToString();
        string text = data["text"].ToString();

        if (!isDestroyed)
        {
            StartCoroutine(HandleNewMessageRoutine(from, text));
        }
    }

    private IEnumerator HandleNewMessageRoutine(string from, string text)
    {
        yield return null;

        if (!isDestroyed)
        {
            DisplayMessageLocally(from, text);
            yield return new WaitForEndOfFrame();

            if (scrollRect != null && contentPanel != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentPanel.GetComponent<RectTransform>());
                scrollRect.verticalNormalizedPosition = 0f;
            }
        }
    }

    public void DisplayMessageLocally(string from, string text)
    {
        if (isDestroyed || contentPanel == null)
            return;

        GameObject prefabToUse = from == "admin" ? leftMessagePrefab : rightMessagePrefab;
        if (prefabToUse == null) return;

        GameObject bubble = Instantiate(prefabToUse, contentPanel);
        TMP_Text messageText = bubble.GetComponentInChildren<TMP_Text>();
        if (messageText != null)
        {
            messageText.text = text;
        }
    }

    private void ClearMessageUI()
    {
        if (isDestroyed || contentPanel == null)
            return;

        for (int i = contentPanel.childCount - 1; i >= 0; i--)
        {
            if (contentPanel.GetChild(i) != null)
                Destroy(contentPanel.GetChild(i).gameObject);
        }
    }
}
