using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections;

public class AppointmentNotificationManager : MonoBehaviour
{
    [Header("UI")]
    public Transform notificationContainer;   // ✅ Assign: ScrollView Content
    public GameObject notificationPrefab;     // ✅ Assign: Prefab with NotificationItem script

    private DatabaseReference dbRef;
    private FirebaseAuth auth;

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

        // ✅ Wait for user to be logged in before listening
        yield return new WaitUntil(() => auth.CurrentUser != null);

        dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        string currentUserId = auth.CurrentUser.UserId;
        Debug.Log("✅ Listening for appointment updates for user: " + currentUserId);

        // Listen only to this user's appointments
        FirebaseDatabase.DefaultInstance
            .GetReference("appointments")
            .OrderByChild("userUid")
            .EqualTo(currentUserId)
            .ValueChanged += OnAppointmentsChanged;
    }

    private void OnAppointmentsChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError("Firebase error: " + args.DatabaseError.Message);
            return;
        }

        if (!args.Snapshot.Exists) return;

        foreach (var child in args.Snapshot.Children)
        {
            string status = child.Child("status").Value?.ToString();

            // Read notification flag
            bool notif = false;
            if (child.Child("notification").Value != null)
            {
                bool.TryParse(child.Child("notification").Value.ToString(), out notif);
            }

            if (notif)
            {
                Debug.Log($"🔔 Notification detected for appointment {child.Key}, status = {status}");

                string title = "";
                string description = "";
                NotificationItem.NotificationType type = NotificationItem.NotificationType.Pending;

                if (status == "approved")
                {
                    title = "Appointment Approved";
                    description = "✅ Your appointment has been approved!";
                    type = NotificationItem.NotificationType.Approved;
                }
                else if (status == "declined")
                {
                    title = "Appointment Declined";
                    description = "❌ Please contact admin for more details.";
                    type = NotificationItem.NotificationType.Declined;
                }
                else
                {
                    title = "Appointment Update";
                    description = "Your appointment status has changed.";
                    type = NotificationItem.NotificationType.Pending;
                }

                // Add notification to UI
                AddNotification(title, description, type, DateTime.UtcNow);

                // Reset notification so it doesn’t re-trigger
                dbRef.Child("appointments").Child(child.Key).Child("notification").SetValueAsync(false);
            }
        }
    }

    private void AddNotification(string title, string description, NotificationItem.NotificationType type, DateTime timestamp)
    {
        if (notificationPrefab == null || notificationContainer == null)
        {
            Debug.LogWarning("⚠️ Notification prefab or container is not assigned!");
            return;
        }

        GameObject notifGO = Instantiate(notificationPrefab, notificationContainer);

        NotificationItem notifItem = notifGO.GetComponent<NotificationItem>();
        if (notifItem != null)
        {
            notifItem.SetData(title, description, type, timestamp);
        }
        else
        {
            Debug.LogWarning("⚠️ Notification prefab is missing NotificationItem script!");
        }
    }
}
