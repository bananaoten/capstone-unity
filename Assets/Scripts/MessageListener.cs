using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

public class MessageListener : MonoBehaviour
{
    public Transform contentPanel; // ScrollView/Viewport/Content
    public GameObject leftMessagePrefab;  // For admin (left side)
    public GameObject rightMessagePrefab; // For user (right side)

    private string currentUserId;
    private DatabaseReference msgRef;
    private ScrollRect scrollRect;
    private bool isListening = false;

    async void Start()
    {
        scrollRect = contentPanel.parent.parent.GetComponent<ScrollRect>();

        // Wait for Firebase dependencies before calling any Firebase APIs
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();

        if (dependencyStatus == DependencyStatus.Available)
        {
            Debug.Log("Firebase dependencies are available.");

            if (FirebaseAuth.DefaultInstance.CurrentUser != null)
            {
                SubscribeToCurrentUser();
            }

            FirebaseAuth.DefaultInstance.StateChanged += HandleAuthStateChanged;
        }
        else
        {
            Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            // Handle dependency error accordingly (e.g. show error UI)
        }
    }

    void OnDestroy()
    {
        if (FirebaseAuth.DefaultInstance != null)
        {
            FirebaseAuth.DefaultInstance.StateChanged -= HandleAuthStateChanged;
        }
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
            ClearMessageUI();
        }

        currentUserId = newUserId;
        msgRef = FirebaseDatabase.DefaultInstance.GetReference("messages").Child(currentUserId);

        // Load previous messages
        msgRef.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
                return;

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

        msgRef.ChildAdded += HandleNewMessage;
        isListening = true;
    }

    private void HandleNewMessage(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null || args.Snapshot?.Value == null)
            return;

        var data = args.Snapshot.Value as Dictionary<string, object>;
        if (data == null || !data.ContainsKey("from") || !data.ContainsKey("text"))
            return;

        string from = data["from"].ToString();
        string text = data["text"].ToString();

        DisplayMessageLocally(from, text);
        StartCoroutine(ScrollToBottomCoroutine());
    }

    public void DisplayMessageLocally(string from, string text)
    {
        GameObject prefabToUse = from == "admin" ? leftMessagePrefab : rightMessagePrefab;
        GameObject bubble = Instantiate(prefabToUse, contentPanel);
        TMP_Text messageText = bubble.GetComponentInChildren<TMP_Text>();
        messageText.text = text;
    }

    private IEnumerator ScrollToBottomCoroutine()
    {
        yield return new WaitForEndOfFrame();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentPanel.GetComponent<RectTransform>());
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
