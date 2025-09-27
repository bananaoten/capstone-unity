using UnityEngine;
using Firebase;
using Firebase.Database;
using System;

public class NotificationManager : MonoBehaviour
{
    private DatabaseReference dbRef;
    public GameObject notificationPrefab; // UI popup prefab

    void Start()
    {
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        ListenForReplies("user123"); // replace with logged in user's UID
    }

    void ListenForReplies(string userId)
    {
        FirebaseDatabase.DefaultInstance.GetReference("messages")
            .OrderByChild("receiver")
            .EqualTo(userId)
            .ValueChanged += HandleMessageChanged;
    }

    void HandleMessageChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null) return;
        if (args.Snapshot == null || !args.Snapshot.HasChildren) return;

        foreach (var childSnapshot in args.Snapshot.Children)
        {
            string senderId = childSnapshot.Child("sender").Value.ToString();
            string message = childSnapshot.Child("text").Value.ToString();
            bool isRead = Convert.ToBoolean(childSnapshot.Child("isRead").Value);

            if (!isRead && (senderId == "admin" || senderId == "agent"))
            {
                ShowInAppNotification(message);

                // Mark as read to avoid multiple popups
                childSnapshot.Reference.Child("isRead").SetValueAsync(true);
            }
        }
    }

    void ShowInAppNotification(string message)
    {
        GameObject notif = Instantiate(notificationPrefab, GameObject.Find("Canvas").transform);
        notif.GetComponentInChildren<UnityEngine.UI.Text>().text = message;
    }
}
