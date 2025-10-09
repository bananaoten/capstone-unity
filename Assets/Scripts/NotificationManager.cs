using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections;

public class NotificationManager : MonoBehaviour
{
    private DatabaseReference dbRef;
    private FirebaseAuth auth;
    public GameObject notificationPrefab; // UI popup prefab

    private void Awake()
    {
        auth = FirebaseAuth.DefaultInstance;
    }

    private void Start()
    {
        StartCoroutine(InitializeFirebase());
    }

    private IEnumerator InitializeFirebase()
    {
        var check = FirebaseApp.CheckAndFixDependenciesAsync();
        yield return new WaitUntil(() => check.IsCompleted);

        if (check.Result != DependencyStatus.Available)
        {
            Debug.LogError("❌ Firebase dependencies not available: " + check.Result);
            yield break;
        }

        yield return new WaitUntil(() => auth.CurrentUser != null);

        dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        string userId = auth.CurrentUser.UserId;
        Debug.Log("✅ Listening for messages for user: " + userId);

        ListenForReplies(userId);
    }

    private void ListenForReplies(string userId)
    {
        FirebaseDatabase.DefaultInstance.GetReference("messages")
            .OrderByChild("receiver")
            .EqualTo(userId)
            .ValueChanged += HandleMessageChanged;
    }

    private void HandleMessageChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null) return;
        if (args.Snapshot == null || !args.Snapshot.HasChildren) return;

        foreach (var childSnapshot in args.Snapshot.Children)
        {
            string senderId = childSnapshot.Child("sender").Value?.ToString();
            string message = childSnapshot.Child("text").Value?.ToString();
            bool isRead = false;

            if (childSnapshot.Child("isRead").Value != null)
                bool.TryParse(childSnapshot.Child("isRead").Value.ToString(), out isRead);

            if (!isRead && (senderId == "admin" || senderId == "agent"))
            {
                Debug.Log("📩 New message received from " + senderId);
                ShowInAppNotification(message);

                // Mark as read to avoid multiple popups
                childSnapshot.Reference.Child("isRead").SetValueAsync(true);
            }
        }
    }

    private void ShowInAppNotification(string message)
    {
        if (notificationPrefab == null)
        {
            Debug.LogWarning("⚠️ Notification prefab not assigned!");
            return;
        }

        var canvas = GameObject.Find("Canvas");
        if (canvas == null)
        {
            Debug.LogError("❌ Canvas not found!");
            return;
        }

        GameObject notif = Instantiate(notificationPrefab, canvas.transform);
        notif.GetComponentInChildren<UnityEngine.UI.Text>().text = message;
    }
}
